using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WildGrassPOSLibrary.Models;

namespace WildGrass_Desktop_f8.Functions
{
    internal class GenerateSpreadSheetClass
    {
        public void generateExcelSpreadSheet(Dictionary<string, ProductClass> productsArray, string categorySelected, System.Windows.Controls.Button control, Action<bool> method)
        {
            bool success = false;
            if (productsArray != null)
            {
                if (productsArray.Count > 0)
                {
                    Microsoft.Office.Interop.Excel.Application xcelApp = new Microsoft.Office.Interop.Excel.Application();
                    //xcelApp.Application.Workbooks.Add(Type.Missing);

                    Workbook wb = xcelApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
                    Worksheet ws = (Worksheet)wb.Worksheets[1];

                    int i = 1;
                    //create the headers we need
                    ws.Range[i, 1].Value = "Product ID";
                    ws.Range[i, 2].Value = "BarCode";
                    ws.Range[i, 3].Value = "Product";
                    ws.Range[i, 4].Value = "Brand";
                    ws.Range[i, 5].Value = "Flavor/ Specification";
                    ws.Range[i, 6].Value = "Size";
                    ws.Range[i, 7].Value = "Price";
                    ws.Range[i, 8].Value = "VAT";
                    ws.Range[i, 9].Value = "Inventory tracking";
                    ws.Range[i, 10].Value = "Quantity";
                    ws.Range[i, 11].Value = "Buffer Quantity";

                    i++;
                    //add in the actual data
                    foreach (var item in productsArray)
                    {
                        if (categorySelected == "All Categories")
                        {
                            ws.Range[i, 1].Value = item.Value.pid;
                            ws.Range[i, 2].Value = item.Value.barcode;
                            ws.Range[i, 3].Value = item.Value.productName;
                            ws.Range[i, 4].Value = item.Value.brandName;
                            ws.Range[i, 5].Value = item.Value.flavor;
                            ws.Range[i, 6].Value = item.Value.size;
                            ws.Range[i, 7].Value = "K " + item.Value.price;
                            ws.Range[i, 8].Value = item.Value.vat;
                            ws.Range[i, 9].Value = item.Value.inventoryTracking;
                            ws.Range[i, 10].Value = item.Value.qtyInStock;
                            ws.Range[i, 11].Value = item.Value.bufferQty;
                            i++;
                        }
                        else if (categorySelected == "Tracking")
                        {
                            if (item.Value.inventoryTracking == "on")
                            {
                                ws.Range[i, 1].Value = item.Value.pid;
                                ws.Range[i, 2].Value = item.Value.barcode;
                                ws.Range[i, 3].Value = item.Value.productName;
                                ws.Range[i, 4].Value = item.Value.brandName;
                                ws.Range[i, 5].Value = item.Value.flavor;
                                ws.Range[i, 6].Value = item.Value.size;
                                ws.Range[i, 7].Value = "K " + item.Value.price;
                                ws.Range[i, 8].Value = item.Value.vat;
                                ws.Range[i, 9].Value = item.Value.inventoryTracking;
                                ws.Range[i, 10].Value = item.Value.qtyInStock;
                                ws.Range[i, 11].Value = item.Value.bufferQty;
                                i++;
                            }
                        }
                        else if (categorySelected == "Non Tracking")
                        {
                            if (item.Value.inventoryTracking != "on")
                            {
                                ws.Range[i, 1].Value = item.Value.pid;
                                ws.Range[i, 2].Value = item.Value.barcode;
                                ws.Range[i, 3].Value = item.Value.productName;
                                ws.Range[i, 4].Value = item.Value.brandName;
                                ws.Range[i, 5].Value = item.Value.flavor;
                                ws.Range[i, 6].Value = item.Value.size;
                                ws.Range[i, 7].Value = "K " + item.Value.price;
                                ws.Range[i, 8].Value = item.Value.vat;
                                ws.Range[i, 9].Value = item.Value.inventoryTracking;
                                ws.Range[i, 10].Value = item.Value.qtyInStock;
                                ws.Range[i, 11].Value = item.Value.bufferQty;
                                i++;
                            }
                        }
                        else
                        {
                            if (item.Value.category == categorySelected)
                            {
                                ws.Range[i, 1].Value = item.Value.pid;
                                ws.Range[i, 2].Value = item.Value.barcode;
                                ws.Range[i, 3].Value = item.Value.productName;
                                ws.Range[i, 4].Value = item.Value.brandName;
                                ws.Range[i, 5].Value = item.Value.flavor;
                                ws.Range[i, 6].Value = item.Value.size;
                                ws.Range[i, 7].Value = "K " + item.Value.price;
                                ws.Range[i, 8].Value = item.Value.vat;
                                ws.Range[i, 9].Value = item.Value.inventoryTracking;
                                ws.Range[i, 10].Value = item.Value.qtyInStock;
                                ws.Range[i, 11].Value = item.Value.bufferQty;
                                i++;
                            }
                        }
                    }
                    wb.Save();
                    //xcelApp.Columns.AutoFit();
                    //xcelApp.Visible = true;
                    success = true;
                }
            }
            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void generateExcelSpreadSheet(Dictionary<string, ReceiptClass> receiptsArray, TextBlock control, Action<bool> method)
        {
            bool success = false;
            if (receiptsArray != null)
            {
                if (receiptsArray.Count > 0)
                {
                    Microsoft.Office.Interop.Excel.Application xcelApp = new Microsoft.Office.Interop.Excel.Application();
                    xcelApp.Application.Workbooks.Add(Type.Missing);

                    int i = 1;
                    //create the headers we need 
                    xcelApp.Cells[i, 1] = "Date";
                    xcelApp.Cells[i, 2] = "Time";
                    xcelApp.Cells[i, 3] = "Details";
                    xcelApp.Cells[i, 4] = "Folio";
                    xcelApp.Cells[i, 5] = "   Dr   ";
                    xcelApp.Cells[i, 6] = "   Cr   ";

                    i++;
                    //add in the actual data
                    foreach (var item in receiptsArray)
                    {
                        xcelApp.Cells[i, 1] = item.Value.date;
                        xcelApp.Cells[i, 2] = item.Value.time;
                        xcelApp.Cells[i, 3] = "Sales";
                        xcelApp.Cells[i, 4] = item.Value.folio;
                        xcelApp.Cells[i, 5] = item.Value.total;
                        xcelApp.Cells[i, 6] = "";
                        i++;
                        if (item.Value.matchingReturns == "true")
                        {
                            xcelApp.Cells[i, 1] = item.Value.date;
                            xcelApp.Cells[i, 2] = item.Value.time;
                            xcelApp.Cells[i, 3] = "Sales Returns";
                            xcelApp.Cells[i, 4] = item.Value.folio;
                            xcelApp.Cells[i, 5] = "";
                            xcelApp.Cells[i, 6] = item.Value.total;
                            i++;
                        }
                    }
                    xcelApp.Columns.AutoFit();
                    xcelApp.Visible = true;
                    success = true;
                }
            }
            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }

        public void generateExcelSpreadSheet(Dictionary<string, InvoiceClass> invoicesArray, TextBlock control, Action<bool> method)
        {
            bool success = false;
            if (invoicesArray != null)
            {
                if (invoicesArray.Count > 0)
                {
                    Microsoft.Office.Interop.Excel.Application xcelApp = new Microsoft.Office.Interop.Excel.Application();
                    xcelApp.Application.Workbooks.Add(Type.Missing);

                    int i = 1;
                    //create the headers we need 
                    xcelApp.Cells[i, 1] = "Date";
                    xcelApp.Cells[i, 2] = "Time";
                    xcelApp.Cells[i, 3] = "Details";
                    xcelApp.Cells[i, 4] = "Folio";
                    xcelApp.Cells[i, 5] = "   Dr   ";
                    xcelApp.Cells[i, 6] = "   Cr   ";

                    i++;
                    //add in the actual data
                    foreach (var item in invoicesArray)
                    {
                        xcelApp.Cells[i, 1] = item.Value.date;
                        xcelApp.Cells[i, 2] = item.Value.time;
                        xcelApp.Cells[i, 3] = item.Value.customerName;
                        xcelApp.Cells[i, 4] = item.Value.folio;
                        xcelApp.Cells[i, 5] = "";
                        xcelApp.Cells[i, 6] = item.Value.total;
                        i++;
                        if (item.Value.matchingReturns == "true")
                        {
                            xcelApp.Cells[i, 1] = item.Value.date;
                            xcelApp.Cells[i, 2] = item.Value.time;
                            xcelApp.Cells[i, 3] = "Sales Returns";
                            xcelApp.Cells[i, 4] = item.Value.folio;
                            xcelApp.Cells[i, 5] = item.Value.total;
                            xcelApp.Cells[i, 6] = "";
                            i++;
                        }
                    }
                    xcelApp.Columns.AutoFit();
                    xcelApp.Visible = true;
                    success = true;
                }
            }
            control.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                method, success);
        }
    }
}
