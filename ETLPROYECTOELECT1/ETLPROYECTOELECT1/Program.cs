using ETLPROYECTOELECT1.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using ETLPROYECTOELECT1.Interfaces;

// Leer configuracion
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// 1. Validar la cadena de conexión de forma segura
string? connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("ERROR CRÍTICO: La cadena de conexión 'DefaultConnection' no se encontró en la configuración (appsettings.json).");
    // Retorna de forma limpia si la configuración es incorrecta
    return;
}

// Crear servicios
var extractor = new FileDataSourceReader();
var transformer = new DataCleanserService();
var loader = new TargetDatabaseWriter(connectionString);
var ETLPROYECTOELECT1 = new ETLPROYECTOELECT1Service(extractor, transformer, loader, configuration);

try
{
    Console.WriteLine("SISTEMA DE ANALISIS DE VENTAS ETL");
    Console.WriteLine(" PROGRAMA DE ANALIZAR VENTAS Y ORGANIZACION POR JOHAN TORRES ");
    Console.WriteLine();
    await ETLPROYECTOELECT1.RunETLPROYECTOELECT1Async();
    Console.WriteLine();
    Console.WriteLine("Presiona cualquier tecla para salir...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine("Presiona cualquier tecla para salir...");
    Console.ReadKey();
}