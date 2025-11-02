namespace Persistence.SQL.Referencial
{
    public class MarcaSQL
    {
        private string query = "";

        public string SelectMarca()
        {
            query = @"select m.codmarca, m.nummarca, m.desmarca from referential.marca m where m.soloservicio = @marca ;";
            return query;
        }
    }
}
