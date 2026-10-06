using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.NetworkInformation;
using System.IO;
using Newtonsoft.Json;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using System.Windows.Threading;
using WildGrass_Desktop_f8.Activities.main;
using System.Threading;
using WildGrass_Desktop_f8.Functions;
using System.Windows;
using WildGrassPOSLibrary.Models;
using WildGrass_Desktop_f8.Services;
using WildGrass_Desktop;
using System.Windows.Controls;

namespace WildGrass_Desktop_f8.Services
{
    internal class OverviewReconciliationServicesClass
    {
        bool businessYes;
        bool userYes;


        string uid;
        string bid;

        int duration = 2000;
        int numberOfTries = 3;

        Action showOnline;
        Action showOffline;
        Action loadUI;
        Action<string> currentlyRunning;
        Button control;
        CancellationToken cancelToken;
        IFirebaseClient client;
        DatabaseDirectoryServicesClass DatabaseDirectory;
        WildGrassPOSLibrary.Services.LocalDirectoryServicesClass localDirectory;
        ReconilliationClass reconilliationClass;
        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass;
        FirebaseResponse currentFirebaseVersionsRes;


        public OverviewReconciliationServicesClass(Action showOnline, Action showOffline, Action loadUI, Action<string> currenctlyLoading, Button control, CancellationToken cancelToken)
        {
            reconilliationClass = new();
            prevelantClass = new();
            DatabaseDirectory = new();
            localDirectory = new();
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();

            IFirebaseConfig ifc = new FirebaseConfig
            {
                AuthSecret = "btiSgvaHMXQWODLMFQSgFBHLJuBykOfzwu9xbfdV",
                BasePath = "https://long-walk-pos.firebaseio.com/"
            };

            try
            {
                client = new FireSharp.FirebaseClient(ifc);
            }
            catch (Exception)
            {

            }

            this.showOnline = showOnline;
            this.showOffline = showOffline;
            this.loadUI = loadUI;
            this.control = control;
            currentlyRunning = currenctlyLoading;
            this.cancelToken = cancelToken;
        }

        public void checkConnection()
        {
            if (cancelToken.IsCancellationRequested)
            {
                //cancelToken.ThrowIfCancellationRequested();
            }
            else
            {
                bool result = reconilliationClass.IsConnectedToInternet();

                if (result)
                {
                    control.Dispatcher.BeginInvoke(
                           System.Windows.Threading.DispatcherPriority.Normal,
                           showOnline);

                    ResolveQueuedData();
                    checkVersions();
                }
                else
                {
                    control.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        showOffline);
                    tryAgainSoon();
                }
            }
        }

        private void ResolveQueuedData()
        {

        }

