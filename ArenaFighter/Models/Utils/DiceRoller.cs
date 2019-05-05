using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ArenaFighter.Models.Utils
{
    //Based on https://stackoverflow.com/a/42426750
    public static class DiceRoller
    {
        private static readonly RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider();
        public static readonly Dictionary<Func<int>,string> dieNames = new Dictionary<Func<int>, string>() {
            [CoinFlip] = "1d2",
            [ThreeSidedDie] = "1d3",
            [FourSidedDie] = "1d4",
            [SixSidedDie] = "1d6",
            [EightSidedDie] = "1d8",
            [TenSidedDie] = "1d10",
            [TwelveSidedDie] = "1d12",
            [TwentySidedDie] = "1d20",
            [TwoDTwo] = "2d2",
            [TwoDThree] = "2d3",
            [TwoDFour] = "2d4",
            [TwoDSix] = "2d6",
        };
        
        public static Func<int>[] dieSizes = new Func<int>[] {CoinFlip, ThreeSidedDie, FourSidedDie, SixSidedDie, EightSidedDie, TenSidedDie, TwelveSidedDie};
        public static Func<int> enlargeDie(Func<int> die) {
            int i = Array.FindIndex<Func<int>>(dieSizes, d => d.Equals(die));
            if (i == -1 || i == (dieSizes.Length - 1)) return die;
            return dieSizes[i + 1];
        }
        public static Func<int> shrinkDie(Func<int> die) {
            int i = Array.FindIndex<Func<int>>(dieSizes, d => d.Equals(die));
            if (i == -1 || i == 0) return die;
            return dieSizes[i - 1];
        }

        public static int TwentySidedDie()
        {
            return Next(1, 21);
        }

        public static int TwelveSidedDie()
        {
            return Next(1, 13);
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

        public static int ThreeSidedDie()
        {
            return Next(1, 4);
        }

        public static int CoinFlip()
        {
            return Next(1, 2);
        }

        public static int TwoDSix() {
            return SixSidedDie() + SixSidedDie();
        }

        public static int TwoDFour() {
            return FourSidedDie() + FourSidedDie();
        }

        public static int TwoDThree() {
            return ThreeSidedDie() + ThreeSidedDie();
        }

        public static int TwoDTwo() {
            return CoinFlip() + CoinFlip();
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
