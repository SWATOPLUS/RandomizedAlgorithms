using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RA12BadElement
{
    internal class Program
    {
        private const string InputFileName = "element.in";
        private const string OutputFileName = "element.out";
        private const int CheckIterations = 20;
        private const bool NoBadElementMode = false;

        private static void Main()
        {
            var lines = File.ReadAllLines(InputFileName);
            var queue = new Queue<string>(lines);
            var random = new Random(42);
            var outputBuilder = new StringBuilder();

            while (queue.Any())
            {
                var size = int.Parse(queue.Dequeue());

                if (size == 0)
                {
                    break;
                }

                var a = ParseMatrix(queue.Dequeue(), size);
                var b = ParseMatrix(queue.Dequeue(), size);
                var c = ParseMatrix(queue.Dequeue(), size);

                var result = FindBadElement(random, a, b, c, size);

                if (NoBadElementMode)
                {
                    if (result == null)
                    {
                        outputBuilder.AppendLine("YES");
                    }
                    else
                    {
                        outputBuilder.AppendLine("NO");
                    }

                }
                else
                {
                    if (result == null)
                    {
                        outputBuilder.AppendLine("No");
                    }
                    else
                    {
                        outputBuilder.AppendLine("Yes");
                        outputBuilder.AppendLine($"{result.Value.Item1 + 1} {result.Value.Item2 + 1}");
                    }
                }
            }

            File.WriteAllText(OutputFileName, outputBuilder.ToString());
        }

        private static (int, int)? FindBadElement(Random random, BitArray[] a, BitArray[] b, BitArray[] c, int size)
        {
            for (var i = 0; i < CheckIterations; i++)
            {
                var x = random.NextBitArray(size);
                var bx = MultiplyMatrixColumn(b, x);
                var abx = MultiplyMatrixColumn(a, bx);
                var cx = MultiplyMatrixColumn(c, x);

                var diffX = abx.FindDiffIndex(cx);

                if (diffX != -1)
                {
                    var indexX = size - diffX - 1;
                    var ab = MultiplyRowMatrix(a[indexX], b);
                    var diffY = ab.FindDiffIndex(c[indexX]);
                    var indexY = size - diffY - 1;

                    return (indexX, indexY);
                }
            }

            return null;
        }

        private static BitArray MultiplyMatrixColumn(BitArray[] matrix, BitArray column)
        {
            var result = new BitArray(matrix.Length);

            for (var i = 0; i < matrix.Length; i++)
            {
                var element = ((BitArray) matrix[i].Clone()).And(column);
                result.Set(matrix.Length - i - 1, element.CountSetBits() % 2 == 1);
            }

            return result;
        }

        private static BitArray MultiplyRowMatrix(BitArray r, BitArray[] matrix)
        {
            var result = new BitArray(matrix.Length);

            for (var i = 0; i < matrix.Length; i++)
            {
                if (r[i])
                {
                    result = result.Xor(matrix[matrix.Length - i - 1]);
                }
            }

            return result;
        }

        private static BitArray[] ParseMatrix(string matrix, int size)
        {
            var shift = (4 - size % 4) % 4;

            return matrix
                .Split(' ')
                .Select(ParseMatrixRow)
                .Select(x =>
                {
                    var array = x.RightShift(shift);
                    array.Length = size;

                    return array;
                })
                .ToArray();
        }

        private static BitArray ParseMatrixRow(string row)
        {
            var fixedRow = row.Length % 2 == 0 ? row : "0" + row;
            var leadZeroCount = (16 - fixedRow.Length % 16) % 16;
            var bytes = new byte[(leadZeroCount + fixedRow.Length) / 2];

            for (var i = 0; i < fixedRow.Length / 2; i++)
            {
                var low = fixedRow[fixedRow.Length - i * 2 - 1].ParseHex();
                var high = fixedRow[fixedRow.Length - i * 2 - 2].ParseHex();

                bytes[i] = (byte) (high * 16 + low);
            }

            return new BitArray(bytes);
        }
    }

    public static class RandomExtensions
    {
        public static BitArray NextBitArray(this Random random, int size)
        {
            var byteCount = size.DivUp(8);
            var bytes = new byte[byteCount];
            random.NextBytes(bytes);

            return new BitArray(bytes) {Length = size};
        }
    }

    public static class NumericExtensions
    {
        public static int DivUp(this int number, int divider)
        {
            if (number % divider == 0)
            {
                return number / divider;
            }

            return number / divider + 1;
        }
    }

    public static class StringExtensions
    {
        public static int ParseHex(this char c)
        {
            unchecked
            {
                if ('0' <= c && c <= '9')
                {
                    return c - '0';
                }

                if ('A' <= c && c <= 'F')
                {
                    return c - 'A' + 10;
                }

                return -1;
            }
        }
    }

    public static class CollectionExtensions
    {
        public static uint CountSetBits(this BitArray bitArray)
        {
            unchecked
            {
                var ints = new int[(bitArray.Count >> 5) + 1];
                bitArray.CopyTo(ints, 0);

                var count = 0;

                // fix for not truncated bits in last integer that may have been set to true with SetAll()
                ints[ints.Length - 1] &= ~(-1 << (bitArray.Count % 32));

                for (Int32 i = 0; i < ints.Length; i++)
                {

                    Int32 c = ints[i];

                    // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
                    unchecked
                    {
                        c -= ((c >> 1) & 0x55555555);
                        c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
                        c = ((c + (c >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
                    }

                    count += c;

                }

                return (uint) count;
            }
        }

        public static int FindDiffIndex(this BitArray a, BitArray b)
        {
            for (var i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
