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
        public ulong DaysPassed { get; }
        public Campaign(string playerName, Genders gender, Race race)
        {
            Player = new Player(playerName, gender, race);
        }

        public IEnumerable<Battle> PastBattles { get; set; }

        public Player Player { get; set; }
    }
}
