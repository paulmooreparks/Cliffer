using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

namespace ClifferBasic.Commands;

[Command("list", "List the current program in memory")]
internal class ListCommand {
    public int Execute(Dictionary<int, string[]> program) {
        var lineNumbers = program.Keys.OrderBy(x => x).ToList();

        foreach (var lineNumber in lineNumbers) {
            string line = string.Join(" ", program[lineNumber]);
            Console.WriteLine($"{lineNumber} {line}");
        }

        return Result.Success;
    }
}
