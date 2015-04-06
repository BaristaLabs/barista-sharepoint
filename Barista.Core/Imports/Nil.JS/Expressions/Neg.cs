﻿using System;
using Barista.NiL.JS.Core;

namespace Barista.NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Neg : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number; // -int.MinValue == int.MinValue, но должно быть -int.MinValue == -(double)int.MinValue;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public Neg(Expression first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var val = first.Evaluate(context);
            if (val.valueType == JSObjectType.Int
                || val.ValueType == JSObjectType.Bool)
            {
                if (val.iValue == 0)
                {
                    tempContainer.valueType = JSObjectType.Double;
                    tempContainer.dValue = -0.0;
                }
                else
                {
                    if (val.iValue == int.MinValue)
                    {
                        tempContainer.valueType = JSObjectType.Double;
                        tempContainer.dValue = val.iValue;
                    }
                    else
                    {
                        tempContainer.valueType = JSObjectType.Int;
                        tempContainer.iValue = -val.iValue;
                    }
                }
            }
            else
            {
                tempContainer.dValue = -Tools.JSObjectToDouble(val);
                tempContainer.valueType = JSObjectType.Double;
            }
            return tempContainer;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "-" + first;
        }
    }
}