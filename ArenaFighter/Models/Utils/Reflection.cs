using System.Collections.Generic;
using System.Reflection;
using System;

using ArenaFighter.Models;

namespace ArenaFighter.Models.Utils
{
    public sealed class Reflection
    {
        private static readonly Lazy<Reflection> lazy = new Lazy<Reflection>(() => new Reflection());
        public static Reflection instance;

        public static Reflection Instance()
        {
            return lazy.Value;
        }


        Assembly runningAssembly = Assembly.GetExecutingAssembly();

        Type equipmentType = typeof(Equipment);

        IDictionary<Slot,List<Type>> allEquipmentTypes = new Dictionary<Slot,List<Type>>();
        IDictionary<Slot,List<Equipment>> allEquipment = new Dictionary<Slot,List<Equipment>>();

        private Reflection() {
            foreach ( Slot s in Enum.GetValues(typeof(Slot))) {
                allEquipmentTypes[s] = new List<Type>();
                allEquipment[s] = new List<Equipment>();
            }
            foreach (var type in runningAssembly.GetTypes()) {
                if (equipmentType.IsAssignableFrom(type) && !type.IsAbstract){
                    var e = (Equipment)Activator.CreateInstance(type);
                    allEquipment[e.Slot].Add(e);
                    allEquipmentTypes[e.Slot].Add(type);
                    //Console.WriteLine(e);
                }
            }
            instance = this;
        }

        public IDictionary<Slot,Equipment> GenerateRandomEquipment(int quality, Slot? forSlot=null) {
            var dictionary = new Dictionary<Slot, Equipment>();
            dynamic slots = new List<Slot>();
            if (forSlot == null) slots = Enum.GetValues(typeof(Slot));
            else slots.Add((Slot)forSlot);
            foreach (Slot s in slots) {
                var list = allEquipment[s];
                //if (list.Count == 1) dictionary[s] = list[0];
                if (list.Count > 1) dictionary[s] = list[DiceRoller.Next(0,list.Count)];
                else Console.WriteLine($"Found no {s}!");
            }
            return dictionary;
        }
    }
}