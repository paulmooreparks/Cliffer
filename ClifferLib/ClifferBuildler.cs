using System.Reflection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cliffer;
public class ClifferBuilder : IClifferBuilder {
    public IServiceCollection Services = new ServiceCollection();

    internal readonly ClifferBuilderContext Context = new ClifferBuilderContext();
    internal readonly IConfigurationBuilder ConfigurationBuilder = new ConfigurationBuilder();

    internal IServiceProvider ServiceProvider => Services.BuildServiceProvider();

    internal ClifferBuilder() {
        Context.Configuration = ConfigurationBuilder.Build();
        Services.AddSingleton(Context);
        Services.AddSingleton(Context.Configuration);
        Services.AddSingleton(this);
        Services.AddSingleton<IServiceProvider>(ServiceProvider);
    }

    public IClifferBuilder ConfigureAppConfiguration(Action<ClifferBuilderContext, IConfigurationBuilder> configureDelegate) {
        configureDelegate(Context, ConfigurationBuilder);
        return this;
    }

    public IClifferBuilder ConfigureServices(Action<IServiceCollection> configureServices) {
        configureServices(Services);
        return this;
    }

    internal IClifferBuilder BuildCommands() {
        var entryAssembly = Assembly.GetEntryAssembly();

        if (entryAssembly == null) {
            throw new ApplicationException("No entry assembly found");
        }

        var rootCommandTypes = entryAssembly.GetTypes().Where(t => t.GetCustomAttribute<Cliffer.RootCommandAttribute>() != null);
        var rootCommandType = rootCommandTypes.FirstOrDefault();

        if (rootCommandType == null) {
            throw new ApplicationException("No root command found");
        }

        var rootCommandAttribute = rootCommandType.GetCustomAttribute<Cliffer.RootCommandAttribute>();

        if (rootCommandAttribute is null) {
            throw new ApplicationException("No root command attribute found");
        }

        var rootHandlerMethod = rootCommandType.GetMethod(rootCommandAttribute.HandlerMethodName, BindingFlags.Public | BindingFlags.Instance);

        if (rootHandlerMethod == null) {
            throw new ApplicationException("No root handler method found");
        }

        var rootConstructorInfo = rootCommandType.GetConstructors().FirstOrDefault();

        if (rootConstructorInfo == null) {
            throw new ApplicationException($"No constructor found for type {rootCommandType.FullName}");
        }

        // Prepare an array to hold the parameter values for the constructor
        var rootConstructorParams = rootConstructorInfo.GetParameters();
        var rootConstructorParamValues = new object[rootConstructorParams.Length];

        // Fill the array with instances obtained from the service container
        for (int i = 0; i < rootConstructorParams.Length; i++) {
            var paramType = rootConstructorParams[i].ParameterType;
            var serviceInstance = ServiceProvider.GetService(paramType);
            rootConstructorParamValues[i] = serviceInstance ?? throw new InvalidOperationException($"Service for type {paramType.FullName} not found");
        }

        var rootCommand = new RootCommand();

        if (!string.IsNullOrEmpty(rootCommandAttribute.Name)) {
            rootCommand.Name = rootCommandAttribute.Name;
        }

        rootCommand.Description = rootCommandAttribute.Description;
        rootCommand.IsHidden = rootCommandAttribute.IsHidden;

        {
            var aliases = rootCommandAttribute.Aliases;

            if (aliases != null) {
                foreach (var alias in aliases) {
                    rootCommand.AddAlias(alias);
                }
            }
        }

        Services.AddSingleton(rootCommand);

        // Create an instance of the command using the constructor with parameters
        var rootCommandInstance = rootConstructorInfo.Invoke(rootConstructorParamValues);

        if (rootCommandInstance is not null) {
            AttachDynamicHandler(rootCommandType, rootCommand, rootCommandInstance!, rootHandlerMethod);

            var configureMethod = rootCommandType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Instance);

            if (configureMethod != null) {
                var configureMethodParams = configureMethod.GetParameters();
                var configureParamValues = new object[configureMethodParams.Length];

                for (int i = 0; i < configureParamValues.Length; i++) {
                    var paramType = configureMethodParams[i].ParameterType;
                    var serviceInstance = ServiceProvider.GetService(paramType);
                    configureParamValues[i] = serviceInstance ?? throw new InvalidOperationException($"Service for type {paramType.FullName} not found");
                }

                configureMethod.Invoke(rootCommandInstance, configureParamValues);
            }
        }

