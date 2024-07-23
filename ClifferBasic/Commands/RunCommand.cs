using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

namespace ClifferBasic.Commands;

[Command("run", "Run the program currently in memory")]
internal class RunCommand {
    public int Execute(Dictionary<int, string[]> program) {
        Console.Error.WriteLine("Not implemented yet");
        return Result.Success;
    }
}
