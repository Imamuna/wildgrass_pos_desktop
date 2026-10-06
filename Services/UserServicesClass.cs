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

namespace WildGrass_Desktop_f8.Services
{
    internal class UserServicesClass
    {
        //ID's
        string uid = "";
        string bid = "";
        string eid = "";


        int duration = 2000;
        int numberOfTries = 3;

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
    }
}
