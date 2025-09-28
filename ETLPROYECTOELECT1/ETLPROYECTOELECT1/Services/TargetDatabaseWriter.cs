using ETLPROYECTOELECT1.Interfaces;
using ETLPROYECTOELECT1.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ETLPROYECTOELECT1.Services
{
    public class TargetDatabaseWriter : IDataLoader
    {
        private readonly string _connectionString;

        public TargetDatabaseWriter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task LoadCustomersAsync(List<Customer> customers)
        {
            if (!customers.Any())
            {
                Console.WriteLine("   - No hay clientes para cargar");
                return;
            }

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Configurar timeout más largo
            using var transaction = connection.BeginTransaction();

            try
            {
                // Usar SqlBulkCopy para mejor rendimiento
                using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
                bulkCopy.DestinationTableName = "customers";
                bulkCopy.BatchSize = 1000; // Procesar en lotes
                bulkCopy.BulkCopyTimeout = 300; // 5 minutos timeout

                // Limpiar cualquier mapeo anterior
                bulkCopy.ColumnMappings.Clear();

                // Mapear columnas (DataTable -> Base de Datos)
                bulkCopy.ColumnMappings.Add("CustomerId", "customerid");
                bulkCopy.ColumnMappings.Add("FirstName", "firstname");
                bulkCopy.ColumnMappings.Add("LastName", "lastname");
                bulkCopy.ColumnMappings.Add("Email", "email");
                bulkCopy.ColumnMappings.Add("Phone", "phone");
                bulkCopy.ColumnMappings.Add("City", "city");
                bulkCopy.ColumnMappings.Add("Country", "country");

                // Crear DataTable
                var dataTable = CreateCustomersDataTable(customers);

                await bulkCopy.WriteToServerAsync(dataTable);
                await transaction.CommitAsync();

                Console.WriteLine($"   - {customers.Count} clientes cargados a la base de datos (usando bulk insert)");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"   - Error cargando clientes: {ex.Message}");
                throw;
            }
        }

        public async Task LoadProductsAsync(List<Product> products)
        {
            if (!products.Any())
            {
                Console.WriteLine("   - No hay productos para cargar");
                return;
            }

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
                bulkCopy.DestinationTableName = "products";
                bulkCopy.BatchSize = 1000;
                bulkCopy.BulkCopyTimeout = 300;

                // Limpiar cualquier mapeo anterior
                bulkCopy.ColumnMappings.Clear();

                // Mapear columnas (DataTable -> Base de Datos)
                bulkCopy.ColumnMappings.Add("ProductId", "productid");
                bulkCopy.ColumnMappings.Add("ProductName", "productname");
                bulkCopy.ColumnMappings.Add("Category", "category");
                bulkCopy.ColumnMappings.Add("Price", "price");
                bulkCopy.ColumnMappings.Add("Stock", "stock");

                var dataTable = CreateProductsDataTable(products);

                await bulkCopy.WriteToServerAsync(dataTable);
                await transaction.CommitAsync();

                Console.WriteLine($"   - {products.Count} productos cargados a la base de datos (usando bulk insert)");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"   - Error cargando productos: {ex.Message}");
                throw;
            }
        }

        public async Task LoadOrdersAsync(List<Orders> orders)
        {
            if (!orders.Any())
            {
                Console.WriteLine("   - No hay órdenes para cargar");
                return;
            }

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
                bulkCopy.DestinationTableName = "orders";
                bulkCopy.BatchSize = 1000;
                bulkCopy.BulkCopyTimeout = 300;

                // Limpiar cualquier mapeo anterior
                bulkCopy.ColumnMappings.Clear();

                // Mapear columnas (DataTable -> Base de Datos)
                bulkCopy.ColumnMappings.Add("OrderId", "orderid");
                bulkCopy.ColumnMappings.Add("CustomerId", "customerid");
                bulkCopy.ColumnMappings.Add("OrderDate", "orderdate");
                bulkCopy.ColumnMappings.Add("Status", "status");

                var dataTable = CreateOrdersDataTable(orders);

                await bulkCopy.WriteToServerAsync(dataTable);
                await transaction.CommitAsync();

                Console.WriteLine($"   - {orders.Count} órdenes cargadas a la base de datos (usando bulk insert)");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"   - Error cargando órdenes: {ex.Message}");
                throw;
            }
        }

        public async Task LoadOrderDetailsAsync(List<OrderDetails> orderDetails)
        {
            if (!orderDetails.Any())
            {
                Console.WriteLine("   - No hay detalles de órdenes para cargar");
                return;
            }

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Procesar en lotes más pequeños para detalles de órdenes
                const int batchSize = 5000;
                var totalBatches = (int)Math.Ceiling((double)orderDetails.Count / batchSize);

                for (int i = 0; i < totalBatches; i++)
                {
                    var batch = orderDetails.Skip(i * batchSize).Take(batchSize).ToList();

                    using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
                    bulkCopy.DestinationTableName = "order_details";
                    bulkCopy.BatchSize = 1000;
                    bulkCopy.BulkCopyTimeout = 600; // Más tiempo para detalles

                    // Limpiar cualquier mapeo anterior
                    bulkCopy.ColumnMappings.Clear();

                    // Mapear columnas (DataTable -> Base de Datos)
                    bulkCopy.ColumnMappings.Add("OrderID", "orderid");
                    bulkCopy.ColumnMappings.Add("ProductId", "productid");
                    bulkCopy.ColumnMappings.Add("Quantity", "quantity");
                    bulkCopy.ColumnMappings.Add("TotalPrice", "totalprice");

                    var dataTable = CreateOrderDetailsDataTable(batch);

                    await bulkCopy.WriteToServerAsync(dataTable);

                    Console.WriteLine($"   - Lote {i + 1}/{totalBatches} procesado ({batch.Count} detalles)");
                }

                await transaction.CommitAsync();

                Console.WriteLine($"   - {orderDetails.Count} detalles de orden cargados a la base de datos (usando bulk insert en lotes)");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"   - Error cargando detalles de órdenes: {ex.Message}");
                throw;
            }
        }

        public async Task ClearAllTablesAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Configurar timeout más largo para la limpieza
            using var command = new SqlCommand("EXEC sp_ClearAllTables;", connection);
            command.CommandTimeout = 300; // 5 minutos timeout

            try
            {
                await command.ExecuteNonQueryAsync();
                Console.WriteLine("   - Todas las tablas limpiadas (usando sp_ClearAllTables)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   - Error limpiando tablas: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ValidateForeignKeysAsync()
        {
            const string sql = @"
                SELECT COUNT(*) FROM orders o 
                LEFT JOIN customers c ON o.CustomerID = c.CustomerID 
                WHERE c.CustomerID IS NULL";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 120; // 2 minutos timeout

            var result = await command.ExecuteScalarAsync();
            var invalidOrders = result != null ? (int)result : 0;

            return invalidOrders == 0;
        }

        // Métodos auxiliares para crear DataTables
        private DataTable CreateCustomersDataTable(List<Customer> customers)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("CustomerId", typeof(int));
            dataTable.Columns.Add("FirstName", typeof(string));
            dataTable.Columns.Add("LastName", typeof(string));
            dataTable.Columns.Add("Email", typeof(string));
            dataTable.Columns.Add("Phone", typeof(string));
            dataTable.Columns.Add("City", typeof(string));
            dataTable.Columns.Add("Country", typeof(string));

            foreach (var customer in customers)
            {
                dataTable.Rows.Add(
                    customer.CustomerId,
                    customer.FirstName ?? (object)DBNull.Value,
                    customer.LastName ?? (object)DBNull.Value,
                    customer.Email ?? (object)DBNull.Value,
                    customer.Phone ?? (object)DBNull.Value,
                    customer.City ?? (object)DBNull.Value,
                    customer.Country ?? (object)DBNull.Value
                );
            }

            return dataTable;
        }

        private DataTable CreateProductsDataTable(List<Product> products)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ProductId", typeof(int));
            dataTable.Columns.Add("ProductName", typeof(string));
            dataTable.Columns.Add("Category", typeof(string));
            dataTable.Columns.Add("Price", typeof(decimal));
            dataTable.Columns.Add("Stock", typeof(int));

            foreach (var product in products)
            {
                dataTable.Rows.Add(
                    product.ProductId,
                    product.ProductName ?? (object)DBNull.Value,
                    product.Category ?? (object)DBNull.Value,
                    product.Price,
                    product.Stock
                );
            }

            return dataTable;
        }

        private DataTable CreateOrdersDataTable(List<Orders> orders)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("OrderId", typeof(int));
            dataTable.Columns.Add("CustomerId", typeof(int));
            dataTable.Columns.Add("OrderDate", typeof(DateTime));
            dataTable.Columns.Add("Status", typeof(string));

            foreach (var order in orders)
            {
                dataTable.Rows.Add(
                    order.OrderId,
                    order.CustomerId,
                    order.OrderDate,
                    order.Status ?? (object)DBNull.Value
                );
            }

            return dataTable;
        }

        private DataTable CreateOrderDetailsDataTable(List<OrderDetails> orderDetails)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("OrderID", typeof(int));
            dataTable.Columns.Add("ProductId", typeof(int));
            dataTable.Columns.Add("Quantity", typeof(int));
            dataTable.Columns.Add("TotalPrice", typeof(decimal));

            foreach (var detail in orderDetails)
            {
                dataTable.Rows.Add(
                    detail.OrderID,
                    detail.ProductId,
                    detail.Quantity,
                    detail.TotalPrice
                );
            }

            return dataTable;
        }
    }
}