using System.Data;
namespace Persistencia.DapperConexion
{
    public interface IFactoryConexion
    {
         void CloseConnection();
         IDbConnection GetConnection();
    }
}