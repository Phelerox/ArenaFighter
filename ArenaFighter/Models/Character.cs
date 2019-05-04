using System.Reflection.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using ArenaFighter.Models.Utils;

namespace ArenaFighter.Models
{
    public enum Attribute {
        Strength,
        Dexterity,
        Constitution,
        Intelligence,
        Wisdom,
        Charisma,
        CurHitPoints,
        MaxHitPoints,
    }


    public abstract class BaseCharacter
    {
        protected Dictionary<Genders, List<string>> nameCandidates = new Dictionary<Genders, List<string>>
        {
            [Genders.Male] = new List<string>() {"Joe", "Blah"},
            [Genders.Female] = new List<string>() {"Anna", "Blahh"},
            [Genders.Other] = new List<string>() {"Kim"},
        };
        protected Dictionary<Attribute, int> attributes = new Dictionary<Attribute, int>();
        protected Dictionary<Slot, Equipment> equipment = new Dictionary<Slot, Equipment>();
        
        public Race Race { get; set; }
        
        private Weapon mainHand = new ShortSword();
        private Equipment offHand;



        private IList<Modifier> otherModifiers = new List<Modifier>();

        private ulong ageInDaysAtStart;
        private long agedDays;
        private int experience;
        private int[] experienceToLevel = new int[] {300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000, 85000, 100000, 120000, 140000, 165000, 195000, 225000, 265000, 305000, 355000};
        

        public int Level { get; }

        public int Proficiency {
            get { if (Level < 5 ) return 2;
                  else if (Level < 9) return 3;
                  else if (Level < 13) return 4;
                  else if (Level < 17) return 5;
                  else return 6; }
                }
        

        public Tuple<int, int> Age {
            get { ulong ageInDays = ageInDaysAtStart + (ulong) agedDays;
                  return new Tuple<int, int>((int) (ageInDays / 365) , (int) (ageInDays % 365));
                }
        }

        public void age(int days) {
            agedDays += days;
        }


        public string Name { get; set; }
        public Genders Gender { get; set; }

        public IList<Modifier> Modifiers {
            get { return new List<Modifier>(otherModifiers) {(Modifier)mainHand, (Modifier)offHand, (Modifier)Race}; }
        }

        public int AddAllModifiers(Attribute attribute) {
            int total = 0;
            foreach (Modifier m in Modifiers) {
                if (m == null) continue;
                total += m.GetModifierFor(attribute);
            }
            return total;
        }

        public int ArmorClass {
            get { return 10+DexterityMod+WisdomMod;} //temporarily use Monk's Unarmored Defense for this
        }

        public int ToAbilityScoreModifier(int score) {
            return (int) Math.Floor(((double) score - 10) / 2);
        }


        public int Strength
        {
            get { return attributes[Attribute.Strength] + AddAllModifiers(Attribute.Strength); }
        }

        public int StrengthMod
        {
            get { return ToAbilityScoreModifier(Strength); }
        }

        public int Dexterity
        {
            get { return attributes[Attribute.Dexterity] + AddAllModifiers(Attribute.Dexterity); }
        }

        public int DexterityMod
        {
            get { return ToAbilityScoreModifier(Dexterity); }
        }

        public int Constitution
        {
            get { return attributes[Attribute.Constitution] + AddAllModifiers(Attribute.Constitution); }
        }

        public int ConstitutionMod
        {
            get { return ToAbilityScoreModifier(Constitution); }
        }

        public int Intelligence
        {
            get { return attributes[Attribute.Intelligence] + AddAllModifiers(Attribute.Intelligence); }
        }

        public int IntelligenceMod
        {
            get { return ToAbilityScoreModifier(Intelligence); }
        }

        public int Wisdom
        {
            get { return attributes[Attribute.Wisdom] + AddAllModifiers(Attribute.Wisdom); }
        }

        public int WisdomMod
        {
            get { return ToAbilityScoreModifier(Wisdom); }
        }

        public int Charisma
        {
            get { return attributes[Attribute.Charisma] + AddAllModifiers(Attribute.Charisma); }
        }

        public int CharismaMod
        {
            get { return ToAbilityScoreModifier(Charisma); }
        }

        public int CurHitPoints
        {
            get { return attributes[Attribute.CurHitPoints]; }
        }

        public int MaxHitPoints
        {
            get { return attributes[Attribute.MaxHitPoints] + AddAllModifiers(Attribute.MaxHitPoints); }
        }

        protected BaseCharacter(string name=null, Genders? gender=null, Race race=null)
        {
            Level = 1;
            if (race != null) {
                Race = race;
            } else {
                Race = new Human();
            }
            if (gender != null) {
                Gender = (Genders) gender;
            } else {
                Gender = (Genders) DiceRoller.Next(0,(int) Genders.Other);
            }
            if (name != null) {
                Name = name;
            } else {
                //Breakpoint
                Name = nameCandidates[Gender].ElementAt(DiceRoller.Next(0, nameCandidates[Gender].Count));
            }

            GenerateAttributes();
            ageInDaysAtStart = (ulong) DiceRoller.Next(365*15,365*70);
        }

        public virtual void GenerateAttributes(int bonus = 0)
        {
            for (int a = 0; a<6; a++)
            {
                attributes[(Attribute) a] = DiceRoller.Roll4d6DropLowest() + bonus;
            }

            attributes[Attribute.MaxHitPoints] = 10 + ConstitutionMod;
            attributes[Attribute.CurHitPoints] = MaxHitPoints;
            return;
        }

        public virtual int AttackRoll(bool? advantage = null) {
            if (advantage == null) { //Normal roll
                return DiceRoller.TwentySidedDie() + Proficiency + StrengthMod;
            }
            if ((bool) advantage) {
                return Math.Max(AttackRoll(), AttackRoll());
            } else {
                return Math.Min(AttackRoll(), AttackRoll());
            }
        }

        public virtual int DamageRoll(bool critical = false) {
            Func<int> damageDie = mainHand.DamageDie ?? DiceRoller.FourSidedDie;
            return StrengthMod + (critical ? damageDie() + damageDie() : damageDie());
        }

        public override string ToString()
        {
            string str = Name + "\n";
            Tuple<int, int> ageYearsAndDays = Age;
            str += $"{Race.ToString()} {Gender}, aged {ageYearsAndDays.Item1} years and {ageYearsAndDays.Item2} days.\n";
            if (attributes.Count > 0)
            {
                foreach (Attribute a in attributes.Keys)
                {
                    str += $"  {a}: {attributes[a]} + {AddAllModifiers(a)}\n";
                }
            }
            return str;
        }
    }


    public class Player : BaseCharacter
    {
        public Player(string name, Genders gender, Race race = null) : base(name, gender, race)
        {

        }
    }


    public class Champion : BaseCharacter
    {
        public override void GenerateAttributes(int bonus = 2)
        {
            Console.WriteLine("Override success!");
            base.GenerateAttributes(bonus);
        }
    }

    public class Gladiator : BaseCharacter
    {

    }

    public class Soldier : BaseCharacter
    {
        public override void GenerateAttributes(int bonus = -2)
        {
            Console.WriteLine("Override success!");
            base.GenerateAttributes(bonus);
        }
    }

    public class Novice : BaseCharacter
    {
        public override void GenerateAttributes(int bonus = -4)
        {
            Console.WriteLine("Override success!");
            base.GenerateAttributes(bonus);
        }
    }
}
