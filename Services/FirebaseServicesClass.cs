using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System.Threading;

namespace WildGrass_Desktop_f8.Services
{
    internal class FirebaseServicesClass
    {
        int duration = 2000;
        int numberOfTries = 3;

        //Auth
        public FirebaseAuthProvider firebaseAuthProvider;

        //RTDB
        public IFirebaseClient client;

        public FirebaseServicesClass()
        {
            IFirebaseConfig ifc = new FireSharp.Config.FirebaseConfig
            {
                AuthSecret = "btiSgvaHMXQWODLMFQSgFBHLJuBykOfzwu9xbfdV",
                BasePath = "https://long-walk-pos.firebaseio.com/"
            };

            //Auth
            IHost _host = Host
                .CreateDefaultBuilder()
                .ConfigureServices((context, service) =>
                {
                    string FirebaseApiKey = context.Configuration.GetValue<string>("FIREBASE_API_KEY");
                    service.AddSingleton(new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(FirebaseApiKey)));
                })
                .Build();
            firebaseAuthProvider = _host.Services.GetRequiredService<FirebaseAuthProvider>();

            //RTDB
            for (int i = 0; i<numberOfTries; i++)
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
    }
}
