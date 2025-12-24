namespace Persistence.SQL.Reporte.Compras
{
    public class CompraReporteSQL
    {
        private string query = "";

        public string SelectCompraListado(bool opcionproveedor = false, bool fecha = false, bool sucursal = false)
        {
            query = @"select c.codcompra, TO_CHAR(c.fechacompra, 'DD/MM/YYYY') as fechacompra , tc.numtipocomprobante, c.numcompra , prv.nrodocprv , prv.razonsocial,
                em.desestmov , m.nummoneda , c.cotizacion,
                case 
                    when c.condicionpago = 0 then 'CONTADO'
                    when c.condicionpago = 1 then 'CREDITO'
                end as condicion, c.totaliva , c.totalexento , c.totalcompra 
                from purchase.compras c 
                inner join referential.proveedor prv on c.codproveedor = prv.codproveedor 
                inner join referential.tipocomprobante tc on c.codtipocomprobante = tc.codtipocomprobante 
                inner join referential.estadomovimiento em on c.codestmov = em.codestmov
                inner join referential.moneda m on c.codmoneda = m.codmoneda
                where 1 = 1";

            if (opcionproveedor)
                query += " and prv.nrodocprv = @nrodocprv";

            if (fecha)
                query += " and c.fechacompra between @fechainicio and @fechafin";

            if (sucursal)
                query += " and c.codsucursal = @codsucursal";

            query += ";";
            return query;
        }


        public string SelectCompraDetListado()
        {
            query = @"select p.codigobarra, p.desproducto, ti.desiva , cd.cantidad, cd.preciobruto, cd.precioneto, (cd.cantidad * cd.preciobruto) as totallinea 
                        from purchase.comprasdet cd
                        inner join referential.producto p on cd.codproducto = p.codproducto 
                        inner join referential.tipoiva ti on cd.codiva = ti.codiva where cd.codcompra = @codcompra;";
            return query;
        }


    }
}
