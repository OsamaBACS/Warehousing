using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Classes
{
    public class UserDeviceRepo : RepositoryBase<UserDevice>, IUserDeviceRepo
    {
        private readonly ILogger<UserDeviceRepo> _logger;
        private readonly IConfiguration _config;

        public UserDeviceRepo(WarehousingContext context, ILogger<UserDeviceRepo> logger, IConfiguration config) : base(context, logger, config)
        {
            _logger = logger;
            _config = config;
        }
    }
}