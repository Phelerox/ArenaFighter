using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArenaFighter.ConsoleApplicationBase {
    class Program {
        static void Main(string[] args) {
            Console.Title = typeof(Program).Name;
            Run();
        }

        static void Run() {
            AppState.SetState(State.RUNNING);
            while (AppState.GetState() > State.IDLE) {
                var consoleInput = ReadFromConsole();
                if (string.IsNullOrWhiteSpace(consoleInput))continue;

                try {
                    // Create a ConsoleCommand instance:
                    var cmd = new ConsoleCommand(consoleInput);

                    switch (cmd.Name) {
                        case "help":
                        case "?":
                        case "ayuda":
                            WriteToConsole(BuildHelpMessage());
                            break;
                        case "exit":
                        case "salir":
                            WriteToConsole("Closing program...");
                            return;
                        default:
                            // Execute the command:
                            string result = CommandHandler.Execute(cmd);

                            // Write out the result:
                            WriteToConsole(result);
                            break;
                    }
                } catch (Exception ex) {
                    // OOPS! Something went wrong - Write out the problem:
                    WriteToConsole(ex.Message);
                }
            }
        }

        static void WriteToConsole(string message = "") {
            if (message.Length > 0) {
                Console.WriteLine(message);
            }
        }

        const string _readPrompt = "console> ";
        public static string ReadFromConsole(string promptMessage = "") {
            // Show a prompt, and get input:
            Console.Write(_readPrompt + promptMessage);
            return Console.ReadLine();
        }

        static string BuildHelpMessage(string library = null) {
            var sb = new StringBuilder("Commands: ");
            sb.AppendLine();
            foreach (var item in CommandLibrary.Content) {
                if (library != null && item.Key != library)
                    continue;
                foreach (var cmd in item.Value.MethodDictionary) {
                    sb.Append(ConsoleFormatting.Indent(1));
                    sb.Append(item.Key);
                    sb.Append(".");
                    sb.Append(cmd.Key);
                    sb.AppendLine();
                }

            }
            return sb.ToString();
        }
    }
}
