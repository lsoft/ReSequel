using Main.Logger;
using System;

namespace Main.Progress
{
    public class ValidationProgressFactory
    {
        private readonly IProcessLogger _logger;

        public ValidationProgressFactory(
            IProcessLogger logger
            )
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;
        }


        public ValidationProgress Create(
            )
        {
            return
                new ValidationProgress(
                    _logger
                    );
        }
    }


}
