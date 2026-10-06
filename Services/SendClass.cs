using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.NetworkInformation;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Net.Http;
using System.Windows;

using Newtonsoft.Json;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System.Windows.Controls;
using WildGrassPOSLibrary.Models;
using WildGrass_Desktop_f8.Services;

namespace WildGrass_Desktop
{
    class SendClass
    {
        string emailMessage = "";
        //check local storage for updates
        string uid = "";
        string bid = "";
        string eid = "";

        //services
        DatabaseDirectoryServicesClass DatabaseDirectory = new();

        IFirebaseConfig ifc = new FirebaseConfig
        {
            AuthSecret = "btiSgvaHMXQWODLMFQSgFBHLJuBykOfzwu9xbfdV",
            BasePath = "https://long-walk-pos.firebaseio.com/"
        };

        IFirebaseClient client;

        public void sendMessage(string message, string phone, TextBox control, Action<string> errorMethod, Action<bool> method)
        {
            bool success = false;
            //we check firebase setup
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool internet = reconilliation.IsConnectedToInternet();
            if (internet)
            {
                WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new ();
                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();
                eid = prevelantClass.getEid();
                try
                {
                    client = new FireSharp.FirebaseClient(ifc);
                }
                catch (Exception)
                {

                }

                FirebaseResponse res;
                FirebaseResponse res1;
                FirebaseResponse res2;
                Dictionary<string, BalanceClass> balances1 = new Dictionary<string, BalanceClass>();
                Dictionary<string, BalanceClass> balances2 = new Dictionary<string, BalanceClass>();
                Dictionary<string, BalanceClass> allBalances = new Dictionary<string, BalanceClass>();
                try
                {
                    res = client.Get(DatabaseDirectory.Businesses() + "/" + bid);
                    BusinessClass business = JsonConvert.DeserializeObject<BusinessClass>(res.Body.ToString());
                    if (business.cmRegistered != null)
                    {
                        if (business.cmRegistered == "true")
                        {
                            if (business.cmSenderId != null)
                            {
                                //success
                                int balance = Convert.ToInt32(business.cmBalance);
                                if (balance > 0)
                                {
                                    SendClass sendClass = new SendClass();
                                    sendClass.ViaSMS(message, phone, business.cmSenderId);
                                    //resetStage();
                                    success = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //error e
                    control.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        errorMethod, e.Message);
                }
            }
            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void loadBalance(TextBlock control, Action<string> method)
        {
            ReconilliationClass reconilliation = new ReconilliationClass();
            bool internet = reconilliation.IsConnectedToInternet();
            if (internet)
            {
                WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();

                uid = prevelantClass.getUid();
                bid = prevelantClass.getBid();
                eid = prevelantClass.getEid();
                try
                {
                    client = new FireSharp.FirebaseClient(ifc);
                }
                catch (Exception)
                {

                }
                FirebaseResponse res;
                try
                {
                    res = client.Get(DatabaseDirectory.FPCSBusinesses() + "/" + bid);
                    BusinessClass business = JsonConvert.DeserializeObject<BusinessClass>(res.Body.ToString());

                    control.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        method, business.cmBalance);
                }
                catch (Exception)
                {
                    //error
                    control.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        method, "---");
                }
            }
            else
            {
                //no internet
                control.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        method, "---");
            }
        }

        public void ViaEmail (QuotationClass entry, string downloadURL)
        {
            emailMessage = "Dear Esteemed Customer, find link to download " + entry.sdType + " from WildGrass IPO, Thank you for your business.\nIf you have enquires please contact us on +260979165078 \n\n\n\n\n" + downloadURL;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.Credentials = new NetworkCredential("lubingatimothy@gmail.com", "Moreauone@1");
            smtpClient.EnableSsl = true;
            smtpClient.Port = 587; //genrally 587 is used for sending email
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("lubingatimothy@gmail.com", "WildGrass", Encoding.UTF8);
            mail.To.Add(new MailAddress("roselynkhoma@gmail.com", "Client", Encoding.UTF8));

            //mail.Attachments.Add(new Attachment(stream, entry.folio + ".pdf"));

            mail.Subject = "Test Mail Function: " + entry.sdType;
            mail.SubjectEncoding = Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.Body = emailMessage;
            mail.BodyEncoding = Encoding.UTF8;

            //send an email
            try
            {
               // smtpClient.Send(mail);
               // System.Windows.Forms.MessageBox.Show("Sent successfully");
            }
            catch (Exception e)
            {

               // System.Windows.Forms.MessageBox.Show("Trouble in paradise: " + e.Message);
            }
            //clean up
            mail.Dispose();

            /*
            //first order of business retrieve file if it exists, else create a new one
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = System.IO.Path.Combine(systemPath, "WildGrass");
            string dirPdf = complete + @"\data\" + uid + @"\businesses\" + bid + @"\output\" + entry.sdType;
            dirPdf += @"\" + entry.folio + ".pdf";
            string pdfLink = "";
            if (File.Exists(dirPdf))
            {
                var stream = File.Open(dirPdf, FileMode.Open);
                //call actual sending funtion 

            } else
            {
                //generate pdf then call actual sending function 
                System.Windows.Forms.MessageBox.Show("We came up empty");
            }
            */
        }

        public async void EmailServiceSend(QuotationClass entry)
        {
            WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new ();
            BusinessClass business = prevelantClass.GetBusiness();
            string downloadURL = "wildgrasspay.web.app/?rid=" + entry.folio + "&date=" + entry.day + entry.month + entry.year;
            string message = "Dear Esteemed Customer, find link to download " + entry.sdType + " from WildGrass IPO, Thank you for your business.\nIf you have enquires please contact us on +260979165078 \n\n\n\n\n" + downloadURL;

            Dictionary<string, string> paramss = new Dictionary<string, string>();
            paramss.Add("from_name", entry.businessName);
            paramss.Add("businessName", entry.businessName);
            paramss.Add("localArea", entry.localArea);
            paramss.Add("phone", business.businessPhone);
            paramss.Add("email", entry.customerEmail);
            paramss.Add("customerName", entry.customerFirstName);
            paramss.Add("message", message);
            paramss.Add("link", downloadURL);
            paramss.Add("sdType", entry.sdType);
            paramss.Add("address", business.address);
            paramss.Add("BusinessTitle", business.businessTitle);
            paramss.Add("reply_to", business.businessEmail);


            EmailJsAPIClass data1 = new EmailJsAPIClass()
            {
                service_id = "service_j0ha9bb",
                template_id = "template_61hd5o2",
                user_id = "GDLc6-brgk16C77oO",
                template_params = paramss
            };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.emailjs.com/api/v1.0/email/send");
            request.Method = "POST";
            //request.Headers.Add("Authorization: OAuth " + accessToken);
            //string postData = string.Format("param1=something&param2=something_else");
            //string postData1 = JsonConvert.ToString(data1);
            string postData = JsonConvert.SerializeObject(data1);
            byte[] data = Encoding.UTF8.GetBytes(postData);

            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "application/json";
            request.ContentLength = data.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    // Do something with response
                    MessageBox.Show("response: " + response.ToString(), "system testing");
                }
            }
            catch (WebException ex)
            {
                // Handle error
                MessageBox.Show("error: " + ex.Message.ToString(), "system testing");
                //MessageBox.Show("passed data: " + postData, "System testing");
                //MessageBox.Show("passed data: " + data, "System testing");
            }

