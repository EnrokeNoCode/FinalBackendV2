namespace Persistence.SQL.Venta
{
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
                case 3:
                    sentence = @"select pv.codpresupuestoventa , to_char(pv.fechapresupuestoventa,'DD/MM/YYYY') as fechapresupuestoventa, t.numtipocomprobante ,pv.numpresupuestoventa 
                                ,c.nrodoc || '- ' || c.nombre || ', ' || c.apellido  as cliente, 
                                v.numvendedor  || '- ' || emp.nombre_emp || ', ' || emp.apellido_emp as vendedor,
                                case when pv.condicionpago = 0 then 'CONTADO' else 'CREDITO' end as condicionpago, pv.totalpresupuestoventa,
                                pv.diaven, m.nummoneda , pv.cotizacion1 , coalesce(pv.observacion,'') as observacion,
                                case when pv.codpedidov is null then 'NO HAY PEDIDO' else 'Fecha Pedido V.: ' || pvd.fechapedidov    || '  Nro Pedido V.: ' || tc.numtipocomprobante  ||'- ' || pvd.numpedventa  end as datopedidoventa
                                , s.dessucursal 
                                from sales.presupuestoventa pv
                                left join sales.pedidoventa pvd on pv.codpedidov = pvd.codpedidov 
                                inner join referential.vendedor v on pv.codvendedor = v.codvendedor 
                                inner join referential.empleado emp on v.codempleado  = emp.codempleado 
                                inner join referential.tipocomprobante t on pv.codtipocomprobante  = t.codtipocomprobante 
                                left join referential.tipocomprobante tc on pvd.codtipocomprobante  = tc.codtipocomprobante 
                                inner join referential.cliente c on pv.codcliente  = c.codcliente
                                inner join referential.moneda m on pv.codmoneda = m.codmoneda  
                                inner join referential.sucursal s on pv.codsucursal = s.codsucursal where pv.codpresupuestoventa = @codpresupuestoventa;";
                    break;
            }
            return sentence;
        }
        public string SelectWithDetails(int option)
        {
            string sentence = "";
            switch (option)
            {
                case 1:
                    sentence = @"select  pr.codigobarra || ' - ' || pr.desproducto as datoproducto, pvd.codiva , t.desiva as datoiva, 
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
}