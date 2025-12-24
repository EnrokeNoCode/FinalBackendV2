namespace Persistence.SQL.Venta
{
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
                    sentence = @"select 
                                v.codventa , tc.numtipocomprobante || '-' || v.numventa  as numventa, v.fechaventa  , 
                                c.nrodoc || '- ' || c.nombre || ', ' || c.apellido as datocliente,
                                case when v.codpresupuestoventa is null then 'NO HAY PRES. ASIGNADO' 
                                else 'Fecha Presp: ' || pv.fechapresupuestoventa  || '  Nro Presp.: ' || pv.numpresupuestoventa  
                                end as datopresupuesto,
                                v.totaliva, v.totalexento , v.totalventa  , 
                                case when v.condicionpago  = 0 then 'CONTADO' else 'CREDITO' end as condicion, m.nummoneda as moneda, v.cotizacion 
                                from sales.ventas v 
                                left join sales.presupuestoventa pv on v.codpresupuestoventa  = pv.codpresupuestoventa 
                                inner join referential.cliente c  on v.codcliente = c.codcliente 
                                inner join referential.tipocomprobante tc on v.codtipocomprobante = tc.codtipocomprobante 
                                inner join referential.moneda m on v.codmoneda = m.codmoneda 
                                where v.codventa = @codventa;";
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
                    sentence = @"select vd.codventa , vd.codproducto, pr.codigobarra || ' - ' || pr.desproducto as datoproducto, vd.codiva , t.desiva as datoiva,
		                        vd.cantidad, vd.preciobruto, (vd.cantidad* vd.preciobruto) as totallinea
		                        from sales.ventasdet vd
		                        inner join referential.producto pr on vd.codproducto = pr.codproducto
		                        inner join referential.tipoiva t on vd.codiva = t.codiva where vd.codventa = @codventa";
                    break;
                case 2:
                    sentence = @"SELECT * FROM shared.fn_notacredito_list_detalle('VENTA', @codventa);";
                    break;
                case 3:
                    sentence = @"select * from sales .fn_remisionventa_list_detalle(@codventa);";
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

    
}