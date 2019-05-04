using System.Linq;
using System.Collections.Generic;

using Humanizer;

namespace ArenaFighter.Models
{
    public abstract class Modifier
    {
        public virtual Dictionary<Attribute, int> Modifiers { get { return null; } }
        public virtual int GetModifierFor(Attribute attribute) {
            if (Modifiers.ContainsKey(attribute)) {
                return Modifiers[attribute];
            } else {
                return 0;
            }
        }

        public string ToString() {
            return this.GetType().Name.Humanize(LetterCasing.Title);
        }
    }


}