﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Mongo.Entities;

namespace DataLayer.Mongo.Repositories
{
    public interface IFailedLoginAttemptRepository
    {
        public Task InsertFailedLoginAttempt(FailedLoginAttempt loginAttempt);
        public Task<List<FailedLoginAttempt>> GetFailedLoginAttemptsLastTweleveHours(string userId);
    }
}
