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
        public int attackRoll, damageRoll, defenderArmorClass;
        public int damageDealt = 0;
        private string description = "";

        public Attack(BattleStatistics attackerStats, BaseCharacter defender, bool? advantage) {
            this.attacker = attackerStats.Character;
            this.defender = defender;
            this.advantage = advantage;
            defenderArmorClass = defender.ArmorClass;
            attackRoll = attacker.AttackRoll(advantage: advantage);
            criticalHit = attackRoll == Int32.MaxValue;
            criticalMiss = attackRoll == Int32.MinValue;
            hit = (criticalHit || attackRoll >= defenderArmorClass) && !criticalMiss;
            if (hit) {
                damageRoll = attacker.DamageRoll(critical: criticalHit);
                damageDealt = defender.ReceiveDamage(damageRoll);
            }
            attackerStats.ReportAttackResults(hit, criticalHit, criticalMiss, damageDealt);
        }

        public override string ToString() {
            if (this.description != "")return this.description;
            string attackDescription, damageDescription, rollDescription = "";
            damageDescription = hit ? $"{defender.Name} takes {damageDealt} damage" + (damageDealt != damageRoll ? $" (damage reduced by {damageRoll-damageDealt})" : "") + "." : "";
            if (criticalHit) {
                attackDescription = Language.PickRandomString(new string[] { $"{attacker.Name} impresses with {Language.PossessiveAdjective(attacker.Gender)} quick moves!" });
            } else if (criticalMiss) {
                attackDescription = Language.PickRandomString(new string[] { $"{attacker.Name} charges forward, but trips and falls over!" });
            } else if (attackRoll >= defender.ArmorClass + 8) {
                attackDescription = Language.PickRandomString(new string[] { $"{attacker.Name} makes it look so easy when {Language.SubjectPronoun(attacker.Gender)} pulls off a stunningly precise strike." });
            } else if (hit) {
                attackDescription = Language.PickRandomString(new string[] { $"{attacker.Name} charges forward with deadly intent, catching {defender.Name} with {Language.PossessiveAdjective(attacker.Gender)} {attacker.Weapon}." });
            } else if (attackRoll >= defender.ArmorClass - 3) {
                attackDescription = Language.PickRandomString(new string[] { $"{attacker.Name} directs a powerful strike towards {defender.Name} that just barely misses." });
            } else {
                attackDescription = Language.PickRandomString(new string[] { $"{attacker.Name} takes a swing looking really focused but {Language.PossessiveAdjective(attacker.Gender)} ends up way off the mark." });
            }
            if (Program.Debugging) {
                string adv = "";
                if (advantage != null) {
                    adv = " [w/ " + ((bool)advantage ? "advantage" : "disadvantage") + "]";
                }
                rollDescription = $" ({attackRoll} attack{adv} vs {defenderArmorClass} AC)";
            }

            description = attackDescription + $" {attackDescription}{rollDescription}\n{damageDescription} ";

            return description;
        }
    }

    public class CombatTurn {
        private BaseCharacter turnOwner;
        private string description = "";
        public List<Attack> attemptedAttacks = new List<Attack>();
        public CombatTurn(BattleStatistics attacker, BaseCharacter defender, bool? advantage) {
            turnOwner = attacker.Character;
            for (int a = 0; a <= turnOwner.ExtraAttacks; a++) {
                attemptedAttacks.Add(new Attack(attacker, defender, advantage));
            }
        }

        public override string ToString() {
            if (this.description != "")return this.description;
            description = $"{turnOwner.Name}'s turn!\n";
            foreach (Attack a in attemptedAttacks) {
                description += a.ToString() + "\n";
            }
            return description;
        }
    }

    public class Round { //A round is composed of the combatants individual CombatTurns.
        private BaseCharacter combatant, opponent;
        private List<CombatTurn> turns = new List<CombatTurn>();
        private string description = "";

        public bool BattleOver {
            get { return combatant.CurHitPoints <= 0 || opponent.CurHitPoints <= 0; }
        }
        public BaseCharacter StartingCharacter {
            get { return CombatantInitiative > OpponentInitiative ? combatant : (CombatantInitiative < OpponentInitiative ? opponent : (combatant.Initiative >= opponent.Initiative ? combatant : opponent)); }
        }
        public BaseCharacter CharacterGoingLast {
            get { return StartingCharacter == combatant ? opponent : combatant; }
        }
        public bool CombatantStarts {
            get { return StartingCharacter == combatant; }
        }
        public BaseCharacter EarnedAdvantage { get; set; } = null;

        public int CombatantInitiative { get; set; }
        public int OpponentInitiative { get; set; }
        public Dictionary<BaseCharacter, BattleStatistics> statistics = new Dictionary<BaseCharacter, BattleStatistics>();

        public Round(BattleStatistics combatantStats, BattleStatistics opponentStats, bool? combatantAdvantage = null, bool? opponentAdvantage = null) {
            this.combatant = combatantStats.Character;
            this.opponent = opponentStats.Character;
            statistics[combatant] = combatantStats;
            statistics[opponent] = opponentStats;
            CombatantInitiative = this.combatant.Initiative + DiceRoller.TwentySidedDie();
            OpponentInitiative = this.opponent.Initiative + DiceRoller.TwentySidedDie();
            //Initiative
            turns.Add(new CombatTurn(statistics[StartingCharacter], CharacterGoingLast, CombatantStarts ? combatantAdvantage : opponentAdvantage));
            if (!BattleOver) {
                bool otherOneMessedUp = turns.Last().attemptedAttacks.Last().criticalMiss;
                turns.Add(new CombatTurn(statistics[CharacterGoingLast], StartingCharacter, otherOneMessedUp ? (bool?)true : (CombatantStarts ? opponentAdvantage : combatantAdvantage)));
            }
            if (turns.Last().attemptedAttacks.Last().criticalMiss) {
                EarnedAdvantage = StartingCharacter;
            }
        }

        public override string ToString() {
            if (this.description != "")return this.description;
            description += $"New Round! {StartingCharacter.Name} gets to go first.\n";
            foreach (CombatTurn t in turns) {
                description += t.ToString() + "\n";
            }
            return description;
        }
    }
}
