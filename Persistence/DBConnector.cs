using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Persistence{
    public class DBConnector
    {
        private string ConexionCadena = string.Empty;
        public DBConnector() 
        {
            var builder = new ConfigurationBuilder().SetBasePath
                (Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            ConexionCadena = builder.GetSection("ConnectionStrings:DefaultConnection").Value;
        }
        public string cadenaSQL() 
        {
            return ConexionCadena;
        }
        public NpgsqlConnection GetPostgresConnection()
        {
            return new NpgsqlConnection(ConexionCadena);
        }
    }


}