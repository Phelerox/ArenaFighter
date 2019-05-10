using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ArenaFighter.Models.Utils;

using Humanizer;

namespace ArenaFighter.Models {
    public enum Slot {
        MainHand,
        OffHand,
        Armor,
        Helmet,
    }

    public abstract class Equipment : Modifier {
        public abstract int Price { get; }
        public abstract Slot Slot { get; }
        private static int highestPrice = new LoricaPlumata().Price;
        public int ScaledAbundance(double priceScaling = 0.2, double maxPriceCutoffRatio = 0.5) {
            int price = Price;
            if (price > 0 && price <= 50)price = Convert.ToInt32((35 + price) * (50.0 / Math.Max((double)price, 15.0))); //lie to make crappy things less common
            if (price == 0) { return 0; } else if (price < highestPrice * maxPriceCutoffRatio) {
                //return (highestPrice * maxPriceCutoffRatio) / (Math.Max(Price * priceScaling, (Price * priceScaling) + 1));
                //return (highestPrice * maxPriceCutoffRatio) / (Math.Max(Price * priceScaling, (Price * priceScaling) + 1));
                return ((int)(Math.Sqrt(highestPrice * maxPriceCutoffRatio) / (Math.Sqrt((double)Math.Max(price * priceScaling, price * priceScaling + 1)))));
            } else {
                return 0;
            }
        }
        public virtual int Abundance { get { return ScaledAbundance(); } } // 0 Abundanace is the highest rarity
    }

    public abstract class Armor : Equipment {
        public override Slot Slot { get { return Slot.Armor; } }
        public abstract int ArmorClass { get; }
        public abstract int MaxDexBonus { get; }

        public override string ToString() {
            return base.ToString() + $" | Armor Class: {ArmorClass} | Max Dexterity Bonus: {MaxDexBonus} | {Price} gp";
        }
    }

    public class Skin : Armor {
        public override int Price { get { return 0; } }
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
        public override int Price { get { return 10 * 5; } }
        public override int ArmorClass {
            get { return 11; }
        }
    }
    public class StuddedLeather : LightArmor {
        public override int Price { get { return 45 * 5; } }
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
        public override int Price { get { return 7 * 5; } }
        public override int ArmorClass {
            get { return 11; }
        }
    }
    public class ChainShirt : MediumArmor {
        public override int Price { get { return 60 * 5; } }
        public override int ArmorClass {
            get { return 12; }
        }
    }
    public class ScaleMail : MediumArmor {
        public override int Price { get { return (50 * 2 + 35) * 5; } }
        public override int ArmorClass {
            get { return 13; }
        }
    }
    public class Breastplate : MediumArmor {
        public override int Price { get { return 400 * 5; } }
        public override int ArmorClass {
            get { return 14; }
        }
    }
    public class LoricaHamata : MediumArmor {
        public override int Price { get { return 750 * 5; } }
        public override int ArmorClass {
            get { return 15; }
        }
    }

    public class LoricaSquamata : MediumArmor {
        public override int Price { get { return (3000 * 5) * 5; } }
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
        public override int Price { get { return 40 * 5; } }
        public override int ArmorClass {
            get { return 14; }
        }
    }
    public class Chain_mail : HeavyArmor {
        public override int Price { get { return (75 * 2) * 5; } }
        public override int ArmorClass {
            get { return 15; }
        }
    }
    public class Banded_mail : HeavyArmor {
        public override int Price { get { return 75 * 5 * 5; } }
        public override int ArmorClass {
            get { return 16; }
        }
    }
    public class LoricaMusculata : HeavyArmor {
        public override int Price { get { return 200 * 5 * 5; } }
        public override int ArmorClass {
            get { return 17; }
        }
    }
    public class LoricaSegmentata : HeavyArmor {
        public override int Price { get { return 1500 * 5 * 5; } }
        public override int ArmorClass {
            get { return 18; }
        }
    }
    public class LoricaPlumata : HeavyArmor {
        public override int Price { get { return (int)7.5 * 1500 * 5 * 5; } }
        public override int ArmorClass {
            get { return 19; }
        }
        private Dictionary<Attribute, int> attributeModifiers = new Dictionary<Attribute, int>() {
            [Attribute.SavingThrowsBonus] = 1,
        };
        protected override Dictionary<Attribute, int> AttributeModifiers { get { return attributeModifiers; } }

    }

    public abstract class Helmet : Equipment {
        public override Slot Slot { get { return Slot.Helmet; } }

        public override string ToString() {
            return base.ToString() + $" | {Price} gp";
        }
    }

    public class Leather_cap : Helmet {
        public override int Price { get { return 35 * 2 * 5; } }
        private Dictionary<Attribute, int> attributeModifiers = new Dictionary<Attribute, int>() {
            [Attribute.MaxHitPoints] = 3,
        };
        protected override Dictionary<Attribute, int> AttributeModifiers { get { return attributeModifiers; } }
    }

    public class Galea : Helmet {
        public override int Price { get { return 350 * 2 * 5; } }
        private Dictionary<Attribute, int> attributeModifiers = new Dictionary<Attribute, int>() {
            [Attribute.ArmorClass] = 1,
        };
        protected override Dictionary<Attribute, int> AttributeModifiers { get { return attributeModifiers; } }
    }
    public class ImperialGalea : Helmet {
        public override int Price { get { return 850 * 2 * 5; } }
        private Dictionary<Attribute, int> attributeModifiers = new Dictionary<Attribute, int>() {
            [Attribute.ArmorClass] = 1, [Attribute.MaxHitPoints] = 5,
        };
        protected override Dictionary<Attribute, int> AttributeModifiers { get { return attributeModifiers; } }
    }
    public abstract class Shield : Equipment {
        public override Slot Slot { get { return Slot.OffHand; } }

