﻿using System;
using Barista.NiL.JS.Core;

namespace Barista.NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class NumberLess : Expression
    {
        protected internal override Core.PredictedType ResultType
        {
            get
            {
                return Core.PredictedType.Bool;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public NumberLess(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        internal override Core.JSObject Evaluate(Core.Context context)
        {
            int itemp;
            double dtemp;
            var op = first.Evaluate(context);
            if (op.valueType == Core.JSObjectType.Int)
            {
                itemp = op.iValue;
                op = second.Evaluate(context);
                if (op.valueType == Core.JSObjectType.Int)
                {
                    return itemp < op.iValue;
                }
                else if (op.valueType == Core.JSObjectType.Double)
                {
                    return itemp < op.dValue;
                }
                else
                {
                    if (tempContainer == null)
                        tempContainer = new JSObject() { attributes = JSObjectAttributesInternal.Temporary };
                    tempContainer.valueType = JSObjectType.Int;
                    tempContainer.iValue = itemp;
                    return Less.Check(tempContainer, op);
                }
            }
            else if (op.valueType == Core.JSObjectType.Double)
            {
                dtemp = op.dValue;
                op = second.Evaluate(context);
                if (op.valueType == Core.JSObjectType.Int)
                {
                    return dtemp < op.iValue;
                }
                else if (op.valueType == Core.JSObjectType.Double)
                {
                    return dtemp < op.dValue;
                }
                else
                {
                    if (tempContainer == null)
                        tempContainer = new JSObject() { attributes = JSObjectAttributesInternal.Temporary };
                    tempContainer.valueType = JSObjectType.Double;
                    tempContainer.dValue = dtemp;
                    return Less.Check(tempContainer, op);
                }
            }
            else
            {
                if (tempContainer == null)
                    tempContainer = new JSObject() { attributes = JSObjectAttributesInternal.Temporary };
                var temp = tempContainer;
                temp.Assign(op);
                tempContainer = null;
                var res = Less.Check(temp, second.Evaluate(context));
                tempContainer = temp;
                return res;
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " < " + second + ")";
        }
    }
}
