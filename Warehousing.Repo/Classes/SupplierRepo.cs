using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Classes
{
    public class SupplierRepo: RepositoryBase<Supplier>, ISupplierRepo
    {
        public SupplierRepo(WarehousingContext context, ILogger<SupplierRepo> logger, IConfiguration config): base(context, logger, config){}
    }
}