using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;

namespace Warehousing.Repo.Classes
{
    public class InventoryRepo : RepositoryBase<Inventory>, IInventoryRepo
    {
        public InventoryRepo(WarehousingContext context, ILogger<InventoryRepo> logger, IConfiguration config): base(context, logger, config){}
    }
}