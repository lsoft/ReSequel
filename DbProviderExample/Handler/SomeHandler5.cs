using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSoft.Server;

namespace Hanlder
{
    public class SomeHandler5
    {
        public void CheckExtension()
        {
            var dbp = new DBProvider();

            dbp.ExecuteExtension(
                1,
                "select 1 union select 2",
                2);
        }
    }
}
