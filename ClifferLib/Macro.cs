using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Cliffer;

public class Macro : System.CommandLine.Command {
    public string MacroDefinition { get; set; } = default!;

    public Macro(string name, string description, string macroDefinition) : base(name, description) {
        MacroDefinition = macroDefinition;
    }

    public static string[] PreprocessArgs(string[] args, Dictionary<string, Macro>? macros) {
        if (macros is null) {
            return args;
        }

        if (args.Length > 0 && macros.TryGetValue(args[0], out var macro)) {
            var replacement = macro.MacroDefinition;
            var replacementParts = replacement.Split(' ');
            var newArgs = new List<string>(replacementParts);

            if (args.Length > 1) {
                newArgs.AddRange(args.Skip(1));
            }

            return newArgs.ToArray();
        }

        // Return the original args if no macro matches
        return args;
    }
}
