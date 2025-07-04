using CASHelpers;
using Common.UniqueIdentifiers;
using DataLayer.Mongo.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace API.ControllerLogic
{
    public class ApiKeyControllerLogic : IApiKeyControllerLogic
    {
        private readonly ICASExceptionRepository _exceptionRepository;

        private readonly IUserRepository _userRepository;
        public ApiKeyControllerLogic(
            ICASExceptionRepository exceptionRepository,

            IUserRepository userRepository
            )
        {
            this._exceptionRepository = exceptionRepository;

            this._userRepository = userRepository;
        }

        public async Task<IActionResult> RegenerateApiKey(HttpContext context)
        {

            IActionResult result = null;
            try
            {
                Generator generator = new Generator();
                string newApiKey = generator.CreateApiKey();
                string userId = context.Items[Constants.HttpItems.UserID].ToString();
                await this._userRepository.UpdateApiKeyByUserId(userId, newApiKey);
                return new OkObjectResult(new { ApiKey = newApiKey });
            }
            catch (Exception ex)
            {
                await this._exceptionRepository.InsertException(ex.ToString(), MethodBase.GetCurrentMethod().Name);
                result = new BadRequestObjectResult(new { error = "There was an error on our end" });
            }


            return result;
        }
    }
}