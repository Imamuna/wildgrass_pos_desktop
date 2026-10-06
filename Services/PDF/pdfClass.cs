using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Pdfa;
using WildGrass_Desktop;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Geom;
using WildGrassPOSLibrary.Models;
using iText.IO.Image;
using System.Windows;
using iText.Kernel.Pdf.Action;
using System.Reflection;
using iText.Layout.Borders;
using WildGrass_Desktop_f8.Services;
using Nancy;

namespace WildGrass_Desktop_f8.Functions.PDF
{
    internal class pdfClass
    {
        //ID's
        string uid = "";
        string bid = "";
        string eid = "";


        //key values
        PdfDocument pdf;
        PdfPage page;
        int pageNumber = 1;

        //Fonts
        PdfFont oswaldMed;
        PdfFont nunitoLig;
        PdfFont sans;
        PdfFont antonioMed;

        //Colors
        string themeColor;
        string footerTheme;


        Color grayLig;
        Color black;
        Color fontGray;
        Color ourGreen;
        Color white;


        //Dictionary
        Dictionary<string, DateClass> datesArray;


        //Sizes
        float defaultFontSize;
        int margin;
        int HeightY = 780;
        int businessHeight = 0;
        int businessY = 0;

        SettingsClass settingsClass;
        SourceDocumentSettingsClass sourceDocumentSettings;
        SDConfiguration sourceDocumentS;
        BusinessClass business;
        bool duplicate = false;

        WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new ();
        WildGrassPOSLibrary.Services.LocalDirectoryServicesClass localDirectory = new();

