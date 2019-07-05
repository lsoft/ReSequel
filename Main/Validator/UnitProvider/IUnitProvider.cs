using System.Collections.Generic;

namespace Main.Validator.UnitProvider
{
    public interface IUnitProvider
    {
        int TotalVariantCount
        {
            get;
        }

        bool TryRequestNextUnit(out IValidationUnit unit);

        IEnumerable<IValidationUnit> RequestNextUnitSync();
    }
}
