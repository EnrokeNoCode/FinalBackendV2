namespace Persistence.SQL.Referencial
{
    public class MotivoAjusteSQL
    {
        private string query = "";

        public string SelectMotivoAjuste()
        {
            query = @"select m.codmotivo, m.nummotivo, m.desmotivo from referential.motivoajuste m ;";
            return query;
        }
    }
}
