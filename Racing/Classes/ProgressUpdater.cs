using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racing
{
    public class BasicUpdate
    {
        public BasicUpdate(int progressPercent =  0, String text = "")
        {
            ProgressPercentage = progressPercent;
            Text = text;

        }
        private int _progressPercent;
        public int ProgressPercentage
        {
            get
            {
                return _progressPercent;
            }
            set
            {
                if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("value must be between 0 and 100");
                _progressPercent = value;
            }
        }
        public string Text { get; set; }
    }
}
