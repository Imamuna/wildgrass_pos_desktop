using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.IO;
using Newtonsoft.Json;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using WildGrassPOSLibrary.Models;
using System.Windows;
using Firebase.Storage;

using Firebase.Database;
using Firebase.Database.Query;
using System.Windows.Controls;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using WildGrass_Desktop_f8.Activities.dialogs;

namespace WildGrass_Desktop_f8.Services
{
    internal class AppMaintenanceClass
    {
        public int versionNumber = 10;

        //ID's
        string uid = "";
        string bid = "";
        string eid = "";

        UpdateAvailableDialog updateDialog;
        string updateDir = "";


        int duration = 2000;
        int numberOfTries = 3;

        DatabaseDirectoryServicesClass DatabaseDirectory = new();
        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new ();

        IFirebaseConfig ifc = new FirebaseConfig
        {
            AuthSecret = "btiSgvaHMXQWODLMFQSgFBHLJuBykOfzwu9xbfdV",
            BasePath = "https://long-walk-pos.firebaseio.com/"
        };

        IFirebaseClient client;

        private void setup()
        {
            try
            {
                client = new FireSharp.FirebaseClient(ifc);
            }
            catch (Exception)
            {
                //what to do
            }
        }

        public async void downloadBetaVersion(Action<AppVersionClass> method, TextBlock control)
        {

        }

        public async void checkForUpdate(Action<AppVersionClass> method, TextBlock control)
        {
            AppVersionClass stableVersion = new();
            setup();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = await client.GetAsync(DatabaseDirectory.SystemResources() + "/versions/stable");

                    stableVersion = JsonConvert.DeserializeObject<AppVersionClass>(res.Body.ToString());
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            await control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, stableVersion);
        }


        public async void downloadUpdate(UpdateAvailableDialog dialog, string type)
        {
            updateDialog = dialog;
            updateDialog.downloadStarted(); 
            
            
            AppVersionClass stableVersion = new();
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = await client.GetAsync(DatabaseDirectory.SystemResources() + "/versions/" + type);

                    stableVersion = JsonConvert.DeserializeObject<AppVersionClass>(res.Body.ToString());
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            DownloadUpdate(stableVersion.url);
        }

        private void DownloadUpdate(string address)
        {
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            string dir1 = complete + @"\WildGrass Update";
            if (!Directory.Exists(dir1))
            {
                Directory.CreateDirectory(dir1);
            }
            updateDir = dir1 + @"\App.msixbundle";

            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);
            Uri uri = new Uri(address);
            client.DownloadFileAsync(uri, updateDir);
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            updateDialog.downloadProgress(progress);
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            string dir1 = complete + @"\WildGrass Update";
            updateDir = dir1 + @"\App.msixbundle";
            if (e == null) return;
            if (e.Cancelled)
            {
                updateDialog.downloadComplete(false);
                if (e.Equals != null)
                {
                    MessageBox.Show(e.Error.ToString(), "system testing");
                }
            } else
            {
                //we install
                updateDialog.downloadComplete(true);

                //Process.Start("App.msixbundle", updateDir);//maybe we should name msix bundle so that we can check if the latest and version inside match

                Process.Start(new ProcessStartInfo
                {
                    FileName = updateDir,
                    UseShellExecute = true
                });
            }
        }

        private void InstallMSIXBundle()
        {
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            updateDir = complete + @"\App.msixbundle";

            updateDialog.downloadComplete(true);
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(updateDir)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private void Run_scripts()
        {
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            updateDir = complete + @"\App.msixbundle";

            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = $"Add-AppPackage -path \"{updateDir}\"",
                UseShellExecute = false
            };
            var proc = Process.Start(startInfo);
            proc.Exited += OnProcessExited;
        }

        private void OnProcessExited(object sender, EventArgs eventArgs)
        {
            // todo, e.g.
            // System.Windows.Application.Current.Shutdown();
        }
    }
}
