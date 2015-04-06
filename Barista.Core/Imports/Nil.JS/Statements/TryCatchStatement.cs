﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barista.NiL.JS.Core;
using Barista.NiL.JS.BaseLibrary;
using Barista.NiL.JS.Core.TypeProxing;

namespace Barista.NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class TryCatchStatement : CodeNode
    {
        private CodeNode body;
        private CodeBlock catchBody;
        private CodeNode finallyBody;
        private VariableDescriptor catchVariableDesc;

        public CodeNode Body { get { return body; } }
        public CodeNode CatchBody { get { return catchBody; } }
        public CodeNode FinalBody { get { return finallyBody; } }
        public string ExceptionVariableName { get { return catchVariableDesc.name; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            if (!Parser.Validate(state.Code, "try", ref i) || !Parser.isIdentificatorTerminator(state.Code[i]))
                return new ParseResult();
            while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i])) i++;
            if (i >= state.Code.Length)
                throw new JSException(new SyntaxError("Unexpected end of line."));
            if (state.Code[i] != '{')
                throw new JSException((new SyntaxError("Invalid try statement definition at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            var b = CodeBlock.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            CodeNode cb = null;
            string exptn = null;
            if (Parser.Validate(state.Code, "catch (", ref i) || Parser.Validate(state.Code, "catch(", ref i))
            {
                int s = i;
                if (!Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
                    throw new JSException((new SyntaxError("Catch block must contain variable name " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                exptn = Tools.Unescape(state.Code.Substring(s, i - s), state.strict.Peek());
                if (state.strict.Peek())
                {
                    if (exptn == "arguments" || exptn == "eval")
                        throw new JSException((new SyntaxError("Varible name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                }
                while (char.IsWhiteSpace(state.Code[i])) i++;
                if (!Parser.Validate(state.Code, ")", ref i))
                    throw new JSException((new SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                while (char.IsWhiteSpace(state.Code[i])) i++;
                if (state.Code[i] != '{')
                    throw new JSException((new SyntaxError("Invalid catch block statement definition at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                state.functionsDepth++;
                try
                {
                    cb = CodeBlock.Parse(state, ref i).Statement;
                }
                finally
                {
                    state.functionsDepth--;
                }
                while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i])) i++;
            }
            CodeNode f = null;
            if (Parser.Validate(state.Code, "finally", i) && Parser.isIdentificatorTerminator(state.Code[i + 7]))
            {
                i += 7;
                while (char.IsWhiteSpace(state.Code[i])) i++;
                if (state.Code[i] != '{')
                    throw new JSException((new SyntaxError("Invalid finally block statement definition at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                f = CodeBlock.Parse(state, ref i).Statement;
            }
            if (cb == null && f == null)
                throw new JSException((new SyntaxError("try block must contain 'catch' or/and 'finally' block")));
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new TryCatchStatement()
                {
                    body = (CodeBlock)b,
                    catchBody = (CodeBlock)cb,
                    finallyBody = (CodeBlock)f,
                    catchVariableDesc = new VariableDescriptor(exptn, state.functionsDepth + 1),
                    Position = pos,
                    Length = index - pos
                }
            };
        }
#if !NET35
        /*internal override System.Linq.Expressions.Expression CompileToIL(Core.JIT.TreeBuildingState state)
        {
            var except = Expression.Parameter(typeof(object));
            Expression impl = null;
            if (finallyBody == null)
            {
                var bd = body.CompileToIL(state);
                if (bd.Type == typeof(void))
                    bd = Expression.Block(bd, JITHelpers.UndefinedConstant);
                impl = Expression.TryCatch(bd, Expression.Catch(except, makeCatch(except, state)));
            }
            else
            {
                var sexcept = Expression.Parameter(typeof(object));
                var sexceptb = Expression.Parameter(typeof(object));
                var rethrow = Expression.Parameter(typeof(bool));
                var finallyProc = Expression.Parameter(typeof(bool));
                LabelTarget breakTarget = null;
                if (state.BreakLabels.Count != 0)
                {
                    breakTarget = Expression.Label();
                    state.BreakLabels.Push(breakTarget);
                }
                LabelTarget continueTarget = null;
                if (state.ContinueLabels.Count != 0)
                {
                    continueTarget = Expression.Label();
                    state.ContinueLabels.Push(continueTarget);
                }
                LabelTarget finallyBodyTarget = null;
                var tempResult = Expression.Parameter(typeof(JSObject));
                finallyBodyTarget = Expression.Label();
                LabelTarget newReturnTarget = null;
                LabelTarget oldRetirnTarget = null;
                if (state.ReturnTarget != null)
                {
                    newReturnTarget = Expression.Label();
                    oldRetirnTarget = state.ReturnTarget;
                    state.ReturnTarget = newReturnTarget;
                }
                var oldNamedBreaks = new Dictionary<string, LabelTarget>(state.NamedBreakLabels);
                var oldNamedContinies = new Dictionary<string, LabelTarget>(state.NamedContinueLabels);
                List<KeyValuePair<string, LabelTarget>> nBreaks = new List<KeyValuePair<string, LabelTarget>>();
                List<KeyValuePair<string, LabelTarget>> nContinues = new List<KeyValuePair<string, LabelTarget>>();
                if (oldNamedBreaks.Count > 0)
                    foreach (var i in oldNamedBreaks)
                    {
                        nBreaks.Add(new KeyValuePair<string, LabelTarget>(i.Key, Expression.Label()));
                        state.NamedBreakLabels[i.Key] = nBreaks[nBreaks.Count - 1].Value;
                    }
                if (oldNamedContinies.Count > 0)
                    foreach (var i in oldNamedContinies)
                    {
                        nContinues.Add(new KeyValuePair<string, LabelTarget>(i.Key, Expression.Label()));
                        state.NamedContinueLabels[i.Key] = nContinues[nContinues.Count - 1].Value;
                    }
                var labelId = Expression.Parameter(typeof(int));
                state.TryFinally++;
                var bd = body.CompileToIL(state);
                if (bd.Type == typeof(void))
                    bd = Expression.Block(bd, JITHelpers.UndefinedConstant);
                List<Expression> implList = new List<Expression>()
                    {
                        Expression.TryCatch(bd, Expression.Catch(except, Expression.TryCatch(
                            makeCatch(except, state),
                            Expression.Catch(sexcept, Expression.Block(
                                    Expression.Assign(rethrow, JITHelpers.wrap(true)),
                                    Expression.Assign(sexceptb, sexcept),
                                    JITHelpers.UndefinedConstant)
                        ))))
                    };
                implList.Add(Expression.Goto(finallyBodyTarget, Expression.Empty()));
                if (breakTarget != null)
                {
                    implList.Add(Expression.Label(breakTarget));
                    implList.Add(Expression.Assign(labelId, JITHelpers.wrap(0)));
                    implList.Add(Expression.Goto(finallyBodyTarget));
                    state.BreakLabels.Pop();
                }
                if (continueTarget != null)
                {
                    implList.Add(Expression.Label(continueTarget));
                    implList.Add(Expression.Assign(labelId, JITHelpers.wrap(1)));
                    implList.Add(Expression.Goto(finallyBodyTarget));
                    state.ContinueLabels.Pop();
                }
                if (newReturnTarget != null)
                {
                    implList.Add(Expression.Label(newReturnTarget));
                    implList.Add(Expression.Assign(tempResult, Expression.Field(JITHelpers.ContextParameter, "abortInfo")));
                    implList.Add(Expression.Assign(labelId, JITHelpers.wrap(2)));
                    implList.Add(Expression.Goto(finallyBodyTarget));
                }
                for (var i = nBreaks.Count; i-- > 0; )
                {
                    implList.Add(Expression.Label(nBreaks[i].Value));
                    implList.Add(Expression.Assign(labelId, JITHelpers.wrap(i + 3)));
                    implList.Add(Expression.Goto(finallyBodyTarget));
                }
                for (var i = nContinues.Count; i-- > 0; )
                {
                    implList.Add(Expression.Label(nContinues[i].Value));
                    implList.Add(Expression.Assign(labelId, JITHelpers.wrap(-i - 1)));
                    implList.Add(Expression.Goto(finallyBodyTarget));
                }
                state.NamedBreakLabels = oldNamedBreaks;
                state.NamedContinueLabels = oldNamedContinies;
                state.ReturnTarget = oldRetirnTarget;
                state.TryFinally--;
                implList.Add(Expression.Label(finallyBodyTarget));
                implList.Add(finallyBody.CompileToIL(state));
                implList.Add(Expression.IfThen(rethrow, Expression.Throw(sexceptb)));
                if (state.BreakLabels.Count > 0)
                    implList.Add(Expression.IfThen(Expression.Equal(labelId, JITHelpers.wrap(0)), Expression.Goto(state.BreakLabels.Peek())));
                if (state.ContinueLabels.Count > 0)
                    implList.Add(Expression.IfThen(Expression.Equal(labelId, JITHelpers.wrap(1)), Expression.Goto(state.ContinueLabels.Peek())));
                if (state.ReturnTarget != null)
                {
                    if (state.TryFinally <= 0)
                        implList.Add(Expression.IfThen(Expression.Equal(labelId, JITHelpers.wrap(2)), Expression.Goto(state.ReturnTarget, tempResult)));
                    else
                        implList.Add(Expression.IfThen(Expression.Equal(labelId, JITHelpers.wrap(2)), Expression.Block(Expression.Assign(Expression.Field(JITHelpers.ContextParameter, "abortInfo"), tempResult), Expression.Goto(state.ReturnTarget, tempResult))));
                }
                for (var i = nBreaks.Count; i-- > 0; )
                    implList.Add(Expression.IfThen(Expression.Equal(labelId, JITHelpers.wrap(i + 3)), Expression.Goto(state.NamedBreakLabels[nBreaks[i].Key])));
                for (var i = nContinues.Count; i-- > 0; )
                    implList.Add(Expression.IfThen(Expression.Equal(labelId, JITHelpers.wrap(-i - 1)), Expression.Goto(state.NamedContinueLabels[nContinues[i].Key])));
                impl = Expression.Block(new[] { sexceptb, rethrow, labelId, tempResult },
                    implList);
            }
            return Expression.Block(new[] { except }, impl);
        }*/
#endif
        internal override JSObject Evaluate(Context context)
        {
            Exception except = null;
            try
            {
                body.Evaluate(context);
            }
            catch (Exception e)
            {
                if (catchBody != null)
                    catchHandler(context, e);
                else except = e;
            }
            finally
            {
                if (finallyBody != null)
                    if (finallyHandler(context))
                        except = null;
            }
            if (except != null)
                throw except;
            return null;
        }

        private bool finallyHandler(Context context)
        {
#if DEV
            if (context.debugging)
                context.raiseDebugger(finallyBody);
#endif
            var abort = context.abort;
            var ainfo = context.abortInfo;
            if (abort == AbortType.Return && ainfo != null)
            {
                if (ainfo.IsDefinded)
                    ainfo = ainfo.CloneImpl();
                else
                    ainfo = JSObject.Undefined;
            }
            context.abort = AbortType.None;
            context.abortInfo = JSObject.undefined;
            finallyBody.Evaluate(context);
            if (context.abort == AbortType.None)
            {
                context.abort = abort;
                context.abortInfo = ainfo;
            }
            else
                return true;
            return false;
        }
#if !NET35
        /*private Expression makeCatch(ParameterExpression except, Core.JIT.TreeBuildingState state)
        {
            if (catchBody == null)
                return JITHelpers.UndefinedConstant;
            var cb = catchBody.CompileToIL(state);
            if (cb.Type == typeof(void))
                cb = Expression.Block(cb, JITHelpers.UndefinedConstant);
            var catchContext = Expression.Parameter(typeof(Context));
            var tempContainer = Expression.Parameter(typeof(Context));
            return Expression.Block(new[] { catchContext, tempContainer }
                , Expression.Assign(catchContext, Expression.Call(new Func<Context, Context>(createCatchContext).Method, JITHelpers.ContextParameter))
                , Expression.Call(JITHelpers.methodof(prepareCatchVar), JITHelpers.wrap(catchVariableDesc.name), except, catchContext)
                , Expression.TryFinally(
                    Expression.Block(
                        Expression.Assign(tempContainer, JITHelpers.ContextParameter)
                        , Expression.Assign(JITHelpers.ContextParameter, catchContext)
                        , Expression.Call(catchContext, typeof(WithContext).GetMethod("Activate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                        , cb
                    )
                    , Expression.Block(
                        Expression.Assign(JITHelpers.ContextParameter, tempContainer)
                        , Expression.Call(catchContext, typeof(WithContext).GetMethod("Deactivate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                        , Expression.Assign(Expression.Field(JITHelpers.ContextParameter, "abort"), Expression.Field(catchContext, "abort"))
                        , Expression.Assign(Expression.Field(JITHelpers.ContextParameter, "abortInfo"), Expression.Field(catchContext, "abortInfo"))
                    ))
                );
        }*/
#endif
        private void catchHandler(Context context, Exception e)
        {

#if DEV
            if (context.debugging)
                context.raiseDebugger(catchBody);
#endif
            if (catchBody.lines == null || catchBody.lines.Length == 0)
                return;
            JSObject cvar = null;
#if !PORTABLE
            if (e is RuntimeWrappedException)
            {
                cvar = new JSObject();
                cvar.Assign((e as RuntimeWrappedException).WrappedException as JSObject);
            }
            else
#endif
                cvar = e is JSException ? (e as JSException).Avatar.CloneImpl() : TypeProxy.Proxy(e);
            cvar.attributes |= JSObjectAttributesInternal.DoNotDelete;
            var catchContext = new CatchContext(cvar, context, catchVariableDesc.name);
#if DEBUG
            if (!(e is JSException))
                System.Diagnostics.Debugger.Break();
#endif
            try
            {
                catchContext.Activate();
                catchContext.lastResult = catchBody.Evaluate(catchContext) ?? catchContext.lastResult;
            }
            finally
            {
                context.lastResult = catchContext.lastResult ?? context.lastResult;
                catchContext.Deactivate();
            }
            context.abort = catchContext.abort;
            context.abortInfo = catchContext.abortInfo;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (statistic != null)
                statistic.ContainsTry = true;
            CodeNode b = null;
            Parser.Build(ref body, depth, variables, state | _BuildState.Conditional, message, statistic, opts);
            if (catchBody != null)
            {
                catchVariableDesc.owner = this;
                VariableDescriptor oldVarDesc = null;
                variables.TryGetValue(catchVariableDesc.name, out oldVarDesc);
                variables[catchVariableDesc.name] = catchVariableDesc;
                b = catchBody as CodeNode;
                Parser.Build(ref b, depth, variables, state | _BuildState.Conditional, message, statistic, opts);
                catchBody = b != null ? b as CodeBlock ?? ((b is EmptyStatement || b == null) ? new CodeBlock(new CodeNode[0], false) : new CodeBlock(new[] { b }, (state & _BuildState.Strict) != 0)) : new CodeBlock(new CodeNode[0], false);
                if (oldVarDesc != null)
                    variables[catchVariableDesc.name] = oldVarDesc;
                else
                    variables.Remove(catchVariableDesc.name);
                foreach (var v in variables)
                    v.Value.captured = true;
            }
            b = finallyBody as CodeNode;
            Parser.Build(ref b, depth, variables, state, message, statistic, opts);
            finallyBody = b != null ? b as CodeBlock ?? ((b is EmptyStatement || b == null) ? null : new CodeBlock(new[] { b }, (state & _BuildState.Strict) != 0)) : null;
            if (body == null
                || body is EmptyStatement
                || (body is CodeBlock && (body as CodeBlock).lines.Length == 0))
            {
                if (message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Empty (or reduced to empty) try" + (catchBody != null || finallyBody == null ? "..catch" : "") + (finallyBody != null ? "..finally" : "") + " block. Maybe, something missing.");
                _this = finallyBody;
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, Expressions.FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            CodeNode b = null;
            body.Optimize(ref body, owner, message, opts, statistic);
            if (catchBody != null)
            {
                b = catchBody as CodeNode;
                catchBody.Optimize(ref b, owner, message, opts, statistic);
                catchBody = b as CodeBlock ?? new CodeBlock(new[] { b }, owner.body.strict);
            }
            if (finallyBody != null)
            {
                b = finallyBody as CodeNode;
                finallyBody.Optimize(ref b, owner, message, opts, statistic);
                finallyBody = b as CodeBlock ?? new CodeBlock(new[] { b }, owner.body.strict);
            }
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body,
                catchBody,
                finallyBody
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            var sbody = body.ToString();
            var fbody = finallyBody == null ? "" : finallyBody.ToString();
            var cbody = catchBody == null ? "" : catchBody.ToString();
            return "try" + (body is CodeBlock ? sbody : " {" + Environment.NewLine + "  " + sbody + Environment.NewLine + "}") +
                (catchBody != null ?
                Environment.NewLine + "catch (" + catchVariableDesc + ")" +
                (catchBody is CodeBlock ? cbody : "{ " + cbody + " }") : "") +
                (finallyBody != null ?
                Environment.NewLine + "finally" +
                (finallyBody is CodeBlock ? fbody : " { " + fbody + " }") : "");
        }
    }
}