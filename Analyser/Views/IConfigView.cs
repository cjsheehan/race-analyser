using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betabelter
{
    public interface IConfigView
    {
        event EventHandler<EventArgs> ChangeWorkDir;
        String WorkDir { get; set; }
        void Show();
        void Close();
    }
}
