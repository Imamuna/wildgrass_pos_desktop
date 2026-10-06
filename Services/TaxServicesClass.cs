using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using WildGrassPOSLibrary.Models;
using WildGrass_Desktop_f8.Functions;

namespace WildGrass_Desktop_f8.Services
{
    /// <summary>
    /// CRUD Taxes
    /// </summary>
    internal class TaxServicesClass
    {
        //ID's
        string uid = "";
        string bid = "";
        string eid = "";


        int duration = 2000;
        int numberOfTries = 3;

        int totalCount = 0;
        int currentCount = 0;

        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
        DatabaseDirectoryServicesClass DatabaseDirectory = new();
        StandardFirebaseOperationsClass standardFirebaseOperationsClass = new StandardFirebaseOperationsClass();

        IFirebaseConfig ifc = new FirebaseConfig
        {
            AuthSecret = "btiSgvaHMXQWODLMFQSgFBHLJuBykOfzwu9xbfdV",
            BasePath = "https://long-walk-pos.firebaseio.com/"
        };

        IFirebaseClient client;

        private void setup()
        {
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    client = new FireSharp.FirebaseClient(ifc);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
        }

        //Create
        public void CreateTax(TaxClass tax, Action<bool, TaxClass> method, bool update)
        {
            eid = prevelantClass.getEid();
            bool success = false;
            setup();

            //create unique product ID
            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            double time = dt.TimeOfDay.TotalMilliseconds;
            int timeId = Convert.ToInt32(time);
            string id = Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(timeId);

            tax.tax_id = id;

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Taxes()+ tax.tax_id, tax);

                    if (update)
                    {
                        standardFirebaseOperationsClass.UpdateVersion("tax");
                    }
                    standardFirebaseOperationsClass.activityLog(tax.tax_id, "tax", "Created Tax", eid);
                    success = true;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            method(success, tax);
        }


        //Update
        public void UpdateTax(TaxClass tax, Action<bool> method, bool update)
        {
            bool success = false;
            setup();
            eid = prevelantClass.getEid();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse firebase2 = client.Set(DatabaseDirectory.Taxes() + "/" + tax.tax_id, tax);

                    if (update)
                    {
                        standardFirebaseOperationsClass.UpdateVersion("tax");
                    }
                    standardFirebaseOperationsClass.activityLog(tax.tax_id, "tax", "Create Tax", eid);
                    success = true;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            method(success);
        }


        //Delete
        public void DeleteTax(string id, Action<bool> method, bool update)
        {
            eid = prevelantClass.getEid();
            bool success = false;
            setup();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    // remove from products
                    FirebaseResponse response = client.Delete(DatabaseDirectory.Taxes() + "/" + id);

                    //update version
                    standardFirebaseOperationsClass.UpdateVersion("tax");
                    //update activity log
                    if (eid == null)
                    {
                        eid = "1";
                    }
                    else if (eid == "")
                    {
                        eid = "1";
                    }
                    standardFirebaseOperationsClass.activityLog(id, "tax", "Deleted Tax", eid);
                    success = true;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            method(success);
        }
    }
}