        public string GeneratePDF(SourceDocumentClass2 entry, bool duplicate)
        {
            string path = "";
            this.duplicate = duplicate;
            prevelantClass = new();
            business = prevelantClass.GetBusiness();
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            settingsClass = prevelantClass.RetrieveSettings();
            sourceDocumentSettings = settingsClass.sourceDocumentSettings;

            switch (entry.sdType)
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

            datesArray = prevelantClass.LoadDatesSales();

            if (sourceDocumentS == null) sourceDocumentS = sourceDocumentSettings.ReceiptConfiguration;

            try
            {
                //eventually get reed of this class from the main function
                BusinessClass business = prevelantClass.GetBusiness();

                string systemPath1 = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string complete1 = System.IO.Path.Combine(systemPath1, "WildGrass");
                string dirPdf1 = complete1 + @"\data\pdf resources";

                string antonioFile = dirPdf1 + @"\antonio_medium.ttf";
                string open_sansFile = dirPdf1 + @"\open_sans_regular.ttf";
                string nunitoFile = dirPdf1 + @"\nunito_light.ttf";
                string oswaldFile = dirPdf1 + @"\oswald_medium.ttf";

                FontProgram nuntioFontProgram =
                    FontProgramFactory.CreateFont(nunitoFile);
                nunitoLig = PdfFontFactory.CreateFont(nuntioFontProgram, PdfEncodings.WINANSI,
                    PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

                oswaldMed = PdfFontFactory.CreateFont(oswaldFile, PdfEncodings.WINANSI, 
                    PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

                sans = PdfFontFactory.CreateFont(open_sansFile, PdfEncodings.WINANSI,
                    PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

                antonioMed = PdfFontFactory.CreateFont(antonioFile, PdfEncodings.WINANSI,
                    PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

                //send this to folder: Documents/ WildGrass/ businessTitle/ Receipts
                string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string complete = System.IO.Path.Combine(systemPath, "WildGrass");
                string dirPdf = complete + @"\" + business.businessTitle + @"\" + entry.sdType + "s";
                if (!Directory.Exists(dirPdf))
                {
                    Directory.CreateDirectory(dirPdf);
                }
                if (duplicate)
                {
                    dirPdf += @"\" + entry.folio + "_duplicate.pdf";
                    path = dirPdf;
                } else
                {
                    dirPdf += @"\" + entry.folio + ".pdf";
                    path = dirPdf;
                }

                MemoryStream baos = new MemoryStream();
                PdfWriter writer = new PdfWriter(baos);
                pdf = new PdfDocument(writer.SetSmartMode(true));

                //PdfWriter writer = new PdfWriter(dirPdf);
                //pdf = new PdfDocument(writer);

                Document document = new Document(pdf);
                page = pdf.AddNewPage();

                document.SetMargins(0, 0, 0, 0);

                grayLig = new DeviceRgb(249, 249, 249);
                black = new DeviceRgb(0, 0, 0);
                fontGray = new DeviceRgb(51, 51, 51);
                ourGreen = new DeviceRgb(11, 120, 101);
                white = new DeviceRgb(255, 255, 255);
                defaultFontSize = 10.5f;
                margin = 15;

                //Duplicate indicator
                //if (duplicate)
                //{
                //    Paragraph paragraph = duplicateWatermark();
                //    document.Add(paragraph);
                //}


                //Logo
                int ourX = 20;
                if (sourceDocumentS.includeLogo)
                {
                    document = addLogo(document);
                    ourX = 310;
                }


                //Source Document Information
                document = addSourceDocInfo(document, entry, ourX);


                //Business Information
                document = addBusinessInfo(document, entry);
                int HeightYForContactInfo = HeightY;

                //Bank Information
                document = addBankInfo(document, entry);

                //Customer Information
                HeightY -= 15;
                if (entry.includeCustomerDetails || entry.sdType == "Invoice")
                {
                    document = addCustomerInfo(document, entry);
                }

                //Cart Entries
                document = TransactionData2(document, entry);

                //Terms and Conditions
                document = TermsConditions(document, entry);

                //Footer
                document = addFooter(document, entry, page, 1);

                //page numbers
                document = addPageNos(document, pageNumber, entry);

                //Closing
                document.Close();

                byte[] byte1 = baos.ToArray();
                //File(byte1, "application/pdf", dirPdf);
                FileStream fs = File.Create(dirPdf); 
                fs.Write(byte1, 0, (int)byte1.Length);
            }
            catch (Exception e)
            {
                //what to do
                path = e.Message;
                MessageBox.Show("something went wrong, pdf not generated: " + e.Message, "system testing");
            }
            return path;
        }

        public MemoryStream GeneratePDF_online(SourceDocumentClass2 entry, bool duplicate)
        {
            MemoryStream memoryStream = new MemoryStream();
            this.duplicate = duplicate;
            prevelantClass = new();
            //Business services
            business = prevelantClass.GetBusiness();
            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();
            //Settings services
            settingsClass = prevelantClass.RetrieveSettings();
            sourceDocumentSettings = settingsClass.sourceDocumentSettings;

            switch (entry.sdType)
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

            datesArray = prevelantClass.LoadDatesSales();

            if (sourceDocumentS == null) sourceDocumentS = sourceDocumentSettings.ReceiptConfiguration;

            try
            {

                //downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Fnunito_light.ttf?alt=media&token=4b3b81e2-5149-456e-b9de-06b6ee7fadf0", dirPdf + @"\nunito_light.ttf");
                //downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Fantonio_medium.ttf?alt=media&token=48fcd4d1-93b0-4e1d-bb6f-e323724887e1", dirPdf + @"\antonio_medium.ttf");
                //downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Fcoda_caption_extra_bold.ttf?alt=media&token=f238123a-6fd3-4149-b562-7fd256d21843", dirPdf + @"\coda_caption_extra_bold.ttf");
                //downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Fopen_sans_regular.ttf?alt=media&token=2e781640-dc8b-4877-8409-5c7032ea01e5", dirPdf + @"\open_sans_regular.ttf");
                //downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2Foswald_medium.ttf?alt=media&token=befa15fc-9d60-4d8b-a7b9-9c5393bbfe64", dirPdf + @"\oswald_medium.ttf");
                //downResource("https://firebasestorage.googleapis.com/v0/b/long-walk-pos.appspot.com/o/pdf%20resources%2FsRGB_CS_profile.icm?alt=media&token=40de6724-9bbe-4817-b43e-68e58ad4cca4", dirPdf + @"\sRGB_CS_profile.icm");

                BusinessClass business = prevelantClass.GetBusiness();

                string systemPath1 = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string complete1 = System.IO.Path.Combine(systemPath1, "WildGrass");
                string dirPdf1 = complete1 + @"\data\pdf resources";

                string antonioFile = dirPdf1 + @"\antonio_medium.ttf";
                string open_sansFile = dirPdf1 + @"\open_sans_regular.ttf";
                string nunitoFile = dirPdf1 + @"\nunito_light.ttf";
                string oswaldFile = dirPdf1 + @"\oswald_medium.ttf";

                FontProgram nuntioFontProgram =
                    FontProgramFactory.CreateFont(nunitoFile);
                nunitoLig = PdfFontFactory.CreateFont(nuntioFontProgram, PdfEncodings.WINANSI,
                    PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

                oswaldMed = PdfFontFactory.CreateFont(oswaldFile, PdfEncodings.WINANSI,
                    PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

                sans = PdfFontFactory.CreateFont(open_sansFile, PdfEncodings.WINANSI,
                    PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

                antonioMed = PdfFontFactory.CreateFont(antonioFile, PdfEncodings.WINANSI,
                    PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

                MemoryStream baos = new MemoryStream();
                PdfWriter writer = new PdfWriter(baos);
                pdf = new PdfDocument(writer.SetSmartMode(true));

                Document document = new Document(pdf);
                page = pdf.AddNewPage();

                document.SetMargins(0, 0, 0, 0);

                grayLig = new DeviceRgb(249, 249, 249);
                black = new DeviceRgb(0, 0, 0);
                fontGray = new DeviceRgb(51, 51, 51);
                ourGreen = new DeviceRgb(11, 120, 101);
                white = new DeviceRgb(255, 255, 255);
                defaultFontSize = 10.5f;
                margin = 15;

                //Logo
                int ourX = 20;
                if (sourceDocumentS.includeLogo)
                {
                    document = addLogo_online(document);
                    ourX = 310;
                }


                //Source Document Information
                document = addSourceDocInfo(document, entry, ourX);


                //Business Information
                document = addBusinessInfo(document, entry);
                int HeightYForContactInfo = HeightY;

                //Bank Information
                document = addBankInfo(document, entry);

                //Customer Information
                HeightY -= 15;
                if (entry.includeCustomerDetails || entry.sdType == "Invoice")
                {
                    document = addCustomerInfo(document, entry);
                }

                //Cart Entries
                document = TransactionData2(document, entry);

                //Terms and Conditions
                document = TermsConditions(document, entry);

                //Footer
                document = addFooter(document, entry, page, 1);

                //page numbers
                document = addPageNos(document, pageNumber, entry);

                //Closing
                document.Close();

                //pdfData
                byte[] byte1 = baos.ToArray();
                memoryStream = new MemoryStream(byte1);
            }
            catch (Exception e)
            {
                //what to do
                MessageBox.Show("something went wrong, pdf not generated: \n" + e.Message, "system testing");
            }
            return memoryStream;
        }

        //public string GeneratePDF(InvoiceClass entry)
        //{
        //    string path = "";

        //    prevelantClass = new PrevelantClass();

        //    uid = prevelantClass.getUid();
        //    bid = prevelantClass.getBid();
        //    eid = prevelantClass.getEid();

        //    StandardFirebaseOperationsClass standardOperations = new StandardFirebaseOperationsClass();
        //    settingsClass = standardOperations.RetrieveSettings();
        //    sourceDocumentSettings = settingsClass.sourceDocumentSettings;

        //    switch (entry.sdType)
        //    {
        //        case "Reciept":
        //            sourceDocumentS = sourceDocumentSettings.ReceiptConfiguration;
        //            break;

        //        case "Invoice":
        //            sourceDocumentS = sourceDocumentSettings.InvoiceConfiguration;
        //            break;

        //        case "Quotation":
        //            sourceDocumentS = sourceDocumentSettings.QuotationConfiguration;
        //            break;

        //        default:
        //            sourceDocumentS = new SourceDocumentClass2();
        //            break;
        //    }

        //    try
        //    {
        //        //eventually get reed of this class from the main function
        //        BusinessClass business = prevelantClass.GetBusiness();

        //        string systemPath1 = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        //        string complete1 = System.IO.Path.Combine(systemPath1, "WildGrass");
        //        string dirPdf1 = complete1 + @"\data\pdf resources";

        //        string antonioFile = dirPdf1 + @"\antonio_medium.ttf";
        //        string open_sansFile = dirPdf1 + @"\open_sans_regular.ttf";
        //        string nunitoFile = dirPdf1 + @"\nunito_light.ttf";
        //        string oswaldFile = dirPdf1 + @"\oswald_medium.ttf";

        //        oswaldMed = PdfFontFactory.CreateFont(oswaldFile, PdfEncodings.WINANSI,
        //            PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        //        nunitoLig = PdfFontFactory.CreateFont(nunitoFile, PdfEncodings.WINANSI,
        //            PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        //        sans = PdfFontFactory.CreateFont(open_sansFile, PdfEncodings.WINANSI,
        //            PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        //        antonioMed = PdfFontFactory.CreateFont(antonioFile, PdfEncodings.WINANSI,
        //            PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        //        //send this to folder: Documents/ WildGrass/ businessTitle/ Receipts
        //        string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //        string complete = System.IO.Path.Combine(systemPath, "WildGrass");
        //        string dirPdf = complete + @"\" + business.businessTitle + @"\" + entry.sdType + "s";
        //        if (!Directory.Exists(dirPdf))
        //        {
        //            Directory.CreateDirectory(dirPdf);
        //        }
        //        dirPdf += @"\" + entry.folio + ".pdf";
        //        path = dirPdf;

        //        MemoryStream baos = new MemoryStream();
        //        PdfWriter writer = new PdfWriter(baos);
        //        pdf = new PdfDocument(writer.SetSmartMode(true));

        //        //PdfWriter writer = new PdfWriter(dirPdf);
        //        //pdf = new PdfDocument(writer);

        //        Document document = new Document(pdf);
        //        page = pdf.AddNewPage();

        //        document.SetMargins(0, 0, 0, 0);

        //        grayLig = new DeviceRgb(249, 249, 249);
        //        black = new DeviceRgb(0, 0, 0);
        //        fontGray = new DeviceRgb(51, 51, 51);
        //        ourGreen = new DeviceRgb(11, 120, 101);
        //        white = new DeviceRgb(255, 255, 255);
        //        defaultFontSize = 10.5f;
        //        margin = 15;


        //        //Logo
        //        int ourX = 20;
        //        if (sourceDocumentS.includeLogo == true || true)
        //        {
        //            document = addLogo(document);
        //            ourX = 310;
        //        }


        //        //Source Document Information
        //        document = addSourceDocInfo(document, entry, ourX);


        //        //Business Information
        //        document = addBusinessInfo(document, entry);
        //        int HeightYForContactInfo = HeightY;

        //        //Bank Information
        //        document = addBankInfo(document, entry);

        //        //Customer Information
        //        HeightY -= 15;
        //        if (entry.includeCustomerDetails || entry.sdType == "Invoice")
        //        {
        //            document = addCustomerInfo(document, entry);
        //        }

        //        //Footer
        //        document = addFooter(document, entry, page, 1);

        //        //Cart Entries
        //        document = TransactionData2(document, entry);

        //        //Terms and Conditions
        //        document = TermsConditions(document, entry);

        //        //page numbers
        //        document = addPageNos(document, pageNumber, entry);

        //        //Closing
        //        document.Close();

        //        byte[] byte1 = baos.ToArray();
        //        //File(byte1, "application/pdf", dirPdf);
        //        FileStream fs = File.Create(dirPdf);
        //        fs.Write(byte1, 0, (int)byte1.Length);
        //    }
        //    catch (Exception e)
        //    {
        //        //what to do
        //        path = e.Message;
        //        MessageBox.Show("something went wrong, pdf not generated: " + e.Message, "system testing");
        //    }
        //    return path;
        //}

        //public string GeneratePDF(ReceiptClass entry)
        //{
        //    string path = "";

        //    prevelantClass = new PrevelantClass();

        //    uid = prevelantClass.getUid();
        //    bid = prevelantClass.getBid();
        //    eid = prevelantClass.getEid();

        //    StandardFirebaseOperationsClass standardOperations = new StandardFirebaseOperationsClass();
        //    settingsClass = standardOperations.RetrieveSettings();
        //    sourceDocumentSettings = settingsClass.sourceDocumentSettings;

        //    switch (entry.sdType)
        //    {
        //        case "Reciept":
        //            sourceDocumentS = sourceDocumentSettings.ReceiptConfiguration;
        //            break;

        //        case "Invoice":
        //            sourceDocumentS = sourceDocumentSettings.InvoiceConfiguration;
        //            break;

        //        case "Quotation":
        //            sourceDocumentS = sourceDocumentSettings.QuotationConfiguration;
        //            break;

        //        default:
        //            sourceDocumentS = new SourceDocumentClass2();
        //            break;
        //    }

        //    try
        //    {
        //        //eventually get reed of this class from the main function
        //        BusinessClass business = prevelantClass.GetBusiness();

        //        string systemPath1 = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        //        string complete1 = System.IO.Path.Combine(systemPath1, "WildGrass");
        //        string dirPdf1 = complete1 + @"\data\pdf resources";

        //        string antonioFile = dirPdf1 + @"\antonio_medium.ttf";
        //        string open_sansFile = dirPdf1 + @"\open_sans_regular.ttf";
        //        string nunitoFile = dirPdf1 + @"\nunito_light.ttf";
        //        string oswaldFile = dirPdf1 + @"\oswald_medium.ttf";

        //        oswaldMed = PdfFontFactory.CreateFont(oswaldFile, PdfEncodings.WINANSI,
        //            PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        //        nunitoLig = PdfFontFactory.CreateFont(nunitoFile, PdfEncodings.WINANSI,
        //            PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        //        sans = PdfFontFactory.CreateFont(open_sansFile, PdfEncodings.WINANSI,
        //            PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        //        antonioMed = PdfFontFactory.CreateFont(antonioFile, PdfEncodings.WINANSI,
        //            PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        //        //send this to folder: Documents/ WildGrass/ businessTitle/ Receipts
        //        string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //        string complete = System.IO.Path.Combine(systemPath, "WildGrass");
        //        string dirPdf = complete + @"\" + business.businessTitle + @"\" + entry.sdType + "s";
        //        if (!Directory.Exists(dirPdf))
        //        {
        //            Directory.CreateDirectory(dirPdf);
        //        }
        //        dirPdf += @"\" + entry.folio + ".pdf";
        //        path = dirPdf;

        //        MemoryStream baos = new MemoryStream();
        //        PdfWriter writer = new PdfWriter(baos);
        //        pdf = new PdfDocument(writer.SetSmartMode(true));

        //        //PdfWriter writer = new PdfWriter(dirPdf);
        //        //pdf = new PdfDocument(writer);

        //        Document document = new Document(pdf);
        //        page = pdf.AddNewPage();

        //        document.SetMargins(0, 0, 0, 0);

        //        grayLig = new DeviceRgb(249, 249, 249);
        //        black = new DeviceRgb(0, 0, 0);
        //        fontGray = new DeviceRgb(51, 51, 51);
        //        ourGreen = new DeviceRgb(11, 120, 101);
        //        white = new DeviceRgb(255, 255, 255);
        //        defaultFontSize = 10.5f;
        //        margin = 15;


        //        //Logo
        //        int ourX = 20;
        //        if (sourceDocumentS.includeLogo == true || true)
        //        {
        //            document = addLogo(document);
        //            ourX = 310;
        //        }


        //        //Source Document Information
        //        document = addSourceDocInfo(document, entry, ourX);


        //        //Business Information
        //        document = addBusinessInfo(document, entry);
        //        int HeightYForContactInfo = HeightY;

        //        //Bank Information
        //        document = addBankInfo(document, entry);

        //        //Customer Information
        //        HeightY -= 15;
        //        if (entry.includeCustomerDetails || entry.sdType == "Invoice")
        //        {
        //            document = addCustomerInfo(document, entry);
        //        }

        //        //Footer
        //        document = addFooter(document, entry, page, 1);

        //        //Cart Entries
        //        document = TransactionData2(document, entry);

        //        //Terms and Conditions
        //        document = TermsConditions(document, entry);

        //        //page numbers
        //        document = addPageNos(document, pageNumber, entry);

        //        //Closing
        //        document.Close();

        //        byte[] byte1 = baos.ToArray();
        //        //File(byte1, "application/pdf", dirPdf);
        //        FileStream fs = File.Create(dirPdf);
        //        fs.Write(byte1, 0, (int)byte1.Length);
        //    }
        //    catch (Exception e)
        //    {
        //        //what to do
        //        path = e.Message;
        //        MessageBox.Show("something went wrong, pdf not generated: " + e.Message, "system testing");
        //    }
        //    return path;
        //}

        public string GeneratePDF2()
        {
            string path = "";

            prevelantClass = new();

            uid = prevelantClass.getUid();
            bid = prevelantClass.getBid();
            eid = prevelantClass.getEid();

            settingsClass = prevelantClass.RetrieveSettings();
            sourceDocumentSettings = settingsClass.sourceDocumentSettings;


            //eventually get reed of this class from the main function
            BusinessClass business = prevelantClass.GetBusiness();

            string systemPath1 = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete1 = System.IO.Path.Combine(systemPath1, "WildGrass");
            string dirPdf1 = complete1 + @"\data\pdf resources";

            string antonioFile = dirPdf1 + @"\antonio_medium.ttf";
            string open_sansFile = dirPdf1 + @"\open_sans_regular.ttf";
            string nunitoFile = dirPdf1 + @"\nunito_light.ttf";
            string oswaldFile = dirPdf1 + @"\oswald_medium.ttf";

            oswaldMed = PdfFontFactory.CreateFont(oswaldFile, PdfEncodings.WINANSI,
                PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            nunitoLig = PdfFontFactory.CreateFont(nunitoFile, PdfEncodings.WINANSI,
                PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            sans = PdfFontFactory.CreateFont(open_sansFile, PdfEncodings.WINANSI,
                PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            antonioMed = PdfFontFactory.CreateFont(antonioFile, PdfEncodings.WINANSI,
                PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            //send this to folder: Documents/ WildGrass/ businessTitle/ Receipts
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string complete = System.IO.Path.Combine(systemPath, "WildGrass");
            string dirPdf = complete + @"\" + business.businessTitle + @"\" + "Receipt" + "s";
            if (!Directory.Exists(dirPdf))
            {
                Directory.CreateDirectory(dirPdf);
            }
            dirPdf += @"\systemTesting" + ".pdf";
            path = dirPdf;

            PdfWriter writer = new PdfWriter(dirPdf);
            pdf = new PdfDocument(writer);
            Document document = new Document(pdf);
            page = pdf.AddNewPage();

            document.SetMargins(0, 0, 0, 0);

            grayLig = new DeviceRgb(249, 249, 249);
            black = new DeviceRgb(0, 0, 0);
            ourGreen = new DeviceRgb(11, 120, 101);
            white = new DeviceRgb(255, 255, 255);
            defaultFontSize = 11.0f;
            margin = 15;

            Table table = new Table(2, true);

            Cell cell = new Cell();
            cell.Add(new Paragraph("contents go here"));
            cell.SetBorder(Border.NO_BORDER);
            table.AddCell(cell);

            document.Add(table);
            //Closing
            document.Close();

            return path;
        }

        private void setup()
        {
            //step 1: make sure we have all resources
            //if not download them
            //it no internet inform user then develop pdf with different configuration if possible
            //step 2: initialize pdf
            //step 3: add data to pdf
            //step 4: close pdf and return path
        }

        private Document addLogo_online(Document document)
        {
            try
            {
                Uri url = new Url(business.logoUrl);
                ImageData data = ImageDataFactory.Create(url);


                // Creating an Image object 
                Image logo = new Image(data)
                    .SetHeight(80)
                    .SetMaxWidth(310)
                    .SetFixedPosition(1, 20, 740);
                document.Add(logo);
            }
            catch (Exception)
            {

            }

            return document;
        }

        private Document addLogo(Document document)
        {
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = System.IO.Path.Combine(systemPath, "WildGrass");
            string dirPdf = complete + @"\data\" + uid + @"\businesses\" + bid + @"\pdf resources";

            ImageData data;
            if (File.Exists(dirPdf + @"\logo1.png"))
            {
                try
                {
                    String imageFile = dirPdf + @"\logo1.png";
                    data = ImageDataFactory.Create(imageFile);


                    // Creating an Image object 
                    Image logo = new Image(data)
                        .SetHeight(80)
                        .SetMaxWidth(310)
                        .SetFixedPosition(1, 20, 740);
                    document.Add(logo);
                }
                catch (Exception)
                {

                }
            }
            else
            {
                //we let the user know that we are downloading an image
                //setting resources
                //set image to desired directory


                //finally we run our function as originally intended
                //String imageFile = parent + @"\Functions\PDF\PDFResources\logo1.png";
                //data = ImageDataFactory.Create(imageFile);
            }
            // Creating an ImageData object 

            return document;
        }

        private Paragraph addText(string text, float ourX, float bottom, float width)
        {
            Paragraph paragraph = new Paragraph(text)
                .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(ourX, bottom, width);
            return paragraph;
        }

        private Document addSourceDocInfo(Document document, SourceDocumentClass2 entry, int ourX)
        {
            string id = entry.folio;
            if (entry.customIdYes) id = entry.customId;

            Paragraph transInfo1 = addText(entry.sdType, ourX, 800, 250);
            transInfo1.SetFontColor(black);
            transInfo1.SetFontSize(12.0f);
            document.Add(transInfo1);
            HeightY = 780;


            bool invoiceStandard = false;
            if (settingsClass != null)
                if (settingsClass.bookKeepingSettings != null)
                    if (settingsClass.bookKeepingSettings.TaxInvoiceStandard)
                    {
                        invoiceStandard = true;
                    }
            if (entry.sdType == "Receipt" && invoiceStandard)
            {
                Paragraph transInfo11 = addText("Tax Invoice", ourX, HeightY, 250);
                document.Add(transInfo11);
                HeightY -= margin;
            }

            Paragraph transInfo12 = addText("ID : " + id, ourX, HeightY, 250);
            document.Add(transInfo12);
            HeightY -= margin;

            Paragraph transInfo22 = addText("Generated by : " + entry.tagName, ourX, HeightY, 250);
            document.Add(transInfo22);
            HeightY -= margin;

            Paragraph transInfo23 = addText("Employee ID : " + entry.tagEid, ourX, HeightY, 250);
            document.Add(transInfo23);
            HeightY -= margin;

            Paragraph transInfo20 = addText("Issued date : " + entry.date, ourX, HeightY, 250);
            document.Add(transInfo20);
            HeightY -= margin;

            Paragraph transInfo21 = addText("Issued time : " + entry.time, ourX, HeightY, 250);
            document.Add(transInfo21);
            HeightY -= margin;

            switch (entry.sdType)
            {
                case "Quotation":
                    Paragraph transInfo3 = addText("Valid till: " + entry.valideTillDate, ourX, HeightY, 250);
                    document.Add(transInfo3);
                    HeightY -= margin;
                    break;
                case "Invoice":
                    Paragraph transInfo4 = addText("To be paid by: " + entry.valideTillDate, ourX, HeightY, 250);
                    document.Add(transInfo4);
                    HeightY -= margin;
                    break;
            }

            if(entry.comment != null) if(entry.comment.Length > 0)
                {
                    Paragraph transInfo24 = addText("Comment : " + entry.comment, ourX, HeightY, 250);
                    document.Add(transInfo24);
                    HeightY -= margin;
                }

            if (entry.sdType == "Credit Note" || entry.sdType == "Debit Note")
            {
                Paragraph transInfo25 = addText("Reason For Return : " + entry.ReasonForReturn, ourX, HeightY, 250);
                document.Add(transInfo25);
                HeightY -= margin;
            }
            return document;
        }

        private Document addBusinessInfo(Document document, SourceDocumentClass2 entry)
        {

            HeightY -= 10;

            int HeightYinner = HeightY;
            businessY = HeightY;
            int numberLines = 2;

            if(sourceDocumentS.bankInfo == true || true)
            {
                numberLines = 3;
                if (true)
                {
                    numberLines++;
                }
                if (true)
                {
                    numberLines++;
                }
            } else
            {
                if (sourceDocumentS.pacraInfo || true)
                {
                    numberLines++;
                }
                if (sourceDocumentS.zraInfo || true)
                {
                    numberLines++;
                }
            }

            
            int textWidth = 260;
            int rectHeight = 10 + (numberLines * 20) - 15;
            HeightY -= rectHeight;
            businessHeight = rectHeight;
            PdfCanvas pdfCanvas5 = new PdfCanvas(page);
            Rectangle rectangle5 = new Rectangle(10, HeightY + 10, 270, rectHeight);
            pdfCanvas5.SetFillColor(grayLig)
                .Rectangle(rectangle5)
                .Fill();
            Canvas canvas5 = new Canvas(pdfCanvas5, rectangle5);
            canvas5.Close();

            HeightY -= 10;

            HeightYinner -= 10;


            Paragraph bname = addText(business.businessName, 20, HeightYinner, textWidth);
            bname.SetFontColor(black);
            bname.SetFontSize(12.0f);
            document.Add(bname);
            HeightYinner -= margin;


            Paragraph localArea = addText("Local Area: " + business.localArea, 20, HeightYinner, textWidth);
            document.Add(localArea);
            HeightYinner -= margin;

            if (business.tPin != null) if(business.tPin.Length > 0)
                {
                    Paragraph btpin = addText("T-PIN: " + business.tPin, 20, HeightYinner, textWidth);
                    document.Add(btpin);
                    HeightYinner -= margin;
                }

            if (business.vatReg != null) if (business.vatReg.Length > 0)
                {
                    Paragraph btpin = addText("VAT-REG: " + business.vatReg, 20, HeightYinner, textWidth);
                    document.Add(btpin);
                }
            return document;
        }

        private Document addBankInfo(Document document, SourceDocumentClass2 entry)
        {
            if (sourceDocumentS.bankInfo == true)
            {
                int numberLines = 3;
                if (true)
                {
                    numberLines++;
                }
                if (true)
                {
                    numberLines++;
                }
                int rectHeight = 10 + (numberLines * 20) - 15;

                PdfCanvas pdfCanvas5 = new PdfCanvas(page);
                Rectangle rectangle5 = new Rectangle(290, businessY-rectHeight + 10, 270, rectHeight);
                pdfCanvas5.SetFillColor(grayLig)
                    .Rectangle(rectangle5)
                    .Fill();
                Canvas canvas5 = new Canvas(pdfCanvas5, rectangle5);
                canvas5.Close();

                int startLine = 310;
                int textWidth = 260;
                int HeightY = businessY - 10;

                //option to add bank info here
                Paragraph name = addText("Bank: " + business.bankName, startLine, HeightY, textWidth);
                document.Add(name);
                HeightY -= margin;

                Paragraph name1 = addText("Account Name: " + business.bankAccountName, startLine, HeightY, textWidth);
                document.Add(name1);
                HeightY -= margin;

                Paragraph name2 = addText("Branch: " + business.bankBranchName, startLine, HeightY, textWidth);
                document.Add(name2);
                HeightY -= margin;

                Paragraph name3 = addText("A/C No.: " + business.bankAccountNo, startLine, HeightY, textWidth);
                document.Add(name3);
                HeightY -= margin;

                if (true)
                {
                    Paragraph name4 = addText("Sort Code: " + business.bankSortCode, startLine, HeightY, textWidth);
                    document.Add(name4);
                    HeightY -= margin;
                }

                if (true)
                {
                    Paragraph name5 = addText("Swift Code: " + business.bankSwiftCode, startLine, HeightY, textWidth);
                    document.Add(name5);
                    HeightY -= margin;
                }
            }
            return document;
        }

        private Document addCustomerInfo(Document document, SourceDocumentClass2 entry)
        {
            HeightY += 20;
            int HeightYinner = HeightY;
            int numberLines = 0;
            if (entry.customerEmail != "") numberLines++;
            if (entry.customerPhoneNumber != "") numberLines++;
            if (entry.customerTpin != "") numberLines++;
            if (entry.customerAddress != null) if (entry.customerAddress.Length > 0)
            {
                int lent = entry.customerAddress.Length;
                decimal lele = lent/80;
                decimal counter = Math.Ceiling(lele);
                numberLines = numberLines + Convert.ToInt32(counter);
            }
            if (entry.customerFirstName != "" || entry.customerLastName != "")
            {
                if (numberLines == 0)
                {
                    numberLines++;
                }
            }

            int textWidth = 260;
            int textWidth1 = 240;
            int rectHeight = 80;

            HeightY -= rectHeight;

            PdfCanvas pdfCanvas5 = new PdfCanvas(page);
            Rectangle rectangle5 = new Rectangle(10, HeightY + 10, 550, rectHeight);
            pdfCanvas5.SetFillColor(grayLig)
                .Rectangle(rectangle5)
                .Fill();
            Canvas canvas5 = new Canvas(pdfCanvas5, rectangle5);
            canvas5.Close();

            HeightY -= 10;
            HeightYinner -= margin;

            int otherSidesHeight = HeightYinner;

            Paragraph customerInfo = addText("Customer Information", 20, HeightYinner, textWidth1);
            document.Add(customerInfo);
            HeightYinner -= margin;

            if (entry.customerLastName != "" || entry.customerLastName != "")
            {
                Paragraph nameParagrapgh = addText("Name: " + entry.customerFirstName + " " + entry.customerLastName, 20, HeightYinner, textWidth1);
                document.Add(nameParagrapgh);
            }

            if (entry.customerEmail != "")
            {
                Paragraph btpin = addText("Email: " + entry.customerEmail, 280, otherSidesHeight, textWidth);
                document.Add(btpin);
                otherSidesHeight -= margin;
            }

            if (entry.customerPhoneNumber != "")
            {
                Paragraph btpin = addText("Tel: " + entry.customerPhoneNumber, 280, otherSidesHeight, textWidth);
                document.Add(btpin);
                otherSidesHeight -= margin;
            }

            if (entry.customerTpin != null) if (entry.customerTpin.Length > 0)
            {
                Paragraph btpin = addText("T-PIN: " + entry.customerTpin, 280, otherSidesHeight, textWidth);
                document.Add(btpin);
                otherSidesHeight -= margin;
            }

            if (entry.customerAddress != null) if (entry.customerAddress.Length > 0)
            {
                Paragraph btpin = addText("Address: " + entry.customerAddress, 280, otherSidesHeight, textWidth);
                document.Add(btpin);
                otherSidesHeight -= margin;
            }

            return document;
        }

        private Document addPageNos(Document document, int totalPages, SourceDocumentClass2 entry)
        {
            Color foreGroundColor = white;
            if (sourceDocumentS.footerTheme != null) if (sourceDocumentS.footerTheme.Length > 0)
                {
                    switch (sourceDocumentS.footerTheme)
                    {
                        case "light":
                            foreGroundColor = fontGray;
                            break;

                        case "dark":
                            foreGroundColor = white;
                            break;
                    }
                }
            for (int i = 0; i < totalPages; i++)
            {
                int pageNo = i + 1;
                Paragraph address = new Paragraph("Page " + pageNo + " of " + totalPages + " for " + entry.sdType + " #" + entry.folio)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetPageNumber(pageNo)
                    .SetFontColor(foreGroundColor)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFixedPosition(0, 5, 600);
                document.Add(address);
            }

            return document;
        }

        private Document addFooter(Document document, SourceDocumentClass2 entry, PdfPage page, int pageNumber)
        {
            ImageData facebook_icon = getImage("facebook_white");
            ImageData twitter_icon = getImage("twitter_white");
            ImageData instagram_icon = getImage("instagram_white");
            ImageData whatsapp_icon = getImage("whatsapp_white");
            ImageData linkedin_icon = getImage("linkedin_white");
            ImageData website_icon = getImage("website_white");


            if (duplicate)
            {
                Paragraph paragraph = duplicateWatermark();
                paragraph.SetPageNumber(pageNumber);
                document.Add(paragraph);
            }

            Color footerBackground = ourGreen;
            Color foreGroundColor = white;
            themeColor = "#0B7865";
            if (sourceDocumentS.themeColor != null) if (sourceDocumentS.themeColor.Length > 0) themeColor = sourceDocumentS.themeColor;
            try
            {
                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(themeColor);
                footerBackground = new DeviceRgb(color.R, color.G, color.B);
            }
            catch (Exception ex)
            {
                MessageBox.Show("failed to get color: " + ex.Message, "system testing");
            }

            if (sourceDocumentS.footerTheme != null) if (sourceDocumentS.footerTheme.Length > 0)
                {
                    switch (sourceDocumentS.footerTheme)
                    {
                        case "light":
                            foreGroundColor = fontGray;
                            break;

                        case "dark":
                            foreGroundColor = white;
                            break;
                    }
                }

            float footerY = document.GetBottomMargin();

            int pageNo = pdf.GetPageNumber(page);
            PdfCanvas pdfCanvas5 = new PdfCanvas(page);
            Rectangle rectangle5 = new Rectangle(0, footerY, 600, 120);
            pdfCanvas5.SetFillColor(footerBackground)
                .Rectangle(rectangle5)
                .Fill();
            Canvas canvas5 = new Canvas(pdfCanvas5, rectangle5);
            canvas5.Close();

            Rectangle rectangle1 = new Rectangle(300, 40, 2, 70);
            pdfCanvas5.SetFillColor(foreGroundColor)
                .Rectangle(rectangle1)
                .Fill();
            Canvas canvas1 = new Canvas(pdfCanvas5, rectangle1);
            canvas1.Close();


            int margin = 17;
            int HeightYForContactInfo = 90;

            if (sourceDocumentS.address == true || true)
            {
                BusinessClass business = prevelantClass.GetBusiness();
                if (business.address != "" || true)
                {
                    Paragraph address = new Paragraph("Address: " + business.address)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetPageNumber(pageNo)
                    .SetFontColor(foreGroundColor)
                    .SetFixedPosition(20, 60, 260);
                    document.Add(address);
                }
            }

            if (sourceDocumentS.email || sourceDocumentS.phone || sourceDocumentS.website || sourceDocumentS.wildgrassStores || sourceDocumentS.wildgrassMaps || true)
            {
                Paragraph contactInfo = new Paragraph("Our Contact info")
                .SetFontSize(defaultFontSize)
                .SetFont(oswaldMed)
                .SetFontColor(foreGroundColor)
                .SetPageNumber(pageNo)
                .SetFixedPosition(330, HeightYForContactInfo, 220);
                document.Add(contactInfo);
                HeightYForContactInfo -= margin;

                if (sourceDocumentS.email || true)
                {
                    Paragraph bemail = new Paragraph("Email: " + business.businessEmail)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFontColor(foreGroundColor)
                    .SetPageNumber(pageNo)
                    .SetFixedPosition(330, HeightYForContactInfo, 220);
                    document.Add(bemail);
                    HeightYForContactInfo -= margin;
                }

                if (sourceDocumentS.phone || true)
                {
                    Paragraph btell = new Paragraph("Tel: " + business.businessPhone)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFontColor(foreGroundColor)
                    .SetPageNumber(pageNo)
                    .SetFixedPosition(330, HeightYForContactInfo, 220);
                    document.Add(btell);
                    HeightYForContactInfo -= margin;
                }

                if (sourceDocumentS.website && false) //this needs to be a hyper link
                {
                    Paragraph btell = new Paragraph("Website: " + business.businessWebsite)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFontColor(foreGroundColor)
                    .SetPageNumber(pageNo)
                    .SetFixedPosition(330, HeightYForContactInfo, 220);
                    document.Add(btell);
                    HeightYForContactInfo -= margin;
                }

                if (sourceDocumentS.wildgrassStores && false) //this needs to be a hyper link
                {
                    Paragraph btell = new Paragraph("WildGrass Stores: " + "@wildgrasszm")
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFontColor(foreGroundColor)
                    .SetPageNumber(pageNo)
                    .SetFixedPosition(330, HeightYForContactInfo, 220);
                    document.Add(btell);
                    HeightYForContactInfo -= margin;
                }

                if (sourceDocumentS.wildgrassMaps && false) //this needs to be a hyper link
                {
                    Paragraph btell = new Paragraph("WildGrass Maps: " + "@wildgrasszm_loc")
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFontColor(foreGroundColor)
                    .SetPageNumber(pageNo)
                    .SetFixedPosition(330, HeightYForContactInfo, 220);
                    document.Add(btell);
                    HeightYForContactInfo -= margin;
                }
            }

            //add follow us
            int stackHorizontally = 20;
            int height1 = 30;
            int width1 = 40;
            HeightYForContactInfo = 20;
            if (sourceDocumentS.facebook && false)
            {
                Image logo = new Image(facebook_icon)
                    .SetHeight(height1)
                    .SetMaxWidth(width1)
                    .SetFixedPosition(pageNo, stackHorizontally, HeightYForContactInfo);
                document.Add(logo);
                stackHorizontally += 45;
            }
            if (sourceDocumentS.twitter && false)
            {
                Image logo = new Image(twitter_icon)
                    .SetHeight(height1)
                    .SetMaxWidth(width1)
                    .SetFixedPosition(pageNo, stackHorizontally, HeightYForContactInfo);
                document.Add(logo);
                stackHorizontally += 45;
            }
            if (sourceDocumentS.linkedIn && false)
            {
                Image logo = new Image(linkedin_icon)
                    .SetHeight(height1)
                    .SetMaxWidth(width1)
                    .SetFixedPosition(pageNo, stackHorizontally, HeightYForContactInfo);
                document.Add(logo);
                stackHorizontally += 45;
            }
            if (sourceDocumentS.whatsapp && false)
            {
                Image logo = new Image(whatsapp_icon)
                    .SetHeight(height1)
                    .SetMaxWidth(width1)
                    .SetFixedPosition(pageNo, stackHorizontally, HeightYForContactInfo);
                document.Add(logo);
                stackHorizontally += 45;
            }
            if (sourceDocumentS.instagram && false)
            {
                Image logo = new Image(instagram_icon)
                    .SetHeight(height1)
                    .SetMaxWidth(width1)
                    .SetFixedPosition(pageNo, stackHorizontally, HeightYForContactInfo);
                document.Add(logo);
                stackHorizontally += 45;
            }
            if (sourceDocumentS.website && false)
            {
                Image logo = new Image(website_icon)
                    .SetHeight(40)
                    .SetMaxWidth(width1)
                    .SetFixedPosition(pageNo, stackHorizontally, HeightYForContactInfo);
                document.Add(logo);
            }

            return document;
        }

        private Paragraph duplicateWatermark()
        {
            Paragraph paragraph = new Paragraph("DUPLICATE")
                .SetFontSize(110)
                .SetFont(nunitoLig)
                .SetRotationAngle(120)
                .SetOpacity(0.2f)
                .SetFontColor(fontGray)
                .SetFixedPosition(100, 160, 600);
            return paragraph;
        }

        private Document TransactionData2(Document document, SourceDocumentClass2 entry)
        {
            HeightY += 10;
            float cartFontSize = 10.0f;
            PdfCanvas pdfCanvas5 = new PdfCanvas(page);
            Rectangle rectangle5 = new Rectangle(20, HeightY, 540, 2);
            pdfCanvas5.SetFillColor(black)
                .Rectangle(rectangle5)
                .Fill();
            Canvas canvas5 = new Canvas(pdfCanvas5, rectangle5);
            canvas5.Close();
            HeightY -= 25;

            Paragraph detailP = new Paragraph("ITEM DESCRIPTION")
                    .SetFontSize(defaultFontSize)
                    .SetFont(antonioMed)
                    .SetFontColor(black)
                    .SetFixedPosition(20, HeightY, 260);
            document.Add(detailP);

            Paragraph sizeP = new Paragraph("SIZE")
                .SetFontSize(defaultFontSize)
                .SetFont(antonioMed)
                .SetFontColor(black)
                .SetFixedPosition(280, HeightY, 70);
            document.Add(sizeP);

            Paragraph qtyP = new Paragraph("QTY")
                .SetFontSize(defaultFontSize)
                .SetFont(antonioMed)
                .SetFontColor(black)
                .SetFixedPosition(350, HeightY, 40);
            document.Add(qtyP);

            if(entry.sdType != "Delivery Note" && entry.sdType != "Transfer Note")
            {
                Paragraph priceP = new Paragraph("PRICE (" + entry.currency + ")")
                    .SetFontSize(defaultFontSize)
                    .SetFont(antonioMed)
                    .SetFontColor(black)
                    .SetFixedPosition(390, HeightY, 60);
                document.Add(priceP);

                Paragraph totalP = new Paragraph("ITEM TOTAL (" + entry.currency + ")")
                    .SetFontSize(defaultFontSize)
                    .SetFont(antonioMed)
                    .SetFontColor(black)
                    .SetFixedPosition(470, HeightY, 80);
                document.Add(totalP);
            }

            Dictionary<int, PdfItemClass> items = new Dictionary<int, PdfItemClass>();
            Dictionary<int, OnGoingClass> productsToSet = new Dictionary<int, OnGoingClass>();
            if (entry.products != null)
            {
                int i = 0;
                foreach (var item in entry.products)
                {
                    PdfItemClass pdfItem = new PdfItemClass();
                    string dd = item.Value.brandName + " " + item.Value.productName + " " + item.Value.flavor;
                    var ab = dd == null ? string.Empty : dd.Substring(0, Math.Min(70, dd.Length));
                    pdfItem.details = ab;

                    string ss = item.Value.size;
                    var b = ss == null ? string.Empty : ss.Substring(0, Math.Min(40, ss.Length));
                    pdfItem.size = b;

                    double pp = Convert.ToDouble(item.Value.price);
                    pp = Math.Round(pp, 2);
                    pdfItem.price = pp.ToString("N2");

                    pdfItem.qty = item.Value.quantity;

                    double tt = Convert.ToDouble(item.Value.price) * Convert.ToDouble(item.Value.quantity);
                    double tt1 = Math.Round(tt, 2);
                    pdfItem.total = tt1.ToString("N2");

                    pdfItem.type = 0;
                    items.Add(i, pdfItem);
                    i++;

                    if (item.Value.serialNumbers != null) if (item.Value.serialNumbers.Count > 0)
                        {
                            foreach (var serial in item.Value.serialNumbers)
                            {
                                PdfItemClass pdfItem1 = new PdfItemClass();
                                pdfItem1.details = ": : serial number: " + serial.Value.serialNumber;
                                pdfItem1.type = 1;
                                items.Add(i, pdfItem1);
                                i++;
                            }
                        }

                    double sub = 0;
                    double total = 0;
                    string innerDetails = "";

                    switch (item.Value.discountYes)
                    {
                        case 1:
                            sub = tt * item.Value.discountAmount / 100;
                            sub = Math.Round(sub, 2);
                            total = tt - sub;
                            innerDetails = " : : Discount " + item.Value.discountAmount + " %";

                            PdfItemClass pdfItem1 = new PdfItemClass();
                            pdfItem1.details = innerDetails;
                            pdfItem1.total = "(" + sub.ToString("N2") + ")" ;
                            pdfItem1.type = 2;
                            items.Add(i, pdfItem1);
                            i++;
                            break;
                        case 2:
                            total = tt - item.Value.discountAmount;
                            sub = item.Value.discountAmount;
                            innerDetails = " : : Discount " + sub;

                            PdfItemClass pdfItem2 = new PdfItemClass();
                            pdfItem2.details = innerDetails;
                            pdfItem2.total = "(" + sub.ToString("N2") + ")";
                            pdfItem2.type = 2;
                            items.Add(i, pdfItem2);
                            i++;
                            break;
                    }

                    sub = 0;
                    total = 0;
                    innerDetails = "";

                    switch (item.Value.addCostYes)
                    {
                        case 1:
                            sub = tt * item.Value.addCostAmount / 100;
                            sub = Math.Round(sub, 2);
                            total = tt - sub;
                            innerDetails = " : : Additioinal Cost " + item.Value.addCostAmount + " %";

                            PdfItemClass pdfItem1 = new PdfItemClass();
                            pdfItem1.details = innerDetails;
                            pdfItem1.total = sub.ToString("N2");
                            pdfItem1.type = 3;
                            items.Add(i, pdfItem1);
                            i++;
                            break;
                        case 2:
                            total = tt - item.Value.addCostAmount;
                            sub = item.Value.addCostAmount;
                            innerDetails = " : : Additional Cost " + sub;


                            PdfItemClass pdfItem2 = new PdfItemClass();
                            pdfItem2.details = innerDetails;
                            pdfItem2.total = sub.ToString("N2");
                            pdfItem2.type = 3;
                            items.Add(i, pdfItem2);
                            i++;
                            break;
                    }
                }
            }

            //we have effectively created an array of all the data we'll have to display in the order it will be displayed

            float y = HeightY - 10;
            int maxLines = 16;
            if (entry.includeCustomerDetails || entry.sdType == "Invoice") maxLines = 14;
            if (items.Count < maxLines) maxLines = items.Count;
            bool showShading = true;
            for (int i = 0; i < maxLines; i++) //page one max
            {
                if (items.ContainsKey(i))
                {
                    y -= 20.0f;
                    if (showShading)
                    {
                        PdfCanvas pdfCanvas = new PdfCanvas(page);
                        Rectangle rectangle = new Rectangle(10, y - 5.0f, 530, 25.0f);
                        pdfCanvas.SetFillColor(grayLig)
                            .Rectangle(rectangle)
                            .Fill();
                        Canvas canvas = new Canvas(pdfCanvas, rectangle);
                        canvas.Close();
                        if (items.ContainsKey(i + 1)) if (items[i + 1].type == 0)
                            {//only change if whats following is another item
                                showShading = false;
                            }
                    }
                    else
                    {
                        if (items.ContainsKey(i + 1)) if (items[i + 1].type == 0)
                            {
                                showShading = true;
                            }
                    }

                    string details = "";
                    string size = "";
                    string price = "";
                    string qty = "";
                    string itemTotal = "";
                    PdfItemClass item = items[i];
                    if (item.details != null) if (item.details.Length > 0) details = item.details;
                    if (item.size != null) if (item.size.Length > 0) size = item.size;
                    if (item.price != null) if (item.price.Length > 0) price = item.price;
                    if (item.qty != null) if (item.qty.Length > 0) qty = item.qty;
                    if (item.total != null) if (item.total.Length > 0) itemTotal = item.total;

                    Paragraph detailT = new Paragraph(details)
                        .SetFontSize(cartFontSize)
                        .SetFont(nunitoLig)
                        .SetFontColor(fontGray)
                        .SetFixedPosition(20, y, 260);
                    document.Add(detailT);

                    Paragraph sizeT = new Paragraph(size)
                        .SetFontSize(cartFontSize)
                        .SetFont(nunitoLig)
                        .SetFontColor(fontGray)
                        .SetFixedPosition(280, y, 70);
                    document.Add(sizeT);

                    Paragraph qtyT = new Paragraph(qty)
                        .SetFontSize(cartFontSize)
                        .SetFont(nunitoLig)
                        .SetFontColor(fontGray)
                        .SetFixedPosition(350, y, 40);
                    document.Add(qtyT);


                    if (entry.sdType != "Delivery Note" && entry.sdType != "Transfer Note")
                    {
                        Paragraph priceT = new Paragraph(price)
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(390, y, 60);
                        document.Add(priceT);

                        Paragraph totalT = new Paragraph(itemTotal)
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(470, y, 80);
                        document.Add(totalT);
                    }


                    y -= 5.0f;
                    items.Remove(i);
                }
                if (items.Count == 0) break;
            }

            //pages two, three and onwards
            double xyz = items.Count / 26;
            double totalPages = Math.Ceiling(xyz);
            //MessageBox.Show("page count = " + totalPages, "system testing");
            totalPages++;
            for (int iop = 0; iop < totalPages; iop++)
            {
                if (items.Count > 0)
                {
                    pageNumber++;
                    int HeightY2 = 800;
                    PdfPage page2 = pdf.AddNewPage();

                    //-------------------------------------------- Cart Entries --------------------------------------------//
                    HeightY2 += 10;
                    PdfCanvas pdfCanvas52 = new PdfCanvas(page2);
                    Rectangle rectangle52 = new Rectangle(20, HeightY2, 540, 2);
                    pdfCanvas52.SetFillColor(black)
                        .Rectangle(rectangle52)
                        .Fill();
                    Canvas canvas52 = new Canvas(pdfCanvas52, rectangle52);
                    canvas52.Close();
                    HeightY2 -= 25;

                    //if (duplicate)
                    //{
                    //    Paragraph paragraph = duplicateWatermark();
                    //    paragraph.SetPageNumber(pageNumber);
                    //    document.Add(paragraph);
                    //}


                    Paragraph detailP2 = new Paragraph("ITEM DESCRIPTION")
                        .SetFontSize(defaultFontSize)
                        .SetFont(antonioMed)
                        .SetFontColor(black)
                        .SetFixedPosition(pageNumber, 20, HeightY2, 260);
                    document.Add(detailP2);

                    Paragraph sizeP2 = new Paragraph("SIZE")
                        .SetFontSize(defaultFontSize)
                        .SetFont(antonioMed)
                        .SetFontColor(black)
                        .SetFixedPosition(pageNumber, 280, HeightY2, 70);
                    document.Add(sizeP2);

                    Paragraph qtyP2 = new Paragraph("QTY")
                        .SetFontSize(defaultFontSize)
                        .SetFont(antonioMed)
                        .SetFontColor(black)
                        .SetFixedPosition(pageNumber, 350, HeightY2, 40);
                    document.Add(qtyP2);


                    if (entry.sdType != "Delivery Note" && entry.sdType != "Transfer Note")
                    {
                        Paragraph priceP2 = new Paragraph("PRICE (" + entry.currency + ")")
                            .SetFontSize(defaultFontSize)
                            .SetFont(antonioMed)
                            .SetFontColor(black)
                            .SetFixedPosition(pageNumber, 390, HeightY2, 60);
                        document.Add(priceP2);

                        Paragraph totalP2 = new Paragraph("ITEM TOTAL (" + entry.currency + ")")
                            .SetFontSize(defaultFontSize)
                            .SetFont(antonioMed)
                            .SetFontColor(black)
                            .SetFixedPosition(pageNumber, 470, HeightY2, 80);
                        document.Add(totalP2);
                    }

                    y = HeightY2 - 10;
                    maxLines = 26;
                    int lowerNumber = 0;
                    for(int i = 0; i < items.Count; i++)
                    {
                        if (items.ContainsKey(i))
                        {
                            lowerNumber = i;
                            break;
                        }
                    }
                    for (int i = 0; i < maxLines + lowerNumber; i++) //page one max
                    {
                        if (items.ContainsKey(i))
                        {
                            y -= 20.0f;
                            if (showShading)
                            {
                                PdfCanvas pdfCanvas = new PdfCanvas(page2);
                                Rectangle rectangle = new Rectangle(10, y - 5.0f, 530, 25.0f);
                                pdfCanvas.SetFillColor(grayLig)
                                    .Rectangle(rectangle)
                                    .Fill();
                                Canvas canvas = new Canvas(pdfCanvas, rectangle);
                                canvas.Close();
                                if (items.ContainsKey(i + 1)) if (items[i + 1].type == 0)
                                    {//only change if whats following is another item
                                        showShading = false;
                                    }
                            }
                            else
                            {
                                if (items.ContainsKey(i + 1)) if (items[i + 1].type == 0)
                                    {
                                        showShading = true;
                                    }
                            }

                            string details = "";
                            string size = "";
                            string price = "";
                            string qty = "";
                            string itemTotal = "";
                            PdfItemClass item = items[i];
                            if (item.details != null) if (item.details.Length > 0) details = item.details;
                            if (item.size != null) if (item.size.Length > 0) size = item.size;
                            if (item.price != null) if (item.price.Length > 0) price = item.price;
                            if (item.qty != null) if (item.qty.Length > 0) qty = item.qty;
                            if (item.total != null) if (item.total.Length > 0) itemTotal = item.total;

                            Paragraph detailT = new Paragraph(details)
                                .SetFontSize(cartFontSize)
                                .SetFont(nunitoLig)
                                .SetFontColor(fontGray)
                                .SetFixedPosition(pageNumber, 20, y, 260);
                            document.Add(detailT);

                            Paragraph sizeT = new Paragraph(size)
                                .SetFontSize(cartFontSize)
                                .SetFont(nunitoLig)
                                .SetFontColor(fontGray)
                                .SetFixedPosition(pageNumber, 280, y, 70);
                            document.Add(sizeT);

                            Paragraph qtyT = new Paragraph(qty)
                                .SetFontSize(cartFontSize)
                                .SetFont(nunitoLig)
                                .SetFontColor(fontGray)
                                .SetFixedPosition(pageNumber, 350, y, 40);
                            document.Add(qtyT);


                            if (entry.sdType != "Delivery Note" && entry.sdType != "Transfer Note")
                            {
                                Paragraph priceT = new Paragraph(price)
                                    .SetFontSize(cartFontSize)
                                    .SetFont(nunitoLig)
                                    .SetFontColor(fontGray)
                                    .SetFixedPosition(pageNumber, 390, y, 60);
                                document.Add(priceT);

                                Paragraph totalT = new Paragraph(itemTotal)
                                    .SetFontSize(cartFontSize)
                                    .SetFont(nunitoLig)
                                    .SetFontColor(fontGray)
                                    .SetFixedPosition(pageNumber, 470, y, 80);
                                document.Add(totalT);
                            }


                            y -= 5.0f;
                            items.Remove(i);
                        }
                        if (items.Count == 0) break;
                    }
                    y -= 10;

                    //PdfCanvas pdfCanvas32 = new PdfCanvas(page2);
                    //Rectangle rectangle32 = new Rectangle(20, y, 540, 2);
                    //pdfCanvas32.SetFillColor(black)
                    //    .Rectangle(rectangle32)
                    //    .Fill();
                    //Canvas canvas32 = new Canvas(pdfCanvas32, rectangle32);
                    //canvas32.Close();

                    //Footer
                    document = addFooter(document, entry, page2, pageNumber);
                }
            }

            if(y <= 255)
            {
                pageNumber++;
                PdfPage page2 = pdf.AddNewPage();

                //if (duplicate)
                //{
                //    Paragraph paragraph = duplicateWatermark();
                //    paragraph.SetPageNumber(pageNumber);
                //    document.Add(paragraph);
                //}
                //Footer
                document = addFooter(document, entry, page2, pageNumber);
                y = 800;
            }

            //-------------------------------------------- Base Totals --------------------------------------------//

            y -= 30;

            //should we add individual discounts to overall discount? yes, yes we should

            if (entry.sdType != "Delivery Note" && entry.sdType != "Transfer Note")
            {
                string included = entry.inclVat;
                string excluded = entry.exclVat;
                string total = entry.total;
                string discount = entry.discount;
                string addcost = entry.addCost;
                string vat = entry.vat;

                string paid = entry.summations.Total.ToString("N2");
                if (entry.PartPaymentYes)
                {
                    paid = entry.amountPaid.ToString("N2");
                }

                try
                {
                    decimal incl = Convert.ToDecimal(entry.inclVat);
                    incl = Math.Round(incl, 2);
                    included = incl.ToString("N2");
                }
                catch (Exception)
                {

                }
                try
                {
                    decimal incl = Convert.ToDecimal(entry.exclVat);
                    incl = Math.Round(incl, 2);
                    excluded = incl.ToString("N2");
                }
                catch (Exception)
                {

                }
                try
                {
                    decimal incl = Convert.ToDecimal(entry.total);
                    incl = Math.Round(incl, 2);
                    total = incl.ToString("N2");
                }
                catch (Exception)
                {

                }
                try
                {
                    decimal incl = Convert.ToDecimal(entry.discount);
                    incl = Math.Round(incl, 2);
                    discount = incl.ToString("N2");
                }
                catch (Exception)
                {

                }
                try
                {
                    decimal incl = Convert.ToDecimal(entry.vat);
                    incl = Math.Round(incl, 2);
                    vat = incl.ToString("N2");
                }
                catch (Exception)
                {

                }
                try
                {
                    decimal incl = Convert.ToDecimal(entry.addCost);
                    incl = Math.Round(incl, 2);
                    addcost = incl.ToString("N2");
                }
                catch (Exception)
                {

                }


                Paragraph totalTx = new Paragraph("Cart Total")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 280, y, 170);
                document.Add(totalTx);

                Paragraph TotalV = new Paragraph(included)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 470, y, 80);
                document.Add(TotalV);
                y -= margin;

                Paragraph totalTx1 = new Paragraph("Excl_VAT")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 280, y, 170);
                document.Add(totalTx1);

                Paragraph TotalV1 = new Paragraph(excluded)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 470, y, 80);
                document.Add(TotalV1);

                y -= margin;

                Paragraph totalTx2 = new Paragraph("Incl_VAT")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 280, y, 170);
                document.Add(totalTx2);

                Paragraph TotalV2 = new Paragraph(included)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 470, y, 80);
                document.Add(TotalV2);

                y -= margin;

                Paragraph totalTx2v = new Paragraph("VAT")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 280, y, 170);
                document.Add(totalTx2v);

                Paragraph TotalV2v = new Paragraph(vat)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 470, y, 80);
                document.Add(TotalV2v);

                y -= margin;

                Paragraph totalTx3 = new Paragraph("Additional Costs")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 280, y, 170);
                document.Add(totalTx3);

                Paragraph TotalV3 = new Paragraph(addcost)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 470, y, 80);
                document.Add(TotalV3);

                y -= margin;


                if (entry.discount != "")
                {
                    Paragraph totalTx4 = new Paragraph("Discount")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 280, y, 170);
                    document.Add(totalTx4);

                    Paragraph TotalV4 = new Paragraph(discount)
                        .SetFontSize(defaultFontSize)
                        .SetFont(nunitoLig)
                        .SetFontColor(fontGray)
                        .SetFixedPosition(pageNumber, 470, y, 80);
                    document.Add(TotalV4);

                    y -= margin;
                }

                Paragraph totalTx5 = new Paragraph("Total Cost")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 280, y, 170);
                document.Add(totalTx5);

                Paragraph TotalV5 = new Paragraph(total)
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFontColor(fontGray)
                    .SetFixedPosition(pageNumber, 470, y, 80);
                document.Add(TotalV5);

                y -= margin;

                if(entry.sdType == "Receipt")
                {
                    Paragraph totalTx6 = new Paragraph("Paid")
                        .SetFontSize(defaultFontSize)
                        .SetFont(oswaldMed)
                        .SetFontColor(fontGray)
                        .SetFixedPosition(pageNumber, 280, y, 170);
                    document.Add(totalTx6);

                    Paragraph TotalV6 = new Paragraph(paid)
                        .SetFontSize(defaultFontSize)
                        .SetFont(nunitoLig)
                        .SetFontColor(fontGray)
                        .SetFixedPosition(pageNumber, 470, y, 80);
                    document.Add(TotalV6);

                    y -= margin;
                }
            }

            if (entry.PartPaymentYes)
            {
                PdfPage currenctPage = page;
                if (y <= 255)//needs to be dynamic based on the number of entries in matching
                {
                    pageNumber++;
                    PdfPage page2 = pdf.AddNewPage();
                    currenctPage = page2;

                    //if (duplicate)
                    //{
                    //    Paragraph paragraph = duplicateWatermark();
                    //    paragraph.SetPageNumber(pageNumber);
                    //    document.Add(paragraph);
                    //}
                    //Footer
                    document = addFooter(document, entry, page2, pageNumber);
                    y = 800;
                }
                //show part payment panel
                double amountPaid = entry.amountPaid;
                double totalPaid = amountPaid;


                if(entry.Matching != null) if(entry.Matching.Count > 0)
                    {
                        y -= 25.0f;
                        //show 
                        Paragraph detailT1q = new Paragraph("Payment Installments")
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 20, y, 260);
                        document.Add(detailT1q);

                        Paragraph dateT1q = new Paragraph("Date")
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 280, y, 70);
                        document.Add(dateT1q);

                        Paragraph paidT1q = new Paragraph("Payment")
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 390, y, 60);
                        document.Add(paidT1q);


