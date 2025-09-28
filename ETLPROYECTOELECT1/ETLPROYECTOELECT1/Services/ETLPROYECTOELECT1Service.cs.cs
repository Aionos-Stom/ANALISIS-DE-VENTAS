using ETLPROYECTOELECT1.Interfaces;
using ETLPROYECTOELECT1.Models;
using ETLPROYECTOELECT1.Services; // Agregado para TargetDatabaseWriter
using Microsoft.Extensions.Configuration;
using System; // Agregado para usar InvalidOperationException

namespace ETLPROYECTOELECT1.Services
{
    public class ETLPROYECTOELECT1Service
    {
        private readonly IDataExtractor _extractor;
        private readonly IDataTransformer _transformer;
        private readonly IDataLoader _loader;
        private readonly IConfiguration _configuration;

        public ETLPROYECTOELECT1Service(
            IDataExtractor extractor,
            IDataTransformer transformer,
            IDataLoader loader,
            IConfiguration configuration)
        {
            _extractor = extractor;
            _transformer = transformer;
            _loader = loader;
            _configuration = configuration;
        }

        public async Task RunETLPROYECTOELECT1Async()
        {
            Console.WriteLine("Lanzando el programa al aire");
            Console.WriteLine();

            try
            {
                // extraer datos de CSV
                Console.WriteLine("1. Extraer, Extrayendo datos de archivos CSV...");
                var customers = await ExtractCustomersAsync();
                var products = await ExtractProductsAsync();
                var orders = await ExtractOrdersAsync();
                var orderDetails = await ExtractOrderDetailsAsync();

                Console.WriteLine($"   - Clientes extraidos: {customers.Count}");
                Console.WriteLine($"   - Productos extraidos: {products.Count}");
                Console.WriteLine($"   - Ordenes extraidas: {orders.Count}");
                Console.WriteLine($"   - Detalles de ordenes extraidos: {orderDetails.Count}");
                Console.WriteLine();

                // Transformar y limpiar datos
                Console.WriteLine("2. TRANSFORM - Transformando y limpiando datos...");
                customers = TransformCustomers(customers);
                products = TransformProducts(products);
                orders = TransformOrders(orders);
                orderDetails = TransformOrderDetails(orderDetails);

                Console.WriteLine($"   - Clientes despues de transformacion: {customers.Count}");
                Console.WriteLine($"   - Productos despues de transformacion: {products.Count}");
                Console.WriteLine($"   - Ordenes despues de transformacion: {orders.Count}");
                Console.WriteLine($"   - Detalles despues de transformacion: {orderDetails.Count}");
                Console.WriteLine();

                // Cargar datos a la base de datos en orden correcto
                Console.WriteLine("3. LOAD - Cargando datos a la base de datos...");

                // Primero limpiar todas las tablas
                await ClearAllTablesAsync();

                // Cargar en orden: independientes primero, dependientes despues
                await LoadCustomersAsync(customers);
                await LoadProductsAsync(products);
                await LoadOrdersAsync(orders);
                await LoadOrderDetailsAsync(orderDetails);

                Console.WriteLine("   - Datos cargados exitosamente");
                Console.WriteLine();

                //  Verifica datos cargados
                Console.WriteLine("4. VALIDATE - Verificando datos cargados...");
                var isValid = await ValidateIntegrityAsync();

                if (isValid)
                {
                    Console.WriteLine("   - Validacion exitosa: Datos cargados correctamente");
                }
                else
                {
                    Console.WriteLine("   - ADVERTENCIA: Problemas al cargar los datos");
                }

                Console.WriteLine();
                Console.WriteLine(" PROGRAMA ETL COMPLETADO EXITOSAMENTE ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en el Sistema ETL: {ex.Message}");
                throw;
            }
        }

        private async Task<List<Customer>> ExtractCustomersAsync()
        {
            var path = _configuration["DataSources:CustomersPath"];
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("FALTA RUTA: La clave 'DataSources:CustomersPath' no se encontró o es nula en la configuración (appsettings.json).");
            }
            return await _extractor.ExtractCustomersAsync(path);
        }

        private async Task<List<Product>> ExtractProductsAsync()
        {
            var path = _configuration["DataSources:ProductsPath"];
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("FALTA RUTA: La clave 'DataSources:ProductsPath' no se encontró o es nula en la configuración (appsettings.json).");
            }
            return await _extractor.ExtractProductsAsync(path);
        }

        private async Task<List<Orders>> ExtractOrdersAsync()
        {
            var path = _configuration["DataSources:OrdersPath"];
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("FALTA RUTA: La clave 'DataSources:OrdersPath' no se encontró o es nula en la configuración (appsettings.json).");
            }
            return await _extractor.ExtractOrdersAsync(path);
        }

        private async Task<List<OrderDetails>> ExtractOrderDetailsAsync()
        {
            var path = _configuration["DataSources:OrderDetailsPath"];
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("FALTA RUTA: La clave 'DataSources:OrderDetailsPath' no se encontró o es nula en la configuración (appsettings.json).");
            }
            return await _extractor.ExtractOrderDetailsAsync(path);
        }

        private List<Customer> TransformCustomers(List<Customer> customers)
        {
            Console.WriteLine("   - Limpiando y eliminando duplicados de clientes...");
            customers = _transformer.CleanData(customers);
            customers = _transformer.RemoveDuplicates(customers);
            return customers;
        }

        private List<Product> TransformProducts(List<Product> products)
        {
            Console.WriteLine("   - Limpiando y eliminando duplicados de productos...");
            products = _transformer.CleanData(products);
            products = _transformer.RemoveDuplicates(products);
            return products;
        }

        private List<Orders> TransformOrders(List<Orders> orders)
        {
            Console.WriteLine("   - Limpiando y eliminando duplicados de ordenes...");
            orders = _transformer.CleanData(orders);
            orders = _transformer.RemoveDuplicates(orders);
            return orders;
        }

        private List<OrderDetails> TransformOrderDetails(List<OrderDetails> orderDetails)
        {
            Console.WriteLine("   - Limpiando y eliminando duplicados de detalles...");
            orderDetails = _transformer.CleanData(orderDetails);
            orderDetails = _transformer.RemoveDuplicates(orderDetails);
            return orderDetails;
        }

        private async Task LoadCustomersAsync(List<Customer> customers)
        {
            await _loader.LoadCustomersAsync(customers);
        }

        private async Task LoadProductsAsync(List<Product> products)
        {
            await _loader.LoadProductsAsync(products);
        }

        private async Task LoadOrdersAsync(List<Orders> orders)
        {
            await _loader.LoadOrdersAsync(orders);
        }

        private async Task LoadOrderDetailsAsync(List<OrderDetails> orderDetails)
        {
            await _loader.LoadOrderDetailsAsync(orderDetails);
        }

        private async Task ClearAllTablesAsync()
        {
            // CAMBIO APLICADO: Castear a TargetDatabaseWriter
            if (_loader is TargetDatabaseWriter targetDatabaseWriter)
            {
                await targetDatabaseWriter.ClearAllTablesAsync();
            }
        }

        private async Task<bool> ValidateIntegrityAsync()
        {
            // CAMBIO APLICADO: Castear a TargetDatabaseWriter
            if (_loader is TargetDatabaseWriter targetDatabaseWriter)
            {
                return await targetDatabaseWriter.ValidateForeignKeysAsync();
            }
            return true;
        }
    }
}