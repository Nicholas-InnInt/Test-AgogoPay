using Neptune.NsPay.NsPayBackgroundJobs;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Sundial.Services.Interfaces;
using Sundial;
using TimeCrontab;

namespace Neptune.NsPay.Sundial.Services
{
    public class BankAutoService : IBankAutoService
    {
        private readonly ILogger<BankAutoService> _logger;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly INsPayBackgroundJobsService _nsPayBackgroundJobsService;

        public BankAutoService(
            ILogger<BankAutoService> logger,
            ISchedulerFactory schedulerFactory,
            INsPayBackgroundJobsService nsPayBackgroundJobsService)
        {
            _logger = logger;
            _schedulerFactory = schedulerFactory;
            _nsPayBackgroundJobsService = nsPayBackgroundJobsService;
        }

        public bool CheckJob(string id, string taskName)
        {
            var isExist = _schedulerFactory.ContainsJob(id + "->" + taskName);
            return isExist;
        }

        public async Task AddJob(Guid id)
        {
            if (id == Guid.Empty) return;

            var data = await _nsPayBackgroundJobsService.GetFirstAsync(f => f.Id == id);
            if (data == null)
            {
                _logger.LogWarning($"AddJob failed: Job not found for Id: {id}");
                return;
            }

            var taskName = data.Id.ToString() + "->" + data.Name;

            bool existsInScheduler = _schedulerFactory.ContainsJob(taskName);

            if (data.State == NsPayBackgroundJobStateEnum.Running && existsInScheduler)
            {
                _logger.LogInformation($"Job [{taskName}] already exists and marked as Running. Skipped.");
                return;
            }

            try
            {
                AddJob(data); // 加入 scheduler
                data.State = NsPayBackgroundJobStateEnum.Running;
                await _nsPayBackgroundJobsService.UpdateAsync(data); // ✅ 使用 async 方法（若有）
                _logger.LogInformation($"Job [{taskName}] added and state updated to Running.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add Job [{taskName}] :{ex.Message}");
            }
        }

        private void AddJob(NsPayBackgroundJob backgroundJob)
        {
            try
            {
                var taskName = backgroundJob.Id.ToString() + "->" + backgroundJob.Name;
                _schedulerFactory.AddHttpJob(request =>
                {
                    request.RequestUri = backgroundJob.ApiUrl;
                    request.HttpMethod = backgroundJob.RequsetMode switch
                    {
                        NsPayBackgroundJobRequsetModeEnum.Get => HttpMethod.Get,
                        NsPayBackgroundJobRequsetModeEnum.Post => HttpMethod.Post,
                        _ => throw new NotImplementedException(),
                    };
                    request.GroupName = backgroundJob.GroupName;
                    request.Body = backgroundJob.ParamData;
                }, taskName, false, Triggers.Cron(backgroundJob.Cron, CronStringFormat.WithSeconds));
            }
            catch (Exception ex)
            {
                _logger.LogError($"账户：{backgroundJob.Name},创建任务失败：{ex.Message}", ex);
            }
        }

        public async Task PauseJob(Guid id)
        {
            if (id == Guid.Empty) return;
            var data = await _nsPayBackgroundJobsService.GetFirstAsync(f => f.Id == id);

            PauseJob(data.Id.ToString(), data.Name);

            data.IsPaused = false;
            _nsPayBackgroundJobsService.Update(data);
        }

        public void PauseJob(string id, string taskName)
        {
            _schedulerFactory.PauseJob(id + "->" + taskName);
        }

        public async Task RestartJob(Guid id)
        {
            if (id == Guid.Empty) return;
            var data = await _nsPayBackgroundJobsService.GetFirstAsync(f => f.Id == id);
            _schedulerFactory.RemoveJob(data.Id.ToString() + "->" + data.Name);

            AddJob(data);

            data.IsRestart = false;
            data.State = NsPayBackgroundJobStateEnum.Running;
            _nsPayBackgroundJobsService.Update(data);
        }

        public async Task RemoveJob(Guid id)
        {
            if (id == Guid.Empty) return;
            var data = await _nsPayBackgroundJobsService.GetFirstAsync(f => f.Id == id);

            _schedulerFactory.RemoveJob(data.Id.ToString() + "->" + data.Name);
            _nsPayBackgroundJobsService.Delete(data.Id.ToString());
        }
    }
}