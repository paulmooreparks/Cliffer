using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cliffer;

internal class InvocationHelper {
    public static async Task<int> InvokeCommandHandlerAsync(Type commandType, IServiceProvider serviceProvider, object[]? additionalArgs = null) {
        var handlerMethod = FindHandlerMethod(commandType);

        if (handlerMethod == null) {
            throw new InvalidOperationException($"Handler method not found for command type {commandType.Name}");
        }

        var instance = ResolveInstance(serviceProvider, commandType);

        if (instance == null) {
            throw new InvalidOperationException($"Instance of type {commandType.Name} could not be created or resolved.");
        }

        var parameters = ResolveHandlerParameters(handlerMethod, serviceProvider, additionalArgs);

        var result = handlerMethod.Invoke(instance, parameters);

        if (result is Task<int> taskWithIntResult) {
            return await taskWithIntResult;
        }
        else if (result is Task task) {
            await task;
            return 0; // Assuming success
        }
        else if (result is int intResult) {
            return intResult;
        }

        return 0; // Default success case
    }

    private static MethodInfo? FindHandlerMethod(Type commandType) {
        // Implement logic to find the handler method.
        // This could be based on naming conventions, attributes, etc.
        return commandType.GetMethod("Handle");
    }

    private static object? ResolveInstance(IServiceProvider serviceProvider, Type commandType) {
        // Use the service provider to get an instance of the command type.
        // This allows for dependency injection into command handlers.
        return serviceProvider.GetService(commandType) ?? Activator.CreateInstance(commandType);
    }

    private static object?[] ResolveHandlerParameters(MethodInfo method, IServiceProvider serviceProvider, object[]? additionalArgs) {
        var parameters = method.GetParameters();
        var parameterValues = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++) {
            var parameterType = parameters[i].ParameterType;

            // First, try to resolve parameter from additional arguments if available.
            object? value = additionalArgs?.FirstOrDefault(a => a?.GetType() == parameterType);

            // If not found in additional arguments, resolve from the service provider.
            if (value == null) {
                value = serviceProvider.GetService(parameterType);
            }

            parameterValues[i] = value ?? throw new InvalidOperationException($"Could not resolve parameter value for type {parameterType.Name}");
        }

        return parameterValues;
    }
}
