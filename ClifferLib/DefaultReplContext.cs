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

public class DefaultReplContext() : IReplContext 
{
    public virtual string GetTitleMessage() => string.Empty;

    public virtual string GetEntryMessage() {
        var titleMessage = GetTitleMessage();
        var exitCommands = string.Join(", ", GetExitCommands());
        var rootNavCommand = GetRootNavCommand();
        var parentNavCommand = GetParentNavCommand();
        var helpCommands = string.Join(", ", GetHelpCommands());

        var maxWidth = Math.Max(exitCommands.Length, Math.Max(helpCommands.Length, Math.Max(rootNavCommand?.Length ?? 0, parentNavCommand?.Length ?? 0)));

        StringBuilder sb = new StringBuilder();
        
        if (!string.IsNullOrEmpty(titleMessage)) {
            sb.AppendLine(titleMessage);
        }

        if (exitCommands is not null && exitCommands.Any()) {
            sb.AppendLine($@"{exitCommands.PadRight(maxWidth)}  Exit the application");
        }

        if (!string.IsNullOrEmpty(rootNavCommand)) {
            sb.AppendLine($@"{rootNavCommand.PadRight(maxWidth)}  Exit to the root level in the command hierarchy");
        }

        if (!string.IsNullOrEmpty(parentNavCommand)) {
            sb.AppendLine($@"{parentNavCommand.PadRight(maxWidth)}  Exit to the parent level in the command hierarchy");
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

    public virtual string? GetRootNavCommand() => "/";

    public virtual string? GetParentNavCommand() => "..";

    public virtual string[] GetHelpCommands() => ["help", "?", "-?", "--?", "--help"];

    public virtual string GetPrompt(Command command, InvocationContext context) => $"{command.Name}> ";

    public virtual string[] SplitCommandLine(string input) {
        return CommandLineStringSplitter.Instance.Split(input).ToArray();
    }

    public virtual string[] PreprocessArgs(string[] args, Command command, InvocationContext context) => args;

    public virtual async Task<int> RunAsync(Command currentCommand, string[] args) {
        if (args.Length == 0)
            return Result.Success;

        var config = new CommandLineConfiguration(
            currentCommand,
            helpBuilderFactory: _ => new ReplAwareHelpBuilder(this)
        );

        var parser = new Parser(config);
        var result = parser.Parse(args);

        if (args.Contains("--help") || args.Contains("-h") || args.Contains("-?") || args.Contains("help")) {
            var commandToHelp = result.CommandResult.Command;

            var helpContext = new HelpContext(
                new ReplAwareHelpBuilder(this),
                commandToHelp,
                Console.Out
            );

            helpContext.HelpBuilder.Write(helpContext);
            return Result.Success;
        }

        return await parser.InvokeAsync(args);
    }
}
