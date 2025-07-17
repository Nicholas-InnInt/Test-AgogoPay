namespace Neptune.NsPay.MongoToSqlWorker.QuartzFactory.Model
{
    public class JobSchedule
    {
        public JobSchedule(Type jobType, QuartzJob quartzSetting)
        {
            JobType = jobType;
            CronExpression = quartzSetting.CronExpression;
            TriggerType = quartzSetting.TriggerType;
            SimpleRepeatIntervalInMilliSec = quartzSetting.SimpleRepeatCount;
            SimpleRepeatCount = quartzSetting.SimpleRepeatCount;
        }

        public Type JobType { get; }
        public QuartzTriggerTypeEnum TriggerType { get; }
        public string CronExpression { get; }
        public int SimpleRepeatIntervalInMilliSec { get; }
        public int SimpleRepeatCount { get; }
    }
}
