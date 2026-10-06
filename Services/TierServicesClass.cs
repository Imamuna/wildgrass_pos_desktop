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
using WildGrass_Desktop;
using WildGrassPOSLibrary.Models;
using System.Windows;
using Firebase.Storage;

using Firebase.Database;
using Firebase.Database.Query;
using System.Windows.Controls;
using System.Net;
using System.Threading;
using WildGrass_Desktop_f8.Activities.main;
using Spire.Pdf;
using WildGrass_Desktop_f8.Functions;

namespace WildGrass_Desktop_f8.Services
{
    /// <summary>
    /// CRUD Tier
    /// </summary>
    internal class TierServicesClass
    {
        //ID's
        string uid = "";
        string bid = "";
        string eid = "";


        int duration = 2000;
        int numberOfTries = 3;

        int totalCount = 0;
        int currentCount = 0;

        DatabaseDirectoryServicesClass DatabaseDirectory = new();
        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
        CategoryServicesClass categoryServices = new();
        StandardFirebaseOperationsClass standardFirebaseOperationsClass = new StandardFirebaseOperationsClass();

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


        //Create
        public void createTier(TierClass tier, Button control, Action<bool> method, bool update)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();
                eid = prevelantClass.getEid();

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Tiers() + "/" + tier.tid, tier);
                        if (update)
                        {
                            standardFirebaseOperationsClass.UpdateVersion("product");
                        }
                        standardFirebaseOperationsClass.activityLog(tier.name, "tier", "Created Tier", eid);
                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        //Retrive

        //Update
        public void editTier(TierClass tier, Button control, Action<bool> method, bool update)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();
                eid = prevelantClass.getEid();

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Tiers() + "/" + tier.tid, tier);
                        if (update)
                        {
                            standardFirebaseOperationsClass.UpdateVersion("product");
                        }
                        standardFirebaseOperationsClass.activityLog(tier.name, "tier", "Updated Tier", eid);
                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        //Delete
        public void DeleteTier(string id, TextBox control, Action<bool> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();
                eid = prevelantClass.getEid();

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        // remove from products
                        FirebaseResponse response = client.Delete(DatabaseDirectory.Tiers() + "/" + id);

                        //update version
                        standardFirebaseOperationsClass.UpdateVersion("product");
                        //update activity log
                        if (eid == null)
                        {
                            eid = "1";
                        }
                        else if (eid == "")
                        {
                            eid = "1";
                        }
                        standardFirebaseOperationsClass.activityLog(id, "product", "Deleted Tier", eid);
                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }

            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }
    }
}