                        bool sShading = true;
                        foreach(var item in entry.Matching)
                        {
                            //need to retrieve receipts data
                            if(item.Value.rid != entry.folio)
                            {
                                if (datesArray != null) if (datesArray.ContainsKey(item.Value.date))
                                    {
                                        Dictionary<string, SourceDocumentClass2> receiptsArray = datesArray[item.Value.date].receipts;
                                        if (receiptsArray != null) if (receiptsArray.ContainsKey(item.Value.rid))
                                            {
                                                if (sShading)
                                                {
                                                    PdfCanvas pdfCanvas = new PdfCanvas(currenctPage);
                                                    Rectangle rectangle = new Rectangle(10, y - 5.0f, 530, 25.0f);
                                                    pdfCanvas.SetFillColor(grayLig)
                                                        .Rectangle(rectangle)
                                                        .Fill();
                                                    Canvas canvas = new Canvas(pdfCanvas, rectangle);
                                                    canvas.Close();
                                                    sShading = false;
                                                }
                                                else
                                                {
                                                    sShading = true;
                                                }

                                                double totalv = Convert.ToDouble(receiptsArray[item.Value.rid].amountPaid);
                                                totalPaid += totalv;
                                                Paragraph detailTa = new Paragraph("Payment")
                                                    .SetFontSize(cartFontSize)
                                                    .SetFont(nunitoLig)
                                                    .SetFontColor(fontGray)
                                                    .SetFixedPosition(pageNumber, 20, y, 260);
                                                document.Add(detailTa);

                                                Paragraph dateTa = new Paragraph(item.Value.date)
                                                    .SetFontSize(cartFontSize)
                                                    .SetFont(nunitoLig)
                                                    .SetFontColor(fontGray)
                                                    .SetFixedPosition(pageNumber, 280, y, 70);
                                                document.Add(dateTa);

                                                Paragraph paidTa = new Paragraph(totalv.ToString("N2"))
                                                    .SetFontSize(cartFontSize)
                                                    .SetFont(nunitoLig)
                                                    .SetFontColor(fontGray)
                                                    .SetFixedPosition(pageNumber, 390, y, 60);
                                                document.Add(paidTa);
                                            }
                                    }
                                y -= 25.0f;
                            }
                        }

