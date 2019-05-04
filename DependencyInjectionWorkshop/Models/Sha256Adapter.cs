using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public interface IHashedAdapter
    {
        string GetHashedActualPassword(string actualPassword);
    }

    public class Sha256Adapter : IHashedAdapter
    {
        public string GetHashedActualPassword(string actualPassword)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(actualPassword));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            actualPassword = hash.ToString();
            return actualPassword;
        }
    }
}