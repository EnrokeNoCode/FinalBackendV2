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
                    where p.codsucursal = @codsucursal
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


}