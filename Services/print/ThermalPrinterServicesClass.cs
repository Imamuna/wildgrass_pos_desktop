using PrinterUtility;
using System;
using System.Printing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using WildGrassPOSLibrary.Models;
using WildGrass_Desktop;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using WildGrass_Desktop_f8.Functions;
using System.Windows;
using System.Windows.Xps.Packaging;
using System.IO.Packaging;

namespace WildGrass_Desktop_f8.Services.print
{
    internal class ThermalPrinterServicesClass
    {
        int duration = 2000;
        int numberOfTries = 3;

        public void PrintPreview()
        {
            PrinterUtility.EscPosEpsonCommands.EscPosEpson obj = new();
            var bytesValue = Encoding.ASCII.GetBytes("WildGrass POS\n");
            bytesValue = PrintExtensions.AddBytes(bytesValue, Encoding.ASCII.GetBytes("Receipt No.:  1\n"));
            bytesValue = PrintExtensions.AddBytes(bytesValue, Encoding.ASCII.GetBytes("Date:  22, Jan 2022\n"));
            bytesValue = PrintExtensions.AddBytes(bytesValue, Encoding.ASCII.GetBytes("Time:  11:23\n"));
            bytesValue = PrintExtensions.AddBytes(bytesValue, CutPage());

            PrintExtensions.Print(bytesValue, @"\\\\DESKTOP-J5TV8D6\\EPSON TM-T20IIIL Receipt");
        }

        private byte[] CutPage()
        {
            List<byte> obj = new();
            obj.Add(Convert.ToByte(Convert.ToChar(0x1D)));
            obj.Add(Convert.ToByte('V'));
            obj.Add((byte)66);
            obj.Add((byte)3);
            return obj.ToArray();
        }


