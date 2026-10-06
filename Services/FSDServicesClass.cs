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
using WildGrass_Desktop_f8.Activities.main;
using Spire.Pdf;
using WildGrass_Desktop_f8.Functions;
using WildGrass_Desktop;

namespace WildGrass_Desktop_f8.Services
{
    /// <summary>
    /// CRUD FSD
    /// here we enter fsd: receipt, invoice
    /// </summary>
    internal class FSDServicesClass
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
        public async void uploadFileTransaction(SourceDocumentClass2 entry, Button control, Action<bool> method)
        {//customerType is either Customer or Supplier
            bool success = false;
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();

                if (entry.dirPdf.Length > 0 && entry.ext.Length > 0)
                {
                    if (File.Exists(entry.dirPdf))
                    {
                        for (int i = 0; i < numberOfTries; i++)
                        {
                            try
                            {
                                var stream = File.Open(entry.dirPdf, FileMode.Open);

                                var task = new FirebaseStorage("long-walk-pos.appspot.com")
                                    .Child("users")
                                    .Child(uid)
                                    .Child(bid)
                                    .Child(entry.folio + entry.ext)
                                    .PutAsync(stream);

                                // Track progress of the upload
                                task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                                // await the task to wait until upload completes and get the download url
                                var downloadUrl = await task;
                                entry.pdfLink = downloadUrl;
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
                else
                {
                    //there is no file to upload so we set success to true and move on
                    success = true;
                }
            }
            if (success)
            {
                UploadPurchasesData(entry, control, method);
            }
            else
            {
                await control.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    method, success);
            }
        }

        public async void UploadPurchasesData(SourceDocumentClass2 entry, Button control, Action<bool> method)
        {
            bool success = false;
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            DateClass dateEntry = new DateClass
            {
                comp = entry.month + entry.year,
                date = entry.date,
                day = entry.day,
                month = entry.month,
                year = entry.year
            };


            switch (entry.sdType)
            {
                case "ReceiptIn":
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            //now we upload our receipt to both refernces
                            FirebaseResponse firebasea = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + entry.date + "/sd/" + entry.folio, entry);
                            FirebaseResponse firebase2a = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + entry.date + "/date", dateEntry.date);
                            FirebaseResponse firebase3a = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + entry.date + "/comp", dateEntry.comp);
                            FirebaseResponse firebase4a = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + entry.date + "/day", dateEntry.day);
                            FirebaseResponse firebase5a = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + entry.date + "/month", dateEntry.month);
                            FirebaseResponse firebase6a = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + entry.date + "/year", dateEntry.year);
                            FirebaseResponse firebase1a = client.Set(DatabaseDirectory.AllReceiptsIn() + "/" + entry.folio, entry);
                            success = true;
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                    standardFirebaseOperationsClass.activityLog(entry.folio, "transaction", "ReceiptIn", eid);
                    standardFirebaseOperationsClass.UpdateVersion("receiptIn");
                    if (entry.cid != null) if (entry.cid != "")
                        {
                            UpdateCidTransaction(entry.cid, entry.cid, "receiptsIn");
                        }
                    break;
                case "InvoiceIn":
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            //now we upload our receipt to both refernces
                            FirebaseResponse firebase = client.Set(DatabaseDirectory.InvoicesIn() + "/" + entry.date + "/sd/" + entry.folio, entry);
                            FirebaseResponse firebase2 = client.Set(DatabaseDirectory.InvoicesIn() + "/" + entry.date + "/date", dateEntry.date);
                            FirebaseResponse firebase3 = client.Set(DatabaseDirectory.InvoicesIn() + "/" + entry.date + "/comp", dateEntry.comp);
                            FirebaseResponse firebase4 = client.Set(DatabaseDirectory.InvoicesIn() + "/" + entry.date + "/day", dateEntry.day);
                            FirebaseResponse firebase5 = client.Set(DatabaseDirectory.InvoicesIn() + "/" + entry.date + "/month", dateEntry.month);
                            FirebaseResponse firebase6 = client.Set(DatabaseDirectory.InvoicesIn() + "/" + entry.date + "/year", dateEntry.year);
                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + entry.folio, entry);
                            success = true;
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }

                    standardFirebaseOperationsClass.activityLog(entry.folio, "transaction", "InvoiceIn", eid);
                    standardFirebaseOperationsClass.UpdateVersion("invoiceIn");
                    if (entry.cid != null) if (entry.cid != "")
                        {
                            UpdateCidTransaction(entry.cid, entry.cid, "invoicesIn");
                        }
                    break;
            }
            if (success)
            {
                standardFirebaseOperationsClass.UpdateVersion("product");
                productProcessingServices.updateQuantityEntry(entry.products, true, entry.folio);
                ExpensesServicesClass expensesServices = new();
                expensesServices.massSave(entry.products, control);
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
                    standardFirebaseOperationsClass.UpdateVersion("customer");
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
        }

        public void UpdateMatchingReceiptIn(string folio, string date)
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    //now we upload our receipt to both refernces
                    FirebaseResponse firebase = client.Set(DatabaseDirectory.InvoicesIn() + "/" + date + "/sd/" + folio + "/matchingReceipt", "true");
                    FirebaseResponse firebasea = client.Set(DatabaseDirectory.InvoicesIn() + "/" + date + "/sd/" + folio + "/paid", "true");
                    FirebaseResponse firebase1 = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + folio + "/matchingReceipt", "true");
                    FirebaseResponse firebase1a = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + folio + "/paid", "true");

                    standardFirebaseOperationsClass.activityLog(folio, "transaction", "InvoiceIn", eid);

                    standardFirebaseOperationsClass.UpdateVersion("invoiceIn");
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
        }

        public void UpdateMatchingReturnsPurchases(Dictionary<string, ProductEntrySDClass> onGoingArray, string folio, string date, bool invYes, bool recYes, Button control, Action<bool> method)
        {//date is for current, cashDate if for date of receipt, creditDate is for date of invoice... now how in gods name do we access date of invoice??
            string cashDate = "";
            string creditDate = "";

            Dictionary<string, DateClass> receiptsIn = prevelantClass.LoadDatesPurchases();
            Dictionary<string, DateClass> invoicesIn = prevelantClass.LoadDatesPurchasesCredit();
            
            foreach(var dateItem in receiptsIn)
            {
                if (dateItem.Value.sd == null) break;
                if (dateItem.Value.sd.Count == 0) break;
                foreach(var item in dateItem.Value.sd)
                {
                    if(item.Key == folio)
                    {
                        cashDate = dateItem.Value.date;
                        break;
                    }
                }
            }

            foreach (var dateItem in invoicesIn)
            {
                if (dateItem.Value.sd == null) break;
                if (dateItem.Value.sd.Count == 0) break;
                foreach (var item in dateItem.Value.sd)
                {
                    if (item.Key == folio)
                    {
                        creditDate = dateItem.Value.date;
                        break;
                    }
                }
            }

            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();


            bool success = false;


            if (invYes)
            {
                success = false;
                if (creditDate != "")
                {
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            //now we upload our receipt to both refernces
                            FirebaseResponse firebase = client.Set(DatabaseDirectory.InvoicesIn() + "/" + creditDate + "/sd/" + folio + "/matchingReturns", "true");
                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + folio + "/matchingReturns", "true");

                            success = true;
                            standardFirebaseOperationsClass.activityLog(folio, "transaction", "InvoiceIn Returns", eid);
                            standardFirebaseOperationsClass.UpdateVersion("invoiceIn");
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                }
            }
            if (recYes)
            {
                success = false;
                if (cashDate != "")
                {
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            //now we upload our receipt to both refernces
                            FirebaseResponse firebase = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + cashDate + "/sd/" + folio + "/matchingReturns", "true");
                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.AllReceiptsIn() + "/" + folio + "/matchingReturns", "true");

                            success = true;
                            standardFirebaseOperationsClass.activityLog(folio, "transaction", "ReceiptIn Returns", eid);
                            standardFirebaseOperationsClass.UpdateVersion("receiptIn");//receiptIn
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                }
            }
            if (success)
            {
                productProcessingServices.updateQuantityDeduction(onGoingArray);
            }
            control.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    method, success);
        }
    }
}
