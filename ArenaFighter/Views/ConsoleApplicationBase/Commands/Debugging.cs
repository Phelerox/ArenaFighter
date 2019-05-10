using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArenaFighter.Models;
using ArenaFighter.Models.Utils;
using ArenaFighter.Presenters;

namespace ArenaFighter.Views.ConsoleApplicationBase.Commands {
    public static class Debugging {
        public static string ListGladiators() {
            string gladiators = "";
            foreach (BaseCharacter c in Presenter.Instance.Campaign.Gladiators.Values) {
                gladiators += $"ID: {c.id} ({c.Name})\n" + c;
            }
            return Language.PadLinesLeft(gladiators, 3);
        }

        public static string ListDeceasedGladiators() {
            string gladiators = "";
            foreach (BaseCharacter c in Presenter.Instance.Campaign.Gladiators.Values) {
                gladiators += $"ID: {c.id} ({c.Name})\n" + c;
            }
            return Language.PadLinesLeft(gladiators, 3);
        }

        private static BaseCharacter GetCharacterClone(long id) {
            return (BaseCharacter)Presenter.Instance.Campaign.Gladiators[id].Clone();
        }

        public static string SimulateBattlesBetween(long enemyId = -1, long combatantId = 0, int simulations = 1000) {
            BaseCharacter combatant = GetCharacterClone(combatantId);
            BaseCharacter enemy = enemyId > 0 ? GetCharacterClone(combatantId) : (BaseCharacter)MeasureStickJoe.Instance.Clone();
            int wins = 0;
            int losses = 0;
            double percentWon;
            double averageHealthLeft = 0;
            double enemyAverageHealthLeft = 0;
            int initialHitPoints = combatant.CurHitPoints;
            int enemyInitialHitPoints = enemy.CurHitPoints;
            string initialName = combatant.Name;
            string enemyInitialName = enemy.Name;
            for (int i = 0; i < simulations; i++) {
                combatant.DamageTaken = 0;

                enemy.DamageTaken = 0;

                Battle simulation = new Battle(combatant, enemy);
                if (combatant.Equals(simulation.Winner)) {
                    wins++;
                    averageHealthLeft += simulation.CombatantStatistics.Character.CurHitPoints;
                } else if (enemy.Equals(simulation.Winner)) {
                    losses++;
                    enemyAverageHealthLeft += simulation.OpponentStatistics.Character.CurHitPoints;
                } else {
                    throw new Exception("Unknown winner!");
                }
                combatant.Name = initialName;
                enemy.Name = enemyInitialName;
            }
            combatant.CurHitPoints = initialHitPoints;
            enemy.CurHitPoints = enemyInitialHitPoints;
            averageHealthLeft /= wins;
            enemyAverageHealthLeft /= losses;
            percentWon = wins / ((double)wins + losses);
            return $"{percentWon*100}% winrate | AverageHealthLeft: {averageHealthLeft} | EnemyAverageHealthLeft: {enemyAverageHealthLeft}";
        }
    }
}
