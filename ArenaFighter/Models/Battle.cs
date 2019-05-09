using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArenaFighter.Models.Utils;

namespace ArenaFighter.Models {
    public class Battle {
        public List<Round> rounds = new List<Round>();
        public List<string> log = new List<string>();
        private string description;
        private BaseCharacter combatant;
        private BaseCharacter opponent;
        private BattleStatistics combatantStatistics;
        private BattleStatistics opponentStatistics;
        public BattleStatistics CombatantStatistics { get { return (BattleStatistics)combatantStatistics.Clone(); } }
        private double estimatedPowerDifference;
        public double EstimatedRelativePower {
            get { return estimatedPowerDifference; }
        }

        public Battle(BaseCharacter combatant, BaseCharacter opponent, bool startBattle = true) {
            this.combatant = combatant;
            this.opponent = opponent;
            combatantStatistics = new BattleStatistics(combatant, opponent);
            opponentStatistics = new BattleStatistics(opponent, combatant);
            estimatedPowerDifference = combatant.CalculateRelativePower(opponent);

            if (startBattle) {
                StartBattle();
            }

        }

        public Battle StartBattle() {
            while (!NextRound().BattleOver) {

            }
            BattleOver();
            return this;
        }

        public bool? BetweenRoundsMovementOutcome(BaseCharacter character) {
            int roll = DiceRoller.TwentySidedDie();
            if (roll == 20) {
                return true; //Outmaneuver! Advantage on next attack
            } else if (roll == 1) {
                return false; //Stumbled! Disadvantage on next attack
            } else {
                if (rounds.Last().EarnedAdvantage == character)
                    return true; //The enemy stumbled at the end of last round (Critical Miss)
                return null; //no significant outcome
            }
        }

        public Round ContinueBattle() {
            Round round = NextRound();
            if (round != null) {
                return round;
            } else {
                throw new ApplicationException("The dead cannot fight.");
            }
        }

        private Round NextRound() {
            if (combatant.CurHitPoints > 0 && opponent.CurHitPoints > 0) {
                Round round = new Round(combatantStatistics, opponentStatistics, combatantAdvantage : BetweenRoundsMovementOutcome(combatant), opponentAdvantage : BetweenRoundsMovementOutcome(opponent));
                rounds.Add(round);
                log.Add(round.ToString());
                return round;
            } else {
                return null;
            }
        }

        private void BattleOver() {
            combatantStatistics.BattleOver();
            opponentStatistics.BattleOver();
        }

        public override string ToString() {
            if (this.description != "")return this.description;

            return description;
        }

    }

    public class BattleStatistics : ICloneable {
        public BaseCharacter Character { get; set; }
        private string description = "";
        private int enemyAC = 0;
        private int hits = 0;
        private int misses = 0;
        private int criticalHits = 0;
        private int criticalMisses = 0;
        private int totalDamage = 0;
        private int criticalDamage = 0;

        public int AverageDamage {
            get { return (totalDamage) / (hits); }
        }
        public double ExpectedAverageDamage { get; set; }
        public double Accuracy {
            get { return hits / (hits + misses); }
        }
        public double ExpectedAccuracy { get; set; }

        public int CriticalHits {
            get { return criticalHits; }
        }
        public int CriticalMisses {
            get { return criticalMisses; }
        }
        public int Hits {
            get { return hits; }
        }
        public int Misses {
            get { return misses; }
        }
        public int TotalDamage {
            get { return totalDamage; }
        }
        public double ExpectedTotalDamage { get; set; }
        public int CriticalDamage {
            get { return criticalDamage; }
        }
        public double ExpectedTotalCriticalDamage {
            get { return ExpectedCrits * (ExpectedAverageDamage + DiceRoller.averageRoll[Character.Weapon.DamageDie]); }
        }
        public double ExpectedCrits { get { return (hits + misses) * 0.05; } }

        public BattleStatistics(BaseCharacter character, BaseCharacter enemy) {
            Character = character;
            enemyAC = enemy.ArmorClass;
            ExpectedAccuracy = Math.Min(Math.Max((21 - (enemyAC - (character.AttackRoll(rollMax: true) - 20))) / 20.0, 0.05), 0.95);
            ExpectedAverageDamage = enemy.ReceiveDamage(character.DamageRoll(rollMax: true) - character.Weapon.DamageDie(true) + DiceRoller.averageRoll[character.Weapon.DamageDie], character.Weapon.DamageType);
        }

        public void ReportAttackResults(bool hit, bool criticalHit, bool criticalMiss, int damageDealt = 0) {
            if (hit)hits++;
            else misses++;
            if (criticalHit) {
                criticalHits++;
                criticalDamage = damageDealt;
            }
            if (criticalMiss)criticalMisses++;
            totalDamage += damageDealt;
        }

        public void BattleOver() {
            ExpectedTotalDamage = ExpectedAccuracy * ExpectedAverageDamage * (hits + misses);
            ExpectedAverageDamage = (ExpectedAverageDamage * (Hits - CriticalHits) + (CriticalHits * (ExpectedAverageDamage + DiceRoller.averageRoll[Character.Weapon.DamageDie]))) / hits;
        }

        public object Clone() {
            return this.MemberwiseClone();
        }

        public override string ToString() {
            if (description != "")return description;
            description += $"Accuracy: {Accuracy*100} ({hits}/{hits+misses}) | Expected: {ExpectedAccuracy*100} | Critical Misses: {CriticalMisses} ({ExpectedCrits} expected)\n";
            description += $"Total Damage: {TotalDamage} | {AverageDamage} average damage per hit | Expected: {ExpectedTotalDamage} ({ExpectedAverageDamage} average damage per hit)\n";
            description += $"Critical Hits: {CriticalHits} ({ExpectedCrits} expected) | Critical Damage: {CriticalDamage} ({ExpectedTotalCriticalDamage} expected with {ExpectedCrits} critical hits)\n";
            return description;
        }
    }
}
