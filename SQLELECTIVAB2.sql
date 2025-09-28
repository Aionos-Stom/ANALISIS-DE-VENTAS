create database DBELECTIVAETL;
use DBELECTIVAETL;

--LIMPIAR TABLLAS 
-- eliminar tablas dependientes primero para evitar errores de fk
if exists (select * from sys.tables where name = 'order_details')
    drop table order_details;

if exists (select * from sys.tables where name = 'orders')
    drop table orders;

if exists (select * from sys.tables where name = 'customers')
    drop table customers;

if exists (select * from sys.tables where name = 'products')
    drop table products;



--CREACION DE MIS TABLAS
-- tabla products (independiente)
create table products (
    productid int primary key not null,
    productname nvarchar(150) not null,
    category nvarchar(80) not null,
    price decimal(12,2) not null,
    stock int not null default 0
);


-- tabla customers (independiente)  
create table customers (
    customerid int primary key not null,
    firstname nvarchar(80) not null,
    lastname nvarchar(80) not null,
    email nvarchar(200) not null,
    phone nvarchar(40),
    city nvarchar(120),
    country nvarchar(120)
);


-- tabla orders (depende de customers)
create table orders (
    orderid int primary key not null,
    customerid int not null,
    orderdate datetime not null,
    status nvarchar(30) not null default 'pending',
    
    constraint fk_orders_customerid 
        foreign key (customerid) references customers(customerid)
        on delete cascade on update cascade
);


-- tabla order_details (depende de orders y products)
create table order_details (
    orderid int not null,
    productid int not null,
    quantity int not null,
    totalprice decimal(12,2) not null,
    
    primary key (orderid, productid),
    
    constraint fk_orderdetails_orderid 
        foreign key (orderid) references orders(orderid)
        on delete cascade on update cascade,
        
    constraint fk_orderdetails_productid 
        foreign key (productid) references products(productid)
        on delete cascade on update cascade
);



-- indices en foreign keys para mejorar joins
create index ix_orders_customerid on orders(customerid);
create index ix_orderdetails_orderid on order_details(orderid);
create index ix_orderdetails_productid on order_details(productid);

-- indices para consultas comunes
create index ix_orders_orderdate on orders(orderdate);
create index ix_products_category on products(category);
create index ix_customers_email on customers(email);
create index ix_products_price on products(price);
create index ix_orders_status on orders(status);

-- indices compuestos para consultas especificas
create index ix_orders_customer_date on orders(customerid, orderdate);
create index ix_products_category_price on products(category, price);

print 'indices creados para optimizar consultas';


-- validaciones para products
alter table products add constraint ck_products_price_positive 
    check (price > 0);

alter table products add constraint ck_products_stock_nonnegative 
    check (stock >= 0);

alter table products add constraint ck_products_name_notempty
    check (len(trim(productname)) > 0);

-- validaciones para customers
alter table customers add constraint ck_customers_email_format 
    check (email like '%@%.%');

alter table customers add constraint ck_customers_firstname_notempty
    check (len(trim(firstname)) > 0);

alter table customers add constraint ck_customers_lastname_notempty
    check (len(trim(lastname)) > 0);

-- validaciones para orders
alter table orders add constraint ck_orders_status_valid
    check (status in ('pending', 'processing', 'shipped', 'delivered', 'cancelled'));

alter table orders add constraint ck_orders_date_valid
    check (orderdate >= '2020-01-01');

-- validaciones para order_details
alter table order_details add constraint ck_orderdetails_quantity_positive 
    check (quantity > 0);

alter table order_details add constraint ck_orderdetails_totalprice_positive 
    check (totalprice > 0);

-- procedure para limpiar todas las tablas (optimizado)
if exists (select * from sys.procedures where name = 'sp_clearalltables')
    drop procedure sp_clearalltables;


create procedure sp_clearalltables
as
begin
    set nocount on;
    
    begin try
        begin transaction;
        
        -- deshabilitar restricciones fk temporalmente para mejor rendimiento
        alter table order_details nocheck constraint all;
        alter table orders nocheck constraint all;
        
        -- usar truncate para mejor rendimiento (mas rapido que delete)
        truncate table order_details;
        print '   - tabla order_details limpiada';
        
        truncate table orders;
        print '   - tabla orders limpiada';
        
        truncate table products;
        print '   - tabla products limpiada';
        
        truncate table customers;
        print '   - tabla customers limpiada';
        
        -- rehabilitar restricciones fk
        alter table orders check constraint all;
        alter table order_details check constraint all;
        
        commit transaction;
        print 'todas las tablas limpiadas exitosamente usando truncate';
        
    end try
    begin catch
        if @@trancount > 0
            rollback transaction;
            
        -- rehabilitar restricciones en caso de error
        alter table orders check constraint all;
        alter table order_details check constraint all;
        
        declare @errormessage nvarchar(4000) = error_message();
        print 'error limpiando tablas: ' + @errormessage;
        throw;
    end catch
end;


-- procedure para validar integridad de datos
if exists (select * from sys.procedures where name = 'sp_validateintegrity')
    drop procedure sp_validateintegrity;


