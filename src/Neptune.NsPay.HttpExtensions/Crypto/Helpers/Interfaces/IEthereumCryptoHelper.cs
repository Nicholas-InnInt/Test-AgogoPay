using Neptune.NsPay.HttpExtensions.Crypto.Dtos;
using Neptune.NsPay.HttpExtensions.Crypto.Enums;

namespace Neptune.NsPay.HttpExtensions.Crypto.Helpers.Interfaces
{
    public interface IEthereumCryptoHelper
    {
        Task<List<TokenTransactionDto>> GetTokenTransactionsByAddress(TronTokenTypeEnum tokenType, string address);
    }
}