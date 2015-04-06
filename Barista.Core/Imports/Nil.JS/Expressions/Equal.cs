﻿using System;
using Barista.NiL.JS.Core;

namespace Barista.NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public class Equal : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public Equal(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var temp = first.Evaluate(context);
            JSObject tjso;
            int tint;
            double tdouble;
            string tstr;
            var index = 0;
            switch (temp.valueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        tint = temp.iValue;
                        tjso = second.Evaluate(context);
                        switch (tjso.valueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    return tint == tjso.iValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                            case JSObjectType.Double:
                                {
                                    return tint == tjso.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                            case JSObjectType.String:
                                {
                                    tstr = tjso.oValue.ToString();
                                    if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                        return tint == tdouble ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                    else
                                        return false;
                                }
                            case JSObjectType.Date:
                            case JSObjectType.Object:
                                {
                                    tjso = tjso.ToPrimitiveValue_Value_String();
                                    if (tjso.valueType == JSObjectType.Int)
                                        goto case JSObjectType.Int;
                                    if (tjso.valueType == JSObjectType.Bool)
                                        goto case JSObjectType.Int;
                                    if (tjso.valueType == JSObjectType.Double)
                                        goto case JSObjectType.Double;
                                    if (tjso.valueType == JSObjectType.String)
                                        goto case JSObjectType.String;
                                    if (tjso.valueType >= JSObjectType.Object) // null
                                        return false;
                                    throw new NotImplementedException();
                                }
                        }
                        return NiL.JS.BaseLibrary.Boolean.False;
                    }
                case JSObjectType.Double:
                    {
                        tdouble = temp.dValue;
                        tjso = second.Evaluate(context);
                        switch (tjso.valueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    return tdouble == tjso.iValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                            case JSObjectType.Double:
                                {
                                    return tdouble == tjso.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                            case JSObjectType.String:
                                {
                                    tstr = tjso.oValue.ToString();
                                    if (Tools.ParseNumber(tstr, ref index, out tjso.dValue) && (index == tstr.Length))
                                        return tdouble == tjso.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                    else
                                        return false;
                                }
                            case JSObjectType.Date:
                            case JSObjectType.Object:
                                {
                                    tjso = tjso.ToPrimitiveValue_Value_String();
                                    if (tjso.valueType == JSObjectType.Int)
                                        goto case JSObjectType.Int;
                                    if (tjso.valueType == JSObjectType.Bool)
                                        goto case JSObjectType.Int;
                                    if (tjso.valueType == JSObjectType.Double)
                                        goto case JSObjectType.Double;
                                    if (tjso.valueType == JSObjectType.String)
                                        goto case JSObjectType.String;
                                    if (tjso.valueType >= JSObjectType.Object) // null
                                    {
                                        return tdouble == 0 && double.IsPositiveInfinity(1.0 / tdouble);
                                    }
                                    throw new NotImplementedException();
                                }
                        }
                        return false;
                    }
                case JSObjectType.String:
                    {
                        tstr = temp.oValue.ToString();
                        temp = second.Evaluate(context);
                        switch (temp.valueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                        return tdouble == temp.iValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                    else
                                        return false;
                                }
                            case JSObjectType.Double:
                                {
                                    if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                        return tdouble == temp.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                    else
                                        return false;
                                }
                            case JSObjectType.String:
                                {
                                    return string.CompareOrdinal(tstr, temp.oValue.ToString()) == 0 ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                            case JSObjectType.Function:
                            case JSObjectType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    switch (temp.valueType)
                                    {
                                        case JSObjectType.Int:
                                        case JSObjectType.Bool:
                                            {
                                                if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                                    return tdouble == temp.iValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                                else goto
                                                    case JSObjectType.String;
                                            }
                                        case JSObjectType.Double:
                                            {
                                                if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                                    return tdouble == temp.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                                else
                                                    goto case JSObjectType.String;
                                            }
                                        case JSObjectType.String:
                                            {
                                                return string.CompareOrdinal(tstr, temp.Value.ToString()) == 0 ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                            }
                                    }
                                    break;
                                }
                        }
                        return false;
                    }
                case JSObjectType.Function:
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        if (tempContainer == null)
                            tempContainer = new JSObject() { attributes = JSObjectAttributesInternal.Temporary };
                        tempContainer.Assign(temp);
                        temp = tempContainer;

                        tjso = second.Evaluate(context);
                        switch (tjso.valueType)
                        {
                            case JSObjectType.Double:
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    tdouble = tjso.valueType == JSObjectType.Double ? tjso.dValue : tjso.iValue;
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    switch (temp.valueType)
                                    {
                                        case JSObjectType.Bool:
                                        case JSObjectType.Int:
                                            {
                                                return temp.iValue == tdouble ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                            }
                                        case JSObjectType.Double:
                                            {
                                                return temp.dValue == tdouble ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                            }
                                        case JSObjectType.String:
                                            {
                                                tstr = temp.oValue.ToString();
                                                if (Tools.ParseNumber(tstr, ref index, out temp.dValue) && (index == tstr.Length))
                                                    return tdouble == temp.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                                else
                                                    return false;
                                            }
                                    }
                                    return false;
                                }
                            case JSObjectType.String:
                                {
                                    tstr = tjso.oValue.ToString();
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    switch (temp.valueType)
                                    {
                                        case JSObjectType.Double:
                                        case JSObjectType.Bool:
                                        case JSObjectType.Int:
                                            {
                                                temp.dValue = temp.valueType == JSObjectType.Double ? temp.dValue : temp.iValue;
                                                if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                                    return tdouble == temp.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                                else
                                                    return false;
                                            }
                                        case JSObjectType.String:
                                            {
                                                return temp.oValue.ToString() == tstr ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                            }
                                    }
                                    break;
                                }
                            default:
                                {
                                    return temp.oValue == tjso.oValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                        }
                        break;
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistsInObject:
                    {
                        temp = second.Evaluate(context);
                        switch (temp.valueType)
                        {
                            case JSObjectType.Object:
                                {
                                    return temp.oValue == null ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                            default:
                                {
                                    return !temp.IsDefinded ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                        }
                    }
                default: throw new NotImplementedException();
            }
            return false;
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            var res = base.Build(ref _this, depth,variables, state, message, statistic, opts);
            return res;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            base.Optimize(ref _this, owner, message, opts, statistic);
            if (message != null)
            {
                var fc = first as Constant ?? second as Constant;
                if (fc != null)
                {
                    switch (fc.value.valueType)
                    {
                        case JSObjectType.Undefined:
                            message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "To compare with undefined use '===' or '!==' instead of '==' or '!='.");
                            break;
                        case JSObjectType.Object:
                            if (fc.value.oValue == null)
                                message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "To compare with null use '===' or '!==' instead of '==' or '!='.");
                            break;
                    }
                }
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " == " + second + ")";
        }
    }
}