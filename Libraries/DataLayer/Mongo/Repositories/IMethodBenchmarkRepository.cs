using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using DataLayer.Mongo.Entities;

namespace DataLayer.Mongo.Repositories
{
    public interface IMethodBenchmarkRepository
    {
        Task InsertBenchmark(BenchmarkMethodLogger method);
        Task<List<BenchmarkMethod>> GetAmountByEndTimeDescending(int amountToTake);
    }
}
