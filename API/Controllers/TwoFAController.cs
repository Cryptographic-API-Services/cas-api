using API.ControllersLogic;
using Microsoft.AspNetCore.Mvc;
using Validation.Attributes;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TwoFAController : ControllerBase
    {
        private readonly ITwoFAControllerLogic _twoFAControllerLogic;
        public TwoFAController(ITwoFAControllerLogic twoFAControllerLogic)
        {
            this._twoFAControllerLogic = twoFAControllerLogic;
        }

        [HttpGet]
        [Route("Get2FAStatus")]
        [TypeFilter(typeof(ValidateJWTAttribute))]
        public async Task<IActionResult> Get2FAStatus()
        {
            return await this._twoFAControllerLogic.Get2FAStatus(HttpContext);
        }

        [HttpPut]
        [Route("TurnOn2FA")]
        [TypeFilter(typeof(ValidateJWTAttribute))]
        public async Task<IActionResult> TurnOn2FA()
        {
            return await this._twoFAControllerLogic.TurnOn2FA(HttpContext);
        }

        [HttpPut]
        [Route("TurnOff2FA")]
        [TypeFilter(typeof(ValidateJWTAttribute))]
        public async Task<IActionResult> TurnOff2FA()
        {
            return await this._twoFAControllerLogic.TurnOff2FA(HttpContext);
        }
    }
}
