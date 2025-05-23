﻿using System;
using System.Threading.Tasks;
using DataLayer.Mongo.Entities;
using MongoDB.Driver;

namespace DataLayer.Mongo.Repositories
{
    public interface ISuccessfulLoginRepository
    {
        public Task InsertSuccessfulLogin(SuccessfulLogin login);
        public IFindFluent<SuccessfulLogin, SuccessfulLogin> GetAllSuccessfulLoginWithinTimeFrame(string userId, DateTime dateTime);
        public Task UpdateSuccessfulLoginWasMe(string loginId, bool wasThisMe);
        public Task<long> GetLoginsCountAfterDate(DateTime timePeriod, string userId);
    }
}
