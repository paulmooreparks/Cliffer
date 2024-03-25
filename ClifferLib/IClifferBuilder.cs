using System;
using System.CommandLine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cliffer;

public interface IClifferBuilder {
    IClifferBuilder ConfigureAppConfiguration(Action<ClifferBuilderContext, IConfigurationBuilder> configureDelegate);
    IClifferBuilder ConfigureServices(Action<IServiceCollection> configureServices);
    IClifferBuilder BuildCommands(Action<IConfiguration, RootCommand> buildCommands);
    IClifferBuilder BuildCommands(Action<IConfiguration, RootCommand, IServiceProvider> buildCommands);
    IClifferBuilder ConfigureCommands(Action<IConfiguration, RootCommand> configureCommands);
    IClifferBuilder ConfigureCommands(Action<IConfiguration, RootCommand, IServiceProvider> configureCommands);
    IClifferCli Build();
}
