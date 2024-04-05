using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliffer;

internal class DefaultReplContext : IReplContext {
    public string GetEntryMessage() {
        var exitCommands = string.Join(", ", GetExitCommands());
        var popCommands = string.Join(", ", GetPopCommands());
        var helpCommands = string.Join(", ", GetHelpCommands());

        var maxWidth = Math.Max(exitCommands.Length, Math.Max(popCommands.Length, helpCommands.Length));

        var formattedOutput = $@"Type a command or one of the following:
  {exitCommands.PadRight(maxWidth)}  Exit the application
  {popCommands.PadRight(maxWidth)}  Pop up one level in the command hierarchy
  {helpCommands.PadRight(maxWidth)}  Show help and usage information
";

        return formattedOutput;
    }

    public string GetLoopMessage() => string.Empty;

    public string[] GetExitCommands() => new string[] { "exit" };

    public string[] GetPopCommands() => new string[] { "quit" };

    public string[] GetHelpCommands() => new string[] { "help", "?" };

    public string GetPrompt(Command command, InvocationContext context) => $"{command.Name}> ";

    public string[] PreprocessArgs(string[] args, Command command, InvocationContext context) => args;
}
