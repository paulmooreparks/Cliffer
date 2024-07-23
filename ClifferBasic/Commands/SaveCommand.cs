using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;
using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("save", "Save a program to persistent storage")]
[Argument(typeof(string), "filename", "The name of the file to write to disk", arity: ArgumentArity.ExactlyOne)]
internal class SaveCommand {
    public int Execute(string filename, ProgramService programService) {
        programService.Save(filename);
        return Result.Success;
    }
}

