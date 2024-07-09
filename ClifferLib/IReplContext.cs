using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliffer;

public interface IReplContext {
    string GetTitleMessage();
    string GetEntryMessage();
    void OnEntry();
    string GetLoopMessage();
    void OnLoop();
    string GetPrompt(System.CommandLine.Command command, InvocationContext context);
    string[] GetExitCommands();
    string[] GetPopCommands();
    string[] GetHelpCommands();
    string[] PreprocessArgs(string[] args, Command command, InvocationContext context);
}
