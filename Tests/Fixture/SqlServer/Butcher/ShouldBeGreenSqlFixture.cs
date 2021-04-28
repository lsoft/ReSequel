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
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);

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
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);

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
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);

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
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);

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
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);

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
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);

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
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);

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
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);

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
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);

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
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("information_schema.tables"));

            Assert.AreEqual(1, carveResult.ColumnList.Count);
            Assert.IsTrue(carveResult.IsStarReferenced);
        }

        [TestMethod]
        public void SelectWithVariableReferencesStatement()
        {
            const string sqlBody = @"
select
    @a
from information_schema.tables 
where
	TABLE_CATALOG = @b
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(2, carveResult.VariableReferenceList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("information_schema.tables"));
            Assert.IsTrue(carveResult.IsVariableReferenced("@a"));
            Assert.IsTrue(carveResult.IsVariableReferenced("@b"));


            Assert.AreEqual(1, carveResult.ColumnList.Count);
            Assert.IsFalse(carveResult.IsStarReferenced);
        }

        [TestMethod]
        public void SelectWithVariableAndSetReferencesStatement()
        {
            const string sqlBody = @"
select
    @a = @b
from information_schema.tables 
where
	TABLE_CATALOG = @c
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(3, carveResult.VariableReferenceList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("information_schema.tables"));
            Assert.IsTrue(carveResult.IsVariableReferenced("@a"));
            Assert.IsTrue(carveResult.IsVariableReferenced("@b"));
            Assert.IsTrue(carveResult.IsVariableReferenced("@c"));


            Assert.AreEqual(1, carveResult.ColumnList.Count);
            Assert.IsFalse(carveResult.IsStarReferenced);
        }

        [TestMethod]
        public void DeclareVariableStatement()
        {
            const string sqlBody = @"
declare @a int, @b varchar(10);
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(0, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(0, carveResult.ColumnList.Count);
            Assert.AreEqual(2, carveResult.VariableReferenceList.Count);

            Assert.IsTrue(carveResult.IsVariableReferenced("@a"));
            Assert.IsTrue(carveResult.IsVariableReferenced("@b"));
        }

        [TestMethod]
        public void SetVariableStatement()
        {
            const string sqlBody = @"
set @a = 1
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(0, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(0, carveResult.ColumnList.Count);
            Assert.AreEqual(1, carveResult.VariableReferenceList.Count);

            Assert.IsTrue(carveResult.IsVariableReferenced("@a"));
        }

        [TestMethod]
        public void DropIndexStatement1()
        {
            const string sqlBody = @"
DROP INDEX [TestTable0_Index0] ON dbo.TestTable0
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(0, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(1, carveResult.IndexList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("dbo.TestTable0"));
            Assert.IsTrue(carveResult.IsIndexReferenced("dbo.TestTable0", "TestTable0_Index0"));
        }

        [TestMethod]
        public void DropIndexStatement2()
        {
            const string sqlBody = @"
DROP INDEX [TestTable0_Index0] ON #TestTable0
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(1, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(0, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(1, carveResult.IndexList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("#TestTable0"));
            Assert.IsTrue(carveResult.IsIndexReferenced("#TestTable0", "TestTable0_Index0"));
        }

        [TestMethod]
        public void CreateIndexStatement1()
        {
            const string sqlBody = @"
CREATE NONCLUSTERED INDEX [TestTable1_Index0] ON TestTable1
(
	[name] ASC
)
INCLUDE([additional])
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(2, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(1, carveResult.IndexList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("TestTable1"));
            Assert.IsTrue(carveResult.IsColumnReferenced("name"));
            Assert.IsTrue(carveResult.IsColumnReferenced("additional"));
            Assert.IsTrue(carveResult.IsIndexReferenced("TestTable1", "TestTable1_Index0"));
        }


        [TestMethod]
        public void AlterIndexStatement()
        {
            const string sqlBody = @"
ALTER INDEX [TestTable0_Index0] ON [dbo].[TestTable0] DISABLE
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(0, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(1, carveResult.IndexList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("[dbo].[TestTable0]"));
            Assert.IsTrue(carveResult.IsIndexReferenced("[dbo].[TestTable0]", "TestTable0_Index0"));
        }

        [TestMethod]
        public void TruncateTableStatement()
        {
            const string sqlBody = @"
truncate table [dbo].[TestTable0]
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(0, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(0, carveResult.IndexList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("[dbo].[TestTable0]"));
        }

        [TestMethod]
        public void SelectWithIndexStatement1()
        {
            const string sqlBody = @"
select
	t0.id
from dbo.TestTable0 t0 with(index=TestTable0_Index0)
join dbo.TestTable1 t1 with(index=TestTable1_Index0) on t0.id = t1.id
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(2, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(3, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(2, carveResult.IndexList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("[dbo].[TestTable0]"));
            Assert.IsTrue(carveResult.IsTableReferenced("[dbo].[TestTable1]"));
            Assert.IsTrue(carveResult.IsColumnReferenced("id"));
            Assert.IsTrue(carveResult.IsIndexReferenced("[dbo].[TestTable0]", "TestTable0_Index0"));
            Assert.IsTrue(carveResult.IsIndexReferenced("[dbo].[TestTable1]", "TestTable1_Index0"));
        }

        [TestMethod]
        public void SelectWithIndexStatement2()
        {
            const string sqlBody = @"
select
    t0.id
from dbo.TestTable0 t0 with(index=TestTable0_Index0), dbo.TestTable1 t1 with(index=TestTable1_Index0)
where
	t0.id = t1.id
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(2, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(3, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(2, carveResult.IndexList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("[dbo].[TestTable0]"));
            Assert.IsTrue(carveResult.IsTableReferenced("[dbo].[TestTable1]"));
            Assert.IsTrue(carveResult.IsColumnReferenced("id"));
            Assert.IsTrue(carveResult.IsIndexReferenced("[dbo].[TestTable0]", "TestTable0_Index0"));
            Assert.IsTrue(carveResult.IsIndexReferenced("[dbo].[TestTable1]", "TestTable1_Index0"));
        }

        [TestMethod]
        public void SelectFromTableVariableStatement()
        {
            const string sqlBody = @"
select id, [name] from @t
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(1, carveResult.TableVariableList.Count);
            Assert.AreEqual(2, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(0, carveResult.IndexList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("@t"));
            Assert.IsTrue(carveResult.IsColumnReferenced("id"));
            Assert.IsTrue(carveResult.IsColumnReferenced("name"));
        }

        [TestMethod]
        public void InsertWithFunction1()
        {
            const string sqlBody = @"
insert into [TestTable2] (id, name)
values ( 1, dbo.get_name() )
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(2, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(0, carveResult.IndexList.Count);
            Assert.AreEqual(1, carveResult.FunctionList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("TestTable2"));
            Assert.IsTrue(carveResult.IsColumnReferenced("id"));
            Assert.IsTrue(carveResult.IsColumnReferenced("name"));
        }

        [TestMethod]
        public void InsertWithFunction2()
        {
            const string sqlBody = @"
insert into [TestTable2] (id, name)
values ( 1, server_name.database_name.dbo.get_name(old_name) )
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(2, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(0, carveResult.IndexList.Count);
            Assert.AreEqual(1, carveResult.FunctionList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("TestTable2"));
            Assert.IsTrue(carveResult.IsColumnReferenced("id"));
            Assert.IsTrue(carveResult.IsColumnReferenced("name"));
            //we do not parse columns in the function invocation: Assert.IsTrue(carveResult.IsColumnReferenced("old_name"));
        }

        [TestMethod]
        public void SelectFromAlias()
        {
            const string sqlBody = @"
select
    tt2.*
from TestTable2 tt2
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(1, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(0, carveResult.IndexList.Count);
            Assert.AreEqual(0, carveResult.FunctionList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("TestTable2"));
            Assert.IsTrue(carveResult.IsColumnReferenced("*"));
        }


        [TestMethod]
        public void UpdateCte0()
        {
            const string sqlBody = @"
with cte as (
	select id, name from TestTable2
)
update cte
set name = ''
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(2, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(1, carveResult.CteList.Count);
            Assert.AreEqual(3, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(0, carveResult.IndexList.Count);
            Assert.AreEqual(0, carveResult.FunctionList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("TestTable2"));
            Assert.IsTrue(carveResult.IsTableReferenced("cte"));
            Assert.IsTrue(carveResult.IsColumnReferenced("id"));
            Assert.IsTrue(carveResult.IsColumnReferenced("name"));
        }

        [TestMethod]
        public void JoinCte0()
        {
            const string sqlBody = @"
with cte as (
	select id, name from TestTable2
)
select
	*
from TestTable1
join cte ctealias on ctealias.id = TestTable1.id
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(3, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(1, carveResult.CteList.Count);
            Assert.AreEqual(5, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(0, carveResult.IndexList.Count);
            Assert.AreEqual(0, carveResult.FunctionList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("TestTable2"));
            Assert.IsTrue(carveResult.IsTableReferenced("cte"));
            Assert.IsTrue(carveResult.IsStarReferenced);
            Assert.IsTrue(carveResult.IsColumnReferenced("id"));
            Assert.IsTrue(carveResult.IsColumnReferenced("name"));
        }

        [TestMethod]
        public void UpdateFromAlias0()
        {
            const string sqlBody = @"
update tt2
set name = name
from TestTable2 tt2

";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(0, carveResult.CteList.Count);
            Assert.AreEqual(2, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(0, carveResult.IndexList.Count);
            Assert.AreEqual(0, carveResult.FunctionList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("TestTable2"));
            Assert.IsTrue(carveResult.IsColumnReferenced("name"));
        }

        [TestMethod]
        public void SelectFromAlias0()
        {
            const string sqlBody = @"
select
	testtable2.name
from dbo.TestTable2 testtable2
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(0, carveResult.CteList.Count);
            Assert.AreEqual(1, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(0, carveResult.IndexList.Count);
            Assert.AreEqual(0, carveResult.FunctionList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("TestTable2"));
            Assert.IsTrue(carveResult.IsColumnReferenced("name"));
        }

        [TestMethod]
        public void SelectFromAlias1()
        {
            const string sqlBody = @"
select
	testtable1.day,
	testtable3.custom_column
from dbo.TestTable1 testtable1
join dbo.TestTable3 testtable3 on testtable1.id = testtable3.id
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(2, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(0, carveResult.CteList.Count);
            Assert.AreEqual(4, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(0, carveResult.IndexList.Count);
            Assert.AreEqual(0, carveResult.FunctionList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("TestTable1"));
            Assert.IsTrue(carveResult.IsTableReferenced("TestTable3"));
            Assert.IsTrue(carveResult.IsColumnReferenced("id"));
            Assert.IsTrue(carveResult.IsColumnReferenced("day"));
            Assert.IsTrue(carveResult.IsColumnReferenced("custom_column"));
        }

        [TestMethod]
        public void SelectWithDateAdd()
        {
            const string sqlBody = @"
select
	*
from TestTable1
where
    '20210101' > dateadd( YEAR, -1, '20210101' )
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(0, carveResult.CteList.Count);
            Assert.AreEqual(1, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(0, carveResult.IndexList.Count);
            Assert.AreEqual(1, carveResult.FunctionList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("TestTable1"));
            Assert.IsTrue(carveResult.IsStarReferenced);
        }

        [TestMethod]
        public void SelectColumnAlias0()
        {
            const string sqlBody = @"
select
    id,
	id + 1 new_column
from dbo.TestTable1
order by
    new_column
";

            var carveResult = Carve(
                sqlBody
                );

            Assert.IsNotNull(carveResult);

            Assert.AreEqual(1, carveResult.TableList.Count);
            Assert.AreEqual(0, carveResult.TempTableList.Count);
            Assert.AreEqual(0, carveResult.TableVariableList.Count);
            Assert.AreEqual(0, carveResult.CteList.Count);
            Assert.AreEqual(3, carveResult.ColumnList.Count);
            Assert.AreEqual(0, carveResult.VariableReferenceList.Count);
            Assert.AreEqual(0, carveResult.IndexList.Count);
            Assert.AreEqual(0, carveResult.FunctionList.Count);

            Assert.IsTrue(carveResult.IsTableReferenced("TestTable1"));
            Assert.IsTrue(carveResult.IsColumnReferenced("id"));
            Assert.IsTrue(carveResult.IsColumnReferenced("new_column", true));
            Assert.IsFalse(carveResult.IsColumnReferenced("new_column", false));
        }

    }

}
