using CASHelpers;
using DataLayer.Mongo.Repositories;
using DataLayer.Redis;
using Microsoft.AspNetCore.Mvc;
using Models.UserAdmin;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Reflection;

namespace API.ControllerLogic
{
    public class UserAdminControllerLogic : IUserAdminControllerLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly ICASExceptionRepository _exceptionRepository;
        private readonly IRedisClient _redisClient;
        public UserAdminControllerLogic(
            IUserRepository userRepository,
            ICASExceptionRepository exceptionRepository,

            IRedisClient redisClient
            )
        {
            this._userRepository = userRepository;
            this._exceptionRepository = exceptionRepository;
            this._redisClient = redisClient;
        }

        public async Task<IActionResult> DeleteUser(HttpContext context, UserAdminDeleteUserRequest request)
        {

            IActionResult result = null;
            try
            {
                await this._userRepository.DeleteUserByUserId(request.UserId);
                string isUserActiveRedisKey = Constants.RedisKeys.IsActiveUser + request.UserId;
                this._redisClient.SetString(isUserActiveRedisKey, false.ToString(), new TimeSpan(1, 0, 0));
                result = new OkResult();
            }
            catch (Exception ex)
            {
                await this._exceptionRepository.InsertException(ex.ToString(), MethodBase.GetCurrentMethod().Name);
                result = new BadRequestObjectResult(new { error = "There was an error on our deleting the user." });
            }

            return result;
        }

        public async Task<IActionResult> GetUsers(HttpContext context, int pageSkip, int pageSize)
        {

            IActionResult result = null;
            try
            {
                IQueryable<UserTableItem> users = this._userRepository.GetUsersByPage();
                Task<int> usersCount = users.CountAsync();
                Task<List<UserTableItem>> usersList = users.Skip(pageSkip * pageSize).Take(pageSize).ToListAsync();
                await Task.WhenAll(usersCount, usersList);
                result = new OkObjectResult(new { Count = usersCount.Result, UserTableItems = usersList.Result });
            }
            catch (Exception ex)
            {
                await this._exceptionRepository.InsertException(ex.ToString(), MethodBase.GetCurrentMethod().Name);
                result = new BadRequestObjectResult(new { error = "There was an error on our end getting the user list." });
            }

            return result;
        }

        public async Task<IActionResult> UserActivationStatus(HttpContext httpContext, UserActivationStatusRequest request)
        {

            IActionResult result = null;
            try
            {
                await this._userRepository.ChangeUserActivationStatusById(request.UserId, request.IsActive);
                if (!request.IsActive)
                {
                    string isUserActiveRedisKey = Constants.RedisKeys.IsActiveUser + request.UserId;
                    this._redisClient.SetString(isUserActiveRedisKey, false.ToString(), new TimeSpan(1, 0, 0));
                }
                result = new OkResult();
            }
            catch (Exception ex)
            {
                await this._exceptionRepository.InsertException(ex.ToString(), MethodBase.GetCurrentMethod().Name);
                result = new BadRequestObjectResult(new { error = "There was an error on our end changing the user activation status" });
            }

            return result;
        }

        public async Task<IActionResult> UserAdminStatus(HttpContext context, UserAdminStatusRequest request)
        {

            IActionResult result = null;
            try
            {
                await this._userRepository.ChangeUserAdminStatusById(request.UserId, request.IsAdmin);
                string redisString = Constants.RedisKeys.IsUserAdmin + request.UserId;
                this._redisClient.SetString(redisString, request.IsAdmin.ToString(), new TimeSpan(1, 0, 0));
                result = new OkResult();
            }
            catch (Exception ex)
            {
                await this._exceptionRepository.InsertException(ex.ToString(), MethodBase.GetCurrentMethod().Name);
                result = new BadRequestObjectResult(new { error = "There was an error on our end changing the user admin status" });
            }

            return result;
        }
    }
}
