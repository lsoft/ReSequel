using LSoft.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler
{
    class BaseProgram
    {
        protected const string Part2OptionName = "part2";
    }

    class Program : BaseProgram
    {
        private const string Part3OptionName = "part3";

        static void Main(string[] args)
        {
            var dbp = new DBProvider();

            const string JoinOptionName = "join";

            //dbp
            //    .WithGenerator()
            //    .WithQuery("select 1 {0} {1} {2} {3}")
            //    .DeclareOption(JoinOptionName, "union", "except")
            //    .DeclareOption(Part2OptionName, "select 2", "select 2,2")
            //    .DeclareOption(JoinOptionName, "except", "union")
            //    .DeclareOption(Part3OptionName, "select 3", "select 3,3")
            //    ;

            /*

            var generator0 = dbp.WithGenerator(
                );

            generator0
                .WithQuery(@"
select
    {4}
from table_that_does_not_exists t0
{0} join table_that_does_not_exists t1 on t0.id {2} t1.id
{1} join table_that_does_not_exists t2 on t0.id {3} t2.id
")

                .DeclareOption("0", "left", "right", "full", string.Empty)
                .DeclareOption("1", "left", "right", "full", string.Empty)
                .DeclareOption("2", "=", ">", "<", "<=", ">=", "<>")
                .DeclareOption("3", "=", ">", "<", "<=", ">=", "<>")
                .DeclareOption("4", "*", "t0.id", "t1.id", "t2.id")
                ;

             
            var generator1 = dbp.WithGenerator(
                );

            generator1
                .WithQuery(@"
select
    {4}
from X4_TARIF_HEAD t0
{0} join X4_TARIF_HEAD t1 on t0.id {2} t1.id
{1} join X4_TARIF_HEAD t2 on t0.id {3} t2.id
")

                .DeclareOption("0", "left", "right", "full", string.Empty)
                .DeclareOption("1", "left", "right", "full", string.Empty)
                .DeclareOption("2", "=", ">", "<", "<=", ">=", "<>")
                .DeclareOption("3", "=", ">", "<", "<=", ">=", "<>")
                .DeclareOption("4", "*", "t0.id", "t1.id", "t2.id")
                ;
            //*/


            /*
            var generator2 = dbp.WithGenerator(
                );

            generator2
                .WithQuery("select 1,1 {0} {1} {2} {3}")
                .DeclareOption(JoinOptionName, "union", "except")
                .DeclareOption(Part2OptionName, "select 2,2", "select 2,2")
                ;

            generator2
                .DeclareOption(JoinOptionName, "except", "union")
                .DeclareOption(Part3OptionName, "select 3,3", "select 3,3")
                ;


            dbp.PrepareQuery(
                //ReSequel: MUTE next query
                "print 1",
                "print 2"
                );

            //*/

            string q0 = "fake body";
            q0 = "print 0";
            dbp.PrepareQuery(
                q0
                );

            string q1 = "fake body";
            dbp.PrepareQuery(
                q1
                );

            string q2 = "print 0" + "1" + "2";
            dbp.PrepareQuery(
                q2
                );

            const string constq = "print 1";
            dbp.PrepareQuery(
                constq
                );

            dbp.PrepareQuery(
                //ReSequel: MUTE next query at: *NameOfSomeSolution.sln, *DbProviderExample.2017.sln
                "print 2"
                );

            

            dbp.PrepareQuery(@"
--1
INSERT INTO X4_TARIF_HEAD
VALUES
    (@name, null, @headId, null, null, 1);

SELECT @@IDENTITY as ident;
");

            dbp.PrepareQuery(@"
select * from [dbo].[X4_TARIF_HEAD] where id_km in (@t)
");


            dbp.PrepareQuery(@"
INSERT INTO [dbo].[X4_TARIF_HEAD]
           ([name]
           ,[id_const]
           ,[id_matrix]
           ,[id_km]
           ,[id_zone]
           ,[status]
           ,[id_group])
     VALUES
           (@name,
            @id_const,
            @id_matrix,
            @id_km,
            @id_zone,
            @status,
            @id_group
		    )
");
        //*/
        }

        public static void AdditionalMethod(IDBProvider dbProvider)
        {
            dbProvider.ExecuteNonQuery(@"
print 2
GO
print 3
");

            dbProvider.ExecuteNonQuery(@"
select 2
GO
select 3
");

            dbProvider.ExecuteNonQuery(@"
select * from sqlite_sequence
");
            


        }
    }
}
