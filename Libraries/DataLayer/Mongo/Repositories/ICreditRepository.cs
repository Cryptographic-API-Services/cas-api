using System.Threading.Tasks;
using DataLayer.Mongo.Entities;

namespace DataLayer.Mongo.Repositories
{
    public interface ICreditRepository
    {
        public Task AddValidatedCreditInformation(ValidatedCreditCard card);
    }
}
