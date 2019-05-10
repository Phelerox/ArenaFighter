using System.Linq;
//using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;

using ArenaFighter.Models;
using ArenaFighter.Models.Utils;
using ArenaFighter.Views;

namespace ArenaFighter.Presenters {
    public enum GameStage {
        CharacterCreation,
        PrivateQuarters, //CharacterInfo, Shop/Equipment, Training?, Rest, Pay Doctor
        MedicalWard,
        PreBattle, //OpponentSelection, Gambling, Tactics
        Combat,
        CombatVictory,
        CombatDefeat, //Beg for mercy? - Chance of ending up in Medical Ward instead of dying?
        LevelUp, //Randomised minor stat gain? Perk every 4 levels?
        GameOver
    }

    public sealed class Presenter {
        private static readonly Lazy<Presenter> lazy = new Lazy<Presenter>(() => new Presenter());
        private IView view = null;
        public IView View { set { view = (view == null ? value : view); } }

        public Reflection reflection;

        public GameStage CurrentStage { get; }

        private IDictionary<dynamic, bool> nextRequiredActions = new Dictionary<dynamic, bool>();
        private Player MutablePlayer { get { return (Player)campaign.Player; } }
        public Player Player { get { return (Player)campaign.Player.Clone(); } }

        private Dictionary<GameStage, List<dynamic>> validActionsAtStage;

        private Campaign campaign = null;
        public Campaign Campaign { get { return (Campaign)campaign.Clone(); } }

        public static Presenter Instance { get { return lazy.Value; } }

        private Presenter() {
            validActionsAtStage = new Dictionary<GameStage, List<dynamic>>() {
                [GameStage.CharacterCreation] = new List<dynamic>() {
                (Func<string, Genders, Race, bool>)CreateCharacterAction, (Func<bool>)RerollCharacterAction
                }
            };
            reflection = Reflection.Instance();
        }

        public IEnumerable<dynamic> ValidActions() {
            List<dynamic> actions = validActionsAtStage[CurrentStage];
            return actions;
        }

        public bool CreateCharacterAction(string name, Genders gender, Race race) {
            if (ValidActions().Contains((Func<string, Genders, Race, bool>)CreateCharacterAction)) {
                campaign = new Campaign(name, gender, race);
                return true;
            } else {
                return false;
            }
        }

        public bool RerollCharacterAction() {
            if (ValidActions().Contains((Func<bool>)RerollCharacterAction)) {
                MutablePlayer.GenerateAttributes();
                return true;
            } else {
                return false;
            }
        }

        public BaseCharacter NextOpponent { get { return campaign.GenerateNextOpponent(); } }

        public string NextArenaEvent() {
            return campaign.NextArenaEvent(true); //cheat to restore health until function for resting is fully implemented
        }

        public void AdvanceAction() {
            if (CurrentStage == GameStage.CharacterCreation) {

            }

        }

        public ulong DaysPassed() {
            return campaign.DaysPassed;
        }

        public bool GameOver { get { return (!Player.IsAlive) && MutablePlayer.CurHitPoints <= 0; } }

        public String RetireCampaignAction() { //always available even if not in ValidActions()
            return $"Score: {campaign.RetirePlayer()}";
        }

        public IList<Tuple<BaseCharacter, Player, string>> PastBattles {
            get {
                var logs = new List<Tuple<BaseCharacter, Player, string>>();
                campaign.PastBattles.ForEach((b) => logs.Add(Tuple.Create<BaseCharacter, Player, string>(b.OpponentAtStart, (Player)b.CombatantAtStart, string.Concat(b.Log))));
                return logs;
            }
        }

    }
}
