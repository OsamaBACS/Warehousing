using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Classes
{
    public class SubCategoryRepo : RepositoryBase<SubCategory>, ISubCategoryRepo
    {
        public SubCategoryRepo(WarehousingContext context, ILogger<SubCategoryRepo> logger, IConfiguration config): base(context, logger, config){}
    }
}