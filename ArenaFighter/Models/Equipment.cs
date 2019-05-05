using System;
using System.Collections.Generic;

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
        public abstract int Value { get; }
        public abstract Slot Slot{ get; }
    }

    public abstract class Armor : Equipment {
        public override Slot Slot { get { return Slot.Armor; }}
        public abstract int ArmorClass { get; }
        public abstract int MaxDexBonus { get; }

        public override string ToString() {
            return base.ToString() + $" | Armor Class: {ArmorClass} | Max Dexterity Bonus: {MaxDexBonus} | {Value} gp";
        }
    }

    public class Skin : Armor {
        public override int Value { get { return 0; } }
        public override int ArmorClass {
            get { return 10; }
        }
        public override int MaxDexBonus {
            get { return 6; }
        }
    }
    public abstract class LightArmor : Armor {
        public override int MaxDexBonus {
            get { return 8; }
        }
    }
    public class Leather : LightArmor {
        public override int Value { get { return 10*5; } }
        public override int ArmorClass {
            get { return 11; }
        }
    }
    public class StuddedLeather : LightArmor {
        public override int Value { get { return 45*5; } }
        public override int ArmorClass {
            get { return 12; }
        }
    }
    public abstract class MediumArmor : Armor {
        public override int MaxDexBonus {
            get { return 3; }
        }
    }
    public class Hide : MediumArmor {
        public override int Value { get { return 7*5; } }
        public override int ArmorClass {
            get { return 11; }
        }
    }
    public class ChainShirt : MediumArmor {
        public override int Value { get { return 60*5; } }
        public override int ArmorClass {
            get { return 12; }
        }
    }
    public class ScaleMail : MediumArmor {
        public override int Value { get { return (50*2+35)*5; } }
        public override int ArmorClass {
            get { return 13; }
        }
    }
    public class Breastplate : MediumArmor {
        public override int Value { get { return 400*5; } }
        public override int ArmorClass {
            get { return 14; }
        }
    }
    public class Half_plate : MediumArmor {
        public override int Value { get { return 750*5; } }
        public override int ArmorClass {
            get { return 15; }
        }
    }

    public class Brigandine : MediumArmor {
        public override int Value { get { return (3000*5)*5; } }
        public override int ArmorClass {
            get { return 16; }
        }
    }

    public abstract class HeavyArmor : Armor {
        public override int MaxDexBonus {
            get { return 0; }
        }
    }
    public class Ring_mail : HeavyArmor {
        public override int Value { get { return 40*5; } }
        public override int ArmorClass {
            get { return 14; }
        }
    }
    public class Chain_mail : HeavyArmor {
        public override int Value { get { return (75*2)*5; } }
        public override int ArmorClass {
            get { return 15; }
        }
    }
    public class Banded_mail : HeavyArmor {
        public override int Value { get { return 75*5*5; } }
        public override int ArmorClass {
            get { return 16; }
        }
    }
    public class Splint : HeavyArmor {
        public override int Value { get { return 200*5*5; } }
        public override int ArmorClass {
            get { return 17; }
        }
    }
    public class Plate : HeavyArmor {
        public override int Value { get { return 1500*5*5; } }
        public override int ArmorClass {
            get { return 18; }
        }
    }
    public class MasterworkPlate : HeavyArmor {
        public override int Value { get { return (int)7.5*1500*5*5; } }
        public override int ArmorClass {
            get { return 19; }
        }
    }

    public class Shield : Equipment {
        public override Slot Slot { get { return Slot.OffHand; } }
        public override int Value { get { return 10*3*5; } }
        private Dictionary<Attribute, int> modifiers = new Dictionary<Attribute, int>() {
            [Attribute.ArmorClass] = 2,
        };
        public override Dictionary<Attribute, int> Modifiers { get { return modifiers;} }

        public override string ToString() {
            return base.ToString() + $" | {Value} gp";
        }
    }

    public class Buckler : Shield {
        public override int Value { get { return 9*5; } }
        private Dictionary<Attribute, int> modifiers = new Dictionary<Attribute, int>() {
            [Attribute.ArmorClass] = 1,
        };
        public override Dictionary<Attribute, int> Modifiers { get { return modifiers;} }
    }

    public class TowerShield : Shield {
        public override int Value { get { return 400*2*5; } }
        private Dictionary<Attribute, int> modifiers = new Dictionary<Attribute, int>() {
            [Attribute.ArmorClass] = 3,
        };
        public override Dictionary<Attribute, int> Modifiers { get { return modifiers;} }
    }

    public abstract class Weapon : Equipment {
        public override Slot Slot { get { return Slot.MainHand; } }
        public abstract Func<bool,int> DamageDie { get; }
        public virtual string DamageDieSize { get { return DiceRoller.dieNames[DamageDie]; } }
        public virtual bool TwoHanded { get { return false; } }
        public virtual bool Finesse { get { return false; } }
        public virtual bool Versatile { get { return false; } }

        public override string ToString() {
            return base.ToString() + $" | Damage: {DamageDieSize}{(TwoHanded ? " | Two-handed" : " | One-handed")}{(Finesse ? " | Finesse" : "")}{(Versatile ? " | Versatile" : "")} | {Value} gp";
        }
    }

    public class Club : Weapon {
        public override bool Versatile { get { return true; } }
        public override int Value { get { return 2*5; } }
        public override Func<bool,int> DamageDie {
            get { return DiceRoller.FourSidedDie; } }
    }

    public class Dagger : Weapon {
        public override bool Finesse { get { return true; } }
        public override int Value { get { return 3*5; } }
        public override Func<bool,int> DamageDie {
            get { return DiceRoller.FourSidedDie; } }
    }
    public class Spear : Weapon {
        public override bool Versatile { get { return true; } }
        public override int Value { get { return 12*5; } }
        public override Func<bool,int> DamageDie {
            get { return DiceRoller.SixSidedDie; } }
    }
    public class Scimitar : Weapon {
        public override bool Finesse { get { return true; } }
        public override int Value { get { return 30*5; } }
        public override Func<bool,int> DamageDie {
            get { return DiceRoller.SixSidedDie; } }
    }
    public class Battleaxe : Weapon {
        public override bool Finesse { get { return true; } }
        public override int Value { get { return 9*5; } }
        public override Func<bool,int> DamageDie {
            get { return DiceRoller.EightSidedDie; } }
    }

    public class GreatAxe : Weapon {
        public override int Value { get { return 150*3*5; } }
        public override bool TwoHanded { get { return true; } }
        public override Func<bool,int> DamageDie {
            get { return DiceRoller.TwelveSidedDie; } }
    }
    public class GreatSword : Weapon {
        public override int Value { get { return 350*3*5; } }
        public override bool TwoHanded { get { return true; } }
        public override Func<bool,int> DamageDie {
            get { return DiceRoller.TwoDSix; } }
    }
}