using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine.Parsing;

namespace Cliffer;

public static class CommandExtensions {
    public static async Task<int> RunAsync(this Command command, string[] args) {
        var macros = command.Children.OfType<Macro>().ToDictionary(macro => macro.Name, macro => macro);

        if (macros.Any()) {
            return await Macro.PreprocessMacros(command, args, macros);
        }
        
        ClifferEventHandler.PreprocessArgs(args);
        return await command.InvokeAsync(args);
    }

    public static async Task<int> Repl(this Command command, IServiceProvider serviceProvider, InvocationContext context, IReplContext? replContext = null) {
        if (replContext is null) {
            replContext = new DefaultReplContext();
        }

        replContext.OnEntry();

        while (true) {
            try {
                replContext.OnLoop();
                Console.Write($"{replContext.GetPrompt(command, context)}");

                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) {
                    continue;
                }

                input = input.Trim();

                if (replContext.GetPopCommands().Contains(input, StringComparer.OrdinalIgnoreCase)) {
                    return Result.Success;
                }

                if (replContext.GetExitCommands().Contains(input, StringComparer.OrdinalIgnoreCase)) {
                    ClifferEventHandler.Exit(Result.Success);
                }

                var args = replContext.SplitCommandLine(input);

                if (args.Length > 0) {
                    if (replContext.GetHelpCommands().Contains(args[0], StringComparer.OrdinalIgnoreCase)) {
                        args[0] = "--help";
                    }

                    args = replContext.PreprocessArgs(args, command, context);
                }

                await replContext.RunAsync(command, args);
            }
            catch (Exception ex) {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}
