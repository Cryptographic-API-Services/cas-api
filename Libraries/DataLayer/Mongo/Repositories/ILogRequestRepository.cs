using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Mongo.Entities;

namespace DataLayer.Mongo.Repositories
{
    public interface ILogRequestRepository
    {
        Task InsertRequest(LogRequest request);
        Task InsertRequests(List<LogRequest> requests);
    }
}
