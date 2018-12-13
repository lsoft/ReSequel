
using Main.Inclusion;
using Main.Inclusion.Validated;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Main.Validator
{

    public interface IValidator
    {
        ///// <summary>
        ///// No order guarrantee!
        ///// </summary>
        //Task ValidateAsync(
        //    List<IValidatedSqlInclusion> inclusions
        //    );

        /// <summary>
        /// No order guarrantee!
        /// </summary>
        void Validate(
           List<IValidatedSqlInclusion> inclusions
           );
    }
}
