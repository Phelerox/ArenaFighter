using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ArenaFighter.Models;
using ArenaFighter.Models.Utils;

namespace ArenaFighter.Models {
    public class Campaign {
        public ulong DaysPassed { get; }
        protected List<Battle> battles = new List<Battle>();
        public Campaign(string playerName, Genders gender, Race race) {
            Player = new Player(playerName, gender, race);
            Console.WriteLine(MeasureStickJoe.Instance);
            //Console.WriteLine(Player.PowerEstimate);
        }

        public IEnumerable<Battle> PastBattles {
            get { return battles; }
        }

        public Player Player { get; }
    }
}
