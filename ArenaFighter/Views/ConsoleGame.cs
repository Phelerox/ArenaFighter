using System.Security.AccessControl;
using System.Runtime.InteropServices;
using System.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Humanizer;

using ArenaFighter.ConsoleApplicationBase;
using ArenaFighter.Presenters;
using ArenaFighter.Models.Utils;

namespace ArenaFighter.Views
{
    class ConsoleGame : IView
    {
        private Presenter presenter;
        private ArenaFighter.Models.Player player;

        private Menu<Genders> menuCharCreationChooseGender = new Menu<Genders>(
            "What is your characters gender?",
            new Dictionary<string, Tuple<string, Genders>>() {
                ["m"] = new Tuple<string, Genders>("Male", Genders.Male),
                ["f"] = new Tuple<string, Genders>("Female", Genders.Female),
                ["o"] = new Tuple<string, Genders>("Non-Binary", Genders.Non_Binary),
            }
        );


        //string description, Dictionary<string, Tuple<string, Func<string>>> options

        public ConsoleGame()
        {
            presenter = Presenter.Instance(this);
            Console.Title = "Dungeons & Gladiators";
            CharacterCreation();
            GameLoop();
        }

        private void GameLoop() {
            RunDeveloperMode();
        }

        private void CharacterCreation() {;
            ArenaFighter.Models.Race race = new ArenaFighter.Models.MountainDwarf();
            Genders gender = menuCharCreationChooseGender.Ask();
            string name = AskUserForString($"What is {Language.PossessiveAdjective(gender)} name? ");
            player = presenter.CreateCharacter(name, gender, race);
            Console.WriteLine(player);
            
        }

        private string AskUserForString(string prompt) {
            Console.Write(prompt);
            return Console.ReadLine();
        }


        public void RunDeveloperMode()
        {
            AppState.SetState(State.RUNNING);
            while (AppState.GetState() > State.IDLE)
            {
                var consoleInput = ReadFromConsole();
                if (string.IsNullOrWhiteSpace(consoleInput)) continue;

                try
                {
                    // Create a ConsoleCommand instance:
                    var cmd = new ConsoleCommand(consoleInput);

                    switch (cmd.Name)
                    {
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
                }
                catch (Exception ex)
                {
                    // OOPS! Something went wrong - Write out the problem:
                    WriteToConsole(ex.Message);
                }
            }
        }

        static void WriteToConsole(string message = "")
        {
            if (message.Length > 0)
            {
                Console.WriteLine(message);
            }
        }

        const string _readPrompt = "developer mode> ";
        public static string ReadFromConsole(string promptMessage = "")
        {
            // Show a prompt, and get input:
            Console.Write(_readPrompt + promptMessage);
            return Console.ReadLine();
        }

        static string BuildHelpMessage(string library = null)
        {
            var sb = new StringBuilder("Commands: ");
            sb.AppendLine();
            foreach (var item in CommandLibrary.Content)
            {
                if (library != null && item.Key != library)
                    continue;
                foreach (var cmd in item.Value.MethodDictionary)
                {
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