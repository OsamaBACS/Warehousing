using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Shared;
using Warehousing.Repo.Interfaces;

namespace Warehousing.Repo.Classes
{
    public class ProductModifierRepo : RepositoryBase<ProductModifier>, IProductModifierRepo
    {
        public ProductModifierRepo(WarehousingContext context, ILogger<ProductModifierRepo> logger, IConfiguration config) : base(context, logger, config)
        {
        }
    }
}
