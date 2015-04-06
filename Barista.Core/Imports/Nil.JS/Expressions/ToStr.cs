﻿using System;
using Barista.NiL.JS.Core;

namespace Barista.NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ToStr : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.String;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public ToStr(Expression first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var t = first.Evaluate(context);
            if (t.valueType == JSObjectType.String)
                return t;
            tempContainer.valueType = JSObjectType.String;
            tempContainer.oValue = t.ToPrimitiveValue_Value_String().ToString();
            return tempContainer;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " + '')";
        }
    }
}