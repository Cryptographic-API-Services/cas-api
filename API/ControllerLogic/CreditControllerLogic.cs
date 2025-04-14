﻿using System.Reflection;
using System.Text;
using System.Text.Json;
using CASHelpers;
using Common;
using DataLayer.Cache;
using DataLayer.Mongo.Entities;
using DataLayer.Mongo.Repositories;
using DataLayer.RabbitMQ;
using DataLayer.RabbitMQ.QueueMessages;
using Microsoft.AspNetCore.Mvc;
using Models.Credit;
using Payments;
using Stripe;
using Validation.CreditCard;

namespace API.ControllersLogic
{
    public class CreditControllerLogic : ICreditControllerLogic
    {
        private readonly ICreditRepository _creditRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUserRepository _userRepository;
        private readonly ICASExceptionRepository _exceptionRepository;
        private readonly BenchmarkMethodCache _benchmarkMethodCache;
        private readonly CreditCardInformationChangedQueuePublish _ccInfoChangedQueue;
        public CreditControllerLogic(
            ICreditRepository creditRepository,
            IHttpContextAccessor contextAccessor,
            IUserRepository userRepository,
            ICASExceptionRepository exceptionRepository,
            BenchmarkMethodCache benchmarkMethodCache,
            CreditCardInformationChangedQueuePublish ccInfoChangedQueue
            )
        {
            this._creditRepository = creditRepository;
            this._contextAccessor = contextAccessor;
            this._userRepository = userRepository;
            this._exceptionRepository = exceptionRepository;
            this._benchmarkMethodCache = benchmarkMethodCache;
            this._ccInfoChangedQueue = ccInfoChangedQueue;
        }

        #region AddCreditCard
        public async Task<IActionResult> AddCreditCard(AddCreditCardRequest body, HttpContext httpContext)
        {
            BenchmarkMethodLogger logger = new BenchmarkMethodLogger(httpContext);
            IActionResult result = null;
            try
            {
                string userId = httpContext.Items[Constants.HttpItems.UserID].ToString();
                User dbUser = await this._userRepository.GetUserById(userId);
                LuhnWrapper wrapper = new LuhnWrapper();
                if (wrapper.IsCCValid(body.creditCardNumber))
                {
                    StripTokenCard stripTokenCards = new StripTokenCard();
                    // delete card from strip if one exists.
                    if (!string.IsNullOrEmpty(dbUser.StripCardId) && !string.IsNullOrEmpty(dbUser.StripCustomerId))
                    {
                        await stripTokenCards.DeleteCustomerCard(dbUser.StripCustomerId, dbUser.StripCardId);
                        CreditCardInformationChangedQueueMessage newMessage = new CreditCardInformationChangedQueueMessage() { UserEmail = dbUser.Email };
                        this._ccInfoChangedQueue.BasicPublish(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(newMessage)));
                    }
                    string tokenId = await stripTokenCards.CreateTokenCard(body.creditCardNumber, body.expirationMonth, body.expirationYear, body.SecurityCode);
                    Card newCard = await stripTokenCards.AddTokenCardToCustomer(dbUser.StripCustomerId, tokenId);
                    await this._userRepository.AddCardToUser(dbUser.Id, newCard.Id);
                    result = new OkResult();
                }
            }
            catch (Exception ex)
            {
                await this._exceptionRepository.InsertException(ex.ToString(), MethodBase.GetCurrentMethod().Name);
                result = new BadRequestObjectResult(new { error = "There was an error on our end." });
            }
            logger.EndExecution();
            this._benchmarkMethodCache.AddLog(logger);
            return result;
        }
        #endregion

        #region ValidateCreditCard
        public async Task<IActionResult> ValidateCreditCard([FromBody] CreditValidateRequest body, HttpContext httpContext)
        {
            BenchmarkMethodLogger logger = new BenchmarkMethodLogger(httpContext);
            IActionResult result = null;
            try
            {
                if (!string.IsNullOrEmpty(body.CCNumber))
                {
                    LuhnWrapper wrapper = new LuhnWrapper();
                    bool isValidCC = await wrapper.IsCCValidAsync(body.CCNumber);
                    if (isValidCC)
                    {
                        // get user id from token
                        string userId = this._contextAccessor.HttpContext.Items[Constants.HttpItems.UserID].ToString();
                        await this._creditRepository.AddValidatedCreditInformation(new ValidatedCreditCard()
                        {
                            UserID = userId,
                            CreationTime = DateTime.UtcNow,
                            LastModifiedTime = DateTime.UtcNow
                        });
                        result = new OkObjectResult(new { IsValid = true });
                    }
                    else
                    {
                        result = new OkObjectResult(new { IsValid = false });
                    }
                }
            }
            catch (Exception ex)
            {
                await this._exceptionRepository.InsertException(ex.ToString(), MethodBase.GetCurrentMethod().Name);
                result = new BadRequestResult();
            }
            logger.EndExecution();
            this._benchmarkMethodCache.AddLog(logger);
            return result;
        }
        #endregion
    }
}