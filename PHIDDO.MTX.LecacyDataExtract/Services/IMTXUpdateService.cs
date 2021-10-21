using PHIDDO.MTX.LecacyDataExtract.Models.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PHIDDO.MTX.LecacyDataExtract.Updater.Services
{
    public interface IMTXUpdateService
    {
        Task<List<Record>> GetRecords();
        Task<bool> UpdateRecordProcessed(Record record, TravelerResult result, DateTime uploadTime);
    }
}