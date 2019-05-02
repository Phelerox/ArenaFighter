using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using ArenaFighter.Model;


namespace ArenaFighter.Model
{
    public class Campaign
    {
        public Campaign()
        {
            Player = new Player("Player 1");

        }

        public IEnumerable<Battle> PastBattles { get; set; }

        public BaseCharacter Player { get; set; }
    }
}
