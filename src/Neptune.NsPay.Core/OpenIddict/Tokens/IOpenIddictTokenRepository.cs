﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Repositories;

namespace Neptune.NsPay.OpenIddict.Tokens
{
    public interface IOpenIddictTokenRepository : IRepository<OpenIddictToken, Guid>
    {
        Task DeleteManyByApplicationIdAsync(Guid applicationId, bool autoSave = false,
            CancellationToken cancellationToken = default);

        Task DeleteManyByAuthorizationIdAsync(Guid authorizationId, bool autoSave = false,
            CancellationToken cancellationToken = default);

        Task DeleteManyByAuthorizationIdsAsync(Guid[] authorizationIds, bool autoSave = false,
            CancellationToken cancellationToken = default);

        Task<List<OpenIddictToken>> FindAsync(string subject, Guid client,
            CancellationToken cancellationToken = default);

        Task<List<OpenIddictToken>> FindAsync(string subject, Guid client, string status,
            CancellationToken cancellationToken = default);

        Task<List<OpenIddictToken>> FindAsync(string subject, Guid client, string status, string type,
            CancellationToken cancellationToken = default);

        Task<List<OpenIddictToken>> FindByApplicationIdAsync(Guid applicationId,
            CancellationToken cancellationToken = default);

        Task<List<OpenIddictToken>> FindByAuthorizationIdAsync(Guid authorizationId,
            CancellationToken cancellationToken = default);

        Task<OpenIddictToken> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<OpenIddictToken> FindByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default);

        Task<List<OpenIddictToken>> FindBySubjectAsync(string subject, CancellationToken cancellationToken = default);

        Task<List<OpenIddictToken>> ListAsync(int? count, int? offset, CancellationToken cancellationToken = default);

        Task<long> PruneAsync(DateTime date, CancellationToken cancellationToken = default);

        ValueTask<long> RevokeByAuthorizationIdAsync(Guid id, CancellationToken cancellationToken);
    }
}