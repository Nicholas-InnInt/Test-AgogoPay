using Quartz.Spi;
using Quartz;
using Neptune.NsPay.MongoToSqlWorker.QuartzFactory.Model;

namespace Neptune.NsPay.MongoToSqlWorker.QuartzFactory
{
    public class QuartzHostedService : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly IEnumerable<JobSchedule> _jobSchedules;

        public QuartzHostedService(
            ISchedulerFactory schedulerFactory,
            IJobFactory jobFactory,
            IEnumerable<JobSchedule> jobSchedules)
        {
            _schedulerFactory = schedulerFactory;
            _jobSchedules = jobSchedules;
            _jobFactory = jobFactory;
        }
        public IScheduler Scheduler { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            Scheduler.JobFactory = _jobFactory;

            foreach (var jobSchedule in _jobSchedules)
            {
                var job = CreateJob(jobSchedule);

                ITrigger trigger = null;
                if (jobSchedule.TriggerType == QuartzTriggerTypeEnum.Cron)
                    trigger = CreateCronTrigger(jobSchedule);
                else if (jobSchedule.TriggerType == QuartzTriggerTypeEnum.Simple) 
                    trigger = CreateSimpleTrigger(jobSchedule);

                await Scheduler.ScheduleJob(job, trigger, cancellationToken);
            }

            await Scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Scheduler?.Shutdown(cancellationToken);
        }

        private static IJobDetail CreateJob(JobSchedule schedule)
        {
            var jobType = schedule.JobType;
            return JobBuilder
                .Create(jobType)
                .WithIdentity(jobType.FullName)
                .WithDescription(jobType.Name)
                .Build();
        }

        private static ITrigger CreateCronTrigger(JobSchedule schedule)
        {
            return TriggerBuilder
                .Create()
                .WithIdentity($"{schedule.JobType.FullName}.trigger")
                .WithCronSchedule(schedule.CronExpression)
                .WithDescription(schedule.CronExpression)
                .Build();
        }

        private static ITrigger CreateSimpleTrigger(JobSchedule schedule)
        {
            if (schedule.SimpleRepeatCount == -1)
                return SimpleScheduleBuilder.RepeatSecondlyForever(schedule.SimpleRepeatIntervalInMilliSec / 1000).Build();
            else
                return SimpleScheduleBuilder.RepeatSecondlyForever(schedule.SimpleRepeatIntervalInMilliSec / 1000).WithRepeatCount(schedule.SimpleRepeatCount).Build();
        }
    }
}
