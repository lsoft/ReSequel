using LSoft.Server;

namespace Handler
{
    public class SomeHandler3
    {
        public void Execute()
        {
            var dbp = new TransactionManager();

            dbp.PrepareQuery("select 1 a \r\nunion all \r\nselect 2 a");
            dbp.PrepareQuery("select 1 a" + "\r\nunion all" + "\r\nselect 2 a");


        }

    }

}
