﻿using CasDotnetSdk.Hashers;
using CasDotnetSdk.Hybrid;
using CasDotnetSdk.Hybrid.Types;
using System.Text;

namespace EmergencyKit
{
    public class EmergencyKitUtils
    {
        public CreateEmergencyKitResponse CreateEmergencyKit()
        {
            AESRSAHybridInitializer initalizer = new AESRSAHybridInitializer(256, 4096);
            HybridEncryptionWrapper hybridEncryption = new HybridEncryptionWrapper();
            SHAWrapper sha = new SHAWrapper();
            string newKitKey = Guid.NewGuid().ToString();
            AESRSAHybridEncryptResult encryptionResult = hybridEncryption.EncryptAESRSAHybrid(Encoding.UTF8.GetBytes(newKitKey.ToString()), initalizer);
            return new CreateEmergencyKitResponse()
            {
                Key = Convert.ToBase64String(sha.Hash512(Encoding.UTF8.GetBytes(newKitKey))),
                EncryptResult = encryptionResult,
                Initalizer = initalizer
            };
        }
    }
}
