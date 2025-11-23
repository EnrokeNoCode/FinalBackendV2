namespace Persistence.SQL.Compra
{
    public class PedidosCompras_sql
    {

        public string SelectList(int pageSize, int offset)
        {
            return $@"
                    select p.codpedcompra, p.fechapedcompra, t.numtipocomprobante, t.destipocomprobante, 
                           p.numpedcompra, e.nombre_emp || ' ' || e.apellido_emp as empleado, 
                           e2.numestmov, e2.desestmov, s.numsucursal, s.dessucursal
                    from purchase.pedidocompra p
                    inner join referential.tipocomprobante t on p.codtipocomprobante = t.codtipocomprobante
                    inner join referential.empleado e on p.codempleado = e.codempleado
                    inner join referential.estadomovimiento e2 on p.codestmov = e2.codestmov
                    inner join referential.sucursal s on p.codsucursal = s.codsucursal
                    order by p.fechapedcompra, p.codpedcompra desc
                    limit {pageSize} offset {offset}";
        }
        public string Select()
        {
            return @"select p.codpedcompra, p.fechapedcompra, t.numtipocomprobante, t.destipocomprobante, 
                    p.numpedcompra , e.nombre_emp || ' ' || e.apellido_emp as empleado , e2.numestmov , e2.desestmov ,
                    s.numsucursal , s.dessucursal 
                    from purchase.pedidocompra p 
                    inner join referential.tipocomprobante t  on p.codtipocomprobante  = t.codtipocomprobante 
                    inner join referential.empleado e on p.codempleado  = e.codempleado 
                    inner join referential.estadomovimiento e2 on p.codestmov = e2.codestmov 
                    inner join referential.sucursal s on p.codsucursal = s.codsucursal
                    where p.codpedcompra = @codpedcompra";
        }

        public string SelectStatus()
        {
            return @"
                    SELECT codestmov 
                    FROM purchase.pedidocompra
                    WHERE codpedcompra  = @codpedcompra";
        }
        public string SelectWithDetails()
        {
            return @"select p.codpedcompra, p.codproducto, p2.codigobarra, p2.desproducto, p.cantidad, p.costoultimo
                    from purchase.pedidocompradet p
                    inner join referential.producto p2 on P.codproducto = P2.codproducto
                    where p.codpedcompra = @codpedcompra ";
        }

        public string Insert()
        {
            return @"SELECT purchase.fn_insert_pedcompra2(@codtipocomprobante, @codterminal, @ultimo, @numpedcompra, @fechapedcompra, @codestmov, @codempleado, @codsucursal, @detalles);";
        }

        public string Update(int option)
        {
            string sentece = "";
            if (option == 1)
            {
                sentece = @"update purchase.pedidocompra pc set codestmov = @codestado
                             where pc.codpedcompra = @codpedcompra ";
            }
            else if (option == 2)
            {
                sentece = @"select purchase.fn_update_pedidocompradet(@codpedcompra, @detalles)";
            }
            else if (option == 3)
            {
                sentece = @"select purchase.fn_update_pedidocompraestado(@codpedcompra, @codestado)";
            }
            return sentece;
        }
    }

    public class PresupuestoCompras_sql
    {
        public string SelectList(int pageSize, int offset)
        {
            return $@"select p.codpresupuestocompra, p.fechapresupuesto, t.numtipocomprobante, t.destipocomprobante, p.numpresupuestocompra, s.numsucursal, s.dessucursal, 
                    p.totaliva, p.totalexento, p.totalgravada, p.totalpresupuestocompra, p.totaldescuento,
                    prv.nrodocprv , prv.razonsocial , e.numdoc , e.nombre_emp || ', ' || e.apellido_emp as empleado, em.numestmov , em.desestmov ,
                    case 
	                    when p.codpedcompra is null then 'NO HAY PEDIDO'
	                    else 'Fecha Ped: ' || pc.fechapedcompra || '  Nro Ped.: ' || pc.numpedcompra 
                    end as datopedido
                    from purchase.presupuestocompra p 
                    inner join referential.proveedor prv on p.codproveedor = prv.codproveedor 
                    inner join referential.empleado e on p.codempleado = e.codempleado 
                    inner join referential.moneda m on p.codmoneda = m.codmoneda 
                    inner join referential.tipocomprobante t on p.codtipocomprobante = t.codtipocomprobante 
                    inner join referential.sucursal s on p.codsucursal = s.codsucursal 
                    inner join referential.estadomovimiento em on p.codestmov = em.codestmov 
                    left join purchase.pedidocompra pc on p.codpedcompra = pc.codpedcompra 
                    order by p.fechapresupuesto, p.codpresupuestocompra desc
                    limit {pageSize} offset {offset}";
        }

        public string Select(int option)
        {
            string sentence = "";

            if (option == 1)
            {
                sentence = @"select p.codpresupuestocompra , t.numtipocomprobante || '-' || p.numpresupuestocompra as numpresupuestocompra, p.fechapresupuesto, pv.nrodocprv || '- ' || pv.razonsocial as datoproveedor ,
                        case when p.codpedcompra is null then 'NO HAY PEDIDO' else 'Fecha Pedido: ' || pc.fechapedcompra   || '  Nro Pedido: ' || pc.numpedcompra end as datopedido, p.totalpresupuestocompra,
                        case when p.condiciopago  = 0 then 'CONTADO' else 'CREDITO' end as condicion, m.nummoneda as moneda, p.cotizacion 
                        from purchase.presupuestocompra p 
                        inner join referential.proveedor pv on p.codproveedor  = p.codproveedor 
                        inner join referential.tipocomprobante t  on p.codtipocomprobante  = t.codtipocomprobante 
                        inner join referential.moneda m on p.codmoneda = m.codmoneda 
                        left join purchase.pedidocompra pc on p.codpedcompra = pc.codpedcompra where p.codpresupuestocompra = @codpresupuestocompra;";
            }
            else if (option == 2)
            {
                sentence = @"select pc.codpresupuestocompra, pc.fechapresupuesto, pc.codproveedor ,
                            t.numtipocomprobante || '- ' || pc.numpresupuestocompra as datopresupuesto, pc.totalpresupuestocompra 
                            from purchase.presupuestocompra pc 
                            inner join referential.tipocomprobante t on pc.codtipocomprobante = t.codtipocomprobante 
                            where pc.codestmov = 1 and pc.codproveedor = @codproveedor order by 1, pc.fechapresupuesto asc; ";
            }
            else if (option == 3) {
                sentence = @"
                    SELECT codestmov 
                    FROM purchase.presupuestocompra
                    WHERE codpresupuestocompra  = @codpresupuestocompra;";
            }
                return sentence;
        }

        public string SelectDetails(int option)
        {
            string sentence = "";
            if (option == 1)
            {
                sentence = @"select p.codigobarra || ' ' || p.desproducto as datoproducto, pcd.cantidad , pcd.preciobruto , pcd.precioneto , (pcd.preciobruto * pcd.cantidad ) as totallinea, 
                        t.numiva || ' %' as datoiva from purchase.presupuestocompradet pcd
                        inner join referential.producto p on pcd.codproducto = p.codproducto 
                        inner join referential.tipoiva t on pcd.codiva = t.codiva where pcd.codpresupuestocompra = @codpresupuestocompra;";

            }
            else if (option == 2) {
                sentence = @"select pcd.codpresupuestocompra, pcd.codproducto, pr.codigobarra || ' - ' || pr.desproducto as datoproducto, pcd.codiva , t.desiva as datoiva, 
                        pcd.cantidad, pcd.preciobruto, (pcd.cantidad  * pcd.preciobruto) as totallinea 
                        from purchase.presupuestocompradet pcd
                        inner join referential.producto pr on pcd.codproducto = pr.codproducto 
                        inner join referential.tipoiva t on pcd.codiva = t.codiva where pcd.codpresupuestocompra = @codpresupuestocompra";
            }
            ;
            return sentence;
        }

        public string Insert()
        {
            return @"
                     SELECT purchase.fn_insert_presupuestocompra(
                        @codtipocomprobante,
                        @codterminal,
                        @ultimo,
                        @numpresupuestocompra,
                        @fechapresupuesto,
                        @codestmov,
                        @codempleado,
                        @codproveedor,
                        @codmoneda,
                        @codsucursal,
                        @totaliva,
                        @totaldescuento,
                        @totalexento,
                        @totalgravada,
                        @totalpresupuestocompra,
                        @cotizacion,
                        @observacion,
                        @contactoprv,
                        @condiciopago,
                        @detalles
                    );";
        }

        public string Update(int option)
        {
            string sentece = "";
            if (option == 1)
            {
                sentece = @"update purchase.presupuestocompra pc set codestmov = @codestado
                             where pc.codpresupuestocompra = @codpresupuestocompra ";
            }
            else if (option == 2)
            {
                sentece = @"select purchase.fn_update_presupuestocompraestado(@codpresupuestocompra, @codestado)";
            }
            return sentece;
        }

    }

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

    public class NotaCreditoCompra_Sql
    {
        public string SelectList(int pageSize, int offset)
        {
            return $@"select nc.codnotacredito, nc.codcompra, nc.fechanotacredito , 
                    tnc.numtipocomprobante || '- ' || nc.numnotacredito as nronotacredito,
                    tc.numtipocomprobante || '- ' || c.numcompra as datocompra, s.dessucursal ,
                    prv.nrodocprv || '- ' || prv.razonsocial as datoproveedor, nc.totaldevolucion , m.nummoneda 
                    from shared.notacredito nc 
                    inner join purchase.compras c on nc.codcompra = c.codcompra 
                    inner join referential.tipocomprobante tnc on nc.codtipocomprobante = tnc.codtipocomprobante 
                    inner join referential.tipocomprobante tc on tc.codtipocomprobante = c.codtipocomprobante 
                    inner join referential.moneda m on nc.codmoneda = m.codmoneda 
                    inner join referential.proveedor prv on nc.codproveedor  = prv.codproveedor 
                    inner join referential.sucursal s on nc.codsucursal = s.codsucursal 
                    where nc.movimiento = 'COMPRAS'
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
                                'Fecha Compra: ' || c.fechacompra || ' Nro. Fac: ' || tcc.numtipocomprobante || '- ' || c.numcompra  as datocompra
                                from shared.notacredito nc 
                                inner join purchase.compras c on nc.codcompra = c.codcompra
                                inner join referential.tipocomprobante tc on nc.codtipocomprobante = tc.codtipocomprobante
                                inner join referential.tipocomprobante tcc on c.codtipocomprobante = tcc.codtipocomprobante
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
    }

    public class Ajustes_Sql
    {
         public string SelectList(int pageSize, int offset)
         {
            return $@"select a.codajuste , a.fechaajuste , tc.numtipocomprobante  || '- ' || a.numajuste as datoajuste,
                    s.dessucursal , case when a.condicion = 0 then 'SUMA por ' || ma.desmotivo else 'RESTA por ' || ma.desmotivo end as datomotivo ,
                    e.nombre_emp || ', ' || e.apellido_emp as datoempleado
                    from shared.ajustes a 
                    inner join referential.sucursal s on a.codsucursal = s.codsucursal 
                    inner join referential.empleado e on a.codempleado = e.codempleado 
                    inner join referential.tipocomprobante tc on a.codtipocomprobante  = tc.codtipocomprobante 
                    inner join referential.motivoajuste ma on a.codmotivo = ma.codmotivo
                    order by a.codajuste , a.fechaajuste desc
                    limit {pageSize} offset {offset}; ";
         }

        public string Insert()
        {
            return @"
                    SELECT shared.fn_insert_ajustes(
                        @codtipocomprobante,
                        @codsucursal,
                        @numajuste,
                        @fechaajuste,
                        @codmotivo,
                        @codempleado,
                        @condicion,
                        @codterminal,
                        @ultimo,
                        @detalles
                    );";
        }

    }


}




/*
Compras

Nota Remision
Libro Compras

Ventas

Arqueo de Caja
Recaudacion a Depositar
Cobranzas
Libro Ventas

Servicios


 
 */