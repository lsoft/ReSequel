using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Fixture.Sqlite.Validation
{
    [TestClass]
    public class ShouldBeGreenSqlFixture : SqliteFixture
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            SqliteFixture.ClassInit();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            SqliteFixture.ClassCleanup();

        }

        [TestMethod]
        public void SimpleSelectStatement()
        {
            var sqlBody = @"
select 1
";

            var processed = ValidateAgainstSchema(
                sqlBody
            );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void IsNullSelectStatement()
        {
            var sqlBody = @"
select ifnull(1, 1)
";

            var processed = ValidateAgainstSchema(
                sqlBody
            );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void BatchSelectStatement()
        {
            var sqlBody = @"
select 1
GO
select ifnull(1, 1)
";

            var processed = ValidateAgainstSchema(
                sqlBody
            );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

        [TestMethod]
        public void SelectStatementWithVariable()
        {
            var sqlBody = @"
select 1 where 1 = @a
";

            var processed = ValidateAgainstSchema(
                sqlBody
            );

            var report = processed.GenerateReport();

            Assert.IsTrue(report.IsSuccess, report.FailMessage);
        }

    }
}
