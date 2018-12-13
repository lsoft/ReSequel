using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Fixture;

namespace Tests.Fixture.Validation
{
    [TestClass]
    public class RedSqlFixture : BaseFixture
    {
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
            var sqlBody = @"
create table    	    kppk 	 . 	 dbo 	 . 	 ##temp1  (id0 int, id1 int)
create table    	    kppk 	 . 	 dbo 	 . 	 #temp2  (id0 int, id1 int)
create table    	    kppk 	 . 	 dbo 	 . 	 ##temp3  (id0 int, id1 int)
create table    	    kppk 	 . 	 dbo 	 . 	 #temp4  (id0 int, id1 int)
create table    	    dbo 	 . 	 ##temp5  (id0 int, id1 int)
create table    	    dbo 	 . 	 #temp6  (id0 int, id1 int)
create table    	    ##temp7  (id0 int, id1 int)
create table    	    #temp8  (id0 int, id1 int)

insert into     	    kppk 	 . 	 dbo 	 . 	 ##temp1 (id0 , id1) values (1, 1)
insert into     	    kppk 	 . 	 dbo 	 . 	 #temp2 (id0 , id1) values (1, 1)
insert into     	    kppk 	 . 	 dbo 	 . 	 ##temp3 (id0 , id1) values (1, 1)
insert into     	    kppk 	 . 	 dbo 	 . 	 #temp4 (id0 , id1) values (1, 1)
insert into     	    dbo 	 . 	 ##temp5 (id0 , id1) values (1, 1)
insert into     	    dbo 	 . 	 #temp6 (id0 , id1) values (1, 1)
insert into     	    ##temp7 (id0 , id1) values (1, 1)
insert into     	    #temp8 (id0 , id1) values (1, 1)
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }
    }

}
