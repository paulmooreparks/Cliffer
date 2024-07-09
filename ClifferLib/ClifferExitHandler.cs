using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliffer;
public static class ClifferExitHandler {
    public static event Action? OnExit;

    public static void Exit(int exitCode) {
        OnExit?.Invoke();
        Environment.Exit(exitCode);
    }
}
