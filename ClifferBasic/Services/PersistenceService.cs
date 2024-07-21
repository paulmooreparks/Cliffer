using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClifferBasic.Services;

internal class PersistenceService {
    private readonly string _filePath;

    public PersistenceService() {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var clicalcDirectory = Path.Combine(homeDirectory, ".clibasic");

        if (!Directory.Exists(clicalcDirectory)) {
            Directory.CreateDirectory(clicalcDirectory);
        }

        _filePath = Path.Combine(clicalcDirectory, "program.bas");
    }

    internal Dictionary<int, string> Load() {
        try {
            if (File.Exists(_filePath)) {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<Dictionary<int, string>>(json) ?? new Dictionary<int, string>();
            }

            return new Dictionary<int, string>();
        }
        finally {
        }
    }

    internal void Save(Dictionary<int, string> stack) {
        try {
            var json = JsonSerializer.Serialize(stack);
            File.WriteAllText(_filePath, json);
        }
        finally {
        }
    }
}
