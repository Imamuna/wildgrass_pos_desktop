using Nancy.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WildGrass_Desktop_f8.Services;
using WildGrassPOSLibrary.Models;

namespace WildGrass_Desktop_f8.Functions
{
    internal class EmailClass
    {
        //have a txt with credentials that we can simply call each time we need them
        //create a credentials class

        string uid;
        string eid;
        string bid;

        public EmailCredentialsClass GetCredentials()
        {// this will have to be created in settings
            EmailCredentialsClass credentials = null;
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            string dir1 = complete + @"\data\settings" + @"\emailCredentials.txt";
            if (File.Exists(dir1))
            {
                using (StreamReader r = new StreamReader(dir1))
                {
                    string json1 = r.ReadToEnd();
                    Dictionary<string, EmailCredentialsClass> emailcreds = JsonConvert.DeserializeObject<Dictionary<string, EmailCredentialsClass>>(json1);
                    if (emailcreds != null)
                    {
                        foreach (var item in emailcreds)
                        {
                            if (item.Key == "Credentials")
                            {
                                credentials = new EmailCredentialsClass()
                                {
                                    senderId = item.Value.senderId,
                                    password = item.Value.password
                                };
                            }
                        }
                    }
                }
            }
            return credentials;
        }

        public void AddCredentials(string senderId, string password, TextBox control, Action<bool> method)
        {
            bool success = false;

            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.google.com");
                smtpClient.Host = "smtp.gmail.com";
                smtpClient.Port = 587; //genrally 587 is used for sending email
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(senderId, password);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.EnableSsl = true;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderId, "WildGrass", Encoding.UTF8);
                mail.To.Add(new MailAddress("longwalkzambia@yahoo.com", "System Check", Encoding.UTF8));
                mail.Subject = "Authentication";
                mail.Body = "We are ready and set";
                mail.SubjectEncoding = Encoding.UTF8;
                mail.IsBodyHtml = true;
                mail.BodyEncoding = Encoding.UTF8;

                smtpClient.Send(mail);

                success = true;
            }
            catch (Exception e)
            {
                success = false;
                MessageBox.Show("error" + e.Message, "Smart POS manager");
            }

            if(success) CreateCredentials(senderId, password);

            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        private void CreateCredentials(string senderId, string password)
        {
            WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();
            //first we need to confirm that these are correct before setting them to txt
            string behold = "{";
            behold += (char)34 + "Credentials" + (char)34 + ": {";
            behold += (char)34 + "senderId" + (char)34 + ":" + (char)34 + senderId + (char)34 + ",";
            behold += (char)34 + "password" + (char)34 + ":" + (char)34 + password + (char)34;
            behold += "}";
            behold += "}";
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            string dir = complete + @"\data"+ uid + @"\businesses\" + bid + @"\settings";
            //if path doesn't exist we need to create it
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(dir + @"\emailCredentials.txt", behold);
        }

        public bool sendEmail(QuotationClass quotationClass, string reciepient)
        {

            WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
            BusinessClass business = prevelantClass.GetBusiness();

            string url = "wildgrasspay.web.app/?rid=" + quotationClass.folio + "&date=" + quotationClass.day + quotationClass.month + quotationClass.year;

            string text = "Thank you for choosing to business with " + business.businessName + "\nClick on the link to access your " + quotationClass.sdType + "\n" + url + "WIldGrass POS";
            string subject = quotationClass.sdType;

            MailMessage mail = new MailMessage();

            EmailCredentialsClass credentials = GetCredentials();
            bool success = false;
            if (credentials != null)
            {
                try
                {
                    SmtpClient smtpClient = new SmtpClient("smtp.google.com");
                    smtpClient.Host = "smtp.gmail.com";
                    smtpClient.Port = 587;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(credentials.senderId, credentials.password);
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = true;


                    mail.From = new MailAddress(credentials.senderId, business.businessName, Encoding.UTF8);
                    mail.To.Add(new MailAddress(reciepient, "Customer", Encoding.UTF8));
                    mail.Subject = subject;
                    mail.Body = text;
                    mail.SubjectEncoding = Encoding.UTF8;
                    mail.IsBodyHtml = true;
                    mail.BodyEncoding = Encoding.UTF8;

                    smtpClient.Send(mail);

                    success = true;
                }
                catch (Exception e)
                {
                    string message = e.ToString();
                    MessageBox.Show(message);
                }
            }
            

            return success;
            //if this function returns false ask user to setup email credentials
            //else bring up success indicator
        }


        public void callJavascriptEmailSend()
        {

        }

        public async void sendEmail()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.emailjs.com/api/v1.0/email/send");
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.UseDefaultCredentials = true;

            string json = "{\"service_id\":\"service_j0ha9bb\"," +
                              "\"template_id\":\"template_g9e6x94\"," +
                              "\"user_id\":\"GDLc6-brgk16C77oO\"," +
                              "\"template_params\":{" +
                                  "\"fname\":\"Timothy Lubinga\"," +
                                  "\"email\":\"lubingatimothy@gmail.com\"" +
                              "}" +
                              "}";

            EmailJsAPIClass obj = new EmailJsAPIClass
            {
                service_id = "service_j0ha9bb",
                template_id = "template_g9e6x94",
                user_id = "GDLc6-brgk16C77oO"
            };
            Dictionary<string, string> template_params = new Dictionary<string, string>();
            template_params.Add("fname", "Timothy Lubinga");
            template_params.Add("email", "lubingatimothy@gmail.com");

            obj.template_params = template_params;

            var json1 = new JavaScriptSerializer().Serialize(obj);

            using (var client = new HttpClient())
            {
                string url = "https://api.emailjs.com/api/v1.0/email/send";
                var response = await client.PostAsJsonAsync(url, json1);
                MessageBox.Show("method: " + response.ToString(), "system testing");
            }

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                MessageBox.Show("string version: " + json + "\njson serialized: " + json1, "system testing");
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }

        }
    }
}
