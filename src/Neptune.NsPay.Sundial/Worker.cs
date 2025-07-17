using Abp.Json;
using Neptune.NsPay.NsPayBackgroundJobs;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Sundial.Services.Interfaces;
using Sundial;

namespace Neptune.NsPay.Sundial
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly INsPayBackgroundJobsService _nsPayBackgroundJobsService;
        private readonly IBankAutoService _bankAutoService;
        private readonly ISchedulerFactory _schedulerFactory;

        public Worker(
            ILogger<Worker> logger,
            INsPayBackgroundJobsService nsPayBackgroundJobsService,
            IBankAutoService bankAutoService,
            ISchedulerFactory schedulerFactory)
        {
            _logger = logger;
            _nsPayBackgroundJobsService = nsPayBackgroundJobsService;
            _bankAutoService = bankAutoService;
            _schedulerFactory = schedulerFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var tasks = _nsPayBackgroundJobsService.GetAll();
                var jobs = _schedulerFactory.GetJobs();
                _logger.LogInformation("Jobs status: {jobs}", jobs.ToJsonString());

                foreach (var task in tasks)
                {
                    if (_bankAutoService.CheckJob(task.Id.ToString(), task.Name))
                    {
                        if (task.IsPaused)
                        {
                            await _bankAutoService.PauseJob(task.Id);
                        }

                        if (task.IsRestart)
                        {
                            await _bankAutoService.RestartJob(task.Id);
                        }

                        if (task.IsDeleted)
                        {
                            await _bankAutoService.RemoveJob(task.Id);
                        }
                    }
                    else
                    {
                        if (task.State == NsPayBackgroundJobStateEnum.Pending)
                        {
                            await _bankAutoService.AddJob(task.Id);
                        }
                    }
                }
                await Task.Delay(1000 * 6, stoppingToken);
            }
        }
    }
}