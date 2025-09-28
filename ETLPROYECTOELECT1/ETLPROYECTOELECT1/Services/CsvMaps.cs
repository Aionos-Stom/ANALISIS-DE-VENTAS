using CsvHelper.Configuration;
using ETLPROYECTOELECT1.Models;

namespace ETLPROYECTOELECT1.Services
{
    public class CustomerMap : ClassMap<Customer>
    {
        public CustomerMap()
        {
            Map(m => m.CustomerId).Name("CustomerID");
            Map(m => m.FirstName);
            Map(m => m.LastName);
            Map(m => m.Email);
            Map(m => m.Phone);
            Map(m => m.City);
            Map(m => m.Country);
        }
    }

    public class ProductMap : ClassMap<Product>
    {
        public ProductMap()
        {
            Map(m => m.ProductId).Name("ProductID");
            Map(m => m.ProductName);
            Map(m => m.Category);
            Map(m => m.Price);
            Map(m => m.Stock);
        }
    }

    public class OrdersMap : ClassMap<Orders>
    {
        public OrdersMap()
        {
            Map(m => m.OrderId).Name("OrderID");
            Map(m => m.CustomerId).Name("CustomerID");
            Map(m => m.OrderDate);
            Map(m => m.Status);
        }
    }

    public class OrderDetailsMap : ClassMap<OrderDetails>
    {
        public OrderDetailsMap()
        {
            Map(m => m.OrderID).Name("OrderID");
            Map(m => m.ProductId).Name("ProductID");
            Map(m => m.Quantity);
            Map(m => m.TotalPrice);
        }
    }
}
