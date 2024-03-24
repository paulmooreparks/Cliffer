using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cliffer;

public interface IClifferBuilder {
    IClifferBuilder ConfigureAppConfiguration(Action<ClifferBuilderContext, IConfigurationBuilder> configureDelegate);
    IClifferBuilder ConfigureServices(Action<IServiceCollection> configureServices);
    IClifferBuilder BuildCommands(Action<IConfiguration> buildCommands);
    IClifferBuilder ConfigureCommands(Action<IConfiguration> configureCommands);
    IClifferCli Build();
}
