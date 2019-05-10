using System.Linq;

using ArenaFighter.Views;

namespace ArenaFighter {
    public class Program {
        public static bool Debugging = false;
        public static bool DebuggingRandomEquipmentGeneration = false;
        public static void Main(string[] args) {
            if (args.Contains("-v weighttables"))DebuggingRandomEquipmentGeneration = true;
            else if (args.Contains("-v"))Debugging = true;
            new ConsoleGame();
        }
    }
}
