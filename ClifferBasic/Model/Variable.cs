using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClifferBasic.Model;

internal class Variable {
    internal object Value { get; }
    internal Type Type { get; }

    internal Variable(object value, Type type) {
        Value = value;
        Type = type;
    }

    internal double AsDouble() {
        if (Type == typeof(double) || Type == typeof(float) || Type == typeof(int) || Type == typeof(long)) {
            return Convert.ToDouble(Value);
        }

        throw new InvalidOperationException($"Cannot convert type {Type} to double.");
    }

    internal int AsInteger() {
        if (Type == typeof(double) || Type == typeof(float) || Type == typeof(int) || Type == typeof(long)) {
            return Convert.ToInt32(Value);
        }

        throw new InvalidOperationException($"Cannot convert type {Type} to integer.");
    }

    internal string AsString() {
        if (Value is null || string.IsNullOrEmpty(Value.ToString())) {
            return string.Empty;
        }

        if (Type == typeof(double) || Type == typeof(float) || Type == typeof(int) || Type == typeof(long)) {
            return Value.ToString()!;
        }

        throw new InvalidOperationException($"Cannot convert type {Type} to string.");
    }

    public override string? ToString() {
        return Value.ToString();
    }
}

internal class NumericVariable : Variable {
    internal NumericVariable(double value, Type type) : base(value, type) {
    }
}
