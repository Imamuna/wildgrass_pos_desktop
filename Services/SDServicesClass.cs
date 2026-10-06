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
using WildGrass_Desktop_f8.Functions.PDF;
using WildGrass_Desktop_f8.Activities.dialogs;

namespace WildGrass_Desktop_f8.Services
{
    /// <summary>
    /// CRUD SD
    /// here we create sd: receipt, invoice, quotation, order, delivery note, transfer note, credit note, debit note
    /// </summary>
    internal class SDServicesClass
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
        public async Task<string> GetContinuousFolioAsync(string type)
        {
            //get current continous folio and add 1
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            int folioNo = 0;
            //TaxInvoice, CreditNote, DebitNote
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = await client.GetAsync(DatabaseDirectory.Folios() + "/" + type);
                    if(res.Body != "null")
                    {
                        folioNo = JsonConvert.DeserializeObject<int>(res.Body.ToString());
                    }
                    folioNo++;
                    FirebaseResponse res1 = client.Set(DatabaseDirectory.Folios() + "/" + type, Convert.ToString(folioNo));
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            int branchNo = 1;
            int branchCode = 1 * 1000000000;
            folioNo += branchCode;
            return Convert.ToString(folioNo);
        }

        public async void UploadResources(SourceDocumentClass2 sourceDocument)
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            if (sourceDocument.sdType == "Receipt" || sourceDocument.sdType == "Invoice" || sourceDocument.sdType == "Transfer Note")
            {
                bool runDeduction = true;
                if(sourceDocument.Matching != null) if(sourceDocument.Matching.Count > 0)
                    {
                        foreach(var item in sourceDocument.Matching)
                        {
                            if(item.Value.sdType == "Receipt" || item.Value.sdType == "Invoice")
                            {
                                if(item.Value.rid != sourceDocument.folio)
                                {
                                    runDeduction = false;
                                }
                            }
                        }
                    }
                if (runDeduction)
                {
                    productProcessingServices.updateQuantity(sourceDocument);
                }
            }

            string dirPdf = sourceDocument.pdfLink;
            if (File.Exists(dirPdf))
            {
                int NumberOfRetries = 3;
                int DelayOnRetry = 3000;
                for (int i = 1; i <= NumberOfRetries; ++i)
                {
                    try
                    {
                        var stream = File.Open(dirPdf, FileMode.Open);

                        // Construct FirebaseStorage, path to where we want to upload the file and Put it there
                        var task = new FirebaseStorage("long-walk-pos.appspot.com")
                            .Child(sourceDocument.sdType + "s")
                            .Child(sourceDocument.folio + "_duplicate.pdf")
                            .PutAsync(stream);

                        // Track progress of the upload
                        task.Progress.ProgressChanged += (s, e) => doneUploading(s, e);

                        // await the task to wait until upload completes and get the download url
                        var downloadUrl = await task;
                        sourceDocument.pdfLink = downloadUrl;
                        break;
                    }
                    catch (IOException e) when (i <= NumberOfRetries)
                    {
                        Thread.Sleep(DelayOnRetry);
                    }
                }
            }

