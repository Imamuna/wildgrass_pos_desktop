using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using System.IO;
using Newtonsoft.Json;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using WildGrass_Desktop;
using WildGrassPOSLibrary.Models;
using System.Windows;
using Firebase.Storage;


using WildGrass_Desktop_f8.Activities.main;
using WildGrass_Desktop_f8.Functions;
using System.Windows.Controls;

namespace WildGrass_Desktop_f8.Services
{
    internal class PhoneBarcodeScannerServicesClass
    {
        //ID's
        string uid = "";
        string bid = "";
        string eid = "";


        int duration = 2000;
        int numberOfTries = 3;

        DatabaseDirectoryServicesClass DatabaseDirectory = new();
        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new ();
        ProductProcessingServicesClass productProcessingServices = new ProductProcessingServicesClass();
        StandardFirebaseOperationsClass standardFirebaseOperationsClass = new StandardFirebaseOperationsClass();

        IFirebaseConfig ifc = new FirebaseConfig
        {
            AuthSecret = "btiSgvaHMXQWODLMFQSgFBHLJuBykOfzwu9xbfdV",
            BasePath = "https://long-walk-pos.firebaseio.com/"
        };

        IFirebaseClient client;

        private void tryAgainSoon()
        {
            Thread.Sleep(4000);

            checkConnection();
        }

        private async void checkConnection()
        {
            ReconilliationClass reconcilliationClass = new();
            bool result = reconcilliationClass.IsConnectedToInternet();
            if (result)
            {
                //create directory
                string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string complete = System.IO.Path.Combine(systemPath, "WildGrass");
                string dir = complete + @"\data\" + uid + @"\businesses\" + bid;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.OnGoing());
                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(dir + @"\OnlineCart.txt", "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(dir + @"\OnlineCart.txt", res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    Thread.Sleep(1000);
                    checkConnection();
                }
                else
                {
                    tryAgainSoon();
                }
            }
            else
            {
                tryAgainSoon();
            }
        }

        private async void ActivateOnlineCart(Button control, Action<Dictionary<string, OnGoingClass>> method)
        {
            ReconilliationClass reconcilliationClass = new();
            bool result = reconcilliationClass.IsConnectedToInternet();
            bool success = false;
            Dictionary<string, OnGoingClass> products = new();
            if (result)
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse res = await client.GetAsync(DatabaseDirectory.OnGoing());
                        products = JsonConvert.DeserializeObject<Dictionary<string, OnGoingClass>>(res.Body.ToString());
                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }

                if (success)
                {
                    method(products);
                    checkConnection();
                } else
                {
                    tryAgainSoon();
                }
            }
            else
            {
                tryAgainSoon();
            }
        }
    }
}
