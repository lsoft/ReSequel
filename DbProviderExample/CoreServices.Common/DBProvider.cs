using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSoft.Server
{
    public class DBProvider
    {
        public string SqlText
        {
            get;
            set;
        }

        public void UnrelatedMethod(string q)
        {
        }




        public void PrepareQuery(string q1, string q2)
        {
        }

        public void PrepareQuery(string q)
        {
        }

        public void PrepareQuery(string query, CommandType type)
        {
        }




        public void ExecuteNonQuery(string q)
        {
        }




        public void ExecuteQuery(string queryString)
        {
        }

        public void ExecuteQuery(string queryString, int commandTimeout)
        {
        }




        public void ExecuteScaler()
        {
        }

        public void ExecuteScaler(string queryString)
        {
        }

        public Generator WithGenerator(
            )
        {
            return
                new Generator();
        }
    }

    public class Generator
    {
        public Generator WithQuery(
            string queryTemplate
            )
        {
            return
                this;
        }

        public Generator DeclareOption(
            string optionName,
            params string[] parts
            )
        {
            return
                this;
        }
    }

}

