using CASHelpers;
using DataLayer.Mongo.Entities;
using DataLayer.Mongo.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace API.ControllersLogic
{
    public class TwoFAControllerLogic : ITwoFAControllerLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly ICASExceptionRepository _exceptionRepository;

        public TwoFAControllerLogic(
            IUserRepository userRepository,
            ICASExceptionRepository exceptionRepository
            )
        {
            this._userRepository = userRepository;
            this._exceptionRepository = exceptionRepository;

        }

        public async Task<IActionResult> Get2FAStatus(HttpContext httpContext)
        {

            IActionResult result = null;
            try
            {
                string userId = httpContext.Items[Constants.HttpItems.UserID].ToString();
                Email2FA status = await this._userRepository.GetEmail2FAStats(userId);
                result = new OkObjectResult(new { result = status.IsEnabled });
            }
            catch (Exception ex)
            {
                await this._exceptionRepository.InsertException(ex.ToString(), MethodBase.GetCurrentMethod().Name);
            }


            return result;
        }
        public async Task<IActionResult> TurnOff2FA(HttpContext httpContext)
        {

            IActionResult result = null;
            try
            {
                string userId = httpContext.Items[Constants.HttpItems.UserID].ToString();
                await this._userRepository.ChangeEmail2FAStatusToDisabled(userId);
                result = new OkResult();
            }
            catch (Exception ex)
            {
                await this._exceptionRepository.InsertException(ex.ToString(), MethodBase.GetCurrentMethod().Name);
                result = new BadRequestObjectResult(new { error = "There was an error on our end." });
            }


            return result;
        }

        public async Task<IActionResult> TurnOn2FA(HttpContext httpContext)
        {

            IActionResult result = null;
            try
            {
                string userId = httpContext.Items[Constants.HttpItems.UserID].ToString();
                await this._userRepository.ChangeEmail2FAStatusToEnabled(userId);
                result = new OkResult();
            }
            catch (Exception ex)
            {
                await this._exceptionRepository.InsertException(ex.ToString(), MethodBase.GetCurrentMethod().Name);
                result = new BadRequestObjectResult(new { error = "There was an error on our end." });
            }


            return result;
        }
    }
}
