using System;
using System.Collections.Generic;
using Barista.NiL.JS.Core;
using Barista.NiL.JS.Core.JIT;

namespace Barista.NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class GetArgumentsExpression : GetVariableExpression
    {
        internal GetArgumentsExpression(int functionDepth)
            : base("arguments", functionDepth)
        {
        }

        internal override JSObject EvaluateForAssing(Context context)
        {
            var res = context.caller._arguments;
            if (res is Arguments)
                context.caller._arguments = res = res.CloneImpl();
            if (context.fields != null && context.fields.ContainsKey(Name))
                context.fields[Name] = res;
            return res;
        }

        internal sealed override JSObject Evaluate(Context context)
        {
            var res = context.caller._arguments;
            return res;
        }
    }

#if !PORTABLE
    [Serializable]
#endif
    public class GetVariableExpression : VariableReference
    {
        private string variableName;
        internal bool suspendThrow;
        internal bool forceThrow;

        public override string Name { get { return variableName; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal GetVariableExpression(string name, int functionDepth)
        {
            this.functionDepth = functionDepth;
            int i = 0;
            if ((name != "this") && !Parser.ValidateName(name, i, true, true, false))
                throw new ArgumentException("Invalid variable name");
            this.variableName = name;
        }

        internal override JSObject EvaluateForAssing(Context context)
        {
            if (context.strict || forceThrow)
            {
                var res = Descriptor.Get(context, false, functionDepth);
                if (res.valueType < JSObjectType.Undefined && (!suspendThrow || forceThrow))
                    throw new JSException((new NiL.JS.BaseLibrary.ReferenceError("Variable \"" + variableName + "\" is not defined.")));
                if ((res.attributes & JSObjectAttributesInternal.Argument) != 0)
                    context.caller.buildArgumentsObject();
                return res;
            }
            return descriptor.Get(context, true, functionDepth);
        }

        internal override JSObject Evaluate(Context context)
        {
            var res = descriptor.Get(context, false, functionDepth);
            switch (res.valueType)
            {
                case JSObjectType.NotExists:
                    if (suspendThrow)
                        break;
                    throw new JSException(new NiL.JS.BaseLibrary.ReferenceError("Variable \"" + variableName + "\" is not defined."));
                case JSObjectType.Property:
                    {
                        var getter = (res.oValue as PropertyPair).get;
                        if (getter == null)
                            return JSObject.notExists;
                        return getter.Invoke(context.objectSource, null);
                    }
            }
            return res;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        public override string ToString()
        {
            return variableName;
        }

#if !NET35 && !PORTABLE
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            dynamicValues.Add(this);
            var res = System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.ArrayAccess(JITHelpers.DynamicValuesParameter, JITHelpers.cnst(dynamicValues.Count - 1)),
                forAssign ? EvaluateForAssignMethod : EvaluateMethod,
                JITHelpers.ContextParameter
                );
            if (expectedType == typeof(int))
                res = System.Linq.Expressions.Expression.Call(JITHelpers.JSObjectToInt32Method, res);
            return res;
        }
#endif
        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            codeContext = state;

            if (statistic != null && variableName == "this")
                statistic.UseThis = true;
            VariableDescriptor desc = null;
            if (!variables.TryGetValue(variableName, out desc) || desc == null)
            {
                desc = new VariableDescriptor(this, false, functionDepth);
                descriptor = desc;
                variables[variableName] = this.Descriptor;
            }
            else
            {
                desc.references.Add(this);
                descriptor = desc;
            }
            if (depth >= 0 && depth < 2 && desc.IsDefined && (opts & Options.SuppressUselessExpressionsElimination) == 0)
            {
                _this = null;
                Eliminated = true;
                if (message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Unused get of defined variable was removed. Maybe, something missing.");
            }
            else if (variableName == "arguments"
                && functionDepth > 0)
            {
                if (statistic != null)
                    statistic.ContainsArguments = true;
                _this = new GetArgumentsExpression(functionDepth) { descriptor = descriptor };
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            base.Optimize(ref _this, owner, message, opts, statistic);
            if ((opts & Options.SuppressConstantPropogation) == 0
                && !descriptor.captured
                && descriptor.isDefined
                && !statistic.ContainsWith
                && !statistic.ContainsEval
                && (descriptor.owner != owner || !owner.statistic.ContainsArguments))
            {
                var assigns = descriptor.assignations;
                if (assigns != null && assigns.Count > 0)
                {
                    CodeNode lastAssign = null;
                    for (var i = 0; i < assigns.Count; i++)
                    {
                        if (assigns[i].Position == Position)
                        {
                            // ����������� �� �����������
                            lastAssign = null;
                            break;
                        }

                        if (assigns[i].Position > Position)
                        {
                            if ((codeContext & _BuildState.InLoop) != 0 && ((assigns[i] as Expression).codeContext & _BuildState.InLoop) != 0)
                            // ������������ ����� ���� ����� ����� �������������, �� ���� �� ��� � �����, �� ���������� ������� ����.
                            {
                                // ����������� �� �����������
                                lastAssign = null;
                                break;
                            }
                            continue; // ���������� ����
                        }

                        if (descriptor.isReadOnly)
                        {
                            if ((assigns[i] is Assign)
                                && (assigns[i] as Assign).first is Statements.VariableDefineStatement.AllowWriteCN)
                            {
                                lastAssign = assigns[i];
                                break;
                            }
                        }
                        else if (lastAssign == null || assigns[i].Position > lastAssign.Position)
                        {
                            lastAssign = assigns[i];
                        }
                    }
                    var assign = lastAssign as Assign;
                    if (assign != null && (assign.codeContext & _BuildState.Conditional) == 0 && assign.second is Constant)
                    {
                        _this = assign.second;
                    }
                }
            }
        }
    }
}