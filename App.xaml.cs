using Firebase.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WildGrass_Desktop_f8.Activities;
using WildGrass_Desktop_f8.Activities.multiBranch;
using WildGrass_Desktop_f8.Activities.starter;
using WildGrass_Desktop_f8.Tests;

namespace WildGrass_Desktop_f8
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host
                .CreateDefaultBuilder()
                .ConfigureServices((context, service) =>
                {
                    string FirebaseApiKey = context.Configuration.GetValue<string>("FIREBASE_API_KEY");
                    service.AddSingleton(new FirebaseAuthProvider(new FirebaseConfig(FirebaseApiKey)));

                    service.AddSingleton<StarterWindow>((services) => new StarterWindow()
                    {
                        DataContext = new CreateAccountControl(services.GetRequiredService<FirebaseAuthProvider>())
                    });
                })
                .Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {

            SplashWindow splashWindow = new SplashWindow();
            splashWindow.Show();

            //TestingWindow testingWindow = new();
            //testingWindow.Show();

            //StarterWindow starterWindow = _host.Services.GetRequiredService<StarterWindow>();
            //StarterWindow starterWindow = new StarterWindow();
            //starterWindow.Show();
            //MainWindow main = new MainWindow();
            //main.Show();
            //MultiBranchWindow multiBranch = new MultiBranchWindow();
            //multiBranch.Show();

            base.OnStartup(e);
        }

    }
}
