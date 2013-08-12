using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Barista.Imports.CamlexNET.Impl.Operands;
using Barista.Imports.CamlexNET.Interfaces;
using NUnit.Framework;

namespace Barista.Imports.CamlexNET.UnitTests.ReverseEngeneering.Operands
{
    [TestFixture]
    public class ValuesValueOperandTests
    {
        [Test]
        public void test_THAT_operand_with_several_operands_IS_conveted_to_expression_correctly()
        {
            var values = new List<IOperand>();
            values.Add(new TextValueOperand("test1"));
            values.Add(new TextValueOperand("test2"));

            var op = new ValuesValueOperand(values);
            var expr = op.ToExpression();
            Assert.That(expr.ToString(), Is.EqualTo("new [] {\"test1\", \"test2\"}"));
        }
    }
}
