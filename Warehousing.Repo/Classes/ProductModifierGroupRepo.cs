using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Shared;
using Warehousing.Repo.Interfaces;

namespace Warehousing.Repo.Classes
{
    public class ProductModifierGroupRepo : RepositoryBase<ProductModifierGroup>, IProductModifierGroupRepo
    {
        public ProductModifierGroupRepo(WarehousingContext context, ILogger<ProductModifierGroupRepo> logger, IConfiguration config) : base(context, logger, config)
        {
        }
    }
}
