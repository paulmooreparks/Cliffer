using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;
using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("load", "Load a program from persistent storage")]
[Argument(typeof(string), "filename", "The name of the file to load into memory")]
internal class LoadCommand {
    public int Execute(string filename, Dictionary<int, string[]> program, PersistenceService persistenceService) {
        Console.Error.WriteLine("Not implemented yet");
        var newProgram = persistenceService.Load();
        return Result.Success;
    }
}
