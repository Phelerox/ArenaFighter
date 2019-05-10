using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArenaFighter.Views.ConsoleApplicationBase {
    class ConsoleApplicationPrompt {
        static void Main(string[] args) {
            Console.Title = typeof(Program).Name;
            Run();
        }

        public static void Run() {
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
                            WriteToConsole(BuildHelpMessage());
                            break;
                        case "exit":
                            AppState.SetState(State.IDLE);
                            WriteToConsole("Exiting developer mode");
                            break;
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

        public static void WriteToConsole(string message = "") {
            if (message.Length > 0) {
                Console.WriteLine(message);
            }
        }

        private const string _readPrompt = "developer mode> ";
        public static string ReadFromConsole(string promptMessage = "") {
            // Show a prompt, and get input:
            Console.Write(_readPrompt + promptMessage);
            return Console.ReadLine();
        }

        public static string BuildHelpMessage(string library = null) {
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
