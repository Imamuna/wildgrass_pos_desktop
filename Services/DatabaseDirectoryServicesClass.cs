using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildGrass_Desktop;
using WildGrassPOSLibrary.Services;

namespace WildGrass_Desktop_f8.Services
{
    internal class DatabaseDirectoryServicesClass
    {
        PrevalentClass prevelantClass; 

        string uid;
        string bid;
        string eid;
        string prefix;
        bool isSandbox;

        public DatabaseDirectoryServicesClass()
        {
            prevelantClass = new();
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();
            prefix = prevelantClass.getPrefix();
            isSandbox = prevelantClass.getSandbox();
        }

        public DatabaseDirectoryServicesClass(string uid, string bid, string eid, bool isSandbox)
        {
            this.uid=uid;
            this.bid=bid;
            this.eid=eid;
            this.isSandbox=isSandbox;
            prevelantClass = new();
            prefix = prevelantClass.getPrefix();
        }



        //personal data
        public string UsersVersions()
        {
            return prefix + "/" + @"Users/" + uid + "/Versions/";
        }

        public string FPCSUser()
        {
            return prefix + "/" + @"FPCS/Users/";
        }

        public string Users()
        {
            return prefix + "/" + @"Users/";
        }

        //system data

        public string SystemResources()
        {
            return prefix + "/" + @"system resources/";
        }

        public string ClientSds()
        {
            return prefix + "/" + @"Client/";
        }

        //sandbox fixed
        public string SandboxUsers()
        {
            return prefix + "/sandbox/Users/";
        }
        public string SandboxBusinessData()
        {
            return prefix + "/sandbox/BusinessesData/";
        }
        public string SandboxBusinessDetails()
        {
            return prefix + "/sandbox/Users/" + uid + @"/BusinessesDetails/";
        }

        //business data
        public string BusinessData()
        {
            string dir = "/";
            if (isSandbox)
            {
                dir = "/sandbox/";
            }
            return prefix + dir + @"BusinessesData/";
        }
        public string BusinessDetails()
        {
            string dir = "/";
            if (isSandbox)
            {
                dir = "/sandbox/";
            }
            return prefix + dir + @"Users/" + uid + @"/BusinessesDetails/";
        }

        public string TransactionLog()
        {
            return BusinessData() + bid + "/Pay/TransactionLog/";
        }

        public string Folios()
        {
            return BusinessData() + bid + "/folioNumbers/";
        }

        public string Receipts()
        {
            return BusinessData() + bid + "/Receipts/";
        }
        public string AllReceipts()
        {
            return BusinessData() + bid + "/All Receipts/";
        }
        public string Quotations()
        {
            return BusinessData() + bid + "/Quotations/";
        }
        public string Invoices()
        {
            return BusinessData() + bid + "/Invoices/";
        }
        public string AllInvoices()
        {
            return BusinessData() + bid + "/All Invoices/";
        }
        public string Orders()
        {
            return BusinessData() + bid + "/Orders/";
        }
        public string TransferNotes()
        {
            return BusinessData() + bid + "/TransferNotes/";
        }

        public string OnGoing()
        {
            return BusinessData() + bid + "/employees/" + eid + "/onGoing/";
        }

        public string OnGoingVersion()
        {
            return prefix + "/" + @"test/Users/" + uid + "/businesses/" + bid + "/employees/" + eid + "/onGoingVersion/";
        }

        public string Products()
        {
            return BusinessData() + bid + "/products/products/";
        }

        public string Categories()
        {
            return BusinessData() + bid + "/products/categories/";
        }

        public string Expenses()
        {
            //born after Multi-branch Support (AfterMultiBranchSupport AMBS)
            return BusinessData() + bid + "/expenses/expenses/";
        }

        public string Taxes()
        {
            //born after Multi-branch Support (AfterMultiBranchSupport AMBS)
            return BusinessData() + bid + "/taxes/";
        }

        public string ExpenseCategories()
        {
            //born after Multi-branch Support (AfterMultiBranchSupport AMBS)
            return BusinessData() + bid + "/expenses/categories/";
        }

        public string Tiers()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/products/tiers";
            return BusinessData() + bid + "/products/tiers/";
        }

