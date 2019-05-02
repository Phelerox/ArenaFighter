//using System.Runtime.CompilerServices;
using System;

using ArenaFighter.Model;
using ArenaFighter.Model.Util;
using ArenaFighter.View;


namespace ArenaFighter.Presenter
{
    public enum GameMenus {
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

    public sealed class Presenter
    {
        private static readonly Lazy<Presenter> lazy = new Lazy<Presenter>(() => new Presenter());

        private static IView View = null;

        public GameMenus State { get; }

        private Campaign campaign = new Campaign();

        public static Presenter Instance(IView v)
        {
            View = v;
            return lazy.Value;
        }

        private Presenter()
        {
            campaign = new Campaign();
            
        }

    }
}