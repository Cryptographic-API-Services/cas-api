using System;
using DataLayer.Infiscial;

namespace Email_Service
{
    internal class EmailServiceInfiscalEnvironment
    {
        public static void SetEnvironmentKeys()
        {
            Environment.SetEnvironmentVariable("Connection", InfiscialEnvironment.GetSecretFromStorage("CONNECTION"));
            Environment.SetEnvironmentVariable("DatabaseName", InfiscialEnvironment.GetSecretFromStorage("DATABASENAME"));
            Environment.SetEnvironmentVariable("RabbitMqUrl", InfiscialEnvironment.GetSecretFromStorage("RABBITMQURL"));
            Environment.SetEnvironmentVariable("UserCollectionName", InfiscialEnvironment.GetSecretFromStorage("USERCOLLECTIONNAME"));
            Environment.SetEnvironmentVariable("EmailApi", InfiscialEnvironment.GetSecretFromStorage("EMAILAPI"));
            Environment.SetEnvironmentVariable("Domain", InfiscialEnvironment.GetSecretFromStorage("DOMAIN"));
            Environment.SetEnvironmentVariable("Resender", InfiscialEnvironment.GetSecretFromStorage("RESENDER"));
        }
    }
}
