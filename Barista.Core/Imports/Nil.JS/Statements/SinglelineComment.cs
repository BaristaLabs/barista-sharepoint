﻿using Barista.NiL.JS.Core;

namespace Barista.NiL.JS.Statements
{
    /// <summary>
    /// TODO
    /// </summary>
    internal sealed class SinglelineComment : CodeNode
    {
        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "//", ref i))
                return new ParseResult();
            while (i < state.Code.Length && state.Code[i] != '\r' && state.Code[i] != '\n' && !Tools.isLineTerminator(state.Code[i])) i++;
            int end = i;
            if (i < state.Code.Length)
            {
                if (state.Code[i] != '\r')
                {
                    if (state.Code[i] != '\n')
                        i++;
                }
                else if (state.Code[i] != '\n')
                {
                    if (state.Code[i] != '\r')
                        i++;
                }
            }
            try
            {
                return new ParseResult()
                {
                    IsParsed = true,
                    Statement = new SinglelineComment(state.Code.Substring(index + 2, end - index - 2))
                    {
                        Length = i - index,
                        Position = index
                    }
                };
            }
            finally
            {
                index = i;
            }
        }

        public string Text { get; private set; }

        public SinglelineComment(string text)
        {
            Text = text;
        }

        internal override NiL.JS.Core.JSObject Evaluate(NiL.JS.Core.Context context)
        {
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
            return "//" + Text;
        }
    }
}
