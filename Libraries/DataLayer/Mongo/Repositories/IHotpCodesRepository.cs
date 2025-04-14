﻿using System.Threading.Tasks;
using DataLayer.Mongo.Entities;

namespace DataLayer.Mongo.Repositories
{
    public interface IHotpCodesRepository
    {
        public Task<long> GetHighestCounter();
        public Task InsertHotpCode(HotpCode code);
        public Task<HotpCode> GetHotpCodeByIdAndCode(string id);
        public Task UpdateHotpToVerified(string id);
    }
}
