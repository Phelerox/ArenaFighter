using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using ArenaFighter.Model.Util;

namespace ArenaFighter.Model
{
    public enum AS { //Ability Scores
        Strength,
        Dexterity,
        Constitution,
        Intelligence,
        Wisdom,
        Charisma
    }

    public abstract class BaseCharacter
    {
        protected string[] nameCandidates = { "Joe", "Bananas" };
        protected Dictionary<AS, int> abilityScores = new Dictionary<AS, int>();

        string Name { get; }

        int Strength
        {
            get { return abilityScores[AS.Strength]; }
        }

        public int Dexterity
        {
            get { return abilityScores[AS.Dexterity]; }
        }

        public int Constitution
        {
            get { return abilityScores[AS.Constitution]; }
        }

        public int Intelligence
        {
            get { return abilityScores[AS.Intelligence]; }
        }

        public int Wisdom
        {
            get { return abilityScores[AS.Wisdom]; }
        }

        public int Charisma
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

        public virtual IDictionary<AS, int> GenerateAbilityScores(int bonus = 0)
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


    public class Player : BaseCharacter
    {
        public Player(string Name) : base(Name)
        {

        }
    }


    public class Champion : BaseCharacter
    {
        public override IDictionary<AS, int> GenerateAbilityScores(int bonus = 2)
        {
            Console.WriteLine("Override success!");
            return base.GenerateAbilityScores(bonus);
        }
    }

    public class Gladiator : BaseCharacter
    {

    }

    public class Soldier : BaseCharacter
    {
        public override IDictionary<AS, int> GenerateAbilityScores(int bonus = -2)
        {
            Console.WriteLine("Override success!");
            return base.GenerateAbilityScores(bonus);
        }
    }

    public class Novice : BaseCharacter
    {
        public override IDictionary<AS, int> GenerateAbilityScores(int bonus = -4)
        {
            Console.WriteLine("Override success!");
            return base.GenerateAbilityScores(bonus);
        }
    }
}
