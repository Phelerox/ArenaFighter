using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArenaFighter.Models.Utils;

namespace ArenaFighter.Models {
    public class Battle {
        public List<Round> Rounds { get; set; } = new List<Round>();
        public List<string> Log { get; set; } = new List<string>();
        private string description = "";
        private string startBattleDescription = "";
        private readonly BaseCharacter combatant;
        private readonly BaseCharacter opponent;
        public BaseCharacter CombatantAtStart { get; set; }
        public BaseCharacter OpponentAtStart { get; set; }
        private BaseCharacter loser;
        public BaseCharacter Loser { get { return loser; } }
        private BaseCharacter winner;
        public BaseCharacter Winner { get { return winner; } }
        private BattleStatistics startingCharacterStatistics;
        private BattleStatistics characterGoingLastStatistics;
        public BattleStatistics CombatantStatistics { get { return (BattleStatistics)startingCharacterStatistics.Clone(); } }
        public BattleStatistics OpponentStatistics { get { return (BattleStatistics)characterGoingLastStatistics.Clone(); } }
        private readonly double estimatedPowerDifference;
        public double EstimatedRelativePower {
            get { return estimatedPowerDifference; }
        }

        public BaseCharacter StartingCharacter {
            get { return CombatantInitiative > OpponentInitiative ? combatant : (CombatantInitiative < OpponentInitiative ? opponent : (combatant.Initiative >= opponent.Initiative ? combatant : opponent)); }
        }
        public BaseCharacter CharacterGoingLast {
            get { return StartingCharacter.Equals(combatant) ? opponent : combatant; }
        }
        public bool CombatantStarts {
            get { return combatant.Equals(StartingCharacter); }
        }
        public int CombatantInitiative { get; set; }
        public int OpponentInitiative { get; set; }
        public BaseCharacter OtherCharacter(BaseCharacter character) {
            return StartingCharacter.Equals(character) ? CharacterGoingLast : StartingCharacter;
        }

        public Battle(BaseCharacter combatant, BaseCharacter opponent) {
            this.combatant = combatant;
            this.CombatantAtStart = (BaseCharacter)combatant.Clone();
            this.opponent = opponent;
            this.OpponentAtStart = (BaseCharacter)opponent.Clone();
            CombatantInitiative = this.combatant.Initiative + DiceRoller.TwentySidedDie();
            OpponentInitiative = this.opponent.Initiative + DiceRoller.TwentySidedDie();
            startingCharacterStatistics = new BattleStatistics(StartingCharacter, CharacterGoingLast);
            characterGoingLastStatistics = new BattleStatistics(CharacterGoingLast, StartingCharacter);
            estimatedPowerDifference = combatant.CalculateRelativePower(opponent);
            if (combatant.CurHitPoints <= 0 || opponent.CurHitPoints <= 0) {
                throw new ArgumentException("Can't have a battle without at least two living combatants!");
            }
            StartBattle();
        }

        private Battle StartBattle() {
            startBattleDescription = $"\nBattle between {combatant.Name} and {opponent.Name} commencing! {StartingCharacter.Name} seems full of energy!\n";
            do {
                (winner, loser) = NextRound().BattleOver;
            } while (ReferenceEquals(winner, null));
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
                if (Rounds.Count > 0 && Rounds.Last().EarnedAdvantage == character)
                    return true; //The enemy stumbled at the end of last round (Critical Miss)
                return null; //no significant outcome
            }
        }

        private Round NextRound() {
            if (combatant.CurHitPoints > 0 && opponent.CurHitPoints > 0) {
                Round round = new Round(Rounds.Count + 1, startingCharacterStatistics, characterGoingLastStatistics, combatantAdvantage : BetweenRoundsMovementOutcome(StartingCharacter), opponentAdvantage : BetweenRoundsMovementOutcome(CharacterGoingLast));
                Rounds.Add(round);
                Log.Add(round.ToString());
                return round;
            } else {
                return null;
            }
        }

        private void BattleOver() {
            startingCharacterStatistics.BattleOver(Winner);
            characterGoingLastStatistics.BattleOver(Winner);
            combatant.StillStanding(this);
            opponent.StillStanding(this);
            ToString();
        }

        public override string ToString() {
            if (this.description != "")return this.description;
            description += startBattleDescription;
            foreach (Round r in Rounds) {
                description += r;
            }
            return description;
        }

    }

    public class BattleStatistics : ICloneable {
        public BaseCharacter Character { get; set; }
        public bool Won { get; set; }
        private string description = "";
        public readonly int enemyAC = 0;
        private int hits = 0;
        private int misses = 0;
        private int criticalHits = 0;
        private int criticalMisses = 0;
        private int totalDamage = 0;
        private int criticalDamage = 0;

        public int AverageDamage {
            get { return (hits) > 0 ? (totalDamage) / (hits) : 0; }
        }
        public double ExpectedAverageDamage { get; set; }
        public double Accuracy {
            get {
                return (hits + misses) > 0 ? hits / ((double)hits + misses) : 0;
            }
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
            ExpectedAverageDamage = enemy.ReceiveDamage(character.DamageRoll(rollMax: true) - character.Weapon.DamageDie(true) + DiceRoller.averageRoll[character.Weapon.DamageDie], character.Weapon.DamageType, justKidding : true);
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

        public void BattleOver(BaseCharacter winner) {
            ExpectedTotalDamage = ExpectedAccuracy * ExpectedAverageDamage * (hits + misses);
            ExpectedAverageDamage = (hits) > 0 ? (ExpectedAverageDamage * (Hits - CriticalHits) + (CriticalHits * (ExpectedAverageDamage + DiceRoller.averageRoll[Character.Weapon.DamageDie]))) / hits : 0;
            Won = Character.Equals(winner);
            Character = (BaseCharacter)Character.Clone();
            ToString();
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
