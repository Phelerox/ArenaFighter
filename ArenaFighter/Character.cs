using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ArenaFighter.Character
{
    internal enum AS { //Ability Scores
        Strength,
        Dexterity,
        Constitution,
        Intelligence,
        Wisdom,
        Charisma
    }

    internal abstract class BaseCharacter
    {
        protected string[] nameCandidates = { "Joe", "Bananas" };

        protected Dictionary<AS, int> abilityScores = new Dictionary<AS, int>();
           
        internal string Name { get; }

        internal int Strength
        {
            get { return abilityScores[AS.Strength]; }
        }

        internal int Dexterity
        {
            get { return abilityScores[AS.Dexterity]; }
        }

        internal int Constitution
        {
            get { return abilityScores[AS.Constitution]; }
        }

        internal int Intelligence
        {
            get { return abilityScores[AS.Intelligence]; }
        }

        internal int Wisdom
        {
            get { return abilityScores[AS.Wisdom]; }
        }

        internal int Charisma
        {
            get { return abilityScores[AS.Charisma]; }
        }

        protected BaseCharacter()
        {
            if (Name == null)
            {
                Name = nameCandidates[DiceRoller.Next(0, nameCandidates.Length)];
            }
            GenerateAbilityScores();
        }

        protected BaseCharacter(string name) : this()
        {
            Name = name;
        }

        internal virtual IDictionary<AS, int> GenerateAbilityScores(int bonus = 0)
        {
            foreach (AS a in Enum.GetValues(typeof(AS)))
            {
                abilityScores[a] = DiceRoller.Roll4d6DropLowest() + bonus;
            }
            return new Dictionary<AS, int> (abilityScores);
        }

        public override string ToString()
        {
            string str = Name + "\n";
            if (abilityScores.Count > 0)
            {
                foreach (AS a in Enum.GetValues(typeof(AS)))
                {
                    str += $"  {a}: {abilityScores[a]}\n";
                }
            }
            return str;
        }
    }


    internal class Player : BaseCharacter
    {
        public Player(string Name) : base(Name)
        {

        }
    }


    internal class Champion : BaseCharacter
    {
        internal override IDictionary<AS, int> GenerateAbilityScores(int bonus = 2)
        {
            Console.WriteLine("Override success!");
            return base.GenerateAbilityScores(bonus);
        }
    }

    internal class Gladiator : BaseCharacter
    {

    }

    internal class Soldier : BaseCharacter
    {
        internal override IDictionary<AS, int> GenerateAbilityScores(int bonus = -2)
        {
            Console.WriteLine("Override success!");
            return base.GenerateAbilityScores(bonus);
        }
    }

    internal class Novice : BaseCharacter
    {
        internal override IDictionary<AS, int> GenerateAbilityScores(int bonus = -4)
        {
            Console.WriteLine("Override success!");
            return base.GenerateAbilityScores(bonus);
        }
    }
}
