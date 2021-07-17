
using System;
using Main.Inclusion.Validated;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Main.Validator
{

    public interface IValidator
    {
        /// <summary>
        /// No order guarrantee!
        /// </summary>
        /// <param name="shouldBreak">Signal that validation process should be stopped prematurely.</param>
        Task ValidateAsync(
           List<IValidatedSqlInclusion> inclusions,
           Func<bool> shouldBreak
            );
    }
}
