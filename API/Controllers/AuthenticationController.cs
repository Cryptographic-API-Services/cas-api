﻿using API.ControllerLogic;
using CASHelpers.Types.HttpResponses.UserAuthentication;
using Microsoft.AspNetCore.Mvc;
using Models.UserAuthentication.AuthenticationController;
using Validation.Attributes;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationControllerLogic _authenicationControllerLogic;
        private readonly IHttpContextAccessor httpContextAccessor;
        public AuthenticationController(
            IAuthenticationControllerLogic authenicationControllerLogic,
            IHttpContextAccessor httpContextAccessor
            )
        {
            this._authenicationControllerLogic = authenicationControllerLogic;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [Route("OperatingSystemCacheStore")]
        [TypeFilter(typeof(ValidateJWTAttribute))]
        public async Task<IActionResult> StoreOperatingSystemInformationInCache([FromBody] OperatingSystemInformationCacheRequestBody body)
        {
            return await this._authenicationControllerLogic.StoreOperatingSystemInformationInCache(this.httpContextAccessor.HttpContext, body);
        }

        [HttpPut]
        [Route("OperatingSystemCacheRemove")]
        [TypeFilter(typeof(ValidateJWTAttribute))]
        public async Task<IActionResult> RemoveOperatingSystemInformationInCache()
        {
            return await this._authenicationControllerLogic.RemoveOperatingSystemInformationInCache(this.httpContextAccessor.HttpContext);
        }

        [HttpPost]
        [Route("DiffieHellmanAesKey")]
        [TypeFilter(typeof(ValidateJWTAttribute))]
        public async Task<IActionResult> DiffieHellmanAesKeyDerviationForSDK([FromBody] DiffieHellmanAesDerivationRequest body)
        {
            return await this._authenicationControllerLogic.DiffieHellmanAesKeyDerviationForSDK(this.httpContextAccessor.HttpContext, body);
        }
    }
}
