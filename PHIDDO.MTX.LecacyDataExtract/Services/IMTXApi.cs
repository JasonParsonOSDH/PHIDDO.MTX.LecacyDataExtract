using PHIDDO.MTX.LecacyDataExtract.Models.Models;

using System.Threading.Tasks;

namespace PHIDDO.MTX.LecacyDataExtract.Updater.Services
{
    public interface IMTXApi
    {
        Task<string> GetToken();
        Task<TravelerResult> SendCovidResult(string token, TravelerDataSet traveler);
    }
}