        public override string ToString() {
            return base.ToString() + $" | {Price} gp";
        }
    }

    public class Caetra : Shield {
        public override int Price { get { return 9 * 5; } }
        private Dictionary<Attribute, int> attributeModifiers = new Dictionary<Attribute, int>() {
            [Attribute.ArmorClass] = 1,
        };
        protected override Dictionary<Attribute, int> AttributeModifiers { get { return attributeModifiers; } }
    }

    public class Aspis : Shield {
        public override Slot Slot { get { return Slot.OffHand; } }
        public override int Price { get { return 10 * 6 * 5; } }
        private Dictionary<Attribute, int> attributeModifiers = new Dictionary<Attribute, int>() {
            [Attribute.ArmorClass] = 2,
        };
        protected override Dictionary<Attribute, int> AttributeModifiers { get { return attributeModifiers; } }
    }

    public class Scutum : Shield {
        public override int Price { get { return 400 * 2 * 5; } }
        private Dictionary<Attribute, int> attributeModifiers = new Dictionary<Attribute, int>() {
            [Attribute.ArmorClass] = 3,
        };
        protected override Dictionary<Attribute, int> AttributeModifiers { get { return attributeModifiers; } }
    }

    public abstract class Weapon : Equipment {
        public override Slot Slot { get { return Slot.MainHand; } }
        public abstract Func<bool, int> DamageDie { get; }
        public virtual string DamageDieSize { get { return DiceRoller.dieNames[DamageDie]; } }
        public virtual DamageType DamageType { get { return DamageType.Bludgeoning; } }
        public virtual bool TwoHanded { get { return false; } }
        public virtual bool Finesse { get { return false; } }
        public virtual bool Versatile { get { return false; } }

        public override string ToString() {
            return base.ToString() + $" | Damage: {DamageDieSize}{(TwoHanded ? " | Two-handed" : " | One-handed")}{(Finesse ? " | Finesse" : "")}{(Versatile ? " | Versatile" : "")} | {DamageType} | {Price} gp";
        }
    }

    public class Club : Weapon {
        public override bool Versatile { get { return true; } }
        public override int Price { get { return 2 * 5; } }
        public override DamageType DamageType { get { return DamageType.Bludgeoning; } }
        public override Func<bool, int> DamageDie {
            get { return DiceRoller.FourSidedDie; }
        }
    }

    public class Pugio : Weapon {
        public override bool Finesse { get { return true; } }
        public override int Price { get { return 8 * 5; } }
        public override DamageType DamageType { get { return DamageType.Piercing; } }
        public override Func<bool, int> DamageDie {
            get { return DiceRoller.FourSidedDie; }
        }
        private Dictionary<Attribute, int> attributeModifiers = new Dictionary<Attribute, int>() {
            [Attribute.DamageBonus] = 1, [Attribute.AttackRollBonus] = 1,
        };
        protected override Dictionary<Attribute, int> AttributeModifiers { get { return attributeModifiers; } }
    }
    public class Hasta : Weapon {
        public override bool Versatile { get { return true; } }
        public override int Price { get { return 12 * 5; } }
        public override DamageType DamageType { get { return DamageType.Piercing; } }
        public override Func<bool, int> DamageDie {
            get { return DiceRoller.SixSidedDie; }
        }
        private Dictionary<Attribute, int> attributeModifiers = new Dictionary<Attribute, int>() {
            [Attribute.AttackRollBonus] = 1,
        };
    }
    public class Gladius : Weapon {
        public override bool Finesse { get { return true; } }
        public override int Price { get { return 67 * 5; } }
        public override DamageType DamageType { get { return DamageType.Slashing; } }
        public override Func<bool, int> DamageDie {
            get { return DiceRoller.SixSidedDie; }
        }
        private Dictionary<Attribute, int> attributeModifiers = new Dictionary<Attribute, int>() {
            [Attribute.DamageBonus] = 1,
        };
        protected override Dictionary<Attribute, int> AttributeModifiers { get { return attributeModifiers; } }
    }
    public class Spatha : Weapon {
        public override bool Finesse { get { return true; } }
        public override int Price { get { return 167 * 5; } }
        public override DamageType DamageType { get { return DamageType.Slashing; } }
        public override Func<bool, int> DamageDie {
            get { return DiceRoller.EightSidedDie; }
        }
        private Dictionary<Attribute, int> attributeModifiers = new Dictionary<Attribute, int>() {
            [Attribute.AttackRollBonus] = 1,
        };
        protected override Dictionary<Attribute, int> AttributeModifiers { get { return attributeModifiers; } }
    }
    public class Warhammer : Weapon {
        public override bool Versatile { get { return true; } }
        public override int Price { get { return 59 * 5; } }
        public override DamageType DamageType { get { return DamageType.Bludgeoning; } }
        public override Func<bool, int> DamageDie {
            get { return DiceRoller.EightSidedDie; }
        }
    }
    public class GreatAxe : Weapon {
        public override int Price { get { return 150 * 3 * 5; } }
        public override bool TwoHanded { get { return true; } }
        public override DamageType DamageType { get { return DamageType.Slashing; } }
        public override Func<bool, int> DamageDie {
            get { return DiceRoller.TwelveSidedDie; }
        }
    }
    public class GreatSword : Weapon {
        public override int Price { get { return 350 * 3 * 5; } }
        public override bool TwoHanded { get { return true; } }
        public override DamageType DamageType { get { return DamageType.Slashing; } }
        public override Func<bool, int> DamageDie {
            get { return DiceRoller.TwoDSix; }
        }
    }
}
