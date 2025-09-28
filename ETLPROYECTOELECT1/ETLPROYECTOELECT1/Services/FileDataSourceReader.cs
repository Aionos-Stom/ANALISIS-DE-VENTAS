using CsvHelper;
using ETLPROYECTOELECT1.Interfaces;
using ETLPROYECTOELECT1.Models;
using System.Globalization;

namespace ETLPROYECTOELECT1.Services
{
    public class FileDataSourceReader : IDataExtractor
    {
        public async Task<List<Customer>> ExtractCustomersAsync(string source)
        {
            return await ExtractDataAsync<Customer>(source);
        }

        public async Task<List<OrderDetails>> ExtractOrderDetailsAsync(string source)
        {
            return await ExtractDataAsync<OrderDetails>(source);
        }

        public async Task<List<Orders>> ExtractOrdersAsync(string source)
        {
            return await ExtractDataAsync<Orders>(source);
        }

        public async Task<List<Product>> ExtractProductsAsync(string source)
        {
            return await ExtractDataAsync<Product>(source);
        }

        private async Task<List<T>> ExtractDataAsync<T>(string source)
        {
            // Crea una lista para almacenar los datos
            var data = new List<T>();
            using var reader = new StreamReader(source);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Configura el mapeo de headers para cada tipo
            ConfigureHeaderMapping(csv);

            // Lee cada fila del CSV y la convierte a tipo T
            await foreach (var record in csv.GetRecordsAsync<T>())
            {
                data.Add(record);
            }

            return data;
        }

        private void ConfigureHeaderMapping(CsvHelper.CsvReader csv)
        {
            // Configura el mapeo de headers para Customer
            csv.Context.RegisterClassMap<CustomerMap>();
            csv.Context.RegisterClassMap<ProductMap>();
            csv.Context.RegisterClassMap<OrdersMap>();
            csv.Context.RegisterClassMap<OrderDetailsMap>();
        }
    }
}