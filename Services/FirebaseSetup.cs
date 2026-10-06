using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FireSharp.Config;
using FireSharp.Interfaces;

namespace WildGrass_Desktop_f8.Services
{
    internal class FirebaseSetup
    {
        int duration = 2000;
        int numberOfTries = 3;

        IFirebaseConfig ifc = new FirebaseConfig
        {
            AuthSecret = "btiSgvaHMXQWODLMFQSgFBHLJuBykOfzwu9xbfdV",
            BasePath = "https://long-walk-pos.firebaseio.com/"
        };

        public IFirebaseClient setup(IFirebaseClient client)
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
