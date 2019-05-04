using System.Linq;
using System.Collections.Generic;

using Humanizer;

namespace ArenaFighter.Models
{
    public interface Modifier
    {
        public int GetModifierFor(Attribute attribute);
    }

    public abstract class Race : Modifier {
        
        public abstract int GetModifierFor(Attribute attribute);
        
        public string ToString() {
            return this.GetType().Name.Humanize(LetterCasing.Title);
        }
    }

    public class Human : Race {
        private static Dictionary<Attribute, int> modifiers = new Dictionary<Attribute, int>() {
            [Attribute.Strength] = 1,
            [Attribute.Dexterity] = 1,
            [Attribute.Constitution] = 1,
            [Attribute.Intelligence] = 1,
            [Attribute.Wisdom] = 1,
            [Attribute.Charisma] = 1,
        };

        public override int GetModifierFor(Attribute attribute) {
            return modifiers[attribute];
        }
    }

    public class MountainDwarf : Race {
        public static Dictionary<Attribute, int> modifiers = new Dictionary<Attribute, int>() {
            [Attribute.Strength] = 2,
            [Attribute.Dexterity] = 0,
            [Attribute.Constitution] = 2,
            [Attribute.Intelligence] = 0,
            [Attribute.Wisdom] = 0,
            [Attribute.Charisma] = 0,
        };

        public override int GetModifierFor(Attribute attribute) {
            if (modifiers.ContainsKey(attribute)) {
                return modifiers[attribute];
            } else {
                return 0;
            }
        }

    }
}