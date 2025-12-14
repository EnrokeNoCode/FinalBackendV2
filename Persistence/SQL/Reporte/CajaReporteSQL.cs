namespace Persistence.SQL.Reporte
{
    public class CajaReporteSQL
    {
        private string query;

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
