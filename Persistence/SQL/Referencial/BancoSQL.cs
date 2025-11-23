namespace Persistence.SQL.Referencial
{
    public class BancoSQL
    {
        private string query = "";

        public string Select()
        {
            query = @"select b.codbanco, b.numbanco , b.desbanco  from referential.banco b ;";
            return query;
        }
    }
}
