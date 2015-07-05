namespace Barista.Library
{
    using System.Collections.Generic;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using System.Linq;

    [Serializable]
    public class ListInstance<TItemInstance, T> : ObjectInstance
        where TItemInstance : ObjectInstance
    {
        public ListInstance(ScriptEngine engine)
            : base(engine)
        {
            PopulateFunctions();
        }

        protected ListInstance(ObjectInstance prototype)
            : base(prototype)
        {
        }

        public IList<T> List
        {
            get;
            protected set;
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return List.Count;
            }
        }

        [JSFunction(Name = "add")]
        public virtual void Add(TItemInstance item)
        {
            var obj = Unwrap(item);
            List.Add(obj);
        }

        //AsReadOnly
        //Binarysearch

        [JSFunction(Name = "clear")]
        public virtual void Clear()
        {
            List.Clear();
        }

        [JSFunction(Name = "contains")]
        public bool Contains(TItemInstance item)
        {
            var obj = Unwrap(item);
            return List.Contains(obj);
        }

        [JSFunction(Name = "getItemByIndex")]
        public TItemInstance GetItemByIndex(int index)
        {
            var item = List[index];
            var wrapped = Wrap(item);
            return wrapped;
        }

        [JSFunction(Name = "indexOf")]
        public int IndexOf(TItemInstance item)
        {
            var obj = Unwrap(item);
            return List.IndexOf(obj);
        }

        [JSFunction(Name = "insert")]
        public void Insert(int index, TItemInstance item)
        {
            var obj = Unwrap(item);
            List.Insert(index, obj);
        }

        [JSFunction(Name = "remove")]
        public bool Remove(TItemInstance item)
        {
            var obj = Unwrap(item);
            return List.Remove(obj);
        }

        [JSFunction(Name = "removeAt")]
        public void RemoveAt(int index)
        {
            List.RemoveAt(index);
        }

        [JSFunction(Name = "reverse")]
        public void Reverse()
        {
// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            List.Reverse();
        }

        [JSFunction(Name = "setItemByIndex")]
        public void SetItemByIndex(int index, TItemInstance item)
        {
            if (item == null)
                List[index] = default(T);
            else
            {
                var obj = Unwrap(item);
                List[index] = obj;
            }
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = Engine.Array.Construct();
            foreach (var i in List)
                ArrayInstance.Push(result, Wrap(i));

            return result;
        }

        protected virtual TItemInstance Wrap(T item)
        {
            throw new NotImplementedException();
        }

        protected virtual T Unwrap(TItemInstance item)
        {
            throw new NotImplementedException();
        }
    }
}
