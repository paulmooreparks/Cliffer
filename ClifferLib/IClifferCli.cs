using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliffer;

public interface IClifferCli {
    Task<int> RunAsync(string[] args);
}
