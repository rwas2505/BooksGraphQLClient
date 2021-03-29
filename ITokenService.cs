using System.Threading.Tasks;

public interface ITokenService
{
  Task<string> GetToken();
}