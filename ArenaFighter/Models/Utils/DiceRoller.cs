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
        public static readonly Dictionary<Func<bool,int>,string> dieNames = new Dictionary<Func<bool,int>, string>() {
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
        public static readonly Dictionary<Func<bool,int>,double> averageRoll = new Dictionary<Func<bool,int>, double>() {
            [CoinFlip] = 1.5,
            [ThreeSidedDie] = 2.0,
            [FourSidedDie] = 2.5,
            [SixSidedDie] = 3.5,
            [EightSidedDie] = 4.5,
            [TenSidedDie] = 5.5,
            [TwelveSidedDie] = 6.5,
            [TwentySidedDie] = 10.5,
            [TwoDTwo] = 3.0,
            [TwoDThree] = 4.0,
            [TwoDFour] = 5.0,
            [TwoDSix] = 7.0,
        };
        
        public static Func<bool,int>[] dieSizes = new Func<bool,int>[] {CoinFlip, ThreeSidedDie, FourSidedDie, SixSidedDie, EightSidedDie, TenSidedDie, TwelveSidedDie};
        public static Func<bool,int> enlargeDie(Func<bool,int> die) {
            int i = Array.FindIndex<Func<bool,int>>(dieSizes, d => d.Equals(die));
            if (i == -1 || i == (dieSizes.Length - 1)) return die;
            return dieSizes[i + 1];
        }
        public static Func<bool,int> shrinkDie(Func<bool,int> die) {
            int i = Array.FindIndex<Func<bool,int>>(dieSizes, d => d.Equals(die));
            if (i == -1 || i == 0) return die;
            return dieSizes[i - 1];
        }

        public static int TwentySidedDie(bool rollMax = false)
        {
            if (rollMax) return 20;
            return Next(1, 21);
        }

        public static int TwelveSidedDie(bool rollMax = false)
        {
            if (rollMax) return 12;
            return Next(1, 13);
        }

        public static int TenSidedDie(bool rollMax = false)
        {
            if (rollMax) return 10;
            return Next(1, 11);
        }

        public static int EightSidedDie(bool rollMax = false)
        {
            if (rollMax) return 8;
            return Next(1, 9);
        }

        public static int SixSidedDie(bool rollMax = false)
        {
            if (rollMax) return 6;
            return Next(1, 7);
        }

        public static int FourSidedDie(bool rollMax = false)
        {
            if (rollMax) return 4;
            return Next(1, 5);
        }

        public static int ThreeSidedDie(bool rollMax = false)
        {
            if (rollMax) return 3;
            return Next(1, 4);
        }

        public static int CoinFlip(bool rollMax = false)
        {
            if (rollMax) return 2;
            return Next(1, 3);
        }

        public static int TwoDSix(bool rollMax = false) {
            return SixSidedDie(rollMax) + SixSidedDie(rollMax);
        }

        public static int TwoDFour(bool rollMax = false) {
            return FourSidedDie(rollMax) + FourSidedDie(rollMax);
        }

        public static int TwoDThree(bool rollMax = false) {
            return ThreeSidedDie(rollMax) + ThreeSidedDie(rollMax);
        }

        public static int TwoDTwo(bool rollMax = false) {
            return CoinFlip(rollMax) + CoinFlip(rollMax);
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

        public static Tuple<ulong,IEnumerable<int>> RollNTimes(int n, Func<bool,int> die_type)
        {
            ulong sum = 0;
            List<int> rolls = new List<int>();
            for (int i = 0;i<n;i++)
            {
                int roll = die_type(false);
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
