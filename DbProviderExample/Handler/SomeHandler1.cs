using LSoft.Server;
using System;
using System.Data.SqlClient;

namespace Handler
{
    public class SomeHandler1
    {
        public void Execute() 
        {
            var fdbp = new FakeDbProvider();
            fdbp.SqlText = "select 10 a union all select 20";


            var cmd1 = new SqlCommand();
            cmd1.CommandText = @"select 10 a 
union all 
select 20";

            var cmd2 = new SqlCommand();
            cmd2.CommandText = @"create table #t (id int)";

            var cmd3 = new SqlCommand();
            cmd3.CommandText = @"declare @t table (id int)";

            const string a = "print 1";
            string b = "print 2";

            var dbp = new DBProvider();
/*
* 
//*/
            dbp.PrepareQuery("print 1", "print 2");

            dbp.SqlText = "select 10 a union all select 20";
            dbp.SqlText = "select * from dbo.Station";
            dbp.SqlText = a;
            dbp.SqlText = b;
            dbp.SqlText = a + b;

            dbp.ExecuteNonQuery(a);
            dbp.UnrelatedMethod("some unrelated string");
            dbp.ExecuteNonQuery(@"select 1 a union all select 2 a");
            dbp.UnrelatedMethod("some unrelated string");
            dbp.ExecuteNonQuery(@"select 111 a " + "union all " + "select 222 a");
        }

    }
}
