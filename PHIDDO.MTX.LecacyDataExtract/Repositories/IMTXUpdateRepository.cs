using PHIDDO.MTX.LecacyDataExtract.Models.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PHIDDO.MTX.LecacyDataExtract.Updater.Repositories
{
    public interface IMTXUpdateRepository
    {
        Task<List<Record>> GetRecords();
        Task<bool> UpdateRecordProcessed(Record record, TravelerResult result, DateTime uploadTime);

        Task<List<Record>> GetUatRecords();

        Task<bool> UpdateUatRecordProcessed(Record record, TravelerResult result, DateTime uploadTime);
    }
}