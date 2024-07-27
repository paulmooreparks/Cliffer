using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliffer;
public static class ClifferEventHandler {
    public static event Action? OnExit;
    public static event Action<string[]>? OnPreprocessArgs;

    public static void Exit(int exitCode) {
        OnExit?.Invoke();
        Environment.Exit(exitCode);
    }

    public static void PreprocessArgs(string[] args) {
        OnPreprocessArgs?.Invoke(args);
    }
}
