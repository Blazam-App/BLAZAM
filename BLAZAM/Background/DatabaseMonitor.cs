

using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Server.Pages.Error;

namespace BLAZAM.Server.Background
{
    public class DatabaseMonitor : ConnectionMonitor
    {
        private  IDatabaseContext _context;


        public DatabaseMonitor(IDatabaseContext context)
        {
            Interval = 10000;
            _context = context;
            Task.Delay(1000).ContinueWith((oob) => { Tick(null); });
            Monitor();
        }

        protected override void Tick(object? state)
        {
            switch (_context.Status)
            {
                case DatabaseContext.DatabaseStatus.OK:
                        Status = ServiceConnectionState.Up;

                    break;
                case DatabaseContext.DatabaseStatus.ServerUnreachable:

                    Oops.ErrorMessage = "Database server is not reachable! Check your connection string! Is the port open?";
                    goto default;

                case DatabaseContext.DatabaseStatus.IncompleteConfiguration:
                    Oops.ErrorMessage = "Web application configuration is corrupt or missing!";
                    goto default;

                case DatabaseContext.DatabaseStatus.DatabaseConnectionIssue:
                    Oops.ErrorMessage = "Database server is up, but the application is unable to connect to the database!";
                    goto default;
                case DatabaseContext.DatabaseStatus.TablesMissing:
                    Oops.ErrorMessage = "Database is corrupt, or installation was incomplete!";
                    Status = ServiceConnectionState.Up;

                    break;
                    //goto default;
                default:
                    
                        Status = ServiceConnectionState.Down;
                    
                    break;
            }
        }
       
    }
}