namespace Barista.Linq2Rest
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq.Expressions;

  /// <summary>
  /// Represents a class that, using the visitor pattern, visits all nodes in an expression.
  /// </summary>
  public abstract class ExpressionVisitor
  {
    /// <summary>
    /// Visits.
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public virtual Expression Visit(Expression exp)
    {
      if (exp != null)
      {
        ExpressionType nodeType = exp.NodeType;
        switch (nodeType)
        {
          case ExpressionType.Add:
          case ExpressionType.AddChecked:
          case ExpressionType.And:
          case ExpressionType.AndAlso:
          case ExpressionType.ArrayIndex:
          case ExpressionType.Coalesce:
          case ExpressionType.Divide:
          case ExpressionType.Equal:
          case ExpressionType.ExclusiveOr:
          case ExpressionType.GreaterThan:
          case ExpressionType.GreaterThanOrEqual:
          case ExpressionType.LeftShift:
          case ExpressionType.LessThan:
          case ExpressionType.LessThanOrEqual:
          case ExpressionType.Modulo:
          case ExpressionType.Multiply:
          case ExpressionType.MultiplyChecked:
          case ExpressionType.NotEqual:
          case ExpressionType.Or:
          case ExpressionType.OrElse:
          case ExpressionType.Power:
          case ExpressionType.RightShift:
          case ExpressionType.Subtract:
          case ExpressionType.SubtractChecked:
            {
              return this.VisitBinary((BinaryExpression) exp);
            }
          case ExpressionType.ArrayLength:
          case ExpressionType.Convert:
          case ExpressionType.ConvertChecked:
          case ExpressionType.Negate:
          case ExpressionType.UnaryPlus:
          case ExpressionType.NegateChecked:
          case ExpressionType.Not:
          case ExpressionType.Quote:
          case ExpressionType.TypeAs:
            {
              return this.VisitUnary((UnaryExpression) exp);
            }
          case ExpressionType.Call:
            {
              return this.VisitMethodCall((MethodCallExpression) exp);
            }
          case ExpressionType.Conditional:
            {
              return this.VisitConditional((ConditionalExpression) exp);
            }
          case ExpressionType.Constant:
            {
              return this.VisitConstant((ConstantExpression) exp);
            }
          case ExpressionType.Invoke:
            {
              return this.VisitInvocation((InvocationExpression) exp);
            }
          case ExpressionType.Lambda:
            {
              return this.VisitLambda((LambdaExpression) exp);
            }
          case ExpressionType.ListInit:
            {
              return this.VisitListInit((ListInitExpression) exp);
            }
          case ExpressionType.MemberAccess:
            {
              return this.VisitMemberAccess((MemberExpression) exp);
            }
          case ExpressionType.MemberInit:
            {
              return this.VisitMemberInit((MemberInitExpression) exp);
            }
          case ExpressionType.New:
            {
              return this.VisitNew((NewExpression) exp);
            }
          case ExpressionType.NewArrayInit:
          case ExpressionType.NewArrayBounds:
            {
              return this.VisitNewArray((NewArrayExpression) exp);
            }
          case ExpressionType.Parameter:
            {
              return this.VisitParameter((ParameterExpression) exp);
            }
          case ExpressionType.TypeIs:
            {
              return this.VisitTypeIs((TypeBinaryExpression) exp);
            }
        }
        throw new InvalidOperationException(String.Format("Unhandled Expression Type {0}", exp.NodeType));
      }

      return null;
    }

    /// <summary>
    /// Visits Binarily
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    protected virtual Expression VisitBinary(BinaryExpression b)
    {
      Expression expression = this.Visit(b.Left);
      Expression expression1 = this.Visit(b.Right);
      Expression expression2 = this.Visit(b.Conversion);
      if (expression != b.Left || expression1 != b.Right || expression2 != b.Conversion)
      {
        if (b.NodeType != ExpressionType.Coalesce)
        {
          return Expression.MakeBinary(b.NodeType, expression, expression1, b.IsLiftedToNull, b.Method);
        }

        return Expression.Coalesce(expression, expression1, expression2 as LambdaExpression);
      }

      return b;
    }

    internal virtual MemberBinding VisitBinding(MemberBinding binding)
    {
      MemberBindingType bindingType = binding.BindingType;
      switch (bindingType)
      {
        case MemberBindingType.Assignment:
          {
            return this.VisitMemberAssignment((MemberAssignment) binding);
          }
        case MemberBindingType.MemberBinding:
          {
            return this.VisitMemberMemberBinding((MemberMemberBinding) binding);
          }
        case MemberBindingType.ListBinding:
          {
            return this.VisitMemberListBinding((MemberListBinding) binding);
          }
      }
      throw new InvalidOperationException(String.Format("Unhandled Binding Type {0}", binding.BindingType));
    }

    internal virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
    {
      List<MemberBinding> memberBindings = null;
      int num = 0;
      int count = original.Count;
      while (num < count)
      {
        MemberBinding memberBinding = this.VisitBinding(original[num]);
        if (memberBindings == null)
        {
          if (memberBinding != original[num])
          {
            memberBindings = new List<MemberBinding>(count);
            for (int i = 0; i < num; i++)
            {
              memberBindings.Add(original[i]);
            }
            memberBindings.Add(memberBinding);
          }
        }
        else
        {
          memberBindings.Add(memberBinding);
        }
        num++;
      }
      if (memberBindings == null)
      {
        return original;
      }

      return memberBindings;
    }

    internal virtual Expression VisitConditional(ConditionalExpression c)
    {
      Expression expression = this.Visit(c.Test);
      Expression expression1 = this.Visit(c.IfTrue);
      Expression expression2 = this.Visit(c.IfFalse);
      if (expression != c.Test || expression1 != c.IfTrue || expression2 != c.IfFalse)
      {
        return Expression.Condition(expression, expression1, expression2);
      }

      return c;
    }

    internal virtual Expression VisitConstant(ConstantExpression c)
    {
      return c;
    }

    internal virtual ElementInit VisitElementInitializer(ElementInit initializer)
    {
      ReadOnlyCollection<Expression> expressions = this.VisitExpressionList(initializer.Arguments);
      if (expressions == initializer.Arguments)
      {
        return initializer;
      }

      return Expression.ElementInit(initializer.AddMethod, expressions);
    }

    internal virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
    {
      List<ElementInit> elementInits = null;
      int num = 0;
      int count = original.Count;
      while (num < count)
      {
        ElementInit elementInit = this.VisitElementInitializer(original[num]);
        if (elementInits == null)
        {
          if (elementInit != original[num])
          {
            elementInits = new List<ElementInit>(count);
            for (int i = 0; i < num; i++)
            {
              elementInits.Add(original[i]);
            }
            elementInits.Add(elementInit);
          }
        }
        else
        {
          elementInits.Add(elementInit);
        }
        num++;
      }
      if (elementInits == null)
      {
        return original;
      }

      return elementInits;
    }

    internal virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
    {
      List<Expression> expressions = null;
      int num = 0;
      int count = original.Count;
      while (num < count)
      {
        Expression expression = this.Visit(original[num]);
        if (expressions == null)
        {
          if (expression != original[num])
          {
            expressions = new List<Expression>(count);
            for (int i = 0; i < num; i++)
            {
              expressions.Add(original[i]);
            }
            expressions.Add(expression);
          }
        }
        else
        {
          expressions.Add(expression);
        }
        num++;
      }
      if (expressions == null)
      {
        return original;
      }

      return expressions.AsReadOnly();
    }

    internal virtual Expression VisitInvocation(InvocationExpression iv)
    {
      IEnumerable<Expression> expressions = this.VisitExpressionList(iv.Arguments);
      Expression expression = this.Visit(iv.Expression);
      if (expressions != iv.Arguments || expression != iv.Expression)
      {
        return Expression.Invoke(expression, expressions);
      }

      return iv;
    }

    internal virtual Expression VisitLambda(LambdaExpression lambda)
    {
      Expression expression = this.Visit(lambda.Body);
      if (expression == lambda.Body)
      {
        return lambda;
      }

      return Expression.Lambda(lambda.Type, expression, lambda.Parameters);
    }

    internal virtual Expression VisitListInit(ListInitExpression init)
    {
      NewExpression newExpression = this.VisitNew(init.NewExpression);
      IEnumerable<ElementInit> elementInits = this.VisitElementInitializerList(init.Initializers);
      if (newExpression != init.NewExpression || elementInits != init.Initializers)
      {
        return Expression.ListInit(newExpression, elementInits);
      }

      return init;
    }

    internal virtual Expression VisitMemberAccess(MemberExpression m)
    {
      Expression expression = this.Visit(m.Expression);
      if (expression == m.Expression)
      {
        return m;
      }

      return Expression.MakeMemberAccess(expression, m.Member);
    }

    internal virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
    {
      Expression expression = this.Visit(assignment.Expression);
      if (expression == assignment.Expression)
      {
        return assignment;
      }

      return Expression.Bind(assignment.Member, expression);
    }

    internal virtual Expression VisitMemberInit(MemberInitExpression init)
    {
      NewExpression newExpression = this.VisitNew(init.NewExpression);
      IEnumerable<MemberBinding> memberBindings = this.VisitBindingList(init.Bindings);
      if (newExpression != init.NewExpression || memberBindings != init.Bindings)
      {
        return Expression.MemberInit(newExpression, memberBindings);
      }

      return init;
    }

    internal virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
    {
      IEnumerable<ElementInit> elementInits = this.VisitElementInitializerList(binding.Initializers);
      if (elementInits == binding.Initializers)
      {
        return binding;
      }

      return Expression.ListBind(binding.Member, elementInits);
    }

    internal virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
    {
      IEnumerable<MemberBinding> memberBindings = this.VisitBindingList(binding.Bindings);
      if (memberBindings == binding.Bindings)
      {
        return binding;
      }

      return Expression.MemberBind(binding.Member, memberBindings);
    }

    internal virtual Expression VisitMethodCall(MethodCallExpression m)
    {
      Expression expression = this.Visit(m.Object);
      IEnumerable<Expression> expressions = this.VisitExpressionList(m.Arguments);
      if (expression != m.Object || expressions != m.Arguments)
      {
        return Expression.Call(expression, m.Method, expressions);
      }

      return m;
    }

    internal virtual NewExpression VisitNew(NewExpression nex)
    {
      IEnumerable<Expression> expressions = this.VisitExpressionList(nex.Arguments);
      if (expressions == nex.Arguments)
      {
        return nex;
      }

      return Expression.New(nex.Constructor, expressions, nex.Members);
    }

    internal virtual Expression VisitNewArray(NewArrayExpression na)
    {
      IEnumerable<Expression> expressions = this.VisitExpressionList(na.Expressions);
      if (expressions == na.Expressions)
      {
        return na;
      }

      if (na.NodeType != ExpressionType.NewArrayInit)
      {
        return Expression.NewArrayBounds(na.Type.GetElementType(), expressions);
      }

      return Expression.NewArrayInit(na.Type.GetElementType(), expressions);
    }

    /// <summary>
    /// Visits the parameter.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    protected virtual Expression VisitParameter(ParameterExpression p)
    {
      return p;
    }

    internal virtual Expression VisitTypeIs(TypeBinaryExpression b)
    {
      Expression expression = this.Visit(b.Expression);
      if (expression == b.Expression)
      {
        return b;
      }

      return Expression.TypeIs(expression, b.TypeOperand);
    }

    internal virtual Expression VisitUnary(UnaryExpression u)
    {
      Expression expression = this.Visit(u.Operand);
      if (expression == u.Operand)
      {
        return u;
      }

      return Expression.MakeUnary(u.NodeType, expression, u.Type, u.Method);
    }
  }
}