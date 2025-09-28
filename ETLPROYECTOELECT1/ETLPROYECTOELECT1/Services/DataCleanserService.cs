using ETLPROYECTOELECT1.Interfaces;
using ETLPROYECTOELECT1.Models;
using System.Text.RegularExpressions;

namespace ETLPROYECTOELECT1.Services
{
    public class DataCleanserService : IDataTransformer
    {
        public List<T> CleanData<T>(List<T> data)
        {
            // Lista para guardar los datos limpios
            var cleanedData = new List<T>();
            var originalCount = data.Count;

            foreach (var item in data)
            {

                if (item != null && IsValidItem(item))
                {

                    CleanItemData(item);

                    // Validaciones especificas por tipo
                    if (ValidateItemSpecific(item))
                    {
                        cleanedData.Add(item);
                    }
                }
            }

            var cleanedCount = cleanedData.Count;
            var invalidCount = originalCount - cleanedCount;

            if (invalidCount > 0)
            {
                Console.WriteLine($"   - {typeof(T).Name}: {originalCount} originales, {cleanedCount} validos, {invalidCount} invalidos eliminados");
            }

            return cleanedData;
        }

        public List<T> RemoveDuplicates<T>(List<T> data)
        {
            var originalCount = data.Count;

            // Elimina duplicados de clientes
            if (typeof(T) == typeof(Customer))
            {
                var customers = data.Cast<Customer>().ToList();
                // Agrupa por CustomerID y tomar solo el primero de cada grupo
                var uniqueCustomers = customers
                    .GroupBy(c => c.CustomerId)
                    .Select(g => g.First())
                    .ToList();
                var result = uniqueCustomers.Cast<T>().ToList();
                Console.WriteLine($"   - Clientes: {originalCount} originales, {result.Count} unicos, {originalCount - result.Count} duplicados eliminados");
                return result;
            }
            // Elimina duplicados de productos
            else if (typeof(T) == typeof(Product))
            {
                var products = data.Cast<Product>().ToList();
                // Agrupa por ProductID y tomar solo el primero de cada grupo
                var uniqueProducts = products
                    .GroupBy(p => p.ProductId)
                    .Select(g => g.First())
                    .ToList();
                var result = uniqueProducts.Cast<T>().ToList();
                Console.WriteLine($"   - Productos: {originalCount} originales, {result.Count} unicos, {originalCount - result.Count} duplicados eliminados");
                return result;
            }
            // Elimina duplicados de ordenes
            else if (typeof(T) == typeof(Orders))
            {
                var orders = data.Cast<Orders>().ToList();
                // Agrupa por OrderId y tomar solo el primero de cada grupo
                var uniqueOrders = orders
                    .GroupBy(o => o.OrderId)
                    .Select(g => g.First())
                    .ToList();
                var result = uniqueOrders.Cast<T>().ToList();
                Console.WriteLine($"   - Ordenes: {originalCount} originales, {result.Count} unicos, {originalCount - result.Count} duplicados eliminados");
                return result;
            }
            // Elimina duplicados de detalles de ordenes
            else if (typeof(T) == typeof(OrderDetails))
            {
                var orderDetails = data.Cast<OrderDetails>().ToList();
                // Agrupa por OrderID y ProductID (clave compuesta) y tomar solo el primero
                var uniqueOrderDetails = orderDetails
                    .GroupBy(od => new { od.OrderID, od.ProductId })
                    .Select(g => g.First())
                    .ToList();
                var result = uniqueOrderDetails.Cast<T>().ToList();
                Console.WriteLine($"   - Detalles: {originalCount} originales, {result.Count} unicos, {originalCount - result.Count} duplicados eliminados");
                return result;
            }


            return data.Distinct().ToList();
        }

        public bool ValidateIntegrity<T>(List<T> data)
        {
            // Validacion simple: solo verifica que hay datos
            return data != null && data.Count > 0;
        }

        private void CleanItemData<T>(T item)
        {
            if (item is Customer customer)
            {
                CleanCustomerData(customer);
            }
            else if (item is Product product)
            {
                CleanProductData(product);
            }
            else if (item is Orders order)
            {
                CleanOrderData(order);
            }
            else if (item is OrderDetails orderDetail)
            {
                CleanOrderDetailData(orderDetail);
            }
        }

        private bool IsValidItem<T>(T item)
        {
            return item != null;
        }

        private bool ValidateItemSpecific<T>(T item)
        {
            return item switch
            {
                Customer customer => ValidateCustomerSpecific(customer),
                Product product => ValidateProductSpecific(product),
                Orders order => ValidateOrderSpecific(order),
                OrderDetails orderDetail => ValidateOrderDetailSpecific(orderDetail),
                _ => true
            };
        }

        private bool ValidateCustomerSpecific(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Email) || !customer.Email.Contains("@"))
                return false;

            // Valida que CustomerID sea positivo
            if (customer.CustomerId <= 0)
                return false;

            return true;
        }

        private bool ValidateProductSpecific(Product product)
        {
            // Valida que ProductID sea positivo
            if (product.ProductId <= 0)
                return false;

            // Valida que precio sea positivo
            if (product.Price <= 0)
                return false;

            // Valida que stock sea no negativo
            if (product.Stock < 0)
                return false;

            return true;
        }

        private bool ValidateOrderSpecific(Orders order)
        {
            // Valida que OrderID sea positivo
            if (order.OrderId <= 0)
                return false;

            // Valida que CustomerID sea positivo
            if (order.CustomerId <= 0)
                return false;

            // Valida fecha valida
            if (order.OrderDate == DateTime.MinValue)
                return false;

            return true;
        }

        private bool ValidateOrderDetailSpecific(OrderDetails orderDetail)
        {
            // Valida que OrderID sea positivo
            if (orderDetail.OrderID <= 0)
                return false;

            // Valida que ProductID sea positivo
            if (orderDetail.ProductId <= 0)
                return false;

            // Valida que cantidad sea positiva
            if (orderDetail.Quantity <= 0)
                return false;

            // Valida que TotalPrice sea positivo
            if (orderDetail.TotalPrice <= 0)
                return false;

            return true;
        }


        // Metodos de limpieza
        private void CleanCustomerData(Customer customer)
        {
            customer.FirstName = CleanString(customer.FirstName);
            customer.LastName = CleanString(customer.LastName);
            customer.Email = CleanEmail(customer.Email);
            customer.Phone = CleanPhone(customer.Phone);
            customer.City = CleanString(customer.City);
            customer.Country = CleanString(customer.Country);
        }

        private void CleanProductData(Product product)
        {
            product.ProductName = CleanString(product.ProductName);
            product.Category = CleanString(product.Category);
        }

        private void CleanOrderData(Orders order)
        {
            order.Status = CleanString(order.Status);
        }

        private void CleanOrderDetailData(OrderDetails orderDetail)
        {
            // No hay limpieza necesaria
        }

        private string CleanString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input.Trim();
        }

        private string CleanEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            return email.Trim().ToLower();
        }

        private string CleanPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            // Remover caracteres no numéricos excepto +, -, (, ), espacios
            return System.Text.RegularExpressions.Regex.Replace(phone.Trim(), @"[^\d\+\-\(\)\s]", "");
        }
    }
}
