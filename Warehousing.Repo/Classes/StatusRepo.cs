using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Classes
{
    public class StatusRepo: RepositoryBase<Status>, IStatusRepo
    {
        private readonly WarehousingContext _context;

        public StatusRepo(WarehousingContext context, ILogger<StatusRepo> logger, IConfiguration config): base(context, logger, config)
        {
            _context = context;
        }

        public async Task<Status?> GetByCodeAsync(string code)
        {
            return await _context.Statuses.FirstOrDefaultAsync(s => s.Code == code);
        }
    }
}