using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace BLAZAM.Server
{
    [RunInstaller(true)]
    public class ApplicationInstaller : Installer
    {
        private string exampleSettings;

        private string Path { get; set; }
        protected override void OnAfterInstall(IDictionary savedState)
        {

            base.OnAfterInstall(savedState);
            Path = Context.Parameters["path"];
            //throw new InstallException(path);
            var server = Context.Parameters["server"];
            var database = Context.Parameters["database"];
            var username = Context.Parameters["username"];
            var password = Context.Parameters["password"];
          
                exampleSettings = File.ReadAllText(Path + "\\appsettings.json.example");
         
                    // Replace the appropriate values in the connection string
                    string pattern = @"Data Source=(?<server>.*);Database=(?<database>.*);(.*;Integrated Security=False;User ID=(?<username>.*);Password=(?<password>.*);|.*;Persist Security Info=True;Integrated Security=False;Connection Timeout=10;TrustServerCertificate=True;)";
                    string replacement = $"Data Source={server};Database={database};Persist Security Info=True;Integrated Security=False;User ID={username};Password={password};Connection Timeout=10;TrustServerCertificate=True;";
                    string modifiedAppsettingsJson = Regex.Replace(exampleSettings, pattern, replacement);

                    ReplaceConnectionStringValue("Data Source", server);
                    ReplaceConnectionStringValue("Database", database);
                    if (username != null && username != "")
                        ReplaceConnectionStringValue("User ID", username);
                    if (password != null && password != "")
                        ReplaceConnectionStringValue("Password", password);

                    if (MessageBox.Show(exampleSettings, "AppSettings.json") == DialogResult.OK)
                    {
                        // Write the modified appsettings.json file
                        File.WriteAllText(Path + "appsettings.json", exampleSettings);
                    }
               

        }
        public void ReplaceConnectionStringValue(string parameterName, string parameterValue)
        {
            string connectionString = "";
            string connectionStringPattern = "\"SQLConnectionString\":\\s*\"([^\"]*)\"";
            Match match = Regex.Match(exampleSettings, connectionStringPattern);
            if (match.Success)
            {
                connectionString = match.Groups[1].Value;
            }

            string parameterPattern = $@"{parameterName}=(.*?);";

            if (Regex.IsMatch(connectionString, parameterPattern))
            {
                connectionString = Regex.Replace(connectionString, parameterPattern, $"{parameterName}={parameterValue};");
            }
            else
            {
                connectionString += $"{parameterName}={parameterValue};";
            }

            exampleSettings = Regex.Replace(exampleSettings, connectionStringPattern, $"\"SQLConnectionString\": \"{connectionString}\"");
            //MessageBox.Show("Replaced variable: " + exampleSettings);

        }
    }
}
