using Entities.Shopping;

namespace DynamicAuthBlazor.Web.Services
{
    public interface IProductRepository
    {
        public Task<IEnumerable<Prouduct_responseDTO>> getAllProductsAsync();
    }
}
