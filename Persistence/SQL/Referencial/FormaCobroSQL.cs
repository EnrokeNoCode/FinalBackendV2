namespace Persistence.SQL
{
    public class FormaCobroSQL
    {
        private string query = "";

        public string Select(int option)
        {
            switch (option)
            {
                case 1:
                    query = "select codformacobro, numformacobro, desformacobro, tipo from referential.formacobro ;";
                    break;
            }

            return query;
        }

    }
}
