﻿using System;
using System.Collections;
using System.IO;
using System.IO.Compression;

namespace WeylandMod.SharedMap
{
    internal static class SharedMapUtils
    {
        public static byte[] CompressExploredMap(bool[] input)
        {
            using (var memoryStream = new MemoryStream())
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
            {
                var buffer = RleEncode(ToBits(input));

                deflateStream.Write(buffer, 0, buffer.Length);
                deflateStream.Close();

                return memoryStream.ToArray();
            }
        }

        public static bool[] DecompressExploredMap(byte[] input)
        {
            using (var inputStream = new MemoryStream(input))
            using (var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                deflateStream.CopyTo(outputStream);
                deflateStream.Close();

                var buffer = FromBits(RleDecode(outputStream.ToArray()));

                return buffer;
            }
        }

        private static byte[] ToBits(bool[] input)
        {
            var buffer = new byte[input.Length / 8 + (input.Length % 8 == 0 ? 0 : 1)];
            new BitArray(input).CopyTo(buffer, 0);
            return buffer;
        }

        private static bool[] FromBits(byte[] input)
        {
            var buffer = new bool[input.Length * 8];
            new BitArray(input).CopyTo(buffer, 0);
            return buffer;
        }

        private static byte[] RleEncode(byte[] input)
        {
            if (input.Length == 0)
                return input;

            using (var outputStream = new MemoryStream())
            {
                var count = 1;
                var value = input[0];

                for (var index = 1; index < input.Length; ++index)
                {
                    if (count < int.MaxValue && value == input[index])
                    {
                        ++count;
                    }
                    else
                    {
                        outputStream.Write(BitConverter.GetBytes(count), 0, sizeof(int));
                        outputStream.WriteByte(value);

                        count = 1;
                        value = input[index];
                    }
                }

                outputStream.Write(BitConverter.GetBytes(count), 0, sizeof(int));
                outputStream.WriteByte(value);

                return outputStream.ToArray();
            }
        }

        private static byte[] RleDecode(byte[] input)
        {
            if (input.Length == 0)
                return input;

            using (var inputStream = new MemoryStream(input))
            using (var outputStream = new MemoryStream())
            {
                const int sizeofIntPlusByte = sizeof(int) + 1;
                var buffer = new byte[sizeofIntPlusByte];

                while (inputStream.Read(buffer, 0, sizeofIntPlusByte) > 0)
                {
                    var count = BitConverter.ToInt32(buffer, 0);
                    var value = buffer[sizeof(int)];

                    for (var index = 0; index < count; ++index)
                    {
                        outputStream.WriteByte(value);
                    }
                }

                return outputStream.ToArray();
            }
        }
    }
}