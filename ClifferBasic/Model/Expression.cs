using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using ClifferBasic.Services;

namespace ClifferBasic.Model;

internal abstract class Expression {
    internal abstract object Evaluate(VariableStore variableStore);
}

internal abstract class LiteralExpression<T> : Expression  {
    internal T Value { get ; }

    internal LiteralExpression(T value) {
        Value = value;
    }

    internal override object Evaluate(VariableStore variableStore) {
        return Value!;
    }

    public override string ToString() {
        return $"{Value?.ToString()}";
    }

}

internal class NumberExpression : LiteralExpression<double> {
    internal NumberExpression(double value) : base(value) {}
}

internal class StringExpression : LiteralExpression<string> {
    internal StringExpression(string value) : base(value) { }
}

internal class BoolExpression : LiteralExpression<bool> {
    internal BoolExpression(bool value) : base(value) { }
}

internal class VariableExpression : Expression {
    internal string Name { get; }

    internal VariableExpression(string name) {
        Name = name;
    }

    internal override object Evaluate(VariableStore variableStore) {
        return variableStore.GetVariable(Name);
    }
}

internal class DoubleVariableExpression : VariableExpression {
    internal DoubleVariableExpression(string name) : base(name) { }

    internal double ToDouble(VariableStore store) {
        return Convert.ToDouble(Evaluate(store));
    }

    internal override object Evaluate(VariableStore variableStore) {
        Convert.ToDouble(variableStore.GetVariable(Name));
        return variableStore.GetVariable(Name);
    }

    internal T Evaluate<T>(VariableStore variableStore) {
        return ToDouble(variableStore);
    }

}

internal class StringVariableExpression : VariableExpression {
    internal StringVariableExpression(string name) : base(name) { }

    internal string ToString(VariableStore variableStore) {
        return variableStore.GetVariable(Name).ToString() ?? string.Empty;
    }

    internal override object Evaluate(VariableStore variableStore) {
        return variableStore.GetVariable(Name).ToString() ?? string.Empty;
    }
}

internal class AssignmentExpression : Expression {
    internal VariableExpression Left { get; }
    internal Expression Right { get; }

    internal AssignmentExpression(VariableExpression left, Expression right) {
        Left = left;
        Right = right;
    }

    internal override object Evaluate(VariableStore variableStore) {
        return variableStore.SetVariable(Left.Name, Right.Evaluate(variableStore));
    }
}

internal class StringAssignmentExpression : Expression {
    internal VariableExpression Left { get; }
    internal Expression Right { get; }

    internal StringAssignmentExpression(VariableExpression left, Expression right) {
        Left = left;
        Right = right;
    }

    internal override object Evaluate(VariableStore variableStore) {
        throw new NotImplementedException();
    }
}

internal class GroupExpression : Expression {
    private Expression Enclosed { get; }

    internal GroupExpression(Expression enclosed) { 
        Enclosed = enclosed; 
    }

    internal override object Evaluate(VariableStore variableStore) {
        return Enclosed.Evaluate(variableStore);
    }
}

internal class UnaryExpression : Expression {
    internal Token Operator { get; }
    internal Expression Right { get; }

    internal UnaryExpression(Token operatorToken, Expression right) {
        Operator = operatorToken;
        Right = right;
    }

    internal override object Evaluate(VariableStore variableStore) {
        throw new NotImplementedException();
    }
}

internal class BinaryExpression : Expression {
    internal Expression Left { get; }
    internal Token Operator { get; }
    internal Expression Right { get; }

    internal BinaryExpression(Expression left, Token operatorToken, Expression right) {
        Left = left;
        Operator = operatorToken;
        Right = right;
    }

