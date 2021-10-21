using Microsoft.Extensions.Logging;

using PHIDDO.MTX.LecacyDataExtract.Models.Models;
using PHIDDO.MTX.LecacyDataExtract.Updater.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHIDDO.MTX.LecacyDataExtract.Updater.Services
{
    public class MTXUpdateService : IMTXUpdateService
    {
        private IMTXUpdateRepository _repository;
        private ILogger<IMTXUpdateService> _logger;

        public MTXUpdateService(IMTXUpdateRepository repository, ILogger<IMTXUpdateService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<Record>> GetRecords()
        {
            List<Record> records = null;
            try
            {
                var repoRecords = await _repository.GetRecords();
                records = repoRecords;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MTXUpdateService.GetRecords: {ex.Message}");
            }
            return records;
        }

        public async Task<bool> UpdateRecordProcessed(Record record, TravelerResult result, DateTime uploadTime)
        {
            try
            {
                return await _repository.UpdateRecordProcessed(record, result, uploadTime);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MTXUpdateService.UpdateRecordProcessed: {ex.Message}");
            }
            return false;
        }
    }
}
