using BLAZAM.Database.Models.User;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BLAZAM.Pages.Api
{
    public class UnreadNotificationsModel : PageModel
    {
        private readonly IAppDatabaseFactory _dbFactory;
        private readonly ICurrentUserStateService _currentUser;

        public UnreadNotificationsModel(IAppDatabaseFactory dbFactory, ICurrentUserStateService currentUser)
        {
            _dbFactory = dbFactory;
            _currentUser = currentUser;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (_currentUser.State is null)
            {
                return new UnauthorizedResult();
            }

            var context = await _dbFactory.CreateDbContextAsync();
            var notifications = await context.UserNotifications
                .Where(n => n.User.Id == _currentUser.State.Id && !n.IsRead)
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(notifications, serializerSettings);
        }
    }
}
