namespace Neptune.NsPay.Sundial.Services.Interfaces
{
    public interface IBankAutoService
    {
        bool CheckJob(string id, string taskName);
        Task AddJob(Guid id);
        Task PauseJob(Guid id);
        Task RestartJob(Guid id);
        Task RemoveJob(Guid id);
    }
}
