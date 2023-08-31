using System;
using System.IO;
using System.Security.Cryptography;

namespace XLua
{
    public class FilesSignature
    {
        static void usage()
        {
            Console.WriteLine("FilesSignature from_path to_path");
        }

        static void doSignature(string from, string to, SHA1 sha, RSACryptoServiceProvider rsa)
        {
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            foreach (var filename in Directory.GetFiles(from, "*.lua"))
            {
                byte[] filecontent = File.ReadAllBytes(filename);
                byte[] sig = rsa.SignData(filecontent, sha);
                string sigFilePath = Path.Combine(to, Path.GetFileName(filename));
                using (FileStream fs = new FileStream(sigFilePath, FileMode.Create))
                {
                    fs.Write(sig, 0, sig.Length);
                    fs.Write(filecontent, 0, filecontent.Length);
                    fs.Flush();
                }
            }
            foreach (var dir in Directory.GetDirectories(from))
            {
                string newDir = Path.Combine(to, new DirectoryInfo(dir).Name);
                doSignature(dir, newDir, sha, rsa);
            }
        }

        public static void Main(string[] args)
        {
            if (!File.Exists("key_rsa"))
            {
                Console.WriteLine("No key_rsa file found!");
                return;
            }

            if (args.Length != 2)
            {
                usage();
                return;
            }

            SHA1 sha = new SHA1CryptoServiceProvider();
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(File.ReadAllText("key_rsa"));
            doSignature(args[0], args[1], sha, rsa);
        }
    }
}