        private async void checkVersions()
        {
            userYes = false;
            businessYes = false;


            if (File.Exists(localDirectory.MasterVersions()))
            {
                FirebaseResponse res;
                try
                {
                    res = await client.GetAsync(DatabaseDirectory.UsersVersions());

                }
                catch (Exception e)
                {
                    res = null;
                }

                if (res != null)
                {
                    if (res.Body.ToString() != "null")
                    {
                        Dictionary<string, VersionClass> versionsArray = JsonConvert.DeserializeObject<Dictionary<string, VersionClass>>(res.Body.ToString());
                        Dictionary<string, VersionClass> versionsArrayLocal = null;
                        for (int i = 0; i < 3; i++)
                        {
                            try
                            {
                                using (StreamReader r = new StreamReader(localDirectory.MasterVersions()))
                                {
                                    string json = r.ReadToEnd();
                                    versionsArrayLocal = JsonConvert.DeserializeObject<Dictionary<string, VersionClass>>(json);

                                    if (versionsArrayLocal != null)
                                    {
                                        //if any of them is different call the cavary
                                        if (versionsArray["Current"].user != null)
                                        {
                                            if (versionsArray["Current"].user != versionsArrayLocal["Current"].user)
                                            {
                                                userYes = true;
                                            }
                                        }
                                        if (versionsArray["Current"].business != null)
                                        {
                                            if (versionsArray["Current"].business != versionsArrayLocal["Current"].business)
                                            {
                                                businessYes = true;
                                            }
                                        }
                                    }
                                }
                                if (versionsArrayLocal == null)
                                {
                                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Versions());

                                    string dir = localDirectory.UserFolder();
                                    if (!Directory.Exists(dir))
                                    {
                                        Directory.CreateDirectory(dir);
                                    }
                                    currentFirebaseVersionsRes = res1;
                                    allBusinesses();
                                }

                                if (userYes || businessYes)
                                {
                                    currentFirebaseVersionsRes = res;
                                    allBusinesses();
                                }
                                else
                                {
                                    checkConnection();
                                }
                                break;
                            }
                            catch (Exception)
                            {
                                tryAgainSoon();
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(8000);
                        tryAgainSoon();
                    }
                }
                else
                {
                    Thread.Sleep(8000);
                    tryAgainSoon();
                }
            }
            else
            {
                FirebaseResponse res = await client.GetAsync(DatabaseDirectory.UsersVersions());

                if (res != null)
                {
                    if (res.Body.ToString() != "null")
                    {
                        string dir = localDirectory.UserFolder();
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        System.IO.File.WriteAllText(dir + @"\Versions.txt", res.Body.ToString());
                        userYes = true;
                        businessYes = true;
                        allBusinesses();
                    }
                    else
                    {
                        Thread.Sleep(8000);
                        tryAgainSoon();
                    }
                }
                else
                {
                    Thread.Sleep(8000);
                    tryAgainSoon();
                }
            }
        }

        private void allBusinesses()
        {
            control.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    currentlyRunning, "Loading");
            if (businessYes || userYes)
            {
                downloadUser(makeCall);
            }
            else
            {
                makeCall();
            }
        }

        private async void downloadUser(Action method)
        {
            bool result = reconilliationClass.IsConnectedToInternet();
            if (result)
            {
                //standard
                FirebaseResponse res2 = null;
                FirebaseResponse res = null;
                try
                {
                    res = await client.GetAsync(DatabaseDirectory.Users() + "/" + uid);//this is a no brainer
                }
                catch (Exception ex)
                {

                }

                try
                {
                    res2 = await client.GetAsync(DatabaseDirectory.Users());//this one is the real stress, and will only get worse with time
                }
                catch (Exception ex)
                {

                }

                if (res != null && res2 != null)
                {
                    //create directory
                    try
                    {
                        string dir1 = localDirectory.UserFolder();
                        string url = localDirectory.UserData();
                        string url1 = localDirectory.User();
                        if (!Directory.Exists(dir1))
                        {
                            Directory.CreateDirectory(dir1);
                        }

                        if (res2.Body.ToString() == "null")
                        {
                            System.IO.File.WriteAllText(url, "");
                        }
                        else
                        {
                            System.IO.File.WriteAllText(url, res2.Body.ToString());
                        }

                        if (res.Body.ToString() == "null")
                        {
                            System.IO.File.WriteAllText(url1, "");
                        }
                        else
                        {
                            System.IO.File.WriteAllText(url1, res.Body.ToString());
                        }

                        method();
                    }
                    catch (Exception)
                    {
                        tryAgainSoon();
                    }
                }

                //sandbox
                FirebaseResponse res3 = null;
                FirebaseResponse res4 = null;
                try
                {
                    res3 = await client.GetAsync(DatabaseDirectory.SandboxUsers() + "/" + uid);//this is a no brainer
                }
                catch (Exception ex)
                {

                }

                try
                {
                    res4 = await client.GetAsync(DatabaseDirectory.SandboxUsers());//this one is the real stress, and will only get worse with time
                }
                catch (Exception ex)
                {

                }

                if (res3 != null && res4 != null)
                {
                    //create directory
                    try
                    {
                        string dir1 = localDirectory.SandboxUserFolder();
                        string url = localDirectory.SandboxUserData();
                        string url1 = localDirectory.SandboxUser();
                        if (!Directory.Exists(dir1))
                        {
                            Directory.CreateDirectory(dir1);
                        }

                        if (res4.Body.ToString() == "null")
                        {
                            System.IO.File.WriteAllText(url, "");
                        }
                        else
                        {
                            System.IO.File.WriteAllText(url, res4.Body.ToString());
                        }

                        if (res3.Body.ToString() == "null")
                        {
                            System.IO.File.WriteAllText(url1, "");
                        }
                        else
                        {
                            System.IO.File.WriteAllText(url1, res3.Body.ToString());
                        }

                        method();
                    }
                    catch (Exception)
                    {
                        tryAgainSoon();
                    }
                }
            }
            else
            {
                checkConnection();
            }
        }

