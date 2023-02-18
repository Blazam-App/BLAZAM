using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using WixSharp;
using WixSharp.UI.Forms;

namespace WixSharpSetup
{
    public partial class NetCoreDialog : ManagedForm, IManagedDialog
    {
        public NetCoreDialog()
        {
            //NOTE: If this assembly is compiled for v4.0.30319 runtime, it may not be compatible with the MSI hosted CLR.
            //The incompatibility is particularly possible for the Embedded UI scenarios. 
            //The safest way to avoid the problem is to compile the assembly for v3.5 Target Framework.WixSharp Setup
            InitializeComponent();
        }
        public ManagedForm Host;
        ISession session => Host?.Runtime.Session;

        void dialog_Load(object sender, EventArgs e)
        {
            Host = this;
            if (session.Property("INSTALL_TYPE") == "IIS")
            {
                Shell.GoNext();
            }
            banner.Image = Runtime.Session.GetResourceBitmap("WixUI_Bmp_Banner");
            Text = "[ProductName] Setup";
            next.Enabled= false;
            CheckForAspCore();
            //resolve all Control.Text cases with embedded MSI properties (e.g. 'ProductName') and *.wxl file entries
            base.Localize();
        }

        void back_Click(object sender, EventArgs e)
        {
            Shell.GoTo<InstallationType>();

        }

        void next_Click(object sender, EventArgs e)
        {
            Shell.GoNext();
        }

        void cancel_Click(object sender, EventArgs e)
        {
            Shell.Cancel();
        }
        void CheckForAspCore()
        {
            try
            {
                var dirs = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\dotnet\\shared\\Microsoft.NETCore.App");
                if (dirs != null && dirs.Length > 0)
                {

                    foreach (var dir in dirs)
                    {
                        if (dir.Contains("6."))
                        {
                            OutputBox.Text = "Framework Installed";
                            OutputBox.ForeColor = Color.ForestGreen;
                            next.Enabled = true;

                            return;
                        }
                    }

                }
            } catch { }
        }
        private void checkAgainButton_Click(object sender, EventArgs e)
        {
            CheckForAspCore();

        }

        private void DownloadLink_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://aka.ms/dotnet-download");
        }
    }
}