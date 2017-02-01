using SecurityDriven.Inferno.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DSS.LicenseEngine
{
    public class LicenseService
    {
        public LicenseService()
        {

        }

        public string Sign(byte[] key, string payload)
        {

            byte[] data = Encoding.UTF8.GetBytes(payload);
            byte[] signature = null;

            using (var ecdsa = new ECDsaCng(key.ToPrivateKeyFromBlob()) { HashAlgorithm = CngAlgorithm.Sha384 })
            {
                signature = ecdsa.SignData(data);
            }

            return Convert.ToBase64String(signature);
        }

        public bool  Verify(byte[] pubKey, string signature, string payload)
        {

            byte[] data = Encoding.UTF8.GetBytes(payload);
            byte[] signatureBytes = Convert.FromBase64String(signature);


            using (var ecdsa = new ECDsaCng(pubKey.ToPublicKeyFromBlob()) { HashAlgorithm = CngAlgorithm.Sha384 }) // verify DSA signature with public key
            {
                return ecdsa.VerifyData(data, signatureBytes);
            }
        }
    }
}
