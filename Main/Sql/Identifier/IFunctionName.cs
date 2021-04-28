namespace Main.Sql.Identifier
{
    public interface IFunctionName
    {
        string FullFunctionName
        {
            get;
        }

        bool IsSame(
            string otherFunctionName
            );

    }

}
