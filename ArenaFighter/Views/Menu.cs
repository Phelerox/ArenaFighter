using System;
using System.Collections.Generic;
using System.Linq;

using ArenaFighter.Views.ConsoleApplicationBase;

namespace ArenaFighter.Views {
    public class Menu<T> {
        private string question;
        //private Dictionary<string, Tuple<string, T>> options;
        private MenuOptions<T> options;
        private bool simpleSelectors = true;

        public Menu(string question, MenuOptions<T> options) {
            Initialize(question, options);
        }

        public Menu(string question, Dictionary<string, Tuple<string, T>> options) {
            Initialize(question, new MenuOptions<T>(question, options));
        }

        private void Initialize(string description, MenuOptions<T> options) {
            this.question = description;
            this.options = options;
            foreach (string selector in options.Keys) {
                if (selector.Length == 0) {
                    throw new ArgumentException("An option has an empty selector!");
                } else if (selector.Length > 1) {
                    simpleSelectors = false;
                }
            }
        }

        public T Ask(string tempDescription = "") {
            if (tempDescription == "") {
                ConsoleGame.WriteCenteredLines(question, null, "-|", "|-", separator: "");
                Console.WriteLine(question);
            } else {
                Console.WriteLine(tempDescription);
            }
            if (simpleSelectors) {
                return AskWithSimpleSelectors();
            } else {
                return AskWithStringSelectors();
            }
        }

        private T AskWithSimpleSelectors() {
            Console.WriteLine("");
            foreach (string selector in options.Keys) {
                ConsoleGame.WriteCenteredLines($"{selector}) {options[selector].Item1}", null);
            }
            int invalidChoices = 0;
            Console.Write("> ");
            string secret = "hidden";
            int secretProgress = 0;
            while (true) {
                string chosenKey = Console.ReadKey().KeyChar.ToString();
                if (chosenKey[0].Equals(secret[secretProgress])) {
                    ClearCurrentConsoleLine();
                    if (invalidChoices > 0) {
                        Console.Write($"You have made {++invalidChoices} invalid choices. Try again> ");
                    } else {
                        Console.Write("> ");
                    }
                    secretProgress++;
                    if (secretProgress == secret.Length) {
                        secretProgress = 0;
                        ConsoleApplicationPrompt.Run();
                    }
                } else if (options.ContainsKey(chosenKey)) {
                    Tuple<string, T> choice = options[chosenKey];
                    Console.WriteLine();
                    return choice.Item2;
                } else {
                    secretProgress = 0;
                    ClearCurrentConsoleLine();
                    Console.Write($"You have made {++invalidChoices} invalid choices. Try again> ");
                    continue;
                }
            }
        }

        public static void ClearCurrentConsoleLine() {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        private T AskWithStringSelectors() {
            throw new Exception();
            //return null;
        }
    }

    public class MenuOptions<T> {
        private string question;
        private IDictionary<string, Tuple<string, T>> backingDictionary = new Dictionary<string, Tuple<string, T>>();

        public Tuple<string, T> this[string key] {
            get => backingDictionary[key];
            set => backingDictionary[key] = value;
        }
        public IEnumerable<string> Keys { get { return backingDictionary.Keys; } }

        public bool ContainsKey(string key) {
            return backingDictionary.ContainsKey(key);
        }

        public MenuOptions(string question) {
            this.question = question;

        }

        public MenuOptions(string description, Dictionary<string, Tuple<string, T>> dict) : this(description) {
            backingDictionary = dict;
        }

        public MenuOptions<T> AddOption(string optionSelector, string optionDescription, T optionValue) {
            if (backingDictionary.ContainsKey(optionSelector))throw new Exception("optionSelector already exists!");
            backingDictionary[optionSelector] = Tuple.Create<string, T>(optionDescription, optionValue);
            return this;
        }

        public Menu<T> MenuFactory() {
            return new Menu<T>(question, this);
        }

    }

}
