namespace Main.Sql.SqlServer.Visitor.Known
{
    public interface IKnownVariable
    {
        string Name
        {
            get;
        }

        string ToSqlDeclaration();
    }


}