    internal override object Evaluate(VariableStore variableStore) {
        Expression result =  Left switch {
            NumberExpression lvalue => Right switch {
                NumberExpression rvalue => Operator.Type switch {
                    TokenType.Plus => new NumberExpression(lvalue.Value + rvalue.Value),
                    TokenType.Minus => new NumberExpression(lvalue.Value - rvalue.Value),
                    TokenType.Asterisk => new NumberExpression(lvalue.Value * rvalue.Value),
                    TokenType.ForwardSlash => new NumberExpression(lvalue.Value / rvalue.Value),
                    TokenType.Equal => new BoolExpression(lvalue.Value == rvalue.Value),
                    TokenType.GreaterThan => new BoolExpression(lvalue.Value > rvalue.Value),
                    TokenType.GreaterThanOrEqual => new BoolExpression(lvalue.Value >= rvalue.Value),
                    TokenType.LessThan => new BoolExpression(lvalue.Value < rvalue.Value),
                    TokenType.LessThanOrEqual => new BoolExpression(lvalue.Value <= rvalue.Value),
                    _ => throw new Exception($"Invalid operator: {Operator.Lexeme}")
                },
                VariableExpression rvalue => Operator.Type switch {
                    TokenType.Plus => new NumberExpression(lvalue.Value + rvalue.ToDouble(variableStore)),
                    TokenType.Minus => new NumberExpression(lvalue.Value - rvalue.ToDouble(variableStore)),
                    TokenType.Asterisk => new NumberExpression(lvalue.Value * rvalue.ToDouble(variableStore)),
                    TokenType.ForwardSlash => new NumberExpression(lvalue.Value / rvalue.ToDouble(variableStore)),
                    TokenType.Equal => new BoolExpression(lvalue.Value == rvalue.ToDouble(variableStore)),
                    TokenType.GreaterThan => new BoolExpression(lvalue.Value > rvalue.ToDouble(variableStore)),
                    TokenType.GreaterThanOrEqual => new BoolExpression(lvalue.Value >= rvalue.ToDouble(variableStore)),
                    TokenType.LessThan => new BoolExpression(lvalue.Value < rvalue.ToDouble(variableStore)),
                    TokenType.LessThanOrEqual => new BoolExpression(lvalue.Value <= rvalue.ToDouble(variableStore)),
                    _ => throw new NotImplementedException()
                },
                _ => throw new Exception($"Invalid type: {Right}")
            },
            VariableExpression lvalue => Right switch {
                NumberExpression rvalue => Operator.Type switch {
                    TokenType.Plus => new NumberExpression(lvalue.ToDouble(variableStore) + rvalue.Value),
                    TokenType.Minus => new NumberExpression(lvalue.ToDouble(variableStore) - rvalue.Value),
                    TokenType.Asterisk => new NumberExpression(lvalue.ToDouble(variableStore) * rvalue.Value),
                    TokenType.ForwardSlash => new NumberExpression(lvalue.ToDouble(variableStore) / rvalue.Value),
                    TokenType.Equal => new BoolExpression(lvalue.ToDouble(variableStore) == rvalue.Value),
                    TokenType.GreaterThan => new BoolExpression(lvalue.ToDouble(variableStore) > rvalue.Value),
                    TokenType.GreaterThanOrEqual => new BoolExpression(lvalue.ToDouble(variableStore) >= rvalue.Value),
                    TokenType.LessThan => new BoolExpression(lvalue.ToDouble(variableStore) < rvalue.Value),
                    TokenType.LessThanOrEqual => new BoolExpression(lvalue.ToDouble(variableStore) <= rvalue.Value),
                    _ => throw new Exception($"Invalid operator: {Operator.Lexeme}")
                },
                VariableExpression rvalue => Operator.Type switch {
                    TokenType.Plus => new NumberExpression(lvalue.ToDouble(variableStore) + rvalue.ToDouble(variableStore)),
                    TokenType.Minus => new NumberExpression(lvalue.ToDouble(variableStore) - rvalue.ToDouble(variableStore)),
                    TokenType.Asterisk => new NumberExpression(lvalue.ToDouble(variableStore) * rvalue.ToDouble(variableStore)),
                    TokenType.ForwardSlash => new NumberExpression(lvalue.ToDouble(variableStore) / rvalue.ToDouble(variableStore)),
                    TokenType.Equal => new BoolExpression(lvalue.ToDouble(variableStore) == rvalue.ToDouble(variableStore)),
                    TokenType.GreaterThan => new BoolExpression(lvalue.ToDouble(variableStore) > rvalue.ToDouble(variableStore)),
                    TokenType.GreaterThanOrEqual => new BoolExpression(lvalue.ToDouble(variableStore) >= rvalue.ToDouble(variableStore)),
                    TokenType.LessThan => new BoolExpression(lvalue.ToDouble(variableStore) < rvalue.ToDouble(variableStore)),
                    TokenType.LessThanOrEqual => new BoolExpression(lvalue.ToDouble(variableStore) <= rvalue.ToDouble(variableStore)),
                    _ => throw new Exception($"Invalid operator: {Operator.Lexeme}")
                },
                _ => throw new Exception($"Invalid type: {Right}")
            },
            _ => throw new Exception($"Invalid type: {Left}")
        };

        return result.Evaluate(variableStore);
    }
}

