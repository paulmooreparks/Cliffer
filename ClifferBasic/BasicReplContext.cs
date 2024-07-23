﻿using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClifferBasic.Services;
using Cliffer;

namespace ClifferBasic;

internal class BasicReplContext : Cliffer.DefaultReplContext {
    private readonly CommandSplitter _splitter = Utility.GetService<CommandSplitter>()!;
    public override string GetTitleMessage() => "Cliffer Basic";

    public override string GetPrompt(Command command, InvocationContext context) => "> ";

    public override string[] GetPopCommands() => [];

    public override void OnEntry() {
        base.OnEntry();
    }

    public override string[] SplitCommandLine(string input) {
        return _splitter.Split(input).ToArray();
    }

    public override Task<int> RunAsync(Command command, string[] args) {
        if (args.Length > 1 && int.TryParse(args[0], out int lineNumber)) {
            var programService = Utility.GetService<ProgramService>()!;
            programService.Program.SetLine(lineNumber, args.Skip(1).ToArray());
            return Task.FromResult(Result.Success);
        }

        return base.RunAsync(command, args);
    }
}
