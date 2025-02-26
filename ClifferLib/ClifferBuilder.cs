using System.Reflection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cliffer;

public class ClifferBuilder : IClifferBuilder {
    internal IConfiguration? _configuration = null;
    internal IServiceCollection _services = new ServiceCollection();
    internal IServiceProvider? _serviceProvider;
    internal ConfigurationBuilder? _configurationBuilder;

    internal RootCommand _rootCommand = new RootCommand();
    private readonly Dictionary<string, Object> _commands = new Dictionary<string, Object>();

    public ClifferBuilder() 
    {
        ConfigureDefaultConfiguration();
    }

    public IConfiguration BuildConfiguration() {
        if (_configurationBuilder is null) {
            _configurationBuilder = new ConfigurationBuilder();
        }

        return _configurationBuilder.Build();
    }

    private void ConfigureDefaultConfiguration() {
        if (_configurationBuilder is null) {
            _configurationBuilder = new ConfigurationBuilder();
        }

        // Get the directory of the currently executing assembly
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

        if (assemblyDirectory != null) {
            var appSettingsPath = Path.Combine(assemblyDirectory, "appSettings.json");

            if (File.Exists(appSettingsPath)) {
                _configurationBuilder.AddJsonFile(appSettingsPath, optional: true, reloadOnChange: true);
            }
        }

        _configuration = BuildConfiguration();
        _services.AddClifferServices(_configuration);
        ConfigureAppConfiguration();
        ConfigureServices();
    }

    public IClifferBuilder ConfigureAppConfiguration() {
        if (_configurationBuilder is null) {
            _configurationBuilder = new ConfigurationBuilder();
        }

        _configuration = BuildConfiguration();
        _services.AddSingleton<IConfiguration>(_configuration);
        return this;
    }

    public IClifferBuilder ConfigureAppConfiguration(Action<IConfigurationBuilder> configure) {
        if (_configurationBuilder is null) {
            _configurationBuilder = new ConfigurationBuilder();
        }

        configure(_configurationBuilder);
        return ConfigureAppConfiguration();
    }

    public IClifferBuilder ConfigureServices() {
        return this;
    }

    public IClifferBuilder ConfigureServices(Action<IServiceCollection> configureServices) {
        configureServices(_services);
        return ConfigureServices();
    }

    public IClifferBuilder ConfigureServices<TContext>(Action<TContext, IServiceCollection> configureServices, TContext context) {
        configureServices(context, _services);
        return ConfigureServices();
    }

    public IClifferBuilder BuildCommands(IServiceProvider serviceProvider, Action<IConfiguration, RootCommand, IServiceProvider> buildCommands) {
        BuildCommands(serviceProvider);
        buildCommands(_configuration!, _rootCommand, serviceProvider);
        return this;
    }

