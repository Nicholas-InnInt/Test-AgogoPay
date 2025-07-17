namespace Neptune.NsPay.MongoToSqlWorker.QuartzFactory.Model
{
    public class QuartzSetting
    {
        public QuartzJob[] QuartzJobs { get; set; }
    }

    public class QuartzJob
    {
        public string JobType { get; set; }
        public QuartzTriggerTypeEnum TriggerType { get; set; }
        public string CronExpression { get; set; }
        public int SimpleRepeatIntervalInMilliSec { get; }
        public int SimpleRepeatCount { get; }
    }

    public enum QuartzTriggerTypeEnum
    {
        Cron,
        Simple
    }
}
