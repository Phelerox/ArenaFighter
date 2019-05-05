using System.Linq;
using System.Collections.Generic;

using Humanizer;

namespace ArenaFighter.Models
{
    public abstract class Modifier
    {
        public virtual Dictionary<Attribute, int> Modifiers { get { return new Dictionary<Attribute, int>(); } }
        public virtual int GetModifierFor(Attribute attribute) {
            if (Modifiers.ContainsKey(attribute)) {
                return Modifiers[attribute];
            } else {
                return 0;
            }
        }

        public virtual string Name { get { return this.GetType().Name.Humanize(LetterCasing.Title); } }

        public override string ToString() {
            string description = Name;
            if (Modifiers.Count > 0) {
                description += " (";
                bool firstMod = true;
                foreach (Attribute a in Modifiers.Keys) {
                    int mod = Modifiers[a];
                    if (mod == 0) continue;
                    if (!firstMod) description += " ";
                    else firstMod = false;
                    if (mod > 0) description += $"{a}: +{mod}";
                    else if (mod < 0) description += description += $"{a}: {mod}";
                }
                description += ")";
            }
            return description;
        }
    }


}