using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("delete", "Delete a line from the current program in memory", aliases:["del"])]
[Argument(typeof(int), "lineNumber", "The number of the line to delete")]
internal class DeleteCommand {
    public void Execute(int lineNumber, ProgramService programService) {
        if (programService.Program.HasLine(lineNumber)) {
            programService.Program.RemoveLine(lineNumber);
            return;
        }

        Console.Error.WriteLine($"Line {lineNumber} not found");
    }
}
