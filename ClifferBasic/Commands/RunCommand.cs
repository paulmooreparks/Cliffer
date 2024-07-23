using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;
using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("run", "Run the program currently in memory, or optionally load a program from storage and run it")]
[Argument(typeof(string), "filename", "The name of the file to load into memory", arity: Cliffer.ArgumentArity.ZeroOrOne)]
internal class RunCommand {
    private List<string> _illegalCommands = new List<string>() { 
        "exit", 
        "list", 
        "load", 
        "new", 
        "renumber", 
        "run", 
        "save" 
    };

    public RunCommand() { 
        _illegalCommands.Sort();
    }

    public async Task<int> Execute(string filename, CommandSplitter splitter, InvocationContext context, ProgramService programService) {

        if (!string.IsNullOrEmpty(filename)) {
            programService.Load(filename);
        }

        foreach (var line in programService.Program.Listing) {
            var tokens = splitter.Split(line).ToArray();
            
            var parseResult = context.Parser.Parse(tokens.Skip(1).ToArray());

            if (parseResult != null) {
                var commandName = parseResult.CommandResult.Command.Name;

                if (string.Equals("end", commandName)) {
                    return Result.Success;
                }

                if (_illegalCommands.Contains(commandName)) {
                    Console.Error.WriteLine($"Illegal command: {commandName}");
                    return Result.Error;
                }

                var result = await parseResult.InvokeAsync();
            }
            else {
                Console.Error.WriteLine($"Invalid command: {line}");
                return Result.Error;
            }
        }

        return Result.Success;
    }
}
