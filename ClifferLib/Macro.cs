using System.CommandLine;
using Microsoft.Extensions.Configuration;
using System.CommandLine.Parsing;

namespace Cliffer;

public delegate string[] MacroArgumentProcessor(string[] args);

public class Macro : System.CommandLine.Command {
    public MacroDefinition Definition { get; set; } = default!;
    public static MacroArgumentProcessor? CustomMacroArgumentProcessor { get; set; }

    public Macro(string name, string description, string script) : base(name, description) {
        Definition = new MacroDefinition { 
            Name = name, 
            Description = description, 
            Script = script 
        };
    }

    public Macro(MacroDefinition definition) : base(definition.Name, definition.Description) {
        Definition = definition;
    }

    public static async Task<int> PreprocessMacros(Command command, string[] args, Dictionary<string, Macro>? macros) {
        if (macros is null) {
            return await command.InvokeAsync(args);
        }

        int result = Result.Success;

        if (args.Any() && macros.TryGetValue(args[0], out var macro)) {
            var replacement = macro.Definition.Script;
            // var commandSplits = replacement.Split(';');
            var commandSplits  = CommandLineStringSplitter.Instance.Split(replacement).ToArray();

            foreach (var commandSplit in commandSplits) {
                var replacementParts = commandSplit.Split(' ');
                var newArgs = new List<string>();

                if (CustomMacroArgumentProcessor is not null) {
                    newArgs.AddRange(CustomMacroArgumentProcessor(replacementParts));
                }
                else {
                    var configuration = Utility.GetService<IConfiguration>();

                    foreach (var arg in replacementParts) {
                        var newArg = arg.ReplaceVariablePlaceholders(configuration!, args);
                        newArgs.AddRange(newArg.Split(' '));
                    }

                    if (result == Result.Success) {
                        var parseResult = command.Parse(newArgs.ToArray());
                        result = await parseResult.InvokeAsync();
                    }
                }
            }

            return result;
        }

        return await command.InvokeAsync(args);

#if false
        if (args.Any()) {
            var commandArgs = new List<string>();

            foreach (var arg in args) {
                switch (arg) {
                    case ";":
                        ExecuteMacro(commandArgs.ToArray(), macros);
                        break;

                    default:
                        commandArgs.Add(arg);
                        break;
                }
            }

            return ExecuteMacro(args, macros);
        }

        return args;
#endif
    }
}