        public string Customers()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Customers";
            return BusinessData() + bid + "/Customers/";
        }

        public string Suppliers()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Suppliers";
            return BusinessData() + bid + "/Suppliers/";
        }

        public string ReceiptsIn()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/ReceiptIn";
            return BusinessData() + bid + "/ReceiptIn/";
        }

        public string InvoicesIn()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/InvoiceIn";
            return BusinessData() + bid + "/InvoiceIn/";
        }

        public string AllReceiptsIn()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/All ReceiptIn";
            return BusinessData() + bid + "/All ReceiptIn/";
        }

        public string AllInvoicesIn()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/All InvoiceIn";
            return BusinessData() + bid + "/All InvoiceIn/";
        }

        public string CreditNotes()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Credit Note";
            return BusinessData() + bid + "/Credit Note/";
        }

        public string DebitNote()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Debit Note";
            return BusinessData() + bid + "/Debit Note/";
        }

        public string QuantityChange()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/QuantityChange";
            return BusinessData() + bid + "/QuantityChange/";
        }

        public string Employees()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/employees";
            return BusinessData() + bid + "/employees/";
        }

        public string Employees(string bid)
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/employees";
            return BusinessData() + bid + "/employees/";
        }

        public string FPCSEmployees()
        {
            //born after Multi-branch Support (AfterMultiBranchSupport AMBS)
            return BusinessDetails() + bid + "/employees/";
        }

        public string PackageManager()
        {
            return BusinessData() + bid + "/subscription/";
        }

        public string Settings()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/settings";
            return BusinessData() + bid + "/settings/";
        }

        public string Notifications()
        {
            //return @"Users/" + uid + "/businesses/" + bid + "/Notifications";
            return BusinessData() + bid + "/Notifications/";
        }

        public string FPCSBusinesses()
        {
            return prefix + "/" + @"FPCS/Users/" + uid + "/businesses/";
        }

        public string Businesses()
        {
            //return @"Users/" + uid + "/businesses";
            return BusinessDetails();
        }

        public string Versions()
        {
            return BusinessDetails() + bid + "/Versions/";
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
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/folioNumbers/";
        }

        public string ClientSds_old()
        {
            return prefix + "/" + @"Client/";
        }

        public string Receipts_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/Receipts/";
        }
        public string AllReceipts_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/All Receipts/";
        }
        public string Quotations_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/Quotaions/";
        }
        public string Invoices_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/Invoices/";
        }
        public string AllInvoices_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/All Invoices/";
        }
        public string Orders_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/Orders/";
        }
        public string TransferNotes_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/TransferNotes/";
        }

        public string OnGoing_old()
        {
            return prefix + "/" + @"test/Users/" + uid + "/businesses/" + bid + "/employees/" + eid + "/onGoing/";
        }

        public string OnGoingVersion_old()
        {
            return prefix + "/" + @"test/Users/" + uid + "/businesses/" + bid + "/employees/" + eid + "/onGoingVersion/";
        }

        public string Products_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/products/products/";
        }

        public string Categories_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/products/categories/";
        }

        public string Tiers_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/products/tiers/";
        }

        public string Customers_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/Customers/";
        }

        public string Suppliers_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/Suppliers/";
        }

        public string ReceiptsIn_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/ReceiptIn/";
        }

        public string InvoicesIn_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/InvoiceIn/";
        }

        public string AllReceiptsIn_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/All ReceiptIn/";
        }

        public string AllInvoicesIn_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/All InvoiceIn/";
        }

        public string CreditNotes_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/Credit Note/";
        }

        public string DebitNote_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/Debit Note/";
        }

        public string QuantityChange_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/QuantityChange/";
        }

        public string Employees_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/employees/";
        }

        public string Settings_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/settings/";
        }

        public string Notifications_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/" + bid + "/Notifications/";
        }

        public string FPCSBusinesses_old()
        {
            return prefix + "/" + @"FPCS/Users/" + uid + "/businesses/";
        }

        public string Businesses_old()
        {
            return prefix + "/" + @"Users/" + uid + "/businesses/";
        }

        public string Versions_old()
        {
            return prefix + "/" + @"FPCS/Users/" + uid + "/businesses/" + bid + "/Versions/";
        }

        public string FPCSUser_old()
        {
            return prefix + "/" + @"FPCS/Users/";
        }

        public string Users_old()
        {
            return prefix + "/" + @"Users/";
        }

        public string SystemResources_old()
        {
            return prefix + "/" + @"system resources/";
        }
    }
}
