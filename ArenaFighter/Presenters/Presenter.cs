using System.Linq;
//using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;


using ArenaFighter.Models;
using ArenaFighter.Models.Utils;
using ArenaFighter.Views;


namespace ArenaFighter.Presenters
{
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

    public enum Action {
        CreateCharacter,
        RetireCampaign
    }

    public sealed class Presenter
    {
        private static readonly Lazy<Presenter> lazy = new Lazy<Presenter>(() => new Presenter());

        private static IView View = null;

        public Reflection reflection;

        public GameStage CurrentStage { get; }

        private Action nextRequiredAction = Action.CreateCharacter;

        private Dictionary<GameStage, List<Action>> validActionsAtStage = new Dictionary<GameStage, List<Action>>()
        {
            [GameStage.CharacterCreation] = new List<Action>() {Action.CreateCharacter}
        };

        private Campaign campaign = null;

        public static Presenter Instance(IView v)
        {
            View = v;
            return lazy.Value;
        }

        private Presenter()
        {
            reflection = Reflection.Instance();
        }

        public IEnumerable<Action> ValidActions() {
            List<Action> actions = validActionsAtStage[CurrentStage];
            return actions;
        }

        public Player CreateCharacter(string name, Genders gender, Race race) {
            if (ValidActions().Contains(Action.CreateCharacter)) {
                campaign = new Campaign(name, gender, race);
                return campaign.Player;
            }
            else {
                return null;
            }
        }

        public ulong DaysPassed() {
            return campaign.DaysPassed;
        }

        public String RetireCampaign() { //always available even if not in ValidActions()
            return "Score: 0";
        }

    }
}