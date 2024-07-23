using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("list", "List the current program in memory")]
internal class ListCommand {
    public int Execute(ProgramService programService) {
        if (programService == null) {
            return Result.Error;
        }

        foreach (var line in programService.Program.Listing) {
            Console.WriteLine(line);
        }

        return Result.Success;
    }
}
