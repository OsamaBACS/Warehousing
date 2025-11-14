using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Shared;
using Warehousing.Repo.Interfaces;

namespace Warehousing.Repo.Classes
{
    public class ProductVariantRepo : RepositoryBase<ProductVariant>, IProductVariantRepo
    {
        public ProductVariantRepo(WarehousingContext context, ILogger<ProductVariantRepo> logger, IConfiguration config) : base(context, logger, config)
        {
        }
    }
}
