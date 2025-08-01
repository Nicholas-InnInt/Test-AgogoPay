﻿using Neptune.NsPay.HttpExtensions.Crypto.Dtos;
using Neptune.NsPay.HttpExtensions.Crypto.Enums;

namespace Neptune.NsPay.HttpExtensions.Crypto.Helpers.Interfaces
{
    public interface ITronCryptoHelper
    {
        Task<List<TokenTransactionDto>> GetTokenTransactionsByAddress(TronTokenTypeEnum tokenType, string toAddress);
    }
}