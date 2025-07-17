using Abp.Application.Services.Dto;

namespace Neptune.NsPay.Authorization.Users.Dto
{
    public interface IGetLoginAttemptsInput: ISortedResultRequest
    {
        string Filter { get; set; }
    }
}