create procedure sp_validateintegrity
as
begin
    set nocount on;
    
    declare @errors int = 0;
    
    -- verificar ordenes huerfanas
    select @errors = count(*)
    from orders o
    left join customers c on o.customerid = c.customerid
    where c.customerid is null;
    
    if @errors > 0
    begin
        print 'error: ' + cast(@errors as varchar) + ' ordenes sin cliente valido';
        return @errors;
    end
    
    -- verificar detalles huerfanos por orden
    select @errors = count(*)
    from order_details od
    left join orders o on od.orderid = o.orderid
    where o.orderid is null;
    
    if @errors > 0
    begin
        print 'error: ' + cast(@errors as varchar) + ' detalles sin orden valida';
        return @errors;
    end
    
    -- verificar detalles huerfanos por producto
    select @errors = count(*)
    from order_details od
    left join products p on od.productid = p.productid
    where p.productid is null;
    
    if @errors > 0
    begin
        print 'error: ' + cast(@errors as varchar) + ' detalles sin producto valido';
        return @errors;
    end
    
    print 'validacion de integridad exitosa - no se encontraron errores';
    return 0;
end;


-- procedure para obtener estadisticas de tablas
if exists (select * from sys.procedures where name = 'sp_gettablestats')
    drop procedure sp_gettablestats;

-- ver las tablas con mis resultados
create procedure sp_gettablestats
as
begin
    set nocount on;
    
    select 
        'customers' as tabla, 
        count(*) as registros,
        min(customerid) as id_minimo,
        max(customerid) as id_maximo
    from customers
    union all
    select 
        'products' as tabla, 
        count(*) as registros,
        min(productid) as id_minimo,
        max(productid) as id_maximo
    from products
    union all
    select 
        'orders' as tabla, 
        count(*) as registros,
        min(orderid) as id_minimo,
        max(orderid) as id_maximo
    from orders
    union all
    select 
        'order_details' as tabla, 
        count(*) as registros,
        min(orderid) as id_minimo,
        max(orderid) as id_maximo
    from order_details
    order by tabla;
end;



-- vista para resumen de ventas por categoria
if exists (select * from sys.views where name = 'vw_salesbycategory')
    drop view vw_salesbycategory;


create view vw_salesbycategory as
select 
    p.category,
    count(distinct o.orderid) as totalordenes,
    sum(od.quantity) as unidadesvendidas,
    sum(od.totalprice) as ventastotal,
    avg(od.totalprice) as promedioventa,
    count(distinct o.customerid) as clientesunicos
from order_details od
inner join products p on od.productid = p.productid
inner join orders o on od.orderid = o.orderid
group by p.category;


-- vista para clientes activos
if exists (select * from sys.views where name = 'vw_activecustomers')
    drop view vw_activecustomers;


create view vw_activecustomers as
select 
    c.customerid,
    c.firstname + ' ' + c.lastname as nombrecompleto,
    c.email,
    c.city,
    c.country,
    count(distinct o.orderid) as totalordenes,
    sum(od.totalprice) as totalgastado,
    avg(od.totalprice) as promediogasto,
    max(o.orderdate) as ultimacompra
from customers c
inner join orders o on c.customerid = o.customerid
inner join order_details od on o.orderid = od.orderid
group by c.customerid, c.firstname, c.lastname, c.email, c.city, c.country;



-- configurar recovery model para mejor rendimiento durante etl
alter database DBELECTIVAETL set recovery simple;

-- configurar auto growth para evitar fragmentacion
alter database DBELECTIVAETL 
modify file (name = 'DBELECTIVAETL', filegrowth = 100mb);

alter database DBELECTIVAETL 
modify file (name = 'DBELECTIVAETL_log', filegrowth = 50mb);

-- verificar estructura de tablas
print 'estructura de tablas creada:';
select 
    t.table_name as tabla,
    c.column_name as columna,
    c.data_type as tipo,
    c.character_maximum_length as longitud,
    c.is_nullable as nullable,
    case 
        when pk.column_name is not null then 'pk'
        when fk.column_name is not null then 'fk'
        else ''
    end as key_type
from information_schema.tables t
inner join information_schema.columns c on t.table_name = c.table_name
left join information_schema.key_column_usage pk on 
    c.table_name = pk.table_name and 
    c.column_name = pk.column_name and 
    pk.constraint_name like 'pk_%'
left join information_schema.key_column_usage fk on 
    c.table_name = fk.table_name and 
    c.column_name = fk.column_name and 
    fk.constraint_name like 'fk_%'
where t.table_type = 'base table'
    and t.table_name in ('customers', 'products', 'orders', 'order_details')
order by t.table_name, c.ordinal_position;

-- ejecutar estadisticas iniciales
exec sp_gettablestats;

-- conteo basico
select 'customers' as tabla, count(*) as registros from customers
union all
select 'products' as tabla, count(*) as registros from products
union all
select 'orders' as tabla, count(*) as registros from orders
union all
select 'order_details' as tabla, count(*) as registros from order_details;
