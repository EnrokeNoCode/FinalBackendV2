namespace Persistence.SQL.Reporte.Ventas
{
    public class VentaReporteSQL
    {
        private string query = "";

        public string SelectVentaListado(bool opcioncliente = false, bool fecha = false, bool sucursal = false)
        {
            query = @"select v.codventa,  TO_CHAR(v.fechaventa, 'DD/MM/YYYY') as fechaventa , tc.numtipocomprobante, v.numventa, c.nrodoc, c.nombre || ', ' || c.apellido as cliente,
                        em.desestmov , m.nummoneda , v.cotizacion,
                        case 
	                        when v.condicionpago = 0 then 'CONTADO'
	                        when v.condicionpago = 1 then 'CREDITO'
                        end as condicion, v.totaliva , v.totalexento , v.totalventa 
                        from sales.ventas v 
                        inner join referential.cliente c on v.codcliente = c.codcliente 
                        inner join referential.tipocomprobante tc on v.codtipocomprobante = tc.codtipocomprobante 
                        inner join referential.estadomovimiento em on v.codestmov = em.codestmov
                        inner join referential.moneda m on v.codmoneda = m.codmoneda where 1 = 1
                        ";

            if (opcioncliente)
                query += " and c.nrodoc = @nrodoc";

            if (fecha)
                query += " and v.fechaventa between @fechainicio and @fechafin";

            if (sucursal)
                query += " and v.codsucursal = @codsucursal";

            query += ";";
            return query;
        }


        public string SelectVentaDetListado()
        {
            query = @"select p.codigobarra, p.desproducto, ti.desiva , vd.cantidad, vd.preciobruto, vd.precioneto, (vd.cantidad * vd.preciobruto) as totallinea 
                        from sales.ventasdet vd
                        inner join referential.producto p on vd.codproducto = p.codproducto 
                        inner join referential.tipoiva ti on vd.codiva = ti.codiva where vd.codventa = @codventa ;";
            return query;
        }


    }
}
