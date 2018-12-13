using LSoft.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbp = new DBProvider();

            var generator = dbp.WithGenerator(
                );

            const string JoinOptionName = "join";
            const string Part2OptionName = "part2";
            const string Part3OptionName = "part3";

            //generator
            //    .WithQuery("select 1 {0} {1} {2} {3}")
            //    .BindToOption(JoinOptionName, "union", "except")
            //    .BindToOption(Part2OptionName, "select 2", "select 2,2")
            //    .BindToOption(JoinOptionName, "except", "union")
            //    .BindToOption(Part3OptionName, "select 3", "select 3,3")
            //    ;

            generator
                .WithQuery("select 1 {0} {1} {2} {3}")
                .BindToOption(JoinOptionName, "union", "except")
                .BindToOption(Part2OptionName, "select 2", "select 2,2")
                ;

            generator
                .BindToOption(JoinOptionName, "except", "union")
                .BindToOption(Part3OptionName, "select 3", "select 3,3")
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
