using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using WixSharp;
using WixSharpSetup;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Setup
{
    public static class ApplyAppSettingsAction
    {
        
        public static string Username="";
        public static string Password="";
        public static string Database="";
        public static string Server="";
        public static string Port="";
        public static string Custom = "";
        private static  string exampleSettings;
        [CustomAction]
        public static ActionResult Apply(Session session)
        {
            session.Log("Begin CustomAction1");
            session.Log("Install type: " + session.Property("INSTALL_TYPE"));
            session.Log("Install path: " + session.Property("WixSharp_UI_INSTALLDIR"));

            exampleSettings = System.IO.File.ReadAllText(session.Property("WixSharp_UI_INSTALLDIR") + "\\appsettings.json.example");

            // Replace the appropriate values in the connection string
           
            ReplaceConnectionStringValue("Data Source", Server);
            ReplaceConnectionStringValue("Database", Database);
            if (Username != null && Username != "")
                ReplaceConnectionStringValue("User ID", Username);
            if (Password != null && Password != "")
                ReplaceConnectionStringValue("Password", Password);

            if (MessageBox.Show(exampleSettings, "AppSettings.json") == DialogResult.OK)
            {
                // Write the modified appsettings.json file
                System.IO.File.WriteAllText(session.Property("WixSharp_UI_INSTALLDIR") + "appsettings.json", exampleSettings);
            }

            return ActionResult.Success;

        }
        public static void ReplaceConnectionStringValue(string parameterName, string parameterValue)
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
