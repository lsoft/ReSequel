using Main.Progress;

namespace Main.Validator
{
    public interface IValidatorFactory
    {
        IValidator Create(
           ValidationProgress status
            );

    }
}
