﻿using API.ControllerLogic;
using CASHelpers.Types.HttpResponses.BenchmarkAPI;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BenchmarkSDKMethodController : ControllerBase
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IBenchmarkSDKMethodControllerLogic _benchmarkSDKMethodControllerLogic;
        public BenchmarkSDKMethodController(
            IHttpContextAccessor contextAccessor,
            IBenchmarkSDKMethodControllerLogic benchmarkSDKMethodControllerLogic
            )
        {
            this._contextAccessor = contextAccessor;
            this._benchmarkSDKMethodControllerLogic = benchmarkSDKMethodControllerLogic;
        }


        [HttpGet]
        [Route("GetUserBenchmarksByDays")]
        public async Task<IActionResult> GetUserBenchmarksByDays([FromQuery]int daysAgo)
        {
            return await this._benchmarkSDKMethodControllerLogic.GetUserBenchmarksByDays(daysAgo, this._contextAccessor.HttpContext);
        }

        [HttpPost]
        [Route("MethodBenchmark")]
        public async Task<IActionResult> CreateMethodSDKBenchmark([FromBody] BenchmarkSDKMethod sdkMethod)
        {
            return await this._benchmarkSDKMethodControllerLogic.CreateMethodSDKBenchmark(sdkMethod, this._contextAccessor.HttpContext);
        }
    }
}
