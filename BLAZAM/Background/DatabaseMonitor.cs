

using BLAZAM.Common.Data.Database;
using BLAZAM.Server.Pages.Error;

namespace BLAZAM.Server.Background
{
    public class DatabaseMonitor : ConnectionMonitor
    {
        private DatabaseContext _context;


        public DatabaseMonitor(DatabaseContext context)
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
                case DatabaseContext.ConnectionStatus.OK:
                        Connected = ConnectionState.Up;

                    break;
                case DatabaseContext.ConnectionStatus.ServerUnreachable:

                    Oops.ErrorMessage = "Database server is not reachable! Check your connection string! Is the port open?";
                    goto default;

                case DatabaseContext.ConnectionStatus.IncompleteConfiguration:
                    Oops.ErrorMessage = "Web application configuration is corrupt or missing!";
                    goto default;

                case DatabaseContext.ConnectionStatus.DatabaseConnectionIssue:
                    Oops.ErrorMessage = "Database server is up, but the application is unable to connect to the database!";
                    goto default;
                case DatabaseContext.ConnectionStatus.TablesMissing:
                    Oops.ErrorMessage = "Database is corrupt, or installation was incomplete!";
                    Connected = ConnectionState.Up;

                    break;
                    //goto default;
                default:
                    
                        Connected = ConnectionState.Down;
                    
                    break;
            }
        }
       
    }
}