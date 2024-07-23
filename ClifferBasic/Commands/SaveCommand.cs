using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;
using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("save", "Save a program to persistent storage")]
[Argument(typeof(string), "filename", "The name of the file to write to disk")]
internal class SaveCommand {
    public int Execute(Dictionary<int, string[]> program, PersistenceService persistenceService) {
        Console.Error.WriteLine("Not implemented yet");
        persistenceService.Save(program);
        return Result.Success;
    }
}

