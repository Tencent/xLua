using System;
using System.IO;
using System.Security.Cryptography;

namespace XLua
{
    public class FilesSignature
    {
        static void useage()
        {
            Console.WriteLine("FilesSignature from_path to_path");
        }

        static void doSignature(string from, string to, SHA1 sha, RSACryptoServiceProvider rsa)
        {
            if(!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            foreach(var filename in Directory.GetFiles(from, "*.lua"))
            {
                byte[] filecontent = File.ReadAllBytes(filename);
                byte[] sig = rsa.SignData(filecontent, sha);
                FileStream fs = new FileStream(to + "/" + Path.GetFileName(filename), FileMode.Create);
                fs.Write(sig, 0, sig.Length);
                fs.Write(filecontent, 0, filecontent.Length);
                fs.Close();
            }
            foreach(var dir in Directory.GetDirectories(from))
            {

                doSignature(dir, to + "/" + new DirectoryInfo(dir).Name, sha, rsa);
            }
        }

        public static void Main(string[] args)
        {
            if (!File.Exists("key_ras"))
            {
                Console.WriteLine("no key_ras!");
                return;
            }

            if (args.Length != 2)
            {
                useage();
                return;
            }

            SHA1 sha = new SHA1CryptoServiceProvider();
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(File.ReadAllText("key_ras"));
            doSignature(args[0], args[1], sha, rsa);
        }
    }
}