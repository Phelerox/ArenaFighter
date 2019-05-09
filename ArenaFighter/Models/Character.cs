using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ArenaFighter.Models.Utils;

using Humanizer;

namespace ArenaFighter.Models {
    public enum Attribute {
        Strength, //Primary ability score. Increases Attack rolls and provides bonus damage.
        Dexterity, //AC bonus + initiative
        Constitution, //Bonus HP = ConMod * Level
        Intelligence, //Boost both experience and gold gain by 5% per IntMod
        Wisdom, //Boost experience gain by 10% per WisMod
        Charisma, //Boost gold gain by 10% per ChaMod
        DamageTaken,
        MaxHitPoints, // Level 1: Max(Class Hit Die) + ConMod. Increases by Roll(Class Hit Die) + ConMod every level thereafter.
        ArmorClass = 101,
        AttackRollBonus = 102,
        DamageBonus = 103,
        InitiativeBonus = 104,
    }

    public abstract class BaseCharacter {
        protected Dictionary<Genders, List<string>> nameCandidates = new Dictionary<Genders, List<string>> {
            [Genders.Male] = new List<string>() { "Joe", "Tormund" },
            [Genders.Female] = new List<string>() { "Anna", "Brienne" },
            [Genders.Non_Binary] = new List<string>() { "Kim" },
        };
        protected Dictionary<Attribute, int> attributes = new Dictionary<Attribute, int>();
        protected Dictionary<Slot, Equipment> equipment = new Dictionary<Slot, Equipment>();

        public Race Race { get; set; }
        //private OverrideModifierDictionary overrideModifiers = new OverrideModifierDictionary();
        private Dictionary<Type, ICollection<IOverrideModifier>> overrideModifiers = new Dictionary<Type, ICollection<IOverrideModifier>>();
        private ICollection<Modifier> otherModifiers = new HashSet<Modifier>();

        private ulong ageInDaysAtStart;
        private long agedDays;
        private double experience;
        private int[] experienceToLevel = new int[] { 0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000, 85000, 100000, 120000, 140000, 165000, 195000, 225000, 265000, 305000, 355000 };

        public bool Update(int days, int experienceGain) {
            agedDays += days;
            experience += experienceGain * (1 + ((WisdomMod * 2 + IntelligenceMod) * 0.05));
            return CanLevelUp();
        }

        public bool CanLevelUp() {
            (int current, int threshold) = GetExperienceProgressToLevelUp();
            return (current > threshold) ? true : false; //>= would have a risk of leveling up half an experience point too early
        }