        public void ThermalReceipting(SourceDocumentClass2 sd)
        {
            WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new ();
            BusinessClass business = prevelantClass.GetBusiness();

            SettingsClass settingsClass = prevelantClass.RetrieveSettings();
            SourceDocumentSettingsClass sourceDocumentSettings = settingsClass.sourceDocumentSettings;
            SDConfiguration sourceDocumentS;

            switch (sd.sdType)
            {
                case "Receipt":
                    sourceDocumentS = sourceDocumentSettings.ReceiptConfiguration;
                    break;

                case "Invoice":
                    sourceDocumentS = sourceDocumentSettings.InvoiceConfiguration;
                    break;

                case "Quotation":
                    sourceDocumentS = sourceDocumentSettings.QuotationConfiguration;
                    break;

                case "Order":
                    sourceDocumentS = sourceDocumentSettings.OrderConfiguration;
                    break;

                case "Transfer Note":
                    sourceDocumentS = sourceDocumentSettings.DeliveryNoteConfiguration;
                    break;

                case "Delivery Note":
                    sourceDocumentS = sourceDocumentSettings.DeliveryNoteConfiguration;
                    break;

                case "Credit Note":
                    sourceDocumentS = sourceDocumentSettings.CreditNoteConfiguration;
                    break;

                case "Debit Note":
                    sourceDocumentS = sourceDocumentSettings.DebitNoteConfiguration;
                    break;
            }



            var heading = new StringBuilder();
            heading.AppendLine("Address: " + business.address);
            heading.AppendLine("Tel: " + business.businessPhone);
            heading.AppendLine("Email: " + business.businessEmail);
            heading.AppendLine("================");

            heading.AppendLine(sd.sdType + " No: " + sd.folio);
            heading.AppendLine("Date: " + sd.date);
            heading.AppendLine("Time: " + sd.time);
            heading.AppendLine("Comment: " + sd.comment);
            heading.AppendLine("================");


            //const int FIRST_COL_PAD = 20;
            //const int SECOND_COL_PAD = 7;
            //const int THIRD_COL_PAD = 20;
            //foreach (var item in sd.products)
            //{
            //    string name = item.Value.brandName + item.Value.productName + item.Value.flavor + item.Value.size;
            //    heading.Append(name.PadRight(FIRST_COL_PAD));

            //    var breakDown = item.Value.quantity + "x" + string.Format("{0:0.00} " + sd.currency, item.Value.price);
            //    heading.Append(breakDown.PadRight(SECOND_COL_PAD));

            //    heading.AppendLine(string.Format("{0:0.00} " + sd.currency, item.Value.price).PadLeft(THIRD_COL_PAD));
            //}

            //heading.AppendLine("================");


            if (heading == null) return; 

            var printDlg = new PrintDialog();
            var doc = new FlowDocument();
            doc.PagePadding = new System.Windows.Thickness(20);

            Paragraph pHead = new Paragraph(new Run(business.businessName));
            pHead.FontSize = 13;
            doc.Blocks.Add(pHead);

            Paragraph paragraph = new Paragraph(new Run(heading.ToString()));
            paragraph.FontSize = 11;
            doc.Blocks.Add(paragraph);

            Table table = new();
            TableRowGroup tableRowGroup = new TableRowGroup();
            TableRow r = new TableRow();

            doc.BringIntoView();

            doc.TextAlignment = TextAlignment.Center;
            doc.FontSize = 11;
            table.CellSpacing = 0;


            r.Cells.Add(new TableCell(new Paragraph(new Run("Details"))));
            r.Cells[0].ColumnSpan = 3;
            r.Cells[0].Padding = new Thickness(1);

            r.Cells[0].BorderBrush = Brushes.DarkGray;
            r.Cells[0].BorderThickness = new Thickness(0, 0, 1, 1);

            r.Cells.Add(new TableCell(new Paragraph(new Run("Qty"))));
            r.Cells[1].ColumnSpan = 1;
            r.Cells[1].Padding = new Thickness(1);

            r.Cells[1].BorderBrush = Brushes.DarkGray;
            r.Cells[1].BorderThickness = new Thickness(0, 0, 1, 1);

            r.Cells.Add(new TableCell(new Paragraph(new Run("Price " + sd.currency))));
            r.Cells[2].ColumnSpan = 1;
            r.Cells[2].Padding = new Thickness(1);

            r.Cells[2].BorderBrush = Brushes.DarkGray;
            r.Cells[2].BorderThickness = new Thickness(0, 0, 1, 1);

            r.Cells.Add(new TableCell(new Paragraph(new Run("Subtotal " + sd.currency))));
            r.Cells[3].ColumnSpan = 2;
            r.Cells[3].Padding = new Thickness(1);

            r.Cells[3].BorderBrush = Brushes.DarkGray;
            r.Cells[3].BorderThickness = new Thickness(0, 0, 1, 1);

            tableRowGroup.Rows.Add(r);
            table.RowGroups.Add(tableRowGroup);

            foreach (var item in sd.products)
            {
                table.BorderBrush = Brushes.Gray;
                table.BorderThickness = new Thickness(1, 1, 0, 0);
                table.FontSize = 11;
                tableRowGroup = new TableRowGroup();
                r = new TableRow();


                //what do we start with lol name
                string name = item.Value.brandName + item.Value.productName + item.Value.flavor + item.Value.size;
                Paragraph nPara = new Paragraph(new Run(name));
                Paragraph priceP = new Paragraph(new Run(item.Value.price));
                Paragraph quantityP = new Paragraph(new Run(item.Value.quantity));

                double tt = Convert.ToDouble(item.Value.price) * Convert.ToDouble(item.Value.quantity);
                double tt1 = Math.Round(tt, 2);
                Paragraph subP = new Paragraph(new Run(tt1.ToString("N2")));


                r.Cells.Add(new TableCell(nPara));
                r.Cells[0].ColumnSpan = 3;
                r.Cells[0].Padding = new Thickness(1);

                r.Cells[0].BorderBrush = Brushes.DarkGray;
                r.Cells[0].BorderThickness = new Thickness(0, 0, 1, 1);

                r.Cells.Add(new TableCell(quantityP));
                r.Cells[1].ColumnSpan = 1;
                r.Cells[1].Padding = new Thickness(1);

                r.Cells[1].BorderBrush = Brushes.DarkGray;
                r.Cells[1].BorderThickness = new Thickness(0, 0, 1, 1);

                r.Cells.Add(new TableCell(priceP));
                r.Cells[2].ColumnSpan = 1;
                r.Cells[2].Padding = new Thickness(1);

                r.Cells[2].BorderBrush = Brushes.DarkGray;
                r.Cells[2].BorderThickness = new Thickness(0, 0, 1, 1);

                r.Cells.Add(new TableCell(subP));
                r.Cells[3].ColumnSpan = 2;
                r.Cells[3].Padding = new Thickness(1);

                r.Cells[3].BorderBrush = Brushes.DarkGray;
                r.Cells[3].BorderThickness = new Thickness(0, 0, 1, 1);

                tableRowGroup.Rows.Add(r);
                table.RowGroups.Add(tableRowGroup);
            }

            //var summations = new StringBuilder();
            //summations.AppendLine("Excl VAT" + sd.exclVat + sd.currency);
            //summations.AppendLine("Incl VAT: " + sd.inclVat + sd.currency);
            //summations.AppendLine("Discount: " + sd.discount + sd.currency);
            //summations.AppendLine("Additional Cost: " + sd.addCost + sd.currency);
            //summations.AppendLine("VAT: " + sd.vat + sd.currency);
            //summations.AppendLine("Total: " + sd.total + sd.currency);

            table = AddSummaryRow(table, "Excl VAT", sd.exclVat);
            table = AddSummaryRow(table, "Incl VAT", sd.exclVat);
            table = AddSummaryRow(table, "Discount", sd.discount);
            table = AddSummaryRow(table, "Additional Cost", sd.addCost);
            table = AddSummaryRow(table, "VAT", sd.vat);
            table = AddSummaryRow(table, "Total", sd.total);

            doc.Blocks.Add(table);

            //Paragraph sumP = new Paragraph(new Run(summations.ToString()));
            //sumP.FontSize = 11;
            //doc.Blocks.Add(sumP);

            printDlg.PrintDocument((doc as IDocumentPaginatorSource).DocumentPaginator, "Print Caption");
        }

