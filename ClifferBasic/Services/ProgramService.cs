using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Cliffer;

using ClifferBasic.Model;

namespace ClifferBasic.Services;

internal class ProgramService {
    private string _filePath = string.Empty;

    internal ProgramModel Program { get; } = new();

    public ProgramService() {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Directory.SetCurrentDirectory(homeDirectory);
        var cliBasicDirectory = Path.Combine(homeDirectory, ".clibasic");

        if (!Directory.Exists(cliBasicDirectory)) {
            Directory.CreateDirectory(cliBasicDirectory);
        }
    }

    internal ProgramModel Load(string filename) {
        try {
            if (File.Exists(filename)) {
                _filePath = Path.GetFullPath(filename);
                var splitter = Utility.GetService<CommandSplitter>()!;
                var lines = File.ReadAllLines(_filePath);
                Program.New();

                foreach (var line in lines) {
                    var tokens = splitter.Split(line).ToArray();
                    if (tokens.Length > 1 && int.TryParse(tokens[0], out int lineNumber)) {
                        Program.SetLine(lineNumber, tokens.Skip(1).ToArray());
                    }
                    else {
                        throw new ApplicationException($"Invalid line: {line}");
                    }
                }
            }
            else {
                throw new ApplicationException($"File does not exist: {filename}");
            }

            return Program;
        }
        finally {
        }
    }

    internal void Save(string filename) {
        try {
            if (File.Exists(filename)) {
                File.Delete(filename);
            }

            File.AppendAllLines(filename, Program.Listing);
            _filePath = Path.GetFullPath(filename);
        }
        finally {
        }
    }

    internal void New() { 
        Program.New();
    }
}
