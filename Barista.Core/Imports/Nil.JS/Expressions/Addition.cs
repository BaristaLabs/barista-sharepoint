﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Barista.NiL.JS.Core;
using Barista.NiL.JS.Core.JIT;

namespace Barista.NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Addition : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                var frt = first.ResultType;
                var srt = second.ResultType;
                if (frt == PredictedType.String || srt == PredictedType.String)
                    return PredictedType.String;
                if (frt == srt)
                {
                    switch (frt)
                    {
                        case PredictedType.Bool:
                        case PredictedType.Int:
                            return PredictedType.Number;
                        case PredictedType.Double:
                            return PredictedType.Double;
                    }
                }
                if (frt == PredictedType.Bool)
                {
                    if (srt == PredictedType.Double)
                        return PredictedType.Double;
                    if (Tools.IsEqual(srt, PredictedType.Number, PredictedType.Group))
                        return PredictedType.Number;
                }
                if (srt == PredictedType.Bool)
                {
                    if (frt == PredictedType.Double)
                        return PredictedType.Double;
                    if (Tools.IsEqual(frt, PredictedType.Number, PredictedType.Group))
                        return PredictedType.Number;
                }
                return PredictedType.Unknown;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public Addition(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var f = first.Evaluate(context);
            var temp = tempContainer ?? (tempContainer = new JSObject() { attributes = JSObjectAttributesInternal.Temporary });
            temp.valueType = f.valueType;
            temp.iValue = f.iValue;
            temp.dValue = f.dValue;
            temp.oValue = f.oValue;
            temp.__prototype = f.__prototype;
            tempContainer = null;
            Impl(temp, temp, second.Evaluate(context));
            tempContainer = temp;
            return temp;
        }

        internal static void Impl(JSObject resultContainer, JSObject first, JSObject second)
        {
            switch (first.valueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        if (second.valueType >= JSObjectType.Object)
                            second = second.ToPrimitiveValue_Value_String();
                        switch (second.valueType)
                        {
                            case JSObjectType.Int:
                            case JSObjectType.Bool:
                                {
                                    long tl = (long)first.iValue + second.iValue;
                                    if ((int)tl == tl)
                                    {
                                        resultContainer.valueType = JSObjectType.Int;
                                        resultContainer.iValue = (int)tl;
                                    }
                                    else
                                    {
                                        resultContainer.valueType = JSObjectType.Double;
                                        resultContainer.dValue = (double)tl;
                                    }
                                    return;
                                }
                            case JSObjectType.Double:
                                {
                                    resultContainer.valueType = JSObjectType.Double;
                                    resultContainer.dValue = first.iValue + second.dValue;
                                    return;
                                }
                            case JSObjectType.String:
                                {
                                    resultContainer.oValue = new RopeString((first.valueType == JSObjectType.Bool ? (first.iValue != 0 ? "true" : "false") : first.iValue.ToString(CultureInfo.InvariantCulture)), second.oValue);
                                    resultContainer.valueType = JSObjectType.String;
                                    return;
                                }
                            case JSObjectType.NotExists:
                            case JSObjectType.NotExistsInObject:
                            case JSObjectType.Undefined:
                                {
                                    resultContainer.dValue = double.NaN;
                                    resultContainer.valueType = JSObjectType.Double;
                                    return;
                                }
                            case JSObjectType.Object: // x+null
                                {
                                    resultContainer.iValue = first.iValue;
                                    resultContainer.valueType = JSObjectType.Int;
                                    return;
                                }
                        }
                        throw new NotImplementedException();
                    }
                case JSObjectType.Double:
                    {
                        if (second.valueType >= JSObjectType.Object)
                            second = second.ToPrimitiveValue_Value_String();
                        switch (second.valueType)
                        {
                            case JSObjectType.Int:
                            case JSObjectType.Bool:
                                {
                                    resultContainer.valueType = JSObjectType.Double;
                                    resultContainer.dValue = first.dValue + second.iValue;
                                    return;
                                }
                            case JSObjectType.Double:
                                {
                                    resultContainer.valueType = JSObjectType.Double;
                                    resultContainer.dValue = first.dValue + second.dValue;
                                    return;
                                }
                            case JSObjectType.String:
                                {
                                    resultContainer.oValue = new RopeString(Tools.DoubleToString(first.dValue), second.oValue);
                                    resultContainer.valueType = JSObjectType.String;
                                    return;
                                }
                            case JSObjectType.Object: // null
                                {
                                    resultContainer.dValue = first.dValue;
                                    resultContainer.valueType = JSObjectType.Double;
                                    return;
                                }
                            case JSObjectType.NotExists:
                            case JSObjectType.NotExistsInObject:
                            case JSObjectType.Undefined:
                                {
                                    resultContainer.dValue = double.NaN;
                                    resultContainer.valueType = JSObjectType.Double;
                                    return;
                                }
                            default:
                                throw new NotImplementedException();
                        }
                    }
                case JSObjectType.String:
                    {
                        object tstr = first.oValue;
                        switch (second.valueType)
                        {
                            case JSObjectType.String:
                                {
                                    tstr = new RopeString(tstr, second.oValue);
                                    break;
                                }
                            case JSObjectType.Bool:
                                {
                                    tstr = new RopeString(tstr, second.iValue != 0 ? "true" : "false");
                                    break;
                                }
                            case JSObjectType.Int:
                                {
                                    tstr = new RopeString(tstr, second.iValue.ToString(CultureInfo.InvariantCulture));
                                    break;
                                }
                            case JSObjectType.Double:
                                {
                                    tstr = new RopeString(tstr, Tools.DoubleToString(second.dValue));
                                    break;
                                }
                            case JSObjectType.Undefined:
                            case JSObjectType.NotExistsInObject:
                                {
                                    tstr = new RopeString(tstr, "undefined");
                                    break;
                                }
                            case JSObjectType.Object:
                            case JSObjectType.Function:
                            case JSObjectType.Date:
                                {
                                    tstr = new RopeString(tstr, second.ToString());
                                    break;
                                }
                        }
                        resultContainer.oValue = tstr;
                        resultContainer.valueType = JSObjectType.String;
                        return;
                    }
                case JSObjectType.Date:
                    {
                        first = first.ToPrimitiveValue_String_Value();
                        Impl(resultContainer, first, second);
                        return;
                    }
                case JSObjectType.NotExistsInObject:
                case JSObjectType.Undefined:
                    {
                        if (second.valueType >= JSObjectType.Object)
                            second = second.ToPrimitiveValue_Value_String();
                        switch (second.valueType)
                        {
                            case JSObjectType.String:
                                {
                                    resultContainer.valueType = JSObjectType.String;
                                    resultContainer.oValue = new RopeString("undefined", second.oValue);
                                    return;
                                }
                            case JSObjectType.Double:
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    resultContainer.valueType = JSObjectType.Double;
                                    resultContainer.dValue = double.NaN;
                                    return;
                                }
                            case JSObjectType.Object: // undefined+null
                            case JSObjectType.NotExistsInObject:
                            case JSObjectType.Undefined:
                                {
                                    resultContainer.valueType = JSObjectType.Double;
                                    resultContainer.dValue = double.NaN;
                                    return;
                                }
                        }
                        break;
                    }
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        first = first.ToPrimitiveValue_Value_String();
                        if (first.valueType == JSObjectType.Int || first.valueType == JSObjectType.Bool)
                            goto case JSObjectType.Int;
                        else if (first.valueType == JSObjectType.Object) // null
                        {
                            if (second.valueType >= JSObjectType.String)
                                second = second.ToPrimitiveValue_Value_String();
                            if (second.valueType == JSObjectType.String)
                            {
                                resultContainer.oValue = new RopeString("null", second.oValue);
                                resultContainer.valueType = JSObjectType.String;
                                return;
                            }
                            first.iValue = 0;
                            goto case JSObjectType.Int;
                        }
                        else if (first.valueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        else if (first.valueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        break;
                    }
            }
            throw new NotImplementedException();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            var res = base.Build(ref _this, depth, variables, state, message, statistic, opts);
            if (!res && _this == this)
            {
                if (first is StringConcat)
                {
                    _this = first;
                    (first as StringConcat).sources.Add(second);
                }
                else if (second is StringConcat)
                {
                    _this = second;
                    (second as StringConcat).sources.Insert(0, first);
                }
                else
                {
                    if (first is Constant && (first as Constant).value.valueType == JSObjectType.String)
                    {
                        if ((first as Constant).value.oValue.ToString() == "")
                            _this = new ToStr(second);
                        else
                            _this = new StringConcat(new List<Expression>() { first, second });
                    }
                    else if (second is Constant && (second as Constant).value.valueType == JSObjectType.String)
                    {
                        if ((second as Constant).value.oValue.ToString() == "")
                            _this = new ToStr(first);
                        else
                            _this = new StringConcat(new List<Expression>() { first, second });
                    }
                }
            }
            return res;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            base.Optimize(ref _this, owner, message, opts, statistic);
            //if (first.ResultType == PredictedType.String
            //    || second.ResultType == PredictedType.String)
            //{
            //    if (first is StringConcat)
            //    {
            //        _this = first;
            //        (first as StringConcat).sources.Add(second);
            //    }
            //    else if (second is StringConcat)
            //    {
            //        _this = second;
            //        (second as StringConcat).sources.Insert(0, first);
            //    }
            //    else
            //    {
            //        _this = new StringConcat(new List<Expression>() { first, second });
            //    }
            //    return;
            //}
            if (Tools.IsEqual(first.ResultType, PredictedType.Number, PredictedType.Group)
                && Tools.IsEqual(second.ResultType, PredictedType.Number, PredictedType.Group))
            {
                _this = new NumberAddition(first, second);
                return;
            }
        }
#if !PORTABLE && !NET35
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            var ft = first.TryCompile(false, false, null, dynamicValues);
            var st = second.TryCompile(false, false, null, dynamicValues);
            if (ft == st) // null == null
                return null;
            if (ft == null && st != null)
            {
                second = new CompiledNode(second, st, JITHelpers._items.GetValue(dynamicValues) as CodeNode[]);
                return null;
            }
            if (ft != null && st == null)
            {
                first = new CompiledNode(first, ft, JITHelpers._items.GetValue(dynamicValues) as CodeNode[]);
                return null;
            }
            if (ft.Type == st.Type)
                return System.Linq.Expressions.Expression.Add(ft, st);
            return null;
        }
#endif
        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " + " + second + ")";
        }
    }
}
