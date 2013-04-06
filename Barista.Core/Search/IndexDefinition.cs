namespace Barista.Search
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

	/// <summary>
	/// A definition of an index
	/// </summary>
	public class IndexDefinition
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IndexDefinition"/> class.
		/// </summary>
		public IndexDefinition()
		{
			Maps = new HashSet<string>();
			Indexes = new Dictionary<string, FieldIndexing>();
			Stores = new Dictionary<string, FieldStorage>();
			Analyzers = new Dictionary<string, string>();
			SortOptions = new Dictionary<string, SortOptions>();
			Fields = new List<string>();
			TermVectors = new Dictionary<string, FieldTermVector>();
		}

		/// <summary>
		/// Get or set the name of the index
		/// </summary>
		public string Name
    {
      get;
      set;
    }

		/// <summary>
		/// All the map functions for this index
		/// </summary>
		public HashSet<string> Maps
    {
      get;
      set;
    }

		/// <summary>
		/// Gets or sets the reduce function
		/// </summary>
		/// <value>The reduce.</value>
		public string Reduce
    {
      get;
      set;
    }

		/// <summary>
		/// Gets or sets the translator function
		/// </summary>
		public string TransformResults
    {
      get;
      set;
    }

		/// <summary>
		/// Gets a value indicating whether this instance is map reduce index definition
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is map reduce; otherwise, <c>false</c>.
		/// </value>
		public bool IsMapReduce
		{
			get
      {
        return string.IsNullOrEmpty(Reduce) == false;
      }
		}

		public bool IsCompiled
    {
      get;
      set;
    }

		/// <summary>
		/// Returns a boolean value indicating whether this IndexDefinition is of a temporary index
		/// </summary>
		public bool IsTemp
		{
			get
      {
        return !string.IsNullOrEmpty(Name) && Name.StartsWith("Temp/", StringComparison.InvariantCultureIgnoreCase);
      }
		}

		/// <summary>
		/// Gets or sets the stores options
		/// </summary>
		/// <value>The stores.</value>
		public IDictionary<string, FieldStorage> Stores { get; set; }

		/// <summary>
		/// Gets or sets the indexing options
		/// </summary>
		/// <value>The indexes.</value>
		public IDictionary<string, FieldIndexing> Indexes { get; set; }

		/// <summary>
		/// Gets or sets the sort options.
		/// </summary>
		/// <value>The sort options.</value>
		public IDictionary<string, SortOptions> SortOptions { get; set; }

		/// <summary>
		/// Gets or sets the analyzers options
		/// </summary>
		/// <value>The analyzers.</value>
		public IDictionary<string, string> Analyzers { get; set; }

		/// <summary>
		/// The fields that are queryable in the index
		/// </summary>
		public IList<string> Fields { get; set; }

		/// <summary>
		/// Gets or sets the term vectors options
		/// </summary>
		/// <value>The term vectors.</value>
		public IDictionary<string, FieldTermVector> TermVectors { get; set; }

		/// <summary>
		/// Equals the specified other.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public bool Equals(IndexDefinition other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Maps.SequenceEqual(other.Maps) &&
					Equals(other.Name, Name) &&
					Equals(other.Reduce, Reduce) &&
					Equals(other.TransformResults, TransformResults) &&
					DictionaryEquals(other.Stores, Stores) &&
					DictionaryEquals(other.Indexes, Indexes) &&
					DictionaryEquals(other.Analyzers, Analyzers) &&
					DictionaryEquals(other.SortOptions, SortOptions) &&
					DictionaryEquals(other.TermVectors, TermVectors);
		}

		private static bool DictionaryEquals<TKey, TValue>(IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y)
		{
			if (x.Count != y.Count)
				return false;
			foreach (var v in x)
			{
				TValue value;
				if (y.TryGetValue(v.Key, out value) == false)
					return false;
				if (Equals(value, v.Value) == false)
					return false;
			}
			return true;
		}

		private static int DictionaryHashCode<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> x)
		{
			int result = 0;
			foreach (var kvp in x)
			{
				result = (result * 397) ^ kvp.Key.GetHashCode();
				result = (result * 397) ^ (!Equals(kvp.Value, default(TValue)) ? kvp.Value.GetHashCode() : 0);
			}
			return result;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return Equals(obj as IndexDefinition);
		}

		private byte[] m_cachedHashCodeAsBytes;

		/// <summary>
		/// Provide a cached version of the index hash code, which is used when generating
		/// the index etag. 
		/// It isn't really useful for anything else, in particular, we cache that because
		/// we want to avoid calculating the cost of doing this over and over again on each 
		/// query.
		/// </summary>
		public byte[] GetIndexHash()
		{
			if (m_cachedHashCodeAsBytes != null)
				return m_cachedHashCodeAsBytes;

			m_cachedHashCodeAsBytes = BitConverter.GetBytes(GetHashCode());
			return m_cachedHashCodeAsBytes;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int result = Maps.Where(x => x != null).Aggregate(0, (acc, val) => acc * 397 ^ val.GetHashCode());
				result = (result * 397) ^ Maps.Count;
				result = (result * 397) ^ (Reduce != null ? Reduce.GetHashCode() : 0);
				result = (result * 397) ^ (TransformResults != null ? TransformResults.GetHashCode() : 0);
				result = (result * 397) ^ DictionaryHashCode(Stores);
				result = (result * 397) ^ DictionaryHashCode(Indexes);
				result = (result * 397) ^ DictionaryHashCode(Analyzers);
				result = (result * 397) ^ DictionaryHashCode(SortOptions);
				result = (result * 397) ^ DictionaryHashCode(TermVectors);
				return result;
			}
		}

		public string Type
		{
			get
			{
				var name = Name ?? string.Empty;
				if (name.StartsWith("Temp"))
					return "Temp";
				if (name.StartsWith("Auto"))
					return "Auto";
				if (IsCompiled)
					return "Compiled";
				if (IsMapReduce)
					return "MapReduce";
				return "Map";
			}
		}

		/// <summary>
		/// Remove the default values that we don't actually need
		/// </summary>
		public void RemoveDefaultValues()
		{
			var defaultStorage = IsMapReduce ? FieldStorage.Yes : FieldStorage.No;
			foreach (var toRemove in Stores.Where(x => x.Value == defaultStorage).ToArray())
			{
				Stores.Remove(toRemove);
			}
			foreach (var toRemove in Indexes.Where(x => x.Value == FieldIndexing.Default).ToArray())
			{
				Indexes.Remove(toRemove);
			}
			foreach (var toRemove in SortOptions.Where(x => x.Value == Search.SortOptions.None).ToArray())
			{
				SortOptions.Remove(toRemove);
			}
			foreach (var toRemove in Analyzers.Where(x => string.IsNullOrEmpty(x.Value)).ToArray())
			{
				Analyzers.Remove(toRemove);
			}
			foreach (var toRemove in TermVectors.Where(x => x.Value == FieldTermVector.No).ToArray())
			{
				TermVectors.Remove(toRemove);
			}
		}

		public IndexDefinition Clone()
		{
			var indexDefinition = new IndexDefinition
			{
				Name = Name,
				Reduce = Reduce,
				TransformResults = TransformResults,
				m_cachedHashCodeAsBytes = m_cachedHashCodeAsBytes
			};

			if (Maps != null)
				indexDefinition.Maps = new HashSet<string>(Maps);
			if (Analyzers != null)
				indexDefinition.Analyzers = new Dictionary<string, string>(Analyzers);
			if (Fields != null)
				indexDefinition.Fields = new List<string>(Fields);
			if (Indexes != null)
				indexDefinition.Indexes = new Dictionary<string, FieldIndexing>(Indexes);
			if (SortOptions != null)
				indexDefinition.SortOptions = new Dictionary<string, SortOptions>(SortOptions);
			if (Stores != null)
				indexDefinition.Stores = new Dictionary<string, FieldStorage>(Stores);
			if (TermVectors != null)
				indexDefinition.TermVectors = new Dictionary<string, FieldTermVector>(TermVectors);
			return indexDefinition;
		}
	}
}