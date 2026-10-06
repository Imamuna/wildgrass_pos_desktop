using FluentEmail.Core;
using FluentEmail.Smtp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using WildGrass_Desktop;
using WildGrass_Desktop_f8.Services;

namespace WildGrass_Desktop_f8.Functions
{
    internal class EmailServiceClass
    {
        private async void SendEmail()
        {
            var sender = new SmtpSender(() => new SmtpClient("localhost")
            {
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Port = 25
            });

            var email = await Email
                .From("lubingatimothy@gmail.com")
                .To("wildgrasspos@gmail.com")
                .Subject("System Testing")
                .Body("Thanks for buying our product")
                .SendAsync();

        }

        public void WebSend(string date, string folio, string sdType)
        {
            WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
            string uid = prevelantClass.getUid();
            string bid = prevelantClass.getBid();
            string apiKey = "09349202094976634539788732222390948873894kja";
            string url = "https://wildgrassapi.web.app/index.html?apiKey=" + apiKey + "&date=" + date + "&uid=" + uid + "&bid=" + bid + "&folio=" + folio + "&sdType=" + sdType;

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}
