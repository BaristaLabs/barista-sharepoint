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
            this.PopulateFunctions();
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
                return this.List.Count;
            }
        }

        [JSFunction(Name = "add")]
        public void Add(TItemInstance item)
        {
            var obj = Unwrap(item);
            this.List.Add(obj);
        }

        //AsReadOnly
        //Binarysearch

        [JSFunction(Name = "clear")]
        public void Clear()
        {
            this.List.Clear();
        }

        [JSFunction(Name = "contains")]
        public bool Contains(TItemInstance item)
        {
            var obj = Unwrap(item);
            return this.List.Contains(obj);
        }

        [JSFunction(Name = "getItemByIndex")]
        public TItemInstance GetItemByIndex(int index)
        {
            var item = this.List[index];
            var wrapped = Wrap(item);
            return wrapped;
        }

        [JSFunction(Name = "indexOf")]
        public int IndexOf(TItemInstance item)
        {
            var obj = Unwrap(item);
            return this.List.IndexOf(obj);
        }

        [JSFunction(Name = "insert")]
        public void Insert(int index, TItemInstance item)
        {
            var obj = Unwrap(item);
            this.List.Insert(index, obj);
        }

        [JSFunction(Name = "remove")]
        public bool Remove(TItemInstance item)
        {
            var obj = Unwrap(item);
            return this.List.Remove(obj);
        }

        [JSFunction(Name = "removeAt")]
        public void RemoveAt(int index)
        {
            this.List.RemoveAt(index);
        }

        [JSFunction(Name = "reverse")]
        public void Reverse()
        {
// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            this.List.Reverse();
        }

        [JSFunction(Name = "setItemByIndex")]
        public void SetItemByIndex(int index, TItemInstance item)
        {
            if (item == null)
                this.List[index] = default(T);
            else
            {
                var obj = Unwrap(item);
                this.List[index] = obj;
            }
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (var i in this.List)
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
