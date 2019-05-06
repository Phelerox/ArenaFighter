using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using ArenaFighter.Models;
using ArenaFighter.Models.Utils;


namespace ArenaFighter.Models
{
    public class Campaign
    {
        public BaseCharacter measureStickJoe = new Player("Joeee", Genders.Male, new MountainDwarf());//new MeasureStickJoe();
        public ulong DaysPassed { get; }
        public Campaign(string playerName, Genders gender, Race race) {
            Player = new Player(playerName, gender, race);
            Console.WriteLine(measureStickJoe);
            Player.CalculateRelativePower(measureStickJoe);
        }

        public IEnumerable<Battle> PastBattles { get; set; }

        public Player Player { get; set; }
    }
}
