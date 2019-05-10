using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

using ArenaFighter.Models;
using ArenaFighter.Models.Utils;
using ArenaFighter.Presenters;
using ArenaFighter.Views.ConsoleApplicationBase;

using Humanizer;

using ProtoBuf;
using ProtoBuf.Meta;

namespace ArenaFighter.Views {
    class ConsoleGame : IView {
        private Presenter presenter;
        private ArenaFighter.Models.Player Player { get { return presenter.Player; } }

        private Menu<Genders> menuCreationChooseGender = new MenuOptions<Genders>("What is your characters gender?")
            .AddOption("m", "Male", Genders.Male)
            .AddOption("f", "Female", Genders.Female)
            .AddOption("n", "Non-Binary", Genders.Non_Binary).MenuFactory();
        private Menu<bool> menuYesNo = new MenuOptions<bool>("Would you like to Proceed?")
            .AddOption("y", "Yes", true)
            .AddOption("n", "No", false).MenuFactory();

        //string description, Dictionary<string, Tuple<string, Func<string>>> options

        public ConsoleGame() {
            presenter = Presenter.Instance;
            presenter.View = this;
            if (Console.WindowWidth < 90) {
                try {
                    Console.SetWindowSize(Math.Min(90, Console.LargestWindowWidth), Math.Min(Console.LargestWindowHeight, 60));
                } catch (System.PlatformNotSupportedException e) {
                    if (Program.Debugging) {
                        Console.WriteLine($"Exception caught: Can't set WindowSize! {e}");
                    }

                }

            }
            Console.Title = "Dungeons & Gladiators";
            CharacterCreation();
            GameLoop();
            AskUserForString("Bye!");
        }

        private void GameLoop() {
            do {
                WriteCenteredLines(Player.CompareWithAsString(presenter.NextOpponent), null);
                string deathSavingThrows = presenter.NextArenaEvent();
                WriteCenteredLines(presenter.PastBattles.Last().Item3);
                if (deathSavingThrows != null) {
                    WriteCenteredLines(deathSavingThrows, null);
                }
            } while (!presenter.GameOver && menuYesNo.Ask("Fight another battle or retire?"));
            if (presenter.GameOver) {
                Retire();
            } else if (menuYesNo.Ask("Are you sure you want to retire?")) {
                Retire();
            } else {
                GameLoop();
            }
        }

        private void CharacterCreation() {;
            ArenaFighter.Models.Race race = new ArenaFighter.Models.MountainDwarf();
            Genders gender = menuCreationChooseGender.Ask();
            string name = AskUserForString($"What is {Language.PossessiveAdjective(gender)} name? ");
            presenter.CreateCharacterAction(name, gender, race);
            WriteCenteredLines(Player.ToString());
            string prompt = "Proceed from character creation?";
            while (!menuYesNo.Ask(prompt)) {
                presenter.RerollCharacterAction();
                WriteCenteredLines(Player.ToString());
                prompt = $"Proceed from character creation? (You have rerolled {Player.NumberOfRerolls} times)";
            }

        }

        private void ReviewCombatLogs() {
            IList<Tuple<BaseCharacter, Player, string>> battles = presenter.PastBattles;
            int reviewingBattle = battles.Count;
            string prompt = "Would you like to go through your combat history?";
            while (--reviewingBattle >= 0 && menuYesNo.Ask(prompt)) {
                WriteCenteredLines($"Reviewing Battle #{reviewingBattle+1}", null, "->", "<-");
                WriteCenteredLines(battles[reviewingBattle].Item2.CompareWithAsString(battles[reviewingBattle].Item1), null);
                WriteCenteredLines(battles[reviewingBattle].Item3);
                WriteCenteredLines($"End of Battle #{reviewingBattle+1}", null, "->", "<-");
                prompt = $"Review Battle #{reviewingBattle}?";
            }
        }

        private void Retire() {
            string retireMessage = presenter.RetireCampaignAction();
            WriteCenteredLines("GAME OVER", null, "->", "<-");
            WriteCenteredLines(retireMessage, onlyLinesContaining : null);
            WriteCenteredLines("GAME OVER", null, "->", "<-");
            ReviewCombatLogs();

        }

        private string AskUserForString(string prompt) {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        public bool SaveGame(string filename = null) {
            if (filename == null)filename = $"{Player.Name}.sav";
            return false;
        }

        public bool LoadGame(string filename = null) { //TODO: make things serializable with protobuf-net
            if (filename == null)filename = $"{Player.Name}.sav";
            return false;
        }

        public void RunDeveloperMode() {
            ConsoleApplicationPrompt.Run();
        }

        public static void WriteCenteredLines(string output, string onlyLinesContaining = "\u200B", string leftPadding = " ", string rightPadding = " ", string separator = " ") {
            Console.SetCursorPosition(0, Console.CursorTop);
            foreach (string line in output.Split('\n')) {
                int paddingRequired = Math.Max(Console.WindowWidth - line.Length, 0);
                if (onlyLinesContaining != null) {
                    if (!line.Contains(onlyLinesContaining)) {
                        Console.WriteLine(line);
                        continue;
                    }
                    paddingRequired = Math.Max(Console.WindowWidth - line.Length - 1, 0);
                }
                Console.WriteLine(Language.PadLines(line, paddingRequired, leftPadding, rightPadding, separator));
            }
        }
    }
}
