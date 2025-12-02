namespace Persistence.SQL.Venta
{
    public class NotaCreditoVenta_Sql
    {
        public string SelectList(int pageSize, int offset)
        {
            return $@"select nc.codnotacredito, nc.codventa , nc.fechanotacredito , 
                        tnc.numtipocomprobante || '- ' || nc.numnotacredito as nronotacredito,
                        tc.numtipocomprobante || '- ' || v.numventa as datoventa, s.dessucursal ,
                        cl.nrodoc  || '- ' || cl.nombre || ', ' || cl.apellido as datocliente, nc.totaldevolucion , m.nummoneda 
                        from shared.notacredito nc 
                        inner join sales.ventas v on nc.codventa = v.codventa 
                        inner join referential.tipocomprobante tnc on nc.codtipocomprobante = tnc.codtipocomprobante 
                        inner join referential.tipocomprobante tc on tc.codtipocomprobante = v.codtipocomprobante 
                        inner join referential.moneda m on nc.codmoneda = m.codmoneda 
                        inner join referential.cliente cl on nc.codcliente = cl.codcliente 
                        inner join referential.sucursal s on nc.codsucursal = s.codsucursal 
                        where nc.movimiento = 'VENTAS'
                        order by nc.codnotacredito , nc.fechanotacredito desc
                        limit {pageSize} offset {offset}; ";
        }

        public string Insert()
        {
            return @" SELECT shared.fn_insert_notacredito_venta(
                            @codventa,
                            @codcliente,
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
                    c.nrodoc || '- ' || c.nombre || ', ' || c.apellido as datocliente, 
                    'Fecha Venta: ' || v.fechaventa || ' Nro. Fac: ' || tc.numtipocomprobante || '- ' || v.numventa as datoventa
                    from shared.notacredito nc 
                    inner join sales.ventas v on nc.codventa = v.codventa
                    inner join referential.tipocomprobante tc on nc.codtipocomprobante = tc.codtipocomprobante
                    inner join referential.tipocomprobante tcv on v.codtipocomprobante = tcv.codtipocomprobante
                    inner join referential.cliente c on nc.codcliente = c.codcliente
                    where nc.movimiento = 'VENTAS' and nc.codnotacredito = @codnotacredito
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
    }
}