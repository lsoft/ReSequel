namespace SqlServerValidator.Visitor.Known
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
