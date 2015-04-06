using System;
using Barista.NiL.JS.Core;

namespace Barista.NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ToNumber : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public ToNumber(Expression first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            return Tools.JSObjectToNumber(first.Evaluate(context), tempContainer);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "+" + first;
        }
    }
}