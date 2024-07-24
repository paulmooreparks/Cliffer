using Cliffer;
using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("list", "List the current program in memory")]
internal class ListCommand {
    public int Execute(ProgramService programService) {
        if (programService == null) {
            return Result.Error;
        }

        programService.Reset();

        while (programService.Next(out var programLine)) {
            Console.WriteLine(programLine);
        }

        return Result.Success;
    }
}
