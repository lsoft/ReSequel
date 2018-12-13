using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Sql.Identifier
{
    public interface IColumnName
    {
        string ColumnName
        {
            get;
        }

        bool IsSame(
            string otherColumnName
            );

    }

}
