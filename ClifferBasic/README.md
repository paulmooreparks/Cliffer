
# ClifferBasic

ClifferBasic is a sample program for the Cliffer CLI library that implements a very simple BASIC interpreter environment as a REPL (Read-Eval-Print Loop). This project demonstrates the usage of the Cliffer CLI library to build a custom command-line application with an interactive BASIC-like language interpreter.

## Features

- Interactive REPL for executing BASIC-like commands.
- Supports variable assignment and arithmetic operations.
- Commands for printing, listing, saving, and loading programs.
- Extensible command structure using the Cliffer CLI library.

## Commands

- `let`: Assign a value to a variable.
  ```basic
  let x = 5.5
  let y# = 2
  ```
- `print`: Print text to the screen or evaluate an expression.
  ```basic
  print "Hello, World!"
  print x + y#
  ```
- `list`: List the current program in memory.
  ```basic
  list
  ```
- `save`: Save the current program to persistent storage.
  ```basic
  save "filename"
  ```
- `load`: Load a program from persistent storage.
  ```basic
  load "filename"
  ```
- `run`: Run the program currently in memory.
  ```basic
  run
  ```
- `rem`: Add a comment to the program.
  ```basic
  rem This is a comment
  ```

## Project Structure

- [`ClifferBasic.cs`](ClifferBasic.cs): Entry point of the application, sets up the command-line interface and services.
- [`BasicReplContext.cs`](BasicReplContext.cs): Custom REPL context for handling command input and execution.
- [`Commands`](Commands): Directory containing all CLI command implementations.
- [`Services`](Services): Directory containing supporting services.
- [`Model`](Model): Directory containing models classes.

## Example

Here's an example session with ClifferBasic:

```basic
> let x = 20.5
> let y# = 10
> print x + y#
30.5
> 20 print "World!"
> 10 print "Hello"
> 30 let x = 123
> 40 print x
> list
10 print "Hello"
20 print "World!"
30 let x = 123
40 print x
> save "program.bas"
> load "program.bas"
> run
```