        private void makeCall()
        {   //we only update versions when, they have been updated successfully
            control.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    loadUI);
            string dir = localDirectory.UserFolder();
            if (currentFirebaseVersionsRes != null)
            {
                try
                {
                    System.IO.File.WriteAllText(dir + @"\Versions.txt", currentFirebaseVersionsRes.Body.ToString());
                }
                catch (Exception)
                {
                    Thread.Sleep(5000);
                    try
                    {
                        System.IO.File.WriteAllText(dir + @"\Versions.txt", currentFirebaseVersionsRes.Body.ToString());
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(8000);
                        try
                        {
                            System.IO.File.WriteAllText(dir + @"\Versions.txt", currentFirebaseVersionsRes.Body.ToString());
                        }
                        catch (Exception)
                        {
                            //tryAgainSoon();
                        }
                    }
                }
                checkConnection();
            }
            else
            {
                tryAgainSoon();
            }
        }

        private void tryAgainSoon()
        {
            Thread.Sleep(4000);

            checkConnection();
        }

        public async void updateDatabaseDetails(Action<string> method, Action<string> methodDone)
        {
            bool result = reconilliationClass.IsConnectedToInternet();
            if (result)
            {
                bool success = false;
                for(int i = 0; i < 3; i++)
                {
                    try
                    {
                        FirebaseResponse res = await client.GetAsync(DatabaseDirectory.Businesses());
                        Dictionary<string, BusinessClass> businesses = JsonConvert.DeserializeObject<Dictionary<string, BusinessClass>>(res.Body.ToString());
                        if (businesses == null)
                        {
                            FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Businesses_old());
                            Dictionary<string, BusinessClass> businessesOld = JsonConvert.DeserializeObject<Dictionary<string, BusinessClass>>(res1.Body.ToString());
                            if (businessesOld != null) if (businessesOld.Count > 0)
                                {
                                    success = true;
                                }
                        }
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(1000);
                    }
                }
                if (success)
                {
                    await control.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        method, "open");
                    ReSetMyDatabaseDetails(methodDone);
                }
            }
            else
            {
                tryAgainSoon(method, methodDone);
                //we keep trying indefinetly
                //bring dialog to show no internet and the option to try the function again when internet connection is available
            }
        }

        public async void ReSetMyDatabaseDetails(Action<string> method)
        {
            bool success1 = false;
            bool success2 = false;

            //business details
            Dictionary<string, BusinessClass> businesses = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.FPCSBusinesses_old());
                    businesses = JsonConvert.DeserializeObject<Dictionary<string, BusinessClass>>(res1.Body.ToString());

                    foreach (var item in businesses)
                    {
                        //lets set the subscription account of each business
                        int vday = Convert.ToInt32(item.Value.validUntilDay);
                        int vmonth = Convert.ToInt32(item.Value.validUntilMonth);
                        int vyear = Convert.ToInt32(item.Value.validUntilYear);

                        string vdate = "";
                        try
                        {
                            DateTime dt = new DateTime(vyear, vmonth, vday);
                            vdate = dt.ToString("MMM, dd yyyy");
                        }
                        catch (Exception)
                        {

                        }

                        SubscriptionAccountClass subscriptionAccount = new()
                        {
                            maxProducts = 100,
                            maxEmployees = 0,
                            maxRetailing = 100,
                            maxBillEntries = 100,
                            validUntilDay = vday,
                            validUntilMonth = vmonth,
                            validUntilYear = vyear,
                            validUntilDate = vdate,
                            serialYes = false,
                            expiryYes = false,
                            payrollYes = false,
                            accountingModuleYes = false,
                            sdCustomizationYes = false,
                            isShell = false,
                            branchEnabled = false,
                            packageCode = 0,
                            desktopYes = false,
                            maxBranches = 0,
                            tierCounterYes = false,
                            packageName = "Compact"
                        };
                        switch (item.Value.package)
                        {
                            case 0:
                                subscriptionAccount = new()
                                {
                                    maxProducts = 100,
                                    maxEmployees = 0,
                                    maxRetailing = 100,
                                    maxBillEntries = 100,
                                    validUntilDay = vday,
                                    validUntilMonth = vmonth,
                                    validUntilYear = vyear,
                                    validUntilDate = vdate,
                                    serialYes = false,
                                    expiryYes = false,
                                    payrollYes = false,
                                    accountingModuleYes = false,
                                    sdCustomizationYes = false,
                                    isShell = false,
                                    branchEnabled = false,
                                    packageCode = 0,
                                    desktopYes = false,
                                    maxBranches = 0,
                                    tierCounterYes = false,
                                    packageName = "Compact"
                                };
                                break;
                            case 1:
                                subscriptionAccount = new()
                                {
                                    maxProducts = 500,
                                    maxEmployees = 0,
                                    maxRetailing = 100,
                                    maxBillEntries = 100,
                                    validUntilDay = vday,
                                    validUntilMonth = vmonth,
                                    validUntilYear = vyear,
                                    validUntilDate = vdate,
                                    serialYes = false,
                                    expiryYes = false,
                                    payrollYes = false,
                                    accountingModuleYes = true,
                                    sdCustomizationYes = true,
                                    isShell = false,//this variable is now useless
                                    branchEnabled = true,
                                    desktopYes = true,
                                    maxBranches = 3,
                                    tierCounterYes = true,
                                    packageName = "Start Up",
                                    packageCode = 1,
                                };
                                break;
                            case 2:
                                subscriptionAccount = new()
                                {
                                    maxProducts = 800,
                                    maxEmployees = 10,
                                    maxRetailing = 300,
                                    maxBillEntries = 300,
                                    validUntilDay = vday,
                                    validUntilMonth = vmonth,
                                    validUntilYear = vyear,
                                    validUntilDate = vdate,
                                    serialYes = true,
                                    expiryYes = true,
                                    payrollYes = false,
                                    accountingModuleYes = true,
                                    sdCustomizationYes = true,
                                    isShell = false,//this variable is now useless
                                    branchEnabled = true,
                                    desktopYes = true,
                                    maxBranches = 3,
                                    tierCounterYes = true,
                                    packageName = "Small Business",
                                    packageCode = 2,
                                };
                                break;
                            case 3:
                                subscriptionAccount = new()
                                {
                                    maxProducts = 1500,
                                    maxEmployees = 30,
                                    maxRetailing = 300,
                                    maxBillEntries = 300,
                                    validUntilDay = vday,
                                    validUntilMonth = vmonth,
                                    validUntilYear = vyear,
                                    validUntilDate = vdate,
                                    serialYes = true,
                                    expiryYes = true,
                                    payrollYes = true,
                                    accountingModuleYes = true,
                                    sdCustomizationYes = true,
                                    isShell = false,
                                    branchEnabled = true,
                                    packageCode = 3,
                                    desktopYes = true,
                                    maxBranches = 3,
                                    tierCounterYes = true,
                                    packageName = "Premium"
                                };
                                break;
                        }
                        businesses[item.Key].owner = uid;
                        businesses[item.Key].subscription = subscriptionAccount;
                    }
                    await client.SetAsync(DatabaseDirectory.Businesses(), businesses);
                    success2 = true;
                    break;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(duration);
                }
            }

            //overview reconciliation
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    VersionClass versionArray = new VersionClass()
                    {
                        receipt = "1",
                        invoice = "1",
                        quotation = "1",
                        product = "1",
                        tracking = "1",
                        settings = "1",
                        user = "1",
                        employee = "1",
                        business = "1"
                    };
                    FirebaseResponse res2 = client.Set(DatabaseDirectory.Users() + "/" + uid + "/Versions/Current", versionArray);
                    success1 = true;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            if (success1 && success2)
            {
                //we 
            }

            control.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                method, "close");
        }

        public void tryAgainSoon(Action<string> method, Action<string> methodDone)
        {
            Thread.Sleep(4000);

            //updateDatabaseDetails(method, methodDone);
        }
    }
}
