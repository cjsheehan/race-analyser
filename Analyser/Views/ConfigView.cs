using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Betabelter
{
    public partial class ConfigView : Form, IConfigView
    {
        public event EventHandler<EventArgs> ChangeWorkDir;
        public ConfigView()
        {
            InitializeComponent();
        }
        public String WorkDir 
        {
            get
            {
                return this.textBoxWorkDir.Text;
            }
            set
            {
                this.textBoxWorkDir.Text = value;
            }
        }
        void IConfigView.Show()
        {
            ShowDialog();
        }

        void IConfigView.Close()
        {
            Close();
        }

        private void buttonWorkDir_Click(object sender, EventArgs e)
        {
            OnChangeWorkDir();
        }

        protected void OnChangeWorkDir()
        {
            EventHandler<EventArgs> handler = ChangeWorkDir;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
