using System;
using System.IO;
using System.Security.Cryptography;

namespace XLua
{
    public class KeyPairsGen
    {
        private static string exportPrivateKeyToPEMFormat(RSAParameters parameters)
        {
            string privateKey = "";
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    encodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                    encodeIntegerBigEndian(innerWriter, parameters.Modulus);
                    encodeIntegerBigEndian(innerWriter, parameters.Exponent);
                    encodeIntegerBigEndian(innerWriter, parameters.D);
                    encodeIntegerBigEndian(innerWriter, parameters.P);
                    encodeIntegerBigEndian(innerWriter, parameters.Q);
                    encodeIntegerBigEndian(innerWriter, parameters.DP);
                    encodeIntegerBigEndian(innerWriter, parameters.DQ);
                    encodeIntegerBigEndian(innerWriter, parameters.InverseQ);
                    var length = (int)innerStream.Length;
                    encodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                privateKey += "-----BEGIN RSA PRIVATE KEY-----\r\n";
                // Output as Base64 with lines chopped at 64 characters
                for (var i = 0; i < base64.Length; i += 64)
                {
                    privateKey += new string(base64, i, Math.Min(64, base64.Length - i));
                    privateKey += "\r\n";
                }
                privateKey += "-----END RSA PRIVATE KEY-----\r\n";
            }
            return privateKey;
        }

        private static void encodeLength(BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }
                stream.Write((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }

        private static void encodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }
            if (value.Length - prefixZeros == 0)
            {
                encodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    encodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    encodeLength(stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }

        public static void Main(string[] args)
        {
            if (File.Exists("key_ras") || File.Exists("key_ras.pub"))
            {
                Console.WriteLine("key pairs existed!");
            }
            var rsa = new RSACryptoServiceProvider();
            File.WriteAllText("key_ras", rsa.ToXmlString(true));
            File.WriteAllText("key_ras.pub", rsa.ToXmlString(false));
            if (args.Length > 0 && args[0] == "pem")
            {
                File.WriteAllText("key_ras.pem", exportPrivateKeyToPEMFormat(rsa.ExportParameters(true)));
                File.WriteAllText("key_ras.pub.pem", exportPrivateKeyToPEMFormat(rsa.ExportParameters(false)));
            }
        }
    }
}