        public IDictionary<Attribute, int> LevelUp() {
            if (CanLevelUp()) {
                Level += 1;
                var attributeIncreases = new Dictionary<Attribute, int>();
                attributeIncreases[(Attribute)(DiceRoller.SixSidedDie() - 1)] = 1;
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
            int experienceRequiredForCurrentLevel = experienceToLevel[Level - 1];
            int normalizedExperience = (int)this.experience - experienceRequiredForCurrentLevel;
            return new Tuple<int, int>(normalizedExperience, experienceToLevel[Level] - experienceRequiredForCurrentLevel);
        }

        public int Level { get; private set; }

        public Func<bool, int> HitDieType { get { return DiceRoller.TenSidedDie; } }

        public int Proficiency {
            get {
                if (Level < 5)return 2;
                else if (Level < 9)return 3;
                else if (Level < 13)return 4;
                else if (Level < 17)return 5;
                else return 6;
            }
        }

        public Tuple<int, int> Age {
            get {
                ulong ageInDays = ageInDaysAtStart + (ulong)agedDays;
                return new Tuple<int, int>((int)(ageInDays / 365), (int)(ageInDays % 365));
            }
        }

        public string Name { get; set; }
        public Genders Gender { get; set; }
        public string GenderString { get { return Gender.ToFriendlyString(); } }

        public ICollection<Modifier> Modifiers {
            get {
                var set = new HashSet<Modifier>(otherModifiers) { };
                set.UnionWith(Equipment.Values);
                set.UnionWith(GetAllOverrideModifiers().Cast<Modifier>());
                //Todo: add overrideModifiers
                return set;
            }
        }

        public ICollection<Tuple<int, string>> ListAllModifiersFor(Attribute attribute) {
            var bonus = new List<Tuple<int, string>>();
            foreach (Modifier m in Modifiers) {
                if (m == null)continue;
                int mBonus = m.GetModifierFor(attribute);
                if (mBonus == 0)continue;
                bonus.Add(new Tuple<int, string>(mBonus, m.ToString()));
            }
            return bonus;
        }

        public void AddModifier(Modifier m) {
            if (typeof(IOverrideModifier).IsAssignableFrom(m.GetType())) {
                AddOverrideModifier(m.GetType().GetInterfaces().First((i) => i.GetInterfaces().Contains(typeof(IOverrideModifier))), (IOverrideModifier)m);
            } else {
                otherModifiers.Add(m);
            }
        }

        public void AddOverrideModifier(Type IOverrideModifierSubType, IOverrideModifier modifier) {
            if (!IOverrideModifierSubType.GetTypeInfo().IsInterface || !IOverrideModifierSubType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IOverrideModifier))) {
                throw new ArgumentException("IOverrideModifierSubtype must be an interface that implements IOverrideModifier!");
            }
            ICollection<IOverrideModifier> existingSimilarModifiers;
            if (overrideModifiers.TryGetValue(IOverrideModifierSubType, out existingSimilarModifiers)) {
                existingSimilarModifiers.Add(modifier);
            } else {
                existingSimilarModifiers = new HashSet<IOverrideModifier>();
                existingSimilarModifiers.Add(modifier);
                overrideModifiers[IOverrideModifierSubType] = existingSimilarModifiers;
            }
        }

        private ICollection<IOverrideModifier> GetAllOverrideModifiers() {
            var modifiers = new HashSet<IOverrideModifier>();
            foreach (Type overrideModifierType in overrideModifiers.Keys) {
                ICollection<IOverrideModifier> modifierCollection = (ICollection<IOverrideModifier>)overrideModifiers[overrideModifierType];
                if (modifierCollection != null) {
                    modifiers.UnionWith(modifierCollection);
                }
            }
            return modifiers;
        }

