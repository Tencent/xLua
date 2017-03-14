using System.Security.Cryptography;
using System;

namespace XLua
{
    public class SignatureLoader
    {
        private LuaEnv.CustomLoader userLoader;
        RSACryptoServiceProvider rsa;
        SHA1 sha;

        public SignatureLoader(string publicKey, LuaEnv.CustomLoader loader)
        {
            rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            sha = new SHA1CryptoServiceProvider();
            userLoader = loader;
        }

        byte[] load_and_verify(ref string filepath)
        {
            byte[] data = userLoader(ref filepath);
            if (data == null)
            {
                return null;
            }
            if (data.Length < 128)
            {
                throw new InvalidProgramException(filepath + " length less than 128!");
            }

            byte[] sig = new byte[128];
            byte[] filecontent = new byte[data.Length - 128];
            Array.Copy(data, sig, 128);
            Array.Copy(data, 128, filecontent, 0, filecontent.Length);


            if (!rsa.VerifyData(filecontent, sha, sig))
            {
                throw new InvalidProgramException(filepath + " has invalid signature!");
            }

            return filecontent;
        }

        public static implicit operator LuaEnv.CustomLoader(SignatureLoader signatureLoader)
        {
            return signatureLoader.load_and_verify;
        }
    }
}