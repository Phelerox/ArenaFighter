using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ArenaFighter.Models;

namespace ArenaFighter.Models.Utils {
    public sealed class Reflection {
        private static readonly Lazy<Reflection> lazy = new Lazy<Reflection>(() => new Reflection());
        public static Reflection instance;

        public static Reflection Instance() {
            return lazy.Value;
        }

        Assembly runningAssembly = Assembly.GetExecutingAssembly();

        Type equipmentType = typeof(Equipment);

        IDictionary<Slot, List<Type>> allEquipmentTypes = new Dictionary<Slot, List<Type>>();
        IDictionary<Slot, List<Equipment>> allEquipment = new Dictionary<Slot, List<Equipment>>();
        IDictionary<Slot, int[]> allEquipmentRarityLookupTable = new Dictionary<Slot, int[]>();
        private Reflection() {
            foreach (Slot s in Enum.GetValues(typeof(Slot))) {
                allEquipmentTypes[s] = new List<Type>();
                allEquipment[s] = new List<Equipment>();
            }
            foreach (var type in runningAssembly.GetTypes()) {
                if (equipmentType.IsAssignableFrom(type) && !type.IsAbstract) {
                    var e = (Equipment)Activator.CreateInstance(type);
                    allEquipment[e.Slot].Add(e);
                    allEquipmentTypes[e.Slot].Add(type);
                    //Console.WriteLine(e);
                }
            }
            SortedList<int, Equipment> abundanceSorting = new SortedList<int, Equipment>();
            foreach (List<Equipment> equipmentForSlot in allEquipment.Values) {
                equipmentForSlot.Sort(Comparer<Equipment>.Create((a, b) => a.Price.CompareTo(b.Price))); //.OrderBy(e => e.Price);
                if (Program.Debugging) {
                    foreach (Equipment e in equipmentForSlot) {
                        if (!abundanceSorting.ContainsKey(e.Abundance))abundanceSorting.Add(e.Abundance, e);
                    }
                }
            }
            if (Program.Debugging) {
                foreach ((int a, Equipment e)in abundanceSorting) {
                    Console.WriteLine($"{e.ToString()} | Price: {e.Price} | Abundance: {a}");
                }
            }
            ComputeRandomWeightLists();
            instance = this;
        }

        public IDictionary<Slot, Equipment> GenerateRandomEquipment(int qualityBias, Slot? forSlot = null) {
            var dictionary = new Dictionary<Slot, Equipment>();
            dynamic slots = new List<Slot>();
            if (forSlot == null)slots = Enum.GetValues(typeof(Slot));
            else slots.Add((Slot)forSlot);
            foreach (Slot s in slots) {
                var list = allEquipment[s];
                //if (list.Count == 1) dictionary[s] = list[0];
                if (list.Count > 1)dictionary[s] = SelectRandomEquipment(s);
                else Console.WriteLine($"Found no {s}!");
            }
            return dictionary;
        }

        private void ComputeRandomWeightLists() {
            //Source: https://stackoverflow.com/a/9141726
            foreach (Slot slot in allEquipment.Keys) {
                List<Equipment> equipmentForSlot;
                if (allEquipment.TryGetValue(slot, out equipmentForSlot) && equipmentForSlot.Count > 0) {
                    int[] lookup = new int[equipmentForSlot.Count];
                    lookup[0] = (equipmentForSlot[0].Abundance) - 1;
                    for (int i = 1; i < lookup.Length; i++) {
                        lookup[i] = lookup[i - 1] + equipmentForSlot[i].Abundance;
                    }
                    allEquipmentRarityLookupTable[slot] = lookup;
                }
            }
        }

        public Equipment SelectRandomEquipment(Slot forSlot) {
            IList<Equipment> alternatives = allEquipment[forSlot];
            if (alternatives.Count < 1)return null;
            int[] lookup = allEquipmentRarityLookupTable[forSlot];
            int total = lookup[lookup.Length - 1];
            int chosen = DiceRoller.Next(0, total);
            int index = Array.BinarySearch(lookup, chosen);
            if (index < 0) //exact value not found
                index = ~index; //bitwise complement will convert a negative index into the index of the "closest" value found
            return alternatives[index];
        }

        public IDictionary<Slot, Equipment> SelectBestEquipmentWithinBudget(int budgetPerSlot, IDictionary<Slot, int> budgetTable = null, BaseCharacter receivingCharacter = null) {
            var selected = new Dictionary<Slot, Equipment>();
            foreach (Slot slot in allEquipment.Keys) {
                List<Equipment> equipmentForSlot;
                if (allEquipment.TryGetValue(slot, out equipmentForSlot) && equipmentForSlot.Count > 0) {
                    int budgetForSlot = budgetPerSlot;
                    if (budgetTable != null && budgetTable.ContainsKey(slot)) {
                        budgetForSlot = budgetTable[slot];
                    }

                    selected[slot] = SelectBestEquipmentWithinBudget(budgetForSlot, slot);
                }
            }
            return selected;
        }

        public Equipment SelectBestEquipmentWithinBudget(int budget, Slot forSlot, BaseCharacter receivingCharacter = null) {
            //TODO: For Slot.Armor, try to get highest AC (taking into MaxDexBonus and Character DexMod) if receivingCharacter != null
            return allEquipment[forSlot].TakeWhile((e) => e.Price <= budget).Last();
        }

    }
}
