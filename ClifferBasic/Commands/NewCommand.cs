using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("new", "Clear the current program from memory")]
internal class NewCommand {
    public void Execute(ProgramService programService) {
        programService.New();
    }
}
