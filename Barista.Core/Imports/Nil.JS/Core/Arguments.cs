﻿using System;
using System.Collections.Generic;
using System.Collections;

namespace Barista.NiL.JS.Core
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Arguments : JSObject, IEnumerable
    {
        private sealed class _LengthContainer : JSObject
        {
            private readonly Arguments owner;

            public _LengthContainer(Arguments owner)
            {
                this.owner = owner;
            }

            public override void Assign(JSObject value)
            {
                base.Assign(value);
                owner.length = Tools.JSObjectToInt32(value);
            }
        }

        internal JSObject a0;
        internal JSObject a1;
        internal JSObject a2;
        internal JSObject a3;
        internal JSObject a4;
        //internal JSObject a5;
        //internal JSObject a6;
        //internal JSObject a7;
        internal JSObject callee;
        internal JSObject caller;
        private _LengthContainer _length;
        internal int length;

        public int Length
        {
            get { return length; }
        }

        public override JSObject this[string name]
        {
            get
            {
                return base[name];
            }
            set
            {
                switch (name)
                {
                    case "callee":
                        callee = value;
                        return;
                    case "caller":
                        caller = value;
                        return;
                }
                base[name] = value;
            }
        }

        public JSObject this[int index]
        {
            get
            {
                JSObject res = null;
                switch (index)
                {
                    case 0:
                        res = a0;
                        break;
                    case 1:
                        res = a1;
                        break;
                    case 2:
                        res = a2;
                        break;
                    case 3:
                        res = a3;
                        break;
                    case 4:
                        res = a4;
                        break;
                    default:
                        return base[index.ToString()];
                }
                if (res == null)
                    return notExists;
                return res;
            }
            set
            {
                switch (index)
                {
                    case 0:
                        a0 = value;
                        break;
                    case 1:
                        a1 = value;
                        break;
                    case 2:
                        a2 = value;
                        break;
                    case 3:
                        a3 = value;
                        break;
                    case 4:
                        a4 = value;
                        break;
                    default:
                        if (fields == null)
                            fields = createFields();
                        fields[index.ToString()] = value;
                        break;
                }

            }
        }

        public Arguments()
            : base()
        {
            valueType = JSObjectType.Object;
            oValue = this;
            attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject;
        }

        public void Add(JSObject arg)
        {
            this[length++] = arg;
        }

        protected override JSObject getDefaultPrototype()
        {
            return GlobalPrototype ?? Null;
        }

        public override void Assign(NiL.JS.Core.JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) != 0)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new InvalidOperationException("Try to assign to Arguments");
            }
        }

        protected internal override JSObject GetMember(JSObject name, bool createMember, bool own)
        {
            createMember &= (attributes & JSObjectAttributesInternal.Immutable) == 0;
            if (name.valueType == JSObjectType.Int)
            {
                switch (name.iValue)
                {
                    case 0:
                        return (a0 ?? (!createMember ? notExists : (a0 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                    case 1:
                        return (a1 ?? (!createMember ? notExists : (a1 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                    case 2:
                        return (a2 ?? (!createMember ? notExists : (a2 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                    case 3:
                        return (a3 ?? (!createMember ? notExists : (a3 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                    case 4:
                        return (a4 ?? (!createMember ? notExists : (a4 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                    //case 5:
                    //    return (a5 ?? (!createMember ? notExists : (a5 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                    //case 6:
                    //    return (a6 ?? (!createMember ? notExists : (a6 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                    //case 7:
                    //    return (a7 ?? (!createMember ? notExists : (a7 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                }
            }
            switch (name.ToString())
            {
                case "0":
                    return (a0 ?? (!createMember ? notExists : (a0 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                case "1":
                    return (a1 ?? (!createMember ? notExists : (a1 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                case "2":
                    return (a2 ?? (!createMember ? notExists : (a2 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                case "3":
                    return (a3 ?? (!createMember ? notExists : (a3 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                case "4":
                    return (a4 ?? (!createMember ? notExists : (a4 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                //case "5":
                //    return (a5 ?? (!createMember ? notExists : (a5 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                //case "6":
                //    return (a6 ?? (!createMember ? notExists : (a6 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                //case "7":
                //    return (a7 ?? (!createMember ? notExists : (a7 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                case "length":
                    {
                        if (_length == null)
                            _length = new _LengthContainer(this) { valueType = JSObjectType.Int, iValue = length, attributes = JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.Reassign };
                        return _length;
                    }
                case "callee":
                    {
                        if (createMember && (callee.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                        {
                            callee = callee.CloneImpl();
                            callee.attributes = JSObjectAttributesInternal.DoNotEnum;
                        }
                        return (callee ?? (!createMember ? notExists : (callee = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                    }
                case "caller":
                    {
                        if (createMember && (caller.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                        {
                            caller = caller.CloneImpl();
                            callee.attributes = JSObjectAttributesInternal.DoNotEnum;
                        }
                        return (caller ?? (!createMember ? notExists : (caller = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                    }
            }
            return base.GetMember(name, createMember, own);
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            if (a0 != null && a0.IsExist && (!hideNonEnum || (a0.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "0";
            if (a1 != null && a1.IsExist && (!hideNonEnum || (a1.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "1";
            if (a2 != null && a2.IsExist && (!hideNonEnum || (a2.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "2";
            if (a3 != null && a3.IsExist && (!hideNonEnum || (a3.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "3";
            if (a4 != null && a4.IsExist && (!hideNonEnum || (a4.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "4";
            //if (a5 != null && a5.IsExist && (!hideNonEnum || (a5.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "5";
            //if (a6 != null && a6.IsExist && (!hideNonEnum || (a6.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "6";
            //if (a7 != null && a7.IsExist && (!hideNonEnum || (a7.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "7";
            //if (a8 != null && a8.isExist && (!hideNonEnum || (a8.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "8";
            //if (a9 != null && a9.isExist && (!hideNonEnum || (a9.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "9";
            //if (a10 != null && a10.isExist && (!hideNonEnum || (a10.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "10";
            //if (a11 != null && a11.isExist && (!hideNonEnum || (a11.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "11";
            //if (a12 != null && a12.isExist && (!hideNonEnum || (a12.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "12";
            //if (a13 != null && a13.isExist && (!hideNonEnum || (a13.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "13";
            //if (a14 != null && a14.isExist && (!hideNonEnum || (a14.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "14";
            //if (a15 != null && a15.isExist && (!hideNonEnum || (a15.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "15";
            if (callee != null && callee.IsExist && (!hideNonEnum || (callee.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "callee";
            if (caller != null && callee.IsExist && (!hideNonEnum || (caller.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "caller";
            if (_length != null && _length.IsExist && (!hideNonEnum || (_length.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "length";
            var be = getBaseEnumerator(hideNonEnum);
            while (be.MoveNext())
                yield return be.Current;
        }

        private IEnumerator<string> getBaseEnumerator(bool hideNonEnum)
        {
            return base.GetEnumeratorImpl(hideNonEnum);
        }

        protected internal override bool DeleteMember(JSObject name)
        {
            if (name.valueType == JSObjectType.Int)
            {
                switch (name.iValue)
                {
                    case 0:
                        return a0 == null || ((a0.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a0 = null) == null;
                    case 1:
                        return a1 == null || ((a1.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a1 = null) == null;
                    case 2:
                        return a2 == null || ((a2.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a2 = null) == null;
                    case 3:
                        return a3 == null || ((a3.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a3 = null) == null;
                    case 4:
                        return a4 == null || ((a4.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a4 = null) == null;
                    //case 5:
                    //    return a5 == null || ((a5.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a5 = null) == null;
                    //case 6:
                    //    return a6 == null || ((a6.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a6 = null) == null;
                    //case 7:
                    //    return a7 == null || ((a7.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a7 = null) == null;
                }
            }
            switch (name.ToString())
            {
                case "0":
                    return a0 == null || ((a0.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a0 = null) == null;
                case "1":
                    return a1 == null || ((a1.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a1 = null) == null;
                case "2":
                    return a2 == null || ((a2.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a2 = null) == null;
                case "3":
                    return a3 == null || ((a3.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a3 = null) == null;
                case "4":
                    return a4 == null || ((a4.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a4 = null) == null;
                //case "5":
                //    return a5 == null || ((a5.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a5 = null) == null;
                //case "6":
                //    return a6 == null || ((a6.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a6 = null) == null;
                //case "7":
                //    return a7 == null || ((a7.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a7 = null) == null;
            }
            return base.DeleteMember(name);
        }

        internal void Reset()
        {
            fields = null;
            length = 0;
            a0 = null;
            a1 = null;
            a2 = null;
            a3 = null;
            a4 = null;
            //a5 = null;
            //a6 = null;
            //a7 = null;
            callee = null;
            caller = null;
            __prototype = null;
            _length = null;
            valueType = JSObjectType.Object;
            oValue = this;
            attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject;
        }
    }
}
