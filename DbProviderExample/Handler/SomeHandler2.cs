using LSoft.Server;

using DBP = LSoft.Server.DBProvider;

namespace Handler
{
    public class SomeHandler2
    {
        public void Execute()
        {
            const string q1 = "select 1 a";
            const string q2 = "select 1 b";

            var dbp = new DBP();

            dbp.PrepareQuery(q1);
            dbp.PrepareQuery(q2);
        }

    }

}
