namespace Persistence.SQL.Referencial
{
    public class TipoComprobanteSQL
    {
        private string query = "";

        public string SelectTipoComprobanteMov()
        {
            query = @"select t.codtipocomprobante, t.numtipocomprobante, t.destipocomprobante from referential.tipocomprobante t 
                where t.movimiento = @tipomov and t.activomov = @marca ;";
            return query;
        }
    }
}
