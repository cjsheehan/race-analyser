using Racing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betabelter
{
    public class ConfigPresenter : IConfigPresenter
    {
        private IConfigView _view;
        private IConfigModel _model;

        public ConfigPresenter(IConfigView view, IConfigModel model) 
        {
            _view = view;
            _model = model;
            _view.ChangeWorkDir += ChangeWorkDirEventHandler;
        }
        public virtual void Load()
        {
            _view.WorkDir = _model.WorkDir;
            _view.Show();
        }

        public void OpenWorkDir()
        {
            String workDir = _model.WorkDir;
            if (Directory.Exists(workDir))
            {
                using (System.Diagnostics.Process p = new System.Diagnostics.Process())
                {
                    p.StartInfo.FileName = workDir;
                    p.Start();
                }
            }
            else
            {
                // TODO : handle non existent folder
            }
        }

        public bool WorkDirExists()
        {
            return Directory.Exists(_model.WorkDir);
        }

        protected virtual void ChangeWorkDirEventHandler(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();            
            System.Windows.Forms.DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _model.WorkDir = fbd.SelectedPath;
                _view.WorkDir = _model.WorkDir;
            }
        }

    }
}
