using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LowLevelDesign.Hexify;

namespace filehash
{
    class Program
    {
        static void Main()
        {
            Console.Write("A path to a file: ");
            var path = Console.ReadLine();

            if (!File.Exists(path))
            {
                Console.WriteLine($"'{path}' does not point to a file");
            }
            else
            {
                foreach (var (hashName, hashValue) in ComputeHash(path))
                {
                    Console.WriteLine($"{hashName} = {Hex.ToHexString(hashValue)}");
                }
            }
        }

        public static (string hashName, byte[] hashValue)[] ComputeHash(string filePath)
        {
            var sw = new Stopwatch();
            using var stream = File.OpenRead(filePath);
            var rnd = new Random();
            var hashes = new HashAlgorithm[] { MD5.Create(), SHA1.Create(), SHA256.Create() };
            byte[] buffer = new byte[4096];

            var readFileTimeTicks = 0L;
            var hashTimeTicks = 0L;

            while (true)
            {
                sw.Restart();
                var bytesRead = stream.Read(buffer);
                readFileTimeTicks += sw.ElapsedTicks;

                if (bytesRead != buffer.Length)
                {
                    sw.Restart();
                    foreach (var hash in hashes)
                    {
                        hash.TransformFinalBlock(buffer, 0, bytesRead);
                    }
                    hashTimeTicks += sw.ElapsedTicks;

                    Console.WriteLine($"Read file: {TimeSpan.FromTicks(readFileTimeTicks).TotalMilliseconds:0.00} ms");
                    Console.WriteLine($"Hash file: {TimeSpan.FromTicks(hashTimeTicks).TotalMilliseconds:0.00} ms");

                    return hashes.Select(h => (h.GetType().FullName, h.Hash)).ToArray();
                }
                sw.Restart();
                foreach (var hash in hashes)
                {
                    hash.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                }
                hashTimeTicks += sw.ElapsedTicks;
            }
        }
    }
}