            string dirPdf2 = sourceDocument.pdfLink2;
            if (File.Exists(dirPdf2))
            {
                int NumberOfRetries = 3;
                int DelayOnRetry = 3000;
                for (int i = 1; i <= NumberOfRetries; ++i)
                {
                    try
                    {
                        var stream = File.Open(dirPdf2, FileMode.Open);

                        // Construct FirebaseStorage, path to where we want to upload the file and Put it there
                        var task = new FirebaseStorage("long-walk-pos.appspot.com")
                            .Child(sourceDocument.sdType + "s")
                            .Child(sourceDocument.folio + ".pdf")
                            .PutAsync(stream);

                        // Track progress of the upload
                        task.Progress.ProgressChanged += (s, e) => doneUploading(s, e);

                        // await the task to wait until upload completes and get the download url
                        var downloadUrl = await task;
                        sourceDocument.pdfLink2 = downloadUrl;
                        break;
                    }
                    catch (IOException e) when (i <= NumberOfRetries)
                    {
                        Thread.Sleep(DelayOnRetry);
                    }
                }
            }
            UploadTransactionData(sourceDocument);
        }

        public void OldTransaction(SourceDocumentClass2 sourceDocument)
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            if (sourceDocument.sdType == "Receipt" || sourceDocument.sdType == "Invoice" || sourceDocument.sdType == "Transfer Note")
            {
                productProcessingServices.updateQuantity(sourceDocument);
            }
            UploadTransactionData(sourceDocument);
        }

        private void doneUploading(object s, FirebaseStorageProgress e)
        {
            Console.WriteLine($"Progress: {e.Percentage} %");
            if (e.Percentage == 100)
            {
                //MessageBox.Show("We are at 100%", "system testing");
            }
        }

        public async void UploadTransactionData(SourceDocumentClass2 entry)
        {
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            setup();

            DateClass dateEntry = new DateClass
            {
                comp = entry.month + entry.year,
                date = entry.date,
                day = entry.day,
                month = entry.month,
                year = entry.year
            };

            if (entry.sdType == "Receipt")
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //now we upload our receipt to both refernces
                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Receipts() + "/" + entry.date + "/receipts/" + entry.folio, entry);
                        FirebaseResponse firebase2 = client.Set(DatabaseDirectory.Receipts() + "/" + dateEntry.date + "/date", dateEntry.date);
                        FirebaseResponse firebase3 = client.Set(DatabaseDirectory.Receipts() + "/" + dateEntry.date + "/comp", dateEntry.comp);
                        FirebaseResponse firebase4 = client.Set(DatabaseDirectory.Receipts() + "/" + dateEntry.date + "/day", dateEntry.day);
                        FirebaseResponse firebase5 = client.Set(DatabaseDirectory.Receipts() + "/" + dateEntry.date + "/month", dateEntry.month);
                        FirebaseResponse firebase6 = client.Set(DatabaseDirectory.Receipts() + "/" + dateEntry.date + "/year", dateEntry.year);
                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.AllReceipts() + "/" + entry.folio, entry);

                        //update firebase versions
                        standardFirebaseOperationsClass.UpdateVersion("receipt");
                        standardFirebaseOperationsClass.activityLog(entry.folio, "transaction", "Receipt", eid);
                        if (entry.cid != null) if (entry.cid.Length > 0)
                            {
                                UpdateCidTransaction(entry.cid, entry.folio, "receipts");
                            }
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
            else if (entry.sdType == "Invoice")
            {
                entry.matchingReceipt = "false";
                entry.paid = false;

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //now we upload our invoice to both refernces
                        FirebaseResponse firebase = await client.SetAsync(DatabaseDirectory.Invoices() + "/" + entry.date + "/invoices/" + entry.folio, entry);
                        FirebaseResponse firebase2 = await client.SetAsync(DatabaseDirectory.Invoices() + "/" + entry.date + "/date", dateEntry.date);
                        FirebaseResponse firebase3 = await client.SetAsync(DatabaseDirectory.Invoices() + "/" + entry.date + "/comp", dateEntry.comp);
                        FirebaseResponse firebase4 = await client.SetAsync(DatabaseDirectory.Invoices() + "/" + entry.date + "/day", dateEntry.day);
                        FirebaseResponse firebase5 = await client.SetAsync(DatabaseDirectory.Invoices() + "/" + entry.date + "/month", dateEntry.month);
                        FirebaseResponse firebase6 = await client.SetAsync(DatabaseDirectory.Invoices() + "/" + entry.date + "/year", dateEntry.year);
                        FirebaseResponse firebase1 = await client.SetAsync(DatabaseDirectory.AllInvoices() + "/" + entry.folio, entry);


                        //update firebase versions
                        standardFirebaseOperationsClass.UpdateVersion("invoice");
                        standardFirebaseOperationsClass.activityLog(entry.folio, "transaction", "Invoice", eid);
                        if (entry.cid != null) if (entry.cid.Length > 0)
                            {
                                UpdateCidTransaction(entry.cid, entry.folio, "invoices");
                            }
                        break;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        Thread.Sleep(duration);
                    }
                }
            }
            else if (entry.sdType == "Quotation")
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //now we upload our quotations to both refernces
                        FirebaseResponse firebase1 = await client.SetAsync(DatabaseDirectory.Quotations() + "/" + entry.folio, entry);

                        //update firebase versions
                        standardFirebaseOperationsClass.UpdateVersion("quotation");
                        standardFirebaseOperationsClass.activityLog(entry.folio, "transaction", "Quotation", eid);
                        if (entry.cid != null) if (entry.cid.Length > 0)
                            {
                                UpdateCidTransaction(entry.cid, entry.folio, "quotations");
                            }
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
            else if (entry.sdType == "Order")
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //now we upload our quotations to both refernces
                        FirebaseResponse firebase1 = await client.SetAsync(DatabaseDirectory.Orders() + "/" + entry.folio, entry);

                        //update firebase versions
                        standardFirebaseOperationsClass.UpdateVersion("order");
                        standardFirebaseOperationsClass.activityLog(entry.folio, "transaction", "Order", eid);
                        if (entry.cid != null) if (entry.cid.Length > 0)
                            {
                                UpdateCidTransaction(entry.cid, entry.folio, "orders");
                            }
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
            else if (entry.sdType == "Transfer Note")
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //now we upload our quotations to both refernces
                        FirebaseResponse firebase1 = await client.SetAsync(DatabaseDirectory.TransferNotes() + "/" + entry.folio, entry);
                        
                        //update firebase versions
                        standardFirebaseOperationsClass.UpdateVersion("transferNote");
                        standardFirebaseOperationsClass.activityLog(entry.folio, "transaction", "Transfer Note", eid);
                        if (entry.cid != null) if (entry.cid.Length > 0)
                            {
                                UpdateCidTransaction(entry.cid, entry.folio, "transferNotes");
                            }
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
            else if (entry.sdType == "Credit Note")
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //now we upload our quotations to both refernces
                        FirebaseResponse firebase1 = await client.SetAsync(DatabaseDirectory.CreditNotes() + "/" + entry.folio, entry);

                        //update firebase versions
                        standardFirebaseOperationsClass.UpdateVersion("creditNote");
                        standardFirebaseOperationsClass.activityLog(entry.folio, "transaction", "Credit Note", eid);
                        if (entry.cid != null) if (entry.cid.Length > 0)
                            {
                                UpdateCidTransaction(entry.cid, entry.folio, "creditNote");
                            }
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
            else if (entry.sdType == "Debit Note")
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //now we upload our quotations to both refernces
                        FirebaseResponse firebase1 = await client.SetAsync(DatabaseDirectory.DebitNote() + "/" + entry.folio, entry);

                        //update firebase versions
                        standardFirebaseOperationsClass.UpdateVersion("debitNote");
                        standardFirebaseOperationsClass.activityLog(entry.folio, "transaction", "Debit Note", eid);
                        if (entry.cid != null) if (entry.cid.Length > 0)
                            {
                                UpdateCidTransaction(entry.cid, entry.folio, "debitNote");
                            }
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
            else
            {
                //submit error report to titan
            }

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    //now we upload to client side
                    string dateva = entry.day + entry.month + entry.year;
                    FirebaseResponse firebasea1 = await client.SetAsync(DatabaseDirectory.ClientSds() + "/" + dateva + "/files/" + entry.folio, entry);
                    FirebaseResponse firebasea1a = await client.SetAsync(DatabaseDirectory.ClientSds() + "/" + dateva + "/files/" + entry.folio + "/pdfLink", entry.pdfLink2);
                    FirebaseResponse firebasea2 = await client.SetAsync(DatabaseDirectory.ClientSds() + "/" + dateva + "/date", dateEntry.date);
                    FirebaseResponse firebasea3 = await client.SetAsync(DatabaseDirectory.ClientSds() + "/" + dateva + "/comp", dateEntry.comp);
                    FirebaseResponse firebasea4 = await client.SetAsync(DatabaseDirectory.ClientSds() + "/" + dateva + "/day", dateEntry.day);
                    FirebaseResponse firebasea5 = await client.SetAsync(DatabaseDirectory.ClientSds() + "/" + dateva + "/month", dateEntry.month);
                    FirebaseResponse firebasea6 = await client.SetAsync(DatabaseDirectory.ClientSds() + "/" + dateva + "/year", dateEntry.year);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            if (entry.smsYes)
            {
                BusinessClass business = prevelantClass.GetBusiness();

                SendClass send = new SendClass();
                send.ViaSMS(entry, entry.customerPhoneNumber, business.cmSenderId);
            }
            if (entry.emailYes)
            {
                EmailServiceClass emailServiceClass = new EmailServiceClass();
                string docType = entry.sdType + "s";
                if (entry.sdType == "Quotation") docType = "Quotaions";
                emailServiceClass.WebSend(entry.date, entry.folio, docType);
            }

            if(entry.sdType == "Receipt" || entry.sdType == "Invoice")
            {
                if (!entry.matchingDNote)
                {
                    SettingsClass settingsClass = prevelantClass.RetrieveSettings();
                    if (settingsClass != null) if (settingsClass.bookKeepingSettings != null)
                        {
                            if (settingsClass.bookKeepingSettings.AutoGenerateDeliveryNote)
                            {
                                autoGenerateDeliveryNote(entry);
                            }
                        }
                }
            }
        }

        public void printPDF(string dirPdf, string printerName)
        {
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    PdfDocument pdfdocument = new PdfDocument();
                    pdfdocument.LoadFromFile(dirPdf);
                    pdfdocument.PrintSettings.PrinterName = printerName;
                    pdfdocument.PrintSettings.Copies = 1;
                    pdfdocument.Print();
                    pdfdocument.Dispose();
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
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

        public void addToQueuedData(SourceDocumentClass2 sourceDocument, Action loadUI, TextBlock control)
        {
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            //retrieve or create que
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            string dir = complete + @"\data\" + uid + @"\businesses\" + bid;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            dir += @"\QueuedData.txt";

            Dictionary<string, SourceDocumentClass2> queuedDataArray = new Dictionary<string, SourceDocumentClass2>();
            if (File.Exists(dir))
            {
                using (StreamReader r = new StreamReader(dir))
                {
                    string json = r.ReadToEnd();
                    if (json != null)
                    {
                        queuedDataArray = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(json);
                    }
                }
            }

            if (queuedDataArray == null)
            {
                queuedDataArray = new Dictionary<string, SourceDocumentClass2>();
            }

            queuedDataArray.Add(sourceDocument.folio, sourceDocument);

            // serialize and save data
            int countC = 0;
            string customSerialized = "{";
            foreach (var user in queuedDataArray)
            {
                if (countC != 0)
                {
                    customSerialized += ",";
                }

                customSerialized += (char)34 + user.Key + (char)34 + ": {";
                customSerialized += (char)34 + "businessName" + (char)34 + ":" + (char)34 + user.Value.businessName + (char)34 + ",";
                customSerialized += (char)34 + "localArea" + (char)34 + ":" + (char)34 + user.Value.localArea + (char)34 + ",";
                customSerialized += (char)34 + "addCost" + (char)34 + ":" + (char)34 + user.Value.addCost + (char)34 + ",";
                customSerialized += (char)34 + "addCostName" + (char)34 + ":" + (char)34 + user.Value.addCostName + (char)34 + ",";
                customSerialized += (char)34 + "customerEmail" + (char)34 + ":" + (char)34 + user.Value.customerEmail + (char)34 + ",";
                customSerialized += (char)34 + "customerName" + (char)34 + ":" + (char)34 + user.Value.customerName + (char)34 + ",";
                customSerialized += (char)34 + "customerPhoneNumber" + (char)34 + ":" + (char)34 + user.Value.customerPhoneNumber + (char)34 + ",";
                customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + user.Value.date + (char)34 + ",";
                customSerialized += (char)34 + "day" + (char)34 + ":" + (char)34 + user.Value.day + (char)34 + ",";
                customSerialized += (char)34 + "discount" + (char)34 + ":" + (char)34 + user.Value.discount + (char)34 + ",";
                customSerialized += (char)34 + "discountName" + (char)34 + ":" + (char)34 + user.Value.discountName + (char)34 + ",";
                customSerialized += (char)34 + "exclVat" + (char)34 + ":" + (char)34 + user.Value.exclVat + (char)34 + ",";
                customSerialized += (char)34 + "folio" + (char)34 + ":" + (char)34 + user.Value.folio + (char)34 + ",";
                customSerialized += (char)34 + "inclVat" + (char)34 + ":" + (char)34 + user.Value.inclVat + (char)34 + ",";
                customSerialized += (char)34 + "month" + (char)34 + ":" + (char)34 + user.Value.month + (char)34 + ",";
                customSerialized += (char)34 + "tagEid" + (char)34 + ":" + (char)34 + user.Value.tagEid + (char)34 + ",";
                customSerialized += (char)34 + "valideTillDate" + (char)34 + ":" + (char)34 + user.Value.valideTillDate + (char)34 + ",";
                customSerialized += (char)34 + "pdfLink" + (char)34 + ":" + (char)34 + "" + (char)34 + ",";
                customSerialized += (char)34 + "tagName" + (char)34 + ":" + (char)34 + user.Value.tagName + (char)34 + ",";
                customSerialized += (char)34 + "time" + (char)34 + ":" + (char)34 + user.Value.time + (char)34 + ",";
                customSerialized += (char)34 + "total" + (char)34 + ":" + (char)34 + user.Value.total + (char)34 + ",";
                customSerialized += (char)34 + "type" + (char)34 + ":" + (char)34 + user.Value.type + (char)34 + ",";
                customSerialized += (char)34 + "sdType" + (char)34 + ":" + (char)34 + user.Value.sdType + (char)34 + ",";
                customSerialized += (char)34 + "vday" + (char)34 + ":" + (char)34 + user.Value.vday + (char)34 + ",";
                customSerialized += (char)34 + "vmonth" + (char)34 + ":" + (char)34 + user.Value.vmonth + (char)34 + ",";
                customSerialized += (char)34 + "vyear" + (char)34 + ":" + (char)34 + user.Value.vyear + (char)34 + ",";
                customSerialized += (char)34 + "year" + (char)34 + ":" + (char)34 + user.Value.year + (char)34 + ",";

                //additional variables
                customSerialized += (char)34 + "smsYes" + (char)34 + ":" + (char)34 + user.Value.smsYes + (char)34 + ",";
                customSerialized += (char)34 + "emailYes" + (char)34 + ":" + (char)34 + user.Value.emailYes + (char)34 + ",";
                customSerialized += (char)34 + "printYes" + (char)34 + ":" + (char)34 + user.Value.printYes + (char)34 + ",";
                customSerialized += (char)34 + "includeCustomerDetails" + (char)34 + ":" + (char)34 + user.Value.includeCustomerDetails + (char)34 + ",";
                customSerialized += (char)34 + "currency" + (char)34 + ":" + (char)34 + user.Value.currency + (char)34 + ",";
                customSerialized += (char)34 + "customerBank" + (char)34 + ":" + (char)34 + user.Value.customerBank + (char)34 + ",";
                customSerialized += (char)34 + "customerAddress" + (char)34 + ":" + (char)34 + user.Value.customerAddress + (char)34 + ",";
                customSerialized += (char)34 + "customerTpin" + (char)34 + ":" + (char)34 + user.Value.customerTpin + (char)34 + ",";
                customSerialized += (char)34 + "customerFirstName" + (char)34 + ":" + (char)34 + user.Value.customerFirstName + (char)34 + ",";
                customSerialized += (char)34 + "customerLastName" + (char)34 + ":" + (char)34 + user.Value.customerLastName + (char)34 + ",";

                //even more additional variables
                customSerialized += (char)34 + "customId" + (char)34 + ":" + (char)34 + user.Value.customId + (char)34 + ",";
                customSerialized += (char)34 + "customIdYes" + (char)34 + ":" + (char)34 + user.Value.customIdYes + (char)34 + ",";
                customSerialized += (char)34 + "rate" + (char)34 + ":" + (char)34 + user.Value.rate + (char)34 + ",";
                customSerialized += (char)34 + "oldCurrency" + (char)34 + ":" + (char)34 + user.Value.oldCurrency + (char)34 + ",";
                customSerialized += (char)34 + "printerSelected" + (char)34 + ":" + (char)34 + user.Value.printerSelected + (char)34 + ",";
                customSerialized += (char)34 + "paymentMethod" + (char)34 + ":" + (char)34 + user.Value.paymentMethod + (char)34 + ",";
                customSerialized += (char)34 + "matchingQuotation" + (char)34 + ":" + (char)34 + user.Value.matchingQuotation + (char)34 + ",";
                customSerialized += (char)34 + "matchingOrder" + (char)34 + ":" + (char)34 + user.Value.matchingOrder + (char)34 + ",";
                customSerialized += (char)34 + "matchingDNote" + (char)34 + ":" + (char)34 + user.Value.matchingDNote + (char)34 + ",";
                customSerialized += (char)34 + "cid" + (char)34 + ":" + (char)34 + user.Value.cid + (char)34 + ",";
                customSerialized += (char)34 + "dNoteUrl" + (char)34 + ":" + (char)34 + user.Value.dNoteUrl + (char)34 + ",";

                customSerialized += (char)34 + "writtenOff" + (char)34 + ":" + (char)34 + user.Value.writtenOff + (char)34 + ",";
                customSerialized += (char)34 + "archived" + (char)34 + ":" + (char)34 + user.Value.archived + (char)34 + ",";
                customSerialized += (char)34 + "PaymentPlanYes" + (char)34 + ":" + (char)34 + user.Value.PaymentPlanYes + (char)34 + ",";
                customSerialized += (char)34 + "EditedYes" + (char)34 + ":" + (char)34 + user.Value.EditedYes + (char)34 + ",";
                customSerialized += (char)34 + "EditedDate" + (char)34 + ":" + (char)34 + user.Value.EditedDate + (char)34 + ",";
                customSerialized += (char)34 + "EditedTime" + (char)34 + ":" + (char)34 + user.Value.EditedTime + (char)34 + ",";

                if(user.Value.summations != null)
                {
                    customSerialized += (char)34 + "summations" + (char)34 + ": {";

                    customSerialized += (char)34 + "AddCost" + (char)34 + ":" + (char)34 + user.Value.summations.AddCost + (char)34 + ",";
                    customSerialized += (char)34 + "Discount" + (char)34 + ":" + (char)34 + user.Value.summations.Discount + (char)34 + ",";
                    customSerialized += (char)34 + "IndAddCost" + (char)34 + ":" + (char)34 + user.Value.summations.IndAddCost + (char)34 + ",";
                    customSerialized += (char)34 + "IndDiscount" + (char)34 + ":" + (char)34 + user.Value.summations.IndDiscount + (char)34 + ",";
                    customSerialized += (char)34 + "globalAddCost" + (char)34 + ":" + (char)34 + user.Value.summations.globalAddCost + (char)34 + ",";
                    customSerialized += (char)34 + "globalDiscount" + (char)34 + ":" + (char)34 + user.Value.summations.globalDiscount + (char)34 + ",";
                    customSerialized += (char)34 + "Excl" + (char)34 + ":" + (char)34 + user.Value.summations.Excl + (char)34 + ",";
                    customSerialized += (char)34 + "Incl" + (char)34 + ":" + (char)34 + user.Value.summations.Incl + (char)34 + ",";
                    customSerialized += (char)34 + "Vat" + (char)34 + ":" + (char)34 + user.Value.summations.Vat + (char)34 + ",";
                    customSerialized += (char)34 + "Cart" + (char)34 + ":" + (char)34 + user.Value.summations.Cart + (char)34 + ",";
                    customSerialized += (char)34 + "Total" + (char)34 + ":" + (char)34 + user.Value.summations.Total + (char)34;

                    customSerialized += "},";
                }

                if (user.Value.Matching != null) if(user.Value.Matching.Count > 0)
                    {
                        customSerialized += (char)34 + "Matching" + (char)34 + ": {";

                        int countD = 0;
                        foreach (var item in user.Value.Matching)
                        {
                            if (countD != 0)
                            {
                                customSerialized += ",";
                            }
                            customSerialized += (char)34 + item.Key + (char)34 + ": {";
                            customSerialized += (char)34 + "rid" + (char)34 + ":" + (char)34 + item.Value.rid + (char)34 + ",";
                            customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + item.Value.date + (char)34 + ",";
                            customSerialized += (char)34 + "sdType" + (char)34 + ":" + (char)34 + item.Value.sdType + (char)34;
                            customSerialized += "}";

                            countD = 1;
                        }
                        customSerialized += "},";
                    }

                if (user.Value.PaymentPlanEntries != null) if (user.Value.PaymentPlanEntries.Count > 0)
                    {
                        customSerialized += (char)34 + "PaymentPlanEntries" + (char)34 + ": {";

                        int countD = 0;
                        foreach (var item in user.Value.PaymentPlanEntries)
                        {
                            if (countD != 0)
                            {
                                customSerialized += ",";
                            }
                            customSerialized += (char)34 + item.Key + (char)34 + ": {";
                            customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + item.Value.date + (char)34 + ",";
                            customSerialized += (char)34 + "day" + (char)34 + ":" + (char)34 + item.Value.day + (char)34 + ",";
                            customSerialized += (char)34 + "month" + (char)34 + ":" + (char)34 + item.Value.month + (char)34 + ",";
                            customSerialized += (char)34 + "year" + (char)34 + ":" + (char)34 + item.Value.year + (char)34 + ",";
                            customSerialized += (char)34 + "writtenOff" + (char)34 + ":" + (char)34 + item.Value.writtenOff + (char)34 + ",";
                            customSerialized += (char)34 + "total" + (char)34 + ":" + (char)34 + item.Value.total + (char)34 + ",";
                            customSerialized += (char)34 + "paid" + (char)34 + ":" + (char)34 + item.Value.paid + (char)34 + ",";
                            customSerialized += (char)34 + "matchingReceipt" + (char)34 + ":" + (char)34 + item.Value.matchingReceipt + (char)34;
                            customSerialized += "}";

                            countD = 1;
                        }
                        customSerialized += "},";
                    }

                if (user.Value.EditHistory != null) if (user.Value.EditHistory.Count > 0)
                    {
                        customSerialized += (char)34 + "EditHistory" + (char)34 + ": {";

                        int countD = 0;
                        foreach (var item in user.Value.EditHistory)
                        {
                            if (countD != 0)
                            {
                                customSerialized += ",";
                            }
                            customSerialized += (char)34 + item.Key + (char)34 + ": {";
                            customSerialized += (char)34 + "rid" + (char)34 + ":" + (char)34 + item.Value.rid + (char)34 + ",";
                            customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + item.Value.date + (char)34 + ",";
                            customSerialized += (char)34 + "sdType" + (char)34 + ":" + (char)34 + item.Value.sdType + (char)34;
                            customSerialized += "}";

                            countD = 1;
                        }
                        customSerialized += "},";
                    }



                if (user.Value.products != null)
                {
                    Dictionary<string, ProductEntrySDClass> productsQueued = user.Value.products;
                    int countD = 0;
                    if (productsQueued.Count != 0)
                    {
                        customSerialized += (char)34 + "products" + (char)34 + ": {";

                        foreach (var product in productsQueued)
                        {
                            if (countD != 0)
                            {
                                customSerialized += ",";
                            }

                            customSerialized += (char)34 + product.Key + (char)34 + ": {";
                            customSerialized += (char)34 + "brandName" + (char)34 + ":" + (char)34 + product.Value.brandName + (char)34 + ",";
                            customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + product.Value.date + (char)34 + ",";
                            customSerialized += (char)34 + "day" + (char)34 + ":" + (char)34 + product.Value.day + (char)34 + ",";
                            customSerialized += (char)34 + "flavor" + (char)34 + ":" + (char)34 + product.Value.flavor + (char)34 + ",";
                            customSerialized += (char)34 + "month" + (char)34 + ":" + (char)34 + product.Value.month + (char)34 + ",";
                            customSerialized += (char)34 + "price" + (char)34 + ":" + (char)34 + product.Value.price + (char)34 + ",";
                            customSerialized += (char)34 + "productID" + (char)34 + ":" + (char)34 + product.Value.productID + (char)34 + ",";
                            customSerialized += (char)34 + "productName" + (char)34 + ":" + (char)34 + product.Value.productName + (char)34 + ",";
                            customSerialized += (char)34 + "quantity" + (char)34 + ":" + (char)34 + product.Value.quantity + (char)34 + ",";
                            customSerialized += (char)34 + "rid" + (char)34 + ":" + (char)34 + product.Value.rid + (char)34 + ",";
                            customSerialized += (char)34 + "size" + (char)34 + ":" + (char)34 + product.Value.size + (char)34 + ",";
                            customSerialized += (char)34 + "year" + (char)34 + ":" + (char)34 + product.Value.year + (char)34 + ",";
                            customSerialized += (char)34 + "expiryDateTracking" + (char)34 + ":" + (char)34 + product.Value.expiryDateTracking + (char)34 + ",";
                            customSerialized += (char)34 + "serialNumberYes" + (char)34 + ":" + (char)34 + product.Value.serialNumberYes + (char)34;

                            if (product.Value.serialNumbers != null) if (product.Value.serialNumbers.Count > 0)
                                {
                                    customSerialized += ",";
                                    customSerialized += (char)34 + "serialNumbers" + (char)34 + ": {";
                                    int countS = 0;
                                    foreach (var entry in product.Value.serialNumbers)
                                    {
                                        if (countS != 0)
                                        {
                                            customSerialized += ",";
                                        }
                                        customSerialized += (char)34 + entry.Key + (char)34 + ": {";
                                        customSerialized += (char)34 + "serialNumber" + (char)34 + ":" + (char)34 + entry.Value.serialNumber + (char)34 + ",";
                                        customSerialized += (char)34 + "vDate" + (char)34 + ":" + (char)34 + entry.Value.vDate + (char)34 + ",";
                                        customSerialized += (char)34 + "vMonth" + (char)34 + ":" + (char)34 + entry.Value.vMonth + (char)34 + ",";
                                        customSerialized += (char)34 + "vDay" + (char)34 + ":" + (char)34 + entry.Value.vDay + (char)34 + ",";
                                        customSerialized += (char)34 + "vYear" + (char)34 + ":" + (char)34 + entry.Value.vYear + (char)34 + ",";
                                        customSerialized += (char)34 + "pid" + (char)34 + ":" + (char)34 + entry.Value.pid + (char)34 + ",";
                                        customSerialized += (char)34 + "expYes" + (char)34 + ":" + (char)34 + entry.Value.expYes + (char)34;
                                        customSerialized += "}";
                                        countS = 1;
                                    }
                                    customSerialized += "},";
                                }
                            if (product.Value.expiryDateBatches != null) if (product.Value.expiryDateBatches.Count > 0)
                                {
                                    customSerialized += (char)34 + "expiryDateBatches" + (char)34 + ": {";
                                    int countT = 0;
                                    foreach (var entry in product.Value.expiryDateBatches)
                                    {
                                        if (countT != 0)
                                        {
                                            customSerialized += ",";
                                        }
                                        customSerialized += (char)34 + entry.Key + (char)34 + ": {";
                                        customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + entry.Value.date + (char)34 + ",";
                                        customSerialized += (char)34 + "QtyInStock" + (char)34 + ":" + (char)34 + entry.Value.QtyInStock + (char)34 + ",";
                                        customSerialized += (char)34 + "month" + (char)34 + ":" + (char)34 + entry.Value.month + (char)34 + ",";
                                        customSerialized += (char)34 + "day" + (char)34 + ":" + (char)34 + entry.Value.day + (char)34 + ",";
                                        customSerialized += (char)34 + "year" + (char)34 + ":" + (char)34 + entry.Value.year + (char)34;
                                        customSerialized += "}";
                                        countT = 1;
                                    }
                                    customSerialized += "}";
                                }
                            customSerialized += "}";

                            countD = 1;
                        }
                        customSerialized += "},"; //all products
                    }
                    else
                    {
                        customSerialized += (char)34 + "products" + (char)34 + ":" + (char)34 + "," + (char)34;
                    }
                }
                else
                {
                    customSerialized += (char)34 + "products" + (char)34 + ":" + (char)34 + "," + (char)34;
                }
                if (user.Value.onGoing != null)
                {
                    Dictionary<string, OnGoingClass> onGoingQueuedArray = user.Value.onGoing;
                    int countD = 0;
                    if (onGoingQueuedArray.Count != 0)
                    {
                        customSerialized += (char)34 + "onGoing" + (char)34 + ": {";

                        foreach (var product in onGoingQueuedArray)
                        {
                            if (countD != 0)
                            {
                                customSerialized += ",";
                            }

                            customSerialized += (char)34 + product.Key + (char)34 + ": {";
                            customSerialized += (char)34 + "brandName" + (char)34 + ":" + (char)34 + product.Value.brandName + (char)34 + ",";
                            customSerialized += (char)34 + "flavor" + (char)34 + ":" + (char)34 + product.Value.flavor + (char)34 + ",";
                            customSerialized += (char)34 + "price" + (char)34 + ":" + (char)34 + product.Value.price + (char)34 + ",";
                            customSerialized += (char)34 + "productID" + (char)34 + ":" + (char)34 + product.Value.productID + (char)34 + ",";
                            customSerialized += (char)34 + "productName" + (char)34 + ":" + (char)34 + product.Value.productName + (char)34 + ",";
                            customSerialized += (char)34 + "quantity" + (char)34 + ":" + (char)34 + product.Value.quantity + (char)34 + ",";
                            customSerialized += (char)34 + "size" + (char)34 + ":" + (char)34 + product.Value.size + (char)34 + ",";
                            customSerialized += (char)34 + "counter" + (char)34 + ":" + (char)34 + product.Value.counter + (char)34 + ",";
                            customSerialized += (char)34 + "qtyInStock" + (char)34 + ":" + (char)34 + product.Value.qtyInStock + (char)34 + ",";
                            customSerialized += (char)34 + "tracking" + (char)34 + ":" + (char)34 + product.Value.tracking + (char)34 + ",";
                            customSerialized += (char)34 + "vat" + (char)34 + ":" + (char)34 + product.Value.vat + (char)34 + ",";
                            customSerialized += (char)34 + "barcode" + (char)34 + ":" + (char)34 + product.Value.barcode + (char)34 + ",";
                            customSerialized += (char)34 + "expiryDateTracking" + (char)34 + ":" + (char)34 + product.Value.expiryDateTracking + (char)34 + ",";
                            customSerialized += (char)34 + "serialNumberYes" + (char)34 + ":" + (char)34 + product.Value.serialNumberYes + (char)34;

                            if (product.Value.serialNumbers != null) if (product.Value.serialNumbers.Count > 0)
                                {
                                    customSerialized += ",";
                                    customSerialized += (char)34 + "serialNumbers" + (char)34 + ": {";
                                    int countS = 0;
                                    foreach (var entry in product.Value.serialNumbers)
                                    {
                                        if (countS != 0)
                                        {
                                            customSerialized += ",";
                                        }
                                        customSerialized += (char)34 + entry.Key + (char)34 + ": {";
                                        customSerialized += (char)34 + "serialNumber" + (char)34 + ":" + (char)34 + entry.Value.serialNumber + (char)34 + ",";
                                        customSerialized += (char)34 + "vDate" + (char)34 + ":" + (char)34 + entry.Value.vDate + (char)34 + ",";
                                        customSerialized += (char)34 + "vMonth" + (char)34 + ":" + (char)34 + entry.Value.vMonth + (char)34 + ",";
                                        customSerialized += (char)34 + "vDay" + (char)34 + ":" + (char)34 + entry.Value.vDay + (char)34 + ",";
                                        customSerialized += (char)34 + "vYear" + (char)34 + ":" + (char)34 + entry.Value.vYear + (char)34 + ",";
                                        customSerialized += (char)34 + "pid" + (char)34 + ":" + (char)34 + entry.Value.pid + (char)34 + ",";
                                        customSerialized += (char)34 + "expYes" + (char)34 + ":" + (char)34 + entry.Value.expYes + (char)34;
                                        customSerialized += "}";
                                        countS = 1;
                                    }
                                    customSerialized += "}";
                                }
                            if (product.Value.expiryDateBatches != null) if (product.Value.expiryDateBatches.Count > 0)
                                {
                                    customSerialized += ",";
                                    customSerialized += (char)34 + "expiryDateBatches" + (char)34 + ": {";
                                    int countT = 0;
                                    foreach (var entry in product.Value.expiryDateBatches)
                                    {
                                        if (countT != 0)
                                        {
                                            customSerialized += ",";
                                        }
                                        customSerialized += (char)34 + entry.Key + (char)34 + ": {";
                                        customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + entry.Value.date + (char)34 + ",";
                                        customSerialized += (char)34 + "QtyInStock" + (char)34 + ":" + (char)34 + entry.Value.QtyInStock + (char)34 + ",";
                                        customSerialized += (char)34 + "month" + (char)34 + ":" + (char)34 + entry.Value.month + (char)34 + ",";
                                        customSerialized += (char)34 + "day" + (char)34 + ":" + (char)34 + entry.Value.day + (char)34 + ",";
                                        customSerialized += (char)34 + "year" + (char)34 + ":" + (char)34 + entry.Value.year + (char)34;
                                        customSerialized += "}";
                                        countT = 1;
                                    }
                                    customSerialized += "}";
                                }
                            customSerialized += "}"; //individual onGoing entries

                            countD = 1;
                        }
                        customSerialized += "}"; //all onGoing entries
                    }
                    else
                    {
                        customSerialized += (char)34 + "onGoing" + (char)34 + ":" + (char)34 + "" + (char)34;
                    }
                }
                else
                {
                    customSerialized += (char)34 + "onGoing" + (char)34 + ":" + (char)34 + "" + (char)34;
                }
                customSerialized += "}"; //individual entry close
                countC = 1;
            }
            customSerialized += "}"; //all entries close

            System.IO.File.WriteAllText(dir, customSerialized);

            //what do we do now? maybe update the queued data visual display
            control.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    loadUI);
        }

        public void RecreateQueuedData(Dictionary<string, SourceDocumentClass2> queuedDataArray, Dictionary<int, string> resolvedDataArray) //when we are done we reload the display that shows queued data?
        {
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            //retrieve or create que
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            string dir = complete + @"\data\" + uid + @"\businesses\" + bid;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            dir += @"\QueuedData.txt";

            foreach (var item in resolvedDataArray)
            {
                if (queuedDataArray.ContainsKey(item.Value)) queuedDataArray.Remove(item.Value);
            }

            if (queuedDataArray == null)
            {
                try
                {
                    File.Delete(complete + @"\data\" + uid + @"\businesses\" + bid + @"\QueuedData.txt");
                }
                catch (Exception)
                {
                    System.IO.File.WriteAllText(complete + @"\data\" + uid + @"\businesses\" + bid + @"\QueuedData.txt", "");
                }
            }
            else
            {
                if (queuedDataArray.Count == 0)
                {
                    try
                    {
                        File.Delete(complete + @"\data\" + uid + @"\businesses\" + bid + @"\QueuedData.txt");
                    }
                    catch (Exception)
                    {
                        System.IO.File.WriteAllText(complete + @"\data\" + uid + @"\businesses\" + bid + @"\QueuedData.txt", "");
                    }
                }
                else
                {
                    // serialize and save data
                    int countC = 0;
                    string customSerialized = "{";
                    foreach (var user in queuedDataArray)
                    {
                        if (countC != 0)
                        {
                            customSerialized += ",";
                        }

                        customSerialized += (char)34 + user.Key + (char)34 + ": {";
                        customSerialized += (char)34 + "businessName" + (char)34 + ":" + (char)34 + user.Value.businessName + (char)34 + ",";
                        customSerialized += (char)34 + "localArea" + (char)34 + ":" + (char)34 + user.Value.localArea + (char)34 + ",";
                        customSerialized += (char)34 + "addCost" + (char)34 + ":" + (char)34 + user.Value.addCost + (char)34 + ",";
                        customSerialized += (char)34 + "addCostName" + (char)34 + ":" + (char)34 + user.Value.addCostName + (char)34 + ",";
                        customSerialized += (char)34 + "customerEmail" + (char)34 + ":" + (char)34 + user.Value.customerEmail + (char)34 + ",";
                        customSerialized += (char)34 + "customerName" + (char)34 + ":" + (char)34 + user.Value.customerName + (char)34 + ",";
                        customSerialized += (char)34 + "customerPhoneNumber" + (char)34 + ":" + (char)34 + user.Value.customerPhoneNumber + (char)34 + ",";
                        customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + user.Value.date + (char)34 + ",";
                        customSerialized += (char)34 + "day" + (char)34 + ":" + (char)34 + user.Value.day + (char)34 + ",";
                        customSerialized += (char)34 + "discount" + (char)34 + ":" + (char)34 + user.Value.discount + (char)34 + ",";
                        customSerialized += (char)34 + "discountName" + (char)34 + ":" + (char)34 + user.Value.discountName + (char)34 + ",";
                        customSerialized += (char)34 + "exclVat" + (char)34 + ":" + (char)34 + user.Value.exclVat + (char)34 + ",";
                        customSerialized += (char)34 + "folio" + (char)34 + ":" + (char)34 + user.Value.folio + (char)34 + ",";
                        customSerialized += (char)34 + "inclVat" + (char)34 + ":" + (char)34 + user.Value.inclVat + (char)34 + ",";
                        customSerialized += (char)34 + "month" + (char)34 + ":" + (char)34 + user.Value.month + (char)34 + ",";
                        customSerialized += (char)34 + "tagEid" + (char)34 + ":" + (char)34 + user.Value.tagEid + (char)34 + ",";
                        customSerialized += (char)34 + "valideTillDate" + (char)34 + ":" + (char)34 + user.Value.valideTillDate + (char)34 + ",";
                        customSerialized += (char)34 + "pdfLink" + (char)34 + ":" + (char)34 + "" + (char)34 + ",";
                        customSerialized += (char)34 + "tagName" + (char)34 + ":" + (char)34 + user.Value.tagName + (char)34 + ",";
                        customSerialized += (char)34 + "time" + (char)34 + ":" + (char)34 + user.Value.time + (char)34 + ",";
                        customSerialized += (char)34 + "total" + (char)34 + ":" + (char)34 + user.Value.total + (char)34 + ",";
                        customSerialized += (char)34 + "type" + (char)34 + ":" + (char)34 + user.Value.type + (char)34 + ",";
                        customSerialized += (char)34 + "sdType" + (char)34 + ":" + (char)34 + user.Value.sdType + (char)34 + ",";
                        customSerialized += (char)34 + "vday" + (char)34 + ":" + (char)34 + user.Value.vday + (char)34 + ",";
                        customSerialized += (char)34 + "vmonth" + (char)34 + ":" + (char)34 + user.Value.vmonth + (char)34 + ",";
                        customSerialized += (char)34 + "vyear" + (char)34 + ":" + (char)34 + user.Value.vyear + (char)34 + ",";
                        customSerialized += (char)34 + "year" + (char)34 + ":" + (char)34 + user.Value.year + (char)34 + ",";

                        //additional variables
                        customSerialized += (char)34 + "smsYes" + (char)34 + ":" + (char)34 + user.Value.smsYes + (char)34 + ",";
                        customSerialized += (char)34 + "emailYes" + (char)34 + ":" + (char)34 + user.Value.emailYes + (char)34 + ",";
                        customSerialized += (char)34 + "printYes" + (char)34 + ":" + (char)34 + user.Value.printYes + (char)34 + ",";
                        customSerialized += (char)34 + "includeCustomerDetails" + (char)34 + ":" + (char)34 + user.Value.includeCustomerDetails + (char)34 + ",";
                        customSerialized += (char)34 + "currency" + (char)34 + ":" + (char)34 + user.Value.currency + (char)34 + ",";
                        customSerialized += (char)34 + "customerBank" + (char)34 + ":" + (char)34 + user.Value.customerBank + (char)34 + ",";
                        customSerialized += (char)34 + "customerAddress" + (char)34 + ":" + (char)34 + user.Value.customerAddress + (char)34 + ",";
                        customSerialized += (char)34 + "customerTpin" + (char)34 + ":" + (char)34 + user.Value.customerTpin + (char)34 + ",";
                        customSerialized += (char)34 + "customerFirstName" + (char)34 + ":" + (char)34 + user.Value.customerFirstName + (char)34 + ",";
                        customSerialized += (char)34 + "customerLastName" + (char)34 + ":" + (char)34 + user.Value.customerLastName + (char)34 + ",";

                        //even more additional variables
                        customSerialized += (char)34 + "customId" + (char)34 + ":" + (char)34 + user.Value.customId + (char)34 + ",";
                        customSerialized += (char)34 + "customIdYes" + (char)34 + ":" + (char)34 + user.Value.customIdYes + (char)34 + ",";
                        customSerialized += (char)34 + "rate" + (char)34 + ":" + (char)34 + user.Value.rate + (char)34 + ",";
                        customSerialized += (char)34 + "oldCurrency" + (char)34 + ":" + (char)34 + user.Value.oldCurrency + (char)34 + ",";
                        customSerialized += (char)34 + "printerSelected" + (char)34 + ":" + (char)34 + user.Value.printerSelected + (char)34 + ",";
                        customSerialized += (char)34 + "paymentMethod" + (char)34 + ":" + (char)34 + user.Value.paymentMethod + (char)34 + ",";
                        customSerialized += (char)34 + "matchingQuotation" + (char)34 + ":" + (char)34 + user.Value.matchingQuotation + (char)34 + ",";
                        customSerialized += (char)34 + "matchingOrder" + (char)34 + ":" + (char)34 + user.Value.matchingOrder + (char)34 + ",";
                        customSerialized += (char)34 + "matchingDNote" + (char)34 + ":" + (char)34 + user.Value.matchingDNote + (char)34 + ",";
                        customSerialized += (char)34 + "cid" + (char)34 + ":" + (char)34 + user.Value.cid + (char)34 + ",";
                        customSerialized += (char)34 + "dNoteUrl" + (char)34 + ":" + (char)34 + user.Value.dNoteUrl + (char)34 + ",";

                        customSerialized += (char)34 + "writtenOff" + (char)34 + ":" + (char)34 + user.Value.writtenOff + (char)34 + ",";
                        customSerialized += (char)34 + "archived" + (char)34 + ":" + (char)34 + user.Value.archived + (char)34 + ",";
                        customSerialized += (char)34 + "PaymentPlanYes" + (char)34 + ":" + (char)34 + user.Value.PaymentPlanYes + (char)34 + ",";
                        customSerialized += (char)34 + "EditedYes" + (char)34 + ":" + (char)34 + user.Value.EditedYes + (char)34 + ",";
                        customSerialized += (char)34 + "EditedDate" + (char)34 + ":" + (char)34 + user.Value.EditedDate + (char)34 + ",";
                        customSerialized += (char)34 + "EditedTime" + (char)34 + ":" + (char)34 + user.Value.EditedTime + (char)34 + ",";

                        if (user.Value.summations != null)
                        {
                            customSerialized += (char)34 + "summations" + (char)34 + ": {";

                            customSerialized += (char)34 + "AddCost" + (char)34 + ":" + (char)34 + user.Value.summations.AddCost + (char)34 + ",";
                            customSerialized += (char)34 + "Discount" + (char)34 + ":" + (char)34 + user.Value.summations.Discount + (char)34 + ",";
                            customSerialized += (char)34 + "IndAddCost" + (char)34 + ":" + (char)34 + user.Value.summations.IndAddCost + (char)34 + ",";
                            customSerialized += (char)34 + "IndDiscount" + (char)34 + ":" + (char)34 + user.Value.summations.IndDiscount + (char)34 + ",";
                            customSerialized += (char)34 + "globalAddCost" + (char)34 + ":" + (char)34 + user.Value.summations.globalAddCost + (char)34 + ",";
                            customSerialized += (char)34 + "globalDiscount" + (char)34 + ":" + (char)34 + user.Value.summations.globalDiscount + (char)34 + ",";
                            customSerialized += (char)34 + "Excl" + (char)34 + ":" + (char)34 + user.Value.summations.Excl + (char)34 + ",";
                            customSerialized += (char)34 + "Incl" + (char)34 + ":" + (char)34 + user.Value.summations.Incl + (char)34 + ",";
                            customSerialized += (char)34 + "Vat" + (char)34 + ":" + (char)34 + user.Value.summations.Vat + (char)34 + ",";
                            customSerialized += (char)34 + "Cart" + (char)34 + ":" + (char)34 + user.Value.summations.Cart + (char)34 + ",";
                            customSerialized += (char)34 + "Total" + (char)34 + ":" + (char)34 + user.Value.summations.Total + (char)34;

                            customSerialized += "},";
                        }

                        if (user.Value.Matching != null) if (user.Value.Matching.Count > 0)
                            {
                                customSerialized += (char)34 + "Matching" + (char)34 + ": {";

                                int countD = 0;
                                foreach (var item in user.Value.Matching)
                                {
                                    if (countD != 0)
                                    {
                                        customSerialized += ",";
                                    }
                                    customSerialized += (char)34 + item.Key + (char)34 + ": {";
                                    customSerialized += (char)34 + "rid" + (char)34 + ":" + (char)34 + item.Value.rid + (char)34 + ",";
                                    customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + item.Value.date + (char)34 + ",";
                                    customSerialized += (char)34 + "sdType" + (char)34 + ":" + (char)34 + item.Value.sdType + (char)34;
                                    customSerialized += "}";

                                    countD = 1;
                                }
                                customSerialized += "},";
                            }

                        if (user.Value.PaymentPlanEntries != null) if (user.Value.PaymentPlanEntries.Count > 0)
                            {
                                customSerialized += (char)34 + "PaymentPlanEntries" + (char)34 + ": {";

                                int countD = 0;
                                foreach (var item in user.Value.PaymentPlanEntries)
                                {
                                    if (countD != 0)
                                    {
                                        customSerialized += ",";
                                    }
                                    customSerialized += (char)34 + item.Key + (char)34 + ": {";
                                    customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + item.Value.date + (char)34 + ",";
                                    customSerialized += (char)34 + "day" + (char)34 + ":" + (char)34 + item.Value.day + (char)34 + ",";
                                    customSerialized += (char)34 + "month" + (char)34 + ":" + (char)34 + item.Value.month + (char)34 + ",";
                                    customSerialized += (char)34 + "year" + (char)34 + ":" + (char)34 + item.Value.year + (char)34 + ",";
                                    customSerialized += (char)34 + "writtenOff" + (char)34 + ":" + (char)34 + item.Value.writtenOff + (char)34 + ",";
                                    customSerialized += (char)34 + "total" + (char)34 + ":" + (char)34 + item.Value.total + (char)34 + ",";
                                    customSerialized += (char)34 + "paid" + (char)34 + ":" + (char)34 + item.Value.paid + (char)34 + ",";
                                    customSerialized += (char)34 + "matchingReceipt" + (char)34 + ":" + (char)34 + item.Value.matchingReceipt + (char)34;
                                    customSerialized += "}";

                                    countD = 1;
                                }
                                customSerialized += "},";
                            }

                        if (user.Value.EditHistory != null) if (user.Value.EditHistory.Count > 0)
                            {
                                customSerialized += (char)34 + "EditHistory" + (char)34 + ": {";

                                int countD = 0;
                                foreach (var item in user.Value.EditHistory)
                                {
                                    if (countD != 0)
                                    {
                                        customSerialized += ",";
                                    }
                                    customSerialized += (char)34 + item.Key + (char)34 + ": {";
                                    customSerialized += (char)34 + "rid" + (char)34 + ":" + (char)34 + item.Value.rid + (char)34 + ",";
                                    customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + item.Value.date + (char)34 + ",";
                                    customSerialized += (char)34 + "sdType" + (char)34 + ":" + (char)34 + item.Value.sdType + (char)34;
                                    customSerialized += "}";

                                    countD = 1;
                                }
                                customSerialized += "},";
                            }

                        if (user.Value.products != null)
                        {
                            Dictionary<string, ProductEntrySDClass> productsQueued = user.Value.products;
                            int countD = 0;
                            if (productsQueued.Count != 0)
                            {
                                customSerialized += (char)34 + "products" + (char)34 + ": {";

                                foreach (var product in productsQueued)
                                {
                                    if (countD != 0)
                                    {
                                        customSerialized += ",";
                                    }

                                    customSerialized += (char)34 + product.Key + (char)34 + ": {";
                                    customSerialized += (char)34 + "brandName" + (char)34 + ":" + (char)34 + product.Value.brandName + (char)34 + ",";
                                    customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + product.Value.date + (char)34 + ",";
                                    customSerialized += (char)34 + "day" + (char)34 + ":" + (char)34 + product.Value.day + (char)34 + ",";
                                    customSerialized += (char)34 + "flavor" + (char)34 + ":" + (char)34 + product.Value.flavor + (char)34 + ",";
                                    customSerialized += (char)34 + "month" + (char)34 + ":" + (char)34 + product.Value.month + (char)34 + ",";
                                    customSerialized += (char)34 + "price" + (char)34 + ":" + (char)34 + product.Value.price + (char)34 + ",";
                                    customSerialized += (char)34 + "productID" + (char)34 + ":" + (char)34 + product.Value.productID + (char)34 + ",";
                                    customSerialized += (char)34 + "productName" + (char)34 + ":" + (char)34 + product.Value.productName + (char)34 + ",";
                                    customSerialized += (char)34 + "quantity" + (char)34 + ":" + (char)34 + product.Value.quantity + (char)34 + ",";
                                    customSerialized += (char)34 + "rid" + (char)34 + ":" + (char)34 + product.Value.rid + (char)34 + ",";
                                    customSerialized += (char)34 + "size" + (char)34 + ":" + (char)34 + product.Value.size + (char)34 + ",";
                                    customSerialized += (char)34 + "year" + (char)34 + ":" + (char)34 + product.Value.year + (char)34 + ",";
                                    customSerialized += (char)34 + "expiryDateTracking" + (char)34 + ":" + (char)34 + product.Value.expiryDateTracking + (char)34 + ",";
                                    customSerialized += (char)34 + "serialNumberYes" + (char)34 + ":" + (char)34 + product.Value.serialNumberYes + (char)34;

                                    if (product.Value.serialNumbers != null) if (product.Value.serialNumbers.Count > 0)
                                        {
                                            customSerialized += ",";
                                            customSerialized += (char)34 + "serialNumbers" + (char)34 + ": {";
                                            int countS = 0;
                                            foreach (var entry in product.Value.serialNumbers)
                                            {
                                                if (countS != 0)
                                                {
                                                    customSerialized += ",";
                                                }
                                                customSerialized += (char)34 + entry.Key + (char)34 + ": {";
                                                customSerialized += (char)34 + "serialNumber" + (char)34 + ":" + (char)34 + entry.Value.serialNumber + (char)34 + ",";
                                                customSerialized += (char)34 + "vDate" + (char)34 + ":" + (char)34 + entry.Value.vDate + (char)34 + ",";
                                                customSerialized += (char)34 + "vMonth" + (char)34 + ":" + (char)34 + entry.Value.vMonth + (char)34 + ",";
                                                customSerialized += (char)34 + "vDay" + (char)34 + ":" + (char)34 + entry.Value.vDay + (char)34 + ",";
                                                customSerialized += (char)34 + "vYear" + (char)34 + ":" + (char)34 + entry.Value.vYear + (char)34 + ",";
                                                customSerialized += (char)34 + "pid" + (char)34 + ":" + (char)34 + entry.Value.pid + (char)34 + ",";
                                                customSerialized += (char)34 + "expYes" + (char)34 + ":" + (char)34 + entry.Value.expYes + (char)34;
                                                customSerialized += "}";
                                                countS = 1;
                                            }
                                            customSerialized += "}";
                                        }
                                    if (product.Value.expiryDateBatches != null) if (product.Value.expiryDateBatches.Count > 0)
                                        {
                                            customSerialized += ",";
                                            customSerialized += (char)34 + "expiryDateBatches" + (char)34 + ": {";
                                            int countT = 0;
                                            foreach (var entry in product.Value.expiryDateBatches)
                                            {
                                                if (countT != 0)
                                                {
                                                    customSerialized += ",";
                                                }
                                                customSerialized += (char)34 + entry.Key + (char)34 + ": {";
                                                customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + entry.Value.date + (char)34 + ",";
                                                customSerialized += (char)34 + "QtyInStock" + (char)34 + ":" + (char)34 + entry.Value.QtyInStock + (char)34 + ",";
                                                customSerialized += (char)34 + "month" + (char)34 + ":" + (char)34 + entry.Value.month + (char)34 + ",";
                                                customSerialized += (char)34 + "day" + (char)34 + ":" + (char)34 + entry.Value.day + (char)34 + ",";
                                                customSerialized += (char)34 + "year" + (char)34 + ":" + (char)34 + entry.Value.year + (char)34;
                                                customSerialized += "}";
                                                countT = 1;
                                            }
                                            customSerialized += "}";
                                        }
                                    customSerialized += "}";

                                    countD = 1;
                                }
                                customSerialized += "},"; //all products
                            }
                            else
                            {
                                customSerialized += (char)34 + "products" + (char)34 + ":" + (char)34 + "," + (char)34;
                            }
                        }
                        else
                        {
                            customSerialized += (char)34 + "products" + (char)34 + ":" + (char)34 + "," + (char)34;
                        }
                        if (user.Value.onGoing != null)
                        {
                            Dictionary<string, OnGoingClass> onGoingQueuedArray = user.Value.onGoing;
                            int countD = 0;
                            if (onGoingQueuedArray.Count != 0)
                            {
                                customSerialized += (char)34 + "onGoing" + (char)34 + ": {";

                                foreach (var product in onGoingQueuedArray)
                                {
                                    if (countD != 0)
                                    {
                                        customSerialized += ",";
                                    }

                                    customSerialized += (char)34 + product.Key + (char)34 + ": {";
                                    customSerialized += (char)34 + "brandName" + (char)34 + ":" + (char)34 + product.Value.brandName + (char)34 + ",";
                                    customSerialized += (char)34 + "flavor" + (char)34 + ":" + (char)34 + product.Value.flavor + (char)34 + ",";
                                    customSerialized += (char)34 + "price" + (char)34 + ":" + (char)34 + product.Value.price + (char)34 + ",";
                                    customSerialized += (char)34 + "productID" + (char)34 + ":" + (char)34 + product.Value.productID + (char)34 + ",";
                                    customSerialized += (char)34 + "productName" + (char)34 + ":" + (char)34 + product.Value.productName + (char)34 + ",";
                                    customSerialized += (char)34 + "quantity" + (char)34 + ":" + (char)34 + product.Value.quantity + (char)34 + ",";
                                    customSerialized += (char)34 + "size" + (char)34 + ":" + (char)34 + product.Value.size + (char)34 + ",";
                                    customSerialized += (char)34 + "counter" + (char)34 + ":" + (char)34 + product.Value.counter + (char)34 + ",";
                                    customSerialized += (char)34 + "qtyInStock" + (char)34 + ":" + (char)34 + product.Value.qtyInStock + (char)34 + ",";
                                    customSerialized += (char)34 + "tracking" + (char)34 + ":" + (char)34 + product.Value.tracking + (char)34 + ",";
                                    customSerialized += (char)34 + "vat" + (char)34 + ":" + (char)34 + product.Value.vat + (char)34 + ",";
                                    customSerialized += (char)34 + "barcode" + (char)34 + ":" + (char)34 + product.Value.barcode + (char)34;
                                    customSerialized += (char)34 + "expiryDateTracking" + (char)34 + ":" + (char)34 + product.Value.expiryDateTracking + (char)34 + ",";
                                    customSerialized += (char)34 + "serialNumberYes" + (char)34 + ":" + (char)34 + product.Value.serialNumberYes + (char)34;

                                    if (product.Value.serialNumbers != null) if (product.Value.serialNumbers.Count > 0)
                                        {
                                            customSerialized += ",";
                                            customSerialized += (char)34 + "serialNumbers" + (char)34 + ": {";
                                            int countS = 0;
                                            foreach (var entry in product.Value.serialNumbers)
                                            {
                                                if (countS != 0)
                                                {
                                                    customSerialized += ",";
                                                }
                                                customSerialized += (char)34 + entry.Key + (char)34 + ": {";
                                                customSerialized += (char)34 + "serialNumber" + (char)34 + ":" + (char)34 + entry.Value.serialNumber + (char)34 + ",";
                                                customSerialized += (char)34 + "vDate" + (char)34 + ":" + (char)34 + entry.Value.vDate + (char)34 + ",";
                                                customSerialized += (char)34 + "vMonth" + (char)34 + ":" + (char)34 + entry.Value.vMonth + (char)34 + ",";
                                                customSerialized += (char)34 + "vDay" + (char)34 + ":" + (char)34 + entry.Value.vDay + (char)34 + ",";
                                                customSerialized += (char)34 + "vYear" + (char)34 + ":" + (char)34 + entry.Value.vYear + (char)34 + ",";
                                                customSerialized += (char)34 + "pid" + (char)34 + ":" + (char)34 + entry.Value.pid + (char)34 + ",";
                                                customSerialized += (char)34 + "expYes" + (char)34 + ":" + (char)34 + entry.Value.expYes + (char)34;
                                                customSerialized += "}";
                                                countS = 1;
                                            }
                                            customSerialized += "},";
                                        }
                                    if (product.Value.expiryDateBatches != null) if (product.Value.expiryDateBatches.Count > 0)
                                        {
                                            customSerialized += (char)34 + "expiryDateBatches" + (char)34 + ": {";
                                            int countT = 0;
                                            foreach (var entry in product.Value.expiryDateBatches)
                                            {
                                                if (countT != 0)
                                                {
                                                    customSerialized += ",";
                                                }
                                                customSerialized += (char)34 + entry.Key + (char)34 + ": {";
                                                customSerialized += (char)34 + "date" + (char)34 + ":" + (char)34 + entry.Value.date + (char)34 + ",";
                                                customSerialized += (char)34 + "QtyInStock" + (char)34 + ":" + (char)34 + entry.Value.QtyInStock + (char)34 + ",";
                                                customSerialized += (char)34 + "month" + (char)34 + ":" + (char)34 + entry.Value.month + (char)34 + ",";
                                                customSerialized += (char)34 + "day" + (char)34 + ":" + (char)34 + entry.Value.day + (char)34 + ",";
                                                customSerialized += (char)34 + "year" + (char)34 + ":" + (char)34 + entry.Value.year + (char)34;
                                                customSerialized += "}";
                                                countT = 1;
                                            }
                                            customSerialized += "}";
                                        }
                                    customSerialized += "}";
                                    countD = 1;
                                }
                                customSerialized += "}"; //all onGoing entries
                            }
                            else
                            {
                                customSerialized += (char)34 + "onGoing" + (char)34 + ":" + (char)34 + "" + (char)34;
                            }
                        }
                        else
                        {
                            customSerialized += (char)34 + "onGoing" + (char)34 + ":" + (char)34 + "" + (char)34;
                        }
                        customSerialized += "}"; //individual entry close
                        countC = 1;
                    }
                    customSerialized += "}"; //all entries close

                    System.IO.File.WriteAllText(dir, customSerialized);
                }
            }
        }
        
        public async void uploadFileDeliveryNote(SourceDocumentClass2 entry, string date)
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();

            //need to pass function that ensures we are using the correct date for each entry;

            if (entry.dirPdf.Length > 0)
            {
                if (File.Exists(entry.dirPdf))
                {
                    for (int i1 = 0; i1 < numberOfTries; i1++)
                    {
                        try
                        {
                            var stream = File.Open(entry.dirPdf, FileMode.Open);

                            var task = new FirebaseStorage("long-walk-pos.appspot.com")
                                .Child("users")
                                .Child(uid)
                                .Child(bid)
                                .Child(entry.sdType + "s")
                                .Child(entry.folio + ".pdf")
                                .PutAsync(stream);

                            // Track progress of the upload
                            task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                            // await the task to wait until upload completes and get the download url
                            var downloadUrl = await task;
                            string dNoteUrl = downloadUrl;

                            if (entry.matchingQuotation)
                            {
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        FirebaseResponse firebase1q = client.Set(DatabaseDirectory.Quotations() + "/" + entry.folio + "/matchingDNote", true);
                                        FirebaseResponse firebase1qa = client.Set(DatabaseDirectory.Quotations() + "/" + entry.folio + "/dNoteUrl", dNoteUrl);
                                        standardFirebaseOperationsClass.UpdateVersion("quotation");
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(duration);
                                    }
                                }
                            }
                            if (entry.matchingReceipt == "true")
                            {
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        FirebaseResponse firebase1a = client.Set(DatabaseDirectory.AllReceipts() + "/" + entry.folio + "/matchingDNote", true);
                                        FirebaseResponse firebase1aa = client.Set(DatabaseDirectory.AllReceipts() + "/" + entry.folio + "/dNoteUrl", dNoteUrl);
                                        FirebaseResponse firebasea = client.Set(DatabaseDirectory.Receipts() + "/" + date + "/receipts/" + entry.folio + "/matchingDNote", true);
                                        FirebaseResponse firebaseaa = client.Set(DatabaseDirectory.Receipts() + "/" + date + "/receipts/" + entry.folio + "/dNoteUrl", dNoteUrl);
                                        standardFirebaseOperationsClass.UpdateVersion("receipt");
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(duration);
                                    }
                                }
                            }
                            if (entry.matchingInvoice == "true")
                            {
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        //now we upload our invoice to both refernces
                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.AllInvoices() + "/" + entry.folio + "/matchingDNote", true);
                                        FirebaseResponse firebase1a = client.Set(DatabaseDirectory.AllInvoices() + "/" + entry.folio + "/dNoteUrl", dNoteUrl);
                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Invoices() + "/" + date + "/invoices/" + entry.folio + "/matchingDNote", true);
                                        FirebaseResponse firebasea = client.Set(DatabaseDirectory.Invoices() + "/" + date + "/invoices/" + entry.folio + "/dNoteUrl", dNoteUrl);
                                        standardFirebaseOperationsClass.UpdateVersion("invoice");
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(duration);
                                    }
                                }
                            }
                            if (entry.matchingOrder)
                            {
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        FirebaseResponse firebase1q = client.Set(DatabaseDirectory.Orders() + "/" + entry.folio + "/matchingDNote", true);
                                        FirebaseResponse firebase1qa = client.Set(DatabaseDirectory.Orders() + "/" + entry.folio + "/dNoteUrl", dNoteUrl);
                                        standardFirebaseOperationsClass.UpdateVersion("order");
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(duration);
                                    }
                                }
                            }

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

        public async void autoGenerateDeliveryNote(SourceDocumentClass2 entry)
        {
            SourceDocumentClass2 sourceDocument = entry;
            if (!sourceDocument.matchingDNote)
            {
                //create unique product ID
                DateTime dtt = DateTime.Now;
                int month = dtt.Month;
                int year = dtt.Year;
                int daySele = dtt.Day;
                double time = dtt.TimeOfDay.TotalMilliseconds;
                int timeId = Convert.ToInt32(time);
                string rid = "";
                string date = dtt.ToString("MMM, dd yyyy");
                string timeToPass = dtt.ToString("HH:mm:ss tt");


                rid = "WG" + Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(timeId);

                SettingsClass settingsClass = prevelantClass.RetrieveSettings();
                if (settingsClass != null)
                    if (settingsClass.bookKeepingSettings != null)
                        if (settingsClass.bookKeepingSettings.TaxInvoiceStandard)
                        {
                            SDServicesClass sDServices = new SDServicesClass();
                            string folioNo = await sDServices.GetContinuousFolioAsync("DeliveryNote");
                            sourceDocument.customId = folioNo;
                            sourceDocument.customIdYes = true;
                        }
                
                sourceDocument.folio = rid;
                sourceDocument.sdType = "Delivery Note";

                pdfClass pdfOps = new pdfClass();
                string pdfLocation = pdfOps.GeneratePDF(sourceDocument, false);

                sourceDocument.pdfLink = pdfLocation;
                sourceDocument.dirPdf = pdfLocation;

                updateDeliveryNote(sourceDocument);
            }
        }

        public void WriteOff(SourceDocumentClass2 entry)
        {
            //set as written off

            //look for matching and set those as written off too
        }

        //Update
        public void UpdateMatchingReturns(int sdtype, string matchingReceipt, bool matchingQuotation, string folio, string date, string day, string month, string year, TextBox control, Action<bool> method, bool returnProductsYes, SourceDocumentClass2 sourceDoc)
        {
            bool success = false;
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            setup();

            //accuracy to the max
            string cashDate = "";
            string creditDate = "";

            Dictionary<string, DateClass> receipts = prevelantClass.LoadDatesSales();
            Dictionary<string, DateClass> invoices = prevelantClass.LoadDatesCredit();

            foreach (var dateItem in receipts)
            {
                if (dateItem.Value.receipts == null) break;
                if (dateItem.Value.receipts.Count == 0) break;
                foreach (var item in dateItem.Value.receipts)
                {
                    if (item.Key == folio)
                    {
                        cashDate = dateItem.Value.date;
                        break;
                    }
                }
            }

            foreach (var dateItem in invoices)
            {
                if (dateItem.Value.invoices == null) break;
                if (dateItem.Value.invoices.Count == 0) break;
                foreach (var item in dateItem.Value.invoices)
                {
                    if (item.Key == folio)
                    {
                        creditDate = dateItem.Value.date;
                        break;
                    }
                }
            }

            if (sdtype == 1)
            {
                if (creditDate != "")
                {
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            //now we upload our invoice to both refernces
                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.AllInvoices() + "/" + folio + "/matchingReturns", "true");
                            FirebaseResponse firebase = client.Set(DatabaseDirectory.Invoices() + "/" + creditDate + "/invoices/" + folio + "/matchingReturns", "true");
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                }

                if (matchingReceipt == "true" && cashDate != "")
                {
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            FirebaseResponse firebase11 = client.Set(DatabaseDirectory.AllReceipts() + "/" + folio + "/matchingReturns", "true");
                            FirebaseResponse firebase111 = client.Set(DatabaseDirectory.Receipts() + "/" + cashDate + "/receipts/" + folio + "/matchingReturns", "true");
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                }
                if (matchingQuotation)
                {
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            FirebaseResponse firebase1q = client.Set(DatabaseDirectory.Quotations() + "/"  + folio + "/matchingReturns", "true");
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                }

                //now we upload to client side
                string dateva = day + month + year;
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse firebasea1 = client.Set(DatabaseDirectory.ClientSds() + "/"  + dateva + "/files/" + folio + "/matchingReturns", "true");

                        //update firebase
                        standardFirebaseOperationsClass.UpdateVersion("invoice");
                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
            else if (sdtype == 0)
            {
                if (cashDate != "")
                {
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            //now we upload our invoice to both refernces
                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.AllReceipts() + "/"  + folio + "/matchingReturns", "true");
                            FirebaseResponse firebase = client.Set(DatabaseDirectory.Receipts() + "/"  + cashDate + "/receipts/" + folio + "/matchingReturns", "true");
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                }

                if (matchingReceipt == "true" && creditDate != "")
                {
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            FirebaseResponse firebase1a = client.Set(DatabaseDirectory.AllInvoices() + "/"  + folio + "/matchingReturns", "true");
                            FirebaseResponse firebasea = client.Set(DatabaseDirectory.Invoices() + "/"  + creditDate + "/invoices/" + folio + "/matchingReturns", "true");
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                }
                if (matchingQuotation)
                {
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            FirebaseResponse firebase1q = client.Set(DatabaseDirectory.Quotations() + "/" + folio + "/matchingReturns", "true");
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                }
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        //now we upload to client side
                        string dateva = day + month + year;
                        FirebaseResponse firebasea1 = client.Set(DatabaseDirectory.ClientSds() + "/"  + dateva + "/files/" + folio + "/matchingReturns", "true"); //the original invoice will be overriden

                        //update firebase
                        standardFirebaseOperationsClass.UpdateVersion("receipt");
                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
            if (success)
            {
                if (returnProductsYes)
                {
                    ProductProcessingServicesClass processingServicesClass = new();
                    processingServicesClass.updateQuantityEntry(sourceDoc.products, false, sourceDoc.folio);
                }
            }


            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public async void UpdateMatchingInvoice(string folio, string date, string day, string month, string year, bool matchingQuotation)
        {
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            setup();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    //now we upload our invoice to both refernces
                    FirebaseResponse firebase1 = await client.SetAsync(DatabaseDirectory.AllInvoices() + "/" + folio + "/matchingReceipt", "true");
                    FirebaseResponse firebase12 = await client.SetAsync(DatabaseDirectory.AllInvoices() + "/" + folio + "/paid", "true");
                    FirebaseResponse firebase = await client.SetAsync(DatabaseDirectory.Invoices() + "/" + date + "/invoices/" + folio + "/matchingReceipt", "true");
                    FirebaseResponse firebase2 = await client.SetAsync(DatabaseDirectory.Invoices() + "/" + date + "/invoices/" + folio + "/paid", "true");

                    if (matchingQuotation)
                    {
                        FirebaseResponse firebase1q = client.Set(DatabaseDirectory.Quotations() + "/" + folio + "/matchingReceipt", "true");
                        standardFirebaseOperationsClass.UpdateVersion("quotation");
                    }

                    //now we upload to client side
                    string dateva = day + month + year;
                    FirebaseResponse firebasea1 = await client.SetAsync(DatabaseDirectory.ClientSds() + "/" + dateva + "/files/" + folio + "/matchingReceipt", "true");
                    FirebaseResponse firebasea12 = await client.SetAsync(DatabaseDirectory.ClientSds() + "/" + dateva + "/files/" + folio + "/paid", "true");

                    //update firebase
                    standardFirebaseOperationsClass.UpdateVersion("invoice");
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
        }

        public async void UpdateMatchingQuotation(string folio, string date, string day, string month, string year, int type)
        {
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            setup();

            string dateva = day + month + year;

            switch (type)
            {
                case 0:
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            //now we upload our invoice to both refernces
                            FirebaseResponse firebase1q = await client.SetAsync(DatabaseDirectory.Quotations() + "/" + folio + "/matchingReceipt", "true");
                            standardFirebaseOperationsClass.UpdateVersion("quotation");
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                    break;
                case 1:
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            //now we upload our invoice to both refernces
                            FirebaseResponse firebase1qa = await client.SetAsync(DatabaseDirectory.Quotations() + "/" + folio + "/matchingInvoice", "true");
                            standardFirebaseOperationsClass.UpdateVersion("quotation");
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                    break;
            }
        }
        
        public void ArchieveQuotation(string rid, bool status, Button control, Action<bool> method, bool update)
        {
            bool success = false;
            eid = prevelantClass.getEid();
            setup();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Quotations() + "/" + rid + "/archived", status);

                    if (update)
                    {
                        standardFirebaseOperationsClass.UpdateVersion("quotation");
                    }
                    success = true;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void CancelOrder(string rid, bool status, Button control, Action<bool> method, bool update)
        {
            bool success = false;
            eid = prevelantClass.getEid();
            setup();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Orders() + "/" + rid + "/archived", status);

                    if (update)
                    {
                        standardFirebaseOperationsClass.UpdateVersion("order");
                    }
                    success = true;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void WriteOffBadDebt(string rid, string date, bool status, Button control, Action<bool> method, bool update, bool isFsd)
        {
            bool success = false;
            eid = prevelantClass.getEid();
            setup();

            if (isFsd){
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + rid + "/writtenOff", status);
                        FirebaseResponse firebasea = client.Set(DatabaseDirectory.InvoicesIn() + "/" + date + "/sd/" + rid + "/writtenOff", status);

                        if (update)
                        {
                            standardFirebaseOperationsClass.UpdateVersion("invoiceIn");
                        }
                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            } else
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoices() + "/" + rid + "/writtenOff", status);
                        FirebaseResponse firebasea = client.Set(DatabaseDirectory.Invoices() + "/" + date + "/invoices/" + rid + "/writtenOff", status);

                        if (update)
                        {
                            standardFirebaseOperationsClass.UpdateVersion("invoice");
                        }
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

        //Retrieve

        //Reshape functions

        public void ReshapeDNA(Action<string> method, CounterControl control)
        {
            setup();
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            int completedTasks = 0;
            //Transfer Notes
            Dictionary<string, SourceDocumentClass2> transferNotesArray = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(DatabaseDirectory.TransferNotes());
                    transferNotesArray = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res.Body.ToString());
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            if (transferNotesArray != null) if (transferNotesArray.Count > 0)
                {
                    foreach (var item in transferNotesArray)
                    {
                        SourceDocumentClass2 sd = item.Value;
                        Dictionary<string, MatchingClass> matchingArray = sd.Matching;
                        if (matchingArray == null) matchingArray = new();
                        
                        if (true)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Transfer Note"
                            };
                            if (!matchingArray.ContainsKey("aa0"))
                            {
                                matchingArray.Add("aa0", matching);
                            }
                        }
                        if (sd.matchingQuotation)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Quotation"
                            };
                            if (!matchingArray.ContainsKey("aa1"))
                            {
                                matchingArray.Add("aa1", matching);
                            }
                        }
                        if (sd.matchingOrder)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Order"
                            };
                            if (!matchingArray.ContainsKey("aa2"))
                            {
                                matchingArray.Add("aa2", matching);
                            }
                        }
                        if (sd.matchingInvoice == "true")
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Invoice"
                            };
                            if (!matchingArray.ContainsKey("aa3"))
                            {
                                matchingArray.Add("aa3", matching);
                            }
                        }
                        if (sd.matchingReceipt == "true")
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Receipt"
                            };
                            if (!matchingArray.ContainsKey("aa4"))
                            {
                                matchingArray.Add("aa4", matching);
                            }
                        }

                        sd.Matching = matchingArray;

                        for (int i = 0; i < numberOfTries; i++)
                        {
                            try
                            {
                                if (sd.date != null) if (sd.folio != null) if(sd.folio.Length > 0 && sd.date.Length > 0)
                                        {
                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.TransferNotes() + "/" + sd.folio, sd);
                                        }
                                break;
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(duration);
                            }
                        }
                    }
                    standardFirebaseOperationsClass.UpdateVersion("transferNote");
                }

            //Quotations
            Dictionary<string, SourceDocumentClass2> quotationsArray = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(DatabaseDirectory.Quotations());
                    quotationsArray = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res.Body.ToString());
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            if(quotationsArray != null) if(quotationsArray.Count > 0)
                {
                    foreach (var item in quotationsArray)
                    {
                        SourceDocumentClass2 sd = item.Value;
                        Dictionary<string, MatchingClass> matchingArray = sd.Matching;
                        if (matchingArray == null) matchingArray = new();


                        if (sd.matchingQuotation || true)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Quotation"
                            };
                            if (!matchingArray.ContainsKey("aa1"))
                            {
                                matchingArray.Add("aa1", matching);
                            }
                        }
                        if (sd.matchingOrder)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Order"
                            };
                            if (!matchingArray.ContainsKey("aa2"))
                            {
                                matchingArray.Add("aa2", matching);
                            }
                        }
                        if (sd.matchingInvoice == "true")
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Invoice"
                            };
                            if (!matchingArray.ContainsKey("aa3"))
                            {
                                matchingArray.Add("aa3", matching);
                            }
                        }
                        if (sd.matchingReceipt == "true")
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Receipt"
                            };
                            if (!matchingArray.ContainsKey("aa4"))
                            {
                                matchingArray.Add("aa4", matching);
                            }
                        }

                        sd.Matching = matchingArray;

                        for (int i = 0; i < numberOfTries; i++)
                        {
                            try
                            {
                                if (sd.date != null) if (sd.folio != null) if (sd.folio.Length > 0 && sd.date.Length > 0)
                                        {
                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.Quotations() + "/" + sd.folio, sd);
                                        }
                                break;
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(duration);
                            }
                        }
                    }
                    standardFirebaseOperationsClass.UpdateVersion("quotation");
                }

            //All Receipt
            Dictionary<string, SourceDocumentClass2> receiptsArray = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(DatabaseDirectory.AllReceipts());
                    receiptsArray = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res.Body.ToString());
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            if (receiptsArray != null) if (receiptsArray.Count > 0)
                {
                    foreach (var item in receiptsArray)
                    {
                        SourceDocumentClass2 sd = item.Value;
                        Dictionary<string, MatchingClass> matchingArray = sd.Matching;
                        if (matchingArray == null) matchingArray = new();

                        if (sd.matchingQuotation)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Quotation"
                            };
                            if (!matchingArray.ContainsKey("aa1"))
                            {
                                matchingArray.Add("aa1", matching);
                            }
                        }
                        if (sd.matchingOrder)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Order"
                            };
                            if (!matchingArray.ContainsKey("aa2"))
                            {
                                matchingArray.Add("aa2", matching);
                            }
                        }
                        if (sd.matchingInvoice == "true")
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Invoice"
                            };
                            if (!matchingArray.ContainsKey("aa3"))
                            {
                                matchingArray.Add("aa3", matching);
                            }
                        }
                        if (sd.matchingReceipt == "true" || true)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Receipt"
                            };
                            if (!matchingArray.ContainsKey("aa4"))
                            {
                                matchingArray.Add("aa4", matching);
                            }
                        }

                        sd.Matching = matchingArray;

                        for (int i = 0; i < numberOfTries; i++)
                        {
                            try
                            {
                                if (sd.date != null) if (sd.folio != null) if (sd.folio.Length > 0 && sd.date.Length > 0)
                                        {
                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceipts() + "/" + sd.folio, sd);
                                        }
                                completedTasks++;
                                break;
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(duration);
                            }
                        }
                    }
                }

            //Receipts
            Dictionary<string, DateClass> datesArray = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(DatabaseDirectory.Receipts());
                    datesArray = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(res.Body.ToString());
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            if(datesArray != null) if(datesArray.Count > 0)
                {
                    foreach(var date in datesArray)
                    {
                        if(date.Value.receipts != null) if(date.Value.receipts.Count > 0)
                            {
                                foreach (var item in date.Value.receipts)
                                {
                                    SourceDocumentClass2 sd = item.Value;
                                    Dictionary<string, MatchingClass> matchingArray = sd.Matching;
                                    if (matchingArray == null) matchingArray = new();

                                    if (sd.matchingQuotation)
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Quotation"
                                        };
                                        if (!matchingArray.ContainsKey("aa1"))
                                        {
                                            matchingArray.Add("aa1", matching);
                                        }
                                    }
                                    if (sd.matchingOrder)
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Order"
                                        };
                                        if (!matchingArray.ContainsKey("aa2"))
                                        {
                                            matchingArray.Add("aa2", matching);
                                        }
                                    }
                                    if (sd.matchingInvoice == "true")
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Invoice"
                                        };
                                        if (!matchingArray.ContainsKey("aa3"))
                                        {
                                            matchingArray.Add("aa3", matching);
                                        }
                                    }
                                    if (sd.matchingReceipt == "true" || true)
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Receipt"
                                        };
                                        if (!matchingArray.ContainsKey("aa4"))
                                        {
                                            matchingArray.Add("aa4", matching);
                                        }
                                    }

                                    sd.Matching = matchingArray;

                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (sd.date != null) if (sd.folio != null) if (sd.folio.Length > 0 && sd.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Receipts() + "/" + sd.date + "/receipts/" + sd.folio, sd);
                                                    }
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
                    standardFirebaseOperationsClass.UpdateVersion("receipt");
                }

            //All Invoices
            Dictionary<string, SourceDocumentClass2> invoicesArray = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(DatabaseDirectory.AllInvoices());
                    invoicesArray = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res.Body.ToString());
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            if (invoicesArray != null) if (invoicesArray.Count > 0)
                {
                    foreach (var item in invoicesArray)
                    {
                        SourceDocumentClass2 sd = item.Value;
                        Dictionary<string, MatchingClass> matchingArray = sd.Matching;
                        if (matchingArray == null) matchingArray = new();

                        if (sd.matchingQuotation)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Quotation"
                            };
                            if (!matchingArray.ContainsKey("aa1"))
                            {
                                matchingArray.Add("aa1", matching);
                            }
                        }
                        if (sd.matchingOrder)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Order"
                            };
                            if (!matchingArray.ContainsKey("aa2"))
                            {
                                matchingArray.Add("aa2", matching);
                            }
                        }
                        if (sd.matchingInvoice == "true" || true)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Invoice"
                            };
                            if (!matchingArray.ContainsKey("aa3"))
                            {
                                matchingArray.Add("aa3", matching);
                            }
                        }
                        if (sd.matchingReceipt == "true")
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Receipt"
                            };
                            if (!matchingArray.ContainsKey("aa4"))
                            {
                                matchingArray.Add("aa4", matching);
                            }
                        }

                        sd.Matching = matchingArray;

                        for (int i = 0; i < numberOfTries; i++)
                        {
                            try
                            {
                                if (sd.date != null) if (sd.folio != null) if (sd.folio.Length > 0 && sd.date.Length > 0)
                                        {
                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoices() + "/" + sd.folio, sd);
                                        }
                                break;
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(duration);
                            }
                        }
                    }
                }

            //Invoices
            Dictionary<string, DateClass> datesArray1 = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(DatabaseDirectory.Invoices());
                    datesArray1 = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(res.Body.ToString());
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            if (datesArray1 != null) if (datesArray1.Count > 0)
                {
                    foreach (var date in datesArray1)
                    {
                        if (date.Value.invoices != null) if (date.Value.invoices.Count > 0)
                            {
                                foreach (var item in date.Value.invoices)
                                {
                                    SourceDocumentClass2 sd = item.Value;
                                    Dictionary<string, MatchingClass> matchingArray = sd.Matching;
                                    if (matchingArray == null) matchingArray = new();

                                    if (sd.matchingQuotation)
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Quotation"
                                        };
                                        if (!matchingArray.ContainsKey("aa1"))
                                        {
                                            matchingArray.Add("aa1", matching);
                                        }
                                    }
                                    if (sd.matchingOrder)
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Order"
                                        };
                                        if (!matchingArray.ContainsKey("aa2"))
                                        {
                                            matchingArray.Add("aa2", matching);
                                        }
                                    }
                                    if (sd.matchingInvoice == "true" || true)
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Invoice"
                                        };
                                        if (!matchingArray.ContainsKey("aa3"))
                                        {
                                            matchingArray.Add("aa3", matching);
                                        }
                                    }
                                    if (sd.matchingReceipt == "true")
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Receipt"
                                        };
                                        if (!matchingArray.ContainsKey("aa4"))
                                        {
                                            matchingArray.Add("aa4", matching);
                                        }
                                    }

                                    sd.Matching = matchingArray;

                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (sd.date != null) if (sd.folio != null) if (sd.folio.Length > 0 && sd.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Invoices() + "/" + sd.date + "/invoices/" + sd.folio, sd);
                                                    }
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
                    standardFirebaseOperationsClass.UpdateVersion("invoices");
                }

            //All ReceiptIn
            Dictionary<string, SourceDocumentClass2> receiptInsArray = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(DatabaseDirectory.AllReceiptsIn());
                    receiptInsArray = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res.Body.ToString());
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            if (receiptInsArray != null) if (receiptInsArray.Count > 0)
                {
                    foreach (var item in receiptInsArray)
                    {
                        SourceDocumentClass2 sd = item.Value;
                        Dictionary<string, MatchingClass> matchingArray = sd.Matching;
                        if (matchingArray == null) matchingArray = new();

                        if (sd.matchingQuotation)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Quotation"
                            };
                            if (!matchingArray.ContainsKey("aa1"))
                            {
                                matchingArray.Add("aa1", matching);
                            }
                        }
                        if (sd.matchingOrder)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Order"
                            };
                            if (!matchingArray.ContainsKey("aa2"))
                            {
                                matchingArray.Add("aa2", matching);
                            }
                        }
                        if (sd.matchingInvoice == "true")
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Invoice"
                            };
                            if (!matchingArray.ContainsKey("aa3"))
                            {
                                matchingArray.Add("aa3", matching);
                            }
                        }
                        if (sd.matchingReceipt == "true" || true)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Receipt"
                            };
                            if (!matchingArray.ContainsKey("aa4"))
                            {
                                matchingArray.Add("aa4", matching);
                            }
                        }

                        sd.Matching = matchingArray;

                        for (int i = 0; i < numberOfTries; i++)
                        {
                            try
                            {
                                if (sd.date != null) if (sd.folio != null) if (sd.folio.Length > 0 && sd.date.Length > 0)
                                        {
                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceiptsIn() + "/" + sd.folio, sd);
                                        }
                                break;
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(duration);
                            }
                        }
                    }
                }

            //ReceiptsIn
            Dictionary<string, DateClass> datesArrayFsd = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(DatabaseDirectory.ReceiptsIn());
                    datesArrayFsd = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(res.Body.ToString());
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            if (datesArrayFsd != null) if (datesArrayFsd.Count > 0)
                {
                    foreach (var date in datesArrayFsd)
                    {
                        if (date.Value.sd != null) if (date.Value.sd.Count > 0)
                            {
                                foreach (var item in date.Value.sd)
                                {
                                    SourceDocumentClass2 sd = item.Value;
                                    Dictionary<string, MatchingClass> matchingArray = sd.Matching;
                                    if (matchingArray == null) matchingArray = new();

                                    if (sd.matchingQuotation)
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Quotation"
                                        };
                                        if (!matchingArray.ContainsKey("aa1"))
                                        {
                                            matchingArray.Add("aa1", matching);
                                        }
                                    }
                                    if (sd.matchingOrder)
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Order"
                                        };
                                        if (!matchingArray.ContainsKey("aa2"))
                                        {
                                            matchingArray.Add("aa2", matching);
                                        }
                                    }
                                    if (sd.matchingInvoice == "true")
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Invoice"
                                        };
                                        if (!matchingArray.ContainsKey("aa3"))
                                        {
                                            matchingArray.Add("aa3", matching);
                                        }
                                    }
                                    if (sd.matchingReceipt == "true" || true)
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Receipt"
                                        };
                                        if (!matchingArray.ContainsKey("aa4"))
                                        {
                                            matchingArray.Add("aa4", matching);
                                        }
                                    }

                                    sd.Matching = matchingArray;

                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (sd.date != null) if (sd.folio != null) if (sd.folio.Length > 0 && sd.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + sd.date + "/sd/" + sd.folio, sd);
                                                    }
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
                    standardFirebaseOperationsClass.UpdateVersion("receiptIn");
                }


            //All InvoicesIn
            Dictionary<string, SourceDocumentClass2> invoicesInArray = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(DatabaseDirectory.AllInvoicesIn());
                    invoicesInArray = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res.Body.ToString());
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            if (invoicesInArray != null) if (invoicesInArray.Count > 0)
                {
                    foreach (var item in invoicesInArray)
                    {
                        SourceDocumentClass2 sd = item.Value;
                        Dictionary<string, MatchingClass> matchingArray = sd.Matching;
                        if (matchingArray == null) matchingArray = new();

                        if (sd.matchingQuotation)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Quotation"
                            };
                            if (!matchingArray.ContainsKey("aa1"))
                            {
                                matchingArray.Add("aa1", matching);
                            }
                        }
                        if (sd.matchingOrder)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Order"
                            };
                            if (!matchingArray.ContainsKey("aa2"))
                            {
                                matchingArray.Add("aa2", matching);
                            }
                        }
                        if (sd.matchingInvoice == "true" || true)
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Invoice"
                            };
                            if (!matchingArray.ContainsKey("aa3"))
                            {
                                matchingArray.Add("aa3", matching);
                            }
                        }
                        if (sd.matchingReceipt == "true")
                        {
                            MatchingClass matching = new()
                            {
                                rid = sd.folio,
                                date = sd.date,
                                sdType = "Receipt"
                            };
                            if (!matchingArray.ContainsKey("aa4"))
                            {
                                matchingArray.Add("aa4", matching);
                            }
                        }

                        sd.Matching = matchingArray;

                        for (int i = 0; i < numberOfTries; i++)
                        {
                            try
                            {
                                if (sd.date != null) if (sd.folio != null) if (sd.folio.Length > 0 && sd.date.Length > 0)
                                        {
                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + sd.folio, sd);
                                        }
                                break;
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(duration);
                            }
                        }
                    }
                }

            //InvoicesIn
            Dictionary<string, DateClass> datesArrayFSD1 = new();
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(DatabaseDirectory.InvoicesIn());
                    datesArrayFSD1 = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(res.Body.ToString());
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            if (datesArrayFSD1 != null) if (datesArrayFSD1.Count > 0)
                {
                    foreach (var date in datesArrayFSD1)
                    {
                        if (date.Value.sd != null) if (date.Value.sd.Count > 0)
                            {
                                foreach (var item in date.Value.sd)
                                {
                                    SourceDocumentClass2 sd = item.Value;
                                    Dictionary<string, MatchingClass> matchingArray = sd.Matching;
                                    if (matchingArray == null) matchingArray = new();

                                    if (sd.matchingQuotation)
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Quotation"
                                        };
                                        if (!matchingArray.ContainsKey("aa1"))
                                        {
                                            matchingArray.Add("aa1", matching);
                                        }
                                    }
                                    if (sd.matchingOrder)
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Order"
                                        };
                                        if (!matchingArray.ContainsKey("aa2"))
                                        {
                                            matchingArray.Add("aa2", matching);
                                        }
                                    }
                                    if (sd.matchingInvoice == "true" || true)
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Invoice"
                                        };
                                        if (!matchingArray.ContainsKey("aa3"))
                                        {
                                            matchingArray.Add("aa3", matching);
                                        }
                                    }
                                    if (sd.matchingReceipt == "true")
                                    {
                                        MatchingClass matching = new()
                                        {
                                            rid = sd.folio,
                                            date = sd.date,
                                            sdType = "Receipt"
                                        };
                                        if (!matchingArray.ContainsKey("aa4"))
                                        {
                                            matchingArray.Add("aa4", matching);
                                        }
                                    }

                                    sd.Matching = matchingArray;

                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (sd.date != null) if (sd.folio != null) if (sd.folio.Length > 0 && sd.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.InvoicesIn() + "/" + sd.date + "/sd/" + sd.folio, sd);
                                                    }
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
                    standardFirebaseOperationsClass.UpdateVersion("invoiceIn");
                }


            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    client.SetAsync(DatabaseDirectory.Businesses() + "/" + bid + "/MultiSDSupportYes", true);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            control.Dispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            method, "close");
        }

        public void updateMatching(Dictionary<string, MatchingClass> matchingArray, bool isFsd)
        {
            setup();

            if(matchingArray != null) if(matchingArray.Count > 0)
                {
                    foreach(var entry in matchingArray)
                    {
                        switch (entry.Value.sdType)
                        {
                            case "Receipt":
                                if (isFsd)
                                {
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceiptsIn() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + entry.Value.date + "/sd/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("receiptIn");
                                } 
                                else
                                {
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceipts() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Receipts() + "/" + entry.Value.date + "/receipts/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("receipt");
                                }
                                break;
                            case "Invoice":
                                if (isFsd)
                                {
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.InvoicesIn() + "/" + entry.Value.date + "/sd/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("invoiceIn");
                                }
                                else
                                {
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoices() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Invoices() + "/" + entry.Value.date + "/invoices/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("invoice");
                                }
                                break;
                            case "Quotation":
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                {
                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Quotations() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                }
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(duration);
                                    }
                                }
                                standardFirebaseOperationsClass.UpdateVersion("quotation");
                                break;
                            case "Order":
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                {
                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Orders() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                }
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(duration);
                                    }
                                }
                                standardFirebaseOperationsClass.UpdateVersion("order");
                                break;
                            case "Transfer Note":
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                {
                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.TransferNotes() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                }
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(duration);
                                    }
                                }
                                standardFirebaseOperationsClass.UpdateVersion("transferNote");
                                break;
                            case "Debit Note":
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                {
                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.DebitNote() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                }
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(duration);
                                    }
                                }
                                standardFirebaseOperationsClass.UpdateVersion("debitNote");
                                break;
                            case "Credit Note":
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                {
                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.CreditNotes() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                }
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(duration);
                                    }
                                }
                                standardFirebaseOperationsClass.UpdateVersion("creditNote");
                                break;
                        }
                    }
                }
        }

        public SDChecklistClass updateMatching(Dictionary<string, MatchingClass> matchingArray, bool isFsd, SDChecklistClass checklistClass)
        {
            if (checklistClass.matching_completed == null) checklistClass.matching_completed = new();

            Dictionary<string, string> completed = checklistClass.matching_completed;

            setup();

            bool success = false;
            if (matchingArray != null) if (matchingArray.Count > 0)
                {
                    foreach (var entry in matchingArray)
                    {
                        if (!completed.ContainsKey(entry.Value.rid))
                        {
                            switch (entry.Value.sdType)
                            {
                                case "Receipt":
                                    if (isFsd)
                                    {
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceiptsIn() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + entry.Value.date + "/sd/" + entry.Value.rid + "/Matching", matchingArray);
                                                        }
                                                completed.Add(entry.Value.rid, entry.Value.rid);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.UpdateVersion("receiptIn");
                                    }
                                    else
                                    {
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceipts() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Receipts() + "/" + entry.Value.date + "/receipts/" + entry.Value.rid + "/Matching", matchingArray);
                                                        }
                                                completed.Add(entry.Value.rid, entry.Value.rid);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.UpdateVersion("receipt");
                                    }
                                    break;
                                case "Invoice":
                                    if (isFsd)
                                    {
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.InvoicesIn() + "/" + entry.Value.date + "/sd/" + entry.Value.rid + "/Matching", matchingArray);
                                                        }
                                                completed.Add(entry.Value.rid, entry.Value.rid);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.UpdateVersion("invoiceIn");
                                    }
                                    else
                                    {
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoices() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Invoices() + "/" + entry.Value.date + "/invoices/" + entry.Value.rid + "/Matching", matchingArray);
                                                        }
                                                completed.Add(entry.Value.rid, entry.Value.rid);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.UpdateVersion("invoice");
                                    }
                                    break;
                                case "Quotation":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            success = false;
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Quotations() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            completed.Add(entry.Value.rid, entry.Value.rid);
                                            success = true;
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("quotation");
                                    break;
                                case "Order":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            success = false;
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Orders() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            completed.Add(entry.Value.rid, entry.Value.rid);
                                            success = true;
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("order");
                                    break;
                                case "Transfer Note":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            success = false;
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.TransferNotes() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            completed.Add(entry.Value.rid, entry.Value.rid);
                                            success = true;
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("transferNote");
                                    break;
                                case "Debit Note":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            success = false;
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.DebitNote() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            completed.Add(entry.Value.rid, entry.Value.rid);
                                            success = true;
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("debitNote");
                                    break;
                                case "Credit Note":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            success = false;
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.CreditNotes() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            completed.Add(entry.Value.rid, entry.Value.rid);
                                            success = true;
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("creditNote");
                                    break;
                            }
                        }
                    }
                }
            checklistClass.matching_updated = success;
            return checklistClass;
        }

        public Dictionary<string, string> updateMatching(Dictionary<string, MatchingClass> matchingArray, bool isFsd, Dictionary<string, string> completed)
        {
            setup();

            if (matchingArray != null) if (matchingArray.Count > 0)
                {
                    foreach (var entry in matchingArray)
                    {
                        if (!completed.ContainsKey(entry.Key))
                        {
                            switch (entry.Value.sdType)
                            {
                                case "Receipt":
                                    if (isFsd)
                                    {
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceiptsIn() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + entry.Value.date + "/sd/" + entry.Value.rid + "/Matching", matchingArray);
                                                        }
                                                completed.Add(entry.Key, entry.Key);
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.UpdateVersion("receiptIn");
                                    }
                                    else
                                    {
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceipts() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Receipts() + "/" + entry.Value.date + "/receipts/" + entry.Value.rid + "/Matching", matchingArray);
                                                        }
                                                completed.Add(entry.Key, entry.Key);
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.UpdateVersion("receipt");
                                    }
                                    break;
                                case "Invoice":
                                    if (isFsd)
                                    {
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.InvoicesIn() + "/" + entry.Value.date + "/sd/" + entry.Value.rid + "/Matching", matchingArray);
                                                        }
                                                completed.Add(entry.Key, entry.Key);
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.UpdateVersion("invoiceIn");
                                    }
                                    else
                                    {
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoices() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Invoices() + "/" + entry.Value.date + "/invoices/" + entry.Value.rid + "/Matching", matchingArray);
                                                        }
                                                completed.Add(entry.Key, entry.Key);
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.UpdateVersion("invoice");
                                    }
                                    break;
                                case "Quotation":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Quotations() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            completed.Add(entry.Key, entry.Key);
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("quotation");
                                    break;
                                case "Order":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Orders() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            completed.Add(entry.Key, entry.Key);
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("order");
                                    break;
                                case "Transfer Note":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.TransferNotes() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            completed.Add(entry.Key, entry.Key);
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("transferNote");
                                    break;
                                case "Debit Note":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.DebitNote() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            completed.Add(entry.Key, entry.Key);
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("debitNote");
                                    break;
                                case "Credit Note":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.CreditNotes() + "/" + entry.Value.rid + "/Matching", matchingArray);
                                                    }
                                            completed.Add(entry.Key, entry.Key);
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.UpdateVersion("creditNote");
                                    break;
                            }
                        }
                    }
                }
            return completed;
        }

        public void setAsEdited(SourceDocumentClass2 sd)
        {
            setup();

            DateTime dtt = DateTime.Now;
            string date = dtt.ToString("MMM, dd yyyy");
            string timeToPass = dtt.ToString("HH:mm:ss tt");

            sd.EditedYes = true;
            sd.EditedDate = date;
            sd.EditedTime = timeToPass;


            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    if (sd.date != null) if (sd.folio != null) if (sd.folio.Length > 0 && sd.date.Length > 0)
                            {
                                switch (sd.sdType)
                                {
                                    case "Receipt":
                                        FirebaseResponse firebasea = client.Set(DatabaseDirectory.AllReceipts() + "/" + sd.folio, sd);
                                        FirebaseResponse firebasea1 = client.Set(DatabaseDirectory.Receipts() + "/" + sd.date + "/receipts/" + sd.folio, sd);
                                        break;

                                    case "Invoice":
                                        FirebaseResponse firebasea2 = client.Set(DatabaseDirectory.AllInvoices() + "/" + sd.folio, sd);
                                        FirebaseResponse firebasea3 = client.Set(DatabaseDirectory.Invoices() + "/" + sd.date + "/invoices/" + sd.folio, sd);
                                        break;

                                    case "Quotation":
                                        FirebaseResponse firebase2 = client.Set(DatabaseDirectory.AllInvoices() + "/" + sd.folio, sd);
                                        break;

                                    case "Transfer Note":
                                        FirebaseResponse firebase3 = client.Set(DatabaseDirectory.TransferNotes() + "/" + sd.folio, sd);
                                        break;

                                    case "Order":
                                        FirebaseResponse firebase4 = client.Set(DatabaseDirectory.Orders() + "/" + sd.folio, sd);
                                        break;

                                    case "Credit Note":
                                        FirebaseResponse firebase5 = client.Set(DatabaseDirectory.CreditNotes() + "/" + sd.folio, sd);
                                        break;

                                    case "Debit Note":
                                        FirebaseResponse firebase6 = client.Set(DatabaseDirectory.DebitNote() + "/" + sd.folio, sd);
                                        break;
                                }
                            }
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
        }

        public void setAsEditedFsd(SourceDocumentClass2 sd)
        {
            setup();

            DateTime dtt = DateTime.Now;
            string date = dtt.ToString("MMM, dd yyyy");
            string timeToPass = dtt.ToString("HH:mm:ss tt");

            sd.EditedYes = true;
            sd.EditedDate = date;
            sd.EditedTime = timeToPass;


            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    if (sd.date != null) if (sd.folio != null) if (sd.folio.Length > 0 && sd.date.Length > 0)
                            {
                                switch (sd.sdType)
                                {
                                    case "ReceiptIn":
                                        FirebaseResponse firebasea = client.Set(DatabaseDirectory.AllReceiptsIn() + "/" + sd.folio, sd);
                                        FirebaseResponse firebasea1 = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + sd.date + "/sd/" + sd.folio, sd);
                                        break;

                                    case "InvoiceIn":
                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + sd.folio, sd);
                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.InvoicesIn() + "/" + sd.date + "/sd/" + sd.folio, sd);
                                        break;
                                    case "TransferNoteIn":

                                        break;
                                }
                            }
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
        }

        public void updateMathingReturns(Dictionary<string, MatchingClass> matchingArray, Dictionary<string, ProductEntrySDClass> products, bool isFsd, bool returnProductsYes, TextBox control, Action<bool> method)
        {
            bool success = false;

            setup();
            eid = prevelantClass.getEid();

            if (matchingArray != null) if (matchingArray.Count > 0)
                {
                    foreach (var entry in matchingArray)
                    {
                        if (isFsd)
                        {
                            switch (entry.Value.sdType)
                            {
                                case "Receipt":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceiptsIn() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + entry.Value.date + "/sd/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("receiptIn");
                                    break;
                                case "Invoice":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.InvoicesIn() + "/" + entry.Value.date + "/sd/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("invoiceIn");
                                    break;
                            }
                        } 
                        else
                        {
                            switch (entry.Value.sdType)
                            {
                                case "Receipt":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceipts() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Receipts() + "/" + entry.Value.date + "/receipts/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("receipt");
                                    break;
                                case "Invoice":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoices() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Invoices() + "/" + entry.Value.date + "/invoices/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("invoice");
                                    break;
                                case "Quotation":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Quotations() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("quotation");
                                    break;
                                case "Order":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Orders() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("order");
                                    break;
                                case "Transfer Note":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.TransferNotes() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("transferNote");
                                    break;
                                case "Debit Note":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.DebitNote() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("debitNote");
                                    break;
                                case "Credit Note":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.CreditNotes() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("creditNote");
                                    break;
                            }
                        }
                    }
                }


            if (returnProductsYes)//always return to stock then if returnProducts is false, remove them as damages
            {
                //return
                ProductProcessingServicesClass processingServicesClass = new();
                if (isFsd)
                {
                    productProcessingServices.updateQuantityDeduction(products);
                }
                else
                {
                    processingServicesClass.updateQuantityEntry(products, false, "");
                }
            }
            else
            {
                //write off as damaged
            }

            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void updateMathingReturns(Dictionary<string, MatchingClass> matchingArray, Dictionary<string, ProductEntrySDClass> products, bool isFsd, bool returnProductsYes, Action<bool> method)
        {
            bool success = false;

            setup();
            eid = prevelantClass.getEid();

            if (matchingArray != null) if (matchingArray.Count > 0)
                {
                    foreach (var entry in matchingArray)
                    {
                        if (isFsd)
                        {
                            switch (entry.Value.sdType)
                            {
                                case "Receipt":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceiptsIn() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + entry.Value.date + "/sd/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("receiptIn");
                                    break;
                                case "Invoice":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.InvoicesIn() + "/" + entry.Value.date + "/sd/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("invoiceIn");
                                    break;
                            }
                        }
                        else
                        {
                            switch (entry.Value.sdType)
                            {
                                case "Receipt":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceipts() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Receipts() + "/" + entry.Value.date + "/receipts/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("receipt");
                                    break;
                                case "Invoice":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoices() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Invoices() + "/" + entry.Value.date + "/invoices/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("invoice");
                                    break;
                                case "Quotation":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Quotations() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("quotation");
                                    break;
                                case "Order":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Orders() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("order");
                                    break;
                                case "Transfer Note":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.TransferNotes() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("transferNote");
                                    break;
                                case "Debit Note":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.DebitNote() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("debitNote");
                                    break;
                                case "Credit Note":
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                    {
                                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.CreditNotes() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                    }
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(duration);
                                        }
                                    }
                                    standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                    standardFirebaseOperationsClass.UpdateVersion("creditNote");
                                    break;
                            }
                        }
                    }
                }


            if (returnProductsYes)//always return to stock then if returnProducts is false, remove them as damages
            {
                //return
                ProductProcessingServicesClass processingServicesClass = new();
                if (isFsd)
                {
                    productProcessingServices.updateQuantityDeduction(products);
                }
                else
                {
                    processingServicesClass.updateQuantityEntry(products, false, "");
                }
            }
            else
            {
                //write off as damaged
            }
            method(success);
        }

        public async void updateDeliveryNote(SourceDocumentClass2 entry)
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();

            if (entry.dirPdf.Length > 0)
            {
                if (File.Exists(entry.dirPdf))
                {
                    string dNoteUrl = "";
                    for (int i1 = 0; i1 < numberOfTries; i1++)
                    {
                        try
                        {
                            var stream = File.Open(entry.dirPdf, FileMode.Open);

                            var task = new FirebaseStorage("long-walk-pos.appspot.com")
                                .Child("users")
                                .Child(uid)
                                .Child(bid)
                                .Child(entry.sdType + "s")
                                .Child(entry.folio + ".pdf")
                                .PutAsync(stream);

                            // Track progress of the upload
                            task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                            // await the task to wait until upload completes and get the download url
                            var downloadUrl = await task;
                            dNoteUrl = downloadUrl;
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }

                    if(dNoteUrl != null) if(dNoteUrl.Length > 0)
                        {
                            if (entry.Matching != null) if (entry.Matching.Count > 0)
                                {
                                    foreach (var matchingEntry in entry.Matching)
                                    {
                                        switch (matchingEntry.Value.sdType)
                                        {
                                            case "Receipt":
                                                for (int i = 0; i < numberOfTries; i++)
                                                {
                                                    try
                                                    {
                                                        if (matchingEntry.Value.date != null) if (matchingEntry.Value.rid != null) if (matchingEntry.Value.rid.Length > 0 && matchingEntry.Value.date.Length > 0)
                                                                {
                                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceipts() + "/" + matchingEntry.Value.rid + "/matchingDNote", true);
                                                                    FirebaseResponse firebase1 = client.Set(DatabaseDirectory.AllReceipts() + "/" + matchingEntry.Value.rid + "/dNoteUrl", dNoteUrl);
                                                                    FirebaseResponse firebasea = client.Set(DatabaseDirectory.Receipts() + "/" + matchingEntry.Value.date + "/receipts/" + matchingEntry.Value.rid + "/matchingDNote", true);
                                                                    FirebaseResponse firebasea1 = client.Set(DatabaseDirectory.Receipts() + "/" + matchingEntry.Value.date + "/receipts/" + matchingEntry.Value.rid + "/dNoteUrl", dNoteUrl);
                                                                }
                                                        break;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Thread.Sleep(duration);
                                                    }
                                                }
                                                standardFirebaseOperationsClass.UpdateVersion("receipt");
                                                break;
                                            case "Invoice":
                                                for (int i = 0; i < numberOfTries; i++)
                                                {
                                                    try
                                                    {
                                                        if (matchingEntry.Value.date != null) if (matchingEntry.Value.rid != null) if (matchingEntry.Value.rid.Length > 0 && matchingEntry.Value.date.Length > 0)
                                                                {
                                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoices() + "/" + matchingEntry.Value.rid + "/matchingDNote", true);
                                                                    FirebaseResponse firebase1 = client.Set(DatabaseDirectory.AllInvoices() + "/" + matchingEntry.Value.rid + "/dNoteUrl", dNoteUrl);
                                                                    FirebaseResponse firebasea = client.Set(DatabaseDirectory.Invoices() + "/" + matchingEntry.Value.date + "/invoices/" + matchingEntry.Value.rid + "/matchingDNote", true);
                                                                    FirebaseResponse firebasea1 = client.Set(DatabaseDirectory.Invoices() + "/" + matchingEntry.Value.date + "/invoices/" + matchingEntry.Value.rid + "/dNoteUrl", dNoteUrl);
                                                                }
                                                        break;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Thread.Sleep(duration);
                                                    }
                                                }
                                                standardFirebaseOperationsClass.UpdateVersion("invoices");
                                                break;
                                            case "Quotation":
                                                for (int i = 0; i < numberOfTries; i++)
                                                {
                                                    try
                                                    {
                                                        if (matchingEntry.Value.date != null) if (matchingEntry.Value.rid != null) if (matchingEntry.Value.rid.Length > 0 && matchingEntry.Value.date.Length > 0)
                                                                {
                                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Quotations() + "/" + matchingEntry.Value.rid + "/matchingDNote", true);
                                                                    FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Quotations() + "/" + matchingEntry.Value.rid + "/dNoteUrl", dNoteUrl);
                                                                }
                                                        break;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Thread.Sleep(duration);
                                                    }
                                                }
                                                standardFirebaseOperationsClass.UpdateVersion("quotation");
                                                break;
                                            case "Order":
                                                for (int i = 0; i < numberOfTries; i++)
                                                {
                                                    try
                                                    {
                                                        if (matchingEntry.Value.date != null) if (matchingEntry.Value.rid != null) if (matchingEntry.Value.rid.Length > 0 && matchingEntry.Value.date.Length > 0)
                                                                {
                                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Orders() + "/" + matchingEntry.Value.rid + "/matchingDNote", true);
                                                                    FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Orders() + "/" + matchingEntry.Value.rid + "/dNoteUrl", dNoteUrl);
                                                                }
                                                        break;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Thread.Sleep(duration);
                                                    }
                                                }
                                                standardFirebaseOperationsClass.UpdateVersion("order");
                                                break;
                                            case "Transfer Note":
                                                for (int i = 0; i < numberOfTries; i++)
                                                {
                                                    try
                                                    {
                                                        if (matchingEntry.Value.date != null) if (matchingEntry.Value.rid != null) if (matchingEntry.Value.rid.Length > 0 && matchingEntry.Value.date.Length > 0)
                                                                {
                                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.TransferNotes() + "/" + matchingEntry.Value.rid + "/matchingDNote", true);
                                                                    FirebaseResponse firebase1 = client.Set(DatabaseDirectory.TransferNotes() + "/" + matchingEntry.Value.rid + "/dNoteUrl", dNoteUrl);
                                                                }
                                                        break;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Thread.Sleep(duration);
                                                    }
                                                }
                                                standardFirebaseOperationsClass.UpdateVersion("transferNote");
                                                break;
                                            case "Debit Note":
                                                for (int i = 0; i < numberOfTries; i++)
                                                {
                                                    try
                                                    {
                                                        if (matchingEntry.Value.date != null) if (matchingEntry.Value.rid != null) if (matchingEntry.Value.rid.Length > 0 && matchingEntry.Value.date.Length > 0)
                                                                {
                                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.DebitNote() + "/" + matchingEntry.Value.rid + "/matchingDNote", true);
                                                                    FirebaseResponse firebase1 = client.Set(DatabaseDirectory.DebitNote() + "/" + matchingEntry.Value.rid + "/dNoteUrl", dNoteUrl);
                                                                }
                                                        break;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Thread.Sleep(duration);
                                                    }
                                                }
                                                standardFirebaseOperationsClass.UpdateVersion("debitNote");
                                                break;
                                            case "Credit Note":
                                                for (int i = 0; i < numberOfTries; i++)
                                                {
                                                    try
                                                    {
                                                        if (matchingEntry.Value.date != null) if (matchingEntry.Value.rid != null) if (matchingEntry.Value.rid.Length > 0 && matchingEntry.Value.date.Length > 0)
                                                                {
                                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.CreditNotes() + "/" + matchingEntry.Value.rid + "/matchingDNote", true);
                                                                    FirebaseResponse firebase1 = client.Set(DatabaseDirectory.CreditNotes() + "/" + matchingEntry.Value.rid + "/dNoteUrl", dNoteUrl);
                                                                }
                                                        break;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Thread.Sleep(duration);
                                                    }
                                                }
                                                standardFirebaseOperationsClass.UpdateVersion("creditNote");
                                                break;
                                        }
                                    }
                                }
                        }
                }
            }
        }
    }
}
