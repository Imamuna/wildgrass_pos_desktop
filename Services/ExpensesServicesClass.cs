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
    /// CRUD Expense and expense category
    /// </summary>
    internal class ExpensesServicesClass
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
        public void massSave(Dictionary<string, ProductEntrySDClass> products, Button control)
        {
            ExpensesServicesClass expensesServices = new();
            Dictionary<string, ExpenseClass> expensesArray = expensesServices.LoadDataExpenses();
            if (products != null) if (products.Count > 0)
                {
                    foreach (var item in products)
                    {
                        if (item.Value.save && item.Value.entryType == "expense")
                        {
                            //we have marked this to be saved
                            if (!expensesArray.ContainsKey(item.Value.productID))
                            {
                                ExpenseClass expense = new()
                                {
                                    pid = item.Value.productID,
                                    details = item.Value.productName,
                                    price = 0,
                                    category = item.Value.category,
                                    directExpenseYes = item.Value.directExpenseYes,
                                    fixedExpenseYes = false,
                                };
                                createExpense(expense, control, NullFunction, false);
                            }
                        }
                    }
                }
            standardFirebaseOperationsClass.UpdateVersion("expense");


        }
        private void NullFunction(bool success)
        {

        }
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
                    FirebaseResponse response = client.Set(DatabaseDirectory.ExpenseCategories() + "/" + category + "/categoryName", category);

                    //update version
                    if (update)
                    {
                        standardFirebaseOperationsClass.UpdateVersion("expense");
                    }
                    //activity log
                    standardFirebaseOperationsClass.activityLog(category, "expense", "Created category", eid);
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
        public void createExpense(ExpenseClass expense, Button control, Action<bool> method, bool update)
        {
            bool success = false;
            setup();

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse firebase = client.Set(DatabaseDirectory.Expenses() + "/" + expense.pid, expense);

                    priceChangeUpdate(expense.pid, expense.price, false);
                    if (update)
                    {
                        standardFirebaseOperationsClass.UpdateVersion("expense");
                    }
                    standardFirebaseOperationsClass.activityLog(expense.pid, "expense", "Created Expense", eid);
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
        public void UpdateExpenseDetails(string id, string details, string price, int vatType, string category, bool directCost, bool fixedCost, TextBlock control, Action<bool> method, bool update)
        {
            bool success = false;
            setup();
            eid = prevelantClass.getEid();

            if (category == null) category = "";
            if (price == null) price = "";
            if (details == null) details = "";
            double priceDouble = 0;
            try
            {
                priceDouble = Convert.ToDouble(price);
            }
            catch (Exception)
            {

            }
            //ExpenseClass ex = new();
            //ex.fixedExpenseYes
            if(category.Length != 0 && price.Length != 0 && details.Length != 0)
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse firebase2 = client.Set(DatabaseDirectory.Expenses() + "/" + id + "/category", category);
                        FirebaseResponse firebase5 = client.Set(DatabaseDirectory.Expenses() + "/" + id + "/price", price);
                        FirebaseResponse firebase6 = client.Set(DatabaseDirectory.Expenses() + "/" + id + "/details", details);
                        FirebaseResponse firebase9 = client.Set(DatabaseDirectory.Expenses() + "/" + id + "/vatType", vatType);
                        FirebaseResponse firebase8 = client.Set(DatabaseDirectory.Expenses() + "/" + id + "/directExpenseYes", directCost);
                        FirebaseResponse firebase7 = client.Set(DatabaseDirectory.Expenses() + "/" + id + "/fixedExpenseYes", fixedCost);

                        priceChangeUpdate(id, priceDouble, false);
                        if (update)
                        {
                            standardFirebaseOperationsClass.UpdateVersion("expense");
                        }
                        standardFirebaseOperationsClass.activityLog(id, "expense", "Expense Edit", eid);
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
        public void editCategoryName(Dictionary<string, ExpenseClass> expensesArray, string newName, string oldName, TextBox control, Action<bool> method)
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
                if (expensesArray != null) if (expensesArray.Count > 0)
                    {
                        foreach (var product in expensesArray)
                        {
                            if (product.Value.category == oldName)
                            {
                                count++;
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        FirebaseResponse response3 = client.Set(DatabaseDirectory.Expenses() + "/" + product.Value.pid + "/category", newName);
                                        standardFirebaseOperationsClass.UpdateVersion("expense");
                                        //update activity
                                        standardFirebaseOperationsClass.activityLog(product.Value.pid, "expense", "Edited expense category", eid);
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
                        FirebaseResponse response = client.Set(DatabaseDirectory.ExpenseCategories() + "/" + newName + "/categoryName", newName);
                        FirebaseResponse response1 = client.Delete(DatabaseDirectory.ExpenseCategories() + "/" + oldName);

                        //update version
                        standardFirebaseOperationsClass.UpdateVersion("expense");
                        standardFirebaseOperationsClass.activityLog(oldName, "expense", "Edited category name", eid);
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

        public bool priceChangeUpdate(string pid, double newPrice, bool update)
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
                newPrice = newPrice.ToString()
            };

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    //MessageBox.Show("key: " + pcid);
                    FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Expenses() + "/" + pid + "/priceChanges/" + pcid, change);

                    if (update)
                    {
                        standardFirebaseOperationsClass.UpdateVersion("expense");
                    }
                    standardFirebaseOperationsClass.activityLog(pid, "expense", "Created Expense", eid);
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

        //Delete
        public void DeleteExpense(string id, TextBlock control, Action<bool> method)
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
                        FirebaseResponse response = client.Delete(DatabaseDirectory.Expenses() + "/" + id);

                        //update version
                        standardFirebaseOperationsClass.UpdateVersion("expense");
                        //update activity log
                        if (eid == null)
                        {
                            eid = "1";
                        }
                        else if (eid == "")
                        {
                            eid = "1";
                        }
                        standardFirebaseOperationsClass.activityLog(id, "expense", "Deleted expense", eid);
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
        public void deleteCategoryNdProducts(Dictionary<string, ExpenseClass> expensesArray, string oldName, TextBox control, Action<bool> method)
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
                if (expensesArray != null) if (expensesArray.Count > 0)
                    {
                        foreach (var product in expensesArray)
                        {
                            if (product.Value.category == oldName)
                            {
                                count++;
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        FirebaseResponse response3 = client.Delete(DatabaseDirectory.Expenses() + "/" + product.Value.pid);
                                        //update activity
                                        standardFirebaseOperationsClass.activityLog(product.Value.pid, "expense", "Deleted expense", eid);
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
                        FirebaseResponse response1 = client.Delete(DatabaseDirectory.ExpenseCategories() + "/" + oldName);

                        //update version
                        standardFirebaseOperationsClass.UpdateVersion("expense");
                        standardFirebaseOperationsClass.activityLog(oldName, "expense", "Deleted category", eid);
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
        public void moveProductsDeleteCategory(Dictionary<string, ExpenseClass> expensesArray, string newName, string oldName, TextBox control, Action<bool> method)
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

                if (expensesArray != null) if (expensesArray.Count > 0)
                    {
                        foreach (var product in expensesArray)
                        {
                            if (product.Value.category == oldName)
                            {
                                for (int i = 0; i < numberOfTries; i++)
                                {
                                    try
                                    {
                                        FirebaseResponse response3 = client.Set(DatabaseDirectory.Expenses() + "/" + product.Value.pid + "/category", newName);
                                        //update activity
                                        standardFirebaseOperationsClass.activityLog(product.Value.pid, "expense", "Edited expense category", eid);
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
                        FirebaseResponse response1 = client.Delete(DatabaseDirectory.ExpenseCategories() + "/" + oldName);

                        //update version
                        standardFirebaseOperationsClass.UpdateVersion("expense");
                        standardFirebaseOperationsClass.activityLog(oldName, "expense", "Deleted category", eid);
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
        public Dictionary<string, CategoryClass> LoadDataCategories()
        {
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();

            Dictionary<string, CategoryClass> categoriesArray = new Dictionary<string, CategoryClass>();
            //static ::
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = System.IO.Path.Combine(systemPath, "WildGrass");

            //categories
            if (File.Exists(complete + @"\data\" + uid + @"\businesses\" + bid + @"\Expenses\Categories.txt"))
            {
                using (StreamReader r = new StreamReader(complete + @"\data\" + uid + @"\businesses\" + bid + @"\Expenses\Categories.txt"))
                {
                    string json = r.ReadToEnd();
                    if (json != null) if (json != "")
                        {
                            categoriesArray = JsonConvert.DeserializeObject<Dictionary<string, CategoryClass>>(json);
                        }
                }
            }

            return categoriesArray;
        }

        public Dictionary<string, ExpenseClass> LoadDataExpenses()
        {
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();

            Dictionary<string, ExpenseClass> expensesArray = new ();
            //static ::
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = System.IO.Path.Combine(systemPath, "WildGrass");
            //products
            if (File.Exists(complete + @"\data\" + uid + @"\businesses\" + bid + @"\Expenses\Expenses.txt"))
            {
                using (StreamReader r = new StreamReader(complete + @"\data\" + uid + @"\businesses\" + bid + @"\Expenses\Expenses.txt"))
                {
                    string json = r.ReadToEnd();
                    if (json != null) if (json != "")
                        {
                            expensesArray = JsonConvert.DeserializeObject<Dictionary<string, ExpenseClass>>(json);
                        }
                }
            }

            return expensesArray;
        }
    }
}
