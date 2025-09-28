Este proyecto implementa un sistema de análisis de ventas basado en un proceso ETL (Extracción, Transformación y Carga), desarrollado en C#. Forma parte de una práctica académica orientada a integrar múltiples componentes de ingeniería de datos: diseño de base de datos relacional, procesamiento de datos desde fuentes externas, y carga estructurada en una base de datos relacional.

🧩 Componentes del Proyecto
1. Diseño y Modelado de Base de Datos

Modelo de datos que incluye las siguientes entidades principales:

Productos

Clientes

Ventas

Facturas

Fuentes de datos externas

Definición clara de claves primarias (PK) y claves foráneas (FK) para mantener la integridad referencial.

Diagrama Entidad-Relación (ER) que representa visualmente la estructura y relaciones de la base de datos.

2. Pipeline ETL en C#

Extracción de datos:

Lectura de archivos .CSV que contienen información de productos, clientes y ventas.

Opción para incluir datos desde APIs o bases de datos externas.

Transformación de datos:

Limpieza de datos: eliminación de valores nulos, registros duplicados e inconsistencias.

Normalización de formatos: fechas, precios, identificadores.

Validación de claves referenciales.

Cálculo de campos derivados como: Total = Cantidad × Precio.

Carga de datos:

Inserción de datos en la base de datos modelada.

Respeto por la integridad referencial mediante PK y FK.

3. Entregables

✅ Script SQL con la definición de tablas y relaciones.

✅ Código fuente en C# que implementa todo el pipeline ETL.

✅ Documento explicativo del flujo ETL y el diseño de base de datos.

✅ Capturas de pantalla mostrando la cantidad de registros cargados por tabla.

✅ Resultados (SELECT *) de cada tabla ya cargada, como evidencia.

💡 Tecnologías Utilizadas

Lenguaje: C#

Estructura del RepositoriO
Base de Datos: SQL Server / 
📦 SalesAnalysisETL/
 ┣ 📂 Diagrams/
 ┃ ┗ 📄 modelo_ER.png
 ┣ 📂 Data/
 ┃ ┣ 📄 productos.csv
 ┃ ┣ 📄 clientes.csv
 ┃ ┗ 📄 ventas.csv
 ┣ 📂 Screenshots/
 ┃ ┣ 📄 registros_tablas.png
 ┃ ┗ 📄 resultados_select.png
 ┣ 📂 SQL/
 ┃ ┗ 📄 script_base_de_datos.sql
 ┣ 📂 Source/
 ┃ ┗ 📄 Program.cs
 ┣ 📄 README.md
 ┗ 📄 flujo_ETL_documentacion.pdf

Nombre: (Johan Torres)

Curso: (Electiva 1)

Año: 2025

Formato de entrada: CSV
