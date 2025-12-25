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
                        prd.costoultimo ,
                        'Marca: ' || m.desmarca || ' |Familia: ' || f.desfamilia || ' |Rubro: ' || r.desrubro as datoseccion
                        from referential.producto prd
                        inner join referential.proveedor prv on prd.codproveedor = prv.codproveedor
                        inner join referential.marca m on prd.codmarca = m.codmarca 
                        inner join referential.rubro r on prd.codrubro = r.codrubro 
                        inner join referential.familia f on prd.codfamilia = f.codfamilia 
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

        public string SelectProductoMod()
        {
            query = @"select prd.codproducto, prd.codigobarra ,prd.desproducto , prd.codproveedor ,
                        prv.nrodocprv || '- ' || prv.razonsocial  as datoproveedor, prd.codiva, ti.desiva ,
                        coalesce(prd.afectastock,false) as afectastock,
                        coalesce(prd.activo,false) as activo,
                        prd.codmarca, m.desmarca ,prd.codfamilia ,f.desfamilia, prd.codrubro , r.desrubro ,
                        prd.costoultimo 
                        from referential.producto prd
                        inner join referential.proveedor prv on prd.codproveedor = prv.codproveedor
                        inner join referential.marca m on prd.codmarca = m.codmarca 
                        inner join referential.rubro r on prd.codrubro = r.codrubro 
                        inner join referential.familia f on prd.codfamilia = f.codfamilia 
                        inner join referential.tipoiva ti on prd.codiva = ti.codiva where prd.codproducto = @codproducto ;";
            return query;
        }

        public string UpdateProducto()
        {
            query = @"
                    update referential.producto
                    set codigobarra = @codigobarra,
                        desproducto = @desproducto,
                        codproveedor = @codproveedor,
                        codmarca = @codmarca,
                        codfamilia = @codfamilia,
                        codrubro = @codrubro,
                        codiva = @codiva,
                        afectastock = @afectastock,
                        costoultimo = @costoultimo
                    where codproducto = @codproducto
                ";
            return query;
        }

        public string Insert()
        {
            query = @"select referential.fn_insert_producto(@codigobarra, @desproducto, @codfamilia, @codmarca, @codrubro, @codunidadmedida, @codiva, @codproveedor, @costoultimo, @activo, @afectastock) ;";
            return query;
        }

        public string UpdateDeleteStatus()
        {
            query = @"select referential.fn_inactivar_eliminar_registro(@cod, 'producto')";
            return query;
        }
    }
}
