using Main.Other;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.ConfigurationRelated
{
    public partial class Configuration
    {
        public string ScanScheme
        {
            get;
            set;
        }

        public string FullPathToScanScheme
        {
            get
            {
                return
                    ScanScheme.GetFullPathToFile();
            }
        }

        public bool IsScanSchemeExists
        {
            get
            {
                return
                    File.Exists(FullPathToScanScheme);
            }
        }


        public ConfigurationSolutions Solutions
        {
            get;
            set;
        }

        public ConfigurationSqlExecutors SqlExecutors
        {
            get;
            set;
        }

        public bool IsEmpty
        {
            get
            {
                var result =
                    this.Solutions != null
                    && this.Solutions.Solution != null
                    && this.Solutions.Solution.Length > 0
                    && this.SqlExecutors != null 
                    && this.SqlExecutors.SqlExecutor != null 
                    && this.SqlExecutors.SqlExecutor.Length > 0
                    ;

                return
                    !result;
            }

        }

    }
}
