using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliffer;

public static class CommandExtensions {
    public static async Task<int> RunAsync(this Command command, string[] args) {
        var macros = command.Children.OfType<Macro>().ToDictionary(macro => macro.Name, macro => macro);

        if (macros is not null) {
            args = Macro.PreprocessArgs(args, macros);
        }

        return await command.InvokeAsync(args);
    }

    public static async Task<int> Repl(this Command command, IServiceProvider serviceProvider, InvocationContext context, IReplContext? replContext = null) {
        if (replContext is null) {
            replContext = new DefaultReplContext();
        }

        var entryMessage = replContext.GetEntryMessage();

        if (!string.IsNullOrWhiteSpace(entryMessage)) {
            Console.WriteLine(entryMessage);
        }

        while (true) {
            var loopMessage = replContext.GetLoopMessage();
            if (!string.IsNullOrWhiteSpace(loopMessage)) {
                Console.WriteLine(loopMessage);
            }

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
                Environment.Exit(Result.Success);
            }

            var args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (args.Length > 0) {
                if (replContext.GetHelpCommands().Contains(args[0], StringComparer.OrdinalIgnoreCase)) {
                    args[0] = "--help";
                }

                args = replContext.PreprocessArgs(args, command, context);
            }

            await command.RunAsync(args);
        }
    }
}
