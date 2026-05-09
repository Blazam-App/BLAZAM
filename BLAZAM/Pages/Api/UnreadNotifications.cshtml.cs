using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Pages.Api
{
    public class UnreadNotificationsModel : PageModel
    {
        private readonly IAppDatabaseFactory _dbFactory;
        private IApplicationUserState? _currentUser;
        private readonly IApplicationUserStateService _userStateService;

        public UnreadNotificationsModel(IAppDatabaseFactory dbFactory, IApplicationUserStateService userStateService)
        {
            _dbFactory = dbFactory;
            _userStateService = userStateService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User != null)
            {
                var user = _userStateService.GetUserState(User);
                _currentUser = user;
            }
            if (_currentUser is null || !_currentUser.IsAuthenticated)
            {
                return new UnauthorizedResult();
            }

            using var context = await _dbFactory.CreateDbContextAsync();

            var notifications = await context.UserNotifications
                .Include(n => n.Notification)
                .Where(n => n.User.Id == _currentUser.Id && !n.IsRead)
                .OrderByDescending(n => n.Notification.Created)
                .ToListAsync();


            return new JsonResult(notifications);
        }
    }
}
