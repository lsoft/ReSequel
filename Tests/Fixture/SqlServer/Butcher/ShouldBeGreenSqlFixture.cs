using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Fixture.SqlServer.Butcher
{

    [TestClass]
    public class ShouldBeGreenSqlFixture : SqlServerFixture
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
        public void SelectFromInformationSchema()
        {
            const string TableName = "tables";
            const string SchemaName = "information_schema";
            var databaseName = TestSettings.Default.DatabaseName;

            var table0 = string.Format(
                "[{0}]",
                TableName
                );
            var table1 = string.Format(
                "{0}",
                TableName
                );

            var schemaPlusTable0 = string.Format(
                "[{0}].[{1}]",
                SchemaName,
                TableName
                );
            var schemaPlusTable1 = string.Format(
                "{0}.{1}",
                SchemaName,
                TableName
                );

            var fullTableIdenfitier0 = string.Format(
                "[{0}].[{1}].[{2}]",
                databaseName,
                SchemaName,
                TableName
                );
            var fullTableIdenfitier1 = string.Format(
                "{0}.{1}.{2}",
                databaseName,
                SchemaName,
                TableName
                );

            var sqlBody = string.Format(
                "select * from {0}",
                fullTableIdenfitier0
                );

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced(table0));
            Assert.IsTrue(carveResult.IsTableReferenced(table1));

            Assert.IsTrue(carveResult.IsTableReferenced(schemaPlusTable0));
            Assert.IsTrue(carveResult.IsTableReferenced(schemaPlusTable1));

            Assert.IsTrue(carveResult.IsTableReferenced(fullTableIdenfitier0));
            Assert.IsTrue(carveResult.IsTableReferenced(fullTableIdenfitier1));

            Assert.AreEqual(1, carveResult.ColumnList.Count);
            Assert.IsTrue(carveResult.IsStarReferenced);
        }

        [TestMethod]
        public void WhileStatement()
        {
            const string sqlBody = @"
while(exists (select * from information_schema.tables where table_name = 'testtable0'))
	print 1;
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.IsTrue(carveResult.IsTableReferenced("information_schema.tables"));

            Assert.AreEqual(2, carveResult.ColumnList.Count);
            Assert.IsTrue(carveResult.IsColumnReferenced("table_name"));
            Assert.IsTrue(carveResult.IsStarReferenced);

        }

        [TestMethod]
        public void IfElseStatement()
        {
            const string sqlBody = @"
if(exists (select * from information_schema.tables where table_name = 'testtable0'))
begin
	select name from dbo.TestTable1
end
else
begin
	select [additional] from dbo.TestTable2
end
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(3, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.IsTrue(carveResult.IsTableReferenced("information_schema.tables"));
            Assert.IsTrue(carveResult.IsTableReferenced("dbo.TestTable1"));
            Assert.IsTrue(carveResult.IsTableReferenced("dbo.TestTable2"));

            Assert.AreEqual(4, carveResult.ColumnList.Count);
            Assert.IsTrue(carveResult.IsColumnReferenced("table_name"));
            Assert.IsTrue(carveResult.IsColumnReferenced("[NAME]"));
            Assert.IsTrue(carveResult.IsColumnReferenced("additional"));
            Assert.IsTrue(carveResult.IsStarReferenced);
        }

        [TestMethod]
        public void DeclareTableVariableStatement()
        {
            const string sqlBody = @"
declare @t table (id1 int, id2 int)
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(1, carveResult.TableVariableList.Count);
            Assert.IsTrue(carveResult.IsTableReferenced("@t"));

            Assert.AreEqual(2, carveResult.ColumnList.Count);
            Assert.IsTrue(carveResult.IsColumnReferenced("id1"));
            Assert.IsTrue(carveResult.IsColumnReferenced("[ID2]"));
        }

        [TestMethod]
        public void CreateTempTableStatement()
        {
            const string sqlBody = @"
create table #TempTable (id1 int, id2 int, id3 int)
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(1, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.IsTrue(carveResult.IsTableReferenced("#TempTable"));

            Assert.AreEqual(3, carveResult.ColumnList.Count);
            Assert.IsTrue(carveResult.IsColumnReferenced("id1"));
            Assert.IsTrue(carveResult.IsColumnReferenced("[ID2]"));
            Assert.IsTrue(carveResult.IsColumnReferenced("[id3]"));
        }

        [TestMethod]
        public void CreateTableStatement()
        {
            const string sqlBody = @"
create table RealTable (id1 int, id2 int, id3 int, [id4] int)
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.IsTrue(carveResult.IsTableReferenced("RealTable"));

            Assert.AreEqual(4, carveResult.ColumnList.Count);
            Assert.IsTrue(carveResult.IsColumnReferenced("id1"));
            Assert.IsTrue(carveResult.IsColumnReferenced("[ID2]"));
            Assert.IsTrue(carveResult.IsColumnReferenced("[id3]"));
            Assert.IsTrue(carveResult.IsColumnReferenced("id4"));
        }

        [TestMethod]
        public void DropTableStatement()
        {
            const string sqlBody = @"
drop table TestTable0, TestTable1
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(2, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.IsTrue(carveResult.IsTableReferenced("dbo.TestTable0"));
            Assert.IsTrue(carveResult.IsTableReferenced("TestTable1"));

            Assert.AreEqual(0, carveResult.ColumnList.Count);
        }

        [TestMethod]
        public void AlterTableAddColumnStatement()
        {
            const string sqlBody = @"
alter table TestTable0 add newId bigint null
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.IsTrue(carveResult.IsTableReferenced("dbo.TestTable0"));

            Assert.AreEqual(1, carveResult.ColumnList.Count);
            Assert.IsTrue(carveResult.IsColumnReferenced("newId"));
        }

        [TestMethod]
        public void AlterTableAlterColumnStatement()
        {
            const string sqlBody = @"
alter table TestTable0 alter column id bigint null
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.IsTrue(carveResult.IsTableReferenced("dbo.TestTable0"));

            Assert.AreEqual(1, carveResult.ColumnList.Count);
            Assert.IsTrue(carveResult.IsColumnReferenced("id"));
        }

        [TestMethod]
        public void SelectStarStatement()
        {
            const string sqlBody = @"
select
    *
from information_schema.tables 
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.IsTrue(carveResult.IsTableReferenced("information_schema.tables"));

            Assert.AreEqual(1, carveResult.ColumnList.Count);
            Assert.IsTrue(carveResult.IsStarReferenced);
        }

    }
}
