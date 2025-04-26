using System.CommandLine;
using System.CommandLine.Invocation;

namespace Cliffer;

public interface IReplContext {
    Command GetRootCommand();
    string GetTitleMessage();
    string GetEntryMessage();
    void OnEntry();
    string GetLoopMessage();
    void OnLoop();
    string GetPrompt(System.CommandLine.Command command, InvocationContext context);
    string[] GetExitCommands();
    string[] GetPopCommands();
    string[] GetHelpCommands();
    string[] SplitCommandLine(string input);
    string[] PreprocessArgs(string[] args, Command command, InvocationContext context);
    Task<int> RunAsync(Command command, string[] args);
}
