using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Sql.SqlServer.Validator.Factory
{
    public interface ISqlValidatorFactory
    {
        ISqlValidator Create(
            SqlConnection connection
            );
    }

}
