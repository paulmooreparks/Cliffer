using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cliffer;

public interface IClifferBuilder {
    IClifferBuilder ConfigureAppConfiguration();
    IClifferBuilder ConfigureAppConfiguration(Action<IConfigurationBuilder> configureDelegate);
    IClifferBuilder ConfigureServices();
    IClifferBuilder ConfigureServices(Action<IServiceCollection> configureServices);
    IClifferBuilder ConfigureServices<TContext>(Action<TContext, IServiceCollection> configureServices, TContext context);
    IClifferBuilder ConfigureCommands(Action<IConfiguration, RootCommand> configureCommands);
    IClifferBuilder ConfigureCommands(Action<IConfiguration, RootCommand, IServiceProvider> configureCommands);
    IClifferBuilder BuildCommands(IServiceProvider serviceProvider, Action<IConfiguration, RootCommand, IServiceProvider> buildCommands);
    IConfiguration BuildConfiguration();
    IClifferCli Build(Action<IConfiguration, RootCommand, IServiceProvider>? buildCommands = null);
    IClifferBuilder AddCommands(System.Reflection.Assembly assembly);
}
