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
        private BaseCharacter combatant;
        private BaseCharacter opponent;
        private double estimatedPowerDifference;
        public double EstimatedRelativePower {
            get { return estimatedPowerDifference; }
        }

        public Battle(BaseCharacter combatant, BaseCharacter opponent, bool startBattle = true) {
            this.combatant = combatant;
            this.opponent = opponent;
            estimatedPowerDifference = combatant.CalculateRelativePower(opponent);

            if (startBattle) {
                StartBattle();
            }

        }

        public Battle StartBattle() {
            while (!NextRound().BattleOver) {

            }
            return this;
        }

        public bool? BetweenRoundsMovementOutcome(BaseCharacter character) {
            int roll = DiceRoller.TwentySidedDie();
            if (roll == 20) {
                return true; //Outmaneuver! Advantage on next attack
            } else if (roll == 1) {
                return false; //Stumbled! Disadvantage on next attack
            } else {
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
                Round round = new Round(combatant, opponent, combatantAdvantage : BetweenRoundsMovementOutcome(combatant), opponentAdvantage : BetweenRoundsMovementOutcome(opponent));
                rounds.Add(round);
                return round;
            } else {
                return null;
            }
        }

        public override string ToString() {
            return base.ToString();
        }

    }
}
