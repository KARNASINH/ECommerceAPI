using Microsoft.Data.SqlClient;
namespace ECommerceAPI.Data
{
    public class SqlConnectionFactory
    {
        private readonly IConfiguration _configuration;

        //Injecting IConfuguration object via dependency injecrtion.
        public SqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        //This will fetch the connection string from the Appsettings.json file.
        public SqlConnection CreateConnection()
        => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    }
}
