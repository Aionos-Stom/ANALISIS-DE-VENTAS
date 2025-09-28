using ETLPROYECTOELECT1.Models;

namespace ETLPROYECTOELECT1.Interfaces
{
    public interface IDataExtractor
    {
        Task<List<Product>> ExtractProductsAsync(string source);
        Task<List<Customer>> ExtractCustomersAsync(string source);
        Task<List<Orders>> ExtractOrdersAsync(string source);
        Task<List<OrderDetails>> ExtractOrderDetailsAsync(string source);
    }
}
