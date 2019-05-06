using System.Threading;
using System.Runtime.InteropServices.ComTypes;
using System.ComponentModel;
using System.Reflection.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Humanizer;

using ArenaFighter.Models.Utils;

namespace ArenaFighter.Models
{
    public enum Attribute {
        Strength,     //Primary ability score. Increases Attack rolls and provides bonus damage.
        Dexterity,    //AC bonus + initiative
        Constitution, //Bonus HP = ConMod * Level
        Intelligence, //Boost both experience and gold gain by 5% per IntMod
        Wisdom,       //Boost experience gain by 10% per WisMod
        Charisma,     //Boost gold gain by 10% per ChaMod
        CurHitPoints,
        MaxHitPoints, // Level 1: Max(Class Hit Die) + ConMod. Increases by Roll(Class Hit Die) + ConMod every level thereafter.
        ArmorClass = 101,
        AttackRollBonus = 102,
        DamageBonus = 103,
        InitiativeBonus = 104,
    }

    public abstract class BaseCharacter
    {
        protected Dictionary<Genders, List<string>> nameCandidates = new Dictionary<Genders, List<string>>
        {
            [Genders.Male] = new List<string>() {"Joe", "Tormund"},
            [Genders.Female] = new List<string>() {"Anna", "Brienne"},
            [Genders.Non_Binary] = new List<string>() {"Kim"},
        };
        protected Dictionary<Attribute, int> attributes = new Dictionary<Attribute, int>();
        protected Dictionary<Slot, Equipment> equipment = new Dictionary<Slot, Equipment>();
        
        public Race Race { get; set; }

        private IList<Modifier> otherModifiers = new List<Modifier>();

        private ulong ageInDaysAtStart;
        private long agedDays;
        private double experience;
        private int[] experienceToLevel = new int[] {0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000, 85000, 100000, 120000, 140000, 165000, 195000, 225000, 265000, 305000, 355000};
        
        public bool Update(int days, int experienceGain) {
            agedDays += days;
            experience += experienceGain * (1 + ((WisdomMod * 2 + IntelligenceMod) * 0.05));
            return CanLevelUp();
        }

        public bool CanLevelUp() {
            (int current, int threshold) = GetExperienceProgressToLevelUp();
            return (current > threshold) ? true : false; //>= would have a risk of leveling up half an experience point too early
        }

        public IDictionary<Attribute,int> LevelUp() {
            if (CanLevelUp()) {
                Level += 1;
                var attributeIncreases = new Dictionary<Attribute, int>();
                attributeIncreases[(Attribute) (DiceRoller.SixSidedDie() - 1)] = 1;
                attributeIncreases[Attribute.MaxHitPoints] = HitDieType(false) + ConstitutionMod;
                foreach (Attribute a in attributeIncreases.Keys) {
                    attributes[a] += attributeIncreases[a];
                }
                return attributeIncreases;
            } else {
                return null;
            }
        }

        public Tuple<int, int> GetExperienceProgressToLevelUp() {
            int experienceRequiredForCurrentLevel = experienceToLevel[Level-1];
            int normalizedExperience = (int) this.experience - experienceRequiredForCurrentLevel;
            return new Tuple<int,int> (normalizedExperience, experienceToLevel[Level] - experienceRequiredForCurrentLevel);
        }

        public int Level { get; private set; }

        public Func<bool,int> HitDieType { get { return DiceRoller.TenSidedDie; } }

