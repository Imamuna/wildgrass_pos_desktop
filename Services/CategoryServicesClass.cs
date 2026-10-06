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
    /// CRUD Category
    /// </summary>
    internal class CategoryServicesClass
    {
        //ID's
        string uid = "";
        string bid = "";
        string eid = "";


        int duration = 2000;
        int numberOfTries = 3;

        DatabaseDirectoryServicesClass DatabaseDirectory = new();
        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
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
        public void createCategory(string category, Button control, Action<bool> method, bool update)
        {
            bool success = false;
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

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse response = client.Set(DatabaseDirectory.Categories() + "/" + category + "/categoryName", category);

                    //update version
                    if (update)
                    {
                        standardFirebaseOperationsClass.UpdateVersion("product");
                    }
                    //activity log
                    standardFirebaseOperationsClass.activityLog(category, "product", "Created category", eid);
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

        //Update
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
                                        standardFirebaseOperationsClass.UpdateVersion("product");
                                        //update activity
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
                    }

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse response = client.Set(DatabaseDirectory.Categories() + "/" + newName + "/categoryName", newName);
                        FirebaseResponse response1 = client.Delete(DatabaseDirectory.Categories() + "/" + oldName);

                        //update version
                        //MessageBox.Show(Convert.ToString(count) + ", moved from " + oldName + " to " + newName, "System testing");
                        standardFirebaseOperationsClass.UpdateVersion("product");
                        standardFirebaseOperationsClass.activityLog(oldName, "product", "Edited category name", eid);
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
                                        //update activity
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
                    }

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse response1 = client.Delete(DatabaseDirectory.Categories() + "/" + oldName);

                        //update version
                        standardFirebaseOperationsClass.UpdateVersion("product");
                        standardFirebaseOperationsClass.activityLog(oldName, "product", "Deleted category", eid);
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
                                        //update activity
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
                    }

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse response1 = client.Delete(DatabaseDirectory.Categories() + "/" + oldName);

                        //update version
                        standardFirebaseOperationsClass.UpdateVersion("product");
                        standardFirebaseOperationsClass.activityLog(oldName, "product", "Deleted category", eid);
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
    }
}