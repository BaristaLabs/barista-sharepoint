﻿using System;
using Barista.NiL.JS.Core;

namespace Barista.NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ContinueStatement : CodeNode
    {
        private JSObject label;

        public JSObject Label { get { return label; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "continue", ref i) || !Parser.isIdentificatorTerminator(state.Code[i]))
                return new ParseResult();
            if (!state.AllowContinue.Peek())
                throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("Invalid use continue statement")));
            while (char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i])) i++;
            int sl = i;
            JSObject label = null;
            if (Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
            {
                label = Tools.Unescape(state.Code.Substring(sl, i - sl), state.strict.Peek());
                if (!state.Labels.Contains(label.oValue.ToString()))
                    throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("Try to continue to undefined label.")));
            }
            int pos = index;
            index = i;
            state.continiesCount++;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ContinueStatement()
                {
                    label = label,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Evaluate(Context context)
        {
            context.abort = AbortType.Continue;
            context.abortInfo = label;
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "continue" + (label != null ? " " + label : "");
        }
    }
}