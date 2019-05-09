using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Humanizer;

namespace ArenaFighter.Models {

    public enum DamageType {
        Bludgeoning,
        Piercing,
        Slashing,
        Fire,
        Water,
        Acid,
        Poison,
        Ice
    }

    public abstract class Modifier {
        protected virtual Dictionary<Attribute, int> AttributeModifiers { get { return new Dictionary<Attribute, int>(); } }
        public virtual int GetModifierFor(Attribute attribute) {
            if (AttributeModifiers.ContainsKey(attribute)) {
                return AttributeModifiers[attribute];
            } else {
                return 0;
            }
        }

        public virtual string Name { get { return this.GetType().Name.Humanize(LetterCasing.Title); } }
        public virtual string Description {
            get { return ""; }
        }
        public override string ToString() {
            string description = Name + (Description.Length > 0 ? " | " + Description + " |" : "");
            if (AttributeModifiers.Count > 0) {
                description += " (";
                bool firstMod = true;
                foreach (Attribute a in AttributeModifiers.Keys) {
                    int mod = AttributeModifiers[a];
                    if (mod == 0)continue;
                    if (!firstMod)description += " ";
                    else firstMod = false;
                    if (mod > 0)description += $"{a}: +{mod}";
                    else if (mod < 0)description += description += $"{a}: {mod}";
                }
                description += ")";
            }
            return description;
        }
    }

    public abstract class Feat : Modifier {

    }

    public interface IOverrideModifier {

    }

    public interface IDamageTaken : IOverrideModifier {
        public dynamic DamageTaken(dynamic damage, DamageType damageType);
    }

    public class HeavyArmorMaster : Feat, IDamageTaken {
        private HashSet<DamageType> physicalDamage = new HashSet<DamageType>() { DamageType.Bludgeoning, DamageType.Piercing, DamageType.Slashing };
        public dynamic DamageTaken(dynamic damage, DamageType damageType) {
            return physicalDamage.Contains(damageType) ? Math.Max(damage - 3, 0) : damage;
        }
        public override string Description {
            get { return "Reduces physical damage taken by 3"; }
        }
    }

}
