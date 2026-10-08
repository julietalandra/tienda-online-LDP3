-- Ejecutar el archivo completo en SQL Server LocalDB. No requiere migraciones.
USE master;
GO
IF DB_ID(N'commit_store') IS NULL
    CREATE DATABASE commit_store;
GO
USE commit_store;
GO
IF OBJECT_ID(N'dbo.categorias', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.categorias (
        idCategoria INT IDENTITY(1,1) CONSTRAINT PK_categorias PRIMARY KEY,
        descripcion NVARCHAR(60) NOT NULL
    );
END;
GO
IF OBJECT_ID(N'dbo.productos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.productos (
        idProducto INT IDENTITY(1,1) CONSTRAINT PK_productos PRIMARY KEY,
        nombre NVARCHAR(100) NOT NULL,
        precio DECIMAL(18,2) NOT NULL CONSTRAINT CK_productos_precio CHECK (precio > 0),
        categoria INT NOT NULL,
        CONSTRAINT FK_productos_categorias FOREIGN KEY (categoria)
            REFERENCES dbo.categorias(idCategoria)
    );
END;
GO
-- Datos manuales de demostración. No repetir el script para reiniciar datos.
IF NOT EXISTS (SELECT 1 FROM dbo.categorias WHERE descripcion = N'Periféricos')
    INSERT INTO dbo.categorias (descripcion) VALUES (N'Periféricos');
IF NOT EXISTS (SELECT 1 FROM dbo.categorias WHERE descripcion = N'Escritorio')
    INSERT INTO dbo.categorias (descripcion) VALUES (N'Escritorio');
IF NOT EXISTS (SELECT 1 FROM dbo.categorias WHERE descripcion = N'Accesorios')
    INSERT INTO dbo.categorias (descripcion) VALUES (N'Accesorios');
GO
DECLARE @perifericos INT = (SELECT MIN(idCategoria) FROM dbo.categorias WHERE descripcion = N'Periféricos');
DECLARE @escritorio INT = (SELECT MIN(idCategoria) FROM dbo.categorias WHERE descripcion = N'Escritorio');
DECLARE @accesorios INT = (SELECT MIN(idCategoria) FROM dbo.categorias WHERE descripcion = N'Accesorios');
IF NOT EXISTS (SELECT 1 FROM dbo.productos WHERE nombre = N'Teclado mecánico')
    INSERT INTO dbo.productos (nombre, precio, categoria) VALUES (N'Teclado mecánico', 85000.00, @perifericos);
IF NOT EXISTS (SELECT 1 FROM dbo.productos WHERE nombre = N'Mouse inalámbrico')
    INSERT INTO dbo.productos (nombre, precio, categoria) VALUES (N'Mouse inalámbrico', 32000.00, @perifericos);
IF NOT EXISTS (SELECT 1 FROM dbo.productos WHERE nombre = N'Auriculares')
    INSERT INTO dbo.productos (nombre, precio, categoria) VALUES (N'Auriculares', 48500.00, @perifericos);
IF NOT EXISTS (SELECT 1 FROM dbo.productos WHERE nombre = N'Soporte para notebook')
    INSERT INTO dbo.productos (nombre, precio, categoria) VALUES (N'Soporte para notebook', 24000.00, @escritorio);
IF NOT EXISTS (SELECT 1 FROM dbo.productos WHERE nombre = N'Hub USB')
    INSERT INTO dbo.productos (nombre, precio, categoria) VALUES (N'Hub USB', 18500.00, @accesorios);
GO
SELECT p.idProducto, p.nombre, p.precio, c.descripcion AS categoria
FROM dbo.productos AS p JOIN dbo.categorias AS c ON p.categoria = c.idCategoria
ORDER BY p.idProducto;
