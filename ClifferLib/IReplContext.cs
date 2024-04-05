using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliffer;

public interface IReplContext {
    string GetEntryMessage();
    string GetLoopMessage();
    string GetPrompt(System.CommandLine.Command command, InvocationContext context);
    string[] GetExitCommands();
    string[] GetPopCommands();
    string[] GetHelpCommands();
    string[] PreprocessArgs(string[] args, Command command, InvocationContext context);
}
