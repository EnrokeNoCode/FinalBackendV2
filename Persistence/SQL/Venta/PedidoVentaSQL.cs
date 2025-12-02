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

}