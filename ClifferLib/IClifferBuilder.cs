using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cliffer;

public interface IClifferBuilder {
    IClifferBuilder ConfigureAppConfiguration();
    IClifferBuilder ConfigureAppConfiguration(Action<IConfigurationBuilder> configureDelegate);
    IClifferBuilder ConfigureServices();
    IClifferBuilder ConfigureServices(Action<IServiceCollection> configureServices);
    IClifferBuilder ConfigureCommands(Action<IConfiguration, RootCommand> configureCommands);
    IClifferBuilder ConfigureCommands(Action<IConfiguration, RootCommand, IServiceProvider> configureCommands);
    IClifferBuilder BuildCommands(IServiceProvider serviceProvider, Action<IConfiguration, RootCommand, IServiceProvider> buildCommands);
    IClifferCli Build(Action<IConfiguration, RootCommand, IServiceProvider>? buildCommands = null);
}
