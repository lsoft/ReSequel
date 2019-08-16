namespace Main.Sql.VariableRef
{
    public interface IVariableRef
    {
        string Name
        {
            get;
        }

        int ReferenceCount
        {
            get;
        }

        bool IsSame(
            string otherVariableName
            );

    }
}
