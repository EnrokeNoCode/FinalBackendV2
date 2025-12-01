namespace Persistence.SQL.Compra
{
    public class OrdenCompra_Sql
    {
        public string SelectList(int pageSize, int offset)
        {
            return $@"select o.codordenc , o.fechaorden , o.fechagenerado , o.numordencompra , s.numsucursal, s.dessucursal, t.numtipocomprobante, t.destipocomprobante,
					m.nummoneda,o.totaliva, o.totalexento, o.totalgravada, o.totalordencompra , o.totaldescuento,
                    prv.nrodocprv , prv.razonsocial , e.numdoc , e.nombre_emp || ', ' || e.apellido_emp as empleado, em.numestmov , em.desestmov ,
                        case 
                            when o.codpresupuestocompra  is null then 'NO HAY PRESUPUESTO'
                            else 'Fecha PrstC: ' || pc.fechapresupuesto  || '  Nro PrstC.: ' || pc.numpresupuestocompra  
                        end as datopresupuesto,
                        case 
                            when o.condicionpago = 0 then 'CONTADO'
                            else 'CREDITO'
                        end as condicion
                    from purchase.ordencompra o 
                    inner join referential.proveedor prv on o.codproveedor = prv.codproveedor 
                    inner join referential.empleado e on o.codempleado = e.codempleado 
                    inner join referential.moneda m on o.codmoneda = m.codmoneda 
                    inner join referential.tipocomprobante t on o.codtipocomprobante = t.codtipocomprobante 
                    inner join referential.sucursal s on o.codsucursal = s.codsucursal 
                    inner join referential.estadomovimiento em on o.codestmov = em.codestmov 
                    left join purchase.presupuestocompra pc on o.codpresupuestocompra  = pc.codpresupuestocompra 
                    where o.codsucursal  = @codsucursal
                    order by o.fechaorden, o.codordenc desc
                    limit {pageSize} offset {offset}";
        }

        public string Select(int option){
            string sentence = "";
            switch (option)
            {
                case 1:
                    sentence = @" SELECT codestmov FROM purchase.ordencompra WHERE codordenc  = @codordenc";
                    break;
                case 2:
                    sentence = @"select 
                            oc.codordenc , tc.numtipocomprobante || '-' || oc.numordencompra as numordencompra, oc.fechaorden , prv.nrodocprv || '- ' || prv.razonsocial as datoproveedor ,
                            case when pc.codpresupuestocompra is null then 'NO HAY PRESUPUESTO' else 'Fecha PrstC: ' || pc.fechapresupuesto   || '  Nro PrstC: ' || pc.numpresupuestocompra end as datopresupuesto,
                            oc.totaliva, oc.totalexento , oc.totalordencompra , 
                            case when oc.condicionpago  = 0 then 'CONTADO' else 'CREDITO' end as condicion, m.nummoneda as moneda, oc.cotizacion 
                            from purchase.ordencompra oc 
                            left join purchase.presupuestocompra pc on oc.codpresupuestocompra = pc.codpresupuestocompra 
                            inner join referential.proveedor prv on oc.codproveedor = prv.codproveedor 
                            inner join referential.tipocomprobante tc on oc.codtipocomprobante = tc.codtipocomprobante 
                            inner join referential.moneda m on oc.codmoneda = m.codmoneda 
                            where oc.codordenc = @codordenc;";
                    break;
                case 3:
                    sentence = @"select oc.codordenc , oc.fechaorden , oc.codproveedor ,
                            t.numtipocomprobante || '- ' || oc.numordencompra as datoordencompra, oc.totalordencompra 
                            from purchase.ordencompra oc 
                            inner join referential.tipocomprobante t on oc.codtipocomprobante = t.codtipocomprobante 
                            where oc.codestmov = 1 and oc.codproveedor = @codproveedor order by 1, oc.fechaorden asc;";
                    break;
            }
            return sentence;

        }

        public string SelectDetails(int option)
        {
            string sentence = "";

            switch (option) { 
                case 1:
                    sentence = @"select ocd.codordenc, ocd.codproducto, pr.codigobarra || ' - ' || pr.desproducto as datoproducto, ocd.codiva , t.desiva as datoiva, 
                        ocd.cantidad, ocd.preciobruto, (ocd.cantidad  * ocd.preciobruto) as totallinea 
                        from purchase.ordencompradet ocd
                        inner join referential.producto pr on ocd.codproducto = pr.codproducto 
                        inner join referential.tipoiva t on ocd.codiva = t.codiva where ocd.codordenc = @codordenc";
                    break;
                case 2:
                    sentence = @"select ocd.codordenc, ocd.codproducto, pr.codigobarra || ' - ' || pr.desproducto as datoproducto, ocd.codiva , t.desiva as datoiva,
                        ocd.cantidad, ocd.preciobruto, (ocd.cantidad* ocd.preciobruto) as totallinea
                        from purchase.ordencompradet ocd
                        inner join referential.producto pr on ocd.codproducto = pr.codproducto
                        inner join referential.tipoiva t on ocd.codiva = t.codiva where ocd.codordenc = @codordenc";
                    break;
            }

            return sentence;
        }
        public string Insert()
        {
            return @"
                        SELECT purchase.fn_insert_ordencompra(
                            @codtipocomprobante,
                            @codterminal,
                            @ultimo,
                            @numordencompra,
                            @fechaorden,
                            @codestmov,
                            @codempleado,
                            @codproveedor,
                            @codmoneda,
                            @codsucursal,
                            @totaliva,
                            @totaldescuento,
                            @totalexento,
                            @totalgravada,
                            @totalordencompra,
                            @cotizacion,
                            @observacion,
                            @condiciopago,
                            @codpresupuestocompra,
                            @detalles
                        );";
        }

        public string Update(int option)
        {
            string sentece = "";
            if (option == 1)
            {
                sentece = @"update purchase.ordencompra oc set codestmov = @codestado
                             where oc.codordenc = @codordenc ";
            }
            else if (option == 2)
            {
                sentece = @"select purchase.fn_update_ordencompraestado(@codordenc,@codestado)";
            }
            return sentece;
        }
    }
}