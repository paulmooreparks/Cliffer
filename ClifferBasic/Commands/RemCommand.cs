using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

namespace ClifferBasic.Commands;

[Command("rem", "Starts a comment line (remark)")]
[Argument(typeof(IEnumerable<string>), "args", "The comment line", Cliffer.ArgumentArity.ZeroOrMore)]
internal class RemCommand {
    public int Execute(IEnumerable<string> args) {
        return Result.Success;
    }
}
