using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace Cliffer;

public interface IClifferCli {
    IServiceProvider ServiceProvider { get; }
    Task<int> RunAsync(string[] args);
}
