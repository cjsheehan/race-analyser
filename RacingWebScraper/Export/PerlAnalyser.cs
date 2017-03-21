using Racing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RacingWebScraper
{

    public class PerlAnalyser : IRaceAnalyser
    {
        public String WorkDir { get; set; }
        private string _scriptFilePath;
        private string _execLoc;
        private const string SCRIPT_NAME = "xmlHorseBuilder.pl";
        private const string XML_FILE = "meeting.xml";
        private const int WAIT = 10000;
        private INotify _ntf;
       
        public PerlAnalyser(INotify ntf)
        {
            if (ntf == null) throw new ArgumentNullException("ntf is null");
            string execLoc = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            _execLoc = execLoc.Replace("file:\\", "");
            _scriptFilePath = _execLoc + "\\" + SCRIPT_NAME;
            _ntf = ntf;
        }

        public async System.Threading.Tasks.Task Analyse(List<Race> races)
        {
            if (races == null)
            {
                _ntf.Notify("races empty", Ntf.ERROR);
                throw new ArgumentNullException("races is null");
            }
            if (String.IsNullOrEmpty(WorkDir))
            {
                _ntf.Notify("WorkDir is empty", Ntf.ERROR);
                throw new ArgumentNullException("WorkDir is null or empty");
            }

            string outFile = WorkDir + "\\" + XML_FILE;

            FileInfo fi = new FileInfo(outFile);
            if(!fi.Exists)
                fi.Create();     

            using (TextWriter tw = new StreamWriter(fi.Open(FileMode.Truncate)))
            {
                // Serializing class
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(races.GetType());
                x.Serialize(tw, races);
            }

            StringBuilder args = new StringBuilder();
            args.Append("\"");
            args.Append(_scriptFilePath);
            args.Append("\" \"");
            args.Append(outFile);
            args.Append("\" \"");
            args.Append(WorkDir);
            args.Append("\"");

            _ntf.Notify(String.Format("Running {0} {1} {2}", _scriptFilePath, outFile, WorkDir), Ntf.MESSAGE);
            await RunPerlProc(args.ToString());
        }

        private async Task RunPerlProc(string args)
        {
            Process p = null;
            bool procSuccess = false;
            await Task.Run( () =>
	        {
                using (p = new Process())
                {
                    p.StartInfo.FileName = "perl";
                    p.StartInfo.Arguments = args;
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                    procSuccess = p.WaitForExit(WAIT);

                }

                if (procSuccess)
                {
                    string launchFolder = WorkDir;
                    using (p = new Process())
                    {
                        string openDir = WorkDir;
                        p.StartInfo.FileName = openDir;
                        p.Start();
                    }
                }
                else
                {
                    throw new InvalidOperationException("RunPerlProc() failed to start external process");
                }
            });
        }
    }
}
