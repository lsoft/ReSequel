namespace Extension.ConfigurationRelated
{
    public class Configuration
    {
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
