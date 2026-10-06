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
using System.ComponentModel;
using WildGrass_Desktop_f8.Services;

namespace WildGrass_Desktop_f8.Functions
{
    internal class StandardFirebaseOperationsClass
    {
        //ID's
        string uid = "";
        string bid = "";
        string eid = "";


        int duration = 2000;
        int numberOfTries = 3;

        DatabaseDirectoryServicesClass DatabaseDirectory = new();
        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();

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

        //Settings
        public void reloadEverything()
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid(); //this will be necessary for the activityLog update

            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;

            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            double time = dt.TimeOfDay.TotalMilliseconds;


            string vid = Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(time) + "V.yayee";
            if (result)
            {
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
                        FirebaseResponse res2 = client.Set(DatabaseDirectory.Versions() + "/Current", versionArray);
                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
            //bless your soul
        }

        public void reloadMultibranch()
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid(); //this will be necessary for the activityLog update

            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;

            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            double time = dt.TimeOfDay.TotalMilliseconds;


            string vid = Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(time) + "V.yayee";
            if (result)
            {
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
                        FirebaseResponse res2 = client.Set(DatabaseDirectory.UsersVersions() + "/Current", versionArray);
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

        public void reloadEverythingLocal()
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid(); //this will be necessary for the activityLog update

            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;

            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            double time = dt.TimeOfDay.TotalMilliseconds;


            string vid = Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(time) + "V.yayee";
            if (result)
            {
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
                //update local
                success = true;
            }
            //bless your soul
        }

        public void SetupSettings(CounterControl control, Action<bool> method)//what if we added a function to be called when opperation is complete
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid(); //this will be necessary for the activityLog update


            //need to create and individually upload each class that makes up settings
            SDConfiguration receiptConfig = new SDConfiguration()
            {
                wildgrassStores = false,
                wildgrassMaps = true,
                facebook = false,
                twitter = false,
                instagram = false,
                linkedIn = false,
                website = false,
                bankInfo = false,
                pacraInfo = false,
                zraInfo = false,
                address = true,
                phone = true,
                email = true,
                termsNdConditions = "",
                includeLogo = false,
                includeTermsNdConditions = false,
                defaultSendOption = "none"
            };

            SDConfiguration quotationConfig = new SDConfiguration()
            {
                wildgrassStores = false,
                wildgrassMaps = true,
                facebook = false,
                twitter = false,
                instagram = false,
                linkedIn = false,
                website = false,
                bankInfo = false,
                pacraInfo = false,
                zraInfo = false,
                address = true,
                phone = true,
                email = true,
                termsNdConditions = "",
                includeLogo = false,
                includeTermsNdConditions = false,
                defaultSendOption = "none"
            };

            SDConfiguration invoiceConfig = new SDConfiguration()
            {
                wildgrassStores = false,
                wildgrassMaps = true,
                facebook = false,
                twitter = false,
                instagram = false,
                linkedIn = false,
                website = false,
                bankInfo = false,
                pacraInfo = false,
                zraInfo = false,
                address = true,
                phone = true,
                email = true,
                termsNdConditions = "",
                includeLogo = false,
                includeTermsNdConditions = false,
                defaultSendOption = "none",

                //more
                themeColor = ""
            };

            SourceDocumentSettingsClass sourceDocumentSettings = new SourceDocumentSettingsClass()
            {
                email = "",
                password = "",
                cMEnabled = false,
                defaultPrinter = "",
                bankInfo = "",
                pacraInfo = "",
                zraInfo = "",
                facebook = "",
                twitter = "",
                linkedIn = "",
                website = "",
                whatsapp = "",
                mobileSmsOption = "sim", //sim or cm
                ReceiptConfiguration = receiptConfig,
                QuotationConfiguration = quotationConfig,
                InvoiceConfiguration = invoiceConfig,
                OrderConfiguration = receiptConfig,
                DeliveryNoteConfiguration = receiptConfig
            };

            GeneralSettingsClass generalSettingsClass = new GeneralSettingsClass()
            {
                sound = true,
                notifications = true,
                package = "",//need to get the package the user is accessing
                WGStoresEnabled = false,
                WGStoresHandle = "",
                hints = true,
                requirePassordDelete = true,
                requirePassordReturns = true
            };

            Dictionary<string, TypeOfLiquidityClass> typesOfLiquidity1 = new Dictionary<string, TypeOfLiquidityClass>();
            typesOfLiquidity1.Add("Cash", new TypeOfLiquidityClass()
            {
                Name = "Cash",
                custom = false,
                enabled = true
            });
            typesOfLiquidity1.Add("Bank", new TypeOfLiquidityClass()
            {
                Name = "Bank",
                custom = false,
                enabled = false
            });
            typesOfLiquidity1.Add("MobileMoney", new TypeOfLiquidityClass()
            {
                Name = "MobileMoney",
                custom = false,
                enabled = false
            });
            typesOfLiquidity1.Add("Cheque", new TypeOfLiquidityClass()
            {
                Name = "Cheque",
                custom = false,
                enabled = false
            });

            BookKeepingSettingsClass bookKeepingSettingsClass = new BookKeepingSettingsClass()
            {
                vat = 16,
                calculateTax = true,
                currency = "ZMW",
                rate = 1,
                typesOfLiquidity = typesOfLiquidity1
            };

