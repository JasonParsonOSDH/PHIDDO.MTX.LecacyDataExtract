using Dapper;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PHIDDO.MTX.LecacyDataExtract.Models.Config;
using PHIDDO.MTX.LecacyDataExtract.Models.Models;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHIDDO.MTX.LecacyDataExtract.Updater.Repositories
{
    public class MTXUpdateRepository : IMTXUpdateRepository
    {
        private IOptionsSnapshot<DatabaseConfig> _dbConfig;
        private ILogger<MTXUpdateRepository> _logger;
        private int _uatCounter = 0;
        private static IConfiguration _config;

        public MTXUpdateRepository(IOptionsSnapshot<DatabaseConfig> dbConfig,
            ILogger<MTXUpdateRepository> logger,
            IConfiguration config)
        {
            _dbConfig = dbConfig;
            _logger = logger;
            _config = config;
        }

        public async Task<List<Record>> GetRecords()
        {
            if (_config["Environment"].Equals("LegacyTest")) return await GetUatRecords();

            IEnumerable<Record> records = null;
            try
            {
                using (var connection = new SqlConnection(_dbConfig.Value.ConnectionString))
                {
                    connection.Open();
                    using var multi = await connection.QueryMultipleAsync("usp_Get_MTX_Data_Extract", null, null, null, CommandType.StoredProcedure);
                    records = multi.Read<Record>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetRecords: {ex.Message}");
            }
            return records?.ToList();
        }

        

        public async Task<bool> UpdateRecordProcessed(Record record, TravelerResult result, DateTime uploadTime)
        {
            if (_config["Environment"].Equals("LegacyTest")) return await UpdateUatRecordProcessed(record, result, uploadTime);

            try
            {
                using (var connection = new SqlConnection(_dbConfig.Value.ConnectionString))
                {
                    var dp = new DynamicParameters();
                    dp.Add("@PHIDDO_Id", record.public_health_case_uid, DbType.Int32, ParameterDirection.Input);
                    dp.Add("@time_uploaded", uploadTime, DbType.DateTime, ParameterDirection.Input);
                    dp.Add("@MtxStatusCode", result.result[0]?.StatusCode ?? "", DbType.String, ParameterDirection.Input);
                    dp.Add("@DataMessage", result.result[0]?.data ?? "", DbType.String, ParameterDirection.Input);
                    dp.Add("@ErrorMessage", result.result[0]?.error ?? "", DbType.String, ParameterDirection.Input);
                    connection.Open();
                    await connection.ExecuteAsync("usp_Save_MTX_Data_Log", dp, null, null, CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateRecordProcessed: {ex.Message}");
            }
            return true;
        }

        #region UAT Functions

        public async Task<List<Record>> GetUatRecords()
        {
            _uatCounter++;
            IEnumerable<Record> records = null;
            try
            {
                using (var connection = new SqlConnection(_dbConfig.Value.ConnectionString))
                {
                    connection.Open();
                    var dp = new DynamicParameters();
                    dp.Add("@RunNumber", _uatCounter, DbType.Int32, ParameterDirection.Input);
                    using var multi = await connection.QueryMultipleAsync("usp_Get_MTX_Data_Extract_UAT", dp, null, null, CommandType.StoredProcedure);
                    records = multi.Read<Record>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetRecords: {ex.Message}");
            }
            return records?.ToList();
        }

        public async Task<bool> UpdateUatRecordProcessed(Record record, TravelerResult result, DateTime uploadTime)
        {
            try
            {
                using (var connection = new SqlConnection(_dbConfig.Value.ConnectionString))
                {
                    var dp = new DynamicParameters();
                    dp.Add("@PHIDDO_Id", record.public_health_case_uid, DbType.Int32, ParameterDirection.Input);
                    dp.Add("@time_uploaded", uploadTime, DbType.DateTime, ParameterDirection.Input);
                    dp.Add("@MtxStatusCode", result.result[0]?.StatusCode ?? "", DbType.String, ParameterDirection.Input);
                    dp.Add("@DataMessage", result.result[0]?.data ?? "", DbType.String, ParameterDirection.Input);
                    dp.Add("@ErrorMessage", result.result[0]?.error ?? "", DbType.String, ParameterDirection.Input);
                    connection.Open();
                    await connection.ExecuteAsync("usp_Save_MTX_Data_Log_UAT", dp, null, null, CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateRecordProcessed: {ex.Message}");
            }
            return true;
        }

        #endregion UAT Functions
    }
}
