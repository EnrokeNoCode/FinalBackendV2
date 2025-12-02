namespace Persistence.SQL.Venta
{
    public class CobroContado_sql
    {
        public string InsertCobros()
        {
            return @"select sales.fn_insert_cobroscontado(@codgestion, @detallesVentas, @detallesFormaPago)";
        }
    }
}