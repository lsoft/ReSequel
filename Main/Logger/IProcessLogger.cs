using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Logger
{
    public interface IProcessLogger
    {
        void ShowProcessMessage(
            string message,
            params object[] args
            );

        void ShowProcessMessage(
            string message
            );
    }
}
