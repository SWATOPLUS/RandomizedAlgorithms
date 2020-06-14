using System;
using System.IO;
using System.Linq;
using System.Text;

namespace RA72CarmichaelNumber
{
    internal static class Program
    {
        private const string InputFileName = "carmichael.in";
        private const string OutputFileName = "carmichael.out";

        private static void Main()
        {
            var lines = File.ReadAllLines(InputFileName);
            var numbers = lines
                .Skip(1)
                .Select(ulong.Parse);

            var random = new Random(42);
            var outputBuilder = new StringBuilder();

            foreach (var number in numbers)
            {
                var result = number.IsCarmichael(random, 10);

                outputBuilder.AppendLine(result ? "Yes" : "No");
            }

            File.WriteAllText(OutputFileName, outputBuilder.ToString());
        }
    }

    public static class LongExtension
    {
        public static bool IsCarmichael(this ulong number, Random random, uint? iterations = null)
        {
            if (number < 9 || number % 2 == 0 || number.IsProbablePrime(random, iterations))
            {
                return false;
            }

            var k = iterations ?? (uint)Math.Log(number);

            for (var i = 0; i < k; i++)
            {
                var tester = random.GetRandomCoprime(number);

                if (PowMod(tester, number - 1, number) != 1)
                {
                    return false;
                }
            }

            return true;
        }

        private static ulong GetRandomCoprime(this Random random, ulong number)
        {
            while (true)
            {
                var result = random.NextULong(2, number - 1);

                if (number.GreatestCommonDivisor(result) == 1)
                {
                    return result;
                }
            }
        }

        private static ulong GreatestCommonDivisor(this ulong a, ulong b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                {
                    a %= b;
                }
                else
                {
                    b %= a;
                }
            }

            return a + b;
        }

        private static ulong MultiplyMod(this ulong a, ulong b, ulong mod)
        {
            unchecked
            {
                var result = 0ul;
                a %= mod;
                b %= mod;

                while (b != 0)
                {
                    if ((b & 1) != 0)
                    {
                        result += a;
                        result %= mod;
                    }

                    b >>= 1;

                    if (a < mod - a)
                    {
                        a <<= 1;
                    }
                    else
                    {
                        a -= (mod - a);
                    }
                }

                return result;
            }
        }

        private static ulong PowMod(this ulong a, ulong pow, ulong mod)
        {
            unchecked
            {
                var result = 1ul;

                while (pow != 0)
                {
                    if ((pow & 1) != 0)
                    {
                        pow--;
                        result = MultiplyMod(result, a, mod);
                    }
                    else
                    {
                        a = MultiplyMod(a, a, mod);
                        pow >>= 1;
                    }
                }

                return result;
            }
        }

        public static bool IsProbablePrime(this long number, Random random, uint? iterations = null)
        {
            if (number < 1)
            {
                return false;
            }

            return IsProbablePrime((ulong)number, random, iterations);
        }

        public static bool IsProbablePrime(this ulong number, Random random, uint? iterations = null)
        {
            if (number == 2 || number == 3)
            {
                return true;
            }

            if (number < 2 || number % 2 == 0)
            {
                return false;
            }

            var s = 0u;
            var t = number - 1;

            while (t % 2 == 0)
            {
                s++;
                t /= 2;
            }

            var k = iterations ?? (uint)Math.Log(number);

            for (var i = 0; i < k; i++)
            {
                var tester = random.NextULong(2, number - 2);

                if (!TestProbablePrime(s, t, number, tester))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TestProbablePrime(uint s, ulong t, ulong n, ulong a)
        {
            var x = a.PowMod(t, n);

            if (x == 1 || x == n - 1)
            {
                return true;
            }

            for (var j = 1; j < s; j++)
            {
                x = x.PowMod(2, n);

                if (x == 1)
                {
                    return false;
                }

                if (x == n - 1)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static class RandomExtensions
    {
        public static ulong NextULong(this Random random, ulong min, ulong max)
        {
            if (max <= min)
            {
                throw new ArgumentOutOfRangeException(nameof(max),
                    $"{nameof(max)} must be greater than or equal {nameof(min)}!");
            }

            return random.NextULong() % (max - min) + min;
        }

        public static ulong NextULong(this Random random)
        {
            var bytes = new byte[sizeof(ulong)];
            random.NextBytes(bytes);

            return BitConverter.ToUInt64(bytes);
        }
    }

}
