using LSoft.Server;

namespace Handler
{
    public class SomeHandler4
    {
        public void Execute()
        {
            var dbp = new FakeDbProvider();

            dbp.PrepareQuery("select 1 a \r\nunion all \r\nselect 2 a");
        }

    }


}
