using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WildGrassPOSLibrary.Models;
using WildGrass_Desktop_f8.Services;

namespace WildGrass_Desktop_f8.Functions
{
    internal class CurrencyClass
    {
        //currency stuff
        string currentCurrency = "";
        decimal currentRate = 0;
        SettingsClass settingsClass;
        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();

        public async void FloatingRate(Action<CurrencyAPIHttpResponseClass> method, TextBox control)
        {
            HttpClient client = new HttpClient();
            string APIKey = "f33ef78ff110a385ab3905087b87b039";

            string toCall = "http://data.fixer.io/api/latest?access_key=" + APIKey;
            CurrencyAPIHttpResponseClass httpResponse = new CurrencyAPIHttpResponseClass();
            httpResponse.success = false;
            try
            {
                HttpResponseMessage response = await client.GetAsync(toCall);
                var responseString = await response.Content.ReadAsStringAsync();
                httpResponse = JsonConvert.DeserializeObject<CurrencyAPIHttpResponseClass>(responseString);
                if (httpResponse.success)
                {
                    saveHttp(responseString);
                }
                else
                {
                    MessageBox.Show("Request failed: " + httpResponse.error.info, "Transactions Manager");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Request failed: " + e.Message, "Transactions Manager");
            }
            await control.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    method, httpResponse);
        }

        private void saveHttp(string res)
        {
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            string dir = complete + @"\data";

            if (res != "null")
            {
                System.IO.File.WriteAllText(dir + @"\AvailableCurrencies.txt", res);
            }
        }

        public decimal calculateRate(string currentRate, string selectedRate, Dictionary<string, decimal> rates)
        {
            decimal cb = 1;
            decimal sb = 1;
            foreach (var rate in rates)
            {
                if(rate.Key == currentRate)
                {
                    cb = rate.Value;
                }
                if(rate.Key == selectedRate)
                {
                    sb = rate.Value;
                }
            }
            decimal sc = 1 / cb * sb;
            return sc;
        }

        public decimal TotalSD(string totalString, decimal rate, string currency)
        {
            settingsClass = prevelantClass.RetrieveSettings();
            if (settingsClass != null) if (settingsClass.bookKeepingSettings != null)
                {
                    currentCurrency = settingsClass.bookKeepingSettings.currency;
                    currentRate = settingsClass.bookKeepingSettings.rate;
                }

            decimal total = Convert.ToDecimal(totalString);

            if (currency != currentCurrency)
            {
                if (currentRate == 0) currentRate = 1;
                if (rate == 0) rate = 1;

                total = total / rate * currentRate;
            }

            //this logic is outdated::
            //Logic of function:
            //we don't store the value of the currency value based on currency
            //we save the original value as well as the rate and carry out multiplication to get the current value each time
            //As such we say:
            //currentValue = oldValue * currentRate
            //once we have currenct value of the currency then we transform this value to the new currency by multiplying by the new rate
            //newValue = currentValue * rate
            //substituting the first equation into the second we get
            //newValue = (oldValue * currentRate) * rate
            //using the same variable for value
            //value = value*currentRate*rate
            //value *= currentRate*rate
            //the last equation is equal to the equation we are using in our function


            return total;
        }

        public decimal Total(string totalString)
        {
            settingsClass = prevelantClass.RetrieveSettings();
            if (settingsClass != null) if (settingsClass.bookKeepingSettings != null)
                {
                    currentCurrency = settingsClass.bookKeepingSettings.currency;
                    currentRate = settingsClass.bookKeepingSettings.rate;
                }
            if (currentRate == null) currentRate = 1;
            if (currentRate == 0) currentRate = 1;
            decimal total = Convert.ToDecimal(totalString);
            total *= currentRate;

            return total;
        }
        public decimal Total(double totalDouble)
        {
            settingsClass = prevelantClass.RetrieveSettings();
            if (settingsClass != null) if (settingsClass.bookKeepingSettings != null)
                {
                    currentCurrency = settingsClass.bookKeepingSettings.currency;
                    currentRate = settingsClass.bookKeepingSettings.rate;
                }
            if (currentRate == null) currentRate = 1;
            if (currentRate == 0) currentRate = 1;
            decimal total = Convert.ToDecimal(totalDouble);
            total *= currentRate;

            return total;
        }

        public string PriceSave(string totalString)
        {
            settingsClass = prevelantClass.RetrieveSettings();
            if (settingsClass != null) if (settingsClass.bookKeepingSettings != null)
                {
                    currentCurrency = settingsClass.bookKeepingSettings.currency;
                    currentRate = settingsClass.bookKeepingSettings.rate;
                }
            if (currentRate == null) currentRate = 1;
            if (currentRate == 0) currentRate = 1;
            decimal total = Convert.ToDecimal(totalString);
            total /= currentRate;
            string tt = Convert.ToString(total);
            return tt;
        }
    }
}
