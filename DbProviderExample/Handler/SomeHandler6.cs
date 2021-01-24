using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSoft.Server;

namespace Hanlder
{
    public class SomeHandler6
    {
        public const string PartCommon = "select 0;";
        public const string Part1 = "select 1;";
        public const string Part2 = "select 2;";
        public const string Part3 = "select 3;";

        public const string Query1 = PartCommon + Part1;
        public const string Query2 = PartCommon + Part2;

        public void CheckExtension()
        { 
            var dbp = new DBProvider();
            dbp.ExecuteNonQuery(Query1);
            dbp.ExecuteNonQuery(Query2);

            const string Query3 = PartCommon + Part3;
            dbp.ExecuteNonQuery(Query3);
        }
    }
}
