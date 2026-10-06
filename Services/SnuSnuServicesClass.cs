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
using System.Threading;

namespace WildGrass_Desktop_f8.Services
{
    internal class SnuSnuServicesClass
    {
        //ID's
        string uid = "";
        string bid = "";
        string eid = "";


        int duration = 2000;
        int numberOfTries = 3;

        IFirebaseConfig ifc = new FirebaseConfig
        {
            AuthSecret = "btiSgvaHMXQWODLMFQSgFBHLJuBykOfzwu9xbfdV",
            BasePath = "https://long-walk-pos.firebaseio.com/"
        };

        IFirebaseClient client;

        public SnuSnuServicesClass(string uid, string bid, string eid, IFirebaseConfig ifc, IFirebaseClient client)
        {
            this.uid=uid;
            this.bid=bid;
            this.eid=eid;
            this.ifc=ifc;

            if(client == null)
            {
                setup(this.client);
            } else
            {
                this.client=client;
            }
        }

        private IFirebaseClient setup(IFirebaseClient? client)
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
            return client;
        }


    }
}
