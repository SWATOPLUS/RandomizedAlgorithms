using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace RA90PrimitiveRoot
{
    internal static class Program
    {
        private const string InputFileName = "primes.in";
        private const string OutputFileName = "roots.out";

        private static void Main()
        {
            var lines = File.ReadAllLines(InputFileName);
            var numbers = lines
                .Skip(1)
                .Select(uint.Parse);

            var random = new Random(42);
            var outputBuilder = new StringBuilder();

            foreach (var number in numbers)
            {
                var result = number.GetPrimitiveRoot(random, 100);

                outputBuilder.AppendLine(result?.ToString() ?? "-1");
            }

            File.WriteAllText(OutputFileName, outputBuilder.ToString());
        }
    }

    public static class LongExtension
    {
        private static IEnumerable<uint> GetPrimeFactors(this uint number)
        {
            var root = (uint) Math.Floor(Math.Sqrt(number));
            var remainPart = number;

            if (remainPart % 2 == 0)
            {
                while (remainPart % 2 == 0)
                {
                    remainPart /= 2;
                }

                yield return 2;
            }

            for (var i = 3u; i <= root; i+=2)
            {
                if (remainPart % i == 0)
                {
                    while (remainPart % i == 0)
                    {
                        remainPart /= i;
                    }

                    yield return i;
                }
            }

            if (remainPart != 1)
            {
                yield return remainPart;
            }
        }

        public static uint? GetPrimitiveRoot(this uint number, Random random, uint iterations)
        {
            if (number == 2)
            {
                return 1;
            }
            if (number == 3)
            {
                return 2;
            }

            var factors = GetPrimeFactors(number - 1)
                .ToArray();

            for (var i = 0; i <= iterations; i++)
            {
                var root = random.NextUInt(2, number - 1);

                if (IsPrimeRoot(root, number, factors))
                {
                    return root;
                }

            }
            return null;
        }

        private static bool IsPrimeRoot(uint root, uint number, uint[] factors)
        {
            foreach (var factor in factors)
            {
                if (PowMod(root, (number - 1) / factor, number) == 1)
                {
                    return false;
                }
            }

            return true;
        }

        private static uint MultiplyMod(this uint a, uint b, uint mod)
        {
            unchecked
            {
                var result = 0u;
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

        private static uint PowMod(this uint a, uint pow, uint mod)
        {
            return (uint)BigInteger.ModPow(a, pow, mod);
        }

        private static uint PowMod_Slow(this uint a, uint pow, uint mod)
        {
            unchecked
            {
                var result = 1u;

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
    }

    public static class RandomExtensions
    {
        public static uint NextUInt(this Random random, uint min, uint max)
        {
            if (max <= min)
            {
                throw new ArgumentOutOfRangeException(nameof(max),
                    $"{nameof(max)} must be greater than or equal {nameof(min)}!");
            }

            return random.NextUInt() % (max - min) + min;
        }

        public static uint NextUInt(this Random random)
        {
            var bytes = new byte[sizeof(uint)];
            random.NextBytes(bytes);

            return BitConverter.ToUInt32(bytes);
        }
    }

}
