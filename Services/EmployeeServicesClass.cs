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
    internal class EmployeeServicesClass
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

        public void createEmployee(EmployeeClass employee, TextBox control, Action<bool> method)
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
                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Employees() + employee.eid, employee);
                        FirebaseResponse firebase2a = client.Set(DatabaseDirectory.FPCSEmployees() + employee.eid, employee);

                        standardFirebaseOperationsClass.UpdateVersion("employee");
                        standardFirebaseOperationsClass.activityLog(employee.eid, "management", "Created Employee account", this.eid);
                        success = true;

                        BusinessClass business = prevelantClass.GetBusiness();
                        string message = "Welcome " + employee.firstName + " to " + business.businessTitle + " Team" + " \nAccount Login Password: ";
                        message += "\nPassword: " + employee.password;
                        SendClass sendClass = new SendClass();
                        sendClass.ViaSMSGeneral(message, employee.phoneNumber);
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

        public void resetPassword(EmployeeClass employee, TextBox control, Action<bool> method)
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
                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Employees() + employee.eid + "/password", employee.password);
                        FirebaseResponse firebase2a = client.Set(DatabaseDirectory.FPCSEmployees() + employee.eid + "/password", employee.password);

                        standardFirebaseOperationsClass.UpdateVersion("employee");
                        standardFirebaseOperationsClass.activityLog(employee.eid, "management", "Created Employee account", this.eid);
                        success = true;

                        BusinessClass business = prevelantClass.GetBusiness();
                        string message = "Hey " + employee.firstName + "\nYour " + business.businessTitle + " Team Password has been updated " + " \nAccount Login Password: ";
                        message += "\nPassword: " + employee.password;
                        SendClass sendClass = new SendClass();
                        sendClass.ViaSMSGeneral(message, employee.phoneNumber);
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

        public void UpdateEmployeeDetails(string id, string fname, string lname, string name, string email, string phone, string role, AccessLevelClass access, TextBlock control, Action<bool> method)
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
                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Employees() + id + "/name", name);
                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Employees() + id + "/phoneNumber", phone);
                        FirebaseResponse firebase2 = client.Set(DatabaseDirectory.Employees() + id + "/position", role);
                        FirebaseResponse firebase3 = client.Set(DatabaseDirectory.Employees() + id + "/accessLevelBundle", access);
                        FirebaseResponse firebase4 = client.Set(DatabaseDirectory.Employees() + id + "/email", email);
                        FirebaseResponse firebase5 = client.Set(DatabaseDirectory.Employees() + id + "/firstName", fname);
                        FirebaseResponse firebase6 = client.Set(DatabaseDirectory.Employees() + id + "/lastName", lname);

                        FirebaseResponse firebasea = client.Set(DatabaseDirectory.FPCSEmployees() + id + "/name", name);
                        FirebaseResponse firebase1a = client.Set(DatabaseDirectory.FPCSEmployees() + id + "/phoneNumber", phone);
                        FirebaseResponse firebase2a = client.Set(DatabaseDirectory.FPCSEmployees() + id + "/position", role);
                        FirebaseResponse firebase3a = client.Set(DatabaseDirectory.FPCSEmployees() + id + "/accessLevelBundle", access);
                        FirebaseResponse firebase4a = client.Set(DatabaseDirectory.FPCSEmployees() + id + "/email", email);
                        FirebaseResponse firebase5a = client.Set(DatabaseDirectory.FPCSEmployees() + id + "/firstName", fname);
                        FirebaseResponse firebase6a = client.Set(DatabaseDirectory.FPCSEmployees() + id + "/lastName", lname);

                        //only relevant for dets created without updating businessDetails
                        FirebaseResponse firebase7a = client.Set(DatabaseDirectory.FPCSEmployees() + id + "/eid", id);
                        FirebaseResponse firebase8a = client.Set(DatabaseDirectory.FPCSEmployees() + id + "/uid", uid);

                        //update version
                        standardFirebaseOperationsClass.UpdateVersion("employee");
                        //update activity log
                        if (eid == null)
                        {
                            eid = "1";
                        }
                        else if (eid == "")
                        {
                            eid = "1";
                        }
                        standardFirebaseOperationsClass.activityLog(id, "management", "Edit Employee details", eid);
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

        public void DeleteEmployee(string id, TextBox control, Action<bool> method)
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
                        FirebaseResponse response = client.Delete(DatabaseDirectory.Employees() + id);
                        FirebaseResponse response2a = client.Delete(DatabaseDirectory.FPCSEmployees() + id);

                        //update version
                        standardFirebaseOperationsClass.UpdateVersion("employee");
                        //update activity log
                        if (eid == null)
                        {
                            eid = "1";
                        }
                        else if (eid == "")
                        {
                            eid = "1";
                        }
                        standardFirebaseOperationsClass.activityLog(id, "management", "Deleted Employee Account", eid);
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

        public void SearchForUser(string searchTerm, TextBox control, Action<ResponseClass> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool result = reconilliation.IsConnectedToInternet();
            ResponseClass response = new();
            response.Success = false;


            Dictionary<string, UserClass> usersArray = new();
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
                        FirebaseResponse res = client.Get(DatabaseDirectory.FPCSUser());
                        usersArray = JsonConvert.DeserializeObject<Dictionary<string, UserClass>>(res.Body.ToString());
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
            }

            if(usersArray != null) if(usersArray.Count > 0)
                {
                    foreach (var user in usersArray)
                    {
                        if (user.Value.phone == searchTerm || user.Value.email == searchTerm)
                        {
                            response.Success = true;
                            response.Message = user.Value.userName;
                            response.Message2 = user.Value.uid;
                        }
                    }
                }

            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, response);
        }

        public void AddUserToAccount(string accountEid, string userId, string user_name, TextBox control, Action<bool> method)
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
                        //updating our records
                        FirebaseResponse firebase = client.Set(DatabaseDirectory.Employees() + accountEid + "/uid", userId);
                        FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Employees() + accountEid + "/user_name", user_name);
                        FirebaseResponse firebase2 = client.Set(DatabaseDirectory.Employees() + accountEid + "/requestApproved", 0);

                        FirebaseResponse firebasea = client.Set(DatabaseDirectory.FPCSEmployees() + accountEid + "/uid", userId);
                        FirebaseResponse firebase1a = client.Set(DatabaseDirectory.FPCSEmployees() + accountEid + "/user_name", user_name);
                        FirebaseResponse firebase2a = client.Set(DatabaseDirectory.FPCSEmployees() + accountEid + "/requestApproved", 0);

                        //updating his records
                        FirebaseResponse firebase3 = client.Set(DatabaseDirectory.FPCSUser() + "/" + userId + "/employeedBusinesses/" + bid + "/uid", uid);
                        FirebaseResponse firebase4 = client.Set(DatabaseDirectory.FPCSUser() + "/" + userId + "/employeedBusinesses/" + bid + "/bid", bid);
                        FirebaseResponse firebase5 = client.Set(DatabaseDirectory.Users() + "/" + userId + "/employeedBusinesses/" + bid + "/uid", uid);
                        FirebaseResponse firebase6 = client.Set(DatabaseDirectory.Users() + "/" + userId + "/employeedBusinesses/" + bid + "/bid", bid);

                        standardFirebaseOperationsClass.UpdateVersion("employee");
                        standardFirebaseOperationsClass.activityLog(accountEid, "management", "Added User to Employee account", this.eid);
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

        public void RemoveUserFromAccount(string accountEid, string userId, TextBlock control, Action<bool> method)
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
                        //updating our records
                        FirebaseResponse firebase = client.Delete(DatabaseDirectory.Employees() + accountEid + "/uid");
                        FirebaseResponse firebase1 = client.Delete(DatabaseDirectory.Employees() + accountEid + "/user_name");
                        FirebaseResponse firebase2 = client.Delete(DatabaseDirectory.Employees() + accountEid + "/requestApproved");
                        //updating our records
                        FirebaseResponse firebasea = client.Delete(DatabaseDirectory.FPCSEmployees() + accountEid + "/uid");
                        FirebaseResponse firebase1a = client.Delete(DatabaseDirectory.FPCSEmployees() + accountEid + "/user_name");
                        FirebaseResponse firebase2a = client.Delete(DatabaseDirectory.FPCSEmployees() + accountEid + "/requestApproved");

                        //updating his records
                        FirebaseResponse firebase3 = client.Delete(DatabaseDirectory.FPCSUser() + "/" + userId + "/employeedBusinesses/" + bid);
                        FirebaseResponse firebase5 = client.Delete(DatabaseDirectory.Users() + "/" + userId + "/employeedBusinesses/" + bid);

                        standardFirebaseOperationsClass.UpdateVersion("employee");
                        standardFirebaseOperationsClass.activityLog(accountEid, "management", "Removed User from Employee account", this.eid);
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
}
