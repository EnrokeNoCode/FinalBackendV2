namespace Persistence.SQL.Venta
{
    public class PedidoVenta_sql
    {
        public string SelectList(int pageSize, int offset)
        {
            return $@"select p.codpedidov , p.fechapedidov , t.numtipocomprobante || ' ' || p.numpedventa as numpedventa  , s.numsucursal || ' ' || s.dessucursal as sucursal
                , c.nrodoc || '- ' || c.nombre || ', ' || c.apellido  as cliente, v.numvendedor  || '- ' || emp.nombre_emp || ', ' || emp.apellido_emp as vendedor,
                e.desestmov , p.totalpedidov 
                from sales.pedidoventa p 
                inner join referential.sucursal s on p.codsucursal = s.codsucursal 
                inner join referential.vendedor v on p.codvendedor = v.codvendedor 
                inner join referential.empleado emp on v.codempleado  = emp.codempleado 
                inner join referential.tipocomprobante t on p.codtipocomprobante  = t.codtipocomprobante 
                inner join referential.estadomovimiento e on p.codestmov = e.codestmov 
                inner join referential.cliente c on p.codcliente  = c.codcliente 
                order by p.fechapedidov desc
                limit {pageSize} offset {offset}";
        }

        public string SelectListPrst()
        {
            return @"
                SELECT 
                    p.codpedidov,
                    p.fechapedidov,
                    t.numtipocomprobante || ' ' || p.numpedventa AS numpedventa,
                    s.numsucursal || ' ' || s.dessucursal AS sucursal,
                    c.nrodoc || '- ' || c.nombre || ', ' || c.apellido AS cliente,
                    v.numvendedor || '- ' || emp.nombre_emp || ', ' || emp.apellido_emp AS vendedor,
                    e.desestmov,
                    p.totalpedidov
                FROM sales.pedidoventa p
                INNER JOIN referential.sucursal s ON p.codsucursal = s.codsucursal
                INNER JOIN referential.vendedor v ON p.codvendedor = v.codvendedor
                INNER JOIN referential.empleado emp ON v.codempleado = emp.codempleado
                INNER JOIN referential.tipocomprobante t ON p.codtipocomprobante = t.codtipocomprobante
                INNER JOIN referential.estadomovimiento e ON p.codestmov = e.codestmov
                INNER JOIN referential.cliente c ON p.codcliente = c.codcliente
                WHERE p.codcliente = @codcliente
                ORDER BY p.fechapedidov DESC;
            ";
        }


        public string SelectStatus()
        {
            return @"
                    SELECT codestmov 
                    FROM sales.pedidoventa
                    WHERE codpedidov  = @codpedventa";
        }

        public string Insert()
        {
            return @"SELECT sales.fn_insert_pedventa(
                        @codtipocomprobante,
                        @codsucursal,
                        @codestmov,
                        @fechapedidov,
                        @numpedventa,
                        @codvendedor,
                        @codcliente,
                        @codmoneda,
                        @totalpedidov,
                        @cotizacion1,
                        @ultimo,
                        @codterminal,
                        @detalles
                    );";
        }

        public string Update(int option)
        {
            string sentece = "";
            if (option == 1)
            {
                sentece = @"update sales.pedidoventa pc set codestmov = @codestado
                             where pc.codpedidov = @codpedventa ";
            }
            else if (option == 2)
            {
                sentece = @"";
            }
            else if (option == 3)
            {
                sentece = @"select sales.fn_update_pedidoventaestado(@codpedidov, @codestado)";
            }
            return sentece;
        }
    }

    public class PresupuestoVenta_sql
    {
        public string SelectList(int pageSize, int offset)
        {
            return $@"select pv.codpresupuestoventa , pv.fechapresupuestoventa , t.numtipocomprobante  || ' ' || pv.numpresupuestoventa as numpresupuestoventa, s.numsucursal || ' ' || s.dessucursal as sucursal
                    ,c.nrodoc || '- ' || c.nombre || ', ' || c.apellido  as cliente, v.numvendedor  || '- ' || emp.nombre_emp || ', ' || emp.apellido_emp as vendedor,
                    e.desestmov, case when pv.condicionpago = 0 then 'CONTADO' else 'CREDITO' end as condicionpago, pv.totalpresupuestoventa,
                    case when pv.codpedidov is null then 'NO HAY PEDIDO' else 'Fecha Pedido V.: ' || pdv.fechapedidov   || '  Nro Pedido V..: ' || pdv.numpedventa  end as datopedidoventa
                    from sales.presupuestoventa pv
                    inner join referential.sucursal s on pv.codsucursal = s.codsucursal 
                    inner join referential.vendedor v on pv.codvendedor = v.codvendedor 
                    inner join referential.empleado emp on v.codempleado  = emp.codempleado 
                    inner join referential.tipocomprobante t on pv.codtipocomprobante  = t.codtipocomprobante 
                    inner join referential.estadomovimiento e on pv.codestmov = e.codestmov 
                    inner join referential.cliente c on pv.codcliente  = c.codcliente 
                    left join sales.pedidoventa pdv on pv.codpedidov = pdv.codpedidov order by 1
                    limit {pageSize} offset {offset};";
        }

        public string Select(int option)
        {
            string sentence = "";
            switch (option)
            {
                case 1:
                    sentence = @"SELECT codestmov 
                                FROM sales.presupuestoventa
                                WHERE codpresupuestoventa  = @codpresupuestoventa";
                    break;
                case 2:
                    sentence = @"select pv.codpresupuestoventa , pv.fechapresupuestoventa , t.numtipocomprobante  || ' ' || pv.numpresupuestoventa as numpresupuestoventa
                                ,c.nrodoc || '- ' || c.nombre || ', ' || c.apellido  as cliente, pv.codvendedor, 
                                v.numvendedor  || '- ' || emp.nombre_emp || ', ' || emp.apellido_emp as vendedor,
                                pv.condicionpago as codcondicion,
                                case when pv.condicionpago = 0 then 'CONTADO' else 'CREDITO' end as condicionpago, pv.totalpresupuestoventa
                                from sales.presupuestoventa pv
                                inner join referential.vendedor v on pv.codvendedor = v.codvendedor 
                                inner join referential.empleado emp on v.codempleado  = emp.codempleado 
                                inner join referential.tipocomprobante t on pv.codtipocomprobante  = t.codtipocomprobante 
                                inner join referential.cliente c on pv.codcliente  = c.codcliente where pv.codcliente = @codcliente and pv.codestmov = 1 ";
                    break;
            }
            return sentence;
        }
        public string SelectDetails(int option)
        {
            string sentence = "";
            switch (option)
            {
                case 1:
                    sentence = @"select pvd.codpresupuestoventa , pvd.codproducto, pr.codigobarra || ' - ' || pr.desproducto as datoproducto, pvd.codiva , t.desiva as datoiva, 
                        pvd.cantidad, pvd.preciobruto, (pvd.cantidad  * pvd.preciobruto) as totallinea 
                        from sales.presupuestoventadet pvd
                        inner join referential.producto pr on pvd.codproducto = pr.codproducto 
                        inner join referential.tipoiva t on pvd.codiva = t.codiva where pvd.codpresupuestoventa = @codpresupuestoventa";
                    break;
            }
            return sentence;
        }

        public string Insert()
        {
            return @"SELECT sales.fn_insert_presupuestoventa(
                        @codtipocomprobante,
                        @codsucursal,
                        @codvendedor,
                        @codcliente,
                        @fechapresupuesto,
                        @numprstventa,
                        @codpedidov,
                        @observacion,
                        @diaven,
                        @condicionpago,
                        @codmoneda,
                        @cotizacion,
                        @codestmov,
                        @totaliva,
                        @totaldescuento,
                        @totalexento,
                        @totalgravada,
                        @totalpresupuestoventa,
                        @codterminal,
                        @ultimo,
                        @detalles
                    );";
        }
        public string Update(int option)
        {
            string sentece = "";
            if (option == 1)
            {
                sentece = @"update sales.presupuestoventa pc set codestmov = @codestado
                             where pc.codpresupuestoventa = @codpresupuestoventa ";
            }
            else if (option == 2)
            {
                sentece = @"";
            }
            else if (option == 3)
            {
                sentece = @"select sales.fn_update_presupuestoventaestado(@codpresupuestoventa, @codestado)";
            }
            return sentece;
        }

    }
    public class Ventas_sql
    {
        public string SelectList(int pageSize, int offset)
        {
            return $@"select
                    v.codventa , tc.numtipocomprobante || '-' || v.numventa as datosventa, v.fechaventa , c.nrodoc || '- ' || c.nombre || ', ' || c.apellido as datocliente ,
                    s.dessucursal as datosucursal, e.desestmov as desestmov , ven.numvendedor || '- ' || emp.nombre_emp || ', ' || emp.apellido_emp as datovendedor, v.totalventa,
                    case when pv.codpresupuestoventa is null then 'NO HAY PRESUPUESTO' else 'Fecha PrstV: ' || pv.fechapresupuestoventa   || ' Nro PrstV: ' || pv.numpresupuestoventa end as datopresupuesto,
                    case when v.condicionpago  = 0 then 'CONTADO' else 'CREDITO' end as condicion, m.nummoneda as moneda
                    from sales.ventas v 
                    left join sales.presupuestoventa pv on v.codpresupuestoventa = pv.codpresupuestoventa 
                    inner join referential.tipocomprobante tc on v.codtipocomprobante = tc.codtipocomprobante 
                    inner join referential.cliente c on v.codcliente = c.codcliente 
                    inner join referential.vendedor ven on v.codvendedor = ven.codvendedor 
                    inner join referential.sucursal s on v.codsucursal = s.codsucursal 
                    inner join referential.moneda m on v.codmoneda = m.codmoneda 
                    inner join referential.estadomovimiento e on v.codestmov = e.codestmov 
                    inner join referential.empleado emp on ven.codempleado = emp.codempleado
                    order by v.fechaventa, v.codventa desc
                                limit {pageSize} offset {offset}";

        }

        public string Select(int option)
        {
            string sentence = "";
            switch (option)
            {
               
                case 1:
                    //para el update estado
                    sentence = @"";
                    break;
                case 2:
                    //para el ver
                    sentence = @"";
                    break;
                case 3:
                    sentence = @"select v.codventa, 'Fecha Venta: ' || v.fechaventa || ' Nro. Fac: ' || tc.numtipocomprobante || '- ' || v.numventa as datoventa,
                                v.codmoneda , m.nummoneda ,v.cotizacion
                                from sales.ventas v 
                                inner join referential.tipocomprobante tc on v.codtipocomprobante = tc.codtipocomprobante
                                inner join referential.moneda m on v.codmoneda = m.codmoneda 
                                where v.codcliente = @codcliente and v.codestmov != 4 order by v.fechaventa;";
                    break;
                case 4:
                    // para las cobranzas
                    sentence = @"select v.codventa, v.numventa, TO_CHAR(v.fechaventa, 'DD/MM/YYYY'), v.totalventa, 
                                v.codmoneda, v.cotizacion, m.nummoneda 
                                from sales.ventas v 
                                inner join referential.moneda m on v.codmoneda = m.codmoneda 
                                where v.codestmov = 1 and v.condicionpago = 0 and v.codcliente = @codcliente;";
                    break;
            }
            return sentence;
        }

        public string SelectDetails(int option)
        {
            string sentence = "";
            switch (option)
            {
                case 1:
                    // este es para el ver
                    sentence = @"";
                    break;
                case 2:
                    sentence = @"SELECT * FROM shared.fn_notacredito_list_detalle('VENTA', @codventa);";
                    break;
            }
            return sentence;
        }

        public string Insert()
        {
            return @"
                    SELECT sales.fn_insert_ventas(
                        @codtipocomprobante,
                        @numventa,
                        @fechaventa,
                        @codcliente,
                        @codterminal,
                        @ultimo,
                        @finvalideztimbrado,
                        @nrotimbrado,
                        @codsucursal,
                        @codvendedor,
                        @codestmov,
                        @condicionpago,
                        @codmoneda,
                        @cotizacion,
                        @observacion,
                        @totaliva,
                        @totaldescuento,
                        @totalexento,
                        @totalgravada,
                        @totalventa,
                        @codpresupuestoventa,
                        @cant_cuotas,
                        @detalles
                    );";
        }

        public string InsertCobros()
        {
            return @"select sales.fn_insert_cobroscontado(@codgestion, @detallesVentas, @detallesFormaPago)";
        }

        public string Update(int option) {
            string sentence = "";
            switch (option) { 
                case 1:
                    sentence = @"select sales.fn_update_ventaestado(@codventa, @codestado)";
                break;
            }
            return sentence;
        }
    }

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