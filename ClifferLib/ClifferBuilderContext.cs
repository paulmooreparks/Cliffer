using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace Cliffer;

public class ClifferBuilderContext {
    public IConfiguration Configuration { get; set; } = default!;
}
