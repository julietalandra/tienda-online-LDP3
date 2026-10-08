-- Instalación nueva: ejecutar una sola vez en SQL Server.
create database commit_store;
go

use commit_store;
go

create table [categorías] (
    idCategoria int identity primary key,
    [descripción] nvarchar(60) not null
);

create table productos (
    idProducto int identity primary key,
    nombre nvarchar(100) not null,
    precio decimal(18,2) not null check (precio > 0),
    [categoría] int not null references [categorías](idCategoria)
);
go

-- Categorías de demostración.
insert into [categorías] ([descripción]) values (N'Periféricos');
insert into [categorías] ([descripción]) values (N'Escritorio');
insert into [categorías] ([descripción]) values (N'Accesorios');

-- Cinco productos de demostración. las categorías son 1, 2 y 3.
insert into productos (nombre, precio, [categoría]) values (N'Teclado mecánico', 85000.00, 1);
insert into productos (nombre, precio, [categoría]) values (N'Mouse inalámbrico', 32000.00, 1);
insert into productos (nombre, precio, [categoría]) values (N'Auriculares', 48500.00, 1);
insert into productos (nombre, precio, [categoría]) values (N'Soporte para notebook', 24000.00, 2);
insert into productos (nombre, precio, [categoría]) values (N'Hub USB', 18500.00, 3);
