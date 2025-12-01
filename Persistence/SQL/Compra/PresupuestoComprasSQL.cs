namespace Persistence.SQL.Compra
{
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
                    where p.codsucursal = @codsucursal
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
}