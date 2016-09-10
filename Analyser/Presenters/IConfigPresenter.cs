using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betabelter
{
    public interface IConfigPresenter
    {
        void Load();
        void OpenWorkDir();
        bool WorkDirExists();
    }
}
