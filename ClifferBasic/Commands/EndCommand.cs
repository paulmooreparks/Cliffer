using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;
using ClifferBasic.Services;

namespace ClifferBasic.Commands;
[Command("end", "End the currently running program")]
internal class EndCommand {
    public void Execute(ProgramService programService) {
        programService.Program.End();
        return;
    }
}