    internal IClifferBuilder BuildCommands(IServiceProvider serviceProvider) {
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

        var argumentAttributes = rootConstructorInfo.GetCustomAttributes<ArgumentAttribute>();

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

                    var defaultValueMethod = rootCommandType.GetMethod(attr.DefaultValueMethodName, BindingFlags.NonPublic | BindingFlags.Instance);

                    if (defaultValueMethod != null) {
                        argument.SetDefaultValueFactory(() => {
                            var handlerParams = defaultValueMethod.GetParameters();
                            var parameterValues = new object[handlerParams.Length];

                            for (int i = 0; i < handlerParams.Length; i++) {
                                var param = handlerParams[i];
                                object? value = null;
                                value = _serviceProvider.GetService(param.ParameterType);

                                // Assign the resolved value to the parameterValues array
                                if (value != null) {
                                    parameterValues[i] = value;
                                }
                            }

                            return defaultValueMethod.Invoke(_rootCommand, parameterValues);
                        });
                    }

                    _rootCommand.AddArgument(argument);
                }
            }
        }

        var optionAttributes = rootCommandType.GetCustomAttributes<OptionAttribute>();

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

                    System.CommandLine.ArgumentArity arity = attr.Arity switch {
                        ArgumentArity.Zero => System.CommandLine.ArgumentArity.Zero,
                        ArgumentArity.ZeroOrOne => System.CommandLine.ArgumentArity.ZeroOrOne,
                        ArgumentArity.ExactlyOne => System.CommandLine.ArgumentArity.ExactlyOne,
                        ArgumentArity.ZeroOrMore => System.CommandLine.ArgumentArity.ZeroOrMore,
                        ArgumentArity.OneOrMore => System.CommandLine.ArgumentArity.OneOrMore,
                        _ => System.CommandLine.ArgumentArity.ZeroOrOne
                    };

                    option.Arity = arity;
                    option.IsHidden = attr.IsHidden;
                    option.IsRequired = attr.IsRequired;
                    option.AllowMultipleArgumentsPerToken = attr.AllowMultipleArgumentsPerToken;

                    if (attr.FromAmong.Length > 0) {
                        option.FromAmong(attr.FromAmong);
                    }

                    var defaultValueMethod = rootCommandType.GetMethod(attr.DefaultValueMethodName, BindingFlags.NonPublic | BindingFlags.Instance);

                    if (defaultValueMethod != null) {
                        option.SetDefaultValueFactory(() => {
                            var handlerParams = defaultValueMethod.GetParameters();
                            var parameterValues = new object[handlerParams.Length];

                            for (int i = 0; i < handlerParams.Length; i++) {
                                var param = handlerParams[i];
                                object? value = null;
                                value = _serviceProvider.GetService(param.ParameterType);

                                // Assign the resolved value to the parameterValues array
                                if (value != null) {
                                    parameterValues[i] = value;
                                }
                            }

                            return defaultValueMethod.Invoke(_rootCommand, parameterValues);
                        });
                    }

                    _rootCommand.AddOption(option);
                }
            }
        }

        // Prepare an array to hold the parameter values for the constructor
        var rootConstructorParams = rootConstructorInfo.GetParameters();
        var rootConstructorParamValues = new object[rootConstructorParams.Length];

        // Fill the array with instances obtained from the service container
        for (int i = 0; i < rootConstructorParams.Length; i++) {
            var param = rootConstructorParams[i];
            var paramType = rootConstructorParams[i].ParameterType;

            Symbol? symbol = null;
            var optionAttribute = param.GetCustomAttribute<OptionParamAttribute>();
            if (optionAttribute != null) {
                symbol = _rootCommand.Options.FirstOrDefault(o => o.HasAlias(optionAttribute.Name));
            }
            else {
                var argumentAttribute = param.GetCustomAttribute<ArgumentParamAttribute>();
                if (argumentAttribute != null) {
                    symbol = _rootCommand.Arguments.FirstOrDefault(a => a.Name == argumentAttribute.Name);
                }
            }

            if (symbol == null) {
                symbol = _rootCommand.Children.FirstOrDefault(c => c.Name == param.Name);
            }

            // Determine if the child is an option or an argument and get the value accordingly
            if (symbol is Option option) {
                if (paramType == typeof(Option)) {
                    rootConstructorParamValues[i] = option;
                }
            }
            else if (symbol is Argument argument) {
                if (paramType == typeof(Argument)) {
                    rootConstructorParamValues[i] = argument;
                }
            }
            else {
                if (paramType == typeof(System.CommandLine.RootCommand)) {
                    rootConstructorParamValues[i] = _rootCommand;
                }
                else {
                    var serviceInstance = serviceProvider.GetService(paramType);
                    rootConstructorParamValues[i] = serviceInstance ?? throw new InvalidOperationException($"Service for type {paramType.FullName} not found");
                }
            }
        }

        if (!string.IsNullOrEmpty(rootCommandAttribute.Name)) {
            _rootCommand.Name = rootCommandAttribute.Name;
        }

        _rootCommand.Description = rootCommandAttribute.Description;
        _rootCommand.IsHidden = rootCommandAttribute.IsHidden;

        {
            var aliases = rootCommandAttribute.Aliases;

            if (aliases != null) {
                foreach (var alias in aliases) {
                    _rootCommand.AddAlias(alias);
                }
            }
        }

        _services.AddSingleton(_rootCommand);
        _serviceProvider = _services.BuildServiceProvider();

        // Create an instance of the command using the constructor with parameters
        var rootCommandInstance = rootConstructorInfo.Invoke(rootConstructorParamValues);

        if (rootCommandInstance is not null) {
            AttachDynamicHandler(rootCommandType, _rootCommand, rootCommandInstance!, rootHandlerMethod);
            _commands.Add(_rootCommand.Name, rootCommandInstance!);


            var configureMethod = rootCommandType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Instance);

            if (configureMethod != null) {
                var configureMethodParams = configureMethod.GetParameters();
                var configureParamValues = new object[configureMethodParams.Length];

                for (int i = 0; i < configureParamValues.Length; i++) {
                    var paramType = configureMethodParams[i].ParameterType;
                    var serviceInstance = _serviceProvider.GetService(paramType);
                    configureParamValues[i] = serviceInstance ?? throw new InvalidOperationException($"Service for type {paramType.FullName} not found");
                }

                configureMethod.Invoke(rootCommandInstance, configureParamValues);
            }
        }

        return AddCommands(entryAssembly);
    }

    public IClifferBuilder AddCommands(Assembly assembly) {
        /* TODO: This can be optimised. If the CLI is not starting in interactive mode and is not displaying the help text, 
        then all of the commands do not need to be processed. That could improve startup time in a non-interactive environment. */

        if (_serviceProvider is null) {
            throw new ApplicationException("Service provider is null");
        }

        var commandTypes = assembly.GetTypes()
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
                        constructorParamValues[i] = _serviceProvider;
                        continue;
                    }

                    var serviceInstance = _serviceProvider.GetService(paramType);
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
                                            value = _serviceProvider.GetService(param.ParameterType);

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

                                System.CommandLine.ArgumentArity arity = attr.Arity switch {
                                    ArgumentArity.Zero => System.CommandLine.ArgumentArity.Zero,
                                    ArgumentArity.ZeroOrOne => System.CommandLine.ArgumentArity.ZeroOrOne,
                                    ArgumentArity.ExactlyOne => System.CommandLine.ArgumentArity.ExactlyOne,
                                    ArgumentArity.ZeroOrMore => System.CommandLine.ArgumentArity.ZeroOrMore,
                                    ArgumentArity.OneOrMore => System.CommandLine.ArgumentArity.OneOrMore,
                                    _ => System.CommandLine.ArgumentArity.ZeroOrOne
                                };

                                option.Arity = arity;
                                option.IsHidden = attr.IsHidden;
                                option.IsRequired = attr.IsRequired;
                                option.AllowMultipleArgumentsPerToken = attr.AllowMultipleArgumentsPerToken;

                                if (attr.FromAmong.Length > 0) {
                                    option.FromAmong(attr.FromAmong);
                                }

                                var defaultValueMethod = type.GetMethod(attr.DefaultValueMethodName, BindingFlags.NonPublic | BindingFlags.Instance);

                                if (defaultValueMethod != null) {
                                    option.SetDefaultValueFactory(() => {
                                        var handlerParams = defaultValueMethod.GetParameters();
                                        var parameterValues = new object[handlerParams.Length];

                                        for (int i = 0; i < handlerParams.Length; i++) {
                                            var param = handlerParams[i];
                                            object? value = null;
                                            value = _serviceProvider.GetService(param.ParameterType);

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
                        _commands.Add(command.Name, commandInstance!);
                    }

                    var configureMethod = type.GetMethod("Configure", BindingFlags.Public | BindingFlags.Instance);

                    if (configureMethod != null) {
                        var configureMethodParams = configureMethod.GetParameters();
                        var configureParamValues = new object[configureMethodParams.Length];

                        for (int i = 0; i < configureParamValues.Length; i++) {
                            var paramType = configureMethodParams[i].ParameterType;

                            if (paramType == typeof(Command)) {
                                configureParamValues[i] = command;
                                continue;
                            }

                            var serviceInstance = _serviceProvider.GetService(paramType);
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

        // Add top-level commands to _rootCommand or another appropriate place
        foreach (var command in commands.Values) {
            if (!commandRelations.Any(relation => relation.Child == command.Name)) {
                _rootCommand.AddCommand(command);
            }
        }

        return this;
    }

    public IClifferBuilder ConfigureCommands(Action<IConfiguration, RootCommand, IServiceProvider> configureCommands) {
        if (_serviceProvider is null) {
            throw new InvalidOperationException("Service provider not found");
        }

        if (_configuration is null) {
            throw new InvalidOperationException("Configuration not found");
        }

        configureCommands(_configuration, _rootCommand, _serviceProvider);
        return this;
    }

    public IClifferCli Build(Action<IConfiguration, RootCommand, IServiceProvider>? buildCommands = null) {
        if (_serviceProvider is null) {
            _serviceProvider = _services.BuildServiceProvider();
        }

        BuildCommands(_serviceProvider);
        _configuration = _serviceProvider.GetService<IConfiguration>()!;

        if (buildCommands is not null) {
            buildCommands(_configuration, _rootCommand, _serviceProvider);
        }

        Utility.SetServiceProvider(_serviceProvider);

        var macros = new Dictionary<string, MacroDefinition>();
        var macroSection = _configuration.GetSection("Cliffer").GetSection("Macros");
        var baseMacros = macroSection.Get<MacroDefinition[]>();

        if (baseMacros is not null) {
            foreach (var macro in baseMacros) {
                macros.Add(macro.Name, macro);
            }
        }

        var entryAssembly = Assembly.GetEntryAssembly();

        if (entryAssembly is not null) {
            var macroProperties = entryAssembly.GetTypes()
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .Where(prop => Attribute.IsDefined(prop, typeof(MacroAttribute)));

            foreach (var property in macroProperties) {
                var attribute = property.GetCustomAttribute<MacroAttribute>();

                if (attribute is not null) {
                    var macroScript = property.GetValue(null) as string;

                    if (!string.IsNullOrEmpty(macroScript)) {
                        var macro = new MacroDefinition() { Name = attribute.Name, Description = attribute.Description, Script = macroScript };
                        macros.Add(attribute.Name, macro);
                    }
                }
            }
        }

        foreach (var macro in macros) {
            var macroCommand = new Macro(macro.Value.Name, $"[macro] {macro.Value.Description}", macro.Value.Script);
            _rootCommand.AddCommand(macroCommand);
        }

        return new ClifferCli(_serviceProvider, _services, _rootCommand);
    }

    public void AttachDynamicHandler(Type commandType, Command command, Object commandInstance, MethodInfo handlerMethod) {
        command.Handler = CommandHandler.Create(async Task<int> (InvocationContext invocationContext) => {
            if (_serviceProvider is null) {
                throw new InvalidOperationException("Service provider not found");
            }

            var commandType = command.GetType();
            var handlerParams = handlerMethod.GetParameters();
            var parameterValues = new object[handlerParams.Length];

            for (int i = 0; i < handlerParams.Length; i++) {
                var param = handlerParams[i];
                if (param.ParameterType == typeof(InvocationContext)) {
                    parameterValues[i] = invocationContext;
                    continue;
                }

                if (param.ParameterType == typeof(Command)) {
                    parameterValues[i] = invocationContext.ParseResult.CommandResult.Command;
                    continue;
                }

                Symbol? symbol = param switch {
                    _ when param.GetCustomAttribute<OptionParamAttribute>() is OptionParamAttribute optionAttribute
                        => command.Options.FirstOrDefault(o => o.HasAlias(optionAttribute.Name)),

                    _ when param.GetCustomAttribute<ArgumentParamAttribute>() is ArgumentParamAttribute argumentAttribute
                        => command.Arguments.FirstOrDefault(a => a.Name == argumentAttribute.Name),

                    _ => command.Children.FirstOrDefault(c => c.Name == param.Name)
                };

                object? value = null;

                // Determine if the child is an option or an argument and get the value accordingly
                if (symbol is Option option) {
                    if (param.ParameterType == typeof(Option)) {
                        value = option;
                    }
                    else {
                        value = invocationContext.ParseResult.GetValueForOption(option);
                    }
                }
                else if (symbol is Argument argument) {
                    if (param.ParameterType == typeof(Argument)) {
                        value = argument;
                    }
                    else {
                        value = invocationContext.ParseResult.GetValueForArgument(argument);
                    }

#if false
                    /* Nullable types still aren't supported as parameters because I'm still trying to figure out 
                    how to pass them. If anyone has any ideas here, please let me know. */

                    if (param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>) && value is not null) {
                        var underlyingType = Nullable.GetUnderlyingType(param.ParameterType);

                        if (value is not null && underlyingType is not null) {
                            value = Convert.ChangeType(invocationContext.ParseResult.GetValueForArgument(argument), underlyingType);
                            var nullableType = typeof(Nullable<>).MakeGenericType(underlyingType);
                            var nullableInstance = Activator.CreateInstance(nullableType, new object[] { });
                            // value = Activator.CreateInstance(param.ParameterType, new object?[] { value });
                        }
                    }
#endif
                }
                else {
                    var commandParamAttribute = param.GetCustomAttribute<CommandParamAttribute>();
                    if (commandParamAttribute != null) {
                        value = _commands.FirstOrDefault(kvp => kvp.Key == commandParamAttribute.Name).Value;
                    }
                    else {
                        // If the child is none of the above, then get an instance of the type from the service container (dependency injection)
                        value = _serviceProvider.GetService(param.ParameterType);
                    }
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

    private static object? ConvertToType(object value, Type targetType) {
        // Use reflection to convert value to the target type
        MethodInfo? method = typeof(ClifferBuilder).GetMethod("ConvertToGeneric", BindingFlags.Static | BindingFlags.NonPublic);
        MethodInfo? generic = method?.MakeGenericMethod(targetType);
        return generic?.Invoke(null, new object[] { value });
    }

    private static T ConvertToGeneric<T>(object value) {
        return (T)value;
    }

    public IClifferBuilder ConfigureCommands(Action<IConfiguration, RootCommand> configureCommands) {
        return ConfigureCommands((configuration, rootCommand, serviceProvider) => configureCommands(configuration, _rootCommand));
    }
}