        /* TODO: This can be optimised. if the CLI is not starting in interactive mode and is not displaying the help text, 
        then all of the commands do not need to be processed. That could improve startup time in a non-interactive environment. */

        // Use reflection to find all ICommandConfiguration classes annotated with CommandAttribute
        var commandTypes = entryAssembly.GetTypes()
            .Where(t => t.GetCustomAttribute<CommandAttribute>() != null);

        var commands = new SortedDictionary<string, System.CommandLine.Command>();
        var commandRelations = new List<(string Child, string Parent)>();

        foreach (var type in commandTypes) {
            var commandAttribute = type.GetCustomAttribute<Cliffer.CommandAttribute>();

            if (commandAttribute is not null) {
                var command = new Command(commandAttribute.Name, commandAttribute.Description);
                commands.Add(commandAttribute.Name, command);

                if (!string.IsNullOrEmpty(commandAttribute.Parent)) {
                    commandRelations.Add((commandAttribute.Name, commandAttribute.Parent));
                }

                // Find the constructor with parameters (if any)
                var constructorInfo = type.GetConstructors().FirstOrDefault();
                if (constructorInfo == null) {
                    throw new InvalidOperationException($"No constructor found for type {type.FullName}");
                }

                // Prepare an array to hold the parameter values for the constructor
                var constructorParams = constructorInfo.GetParameters();
                var constructorParamValues = new object[constructorParams.Length];

                // Fill the array with instances obtained from the service container
                for (int i = 0; i < constructorParams.Length; i++) {
                    var paramType = constructorParams[i].ParameterType;

                    if (paramType == typeof(IServiceProvider)) {
                        rootConstructorParamValues[i] = ServiceProvider;
                        continue;
                    }

                    var serviceInstance = ServiceProvider.GetService(paramType);
                    constructorParamValues[i] = serviceInstance ?? throw new InvalidOperationException($"Service for type {paramType.FullName} not found");
                }

                // Create an instance of the command using the constructor with parameters
                var commandInstance = constructorInfo.Invoke(constructorParamValues);

                if (commandInstance is not null) {
                    command.IsHidden = commandAttribute.IsHidden;

                    {
                        var aliases = commandAttribute.Aliases;

                        if (aliases != null) {
                            foreach (var alias in aliases) {
                                command.AddAlias(alias);
                            }
                        }
                    }

                    var argumentAttributes = type.GetCustomAttributes<ArgumentAttribute>();

                    if (argumentAttributes is not null) {
                        foreach (var attr in argumentAttributes) {
                            var argumentType = typeof(Argument<>).MakeGenericType(attr.Type);
                            var constructor = argumentType.GetConstructor(new[] { typeof(string), typeof(string) });

                            if (constructor != null) {
                                var argument = (Argument)constructor.Invoke(new object?[] { attr.Name, attr.Description });

                                System.CommandLine.ArgumentArity arity = attr.Arity switch {
                                    ArgumentArity.Zero => System.CommandLine.ArgumentArity.Zero,
                                    ArgumentArity.ZeroOrOne => System.CommandLine.ArgumentArity.ZeroOrOne,
                                    ArgumentArity.ExactlyOne => System.CommandLine.ArgumentArity.ExactlyOne,
                                    ArgumentArity.ZeroOrMore => System.CommandLine.ArgumentArity.ZeroOrMore,
                                    ArgumentArity.OneOrMore => System.CommandLine.ArgumentArity.OneOrMore,
                                    _ => System.CommandLine.ArgumentArity.ZeroOrOne
                                };

                                argument.Arity = arity;
                                argument.IsHidden = attr.IsHidden;

                                var defaultValueMethod = type.GetMethod(attr.DefaultValueMethodName, BindingFlags.NonPublic | BindingFlags.Instance);

                                if (defaultValueMethod != null) {
                                    argument.SetDefaultValueFactory(() => {
                                        var handlerParams = defaultValueMethod.GetParameters();
                                        var parameterValues = new object[handlerParams.Length];

                                        for (int i = 0; i < handlerParams.Length; i++) {
                                            var param = handlerParams[i];
                                            object? value = null;
                                            value = ServiceProvider.GetService(param.ParameterType);

                                            // Assign the resolved value to the parameterValues array
                                            if (value != null) {
                                                parameterValues[i] = value;
                                            }
                                        }

                                        return defaultValueMethod.Invoke(commandInstance, parameterValues);
                                    });
                                }

                                command.AddArgument(argument);
                            }
                        }
                    }

                    var optionAttributes = type.GetCustomAttributes<OptionAttribute>();

                    if (optionAttributes is not null) {
                        foreach (var attr in optionAttributes) {
                            var optionType = typeof(Option<>).MakeGenericType(attr.Type);
                            var constructor = optionType.GetConstructor(new[] { typeof(string), typeof(string) });

                            if (constructor != null) {
                                var option = (Option)constructor.Invoke(new object?[] { attr.Name, attr.Description });
                                var aliases = attr.Aliases;

                                if (aliases != null) {
                                    foreach (var alias in aliases) {
                                        option.AddAlias(alias);
                                    }
                                }

                                option.IsHidden = attr.IsHidden;
                                option.IsRequired = attr.IsRequired;
                                option.AllowMultipleArgumentsPerToken = attr.AllowMultipleArgumentsPerToken;

                                var defaultValueMethod = type.GetMethod(attr.DefaultValueMethodName, BindingFlags.NonPublic | BindingFlags.Instance);

                                if (defaultValueMethod != null) {
                                    option.SetDefaultValueFactory(() => {
                                        var handlerParams = defaultValueMethod.GetParameters();
                                        var parameterValues = new object[handlerParams.Length];

                                        for (int i = 0; i < handlerParams.Length; i++) {
                                            var param = handlerParams[i];
                                            object? value = null;
                                            value = ServiceProvider.GetService(param.ParameterType);

                                            // Assign the resolved value to the parameterValues array
                                            if (value != null) {
                                                parameterValues[i] = value;
                                            }
                                        }

                                        return defaultValueMethod.Invoke(commandInstance, parameterValues);
                                    });
                                }

                                command.AddOption(option);
                            }
                        }
                    }

                    var handlerMethod = type.GetMethod(commandAttribute.HandlerMethodName, BindingFlags.Public | BindingFlags.Instance);

                    if (handlerMethod != null) {
                        AttachDynamicHandler(type, command, commandInstance!, handlerMethod);
                    }

                    var configureMethod = type.GetMethod("Configure", BindingFlags.Public | BindingFlags.Instance);

                    if (configureMethod != null) {
                        var configureMethodParams = configureMethod.GetParameters();
                        var configureParamValues = new object[configureMethodParams.Length];

                        for (int i = 0; i < configureParamValues.Length; i++) {
                            var paramType = configureMethodParams[i].ParameterType;

                            if ( paramType == typeof(Command)) {
                                configureParamValues[i] = command;
                                continue;
                            }

                            var serviceInstance = ServiceProvider.GetService(paramType);
                            configureParamValues[i] = serviceInstance ?? throw new InvalidOperationException($"Service for type {paramType.FullName} not found");
                        }

                        configureMethod.Invoke(commandInstance, configureParamValues);
                    }
                }
            }
        }

