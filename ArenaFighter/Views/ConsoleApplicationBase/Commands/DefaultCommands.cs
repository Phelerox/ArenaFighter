using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// All console commands must be in the sub-namespace Commands:
namespace ArenaFighter.Views.ConsoleApplicationBase.Commands {
    // Must be a public static class:
    public static class DefaultCommands {
        // Methods used as console commands must be public and must return a string

        public static string Exit() {
            AppState.SetState(State.IDLE);
            return "Exiting developer mode..";
        }
    }
}
