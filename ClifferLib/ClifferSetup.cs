using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cliffer; 

public static class ClifferSetup {
    public static IServiceCollection AddClifferServices(this IServiceCollection services, IConfiguration configuration) {
        // services.AddSingleton<IConfiguration>(configuration);
        // services.AddSingleton<IClifferBuilder>(new ClifferBuilder());
        return services;
    }
}
