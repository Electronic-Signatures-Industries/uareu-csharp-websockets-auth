using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecurityDriven.Inferno.Extensions;
using System.Security.Cryptography;
using System.IO;

namespace DSS.LicenseEngine.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GenerateKeyPair()
        {
            CngKey dsaKeyPrivate = CngKeyExtensions.CreateNewDsaKey(); // generate DSA keys
            byte[] dsaKeyPrivateBlob = dsaKeyPrivate.GetPrivateBlob(); // export private key as bytes
            byte[] dsaKeyPublicBlob = dsaKeyPrivate.GetPublicBlob(); // export public key as bytes

            FileStream stream = new FileStream("key_pub", FileMode.Create);
            stream.Write(dsaKeyPublicBlob, 0, dsaKeyPublicBlob.Length);
            stream.Close();

            stream = new FileStream("key_pvk", FileMode.Create);
            stream.Write(dsaKeyPrivateBlob, 0, dsaKeyPrivateBlob.Length);
            stream.Close();

        }

        [TestMethod]
        public void TestSign() {
            CngKey dsaKeyPrivate = CngKeyExtensions.CreateNewDsaKey(); // generate DSA keys
            byte[] dsaKeyPrivateBlob = dsaKeyPrivate.GetPrivateBlob(); // export private key as bytes
            byte[] dsaKeyPublicBlob = dsaKeyPrivate.GetPublicBlob(); // export public key as bytes

            LicenseEngine.LicenseService licenser = new LicenseService();
            var signature = licenser.Sign(dsaKeyPrivateBlob, "{ok:1}");

            Assert.IsTrue(signature.Length == 128);
        }

        [TestMethod]
        public void TestVerifyOK() {
            CngKey dsaKeyPrivate = CngKeyExtensions.CreateNewDsaKey(); // generate DSA keys
            byte[] dsaKeyPrivateBlob = dsaKeyPrivate.GetPrivateBlob(); // export private key as bytes
            byte[] dsaKeyPublicBlob = dsaKeyPrivate.GetPublicBlob(); // export public key as bytes

            var license = "{ok:1}";
            LicenseEngine.LicenseService licenser = new LicenseService();
            var signature = licenser.Sign(dsaKeyPrivateBlob, license);

            Assert.IsTrue(signature.Length == 128);

            var isValid = licenser.Verify(dsaKeyPublicBlob, signature, license);
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void TestVerifyFail() {
            CngKey dsaKeyPrivate = CngKeyExtensions.CreateNewDsaKey(); // generate DSA keys
            byte[] dsaKeyPrivateBlob = dsaKeyPrivate.GetPrivateBlob(); // export private key as bytes
            byte[] dsaKeyPublicBlob = dsaKeyPrivate.GetPublicBlob(); // export public key as bytes

            var license = "{ok:1}";
            LicenseEngine.LicenseService licenser = new LicenseService();
            var signature = licenser.Sign(dsaKeyPrivateBlob, license);

            Assert.IsTrue(signature.Length == 128);
            license = "{ok:5}";
            var isValid = licenser.Verify(dsaKeyPublicBlob, signature, license);
            Assert.IsFalse(isValid);
        }

    }
}
