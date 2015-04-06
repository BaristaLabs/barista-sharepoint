﻿using System;
using Barista.NiL.JS.Core;
using Barista.NiL.JS.Core.Modules;

namespace Barista.NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Uint8ClampedArray : TypedArray
    {
        protected override JSObject this[int index]
        {
            get
            {
                var res = new Element(this, index);
                res.iValue = getValue(index);
                res.valueType = JSObjectType.Int;
                return res;
            }
            set
            {
                if (index < 0 || index > length.iValue)
                    throw new JSException(new RangeError());
                buffer.Data[index + byteOffset] = (byte)System.Math.Min(255, System.Math.Max(0, Tools.JSObjectToInt32(value, 0, false)));
            }
        }

        private byte getValue(int index)
        {
            return buffer.Data[index + byteOffset];
        }

        public override int BYTES_PER_ELEMENT
        {
            get { return 1; }
        }

        public Uint8ClampedArray()
            : base() { }

        public Uint8ClampedArray(int length)
            : base(length) { }

        public Uint8ClampedArray(ArrayBuffer buffer)
            : base(buffer, 0, buffer.byteLength) { }

        public Uint8ClampedArray(ArrayBuffer buffer, int bytesOffset)
            : base(buffer, bytesOffset, buffer.byteLength - bytesOffset) { }

        public Uint8ClampedArray(ArrayBuffer buffer, int bytesOffset, int length)
            : base(buffer, bytesOffset, length) { }

        public Uint8ClampedArray(JSObject src)
            : base(src) { }

        [ArgumentsLength(2)]
        public override TypedArray subarray(Arguments args)
        {
            return subarrayImpl<Uint8ClampedArray>(args[0], args[1]);
        }

        [Hidden]
        public override Type ElementType
        {
            [Hidden]
            get { return typeof(byte); }
        }

        protected internal override System.Array ToNativeArray()
        {
            var res = new byte[length.iValue];
            for (var i = 0; i < res.Length; i++)
                res[i] = getValue(i);
            return res;
        }
    }
}
