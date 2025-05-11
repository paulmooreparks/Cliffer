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

    public static async Task<int> Repl(this Command command, IServiceProvider serviceProvider, InvocationContext invContext, IReplContext? replContext = null) {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(invContext);

        replContext ??= new DefaultReplContext(command);
        replContext.OnEntry();

        while (true) {
            try {
                replContext.OnLoop();
                Console.Write($"{replContext.GetPrompt(command, invContext)}");

                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) {
                    continue;
                }

                input = input.Trim();

                if (replContext.ExitCommands.Contains(input, StringComparer.OrdinalIgnoreCase)) {
                    ClifferEventHandler.Exit(Result.Success);
                }

                var rootPrefix = replContext.RootNavCommand ?? "/";
                var parentPrefix = replContext.ParentNavCommand ?? "..";

                if (string.Equals(rootPrefix, input) ||
                    string.Equals(parentPrefix, input) ||
                    replContext.PopCommands.Contains(input, StringComparer.OrdinalIgnoreCase)) {
                    return Result.Success;
                }

                var inputArgs = replContext.SplitCommandLine(input);
                if (inputArgs.Length == 0) {
                    continue;
                }

                // Absolute path (/foo/bar)
                // TODO: See comments below on ".." handler.
                if (inputArgs[0].StartsWith(rootPrefix)) {
                    var trimmed = inputArgs[0].Substring(rootPrefix.Length);
                    var args = new[] { trimmed }.Concat(inputArgs.Skip(1)).ToArray();
                    var parseResult = replContext.RootCommand.Parse(args);
                    if (!parseResult.Errors.Any()) {
                        await parseResult.InvokeAsync();
                    }
                    else {
                        foreach (var error in parseResult.Errors) {
                            Console.Error.WriteLine(error.Message);
                        }
                    }
                    continue;
                }

                // Relative path (..command, ../foo, ../../bar/baz)
                // TODO: Not sure I'll keep this around, unless I go to deeply-nested folders. 
                // Right now, it behaves mostly like '/' above.
                if (inputArgs[0].StartsWith(parentPrefix)) {
                    var baseArg = inputArgs[0];
                    var remainingArgs = inputArgs.Skip(1).ToArray();
                    var commandParts = new List<string>();
                    var levelUps = 0;

                    if (baseArg.StartsWith(parentPrefix + "/")) {
                        var parts = baseArg.Split('/');
                        levelUps = parts.TakeWhile(p => p == parentPrefix).Count();
                        commandParts.AddRange(parts.Skip(levelUps));
                    }
                    else if (baseArg.StartsWith(parentPrefix) && baseArg.Length > parentPrefix.Length) {
                        levelUps = 1;
                        commandParts.Add(baseArg.Substring(parentPrefix.Length));
                    }
                    else if (baseArg == parentPrefix) {
                        return Result.Success;
                    }

                    var target = replContext.CurrentCommand;
                    for (int i = 0; i < levelUps; i++) {
                        target = target.Parents.FirstOrDefault() as Command ?? target;
                    }

                    var args = commandParts.Concat(remainingArgs).ToArray();
                    var parseResult = target.Parse(args);
                    if (!parseResult.Errors.Any()) {
                        await parseResult.InvokeAsync();
                    }
                    else {
                        foreach (var error in parseResult.Errors) {
                            Console.Error.WriteLine(error.Message);
                        }
                    }
                    continue;
                }

                // Try running from root if not found in current scope
                if (replContext.HelpCommands.Contains(inputArgs[0], StringComparer.OrdinalIgnoreCase)) {
                    inputArgs[0] = "--help";
                }

                var preprocessedArgs = replContext.PreprocessArgs(inputArgs, command, invContext);
                var parseCurrent = command.Parse(preprocessedArgs);

                if (!parseCurrent.Errors.Any()) {
                    _ = await replContext.RunAsync(command, preprocessedArgs);
                }
                else {
                    var parseRoot = replContext.RootCommand.Parse(preprocessedArgs);
                    if (!parseRoot.Errors.Any()) {
                        _ = await parseRoot.InvokeAsync();
                    }
                    else {
                        foreach (var error in parseRoot.Errors) {
                            Console.Error.WriteLine(error.Message);
                        }
                    }
                }
            }
            catch (Exception ex) {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}
