using System;
using System.Diagnostics;
using Main.Inclusion.Validated.Result;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Fixture;

namespace Tests.Fixture.Validation
{

    [TestClass]
    public class ShouldBeGreenSqlFixture : BaseFixture
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            BaseFixture.ClassInit();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            BaseFixture.ClassCleanup();

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
        public void CorrectCreateTableStatement()
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

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
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
            //select @@identity as ident


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
        public void CreateConfigureDropTempTable1()
        {
            var sqlBody = @"
create table #droptable (id int)

create nonclustered index ix_temp_1 on #droptable (id)

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

    }
}
