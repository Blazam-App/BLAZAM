using Blazorise;
using Microsoft.AspNetCore.Components;

namespace BLAZAM.Server.Data.Services
{
    /// <summary>
    /// Provides an administrative token. Note that tokens expire
    /// after one hour and are static to the server, meaning every 
    /// user and the server itself gets the same token. This may
    /// be considered a security risk, but the lack of any data
    /// transfer or server control via the RestAPI mean there
    /// is no risk.
    /// 
    /// If a real RestAPI is implemente this will need to change to jwt tokens.
    /// </summary>
    public class AdminTokenService
    {
        private readonly IMessageService _messageService;
        private readonly ApplicationManager _applicationManager;
        private readonly ILogger<AdminTokenService> _logger;

        private static AdminToken token = new AdminToken();

        /// <summary>
        /// Returns either an existing token or creates a new one.
        /// Tokens last one hour before expiring
        /// </summary>
        public static AdminToken Token
        {
            get
            {
                if (token.IsExpired)
                    token = new AdminToken();
                return token;
            }
        }

        public AdminTokenService(ILogger<AdminTokenService> logger, ApplicationManager applicationManager, IMessageService messageService)
        {
            _messageService = messageService;
            _applicationManager = applicationManager;
            _logger = logger;
        }


    }

    /// <summary>
    /// A token to be used to authenticate administrative api
    /// requests to this sever. The primary use case is
    /// for authorizing the self update executable.
    /// </summary>
    public class AdminToken
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        private DateTime expirationTime = DateTime.Now + TimeSpan.FromMinutes(60);
        public bool IsExpired
        {
            get
            {
                return DateTime.Now > expirationTime;
            }
        }

    }
}
