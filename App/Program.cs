using System;
using System.IO;
using Core;

namespace App
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] assemblyProtected = Protector.Protect(File.ReadAllBytes(args[0]));

            File.WriteAllBytes(Guid.NewGuid().ToString() + ".exe", assemblyProtected);

        }
    }
}
