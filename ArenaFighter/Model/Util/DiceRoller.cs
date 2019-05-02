using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ArenaFighter.Model.Util
{
    //Based on https://stackoverflow.com/a/42426750
    public static class DiceRoller
    {
        private static readonly RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider();

        public static int TwentySidedDie()
        {
            return Next(1, 21);
        }

        public static int TenSidedDie()
        {
            return Next(1, 11);
        }

        public static int EightSidedDie()
        {
            return Next(1, 9);
        }

        public static int SixSidedDie()
        {
            return Next(1, 7);
        }

        public static int FourSidedDie()
        {
            return Next(1, 5);
        }



        public static int Roll4d6DropLowest()
        {
            int sum = 0;
            int lowest = int.MaxValue;
            for (int i = 0;i<4;i++)
            {
                int roll = SixSidedDie();
                sum += roll;
                if (roll < lowest)
                    lowest = roll;
            }
            return sum - lowest;
        }

        public static Tuple<ulong,IEnumerable<int>> RollNTimes(int n, Func<int> die_type)
        {
            ulong sum = 0;
            List<int> rolls = new List<int>();
            for (int i = 0;i<n;i++)
            {
                int roll = die_type();
                sum += (ulong) roll;
                rolls.Add(roll);
            }
            return new Tuple<ulong,IEnumerable<int>>(sum, rolls);
        }

        public static int Next(int minValue, int maxExclusiveValue)
        {
            if (minValue >= maxExclusiveValue)
                throw new ArgumentOutOfRangeException("minValue must be lower than maxExclusiveValue");

            long diff = (long)maxExclusiveValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;

            uint ui;
            do
            {
                ui = GetRandomUInt();
            } while (ui >= upperBound);
            return (int)(minValue + (ui % diff));
        }

        private static uint GetRandomUInt()
        {
            var randomBytes = GenerateRandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(randomBytes, 0);
        }

        private static byte[] GenerateRandomBytes(int intsNumber)
        {
            byte[] buffer = new byte[intsNumber];
            csp.GetBytes(buffer);
            return buffer;
        }
    }
}
