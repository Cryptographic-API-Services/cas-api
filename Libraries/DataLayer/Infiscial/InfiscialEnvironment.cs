﻿using Infisical.Sdk;
using System;

namespace DataLayer.Infiscial
{
    public static class InfiscialEnvironment
    {
        public static string GetSecretFromStorage(string secretName)
        {
            ClientSettings settings = new ClientSettings
            {
                Auth = new AuthenticationOptions
                {
                    UniversalAuth = new UniversalAuthMethod
                    {
                        ClientId = Environment.GetEnvironmentVariable("INFISICAL_CLIENT_ID"),
                        ClientSecret = Environment.GetEnvironmentVariable("INFISICAL_CLIENT_SECRET")
                    }
                }
            };

            var infisicalClient = new InfisicalClient(settings);

            var getSecretOptions = new GetSecretOptions
            {
                SecretName = secretName,
                ProjectId = Environment.GetEnvironmentVariable("INFISICAL_PROJECT_ID"),
                Environment = Environment.GetEnvironmentVariable("INFISICAL_ENVIRONMENT"),
            };
            var secret = infisicalClient.GetSecret(getSecretOptions);
            return secret.SecretValue;
        }
    }
}
