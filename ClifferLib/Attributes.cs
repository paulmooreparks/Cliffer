using System;

namespace Cliffer;

public enum ArgumentArity {
    Zero,
    ZeroOrOne,
    ExactlyOne,
    ZeroOrMore,
    OneOrMore
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public abstract class SymbolAttribute : Attribute {
    public string Name { get; }
    public string Description { get; }
    public bool IsHidden { get; set; } = false;

    public string[]? Aliases { get; } = null;

    public SymbolAttribute(string name, string description, string[]? aliases = null) {
        Name = name;
        Description = description;
        Aliases = aliases;
    }
}

public sealed class RootCommandAttribute : SymbolAttribute {
    public string HandlerMethodName { get; set; } = "Execute";

    public RootCommandAttribute(string description) : base(string.Empty, description, null) {
    }
}

public sealed class CommandAttribute : SymbolAttribute {
    public string HandlerMethodName { get; set; } = "Execute";
    public string? Parent { get; set; } = null;

    public CommandAttribute(string name, string description, string[]? aliases = null) : base(name, description, aliases) {
    }
}

public sealed class OptionAttribute : SymbolAttribute {
    public Type Type { get; }
    public ArgumentArity Arity { get; set; } = ArgumentArity.ZeroOrOne;
    public bool IsRequired { get; set; } = false;
    public bool AllowMultipleArgumentsPerToken { get; set; } = false;
    public string DefaultValueMethodName { get; set; } = string.Empty;
    public string[] FromAmong { get; set; } = Array.Empty<string>();

    public OptionAttribute(Type type, string name, string description) : base(name, description, null) {
        Type = type;
    }
    public OptionAttribute(Type type, string name, string description, string[]? aliases) : base(name, description, aliases) {
        Type = type;
    }
}

public sealed class ArgumentAttribute : SymbolAttribute {
    public Type Type { get; }
    public ArgumentArity Arity { get; set; } = ArgumentArity.ZeroOrOne;
    public string DefaultValueMethodName { get; set; } = string.Empty;

    public ArgumentAttribute(Type type, string name, string description) : this(type, name, description, ArgumentArity.ZeroOrOne) {
    }

    public ArgumentAttribute(Type type, string name, string description, ArgumentArity arity) : base(name, description) {
        Type = type;
        Arity = arity;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ServiceAttribute : Attribute {
    public Type ServiceType { get; }

    public ServiceAttribute(Type serviceType) {
        ServiceType = serviceType;
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class CommandParamAttribute : Attribute {
    public string Name { get; }

    public CommandParamAttribute(string name) {
        Name = name;
    }
}

// Attribute to indicate a parameter is bound to a command-line option
[AttributeUsage(AttributeTargets.Parameter)]
public class OptionParamAttribute : Attribute {
    public string Name { get; }

    public OptionParamAttribute(string name) {
        Name = name;
    }
}

// Attribute to indicate a parameter is bound to a command-line argument
[AttributeUsage(AttributeTargets.Parameter)]
public class ArgumentParamAttribute : Attribute {
    public string Name { get; }

    public ArgumentParamAttribute(string name) {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MacroAttribute : Attribute {
    public string Name { get; }
    public string Description { get; }

    public MacroAttribute(string name, string description) {
        Name = name;
        Description = description;
    }
}
