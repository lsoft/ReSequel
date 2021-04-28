using Main.Inclusion.Scanner.Generator;
using Main.Inclusion.Validated.Result;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Fixture.SqlServer.Validation
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
        public void CorrectCursorStatement()
        {
            var sqlBody = @"
declare headCursor cursor for select id from TestTable0;

open headCursor;
declare @currentheadid int;
fetch headCursor into @currentheadid;

while @@FETCH_STATUS = 0
begin

    fetch headCursor into @currentheadid;
end

close headCursor;
deallocate headCursor;
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectSetStatement1()
        {
            var sqlBody = @"
set nocount on
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectSetStatement2()
        {
            var sqlBody = @"
SET IDENTITY_INSERT TestTable0 ON
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectPrintStatement()
        {
            var sqlBody = @"
print 1
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectReturnStatement()
        {
            var sqlBody = @"
return
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectBeginEndStatement()
        {
            var sqlBody = @"
begin
	print 1
end
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectRaiseErrorStatement()
        {
            var sqlBody = @"
RAISERROR (N'Message text', -- Message text.  
           10, -- Severity,  
           1, -- State,  
           N'First argument', -- First argument.  
           5); -- Second argument.  
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectIfStatement1()
        {
            var sqlBody = @"
if(0=1)
begin
	print 1
end
else
begin
	print 2
end
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectIfStatement2()
        {
            var sqlBody = @"
if exists(select * from TestTable0)
begin
	print 1
end
else
begin
	print 2
end
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectSelectStatement()
        {
            var sqlBody = "select * from [information_schema].[tables]";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectInsertStatement()
        {
            var sqlBody = @"
INSERT INTO dbo.TestTable0
    (id, name)
VALUES
    (1, 'some name');
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectInsertWith2SameVariableStatement()
        {
            var sqlBody = @"
INSERT INTO dbo.TestTable0
    ([id], [name], [additional])
VALUES
    (1, @somevar, @somevar);
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectUpdateStatement()
        {
            var sqlBody = @"
update
    dbo.TestTable0
set 
    name = 'some name'
where
    id = 1
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectDeleteStatement()
        {
            var sqlBody = @"
delete from
    dbo.TestTable0
where
    id = 1
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectDropTableStatement()
        {
            var sqlBody = @"
drop table
    dbo.TestTable0
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }


        [TestMethod]
        public void IncorrectDropTableStatement()
        {
            var sqlBody = @"
drop table
    UnknownTable
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }


        [TestMethod]
        public void CorrectCreateTableStatement()
        {
            var sqlBody = @"
CREATE TABLE[dbo].[TestTableX]
(
    [id][int] NOT NULL,
    [name] [varchar] (100) NOT NULL,
    [additional] [varchar] (100) NULL,

    CONSTRAINT[PK_TestTable1] PRIMARY KEY CLUSTERED
    (
       [id] ASC
    ) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

) ON [PRIMARY]
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }


        /// <summary>
        /// Duplicate table!
        /// </summary>
        [TestMethod]
        public void IncorrectCreateTableStatement()
        {
            var sqlBody = @"
CREATE TABLE[dbo].[TestTable1]
(
    [id][int] NOT NULL,
    [name] [varchar] (100) NOT NULL,
    [additional] [varchar] (100) NULL,

    CONSTRAINT[PK_TestTable1] PRIMARY KEY CLUSTERED
    (
       [id] ASC
    ) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

) ON [PRIMARY]
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectSetVariableStatement()
        {
            var sqlBody = @"
declare @t int

set @t = 1
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void ComplexTest0()
        {
            var sqlBody = @"
IF (1=1)
BEGIN
    DECLARE @tid BIGINT
    SET @tid = 1

    INSERT INTO TestTable2 (id, [name])
    VALUES (@tid, '0');

    SELECT @tid AS ID
END
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void ComplexTest1()
        {
            var sqlBody = @"
IF (0 = 1)
BEGIN
	select * from @tid
END
else
begin
    DECLARE @tid table (id BIGINT)

	select * from @tid
end
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void ComplexTest2()
        {
            var sqlBody = @"
IF (1=1)
BEGIN
    DECLARE @tid table (id BIGINT)

	select * from @tid
END
else
begin
	select * from @tid
end
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void ComplexTest3()
        {
            var sqlBody = @"
while (1=1)
BEGIN
    DECLARE @tid BIGINT
    SET @tid = 1

    INSERT INTO TestTable2 (id, [name])
    VALUES (@tid, '0');

    SELECT @tid AS ID
END
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void ComplexTest4()
        {
            var sqlBody = @"
IF( @trainType >= 80 AND @trainType <= 99 )
    print 1
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void ComplexTest5()
        {
            var sqlBody = @"
insert into TestTable0 (id, [name], [additional])
select @id, @name, @additional
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncompleteInsertIdentity()
        {
            var sqlBody = @"
INSERT INTO dbo.TestTable0
VALUES
    ('some name');
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectInsertAndSelectIdentity()
        {
            var sqlBody = @"
INSERT INTO dbo.TestTable0
    (id, name)
VALUES
    (1, 'some name');

SELECT @@IDENTITY as ident;
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void DuplicateVariable()
        {
            var sqlBody = @"
update
    dbo.TestTable0
set
    id = @id, name = @name, additional = @name
where
    id = @id and name = @name

";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void TableVariable1()
        {
            var sqlBody = @"
declare @t table ([id] int, [name] varchar(100))

select id, [name] from @t
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void TableVariable2()
        {
            var sqlBody = @"
declare @t table ([id] int, [name] varchar(100))

insert into @t
select id, [name] from dbo.TestTable0
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void TableVariable3()
        {
            var sqlBody = @"
declare @t table ([id] int, [name] varchar(100))

insert into @t
select id, [name] from dbo.TestTable0

insert into dbo.TestTable0
select id, [name], null from @t
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void TwoVariablesInFunction()
        {
            var sqlBody = @"
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE table_name = SUBSTRING(table_name, @start, @length)
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void TwoVariablesInAriphmetic()
        {
            var sqlBody = @"
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE 1 = @a - @b
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CreateCorrectTempTables()
        {
            var sqlBody = string.Format(@"
create table	{0}	.	dbo	.	##temp1  (id0 int, id1 int)
create table	{0}	.	dbo	.	#temp2  (id0 int, id1 int)

create table	{0}	.	dbo	.	##temp3  (id0 int, id1 int)
create table	{0}	.	dbo	.	#temp4  (id0 int, id1 int)

create table			dbo	.	##temp5  (id0 int, id1 int)
create table			dbo	.	#temp6  (id0 int, id1 int)

create table					##temp7  (id0 int, id1 int)
create table					#temp8  (id0 int, id1 int)
", TestSettings.Default.DatabaseName);

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CreateAndUpdateTempTables()
        {
            var sqlBody = @"
create table #temp8  (id0 int, id1 int)

insert into #temp8 (id0, id1) values (1, 2)
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.AreEqual(
                ValidationResultEnum.CannotValidate,
                report.Result,
                report.FailMessage
                );
        }

        [TestMethod]
        public void DropTempTable()
        {
            var sqlBody = @"
create table #droptable (id int)

drop table #droptable;select
    1 as i
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }


        [TestMethod]
        public void CallCorrectStoredProcedure()
        {
            var sqlBody = @"
[sys].[sp_columns] 'TestTable0'
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CallIncorrectStoredProcedure()
        {
            var sqlBody = @"
[sys].[sp_procedure_that_does_not_exists] 'TestTable0'
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectAlterIndex()
        {
            var sqlBody = @"
ALTER INDEX [TestTable0_Index0] ON [dbo].[TestTable0] DISABLE
ALTER INDEX [TestTable0_Index0] ON [dbo].[TestTable0] REBUILD
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectAlterIndex1()
        {
            var sqlBody = @"
ALTER INDEX [TestTable0_Index0] ON UnknownTable REBUILD
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectAlterIndex2()
        {
            var sqlBody = @"
ALTER INDEX [UnknownIndex] ON [dbo].[TestTable0] DISABLE
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CreateConfigureDropTempTable1()
        {
            var sqlBody = @"
create table #droptable (id int)

create nonclustered index ix_temp_1 on #droptable (id)

drop table #droptable;select --please do not change this formatting
	1 as i
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void ColumnNameContainsSharp()
        {
            var sqlBody = @"
select
	row_number() OVER(order by id) as #t
from dbo.TestTable0
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void SelectColumnAlias0()
        {
            var sqlBody = @"
select
    id,
	id + 1 new_column
from dbo.TestTable1
order by
    new_column
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectDeclareVariable()
        {
            var sqlBody = @"
declare @t0 int, @t1 varchar(100)
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectGeneratorVariable()
        {
            var generator = new Generator();
            generator.WithQuery(@"
select
    {4}
from dbo.TestTable0 t0
{0} join dbo.TestTable1 t1 on t0.id {2} t1.id
{1} join dbo.TestTable2 t2 on t0.id {3} t2.id
");

            generator.DeclareOption("0", "left", "right", "full", string.Empty);
            generator.DeclareOption("1", "left", "right", "full", string.Empty);
            generator.DeclareOption("2", "=", ">", "<", "<=", ">=", "<>");
            generator.DeclareOption("3", "=", ">", "<", "<=", ">=", "<>");
            generator.DeclareOption("4", "*", "t0.id", "t1.id", "t2.id");
            generator.DoFixing();

            var processed = ValidateAgainstSchema(
                generator
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void DeclarationStatement()
        {
            var sqlBody = @"
declare @a int;
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void DeclarationTableStatement()
        {
            var sqlBody = @"
DECLARE @tid table (id BIGINT)
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void GroupByCaseStatement()
        {
            var sqlBody = @"
select
    case when TestTable0.id in (@mobileType0, @mobileType1) then 1 else 0 end isMobile
from dbo.TestTable0
GROUP BY 
	case when TestTable0.id in (@mobileType0, @mobileType1) then 1 else 0 end

";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void DateAddStatement()
        {
            var sqlBody = @"
SELECT 
	DATEADD(day, @deadline, 1)
FROM dbo.TestTable1
WHERE
	getdate() < DATEADD(day, @deadline, 1)
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void ComplexTest6()
        {
            var sqlBody = @"
DECLARE @version INT = 1

INSERT INTO dbo.TestTable3
    (name, additional, custom_column, database_version)
VALUES
	(@binary, @binary, @version, @version)
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }


        [TestMethod]
        public void ComplexTest7()
        {
            var sqlBody = @"
SELECT
    1, 2
FROM dbo.TestTable1
WHERE
    getdate() < dateadd(day, @a, @b)
UNION ALL
SELECT
    1, 2
FROM dbo.TestTable1
WHERE
    getdate() < dateadd(day, @c, @d)
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectVariableDeclaration()
        {
            const string sqlBody = @"
declare @a int, @b varchar(10);
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectVariableDeclaration()
        {
            const string sqlBody = @"
declare @a int @b varchar(10);
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectCreateIndexStatement()
        {
            const string sqlBody = @"
CREATE NONCLUSTERED INDEX [TestTable1_NewIndex] ON [dbo].[TestTable1]
(
	[name] ASC
)
INCLUDE([additional])
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectCreateIndexStatement1()
        {
            const string sqlBody = @"
CREATE NONCLUSTERED INDEX [TestTable1_Index0] ON [dbo].[TestTable1]
(
	[unknown_column] ASC
)
INCLUDE([additional])
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectCreateIndexStatement2()
        {
            const string sqlBody = @"
CREATE NONCLUSTERED INDEX [TestTable1_Index0] ON [dbo].[TestTable1]
(
	[name] ASC
)
INCLUDE([unknown_column])
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        /// <summary>
        /// we are trying to create a duplicate index!
        /// </summary>
        [TestMethod]
        public void IncorrectCreateIndexStatement3()
        {
            const string sqlBody = @"
CREATE NONCLUSTERED INDEX [TestTable0_Index0] ON [dbo].[TestTable0]
(
	[name] ASC
)
INCLUDE([additional])
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectDropIndexStatement()
        {
            const string sqlBody = @"
DROP INDEX [TestTable0_Index0] ON dbo.TestTable0
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectDropIndexStatement1()
        {
            const string sqlBody = @"
DROP INDEX [TestTable0_Index0] ON UnknownTable
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectDropIndexStatement2()
        {
            const string sqlBody = @"
DROP INDEX [UnknownIndex] ON dbo.TestTable0
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectTruncateTableStatement()
        {
            const string sqlBody = @"
truncate table dbo.TestTable0
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectTruncateTableStatement()
        {
            const string sqlBody = @"
truncate table UnknownTable
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }


        [TestMethod]
        public void CorrectSelectJoinWithIndexStatement()
        {
            const string sqlBody = @"
select
	t0.id
from dbo.TestTable0 t0 with(index=TestTable0_Index0)
join dbo.TestTable1 t1 with(index=TestTable1_Index0) on t0.id = t1.id
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectSelectJoinWithIndexStatement1()
        {
            const string sqlBody = @"
select
	t0.id
from dbo.TestTable0 t0 with(index=UnknownIndex)
join dbo.TestTable1 t1 with(index=TestTable1_Index0) on t0.id = t1.id
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectSelectJoinWithIndexStatement2()
        {
            const string sqlBody = @"
select
	t0.id
from dbo.TestTable0 t0 with(index=TestTable0_Index0)
join dbo.TestTable1 t1 with(index=Unknown) on t0.id = t1.id
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        /// <summary>
        /// indexes are switched!
        /// </summary>
        [TestMethod]
        public void IncorrectSelectJoinWithIndexStatement3()
        {
            const string sqlBody = @"
select
	t0.id
from dbo.TestTable0 t0 with(index=TestTable1_Index0)
join dbo.TestTable1 t1 with(index=TestTable0_Index0) on t0.id = t1.id
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }


        [TestMethod]
        public void CorrectSelectFromFromWithIndexStatement()
        {
            const string sqlBody = @"
select
    t0.id
from dbo.TestTable0 t0 with(index=TestTable0_Index0), dbo.TestTable1 t1 with(index=TestTable1_Index0)
where
	t0.id = t1.id
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectSelectFromFromWithIndexStatement1()
        {
            const string sqlBody = @"
select
    t0.id
from dbo.TestTable0 t0 with(index=UnknownIndex), dbo.TestTable1 t1 with(index=TestTable1_Index0)
where
	t0.id = t1.id
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectSelectFromFromWithIndexStatement2()
        {
            const string sqlBody = @"
select
    t0.id
from dbo.TestTable0 t0 with(index=TestTable0_Index0), dbo.TestTable1 t1 with(index=UnknownIndex)
where
	t0.id = t1.id
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        /// <summary>
        /// indexes are switched!
        /// </summary>
        [TestMethod]
        public void IncorrectSelectFromFromWithIndexStatement3()
        {
            const string sqlBody = @"
select
    t0.id
from dbo.TestTable0 t0 with(index=TestTable1_Index0), dbo.TestTable1 t1 with(index=TestTable0_Index0)
where
	t0.id = t1.id
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectUpdateStatement1()
        {
            const string sqlBody = @"
update UnknownTable
set
    name = '1'
where
	id = 1
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }


        [TestMethod]
        public void IncorrectUpdateStatement2()
        {
            const string sqlBody = @"
update TestTable0
set
    unknown_column = '1'
where
	name = 1
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }


        [TestMethod]
        public void IncorrectUpdateStatement3()
        {
            const string sqlBody = @"
update TestTable0
set
    name = '1'
where
	unknown_column = 1
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectUpdateStatement4()
        {
            const string sqlBody = @"

update TestTable0
set
	name = '1'
from TestTable0 t0 with (index = UnknownIndex)
join TestTable1 t1 on t0.id = t1.id
where
	t0.id = 12 
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectDeleteStatement1()
        {
            const string sqlBody = @"

delete from UnknownTable
where
    id = 1
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectDeleteStatement2()
        {
            const string sqlBody = @"

delete from TestTable0
where
    unknown_column = 1
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IncorrectDeleteStatement3()
        {
            const string sqlBody = @"

delete from TestTable0
from TestTable0 t0 with (index = 123)
join TestTable1 t1 on t0.id = t1.id
where
	t0.id = 12 
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsFalse(report.IsSuccess, report.FailMessage);
        }


        [TestMethod]
        public void CorrectInsertWithFunction1()
        {
            const string sqlBody = @"
insert into [TestTable2] (id, name)
values ( 1, dbo.get_name1() )
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectInsertWithFunction2()
        {
            const string sqlBody = @"
insert into [TestTable2] (id, name)
values ( 1, dbo.get_name2('a') )
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectSelectFromAlias()
        {
            const string sqlBody = @"
select
    tt2.*
from TestTable2 tt2
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }


        [TestMethod]
        public void CorrectUpdateCte0()
        {
            const string sqlBody = @"
with cte as (
	select id, name from TestTable2
)
update cte
set name = ''
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }


        [TestMethod]
        public void CorrectUpdateFromAlias0()
        {
            const string sqlBody = @"
update tt2
set name = name
from TestTable2 tt2
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectSelectFromAlias0()
        {
            const string sqlBody = @"
select
	testtable2.name
from dbo.TestTable2 testtable2
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void CorrectSelectFromAlias1()
        {
            const string sqlBody = @"
select
	TestTable.day,
	TestTable3.custom_column
from [dbo].[TestTable1] as TestTable
left join [dbo].[TestTable3] as TestTable3 on TestTable.id = TestTable3.id
";

            var processed = ValidateAgainstSchema(
                sqlBody
                );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

    }
}
