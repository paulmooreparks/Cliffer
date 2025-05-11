using System;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.IO;
using System.Text;

namespace Cliffer;

public class ReplAwareHelpBuilder : HelpBuilder {
    private readonly IReplContext? _replContext;

    public ReplAwareHelpBuilder(IReplContext? replContext = null, IConsole? console = null)
        : base(LocalizationResources.Instance, Console.WindowWidth) {
        _replContext = replContext;
    }

    public override void Write(HelpContext context) {
        base.Write(context);

        if (_replContext is null)
            return;

        var exitCommands = _replContext.ExitCommands;
        var popCommands = _replContext.PopCommands;
        var rootCommand = _replContext.RootNavCommand;
        var parentCommand = _replContext.ParentNavCommand;

        bool isAtRoot = string.IsNullOrWhiteSpace(parentCommand) && (popCommands.Length == 0);

        var helpSections = new List<(string Title, List<(string, string)> Entries)>();

        var exitSection = new List<(string, string)>();
        foreach (var exit in exitCommands) {
            exitSection.Add((exit, "Exit CLI"));
        }

        var navSection = new List<(string, string)>();
        if (!isAtRoot) {
            if (!string.IsNullOrWhiteSpace(rootCommand))
                navSection.Add((rootCommand, "Return to root"));

            if (!string.IsNullOrWhiteSpace(parentCommand))
                navSection.Add((parentCommand, "Return to parent"));

            foreach (var pop in popCommands) {
                navSection.Add((pop, "Pop up one level"));
            }
        }

        if (exitSection.Count > 0)
            helpSections.Add(("REPL Navigation Commands:", exitSection));

        if (navSection.Count > 0)
            helpSections.Add(("REPL Hierarchy Navigation:", navSection));

        foreach (var (title, entries) in helpSections) {
            context.Output.WriteLine();
            context.Output.WriteLine(title);

            int padding = entries.Max(e => e.Item1.Length) + 4;

            foreach (var (cmd, desc) in entries) {
                context.Output.WriteLine($"  {cmd.PadRight(padding)}{desc}");
            }
        }
    }
}
