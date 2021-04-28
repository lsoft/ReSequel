namespace Main.Sql.Identifier
{
    public interface IColumnName
    {
        string ColumnName
        {
            get;
        }

        bool IsAlias
        {
            get;
        }

        bool IsStar
        {
            get;
        }

        bool IsSame(
            string otherColumnName,
            bool isAlias = false
            );

    }

}
