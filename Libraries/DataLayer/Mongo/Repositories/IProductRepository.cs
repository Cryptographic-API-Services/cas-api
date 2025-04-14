using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Mongo.Entities;

namespace DataLayer.Mongo.Repositories
{
    public interface IProductRepository
    {
        public Task InsertProduct(Product newProduct);
        public Task<List<Product>> GetAllProducts();
        public Task<Product> GetProductByName(string productName);
    }
}
