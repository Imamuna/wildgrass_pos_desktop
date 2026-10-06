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
    /// Updates to QC, PC, Tracking and stock changes;
    /// stock entry, generating transactions, entering fsd;
    /// </summary>
    internal class ProductProcessingServicesClass
    {
        //ID's
        string uid = "";
        string bid = "";
        string eid = "";


        int duration = 2000;
        int numberOfTries = 3;

        DatabaseDirectoryServicesClass DatabaseDirectory = new();
        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
        ProductServicesClass productServices = new();
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

        public void updateQuantity(SourceDocumentClass2 sourceDocument)
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            Dictionary<string, ProductClass> productsArray = prevelantClass.LoadDataProducts();

            if (sourceDocument.products != null)
            {
                if (sourceDocument.products.Count != 0)
                {
                    //updates product details
                    foreach (var product in sourceDocument.products)
                    {
                        if (!productsArray.ContainsKey(product.Key)) break;
                        if (productsArray[product.Key].inventoryTracking != "on") break;

                        if (productsArray[product.Key].serialNumberYes)
                        {
                            if (product.Value.serialNumbers == null) break;
                            if (product.Value.serialNumbers.Count == 0) break;
                            foreach (var item in product.Value.serialNumbers)
                            {
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        FirebaseResponse firebase = client.Delete(DatabaseDirectory.Products() + "/" + product.Key + "/serialNumbers/" + item.Key);
                                        productsArray[product.Key].serialNumbers.Remove(item.Key);
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(duration);
                                    }
                                }
                            }
                            int qty = productsArray[product.Key].serialNumbers.Count;
                            for (int i = 0; i < numberOfTries; i++)
                            {
                                try
                                {
                                    FirebaseResponse firebase789 = client.Set(DatabaseDirectory.Products() + "/" + product.Key + "/qtyInStock", Convert.ToString(qty));
                                    break;
                                }
                                catch (Exception)
                                {
                                    Thread.Sleep(duration);
                                }
                            }
                        }
                        else if (productsArray[product.Key].expiryDateTracking)
                        {
                            if (product.Value.expiryDateBatches == null) break;
                            if (product.Value.expiryDateBatches.Count == 0) break;
                            foreach (var item in product.Value.expiryDateBatches)
                            {
                                int reduceBy = item.Value.QtyInStock;//from cart
                                int qtyInStock = productsArray[product.Key].expiryDateBatches[item.Key].QtyInStock;//from inventory records
                                int quantity = qtyInStock - reduceBy;//subtract cart products from inventory records
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Products() + "/" + product.Key + "/expiryDateBatches/" + item.Key + "/QtyInStock", quantity);
                                        productsArray[product.Key].expiryDateBatches[item.Key].QtyInStock = quantity;
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(duration);
                                    }
                                }
                            }
                            int total = 0;
                            foreach (var item in productsArray[product.Key].expiryDateBatches)
                            {
                                total += item.Value.QtyInStock;
                            }
                            for (int i = 0; i < numberOfTries; i++)
                            {
                                try
                                {
                                    FirebaseResponse firebase789 = client.Set(DatabaseDirectory.Products() + "/" + product.Key + "/qtyInStock", Convert.ToString(total));
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
                            double qtyInStock = Convert.ToDouble(productsArray[product.Key].qtyInStock);
                            double reduceBy = Convert.ToDouble(product.Value.quantity);
                            double quantity = qtyInStock - reduceBy;

                            string qtyInStockaaa = Convert.ToString(quantity);
                            productsArray[product.Key].qtyInStock = qtyInStockaaa;
                            for (int i = 0; i < numberOfTries; i++)
                            {
                                try
                                {
                                    FirebaseResponse firebase789 = client.Set(DatabaseDirectory.Products() + "/" + product.Key + "/qtyInStock", qtyInStockaaa);
                                    break;
                                }
                                catch (Exception)
                                {
                                    Thread.Sleep(duration);
                                }
                            }
                        }
                    }
                    standardFirebaseOperationsClass.UpdateVersion("product");
                }
            }
            updateStockTrackingAsync(sourceDocument.products, sourceDocument, productsArray);
        }

        public async Task<SDChecklistClass> updateQuantity(SourceDocumentClass2 sourceDocument, SDChecklistClass checklistClass)
        {
            if (checklistClass.productQuantity_completed == null) checklistClass.productQuantity_completed = new();
            Dictionary<string, string> completed = checklistClass.productQuantity_completed;
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            Dictionary<string, ProductClass> productsArray = prevelantClass.LoadDataProducts();

            if (sourceDocument.products != null)
            {
                if (sourceDocument.products.Count != 0)
                {
                    //updates product details
                    foreach (var product in sourceDocument.products)
                    {
                        if (!completed.ContainsKey(product.Key))
                        {
                            if (productsArray.ContainsKey(product.Key)) if (productsArray[product.Key].inventoryTracking == "on")
                                {
                                    if (productsArray[product.Key].serialNumberYes)
                                    {
                                        if (product.Value.serialNumbers == null) break;
                                        if (product.Value.serialNumbers.Count == 0) break;
                                        foreach (var item in product.Value.serialNumbers)
                                        {
                                            for (int i = 0; i < numberOfTries; i++)
                                            {
                                                try
                                                {
                                                    FirebaseResponse firebase = client.Delete(DatabaseDirectory.Products() + "/" + product.Key + "/serialNumbers/" + item.Key);
                                                    productsArray[product.Key].serialNumbers.Remove(item.Key);
                                                    break;
                                                }
                                                catch (Exception)
                                                {
                                                    Thread.Sleep(duration);
                                                }
                                            }
                                        }
                                        int qty = productsArray[product.Key].serialNumbers.Count;
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                FirebaseResponse firebase789 = client.Set(DatabaseDirectory.Products() + "/" + product.Key + "/qtyInStock", Convert.ToString(qty));
                                                if (!completed.ContainsKey(product.Key))
                                                {
                                                    completed.Add(product.Key, product.Key);
                                                }
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                    }
                                    else if (productsArray[product.Key].expiryDateTracking)
                                    {
                                        if (product.Value.expiryDateBatches == null) break;
                                        if (product.Value.expiryDateBatches.Count == 0) break;
                                        foreach (var item in product.Value.expiryDateBatches)
                                        {
                                            int reduceBy = item.Value.QtyInStock;//from cart
                                            int qtyInStock = productsArray[product.Key].expiryDateBatches[item.Key].QtyInStock;//from inventory records
                                            int quantity = qtyInStock - reduceBy;//subtract cart products from inventory records
                                            for (int i = 0; i < numberOfTries; i++)
                                            {
                                                try
                                                {
                                                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Products() + "/" + product.Key + "/expiryDateBatches/" + item.Key + "/QtyInStock", quantity);
                                                    productsArray[product.Key].expiryDateBatches[item.Key].QtyInStock = quantity;
                                                    break;
                                                }
                                                catch (Exception)
                                                {
                                                    Thread.Sleep(duration);
                                                }
                                            }
                                        }
                                        int total = 0;
                                        foreach (var item in productsArray[product.Key].expiryDateBatches)
                                        {
                                            total += item.Value.QtyInStock;
                                        }
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                FirebaseResponse firebase789 = client.Set(DatabaseDirectory.Products() + "/" + product.Key + "/qtyInStock", Convert.ToString(total));
                                                if (!completed.ContainsKey(product.Key))
                                                {
                                                    completed.Add(product.Key, product.Key);
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
                                        double qtyInStock = Convert.ToDouble(productsArray[product.Key].qtyInStock);
                                        double reduceBy = Convert.ToDouble(product.Value.quantity);
                                        double quantity = qtyInStock - reduceBy;

                                        string qtyInStockaaa = Convert.ToString(quantity);
                                        productsArray[product.Key].qtyInStock = qtyInStockaaa;
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                FirebaseResponse firebase789 = client.Set(DatabaseDirectory.Products() + "/" + product.Key + "/qtyInStock", qtyInStockaaa);
                                                if (!completed.ContainsKey(product.Key))
                                                {
                                                    completed.Add(product.Key, product.Key);
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
                    }
                    standardFirebaseOperationsClass.UpdateVersion("product");
                }
            }
            checklistClass.productQuantity_completed = completed;
            checklistClass.productQC_completed = await updateStockTrackingAsync(sourceDocument, productsArray, checklistClass);
            return checklistClass;
        }

        public async Task<Dictionary<string, string>> updateStockTrackingAsync(SourceDocumentClass2 receipt, Dictionary<string, ProductClass> localProductsArray, SDChecklistClass checklistClass)
        {
            if (checklistClass.productQC_completed == null) checklistClass.productQC_completed = new();
            Dictionary<string, string> completed = checklistClass.productQC_completed;
            Dictionary<string, ProductEntrySDClass> upGoingArray = receipt.products;

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            setup();
            //never gets called if its a quotation
            //we start by retrieving the current version of quantity change
            var auth = "btiSgvaHMXQWODLMFQSgFBHLJuBykOfzwu9xbfdV"; // your app secret
            var firebaseClient = new FirebaseClient(
              "https://long-walk-pos.firebaseio.com/",
              new FirebaseOptions
              {
                  AuthTokenAsyncFactory = () => Task.FromResult(auth)
              });
            Dictionary<string, StockReconcilliationClass> stockArray = new();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    var Entries = await firebaseClient
                              .Child("Users")
                              .Child(uid)
                              .Child("businesses")
                              .Child(bid)
                              .Child("QuantityChange")
                              .Child(receipt.date)//need current day
                              .Child("stock")
                              .OnceAsync<StockReconcilliationClass>();

                    if (Entries != null)
                    {
                        if (Entries.Count != 0)
                        {
                            foreach (var item in Entries)
                            {
                                StockReconcilliationClass stock = new StockReconcilliationClass
                                {
                                    details = item.Object.details,
                                    starting = item.Object.starting,
                                    manual = item.Object.manual,
                                    pid = item.Object.pid,
                                    sales = item.Object.sales,
                                    ending = item.Object.ending
                                };

                                stockArray.Add(item.Key, stock);
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

            DateClass dateEntry = new DateClass
            {
                comp = receipt.month + receipt.year,
                date = receipt.date,
                day = receipt.day,
                month = receipt.month,
                year = receipt.year
            };

            int qcCreated_count = 0;

            //QC update
            if (upGoingArray != null)
            {
                if (upGoingArray.Count != 0)
                {
                    foreach (var entry in upGoingArray.ToList())
                    {
                        if (!completed.ContainsKey(entry.Key))
                        {
                            if (entry.Value.brandName != "Custom Entry") // ensuring we skip custom entries
                            {
                                qcCreated_count++;

                                QuantityChangeClass quantityChangeEntry = new QuantityChangeClass();
                                StockReconcilliationClass stockEntry = new StockReconcilliationClass();
                                string pid = entry.Key;
                                int iti = 0;


                                if (stockArray != null)
                                {
                                    if (stockArray.Count != 0)
                                    {
                                        foreach (var item in stockArray)
                                        {
                                            if (item.Key == pid)
                                            {
                                                iti++;
                                                stockEntry = item.Value;
                                            }
                                        }
                                    }
                                }

                                if (iti == 0)
                                {
                                    double ending, starting, change;
                                    change = Convert.ToDouble(entry.Value.quantity);
                                    if (!double.TryParse(localProductsArray[pid].qtyInStock, out starting))
                                    {
                                        starting = 0;
                                    }
                                    ending = starting - change;

                                    //add it to the Entires array
                                    Dictionary<string, QuantityChangeClass> QCs = new Dictionary<string, QuantityChangeClass>();
                                    quantityChangeEntry.qcid = receipt.folio;
                                    quantityChangeEntry.date = receipt.date;
                                    quantityChangeEntry.time = receipt.time;
                                    quantityChangeEntry.day = receipt.day;
                                    quantityChangeEntry.month = receipt.month;
                                    quantityChangeEntry.year = receipt.year;
                                    quantityChangeEntry.employee = receipt.tagName;
                                    quantityChangeEntry.eid = receipt.tagEid;
                                    quantityChangeEntry.details = "Tracking";
                                    quantityChangeEntry.newQty = Convert.ToString(ending);
                                    quantityChangeEntry.oldQty = localProductsArray[pid].qtyInStock;
                                    QCs.Add(quantityChangeEntry.qcid, quantityChangeEntry);

                                    //create new entry
                                    stockEntry = new StockReconcilliationClass
                                    {
                                        details = entry.Value.brandName + " " + entry.Value.productName + " " + entry.Value.flavor + " " + entry.Value.size,
                                        starting = localProductsArray[pid].qtyInStock,
                                        manual = "",
                                        pid = pid,
                                        sales = entry.Value.quantity,
                                        ending = Convert.ToString(ending),
                                        QCs = QCs
                                    };
                                }
                                else
                                {
                                    //update Entries array
                                    double oldSales, increase, newSales;
                                    increase = Convert.ToDouble(entry.Value.quantity);
                                    oldSales = 0;
                                    try
                                    {
                                        oldSales = Convert.ToDouble(stockEntry.sales);
                                    }
                                    catch (Exception)
                                    { }
                                    newSales = oldSales + increase;

                                    stockEntry.sales = Convert.ToString(newSales);
                                    if (stockEntry.details == null) stockEntry.details = entry.Value.brandName + " " + entry.Value.productName + " " + entry.Value.flavor + " " + entry.Value.size;
                                    if (stockEntry.starting == null) stockEntry.starting = localProductsArray[pid].qtyInStock;
                                    if (stockEntry.manual == null) stockEntry.manual = "";
                                    double ending, current, change;
                                    change = Convert.ToDouble(entry.Value.quantity);
                                    current = Convert.ToDouble(stockEntry.ending);
                                    ending = current - change;

                                    stockEntry.ending = Convert.ToString(ending);

                                    quantityChangeEntry.qcid = receipt.folio;
                                    quantityChangeEntry.date = receipt.date;
                                    quantityChangeEntry.time = receipt.time;
                                    quantityChangeEntry.day = receipt.day;
                                    quantityChangeEntry.month = receipt.month;
                                    quantityChangeEntry.year = receipt.year;
                                    quantityChangeEntry.employee = receipt.tagName;
                                    quantityChangeEntry.eid = receipt.tagEid;
                                    quantityChangeEntry.details = "Tracking";
                                    quantityChangeEntry.newQty = stockEntry.ending;
                                    quantityChangeEntry.oldQty = Convert.ToString(current);

                                    //stockEntry.QCs.Add(quantityChangeEntry.qcid, quantityChangeEntry);

                                }

                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        //here we set the QC and the stock entry data for that product
                                        FirebaseResponse firebasea1 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/QCs/" + receipt.folio, quantityChangeEntry);
                                        FirebaseResponse firebasea11 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/details/", stockEntry.details);
                                        FirebaseResponse firebasea12 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/ending/", stockEntry.ending);
                                        FirebaseResponse firebasea13 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/manual/", stockEntry.manual);
                                        FirebaseResponse firebasea14 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/pid/", pid);
                                        FirebaseResponse firebasea15 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/sales/", stockEntry.sales);
                                        FirebaseResponse firebasea16 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/starting/", stockEntry.starting);

                                        if (!completed.ContainsKey(entry.Key))
                                        {
                                            completed.Add(entry.Key, entry.Key);
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
                    
                    if(qcCreated_count > 0)
                    {
                        for (int i = 0; i < numberOfTries; i++)
                        {
                            try
                            {
                                FirebaseResponse firebase2 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/date", dateEntry.date);
                                FirebaseResponse firebase3 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/comp", dateEntry.comp);
                                FirebaseResponse firebase4 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/day", dateEntry.day);
                                FirebaseResponse firebase5 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/month", dateEntry.month);
                                FirebaseResponse firebase6 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/year", dateEntry.year);
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

            //UPDATE VERSION
            standardFirebaseOperationsClass.UpdateVersion("tracking");
            //Notification
            standardFirebaseOperationsClass.createProductNotification(upGoingArray);

            return completed;
        }

        public async void updateStockTrackingAsync(Dictionary<string, ProductEntrySDClass> upGoingArray, SourceDocumentClass2 receipt, Dictionary<string, ProductClass> localProductsArray)
        {
            Dictionary<string, string> completed = new();


            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            setup();
            //never gets called if its a quotation
            //we start by retrieving the current version of quantity change
            var auth = "btiSgvaHMXQWODLMFQSgFBHLJuBykOfzwu9xbfdV"; // your app secret
            var firebaseClient = new FirebaseClient(
              "https://long-walk-pos.firebaseio.com/",
              new FirebaseOptions
              {
                  AuthTokenAsyncFactory = () => Task.FromResult(auth)
              });
            Dictionary<string, StockReconcilliationClass> stockArray = new();


            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    var Entries = await firebaseClient
                              .Child("Users")
                              .Child(uid)
                              .Child("businesses")
                              .Child(bid)
                              .Child("QuantityChange")
                              .Child(receipt.date)//need current day
                              .Child("stock")
                              .OnceAsync<StockReconcilliationClass>();

                    if (Entries != null)
                    {
                        if (Entries.Count != 0)
                        {
                            foreach (var item in Entries)
                            {
                                StockReconcilliationClass stock = new StockReconcilliationClass
                                {
                                    details = item.Object.details,
                                    starting = item.Object.starting,
                                    manual = item.Object.manual,
                                    pid = item.Object.pid,
                                    sales = item.Object.sales,
                                    ending = item.Object.ending
                                };

                                stockArray.Add(item.Key, stock);
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

            DateClass dateEntry = new DateClass
            {
                comp = receipt.month + receipt.year,
                date = receipt.date,
                day = receipt.day,
                month = receipt.month,
                year = receipt.year
            };

            //QC update
            if (upGoingArray != null)
            {
                if (upGoingArray.Count != 0)
                {
                    foreach (var entry in upGoingArray.ToList())
                    {
                        if (entry.Value.brandName != "Custom Entry") // ensuring we skip custom entries
                        {
                            QuantityChangeClass quantityChangeEntry = new QuantityChangeClass();
                            StockReconcilliationClass stockEntry = new StockReconcilliationClass();
                            string pid = entry.Key;
                            int iti = 0;


                            if (stockArray != null)
                            {
                                if (stockArray.Count != 0)
                                {
                                    foreach (var item in stockArray)
                                    {
                                        if (item.Key == pid)
                                        {
                                            iti++;
                                            stockEntry = item.Value;
                                        }
                                    }
                                }
                            }

                            if (iti == 0)
                            {
                                double ending, starting, change;
                                change = Convert.ToDouble(entry.Value.quantity);
                                if (!double.TryParse(localProductsArray[pid].qtyInStock, out starting))
                                {
                                    starting = 0;
                                }
                                ending = starting - change;

                                //add it to the Entires array
                                Dictionary<string, QuantityChangeClass> QCs = new Dictionary<string, QuantityChangeClass>();
                                quantityChangeEntry.qcid = receipt.folio;
                                quantityChangeEntry.date = receipt.date;
                                quantityChangeEntry.time = receipt.time;
                                quantityChangeEntry.day = receipt.day;
                                quantityChangeEntry.month = receipt.month;
                                quantityChangeEntry.year = receipt.year;
                                quantityChangeEntry.employee = receipt.tagName;
                                quantityChangeEntry.eid = receipt.tagEid;
                                quantityChangeEntry.details = "Tracking";
                                quantityChangeEntry.newQty = Convert.ToString(ending);
                                quantityChangeEntry.oldQty = localProductsArray[pid].qtyInStock;
                                QCs.Add(quantityChangeEntry.qcid, quantityChangeEntry);

                                //create new entry
                                stockEntry = new StockReconcilliationClass
                                {
                                    details = entry.Value.brandName + " " + entry.Value.productName + " " + entry.Value.flavor + " " + entry.Value.size,
                                    starting = localProductsArray[pid].qtyInStock,
                                    manual = "",
                                    pid = pid,
                                    sales = entry.Value.quantity,
                                    ending = Convert.ToString(ending),
                                    QCs = QCs
                                };
                            }
                            else
                            {
                                //update Entries array
                                double oldSales, increase, newSales;
                                increase = Convert.ToDouble(entry.Value.quantity);
                                oldSales = 0;
                                try
                                {
                                    oldSales = Convert.ToDouble(stockEntry.sales);
                                }
                                catch (Exception)
                                { }
                                newSales = oldSales + increase;

                                stockEntry.sales = Convert.ToString(newSales);
                                if (stockEntry.details == null) stockEntry.details = entry.Value.brandName + " " + entry.Value.productName + " " + entry.Value.flavor + " " + entry.Value.size;
                                if (stockEntry.starting == null) stockEntry.starting = localProductsArray[pid].qtyInStock;
                                if (stockEntry.manual == null) stockEntry.manual = "";
                                double ending, current, change;
                                change = Convert.ToDouble(entry.Value.quantity);
                                current = Convert.ToDouble(stockEntry.ending);
                                ending = current - change;

                                stockEntry.ending = Convert.ToString(ending);

                                quantityChangeEntry.qcid = receipt.folio;
                                quantityChangeEntry.date = receipt.date;
                                quantityChangeEntry.time = receipt.time;
                                quantityChangeEntry.day = receipt.day;
                                quantityChangeEntry.month = receipt.month;
                                quantityChangeEntry.year = receipt.year;
                                quantityChangeEntry.employee = receipt.tagName;
                                quantityChangeEntry.eid = receipt.tagEid;
                                quantityChangeEntry.details = "Tracking";
                                quantityChangeEntry.newQty = stockEntry.ending;
                                quantityChangeEntry.oldQty = Convert.ToString(current);

                                //stockEntry.QCs.Add(quantityChangeEntry.qcid, quantityChangeEntry);

                            }

                            for (int i = 0; i < numberOfTries; i++)
                            {
                                try
                                {
                                    //here we set the QC and the stock entry data for that product
                                    FirebaseResponse firebasea1 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/QCs/" + receipt.folio, quantityChangeEntry);
                                    FirebaseResponse firebasea11 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/details/", stockEntry.details);
                                    FirebaseResponse firebasea12 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/ending/", stockEntry.ending);
                                    FirebaseResponse firebasea13 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/manual/", stockEntry.manual);
                                    FirebaseResponse firebasea14 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/pid/", pid);
                                    FirebaseResponse firebasea15 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/sales/", stockEntry.sales);
                                    FirebaseResponse firebasea16 = client.Set(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/stock/" + pid + "/starting/", stockEntry.starting);
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
                            FirebaseResponse firebase2 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/date", dateEntry.date);
                            FirebaseResponse firebase3 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/comp", dateEntry.comp);
                            FirebaseResponse firebase4 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/day", dateEntry.day);
                            FirebaseResponse firebase5 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/month", dateEntry.month);
                            FirebaseResponse firebase6 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + receipt.date + "/year", dateEntry.year);
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                }
            }

            //UPDATE VERSION
            standardFirebaseOperationsClass.UpdateVersion("tracking");
            //Notification
            standardFirebaseOperationsClass.createProductNotification(upGoingArray);
        }

        public async Task<QueuedSdDataClass> SDProductUpdate(QueuedSdDataClass queuedSd)
        {
            if (queuedSd.Sd.isFsd)
            {
                queuedSd.checklist = await updateQuantityEntry(queuedSd.Sd.products, true, queuedSd.Sd.folio, queuedSd.checklist);
            } else
            {
                queuedSd.checklist = await updateQuantity(queuedSd.Sd, queuedSd.checklist);
            }

            return queuedSd;
        }

        public async Task<SDChecklistClass> updateQuantityEntry(Dictionary<string, ProductEntrySDClass> products, bool purchases, string folio, SDChecklistClass checklistClass)
        {
            if (checklistClass.productPC_completed == null) checklistClass.productPC_completed = new();
            Dictionary<string, string> completed_price = checklistClass.productPC_completed;
            Dictionary<string, string> completed_quantity = checklistClass.productPC_completed;
            Dictionary<string, string> completed_qc = checklistClass.productQuantity_completed;


            Dictionary<string, ProductClass> productArray = prevelantClass.LoadDataProducts();
            if (products != null) if (products.Count > 0)
                {
                    foreach (var item in products)
                    {
                        //stuff to do with products
                        if (productArray.ContainsKey(item.Value.productID))
                        {
                            ProductEntrySDClass sdProduct = item.Value;
                            ProductClass product = productArray[sdProduct.productID];
                            int inventoryType = 0;
                            int oldQty = 0;
                            int change = 0;
                            try { oldQty = Convert.ToInt32(product.qtyInStock); } catch (Exception) { }
                            try { change = Convert.ToInt32(sdProduct.quantity); } catch (Exception) { }
                            int newQty = oldQty + change;
                            if (product.serialNumberYes)
                            {
                                inventoryType = 1;
                            }
                            else if (product.expiryDateTracking)
                            {
                                inventoryType = 2;
                            }

                            //update price change
                            if (!completed_price.ContainsKey(item.Key))
                            {
                                if (purchases)
                                {
                                    bool success = purchasePriceUpdate(item.Value.productID, sdProduct.price, folio);
                                    if (success)
                                    {
                                        if (!completed_price.ContainsKey(item.Key))
                                        {
                                            completed_price.Add(item.Key, item.Key);
                                        }
                                    }
                                }
                            }

                            //update quantity
                            if (!completed_quantity.ContainsKey(item.Key))
                            {
                                bool success = await StockEntry2(sdProduct.productID, inventoryType, Convert.ToString(newQty), change, sdProduct.serialNumbers, sdProduct.expiryDateBatches, product);
                                if (success)
                                {
                                    if (!completed_quantity.ContainsKey(item.Key))
                                    {
                                        completed_quantity.Add(item.Key, item.Key);
                                    }
                                }
                            }

                            //update quantity change
                            if (!completed_qc.ContainsKey(item.Key))
                            {
                                bool success = await qcManualUpdate(sdProduct.productID, change);
                                if (success)
                                {
                                    if (!completed_qc.ContainsKey(item.Key))
                                    {
                                        completed_qc.Add(item.Key, item.Key);
                                    }
                                }
                            }
                        }
                    }
                }
            checklistClass.productPC_completed = completed_price;
            checklistClass.productQuantity_completed = completed_quantity;
            checklistClass.productQC_completed = completed_qc;

            return checklistClass;
        }
        public async Task<bool> StockEntry2(string pid, int inventoryType, string newQty, int quantityChange, Dictionary<string, SerialNumberClass> serialNumbers, Dictionary<string, ExpiryDateBatchClass> expiryDateBatches, ProductClass localData)
        {
            bool success = false;
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();


            switch (inventoryType)
            {
                case 1: //serial
                    if (serialNumbers != null) if (serialNumbers.Count > 0)
                        {
                            foreach (var item in serialNumbers)
                            {
                                bool exists = false;
                                if (localData != null) if (localData.serialNumbers != null) if (localData.serialNumbers.ContainsKey(item.Value.serialNumber))
                                        {
                                            exists = true;
                                        }

                                if (exists)
                                {
                                    //if it exists don't update, its already there
                                    int changeQty = Convert.ToInt32(newQty) - 1;
                                    if (changeQty < 0) changeQty = 0;
                                    newQty = Convert.ToString(changeQty);
                                }
                                else
                                {
                                    //business as usual
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            FirebaseResponse firebase4 = client.Set(@"Users/" + uid + "/businesses/" + bid + "/products/products/" + pid + "/serialNumbers/" + item.Value.serialNumber, item.Value);
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
                        }
                    break;
                case 2: //expiry
                    if (expiryDateBatches != null) if (expiryDateBatches.Count > 0)
                        {
                            foreach (var item in expiryDateBatches)
                            {
                                bool exists = false;
                                if (localData != null) if (localData.expiryDateBatches != null) if (localData.expiryDateBatches.ContainsKey(item.Key))
                                        {
                                            exists = true;
                                            int newValue = localData.expiryDateBatches[item.Key].QtyInStock + item.Value.QtyInStock;
                                            if (newValue > 0)
                                            {
                                                for (int i = 0; i < numberOfTries; i++)
                                                {
                                                    try
                                                    {
                                                        FirebaseResponse firebase4 = client.Set(@"Users/" + uid + "/businesses/" + bid + "/products/products/" + pid + "/expiryDateBatches/" + item.Value.date + "/QtyInStock", newValue);
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
                                if (!exists)
                                {
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            FirebaseResponse firebase4 = client.Set(@"Users/" + uid + "/businesses/" + bid + "/products/products/" + pid + "/expiryDateBatches/" + item.Value.date, item.Value);
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
                        }
                    break;
            }
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse firebase6 = client.Set(DatabaseDirectory.Products() + "/" + pid + "/qtyInStock", newQty);

                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            standardFirebaseOperationsClass.UpdateVersion("product");
            //update activity log
            standardFirebaseOperationsClass.activityLog(pid, "product", "Manual Quantity Change", eid);

            return success;
        }

        public async Task<bool> qcManualUpdate(string pid, double change)
        {
            setup();

            bool success = false;

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            UsersClass user = prevelantClass.GetCurrentUser();
            string employee = user.userName;
            if (eid == "")
            {
                eid = "1";
            }

            DateTime dt = DateTime.Now;
            string date = dt.ToString("MMM, dd yyyy");
            string time = dt.ToString("HH:mm:ss tt");
            double timeId1 = dt.TimeOfDay.TotalMilliseconds;
            int timeId = Convert.ToInt32(timeId1);
            string day = Convert.ToString(dt.Day);
            int mm = dt.Month;
            string month = Convert.ToString(mm);
            string year = Convert.ToString(dt.Year);

            string rid = "WG" + year + month + day + Convert.ToString(timeId);

            //we are loading today's stock data from firebase
            var auth = "btiSgvaHMXQWODLMFQSgFBHLJuBykOfzwu9xbfdV"; // your app secret
            var firebaseClient = new FirebaseClient(
              "https://long-walk-pos.firebaseio.com/",
              new FirebaseOptions
              {
                  AuthTokenAsyncFactory = () => Task.FromResult(auth)
              });
            Dictionary<string, StockReconcilliationClass> stockArray = new();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    var Entries = await firebaseClient
                              .Child("Users")
                              .Child(uid)
                              .Child("businesses")
                              .Child(bid)
                              .Child("QuantityChange")
                              .Child(date)//need current day
                              .Child("stock")
                              .OnceAsync<StockReconcilliationClass>();

                    if (Entries != null)
                    {
                        if (Entries.Count != 0)
                        {
                            foreach (var item in Entries)
                            {
                                StockReconcilliationClass stock = new StockReconcilliationClass
                                {
                                    details = item.Object.details,
                                    starting = item.Object.starting,
                                    manual = item.Object.manual,
                                    pid = item.Object.pid,
                                    sales = item.Object.sales,
                                    ending = item.Object.ending
                                };

                                stockArray.Add(item.Key, stock);
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

            DateClass dateEntry = new DateClass
            {
                comp = month + year,
                date = date,
                day = day,
                month = month,
                year = year
            };

            //QC update
            Dictionary<string, ProductClass> localProductsArray = prevelantClass.LoadDataProducts();
            if (localProductsArray != null) if (localProductsArray.ContainsKey(pid))
                {
                    ProductClass product = localProductsArray[pid];

                    if (product.inventoryTracking == "on")
                    {
                        QuantityChangeClass quantityChangeEntry = new QuantityChangeClass();
                        StockReconcilliationClass stockEntry = new StockReconcilliationClass();

                        int iti = 0;

                        if (stockArray != null)
                        {
                            if (stockArray.ContainsKey(pid))
                            {
                                stockEntry = stockArray[pid];
                                iti++;
                            }
                        }

                        if (iti == 0)
                        {
                            double ending, starting;
                            starting = Convert.ToDouble(product.qtyInStock);
                            ending = starting + change;

                            //add it to the Entires array
                            Dictionary<string, QuantityChangeClass> QCs = new Dictionary<string, QuantityChangeClass>();
                            quantityChangeEntry.qcid = rid;
                            quantityChangeEntry.date = date;
                            quantityChangeEntry.time = time;
                            quantityChangeEntry.day = day;
                            quantityChangeEntry.month = month;
                            quantityChangeEntry.year = year;
                            quantityChangeEntry.employee = employee;
                            quantityChangeEntry.eid = eid;
                            quantityChangeEntry.details = "Manual";
                            quantityChangeEntry.newQty = Convert.ToString(ending);
                            quantityChangeEntry.oldQty = localProductsArray[pid].qtyInStock;
                            QCs.Add(quantityChangeEntry.qcid, quantityChangeEntry);

                            //create new entry
                            stockEntry = new StockReconcilliationClass
                            {
                                details = product.brandName + " " + product.productName + " " + product.flavor + " " + product.size,
                                starting = product.qtyInStock,
                                manual = Convert.ToString(change),
                                pid = pid,
                                sales = "",
                                ending = Convert.ToString(ending),
                                QCs = QCs
                            };
                        }
                        else
                        {
                            //update Entries array
                            double oldManual, increase, newManual;
                            increase = change;
                            oldManual = 0;
                            try
                            {
                                oldManual = Convert.ToDouble(stockEntry.manual);
                            }
                            catch (Exception) { }
                            newManual = oldManual + increase;

                            stockEntry.manual = Convert.ToString(newManual);
                            if (stockEntry.details == null) stockEntry.details = product.brandName + " " + product.productName + " " + product.flavor + " " + product.size;
                            if (stockEntry.starting == null) stockEntry.starting = localProductsArray[pid].qtyInStock;
                            if (stockEntry.sales == null) stockEntry.sales = "";
                            double ending, current;
                            current = Convert.ToDouble(stockEntry.ending);
                            ending = current + change;

                            stockEntry.ending = Convert.ToString(ending);

                            quantityChangeEntry.qcid = rid;
                            quantityChangeEntry.date = date;
                            quantityChangeEntry.time = time;
                            quantityChangeEntry.day = day;
                            quantityChangeEntry.month = month;
                            quantityChangeEntry.year = year;
                            quantityChangeEntry.employee = employee;
                            quantityChangeEntry.eid = eid;
                            quantityChangeEntry.details = "Manual";
                            quantityChangeEntry.newQty = stockEntry.ending;
                            quantityChangeEntry.oldQty = Convert.ToString(current);
                        }

                        for (int i = 0; i < numberOfTries; i++)
                        {
                            try
                            {
                                //here we set the QC and the stock entry data for that product
                                FirebaseResponse firebasea1 = client.Set(DatabaseDirectory.QuantityChange() + "/" + date + "/stock/" + pid + "/QCs/" + rid, quantityChangeEntry);
                                FirebaseResponse firebasea11 = client.Set(DatabaseDirectory.QuantityChange() + "/" + date + "/stock/" + pid + "/details/", stockEntry.details);
                                FirebaseResponse firebasea12 = client.Set(DatabaseDirectory.QuantityChange() + "/" + date + "/stock/" + pid + "/ending/", stockEntry.ending);
                                FirebaseResponse firebasea13 = client.Set(DatabaseDirectory.QuantityChange() + "/" + date + "/stock/" + pid + "/manual/", stockEntry.manual);
                                FirebaseResponse firebasea14 = client.Set(DatabaseDirectory.QuantityChange() + "/" + date + "/stock/" + pid + "/pid/", pid);
                                FirebaseResponse firebasea15 = client.Set(DatabaseDirectory.QuantityChange() + "/" + date + "/stock/" + pid + "/sales/", stockEntry.sales);
                                FirebaseResponse firebasea16 = client.Set(DatabaseDirectory.QuantityChange() + "/" + date + "/stock/" + pid + "/starting/", stockEntry.starting);

                                success = true;
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
                            FirebaseResponse firebase2 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + date + "/date", dateEntry.date);
                            FirebaseResponse firebase3 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + date + "/comp", dateEntry.comp);
                            FirebaseResponse firebase4 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + date + "/day", dateEntry.day);
                            FirebaseResponse firebase5 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + date + "/month", dateEntry.month);
                            FirebaseResponse firebase6 = await client.SetAsync(DatabaseDirectory.QuantityChange() + "/" + date + "/year", dateEntry.year);
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                }

            //UPDATE VERSION
            standardFirebaseOperationsClass.UpdateVersion("tracking");

            return success;
        }

        public bool priceChangeUpdate(string pid, string newPrice)
        {
            bool success = false;
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            UsersClass user = prevelantClass.GetCurrentUser();
            string employee = user.userName;
            if (eid == "")
            {
                eid = "1";
            }


            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            string time = dt.ToString("HH:mm:ss tt");
            double time1 = dt.TimeOfDay.TotalMilliseconds;
            int timeId = Convert.ToInt32(time1);
            string pcid = Convert.ToString(year) + Convert.ToString(month) + Convert.ToString(daySele) + Convert.ToString(timeId) + "pc";

            PriceChangeClass change = new PriceChangeClass()
            {
                month = month,
                year = year,
                day = daySele,
                time = time,
                pcid = pcid,
                eid = eid,
                employee = employee,
                newPrice = newPrice
            };

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    //MessageBox.Show("key: " + pcid);
                    FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Products() + "/" + pid + "/priceChanges/" + pcid, change);

                    standardFirebaseOperationsClass.UpdateVersion("product");
                    standardFirebaseOperationsClass.activityLog(pid, "product", "Created Product", eid);
                    success = true;
                    break;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Error: " + ex.Message);
                    Thread.Sleep(duration);
                }
            }
            return success;
        }

        public void updateQuantityEntry(Dictionary<string, ProductEntrySDClass> products, bool purchases, string folio)
        {
            Dictionary<string, ProductClass> productArray = prevelantClass.LoadDataProducts();
            if (products != null) if (products.Count > 0)
                {
                    foreach (var item in products)
                    {
                        //stuff to do with products
                        if (productArray.ContainsKey(item.Value.productID))
                        {
                            ProductEntrySDClass sdProduct = item.Value;
                            ProductClass product = productArray[sdProduct.productID];
                            int inventoryType = 0;
                            int oldQty = 0;
                            int change = 0;
                            try { oldQty = Convert.ToInt32(product.qtyInStock); } catch (Exception) { }
                            try { change = Convert.ToInt32(sdProduct.quantity); } catch (Exception) { }
                            int newQty = oldQty + change;
                            if (product.serialNumberYes)
                            {
                                inventoryType = 1;
                            }
                            else if (product.expiryDateTracking)
                            {
                                inventoryType = 2;
                            }
                            if (purchases)
                            {
                                purchasePriceUpdate(item.Value.productID, sdProduct.price, folio);
                            }
                            StockEntry1(sdProduct.productID, inventoryType, Convert.ToString(newQty), change, sdProduct.serialNumbers, sdProduct.expiryDateBatches, product);
                        }
                    }
                }
        }

        public void updateQuantityDeduction(Dictionary<string, ProductEntrySDClass> onGoingArray)
        {
            Dictionary<string, ProductClass> productArray = prevelantClass.LoadDataProducts();
            if (onGoingArray != null) if (onGoingArray.Count > 0)
                {
                    foreach (var item in onGoingArray)
                    {
                        if (productArray.ContainsKey(item.Value.productID))
                        {
                            ProductEntrySDClass sdProduct = item.Value;
                            ProductClass product = productArray[sdProduct.productID];
                            int inventoryType = 0;
                            int oldQty = 0;
                            int change = 0;
                            try { oldQty = Convert.ToInt32(product.qtyInStock); } catch (Exception) { }
                            try { change = Convert.ToInt32(sdProduct.quantity); } catch (Exception) { }
                            int newQty = oldQty - change;
                            if (product.serialNumberYes)
                            {
                                inventoryType = 1;
                            }
                            else if (product.expiryDateTracking)
                            {
                                inventoryType = 2;
                            }
                            change *= -1;
                            StockDeduct(sdProduct.productID, inventoryType, Convert.ToString(newQty), change, sdProduct.serialNumbers, sdProduct.expiryDateBatches, product);
                        }
                    }
                    standardFirebaseOperationsClass.UpdateVersion("product");
                }
        }

        public bool purchasePriceUpdate(string pid, string newPrice, string rid)
        {
            bool success = false;
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            UsersClass user = prevelantClass.GetCurrentUser();
            string employee = user.userName;
            if (eid == "")
            {
                eid = "1";
            }

            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            string time = dt.ToString("HH:mm:ss tt");
            double time1 = dt.TimeOfDay.TotalMilliseconds;
            int timeId = Convert.ToInt32(time1);
            string pcid = rid;

            PriceChangeClass change = new PriceChangeClass()
            {
                month = month,
                year = year,
                day = daySele,
                time = time,
                pcid = pcid,
                eid = eid,
                employee = employee,
                newPrice = newPrice
            };

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    //MessageBox.Show("Price change called: " + pid + ", price: " + newPrice, "system testing");
                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Products() + "/" + pid + "/purchasePrice/" + pcid, change);

                    standardFirebaseOperationsClass.UpdateVersion("product");
                    standardFirebaseOperationsClass.activityLog(pid, "product", "Created Product", eid);
                    success = true;
                    break;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                    Thread.Sleep(duration);
                }
            }
            return success;
        }

        public async void StockEntry1(string pid, int inventoryType, string newQty, int quantityChange, Dictionary<string, SerialNumberClass> serialNumbers, Dictionary<string, ExpiryDateBatchClass> expiryDateBatches, TextBlock control, Action<bool> method, ProductClass localData)
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


                switch (inventoryType)
                {
                    case 1: //serial
                        if (serialNumbers != null) if (serialNumbers.Count > 0)
                            {
                                foreach (var item in serialNumbers)
                                {
                                    bool exists = false;
                                    if (localData != null) if (localData.serialNumbers != null) if (localData.serialNumbers.ContainsKey(item.Value.serialNumber))
                                            {
                                                exists = true;
                                            }
                                    if (exists)
                                    {
                                        int changeQty = Convert.ToInt32(newQty) - 1;
                                        if (changeQty < 0) changeQty = 0;
                                        newQty = Convert.ToString(changeQty);
                                    } 
                                    else
                                    {
                                        //business as usual
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                FirebaseResponse firebase4 = client.Set(@"Users/" + uid + "/businesses/" + bid + "/products/products/" + pid + "/serialNumbers/" + item.Value.serialNumber, item.Value);
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
                        break;
                    case 2: //expiry
                        if (expiryDateBatches != null) if (expiryDateBatches.Count > 0)
                            {
                                foreach (var item in expiryDateBatches)
                                {
                                    bool exists = false;
                                    if (localData != null) if (localData.expiryDateBatches != null) if (localData.expiryDateBatches.ContainsKey(item.Key))
                                            {
                                                exists = true;
                                                int newValue = localData.expiryDateBatches[item.Key].QtyInStock + item.Value.QtyInStock;
                                                if (newValue > 0)
                                                {
                                                    for (int i = 0; i < numberOfTries; i++)
                                                    {
                                                        try
                                                        {
                                                            FirebaseResponse firebase4 = client.Set(@"Users/" + uid + "/businesses/" + bid + "/products/products/" + pid + "/expiryDateBatches/" + item.Value.date + "/QtyInStock", newValue);
                                                            break;
                                                        }
                                                        catch (Exception)
                                                        {
                                                            Thread.Sleep(duration);
                                                        }
                                                    }
                                                }
                                            }
                                    if (!exists)
                                    {
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                FirebaseResponse firebase4 = client.Set(@"Users/" + uid + "/businesses/" + bid + "/products/products/" + pid + "/expiryDateBatches/" + item.Value.date, item.Value);
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
                        break;
                }
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse firebase6 = client.Set(DatabaseDirectory.Products() + "/" + pid + "/qtyInStock", newQty);

                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }

                standardFirebaseOperationsClass.UpdateVersion("product");
                //update QC
                qcManualUpdate(pid, quantityChange);
                //update activity log
                standardFirebaseOperationsClass.activityLog(pid, "product", "Manual Quantity Change", eid);
            }

            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public async void StockEntry1(string pid, int inventoryType, string newQty, int quantityChange, Dictionary<string, SerialNumberClass> serialNumbers, Dictionary<string, ExpiryDateBatchClass> expiryDateBatches, ProductClass localData)
        {
            setup();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();


            switch (inventoryType)
            {
                case 1: //serial
                    if (serialNumbers != null) if (serialNumbers.Count > 0)
                        {
                            foreach (var item in serialNumbers)
                            {
                                bool exists = false;
                                if (localData != null) if (localData.serialNumbers != null) if (localData.serialNumbers.ContainsKey(item.Value.serialNumber))
                                        {
                                            exists = true;
                                        }

                                if (exists)
                                {
                                    //if it exists don't update, its already there
                                    int changeQty = Convert.ToInt32(newQty) - 1;
                                    if (changeQty < 0) changeQty = 0;
                                    newQty = Convert.ToString(changeQty);
                                } else
                                {
                                    //business as usual
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            FirebaseResponse firebase4 = client.Set(@"Users/" + uid + "/businesses/" + bid + "/products/products/" + pid + "/serialNumbers/" + item.Value.serialNumber, item.Value);
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
                    break;
                case 2: //expiry
                    if (expiryDateBatches != null) if (expiryDateBatches.Count > 0)
                        {
                            foreach (var item in expiryDateBatches)
                            {
                                bool exists = false;
                                if (localData != null) if (localData.expiryDateBatches != null) if (localData.expiryDateBatches.ContainsKey(item.Key))
                                        {
                                            exists = true;
                                            int newValue = localData.expiryDateBatches[item.Key].QtyInStock + item.Value.QtyInStock;
                                            if (newValue > 0)
                                            {
                                                for (int i = 0; i < numberOfTries; i++)
                                                {
                                                    try
                                                    {
                                                        FirebaseResponse firebase4 = client.Set(@"Users/" + uid + "/businesses/" + bid + "/products/products/" + pid + "/expiryDateBatches/" + item.Value.date + "/QtyInStock", newValue);
                                                        break;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Thread.Sleep(duration);
                                                    }
                                                }
                                            }
                                        }
                                if (!exists)
                                {
                                    for (int i = 0; i < numberOfTries; i++)
                                    {
                                        try
                                        {
                                            FirebaseResponse firebase4 = client.Set(@"Users/" + uid + "/businesses/" + bid + "/products/products/" + pid + "/expiryDateBatches/" + item.Value.date, item.Value);
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
                    break;
            }
            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse firebase6 = client.Set(DatabaseDirectory.Products() + "/" + pid + "/qtyInStock", newQty);

                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }

            standardFirebaseOperationsClass.UpdateVersion("product");
            //update QC
            qcManualUpdate(pid, quantityChange);
            //update activity log
            standardFirebaseOperationsClass.activityLog(pid, "product", "Manual Quantity Change", eid);
        }

        public bool StockDeduct(string pid, int inventoryType, string newQty, int quantityChange, Dictionary<string, SerialNumberClass> serialNumbers, Dictionary<string, ExpiryDateBatchClass> expiryDateBatches, ProductClass localData)
        {
            bool success = false;
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();

            if (result)
            {
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();//this is inefficient
                eid = prevelantClass.getEid();


                switch (inventoryType)
                {
                    case 1: //serial
                        if (serialNumbers != null) if (serialNumbers.Count > 0)
                            {
                                foreach (var item in serialNumbers)
                                {
                                    bool exists = false;
                                    if (localData != null) if (localData.serialNumbers != null) if (localData.serialNumbers.ContainsKey(item.Value.serialNumber))
                                            {
                                                exists = true;
                                                //if it exists continue, business as usual
                                                for (int i = 0; i < numberOfTries; i++)
                                                {
                                                    try
                                                    {
                                                        FirebaseResponse firebase4 = client.Delete(@"Users/" + uid + "/businesses/" + bid + "/products/products/" + pid + "/serialNumbers/" + item.Value.serialNumber);
                                                        break;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Thread.Sleep(duration);
                                                    }
                                                }
                                            }
                                    if (!exists)
                                    {
                                        //if it doesn't exist don't update, it's already not there
                                        int changeQty = Convert.ToInt32(newQty) + 1;
                                        if (changeQty! > 0) changeQty = 0;
                                        newQty = Convert.ToString(changeQty);
                                    }
                                }
                            }
                        break;
                    case 2: //expiry
                        if (expiryDateBatches != null) if (expiryDateBatches.Count > 0)
                            {
                                foreach (var item in expiryDateBatches)
                                {
                                    if (localData != null) if (localData.expiryDateBatches != null) if (localData.expiryDateBatches.ContainsKey(item.Key))
                                            {
                                                int newValue = localData.expiryDateBatches[item.Key].QtyInStock - item.Value.QtyInStock;
                                                if (newValue > 0)
                                                {
                                                    for (int i = 0; i < numberOfTries; i++)
                                                    {
                                                        try
                                                        {
                                                            FirebaseResponse firebase4 = client.Set(@"Users/" + uid + "/businesses/" + bid + "/products/products/" + pid + "/expiryDateBatches/" + item.Value.date + "/QtyInStock", newValue);
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
                                                    int changeQty = Convert.ToInt32(newQty) + newValue;
                                                    if (changeQty < 0) changeQty = 0;
                                                    newQty = Convert.ToString(changeQty);
                                                    for (int i = 0; i < numberOfTries; i++)
                                                    {
                                                        try
                                                        {
                                                            FirebaseResponse firebase4 = client.Delete(@"Users/" + uid + "/businesses/" + bid + "/products/products/" + pid + "/expiryDateBatches/" + item.Value.date);
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
                        break;
                }
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse firebase6 = client.Set(DatabaseDirectory.Products() + "/" + pid + "/qtyInStock", newQty);
                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
                //update QC
                qcManualUpdate(pid, quantityChange);
                //update activity log
                standardFirebaseOperationsClass.activityLog(pid, "product", "Manual Quantity Change", eid);
            }
            return success;
        }
    }
}
