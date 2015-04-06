﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barista.NiL.JS.Core;
using Barista.NiL.JS.Core.Modules;
using Barista.NiL.JS.Core.TypeProxing;

namespace Barista.NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Array : JSObject
    {
        [Hidden]
        private sealed class _lengthField : JSObject
        {
            private Array array;

            public _lengthField(Array owner)
            {
                attributes |= JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.Reassign;
                array = owner;
                if ((long)(int)array.data.Length == array.data.Length)
                {
                    this.iValue = (int)array.data.Length;
                    this.valueType = JSObjectType.Int;
                }
                else
                {
                    this.dValue = array.data.Length;
                    this.valueType = JSObjectType.Double;
                }
            }

            public override void Assign(JSObject value)
            {
                var nlenD = Tools.JSObjectToDouble(value);
                var nlen = (uint)nlenD;
                if (double.IsNaN(nlenD) || double.IsInfinity(nlenD) || nlen != nlenD)
                    throw new JSException(new RangeError("Invalid array length"));
                if ((attributes & JSObjectAttributesInternal.ReadOnly) != 0)
                    return;
                array.setLength(nlen);
                if ((long)(int)array.data.Length == array.data.Length)
                {
                    this.iValue = (int)array.data.Length;
                    this.valueType = JSObjectType.Int;
                }
                else
                {
                    this.dValue = array.data.Length;
                    this.valueType = JSObjectType.Double;
                }
            }
        }

        private static readonly SparseArray<JSObject> emptyData = new SparseArray<JSObject>();
        [Hidden]
        internal SparseArray<JSObject> data;

        [DoNotEnumerate]
        public Array()
        {
            oValue = this;
            valueType = JSObjectType.Object;
            data = new SparseArray<JSObject>();
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Array(int length)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (length < 0)
                throw new JSException((new RangeError("Invalid array length.")));
            data = new SparseArray<JSObject>();
            if (length > 0)
                data[length - 1] = null;
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [Hidden]
        [CLSCompliant(false)]
        public Array(long length)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (length < 0 || (uint)length > uint.MaxValue)
                throw new JSException((new RangeError("Invalid array length.")));
            data = new SparseArray<JSObject>((int)length);
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Array(double d)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (((long)d != d) || (d < 0) || (d > 0xffffffff))
                throw new JSException((new RangeError("Invalid array length.")));
            data = new SparseArray<JSObject>();
            if (d > 0)
                data[(int)((uint)d - 1)] = null;
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Array(Arguments args)
        {
            if (args == null)
                throw new NullReferenceException("args is null");
            oValue = this;
            valueType = JSObjectType.Object;
            data = new SparseArray<JSObject>();
            for (var i = 0; i < args.length; i++)
                data[i] = args[i].CloneImpl();
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [Hidden]
        public Array(ICollection collection)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (collection == null)
                throw new ArgumentNullException("collection");
            data = new SparseArray<JSObject>();
            var index = 0;
            foreach (var e in collection)
                data[index++] = (e as JSObject ?? TypeProxy.Proxy(e)).CloneImpl();
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [Hidden]
        public Array(IEnumerable enumerable)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            data = new SparseArray<JSObject>();
            var index = 0;
            foreach (var e in enumerable)
                data[index++] = (e as JSObject ?? TypeProxy.Proxy(e)).CloneImpl();
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [Hidden]
        internal Array(IEnumerator enumerator)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (enumerator == null)
                throw new ArgumentNullException("enumerator");
            data = new SparseArray<JSObject>();
            var index = 0;
            while (enumerator.MoveNext())
            {
                var e = enumerator.Current;
                data[index++] = (e as JSObject ?? TypeProxy.Proxy(e)).CloneImpl();
            }
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) == 0)
                throw new InvalidOperationException("Try to assign to array");
        }

        [Hidden]
        public void Add(JSObject obj)
        {
            data.Add(obj);
        }

        private _lengthField _lengthObj;
        [Hidden]
        public JSObject length
        {
            [Hidden]
            get
            {
                if (_lengthObj == null)
                    _lengthObj = new _lengthField(this);
                if (data.Length <= int.MaxValue)
                {
                    _lengthObj.iValue = (int)data.Length;
                    _lengthObj.valueType = JSObjectType.Int;
                }
                else
                {
                    _lengthObj.dValue = data.Length > uint.MaxValue ? 0 : data.Length;
                    _lengthObj.valueType = JSObjectType.Double;
                }
                return _lengthObj;
            }
        }

        [Hidden]
        internal bool setLength(long nlen)
        {
            if (data.Length == nlen)
                return true;
            if (nlen < 0)
                throw new JSException(new RangeError("Invalid array length"));
            if (data.Length > nlen)
            {
                var res = true;
                foreach (var element in data.Reversed)
                {
                    if ((uint)element.Key < nlen)
                        break;
                    if (element.Value != null
                        && element.Value.IsExist
                        && (element.Value.attributes & JSObjectAttributesInternal.DoNotDelete) != 0)
                    {
                        nlen = element.Key;
                        res = false;
                    }
                }
                if (!res)
                {
                    setLength(nlen + 1); // áåñêîíå÷íîé ðåêóðñèè íå ìîæåò áûòü.
                    return false;
                }
            }
            if (data.Length > nlen) do
                {
                    data.RemoveAt((int)(data.Length - 1));
                    data.Trim();
                }
                while (data.Length > nlen);
            if (data.Length != nlen)
                data[(int)nlen - 1] = data[(int)nlen - 1];
            return true;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject concat(JSObject self, Arguments args)
        {
            Array res = null;
            var lenObj = self.GetMember("length", true);
            if (!lenObj.IsDefinded)
            {
                if (self.valueType < JSObjectType.Object)
                    self = self.ToObject();
                res = new Array() { self };
            }
            else
                res = Tools.iterableToArray(self, true, true, false, -1);
            for (var i = 0; i < args.length; i++)
            {
                var v = args[i];
                var varr = v.oValue as Array;
                if (varr != null)
                {
                    varr = Tools.iterableToArray(varr, true, false, false, -1);
                    for (var ai = 0; ai < varr.data.Length; ai++)
                    {
                        var item = varr.data[ai];
                        res.data.Add(item);
                    }
                }
                else
                {
                    res.data.Add(v.CloneImpl());
                }
            }
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject every(JSObject self, Arguments args)
        {
            bool nativeMode = self.GetType() == typeof(Array);
            Array src;
            if (!self.IsDefinded || (self.valueType >= JSObjectType.Object && self.oValue == null))
                throw new JSException(new TypeError("Can not call Array.prototype.every for null or undefined"));
            if (!nativeMode)
                src = Tools.iterableToArray(self, false, false, false, -1);
            else
                src = self as Array;

            Function f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = new JSObject();
            ao[1] = new JSObject();
            ao[2] = self;
            if (self.valueType < JSObjectType.Object)
            {
                ao[2] = new JSObject() { valueType = JSObjectType.Object, __proto__ = self.__proto__, fields = src.fields };
                ao[2].oValue = self;
            }
            var tb = args.Length > 1 ? args[1] : null;
            var context = Context.CurrentContext;
            IEnumerator<KeyValuePair<int, JSObject>> alternativeEnum = null;
            long prew = -1;
            var _length = src.data.Length;
            var mainEnum = (src.data as IEnumerable<KeyValuePair<int, JSObject>>).GetEnumerator();
            bool moved = true;
            while (moved)
            {
                moved = mainEnum.MoveNext();
                while (moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    moved = mainEnum.MoveNext();
                var element = mainEnum.Current;
                uint key = (uint)element.Key;
                if (!moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    key = _length;
                bool alter = false;
                var value = element.Value;
                if (key - prew > 1 || (!moved && key < _length))
                {
                    if (alternativeEnum == null)
                    {
                        alternativeEnum = (Tools.iterableToArray(self.__proto__, false, false, false, _length).data as IEnumerable<KeyValuePair<int, JSObject>>).GetEnumerator();
                        alternativeEnum.MoveNext();
                    }
                    alter = true;
                }
                do
                {
                    if (alter)
                    {
                        bool amoved = (uint)alternativeEnum.Current.Key > prew || alternativeEnum.MoveNext();
                        while ((uint)alternativeEnum.Current.Key <= prew && amoved)
                            amoved = alternativeEnum.MoveNext();
                        if (amoved && (uint)alternativeEnum.Current.Key > prew
                                   && ((uint)alternativeEnum.Current.Key < (uint)element.Key
                                        || !moved && (uint)alternativeEnum.Current.Key < _length))
                        {
                            key = (uint)alternativeEnum.Current.Key;
                            value = alternativeEnum.Current.Value;
                        }
                        else
                        {
                            alter = false;
                            value = element.Value;
                            key = (uint)element.Key;
                        }
                    }
                    if (key >= _length
                        || (!alter && !moved))
                        break;
                    prew = key;
                    if (value == null || !value.IsExist)
                        continue;
                    if (value.valueType == JSObjectType.Property)
                        value = (value.oValue as PropertyPair).get == null ? undefined : (value.oValue as PropertyPair).get.Invoke(self, null);
                    ao[0].Assign(value);
                    ao[1].Assign(context.wrap(key));
                    if (!(bool)f.Invoke(tb, ao))
                        return false;
                }
                while (alter);
            }
            return true;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject some(JSObject self, Arguments args)
        {
            bool nativeMode = self.GetType() == typeof(Array);
            Array src;
            if (!self.IsDefinded || (self.valueType >= JSObjectType.Object && self.oValue == null))
                throw new JSException(new TypeError("Can not call Array.prototype.some for null or undefined"));
            if (!nativeMode)
                src = Tools.iterableToArray(self, false, false, false, -1);
            else
                src = self as Array;

            Function f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = new JSObject();
            ao[1] = new JSObject();
            ao[2] = self;
            if (self.valueType < JSObjectType.Object)
            {
                ao[2] = new JSObject() { valueType = JSObjectType.Object, __proto__ = self.__proto__, fields = src.fields };
                ao[2].oValue = self;
            }
            var tb = args.Length > 1 ? args[1] : null;
            var context = Context.CurrentContext;
            IEnumerator<KeyValuePair<int, JSObject>> alternativeEnum = null;
            long prew = -1;
            var _length = src.data.Length;
            var mainEnum = (src.data as IEnumerable<KeyValuePair<int, JSObject>>).GetEnumerator();
            bool moved = true;
            while (moved)
            {
                moved = mainEnum.MoveNext();
                while (moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    moved = mainEnum.MoveNext();
                var element = mainEnum.Current;
                uint key = (uint)element.Key;
                if (!moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    key = _length;
                bool alter = false;
                var value = element.Value;
                if (key - prew > 1 || (!moved && key < _length))
                {
                    if (alternativeEnum == null)
                    {
                        alternativeEnum = (Tools.iterableToArray(self.__proto__, false, false, false, _length).data as IEnumerable<KeyValuePair<int, JSObject>>).GetEnumerator();
                        alternativeEnum.MoveNext();
                    }
                    alter = true;
                }
                do
                {
                    if (alter)
                    {
                        bool amoved = (uint)alternativeEnum.Current.Key > prew || alternativeEnum.MoveNext();
                        while ((uint)alternativeEnum.Current.Key <= prew && amoved)
                            amoved = alternativeEnum.MoveNext();
                        if (amoved && (uint)alternativeEnum.Current.Key > prew
                                   && ((uint)alternativeEnum.Current.Key < (uint)element.Key
                                        || !moved && (uint)alternativeEnum.Current.Key < _length))
                        {
                            key = (uint)alternativeEnum.Current.Key;
                            value = alternativeEnum.Current.Value;
                        }
                        else
                        {
                            alter = false;
                            value = element.Value;
                            key = (uint)element.Key;
                        }
                    }
                    if (key >= _length
                        || (!alter && !moved))
                        break;
                    prew = key;
                    if (value == null || !value.IsExist)
                        continue;
                    if (value.valueType == JSObjectType.Property)
                        value = (value.oValue as PropertyPair).get == null ? undefined : (value.oValue as PropertyPair).get.Invoke(self, null);
                    ao[0].Assign(value);
                    ao[1].Assign(context.wrap(key));
                    if ((bool)f.Invoke(tb, ao))
                        return true;
                }
                while (alter);
            }
            return false;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject filter(JSObject self, Arguments args)
        {
            bool nativeMode = self.GetType() == typeof(Array);
            Array src;
            if (!self.IsDefinded || (self.valueType >= JSObjectType.Object && self.oValue == null))
                throw new JSException(new TypeError("Can not call Array.prototype.filter for null or undefined"));
            if (!nativeMode)
                src = Tools.iterableToArray(self, false, false, false, -1);
            else
                src = self as Array;

            Function f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = new JSObject();
            ao[1] = new JSObject();
            ao[2] = self;
            if (self.valueType < JSObjectType.Object)
            {
                ao[2] = new JSObject() { valueType = JSObjectType.Object, __proto__ = self.__proto__, fields = src.fields };
                ao[2].oValue = self;
            }
            var tb = args.Length > 1 ? args[1] : null;
            var context = Context.CurrentContext;
            IEnumerator<KeyValuePair<int, JSObject>> alternativeEnum = null;
            long prew = -1;
            var _length = (src as Array).data.Length;
            var mainEnum = ((src as Array).data as IEnumerable<KeyValuePair<int, JSObject>>).GetEnumerator();
            var res = new SparseArray<JSObject>();
            bool moved = true;
            while (moved)
            {
                moved = mainEnum.MoveNext();
                while (moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    moved = mainEnum.MoveNext();
                var element = mainEnum.Current;
                uint key = (uint)element.Key;
                if (!moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    key = _length;
                bool alter = false;
                var value = element.Value;
                if (key - prew > 1 || (!moved && key < _length))
                {
                    if (alternativeEnum == null)
                    {
                        alternativeEnum = (Tools.iterableToArray(self.__proto__, false, false, false, _length).data as IEnumerable<KeyValuePair<int, JSObject>>).GetEnumerator();
                        alternativeEnum.MoveNext();
                    }
                    alter = true;
                }
                do
                {
                    if (alter)
                    {
                        bool amoved = (uint)alternativeEnum.Current.Key > prew || alternativeEnum.MoveNext();
                        while ((uint)alternativeEnum.Current.Key <= prew && amoved)
                            amoved = alternativeEnum.MoveNext();
                        if (amoved && (uint)alternativeEnum.Current.Key > prew
                                   && ((uint)alternativeEnum.Current.Key < (uint)element.Key
                                        || !moved && (uint)alternativeEnum.Current.Key < _length))
                        {
                            key = (uint)alternativeEnum.Current.Key;
                            value = alternativeEnum.Current.Value;
                        }
                        else
                        {
                            alter = false;
                            value = element.Value;
                            key = (uint)element.Key;
                        }
                    }
                    if (key >= _length
                        || (!alter && !moved))
                        break;
                    prew = key;
                    if (value == null || !value.IsExist)
                        continue;
                    if (value.valueType == JSObjectType.Property)
                        value = (value.oValue as PropertyPair).get == null ? undefined : (value.oValue as PropertyPair).get.Invoke(self, null);
                    ao[0].Assign(value);
                    ao[1].Assign(context.wrap(key));
                    if ((bool)f.Invoke(tb, ao))
                        res[(int)(uint)key] = value.CloneImpl();
                }
                while (alter);
            }
            var rres = new Array();
            foreach (var item in res)
                if (item != null)
                    rres.data.Add(item);
            return rres;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject map(JSObject self, Arguments args)
        {
            bool nativeMode = self.GetType() == typeof(Array);
            Array src;
            if (!self.IsDefinded || (self.valueType >= JSObjectType.Object && self.oValue == null))
                throw new JSException(new TypeError("Can not call Array.prototype.map for null or undefined"));
            if (!nativeMode)
                src = Tools.iterableToArray(self, false, false, false, -1);
            else
                src = self as Array;

            Function f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = new JSObject();
            ao[1] = new JSObject();
            ao[2] = self;
            if (self.valueType < JSObjectType.Object)
            {
                ao[2] = new JSObject() { valueType = JSObjectType.Object, __proto__ = self.__proto__, fields = src.fields };
                ao[2].oValue = self;
            }
            var tb = args.Length > 1 ? args[1] : null;
            var context = Context.CurrentContext;
            IEnumerator<KeyValuePair<int, JSObject>> alternativeEnum = null;
            long prew = -1;
            var _length = (src as Array).data.Length;
            var mainEnum = ((src as Array).data as IEnumerable<KeyValuePair<int, JSObject>>).GetEnumerator();
            var res = new Array();
            bool moved = true;
            while (moved)
            {
                moved = mainEnum.MoveNext();
                while (moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    moved = mainEnum.MoveNext();
                var element = mainEnum.Current;
                uint key = (uint)element.Key;
                if (!moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    key = _length;
                bool alter = false;
                var value = element.Value;
                if (key - prew > 1 || (!moved && key < _length))
                {
                    if (alternativeEnum == null)
                    {
                        alternativeEnum = (Tools.iterableToArray(self.__proto__, false, false, false, _length).data as IEnumerable<KeyValuePair<int, JSObject>>).GetEnumerator();
                        alternativeEnum.MoveNext();
                    }
                    alter = true;
                }
                do
                {
                    if (alter)
                    {
                        bool amoved = (uint)alternativeEnum.Current.Key > prew || alternativeEnum.MoveNext();
                        while ((uint)alternativeEnum.Current.Key <= prew && amoved)
                            amoved = alternativeEnum.MoveNext();
                        if (amoved && (uint)alternativeEnum.Current.Key > prew
                                   && ((uint)alternativeEnum.Current.Key < (uint)element.Key
                                        || !moved && (uint)alternativeEnum.Current.Key < _length))
                        {
                            key = (uint)alternativeEnum.Current.Key;
                            value = alternativeEnum.Current.Value;
                        }
                        else
                        {
                            alter = false;
                            value = element.Value;
                            key = (uint)element.Key;
                        }
                    }
                    if (key >= _length
                        || (!alter && !moved))
                        break;
                    prew = key;
                    if (value == null || !value.IsExist)
                        continue;
                    if (value.valueType == JSObjectType.Property)
                        value = (value.oValue as PropertyPair).get == null ? undefined : (value.oValue as PropertyPair).get.Invoke(self, null);
                    ao[0].Assign(value);
                    ao[1].Assign(context.wrap(key));
                    res.data[(int)key] = f.Invoke(tb, ao).CloneImpl();
                }
                while (alter);
            }
            res.data[(int)_length - 1] = res.data[(int)_length - 1];
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject forEach(JSObject self, Arguments args)
        {
            bool nativeMode = self.GetType() == typeof(Array);
            Array src;
            if (!self.IsDefinded || (self.valueType >= JSObjectType.Object && self.oValue == null))
                throw new JSException(new TypeError("Can not call Array.prototype.forEach for null or undefined"));
            if (!nativeMode)
                src = Tools.iterableToArray(self, false, false, false, -1);
            else
                src = self as Array;

            Function f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = new JSObject();
            ao[1] = new JSObject();
            ao[2] = self;
            if (self.valueType < JSObjectType.Object)
            {
                ao[2] = new JSObject() { valueType = JSObjectType.Object, __proto__ = self.__proto__, fields = src.fields };
                ao[2].oValue = self;
            }
            var tb = args.Length > 1 ? args[1] : null;
            var context = Context.CurrentContext;
            IEnumerator<KeyValuePair<int, JSObject>> alternativeEnum = null;
            long prew = -1;
            var _length = (src as Array).data.Length;
            var mainEnum = ((src as Array).data as IEnumerable<KeyValuePair<int, JSObject>>).GetEnumerator();
            bool moved = true;
            while (moved)
            {
                moved = mainEnum.MoveNext();
                while (moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    moved = mainEnum.MoveNext();
                var element = mainEnum.Current;
                uint key = (uint)element.Key;
                if (!moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    key = _length;
                bool alter = false;
                var value = element.Value;
                if (key - prew > 1 || (!moved && key < _length))
                {
                    if (alternativeEnum == null)
                    {
                        alternativeEnum = (Tools.iterableToArray(self.__proto__, false, false, false, _length).data as IEnumerable<KeyValuePair<int, JSObject>>).GetEnumerator();
                        alternativeEnum.MoveNext();
                    }
                    alter = true;
                }
                do
                {
                    if (alter)
                    {
                        bool amoved = (uint)alternativeEnum.Current.Key > prew || alternativeEnum.MoveNext();
                        while ((uint)alternativeEnum.Current.Key <= prew && amoved)
                            amoved = alternativeEnum.MoveNext();
                        if (amoved && (uint)alternativeEnum.Current.Key > prew
                                   && ((uint)alternativeEnum.Current.Key < (uint)element.Key
                                        || !moved && (uint)alternativeEnum.Current.Key < _length))
                        {
                            key = (uint)alternativeEnum.Current.Key;
                            value = alternativeEnum.Current.Value;
                        }
                        else
                        {
                            alter = false;
                            value = element.Value;
                            key = (uint)element.Key;
                        }
                    }
                    if (key >= _length
                        || (!alter && !moved))
                        break;
                    prew = key;
                    if (value == null || !value.IsExist)
                        continue;
                    if (value.valueType == JSObjectType.Property)
                        value = (value.oValue as PropertyPair).get == null ? undefined : (value.oValue as PropertyPair).get.Invoke(self, null);
                    ao[0].Assign(value);
                    ao[1].Assign(context.wrap(key));
                    f.Invoke(tb, ao);
                }
                while (alter);
            }
            return null;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject indexOf(JSObject self, Arguments args)
        {
            bool nativeMode = self.GetType() == typeof(Array);
            Array src;
            if (!self.IsDefinded || (self.valueType >= JSObjectType.Object && self.oValue == null))
                throw new JSException(new TypeError("Can not call Array.prototype.every for null or undefined"));
            var _length = nativeMode ? (self as Array).data.Length : Tools.getLengthOfIterably(self, false);
            var fromIndex = args.length > 1 ? Tools.JSObjectToInt64(args[1], 0, true) : 0;
            if (fromIndex < 0)
                fromIndex += _length;

            if (!nativeMode)
                src = Tools.iterableToArray(self, false, false, false, _length);
            else
                src = self as Array;
            JSObject image = args[0];
            IEnumerator<KeyValuePair<int, JSObject>> alternativeEnum = null;
            long prew = fromIndex - 1;
            var mainEnum = (src.data as IEnumerable<KeyValuePair<int, JSObject>>).GetEnumerator();
            bool moved = true;
            while (moved)
            {
                moved = mainEnum.MoveNext();
                while (moved && ((uint)mainEnum.Current.Key < fromIndex || mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    moved = mainEnum.MoveNext();
                var element = mainEnum.Current;
                uint key = (uint)element.Key;
                if (!moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    key = (uint)_length;
                bool alter = false;
                var value = element.Value;
                if (key - prew > 1 || (!moved && key < _length))
                {
                    if (alternativeEnum == null)
                    {
                        alternativeEnum = (Tools.iterableToArray(self.__proto__, false, false, false, _length).data as IEnumerable<KeyValuePair<int, JSObject>>).GetEnumerator();
                        alternativeEnum.MoveNext();
                    }
                    alter = true;
                }
                do
                {
                    if (alter)
                    {
                        bool amoved = (uint)alternativeEnum.Current.Key > prew || alternativeEnum.MoveNext();
                        while ((uint)alternativeEnum.Current.Key <= prew && amoved)
                            amoved = alternativeEnum.MoveNext();
                        if (amoved && (uint)alternativeEnum.Current.Key > prew
                                   && ((uint)alternativeEnum.Current.Key < (uint)element.Key
                                        || !moved && (uint)alternativeEnum.Current.Key < _length))
                        {
                            key = (uint)alternativeEnum.Current.Key;
                            value = alternativeEnum.Current.Value;
                        }
                        else
                        {
                            alter = false;
                            value = element.Value;
                            key = (uint)element.Key;
                        }
                    }
                    if (key >= _length
                        || (!alter && !moved))
                        break;
                    prew = key;
                    if (value == null || !value.IsExist)
                        continue;
                    if (value.valueType == JSObjectType.Property)
                        value = (value.oValue as PropertyPair).get == null ? undefined : (value.oValue as PropertyPair).get.Invoke(self, null);
                    if (Expressions.StrictEqual.Check(value, image))
                        return key;
                }
                while (alter);
            }
            return -1;
        }

        [DoNotEnumerateAttribute]
        public static JSObject isArray(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            return args[0].Value is Array || args[0].Value == TypeProxy.GetPrototype(typeof(Array));
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject join(JSObject self, Arguments separator)
        {
            return joinImpl(self, separator == null || separator.length == 0 || !separator[0].IsDefinded ? "," : separator[0].ToString(), false);
        }

        private static string joinImpl(JSObject self, string separator, bool locale)
        {
            if ((self.oValue == null && self.valueType >= JSObjectType.Object) || self.valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("Array.prototype.join called for null or undefined"));
            if (self.valueType >= JSObjectType.Object && self.oValue.GetType() == typeof(Array))
            {
                var selfa = self as Array;
                if (selfa.data.Length == 0)
                    return "";
                var _data = selfa.data;
                try
                {
                    selfa.data = emptyData;
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    JSObject t;
                    JSObject temp = 0;
                    for (var i = 0L; i < (long)_data.Length; i++)
                    {
                        t = _data[(int)i];
                        if (i <= int.MaxValue)
                            temp.iValue = (int)i;
                        else
                        {
                            temp.dValue = i;
                            temp.valueType = JSObjectType.Double;
                        }
                        if (((t != null && t.IsExist) || null != (t = self.GetMember(temp, false, false)))
                            && t.IsDefinded)
                        {
                            if (t.valueType < JSObjectType.String || t.oValue != null)
                                sb.Append(locale ? t.ToPrimitiveValue_LocaleString_Value() : t.ToPrimitiveValue_String_Value());
                        }
                        sb.Append(separator);
                    }
                    sb.Length -= separator.Length;
                    return sb.ToString();
                }
                finally
                {
                    selfa.data = _data;
                }
            }
            else
            {
                return joinImpl(Tools.iterableToArray(self, true, false, false, -1), separator, locale);
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject lastIndexOf(JSObject self, Arguments args)
        {
            bool nativeMode = self.GetType() == typeof(Array);
            Array src;
            if (!self.IsDefinded || (self.valueType >= JSObjectType.Object && self.oValue == null))
                throw new JSException(new TypeError("Can not call Array.prototype.every for null or undefined"));

            var _length = nativeMode ? (self as Array).data.Length : Tools.getLengthOfIterably(self, false);
            var fromIndex = args.length > 1 ? Tools.JSObjectToInt64(args[1], 0, true) : _length;
            if (fromIndex < 0)
                fromIndex += _length;

            if (!nativeMode)
                src = Tools.iterableToArray(self, false, false, false, _length);
            else
                src = self as Array;

            JSObject image = args[0];
            IEnumerator<KeyValuePair<int, JSObject>> alternativeEnum = null;
            long prew = fromIndex + 1;
            var mainEnum = (src as Array).data.Reversed.GetEnumerator();
            bool moved = true;
            while (moved)
            {
                moved = mainEnum.MoveNext();
                while (moved && ((uint)mainEnum.Current.Key > fromIndex || mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    moved = mainEnum.MoveNext();
                var element = mainEnum.Current;
                uint key = (uint)element.Key;
                if (!moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.IsExist))
                    key = 0;
                bool alter = false;
                var value = element.Value;
                if (prew - key > 1 || !moved)
                {
                    if (alternativeEnum == null)
                    {
                        alternativeEnum = Tools.iterableToArray(self.__proto__, false, false, false, _length).data.Reversed.GetEnumerator();
                        alternativeEnum.MoveNext();
                    }
                    alter = true;
                }
                do
                {
                    if (alter)
                    {
                        bool amoved = (uint)alternativeEnum.Current.Key < prew || alternativeEnum.MoveNext();
                        while ((uint)alternativeEnum.Current.Key >= prew && amoved)
                            amoved = alternativeEnum.MoveNext();
                        if (amoved && (uint)alternativeEnum.Current.Key < prew
                                   && ((uint)alternativeEnum.Current.Key > (uint)element.Key
                                        || !moved))
                        {
                            key = (uint)alternativeEnum.Current.Key;
                            value = alternativeEnum.Current.Value;
                        }
                        else
                        {
                            alter = false;
                            value = element.Value;
                            key = (uint)element.Key;
                        }
                    }
                    if (key >= _length
                        || (!alter && !moved))
                        break;
                    prew = key;
                    if (value == null || !value.IsExist)
                        continue;
                    if (value.valueType == JSObjectType.Property)
                        value = (value.oValue as PropertyPair).get == null ? undefined : (value.oValue as PropertyPair).get.Invoke(self, null);
                    if (Expressions.StrictEqual.Check(value, image))
                        return key;
                }
                while (alter);
            }
            return -1;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSObject pop(JSObject self)
        {
            notExists.valueType = JSObjectType.NotExistsInObject;
            if (self.GetType() == typeof(Array))
            {
                var selfa = self as Array;
                if (selfa.data.Length == 0)
                    return notExists;
                int newLen = (int)(selfa.data.Length - 1);
                var res = selfa.data[newLen] ?? self[newLen.ToString()];
                if (res.valueType == JSObjectType.Property)
                    res = ((res.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null);
                selfa.data.RemoveAt(newLen);
                selfa.data[newLen - 1] = selfa.data[newLen - 1];
                return res;
            }
            else
            {
                var length = Tools.getLengthOfIterably(self, true);
                if (length <= 0 || length > uint.MaxValue)
                    return notExists;
                length--;
                var tres = self.GetMember(Context.CurrentContext.wrap(length.ToString()), true, false);
                JSObject res;
                if (tres.valueType == JSObjectType.Property)
                    res = ((tres.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null);
                else
                    res = tres.CloneImpl();
                if ((tres.attributes & JSObjectAttributesInternal.DoNotDelete) == 0)
                {
                    tres.oValue = null;
                    tres.valueType = JSObjectType.NotExistsInObject;
                }
                self["length"] = length;
                return res;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject push(JSObject self, Arguments args)
        {
            notExists.valueType = JSObjectType.NotExistsInObject;
            if (self.GetType() == typeof(Array))
            {
                var selfa = self as Array;
                for (var i = 0; i < args.length; i++)
                {
                    if (selfa.data.Length == uint.MaxValue)
                    {
                        if (selfa.fields == null)
                            selfa.fields = createFields();
                        selfa.fields[uint.MaxValue.ToString()] = args.a0.CloneImpl();
                        throw new JSException(new RangeError("Invalid length of array"));
                    }
                    selfa.data.Add(args[i].CloneImpl());
                }
                return selfa.length;
            }
            else
            {
                var length = Tools.getLengthOfIterably(self, false);
                var i = length;
                length += args.length;
                self["length"] = length;
                for (var j = 0; i < length; i++, j++)
                    self[i.ToString()] = args[j].CloneImpl();
                return length;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSObject reverse(JSObject self)
        {
            Arguments args = null;
            if (self.GetType() == typeof(Array))
            {
                var selfa = self as Array;
                for (var i = selfa.data.Length >> 1; i-- > 0; )
                {
                    var item0 = selfa.data[(int)(selfa.data.Length - 1 - i)];
                    var item1 = selfa.data[(int)(i)];
                    JSObject value0, value1;
                    if (item0 == null || !item0.IsExist)
                        item0 = selfa.__proto__[(selfa.data.Length - 1 - i).ToString()];
                    if (item0.valueType == JSObjectType.Property)
                        value0 = ((item0.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null).CloneImpl();
                    else
                        value0 = item0;
                    if (item1 == null || !item1.IsExist)
                        item1 = selfa.__proto__[i.ToString()];
                    if (item1.valueType == JSObjectType.Property)
                        value1 = ((item1.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null).CloneImpl();
                    else
                        value1 = item1;
                    if (item0 != null && item0.valueType == JSObjectType.Property)
                    {
                        if (args == null)
                            args = new Arguments();
                        args.length = 1;
                        args.a0 = item1;
                        ((item0.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, args);
                    }
                    else if (value1.IsExist)
                        selfa.data[(int)(selfa.data.Length - 1 - i)] = value1;
                    else if (item0 != null)
                        selfa.data[(int)(selfa.data.Length - 1 - i)] = null;

                    if (item1 != null && item1.valueType == JSObjectType.Property)
                    {
                        if (args == null)
                            args = new Arguments();
                        args.length = 1;
                        args.a0 = item0;
                        ((item1.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, args);
                    }
                    else if (value0.IsExist)
                        selfa.data[(int)i] = value0;
                    else if (item1 != null)
                        selfa.data[(int)i] = null;
                }
                return self;
            }
            else
            {
                var length = Tools.getLengthOfIterably(self, false);
                if (length > 1)
                    for (var i = 0; i < length >> 1; i++)
                    {
                        JSObject i0 = i.ToString();
                        JSObject i1 = (length - 1 - i).ToString();
                        var item0 = self.GetMember(i0, false, false);
                        var item1 = self.GetMember(i1, false, false);
                        var value0 = item0;
                        var value1 = item1;
                        if (value0.valueType == JSObjectType.Property)
                            value0 = ((item0.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null).CloneImpl();
                        else
                            value0 = value0.CloneImpl();
                        if (value1.valueType == JSObjectType.Property)
                            value1 = ((item1.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null).CloneImpl();
                        else
                            value1 = value1.CloneImpl();

                        if (item0.valueType == JSObjectType.Property)
                        {
                            if (args == null)
                                args = new Arguments();
                            args.length = 1;
                            args.a0 = value1;
                            ((item0.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, args);
                        }
                        else if (value1.IsExist)
                            self.GetMember(i0, true, true).Assign(value1);
                        else
                        {
                            var t = self.GetMember(i0, true, true);
                            if ((t.attributes & JSObjectAttributesInternal.DoNotDelete) == 0)
                            {
                                t.oValue = null;
                                t.valueType = JSObjectType.NotExists;
                            }
                        }
                        if (item1.valueType == JSObjectType.Property)
                        {
                            if (args == null)
                                args = new Arguments();
                            args.length = 1;
                            args.a0 = value0;
                            ((item1.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, args);
                        }
                        else if (value0.IsExist)
                            self.GetMember(i1, true, true).Assign(value0);
                        else
                        {
                            var t = self.GetMember(i1, true, true);
                            if ((t.attributes & JSObjectAttributesInternal.DoNotDelete) == 0)
                            {
                                t.oValue = null;
                                t.valueType = JSObjectType.NotExists;
                            }
                        }
                    }
                return self;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject reduce(JSObject self, Arguments args)
        {
            JSObject _this = self;
            if (_this.valueType < JSObjectType.Object)
            {
                _this = new JSObject()
                {
                    valueType = JSObjectType.Object,
                    __proto__ = self.__proto__,
                    oValue = self
                };
            }

            //if (self.GetType() != typeof(Array)) // при переборе должны быть получены значения из прототипов. 
            // Если просто запрашивать пропущенные индексы, то может быть большое падение производительности в случаях
            // когда пропуски очень огромны и элементов реально нет ни здесь, ни в прототипе
            var src = Tools.iterableToArray(self, false, false, false, -1);
            var func = args[0].oValue as Function;
            var accum = new JSObject() { valueType = JSObjectType.NotExists };
            if (args.length > 1)
                accum.Assign(args[1]);
            bool skip = false;
            if (accum.valueType < JSObjectType.Undefined)
            {
                if (src.data.Length == 0)
                    throw new JSException(new TypeError("Array is empty."));
                skip = true;
            }
            else if (src.data.Length == 0)
                return accum;
            if (func == null || func.valueType != JSObjectType.Function)
                throw new JSException(new TypeError("First argument on reduce mast be a function."));
            if (accum.GetType() != typeof(JSObject))
                accum = accum.CloneImpl();
            args.length = 4;
            args.a1 = new JSObject();
            args[2] = new JSObject();
            bool called = false;
            foreach (var element in (src.data as IEnumerable<KeyValuePair<int, JSObject>>))
            {
                if (self.GetType() == typeof(Array) && (self as Array).data.Length <= (uint)element.Key)
                    break;
                if (element.Value == null || !element.Value.IsExist)
                    continue;
                args[0] = accum;
                if (element.Value.valueType == JSObjectType.Property)
                    args.a1.Assign((element.Value.oValue as PropertyPair).get == null ? undefined : (element.Value.oValue as PropertyPair).get.Invoke(self, null));
                else
                    args.a1.Assign(element.Value);
                called = true;
                if (skip)
                {
                    accum.Assign(args.a1);
                    skip = false;
                    continue;
                }
                if (element.Key >= 0)
                {
                    args.a2.valueType = JSObjectType.Int;
                    args.a2.iValue = element.Key;
                }
                else
                {
                    args.a2.valueType = JSObjectType.Double;
                    args.a2.dValue = (uint)element.Key;
                }
                args[3] = _this;
                accum.Assign(func.Invoke(args));
            }
            if (!called && skip)
                throw new JSException(new TypeError("Array is empty."));
            return accum;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject reduceRight(JSObject self, Arguments args)
        {
            if (self.valueType < JSObjectType.Object)
            {
                self = new JSObject()
                {
                    valueType = JSObjectType.Object,
                    __proto__ = self.__proto__,
                    oValue = self
                };
            }
            //if (self.GetType() != typeof(Array))
            var src = Tools.iterableToArray(self, false, false, false, -1);
            var func = args[0].oValue as Function;
            var accum = new JSObject() { valueType = JSObjectType.NotExists };
            if (args.length > 1)
                accum.Assign(args[1]);
            var context = Context.CurrentContext;
            bool skip = false;
            if (accum.valueType < JSObjectType.Undefined)
            {
                if (src.data.Length == 0)
                    throw new JSException(new TypeError("Array is empty."));
                skip = true;
            }
            else if (src.data.Length == 0)
                return accum;
            if (func == null)
                throw new JSException(new TypeError("First argument on reduceRight mast be a function."));
            if (accum.GetType() != typeof(JSObject))
                accum = accum.CloneImpl();
            args.length = 4;
            args.a1 = new JSObject();
            args[2] = new JSObject();
            bool called = false;
            foreach (var element in src.data.Reversed)
            {
                if (self.GetType() == typeof(Array) && (self as Array).data.Length <= (uint)element.Key)
                    continue;
                if (element.Value == null || !element.Value.IsExist)
                    continue;
                args[0] = accum;
                if (element.Value.valueType == JSObjectType.Property)
                    args.a1.Assign((element.Value.oValue as PropertyPair).get == null ? undefined : (element.Value.oValue as PropertyPair).get.Invoke(self, null));
                else
                    args.a1.Assign(element.Value);
                called = true;
                if (skip)
                {
                    accum.Assign(args.a1);
                    skip = false;
                    continue;
                }
                if (element.Key >= 0)
                {
                    args.a2.valueType = JSObjectType.Int;
                    args.a2.iValue = element.Key;
                }
                else
                {
                    args.a2.valueType = JSObjectType.Double;
                    args.a2.dValue = (uint)element.Key;
                }
                args[3] = self;
                accum.Assign(func.Invoke(args));
            }
            if (!called && skip)
                throw new JSException(new TypeError("Array is empty."));
            return accum;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSObject shift(JSObject self)
        {
            notExists.valueType = JSObjectType.NotExistsInObject;
            if (self.GetType() == typeof(Array))
            {
                var selfa = self as Array;
                if (selfa.data.Length == 0)
                    return notExists;
                var len = selfa.data.Length;
                var res = selfa.data[0];
                try
                {
                    if (res == null || !res.IsExist)
                        res = selfa.__proto__["0"];
                }
                catch
                { }
                selfa.data[0] = null;
                if (res.valueType == JSObjectType.Property)
                    res = ((res.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null);

                var prew = 0U;
                foreach (var item in (selfa.data as IEnumerable<KeyValuePair<int, JSObject>>))
                {
                    if ((uint)item.Key >= len)
                        break;
                    if ((uint)item.Key - prew > 1)
                        break;
                    prew = (uint)item.Key;
                }
                Array source = selfa;
                if (len - prew > 1)
                    source = Tools.iterableToArray(self, false, false, false, -1);

                JSObject prw = null;
                foreach (var item in (source.data as IEnumerable<KeyValuePair<int, JSObject>>))
                {
                    if (item.Key == 0)
                    {
                        prw = item.Value;
                        continue;
                    }
                    var value = item.Value;
                    if (value != null && value.valueType == JSObjectType.Property)
                        value = ((value.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null);
                    if (prw != null && prw.valueType == JSObjectType.Property)
                    {
                        ((prw.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, new Arguments() { a0 = value, length = 1 });
                    }
                    else
                    {
                        if (item.Value != null)
                        {
                            if (item.Value.IsExist)
                                selfa.data[item.Key - 1] = value;
                            if (item.Value.valueType != JSObjectType.Property)
                                selfa.data[item.Key] = null;
                        }
                    }
                    prw = item.Value;
                }
                if (len == 1)
                    selfa.data.Clear();
                else
                {
                    selfa.data.RemoveAt((int)len - 1);
                    selfa.data.Trim();
                    if (len - 1 > selfa.data.Length)
                        selfa.data[(int)len - 2] = selfa.data[(int)len - 2];
                }
                return res;
            }
            else
            {
                var lenObj = self["length"];
                if (lenObj.valueType == JSObjectType.Property)
                    lenObj = ((lenObj.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null);
                long _length = (long)(uint)Tools.JSObjectToDouble(lenObj);
                if (_length > uint.MaxValue)
                    throw new JSException(new RangeError("Invalid array length"));
                if (_length == 0)
                {
                    self["length"] = lenObj = _length;
                    return notExists;
                }
                var ti = new JSObject() { valueType = JSObjectType.String, oValue = "0" };
                var t = self.GetMember(ti, true, false);
                var res = t;
                if (res.valueType == JSObjectType.Property)
                    res = ((res.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null).CloneImpl();
                else
                    res = res.CloneImpl();
                if ((t.attributes & (JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete)) == 0)
                {
                    t.oValue = null;
                    t.valueType = JSObjectType.NotExists;
                }
                if (_length == 1)
                {
                    self["length"] = lenObj = _length - 1;
                    return res;
                }
                var protoSource = Tools.iterableToArray(self, false, true, false, -1);
                self["length"] = lenObj = _length - 1;

                List<string> keysToRemove = new List<string>();
                foreach (var key in self)
                {
                    var pindex = 0;
                    var dindex = 0.0;
                    long lindex = 0;
                    if (Tools.ParseNumber(key, ref pindex, out dindex)
                        && (pindex == key.Length)
                        && (lindex = (long)dindex) == dindex
                        && lindex < _length)
                    {
                        var temp = self[key];
                        if (!temp.IsExist)
                            continue;
                        if (temp.valueType != JSObjectType.Property)
                            keysToRemove.Add(key);
                    }
                }
                var tjo = new JSObject() { valueType = JSObjectType.String };
                for (var i = 0; i < keysToRemove.Count; i++)
                {
                    tjo.oValue = keysToRemove[i];
                    var to = self.GetMember(tjo, true, false);
                    if ((to.attributes & (JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete)) == 0)
                    {
                        to.oValue = null;
                        to.valueType = JSObjectType.NotExists;
                    }
                }
                tjo.valueType = JSObjectType.Int;
                foreach (var item in (protoSource.data as IEnumerable<KeyValuePair<int, JSObject>>))
                {
                    if ((uint)item.Key > int.MaxValue)
                    {
                        tjo.valueType = JSObjectType.Double;
                        tjo.dValue = (uint)(item.Key - 1);
                    }
                    else
                        tjo.iValue = (item.Key - 1);
                    if (item.Value != null && item.Value.IsExist)
                    {
                        var temp = self.GetMember(tjo, true, false);
                        if (temp.valueType == JSObjectType.Property)
                            ((temp.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, new Arguments() { a0 = item.Value, length = 1 });
                        else
                            temp.Assign(item.Value);
                    }
                }
                return res;
            }
        }

        [DoNotEnumerate]
        [ArgumentsLength(2)]
        [InstanceMember]
        public static JSObject slice(JSObject self, Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (!self.IsDefinded || (self.valueType >= JSObjectType.Object && self.oValue == null))
                throw new JSException(new TypeError("Can not call Array.prototype.slice for null or undefined"));
            HashSet<string> processedKeys = null;
            Array res = new Array();
            for (; ; )
            {
                if (self.GetType() == typeof(Array))
                {
                    var selfa = self as Array;
                    var startIndex = Tools.JSObjectToInt64(args[0], 0, true);
                    if (startIndex < 0)
                        startIndex += selfa.data.Length;
                    if (startIndex < 0)
                        startIndex = 0;
                    var endIndex = Tools.JSObjectToInt64(args[1], selfa.data.Length, true);
                    if (endIndex < 0)
                        endIndex += selfa.data.Length;
                    var len = selfa.data.Length;
                    foreach (var element in (selfa.data as IEnumerable<KeyValuePair<int, JSObject>>))
                    {
                        if (element.Key >= len) // ýýý...
                            break;
                        var value = element.Value;
                        if (value == null || !value.IsExist)
                            continue;
                        if (value.valueType == JSObjectType.Property)
                            value = (value.oValue as PropertyPair).get == null ? undefined : (value.oValue as PropertyPair).get.Invoke(self, null);
                        if (processedKeys != null)
                        {
                            var sk = element.Key.ToString();
                            if (processedKeys.Contains(sk))
                                continue;
                            processedKeys.Add(sk);
                        }
                        if (element.Key >= startIndex && element.Key < endIndex)
                            res.data[element.Key - (int)startIndex] = element.Value.CloneImpl();
                    }
                }
                else
                {
                    var lenObj = self["length"]; // òóò æå ïðîâåðêà íà null/undefined ñ ïàäåíèåì åñëè íàäî
                    if (!lenObj.IsDefinded)
                        return res;
                    if (lenObj.valueType == JSObjectType.Property)
                        lenObj = ((lenObj.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null);
                    if (lenObj.valueType >= JSObjectType.Object)
                        lenObj = lenObj.ToPrimitiveValue_Value_String();
                    if (!lenObj.IsDefinded)
                        return new Array();
                    long _length = (uint)Tools.JSObjectToInt64(lenObj);
                    var startIndex = Tools.JSObjectToInt64(args[0], 0, true);
                    if (startIndex < 0)
                        startIndex += _length;
                    if (startIndex < 0)
                        startIndex = 0;
                    var endIndex = Tools.JSObjectToInt64(args[1], _length, true);
                    if (endIndex < 0)
                        endIndex += _length;
                    var @enum = self.GetEnumeratorImpl(false);
                    while (@enum.MoveNext())
                    {
                        var i = @enum.Current;
                        var pindex = 0;
                        var dindex = 0.0;
                        long lindex = 0;
                        if (Tools.ParseNumber(i, ref pindex, out dindex)
                            && (pindex == i.Length)
                            && dindex < _length
                            && (lindex = (long)dindex) == dindex)
                        {
                            var temp = self[i];
                            if (temp.valueType == JSObjectType.Property)
                                temp = ((temp.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null);
                            if (!temp.IsExist)
                                continue;
                            if (processedKeys != null)
                            {
                                var sk = lindex.ToString();
                                if (processedKeys.Contains(sk))
                                    continue;
                                processedKeys.Add(sk);
                            }
                            if (lindex >= startIndex && lindex < endIndex)
                                res.data[(int)lindex - (int)startIndex] = temp.CloneImpl();
                        }
                    }
                }
                var crnt = self;
                if (self.__proto__ == Null)
                    break;
                self = self.__proto__;
                if (self == null || (self.valueType >= JSObjectType.String && self.oValue == null))
                    break;
                if (processedKeys == null)
                    processedKeys = new HashSet<string>(crnt);
            }
            return res;
        }

        [DoNotEnumerate]
        [ArgumentsLength(2)]
        [InstanceMember]
        public static JSObject splice(JSObject self, Arguments args)
        {
            return spliceImpl(self, args, true);
        }

        private static JSObject spliceImpl(JSObject self, Arguments args, bool needResult)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length == 0)
                return needResult ? new Array() : null;
            if (self.GetType() == typeof(Array))
            {
                var selfa = self as Array;
                var _length = selfa.data.Length;
                long pos0 = (long)System.Math.Min(Tools.JSObjectToDouble(args[0]), selfa.data.Length);
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSObjectType.Undefined)
                        pos1 = 0;
                    else
                        pos1 = (long)System.Math.Min(Tools.JSObjectToDouble(args[1]), selfa.data.Length);
                }
                else
                    pos1 = selfa.data.Length;
                if (pos0 < 0)
                    pos0 = selfa.data.Length + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = 0;
                if (pos1 == 0 && args.length <= 2)
                    return needResult ? new Array() : null;
                pos0 = (uint)System.Math.Min(pos0, selfa.data.Length);
                pos1 += pos0;
                pos1 = (uint)System.Math.Min(pos1, selfa.data.Length);
                var res = needResult ? new Array((int)(pos1 - pos0)) : null;
                var delta = System.Math.Max(0, args.length - 2) - (pos1 - pos0);
                foreach (var node in (delta > 0 ? selfa.data.Reversed : (selfa.data as IEnumerable<KeyValuePair<int, JSObject>>)))
                {
                    if (node.Key < pos0)
                        continue;
                    if (node.Key >= pos1 && delta == 0)
                        break;
                    var key = node.Key;
                    var value = node.Value;
                    if (value == null || !value.IsExist)
                    {
                        value = selfa.__proto__[((uint)key).ToString()];
                        if (!value.IsExist)
                            continue;
                        value = value.CloneImpl();
                    }
                    if (value.valueType == JSObjectType.Property)
                        value = ((value.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null).CloneImpl();
                    if (key < pos1)
                    {
                        if (needResult)
                            res.data[(int)(key - pos0)] = value;
                    }
                    else
                    {
                        var t = selfa.data[(int)(key + delta)];
                        if (t != null && t.valueType == JSObjectType.Property)
                            ((t.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, new Arguments() { a0 = value, length = 1 });
                        else
                            selfa.data[(int)(key + delta)] = value;
                        selfa.data[(int)(key)] = null;
                    }
                }
                if (delta < 0) do
                        selfa.data.RemoveAt((int)(selfa.data.Length - 1));
                    while (++delta < 0);
                for (var i = 2; i < args.length; i++)
                    if (args[i].IsExist)
                    {
                        var t = selfa.data[(int)(pos0 + i - 2)];
                        if (t != null && t.valueType == JSObjectType.Property)
                            ((t.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, new Arguments() { a0 = args[i], length = 1 });
                        else
                            selfa.data[(int)(pos0 + i - 2)] = args[i].CloneImpl();
                    }
                return res;
            }
            else // êòî-òî îòïðàâèë îáúåêò ñ ïîëåì length
            {
                long _length = Tools.getLengthOfIterably(self, false);
                var pos0 = (long)System.Math.Min(Tools.JSObjectToDouble(args[0]), _length);
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSObjectType.Undefined)
                        pos1 = 0;
                    else
                        pos1 = (long)System.Math.Min(Tools.JSObjectToDouble(args[1]), _length);
                }
                else
                    pos1 = _length;
                if (pos0 < 0)
                    pos0 = _length + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = 0;
                if (pos1 == 0 && args.length <= 2)
                {
                    var lenobj = self.GetMember("length", true, false);
                    if (lenobj.valueType == JSObjectType.Property)
                        ((lenobj.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, new Arguments() { a0 = _length, length = 1 });
                    else
                        lenobj.Assign(_length);
                    return new Array();
                }
                pos0 = (uint)System.Math.Min(pos0, _length);
                pos1 += pos0;
                pos1 = (uint)System.Math.Min(pos1, _length);
                var delta = System.Math.Max(0, args.length - 2) - (pos1 - pos0);
                var res = needResult ? new Array() : null;
                long prewKey = -1;
                foreach (var keyS in Tools.iterablyEnum(_length, self))
                {
                    if (prewKey == -1)
                        prewKey = (uint)keyS.Key;
                    if (keyS.Key - prewKey > 1 && keyS.Key < pos1)
                    {
                        for (var i = prewKey + 1; i < keyS.Key; i++)
                        {
                            var value = self.__proto__[i.ToString()];
                            if (value.valueType == JSObjectType.Property)
                                value = ((value.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self.__proto__, null).CloneImpl();
                            else
                                value = value.CloneImpl();
                            if (needResult)
                                res.data[(int)i] = value.CloneImpl();
                        }
                    }
                    if (keyS.Key >= pos1)
                        break;
                    else if (pos0 <= keyS.Key)
                    {
                        var value = self[keyS.Value];
                        if (value.ValueType == JSObjectType.Property)
                            value = ((value.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null).CloneImpl();
                        else
                            value = value.CloneImpl();
                        if (needResult)
                            res.data[(int)(keyS.Key - pos0)] = value;
                    }
                    prewKey = keyS.Key;
                }
                if (prewKey == -1 && needResult)
                {
                    for (var i = 0; i < (pos1 - pos0); i++)
                        res.Add(self.__proto__[(i + pos0).ToString()].CloneImpl());
                }
                var tjo = new JSObject();
                if (delta > 0)
                {
                    for (var i = _length; i-- > pos1; )
                    {
                        if (i <= int.MaxValue)
                        {
                            tjo.valueType = JSObjectType.Int;
                            tjo.iValue = (int)(i + delta);
                        }
                        else
                        {
                            tjo.valueType = JSObjectType.Double;
                            tjo.dValue = i + delta;
                        }
                        var dst = self.GetMember(tjo, true, false);
                        if (i + delta <= int.MaxValue)
                        {
                            tjo.valueType = JSObjectType.Int;
                            tjo.iValue = (int)(i);
                        }
                        else
                        {
                            tjo.valueType = JSObjectType.Double;
                            tjo.dValue = i;
                        }
                        var src = self.GetMember(tjo, true, false);
                        if (src.valueType == JSObjectType.Property)
                            src = ((src.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null);
                        if (dst.valueType == JSObjectType.Property)
                            ((dst.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, new Arguments() { a0 = src, length = 1 });
                        else
                            dst.Assign(src);
                    }
                }
                else if (delta < 0)
                {
                    for (var i = pos0; i < pos1; i++)
                    {
                        if (i + delta <= int.MaxValue)
                        {
                            tjo.valueType = JSObjectType.Int;
                            tjo.iValue = (int)(i);
                        }
                        else
                        {
                            tjo.valueType = JSObjectType.Double;
                            tjo.dValue = i;
                        }
                        var src = self.GetMember(tjo, true, false);
                        if (i >= _length + delta)
                        {
                            if ((src.attributes & JSObjectAttributesInternal.DoNotDelete) == 0)
                            {
                                src.valueType = JSObjectType.NotExists;
                                src.oValue = null;
                            }
                        }
                    }

                    for (var i = pos1; i < _length; i++)
                    {
                        if (i <= int.MaxValue)
                        {
                            tjo.valueType = JSObjectType.Int;
                            tjo.iValue = (int)(i + delta);
                        }
                        else
                        {
                            tjo.valueType = JSObjectType.Double;
                            tjo.dValue = i + delta;
                        }
                        var dst = self.GetMember(tjo, true, false);
                        if (i + delta <= int.MaxValue)
                        {
                            tjo.valueType = JSObjectType.Int;
                            tjo.iValue = (int)(i);
                        }
                        else
                        {
                            tjo.valueType = JSObjectType.Double;
                            tjo.dValue = i;
                        }
                        var srcItem = self.GetMember(tjo, true, false);
                        var src = srcItem;
                        if (src.valueType == JSObjectType.Property)
                            src = ((src.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null);
                        if (dst.valueType == JSObjectType.Property)
                            ((dst.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, new Arguments() { a0 = src, length = 1 });
                        else
                            dst.Assign(src);
                        if (i >= _length + delta)
                        {
                            if ((srcItem.attributes & JSObjectAttributesInternal.DoNotDelete) == 0)
                            {
                                srcItem.valueType = JSObjectType.NotExists;
                                srcItem.oValue = null;
                            }
                        }
                    }
                }
                for (var i = 2; i < args.length; i++)
                {
                    if ((i - 2 + pos0) <= int.MaxValue)
                    {
                        tjo.valueType = JSObjectType.Int;
                        tjo.iValue = (int)(i - 2 + pos0);
                    }
                    else
                    {
                        tjo.valueType = JSObjectType.Double;
                        tjo.dValue = (i - 2 + pos0);
                    }
                    var dst = self.GetMember(tjo, true, false);
                    if (dst.valueType == JSObjectType.Property)
                        ((dst.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, new Arguments() { a0 = args[i], length = 1 });
                    else
                        dst.Assign(args[i]);
                }
                {
                    _length += delta;
                    var lenobj = self.GetMember("length", true, false);
                    if (lenobj.valueType == JSObjectType.Property)
                        ((lenobj.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(self, new Arguments() { a0 = _length, length = 1 });
                    else
                        lenobj.Assign(_length);
                }
                return res;
            }
        }

        private sealed class JSComparer : IComparer<JSObject>
        {
            Arguments args;
            JSObject first;
            JSObject second;
            Function comparer;

            public JSComparer(Arguments args, JSObject first, JSObject second, Function comparer)
            {
                this.args = args;
                this.first = first;
                this.second = second;
                this.comparer = comparer;
            }

            public int Compare(JSObject x, JSObject y)
            {
                first.Assign(x);
                second.Assign(y);
                args[0] = first;
                args[1] = second;
                var res = Tools.JSObjectToInt32(comparer.Invoke(JSObject.undefined, args));
                return res;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject sort(JSObject self, Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var comparer = args[0].oValue as Function;
            if (self.GetType() == typeof(Array))
            {
                var selfa = self as Array;
                var len = selfa.data.Length;
                if (comparer != null)
                {
                    var second = new JSObject();
                    var first = new JSObject();
                    args.length = 2;
                    args[0] = first;
                    args[1] = second;

                    var tt = new BinaryTree<JSObject, List<JSObject>>(new JSComparer(args, first, second, comparer));
                    uint length = selfa.data.Length;
                    foreach (var item in (selfa.data as IEnumerable<KeyValuePair<int, JSObject>>))
                    {
                        if (item.Value == null || !item.Value.IsDefinded)
                            continue;
                        var v = item.Value;
                        if (v.valueType == JSObjectType.Property)
                            v = ((v.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null).CloneImpl();
                        List<JSObject> list = null;
                        if (!tt.TryGetValue(v, out list))
                            tt[v] = list = new List<JSObject>();
                        list.Add(item.Value);
                    }
                    selfa.data.Clear();
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = 0; i < node.value.Count; i++)
                            selfa.data.Add(node.value[i]);
                    }
                    selfa.data[(int)length - 1] = selfa.data[(int)length - 1];
                }
                else
                {
                    var tt = new BinaryTree<string, List<JSObject>>(StringComparer.Ordinal);
                    uint length = selfa.data.Length;
                    foreach (var item in (selfa.data as IEnumerable<KeyValuePair<int, JSObject>>))
                    {
                        if (item.Value == null || !item.Value.IsExist)
                            continue;
                        var v = item.Value;
                        if (v.valueType == JSObjectType.Property)
                            v = ((v.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null).CloneImpl();
                        List<JSObject> list = null;
                        var key = v.ToString();
                        if (!tt.TryGetValue(key, out list))
                            tt[key] = list = new List<JSObject>();
                        list.Add(item.Value);
                    }
                    selfa.data.Clear();
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = 0; i < node.value.Count; i++)
                            selfa.data.Add(node.value[i]);
                    }
                    selfa.data[(int)length - 1] = selfa.data[(int)length - 1];
                }
            }
            else
            {
                var len = Tools.getLengthOfIterably(self, false);
                if (comparer != null)
                {
                    var second = new JSObject();
                    var first = new JSObject();
                    args.length = 2;
                    args[0] = first;
                    args[1] = second;

                    var tt = new BinaryTree<JSObject, List<JSObject>>(new JSComparer(args, first, second, comparer));
                    List<string> keysToRemove = new List<string>();
                    foreach (var key in Tools.iterablyEnum(len, self))
                    {
                        keysToRemove.Add(key.Value);
                        var item = self[key.Value];
                        if (item.IsDefinded)
                        {
                            item = item.CloneImpl();
                            JSObject value;
                            if (item.valueType == JSObjectType.Property)
                                value = ((item.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(self, null);
                            else
                                value = item;
                            List<JSObject> els = null;
                            if (!tt.TryGetValue(value, out els))
                                tt[value] = els = new List<JSObject>();
                            els.Add(item);
                        }
                    }
                    var tjo = new JSObject() { valueType = JSObjectType.String };
                    for (var i = keysToRemove.Count; i-- > 0; )
                    {
                        tjo.oValue = keysToRemove[i];
                        var t = self.GetMember(tjo, true, false);
                        if ((t.attributes & JSObjectAttributesInternal.DoNotDelete) == 0)
                        {
                            t.oValue = null;
                            t.valueType = JSObjectType.NotExists;
                        }
                    }
                    var index = 0u;
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = node.value.Count; i-- > 0; )
                            self[(index++).ToString()] = node.value[i];
                    }
                }
                else
                {
                    var tt = new BinaryTree<string, List<JSObject>>(StringComparer.Ordinal);
                    List<string> keysToRemove = new List<string>();
                    foreach (var key in self)
                    {
                        var pindex = 0;
                        var dindex = 0.0;
                        if (Tools.ParseNumber(key, ref pindex, out dindex) && (pindex == key.Length)
                            && dindex < len)
                        {
                            keysToRemove.Add(key);
                            var value = self[key];
                            if (value.IsDefinded)
                            {
                                value = value.CloneImpl();
                                List<JSObject> els = null;
                                var skey = value.ToString();
                                if (!tt.TryGetValue(skey, out els))
                                    tt[skey] = els = new List<JSObject>();
                                els.Add(value);
                            }
                        }
                    }
                    for (var i = keysToRemove.Count; i-- > 0; )
                        self[keysToRemove[i]].valueType = JSObjectType.NotExists;
                    var index = 0u;
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = node.value.Count; i-- > 0; )
                            self[(index++).ToString()] = node.value[i];
                    }
                }
            }
            return self;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject unshift(JSObject self, Arguments args)
        {
            for (var i = args.length; i-- > 0; )
                args[i + 2] = args[i];
            args.length += 2;
            args.a0 = 0;
            args.a1 = args.a0;
            spliceImpl(self, args, false);
            return Tools.getLengthOfIterably(self, false);
        }

        [Hidden]
        public override string ToString()
        {
            return joinImpl(this, ",", false);
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        [ArgumentsLength(0)]
        public new JSObject toString(Arguments args)
        {
            if (this.GetType() != typeof(Array) && !this.GetType().IsSubclassOf(typeof(Array)))
                throw new JSException(new TypeError("Try to call Array.toString on not Array object."));
            return this.ToString();
        }

        [DoNotEnumerate]
        public new JSObject toLocaleString()
        {
            return joinImpl(this, ",", true);
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            foreach (var node in (data as IEnumerable<KeyValuePair<int, JSObject>>))
            {
                if (node.Value != null
                    && node.Value.IsExist
                    && (!hideNonEnum || (node.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                    yield return ((uint)node.Key).ToString();
            }
            if (!hideNonEnum)
                yield return "length";
            if (fields != null)
            {
                foreach (var f in fields)
                {
                    if (f.Value.IsExist && (!hideNonEnum || (f.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                        yield return f.Key;
                }
            }
        }

        [Hidden]
        public JSObject this[int index]
        {
            [Hidden]
            get
            {
                notExists.valueType = JSObjectType.NotExistsInObject;
                var res = data[(int)index] ?? notExists;
                if (res.valueType < JSObjectType.Undefined)
                    return __proto__.GetMember(index, false, false);
                return res;
            }
            [Hidden]
            set
            {
                if (index >= data.Length
                    && _lengthObj != null
                    && (_lengthObj.attributes & JSObjectAttributesInternal.ReadOnly) != 0)
                    return; // fixed size array. Item could not be added

                var res = data[index];
                if (res == null)
                {
                    res = new JSObject() { valueType = JSObjectType.NotExistsInObject };
                    data[index] = res;
                }
                else if ((res.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                    data[index] = res = res.CloneImpl();
                if (res.valueType == JSObjectType.Property)
                {
                    var setter = (res.oValue as PropertyPair).set;
                    if (setter != null)
                        setter.Invoke(this, new Arguments { value });
                    return;
                }
                res.Assign(value);
            }
        }

        [Hidden]
        internal protected override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            if (name.valueType == JSObjectType.String && string.CompareOrdinal("length", name.oValue.ToString()) == 0)
                return length;
            bool isIndex = false;
            int index = 0;
            JSObject tname = name;
            if (tname.valueType >= JSObjectType.Object)
                tname = tname.ToPrimitiveValue_String_Value();
            switch (tname.valueType)
            {
                case JSObjectType.Int:
                    {
                        isIndex = (tname.iValue & int.MinValue) == 0;
                        index = tname.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        isIndex = tname.dValue >= 0 && tname.dValue < uint.MaxValue && (long)tname.dValue == tname.dValue;
                        if (isIndex)
                            index = (int)(uint)tname.dValue;
                        break;
                    }
                case JSObjectType.String:
                    {
                        var fc = tname.oValue.ToString()[0];
                        if ('0' <= fc && '9' >= fc)
                        {
                            var dindex = 0.0;
                            int si = 0;
                            if (Tools.ParseNumber(tname.oValue.ToString(), ref si, out dindex)
                                && (si == tname.oValue.ToString().Length)
                                && dindex >= 0
                                && dindex < uint.MaxValue
                                && (long)dindex == dindex)
                            {
                                isIndex = true;
                                index = (int)(uint)dindex;
                            }
                        }
                        break;
                    }
            }
            if (isIndex)
            {
                forWrite &= (attributes & JSObjectAttributesInternal.Immutable) == 0;
                if (forWrite)
                {
                    if (_lengthObj != null && (_lengthObj.attributes & JSObjectAttributesInternal.ReadOnly) != 0 && index >= data.Length)
                    {
                        if (own)
                            throw new JSException(new TypeError("Can not add item to fixed size array"));
                        return notExists;
                    }
                    var res = data[index];
                    if (res == null)
                    {
                        res = new JSObject() { valueType = JSObjectType.NotExistsInObject };
                        data[index] = res;
                    }
                    else if ((res.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                        data[index] = res = res.CloneImpl();
                    return res;
                }
                else
                {
                    notExists.valueType = JSObjectType.NotExistsInObject;
                    var res = data[index] ?? notExists;
                    if (res.valueType < JSObjectType.Undefined && !own)
                        return __proto__.GetMember(name, forWrite, own);
                    return res;
                }
            }

            //if ((attributes & JSObjectAttributesInternal.ProxyPrototype) != 0)
            //    return __proto__.GetMember(name, create, own);
            return DefaultFieldGetter(name, forWrite, own);
        }

        /*internal override bool DeleteMember(JSObject name)
        {
            if (name.valueType == JSObjectType.String && string.CompareOrdinal("length", name.oValue.ToString()) == 0)
                return false;
            bool isIndex = false;
            int index = 0;
            JSObject tname = name;
            if (tname.valueType >= JSObjectType.Object)
                tname = tname.ToPrimitiveValue_String_Value();
            switch (tname.valueType)
            {
                case JSObjectType.Int:
                    {
                        isIndex = tname.iValue >= 0;
                        index = tname.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        isIndex = tname.dValue >= 0 && tname.dValue < uint.MaxValue && (long)tname.dValue == tname.dValue;
                        if (isIndex)
                            index = (int)(uint)tname.dValue;
                        break;
                    }
                case JSObjectType.String:
                    {
                        var fc = tname.oValue.ToString()[0];
                        if ('0' <= fc && '9' >= fc)
                        {
                            var dindex = 0.0;
                            int si = 0;
                            if (Tools.ParseNumber(tname.oValue.ToString(), ref si, out dindex)
                                && (si == tname.oValue.ToString().Length)
                                && dindex >= 0
                                && dindex < uint.MaxValue
                                && (long)dindex == dindex)
                            {
                                isIndex = true;
                                index = (int)(uint)dindex;
                            }
                        }
                        break;
                    }
            }
            if (isIndex)
            {
                var t = data[index];
                if (t == null)
                    return true;
                if (t.IsExist
                    && (t.attributes & JSObjectAttributesInternal.DoNotDelete) != 0)
                    return false;
                data[index] = null;
                return true;
            }
            return base.DeleteMember(name);
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Не убирть!</remarks>
        /// <returns></returns>
        [Hidden]
        public override JSObject valueOf()
        {
            return base.valueOf();
        }
    }
}