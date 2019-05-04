using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileRepo
    {
        public string GetUserPassword(string accountId)
        {
            string expectPassword;
            using (var connection = new SqlConnection("my connection string"))
            {
                expectPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return expectPassword;
        }
    }
}