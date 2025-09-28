using ETLPROYECTOELECT1.Models;

namespace ETLPROYECTOELECT1.Interfaces
{
    public interface IDataLoader
    {
        Task LoadProductsAsync(List<Product> products);
        Task LoadCustomersAsync(List<Customer> customers);
        Task LoadOrdersAsync(List<Orders> orders);
        Task LoadOrderDetailsAsync(List<OrderDetails> OrderDetails);
        Task ClearAllTablesAsync();
    }
}
