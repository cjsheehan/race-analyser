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
       
        public PerlAnalyser()
        {
            string execLoc = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            _execLoc = execLoc.Replace("file:\\", "");
            _scriptFilePath = _execLoc + "\\" + SCRIPT_NAME;
        }
        public async System.Threading.Tasks.Task Analyse(List<IRace> races)
        {
            if (races == null) throw new ArgumentNullException("List<IRace> races cannot be null");
             if (String.IsNullOrEmpty(WorkDir)) throw new ArgumentNullException("string WorkDir cannot be null or empty");

            string outFile = WorkDir + "\\" + XML_FILE;

            FileInfo fi = new FileInfo(outFile);
            if(!fi.Exists)
                fi.Create();

            List<DTO_Race> dtoRaces = new List<DTO_Race>();
            foreach (var v in races)
            {
                DTO_Race race = null;
                if (v != null)
                {
                    RacingFactory.CreateRaceDTO(v, ref race);
                }

                if (race != null)
                {
                    dtoRaces.Add(race);
                }
            }
            

            using (TextWriter tw = new StreamWriter(fi.Open(FileMode.Truncate)))
            {
                // Serializing class
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(dtoRaces.GetType());
                x.Serialize(tw, dtoRaces);
            }


            StringBuilder args = new StringBuilder();
            args.Append("\"");
            args.Append(_scriptFilePath);
            args.Append("\" \"");
            args.Append(outFile);
            args.Append("\" \"");
            args.Append(WorkDir);
            args.Append("\"");

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
