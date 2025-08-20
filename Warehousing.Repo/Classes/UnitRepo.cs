using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Classes
{
    public class UnitRepo: RepositoryBase<Unit>, IUnitRepo
    {
        public UnitRepo(WarehousingContext context, ILogger<UnitRepo> logger, IConfiguration config): base(context, logger, config){}
    }
}