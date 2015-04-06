﻿using System;
using System.Collections.Generic;
using Barista.NiL.JS.Core;
using Barista.NiL.JS.BaseLibrary;
using Barista.NiL.JS.Statements;

namespace Barista.NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Json : Expression
    {
        private string[] fields;
        private CodeNode[] values;

        public CodeNode[] Initializators { get { return values; } }
        public string[] Fields { get { return fields; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Object;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        private Json(Dictionary<string, CodeNode> fields)
        {
            this.fields = new string[fields.Count];
            this.values = new CodeNode[fields.Count];
            int i = 0;
            foreach (var f in fields)
            {
                this.fields[i] = f.Key;
                this.values[i++] = f.Value;
            }
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            if (state.Code[index] != '{')
                throw new ArgumentException("Invalid JSON definition");
            var flds = new Dictionary<string, CodeNode>();
            int i = index;
            int pos = 0;
            while (state.Code[i] != '}')
            {
                do i++; while (char.IsWhiteSpace(state.Code[i]));
                int s = i;
                if (state.Code[i] == '}')
                    break;
                pos = i;
                if ((i = pos) >= 0 && Parser.Validate(state.Code, "set ", ref i)
                     && (state.Code[i] == '"' || state.Code[i] == '\'' || !Parser.isIdentificatorTerminator(state.Code[i])))
                {
                    i = pos;
                    var setter = FunctionExpression.Parse(state, ref i, FunctionType.Set).Statement as FunctionExpression;
                    if (!flds.ContainsKey(setter.Name))
                    {
                        var vle = new Constant(new JSObject() { valueType = JSObjectType.Object, oValue = new CodeNode[2] { setter, null } });
                        vle.value.valueType = JSObjectType.Property;
                        flds.Add(setter.Name, vle);
                    }
                    else
                    {
                        var vle = flds[setter.Name];
                        if (!(vle is Constant)
                            || (vle as Constant).value.valueType != JSObjectType.Property)
                            throw new JSException((new SyntaxError("Try to define setter for defined field at " + CodeCoordinates.FromTextPosition(state.Code, pos, 0))));
                        if (((vle as Constant).value.oValue as CodeNode[])[0] != null)
                            throw new JSException((new SyntaxError("Try to redefine setter " + setter.Name + " at " + CodeCoordinates.FromTextPosition(state.Code, pos, 0))));
                        ((vle as Constant).value.oValue as CodeNode[])[0] = setter;
                    }
                }
                else if ((i = pos) >= 0 && Parser.Validate(state.Code, "get ", ref i)
                    && (state.Code[i] == '"' || state.Code[i] == '\'' || !Parser.isIdentificatorTerminator(state.Code[i])))
                {
                    i = pos;
                    var getter = FunctionExpression.Parse(state, ref i, FunctionType.Get).Statement as FunctionExpression;
                    if (!flds.ContainsKey(getter.Name))
                    {
                        var vle = new Constant(new JSObject() { valueType = JSObjectType.Object, oValue = new CodeNode[2] { null, getter } });
                        vle.value.valueType = JSObjectType.Property;
                        flds.Add(getter.Name, vle);
                    }
                    else
                    {
                        var vle = flds[getter.Name];
                        if (!(vle is Constant)
                            || (vle as Constant).value.valueType != JSObjectType.Property)
                            throw new JSException((new SyntaxError("Try to define getter for defined field at " + CodeCoordinates.FromTextPosition(state.Code, pos, 0))));
                        if (((vle as Constant).value.oValue as CodeNode[])[1] != null)
                            throw new JSException((new SyntaxError("Try to redefine getter " + getter.Name + " at " + CodeCoordinates.FromTextPosition(state.Code, pos, 0))));
                        ((vle as Constant).value.oValue as CodeNode[])[1] = getter;
                    }
                }
                else
                {
                    i = pos;
                    var fieldName = "";
                    if (Parser.ValidateName(state.Code, ref i, false, true, state.strict.Peek()))
                        fieldName = Tools.Unescape(state.Code.Substring(s, i - s), state.strict.Peek());
                    else if (Parser.ValidateValue(state.Code, ref i))
                    {
                        double d = 0.0;
                        int n = s;
                        if (Tools.ParseNumber(state.Code, ref n, out d))
                            fieldName = Tools.DoubleToString(d);
                        else if (state.Code[s] == '\'' || state.Code[s] == '"')
                            fieldName = Tools.Unescape(state.Code.Substring(s + 1, i - s - 2), state.strict.Peek());
                        else if (flds.Count != 0)
                            throw new JSException((new SyntaxError("Invalid field name at " + CodeCoordinates.FromTextPosition(state.Code, pos, i - pos))));
                        else
                            return new ParseResult();
                    }
                    else
                        return new ParseResult();
                    while (char.IsWhiteSpace(state.Code[i]))
                        i++;
                    if (state.Code[i] != ':')
                        return new ParseResult();
                    do i++; while (char.IsWhiteSpace(state.Code[i]));
                    var initializator = ExpressionTree.Parse(state, ref i, false).Statement;
                    CodeNode aei = null;
                    if (flds.TryGetValue(fieldName, out aei))
                    {
                        if (((state.strict.Peek() && (!(aei is Constant) || (aei as Constant).value != JSObject.undefined))
                            || (aei is Constant && ((aei as Constant).value.valueType == JSObjectType.Property))))
                            throw new JSException(new SyntaxError("Try to redefine field \"" + fieldName + "\" at " + CodeCoordinates.FromTextPosition(state.Code, pos, i - pos)));
                        if (state.message != null)
                            state.message(MessageLevel.Warning, CodeCoordinates.FromTextPosition(state.Code, initializator.Position, 0), "Duplicate key \"" + fieldName + "\"");
                    }
                    flds[fieldName] = initializator;
                }
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
                if ((state.Code[i] != ',') && (state.Code[i] != '}'))
                    return new ParseResult();
            }
            i++;
            pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new Json(flds)
                {
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Evaluate(Context context)
        {
            var res = JSObject.CreateObject(false);
            if (fields.Length == 0)
                return res;
            res.fields = JSObject.createFields(fields.Length);
            for (int i = 0; i < fields.Length; i++)
            {
                var val = values[i].Evaluate(context);
                if (val.valueType == JSObjectType.Property)
                {
                    var gs = val.oValue as CodeNode[];
                    var prop = res.fields[fields[i]] = new JSObject();
                    prop.oValue = new PropertyPair(gs[1] != null ? gs[1].Evaluate(context) as Function : null, gs[0] != null ? gs[0].Evaluate(context) as Function : null);
                    prop.valueType = JSObjectType.Property;
                }
                else
                {
                    val = val.CloneImpl();
                    val.attributes = JSObjectAttributesInternal.None;
                    if (this.fields[i] == "__proto__")
                        res.__proto__ = val;
                    else
                        res.fields[this.fields[i]] = val;
                }
            }
            return res;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            codeContext = state;

            for (int i = 0; i < values.Length; i++)
            {
                if ((values[i] is Constant) && ((values[i] as Constant).value.valueType == JSObjectType.Property))
                {
                    var gs = (values[i] as Constant).value.oValue as CodeNode[];
                    Parser.Build(ref gs[0], 1,variables, state, message, statistic, opts);
                    Parser.Build(ref gs[1], 1,variables, state, message, statistic, opts);
                }
                else
                    Parser.Build(ref values[i], 2,variables, state, message, statistic, opts);
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            for (var i = Initializators.Length; i-- > 0; )
            {
                var cn = Initializators[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, statistic);
                Initializators[i] = cn as Expression;
            }
        }

        protected override CodeNode[] getChildsImpl()
        {
            return values;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            string res = "{ ";
            for (int i = 0; i < fields.Length; i++)
            {
                if ((values[i] is Constant) && ((values[i] as Constant).value.valueType == JSObjectType.Property))
                {
                    var gs = (values[i] as Constant).value.oValue as CodeNode[];
                    res += gs[0];
                    if (gs[0] != null && gs[1] != null)
                        res += ", ";
                    res += gs[1];
                }
                else
                    res += "\"" + fields[i] + "\"" + " : " + values[i];
                if (i + 1 < fields.Length)
                    res += ", ";
            }
            return res + " }";
        }
    }
}