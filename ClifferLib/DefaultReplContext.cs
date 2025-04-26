using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliffer;

public class DefaultReplContext : IReplContext {
    public DefaultReplContext() { }

    public DefaultReplContext(IReplContext parentContext, Command parentCommand) {
        ParentContext = parentContext;
        ParentCommand = parentCommand;
    }

    public virtual IReplContext? ParentContext { get; set; }
    public virtual Command? ParentCommand { get; set; }

    public virtual Command GetRootCommand() {
        IReplContext? ctx = this;

        while (ctx is DefaultReplContext dc && dc.ParentContext is not null) {
            ctx = dc.ParentContext;
        }

        return (ctx as DefaultReplContext)?.ParentCommand ?? throw new InvalidOperationException("No root command found.");
    }

    public virtual string GetTitleMessage() => string.Empty;

    public virtual string GetEntryMessage() {
        var titleMessage = GetTitleMessage();
        var exitCommands = string.Join(", ", GetExitCommands());
        var popCommands = string.Join(", ", GetPopCommands());
        var helpCommands = string.Join(", ", GetHelpCommands());

        var maxWidth = Math.Max(exitCommands.Length, Math.Max(popCommands.Length, helpCommands.Length));

        StringBuilder sb = new StringBuilder();
        
        if (!string.IsNullOrEmpty(titleMessage)) {
            sb.AppendLine(titleMessage);
        }

        if (exitCommands is not null && exitCommands.Any()) {
            sb.AppendLine($@"{exitCommands.PadRight(maxWidth)}  Exit the application");
        }

        if (popCommands is not null && popCommands.Any()) {
            sb.AppendLine($@"{popCommands.PadRight(maxWidth)}  Pop up one level in the command hierarchy");
        }

        if (helpCommands is not null && helpCommands.Any()) {
            sb.AppendLine($@"{helpCommands.PadRight(maxWidth)}  Show help and usage information");
        }

        var formattedOutput = sb.ToString();

        return formattedOutput;
    }

    public virtual void OnEntry() {
        var entryMessage = GetEntryMessage();

        if (!string.IsNullOrWhiteSpace(entryMessage)) {
            Console.WriteLine(entryMessage);
        }
    }

    public virtual string GetLoopMessage() => string.Empty;

    public virtual void OnLoop() {
        var loopMessage = GetLoopMessage();

        if (!string.IsNullOrWhiteSpace(loopMessage)) {
            Console.WriteLine(loopMessage);
        }
    }

    public virtual string[] GetExitCommands() => ["exit"];

    public virtual string[] GetPopCommands() => [];

    public virtual string[] GetHelpCommands() => ["help", "?", "-?", "--?", "--help"];

    public virtual string GetPrompt(Command command, InvocationContext context) => $"{command.Name}> ";

    public virtual string[] SplitCommandLine(string input) {
        return CommandLineStringSplitter.Instance.Split(input).ToArray();
    }

    public virtual string[] PreprocessArgs(string[] args, Command command, InvocationContext context) => args;

    public virtual async Task<int> RunAsync(Command currentCommand, string[] args) {
        if (args.Length == 0)
            return Result.Success;

        string firstArg = args[0];

        // Navigate to root command
        if (firstArg == "/") {
            var rootCommand = GetRootCommand();
            return await rootCommand.RunAsync(args.Skip(1).ToArray());
        }

        // Navigate to parent command
        if (firstArg == "..") {
            if (ParentCommand is not null)
                return await ParentCommand.RunAsync(args.Skip(1).ToArray());

            Console.Error.WriteLine("No parent context available.");
            return Result.ErrorInvalidArgument;
        }

        // `/command` or `/a/b/c`
        // TODO: need path normalisation
        if (firstArg.StartsWith('/')) {
            var root = GetRootCommand();
            Command? resolved = root;

            foreach (var part in firstArg.Trim('/').Split('/')) {
                resolved = resolved?.Children.OfType<Command>().FirstOrDefault(c => c.Name == part);
                if (resolved is null) {
                    Console.Error.WriteLine($"Invalid command path: {firstArg}");
                    return Result.ErrorInvalidArgument;
                }
            }

            return await resolved.RunAsync(args.Skip(1).ToArray());
        }

        // ..command or ../command
        if (firstArg.StartsWith("..")) {
            if (ParentCommand is not null) {
                string childName = firstArg.StartsWith("../")
                    ? firstArg[3..]
                    : firstArg[2..];

                var child = ParentCommand.Children
                    .OfType<Command>()
                    .FirstOrDefault(c => c.Name == childName);

                if (child is not null)
                    return await child.RunAsync(args.Skip(1).ToArray());

                Console.Error.WriteLine($"No such command '{childName}' in parent context.");
                return Result.ErrorInvalidArgument;
            }

            Console.Error.WriteLine("No parent context available.");
            return Result.ErrorInvalidArgument;
        }

        // Otherwise, use the current context normally
        return await currentCommand.RunAsync(args);
    }
}
