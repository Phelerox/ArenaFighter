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
        public int attackRoll, damageRoll;

        public Attack(BaseCharacter attacker, BaseCharacter defender, bool? advantage) {
            this.attacker = attacker;
            this.defender = defender;
            this.advantage = advantage;
            attackRoll = attacker.AttackRoll(advantage: advantage);
            criticalHit = attackRoll == 20;
            criticalMiss = attackRoll == 1;
            hit = (criticalHit || attackRoll >= defender.ArmorClass) && !criticalMiss;
            if (hit) {
                damageRoll = attacker.DamageRoll(critical: criticalHit);
                defender.ReceiveDamage(damageRoll);
            }

        }
    }

    public class CombatTurn {
        private string description;
        private List<Attack> attemptedAttacks = new List<Attack>();
        public CombatTurn(BaseCharacter attacker, BaseCharacter defender, bool? advantage) {

        }

        public override string ToString() {
            return base.ToString();
        }
    }

    public class Round { //A round is composed of the combatants individual CombatTurns.
        private BaseCharacter combatant, opponent;
        private List<CombatTurn> turns = new List<CombatTurn>();

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

        public int CombatantInitiative { get; set; }
        public int OpponentInitiative { get; set; }

        public Round(BaseCharacter combatant, BaseCharacter opponent, bool? combatantAdvantage = null, bool? opponentAdvantage = null) {
            this.combatant = combatant;
            this.opponent = opponent;
            CombatantInitiative = combatant.Initiative + DiceRoller.TwentySidedDie();
            OpponentInitiative = opponent.Initiative + DiceRoller.TwentySidedDie();
            //Initiative
            turns.Add(new CombatTurn(StartingCharacter, CharacterGoingLast, CombatantStarts ? combatantAdvantage : opponentAdvantage));
            if (!BattleOver) {
                turns.Add(new CombatTurn(CharacterGoingLast, StartingCharacter, CombatantStarts ? opponentAdvantage : combatantAdvantage));
            }
        }

        public override string ToString() {
            return base.ToString();
        }
    }
}