            using (var client = new HttpClient())
            {
                // This would be the like http://www.uber.com
                client.BaseAddress = new Uri("https://api.emailjs.com/api/v1.0/email/send");

                // serialize your json using newtonsoft json serializer then add it to the StringContent
                var content = new StringContent(postData, Encoding.UTF8, "application/json");

                // method address would be like api/callUber:SomePort for example
                var result = await client.PostAsync("POST", content);
                string resultContent = await result.Content.ReadAsStringAsync();
                MessageBox.Show("method 2: " + resultContent, "system testing");
            }
        }

        public async void testSendEmail()
        {
            WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new ();
            BusinessClass business = prevelantClass.GetBusiness();
            string downloadURL = "wildgrasspay.web.app/?rid=";
            string message = "Dear Esteemed Customer, find link to download from WildGrass IPO, Thank you for your business.\nIf you have enquires please contact us on +260979165078 \n\n\n\n\n" + downloadURL;

            Dictionary<string, string> paramss = new Dictionary<string, string>();
            paramss.Add("from_name", "");
            paramss.Add("businessName", "");
            paramss.Add("localArea", "");
            paramss.Add("phone", business.businessPhone);
            paramss.Add("email", "lubingatimothy@gmail.com");
            paramss.Add("message", message);
            paramss.Add("link", downloadURL);
            paramss.Add("address", business.address);
            paramss.Add("BusinessTitle", business.businessTitle);
            paramss.Add("reply_to", business.businessEmail);


            EmailJsAPIClass data1 = new EmailJsAPIClass()
            {
                service_id = "service_j0ha9bb",
                template_id = "template_61hd5o2",
                user_id = "GDLc6-brgk16C77oO",
                template_params = paramss
            };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.emailjs.com/api/v1.0/email/send");
            request.Method = "POST";
            //request.Headers.Add("Authorization: OAuth " + accessToken);
            //string postData = string.Format("param1=something&param2=something_else");
            //string postData1 = JsonConvert.ToString(data1);
            string postData = JsonConvert.SerializeObject(data1);
            byte[] data = Encoding.UTF8.GetBytes(postData);

            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "application/json";
            request.ContentLength = data.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    // Do something with response
                    MessageBox.Show("response: " + response.ToString(), "system testing");
                }
            }
            catch (WebException ex)
            {
                // Handle error
                MessageBox.Show("error: " + ex.Message.ToString(), "system testing");
                //MessageBox.Show("passed data: " + postData, "System testing");
                //MessageBox.Show("passed data: " + data, "System testing");
            }
        }

        public async void ViaSMS(SourceDocumentClass2 sourceDocument, string phoneNumber, string senderId)
        {
            HttpClient client = new HttpClient();
            string url = "wildgrasspay.web.app/?rid=" + sourceDocument.folio + "&date=" + sourceDocument.day + sourceDocument.month + sourceDocument.year;
            string encodedString = Uri.EscapeDataString(url);
            string encodeId = Uri.EscapeDataString(senderId);
            string success = "";


            string toCall = "https://bulksms.zamtel.co.zm/api/v2.1/action/send/api_key/3348fc0137e4af53d0712b8b043b19f0/contacts/" + phoneNumber + "/senderId/" + encodeId + "/message/click%20link%20to%20download%20" + sourceDocument.sdType + "%20" + encodedString + "%20WildGrass%20POS";

            //HttpResponseMessage response = await client.GetAsync("https://bulksms.zamtel.co.zm/api/sms/createSenderID?key=3348fc0137e4af53d0712b8b043b19f0&senderId=world%20Hello");

            try
            {
                HttpResponseMessage response = await client.GetAsync(toCall);
                var responseString = await response.Content.ReadAsStringAsync();
                HttpResponseClass httpResponse = JsonConvert.DeserializeObject<HttpResponseClass>(responseString);
                success = httpResponse.success;

                if (httpResponse.success == "false")
                {
                    MessageBox.Show("Success: ," + httpResponse.success + " Response: " + httpResponse.responseText);
                }
            }
            catch (Exception)
            {
                //queue request and try again later
                //MessageBox.Show("Request failed: We've queued the request and we will try again later");
            }

            //call reduction function if successfull
            if (success == "true")
            {
                deductMessage();
            }
        }
        public async void ViaSMS(QuotationClass quotationClass, string phoneNumber, string senderId)
        {
            HttpClient client = new HttpClient();
            string url = "wildgrasspay.web.app/?rid=" + quotationClass.folio + "&date=" + quotationClass.day + quotationClass.month + quotationClass.year;
            string encodedString = Uri.EscapeDataString(url);
            string encodeId = Uri.EscapeDataString(senderId);
            string success = "";


            string toCall = "https://bulksms.zamtel.co.zm/api/v2.1/action/send/api_key/3348fc0137e4af53d0712b8b043b19f0/contacts/" + phoneNumber + "/senderId/" + encodeId + "/message/click%20link%20to%20download%20" + quotationClass.sdType + "%20" + encodedString + "%20WildGrass%20POS";

            //HttpResponseMessage response = await client.GetAsync("https://bulksms.zamtel.co.zm/api/sms/createSenderID?key=3348fc0137e4af53d0712b8b043b19f0&senderId=world%20Hello");

            try
            {
                HttpResponseMessage response = await client.GetAsync(toCall);
                var responseString = await response.Content.ReadAsStringAsync();
                HttpResponseClass httpResponse = JsonConvert.DeserializeObject<HttpResponseClass>(responseString);
                success = httpResponse.success;

                if (httpResponse.success == "false")
                {
                    MessageBox.Show("Success: ," + httpResponse.success + " Response: " + httpResponse.responseText);
                }
            }
            catch (Exception)
            {
                //queue request and try again later
                //MessageBox.Show("Request failed: We've queued the request and we will try again later");
            }

            //call reduction function if successfull
            if (success == "true")
            {
                deductMessage();
            }
        }

        public async void ViaSMS(InvoiceClass quotationClass, string phoneNumber, string senderId)
        {
            HttpClient client = new HttpClient();
            string url = "wildgrasspay.web.app/?rid=" + quotationClass.folio + "&date=" + quotationClass.day + quotationClass.month + quotationClass.year;
            string encodedString = Uri.EscapeDataString(url);
            string encodeId = Uri.EscapeDataString(senderId);
            string success = "";

            string toCall = "https://bulksms.zamtel.co.zm/api/v2.1/action/send/api_key/3348fc0137e4af53d0712b8b043b19f0/contacts/" + phoneNumber + "/senderId/" + encodeId + "/message/click%20link%20to%20download%20" + quotationClass.sdType + "%20" + encodedString + "%20WildGrass%20POS";

            //HttpResponseMessage response = await client.GetAsync("https://bulksms.zamtel.co.zm/api/sms/createSenderID?key=3348fc0137e4af53d0712b8b043b19f0&senderId=world%20Hello");

            try
            {
                HttpResponseMessage response = await client.GetAsync(toCall);
                var responseString = await response.Content.ReadAsStringAsync();
                HttpResponseClass httpResponse = JsonConvert.DeserializeObject<HttpResponseClass>(responseString);
                success = httpResponse.success;
                if (httpResponse.success == "false")
                {
                    MessageBox.Show("Success: false, Response: " + httpResponse.responseText);
                }
            }
            catch (Exception)
            {
                //queue request and try again later
                MessageBox.Show("Request failed: We've queued the request and we will try again later");
            }

            //call reduction function if successfull
            if (success == "true")
            {
                deductMessage();
            }
        }

        public async void ViaSMS(string messge, string phoneNumber, string senderId)
        {
            HttpClient client = new HttpClient();;
            string encodedString = Uri.EscapeDataString(messge);
            string encodeId = Uri.EscapeDataString(senderId);
            string success = "";

            string toCall = "https://bulksms.zamtel.co.zm/api/v2.1/action/send/api_key/3348fc0137e4af53d0712b8b043b19f0/contacts/" + phoneNumber + "/senderId/" + encodeId + "/message/" + encodedString;

            //HttpResponseMessage response = await client.GetAsync("https://bulksms.zamtel.co.zm/api/sms/createSenderID?key=3348fc0137e4af53d0712b8b043b19f0&senderId=world%20Hello");

            try
            {
                HttpResponseMessage response = await client.GetAsync(toCall);
                var responseString = await response.Content.ReadAsStringAsync();
                HttpResponseClass httpResponse = JsonConvert.DeserializeObject<HttpResponseClass>(responseString);
                success = httpResponse.success;
                if (httpResponse.success == "false")
                {
                    MessageBox.Show("Success: false, Response: " + httpResponse.responseText);
                }
            }
            catch (Exception)
            {
                //queue request and try again later
                MessageBox.Show("Request failed: We've queued the request and we will try again later");
            }

            //call reduction function if successfull
            if (success == "true")
            {
                deductMessage();
            }
        }


        public async void ViaSMSGeneral(string messge, string phoneNumber)
        {
            HttpClient client = new HttpClient(); ;
            string encodedString = Uri.EscapeDataString(messge);
            string encodeId = Uri.EscapeDataString("WildGrass");
            string success = "";

            string toCall = "https://bulksms.zamtel.co.zm/api/v2.1/action/send/api_key/3348fc0137e4af53d0712b8b043b19f0/contacts/" + phoneNumber + "/senderId/" + encodeId + "/message/" + encodedString;

            //HttpResponseMessage response = await client.GetAsync("https://bulksms.zamtel.co.zm/api/sms/createSenderID?key=3348fc0137e4af53d0712b8b043b19f0&senderId=world%20Hello");

            try
            {
                HttpResponseMessage response = await client.GetAsync(toCall);
                var responseString = await response.Content.ReadAsStringAsync();
                HttpResponseClass httpResponse = JsonConvert.DeserializeObject<HttpResponseClass>(responseString);
                success = httpResponse.success;
                if (httpResponse.success == "false")
                {
                    MessageBox.Show("Success: false, Response: " + httpResponse.responseText);
                }
            }
            catch (Exception)
            {
                //queue request and try again later
                MessageBox.Show("Request failed: We've queued the request and we will try again later");
            }
        }

        private async void deductMessage()
        {
            WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new ();
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();
            try
            {
                client = new FireSharp.FirebaseClient(ifc);
            }
            catch (Exception)
            {

            }

            FirebaseResponse res;
            FirebaseResponse res1;
            FirebaseResponse res2;
            Dictionary<string, BalanceClass> balances1 = new Dictionary<string, BalanceClass>();
            Dictionary<string, BalanceClass> balances2 = new Dictionary<string, BalanceClass>();
            Dictionary<string, BalanceClass> allBalances = new Dictionary<string, BalanceClass>();

            res = await client.GetAsync(DatabaseDirectory.FPCSBusinesses() + "/" + bid);
            BusinessClass business = JsonConvert.DeserializeObject<BusinessClass>(res.Body.ToString());

            //getting lists
            res1 = await client.GetAsync(DatabaseDirectory.Businesses() + "/" + bid + "/cmBalances/yearly");
            balances1 = JsonConvert.DeserializeObject<Dictionary<string, BalanceClass>>(res1.Body.ToString());

            res2 = await client.GetAsync(DatabaseDirectory.Businesses() + "/" + bid + "/cmBalances/monthly");
            balances2 = JsonConvert.DeserializeObject<Dictionary<string, BalanceClass>>(res2.Body.ToString());

            int balance = Convert.ToInt32(business.cmBalance);
            //putting lists together
            if (balances2 != null)
            {
                allBalances = balances2;
            }
            if (balances1 != null)
            {
                if (balances1.Count != 0)
                {
                    foreach (var item in balances1)
                    {
                        if (!allBalances.ContainsKey(item.Key)) allBalances.Add(item.Key, item.Value);
                    }
                }
            }


            //getting the smallest weight in the list
            BalanceClass lowest = new BalanceClass();
            int coucou = 0;
            if (allBalances != null)
            {
                if (allBalances.Count != 0)
                {
                    foreach (var item in allBalances)
                    {
                        if (coucou == 0) lowest = item.Value;
                        int current = Convert.ToInt32(lowest.weight);
                        int challenger = Convert.ToInt32(item.Value.weight);
                        if (challenger < current) lowest = item.Value;
                        coucou++;
                    }
                }
            }
            if (coucou != 0)
            {
                string weight = lowest.weight;
                int cuurent = Convert.ToInt32(lowest.messages);
                cuurent--;
                string messages = Convert.ToString(cuurent);

                balance--;
                string cmbal = Convert.ToString(balance);

                FirebaseResponse firebase3 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/cmBalances/" + lowest.duration + "/" + weight + "/messages", messages);
                FirebaseResponse firebase3a = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/cmBalances/" + lowest.duration + "/" + weight + "/messages", messages);

                FirebaseResponse firebase5a = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/cmBalance", cmbal);
                FirebaseResponse firebase5 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/cmBalance", cmbal);
            }
            else
            {
                //we deduct from direct cm balance
                balance--;
                string cmbal = Convert.ToString(balance);

                FirebaseResponse firebase5a = client.Set(DatabaseDirectory.Businesses() + "/" + bid + "/cmBalance", cmbal);
                FirebaseResponse firebase5 = client.Set(DatabaseDirectory.FPCSBusinesses() + "/" + bid + "/cmBalance", cmbal);
            }
        }

        public async void RegisterSenderId(string name, Action<string> method1, Action<string> method2, TextBox control)
        {
            HttpClient client = new HttpClient();
            string encodeId = Uri.EscapeDataString(name);
            string success = "";
            string responsePass = "";

            string toCall = "https://bulksms.zamtel.co.zm/api/sms/createSenderID?key=3348fc0137e4af53d0712b8b043b19f0&senderId=" + encodeId;

            try
            {
                HttpResponseMessage response = await client.GetAsync(toCall);
                var responseString = await response.Content.ReadAsStringAsync();
                HttpResponseClass httpResponse = JsonConvert.DeserializeObject<HttpResponseClass>(responseString);

                success = httpResponse.success;
                responsePass = httpResponse.responseText;
            }
            catch (Exception)
            {
                //queue request and try again later
                MessageBox.Show("Request failed: We've queued the request and we will try again later");
            }

            if (success == "true")
            {
                //update firebase

                //then local will be updated automatically

                control.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    method1, name);
            } else
            {
                control.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    method2, responsePass);
            }
        }
    }
}
