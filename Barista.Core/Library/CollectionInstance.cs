namespace Barista.Library
{
    using System.Collections.ObjectModel;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
    public class CollectionInstance<TBaseCollection, TItemInstance, T> : ObjectInstance
        where TBaseCollection : Collection<T>
        where TItemInstance : ObjectInstance
    {
        public CollectionInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        protected CollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
        }

        public TBaseCollection Collection
        {
            get;
            set;
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return this.Collection.Count;
            }
        }

        [JSFunction(Name = "add")]
        public void Add(TItemInstance item)
        {
            var obj = Unwrap(item);
            this.Collection.Add(obj);
        }

        [JSFunction(Name = "clear")]
        public void Clear()
        {
            this.Collection.Clear();
        }

        [JSFunction(Name = "contains")]
        public bool Contains(TItemInstance item)
        {
            var obj = Unwrap(item);
            return this.Collection.Contains(obj);
        }

        [JSFunction(Name = "getItemByIndex")]
        public TItemInstance GetItemByIndex(int index)
        {
            var item = this.Collection[index];
            var wrapped = Wrap(item);
            return wrapped;
        }

        [JSFunction(Name = "indexOf")]
        public int IndexOf(TItemInstance item)
        {
            var obj = Unwrap(item);
            return this.Collection.IndexOf(obj);
        }

        [JSFunction(Name = "insert")]
        public void Insert(int index, TItemInstance item)
        {
            var obj = Unwrap(item);
            this.Collection.Insert(index, obj);
        }

        [JSFunction(Name = "remove")]
        public bool Remove(TItemInstance item)
        {
            var obj = Unwrap(item);
            return this.Collection.Remove(obj);
        }

        [JSFunction(Name = "removeAt")]
        public void RemoveAt(int index)
        {
            this.Collection.RemoveAt(index);
        }

        [JSFunction(Name = "setItemByIndex")]
        public void SetItemByIndex(int index, TItemInstance item)
        {
            if (item == null)
                this.Collection[index] = default(T);
            else
            {
                var obj = Unwrap(item);
                this.Collection[index] = obj;
            }
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (var i in this.Collection)
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
