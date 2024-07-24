using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("return", "Return from a subroutine")]
internal class ReturnCommand {
    public void Execute(ProgramService programService) {
        var success = programService.Return(out var programLine);

        if (!success) {
            Console.Error.WriteLine($"Could not return from subroutine");
            var command = new EndCommand();
            command.Execute(programService);
        }
    }
}
