using Microsoft.AspNetCore.Mvc;

namespace API.ControllersLogic
{
    public interface ITwoFAControllerLogic
    {
        public Task<IActionResult> Get2FAStatus(HttpContext httpContext);
        public Task<IActionResult> TurnOn2FA(HttpContext httpContext);
        public Task<IActionResult> TurnOff2FA(HttpContext httpContext);
    }
}
