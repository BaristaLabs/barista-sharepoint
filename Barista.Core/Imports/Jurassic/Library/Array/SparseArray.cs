namespace Jurassic.Library
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// Represents an array with non-consecutive elements.
  /// </summary>
  [Serializable]
  internal sealed class SparseArray
  {
    private const int NodeShift = 5;
    private const int NodeSize = 1 << NodeShift;
    private const uint NodeMask = NodeSize - 1;
    private const uint NodeInverseMask = ~NodeMask;

    [Serializable]
    private class Node
    {
      public readonly object[] Array;

      public Node()
      {
        this.Array = new object[NodeSize];
      }
    }

    private int m_depth;  // Depth of tree.
    private int m_mask;   // Valid index mask.
    private Node m_root;  // Root of tree.

    private object[] m_recent;        // Most recently accessed array.
    private uint m_recentStart = uint.MaxValue;   // The array index the most recent array starts at.

    /// <summary>
    /// Creates a sparse array from the given dense array.
    /// </summary>
    /// <param name="array"> The array to copy items from. </param>
    /// <param name="length"> The number of items to copy. </param>
    /// <returns> A new sparse array containing the items from the given array. </returns>
    public static SparseArray FromDenseArray(object[] array, int length)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (length > array.Length)
        throw new ArgumentOutOfRangeException("length");
      var result = new SparseArray();
      result.CopyTo(array, 0, length);
      return result;
    }

    ///// <summary>
    ///// Gets the total number of items that can be stored in this tree without having to
    ///// increase the size of the tree.
    ///// </summary>
    //public uint Capacity
    //{
    //    get { return this.depth == 0 ? 0 : (uint)Math.Pow(NodeSize, this.depth); }
    //}

    public object this[uint index]
    {
      get
      {
        if ((index & NodeInverseMask) == this.m_recentStart)
          return this.m_recent[index & NodeMask];
        return GetValueNonCached(index);
      }
      set
      {
        value = value ?? Undefined.Value;
        if ((index & NodeInverseMask) == this.m_recentStart)
          this.m_recent[index & NodeMask] = value;
        else
        {
          object[] array = FindOrCreateArray(index, true);
          array[index & NodeMask] = value;
        }
      }
    }

    /// <summary>
    /// Deletes (sets to <c>null</c>) an array element.
    /// </summary>
    /// <param name="index"> The index of the array element to delete. </param>
    public void Delete(uint index)
    {
      if ((index & NodeInverseMask) == this.m_recentStart)
        this.m_recent[index & NodeMask] = null;
      else
      {
        var array = FindOrCreateArray(index, false);
        if (array == null)
          return;
        array[index & NodeMask] = null;
      }
    }

    /// <summary>
    /// Deletes (sets to <c>null</c>) a range of array elements.
    /// </summary>
    /// <param name="start"> The index of the first array element to delete. </param>
    /// <param name="length"> The number of array elements to delete. </param>
    public void DeleteRange(uint start, uint length)
    {
      if (this.m_root == null)
        return;
      DeleteRange(start, length, null, this.m_root, 0, this.m_depth);
    }

    /// <summary>
    /// Deletes (sets to <c>null</c>) a range of array elements.
    /// </summary>
    /// <param name="start"> The index of the first array element to delete. </param>
    /// <param name="length"> The number of array elements to delete. </param>
    /// <param name="parentNode"> The parent node of the node to delete from.  Can be <c>null</c>. </param>
    /// <param name="node"> The node to delete from. </param>
    /// <param name="nodeIndex"> The index of the node, in the parent node's array. </param>
    /// <param name="nodeDepth"> The depth of the tree, treating <paramref name="node"/> as the root. </param>
    private void DeleteRange(uint start, uint length, Node parentNode, Node node, int nodeIndex, int nodeDepth)
    {
      uint nodeLength = (NodeShift * nodeDepth) >= 32 ? uint.MaxValue : 1u << NodeShift * nodeDepth;
      uint nodeStart = nodeLength * (uint)nodeIndex;
      if (parentNode != null && (nodeStart >= start + length || nodeStart + nodeLength <= start))
      {
        // Delete the entire node.
        parentNode.Array[nodeIndex] = null;
        return;
      }

      if (nodeDepth == 1)
      {
        // The node is a leaf node.
        for (int i = 0; i < NodeSize; i++)
        {
          uint index = (uint)(nodeStart + i);
          if (index >= start && index < start + length)
            node.Array[i] = null;
        }
      }
      else
      {
        // The node is a branch node.
        for (int i = 0; i < NodeSize; i++)
        {
          var element = node.Array[i] as Node;
          if (element != null)
          {
            DeleteRange(start, length, node, element, i, nodeDepth - 1);
          }
        }
      }
    }

    private object GetValueNonCached(uint index)
    {
      object[] array = FindOrCreateArray(index, false);
      if (array == null)
        return null;
      return array[index & NodeMask];
    }

    //public bool Exists(uint index)
    //{
    //    object[] array;
    //    if ((index & NodeInverseMask) == this.recentStart)
    //        array = this.recent;
    //    else
    //    {
    //        array = FindOrCreateArray(index, writeAccess: false);
    //        if (array == null)
    //            return false;
    //    }

    //    return array[index & NodeMask] != null;
    //}

    //public object RemoveAt(uint index)
    //{
    //    object[] array;
    //    if ((index & NodeInverseMask) == this.recentStart)
    //        array = this.recent;
    //    else
    //    {
    //        array = FindOrCreateArray(index, writeAccess: false);
    //        if (array == null)
    //            return null;
    //    }
    //    index &= index & NodeMask;
    //    object result = array[index];
    //    array[index] = null;
    //    if (result == NullPlaceholder.Value)
    //        return null;
    //    return result;
    //}

    private struct NodeInfo
    {
      public int Depth;
      public uint Index;
      public Node Node;
    }

    public struct Range
    {
      public object[] Array;
      public uint StartIndex;
      public int Length { get { return this.Array.Length; } }
    }

    public IEnumerable<Range> Ranges
    {
      get
      {
        if (this.m_root == null)
          yield break;

        var stack = new Stack<NodeInfo>();
        stack.Push(new NodeInfo { Depth = 1, Index = 0, Node = this.m_root });

        while (stack.Count > 0)
        {
          var info = stack.Pop();
          var node = info.Node;
          if (info.Depth < this.m_depth)
          {
            for (var i = NodeSize - 1; i >= 0; i--)
              if (node.Array[i] != null)
                stack.Push(new NodeInfo { Depth = info.Depth + 1, Index = (uint)(info.Index * NodeSize + i), Node = (Node)node.Array[i] });
          }
          else
          {
            yield return new Range { Array = info.Node.Array, StartIndex = info.Index * NodeSize };
          }
        }
      }
    }

    private object[] FindOrCreateArray(uint index, bool writeAccess)
    {
      // Check if the index is out of bounds.
      if ((index & this.m_mask) != index || this.m_depth == 0)
      {
        if (writeAccess == false)
          return null;

        // Create one or more new root nodes.
        do
        {
          var newRoot = new Node();
          newRoot.Array[0] = this.m_root;
          this.m_root = newRoot;
          this.m_depth++;
          this.m_mask = NodeShift * this.m_depth >= 32 ? -1 : (1 << NodeShift * this.m_depth) - 1;
        } while ((index & this.m_mask) != index);
      }

      // Find the node.
      Node current = this.m_root;
      for (int depth = this.m_depth - 1; depth > 0; depth--)
      {
        uint currentIndex = (index >> (depth * NodeShift)) & NodeMask;
        var newNode = (Node)current.Array[currentIndex];
        if (newNode == null)
        {
          if (writeAccess == false)
            return null;
          newNode = new Node();
          current.Array[currentIndex] = newNode;
        }
        current = newNode;
      }

      // Populate the MRU members.
      this.m_recent = current.Array;
      this.m_recentStart = index & NodeInverseMask;

      return current.Array;
    }

    /// <summary>
    /// Copies the elements of the sparse array to this sparse array, starting at a particular
    /// index.  Existing values are overwritten.
    /// </summary>
    /// <param name="source"> The sparse array to copy. </param>
    /// <param name="start"> The zero-based index at which copying begins. </param>
    /// <param name="length"> The number of elements to copy. </param>
    public void CopyTo(object[] source, uint start, int length)
    {
      int sourceOffset = 0;
      do
      {
        // Get a reference to the array to copy to.
        object[] dest = FindOrCreateArray(start, true);
        int destOffset = (int)(start & NodeMask);

        // Copy as much as possible.
        int copyLength = Math.Min(length - sourceOffset, dest.Length - destOffset);
        Array.Copy(source, sourceOffset, dest, destOffset, copyLength);

        start += (uint)copyLength;
        sourceOffset += copyLength;
      } while (sourceOffset < length);
    }

    /// <summary>
    /// Copies the elements of the given sparse array to this sparse array, starting at a
    /// particular index.  Existing values are overwritten.
    /// </summary>
    /// <param name="source"> The sparse array to copy. </param>
    /// <param name="start"> The zero-based index at which copying begins. </param>
    public void CopyTo(SparseArray source, uint start)
    {
      foreach (var sourceRange in source.Ranges)
      {
        int sourceOffset = 0;
        uint destIndex = start + sourceRange.StartIndex;

        do
        {
          // Get a reference to the array to copy to.
          object[] dest = FindOrCreateArray(start, true);
          int destOffset = (int)(destIndex & NodeMask);

          // Copy as much as possible.
          int copyLength = Math.Min(sourceRange.Length - sourceOffset, dest.Length - destOffset);
          Array.Copy(sourceRange.Array, sourceOffset, dest, destOffset, copyLength);

          start += (uint)copyLength;
          sourceOffset += copyLength;
        } while (sourceOffset < sourceRange.Length);
      }
    }
  }
}
