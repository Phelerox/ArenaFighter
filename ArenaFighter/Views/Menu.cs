using System;
using System.Collections.Generic;

namespace ArenaFighter.Views {
    public class Menu<T> {
        private string description;
        private Dictionary<string, Tuple<string, T>> options;
        private bool simpleSelectors = true;

        public Menu(string description, Dictionary<string, Tuple<string, T>> options) {
            this.description = description;
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
                Console.WriteLine(description);
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
                Console.WriteLine($"\t\t {selector}) {options[selector].Item1}");
            }
            int invalidChoices = 0;
            Console.Write("> ");
            while (true) {
                string chosenKey = Console.ReadKey().KeyChar.ToString();
                if (options.ContainsKey(chosenKey)) {
                    Tuple<string, T> choice = options[chosenKey];
                    Console.WriteLine();
                    return choice.Item2;
                } else {
                    ClearCurrentConsoleLine();
                    Console.Write($"You have made {++invalidChoices} invalid choices. Try again> ");
                    continue;
                }
            }
            //throw new Exception();
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
}
