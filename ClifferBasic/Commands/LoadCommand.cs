using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;
using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("load", "Load a program from persistent storage")]
[Argument(typeof(string), "filename", "The name of the file to load into memory", arity: ArgumentArity.ExactlyOne)]
internal class LoadCommand {
    public int Execute(string filename, ProgramService programService) {
        var newProgram = programService.Load(filename);
        return Result.Success;
    }
}
