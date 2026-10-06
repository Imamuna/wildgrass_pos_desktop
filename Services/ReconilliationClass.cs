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
using WildGrass_Desktop_f8.Activities.dialogs;
using WildGrass_Desktop_f8;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace WildGrass_Desktop
{
    class ReconilliationClass
    {
        private string uid;
        private string bid;
        IFirebaseClient client;
        DatabaseDirectoryServicesClass DatabaseDirectory = new();
        WildGrassPOSLibrary.Services.LocalDirectoryServicesClass localDirectory = new();

        bool receiptYes = false;
        bool invoiceYes = false;
        bool quotationYes = false;
        bool productsYes = false;
        bool categoryYes = false;
        bool tierYes = false;
        bool trackingYes = false;
        bool expenseYes = false;
        bool taxYes = false;
        bool employeeYes = false;
        bool notificationYes = false;
        //add-on check parameters
        bool settingsYes = false;
        bool userYes = false;
        bool businessYes = false;
        bool orderYes = false;
        bool tNoteYes = false;
        bool customerYes = false;
        bool supplierYes = false;
        bool receiptInYes = false;
        bool invoiceInYes = false;

        FirebaseResponse currentFirebaseVersionsRes;


        Dictionary<string, ProductClass> productsArray = new Dictionary<string, ProductClass>();
        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new ();

        int duration = 2000;
        int numberOfTries = 3;

        Action showOnline;
        Action showOffline;
        Action ResolveQueuedData;
        Action loadUI;
        Action<string> currentlyRunning;
        CounterControl control;
        CancellationToken cancelToken;

        public void activeReconcilliation(Action showOnline, Action showOffline, Action loadUI, Action<string> currenctlyLoading, CounterControl control, CancellationToken cancelToken, Action resolveQueued)
        {
            this.showOnline = showOnline;
            this.showOffline = showOffline;
            this.loadUI = loadUI;
            this.control = control;
            currentlyRunning = currenctlyLoading;
            ResolveQueuedData = resolveQueued;
            this.cancelToken = cancelToken;

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

            checkConnection();
        }

        private void checkConnection()
        {
            if (cancelToken.IsCancellationRequested)
            {
                //we have stopped the operation
                //MessageBox.Show("We are closing reconcilliation");
                //cancelToken.ThrowIfCancellationRequested();
            } else
            {
                bool result = IsConnectedToInternet();

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

        public Dictionary<string, SourceDocumentClass2> addAllQueuedDataReceivables(Dictionary<string, SourceDocumentClass2> invoicesArray)
        {
            Dictionary<string, QueuedSdDataClass> queuedDataArray = prevelantClass.LoadQueuedData();

            if (queuedDataArray != null)
            {
                if (queuedDataArray.Count > 0)
                {
                    foreach (var entry in queuedDataArray)
                    {
                        if (entry.Value.Sd.sdType == "Invoice")
                        {
                            if (!invoicesArray.ContainsKey(entry.Value.Sd.folio))
                            {
                                invoicesArray.Add(entry.Value.Sd.folio, entry.Value.Sd);
                            }
                        }
                    }
                }
            }

            return invoicesArray;
        }

        public Dictionary<string, SourceDocumentClass2> addQueuedDataQuotation(Dictionary<string, SourceDocumentClass2> quotationsArray)
        {
            Dictionary<string, QueuedSdDataClass> queuedDataArray = prevelantClass.LoadQueuedData();

            if (queuedDataArray != null)
            {
                if (queuedDataArray.Count != 0)
                {
                    foreach (var entry in queuedDataArray)
                    {
                        if (entry.Value.Sd.sdType == "Quotation")
                        {
                            if (!quotationsArray.ContainsKey(entry.Value.Sd.folio))
                            {
                                quotationsArray.Add(entry.Value.Sd.folio, entry.Value.Sd);
                            }
                        }
                    }
                }
            }

            return quotationsArray;
        }

        public Dictionary<string, SourceDocumentClass2> addQueuedDataOrder(Dictionary<string, SourceDocumentClass2> ordersArray)
        {
            Dictionary<string, QueuedSdDataClass> queuedDataArray = prevelantClass.LoadQueuedData();

            if (queuedDataArray != null)
            {
                if (queuedDataArray.Count != 0)
                {
                    foreach (var entry in queuedDataArray)
                    {
                        if (entry.Value.Sd.sdType == "Order")
                        {
                            if (!ordersArray.ContainsKey(entry.Value.Sd.folio))
                            {
                                ordersArray.Add(entry.Value.Sd.folio, entry.Value.Sd);
                            }
                        }
                    }
                }
            }

            return ordersArray;
        }

        public Dictionary<string, SourceDocumentClass2> addQueuedDataTransferNote(Dictionary<string, SourceDocumentClass2> dNoteArray)
        {
            Dictionary<string, QueuedSdDataClass> queuedDataArray = prevelantClass.LoadQueuedData();

            if (queuedDataArray != null)
            {
                if (queuedDataArray.Count != 0)
                {
                    foreach (var entry in queuedDataArray)
                    {
                        if (entry.Value.Sd.sdType == "Transfer Note")
                        {
                            if (!dNoteArray.ContainsKey(entry.Value.Sd.folio))
                            {
                                dNoteArray.Add(entry.Value.Sd.folio, entry.Value.Sd);
                            }
                        }
                    }
                }
            }

            return dNoteArray;
        }

        public Dictionary<string, DateClass> addQueuedDataReceipt(Dictionary<string, DateClass> dateArray)
        {
            Dictionary<string, QueuedSdDataClass> queuedDataArray = prevelantClass.LoadQueuedData();

            if (queuedDataArray != null)
            {
                if (queuedDataArray.Count != 0)
                {

                    foreach (var entry in queuedDataArray)
                    {
                        if (entry.Value.Sd.sdType == "Receipt")
                        {
                            if (dateArray == null)
                                dateArray = new();
                            if (!dateArray.ContainsKey(entry.Value.Sd.date))
                                {
                                    //create the date entry before continueing
                                    DateClass date = new DateClass
                                    {
                                        date = entry.Value.Sd.date,
                                        day = entry.Value.Sd.day,
                                        month = entry.Value.Sd.month,
                                        year = entry.Value.Sd.year,
                                        comp = entry.Value.Sd.month + entry.Value.Sd.year,
                                        receipts = new Dictionary<string, SourceDocumentClass2>()
                                    };
                                    dateArray.Add(date.date, date);
                                }

                            Dictionary<string, SourceDocumentClass2> receiptArray = dateArray[entry.Value.Sd.date].receipts;
                            if (receiptArray == null) receiptArray = new();

                            if (!receiptArray.ContainsKey(entry.Value.Sd.folio))
                            {
                                receiptArray.Add(entry.Value.Sd.folio, entry.Value.Sd);
                            }
                            dateArray[entry.Value.Sd.date].receipts = receiptArray;
                        }
                    }
                }
            }

            return dateArray;
        }

        public Dictionary<string, DateClass> addQueuedDataInvoice(Dictionary<string, DateClass> dateArray)
        {
            Dictionary<string, QueuedSdDataClass> queuedDataArray = prevelantClass.LoadQueuedData();

            if (queuedDataArray != null)
            {
                if (queuedDataArray.Count != 0)
                {
                    foreach (var entry in queuedDataArray)
                    {
                        if (entry.Value.Sd.sdType == "Invoice")
                        {
                            if (dateArray == null)
                                dateArray = new();
                            if (!dateArray.ContainsKey(entry.Value.Sd.date))
                            {
                                //create the date entry before continueing
                                DateClass date = new DateClass
                                {
                                    date = entry.Value.Sd.date,
                                    day = entry.Value.Sd.day,
                                    month = entry.Value.Sd.month,
                                    year = entry.Value.Sd.year,
                                    comp = entry.Value.Sd.month + entry.Value.Sd.year,
                                    invoices = new Dictionary<string, SourceDocumentClass2>()
                                };
                                dateArray.Add(date.date, date);
                            }


                            Dictionary<string, SourceDocumentClass2> invoicesArray = dateArray[entry.Value.Sd.date].invoices;
                            if (invoicesArray == null)
                                invoicesArray = new();

                            if (!invoicesArray.ContainsKey(entry.Value.Sd.folio))
                            {
                                invoicesArray.Add(entry.Value.Sd.folio, entry.Value.Sd);
                            }
                            dateArray[entry.Value.Sd.date].invoices = invoicesArray;
                        }
                    }
                }
            }

            return dateArray;
        }

        public Dictionary<string, ProductClass> addQueuedDataProduct(Dictionary<string, ProductClass> productsArray)
        {
            //Dictionary<string, QueuedSdDataClass> queuedDataArray = new Dictionary<string, QueuedSdDataClass>();

            //string dir = localDirectory.QueuedData();

            //if (File.Exists(dir))
            //{
            //    using (StreamReader r = new StreamReader(dir))
            //    {
            //        string json = r.ReadToEnd();
            //        if (json != null)
            //        {
            //            queuedDataArray = JsonConvert.DeserializeObject<Dictionary<string, QueuedSdDataClass>>(json);
            //        }
            //    }
            //}
            //else
            //{
            //    queuedDataArray = null;
            //}

            //if (queuedDataArray != null)
            //{
            //    if (queuedDataArray.Count != 0)
            //    {
            //        foreach (var entry in queuedDataArray)
            //        {
            //            if (entry.Value.Sd.sdType != "Quotation")
            //            {
            //                Dictionary<string, ProductEntrySDClass> onGoingArray = entry.Value.Sd.products;
            //                if (onGoingArray != null)
            //                {
            //                    foreach (var product in onGoingArray)
            //                    {
            //                        if (productsArray[product.Key].inventoryTracking == "on")
            //                        {
            //                            double qtyInStock = Convert.ToDouble(productsArray[product.Key].qtyInStock);
            //                            double reduceBy = Convert.ToDouble(product.Value.quantity);
            //                            double quantity = qtyInStock - reduceBy;

            //                            productsArray[product.Key].qtyInStock = Convert.ToString(quantity);
            //                        }
            //                    }
            //                }
            //            }

            //        }
            //    }
            //}

            return productsArray;
        }

        public void ResolveQueuedData_phasedOut()
        {
            WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();

            Dictionary<string, QueuedSdDataClass> queuedDataArray = new();

            string systemPath1 = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete1 = Path.Combine(systemPath1, "WildGrass");
            string dir = complete1 + @"\data\" + uid + @"\businesses\" + bid;
            dir += @"\QueuedData.txt";

            if (File.Exists(dir))
            {
                using (StreamReader r = new StreamReader(dir))
                {
                    string json = r.ReadToEnd();
                    if (json != null)
                    {
                        queuedDataArray = JsonConvert.DeserializeObject<Dictionary<string, QueuedSdDataClass>>(json);
                    }
                }
            }
            bool weHaveData = false;
            if (queuedDataArray != null) if (queuedDataArray.Count > 0)
                {
                    weHaveData = true;
                    showIssue("resolving queued data");
                    string eid = prevelantClass.getEid();

                    ProcessTransactionServicesClass processTransactionServices = new(uid, bid, eid, client);
                    foreach (var entry in queuedDataArray)
                    {
                        //main.counterMenu.transactionHandlerControl.ProcessTransaction(entry.Value);
                        processTransactionServices.GenerateSD(entry.Value, true, EmptyCall);
                    }
                }

            if (!weHaveData)
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        File.Delete(dir);
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }
        }

        private void EmptyCall(SourceDocumentClass2 sd, bool success)
        {

        }

        private void tryAgainSoon()
        {
            Thread.Sleep(4000);

            checkConnection();
        }

        public bool checkVersionProduct()
        {
            productsYes = false;

            if (File.Exists(localDirectory.Versions()))
            {
                FirebaseResponse res;
                try
                {
                    res = client.Get(@"FPCS/Users/" + uid + "/businesses/" + bid + "/Versions");

                }
                catch (Exception e)
                {
                    res = null;
                }

                if (res != null)
                {
                    Dictionary<string, VersionClass> versionsArray = JsonConvert.DeserializeObject<Dictionary<string, VersionClass>>(res.Body.ToString());

                    using (StreamReader r = new StreamReader(localDirectory.Versions()))
                    {
                        string json = r.ReadToEnd();
                        Dictionary<string, VersionClass> versionsArrayLocal = JsonConvert.DeserializeObject<Dictionary<string, VersionClass>>(json);

                        if (versionsArrayLocal != null)
                        {
                            if (versionsArray["Current"].product != versionsArrayLocal["Current"].product)
                            {
                                productsYes = true;
                            }
                        }
                    }
                }
            }

            return productsYes;
        }

        private async void checkVersions()
        {
            receiptYes = false;
            invoiceYes = false;
            quotationYes = false;
            productsYes = false;
            categoryYes = false;
            tierYes = false;
            trackingYes = false;
            employeeYes = false;
            settingsYes = false;
            notificationYes = false;
            userYes = false;
            businessYes = false;
            expenseYes = false;
            taxYes = false;
            orderYes = false;
            tNoteYes = false;
            customerYes = false;
            supplierYes = false;
            receiptInYes = false;
            invoiceInYes = false;

            if (File.Exists(localDirectory.Versions()))
            {
                FirebaseResponse res;
                try
                {
                    res = await client.GetAsync(DatabaseDirectory.Versions());

                } catch (Exception e)
                {
                    res = null;
                }

                if (res != null)
                {
                    if(res.Body.ToString() != "null")
                    {
                        Dictionary<string, VersionClass> versionsArray = JsonConvert.DeserializeObject<Dictionary<string, VersionClass>>(res.Body.ToString());
                        Dictionary<string, VersionClass> versionsArrayLocal = null;
                        for(int i = 0; i < 3; i++)
                        {
                            try
                            {
                                using (StreamReader r = new StreamReader(localDirectory.Versions()))
                                {
                                    string json = r.ReadToEnd();
                                    versionsArrayLocal = JsonConvert.DeserializeObject<Dictionary<string, VersionClass>>(json);

                                    if (versionsArrayLocal != null)
                                    {
                                        //if any of them is different call the cavary
                                        if (versionsArray["Current"].receipt != versionsArrayLocal["Current"].receipt)
                                        {
                                            receiptYes = true;
                                        }
                                        if (versionsArray["Current"].invoice != versionsArrayLocal["Current"].invoice)
                                        {
                                            invoiceYes = true;
                                        }
                                        if (versionsArray["Current"].quotation != versionsArrayLocal["Current"].quotation)
                                        {
                                            quotationYes = true;
                                        }
                                        if (versionsArray["Current"].expense != versionsArrayLocal["Current"].expense)
                                        {
                                            expenseYes = true;
                                        }
                                        if (versionsArray["Current"].tax != versionsArrayLocal["Current"].tax)
                                        {
                                            taxYes = true;
                                        }
                                        if (versionsArray["Current"].product != versionsArrayLocal["Current"].product)
                                        {
                                            productsYes = true;
                                            categoryYes = true;
                                            tierYes = true;
                                        }
                                        if (versionsArray["Current"].tracking != versionsArrayLocal["Current"].tracking)
                                        {
                                            trackingYes = true;

                                        }
                                        if (versionsArray["Current"].employee != versionsArrayLocal["Current"].employee)
                                        {
                                            employeeYes = true;
                                        }
                                        if (versionsArray["Current"].settings != null)
                                        {
                                            if (versionsArray["Current"].settings != versionsArrayLocal["Current"].settings)
                                            {
                                                settingsYes = true;
                                            }
                                        }
                                        if (versionsArray["Current"].notification != null)
                                        {
                                            if (versionsArray["Current"].notification != versionsArrayLocal["Current"].notification)
                                            {
                                                notificationYes = true;
                                            }
                                        }
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
                                        if (versionsArray["Current"].order != null)
                                        {
                                            if (versionsArray["Current"].order != versionsArrayLocal["Current"].order)
                                            {
                                                orderYes = true;
                                            }
                                        }
                                        if (versionsArray["Current"].transferNote != null)
                                        {
                                            if (versionsArray["Current"].transferNote != versionsArrayLocal["Current"].transferNote)
                                            {
                                                tNoteYes = true;
                                            }
                                        }
                                        if (versionsArray["Current"].customer != null)
                                        {
                                            if (versionsArray["Current"].customer != versionsArrayLocal["Current"].customer)
                                            {
                                                customerYes = true;
                                            }
                                        }
                                        if (versionsArray["Current"].supplier != null)
                                        {
                                            if (versionsArray["Current"].supplier != versionsArrayLocal["Current"].supplier)
                                            {
                                                supplierYes = true;
                                            }
                                        }
                                        if (versionsArray["Current"].receiptIn != null)
                                        {
                                            if (versionsArray["Current"].receiptIn != versionsArrayLocal["Current"].receiptIn)
                                            {
                                                receiptInYes = true;
                                            }
                                        }
                                        if (versionsArray["Current"].invoiceIn != null)
                                        {
                                            if (versionsArray["Current"].invoiceIn != versionsArrayLocal["Current"].invoiceIn)
                                            {
                                                invoiceInYes = true;
                                            }
                                        }
                                    }
                                }
                                if (versionsArrayLocal == null)
                                {
                                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Versions());

                                    string dir = localDirectory.BusinessFolder();
                                    if (!Directory.Exists(dir))
                                    {
                                        Directory.CreateDirectory(dir);
                                    }
                                    currentFirebaseVersionsRes = res1;
                                    allReceipt();
                                }

                                if (receiptYes || invoiceYes || quotationYes || productsYes || categoryYes || tierYes || trackingYes || employeeYes || settingsYes || userYes || businessYes || notificationYes || orderYes || tNoteYes || customerYes || supplierYes || receiptInYes || invoiceInYes || expenseYes || taxYes)
                                {
                                    currentFirebaseVersionsRes = res;
                                    allReceipt();
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
                    } else
                    {
                        Thread.Sleep(8000);
                        tryAgainSoon();
                    }        
                } else
                {
                    Thread.Sleep(8000);
                    tryAgainSoon();
                }
            }
            else
            {
                FirebaseResponse res = await client.GetAsync(DatabaseDirectory.Versions());

                if(res != null)
                {
                    if(res.Body.ToString() != "null")
                    {
                        string dir = localDirectory.BusinessFolder();
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        File.WriteAllText(localDirectory.Versions(), res.Body.ToString());
                        receiptYes = true;
                        invoiceYes = true;
                        quotationYes = true;
                        productsYes = true;
                        categoryYes = true;
                        tierYes = true;
                        trackingYes = true;
                        employeeYes = true;
                        settingsYes = true;
                        notificationYes = true;
                        userYes = true;
                        businessYes = true;
                        expenseYes = true;
                        taxYes = true;
                        orderYes = true;
                        tNoteYes = true;
                        customerYes = true;
                        supplierYes = true;
                        receiptInYes = true;
                        invoiceInYes = true;
                        allReceipt();
                    } else
                    {
                        Thread.Sleep(8000);
                        tryAgainSoon();
                    }
                } else
                {
                    Thread.Sleep(8000);
                    tryAgainSoon();
                }
            }
        }

        private void showIssue(string message)
        {
            control.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    currentlyRunning, message);
        }

        private void allReceipt()
        {
            control.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    currentlyRunning, "Loading");
            if (receiptYes)
            {
                downloadReceipt(allInvoice);
            }
            else
            {
                allInvoice();
            }

        }
        private void allInvoice()
        {
            if (invoiceYes)
            {
                downloadInvoice(allAllInvoice);
            }
            else
            {
                allAllInvoice();
            }

        }
        private void allAllInvoice()
        {
            if (invoiceYes)
            {
                downloadAllInvoices(allQuotation);
            }
            else
            {
                allQuotation();
            }

        }
        private void allQuotation()
        {
            if (quotationYes)
            {
                downloadQuotation(allTNote);
            }
            else
            {
                allTNote();
            }

        }
        private void allTNote()
        {
            if (tNoteYes)
            {
                downloadTNote(allOrder);
            }
            else
            {
                allOrder();
            }

        }
        private void allOrder()
        {
            if (orderYes)
            {
                downloadOrder(allExpenses);
            }
            else
            {
                allExpenses();
            }

        }
        private void allExpenses()
        {
            if (expenseYes)
            {
                downloadExpenses(allTaxes);
            }
            else
            {
                allTaxes();
            }

        }
        private void allTaxes()
        {
            if (taxYes)
            {
                downloadTaxes(allProduct);
            }
            else
            {
                allProduct();
            }

        }
        private void allProduct()
        {
            if (productsYes)
            {
                downloadProducts(allCategory);
            }
            else
            {
                allCategory();
            }

        }
        private void allCategory()
        {
            if (categoryYes)
            {
                downloadCategories(allTier);
            }
            else
            {
                allTier();
            }

        }
        private void allTier()
        {
            if (tierYes)
            {
                downloadTiers(allTracking);
            }
            else
            {
                allTracking();
            }

        }
        private void allTracking()
        {
            if (trackingYes)
            {
                downloadTracking(allEmployee);
            }
            else
            {
                allEmployee();
            }

        }
        private void allEmployee()
        {
            if (receiptYes || invoiceYes || quotationYes || productsYes || categoryYes || trackingYes || employeeYes || settingsYes || userYes || businessYes || notificationYes)
            {
                downloadEmployees(allSettings);
            }
            else
            {
                allSettings();
            }

        }
        private void allSettings()
        {
            if (settingsYes || userYes || businessYes)
            {
                downloadSettings(allNotifications);
            }
            else
            {
                allNotifications();
            }
        }
        private void allNotifications()
        {
            if (notificationYes)
            {
                downloadNotifications(allCustomers);
            } else
            {
                allCustomers();
            }
        }
        private void allCustomers()
        {
            if (customerYes)
            {
                downloadCustomer(allSuppliers);
            }
            else
            {
                allSuppliers();
            }

        }
        private void allSuppliers()
        {
            if (supplierYes)
            {
                downloadSupplier(allReceiptIns);
            }
            else
            {
                allReceiptIns();
            }

        }
        private void allReceiptIns()
        {
            if (receiptInYes)
            {
                downloadReceiptIn(allInvoiceIns);
            }
            else
            {
                allInvoiceIns();
            }
        }
        private void allInvoiceIns()
        {
            if (invoiceInYes)
            {
                downloadInvoiceIn(makeCall);
            }
            else
            {
                makeCall();
            }
        }

        private void makeCall()
        {   //we only update versions when, they have been updated successfully
            control.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    loadUI);
            string dir = localDirectory.BusinessFolder();
            string url = localDirectory.Versions();
            if(currentFirebaseVersionsRes != null)
            {
                try
                {
                    System.IO.File.WriteAllText(url, currentFirebaseVersionsRes.Body.ToString());
                }
                catch (Exception)
                {
                    Thread.Sleep(5000);
                    try
                    {
                        System.IO.File.WriteAllText(url, currentFirebaseVersionsRes.Body.ToString());
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(8000);
                        try
                        {
                            System.IO.File.WriteAllText(url, currentFirebaseVersionsRes.Body.ToString());
                        }
                        catch (Exception)
                        {
                            //tryAgainSoon();
                        }
                    }
                }
                checkConnection();
            } else
            {
                tryAgainSoon();
            }
        }

        private async void downloadReceipt(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                FirebaseResponse res = null;
                try
                {
                    res = await client.GetAsync(DatabaseDirectory.Receipts());
                }
                catch (Exception)
                {
                    res = null;
                }

                if (res != null)
                {
                    //create directory
                    try
                    {
                        string dir1 = localDirectory.ReceiptsFolder();
                        string url = localDirectory.Receipts();
                        if (!Directory.Exists(dir1))
                        {
                            Directory.CreateDirectory(dir1);
                        }

                        if (res.Body.ToString() == "null")
                        {
                            System.IO.File.WriteAllText(url, "");
                        }
                        else
                        {
                            System.IO.File.WriteAllText(url, res.Body.ToString());
                        }


                        bool getReturns = await downloadCreditNotes();

                        if (getReturns)
                        {
                            method();
                        }
                        else
                        {
                            tryAgainSoon();
                        }
                    }
                    catch (Exception)
                    {
                        tryAgainSoon();
                    }
                    
                }
                else
                {
                    checkConnection();
                }
            } else
            {
                checkConnection();
            }
        }

        private async void downloadInvoice(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                try
                {
                    FirebaseResponse res = await client.GetAsync(DatabaseDirectory.Invoices());

                    //create directory
                    string dir1 = localDirectory.InvoicesFolder();
                    string url = localDirectory.Invoices();
                    if (!Directory.Exists(dir1))
                    {
                        Directory.CreateDirectory(dir1);
                    }
                    
                    if (res.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res.Body.ToString());
                    }


                    method();
                }
                catch (Exception)
                {
                    tryAgainSoon();
                }
            }
            else
            {
                checkConnection();
            }
        }

        private async void downloadAllInvoices(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.InvoicesFolder();
                string url = localDirectory.AllInvoices();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.AllInvoices());
                    
                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {

                    bool getReturns = await downloadCreditNotes();

                    if (getReturns)
                    {
                        method();
                    }
                    else
                    {
                        tryAgainSoon();
                    }
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }
        }

        private async void downloadQuotation(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.QuotationsFolder();
                string url = localDirectory.Quotations();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Quotations());
                    
                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }

        }

        private async void downloadCustomer(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.CustomersFolder();
                string url = localDirectory.Customers();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Customers());

                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }

        }

        private async void downloadSupplier(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.SuppliersFolder();
                string url = localDirectory.Suppliers();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Suppliers());

                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }

        }

        private async void downloadReceiptIn(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                FirebaseResponse res = null;
                try
                {
                    res = await client.GetAsync(DatabaseDirectory.ReceiptsIn());
                }
                catch (Exception)
                {
                    res = null;
                }

                if (res != null)
                {
                    //create directory
                    try
                    {
                        string dir1 = localDirectory.ReceiptsInFolder();
                        string url = localDirectory.ReceiptsIn();
                        if (!Directory.Exists(dir1))
                        {
                            Directory.CreateDirectory(dir1);
                        }

                        if (res.Body.ToString() == "null")
                        {
                            System.IO.File.WriteAllText(url, "");
                        }
                        else
                        {
                            System.IO.File.WriteAllText(url, res.Body.ToString());
                        }

                        bool getReturns = await downloadDebitNotes();

                        if (getReturns)
                        {
                            method();
                        } else
                        {
                            tryAgainSoon();
                        }

                    }
                    catch (Exception)
                    {
                        tryAgainSoon();
                    }

                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }
        }

        private async void downloadInvoiceIn(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                FirebaseResponse res = null;
                try
                {
                    res = await client.GetAsync(DatabaseDirectory.InvoicesIn());
                }
                catch (Exception)
                {
                    res = null;
                }

                if (res != null)
                {
                    //create directory
                    try
                    {
                        string dir1 = localDirectory.InvoicesInFolder();
                        string url = localDirectory.InvoicesIn();
                        if (!Directory.Exists(dir1))
                        {
                            Directory.CreateDirectory(dir1);
                        }

                        if (res.Body.ToString() == "null")
                        {
                            System.IO.File.WriteAllText(url, "");
                        }
                        else
                        {
                            System.IO.File.WriteAllText(url, res.Body.ToString());
                        }

                        downloadAllInvoicesIn(method);
                    }
                    catch (Exception)
                    {
                        tryAgainSoon();
                    }

                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }
        }

        private async void downloadAllInvoicesIn(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.InvoicesFolder();
                string url = localDirectory.AllInvoicesIn();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.AllInvoicesIn());

                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {

                    bool getReturns = await downloadDebitNotes();

                    if (getReturns)
                    {
                        method();
                    }
                    else
                    {
                        tryAgainSoon();
                    }
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }
        }

        private async void downloadOrder(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.OrdersFolder();
                string url = localDirectory.Orders();

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Orders());

                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }

        }

        private async Task<bool> downloadDebitNotes()
        {
            bool success = false;
            FirebaseResponse res = null;
            try
            {
                res = await client.GetAsync(DatabaseDirectory.DebitNote());
            }
            catch (Exception)
            {
                res = null;
            }

            if (res != null)
            {
                //create directory
                try
                {
                    string dir1 = localDirectory.DebitNotesFolder();
                    string url = localDirectory.DebitNotes();
                    if (!Directory.Exists(dir1))
                    {
                        Directory.CreateDirectory(dir1);
                    }

                    if (res.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res.Body.ToString());
                    }

                    success = true;
                }
                catch (Exception)
                {
                }

            }
            return success;
        }

        private async Task<bool> downloadCreditNotes()
        {
            bool success = false;
            FirebaseResponse res = null;
            try
            {
                res = await client.GetAsync(DatabaseDirectory.CreditNotes());
            }
            catch (Exception)
            {
                res = null;
            }

            if (res != null)
            {
                //create directory
                try
                {
                    string dir1 = localDirectory.CreditNotesFolder();
                    string url = localDirectory.CreditNotes();
                    if (!Directory.Exists(dir1))
                    {
                        Directory.CreateDirectory(dir1);
                    }

                    if (res.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res.Body.ToString());
                    }

                    success = true;
                }
                catch (Exception)
                {
                }

            }
            return success;
        }

        private async void downloadTNote(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.TransferNotesFolder();
                string url = localDirectory.TransferNotes();

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.TransferNotes());

                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }

        }

        private async void downloadExpenses(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.ExpensesFolder();
                string url = localDirectory.Expenses();
                string url2 = localDirectory.ExpenseCategories();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Expenses());
                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }


                    FirebaseResponse res2 = await client.GetAsync(DatabaseDirectory.ExpenseCategories());

                    if (res2.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url2, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url2, res2.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }
        }

        private async void downloadTaxes(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.TaxesFolder();
                string url = localDirectory.Taxes();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Taxes());
                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }
        }

        private async void downloadProducts(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.ProductsFolder();
                string url = localDirectory.Products();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Products());
                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    } else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }
        }

        private async void downloadCategories(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.ProductsFolder();
                string url = localDirectory.Categories();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Categories());
                    
                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }

        }

        private async void downloadTiers(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.ProductsFolder();
                string url = localDirectory.Tiers();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Tiers());

                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }

        }

        private async void downloadTracking(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.ProductsFolder();
                string url = localDirectory.QuantityChange();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    //FirebaseResponse res1 = await client.GetAsync(@"Users/" + uid + "/businesses/" + bid + "/Tracking");
                    
                    //if (res1.Body.ToString() == "null")
                    //{
                    //    System.IO.File.WriteAllText(dir + @"\Tracking.txt", "");
                    //}
                    //else
                    //{
                    //    System.IO.File.WriteAllText(dir + @"\Tracking.txt", res1.Body.ToString());
                    //}


                    FirebaseResponse res2 = await client.GetAsync(DatabaseDirectory.QuantityChange());
                    if (res2.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res2.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }
        }

        private async void downloadEmployees(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.EmployeesFolder();
                string url = localDirectory.Employees();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Employees());
                    
                    if (res1.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res1.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }
        }

        private async void downloadSettings(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                StandardFirebaseOperationsClass standardOperations = new StandardFirebaseOperationsClass();
                standardOperations.DownloadLogo();

                //create directory
                string dir2 = localDirectory.UserFolder();
                string url = localDirectory.UserData();
                if (!Directory.Exists(dir2))
                {
                    Directory.CreateDirectory(dir2);
                }

                string dir = localDirectory.SettingsFolder();
                string url1 = localDirectory.Settings();
                //if path doesn't exist we need to create it
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res2 = await client.GetAsync(DatabaseDirectory.FPCSUser());
                    if (res2.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res2.Body.ToString());
                    }

                    FirebaseResponse res3 = await client.GetAsync(DatabaseDirectory.Settings());
                    
                    if (res3.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url1, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url1, res3.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                    checkConnection();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }
        }

        private async void downloadNotifications(Action method)
        {
            bool result = IsConnectedToInternet();
            if (result)
            {
                //create directory
                string dir = localDirectory.BusinessFolder();
                string url = localDirectory.Notifications();

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int errorred = 0;
                try
                {
                    FirebaseResponse res3 = await client.GetAsync(DatabaseDirectory.Notifications());

                    if (res3.Body.ToString() == "null")
                    {
                        System.IO.File.WriteAllText(url, "");
                    }
                    else
                    {
                        System.IO.File.WriteAllText(url, res3.Body.ToString());
                    }
                }
                catch (Exception)
                {
                    errorred++;
                }
                if (errorred == 0)
                {
                    method();
                    checkConnection();
                }
                else
                {
                    checkConnection();
                }
            }
            else
            {
                checkConnection();
            }
        }


        //need to do this asynchronously
        public bool IsConnectedToInternet()
        {
            string host = "google.com";
            bool result = false;
            Ping p = new Ping();
            try
            {
                PingReply reply = p.Send(host, 3000);
                if (reply.Status == IPStatus.Success) return true;
            }
            catch
            {

            }
            return result;
        }

        public void tryAgainSoon(Action<string> method, CounterControl control)
        {
            Thread.Sleep(4000);

            //updateDatabaseData(method, control);
        }

        //lets re-arrange the world
        public async void MasterFunction()
        {
            //In this function we go user by user ensuring that their data is transformed then clearing the old database version
            //a csv of names, number of businesses, if converted to MBS, no. receipts, no. invoices, no. products

            FirebaseResponse res = await client.GetAsync(DatabaseDirectory.Users());
            Dictionary<string, UserClass> users = JsonConvert.DeserializeObject<Dictionary<string, UserClass>>(res.Body.ToString());


            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string complete = System.IO.Path.Combine(systemPath, "WildGrass");
            string dirPdf = complete + @"\WildGrass Master Function" + @"\" + @"Report";
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }
            dirPdf += @"\Users Stats.csv";
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(dirPdf,
                FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine("sep=,");
                    writer.WriteLine("Name, Number Of Businesses, MBS, Receipts, Invoice, Products");
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async void updateDatabaseData(Action<string> method, CounterControl control)
        {
            bool result = IsConnectedToInternet();

            if (result)
            {
                WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
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


                bool success = false;
                bool data = false;
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse res = await client.GetAsync(DatabaseDirectory.Businesses() + "/" + bid + "/MultiBranchSupportYes");
                        if(res.Body != "null")
                        {
                            data = JsonConvert.DeserializeObject<bool>(res.Body.ToString());
                        }
                        success = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        Thread.Sleep(duration);
                    }
                }
                if (success)
                {
                    if (!data)
                    {
                        control.Dispatcher.BeginInvoke(
                            DispatcherPriority.Normal,
                            method, "open");

                        ReSetMyDatabase(method, control);
                    }
                } 
                else
                {
                    tryAgainSoon(method, control);
                }
            }
            else
            {
                tryAgainSoon(method, control);
            }
        }

        private async void ReSetMyDatabase(Action<string> method, CounterControl control)
        {
            int completedTasks = 0;
            //---             Data                     ---//
            //employees
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Employees_old());
                    Dictionary<string, EmployeeClass> data = JsonConvert.DeserializeObject<Dictionary<string, EmployeeClass>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.Employees(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //customers
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Customers_old());
                    Dictionary<string, CustomerClass> data = JsonConvert.DeserializeObject<Dictionary<string, CustomerClass>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.Customers(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            //-- PRODUCTS DATA --//
            //products
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Products_old());
                    Dictionary<string, ProductClass> data = JsonConvert.DeserializeObject<Dictionary<string, ProductClass>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.Products(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //categories
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Categories_old());
                    Dictionary<string, CategoryClass> data = JsonConvert.DeserializeObject<Dictionary<string, CategoryClass>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.Categories(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //quantity change
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.QuantityChange_old());
                    Dictionary<string, DateClass> data = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.QuantityChange(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            //-- TRANSACTION DATA --//
            //receipts
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Receipts_old());
                    Dictionary<string, DateClass> data = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.Receipts(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //all receipts
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.AllReceipts_old());
                    Dictionary<string, SourceDocumentClass2> data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.AllReceipts(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //invoices
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Invoices_old());
                    Dictionary<string, DateClass> data = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.Invoices(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //all invoices
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.AllInvoices_old());
                    Dictionary<string, SourceDocumentClass2> data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.AllInvoices(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //receiptsIn
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.ReceiptsIn_old());
                    Dictionary<string, DateClass> data = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.ReceiptsIn(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //all receiptsIn
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.AllReceiptsIn_old());
                    Dictionary<string, SourceDocumentClass2> data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.AllReceiptsIn(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //invoicesIn
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.InvoicesIn_old());
                    Dictionary<string, DateClass> data = JsonConvert.DeserializeObject<Dictionary<string, DateClass>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.InvoicesIn(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //all invoicesIn
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.AllInvoicesIn_old());
                    Dictionary<string, SourceDocumentClass2> data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.AllInvoicesIn(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //quotations
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Quotations_old());
                    Dictionary<string, SourceDocumentClass2> data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.Quotations(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //credit note
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.CreditNotes_old());
                    Dictionary<string, SourceDocumentClass2> data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.CreditNotes(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //debit note
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.DebitNote_old());
                    Dictionary<string, SourceDocumentClass2> data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.DebitNote(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //orders
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Orders_old());
                    Dictionary<string, SourceDocumentClass2> data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.Orders(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //transfer notes
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.TransferNotes_old());
                    Dictionary<string, SourceDocumentClass2> data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass2>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.TransferNotes(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }


            //settings
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Settings_old());
                    SettingsClass data = JsonConvert.DeserializeObject<SettingsClass>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.Settings(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //notifications
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = await client.GetAsync(DatabaseDirectory.Notifications_old());
                    Dictionary<string, NotificationClass> data = JsonConvert.DeserializeObject<Dictionary<string, NotificationClass>>(res1.Body.ToString());
                    await client.SetAsync(DatabaseDirectory.Notifications(), data);
                    completedTasks++;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }


            bool success = false;
            bool data1 = false;
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res2 = await client.GetAsync(DatabaseDirectory.Businesses() + "/" + bid + "/MultiSDSupportYes");
                    if (res2.Body != "null")
                    {
                        data1 = JsonConvert.DeserializeObject<bool>(res2.Body.ToString());
                    }
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
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        await client.SetAsync(DatabaseDirectory.Businesses() + "/" + bid + "/MultiBranchSupportYes", true);
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }


                if (!data1)
                {
                    SDServicesClass sDServicesClass = new();
                    sDServicesClass.ReshapeDNA(method, control);
                }
                else
                {
                    await control.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    method, "close");
                }
            }
            else
            {
                tryAgainSoon(method, control);
            }
        }

        public void UpdateAllDatasets(Action done)
        {
            DatabaseDirectoryServicesClass sandbox_databaseDirectory = new(uid, bid, "1", true);
            //get all users sandbox
            FirebaseResponse? res1 = null;
            try
            {
                res1 = client.Get(sandbox_databaseDirectory.Users());//this one is the real stress, and will only get worse with time
            }
            catch (Exception)
            {

            }
            if (res1 != null)
            {
                if (res1.Body.ToString() != "null")
                {
                    Dictionary<string, UserClass>? users = JsonConvert.DeserializeObject<Dictionary<string, UserClass>>(res1.Body.ToString());

                    if (users != null) if (users.Count > 0)
                        {
                            foreach (var user in users)
                            {
                                //sandbox business data
                                if (user.Value.BusinessesDetails != null) if (user.Value.BusinessesDetails.Count > 0)
                                    {
                                        foreach (var business in user.Value.BusinessesDetails)
                                        {
                                            callForEach(user.Key, business.Key, false);
                                        }
                                    }
                            }
                        }
                }
            }

            //get all users
            FirebaseResponse? res2 = null;
            try
            {
                res2 = client.Get(DatabaseDirectory.Users());//this one is the real stress, and will only get worse with time
            }
            catch (Exception)
            {

            }
            if (res2 != null)
            {
                if (res2.Body.ToString() != "null")
                {
                    Dictionary<string, UserClass>? users = JsonConvert.DeserializeObject<Dictionary<string, UserClass>>(res2.Body.ToString());

                    if (users != null) if (users.Count > 0)
                        {
                            foreach (var user in users)
                            {
                                //business data
                                if (user.Value.BusinessesDetails != null) if (user.Value.BusinessesDetails.Count > 0)
                                    {
                                        foreach (var business in user.Value.BusinessesDetails)
                                        {
                                            callForEach(user.Key, business.Key, false);
                                        }
                                    }
                            }
                        }
                }
            }

            //client data
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(DatabaseDirectory.ClientSds());
                    Dictionary<string, SourceDocumentClass>? data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass>>(res.Body.ToString());
                    Dictionary<string, SourceDocumentClass2> paste = new();
                    if (data != null) if (data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                SourceDocumentClass2 sd2 = updateSourceDoc(item.Value);
                                paste.Add(item.Key, sd2);
                            }
                        }
                    client.Set(DatabaseDirectory.ClientSds(), paste);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            done();
        }

        SourceDocumentClass2 updateSourceDoc(SourceDocumentClass sd)
        {
            SourceDocumentClass2 sd2 = new()
            {
                sdCode = sd.sdCode,
                sdType = sd.sdType,
                isFsd = sd.isFsd,
                businessName = sd.businessName,
                localArea = sd.localArea,
                addCost = sd.addCost,
                addCostName = sd.addCostName,
                customerEmail = sd.customerEmail,
                customerPhoneNumber = sd.customerPhoneNumber,
                cdate = sd.cdate,
                wildgrassGenerated = sd.wildgrassGenerated,
                date = sd.date,
                day = sd.day,
                discount = sd.discount,
                discountName = sd.discountName,
                exclVat = sd.exclVat,
                vat = sd.vat,
                comment = sd.comment,
                folio = sd.folio,
                inclVat = sd.inclVat,
                month = sd.month,
                pdfLink = sd.pdfLink,
                pdfLink2 = sd.pdfLink,
                onGoing = sd.onGoing,
                products = sd.products,
                tagEid = sd.tagEid,
                tagName = sd.tagName,
                ReasonForReturn = sd.ReasonForReturn,
                ReturnTagEid = sd.ReturnTagEid,
                ReturnTagName = sd.ReturnTagName,
                time = sd.time,
                total = sd.total,
                matchingReceipt = sd.matchingReceipt,
                matchingInvoice = sd.matchingInvoice,
                matchingReturns = sd.matchingReturns,
                type = sd.type,
                valideTillDate = sd.valideTillDate,
                vday = sd.vday,
                vmonth = sd.vmonth,
                vyear = sd.vyear,
                year = sd.year,
                smsYes = sd.smsYes,
                emailYes = sd.emailYes,
                printYes = sd.printYes,
                printerSelected = sd.printerSelected,
                customerBankDetails= sd.customerBankDetails,
                includeCustomerDetails = sd.includeCustomerDetails,
                currency = sd.currency,
                customerBank = sd.customerBank,
                customerAddress = sd.customerAddress,
                customerTpin = sd.customerTpin,
                customerFirstName = sd.customerFirstName,
                customerLastName = sd.customerLastName,
                customerName= sd.customerName,
                customIdYes= sd.customIdYes,
                queuedYes = sd.queuedYes,
                platform = sd.platform,
                rate = sd.rate,
                customId = sd.customId,
                oldCurrency = sd.oldCurrency,
                paymentMethod = sd.paymentMethod,
                matchingQuotation = sd.matchingQuotation,
                matchingDNote = sd.matchingDNote,
                matchingOrder = sd.matchingOrder,
                dirPdf = sd.dirPdf,
                dirPdf2 = sd.dirPdf2,
                customerId = sd.customerId,
                dNoteUrl = sd.dNoteUrl,
                dNoteUrl2 = sd.dirPdf2,
                cid = sd.cid,
                writtenOff = sd.writtenOff,
                archived = sd.archived,
                PaymentPlanYes = sd.PaymentPlanYes,
                PartPaymentYes = sd.PartPaymentYes,
                amountPaid = sd.amountPaid,
                EditedYes = sd.EditedYes,
                EditedDate = sd.EditedDate,
                EditedTime = sd.EditedTime,
                dt = sd.dt,
                summations = sd.summations,
                EditHistory = sd.EditHistory,
                PaymentPlanEntries= sd.PaymentPlanEntries,
                Matching = sd.Matching,
                ext = sd.ext,
            };
            if (sd.sdType == "ReceiptIn" || sd.sdType == "InvoiceIn")
            {
                sd2.includeCustomerDetails = sd.includeCustomerDetails;
                sd2.customerBankDetails = sd.customerBankDetails;
                sd2.customerId = sd.cid;
                sd2.customerFirstName = sd.customerFirstName;
                sd2.customerLastName = sd.customerLastName;
                sd2.customerEmail = sd.customerEmail;
                sd2.customerName = sd.customerName;
                sd2.customerPhoneNumber = sd.customerPhoneNumber;
                sd2.customerAddress = sd.customerAddress;
                sd2.customerTpin = sd.customerTpin;
                sd2.isFsd = true;
            }
            if (sd.paid == "true")
            {
                sd2.paid = true;
            }
            switch (sd.sdType)
            {
                case "Receipt":
                    sd2.sdCode = 0;
                    sd2.paid = true;
                    break;

                case "Invoice":
                    sd2.sdCode = 1;
                    break;

                case "Quotation":
                    sd2.sdCode = 2;
                    break;

                case "Transfer Note":
                    sd2.sdCode = 3;
                    break;

                case "Order":
                    sd2.sdCode = 4;
                    break;

                case "Credit Note":
                    sd2.sdCode = 5;
                    break;

                case "Delivery Note":
                    sd2.sdCode = 6;
                    break;

                case "ReceiptIn":
                    sd2.sdCode = 0;
                    sd2.paid = true;
                    break;

                case "InvoiceIn":
                    sd2.sdCode = 1;
                    break;

                case "TransferNoteIn":
                    sd2.sdCode = 7;
                    break;
            }
            return sd2;
        }

        SourceDocumentClass2 updateSourceDoc(SourceDocumentClass sd, Dictionary<string, DateClass2>? datesArray)
        {
            SourceDocumentClass2 sd2 = updateSourceDoc(sd);

            bool paid = false;
            decimal totalPaid = 0;
            if (sd.sdType == "Invoice" || sd.sdType == "InvoiceIn")
            {
                decimal totalPending = Convert.ToDecimal(sd.total);
                if (sd.Matching != null) if (sd.Matching.Count > 0)
                    {
                        foreach (var matchingEntry in sd.Matching)
                        {
                            if (matchingEntry.Value.sdType == "Receipt")
                            {
                                if (datesArray != null) if (datesArray.ContainsKey(matchingEntry.Value.date))
                                    {
                                        Dictionary<string, SourceDocumentClass> receiptsArray = datesArray[matchingEntry.Value.date].receipts;
                                        if (sd.sdType == "InvoiceIn")
                                        {
                                            receiptsArray = datesArray[matchingEntry.Value.date].sd;
                                        }
                                        if (receiptsArray != null) if (receiptsArray.ContainsKey(matchingEntry.Value.rid))
                                            {
                                                if (receiptsArray[matchingEntry.Value.rid].PartPaymentYes)
                                                {
                                                    decimal tt = Convert.ToDecimal(receiptsArray[matchingEntry.Value.rid].amountPaid);
                                                    totalPaid += tt;
                                                }
                                                else
                                                {
                                                    paid = true;
                                                }
                                            }
                                    }
                                if (totalPending <= totalPaid)
                                {
                                    paid = true;
                                }
                            }
                        }
                    }
            }
            sd2.paid = paid;
            sd2.amountPaid = Double.Parse(totalPaid.ToString());
            return sd2;
        }

        private void callForEach(string uid, string bid, bool isSandbox)
        {
            DatabaseDirectoryServicesClass databaseDirectory = new(uid, bid, "", isSandbox);

            //-- TRANSACTION DATA --//
            Dictionary<string, DateClass2>? receipts = new();
            Dictionary<string, DateClass2>? receiptsIn = new();
            //receipts
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.Receipts());
                    receipts = JsonConvert.DeserializeObject<Dictionary<string, DateClass2>>(res1.Body.ToString());
                    Dictionary<string, DateClass> paste = new();
                    if (receipts != null) if (receipts.Count > 0)
                        {
                            foreach (var date in receipts)
                            {
                                Dictionary<string, SourceDocumentClass2> paste_sds = new();
                                if (date.Value.receipts != null) if (date.Value.receipts.Count > 0)
                                    {
                                        foreach (var item in date.Value.receipts)
                                        {
                                            SourceDocumentClass2 sd2 = updateSourceDoc(item.Value);
                                            paste_sds.Add(item.Key, sd2);
                                        }
                                    }
                                DateClass date2 = new()
                                {
                                    date = date.Value.date,
                                    day = date.Value.day,
                                    comp = date.Value.comp,
                                    month = date.Value.month,
                                    year = date.Value.year,
                                    receipts = paste_sds,
                                };
                                paste.Add(date.Key, date2);
                            }
                        }
                    client.Set(databaseDirectory.Receipts(), paste);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //all receipts
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.AllReceipts());
                    Dictionary<string, SourceDocumentClass>? data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass>>(res1.Body.ToString());
                    Dictionary<string, SourceDocumentClass2> paste_sds = new();
                    if (data != null) if (data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                SourceDocumentClass2 sd2 = updateSourceDoc(item.Value);
                                paste_sds.Add(item.Key, sd2);
                            }
                        }
                    client.Set(databaseDirectory.AllReceipts(), paste_sds);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //receiptsIn
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.ReceiptsIn());
                    receiptsIn = JsonConvert.DeserializeObject<Dictionary<string, DateClass2>>(res1.Body.ToString());
                    Dictionary<string, DateClass> paste = new();
                    if (receiptsIn != null) if (receiptsIn.Count > 0)
                        {
                            foreach (var date in receiptsIn)
                            {
                                Dictionary<string, SourceDocumentClass2> paste_sds = new();
                                if (date.Value.sd != null) if (date.Value.sd.Count > 0)
                                    {
                                        foreach (var item in date.Value.sd)
                                        {
                                            SourceDocumentClass2 sd2 = updateSourceDoc(item.Value);
                                            paste_sds.Add(item.Key, sd2);
                                        }
                                    }
                                DateClass date2 = new()
                                {
                                    date = date.Value.date,
                                    day = date.Value.day,
                                    comp = date.Value.comp,
                                    month = date.Value.month,
                                    year = date.Value.year,
                                    sd = paste_sds,
                                };
                                paste.Add(date.Key, date2);
                            }
                        }
                    client.Set(databaseDirectory.ReceiptsIn(), paste);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //all receiptsIn
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.AllReceiptsIn());
                    Dictionary<string, SourceDocumentClass>? data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass>>(res1.Body.ToString());
                    Dictionary<string, SourceDocumentClass2> paste_sds = new();
                    if (data != null) if (data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                SourceDocumentClass2 sd2 = updateSourceDoc(item.Value);
                                paste_sds.Add(item.Key, sd2);
                            }
                        }
                    client.Set(databaseDirectory.AllReceiptsIn(), paste_sds);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            //invoices
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.Invoices());
                    Dictionary<string, DateClass2>? data = JsonConvert.DeserializeObject<Dictionary<string, DateClass2>>(res1.Body.ToString());
                    Dictionary<string, DateClass> paste = new();
                    if (data != null) if (data.Count > 0)
                        {
                            foreach (var date in data)
                            {
                                Dictionary<string, SourceDocumentClass2> paste_sds = new();
                                if (date.Value.invoices != null) if (date.Value.invoices.Count > 0)
                                    {
                                        foreach (var item in date.Value.invoices)
                                        {
                                            SourceDocumentClass2 sd2 = updateSourceDoc(item.Value, receipts);
                                            paste_sds.Add(item.Key, sd2);
                                        }
                                    }
                                DateClass date2 = new()
                                {
                                    date = date.Value.date,
                                    day = date.Value.day,
                                    comp = date.Value.comp,
                                    month = date.Value.month,
                                    year = date.Value.year,
                                    invoices = paste_sds,
                                };
                                paste.Add(date.Key, date2);
                            }
                        }
                    client.Set(databaseDirectory.Invoices(), paste);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //all invoices
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.AllInvoices());
                    Dictionary<string, SourceDocumentClass>? data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass>>(res1.Body.ToString());
                    Dictionary<string, SourceDocumentClass2> paste_sds = new();
                    if (data != null) if (data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                SourceDocumentClass2 sd2 = updateSourceDoc(item.Value, receipts);
                                paste_sds.Add(item.Key, sd2);
                            }
                        }
                    client.Set(databaseDirectory.AllInvoices(), paste_sds);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //invoicesIn
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.InvoicesIn());
                    Dictionary<string, DateClass2>? data = JsonConvert.DeserializeObject<Dictionary<string, DateClass2>>(res1.Body.ToString());
                    Dictionary<string, DateClass> paste = new();
                    if (data != null) if (data.Count > 0)
                        {
                            foreach (var date in data)
                            {
                                Dictionary<string, SourceDocumentClass2> paste_sds = new();
                                if (date.Value.sd != null) if (date.Value.sd.Count > 0)
                                    {
                                        foreach (var item in date.Value.sd)
                                        {
                                            SourceDocumentClass2 sd2 = updateSourceDoc(item.Value, receiptsIn);
                                            paste_sds.Add(item.Key, sd2);
                                        }
                                    }
                                DateClass date2 = new()
                                {
                                    date = date.Value.date,
                                    day = date.Value.day,
                                    comp = date.Value.comp,
                                    month = date.Value.month,
                                    year = date.Value.year,
                                    sd = paste_sds,
                                };
                                paste.Add(date.Key, date2);
                            }
                        }
                    client.Set(databaseDirectory.InvoicesIn(), paste);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //all invoicesIn
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.AllInvoicesIn());
                    Dictionary<string, SourceDocumentClass>? data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass>>(res1.Body.ToString());
                    Dictionary<string, SourceDocumentClass2> paste_sds = new();
                    if (data != null) if (data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                SourceDocumentClass2 sd2 = updateSourceDoc(item.Value, receiptsIn);
                                paste_sds.Add(item.Key, sd2);
                            }
                        }
                    client.Set(databaseDirectory.AllInvoicesIn(), paste_sds);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //quotations
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.Quotations());
                    Dictionary<string, SourceDocumentClass>? data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass>>(res1.Body.ToString());
                    Dictionary<string, SourceDocumentClass2> paste_sds = new();
                    if (data != null) if (data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                SourceDocumentClass2 sd2 = updateSourceDoc(item.Value);
                                paste_sds.Add(item.Key, sd2);
                            }
                        }
                    client.Set(databaseDirectory.Quotations(), paste_sds);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //credit note
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.CreditNotes());
                    Dictionary<string, SourceDocumentClass>? data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass>>(res1.Body.ToString());
                    Dictionary<string, SourceDocumentClass2> paste_sds = new();
                    if (data != null) if (data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                SourceDocumentClass2 sd2 = updateSourceDoc(item.Value);
                                paste_sds.Add(item.Key, sd2);
                            }
                        }
                    client.Set(databaseDirectory.CreditNotes(), paste_sds);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //debit note
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.DebitNote());
                    Dictionary<string, SourceDocumentClass>? data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass>>(res1.Body.ToString());
                    Dictionary<string, SourceDocumentClass2> paste_sds = new();
                    if (data != null) if (data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                SourceDocumentClass2 sd2 = updateSourceDoc(item.Value);
                                paste_sds.Add(item.Key, sd2);
                            }
                        }
                    client.Set(databaseDirectory.DebitNote(), paste_sds);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //orders
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.Orders());
                    Dictionary<string, SourceDocumentClass>? data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass>>(res1.Body.ToString());
                    Dictionary<string, SourceDocumentClass2> paste_sds = new();
                    if (data != null) if (data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                SourceDocumentClass2 sd2 = updateSourceDoc(item.Value);
                                paste_sds.Add(item.Key, sd2);
                            }
                        }
                    client.Set(databaseDirectory.Orders(), paste_sds);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            //transfer notes
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res1 = client.Get(databaseDirectory.TransferNotes());
                    Dictionary<string, SourceDocumentClass>? data = JsonConvert.DeserializeObject<Dictionary<string, SourceDocumentClass>>(res1.Body.ToString());
                    Dictionary<string, SourceDocumentClass2> paste_sds = new();
                    if (data != null) if (data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                SourceDocumentClass2 sd2 = updateSourceDoc(item.Value);
                                paste_sds.Add(item.Key, sd2);
                            }
                        }
                    client.Set(databaseDirectory.TransferNotes(), paste_sds);
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
