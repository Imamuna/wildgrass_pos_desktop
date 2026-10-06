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
    /// CRUD Product
    /// </summary>
    internal class ProductServicesClass
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
        public void createProduct(ProductClass product, Button control, Action<bool> method, bool update)
        {
            bool success = false;
            setup();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Products() + "/" + product.pid, product);

                    priceChangeUpdate(product.pid, product.price, false);
                    if (update)
                    {
                        standardFirebaseOperationsClass.UpdateVersion("product");
                    }
                    standardFirebaseOperationsClass.activityLog(product.pid, "product", "Created Product", eid);
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

        public void setData()
        {
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            //static ::
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = System.IO.Path.Combine(systemPath, "WildGrass");
            Dictionary<string, ProductClass> products = prevelantClass.LoadDataProducts();
            Dictionary<string, ProductClass> retrieved = new();

            //create directory
            string dir = complete + @"\data\" + uid + @"\businesses\" + bid + @"\QueuedData";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }


            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    string data = JsonConvert.SerializeObject(products);
                    System.IO.File.WriteAllText(dir + @"\Products.txt", data);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Thread.Sleep(duration);
                }
            }

            //now we retrieve

            if (File.Exists(dir + @"\Products.txt"))
            {
                using (StreamReader r = new StreamReader(dir + @"\Products.txt"))
                {
                    string json = r.ReadToEnd();
                    if (json != null) if (json != "")
                        {
                            retrieved = JsonConvert.DeserializeObject<Dictionary<string, ProductClass>>(json);
                        }
                }
            }

            MessageBox.Show(retrieved.Count.ToString());
        }

        //Update
        public void UpdateproductDetails(string id, string brand, string product, string flavor, string size, string price, string vat, int vatType, string barcode, string category, TextBlock control, Action<bool> method, bool update)
        {
            bool success = false;
            setup();
            eid = prevelantClass.getEid();

            if (barcode == null) barcode = "";
            if (brand == null) brand = "";
            if (category == null) category = "";
            if (flavor == null) flavor = "";
            if (price == null) price = "";
            if (product == null) product = "";
            if (size == null) size = "";
            if (vat == null) vat = "";

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Products() + "/" + id + "/barcode", barcode);
                    FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Products() + "/" + id + "/brandName", brand);
                    FirebaseResponse firebase2 = client.Set(DatabaseDirectory.Products() + "/" + id + "/category", category);
                    FirebaseResponse firebase3 = client.Set(DatabaseDirectory.Products() + "/" + id + "/flavor", flavor);
                    FirebaseResponse firebase5 = client.Set(DatabaseDirectory.Products() + "/" + id + "/price", price);
                    FirebaseResponse firebase6 = client.Set(DatabaseDirectory.Products() + "/" + id + "/productName", product);
                    FirebaseResponse firebase7 = client.Set(DatabaseDirectory.Products() + "/" + id + "/size", size);
                    FirebaseResponse firebase9 = client.Set(DatabaseDirectory.Products() + "/" + id + "/vatType", vatType);
                    FirebaseResponse firebase8 = client.Set(DatabaseDirectory.Products() + "/" + id + "/vat", vat);

                    priceChangeUpdate(id, price, false);
                    if (update)
                    {
                        standardFirebaseOperationsClass.UpdateVersion("product");
                    }
                    standardFirebaseOperationsClass.activityLog(id, "product", "Product Edit", eid);
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
        public void UpdateproductQty(ProductClass product, double change, TextBlock control, Action<bool> method, bool update)
        {
            bool success = false;
            setup();
            eid = prevelantClass.getEid();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Products() + "/" + product.pid + "/bufferQty", product.bufferQty);
                    FirebaseResponse firebase6 = client.Set(DatabaseDirectory.Products() + "/" + product.pid + "/qtyInStock", product.qtyInStock);
                    FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Products() + "/" + product.pid + "/inventoryTracking", product.inventoryTracking);
                    FirebaseResponse firebase2 = client.Set(DatabaseDirectory.Products() + "/" + product.pid + "/expiryDateTracking", product.expiryDateTracking);
                    FirebaseResponse firebase3 = client.Set(DatabaseDirectory.Products() + "/" + product.pid + "/serialNumberYes", product.serialNumberYes);

                    if (product.serialNumberYes)
                    {
                        if (product.serialNumbers != null) if (product.serialNumbers.Count > 0)
                            {
                                foreach (var item in product.serialNumbers)
                                {
                                    FirebaseResponse firebase4 = client.Set(DatabaseDirectory.Products() + "/" + product.pid + "/serialNumbers/" + item.Value.serialNumber, item.Value);
                                }
                            }
                    }

                    if (product.expiryDateTracking)
                    {
                        if (product.expiryDateBatches != null) if (product.expiryDateBatches.Count > 0)
                            {
                                foreach (var item in product.expiryDateBatches)
                                {
                                    FirebaseResponse firebase4 = client.Set(DatabaseDirectory.Products() + "/" + product.pid + "/expiryDateBatches/" + item.Value.date, item.Value);
                                }
                            }
                    }
                    //update QC
                    ProductProcessingServicesClass productProcessingServices = new(); 
                    productProcessingServices.qcManualUpdate(product.pid, change);

                    if (update)
                    {
                        standardFirebaseOperationsClass.UpdateVersion("product");
                    }
                    //update activity log
                    standardFirebaseOperationsClass.activityLog(product.pid, "product", "Manual Quantity Change", eid);
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
        
        public bool priceChangeUpdate(string pid, string newPrice, bool update)
        {
            bool success = false;
            setup();
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

                    if (update)
                    {
                        standardFirebaseOperationsClass.UpdateVersion("product");
                    }
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
        //Update:Mass
        public void massDelete(Dictionary<string, ProductClass> productsArray, TextBox control, Action<bool> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                setup();
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
                                    //standardFirebaseOperationsClass.UpdateVersion("product");

                                    //update activity log
                                    standardFirebaseOperationsClass.activityLog(product.Value.pid, "product", "Deleted product", eid);
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
                standardFirebaseOperationsClass.UpdateVersion("product");
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
                                    //standardFirebaseOperationsClass.UpdateVersion("product");

                                    //activity log
                                    standardFirebaseOperationsClass.activityLog(product.Value.pid, "product", "Edited product category", eid);
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
                standardFirebaseOperationsClass.UpdateVersion("product");
                success = true;
            }

            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        //Delete
        public void DeleteProduct(string id, TextBox control, Action<bool> method)
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
                        FirebaseResponse response = client.Delete(DatabaseDirectory.Products() + "/" + id);

                        //remove from tracking
                        //FirebaseResponse response1 = client.Delete(@"Users/" + uid + "/businesses/" + bid + "/Tracking" + id);


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
                        standardFirebaseOperationsClass.activityLog(id, "product", "Deleted product", eid);
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
        //(if we are online skip local database and load straight from firebase)

        public void ImportProductData(Dictionary<string, ProductClass> dataIn, Button control, TextBlock control1, Action<bool> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            bool success = false;

            if (result)
            {
                success = true;
                setup();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();
                eid = prevelantClass.getEid();
                //firstly we get current category and product data;
                Dictionary<string, ProductClass> products = prevelantClass.LoadDataProducts();
                Dictionary<string, CategoryClass> categories = prevelantClass.LoadDataCategories();


                foreach (var item in dataIn)
                {
                    ProductClass product = item.Value;
                    //data needs to be in correct format
                    try
                    {
                        CurrencyClass currencyClass = new CurrencyClass();
                        product.price = currencyClass.PriceSave(item.Value.price);
                        success = true;
                    }
                    catch (Exception)
                    {
                        success = false;
                    }

                    if (product.productName != null)
                    {
                        if (product.productName.Length == 0)
                            success = false;
                    } else
                    {
                        success = false;
                    }

                    if (success)
                    {
                        if (product.category == null)
                            product.category = "General";

                        if (product.category.Length == 0)
                            product.category = "General";

                        //if category doesn't exist create new category
                        if (categories == null)
                        {
                            categoryServices.createCategory(product.category, control, emptyFunction, false);
                        }
                        else if (!categories.ContainsKey(product.category))
                        {
                            categoryServices.createCategory(product.category, control, emptyFunction, false);
                        }

                        bool update = false;
                        if (products != null) if (products.ContainsKey(item.Key))
                                update = true;

                        if (update)
                        {
                            if (product.inventoryTracking == "on")
                            {
                                if (product.qtyInStock != null && product.bufferQty != null)
                                {
                                    try
                                    {
                                        int prp = Convert.ToInt32(product.qtyInStock);
                                        int prp1 = Convert.ToInt32(product.bufferQty);
                                    }
                                    catch (Exception)
                                    {
                                        item.Value.qtyInStock = "0";
                                        item.Value.bufferQty = "0";
                                    }

                                    double change = 0;
                                    if (products[item.Key].qtyInStock != product.qtyInStock || products[item.Key].bufferQty != product.bufferQty)
                                    {
                                        try
                                        {
                                            change = Convert.ToDouble(product.qtyInStock) - Convert.ToDouble(products[item.Key].qtyInStock);
                                        }
                                        catch (Exception)
                                        {

                                        }
                                        UpdateproductQty(product, change, control1, emptyFunction, false);
                                    }
                                }
                            }

                            //move on, to upload data
                            UpdateproductDetails(product.pid, product.brandName, product.productName, product.flavor, product.size, product.price, product.vat, product.vatType, product.barcode, product.category, control1, progressOfImport, false);
                        }
                        else
                        {
                            createProduct(product, control, progressOfImport, false);
                        }
                        //update version
                        standardFirebaseOperationsClass.UpdateVersion("product");
                    }
                }
            }
            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        private void emptyFunction(bool success)
        {

        }

        private void progressOfImport(bool success)
        {
            currentCount++;
        }
    }
}
