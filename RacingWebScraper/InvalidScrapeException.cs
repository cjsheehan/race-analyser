using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacingWebScraper
{
    public class InvalidScrapeException : Exception
    {
        public InvalidScrapeException()
        {
        }

        public InvalidScrapeException(string message)
            : base(message)
        {
        }

        public InvalidScrapeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