        public int AddAllModifiers(Attribute attribute) {
            int total = 0;
            ISet<Modifier> alreadyProcessed = new HashSet<Modifier>();
            foreach (Modifier m in Modifiers) {
                if (m == null)continue;
                else if (alreadyProcessed.Contains(m)) {
                    Console.WriteLine($"Duplicate modifier! {m}");
                    continue;
                }
                alreadyProcessed.Add(m);
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
                    if (e.Slot == Slot.OffHand)offHand = e;
                    else wearing[e.Slot] = e;
                    if (e.Slot == Slot.MainHand)mainHand = (Weapon)e;
                }
                if ((mainHand == null || (mainHand.TwoHanded == false)) && offHand != null) {
                    wearing[Slot.OffHand] = offHand;
                }
                return wearing;
            }
        }

        public Weapon Weapon { get { return (Weapon)Equipment[Slot.MainHand]; } }

        public void EquipRandomEquipment() {
            foreach (Equipment e in Reflection.instance.GenerateRandomEquipment(0).Values) {
                equipment[e.Slot] = e;
            }
        }

        public void EquipBestEquipmentWithinBudget(int budgetPerSlot = 600) {
            foreach (Equipment e in Reflection.instance.SelectBestEquipmentWithinBudget(budgetPerSlot).Values) {
                equipment[e.Slot] = e;
            }
        }

        public bool EquipItem(Equipment eq) {
            equipment[eq.Slot] = eq;
            return true;
        }

        public int ArmorClass {
            get { return ArmorClassFromArmor + AddAllModifiers(Attribute.ArmorClass); }
        }

        public int ArmorClassFromArmor {
            get { return ((Armor)Equipment[Slot.Armor]).ArmorClass + Math.Min(DexterityMod, ((Armor)Equipment[Slot.Armor]).MaxDexBonus); }
        }

        public int ToAbilityScoreModifier(int score) {
            return (int)Math.Floor(((double)score - 10) / 2);
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

        public int Constitution {
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
            get { return MaxHitPoints - attributes[Attribute.DamageTaken]; }
            set { attributes[Attribute.DamageTaken] = MaxHitPoints - value; }
        }

        public int MaxHitPoints {
            get { return attributes[Attribute.MaxHitPoints] + AddAllModifiers(Attribute.MaxHitPoints); }
        }

        public int ExtraAttacks {
            get { return (Level >= 20 ? 3 : (Level >= 11 ? 2 : (Level >= 5 ? 1 : 0))); }
        }

        public int Initiative {
            get { return DexterityMod + AddAllModifiers(Attribute.InitiativeBonus); }
        }

        protected BaseCharacter(string name = null, Genders? gender = null, Race race = null) {
            Level = 1;
            if (race != null) {
                Race = race;
            } else {
                Race = new Human();
            }
            otherModifiers.Add((Modifier)Race);
            if (gender != null) {
                Gender = (Genders)gender;
            } else {
                Gender = (Genders)DiceRoller.Next(0, (int)Genders.Non_Binary);
            }
            if (name != null) {
                Name = name;
            } else {
                //Breakpoint
                Name = nameCandidates[Gender].ElementAt(DiceRoller.Next(0, nameCandidates[Gender].Count));
            }
            GenerateAttributes();
            EquipRandomEquipment();
            ageInDaysAtStart = (ulong)DiceRoller.Next(365 * 15, 365 * 70);
        }

        public virtual void GenerateAttributes(int bonus = 0) {
            for (int a = 0; a < 6; a++) {
                attributes[(Attribute)a] = DiceRoller.Roll4d6DropLowest() + bonus;
            }

            attributes[Attribute.MaxHitPoints] = HitDieType(true) + HitDieType(false) + ConstitutionMod;
            return;
        }

        public virtual int AttackRoll(bool? advantage = null, bool rollMax = false) {
            if (advantage == null) { //Normal roll
                Weapon weapon = equipment.ContainsKey(Slot.MainHand) ? (Weapon)equipment[Slot.MainHand] : null;
                bool finesse = weapon?.Finesse ?? false;
                int roll = DiceRoller.TwentySidedDie(rollMax: rollMax);
                if (roll == 20 && !rollMax) {
                    return Int32.MaxValue;
                } else if (roll == 1) {
                    return Int32.MinValue;
                }
                return roll + Proficiency + (finesse ? Math.Max(StrengthMod, DexterityMod) : StrengthMod) + AddAllModifiers(Attribute.AttackRollBonus);
            }
            if ((bool)advantage) {
                return Math.Max(AttackRoll(rollMax: rollMax), AttackRoll(rollMax: rollMax));
            } else {
                return Math.Min(AttackRoll(rollMax: rollMax), AttackRoll(rollMax: rollMax));
            }
        }

        public virtual int DamageRoll(bool critical = false, bool rollMax = false) {
            Weapon weapon = equipment.ContainsKey(Slot.MainHand) ? (Weapon)equipment[Slot.MainHand] : null;
            bool finesse = weapon?.Finesse ?? false;
            bool versatile = weapon?.Versatile ?? false;
            Func<bool, int> damageDie = ((versatile && (Equipment.ContainsKey(Slot.OffHand) && Equipment[Slot.OffHand] != null) ?
                DiceRoller.enlargeDie(weapon?.DamageDie) : weapon?.DamageDie) ?? DiceRoller.FourSidedDie);
            return (finesse ? Math.Max(StrengthMod, DexterityMod) : StrengthMod) + (critical ? damageDie(rollMax) + damageDie(rollMax) : damageDie(rollMax)) + AddAllModifiers(Attribute.DamageBonus);
        }

        public virtual dynamic ReceiveDamage(dynamic damage, DamageType damageType = DamageType.Slashing, bool justKidding = false) {
            ICollection<IOverrideModifier> overrides;
            if (overrideModifiers.TryGetValue(typeof(IDamageTaken), out overrides)) {
                foreach (IDamageTaken overrideModifier in overrides) {
                    damage = overrideModifier.DamageTaken(damage, damageType);
                }
            }
            if (justKidding)return damage;
            else CurHitPoints -= (int)damage;
            return damage;
        }

        public double PowerEstimate {
            get {
                double powerDifferenceInPercent = CalculateRelativePower(MeasureStickJoe.Instance);
                double powerFloor = -20;
                double powerRoof = 0;
                return Interpolate(powerFloor, 0, powerRoof, 200, powerDifferenceInPercent);
            }
        }

        private double Interpolate(double x0, double y0, double x1, double y1, double x) {
            double slope = (y1 - y0) / (x1 - x0);
            return y0 + slope * (x - x0);
        }

        public virtual double CalculateRelativePower(BaseCharacter enemy) { //Idea: maybe rework this to instead be based on minimum amount of rounds where the confidence interval of having beaten the opponent by that point is > 50%? The current model based on averages does not take into account the chance of happening to hit all attacks (expectedDamage is irrelevant then since it is based on hitChance) and/or only rolling high damage
            double averageAttackRoll = AttackRoll(rollMax: true) - DiceRoller.averageRoll[DiceRoller.TwentySidedDie];
            double averageAttackRollOfEnemy = enemy.AttackRoll(rollMax: true) - DiceRoller.averageRoll[DiceRoller.TwentySidedDie];
            double chanceOfHittingEnemy = Math.Min(Math.Max((21 - (enemy.ArmorClass - (AttackRoll(rollMax: true) - 20))) / 20.0, 0.05), 0.95);
            double enemyChanceOfHittingMe = Math.Min(Math.Max((21 - (ArmorClass - (enemy.AttackRoll(rollMax: true) - 20))) / 20.0, 0.05), 0.95);
            double averageDamage = DamageRoll(rollMax: true) - Weapon.DamageDie(true) + DiceRoller.averageRoll[Weapon.DamageDie];
            double enemyAverageDamage = enemy.DamageRoll(rollMax: true) - enemy.Weapon.DamageDie(true) + DiceRoller.averageRoll[enemy.Weapon.DamageDie];
            double expectedDamage = chanceOfHittingEnemy * enemy.ReceiveDamage(averageDamage, Weapon.DamageType, justKidding : true);
            double enemyExpectedDamage = enemyChanceOfHittingMe * ReceiveDamage(enemyAverageDamage, enemy.Weapon.DamageType, justKidding : true);
            double expectedRoundsToKillEnemy = enemy.MaxHitPoints / expectedDamage;
            double enemyExpectedRoundsToKillMe = MaxHitPoints / enemyExpectedDamage;
            double minInverse = Math.Min(1 / expectedRoundsToKillEnemy, 1 / enemyExpectedRoundsToKillMe);
            double maxInverse = Math.Max(1 / expectedRoundsToKillEnemy, 1 / enemyExpectedRoundsToKillMe);
            double normalizedInverse = (1 / expectedRoundsToKillEnemy) / minInverse;
            double enemyNormalizedInverse = (1 / enemyExpectedRoundsToKillMe) / minInverse;
            double maxNormalizedInverse = Math.Max(normalizedInverse, enemyNormalizedInverse);
            if (Program.Debugging) {
                Console.WriteLine($"{enemy.Name} expected attackAttempts to kill me: {enemyExpectedRoundsToKillMe} \n {Name} expected attackAttempts to kill enemy: {expectedRoundsToKillEnemy}");
                if (normalizedInverse > enemyNormalizedInverse) {
                    Console.WriteLine($"I'm {((maxNormalizedInverse - 1) * 100).ToString("n2")}% stronger.");
                } else {
                    Console.WriteLine($"{enemy.Name} is {((maxNormalizedInverse - 1) * 100).ToString("n2")}% stronger.");
                }
            }
            if (normalizedInverse > enemyNormalizedInverse) {
                return (maxNormalizedInverse - 1); //I'm stronger than the enemy
            } else {
                return -(maxNormalizedInverse - 1);
            }
        }

        protected virtual Tuple<double, int, int> SimulateFights(BaseCharacter enemy, int simulations = 1) {
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
                var attributesToDisplay = new Dictionary<Attribute, int>(attributes);
                attributesToDisplay[Attribute.ArmorClass] = ArmorClassFromArmor;
                foreach (Attribute a in attributesToDisplay.Keys) {
                    int totalModifier = AddAllModifiers(a);
                    description += $"  {a}: {attributesToDisplay[a] + totalModifier}";
                    if (totalModifier > 0)description += $" ({attributesToDisplay[a]}+{totalModifier})";
                    else if (totalModifier < 0)description += $" ({attributesToDisplay[a]}-{totalModifier})";
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
                        } else description += " empty";
                        description += "\n";
                    }
                }
                foreach (Modifier m in Modifiers) {
                    if (alreadyProcessed.Contains(m))continue;
                    description += $"{m}\n";
                }
            }
            description += $"Estimated Combat Prowess: {PowerEstimate}";
            return description;
        }
    }

    public class Player : BaseCharacter {
        public Player(string name, Genders gender, Race race, bool overrideStats = false) : base(name, gender, race) {
            EquipBestEquipmentWithinBudget(budgetPerSlot: 350);
            //Remove, it's just for debug purposes
            if (overrideStats) {
                EquipItem(new GreatAxe());
                EquipItem(new LoricaSquamata());
                AddModifier(new HeavyArmorMaster());

                attributes[Attribute.Strength] = 16;
                attributes[Attribute.Dexterity] = 14;
                attributes[Attribute.Constitution] = 16;
                attributes[Attribute.Intelligence] = 10;
                attributes[Attribute.Wisdom] = 10;
                attributes[Attribute.Charisma] = 10;
                attributes[Attribute.MaxHitPoints] = HitDieType(true) + ConstitutionMod + ConstitutionMod;
            }
        }
    }

    public class Champion : BaseCharacter {
        public override void GenerateAttributes(int bonus = 2) {
            base.GenerateAttributes(bonus);
        }
    }

    public class Gladiator : BaseCharacter {

    }

    public class Soldier : BaseCharacter {
        public override void GenerateAttributes(int bonus = -2) {
            base.GenerateAttributes(bonus);
        }
    }

    public class Novice : BaseCharacter {
        public override void GenerateAttributes(int bonus = -4) {
            base.GenerateAttributes(bonus);
        }
    }

    public class MeasureStickJoe : BaseCharacter {
        public static MeasureStickJoe Instance { get; } = new MeasureStickJoe();

        public MeasureStickJoe() : base("MeasureStick Joe", Genders.Male, new MountainDwarf()) {
            EquipItem(new GreatAxe());
            EquipItem(new LoricaSquamata());

            attributes[Attribute.Strength] = 16;
            attributes[Attribute.Dexterity] = 14;
            attributes[Attribute.Constitution] = 16;
            attributes[Attribute.Intelligence] = 10;
            attributes[Attribute.Wisdom] = 10;
            attributes[Attribute.Charisma] = 10;
            attributes[Attribute.MaxHitPoints] = HitDieType(true) + ConstitutionMod + ConstitutionMod;
        }
    }
}
