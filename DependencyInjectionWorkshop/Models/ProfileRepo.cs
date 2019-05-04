using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfileRepo
    {
        string GetUserPassword(string accountId);
    }

    public class ProfileRepo : IProfileRepo
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