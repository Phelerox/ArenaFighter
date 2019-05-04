using System.Linq;
using System.Collections.Generic;

using Humanizer;

namespace ArenaFighter.Models
{
    public abstract class Race : Modifier {

    }

    public class Human : Race {
        private Dictionary<Attribute, int> modifiers = new Dictionary<Attribute, int>() {
            [Attribute.Strength] = 1,
            [Attribute.Dexterity] = 1,
            [Attribute.Constitution] = 1,
            [Attribute.Intelligence] = 1,
            [Attribute.Wisdom] = 1,
            [Attribute.Charisma] = 1,
        };
        public override Dictionary<Attribute, int> Modifiers { get { return modifiers;} }
    }

    public class MountainDwarf : Race {
        private Dictionary<Attribute, int> modifiers = new Dictionary<Attribute, int>() {
            [Attribute.Strength] = 2,
            [Attribute.Dexterity] = 0,
            [Attribute.Constitution] = 2,
            [Attribute.Intelligence] = 0,
            [Attribute.Wisdom] = 0,
            [Attribute.Charisma] = 0,
        };
        public override Dictionary<Attribute, int> Modifiers { get { return modifiers;} }

    }
}