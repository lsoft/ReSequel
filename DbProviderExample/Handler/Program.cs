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

            var generator = dbp.WithGenerator(
                );

            generator
                .WithQuery("select 1,1 {0} {1} {2} {3}")
                .DeclareOption(JoinOptionName, "union", "except")
                .DeclareOption(Part2OptionName, "select 2,2", "select 2,2")
                ;

            generator
                .DeclareOption(JoinOptionName, "except", "union")
                .DeclareOption(Part3OptionName, "select 3,3", "select 3,3")
                ;


            dbp.PrepareQuery(
                //ReSequel: MUTE next query
                "print 1",
                "print 2"
                );

            //dbp1.PrepareQuery(
            //    "print 1"
            //    );

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
        }
    }
}
