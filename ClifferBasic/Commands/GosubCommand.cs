using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("gosub", "Jump to a subroutine")]
[Argument(typeof(int), "lineNumber", "The line number where the subroutine begins")]
internal class GosubCommand {
    public void Execute(int lineNumber, ProgramService programService) {
        var success = programService.Gosub(lineNumber, out var programLine);

        if (!success) {
            Console.Error.WriteLine($"Line {lineNumber} not found.");
            var command = new EndCommand();
            command.Execute(programService);
        }
    }
}
