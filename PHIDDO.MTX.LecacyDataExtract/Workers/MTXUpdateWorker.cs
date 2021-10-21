using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using PHIDDO.MTX.LecacyDataExtract.Models.Models;
using PHIDDO.MTX.LecacyDataExtract.Updater.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PHIDDO.MTX.LecacyDataExtract.Updater.Workers
{
    public class MTXUpdateWorker : BackgroundService
    {
        private IHostApplicationLifetime _hostApplicationLifetime;
        private ILogger<MTXUpdateWorker> _logger;
        private IMTXUpdateService _service;
        private IMTXApi _api;

        public MTXUpdateWorker(IHostApplicationLifetime hostApplicationLifetime,
            ILogger<MTXUpdateWorker> logger,
            IMTXUpdateService service,
            IMTXApi api)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
            _service = service;
            _api = api;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            _logger.LogInformation($"Made it to the worker");
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    List<Record>  records = await _service.GetRecords();

                    if (records is null || records.Count == 0)
                    {
                        _logger.LogInformation($"No records to process");
                    }
                    else
                    {
                        var token = await _api.GetToken();
                        if (token != "")
                        {
                            DateTime uploadTime = DateTime.Now;
                            // .Take(500)
                            foreach (var record in records)
                            {
                                var traveler = new TravelerDataSet();
                                traveler.travelerData = new List<Record>();
                                traveler.travelerData.Add(record);
                                var result = await _api.SendCovidResult(token, traveler);
                                if(result != null)
                                {
                                    var updateResult = await _service.UpdateRecordProcessed(record, result, uploadTime);
                                }
                            }
                        }
                    }
                    _logger.LogInformation("Sleeping for a bit....");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
            catch (Exception ex) when (False(() => _logger.LogCritical(ex, "Fatal error")))
            {
                throw;
            }
            finally
            {
                _hostApplicationLifetime.StopApplication();
            }
        });

        private static bool False(Action action) { action(); return false; }
    }
}
