using System;

using Humanizer;

using ArenaFighter.Models.Utils;

namespace ArenaFighter.Models
{
    public enum Slot {
        MainHand,
        OffHand,
        Armor,
        Helmet,
    }

    public abstract class Equipment : Modifier
    {
        public Slot Slot{ get; set; }

        public abstract int GetModifierFor(Attribute attribute);

        public string ToString() {
            return this.GetType().Name.Humanize(LetterCasing.Title);
        }
    }

    public abstract class Weapon : Equipment {
        public abstract Func<int> DamageDie { get; }
        public bool TwoHanded { get; }
    }

    public class ShortSword : Weapon {
        private Slot slot = Slot.MainHand;
        public bool twoHanded = false;
        public override Func<int> DamageDie {
            get {
                return DiceRoller.SixSidedDie;
            }}

        public ShortSword() {
            Console.WriteLine(this.ToString());
            Console.WriteLine(DamageDie());
        }

        public override int GetModifierFor(Attribute attribute) {
            return 0;
        }
    }

    public class GreatAxe : Weapon {
        private Slot slot = Slot.MainHand;
        public bool twoHanded = true;
        public override Func<int> DamageDie {
            get {
                return DiceRoller.TwelveSidedDie;
            }}

        public override int GetModifierFor(Attribute attribute) {
            return 0;
        }

    }
}