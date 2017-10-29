using System;
using System.IO;
using System.Security.Cryptography;

namespace XLua
{
    public class KeyPairsGen
    {
        
        public static void Main(string[] args)
        {
            if (File.Exists("key_ras") || File.Exists("key_ras.pub"))
            {
                Console.WriteLine("key pairs existed!");
            }
            var rsa = new RSACryptoServiceProvider();
            File.WriteAllText("key_ras", rsa.ToXmlString(true));
            File.WriteAllText("key_ras.pub", Convert.ToBase64String(rsa.ExportCspBlob(false)));
        }
    }
}