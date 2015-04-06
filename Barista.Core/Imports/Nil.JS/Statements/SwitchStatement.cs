﻿using System;
using System.Collections.Generic;
using System.Linq;
using Barista.NiL.JS.BaseLibrary;
using Barista.NiL.JS.Core;
using Barista.NiL.JS.Expressions;

namespace Barista.NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class SwitchCase
    {
        internal int index;
        internal CodeNode statement;

        public int Index { get { return index; } }
        public CodeNode Statement { get { return statement; } }
    }

#if !PORTABLE
    [Serializable]
#endif
    public sealed class SwitchStatement : CodeNode
    {
        private FunctionExpression[] functions;
        private readonly CodeNode[] lines;
        private SwitchCase[] cases;
        private CodeNode image;

        public FunctionExpression[] Functions { get { return functions; } }
        public CodeNode[] Body { get { return lines; } }
        public SwitchCase[] Cases { get { return cases; } }
        public CodeNode Image { get { return image; } }

        internal SwitchStatement(CodeNode[] body)
        {
            this.lines = body;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "switch (", ref i) && !Parser.Validate(state.Code, "switch(", ref i))
                return new ParseResult();
            while (char.IsWhiteSpace(state.Code[i])) i++;
            var image = ExpressionTree.Parse(state, ref i).Statement;
            if (state.Code[i] != ')')
                throw new JSException((new SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            if (state.Code[i] != '{')
                throw new JSException((new SyntaxError("Expected \"{\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var body = new List<CodeNode>();
            var funcs = new List<FunctionExpression>();
            var cases = new List<SwitchCase>();
            cases.Add(null);
            state.AllowBreak.Push(true);
            while (state.Code[i] != '}')
            {
                do
                {
                    if (Parser.Validate(state.Code, "case", i) && Parser.isIdentificatorTerminator(state.Code[i + 4]))
                    {
                        i += 4;
                        while (char.IsWhiteSpace(state.Code[i])) i++;
                        var sample = ExpressionTree.Parse(state, ref i).Statement;
                        if (state.Code[i] != ':')
                            throw new JSException((new SyntaxError("Expected \":\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                        i++;
                        cases.Add(new SwitchCase() { index = body.Count, statement = sample });
                    }
                    else if (Parser.Validate(state.Code, "default", i) && Parser.isIdentificatorTerminator(state.Code[i + 7]))
                    {
                        if (cases[0] != null)
                            throw new JSException((new SyntaxError("Duplicate default case in switch at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                        i += 7;
                        if (state.Code[i] != ':')
                            throw new JSException((new SyntaxError("Expected \":\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                        i++;
                        cases[0] = new SwitchCase() { index = body.Count, statement = null };
                    }
                    else break;
                    while (char.IsWhiteSpace(state.Code[i]) || (state.Code[i] == ';')) i++;
                } while (true);
                if (cases.Count == 1 && cases[0] == null)
                    throw new JSException((new SyntaxError("Switch statement must be contain cases. " + CodeCoordinates.FromTextPosition(state.Code, index, 0))));
                var t = Parser.Parse(state, ref i, 0);
                if (t == null)
                    continue;
                if (t is FunctionExpression)
                {
                    if (state.strict.Peek())
                        throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                    funcs.Add(t as FunctionExpression);
                }
                else
                    body.Add(t);
                while (char.IsWhiteSpace(state.Code[i]) || (state.Code[i] == ';')) i++;
            }
            state.AllowBreak.Pop();
            i++;
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new SwitchStatement(body.ToArray())
                {
                    functions = funcs.ToArray(),
                    cases = cases.ToArray(),
                    image = image,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Evaluate(Context context)
        {
            if (functions != null)
                throw new InvalidOperationException();
            int i = cases[0] != null ? cases[0].index : 0;
            var imageVal = image.Evaluate(context);
            for (int j = 1; j < cases.Length; j++)
            {
#if DEV
                if (context.debugging)
                    context.raiseDebugger(cases[j].statement);
#endif
                if (Expressions.StrictEqual.Check(imageVal, cases[j].statement.Evaluate(context)))
                {
                    i = cases[j].index;
                    break;
                }
            }
            while (i-- > 0)
            {
                context.lastResult = lines[i].Evaluate(context) ?? context.lastResult;
                if (context.abort != AbortType.None)
                {
                    if (context.abort == AbortType.Break)
                        context.abort = AbortType.None;
                    return context.abortInfo;
                }
            }
            return null;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (depth < 1)
                throw new InvalidOperationException();
            Parser.Build(ref image, 2, variables, state, message, statistic, opts);
            for (int i = 0; i < lines.Length; i++)
                Parser.Build(ref lines[i], 1, variables, state | _BuildState.Conditional, message, statistic, opts);
            for (int i = 0; functions != null && i < functions.Length; i++)
            {
                CodeNode stat = functions[i];
                Parser.Build(ref stat, 1, variables, state, message, statistic, opts);
                variables[functions[i].Name] = new VariableDescriptor(functions[i].Reference, true, functions[i].Reference.functionDepth);
            }
            functions = null;
            for (int i = 1; i < cases.Length; i++)
                Parser.Build(ref cases[i].statement, 2, variables, state, message, statistic, opts);
            for (int i = 0; i < lines.Length / 2; i++)
            {
                var t = lines[i];
                lines[i] = lines[lines.Length - 1 - i];
                lines[lines.Length - 1 - i] = t;
            }
            for (int i = cases[0] != null ? 0 : 1; i < cases.Length; i++)
                cases[i].index = lines.Length - cases[i].index;
            return false;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                image
            };
            res.AddRange(lines);
            res.AddRange(functions);
            res.AddRange(from c in cases select c.statement);
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override void Optimize(ref CodeNode _this, Expressions.FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            image.Optimize(ref image, owner, message, opts, statistic);
            for (var i = 1; i < cases.Length; i++)
                cases[i].statement.Optimize(ref cases[i].statement, owner, message, opts, statistic);
            for (var i = lines.Length; i-- > 0; )
            {
                var cn = lines[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, statistic);
                lines[i] = cn;
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            string res = "switch (" + image + ") {" + Environment.NewLine;
            var replp = Environment.NewLine;
            var replt = Environment.NewLine + "  ";
            for (int i = lines.Length; i-- > 0; )
            {
                for (int j = 0; j < cases.Length; j++)
                {
                    if (cases[j] != null && cases[j].index == i)
                    {
                        res += "case " + cases[j].statement + ":" + Environment.NewLine;
                    }
                }
                string lc = lines[i].ToString().Replace(replp, replt);
                res += "  " + lc + (lc[lc.Length - 1] != '}' ? ";" + Environment.NewLine : Environment.NewLine);
            }
            if (functions != null)
                for (var i = 0; i < functions.Length; i++)
                {
                    var func = functions[i].ToString().Replace(replp, replt);
                    res += "  " + func + Environment.NewLine;
                }
            return res + "}";
        }
    }
}