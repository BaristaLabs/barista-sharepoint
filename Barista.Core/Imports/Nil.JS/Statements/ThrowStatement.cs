﻿using System;
using System.Collections.Generic;
using Barista.NiL.JS.BaseLibrary;
using Barista.NiL.JS.Core;
using Barista.NiL.JS.Core.TypeProxing;
using Barista.NiL.JS.Expressions;

namespace Barista.NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ThrowStatement : CodeNode
    {
        private CodeNode body;

        public ThrowStatement(Exception e)
        {
            body = new Constant(TypeProxy.Proxy(e));
        }

        internal ThrowStatement(CodeNode statement)
        {
            body = statement;
        }

        public CodeNode Body { get { return body; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "throw", ref i) || (!char.IsWhiteSpace(state.Code[i]) && (state.Code[i] != '(')))
                return new ParseResult();
            var b = Parser.Parse(state, ref i, 1, true);
            if (b is EmptyStatement)
                throw new JSException((new SyntaxError("Can't throw result of EmptyStatement " + CodeCoordinates.FromTextPosition(state.Code, i - 1, 0))));
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ThrowStatement(b)
                {
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Evaluate(Context context)
        {
            throw new JSException(body.Evaluate(context));
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref body, 2, variables, state, message, statistic, opts);
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            if (body != null)
                body.Optimize(ref body, owner, message, opts, statistic);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "throw" + (body != null ? " " + body : "");
        }
    }
}