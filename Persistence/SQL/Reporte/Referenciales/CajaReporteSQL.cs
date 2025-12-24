namespace Persistence.SQL.Reporte.Referenciales
{
    public class CajaReporteSQL
    {
        private string query;

        public string CajaInfoCobros()
        {
            query = @"select s.dessucursal as sucursal, c.numcaja || '- ' || c.descaja as caja, 
                        cbr.numcobrador || '- ' || emp.nombre_emp as cobrador from referential.cajagestion cg
                        inner join referential.caja c on cg.codcaja = c.codcaja 
                        inner join referential.cobrador cbr on cg.codcobrador = cbr.codcobrador 
                        inner join referential.empleado emp on cbr.codempleado = emp.codempleado 
                        inner join referential.sucursal s on c.codsucursal = s.codsucursal 
                        where cg.codgestion = @codgestion;";
            return query;
        }
        public string CajaReporteCobros()
        {
            query = @"select fc.desformacobro as formacobro,
                        'EFECTIVO' as datoformacobro,
                        cfi.montoefectivo as montocobro  from sales.cajaingreso ci
                    inner join sales.cajafacturaventacobradas cfvc on ci.codingreso = cfvc.codingreso 
                    inner join sales.cajaefectivoingreso cfi on ci.codingreso = cfi.codingreso 
                    inner join referential.formacobro fc on ci.codformacobro = fc.codformacobro 
                    inner join referential.moneda m on cfi.codmoneda = m.codmoneda 
                    where fc.tipo = 'EFE' and ci.codgestion = @codgestion
                    union all 
                    select fc.desformacobro as formacobro,
                        b.desbanco || ' - ' || ccr.numcheque || ' - ' || ccr.librador || ' - ' || ccr.fechavto as datoformacobro,
                        ccr.montocheque as montocobro  from sales.cajaingreso ci
                    inner join sales.cajafacturaventacobradas cfvc on ci.codingreso = cfvc.codingreso 
                    inner join sales.cajachequerecibido ccr on ci.codingreso = ccr.codingreso 
                    inner join referential.banco b on ccr.codbanco = b.codbanco
                    inner join referential.formacobro fc on ci.codformacobro = fc.codformacobro 
                    inner join referential.moneda m on ccr.codmoneda = m.codmoneda 
                    where fc.tipo = 'CHE' and ci.codgestion = @codgestion
                    union all
                    select fc.desformacobro as formacobro,
                        tt.destipotar || ' - ' || cti.numvaucher as datoformacobro,
                        cti.montotarjeta as monto
                    from sales.cajaingreso ci
                    inner join sales.cajafacturaventacobradas cfvc on ci.codingreso = cfvc.codingreso 
                    inner join sales.cajatarjetasingreso cti on ci.codingreso = cti.codingreso 
                    inner join referential.tipotarjeta tt on cti.codtipotar = tt.codtipotar
                    inner join referential.formacobro fc on ci.codformacobro = fc.codformacobro 
                    inner join referential.moneda m on cti.codmoneda = m.codmoneda 
                    where fc.tipo = 'TAR' and ci.codgestion = @codgestion;";
            return query;
        }


    }
}
