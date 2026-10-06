using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WildGrass_Desktop;
using WildGrassPOSLibrary.Models;
using WildGrass_Desktop_f8.Services;

namespace WildGrass_Desktop_f8.Functions
{
    internal class GenerateCSV
    {
        //currency stuff
        string currency = "";
        SettingsClass settingsClass = new();
        CurrencyClass currencyClass = new();

        //Services
        WildGrassPOSLibrary.Services.LocalDirectoryServicesClass localDirectory = new();
        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
        public void testGenerateCSV()
        {
            WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new ();
            BusinessClass business = prevelantClass.GetBusiness();

            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string complete = System.IO.Path.Combine(systemPath, "WildGrass");
            string dirPdf = complete + @"\" + business.businessTitle + @"\" + "Reports";
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }
            dirPdf += @"\testfile" + ".csv";
            string data = "Col1, Col2, Col2";
            var filepath = "your_path.csv";
            using (StreamWriter writer = new StreamWriter(new FileStream(dirPdf,
            FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine("Hello, Goodbye");
            }
        }

        public void generateSalesTemplate(TextBlock control, Action<Response> method)
        {
            bool success = false;
            WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
            BusinessClass business = prevelantClass.GetBusiness();

            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string complete = System.IO.Path.Combine(systemPath, "WildGrass");
            string dirPdf = complete + @"\" + business.businessTitle + @"\" + @"Reports\Sales";
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }
            dirPdf += @"\Sales Template.csv";

            Response res = new Response();
            res.responseText = dirPdf;
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(dirPdf,
                FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine("sep=,");
                    writer.WriteLine("Date, Time, Details, Folio, Dr, Cr, Vat");
                }
                success = true;
            }
            catch (Exception ex)
            {
                res.responseText = ex.Message;
            }
            res.success = success;
            control.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               method, res);
        }

        public void generateSalesAccountCSV (int dateRange, string dateSelect, int selectedDay, int selectedMonth, int selectedYear, TextBlock control, Action<Response> method)
        {
            bool success = false;
            string dirPdf = localDirectory.SalesReportsFolder();
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }
            switch(dateRange)
            {
                case 0:
                    dirPdf += @"\" + selectedDay + ", " + selectedMonth + " " + selectedYear + " daily sales.csv";
                    break;
                case 1:
                    dirPdf += @"\" + selectedMonth + " " + selectedYear + " monthly sales.csv";
                    break ;
                case 2:
                    dirPdf += @"\" + selectedYear + " yearly sales.csv";
                    break;
            }

            Dictionary<string, DateClass> dates = prevelantClass.LoadDatesSales();
            Dictionary<string, SourceDocumentClass2> receiptsArray = new();
            switch (dateRange)
            {
                case 0://daily
                    if (dates == null)
                        break;
                    if (!dates.ContainsKey(dateSelect))
                        break;
                    receiptsArray = dates[dateSelect].receipts;
                    break;
                case 1://monthly
                    if (dates == null)
                        break;
                    foreach (var date in dates)
                    {
                        if(date.Value.month == Convert.ToString(selectedMonth))
                        {
                            if(date.Value.receipts != null)
                            {
                                foreach(var invoice in date.Value.receipts)
                                {
                                    receiptsArray.Add(invoice.Key, invoice.Value);
                                }
                            }
                        }
                    }
                    break;
                case 2://yearly
                    if (dates == null)
                        break;
                    foreach (var date in dates)
                    {
                        if (date.Value.year == Convert.ToString(selectedYear))
                        {
                            if (date.Value.receipts != null)
                            {
                                foreach (var invoice in date.Value.receipts)
                                {
                                    receiptsArray.Add(invoice.Key, invoice.Value);
                                }
                            }
                        }
                    }
                    break;
            }

            Response res = new Response();
            res.responseText = dirPdf;
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(dirPdf,
                FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine("sep=,");
                    writer.WriteLine("Date, Details, Folio, Dr, Cr, Vat");
                    foreach (var item in receiptsArray)
                    {
                        string folio = item.Value.folio;
                        folio = folio.Replace(",", "");
                        string line = item.Value.day + "/" + item.Value.month + "/" + item.Value.year + "," + "Sales" + "," + item.Value.folio + "," + item.Value.total + "," + "" + "," + item.Value.exclVat;
                        writer.WriteLine(line);
                        if (item.Value.matchingReturns == "true")
                        {
                            string line2 = item.Value.day + "/" + item.Value.month + "/" + item.Value.year + "," + "Sales Return" + "," + item.Value.folio + "," + "" + "," + item.Value.total + ",(" + item.Value.exclVat + ")";
                            writer.WriteLine(line2);
                        }
                    }
                    //add a balance bd?
                }
                success = true;
            }
            catch (Exception ex)
            {
                res.responseText = ex.Message;
            }
            res.success = success;
            control.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               method, res);
        }

        public void generateSalesDaybookCSV (int dateRange, string dateSelect, int selectedDay, int selectedMonth, int selectedYear, TextBlock control, Action<Response> method)
        {
            bool success = false;
            string dirPdf = localDirectory.SalesReportsFolder();
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }

            Dictionary<string, DateClass> dates = prevelantClass.LoadDatesCredit();
            Dictionary<string, SourceDocumentClass2> receiptsArray = new();
            switch (dateRange)
            {
                case 0://daily
                    if (dates == null)
                        break;
                    if (!dates.ContainsKey(dateSelect))
                        break;
                    receiptsArray = dates[dateSelect].receipts;
                    break;
                case 1://monthly
                    if (dates == null)
                        break;
                    foreach (var date in dates)
                    {
                        if (date.Value.month == Convert.ToString(selectedMonth))
                        {
                            if (date.Value.invoices != null)
                            {
                                foreach (var invoice in date.Value.invoices)
                                {
                                    receiptsArray.Add(invoice.Key, invoice.Value);
                                }
                            }
                        }
                    }
                    break;
                case 2://yearly
                    if (dates == null)
                        break;
                    foreach (var date in dates)
                    {
                        if (date.Value.year == Convert.ToString(selectedYear))
                        {
                            if (date.Value.invoices != null)
                            {
                                foreach (var invoice in date.Value.invoices)
                                {
                                    receiptsArray.Add(invoice.Key, invoice.Value);
                                }
                            }
                        }
                    }
                    break;
            }
            switch (dateRange)
            {
                case 0:
                    dirPdf += @"\" + selectedDay + ", " + selectedMonth + " " + selectedYear + " daily credit sales.csv";
                    break;
                case 1:
                    dirPdf += @"\" + selectedMonth + " " + selectedYear + " monthly credit sales.csv";
                    break;
                case 2:
                    dirPdf += @"\" + selectedYear + " yearly credit sales.csv";
                    break;
            }


            Response res = new Response();
            res.responseText = dirPdf;
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(dirPdf,
                FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine("sep=,");
                    writer.WriteLine("Date, Details, Folio, Dr, Cr, Vat");
                    foreach (var item in receiptsArray)
                    {
                        string folio = item.Value.folio;
                        if (item.Value.folio != null)
                        {
                            folio = folio.Replace(",", "");
                        }
                        string line = item.Value.day + "/" + item.Value.month + "/" + item.Value.year + "," + item.Value.customerName + "," + folio + "," + "" + "," + item.Value.total + "," + item.Value.exclVat;
                        writer.WriteLine(line);
                        if (item.Value.matchingReturns == "true")
                        {
                            string line2 = item.Value.day + "/" + item.Value.month + "/" + item.Value.year + "," + "Sales Return" + "," + folio + "," + item.Value.total + "," + "" + ",(" + item.Value.exclVat + ")";
                            writer.WriteLine(line2);
                        }
                    }
                }
                success = true;
            }
            catch (Exception ex)
            {
                res.responseText = ex.Message;
            }
            res.success = success;
            control.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               method, res);
        }

        public void generatePurchasesDayBookCSV(int dateRange, string dateSelect, int selectedDay, int selectedMonth, int selectedYear, TextBlock control, Action<Response> method)
        {
            bool success = false;
            string dirPdf = localDirectory.PurchasesReportsFolder();
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }
            switch (dateRange)
            {
                case 0:
                    dirPdf += @"\" + selectedDay + ", " + selectedMonth + " " + selectedYear + " daily credit purchases.csv";
                    break;
                case 1:
                    dirPdf += @"\" + selectedMonth + " " + selectedYear + " monthly credit purchases.csv";
                    break;
                case 2:
                    dirPdf += @"\" + selectedYear + " yearly credit purchases.csv";
                    break;
            }

            Dictionary<string, DateClass> dates = prevelantClass.LoadDatesPurchasesCredit();
            Dictionary<string, SourceDocumentClass2> receiptsArray = new();
            switch (dateRange)
            {
                case 0://daily
                    if (dates == null)
                        break;
                    if (!dates.ContainsKey(dateSelect))
                        break;
                    receiptsArray = dates[dateSelect].sd;
                    break;
                case 1://monthly
                    if (dates == null)
                        break;
                    foreach (var date in dates)
                    {
                        if (date.Value.month == Convert.ToString(selectedMonth))
                        {
                            if (date.Value.sd != null)
                            {
                                foreach (var invoice in date.Value.sd)
                                {
                                    receiptsArray.Add(invoice.Key, invoice.Value);
                                }
                            }
                        }
                    }
                    break;
                case 2://yearly
                    if (dates == null)
                        break;
                    foreach (var date in dates)
                    {
                        if (date.Value.year == Convert.ToString(selectedYear))
                        {
                            if (date.Value.sd != null)
                            {
                                foreach (var invoice in date.Value.sd)
                                {
                                    receiptsArray.Add(invoice.Key, invoice.Value);
                                }
                            }
                        }
                    }
                    break;
            }

            Response res = new Response();
            res.responseText = dirPdf;
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(dirPdf,
                FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine("sep=,");
                    writer.WriteLine("Date, Details, Folio, Dr, Cr, Vat");
                    foreach (var item in receiptsArray)
                    {
                        string folio = item.Value.folio;
                        folio = folio.Replace(",", "");
                        string line = item.Value.day + "/" + item.Value.month + "/" + item.Value.year + "," + "Sales" + "," + item.Value.folio + "," + item.Value.total + "," + "" + "," + item.Value.exclVat;
                        writer.WriteLine(line);
                        if (item.Value.matchingReturns == "true")
                        {
                            string line2 = item.Value.day + "/" + item.Value.month + "/" + item.Value.year + "," + "Sales Return" + "," + item.Value.folio + "," + "" + "," + item.Value.total + ",(" + item.Value.exclVat + ")";
                            writer.WriteLine(line2);
                        }
                    }
                    //add a balance bd?
                }
                success = true;
            }
            catch (Exception ex)
            {
                res.responseText = ex.Message;
            }
            res.success = success;
            control.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               method, res);
        }

        public void generatePurchasesAccountCSV(int dateRange, string dateSelect, int selectedDay, int selectedMonth, int selectedYear, TextBlock control, Action<Response> method)
        {
            bool success = false;
            string dirPdf = localDirectory.PurchasesReportsFolder();
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }

            Dictionary<string, DateClass> dates = prevelantClass.LoadDatesPurchases();
            Dictionary<string, SourceDocumentClass2> receiptsArray = new();
            switch (dateRange)
            {
                case 0://daily
                    if (dates == null)
                        break;
                    if (!dates.ContainsKey(dateSelect))
                        break;
                    receiptsArray = dates[dateSelect].sd;
                    break;
                case 1://monthly
                    if (dates == null)
                        break;
                    foreach (var date in dates)
                    {
                        if (date.Value.month == Convert.ToString(selectedMonth))
                        {
                            if (date.Value.sd != null)
                            {
                                foreach (var invoice in date.Value.sd)
                                {
                                    receiptsArray.Add(invoice.Key, invoice.Value);
                                }
                            }
                        }
                    }
                    break;
                case 2://yearly
                    if (dates == null)
                        break;
                    foreach (var date in dates)
                    {
                        if (date.Value.year == Convert.ToString(selectedYear))
                        {
                            if (date.Value.sd != null)
                            {
                                foreach (var invoice in date.Value.sd)
                                {
                                    receiptsArray.Add(invoice.Key, invoice.Value);
                                }
                            }
                        }
                    }
                    break;
            }
            switch (dateRange)
            {
                case 0:
                    dirPdf += @"\" + selectedDay + ", " + selectedMonth + " " + selectedYear + " daily purchases.csv";
                    break;
                case 1:
                    dirPdf += @"\" + selectedMonth + " " + selectedYear + " monthly purchases.csv";
                    break;
                case 2:
                    dirPdf += @"\" + selectedYear + " yearly purchases.csv";
                    break;
            }


            Response res = new Response();
            res.responseText = dirPdf;
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(dirPdf,
                FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine("sep=,");
                    writer.WriteLine("Date, Details, Folio, Dr, Cr, Vat");
                    foreach (var item in receiptsArray)
                    {
                        string folio = item.Value.folio;
                        if (item.Value.folio != null)
                        {
                            folio = folio.Replace(",", "");
                        }
                        string line = item.Value.day + "/" + item.Value.month + "/" + item.Value.year + "," + item.Value.customerName + "," + folio + "," + "" + "," + item.Value.total + "," + item.Value.exclVat;
                        writer.WriteLine(line);
                        if (item.Value.matchingReturns == "true")
                        {
                            string line2 = item.Value.day + "/" + item.Value.month + "/" + item.Value.year + "," + "Purchases Return" + "," + folio + "," + item.Value.total + "," + "" + ",(" + item.Value.exclVat + ")";
                            writer.WriteLine(line2);
                        }
                    }
                }
                success = true;
            }
            catch (Exception ex)
            {
                res.responseText = ex.Message;
            }
            res.success = success;
            control.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               method, res);
        }

        public void generateReceivablesCSV (TextBlock control, Action<Response> method)
        {
            Dictionary<string, SourceDocumentClass2> receiptsArray = prevelantClass.LoadDataAllInvoices();
            bool success = false;
            string dirPdf = localDirectory.ReceivablesReportsFolder();
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }
            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            string date = dt.ToString("dd, MMM yyyy");
            dirPdf += @"\" + date + " Receivables.csv";


            Response res = new Response();
            res.responseText = dirPdf;
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(dirPdf,
                FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine("sep=,");
                    writer.WriteLine("Date, Time, Due Date, Customer Name, Phone number, Folio, Sum,");
                    foreach (var item in receiptsArray)
                    {
                        if(!item.Value.paid && item.Value.matchingReturns == "false")
                        {
                            string folio = item.Value.folio;
                            if (item.Value.folio != null)
                            {
                                folio = folio.Replace(",", "");
                            }

                            string line = item.Value.day + "/" + item.Value.month + "/" + item.Value.year + "," + item.Value.time + "," + item.Value.vday + "/" + item.Value.vmonth + "/" + item.Value.vyear + "," + item.Value.customerName + "," + item.Value.customerPhoneNumber + "," + folio + "," + item.Value.total;
                            writer.WriteLine(line);
                        }
                    }
                }
                success = true;
            }
            catch (Exception ex)
            {
                res.responseText = ex.Message;
            }
            res.success = success;
            control.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               method, res);
        }

        public void generateQuotationsCSV(string selector, TextBlock control, Action<Response> method)
        {
            SDServicesClass sDServices = new SDServicesClass();

            Dictionary<string, SourceDocumentClass2> quotationsArray = prevelantClass.LoadDataQuotations();
            Dictionary<string, SourceDocumentClass2> allQuotationsArray = new();
            if (quotationsArray != null) if (quotationsArray.Count > 0)
                {
                    foreach (var quotation in quotationsArray)
                    {
                        SourceDocumentClass2 quote = quotation.Value;
                        DateTime dt = DateTime.Now;
                        int month = dt.Month;
                        int year = dt.Year;
                        int daySele = dt.Day;

                        try
                        {
                            if (selector == "Valid")
                            {
                                if (Convert.ToInt32(quote.vyear) > year)
                                {
                                    allQuotationsArray.Add(quotation.Key, quotation.Value);
                                }
                                else if (Convert.ToInt32(quote.vyear) == year)
                                {
                                    if (Convert.ToInt32(quote.vmonth) > month)
                                    {
                                        allQuotationsArray.Add(quotation.Key, quotation.Value);
                                    }
                                    else if (Convert.ToInt32(quote.vmonth) > month)
                                    {
                                        if (Convert.ToInt32(quote.vday) >= daySele)
                                        {
                                            allQuotationsArray.Add(quotation.Key, quotation.Value);
                                        }
                                    }
                                }
                            }
                            else if (selector == "Expired")
                            {
                                if (Convert.ToInt32(quote.vyear) < year)
                                {
                                    allQuotationsArray.Add(quotation.Key, quotation.Value);
                                }
                                else if (Convert.ToInt32(quote.vyear) == year)
                                {
                                    if (Convert.ToInt32(quote.vmonth) < month)
                                    {
                                        allQuotationsArray.Add(quotation.Key, quotation.Value);
                                    }
                                    else if (Convert.ToInt32(quote.vmonth) < month)
                                    {
                                        if (Convert.ToInt32(quote.vday) <= daySele)
                                        {
                                            allQuotationsArray.Add(quotation.Key, quotation.Value);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }

            bool success = false;
            string dirPdf = localDirectory.QuotationsReportsFolder();
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }
            DateTime dt1 = DateTime.Now;
            string date = dt1.ToString("dd, MMM yyyy");
            dirPdf += @"\" + date + " " + selector + " quotations.csv";


            Response res = new Response();
            res.responseText = dirPdf;
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(dirPdf,
                FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine("sep=,");
                    writer.WriteLine("Date, Time, Expiry Date, Customer Name, Phone number, Folio, Sum");
                    foreach (var item in allQuotationsArray)
                    {
                        string folio = item.Value.folio;
                        if (item.Value.folio != null)
                        {
                            folio = folio.Replace(",", "");
                        }
                        string line = item.Value.day + "/" + item.Value.month + "/" + item.Value.year + "," + item.Value.time + "," + item.Value.vday + "/" + item.Value.vmonth + "/" + item.Value.vyear + "," + item.Value.customerName + "," + item.Value.customerPhoneNumber + "," + folio + "," + item.Value.total;
                        writer.WriteLine(line);
                    }
                }
                success = true;
            }
            catch (Exception ex)
            {
                res.responseText = ex.Message;
            }
            res.success = success;
            control.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               method, res);
        }

        public void generateInventoryTemplate(TextBlock control, Action<Response> method)
        {
            bool success = false;
            string dirPdf = localDirectory.InventoryReportsFolder();
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }
            dirPdf += @"\" + "InventoryTemplate.csv";

            Response res = new Response();
            res.responseText = dirPdf;
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(dirPdf,
                FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine("sep=,");
                    writer.WriteLine("Product ID, Barcode, Product Name, Brand, Specification, Size, Category, Price, VAT, Inventory Tracking, Quantity, Buffer");
                }
                success = true;
            }
            catch (Exception ex)
            {
                res.responseText = ex.Message;
            }
            res.success = success;
            control.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               method, res);
        }

        public void generateInventoryCSV(string category, TextBlock control, Action<Response> method)
        {
            Dictionary<string, ProductClass> productsArray = prevelantClass.LoadDataProducts();
            bool success = false;
            string dirPdf = localDirectory.InventoryReportsFolder();
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }
            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            string date = dt.ToString("dd, MMM yyyy");
            if (category == "All Categories")
            {
                dirPdf += @"\" + date + " Inventory.csv";
            } 
            else
            {
                dirPdf += @"\" + date + " " + category + " Inventory.csv";
            }

            //currency
            settingsClass = prevelantClass.RetrieveSettings();
            if (settingsClass != null) if (settingsClass.bookKeepingSettings != null) currency = settingsClass.bookKeepingSettings.currency;
            Response res = new Response();
            res.responseText = dirPdf;
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(dirPdf,
                FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine("sep=,");
                    writer.WriteLine("Product ID, Barcode, Product Name, Brand, Specification, Size, Category, Price ("+ currency +"), VAT, Inventory Tracking, Quantity, Buffer");
                    foreach (var item in productsArray)
                    {

                        decimal pric = currencyClass.Total(item.Value.price);

                        string pid = item.Value.pid.Replace(",", "");
                        string barcode = item.Value.barcode.Replace(",", "");
                        string productName = item.Value.productName.Replace(",", "");
                        string brandName = item.Value.brandName.Replace(",", "");
                        string flavor = item.Value.flavor.Replace(",", "");
                        string size = item.Value.size.Replace(",", "");
                        string category1 = item.Value.category.Replace(",", "");
                        string price = pric.ToString().Replace(",", "");
                        string qtyInStock = item.Value.qtyInStock.Replace(",", "");
                        string bufferQty = item.Value.bufferQty.Replace(",", "");

                        int tracking = 0;
                        if(item.Value.inventoryTracking == "on")
                        {
                            tracking = 1;
                        }

                        if (category == "All Categories")
                        {
                            string line = pid + "," + barcode + "," + productName + "," + brandName + "," + flavor + "," + size + "," + category1 + "," + price + "," + item.Value.vatType + "," + tracking.ToString() + "," + qtyInStock + "," + bufferQty;
                            writer.WriteLine(line);
                        } else if (item.Value.category == category)
                        {
                            string line = pid + "," + barcode + "," + productName + "," + brandName + "," + flavor + "," + size + "," + category1 + "," + price + "," + item.Value.vatType + "," + tracking.ToString() + "," + qtyInStock + "," + bufferQty;
                            writer.WriteLine(line);
                        }
                    }
                }
                success = true;
            }
            catch (Exception ex)
            {
                res.responseText = ex.Message;
            }
            res.success = success;
            control.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               method, res);
        }

        public void generateEmployeesyCSV(TextBlock control, Action<Response> method)
        {
            bool success = false;
            Dictionary<string, EmployeeClass> employeesArray = prevelantClass.LoadDataEmployees();
            string dirPdf = localDirectory.EmployeeReportsFolder();
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }
            DateTime dt = DateTime.Now;
            int month = dt.Month;
            int year = dt.Year;
            int daySele = dt.Day;
            string date = dt.ToString("dd, MMM yyyy");
            dirPdf += @"\" + date + " employees.csv";


            Response res = new Response();
            res.responseText = dirPdf;
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(dirPdf,
                FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine("sep=,");
                    writer.WriteLine("Employee ID, First Name, Last Name, Phone Number, Email, Access Level, Role");
                    foreach (var item in employeesArray)
                    {
                        if(item.Value.position != "")
                        {
                            string line = item.Value.eid + "," + item.Value.firstName + "," + item.Value.lastName + ",'" + item.Value.phoneNumber + "'," + item.Value.email + "," + item.Value.accessLevel + "," + item.Value.position;
                            writer.WriteLine(line);
                        }
                    }
                }
                success = true;
            }
            catch (Exception ex)
            {
                res.responseText = ex.Message;
            }
            res.success = success;
            control.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               method, res);
        }
    }
}
