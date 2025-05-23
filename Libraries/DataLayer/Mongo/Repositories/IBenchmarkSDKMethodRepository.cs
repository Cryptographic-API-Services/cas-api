﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Mongo.Entities;

namespace DataLayer.Mongo.Repositories
{
    public interface IBenchmarkSDKMethodRepository
    {
        Task InsertSDKMethodBenchmark(BenchmarkSDKMethod method);
        Task<List<BenchmarkSDKMethod>> GetUserBenchmarksDaysAgo(string userId, int daysAgo);
    }
}
