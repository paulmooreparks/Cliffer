using System.CommandLine;
using System.CommandLine.Invocation;

namespace Cliffer;

public interface IReplContext {
    Command RootCommand { get; }

    Command CurrentCommand { get; }

    string TitleMessage { get; }

    string EntryMessage { get; }

    string LoopMessage { get; }

    string[] ExitCommands { get; }

    string[] PopCommands { get; }

    string? RootNavCommand { get; }

    string? ParentNavCommand { get; }

    string[] HelpCommands { get; }

    void OnEntry();
    void OnLoop();

    string GetPrompt(System.CommandLine.Command command, InvocationContext context);
    string[] SplitCommandLine(string input);
    string[] PreprocessArgs(string[] args, Command command, InvocationContext context);
    Task<int> RunAsync(Command command, string[] args);
}
