using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ArenaFighter.Models;
using ArenaFighter.Models.Utils;

namespace ArenaFighter.Models {
    public class Campaign : ICloneable {
        public ulong DaysPassed { get; }
        protected List<Battle> battles = new List<Battle>();
        protected IDictionary<long, BaseCharacter> gladiators = new Dictionary<long, BaseCharacter>();
        protected IDictionary<long, BaseCharacter> deceasedGladiators = new Dictionary<long, BaseCharacter>();
        public IDictionary<long, BaseCharacter> Gladiators { get { return new Dictionary<long, BaseCharacter>(gladiators); } }
        public BaseCharacter NextOpponent { get; set; } = null;

        public Campaign(string playerName, Genders gender, Race race) {
            Player = new Player(playerName, gender, race);
            gladiators[Player.id] = Player;

        }

        public BaseCharacter GenerateNextOpponent() {
            int randomizations = 0;
            do {
                NextOpponent = new Novice();
                randomizations++;
            } while (Math.Abs(Player.CalculateRelativePower(NextOpponent)) > 0.4 && randomizations < 1000);

            if (Program.Debugging) {
                Console.WriteLine(NextOpponent);
                Console.WriteLine($"\t\t\tRandomized {randomizations} times! Power difference in your favor: {Player.CalculateRelativePower(NextOpponent)*100}%");
            }
            return NextOpponent;
        }
        public string NextArenaEvent(bool healUpFirst = false) {
            if (NextOpponent == null) {
                GenerateNextOpponent();
            }
            gladiators[NextOpponent.id] = NextOpponent;
            if (healUpFirst)Player.CurHitPoints = Player.MaxHitPoints;
            Battle b = new Battle(Player, NextOpponent);
            battles.Add(b);
            if (NextOpponent.StillStanding(b)) {

            } else {
                gladiators.Remove(NextOpponent.id);
                deceasedGladiators[NextOpponent.id] = NextOpponent;
            }
            NextOpponent = null;

            if (Player.StillStanding(b)) {

            } else {
                string savingThrows = Player.DeathSavingThrows();
                if (!Player.IsAlive) {
                    gladiators.Remove(Player.id);
                    deceasedGladiators[Player.id] = Player;
                }
                return savingThrows;
            }

            return null;
        }
        public List<Battle> PastBattles {
            get { return battles; }
        }

        public Player Player { get; set; }

        public long RetirePlayer() {
            return (long)(Player.Score * (Player.IsAlive ? 1.5 : 1));
        }

        public object Clone() {
            return this.MemberwiseClone();
        }
    }
}
