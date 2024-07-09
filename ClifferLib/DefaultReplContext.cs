using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliffer;

public class DefaultReplContext : IReplContext {
    public virtual string GetTitleMessage() => string.Empty;

    public virtual string GetEntryMessage() {
        var titleMessage = GetTitleMessage();
        var exitCommands = string.Join(", ", GetExitCommands());
        var popCommands = string.Join(", ", GetPopCommands());
        var helpCommands = string.Join(", ", GetHelpCommands());

        var maxWidth = Math.Max(exitCommands.Length, Math.Max(popCommands.Length, helpCommands.Length));

        var formattedOutput = $@"{titleMessage}
Type a command or one of the following:
  {exitCommands.PadRight(maxWidth)}  Exit the application
  {popCommands.PadRight(maxWidth)}  Pop up one level in the command hierarchy
  {helpCommands.PadRight(maxWidth)}  Show help and usage information
";

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

    public virtual string[] GetExitCommands() => new string[] { "exit" };

    public virtual string[] GetPopCommands() => new string[] { "quit" };

    public virtual string[] GetHelpCommands() => new string[] { "help", "?" };

    public virtual string GetPrompt(Command command, InvocationContext context) => $"{command.Name}> ";

    public virtual string[] PreprocessArgs(string[] args, Command command, InvocationContext context) => args;
}
