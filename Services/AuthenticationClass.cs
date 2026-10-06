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
using System.Diagnostics;

namespace WildGrass_Desktop_f8.Services
{
    internal class AuthenticationClass
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

        public void retrieveAccountPortal(string phone, string email, Action<bool> method)
        {
            setup();

            ReconilliationClass reconilliationClass = new ReconilliationClass();
            bool result = reconilliationClass.IsConnectedToInternet();
            bool success = false;

            if (result)
            {
                Dictionary<string, UsersClass> allUsers = new();

                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse res = client.Get(DatabaseDirectory.FPCSUser());
                        allUsers = JsonConvert.DeserializeObject<Dictionary<string, UsersClass>>(res.Body.ToString());
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }

                bool phoneFound = false;
                bool emailFound = false;
                string uid = "";

                Random generator = new Random();
                string phoneOTP = generator.Next(0, 1000000).ToString("D6");
                string emailOTP = generator.Next(0, 1000000).ToString("D6");


                if (allUsers != null)
                {
                    foreach (var item in allUsers)
                    {
                        if (item.Value.phone == phone)
                        {
                            phoneFound = true;
                            uid = item.Value.uid;
                        }
                        if (item.Value.email == email)
                        {
                            emailFound = true;
                            uid = item.Value.uid;
                        }
                    }

                    if (phoneFound || emailFound)
                    {
                        for (int i = 0; i < numberOfTries; i++)
                        {
                            try
                            {
                                FirebaseResponse response1 = client.Set(@"Onboarding customers/customers in process/" + uid + "/email", email);
                                FirebaseResponse response2 = client.Set(@"Onboarding customers/customers in process/" + uid + "/phone", phone);
                                FirebaseResponse response3 = client.Set(@"Onboarding customers/customers in process/" + uid + "/phoneOTP", phoneOTP);
                                FirebaseResponse response4 = client.Set(@"Onboarding customers/customers in process/" + uid + "/emailOTP", emailOTP);


                                string url = "https://wildgrass.web.app/pages/forgotPassword.html?folio=" + uid;
                                //string url = "http://127.0.0.1:5501/pages/accountCreatePortal.html?folio=" + user.uid;
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = url,
                                    UseShellExecute = true
                                });
                                success = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                Thread.Sleep(duration);
                            }
                        }
                    }
                }

            }
        }

        public void loginAccount(string key, string pass, Action<bool, UserClass, Dictionary<string, UserClass>, string, string> method)
        {
            setup();

            Dictionary<string, UserClass> allUsers = new();

            string message = "Please check your internet connection";

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    FirebaseResponse res = client.Get(DatabaseDirectory.Users());
                    allUsers = JsonConvert.DeserializeObject<Dictionary<string, UserClass>>(res.Body.ToString());
                    break;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(duration);
                    MessageBox.Show(ex.Message);
                }
            }


            bool phoneFound = false;
            bool emailFound = false;
            string uid = "";

            bool credentialsConfirmed = false;
            UserClass user = new();

            if (allUsers != null)
            {
                message = "Please check phone/email entered";
                foreach (var item in allUsers)
                {
                    if (item.Value.phone == key)
                    {
                        phoneFound = true;
                        uid = item.Value.uid;
                    }
                    if (item.Value.email == key)
                    {
                        emailFound = true;
                        uid = item.Value.uid;
                    }

                    if (phoneFound || emailFound)
                    {
                        message = "Incorrect password";
                        if (item.Value.password == pass)
                        {
                            credentialsConfirmed = true;
                            user = item.Value;
                            break;
                        }
                    }

                }
            }

            method(credentialsConfirmed, user, allUsers, uid, message);
        }

        public void createAccountPortal(UserClass user, Button control, Action<bool> method)
        {
            setup();

            ReconilliationClass reconilliationClass = new ReconilliationClass();
            bool result = reconilliationClass.IsConnectedToInternet();
            bool success = false;

            if (result)
            {
                //check if the user has an active internet connection before trying this shit
                FirebaseResponse res = client.Get(DatabaseDirectory.FPCSUser());
                Dictionary<string, UsersClass> allUsers = JsonConvert.DeserializeObject<Dictionary<string, UsersClass>>(res.Body.ToString());

                bool phoneFound = false;
                bool emailFound = false;

                Random generator = new Random();
                string phoneOTP = generator.Next(0, 1000000).ToString("D6");
                string emailOTP = generator.Next(0, 1000000).ToString("D6");

                if (allUsers != null)
                {
                    foreach (var item in allUsers)
                    {
                        if (item.Value.phone == user.phone)
                        {
                            phoneFound = true;
                        }
                        if (item.Value.email == user.email)
                        {
                            emailFound = true;
                        }
                    }
                }
                if (phoneFound)
                {
                    MessageBox.Show("User with this phone number already exists", "Account Manager");
                }
                else if (emailFound)
                {
                    MessageBox.Show("User with this email address already exists", "Account Manager");
                }
                else
                {
                    for (int i = 0; i < numberOfTries; i++)
                    {
                        try
                        {
                            FirebaseResponse response = client.Set(@"Onboarding customers/customers in process/" + user.uid, user);
                            FirebaseResponse response1 = client.Set(@"Onboarding customers/customers in process/" + user.uid + "/phoneOTP", phoneOTP);
                            FirebaseResponse response2 = client.Set(@"Onboarding customers/customers in process/" + user.uid + "/emailOTP", emailOTP);


                            string url = "https://wildgrass.web.app/pages/accountCreatePortal.html?folio=" + user.uid;
                            //string url = "http://127.0.0.1:5501/pages/accountCreatePortal.html?folio=" + user.uid;
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = url,
                                UseShellExecute = true
                            });
                            success = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(duration);
                        }
                    }
                }
            }

            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void editPersonalInfo(UserClass user, Button control, Action<bool> method)
        {
            setup();

            ReconilliationClass reconilliationClass = new ReconilliationClass();
            bool result = reconilliationClass.IsConnectedToInternet();
            bool success = false;
            if (result)
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        FirebaseResponse response1 = client.Set(DatabaseDirectory.Users() + "/" + user.uid + "/firstName", user.firstName);
                        FirebaseResponse response2 = client.Set(DatabaseDirectory.Users() + "/" + user.uid + "/lastName", user.lastName);
                        FirebaseResponse response3 = client.Set(DatabaseDirectory.Users() + "/" + user.uid + "/userName", user.userName);
                        FirebaseResponse response4 = client.Set(DatabaseDirectory.Users() + "/" + user.uid + "/email", user.email);
                        FirebaseResponse response5 = client.Set(DatabaseDirectory.Users() + "/" + user.uid + "/phone", user.phone);
                        FirebaseResponse response1a = client.Set(DatabaseDirectory.FPCSUser() + "/" + user.uid + "/firstName", user.firstName);
                        FirebaseResponse response2a = client.Set(DatabaseDirectory.FPCSUser() + "/" + user.uid + "/lastName", user.lastName);
                        FirebaseResponse response3a = client.Set(DatabaseDirectory.FPCSUser() + "/" + user.uid + "/userName", user.userName);
                        FirebaseResponse response4a = client.Set(DatabaseDirectory.FPCSUser() + "/" + user.uid + "/email", user.email);
                        FirebaseResponse response5a = client.Set(DatabaseDirectory.FPCSUser() + "/" + user.uid + "/phone", user.phone);

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