                        if (sShading)
                        {
                            PdfCanvas pdfCanvas = new PdfCanvas(currenctPage);
                            Rectangle rectangle = new Rectangle(10, y - 5.0f, 530, 25.0f);
                            pdfCanvas.SetFillColor(grayLig)
                                .Rectangle(rectangle)
                                .Fill();
                            Canvas canvas = new Canvas(pdfCanvas, rectangle);
                            canvas.Close();
                            sShading = false;
                        }
                        else
                        {
                            sShading = true;
                        }

                        Paragraph detailT = new Paragraph("Payment")
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 20, y, 260);
                        document.Add(detailT);

                        Paragraph dateT = new Paragraph("Today")
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 280, y, 70);
                        document.Add(dateT);

                        Paragraph paidT = new Paragraph(amountPaid.ToString("N2"))
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 390, y, 60);
                        document.Add(paidT);
                        y -= 25.0f;

                        if (sShading)
                        {
                            PdfCanvas pdfCanvas = new PdfCanvas(currenctPage);
                            Rectangle rectangle = new Rectangle(10, y - 5.0f, 530, 25.0f);
                            pdfCanvas.SetFillColor(grayLig)
                                .Rectangle(rectangle)
                                .Fill();
                            Canvas canvas = new Canvas(pdfCanvas, rectangle);
                            canvas.Close();
                            sShading = false;
                        }
                        else
                        {
                            sShading = true;
                        }

                        Paragraph detailT1 = new Paragraph("Total Paid")
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 20, y, 260);
                        document.Add(detailT1);

                        Paragraph paidT1 = new Paragraph(totalPaid.ToString("N2"))
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 390, y, 60);
                        document.Add(paidT1);
                        y -= 25.0f;

                        if (sShading)
                        {
                            PdfCanvas pdfCanvas = new PdfCanvas(currenctPage);
                            Rectangle rectangle = new Rectangle(10, y - 5.0f, 530, 25.0f);
                            pdfCanvas.SetFillColor(grayLig)
                                .Rectangle(rectangle)
                                .Fill();
                            Canvas canvas = new Canvas(pdfCanvas, rectangle);
                            canvas.Close();
                            sShading = false;
                        }
                        else
                        {
                            sShading = true;
                        }

                        Paragraph detailT2 = new Paragraph("Balance")
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 20, y, 260);
                        document.Add(detailT2);

                        double balance = entry.summations.Total - totalPaid;
                        Paragraph paidT2 = new Paragraph(balance.ToString("N2"))
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 390, y, 60);
                        document.Add(paidT2);
                        y -= 25.0f;
                    }
            }

            if (entry.sdType == "Invoice" && entry.PaymentPlanYes)
            {
                if(entry.PaymentPlanEntries != null) if(entry.PaymentPlanEntries.Count > 0)
                    {
                        PdfPage currenctPage = page;
                        if (y <= 255)
                        {
                            pageNumber++;
                            PdfPage page2 = pdf.AddNewPage();
                            currenctPage = page2;

                            //if (duplicate)
                            //{
                            //    Paragraph paragraph = duplicateWatermark();
                            //    paragraph.SetPageNumber(pageNumber);
                            //    document.Add(paragraph);
                            //}
                            //Footer
                            document = addFooter(document, entry, page2, pageNumber);
                            y = 800;
                        }
                        //show payment plan

                        Paragraph detailT1 = new Paragraph("Payment Plan")
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 20, y, 260);
                        document.Add(detailT1);

                        Paragraph dateT1 = new Paragraph("Date")
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 280, y, 70);
                        document.Add(dateT1);

                        Paragraph paidT1 = new Paragraph("Pending Payment")
                            .SetFontSize(cartFontSize)
                            .SetFont(nunitoLig)
                            .SetFontColor(fontGray)
                            .SetFixedPosition(pageNumber, 390, y, 60);
                        document.Add(paidT1);

                        y -= 25.0f;

                        bool sShading = true;
                        foreach (var item in entry.PaymentPlanEntries)
                        {
                            if (sShading)
                            {
                                PdfCanvas pdfCanvas = new PdfCanvas(currenctPage);
                                Rectangle rectangle = new Rectangle(10, y - 5.0f, 530, 25.0f);
                                pdfCanvas.SetFillColor(grayLig)
                                    .Rectangle(rectangle)
                                    .Fill();
                                Canvas canvas = new Canvas(pdfCanvas, rectangle);
                                canvas.Close();
                                showShading = false;
                            }
                            else
                            {
                                showShading = true;
                            }

                            Paragraph detailTa = new Paragraph("Installment Due Date")
                                .SetFontSize(cartFontSize)
                                .SetFont(nunitoLig)
                                .SetFontColor(fontGray)
                                .SetFixedPosition(pageNumber, 20, y, 260);
                            document.Add(detailTa);

                            Paragraph dateTa = new Paragraph(item.Value.date)
                                .SetFontSize(cartFontSize)
                                .SetFont(nunitoLig)
                                .SetFontColor(fontGray)
                                .SetFixedPosition(pageNumber, 280, y, 70);
                            document.Add(dateTa);

                            Paragraph paidTa = new Paragraph(item.Value.total.ToString("N2"))
                                .SetFontSize(cartFontSize)
                                .SetFont(nunitoLig)
                                .SetFontColor(fontGray)
                                .SetFixedPosition(pageNumber, 390, y, 60);
                            document.Add(paidTa);


                            y -= 25.0f;
                        }
                    }
            }

            return document;
        }

        private Document TransactionData(Document document, SourceDocumentClass2 entry)
        {
            HeightY += 10;
            float cartFontSize = 10.0f;
            PdfCanvas pdfCanvas5 = new PdfCanvas(page);
            Rectangle rectangle5 = new Rectangle(20, HeightY, 540, 2);
            pdfCanvas5.SetFillColor(black)
                .Rectangle(rectangle5)
                .Fill();
            Canvas canvas5 = new Canvas(pdfCanvas5, rectangle5);
            canvas5.Close();
            HeightY -= 25;

            Paragraph detailP = new Paragraph("ITEM DESCRIPTION")
                    .SetFontSize(defaultFontSize)
                    .SetFont(antonioMed)
                    .SetFixedPosition(20, HeightY, 260);
            document.Add(detailP);

            Paragraph sizeP = new Paragraph("SIZE")
                .SetFontSize(defaultFontSize)
                .SetFont(antonioMed)
                .SetFixedPosition(280, HeightY, 70);
            document.Add(sizeP);

            Paragraph qtyP = new Paragraph("QTY")
                .SetFontSize(defaultFontSize)
                .SetFont(antonioMed)
                .SetFixedPosition(350, HeightY, 40);
            document.Add(qtyP);

            Paragraph priceP = new Paragraph("PRICE (" + entry.currency + ")")
                .SetFontSize(defaultFontSize)
                .SetFont(antonioMed)
                .SetFixedPosition(390, HeightY, 60);
            document.Add(priceP);

            Paragraph totalP = new Paragraph("ITEM TOTAL (" + entry.currency + ")")
                .SetFontSize(defaultFontSize)
                .SetFont(antonioMed)
                .SetFixedPosition(470, HeightY, 80);
            document.Add(totalP);


            string details = "";
            string size = "";
            string price = "";
            string qty = "";
            string itemTotal = "";

            int count = 0;
            int actualLength = 0;
            Dictionary<int, OnGoingClass> productsToSet = new Dictionary<int, OnGoingClass>();
            if (entry.onGoing != null)
            {
                int iot = 0;
                foreach (var item in entry.onGoing)
                {
                    if (!productsToSet.ContainsKey(iot)) productsToSet.Add(iot, item.Value);
                    iot++;
                }
                //MessageBox.Show(Convert.ToString(productsToSet.Count) + " : " + Convert.ToString(entry.onGoing.Count)); 

                for (int i = 0; i < 30; i++)
                {
                    if (i < 30)//upper limit is 30
                    {//upper limit should change depending on height of header
                        if (productsToSet.ContainsKey(count))
                        {
                            OnGoingClass item = productsToSet[count];
                            if (i > 0)
                            {
                                details += "\n";
                                size += "\n";
                                price += "\n";
                                qty += "\n";
                                itemTotal += "\n";
                            }
                            count++; //keeps count of the number of products displayed

                            actualLength++;

                            string dd = item.brandName + " " + item.productName + " " + item.flavor;
                            var ab = dd == null ? string.Empty : dd.Substring(0, Math.Min(70, dd.Length));
                            details += ab;

                            string ss = item.size;
                            var b = ss == null ? string.Empty : ss.Substring(0, Math.Min(40, ss.Length));
                            size += b;

                            price += item.price;

                            qty += item.quantity;

                            double tt = Convert.ToDouble(item.price) * Convert.ToDouble(item.quantity);
                            itemTotal += Convert.ToString(tt);

                            if (item.serialNumbers != null) if (item.serialNumbers.Count > 0)
                                {
                                    foreach (var serial in item.serialNumbers)
                                    {
                                        actualLength++; //keeps count of the iterations
                                        i++; //determines the number of iterations we should run
                                        details += "\n: :" + serial.Value.serialNumber;
                                        size += "\n-";
                                        price += "\n-";
                                        qty += "\n-";
                                        itemTotal += "\n-";
                                    }
                                }

                            double sub = 0;
                            double total = 0;
                            string innerDetails = "";

                            switch (item.discountYes)
                            {
                                case 1:
                                    sub = tt * item.discountAmount / 100;
                                    sub = Math.Round(sub, 3);
                                    total = tt - sub;
                                    innerDetails = " : : Discount " + item.discountAmount + " %";


                                    i++; //determines the number of iterations we should run
                                    actualLength++; //keeps count of the iterations
                                    details += "\n" + innerDetails;
                                    size += "\n-";
                                    price += "\n-";
                                    qty += "\n-";
                                    itemTotal += "\n(" + sub + ")";
                                    break;
                                case 2:
                                    total = tt - item.discountAmount;
                                    sub = item.discountAmount;
                                    innerDetails = " : : Discount " + sub;


                                    i++; //determines the number of iterations we should run
                                    actualLength++; //keeps count of the iterations
                                    details += "\n" + innerDetails;
                                    size += "\n-";
                                    price += "\n-";
                                    qty += "\n-";
                                    itemTotal += "\n(" + sub + ")";
                                    break;
                            }

                            sub = 0;
                            total = 0;
                            innerDetails = "";

                            switch (item.addCostYes)
                            {
                                case 1:
                                    sub = tt * item.discountAmount / 100;
                                    sub = Math.Round(sub, 3);
                                    total = tt - sub;
                                    innerDetails = " : : Additioinal Cost " + item.discountAmount + " %";


                                    i++; //determines the number of iterations we should run
                                    actualLength++; //keeps count of the iterations
                                    details += "\n" + innerDetails;
                                    size += "\n-";
                                    price += "\n-";
                                    qty += "\n-";
                                    itemTotal += "\n" + total;
                                    break;
                                case 2:
                                    total = tt - item.discountAmount;
                                    sub = item.discountAmount;
                                    innerDetails = " : : Additional Cost " + sub;


                                    i++; //determines the number of iterations we should run
                                    actualLength++; //keeps count of the iterations
                                    details += "\n" + innerDetails;
                                    size += "\n-";
                                    price += "\n-";
                                    qty += "\n-";
                                    itemTotal += "\n" + total;
                                    break;
                            }
                        }
                        
                    }
                }
                for (int i = 0; i < count; i++)
                {
                    productsToSet.Remove(i);
                }
                //MessageBox.Show(Convert.ToString(count));
            }



            float cartLength = 18.5f * actualLength;
            float y = HeightY - cartLength;

            Paragraph detailT = new Paragraph(details)
                .SetFontSize(cartFontSize)
                .SetFont(nunitoLig)
                .SetFixedPosition(20, y, 260);
            document.Add(detailT);

            Paragraph sizeT = new Paragraph(size)
                .SetFontSize(cartFontSize)
                .SetFont(nunitoLig)
                .SetFixedPosition(280, y, 70);
            document.Add(sizeT);

            Paragraph qtyT = new Paragraph(qty)
                .SetFontSize(cartFontSize)
                .SetFont(nunitoLig)
                .SetFixedPosition(350, y, 40);
            document.Add(qtyT);

            Paragraph priceT = new Paragraph(price)
                .SetFontSize(cartFontSize)
                .SetFont(nunitoLig)
                .SetFixedPosition(390, y, 60);
            document.Add(priceT);

            Paragraph totalT = new Paragraph(itemTotal)
                .SetFontSize(cartFontSize)
                .SetFont(nunitoLig)
                .SetFixedPosition(470, y, 80);
            document.Add(totalT);

            y -= 10;

            PdfCanvas pdfCanvas3 = new PdfCanvas(page);
            Rectangle rectangle3 = new Rectangle(20, y, 540, 2);
            pdfCanvas3.SetFillColor(black)
                .Rectangle(rectangle3)
                .Fill();
            Canvas canvas3 = new Canvas(pdfCanvas3, rectangle3);
            canvas3.Close();

            //page two
            if (productsToSet.Count > 0)
            {
                int HeightY2 = 800;
                PdfPage page2 = pdf.AddNewPage();

                //-------------------------------------------- Cart Entries --------------------------------------------//
                HeightY2 += 10;
                float cartFontSize2 = 10.0f;
                PdfCanvas pdfCanvas52 = new PdfCanvas(page2);
                Rectangle rectangle52 = new Rectangle(20, HeightY2, 540, 2);
                pdfCanvas5.SetFillColor(black)
                    .Rectangle(rectangle5)
                    .Fill();
                Canvas canvas52 = new Canvas(pdfCanvas5, rectangle5);
                canvas52.Close();
                HeightY2 -= 25;


                Paragraph detailP2 = new Paragraph("ITEM DESCRIPTION")
                    .SetFontSize(defaultFontSize)
                    .SetFont(antonioMed)
                    .SetFixedPosition(2, 20, HeightY2, 260);
                document.Add(detailP2);

                Paragraph sizeP2 = new Paragraph("SIZE")
                    .SetFontSize(defaultFontSize)
                    .SetFont(antonioMed)
                    .SetFixedPosition(2, 280, HeightY2, 70);
                document.Add(sizeP2);

                Paragraph qtyP2 = new Paragraph("QTY")
                    .SetFontSize(defaultFontSize)
                    .SetFont(antonioMed)
                    .SetFixedPosition(2, 350, HeightY2, 40);
                document.Add(qtyP2);

                Paragraph priceP2 = new Paragraph("PRICE (ZMK)")
                    .SetFontSize(defaultFontSize)
                    .SetFont(antonioMed)
                    .SetFixedPosition(2, 390, HeightY2, 60);
                document.Add(priceP2);

                Paragraph totalP2 = new Paragraph("ITEM TOTAL (ZMK)")
                    .SetFontSize(defaultFontSize)
                    .SetFont(antonioMed)
                    .SetFixedPosition(2, 470, HeightY2, 80);
                document.Add(totalP2);

                string details2 = "";
                string size2 = "";
                string price2 = "";
                string qty2 = "";
                string itemTotal2 = "";

                int count2 = 0;
                int actualLength2 = 0;
                for (int i = 0; i < entry.onGoing.Count; i++)
                {
                    if (i < 50)
                    {
                        if (productsToSet.ContainsKey(count))
                        {
                            OnGoingClass item = productsToSet[count];
                            if (i > 0)
                            {
                                details2 += "\n";
                                size2 += "\n";
                                price2 += "\n";
                                qty2 += "\n";
                                itemTotal2 += "\n";
                            }
                            count2++;
                            actualLength2++;

                            string dd = item.brandName + " " + item.productName + " " + item.flavor;
                            var ab = dd == null ? string.Empty : dd.Substring(0, Math.Min(70, dd.Length));
                            details2 += ab;

                            string ss = item.size;
                            var b = ss == null ? string.Empty : ss.Substring(0, Math.Min(40, ss.Length));
                            size2 += b;

                            price2 += item.price;

                            qty2 += item.quantity;

                            double tt = Convert.ToDouble(item.price) * Convert.ToDouble(item.quantity);
                            itemTotal2 += Convert.ToString(tt);

                            if (item.serialNumbers != null) if (item.serialNumbers.Count > 0)
                                {
                                    foreach (var serial in item.serialNumbers)
                                    {
                                        actualLength2++;
                                        i++;
                                        details2 += "\n: :" + serial.Value.serialNumber;
                                        size2 += "\n-";
                                        price2 += "\n-";
                                        qty2 += "\n-";
                                        itemTotal2 += "\n-";
                                    }
                                }


                            double sub = 0;
                            double total = 0;
                            string innerDetails = "";

                            switch (item.discountYes)
                            {
                                case 1:
                                    sub = tt * item.discountAmount / 100;
                                    sub = Math.Round(sub, 3);
                                    total = tt - sub;
                                    innerDetails = " : : Discount " + item.discountAmount + " %";


                                    i++; //determines the number of iterations we should run
                                    actualLength2++; //keeps count of the iterations
                                    details2 += "\n" + innerDetails;
                                    size2 += "\n-";
                                    price2 += "\n-";
                                    qty2 += "\n-";
                                    itemTotal2 += "\n(" + sub + ")";
                                    break;
                                case 2:
                                    total = tt - item.discountAmount;
                                    sub = item.discountAmount;
                                    innerDetails = " : : Discount " + sub;


                                    i++; //determines the number of iterations we should run
                                    actualLength2++; //keeps count of the iterations
                                    details2 += "\n" + innerDetails;
                                    size2 += "\n-";
                                    price2 += "\n-";
                                    qty2 += "\n-";
                                    itemTotal2 += "\n(" + sub + ")";
                                    break;
                            }

                            sub = 0;
                            total = 0;
                            innerDetails = "";

                            switch (item.addCostYes)
                            {
                                case 1:
                                    sub = tt * item.discountAmount / 100;
                                    sub = Math.Round(sub, 3);
                                    total = tt - sub;
                                    innerDetails = " : : Additioinal Cost " + item.discountAmount + " %";


                                    i++; //determines the number of iterations we should run
                                    actualLength2++; //keeps count of the iterations
                                    details2 += "\n" + innerDetails;
                                    size2 += "\n-";
                                    price2 += "\n-";
                                    qty2 += "\n-";
                                    itemTotal2 += "\n-" + sub;
                                    break;
                                case 2:
                                    total = tt - item.discountAmount;
                                    sub = item.discountAmount;
                                    innerDetails = " : : Additional Cost " + sub;


                                    i++; //determines the number of iterations we should run
                                    actualLength2++; //keeps count of the iterations
                                    details2 += "\n" + innerDetails;
                                    size2 += "\n-";
                                    price2 += "\n-";
                                    qty2 += "\n-";
                                    itemTotal2 += "\n-" + sub;
                                    break;
                            }
                        }
                    }
                }
                for (int i = 0; i < count2; i++)
                {
                    if (productsToSet.ContainsKey(i)) productsToSet.Remove(i);
                } //what do we do when a single product has serial numbers that exceed the number of iterations we can run on that page

                float cartLength2 = 18.5f * actualLength2;
                float y2 = HeightY2 - cartLength2;

                Paragraph detailT2 = new Paragraph(details2)
                    .SetFontSize(cartFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(2, 20, y2, 260);
                document.Add(detailT2);

                Paragraph sizeT2 = new Paragraph(size2)
                    .SetFontSize(cartFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(2, 280, y2, 70);
                document.Add(sizeT2);

                Paragraph qtyT2 = new Paragraph(qty2)
                    .SetFontSize(cartFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(2, 350, y2, 40);
                document.Add(qtyT2);

                Paragraph priceT2 = new Paragraph(price2)
                    .SetFontSize(cartFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(2, 390, y2, 60);
                document.Add(priceT2);

                Paragraph totalT2 = new Paragraph(itemTotal2)
                    .SetFontSize(cartFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(2, 470, y2, 80);
                document.Add(totalT2);

                y2 -= 10;

                PdfCanvas pdfCanvas32 = new PdfCanvas(page2);
                Rectangle rectangle32 = new Rectangle(20, y2, 540, 2);
                pdfCanvas32.SetFillColor(black)
                    .Rectangle(rectangle32)
                    .Fill();
                Canvas canvas32 = new Canvas(pdfCanvas32, rectangle32);
                canvas32.Close();


                //-------------------------------------------- Base Totals --------------------------------------------//

                y2 -= 30;

                Paragraph totalTx23 = new Paragraph("Cart Total")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(2, 280, y2, 170);
                document.Add(totalTx23);

                Paragraph TotalV23 = new Paragraph(entry.inclVat)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(2, 470, y2, 80);
                document.Add(TotalV23);
                y2 -= margin;

                Paragraph totalTx12 = new Paragraph("Excl_VAT")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(2, 280, y2, 170);
                document.Add(totalTx12);

                Paragraph TotalV12 = new Paragraph(entry.exclVat)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(2, 470, y2, 80);
                document.Add(TotalV12);

                y2 -= margin;

                Paragraph totalTx22 = new Paragraph("Incl_VAT")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(2, 280, y2, 170);
                document.Add(totalTx22);

                Paragraph TotalV22 = new Paragraph(entry.inclVat)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(2, 470, y2, 80);
                document.Add(TotalV22);

                y2 -= margin;

                Paragraph totalTx32 = new Paragraph("Additional Costs")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(2, 280, y2, 170);
                document.Add(totalTx32);

                Paragraph TotalV32 = new Paragraph(entry.addCost)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(2, 470, y2, 80);
                document.Add(TotalV32);

                y2 -= margin;


                if (entry.discount != "")
                {
                    Paragraph totalTx42 = new Paragraph("Discount")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(2, 280, y2, 170);
                    document.Add(totalTx42);

                    Paragraph TotalV42 = new Paragraph(entry.discount)
                        .SetFontSize(defaultFontSize)
                        .SetFont(nunitoLig)
                        .SetFixedPosition(2, 470, y2, 80);
                    document.Add(TotalV42);
                    y2 -= margin;
                }

                Paragraph totalTx52 = new Paragraph("Total Cost")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(2, 280, y2, 170);
                document.Add(totalTx52);

                Paragraph TotalV52 = new Paragraph(entry.total)
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(2, 470, y2, 80);
                document.Add(TotalV52);


                //Footer
                document = addFooter(document, entry, page2, 2);
            }
            else
            {
                //-------------------------------------------- Base Totals --------------------------------------------//

                y -= 30;

                Paragraph totalTx = new Paragraph("Cart Total")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(280, y, 170);
                document.Add(totalTx);

                Paragraph TotalV = new Paragraph(entry.inclVat)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(470, y, 80);
                document.Add(TotalV);
                y -= margin;

                Paragraph totalTx1 = new Paragraph("Excl_VAT")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(280, y, 170);
                document.Add(totalTx1);

                Paragraph TotalV1 = new Paragraph(entry.exclVat)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(470, y, 80);
                document.Add(TotalV1);

                y -= margin;

                Paragraph totalTx2 = new Paragraph("Incl_VAT")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(280, y, 170);
                document.Add(totalTx2);

                Paragraph TotalV2 = new Paragraph(entry.inclVat)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(470, y, 80);
                document.Add(TotalV2);

                y -= margin;

                Paragraph totalTx3 = new Paragraph("Additional Costs")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(280, y, 170);
                document.Add(totalTx3);

                Paragraph TotalV3 = new Paragraph(entry.addCost)
                    .SetFontSize(defaultFontSize)
                    .SetFont(nunitoLig)
                    .SetFixedPosition(470, y, 80);
                document.Add(TotalV3);

                y -= margin;


                if (entry.discount != "")
                {
                    Paragraph totalTx4 = new Paragraph("Discount")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(280, y, 170);
                    document.Add(totalTx4);

                    Paragraph TotalV4 = new Paragraph(entry.discount)
                        .SetFontSize(defaultFontSize)
                        .SetFont(nunitoLig)
                        .SetFixedPosition(470, y, 80);
                    document.Add(TotalV4);

                    y -= margin;
                }

                Paragraph totalTx5 = new Paragraph("Total Cost")
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(280, y, 170);
                document.Add(totalTx5);

                Paragraph TotalV5 = new Paragraph(entry.total)
                    .SetFontSize(defaultFontSize)
                    .SetFont(oswaldMed)
                    .SetFixedPosition(470, y, 80);
                document.Add(TotalV5);
            }
            return document;
        }

        private Document TermsConditions(Document document, SourceDocumentClass2 entry)
        {
            if(sourceDocumentS.includeTermsNdConditions)
            {
                BusinessClass business = prevelantClass.GetBusiness();
                PdfPage page2 = pdf.AddNewPage();
                int pageNumber1 = pdf.GetPageNumber(page2);

                if (duplicate)
                {
                    Paragraph paragraph = new Paragraph("DUPLICATE")
                        .SetFontSize(110)
                        .SetFont(nunitoLig)
                        .SetPageNumber(pageNumber1)
                        .SetRotationAngle(120)
                        .SetOpacity(0.3f)
                        .SetFontColor(fontGray)
                        .SetFixedPosition(100, 160, 600);
                    document.Add(paragraph);
                }

                Paragraph header = new Paragraph("Terms and Conditions")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetPageNumber(pageNumber1)
                    .SetFixedPosition(20, 790, 560)
                    .SetFontSize(14);
                document.Add(header);

                PdfCanvas pdfCanvas5 = new PdfCanvas(page2);
                Rectangle rectangle5 = new Rectangle(40, 790, 540, 1);
                pdfCanvas5.SetFillColor(black)
                    .Rectangle(rectangle5)
                    .Fill();
                Canvas canvas5 = new Canvas(pdfCanvas5, rectangle5);
                canvas5.Close();
                string termsTxt = "";
                if (sourceDocumentS.termsNdConditions != null) termsTxt = sourceDocumentS.termsNdConditions;
                int count = termsTxt.Length;
                int lines = 1;
                while (count > 100)
                {
                    lines++;
                    count -= 100;
                }
                int bottom = 750 - (lines * 11);

                Paragraph subheader = new Paragraph(termsTxt)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetPageNumber(pageNumber1)
                    .SetFontColor(black)
                    .SetFixedPosition(20, bottom, 560)
                    .SetFontSize(defaultFontSize);
                document.Add(subheader);

                pageNumber++;
                //Footer
                document = addFooter(document, entry, page2, pageNumber1);
            }

            return document;
        }

        private ImageData getImage(string name)
        {
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = System.IO.Path.Combine(systemPath, "WildGrass");
            string dirPdf = complete + @"\data\pdf resources";

            ImageData data = null;
            string dir = dirPdf + @"\icons\" + name + ".png";
            if (File.Exists(dir))
            {
                String imageFile = dir;
                data = ImageDataFactory.Create(imageFile);
            }
            return data;
        }

        private ImageData getImage()
        {
            Uri url = new Url(business.logoUrl);
            ImageData data = ImageDataFactory.Create(url);
            return data;
        }
    }
}
