using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArenaFighter.Models.Utils;

namespace ArenaFighter.Models {

    public class Attack {
        public BaseCharacter attacker;
        public BaseCharacter defender;
        public bool? advantage;
        public bool criticalHit, criticalMiss, hit;
        public int attackRoll, damageRoll, defenderArmorClass, defenderInitialHealth;
        public int damageDealt = 0;
        private string description = "";

        public Attack(BattleStatistics attackerStats, BaseCharacter defender, bool? advantage) {
            this.attacker = attackerStats.Character;
            this.defender = defender;
            this.advantage = advantage;
            defenderArmorClass = defender.ArmorClass;
            defenderInitialHealth = defender.CurHitPoints;
            attackRoll = attacker.AttackRoll(advantage: advantage);
            criticalHit = attackRoll == Int32.MaxValue;
            criticalMiss = attackRoll == Int32.MinValue;
            hit = (criticalHit || attackRoll >= defenderArmorClass) && !criticalMiss;
            if (hit) {
                damageRoll = attacker.DamageRoll(critical: criticalHit);
                damageDealt = defender.ReceiveDamage(damageRoll);
            }
            attackerStats.ReportAttackResults(hit, criticalHit, criticalMiss, damageDealt);
            ToString();
        }

        public override string ToString() {
            if (this.description != "")return this.description;
            string attackDescription = "", damageDescription, healthDescription, rollDescription = "";
            healthDescription = defender.CurHitPoints > 0 ? $", and now has {defender.CurHitPoints} Hit Points left." : $", which kills {Language.ObjectPronoun(defender.Gender)}!";
            damageDescription = hit ? $"---> {defender.Name} takes {damageDealt} damage{healthDescription}" + (damageDealt != damageRoll ? $" (damage reduced by {damageRoll-damageDealt})" : "") + "" : "";
            if (advantage != null) {
                attackDescription = (bool)advantage ? "Sensing an opportunity, " : "Trembling a bit, ";
            }
            if (criticalHit) {
                attackDescription += Language.PickRandomString(new string[] { $"{attacker.Name} impresses with {Language.PossessiveAdjective(attacker.Gender)} quick moves!" });
            } else if (criticalMiss) {
                attackDescription += Language.PickRandomString(new string[] { $"{attacker.Name} charges forward, but trips and falls over!" });
            } else if (attackRoll >= defender.ArmorClass + 8) {
                attackDescription += Language.PickRandomString(new string[] { $"{attacker.Name} makes it look so easy when {Language.SubjectPronoun(attacker.Gender)} pulls off a stunningly precise strike." });
            } else if (hit) {
                attackDescription += Language.PickRandomString(new string[] { $"{attacker.Name} charges forward with deadly intent, catching {defender.Name} with {Language.PossessiveAdjective(attacker.Gender)} {attacker.Weapon.Name}." });
            } else if (attackRoll >= defender.ArmorClass - 3) {
                attackDescription += Language.PickRandomString(new string[] { $"{attacker.Name} directs a powerful strike towards {defender.Name} that just barely misses." });
            } else {
                attackDescription += Language.PickRandomString(new string[] { $"{attacker.Name} takes a swing looking really focused but {Language.SubjectPronoun(attacker.Gender)} ends up way off the mark." });
            }
            if (Program.Debugging) {
                string adv = "";
                if (advantage != null) {
                    adv = " [w/ " + ((bool)advantage ? "advantage" : "disadvantage") + "]";
                }
                rollDescription = $" ({attackRoll} attack{adv} vs {defenderArmorClass} AC)";
            }

            description = $"{attackDescription}{rollDescription}\n{damageDescription}";

            return description;
        }
    }

    public class CombatTurn {
        public readonly BaseCharacter turnOwner;
        private string description = "";
        public List<Attack> attemptedAttacks = new List<Attack>();
        public CombatTurn(BattleStatistics attacker, BaseCharacter defender, bool? advantage) {
            turnOwner = attacker.Character;
            for (int a = 0; a <= turnOwner.ExtraAttacks; a++) {
                attemptedAttacks.Add(new Attack(attacker, defender, advantage));
                if (defender.CurHitPoints <= 0) {
                    break;
                }
            }
            ToString();
        }

        public override string ToString() {
            if (this.description != "")return this.description;
            foreach (Attack a in attemptedAttacks) {
                description += a.ToString();
            }
            return description;
        }
    }

    public class Round { //A round is composed of the combatants individual CombatTurns.
        public int RoundNumber { get; set; }
        private BaseCharacter combatant, opponent;
        private List<CombatTurn> turns = new List<CombatTurn>();
        private string description = "";
        private string roundStartDescription = "";

        public Tuple<BaseCharacter, BaseCharacter> BattleOver {
            get {
                var winner = (combatant.CurHitPoints <= 0 ? opponent : (opponent.CurHitPoints <= 0 ? combatant : null));
                return Tuple.Create<BaseCharacter, BaseCharacter>(winner, OtherCharacter(winner));
            }
        }
        public BaseCharacter OtherCharacter(BaseCharacter character) {
            return character != null ? (combatant.Equals(character) ? opponent : combatant) : null;
        }
        public BaseCharacter EarnedAdvantage { get; set; } = null;

        public Dictionary<BaseCharacter, BattleStatistics> statistics = new Dictionary<BaseCharacter, BattleStatistics>();

        public Round(int roundNumber, BattleStatistics combatantStats, BattleStatistics opponentStats, bool? combatantAdvantage = null, bool? opponentAdvantage = null) {
            this.combatant = combatantStats.Character;
            this.opponent = opponentStats.Character;
            this.RoundNumber = roundNumber;
            statistics[combatant] = combatantStats;
            statistics[opponent] = opponentStats;
            roundStartDescription += '\n' + $"\u200BRound {RoundNumber}! {combatant.Name} (HP: {combatant.CurHitPoints}/{combatant.MaxHitPoints}) vs {opponent.Name} (HP: {opponent.CurHitPoints}/{opponent.MaxHitPoints})\n";
            turns.Add(new CombatTurn(statistics[combatant], opponent, combatantAdvantage));
            if (BattleOver.Item1 == null) {
                bool otherOneMessedUp = turns.Last().attemptedAttacks.Last().criticalMiss;
                turns.Add(new CombatTurn(statistics[opponent], combatant, otherOneMessedUp ? (bool?)true : opponentAdvantage));
            }
            if (turns.Last().attemptedAttacks.Last().criticalMiss) {
                EarnedAdvantage = combatant;
            }
            ToString();
        }

        public override string ToString() {
            if (this.description != "")return this.description;

            foreach (CombatTurn t in turns) {
                description += t.ToString() + "\n";
            }
            description = roundStartDescription + description;
            return description;
        }
    }
}
