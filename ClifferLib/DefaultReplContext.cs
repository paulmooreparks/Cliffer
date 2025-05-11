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

public class DefaultReplContext(Command currentCommand) : IReplContext 
{
    public virtual Command RootCommand => GetRoot(CurrentCommand);
    public virtual Command CurrentCommand => currentCommand;

    private static Command GetRoot(Command command) {
        return command.Parents.LastOrDefault() as Command ?? command;
    }

    public virtual string TitleMessage => string.Empty;

    public virtual string EntryMessage {
        get {
            var titleMessage = TitleMessage;
            var exitCommands = string.Join(", ", ExitCommands);
            var rootNavCommand = RootNavCommand;
            var parentNavCommand = ParentNavCommand;
            var helpCommands = string.Join(", ", HelpCommands);

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
    }

    public virtual void OnEntry() {
        var entryMessage = EntryMessage;

        if (!string.IsNullOrWhiteSpace(entryMessage)) {
            Console.WriteLine(entryMessage);
        }
    }

    public virtual string LoopMessage => string.Empty;

    public virtual void OnLoop() {
        var loopMessage = LoopMessage;

        if (!string.IsNullOrWhiteSpace(loopMessage)) {
            Console.WriteLine(loopMessage);
        }
    }

    public virtual string[] ExitCommands => ["exit"];

    public virtual string[] PopCommands => [];

    public virtual string? RootNavCommand => "/";

    public virtual string? ParentNavCommand => "..";

    public virtual string[] HelpCommands => ["?", "-?", "--?", "help", "--help", "-h"];

    public virtual string GetPrompt(Command command, InvocationContext context) => $"{command.Name}> ";

    public virtual string[] SplitCommandLine(string input) {
        return CommandLineStringSplitter.Instance.Split(input).ToArray();
    }

    public virtual string[] PreprocessArgs(string[] args, Command command, InvocationContext context) => args;

    public virtual async Task<int> RunAsync(Command currentCommand, string[] args) {
        if (args.Length == 0) {
            return Result.Success;
        }

        var config = new CommandLineConfiguration(
            currentCommand,
            helpBuilderFactory: _ => new ReplAwareHelpBuilder(this)
        );

        var parser = new Parser(config);
        var result = parser.Parse(args);

        if (result.Errors.Count > 0) {
            foreach (var error in result.Errors) {
                Console.Error.WriteLine(error.Message);
            }

            Console.Error.WriteLine();

            DisplayHelp(result);
            return Result.Error;
        }

        if (args.Any(arg => HelpCommands.Contains(arg, StringComparer.OrdinalIgnoreCase))) {
            var commandToHelp = result.CommandResult.Command;
            DisplayHelp(result);
            return Result.Success;
        }

        return await parser.InvokeAsync(args);
    }
    private void DisplayHelp(ParseResult result) {
        var helpContext = new HelpContext(
            new ReplAwareHelpBuilder(this),
            result.CommandResult.Command,
            Console.Out
        );

        helpContext.HelpBuilder.Write(helpContext);
        Console.Error.WriteLine();
    }
}
