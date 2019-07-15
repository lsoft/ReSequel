using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Fixture.SqlServer.Validation
{
    [TestClass]
    public class RedSqlFixture : SqlServerFixture
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            SqlServerFixture.ClassInit();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            SqlServerFixture.ClassCleanup();
        }

        [TestMethod]
        public void ReadFromUndeclaredTableVariable()
        {
            var sqlBody = @"
insert into dbo.TestTable0
	(id, [name], additional)
select id, [name], null from @incomingTable
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CreateAndInsertTempTables()
        {
            var sqlBody = string.Format(@"
create table    	    {0}	.	 dbo 	 . 	 ##temp1  (id0 int, id1 int)
create table    	    {0}	.	 dbo 	 . 	 #temp2  (id0 int, id1 int)
create table    	    {0}	.	 dbo 	 . 	 ##temp3  (id0 int, id1 int)
create table    	    {0}	.	 dbo 	 . 	 #temp4  (id0 int, id1 int)
create table    	    dbo	.	 ##temp5  (id0 int, id1 int)
create table    	    dbo	.	 #temp6  (id0 int, id1 int)
create table    	    ##temp7  (id0 int, id1 int)
create table    	    #temp8  (id0 int, id1 int)

insert into     	    {0}	.	 dbo 	 . 	 ##temp1 (id0 , id1) values (1, 1)
insert into     	    {0}	.	 dbo 	 . 	 #temp2 (id0 , id1) values (1, 1)
insert into     	    {0}	.	 dbo 	 . 	 ##temp3 (id0 , id1) values (1, 1)
insert into     	    {0}	.	 dbo 	 . 	 #temp4 (id0 , id1) values (1, 1)
insert into     	    dbo	.	 ##temp5 (id0 , id1) values (1, 1)
insert into     	    dbo	.	 #temp6 (id0 , id1) values (1, 1)
insert into     	    ##temp7 (id0 , id1) values (1, 1)
insert into     	    #temp8 (id0 , id1) values (1, 1)
", TestSettings.Default.DatabaseName);

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }
    }

}
