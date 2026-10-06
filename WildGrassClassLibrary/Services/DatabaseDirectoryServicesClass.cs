using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildGrassClassLibrary.Services
{
    internal class DatabaseDirectoryServicesClass
    {

        string uid;
        string bid;
        string eid;

        public string Folios()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/folioNumbers"; 
            return @"BusinessesData/" + bid + "/folioNumbers"; 
        }

        public string ClientSds()
        {
            return @"Client";
        }

        public string Receipts()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Receipts";
            return @"BusinessesData/" + bid + "/Receipts";
        }
        public string AllReceipts()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/All Receipts";
            return @"BusinessesData/" + bid + "/All Receipts";
        }
        public string Quotations()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Quotaions/";
            return @"BusinessesData/" + bid + "/Quotations/";
        }
        public string Invoices()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Invoices";
            return @"BusinessesData/" + bid + "/Invoices";
        }
        public string AllInvoices()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/All Invoices";
            return @"BusinessesData/" + bid + "/All Invoices";
        }
        public string Orders()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Orders";
            return @"BusinessesData/" + bid + "/Orders";
        }
        public string TransferNotes()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/TransferNotes";
            return @"BusinessesData/" + bid + "/TransferNotes";
        }

        public string OnGoing()
        {
            //return @"test/Users/" + uid + "/businesses/" + bid + "/employees/" + eid + "/onGoing";
            return @"BusinessesData/" + bid + "/employees/" + eid + "/onGoing";
        }

        public string OnGoingVersion()
        {
            return @"test/Users/" + uid + "/businesses/" + bid + "/employees/" + eid + "/onGoingVersion";
        }

        public string Products()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/products/products";
            return @"BusinessesData/" + bid + "/products/products";
        }

        public string Categories()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/products/categories";
            return @"BusinessesData/" + bid + "/products/categories";
        }

        public string Expenses()
        {
            //born after Multi-branch Support (AfterMultiBranchSupport AMBS)
            return @"BusinessesData/" + bid + "/expenses/expenses";
        }

        public string ExpenseCategories()
        {
            //born after Multi-branch Support (AfterMultiBranchSupport AMBS)
            return @"BusinessesData/" + bid + "/expenses/categories";
        }

        public string Tiers()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/products/tiers";
            return @"BusinessesData/" + bid + "/products/tiers";
        }

        public string Customers()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Customers";
            return @"BusinessesData/" + bid + "/Customers";
        }

        public string Suppliers()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Suppliers";
            return @"BusinessesData/" + bid + "/Suppliers";
        }

        public string ReceiptsIn()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/ReceiptIn";
            return @"BusinessesData/" + bid + "/ReceiptIn";
        }

        public string InvoicesIn()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/InvoiceIn";
            return @"BusinessesData/" + bid + "/InvoiceIn";
        }

        public string AllReceiptsIn()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/All ReceiptIn";
            return @"BusinessesData/" + bid + "/All ReceiptIn";
        }

        public string AllInvoicesIn()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/All InvoiceIn";
            return @"BusinessesData/" + bid + "/All InvoiceIn";
        }

        public string CreditNotes()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Credit Note";
            return @"BusinessesData/" + bid + "/Credit Note";
        }

        public string DebitNote()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Debit Note";
            return @"BusinessesData/" + bid + "/Debit Note";
        }

        public string QuantityChange()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/QuantityChange";
            return @"BusinessesData/" + bid + "/QuantityChange";
        }

        public string Employees()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/employees";
            return @"BusinessesData/" + bid + "/employees";
        }

        public string PackageManager()
        {
            return @"BusinessesData/" + bid + "/subscription";
        }

        public string Settings()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/settings";
            return @"BusinessesData/" + bid + "/settings";
        }

        public string Notifications()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Notifications";
            return @"BusinessesData/" + bid + "/Notifications";
        }

        public string FPCSBusinesses()
        {
            return @"FPCS/Users/" + uid + "/businesses";
        }

        public string Businesses()
        {
            //return @"Users/" + uid + "/businesses";
            return @"Users/" + uid + @"/BusinessesDetails";
        }

        public string Versions()
        {
            return @"Users/" + uid + @"/BusinessesDetails/" + bid + "/Versions";
        }

        public string UsersVersions()
        {
            return @"Users/" + uid + "/Versions";
        }

        public string FPCSUser()
        {
            return @"FPCS/Users";
        }

        public string Users()
        {
            return @"Users";
        }

        public string SystemResources()
        {
            return @"system resources";
        }

        //now we upload our invoice to both refernces
        //FirebaseResponse firebaseA = await client.SetAsync(@"Businesses/" + bid + "Data/Invoices/" + entry.date + "/invoices/" + entry.folio, entry);
        //FirebaseResponse firebase2A = await client.SetAsync(@"Businesses/" + bid + "Data/Invoices/" + entry.date + "/date", dateEntry.date);
        //FirebaseResponse firebase3A = await client.SetAsync(@"Businesses/" + bid + "Data/Invoices/" + entry.date + "/comp", dateEntry.comp);
        //FirebaseResponse firebase4A = await client.SetAsync(@"Businesses/" + bid + "Data/Invoices/" + entry.date + "/day", dateEntry.day);
        //FirebaseResponse firebase5A = await client.SetAsync(@"Businesses/" + bid + "Data/Invoices/" + entry.date + "/month", dateEntry.month);
        //FirebaseResponse firebase6A = await client.SetAsync(@"Businesses/" + bid + "Data/Invoices/" + entry.date + "/year", dateEntry.year);
        //FirebaseResponse firebase1A = await client.SetAsync(@"Businesses/" + bid + "Data/All Invoices/" + entry.folio, entry);


        public string Folios_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/folioNumbers";
        }

        public string ClientSds_old()
        {
            return @"Client";
        }

        public string Receipts_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/Receipts";
        }
        public string AllReceipts_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/All Receipts";
        }
        public string Quotations_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/Quotaions/";
        }
        public string Invoices_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/Invoices";
        }
        public string AllInvoices_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/All Invoices";
        }
        public string Orders_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/Orders";
        }
        public string TransferNotes_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/TransferNotes";
        }

        public string OnGoing_old()
        {
            return @"test/Users/" + uid + "/businesses/" + bid + "/employees/" + eid + "/onGoing";
        }

        public string OnGoingVersion_old()
        {
            return @"test/Users/" + uid + "/businesses/" + bid + "/employees/" + eid + "/onGoingVersion";
        }

        public string Products_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/products/products";
        }

        public string Categories_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/products/categories";
        }

        public string Tiers_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/products/tiers";
        }

        public string Customers_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/Customers";
        }

        public string Suppliers_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/Suppliers";
        }

        public string ReceiptsIn_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/ReceiptIn";
        }

        public string InvoicesIn_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/InvoiceIn";
        }

        public string AllReceiptsIn_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/All ReceiptIn";
        }

        public string AllInvoicesIn_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/All InvoiceIn";
        }

        public string CreditNotes_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/Credit Note";
        }

        public string DebitNote_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/Debit Note";
        }

        public string QuantityChange_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/QuantityChange";
        }

        public string Employees_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/employees";
        }

        public string Settings_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/settings";
        }

        public string Notifications_old()
        {
            return @"Users/" + uid + "/businesses/" + bid + "/Notifications";
        }

        public string FPCSBusinesses_old()
        {
            return @"FPCS/Users/" + uid + "/businesses";
        }

        public string Businesses_old()
        {
            return @"Users/" + uid + "/businesses";
        }

        public string Versions_old()
        {
            return @"FPCS/Users/" + uid + "/businesses/" + bid + "/Versions";
        }

        public string FPCSUser_old()
        {
            return @"FPCS/Users";
        }

        public string Users_old()
        {
            return @"Users";
        }

        public string SystemResources_old()
        {
            return @"system resources";
        }
    }
}
