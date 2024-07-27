# Cliffer - Command Line Interface Library for .NET

Cliffer is a library built on top of Microsoft's [System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/) library. It provides the following features:

* Attribute-based development, so that you can focus on writing the business logic you need and leave the boilerplate code to the library.
* Automatic help-text generation.
* Built-in support for REPL (read-eval-print loop) interaction.
* Built-in support for macro definitions, both in configuration and in code.
* Targeting multiple platforms with .NET.

Cliffer uses [.NET attributed programming](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/) to define commands, options, arguments, parameters, 
etc., and it manages all the boilerplate logic for you so you can focus on writing your code and just declare your intentions for that code when you're ready to use it. Cliffer takes 
care of the rest.

Here is an example from the [clicalc](https://github.com/paulmooreparks/Cliffer/blob/master/clicalc/CliCalcProgram.cs) sample:

```c#
[Command("push", "Push one or more numbers onto the stack")]
[Argument(typeof(IEnumerable<double>), "values", "Numbers to push onto the stack", Cliffer.ArgumentArity.OneOrMore)]
internal class PushCommand {
    public int Execute(IEnumerable<double> values, Stack<double> stack) {
        stack.PushAll(values);
        return Result.Success;
    }
}
```

In this example, the `Command` attribute declares that the `PushCommand` class implements the `push` command. The `Argument` attribute declares that the command accepts one 
or more arguments of type `double` with the name `values`. The `Execute` method will be called when the user invokes the `push` command, and Cliffer will pass the arguments 
as well as any required services as parameters to the method.

Here is another example:

```c#
[Command("hello", "Say hello")]
[Argument(typeof(string), "addressee", "State whom you are greeting.")]
internal class HelloCommand {
    [Macro("sunshine", "Greet the morning sun.")]
    private static string sunshine => "hello Sunshine";

    public int Execute(string addressee) {
        if (string.IsNullOrEmpty(addressee)) {
            addressee = "World";
        }

        Console.WriteLine($"Hello, {addressee}!");
        return 0;
    }
}
```

In this example, the `HelloCommand` class implements the `hello` command. The `Argument` attribute declares that the command accepts a single argument of type `string`. 
The `Macro` attribute declares a macro that can be used to call `hello` with a pre-defined argument. The `Execute` method will be called when the user invokes the `hello` command.

Note that macros can be defined in the configuration file as well as in code, and if defined in code, they may be defined on any static string property in any class.

## Demo Applications

There are three demo applications to demonstrate the features and capabilities of Cliffer:

* [CliCalc](CliCalc) - A stack calculator implemented as a REPL (read/eval/print loop).
* [ClifferBasic](https://github.com/paulmooreparks/ClifferBasic) - A BASIC interpreter in a REPL environment, like the old Commodore 64 or Apple ][ or TI-99/4A that so many of us grew up with.
* [ClifferDemo](ClifferDemo) - A walkthrough of various Cliffer features.

Unfortunately, these are all in various stages of being built, but feel free to explore them or even add to them.
