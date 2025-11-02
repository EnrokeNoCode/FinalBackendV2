namespace Persistence.SQL.Referencial
{
    public class ProductoSQL
    {
        private string query = "";

        public string SelectProducto(int pageSize, int offset)
        {
            query = @$"select prd.codproducto, prd.codigobarra || '- ' || prd.desproducto as datoproducto,
                        prv.nrodocprv || '- ' || prv.razonsocial  as datoproveedor, ti.desiva as datoiva,
                        CASE 
                            WHEN prd.afectastock = TRUE THEN 'Si Actualiza'
                            WHEN prd.afectastock = false THEN 'No Actualiza'
                            ELSE 'No Actualiza'
                        END AS afectastock,
                        CASE 
                            WHEN prd.activo = TRUE THEN 'Activo'
                            WHEN prd.activo = false THEN 'Inactivo'
                            ELSE 'Inactivo'
                        END AS estado,
                        prd.costoultimo 
                        from referential.producto prd
                        inner join referential.proveedor prv on prd.codproveedor = prv.codproveedor
                        inner join referential.tipoiva ti on prd.codiva = ti.codiva LIMIT {pageSize} OFFSET {offset};";
            return query;
        }
        public string SelectProductoVenta()
        {
            query = @"select p.codproducto, p.codigobarra || '- ' || p.desproducto as datoproducto, pvp.precioventa, 
                      (select coalesce(ps.cantidad, 0) from referential.productosucursal ps where ps.codproducto = pvp.codproducto and ps.codsucursal = pvp.codsucursal) as cantidad,
                      p.codiva , ti.desiva , ti.coheficiente 
                      from referential.precioventaproducto pvp
                      inner join referential.producto p on pvp.codproducto = p.codproducto 
                      inner join referential.tipoiva ti on p.codproducto = ti.codiva 
                      where pvp.codsucursal  = @codsucursal ;";
            return query;
        }

        public string SelectProductoCompra()
        {
            query = @"select prd.codproducto, prd.codigobarra , prd.desproducto,
                        prv.nrodocprv || '- ' || prv.razonsocial  as datoproveedor, ti.codiva , ti.desiva , ti.coheficiente , prd.costoultimo 
                        from referential.producto prd
                        inner join referential.proveedor prv on prd.codproveedor = prv.codproveedor
                        inner join referential.tipoiva ti on prd.codiva = ti.codiva ;";
            return query;
        }
    }
}
