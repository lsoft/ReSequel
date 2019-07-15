namespace Extension.ConfigurationRelated
{
    public class ConfigurationSqlExecutorsSqlExecutor
    {
        public string Name
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public string ConnectionString
        {
            get;
            set;
        }

        public string Parameters
        {
            get;
            set;
        }

        public bool IsDefault
        {
            get;
            set;
        }

        public bool TryGetParameter(string parameterName, out string parameterValue)
        {
            if (string.IsNullOrEmpty(Parameters))
            {
                parameterValue = string.Empty;
                return false;
            }

            var pairs = Parameters.Split(';');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    if (parts[0] == parameterName)
                    {
                        parameterValue = parts[1];
                        return true;
                    }
                }
            }

            parameterValue = string.Empty;
            return false;
        }
    }
}
