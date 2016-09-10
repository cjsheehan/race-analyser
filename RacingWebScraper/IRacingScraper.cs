using Racing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacingWebScraper
{
    public interface IRacingScraper
    {
        event EventHandler<GetRaceDataAsyncCompletedEventArgs> GetRaceDataAsyncCompleted;
        Task<List<IRaceHeader>> GetHeadersAsync(DateTime dt, IProgress<BasicUpdate> progress);
        Task<List<IRaceDetail>> GetRaceDataAsync(List<String> urls, IProgress<BasicUpdate> progress);
    }


    public class GetRaceDataCompletedEventArgs
    {
        public List<IRaceDetail> Races { get; private set; }
        public GetRaceDataCompletedEventArgs(List<IRaceDetail> races)
        {
            Races = races;
        }
    }
}