        // Configure parent-child relationships
        foreach (var relation in commandRelations) {
            if (commands.TryGetValue(relation.Child, out var childCommand) && commands.TryGetValue(relation.Parent, out var parentCommand)) {
                parentCommand.AddCommand(childCommand);
            }
        }

        // Add top-level commands to rootCommand or another appropriate place
        foreach (var command in commands.Values) {
            if (!commandRelations.Any(relation => relation.Child == command.Name)) {
                rootCommand.AddCommand(command);
            }
        }

        return this;
    }
    public IClifferBuilder BuildCommands(Action<IConfiguration, IServiceProvider> buildCommands) {
        BuildCommands();
        buildCommands(Context.Configuration, ServiceProvider);
        return this;
    }

    public IClifferBuilder ConfigureCommands(Action<IConfiguration, IServiceProvider> configureCommands) {
        configureCommands(Context.Configuration, ServiceProvider);
        return this;
    }

    public IClifferCli Build() {
        return new ClifferCli(ServiceProvider);
    }

    public void AttachDynamicHandler(Type commandType, Command command, Object commandInstance, MethodInfo handlerMethod) {
        command.Handler = CommandHandler.Create(async Task<int> (InvocationContext invocationContext) => {
            var commandType = command.GetType();
            var handlerParams = handlerMethod.GetParameters();
            var parameterValues = new object[handlerParams.Length];

            for (int i = 0; i < handlerParams.Length; i++) {
                var param = handlerParams[i];
                if (param.ParameterType == typeof(InvocationContext)) {
                    parameterValues[i] = invocationContext;
                    continue;
                }

                Symbol? symbol = null;
                var optionAttribute = param.GetCustomAttribute<OptionParamAttribute>();
                if (optionAttribute != null) {
                    symbol = command.Options.FirstOrDefault(o => o.HasAlias(optionAttribute.Name));
                }
                else {
                    var argumentAttribute = param.GetCustomAttribute<ArgumentParamAttribute>();
                    if (argumentAttribute != null) {
                        symbol = command.Arguments.FirstOrDefault(a => a.Name == argumentAttribute.Name);
                    }
                }

                if (symbol == null) {
                    symbol = command.Children.FirstOrDefault(c => c.Name == param.Name);
                }

                object? value = null;

                // Determine if the child is an option or an argument and get the value accordingly
                if (symbol is Option option) {
                    value = invocationContext.ParseResult.GetValueForOption(option);
                }
                else if (symbol is Argument argument) {
                    value = invocationContext.ParseResult.GetValueForArgument(argument);
                }
                else {
                    // If the child is neither, then get an instance of the type from the service container (dependency injection)
                    value = ServiceProvider.GetService(param.ParameterType);
                }

                // Assign the resolved value to the parameterValues array
                if (value != null) {
                    parameterValues[i] = value;
                }
            }

            Type returnType = handlerMethod.ReturnType;

            switch (returnType) {
                case Type t when t == typeof(Task):
                    var task = handlerMethod.Invoke(commandInstance, parameterValues) as Task;
                    await task!;
                    return 0;

                case Type t when t == typeof(void):
                    handlerMethod.Invoke(commandInstance, parameterValues);
                    return 0;

                case Type t when t == typeof(Task<int>):
                    var taskInt = handlerMethod.Invoke(commandInstance, parameterValues) as Task<int>;
                    return await taskInt!;

                case Type t when t == typeof(int):
                    var result = handlerMethod.Invoke(commandInstance, parameterValues);
                    return Convert.ToInt32(result);
            }

            return 0;
        });
    }

    public IClifferBuilder BuildCommands(Action<IConfiguration> buildCommands) {
        return BuildCommands((configuration, serviceProvider) => buildCommands(configuration));
    }

    public IClifferBuilder ConfigureCommands(Action<IConfiguration> configureCommands) {
        return ConfigureCommands((configuration, serviceProvider) => configureCommands(configuration));
    }
}
