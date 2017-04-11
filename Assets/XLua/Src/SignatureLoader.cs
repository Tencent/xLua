#if !UNITY_WSA || UNITY_EDITOR
using System.Security.Cryptography;
#else
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
#endif
using System;

namespace XLua
{
    public class SignatureLoader
    {
        private LuaEnv.CustomLoader userLoader;
#if !UNITY_WSA || UNITY_EDITOR
        RSACryptoServiceProvider rsa;
        SHA1 sha;
#else
        AsymmetricKeyAlgorithmProvider rsa;
        CryptographicKey key;
#endif

        public SignatureLoader(string publicKey, LuaEnv.CustomLoader loader)
        {
#if !UNITY_WSA || UNITY_EDITOR
            rsa = new RSACryptoServiceProvider();
            rsa.ImportCspBlob(Convert.FromBase64String(publicKey));
            sha = new SHA1CryptoServiceProvider();
#else
            rsa = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaSignPkcs1Sha1);
            key = rsa.ImportPublicKey(CryptographicBuffer.DecodeFromBase64String(publicKey), CryptographicPublicKeyBlobType.Capi1PublicKey);
#endif
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

#if !UNITY_WSA || UNITY_EDITOR
            if (!rsa.VerifyData(filecontent, sha, sig))
            {
                throw new InvalidProgramException(filepath + " has invalid signature!");
            }
#else
            if (!CryptographicEngine.VerifySignature(key, CryptographicBuffer.CreateFromByteArray(filecontent), CryptographicBuffer.CreateFromByteArray(sig)))
            {
                throw new InvalidProgramException(filepath + " has invalid signature!");
            }
#endif
            return filecontent;
        }


        public static implicit operator LuaEnv.CustomLoader(SignatureLoader signatureLoader)
        {
            return signatureLoader.load_and_verify;
        }
    }
}