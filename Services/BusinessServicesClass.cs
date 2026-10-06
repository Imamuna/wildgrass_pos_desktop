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
using WildGrass_Desktop;
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
    /// CRUD Businesses
    /// </summary>
    internal class BusinessServicesClass
    {
        //ID's
        string uid = "";
        string bid = "";
        string eid = "";


        int duration = 2000;
        int numberOfTries = 3;

        DatabaseDirectoryServicesClass DatabaseDirectory = new();
        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
        StandardFirebaseOperationsClass standardFirebaseOperationsClass = new();

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
        public void createBusiness(BusinessClass business, TextBox control, Action<bool> method)
        {
            setup();
            uid = prevelantClass.getUid();

            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {//create unique product ID
                DateTime dt = DateTime.Now;
                int month = dt.Month;
                int year = dt.Year;
                int daySele = dt.Day;
                double time = dt.TimeOfDay.TotalMilliseconds;
                int timeId = Convert.ToInt32(time);
                string id = "WG" + Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(timeId) + "B";
                business.bid = id;

                Dictionary<string, BusinessClass> businessesArray = new();
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse res44 = client.Get(DatabaseDirectory.Businesses());
                        businessesArray = JsonConvert.DeserializeObject<Dictionary<string, BusinessClass>>(res44.Body.ToString());
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }


                int checker = 0;
                if (businessesArray != null) if (businessesArray.Count > 0)
                    {
                        foreach (var item in businessesArray)
                        {
                            if(item.Value.owner == uid)
                            {
                                if (item.Value.businessTitle == business.businessTitle)
                                {
                                    checker++;
                                }
                            }
                        }
                    }

                if (checker == 0)
                {
                    string vid = "create and dream";
                    VersionClass versionArray = new VersionClass()
                    {
                        receipt = vid,
                        invoice = vid,
                        quotation = vid,
                        product = vid,
                        tracking = vid,
                        settings = vid,
                        user = vid,
                        employee = vid,
                        order = vid,
                        expense = vid,
                        tax = vid,
                        transferNote = vid,
                        business = vid,
                        receiptIn = vid,
                        invoiceIn = vid,
                        supplier = vid,
                        customer = vid
                    };

                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            FirebaseResponse res = client.Set(DatabaseDirectory.Businesses() + "/" + business.bid, business);
                            FirebaseResponse res2 = client.Set(DatabaseDirectory.Businesses() + "/" + business.bid + "/Versions/Current", versionArray);

                            success = true;
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                    standardFirebaseOperationsClass.UpdateVersion("business");
                    standardFirebaseOperationsClass.activityLog(bid, "management", "Created Business", eid);
                }
                else
                {
                    MessageBox.Show("The business name and local area entered already exist in your database");
                }
            }
            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void createShell(BusinessClass business, TextBox control, Action<bool> method)
        {
            setup();
            bool success = false;
            uid = prevelantClass.getUid();

            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            double time = dt.TimeOfDay.TotalMilliseconds;
            int timeId = Convert.ToInt32(time);
            string id = "WG" + Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(timeId) + "B";
            business.bid = id;
            business.owner = uid;

            Dictionary<string, BusinessClass> businessesArray = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res44 = client.Get(DatabaseDirectory.Businesses());
                    businessesArray = JsonConvert.DeserializeObject<Dictionary<string, BusinessClass>>(res44.Body.ToString());
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }


            int checker = 0;
            if (businessesArray != null) if (businessesArray.Count > 0)
                {
                    foreach (var item in businessesArray)
                    {
                        if (item.Value.owner == uid)
                        {
                            if (item.Value.businessTitle == business.businessTitle)
                            {
                                checker++;
                            }
                        }
                    }
                }

            if (checker == 0)
            {
                string vid = "create and dream";
                VersionClass versionArray = new VersionClass()
                {
                    receipt = vid,
                    invoice = vid,
                    quotation = vid,
                    product = vid,
                    tracking = vid,
                    settings = vid,
                    user = vid,
                    employee = vid,
                    order = vid,
                    expense = vid,
                    tax = vid,
                    transferNote = vid,
                    business = vid,
                    receiptIn = vid,
                    invoiceIn = vid,
                    supplier = vid,
                    customer = vid
                };

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse res = client.Set(DatabaseDirectory.Businesses() + "/" + business.bid, business);
                        FirebaseResponse res2 = client.Set(DatabaseDirectory.Businesses() + "/" + business.bid + "/Versions/Current", versionArray);

                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
                standardFirebaseOperationsClass.UpdateVersion("business");
                standardFirebaseOperationsClass.activityLog(bid, "management", "Created Business", eid);
            }
            else
            {
                MessageBox.Show("The business name and overview name entered already exist in your database");
            }

            method(success);
        }

        public void convertToShell(BusinessClass business, string branch, Action<bool> method)
        {
            setup();
            bool success = false;
            uid = prevelantClass.getUid();

            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            double time = dt.TimeOfDay.TotalMilliseconds;
            int timeId = Convert.ToInt32(time);
            string id = "WG" + Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(timeId) + "B";
            business.bid = id;
            business.owner = uid;

            Dictionary<string, BusinessClass> businessesArray = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res44 = client.Get(DatabaseDirectory.Businesses());
                    businessesArray = JsonConvert.DeserializeObject<Dictionary<string, BusinessClass>>(res44.Body.ToString());
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }


            int checker = 0;
            if (businessesArray != null) if (businessesArray.Count > 0)
                {
                    foreach (var item in businessesArray)
                    {
                        if (item.Value.owner == uid)
                        {
                            if (item.Value.businessTitle == business.businessTitle)
                            {
                                checker++;
                            }
                        }
                    }
                }

            if (checker == 0)
            {
                string vid = "create and dream";
                VersionClass versionArray = new VersionClass()
                {
                    receipt = vid,
                    invoice = vid,
                    quotation = vid,
                    product = vid,
                    tracking = vid,
                    settings = vid,
                    user = vid,
                    employee = vid,
                    order = vid,
                    expense = vid,
                    tax = vid,
                    transferNote = vid,
                    business = vid,
                    receiptIn = vid,
                    invoiceIn = vid,
                    supplier = vid,
                    customer = vid
                };

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse res = client.Set(DatabaseDirectory.Businesses() + "/" + business.bid, business);
                        FirebaseResponse res2 = client.Set(DatabaseDirectory.Businesses() + "/" + business.bid + "/Versions/Current", versionArray);
                        FirebaseResponse res3 = client.Set(DatabaseDirectory.Businesses() + "/" + branch + "/owner", business.bid);
                        FirebaseResponse res4 = client.Set(DatabaseDirectory.Businesses() + "/" + branch + "/setAsBranch", true);
                        FirebaseResponse res5 = client.Set(DatabaseDirectory.Businesses() + "/" + branch + "/branchNo", 1);

                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
                standardFirebaseOperationsClass.UpdateVersion("business");
                standardFirebaseOperationsClass.activityLog(bid, "management", "Created Shell Business", eid);
            }
            else
            {
                MessageBox.Show("The business name and overview name entered already exist in your database");
            }

            method(success);
        }

        public void setAsBranch(string business_id, string shell_id, Action<bool> method, bool update)
        {
            setup();
            bool success = false;
            uid = prevelantClass.getUid();

            Dictionary<string, BusinessClass> businessesArray = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res44 = client.Get(DatabaseDirectory.Businesses());
                    businessesArray = JsonConvert.DeserializeObject<Dictionary<string, BusinessClass>>(res44.Body.ToString());
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            if (businessesArray != null) if (businessesArray.ContainsKey(shell_id) && businessesArray.ContainsKey(business_id))
                {
                    int count = 1;
                    foreach(var item in businessesArray)
                    {
                        if(item.Value.owner == shell_id)
                        {
                            count++;
                        }
                    }

                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            FirebaseResponse res3 = client.Set(DatabaseDirectory.Businesses() + "/" + business_id + "/owner", shell_id);
                            FirebaseResponse res4 = client.Set(DatabaseDirectory.Businesses() + "/" + business_id + "/setAsBranch", true);
                            FirebaseResponse res5 = client.Set(DatabaseDirectory.Businesses() + "/" + business_id + "/branchNo", count);

                            standardFirebaseOperationsClass.UpdateVersion("business", shell_id);
                            standardFirebaseOperationsClass.UpdateVersion("business", business_id);
                            standardFirebaseOperationsClass.activityLog(bid, "management", "Convert to Branch", eid);
                            success = true;
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                }
        }

        public void editBusiness(BusinessClass business, TextBox control, Action<bool> method)
        {
            setup();
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        // depracated directories
                        FirebaseResponse res0 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/businessName", business.businessName);
                        FirebaseResponse res2 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/businessTitle", business.businessTitle);
                        FirebaseResponse res4 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/businessEmail", business.businessEmail);
                        FirebaseResponse res6 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/localArea", business.localArea);
                        FirebaseResponse res8 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/address", business.address);
                        FirebaseResponse res10 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/businessPhone", business.businessPhone);
                        FirebaseResponse res12 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/storetype", business.storetype);
                        FirebaseResponse res14 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/tPin", business.tPin);
                        FirebaseResponse res16 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/pacraReg", business.pacraReg);
                        FirebaseResponse re4 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankName", business.bankName);
                        FirebaseResponse re6 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankBranchName", business.bankBranchName);
                        FirebaseResponse re8 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankBranchCode", business.bankBranchCode);
                        FirebaseResponse re10 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankAccountName", business.bankAccountName);
                        FirebaseResponse re2 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankAccountNo", business.bankAccountNo);
                        FirebaseResponse re14 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankSortCode", business.bankSortCode);
                        FirebaseResponse re16 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankSwiftCode", business.bankSwiftCode);

                        FirebaseResponse res1 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/businessName", business.businessName);
                        FirebaseResponse res3 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/businessTitle", business.businessTitle);
                        FirebaseResponse res5 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/businessEmail", business.businessEmail);
                        FirebaseResponse res7 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/localArea", business.localArea);
                        FirebaseResponse res9 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/address", business.address);
                        FirebaseResponse res11 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/businessPhone", business.businessPhone);
                        FirebaseResponse res13 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/storetype", business.storetype);
                        FirebaseResponse res15 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/tPin", business.tPin);
                        FirebaseResponse res17 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/pacraReg", business.pacraReg);
                        FirebaseResponse re5 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankName", business.bankName);
                        FirebaseResponse re7 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankBranchName", business.bankBranchName);
                        FirebaseResponse re9 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankBranchCode", business.bankBranchCode);
                        FirebaseResponse re11 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankAccountName", business.bankAccountName);
                        FirebaseResponse re13 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankAccountNo", business.bankAccountNo);
                        FirebaseResponse re15 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankSortCode", business.bankSortCode);
                        FirebaseResponse re17 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankSwiftCode", business.bankSwiftCode);


                        standardFirebaseOperationsClass.UpdateVersion("business");
                        standardFirebaseOperationsClass.activityLog(bid, "management", "Business Details Edit", eid);
                        success = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(duration);
                        MessageBox.Show(ex.Message, "WildGrass: system testing");
                    }
                }
            }
            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        //Update

        public void uploadBankDetails(string bankName, string bankBranchName, string bankBranchCode, string bankAccountName, string bankAccountNo, string bankSortCode, string bankSwiftCode, TextBox control, Action<bool> method)
        {
            setup();
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse re4 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankName", bankName);
                        FirebaseResponse re6 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankBranchName", bankBranchName);
                        FirebaseResponse re8 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankBranchCode", bankBranchCode);
                        FirebaseResponse re10 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankAccountName", bankAccountName);
                        FirebaseResponse re2 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankAccountNo", bankAccountNo);
                        FirebaseResponse re14 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankSortCode", bankSortCode);
                        FirebaseResponse re16 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/bankSwiftCode", bankSwiftCode);
                        FirebaseResponse re5 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankName", bankName);
                        FirebaseResponse re7 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankBranchName", bankBranchName);
                        FirebaseResponse re9 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankBranchCode", bankBranchCode);
                        FirebaseResponse re11 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankAccountName", bankAccountName);
                        FirebaseResponse re13 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankAccountNo", bankAccountNo);
                        FirebaseResponse re15 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankSortCode", bankSortCode);
                        FirebaseResponse re17 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/bankSwiftCode", bankSwiftCode);

                        standardFirebaseOperationsClass.UpdateVersion("business");
                        standardFirebaseOperationsClass.activityLog(bid, "management", "Bank Details Edit", eid);
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

        public async void uploadLogo(string dirLogo, string imageType, Button control, Action<bool> method)
        {
            bool success = false;
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();

                string logoLink = "";
                if (File.Exists(dirLogo))
                {
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            var stream = File.Open(dirLogo, FileMode.Open);

                            // Construct FirebaseStorage, path to where you want to upload the file and Put it there
                            var task = new FirebaseStorage("long-walk-pos.appspot.com")
                                .Child("users")
                                .Child(uid)
                                .Child(bid)
                                .Child("logo" + imageType)
                                .PutAsync(stream);

                            // Track progress of the upload
                            task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                            // await the task to wait until upload completes and get the download url
                            var downloadUrl = await task;
                            logoLink = downloadUrl;

                            //upload link to firebase directory
                            FirebaseResponse res = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/logoUrl", logoLink);
                            FirebaseResponse res1 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/logoUrl", logoLink);
                            standardFirebaseOperationsClass.UpdateVersion("business");
                            standardFirebaseOperationsClass.activityLog(bid, "management", "Business Details Edit", eid);
                            standardFirebaseOperationsClass.DownloadResources();
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

        public void DownloadLogo()
        {
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            BusinessClass currentBusiness = prevelantClass.GetBusiness();

            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            string specificDir = complete + @"\data\" + uid + @"\businesses\" + bid + @"\pdf resources";
            if (!Directory.Exists(specificDir))
            {
                Directory.CreateDirectory(specificDir);
            }
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            if (result)
            {
                int duration = 2000;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        if (File.Exists(specificDir + @"\logo1.png"))
                        {
                            File.Delete(specificDir + @"\logo1.png");
                        }
                        standardFirebaseOperationsClass.downResource(currentBusiness.logoUrl, specificDir + @"\logo1.png");
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
        }
    }
}