        public void InkjetPrinting(SourceDocumentClass2 sd)
        {
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string complete = System.IO.Path.Combine(systemPath, "WildGrass");
            string dirPdf = complete + @"\\PanCake Factory Kablonga\\Receipts\\WG2022112127275498.pdf";
            string dir = @"C:\Users\lubin\Documents\WildGrass\PanCake Factory Kablonga\Receipts\WG2022112127275498.pdf";
            var printDlg = new PrintDialog();
            XpsDocument xpsPackage = null;

            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    XpsDocument oldXpsPackage = xpsPackage;
                    xpsPackage = new XpsDocument(dirPdf, FileAccess.Read, CompressionOption.NotCompressed);
                    FixedDocumentSequence fixedDocumentSequence = xpsPackage.GetFixedDocumentSequence();
                    printDlg.PrintDocument((fixedDocumentSequence as IDocumentPaginatorSource).DocumentPaginator, "Print Caption");

                    //Process p = new Process();
                    //p.StartInfo = new ProcessStartInfo()
                    //{
                    //    CreateNoWindow = true,
                    //    Verb = "print",
                    //    FileName = sd.dirPdf2 //put the correct path here
                    //};
                    //p.Start();

                    //ShellExecute(NULL, L"print", L"D:\\TestFolder\\WordPdf1.pdf", NULL, NULL, SW_HIDE);
                    break;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Thread.Sleep(duration);
                }
            }
        }

        private Table AddSummaryRow(Table table, string title, string value)
        {
            TableRowGroup tableRowGroup = new TableRowGroup();
            TableRow r = new TableRow();
            r.Cells.Add(new TableCell(new Paragraph(new Run(title))));
            r.Cells[0].ColumnSpan = 5;
            r.Cells[0].Padding = new Thickness(1);

            r.Cells[0].BorderBrush = Brushes.DarkGray;
            r.Cells[0].BorderThickness = new Thickness(0, 0, 1, 1);

            r.Cells.Add(new TableCell(new Paragraph(new Run(value))));
            r.Cells[1].ColumnSpan = 2;
            r.Cells[1].Padding = new Thickness(1);

            r.Cells[1].BorderBrush = Brushes.DarkGray;
            r.Cells[1].BorderThickness = new Thickness(0, 0, 1, 1);
            tableRowGroup.Rows.Add(r);
            table.RowGroups.Add(tableRowGroup);

            return table;
        }

        public void PrintText(string text)
        {
            var printDlg = new PrintDialog();
            var doc = new FlowDocument();
            doc.PagePadding = new System.Windows.Thickness(20);


            Paragraph paragraph = new Paragraph(new Run(text));
            paragraph.FontSize = 12;
            doc.Blocks.Add(paragraph);

            printDlg.PrintDocument((doc as IDocumentPaginatorSource).DocumentPaginator, "Print Caption");
        }

        private PrintQueue FindPrinter(string printerName)
        {
            var printers = new PrintServer().GetPrintQueues();
            foreach (var printer in printers)
            {
                if (printer.FullName == printerName)
                {
                    return printer;
                }
            }
            return LocalPrintServer.GetDefaultPrintQueue();
        }
    }
}
