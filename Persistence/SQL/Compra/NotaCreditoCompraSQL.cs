namespace Persistence.SQL.Compra
{
    public class NotaCreditoCompra_Sql
    {
        public string SelectList(int pageSize, int offset)
        {
            return $@"select nc.codnotacredito, coalesce(nc.codcompra,0) as codcompra, nc.fechanotacredito , 
                    tnc.numtipocomprobante || '- ' || nc.numnotacredito as nronotacredito,
                    case 
                    	when coalesce(nc.codcompra,0) = 0 then 'ANULADO'
                    	when nc.codcompra != 0 then tc.numtipocomprobante || '- ' || c.numcompra
                    end
                    as datocompra, s.dessucursal ,
                    prv.nrodocprv || '- ' || prv.razonsocial as datoproveedor, nc.totaldevolucion , m.nummoneda , em.numestmov , em.desestmov
                    from shared.notacredito nc 
                    left join purchase.compras c on nc.codcompra = c.codcompra 
                    inner join referential.tipocomprobante tnc on nc.codtipocomprobante = tnc.codtipocomprobante 
                    left join referential.tipocomprobante tc on tc.codtipocomprobante = c.codtipocomprobante 
                    inner join referential.moneda m on nc.codmoneda = m.codmoneda 
                    inner join referential.proveedor prv on nc.codproveedor  = prv.codproveedor 
                    inner join referential.sucursal s on nc.codsucursal = s.codsucursal
                    inner join referential.estadomovimiento em on nc.codestmov = em.codestmov
                    where nc.movimiento = 'COMPRAS' and nc.codsucursal = @codsucursal
                    order by nc.codnotacredito , nc.fechanotacredito desc
                    limit {pageSize} offset {offset}";

        }

        public string Insert()
        {
            return @"
                    SELECT shared.fn_insert_notacredito_compra(
                        @codcompra,
                        @codproveedor,
                        @codtipocomprobante,
                        @numnotacredito,
                        @fechanotacredito,
                        @nrotimbrado,
                        @fechavalidez,
                        @codestmov,
                        @codempleado,
                        @codsucursal,
                        @codmoneda,
                        @cotizacion,
                        @totaliva,
                        @totaldescuento,
                        @totalexento,
                        @totalgravada,
                        @totaldevolucion,
                        @codterminal,
                        @ultimo,
                        @detalles
                    );";
        }
        
        public string Select(int option)
        {
            string sentence = "";

            switch (option)
            {
                case 1:
                    sentence = @"select nc.nrotimbrado, nc.fechavalidez, nc.fechanotacredito , tc.numtipocomprobante || '- ' || nc.numnotacredito as datonotacredito, nc.totaldevolucion ,
                                p.nrodocprv  || '- ' || p.razonsocial as datoproveedor, 
                                case
                                when coalesce(nc.codcompra,0) = 0 then 'LA COMPRA YA NO SE ENCUENTRA ASOCIADA'
                                when coalesce(nc.codcompra,0) != 0 then 'Fecha Compra: ' || c.fechacompra || ' Nro. Fac: ' || tcc.numtipocomprobante || '- ' || c.numcompra
                                end
                                as datocompra
                                from shared.notacredito nc 
                                left join purchase.compras c on nc.codcompra = c.codcompra
                                inner join referential.tipocomprobante tc on nc.codtipocomprobante = tc.codtipocomprobante
                                left join referential.tipocomprobante tcc on c.codtipocomprobante = tcc.codtipocomprobante
                                inner join referential.proveedor p on nc.codproveedor = p.codproveedor 
                                where nc.movimiento = 'COMPRAS' and nc.codnotacredito = @codnotacredito
                                ;";
                    break;
            }

            return sentence;
        }

        public string SelectDetail(int option)
        {
            string sentence = "";

            switch (option)
            {
                case 1:
                    sentence = @"select 'Cod Barra: ' || p.codigobarra || ' Descripcion: ' || p.desproducto as datoproducto,
                    t.desiva, ncd.preciobruto, ncd.precioneto, ncd.cantidaddev, (ncd.preciobruto * ncd.cantidaddev ) totallinea  
                    from shared.notacreditodet ncd
                    inner join referential.producto p on ncd.codproducto = p.codproducto
                    inner join referential.tipoiva t on ncd.codiva = t.codiva
                    where ncd.codnotacredito = @codnotacredito;";
                    break;
            }

            return sentence;
        }

        public string Update()
        {
            return @"SELECT shared.fn_update_notacreditoestado(@codnotacredito, @codestado, 'COMPRA')";
        }
    }
}