        public int Proficiency {
            get {
                if (Level < 5 ) return 2;
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

        public string Name { get; set; }
        public Genders Gender { get; set; }
        public string GenderString { get { return Gender.ToFriendlyString(); } }

        public IList<Modifier> Modifiers {
            get {
                var list = new List<Modifier>(otherModifiers) {(Modifier)Race};
                list.AddRange(Equipment.Values);
                return list;}
        }

        public int AddAllModifiers(Attribute attribute) {
            int total = 0;
            foreach (Modifier m in Modifiers) {
                if (m == null) continue;
                total += m.GetModifierFor(attribute);
            }
            return total;
        }

        public IDictionary<Slot, Equipment> Equipment {
            get {
                var wearing = new Dictionary<Slot, Equipment>();
                Weapon mainHand = null;
                Equipment offHand = null;
                foreach (Equipment e in equipment.Values) {
                    if (e.Slot == Slot.OffHand) offHand = e; else wearing[e.Slot] = e;
                    if (e.Slot == Slot.MainHand) mainHand = (Weapon)e;
                }
                if ((mainHand == null || (mainHand.TwoHanded == false)) && offHand != null) {
                        wearing[Slot.OffHand] = offHand;
                    }
                return wearing;
            }
        }

        public Weapon Weapon { get { return (Weapon) equipment[Slot.MainHand]; } }

        public void EquipRandomEquipment() {
            foreach (Equipment e in Reflection.instance.GenerateRandomEquipment(0).Values) {
                equipment[e.Slot] = e;
            }
        }

        public IEnumerable<Tuple<int, string>> ListAllModifiersFor(Attribute attribute) {
            var bonus = new List<Tuple<int, string>>();
            foreach (Modifier m in Modifiers) {
                if (m == null) continue;
                int mBonus = m.GetModifierFor(attribute);
                if (mBonus == 0) continue;
                bonus.Add(new Tuple<int, string> (mBonus, m.ToString()));
            }
            return bonus;
        }

        public int ArmorClass {
            get { return 10+DexterityMod+AddAllModifiers(Attribute.ArmorClass);} //temporarily use Monk's Unarmored Defense for this
        }

        public int ToAbilityScoreModifier(int score) {
            return (int) Math.Floor(((double) score - 10) / 2);
        }

        public int Strength {
            get { return attributes[Attribute.Strength] + AddAllModifiers(Attribute.Strength); }
        }

        public int StrengthMod {
            get { return ToAbilityScoreModifier(Strength); }
        }

        public int Dexterity {
            get { return attributes[Attribute.Dexterity] + AddAllModifiers(Attribute.Dexterity); }
        }

        public int DexterityMod {
            get { return ToAbilityScoreModifier(Dexterity); }
        }

        public int Constitution
        {
            get { return attributes[Attribute.Constitution] + AddAllModifiers(Attribute.Constitution); }
        }

        public int ConstitutionMod {
            get { return ToAbilityScoreModifier(Constitution); }
        }

        public int Intelligence {
            get { return attributes[Attribute.Intelligence] + AddAllModifiers(Attribute.Intelligence); }
        }

        public int IntelligenceMod {
            get { return ToAbilityScoreModifier(Intelligence); }
        }

        public int Wisdom {
            get { return attributes[Attribute.Wisdom] + AddAllModifiers(Attribute.Wisdom); }
        }

        public int WisdomMod {
            get { return ToAbilityScoreModifier(Wisdom); }
        }

        public int Charisma {
            get { return attributes[Attribute.Charisma] + AddAllModifiers(Attribute.Charisma); }
        }

        public int CharismaMod {
            get { return ToAbilityScoreModifier(Charisma); }
        }

        public int CurHitPoints {
            get { return attributes[Attribute.CurHitPoints]; }
        }

        public int MaxHitPoints {
            get { return attributes[Attribute.MaxHitPoints] + AddAllModifiers(Attribute.MaxHitPoints); }
        }

        protected BaseCharacter(string name=null, Genders? gender=null, Race race=null) {
            Level = 1;
            if (race != null) {
                Race = race;
            } else {
                Race = new Human();
            }
            if (gender != null) {
                Gender = (Genders) gender;
            } else {
                Gender = (Genders) DiceRoller.Next(0,(int) Genders.Non_Binary);
            }
            if (name != null) {
                Name = name;
            } else {
                //Breakpoint
                Name = nameCandidates[Gender].ElementAt(DiceRoller.Next(0, nameCandidates[Gender].Count));
            }
            GenerateAttributes();
            EquipRandomEquipment();
            ageInDaysAtStart = (ulong) DiceRoller.Next(365*15,365*70);
        }

        public virtual void GenerateAttributes(int bonus = 0) {
            for (int a = 0; a<6; a++)
            {
                attributes[(Attribute) a] = DiceRoller.Roll4d6DropLowest() + bonus;
            }

            attributes[Attribute.MaxHitPoints] = 10 + ConstitutionMod;
            attributes[Attribute.CurHitPoints] = MaxHitPoints;
            return;
        }

        public virtual int AttackRoll(bool? advantage = null, bool rollMax = false) {
            if (advantage == null) { //Normal roll
                Weapon weapon = equipment.ContainsKey(Slot.MainHand) ? (Weapon)equipment[Slot.MainHand] : null;
                bool finesse = weapon?.Finesse ?? false;
                return DiceRoller.TwentySidedDie(rollMax) + Proficiency + (finesse ? Math.Max(StrengthMod, DexterityMod) : StrengthMod);
            }
            if ((bool) advantage) {
                return Math.Max(AttackRoll(rollMax), AttackRoll(rollMax));
            } else {
                return Math.Min(AttackRoll(rollMax), AttackRoll(rollMax));
            }
        }

        public virtual int DamageRoll(bool critical = false, bool rollMax = false) {
            Weapon weapon = equipment.ContainsKey(Slot.MainHand) ? (Weapon)equipment[Slot.MainHand] : null;
            bool finesse = weapon?.Finesse ?? false;
            bool versatile = weapon?.Versatile ?? false;
            Func<bool,int> damageDie = ((versatile && (Equipment.ContainsKey(Slot.OffHand) && Equipment[Slot.OffHand] != null)
                ? DiceRoller.enlargeDie(weapon?.DamageDie) : weapon?.DamageDie) ?? DiceRoller.FourSidedDie);
            return (finesse ? Math.Max(StrengthMod, DexterityMod) : StrengthMod) + (critical ? damageDie(rollMax) + damageDie(rollMax) : damageDie(rollMax));
        }

        public double PowerEstimate { get { return powerHeuristic(); } }

        protected virtual double powerHeuristic() {
            double averageAttackRoll = AttackRoll(rollMax: true) - DiceRoller.averageRoll[DiceRoller.TwentySidedDie];
            double averageDamage = DamageRoll(rollMax: true) - DiceRoller.averageRoll[Weapon.DamageDie];
            double defenseSkill = ArmorClass;
            double health = MaxHitPoints;
            return averageAttackRoll + averageDamage + defenseSkill + health; ////TODO: weigh them appropriately
        }

        protected virtual double TestRelativePower(BaseCharacter enemy, int simulations) {
            //SimulateFight x simulations times
            return 0.0; ///TODO:
        }

        protected virtual Tuple<double,int,int> SimulateFight(BaseCharacter enemy) {
            double percentWon = 0.0;
            int averageHealthLeft = 0;
            int enemyAverageHealthLeft = 0;
            //TODO: Fight
            return Tuple.Create(percentWon, averageHealthLeft, enemyAverageHealthLeft);
        }

        public override string ToString() {
            string description = Name + "\n";
            Tuple<int, int> ageYearsAndDays = Age;
            description += $"{GenderString} {Race.ToString()} aged {ageYearsAndDays.Item1} years and {ageYearsAndDays.Item2} days.\n";
            ////TODO: gold
            if (attributes.Count > 0) {
                var attributesToDisplay = new Dictionary<Attribute,int> (attributes);
                attributesToDisplay[Attribute.ArmorClass] = ArmorClass;
                foreach (Attribute a in attributes.Keys) {
                    int totalModifier = AddAllModifiers(a);
                    description += $"  {a}: {attributes[a] + totalModifier}";
                    if (totalModifier > 0) description += $" ({attributes[a]}+{totalModifier})";
                    else if (totalModifier < 0) description += $" ({attributes[a]}-{totalModifier})";
                    description += "\n";
                }
            }
            if (Modifiers.Count > 0) {
                IList<Modifier> alreadyProcessed = new List<Modifier>();
                alreadyProcessed.Add(Race);
                if (Equipment.Count > 0) {
                    foreach (Slot s in Enum.GetValues(typeof(Slot))) {
                        description += $"{s.ToString().Humanize(LetterCasing.Title)}: ";
                        if (Equipment.ContainsKey(s) && (Equipment[s] != null)) {
                            Equipment e = Equipment[s];
                            alreadyProcessed.Add(e);
                            description += $" {e}";
                        } else if (s == Slot.OffHand && Equipment.ContainsKey(Slot.MainHand) && ((Weapon)Equipment[Slot.MainHand]).TwoHanded) {
                            description += $" holding {Equipment[Slot.MainHand].Name}";
                        }
                        else description += " empty";
                        description += "\n";
                    }
                }
                foreach (Modifier m in Modifiers) {
                    if (alreadyProcessed.Contains(m)) continue;
                    description += $"{m}\n";
                }
            }
            return description;
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
