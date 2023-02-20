using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
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
        public static Session Session;
        [CustomAction]
        public static ActionResult Apply(Session session)
        {
            Session = session;
            session.Log("Begin CustomAction1");
            session.Log("Install type: " + session.Property("INSTALL_TYPE"));
            Program.DestinationPath = Program.DestinationPath.Replace("%ProgramFiles%",Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            session.Log("Install path: " + Program.DestinationPath);
       
            exampleSettings = System.IO.File.ReadAllText(Program.DestinationPath + "\\appsettings.json.example");

            // Replace the appropriate values in the connection string
           
            ReplaceConnectionStringValue("Data Source", Server);
            ReplaceConnectionStringValue("Database", Database);
            if (Username != null && Username != "")
                ReplaceConnectionStringValue("User ID", Username);
            if (Password != null && Password != "")
                ReplaceConnectionStringValue("Password", Password);
            ReplaceStringValue("InstallType", session.Property("INSTALL_TYPE"));
            ReplaceStringValue("ListeningPort", "80");
            if (MessageBox.Show(exampleSettings, "AppSettings.json") == DialogResult.OK)
            {
                // Write the modified appsettings.json file
                System.IO.File.WriteAllText(session.Property("WixSharp_UI_INSTALLDIR") + "appsettings.json", exampleSettings);
            }

            return ActionResult.Success;

        }
        public static void ReplaceConnectionStringValue(string parameterName, string parameterValue)
        {
            Session.Log("Replacing/Inserting Connection string variable: " + parameterName + ": " + parameterValue);

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
        public static void ReplaceStringValue(string parameterName, string parameterValue)
        {
            Session.Log("Replacing/Inserting " + parameterName + ": " + parameterValue);
            //var configJson = System.IO.File.ReadAllText(Program.DestinationPath + "\\appsettings.json.example");
            Session.Log("Before modification" + exampleSettings);
            // return;
             
           
            string connectionString = "";
            string connectionStringPattern = "\""+parameterName+"\":\\s*\"([^\"]*)\"";
            Match match = Regex.Match(exampleSettings, connectionStringPattern);
            if (match.Success)
            {
                connectionString = match.Groups[1].Value;
                exampleSettings = Regex.Replace(exampleSettings, connectionStringPattern, $"\""+parameterName+"\": \""+parameterValue+"\"");

            }
            return;
            string parameterPattern = $@"{parameterName}=(.*?);";

            if (Regex.IsMatch(connectionString, parameterPattern))
            {
                connectionString = Regex.Replace(connectionString, parameterPattern, $"{parameterName}={parameterValue};");
            }
            else
            {
                connectionString += $"{parameterName}={parameterValue};";
            }

            //MessageBox.Show("Replaced variable: " + exampleSettings);

        }
    }
}
