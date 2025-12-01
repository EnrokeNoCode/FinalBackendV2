namespace Persistence.SQL.Compra
{
    public class Compras_Sql
    {
        public string SelectList(int pageSize, int offset)
        {
            return $@"select c.codcompra , c.fechacompra , c.numcompra , s.numsucursal , s.dessucursal , t.numtipocomprobante, t.destipocomprobante,
                    m.nummoneda,c.totaliva, c.totalexento, c.totalgravada, c.totalcompra , c.totaldescuento,
                    prv.nrodocprv , prv.razonsocial , e.numdoc , e.nombre_emp || ', ' || e.apellido_emp as empleado, em.numestmov , em.desestmov ,
                                case 
                                    when c.codordenc   is null then 'SIN ORDEN'
                                    else 'Fecha OrdenC: ' || o.fechaorden  || '  Nro OrdenC.: ' || o.numordencompra   
                                end as datoordencompra,
                                case 
                                    when c.condicionpago = 0 then 'CONTADO'
                                    else 'CREDITO'
                                end as condicion
                    from purchase.compras c
                    inner join referential.proveedor prv on c.codproveedor = prv.codproveedor 
                    inner join referential.empleado e on c.codempleado = e.codempleado 
                    inner join referential.moneda m on c.codmoneda = m.codmoneda 
                    inner join referential.tipocomprobante t on c.codtipocomprobante = t.codtipocomprobante 
                    inner join referential.sucursal s on c.codsucursal = s.codsucursal 
                    inner join referential.estadomovimiento em on c.codestmov = em.codestmov 
                    left join purchase.ordencompra o on c.codordenc   = o.codordenc
                    where c.codsucursal = @codsucursal
                    order by c.fechacompra, c.codcompra desc
                    limit {pageSize} offset {offset}";
        }

        public string Insert()
        {
            return @"
                    SELECT purchase.fn_insert_compra2(
                        @codtipocomprobante,
                        @numcompra,
                        @fechacompra,
                        @codproveedor,
                        @codterminal,
                        @ultimo,
                        @finvalideztimbrado,
                        @nrotimbrado,
                        @codsucursal,
                        @codempleado,
                        @codestmov,
                        @condicionpago,
                        @codmoneda,
                        @cotizacion,
                        @observacion,
                        @totaliva,
                        @totaldescuento,
                        @totalexento,
                        @totalgravada,
                        @totalcompra,
                        @codordenc,
                        @cant_cuotas,
                        @detalles
                    );";
        }

        public string Select(int option)
        {
            string sentence = "";
            switch (option)
            {
                case 1:
                    sentence = @"SELECT codestmov 
                                FROM purchase.compras
                                WHERE codcompra  = @codcompra";
                    break;
                case 2:
                    sentence = @"select 
                                c.codcompra , tc.numtipocomprobante || '-' || c.numcompra  as numcompra, c.fechacompra  , prv.nrodocprv || '- ' || prv.razonsocial as datoproveedor ,
                                case when c.codordenc is null then 'NO HAY ORDEN' else 'Fecha OrdenC: ' || oc.fechaorden    || '  Nro OrdenC: ' || oc.numordencompra  end as datoordenc,
                                c.totaliva, c.totalexento , c.totalcompra  , 
                                case when oc.condicionpago  = 0 then 'CONTADO' else 'CREDITO' end as condicion, m.nummoneda as moneda, oc.cotizacion 
                                from purchase.compras c 
                                left join purchase.ordencompra oc on oc.codordenc  = c.codordenc 
                                inner join referential.proveedor prv on c.codproveedor = prv.codproveedor 
                                inner join referential.tipocomprobante tc on c.codtipocomprobante = tc.codtipocomprobante 
                                inner join referential.moneda m on c.codmoneda = m.codmoneda 
                                where c.codcompra = @codcompra;";
                    break;
                case 3:
                    sentence = @"select c.codcompra, 'Fecha Compra: ' || c.fechacompra || ' Nro. Fac: ' || tc.numtipocomprobante || '- ' || c.numcompra as datocompra,
                                c.codmoneda , m.nummoneda ,c.cotizacion
                                from purchase.compras c 
                                inner join referential.tipocomprobante tc on c.codtipocomprobante = tc.codtipocomprobante
                                inner join referential.moneda m on c.codmoneda = m.codmoneda 
                                where c.codproveedor = @codproveedor and c.codestmov != 4 order by c.fechacompra;";
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
                    sentence = @"select cd.codcompra , cd.codproducto, pr.codigobarra || ' - ' || pr.desproducto as datoproducto, cd.codiva , t.desiva as datoiva,
                        cd.cantidad, cd.preciobruto, (cd.cantidad* cd.preciobruto) as totallinea
                        from purchase.comprasdet cd
                        inner join referential.producto pr on cd.codproducto = pr.codproducto
                        inner join referential.tipoiva t on cd.codiva = t.codiva where cd.codcompra = @codcompra";
                    break;
                case 2:
                    sentence = @"SELECT * FROM shared.fn_notacredito_list_detalle('COMPRA', @codcompra);";
                    break;
                case 3:
                    sentence = @"select * from purchase.fn_remisioncompra_list_detalle(@codcompra);";
                    break;
            }
            return sentence;
        }

        public string Update(int option)
        {
            string sentece = "";
            if (option == 1)
            {
                sentece = @"update purchase.compras oc set codestmov = @codestado
                             where oc.codcompra = @codcompra ";
            }
            else if (option == 2)
            {
                sentece = @"select purchase.fn_update_compraestado(@codcompra,@codestado)";
            }
            return sentece;
        }
    }
}