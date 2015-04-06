﻿using System;
using System.Globalization;
using System.Text;
using Barista.NiL.JS.Core;
using Barista.NiL.JS.Core.Modules;
using Barista.NiL.JS.Core.TypeProxing;

namespace Barista.NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Number : JSObject
    {
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public readonly static JSObject NaN = double.NaN;
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public readonly static JSObject POSITIVE_INFINITY = double.PositiveInfinity;
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public readonly static JSObject NEGATIVE_INFINITY = double.NegativeInfinity;
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public readonly static JSObject MAX_VALUE = double.MaxValue;
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public readonly static JSObject MIN_VALUE = double.Epsilon;

        [DoNotEnumerate]
        static Number()
        {
            POSITIVE_INFINITY.attributes |= JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.SystemObject;
            NEGATIVE_INFINITY.attributes |= JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.SystemObject;
            MAX_VALUE.attributes |= JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.SystemObject;
            MIN_VALUE.attributes |= JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.SystemObject;
            NaN.attributes |= JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Number()
        {
            valueType = JSObjectType.Int;
            iValue = 0;
            attributes |= JSObjectAttributesInternal.SystemObject | JSObjectAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        public Number(int value)
        {
            valueType = JSObjectType.Int;
            iValue = value;
            attributes |= JSObjectAttributesInternal.SystemObject | JSObjectAttributesInternal.ReadOnly;
        }

        [Hidden]
        public Number(long value)
        {
            if ((long)(int)(value) == value)
            {
                valueType = JSObjectType.Int;
                iValue = (int)value;
            }
            else
            {
                valueType = JSObjectType.Double;
                dValue = value;
            }
            attributes |= JSObjectAttributesInternal.SystemObject | JSObjectAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        public Number(double value)
        {
            valueType = JSObjectType.Double;
            dValue = value;
            attributes |= JSObjectAttributesInternal.SystemObject | JSObjectAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        public Number(string value)
        {
            value = value.Trim(Tools.TrimChars);
            valueType = JSObjectType.Int;
            dValue = value.Length != 0 ? double.NaN : 0;
            valueType = JSObjectType.Double;
            double d = 0;
            int i = 0;
            if (value.Length != 0 && Tools.ParseNumber(value, ref i, out d, 0, Tools.ParseNumberOptions.Default | (Context.CurrentContext.strict ? Tools.ParseNumberOptions.RaiseIfOctal : 0)) && i == value.Length)
                dValue = d;
            attributes |= JSObjectAttributesInternal.SystemObject | JSObjectAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        public Number(Arguments obj)
        {
            valueType = JSObjectType.Double;
            dValue = Tools.JSObjectToDouble(obj[0]);
            attributes |= JSObjectAttributesInternal.SystemObject | JSObjectAttributesInternal.ReadOnly;
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) == 0)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new InvalidOperationException("Try to assign to Number");
            }
        }

        [Hidden]
        internal protected override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            return DefaultFieldGetter(name, forWrite, own); // обращение идёт к Объекту Number, а не к значению number, поэтому члены создавать можно
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject toPrecision(JSObject self, Arguments digits)
        {
            double res = Tools.JSObjectToDouble(self);
            return res.ToString("G" + Tools.JSObjectToInt32(digits[0]), CultureInfo.InvariantCulture);
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject toExponential(JSObject self, Arguments digits)
        {
            double res = 0;
            switch (self.valueType)
            {
                case JSObjectType.Int:
                    {
                        res = self.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        res = self.dValue;
                        break;
                    }
                case JSObjectType.Object:
                    {
                        if (typeof(Number) != self.GetType())
                            throw new JSException((new TypeError("Try to call Number.toExponential on not number object.")));
                        res = self.iValue == 0 ? self.dValue : self.iValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            int dgts = 0;
            switch ((digits ?? JSObject.undefined).valueType)
            {
                case JSObjectType.Int:
                    {
                        dgts = digits.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        dgts = (int)digits.dValue;
                        break;
                    }
                case JSObjectType.String:
                    {
                        double d = 0;
                        int i = 0;
                        if (Tools.ParseNumber(digits.oValue.ToString(), i, out d, Tools.ParseNumberOptions.Default))
                            dgts = (int)d;
                        break;
                    }
                case JSObjectType.Object:
                    {
                        var d = digits[0].ToPrimitiveValue_Value_String();
                        if (d.valueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        if (d.valueType == JSObjectType.Int)
                            goto case JSObjectType.Int;
                        if (d.valueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        break;
                    }
                default:
                    return res.ToString("e", System.Globalization.CultureInfo.InvariantCulture);
            }
            return res.ToString("e" + dgts, System.Globalization.CultureInfo.InvariantCulture);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject toFixed(JSObject self, Arguments digits)
        {
            double res = 0;
            switch (self.valueType)
            {
                case JSObjectType.Int:
                    {
                        res = self.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        res = self.dValue;
                        break;
                    }
                case JSObjectType.Object:
                    {
                        if (typeof(Number) != self.GetType())
                            throw new JSException((new TypeError("Try to call Number.toFixed on not number object.")));
                        res = self.iValue == 0 ? self.dValue : self.iValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            int dgts = Tools.JSObjectToInt32(digits[0], true);
            if (dgts < 0 || dgts > 20)
                throw new JSException((new RangeError("toFixed() digits argument must be between 0 and 20")));
            if (System.Math.Abs(self.dValue) >= 1e+21)
                return self.dValue.ToString("0.####e+0", System.Globalization.CultureInfo.InvariantCulture);
            if (dgts > 0)
                dgts++;
            return System.Math.Round(res, dgts).ToString("0.00000000000000000000".Substring(0, dgts + 1), System.Globalization.CultureInfo.InvariantCulture);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSObject toLocaleString(JSObject self)
        {
            return self.valueType == JSObjectType.Int ? self.iValue.ToString(System.Globalization.CultureInfo.CurrentCulture) : self.dValue.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        [InstanceMember]
        [DoNotEnumerate]
        [CLSCompliant(false)]
        public static JSObject toString(JSObject self, Arguments radix)
        {
            var ovt = self.valueType;
            if (self.valueType > JSObjectType.Double && self.GetType() == typeof(Number))
                self.valueType = self.dValue == 0.0 ? JSObjectType.Int : JSObjectType.Double;
            try
            {
                if (self.valueType != JSObjectType.Int && self.valueType != JSObjectType.Double)
                    throw new JSException((new TypeError("Try to call Number.toString on not Number object")));
                int r = 10;
                if (radix != null && radix.GetMember("length").iValue > 0)
                {
                    var ar = radix[0];
                    if (ar.valueType == JSObjectType.Object && ar.oValue == null)
                        throw new JSException((new Error("Radix can't be null.")));
                    switch (ar.valueType)
                    {
                        case JSObjectType.Int:
                        case JSObjectType.Bool:
                            {
                                r = ar.iValue;
                                break;
                            }
                        case JSObjectType.Double:
                            {
                                r = (int)ar.dValue;
                                break;
                            }
                        case JSObjectType.NotExistsInObject:
                        case JSObjectType.Undefined:
                            {
                                r = 10;
                                break;
                            }
                        default:
                            {
                                r = Tools.JSObjectToInt32(ar);
                                break;
                            }
                    }
                }
                if (r < 2 || r > 36)
                    throw new JSException((new TypeError("Radix must be between 2 and 36.")));
                if (r == 10)
                    return self.ToString();
                else
                {
                    long res = self.iValue;
                    var sres = new StringBuilder();
                    bool neg;
                    if (self.valueType == JSObjectType.Double)
                    {
                        if (double.IsNaN(self.dValue))
                            return "NaN";
                        if (double.IsPositiveInfinity(self.dValue))
                            return "Infinity";
                        if (double.IsNegativeInfinity(self.dValue))
                            return "-Infinity";
                        res = (long)self.dValue;
                        if (res != self.dValue) // your bunny wrote
                        {
                            double dtemp = self.dValue;
                            neg = dtemp < 0;
                            if (neg)
                                dtemp = -dtemp;
                            sres.Append(Tools.NumChars[(int)(dtemp % r)]);
                            res /= r;
                            while (dtemp >= 1.0)
                            {
                                sres.Append(Tools.NumChars[(int)(dtemp % r)]);
                                dtemp /= r;
                            }
                            if (neg)
                                sres.Append('-');
                            for (int i = sres.Length - 1, j = 0; i > j; j++, i--)
                            {
                                sres[i] ^= sres[j];
                                sres[j] ^= sres[i];
                                sres[i] ^= sres[j];
                                sres[i] += (char)((sres[i] / 'A') * ('a' - 'A'));
                                sres[j] += (char)((sres[j] / 'A') * ('a' - 'A'));
                            }
                            return sres.ToString();
                        }
                    }
                    neg = res < 0;
                    if (neg)
                        res = -res;
                    if (res < 0)
                        throw new JSException(new Error("Internal error"));
                    sres.Append(Tools.NumChars[res % r]);
                    res /= r;
                    while (res != 0)
                    {
                        sres.Append(Tools.NumChars[res % r]);
                        res /= r;
                    }
                    if (neg)
                        sres.Append('-');
                    for (int i = sres.Length - 1, j = 0; i > j; j++, i--)
                    {
                        sres[i] ^= sres[j];
                        sres[j] ^= sres[i];
                        sres[i] ^= sres[j];
                        sres[i] += (char)((sres[i] / 'A') * ('a' - 'A'));
                        sres[j] += (char)((sres[j] / 'A') * ('a' - 'A'));
                    }
                    return sres.ToString();
                }
            }
            finally
            {
                self.valueType = ovt;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject valueOf(JSObject self)
        {
            if (self.GetType() == typeof(Number))
                return self.iValue == 0 ? self.dValue : self.iValue;
            if (self.valueType != JSObjectType.Int && self.valueType != JSObjectType.Double)
                throw new JSException((new TypeError("Try to call Number.valueOf on not number object.")));
            return self;
        }

        [Hidden]
        public override string ToString()
        {
            if (valueType == JSObjectType.Int || valueType == JSObjectType.Double)
                return valueType == JSObjectType.Int ? iValue >= 0 && iValue < 16 ? Tools.NumString[iValue] : iValue.ToString(System.Globalization.CultureInfo.InvariantCulture) : Tools.DoubleToString(dValue);
            return iValue != 0 ? iValue.ToString() : Tools.DoubleToString(dValue);
        }

        [Hidden]
        public override int GetHashCode()
        {
            return valueType == JSObjectType.Int ? iValue.GetHashCode() : dValue.GetHashCode();
        }

        [Hidden]
        public static implicit operator Number(int value)
        {
            return new Number(value);
        }

        [Hidden]
        public static implicit operator Number(double value)
        {
            return new Number(value);
        }

        [Hidden]
        public static implicit operator double(Number value)
        {
            return value == null ? 0 : value.valueType == JSObjectType.Int ? value.iValue : value.dValue;
        }

        [Hidden]
        public static explicit operator int(Number value)
        {
            return value == null ? 0 : value.valueType == JSObjectType.Int ? value.iValue : (int)value.dValue;
        }

        [DoNotEnumerate]
        public static JSObject isNaN(Arguments a)
        {
            switch (a[0].valueType)
            {
                case JSObjectType.Double:
                    return double.IsNaN(a[0].dValue);
                default:
                    return Boolean.False;
            }
        }
    }
}