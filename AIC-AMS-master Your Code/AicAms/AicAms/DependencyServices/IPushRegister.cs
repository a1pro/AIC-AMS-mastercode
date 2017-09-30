using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AicAms.DependencyServices
{
    public interface IPushRegister
    {
        void Register(string tag);

        void Unregister();
    }
}
