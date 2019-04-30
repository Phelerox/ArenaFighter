using ArenaFighter.Character;
using System;
using System.Collections.Generic;

namespace ArenaFighter
{
    public static class ConsoleApp
    {
        private static IEnumerable<Campaign> previousCampaigns = new List<Campaign>();
        private static Campaign savedUnfinishedCampaigns = null;
        private static Campaign currentCampaign = new Campaign();

        private static void Main(string[] args)
        {
            currentCampaign = new Campaign();
            //Console.WriteLine(DiceRoller.Roll4d6DropLowest());
            Player player = new Player("Herodotus");
            Console.WriteLine(player);
            Champion champion = new Champion();
            Console.WriteLine(champion);
            Console.ReadLine();
        }

    }
}
