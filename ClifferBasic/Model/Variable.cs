using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClifferBasic.Model;

internal class Variable {
    internal Type Type { get; }

    internal Variable(Type type) {
        Type = type;
    }
}

internal class IntegerVariable : Variable {
    internal int Value { get; set; }

    internal IntegerVariable(int value) : base(typeof(int)) {
        Value = value;
    }

    internal IntegerVariable(object value) : base(typeof(int)) {
        Value = Convert.ToInt32(value);
    }

    public override string? ToString() {
        return Value.ToString();
    }
}

internal class DoubleVariable : Variable {
    internal double Value { get; set; }

    internal DoubleVariable(double value) : base(typeof(double)) {
        Value = value;
    }

    internal DoubleVariable(object value) : base(typeof(double)) {
        Value = Convert.ToDouble(value);
    }

    public override string? ToString() {
        return Value.ToString();
    }
}

internal class StringVariable : Variable {
    internal string Value { get; set; }

    internal StringVariable(string value) : base(typeof(string)) {
        Value = value;
    }

    internal StringVariable(object value) : base(typeof(string)) {
        Value = value?.ToString() ?? string.Empty;
    }

    public override string? ToString() {
        return Value;
    }
}

