using API.HelperServices;
using CASHelpers;
using DataLayer.Mongo.Entities;
using DataLayer.Mongo.Repositories;
using DataLayer.Redis;
using Microsoft.AspNetCore.Mvc;
using Models.UserAdmin;
using Models.UserAuthentication;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace API.ControllerLogic
{
    public class UserAdminControllerLogic : IUserAdminControllerLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly ICASExceptionRepository _exceptionRepository;
        private readonly IRedisClient _redisClient;
        private readonly IJWTPublicKeyTrustCertificate _jwtPublicKeyTrustCertificate;
        public UserAdminControllerLogic(
            IUserRepository userRepository,
            ICASExceptionRepository exceptionRepository,
            IJWTPublicKeyTrustCertificate jwtPublicKeyTrustCertificate,
            IRedisClient redisClient
            )
        {
            this._userRepository = userRepository;
            this._exceptionRepository = exceptionRepository;
            this._redisClient = redisClient;
            this._jwtPublicKeyTrustCertificate = jwtPublicKeyTrustCertificate;
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

        public async Task<IActionResult> RevokeSession(HttpContext context, RevokeUserSessionRequest request)
        {
            IActionResult result = null;
            try
            {
                User activeUser = await this._userRepository.GetUserById(request.UserId);
                ECDSAWrapper ecdsa = new ECDSAWrapper("ES521");
                string publicKeyCacheKey = Constants.RedisKeys.UserTokenPublicKey + activeUser.Id;
                await this._userRepository.SetUserTokenPublicKey(activeUser.Id, ecdsa.PublicKey);
                this._redisClient.SetString(publicKeyCacheKey, ecdsa.PublicKey, new TimeSpan(1, 0, 0));
                string isUserActiveRedisKey = Constants.RedisKeys.IsActiveUser + activeUser.Id;
                this._redisClient.SetString(isUserActiveRedisKey, false.ToString(), new TimeSpan(1, 0, 0));
                string isUserAdminRedisKey = Constants.RedisKeys.IsUserAdmin + activeUser.Id;
                this._redisClient.SetString(isUserAdminRedisKey, activeUser.IsAdmin.ToString(), new TimeSpan(1, 0, 0));
                this._jwtPublicKeyTrustCertificate.CreatePublicKeyTrustCertificate(ecdsa.PublicKey, activeUser.Id);
                result = new OkResult();
            }
            catch(Exception ex)
            {
                this._exceptionRepository.InsertException(ex.ToString(), MethodBase.GetCurrentMethod().Name);
                result = new BadRequestObjectResult(new { error = "There was an error on our end revoking the user session." });
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
