namespace Api.POS.Persistence.SQL.Referencial
{
    public class TerminalSQL
    {
        private string query = "";


        public string SelectTerminalComprobante()
        {
            query = @"select 
                    ct.codterminal ,
                    ct.iniciovalidez , ct.finvalidez, ct.inicio , ct.fin ,ct.actual , ct.nrotimbrado , 
                    tc.numtipocomprobante  || '- ' || tc.destipocomprobante as datocomprobante
                    from referential.comprobanteterminal ct 
                    inner join referential.tipocomprobante tc on ct.codtipocomprobante = tc.codtipocomprobante
                    where ct.codterminal = @codterminal and ct.codtipocomprobante = @codtipocomprobante ;";
            return query;
        }
    }
}