            //let the uploads begin
            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            double time = dt.TimeOfDay.TotalMilliseconds;
            int timeId = Convert.ToInt32(time);
            string id = Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(timeId);

            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //MessageBox.Show("now calling firebase update: " + id);
                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Settings() + "/sourceDocumentSettings", sourceDocumentSettings);
                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Settings() + "/generalSettings", generalSettingsClass);
                        FirebaseResponse firebase2 = client.Set(DatabaseDirectory.Settings() + "/bookKeepingSettings", bookKeepingSettingsClass);

                        string vid = Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(time) + "V.yayee";

                        FirebaseResponse res = client.Set(DatabaseDirectory.Versions() + "/Current/settings", vid);

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

        public void updateSDConfiguration(int sdType, SDConfiguration configuratioin, TextBlock control, Action<bool> method)
        {
            bool success = false;
            string sdConfig = "ReceiptConfiguration";
            switch (sdType)
            {
                case 0:
                    sdConfig = "ReceiptConfiguration";
                    break;
                case 1:
                    sdConfig = "InvoiceConfiguration";
                    break;
                case 2:
                    sdConfig = "QuotationConfiguration";
                    break;
                case 3:
                    sdConfig = "OrderConfiguration";
                    break;
                case 4:
                    sdConfig = "DeliveryNoteConfiguration";
                    break;

            }
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid(); //this will be necessary for the activityLog update

            ReconilliationClass reconilliationClass = new ReconilliationClass();
            bool result = reconilliationClass.IsConnectedToInternet();
            if (result)
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Settings() + "/sourceDocumentSettings/" + sdConfig, configuratioin);

                        UpdateVersion("settings");
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

        public void ChangeCurrency(string selectedCurrency, decimal selectedRate, decimal currentRate, Button control, Action<bool> method)
        {
            bool success = false;

            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            ReconilliationClass reconilliationClass = new ReconilliationClass();
            bool result = reconilliationClass.IsConnectedToInternet();
            if (result)
            {
                if (currentRate == null) currentRate = 1;
                if (currentRate == 0) currentRate = 1;

                decimal newRate = selectedRate * currentRate;
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Settings() + "/bookKeepingSettings/currency", selectedCurrency);
                        FirebaseResponse firebase2 = client.Set(DatabaseDirectory.Settings() + "/bookKeepingSettings/rate", newRate);

                        UpdateVersion("settings");
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

        public async void DownloadResources()
        {
            bool success = false;
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            BusinessClass currentBusiness = prevelantClass.GetBusiness();

            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            string dirPdf = complete + @"\data\pdf resources";
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }
            if (!Directory.Exists(dirPdf + @"\icons"))
            {
                Directory.CreateDirectory(dirPdf + @"\icons");
            }

            try
            {
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Fnunito_light.ttf?alt=media&token=4b3b81e2-5149-456e-b9de-06b6ee7fadf0", dirPdf + @"\nunito_light.ttf");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Fantonio_medium.ttf?alt=media&token=48fcd4d1-93b0-4e1d-bb6f-e323724887e1", dirPdf + @"\antonio_medium.ttf");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Fcoda_caption_extra_bold.ttf?alt=media&token=f238123a-6fd3-4149-b562-7fd256d21843", dirPdf + @"\coda_caption_extra_bold.ttf");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Fopen_sans_regular.ttf?alt=media&token=2e781640-dc8b-4877-8409-5c7032ea01e5", dirPdf + @"\open_sans_regular.ttf");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Foswald_medium.ttf?alt=media&token=befa15fc-9d60-4d8b-a7b9-9c5393bbfe64", dirPdf + @"\oswald_medium.ttf");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2FsRGB_CS_profile.icm?alt=media&token=40de6724-9bbe-4817-b43e-68e58ad4cca4", dirPdf + @"\sRGB_CS_profile.icm");


                //now we do the icons
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Ficons%2Ffacebook.png?alt=media&token=db40886e-485d-42f6-b90f-124d9c46c2f0", dirPdf + @"\icons\facebook.png");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Ficons%2Ffacebook_white.png?alt=media&token=f0e4ca5b-61ac-4635-8dc7-b1178e47335a", dirPdf + @"\icons\facebook_white.png");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Ficons%2Finstagram.png?alt=media&token=f5f18900-874e-4e9d-a2d9-2901260e24f1", dirPdf + @"\icons\instagram.png");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Ficons%2Finstagram_white.png?alt=media&token=387c6483-dd23-4b54-81bf-983cd91216ca", dirPdf + @"\icons\instagram_white.png");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Ficons%2Flinkedin.png?alt=media&token=b6ca7261-a084-4bd2-b3c2-364dc03b8541", dirPdf + @"\icons\linkedin.png");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Ficons%2Flinkedin_white.png?alt=media&token=116f2a1b-56e7-4082-859e-3614fa128d1e", dirPdf + @"\icons\linkedin_white.png");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Ficons%2Ftwitter.png?alt=media&token=634ce42f-1802-4ae6-833c-e1decccd1e64", dirPdf + @"\icons\twitter.png");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Ficons%2Ftwitter_white.png?alt=media&token=b21ab331-cb2e-4a9b-99b7-82b4008d34f3", dirPdf + @"\icons\twitter_white.png");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Ficons%2Fwebsite.png?alt=media&token=2976f10a-6eac-4142-804c-8a56d4602253", dirPdf + @"\icons\website.png");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Ficons%2Fwebsite_white.png?alt=media&token=38d39cd6-8112-45f6-be2d-979e328bf7b5", dirPdf + @"\icons\website_white.png");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Ficons%2Fwhatsapp.png?alt=media&token=63086592-2ef5-4b56-a85d-a2b47156fa9f", dirPdf + @"\icons\whatsapp.png");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Ficons%2Fwhatsapp_white.png?alt=media&token=a3211f8c-0f82-41d9-a531-5b034aedb67f", dirPdf + @"\icons\whatsapp_white.png");
                downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/system%20resources%2Fres%2Fplaceholder.png?alt=media&token=886687ea-0f73-49c5-a66e-4e355a10154a", dirPdf + @"\placeholder.png");

                success = true;
            }
            catch (Exception)
            {

            }

            string specificDir = complete + @"\data\" + uid + @"\businesses\" + bid + @"\pdf resources";
            if (!Directory.Exists(specificDir))
            {
                Directory.CreateDirectory(specificDir);
            }
            try
            {
                if (File.Exists(specificDir + @"\logo1.png"))
                {
                    File.Delete(specificDir + @"\logo1.png");
                }
                downResource(currentBusiness.logoUrl, specificDir + @"\logo1.png");
            }
            catch (Exception e)
            {
                //MessageBox.Show("something went wrong: " + e.Message, "system testing");
            }

            //MessageBox.Show("Success = " + success, "system testing");

            //webClient.DownloadFileAsync(new Uri("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/hALSHm4F2HUBQhostBU05OH1c4i2%2FWG210213051535235B%2FReceipts%2FWILD%20GRASS%3A%20ReceiptLWPAug%2C%2020%20202109%3A56%3A25%20AM.pdf?alt=media&token=9ac92e1f-db08-4de3-b0f3-9609123c5999"), dirPdf + @"\" + "pdf1" + ".pdf");
        }

        public async void downResource(string url, string dir)
        {
            if (!File.Exists(dir))
            {
                if (url != null)
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadFileAsync(new Uri(url), dir);
                }
            }
        }

        private void CompletedDownload(object sender)
        {

        }

        //Transaction Stuff
        private void doneUploading(object s, FirebaseStorageProgress e)
        {
            Console.WriteLine($"Progress: {e.Percentage} %");
            if (e.Percentage == 100)
            {
                //MessageBox.Show("We are at 100%", "system testing");
            }
        }

        public void saveSelectedProducts(Dictionary<string, ProductClass> productArray)
        {
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            string dir = complete + @"\data\" + uid + @"\businesses\" + bid + @"\Products";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            dir += @"\selectedProduct.txt";

            int countC = 0;
            string customSerialized = "{";
            int countD = 0;
            if (productArray.Count != 0)
            {
                foreach (var product in productArray)
                {
                    if (countD != 0)
                    {
                        customSerialized += ",";
                    }

                    customSerialized += (char)34 + product.Key + (char)34 + ": " + (char)34 + product.Value.pid + (char)34;

                    countD = 1;
                }
            }
            else
            {
                customSerialized += (char)34 + "products" + (char)34 + ":" + (char)34 + "," + (char)34;
            }
            customSerialized += "}"; //all entries close

            System.IO.File.WriteAllText(dir, customSerialized);
        }

        public async void activityLog(string tid, string activity, string subActivity, string eid)
        {
            setup();

            if (eid == null)
            {
                eid = "1";
            }
            else if (eid.Length == 0)
            {
                eid = "1";
            }

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();

            DateTime dt = DateTime.Now;
            string date = dt.ToString("MMM, dd yyyy");
            string time = dt.ToString("HH:mm:ss tt");
            double timeId1 = dt.TimeOfDay.TotalMilliseconds;
            int timeId = Convert.ToInt32(timeId1);
            string day = Convert.ToString(dt.Day);
            int mm = dt.Month;
            mm++;
            string month = Convert.ToString(mm);
            string year = Convert.ToString(dt.Year);

            string id = year + month + day + Convert.ToString(timeId) + "activityLog";

            ActivityLogClass activityEntry = new ActivityLogClass()
            {
                tid = tid,
                time = time,
                date = date,
                type = activity,
                sourceDocumentType = subActivity
            };

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    //update
                    FirebaseResponse res = await client.SetAsync(DatabaseDirectory.Employees() + "/" + eid + "/activityLog/" + id, activityEntry);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
        }

        public async void UpdateVersion(string dataset)
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            double time = dt.TimeOfDay.TotalMilliseconds;


            string vid = Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(time) + "V.yayee";

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    //update firebase versions... this insures that all apps using FPCS will update
                    FirebaseResponse res = await client.SetAsync(DatabaseDirectory.Versions() + "/Current/" + dataset, vid);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            if(dataset == "business" || dataset == "user")
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //update firebase versions... this insures that all apps using FPCS will update
                        FirebaseResponse res = await client.SetAsync(DatabaseDirectory.UsersVersions() + "/Current/" + dataset, vid);
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
        }
        public async void UpdateVersion(string dataset, string bid)
        {
            setup();

            uid = prevelantClass.getUid();
            eid = prevelantClass.getEid();

            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            double time = dt.TimeOfDay.TotalMilliseconds;


            string vid = Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(time) + "V.yayee";

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    //update firebase versions... this insures that all apps using FPCS will update
                    FirebaseResponse res = await client.SetAsync(DatabaseDirectory.Versions() + "/Current/" + dataset, vid);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            if (dataset == "business" || dataset == "user")
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //update firebase versions... this insures that all apps using FPCS will update
                        FirebaseResponse res = await client.SetAsync(DatabaseDirectory.UsersVersions() + "/Current/" + dataset, vid);
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
        }

        //business functions
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
                        FirebaseResponse res44 = client.Get(DatabaseDirectory.FPCSBusinesses());
                        businessesArray = JsonConvert.DeserializeObject<Dictionary<string, BusinessClass>>(res44.Body.ToString());
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
                

                int checker = 0;
                if (businessesArray != null) if(businessesArray.Count > 0)
                    {
                        foreach (var item in businessesArray)
                        {
                            if (item.Value.businessTitle == business.businessTitle)
                            {
                                checker++;
                            }
                        }
                    }

                if (checker == 0)
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

                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            //depracated directories
                            FirebaseResponse res = client.Set(DatabaseDirectory.Businesses() + "/" + business.bid, business);
                            FirebaseResponse res1 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + business.bid, business);
                            FirebaseResponse res2 = client.Set(DatabaseDirectory.Businesses() + "/" + business.bid + "/Versions/Current", versionArray);

                            FirebaseResponse response1a = client.Set(DatabaseDirectory.FPCSUser() + "/" + uid + "/myBusinesses/" + business.bid, business.businessTitle);

                            UsersClass currentUser = prevelantClass.GetCurrentUser();
                            if(businessesArray != null) currentUser.businesses = businessesArray;
                            prevelantClass.SetCurrentUser(currentUser);

                            FirebaseResponse res111 = client.Get(DatabaseDirectory.FPCSUser());
                            Dictionary<string, UsersClass> allUsers = JsonConvert.DeserializeObject<Dictionary<string, UsersClass>>(res111.Body.ToString());
                            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                            string complete = System.IO.Path.Combine(systemPath, "WildGrass");
                            string dir = complete + @"\data\" + uid;
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            System.IO.File.WriteAllText(dir + @"\UserData.txt", res111.Body.ToString());

                            success = true;
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                            //MessageBox.Show(ex.Message, "system testing");
                        }
                    }
                    UpdateVersion("business", business.bid);
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


                        UpdateVersion("business");
                        activityLog(bid, "management", "Business Details Edit", eid);
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

                        UpdateVersion("business");
                        activityLog(bid, "management", "Bank Details Edit", eid);
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
                            UpdateVersion("business");
                            activityLog(bid, "management", "Business Details Edit", eid);
                            DownloadResources();
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

        //public async void uploadImage(string dirLogo, string imageType, string name)
        //{
        //    bool success = false;
        //    ReconilliationClass reconilliation = new ReconilliationClass();
        //    bool result = reconilliation.IsConnectedToInternet();
        //    if (result)
        //    {
        //        setup();

        //        uid = prevelantClass.getUid();
        //        bid = prevelantClass.getBid();

        //        string logoLink = "";
        //        if (File.Exists(dirLogo))
        //        {
        //            for (int i = 0; i < numberOfTries; i++)
        //            {
        //                try
        //                {
        //                    var stream = File.Open(dirLogo, FileMode.Open);

        //                    // Construct FirebaseStorage, path to where you want to upload the file and Put it there
        //                    var task = new FirebaseStorage("long-walk-pos.appspot.com")
        //                        .Child("system resources")
        //                        .Child("res")
        //                        .Child(name + imageType)
        //                        .PutAsync(stream);

        //                    // Track progress of the upload
        //                    task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

        //                    // await the task to wait until upload completes and get the download url
        //                    var downloadUrl = await task;
        //                    logoLink = downloadUrl;

        //                    //upload link to firebase directory
        //                    FirebaseResponse res = client.Set(@"system resources/res/" + name, logoLink);
        //                    success = true;
        //                    break;
        //                }
        //                catch (Exception)
        //                {
        //                    Thread.Sleep(duration);
        //                }
        //            }
        //        }
        //    }
        //}

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
                        downResource(currentBusiness.logoUrl, specificDir + @"\logo1.png");
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
        }

        //mass operations
        public void massDelete(Dictionary<string, ProductClass> productsArray, TextBox control, Action<bool> method)
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
                if (eid == null)
                {
                    eid = "1";
                }
                else if (eid == "")
                {
                    eid = "1";
                }

                if (productsArray != null) if (productsArray.Count > 0)
                    {
                        foreach (var product in productsArray)
                        {
                            for (int i = 0; i < numberOfTries; i++)
                            {
                                try
                                {
                                    // remove from products
                                    FirebaseResponse response = client.Delete(DatabaseDirectory.Products() + "/" + product.Value.pid);

                                    //remove from tracking
                                    //FirebaseResponse response1 = client.Delete(@"Users/" + uid + "/businesses/" + bid + "/Tracking" + product.Value.pid);

                                    //update activity log
                                    activityLog(product.Value.pid, "product", "Deleted product", eid);
                                    break;
                                }
                                catch (Exception)
                                {
                                    Thread.Sleep(duration);
                                }
                            }
                        }
                    }

                //update version
                UpdateVersion("product");
                success = true;
            }

            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void massChangeCategory(Dictionary<string, ProductClass> productsArray, string categoy, Button control, Action<bool> method)
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
                if (eid == null)
                {
                    eid = "1";
                }
                else if (eid == "")
                {
                    eid = "1";
                }

                if (productsArray != null) if (productsArray.Count > 0)
                    {
                        foreach (var product in productsArray)
                        {
                            for (int i = 0; i < numberOfTries; i++)
                            {
                                try
                                {
                                    FirebaseResponse response = client.Set(DatabaseDirectory.Products() + "/" + product.Value.pid + "/category", categoy);

                                    //activity log
                                    activityLog(product.Value.pid, "product", "Edited product category", eid);
                                    break;
                                }
                                catch (Exception)
                                {
                                    Thread.Sleep(duration);
                                }
                            }
                        }
                    }

                //update version
                UpdateVersion("product");
                success = true;
            }

            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void editCategoryName(Dictionary<string, ProductClass> productsArray, string newName, string oldName, TextBox control, Action<bool> method)
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
                if (eid == null)
                {
                    eid = "1";
                }
                else if (eid == "")
                {
                    eid = "1";
                }

                int count = 0;
                if (productsArray != null) if (productsArray.Count > 0)
                    {
                        foreach (var product in productsArray)
                        {
                            if (product.Value.category == oldName)
                            {
                                count++;
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        FirebaseResponse response3 = client.Set(DatabaseDirectory.Products() + "/" + product.Value.pid + "/category", newName);
                                        //update activity
                                        activityLog(product.Value.pid, "product", "Edited product category", eid);
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

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse response = client.Set(DatabaseDirectory.Categories() + "/" + newName + "/categoryName", newName);
                        FirebaseResponse response1 = client.Delete(DatabaseDirectory.Categories() + "/" + oldName);

                        //update version
                        //MessageBox.Show(Convert.ToString(count) + ", moved from " + oldName + " to " + newName, "System testing");
                        UpdateVersion("product");
                        activityLog(oldName, "product", "Edited category name", eid);
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

        public void deleteCategoryNdProducts(Dictionary<string, ProductClass> productsArray, string oldName, TextBox control, Action<bool> method)
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
                if (eid == null)
                {
                    eid = "1";
                }
                else if (eid == "")
                {
                    eid = "1";
                }

                int count = 0;
                if (productsArray != null) if (productsArray.Count > 0)
                    {
                        foreach (var product in productsArray)
                        {
                            if (product.Value.category == oldName)
                            {
                                count++;
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        FirebaseResponse response3 = client.Delete(DatabaseDirectory.Products() + "/" + product.Value.pid);
                                        UpdateVersion("product");
                                        //update activity
                                        activityLog(product.Value.pid, "product", "Deleted product", eid);
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

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse response1 = client.Delete(DatabaseDirectory.Categories() + "/" + oldName);

                        //update version
                        UpdateVersion("product");
                        activityLog(oldName, "product", "Deleted category", eid);
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

        public void moveProductsDeleteCategory(Dictionary<string, ProductClass> productsArray, string newName, string oldName, TextBox control, Action<bool> method)
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
                if (eid == null)
                {
                    eid = "1";
                }
                else if (eid == "")
                {
                    eid = "1";
                }

                if (productsArray != null) if (productsArray.Count > 0)
                    {
                        foreach (var product in productsArray)
                        {
                            if (product.Value.category == oldName)
                            {
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        FirebaseResponse response3 = client.Set(DatabaseDirectory.Products() + "/" + product.Value.pid + "/category", newName);
                                        UpdateVersion("product");
                                        //update activity
                                        activityLog(product.Value.pid, "product", "Edited product category", eid);
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

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse response1 = client.Delete(DatabaseDirectory.Categories() + "/" + oldName);

                        //update version
                        UpdateVersion("product");
                        activityLog(oldName, "product", "Deleted category", eid);
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

        //access determine functions
        public void accessCodeEntered(string category, TextBox control, Action<bool> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //FirebaseResponse response = client.Set(@"Users/" + uid + "/businesses/" + bid + "/products/categories/" + category + "/categoryName", category);

                        ////update version
                        //UpdateVersion("product");
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
                success = true;
            }

            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void startFreeTrail(string bid, TextBox control, Action<bool> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();

                BusinessClass business = new()
                {
                    validUntilDay = "",
                    validUntilMonth = "",
                    validUntilYear = "",
                    accessLevel = "Premium",
                    package = 3
                };

                DateTime dt = DateTime.Now;
                dt = dt.AddMonths(2);


                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse res1 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/validUntilDay", dt.Day.ToString());
                        FirebaseResponse res2 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/validUntilMonth", dt.Month.ToString());
                        FirebaseResponse res3 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/validUntilYear", dt.Year.ToString());
                        FirebaseResponse res4 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/accessLevel", "Premium");
                        FirebaseResponse res5 = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/package", 3);


                        //set free trail as used
                        FirebaseResponse resa1 = client.Set(DatabaseDirectory.Users() + "/" + uid + "/freeTrail", "used");
                        FirebaseResponse resa2 = client.Set(DatabaseDirectory.FPCSUser() + "/" + uid + "/freeTrail", "used");

                        //update version
                        UpdateVersion("user");
                        UpdateVersion("business");
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
                success = true;
            }

            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void getFreePackage(string category, TextBox control, Action<bool> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //FirebaseResponse response = client.Set(@"Users/" + uid + "/businesses/" + bid + "/products/categories/" + category + "/categoryName", category);

                        ////update version
                        //UpdateVersion("product");
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
                success = true;
            }

            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        //Notifications
        public void createProductNotification(Dictionary<string, ProductEntrySDClass> onGoingArray)
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();

            Dictionary<string, ProductClass> localProductArray = prevelantClass.LoadDataProducts();

            if (onGoingArray != null) if (onGoingArray.Count > 0)
                {
                    Dictionary<string, NotificationClass> notiArray = prevelantClass.LoadDataNotifications();
                    foreach (var item in onGoingArray)
                    {
                        DateTime dt = DateTime.Now;
                        string date = dt.ToString("MMM, dd yyyy");
                        string time = dt.ToString("HH:mm:ss tt");
                        int month = dt.Month;
                        int year = dt.Year;
                        int daySele = dt.Day;
                        double timeIdq = dt.TimeOfDay.TotalMilliseconds;
                        int timeId = Convert.ToInt32(timeIdq);

                        string nid = Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(timeId) + "nid";


                        if (localProductArray != null) if (localProductArray.ContainsKey(item.Key))
                            {
                                ProductClass product = localProductArray[item.Key];

                                if (product.inventoryTracking == "on")
                                {
                                    double count = Convert.ToDouble(item.Value.quantity);
                                    double qty = Convert.ToDouble(product.qtyInStock);
                                    double buffer = Convert.ToDouble(product.bufferQty);

                                    qty -= count;

                                    double upperLimit = buffer * 25 / 100;
                                    upperLimit = Math.Round(upperLimit, 0);
                                    upperLimit += buffer;

                                    string details = "";
                                    string title = product.brandName + " " + product.productName + " " + product.flavor + " " + product.size;
                                    string type = "product";
                                    string pid = product.pid;

                                    if (qty < buffer)
                                    {
                                        //you are below the line
                                        details = "You are below the line";
                                    }
                                    else if (qty == buffer)
                                    {
                                        //you are on the line, careful
                                        details = "You are on the line";
                                    }
                                    else if (qty <= upperLimit)
                                    {
                                        //getting close to the line
                                        details = "Getting close to the line";
                                    }


                                    NotificationClass noti = new NotificationClass()
                                    {
                                        nid = nid,
                                        date = date,
                                        title = title,
                                        time = time,
                                        type = type,
                                        details = details,
                                        pid = pid
                                    };

                                    if (notiArray != null) if (notiArray.Count > 0)
                                        {
                                            foreach (var item1 in notiArray)
                                            {
                                                if (item1.Value.pid == noti.pid)
                                                {//if true we override the current entry in the database
                                                    noti.nid = item1.Value.nid;
                                                    nid = item1.Value.nid;
                                                }
                                            }
                                        }

                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.Notifications() + "/" + nid, noti);

                                            //update version
                                            UpdateVersion("notification");
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
        }

        public void clearNotifications(Button control, Action<bool> method)
        {
            bool success = false;
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();

            ReconilliationClass reconilliation = new ReconilliationClass();
            bool internet = reconilliation.IsConnectedToInternet();
            if (internet)
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse firebase = client.Delete(DatabaseDirectory.Notifications());

                        //update version
                        UpdateVersion("notification");

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

        public void clearNotification(string nid, Button control, Action<bool> method)
        {
            bool success = false;
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();

            ReconilliationClass reconilliation = new ReconilliationClass();
            bool internet = reconilliation.IsConnectedToInternet();
            if (internet)
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse firebase = client.Delete(DatabaseDirectory.Notifications() + "/" + nid);

                        //update version
                        UpdateVersion("notification");

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

        //customer and supplier functions
        public void createCustomer(CustomerClass customer, Button control, Action<bool> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse response = client.Set(DatabaseDirectory.Customers() + "/" + customer.customerID, customer);

                        //update version
                        UpdateVersion("customer");
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

        public void editCustomer(CustomerClass customer, Button control, Action<bool> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse response0 = client.Set(DatabaseDirectory.Customers() + "/" + customer.customerID + "/firstName", customer.firstName);
                        FirebaseResponse response1 = client.Set(DatabaseDirectory.Customers() + "/" + customer.customerID + "/lastName", customer.lastName);
                        FirebaseResponse response1a = client.Set(DatabaseDirectory.Customers() + "/" + customer.customerID + "/address", customer.address);
                        FirebaseResponse response2 = client.Set(DatabaseDirectory.Customers() + "/" + customer.customerID + "/phone", customer.phone);
                        FirebaseResponse response3 = client.Set(DatabaseDirectory.Customers() + "/" + customer.customerID + "/email", customer.email);
                        FirebaseResponse response4 = client.Set(DatabaseDirectory.Customers() + "/" + customer.customerID + "/tpin", customer.tpin);
                        FirebaseResponse response5 = client.Set(DatabaseDirectory.Customers() + "/" + customer.customerID + "/bankDetails", customer.bankDetails);

                        //update version
                        UpdateVersion("customer");
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

        public async void createCustomer(CustomerClass customer, bool includesFile, CustomerFileClass fileClass, int customerType, Button control, Action<bool> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();

                if (includesFile)
                {
                    if (File.Exists(fileClass.url))
                    {
                        for (int i = 0; i < numberOfTries; i++)
                        {
                            try
                            {
                                var stream = File.Open(fileClass.url, FileMode.Open);

                                var task = new FirebaseStorage("long-walk-pos.appspot.com")
                                    .Child("users")
                                    .Child(uid)
                                    .Child(bid)
                                    .Child(fileClass.fid + fileClass.ext)
                                    .PutAsync(stream);

                                // Track progress of the upload
                                task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                                // await the task to wait until upload completes and get the download url
                                var downloadUrl = await task;
                                fileClass.url = downloadUrl;

                                FirebaseResponse res = client.Set(DatabaseDirectory.Customers() + "/" + customer.customerID + "/files/" + fileClass.fid, fileClass);
                                UpdateVersion("customer");
                                activityLog(bid, "file update", "Customer Details Edit", eid);

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

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse response1 = client.Set(DatabaseDirectory.Customers() + "/" + customer.customerID, customer);

                        //update version
                        UpdateVersion("customer");
                        activityLog(bid, "file update", "Customer Details Created", eid);
                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }

            await control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void updateToBoth(string cid, Button control, Action<bool> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse response = client.Set(DatabaseDirectory.Customers() + "/" + cid + "/type", 2);

                        //update version
                        UpdateVersion("customer");
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

        public void createSupplier(CustomerClass customer, Button control, Action<bool> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse response = client.Set(DatabaseDirectory.Suppliers() + "/" + customer.customerID, customer);

                        //update version
                        UpdateVersion("supplier");
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

        public async void uploadFile(CustomerFileClass fileClass, string customerID, Button control, Action<bool> method)
        {//customerType is either Customer or Supplier
            bool success = false;
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();

                if (File.Exists(fileClass.url))
                {
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            var stream = File.Open(fileClass.url, FileMode.Open);

                            var task = new FirebaseStorage("long-walk-pos.appspot.com")
                                .Child("users")
                                .Child(uid)
                                .Child(bid)
                                .Child(fileClass.fid + fileClass.ext)
                                .PutAsync(stream);

                            // Track progress of the upload
                            task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                            // await the task to wait until upload completes and get the download url
                            var downloadUrl = await task;
                            fileClass.url = downloadUrl;

                            FirebaseResponse res = client.Set(DatabaseDirectory.Customers() + "/" + customerID + "/files/" + fileClass.fid, fileClass);
                            UpdateVersion("customer");
                            activityLog(bid, "file update", "Customer Details Edit", eid);

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
            await control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        
        public void UpdateCidTransaction(string cid, string folio, string sdType)
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse response1 = client.Set(DatabaseDirectory.Customers() + "/" + cid + "/" + sdType + "/" + folio, folio);
                    //update version
                    UpdateVersion("customer");
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
        }

        //new database structure
        public void restructureDatabase()
        {
            //just call me and I will take care of everything...
            setup();
            uid = prevelantClass.getUid();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(@"FPCS/Users/" + uid);
                    UserClass user = JsonConvert.DeserializeObject<UserClass>(res.Body);

                    if(user != null)
                    {
                        if (user.myBusinesses == null) user.myBusinesses = new Dictionary<string, string>();
                        if(user.businesses != null) if (user.businesses.Count > 0)
                            {
                                foreach(var business in user.businesses)
                                {
                                    user.myBusinesses.Add(business.Key, business.Value.businessTitle);

                                    //it might be time to get reed of on of the receipt entries
                                    FirebaseResponse res0 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/Receipts");
                                    FirebaseResponse res1 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/Invoices");
                                    FirebaseResponse res2 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/All Receipts");
                                    FirebaseResponse res3 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/All Invoices");
                                    FirebaseResponse res4 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/Quotaions");
                                    FirebaseResponse res5 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/Customers");
                                    FirebaseResponse res6 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/Orders"); 
                                    FirebaseResponse res7 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/TransferNotes");
                                    FirebaseResponse res8 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/products/products");
                                    FirebaseResponse res8a = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/products/categories");
                                    FirebaseResponse res9 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/QuantityChange");
                                    FirebaseResponse res10 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/Tracking");
                                    FirebaseResponse res11 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/employees");
                                    FirebaseResponse res12 = client.Get(@"Users/" + uid + "/businesses/" + business.Key + "/Notifications");

                                    //dast data into dictionaries of classes
                                    Dictionary<string, DateClass> receipts = new Dictionary<string, DateClass>();
                                    Dictionary<string, DateClass> invoices = new Dictionary<string, DateClass>();
                                    Dictionary<string, ReceiptClass> allReceipts = new Dictionary<string, ReceiptClass>();
                                    Dictionary<string, InvoiceClass> allInvoices = new Dictionary<string, InvoiceClass>();
                                    Dictionary<string, QuotationClass> quotations = new Dictionary<string, QuotationClass>();
                                    Dictionary<string, CustomerClass> customers = new Dictionary<string, CustomerClass>();
                                    Dictionary<string, OrderClass> orders = new Dictionary<string, OrderClass>();
                                    Dictionary<string, DeliveryNoteClass> tNotes = new Dictionary<string, DeliveryNoteClass>();
                                    Dictionary<string, ProductClass> products = new Dictionary<string, ProductClass>();
                                    Dictionary<string, CategoryClass> categories = new Dictionary<string, CategoryClass>();
                                    Dictionary<string, DateClass> quantityChange = new Dictionary<string, DateClass>();
                                    Dictionary<string, TrackingClass> tracking = new Dictionary<string, TrackingClass>();
                                    Dictionary<string, EmployeeClass> employees = new Dictionary<string, EmployeeClass>();
                                    Dictionary<string, NotificationClass> notifications = new Dictionary<string, NotificationClass>();
                                    

                                    string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                                    string complete = System.IO.Path.Combine(systemPath, "WildGrass");
                                    string dir = complete + @"\dataRestructure\" + uid + @"\businesses\" + business.Key;
                                    if (!Directory.Exists(dir))
                                    {
                                        Directory.CreateDirectory(dir);
                                    }

                                    //if (res0 != null)
                                    //{
                                    //    System.IO.File.WriteAllText(dir + @"\receipts.txt", res0.Body.ToString());
                                    //    using (StreamReader r = new StreamReader(dir + @"\receipts.txt"))
                                    //    {
                                    //        string json = r.ReadToEnd();
                                    //        if (json != null)
                                    //        {
                                    //            receipts = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(json);
                                    //        }
                                    //    }
                                    //}
                                    //if (res0 != null) receipts = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(res0.Body.ToString());
                                    //if (res1 != null) invoices = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(res1.Body);
                                    //if (res2 != null) allReceipts = JsonConvert.DeserializeObject<Dictionary<string, ReceiptClass>>(res2.Body);
                                    //if (res3 != null) allInvoices = JsonConvert.DeserializeObject<Dictionary<string, InvoiceClass>>(res3.Body);
                                    if (res4 != null) quotations = JsonConvert.DeserializeObject<Dictionary<string, QuotationClass>>(res4.Body);
                                    if (res5 != null) customers = JsonConvert.DeserializeObject<Dictionary<string, CustomerClass>>(res5.Body);
                                    if (res6 != null) orders = JsonConvert.DeserializeObject<Dictionary<string, OrderClass>>(res6.Body);
                                    if (res7 != null) tNotes = JsonConvert.DeserializeObject<Dictionary<string, DeliveryNoteClass>>(res7.Body);
                                    if (res8 != null) products = JsonConvert.DeserializeObject<Dictionary<string, ProductClass>>(res8.Body);
                                    if (res8a != null) categories = JsonConvert.DeserializeObject<Dictionary<string, CategoryClass>>(res8a.Body);
                                    if (res9 != null) quantityChange = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(res9.Body);
                                    if (res10 != null) tracking = JsonConvert.DeserializeObject<Dictionary<string, TrackingClass>>(res10.Body);
                                    //if (res11 != null) employees = JsonConvert.DeserializeObject<Dictionary<string, EmployeeClass>>(res11.Body);
                                    if (res12 != null) notifications = JsonConvert.DeserializeObject<Dictionary<string, NotificationClass>>(res12.Body);

                                    //receipts = prevelantClass.LoadDatesSales();
                                    //invoices = prevelantClass.LoadDatesCredit();
                                    //allInvoices = prevelantClass.LoadDataAllInvoices();
                                    //quotations = prevelantClass.LoadDataQuotations();
                                    //customers = prevelantClass.LoadDataCustomers();
                                    //orders = prevelantClass.LoadDataOrders();
                                    //tNotes = prevelantClass.LoadDataTNotes();
                                    //products = prevelantClass.LoadDataProducts();
                                    //categories = prevelantClass.LoadDataCategories();
                                    //quantityChange = prevelantClass.LoadDatesQCs();
                                    //employees = prevelantClass.LoadDataEmployees();
                                    //notifications = prevelantClass.LoadDataNotifications();


                                    //recreate the business data
                                    //details
                                    FirebaseResponse response = client.Set(@"Businesses/" + business.Key + "/Details", business.Value);
                                    FirebaseResponse responsea = client.Set(@"Businesses/" + business.Key + "/Details/owner", uid);

                                    FirebaseResponse response0d = client.Delete(@"Businesses/" + business.Key + "/Data/Receipts");
                                    FirebaseResponse response1d = client.Delete(@"Businesses/" + business.Key + "/Data/Invoices");
                                    //data
                                    //FirebaseResponse response0 = client.Set(@"Businesses/" + business.Key + "/Data/Receipts", receipts);
                                    //FirebaseResponse response1 = client.Set(@"Businesses/" + business.Key + "/Data/Invoices", invoices);
                                    //FirebaseResponse response2 = client.Set(@"Businesses/" + business.Key + "/Data/All Receipts", allReceipts);
                                    //FirebaseResponse response3 = client.Set(@"Businesses/" + business.Key + "/Data/All Invoices", allInvoices);
                                    FirebaseResponse response4 = client.Set(@"Businesses/" + business.Key + "/Data/Quotations", quotations);
                                    FirebaseResponse response5 = client.Set(@"Businesses/" + business.Key + "/Data/Customers", customers);
                                    FirebaseResponse response6 = client.Set(@"Businesses/" + business.Key + "/Data/Orders", orders);
                                    FirebaseResponse response7 = client.Set(@"Businesses/" + business.Key + "/Data/TransferNotes", tNotes);
                                    FirebaseResponse response8 = client.Set(@"Businesses/" + business.Key + "/Data/Products/products", products);
                                    FirebaseResponse response8a = client.Set(@"Businesses/" + business.Key + "/Data/Products/categories", categories);
                                    FirebaseResponse response9 = client.Set(@"Businesses/" + business.Key + "/Data/QuantityChange", quantityChange);
                                    FirebaseResponse response10 = client.Set(@"Businesses/" + business.Key + "/Data/Tracking", tracking);
                                    //FirebaseResponse response11 = client.Set(@"Businesses/" + business.Key + "/Data/Employees", employees);
                                    FirebaseResponse response12 = client.Set(@"Businesses/" + business.Key + "/Data/Notifications", notifications);

                                    //delete the old
                                    //FirebaseResponse final = client.Delete(@"Users/" + uid + "/businesses/" + business.Key);
                                }
                            }
                        //upload myBusinesses
                        FirebaseResponse response1a = client.Set(@"Users/" + uid + "/myBusinesses", user.myBusinesses);

                        //we move versions to the correct directory
                        //thats it now we delete fpcs
                    }
                    break;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "system testing");
                }
            }
        }
    }
}
