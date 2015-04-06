﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Barista.NiL.JS.BaseLibrary;
using Barista.NiL.JS.Core.JIT;
using Barista.NiL.JS.Expressions;

namespace Barista.NiL.JS.Core
{
    [Flags]
    internal enum _BuildState
    {
        None = 0,
        Strict = 1,
        //ForAssign = 2,
        Conditional = 4,
        InLoop = 8,
        InWith = 16
    }

#if !PORTABLE
    [Serializable]
#endif
    public abstract class CodeNode
    {
        internal static readonly CodeNode[] emptyCodeNodeArray = new CodeNode[0];
#if !PORTABLE
        internal static readonly MethodInfo EvaluateForAssignMethod = typeof(CodeNode).GetMethod("EvaluateForAssing", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null);
        internal static readonly MethodInfo EvaluateMethod = typeof(CodeNode).GetMethod("Evaluate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null);

#if !NET35
        internal System.Linq.Expressions.Expression JitOverCall(bool forAssign)
        {
            return System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Constant(this),
                this.GetType().GetMethod(forAssign ? "EvaluateForAssing" : "Evaluate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null),
                JITHelpers.ContextParameter
                );
        }
#endif
#endif
        public virtual bool Eliminated { get; internal set; }
        public virtual int Position { get; internal set; }
        public virtual int Length { get; internal set; }
        public virtual int EndPosition { get { return Position + Length; } }

        private CodeNode[] childs;
        public CodeNode[] Childs { get { return childs ?? (childs = getChildsImpl() ?? emptyCodeNodeArray); } }

        protected abstract CodeNode[] getChildsImpl();

        internal virtual JSObject EvaluateForAssing(NiL.JS.Core.Context context)
        {
            return raiseInvalidAssignment();
        }

        protected static JSObject raiseInvalidAssignment()
        {
            throw new JSException(new ReferenceError("Invalid left-hand side in assignment."));
        }

        internal abstract JSObject Evaluate(Context context);

        /// <summary>
        /// Заставляет объект перестроить своё содержимое перед началом выполнения.
        /// </summary>
        /// <param name="_this">Ссылка на экземпляр, для которого происходит вызов функции</param>
        /// <param name="depth">Глубина погружения в выражении</param>
        /// <param name="functionDepth">Глубина погружения в функции. Увеличивается при входе в функцию и уменьшается при выходе из нее</param>
        /// <returns>true если были внесены изменения и требуется повторный вызов функции</returns>
        internal virtual bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            return false;
        }

        internal virtual void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {

        }
#if !PORTABLE
        internal virtual System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            return null;
        }
#endif
        public abstract T Visit<T>(Visitor<T> visitor);
    }
}
