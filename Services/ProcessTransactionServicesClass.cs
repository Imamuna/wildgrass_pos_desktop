using Firebase.Storage;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WildGrass_Desktop;
using WildGrass_Desktop_f8.Functions;
using WildGrass_Desktop_f8.Functions.PDF;
using WildGrassPOSLibrary.Models;

namespace WildGrass_Desktop_f8.Services
{
    internal class ProcessTransactionServicesClass
    {
        //variables
        string uid, bid, eid;
        int duration = 2000;
        int numberOfTries = 3;

        //Services
        DatabaseDirectoryServicesClass DatabaseDirectory;
        StandardFirebaseOperationsClass standardFirebaseOperationsClass;
        ProductProcessingServicesClass productProcessingServices;
        ProductServicesClass productServices;
        SDServicesClass sDServices;
        CategoryServicesClass categoryServices;
        UserServicesClass userServices;

        //Firebase Client
        IFirebaseClient? client;

        public ProcessTransactionServicesClass(string uid, string bid, string eid, IFirebaseClient client)
        {
            this.uid=uid;
            this.bid=bid;
            this.eid=eid;
            this.client=client;

            if (this.client == null)
            {
                //fine I'll do it myself
                FirebaseSetup firebaseSetup = new FirebaseSetup();
                this.client = firebaseSetup.setup(this.client);
            }

            DatabaseDirectory = new();
            standardFirebaseOperationsClass = new();
            productServices = new();
            productProcessingServices = new();
            categoryServices = new();
            userServices = new();
            sDServices = new();
        }

        public ProcessTransactionServicesClass(string uid, string bid, string eid)
        {
            this.uid=uid;
            this.bid=bid;
            this.eid=eid;

            FirebaseSetup firebaseSetup = new FirebaseSetup();
            client = firebaseSetup.setup(client);

            DatabaseDirectory = new();
            standardFirebaseOperationsClass = new();
            productServices = new();
            productProcessingServices = new();
            categoryServices = new();
            userServices = new();
            sDServices = new();
        }

        public void GenerateSD(QueuedSdDataClass queuedSd, bool queuedDataYes, Action<SourceDocumentClass2, bool> method)
        {
            ReconilliationClass reconilliationClass = new ReconilliationClass();
            bool result = reconilliationClass.IsConnectedToInternet();
            bool success = false;
            if(!result) if (queuedSd.referenceSdYes || queuedSd.freeEditYes)
                {
                    method(queuedSd.Sd, success);
                    return;
                }

            SourceDocumentClass2 sourceDocument = queuedSd.Sd;
            SourceDocumentClass2 referenceSd = queuedSd.referenceSd;

            if(referenceSd != null) if (queuedSd.referenceSdYes && referenceSd.sdType == "Invoice" && sourceDocument.sdType == "Receipt")
                {
                    if (sourceDocument.summations.Balance < sourceDocument.summations.Total)
                    {
                        sourceDocument.amountPaid = sourceDocument.summations.Balance;
                        sourceDocument.PartPaymentYes = true;
                    }
                }

            if (sourceDocument.sdType != "Delivery Note")//not a delivery note
            {
                if (queuedSd.freeEditYes && queuedSd.referenceSdYes && referenceSd != null)
                {
                    sourceDocument.EditHistory = referenceSd.EditHistory;//we get its history
                    MatchingClass matchingRef = new()
                    {
                        rid = referenceSd.folio,
                        date = referenceSd.date,
                        sdType = referenceSd.sdType
                    };
                    if (sourceDocument.EditHistory == null)
                    {
                        sourceDocument.EditHistory = new();
                    }
                    sourceDocument.EditHistory.Add(matchingRef.rid, matchingRef);//add it to history

                    //call function to set this file as an edited and returned
                    //set as returned but do not affect any of the matching, also set as edited
                    //nullify everything
                    sDServices.setAsEdited(referenceSd);//set as edited
                    sDServices.updateMathingReturns(referenceSd.Matching, referenceSd.products, queuedSd.isFsd, true, EmptyCall);//make it history                                                                                                                 //once this is over we start afreash as such we set the matching back to new
                }
                else
                {
                    if (queuedSd.referenceSdYes && referenceSd != null)
                    {
                        sourceDocument.matchingDNote = referenceSd.matchingDNote;
                        sourceDocument.dNoteUrl = referenceSd.dNoteUrl;
                        sourceDocument.Matching = referenceSd.Matching;//we override matching new, with that which has data
                    }
                }

                if (sourceDocument.Matching == null)
                {
                    sourceDocument.Matching = new();
                }

                MatchingClass matchingOG = new()//the first entry into matching is the document itself
                {
                    rid = sourceDocument.folio,
                    date = sourceDocument.date,
                    sdType = sourceDocument.sdType
                };

                if (!sourceDocument.Matching.ContainsKey(matchingOG.rid))
                    sourceDocument.Matching.Add(matchingOG.rid, matchingOG);
            }
            else
            {
                if(referenceSd != null)
                    sourceDocument.Matching = referenceSd.Matching;
            }

            ///<summary>
            ///here is the great divide between WildGrass generated and variable and imported data
            ///</summary>
            if (queuedSd.oldTransactionYes)
            {
                //method
                method(sourceDocument, true);
                if (result)
                {
                    sourceDocument.wildgrassGenerated = 1;
                    sDServices.OldTransaction(sourceDocument);

                    RemoveFromQueuedData(queuedSd.Sd.folio);
                }
                else
                {
                    //queue data
                    //call toast message to inform user that data has been queued
                    QueueData(queuedSd);
                }
            }
            else
            {
                sourceDocument.wildgrassGenerated = 0;
                //call function to generate sourceDocument

                if (!queuedDataYes) //if queued data, the transaction will already contain a source document
                {
                    pdfClass pdfOps = new pdfClass();
                    string pdfLocation = pdfOps.GeneratePDF(sourceDocument, true);
                    string pdfLocation2 = pdfOps.GeneratePDF(sourceDocument, false);

                    sourceDocument.pdfLink = pdfLocation;
                    sourceDocument.dirPdf = pdfLocation;
                    sourceDocument.pdfLink2 = pdfLocation2;
                    sourceDocument.dirPdf2 = pdfLocation2;
                }

                //method
                method(sourceDocument, true);

                if (result)
                {
                    if (sourceDocument.sdType == "Delivery Note")
                    {
                        sDServices.updateDeliveryNote(sourceDocument);
                    }
                    else
                    {
                        sDServices.UploadResources(sourceDocument);

                        if (sourceDocument.Matching.Count > 1)
                        {
                            sDServices.updateMatching(sourceDocument.Matching, queuedSd.isFsd);
                        }
                    }
                    RemoveFromQueuedData(queuedSd.Sd.folio);
                }
                else
                {
                    //queue data
                    QueueData(queuedSd);
                }
            }
        }

        public bool IsConnectedToInternet()
        {
            string host = "google.com";
            bool result = false;
            Ping p = new Ping();
            try
            {
                PingReply reply = p.Send(host, 3000);
                if (reply.Status == IPStatus.Success) return true;
            }
            catch
            {

            }
            return result;
        }

        public async void GenerateSD_online(QueuedSdDataClass queuedSd, bool queuedDataYes, Action<QueuedSdDataClass, bool> completeMethod, Action<QueuedSdDataClass, MemoryStream> printMethod, Action<QueuedSdDataClass> updateStatusMethod, bool invoiceStandard)
        {

            if (queuedSd.checklist == null) queuedSd.checklist = new();
            bool result = IsConnectedToInternet();
            if (result)
            {
                SourceDocumentClass2 referenceSd = queuedSd.referenceSd;

                if (queuedSd.Sd.isFsd)
                {
                } else
                {
                    //generate folio
                    if (!queuedSd.checklist.folio_generated)
                    {
                        if (invoiceStandard)
                        {
                            string type = "";
                            switch (queuedSd.Sd.sdCode)
                            {
                                case 0:
                                    type = "Receipt";
                                    break;

                                case 1:
                                    type = "Invoice";
                                    break;

                                case 2:
                                    type = "Quotation";
                                    break;

                                case 3:
                                    type = "TransferNote";
                                    break;

                                case 4:
                                    type = "Order";
                                    break;

                                case 5:
                                    type = "CreditNote";
                                    break;

                                case 6:
                                    type = "DeliveryNote";
                                    break;
                            }
                            //need to retrieve folio
                            SDServicesClass sDServices = new SDServicesClass();
                            string folioNo = await sDServices.GetContinuousFolioAsync(type);
                            if (folioNo != null)
                            {
                                queuedSd.checklist.folio_generated = true;
                                queuedSd.Sd.folio = folioNo;
                            }
                        }
                    }
                    updateStatusMethod(queuedSd);

                    //pdf generation and upload
                    if (!queuedSd.oldTransactionYes)
                    {
                        //upload pdf
                        if (!queuedSd.checklist.originalPdf_uploaded)
                        {
                            //generate pdf
                            MemoryStream originalPdf;
                            if (queuedSd.checklist.originalPdf_generated)
                            {
                                //retrieve the pdf as a stream
                                originalPdf = queuedSd.original;
                            }
                            else
                            {
                                pdfClass pdfOps = new pdfClass();
                                originalPdf = pdfOps.GeneratePDF_online(queuedSd.Sd, false);
                                queuedSd.original = originalPdf;

                                if (originalPdf != null)
                                {
                                    queuedSd.checklist.originalPdf_generated = true;
                                }
                            }
                            printMethod(queuedSd, originalPdf);

                            queuedSd = await UploadPdf(queuedSd, originalPdf, false);
                        }

                        if (!queuedSd.checklist.duplicatePdf_uploaded)
                        {
                            //generate pdf
                            MemoryStream duplicatePdf;
                            if (queuedSd.checklist.duplicatePdf_generated)
                            {
                                duplicatePdf = queuedSd.duplicate;
                            }
                            else
                            {
                                pdfClass pdfOps = new pdfClass();
                                duplicatePdf = pdfOps.GeneratePDF_online(queuedSd.Sd, true);
                                queuedSd.duplicate = duplicatePdf;

                                if (duplicatePdf != null)
                                {
                                    queuedSd.checklist.duplicatePdf_generated = true;
                                }
                            }
                            queuedSd = await UploadPdf(queuedSd, duplicatePdf, true);
                        }
                    }
                    updateStatusMethod(queuedSd);
                }

                //update matching and edits
                if (queuedSd.Sd.sdType != "Delivery Note")//not a delivery note
                {
                    //updating edits
                    if (queuedSd.freeEditYes && queuedSd.referenceSdYes && referenceSd != null)
                    {
                        queuedSd.Sd.EditHistory = referenceSd.EditHistory;//we get its history
                        MatchingClass matchingRef = new()
                        {
                            rid = referenceSd.folio,
                            date = referenceSd.date,
                            sdType = referenceSd.sdType
                        };
                        if (queuedSd.Sd.EditHistory == null)
                        {
                            queuedSd.Sd.EditHistory = new();
                        }
                        queuedSd.Sd.EditHistory.Add(matchingRef.rid, matchingRef);//add it to history

                        //how do we know if we where successfull
                        bool edit_updated = setAsEdited(referenceSd);//set as edited
                        queuedSd.checklist.edited_updated = edit_updated;

                        queuedSd.checklist = updateMathingReturns(queuedSd.checklist, referenceSd.Matching, referenceSd.products, queuedSd.isFsd, true);//make it history    
                        //once this is over we start afreash as such we set the matching back to new
                    }
                    else if (queuedSd.referenceSdYes && referenceSd != null)
                    {
                        queuedSd.Sd.matchingDNote = referenceSd.matchingDNote;
                        queuedSd.Sd.dNoteUrl = referenceSd.dNoteUrl;
                        queuedSd.Sd.Matching = referenceSd.Matching;//we override matching new, with that which has data
                    }

                    //creating matching file
                    if (queuedSd.Sd.Matching == null)
                    {
                        queuedSd.Sd.Matching = new();
                    }

                    MatchingClass matchingOG = new()//the first entry into matching is the document itself
                    {
                        rid = queuedSd.Sd.folio,
                        date = queuedSd.Sd.date,
                        sdType = queuedSd.Sd.sdType
                    };

                    if (!queuedSd.Sd.Matching.ContainsKey(matchingOG.rid))
                        queuedSd.Sd.Matching.Add(matchingOG.rid, matchingOG);

                    //updating matching
                    if (!queuedSd.oldTransactionYes)
                    {
                        if (queuedSd.Sd.Matching.Count > 1)
                        {
                            queuedSd.checklist = sDServices.updateMatching(queuedSd.Sd.Matching, queuedSd.isFsd, queuedSd.checklist);
                        }
                    }
                }
                updateStatusMethod(queuedSd);

                //update product
                ProductProcessingServicesClass productProcessingServices = new();
                queuedSd = await productProcessingServices.SDProductUpdate(queuedSd);
                updateStatusMethod(queuedSd);

                //upload metadata
                if (queuedSd.oldTransactionYes)
                {
                    queuedSd.Sd.wildgrassGenerated = 1;
                    sDServices.OldTransaction(queuedSd.Sd);
                }
                else
                {
                    queuedSd.Sd.wildgrassGenerated = 0;
                    if (queuedSd.Sd.sdType == "Delivery Note")
                    {
                        if (referenceSd != null)
                            queuedSd.Sd.Matching = referenceSd.Matching;

                        sDServices.updateDeliveryNote(queuedSd.Sd);
                    }
                    else
                    {
                        sDServices.UploadTransactionData(queuedSd.Sd);
                    }
                }
            }

            completeMethod(queuedSd, result);
            return;
        }

        public bool setAsEdited(SourceDocumentClass2 sd)
        {
            bool success = false;

            DateTime dtt = DateTime.Now;
            string date = dtt.ToString("MMM, dd yyyy");
            string timeToPass = dtt.ToString("HH:mm:ss tt");

            sd.EditedYes = true;
            sd.EditedDate = date;
            sd.EditedTime = timeToPass;


            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    if (sd.date != null) if (sd.folio != null) if (sd.folio.Length > 0 && sd.date.Length > 0)
                            {
                                switch (sd.sdType)
                                {
                                    case "Receipt":
                                        FirebaseResponse firebasea = client.Set(DatabaseDirectory.AllReceipts() + "/" + sd.folio, sd);
                                        FirebaseResponse firebasea1 = client.Set(DatabaseDirectory.Receipts() + "/" + sd.date + "/receipts/" + sd.folio, sd);
                                        break;

                                    case "Invoice":
                                        FirebaseResponse firebasea2 = client.Set(DatabaseDirectory.AllInvoices() + "/" + sd.folio, sd);
                                        FirebaseResponse firebasea3 = client.Set(DatabaseDirectory.Invoices() + "/" + sd.date + "/invoices/" + sd.folio, sd);
                                        break;

                                    case "Quotation":
                                        FirebaseResponse firebase2 = client.Set(DatabaseDirectory.AllInvoices() + "/" + sd.folio, sd);
                                        break;

                                    case "Transfer Note":
                                        FirebaseResponse firebase3 = client.Set(DatabaseDirectory.TransferNotes() + "/" + sd.folio, sd);
                                        break;

                                    case "Order":
                                        FirebaseResponse firebase4 = client.Set(DatabaseDirectory.Orders() + "/" + sd.folio, sd);
                                        break;

                                    case "Credit Note":
                                        FirebaseResponse firebase5 = client.Set(DatabaseDirectory.CreditNotes() + "/" + sd.folio, sd);
                                        break;

                                    case "Debit Note":
                                        FirebaseResponse firebase6 = client.Set(DatabaseDirectory.DebitNote() + "/" + sd.folio, sd);
                                        break;
                                }
                                success = true;
                            }
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(duration);
                }
            }
            return success;
        }

        public SDChecklistClass updateMathingReturns(SDChecklistClass checklistClass, Dictionary<string, MatchingClass> matchingArray, Dictionary<string, ProductEntrySDClass> products, bool isFsd, bool returnProductsYes)
        {
            if (checklistClass.matchingReturnsSd_completed == null) checklistClass.matchingReturnsSd_completed = new();
            Dictionary<string, string?> updated_completed = checklistClass.matchingReturnsSd_completed;

            bool success = false;

            if (matchingArray != null) if (matchingArray.Count > 0)
                {
                    foreach (var entry in matchingArray)
                    {
                        if (!updated_completed.ContainsKey(entry.Key))
                        {
                            if (isFsd)
                            {
                                switch (entry.Value.sdType)
                                {
                                    case "Receipt":
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceiptsIn() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.ReceiptsIn() + "/" + entry.Value.date + "/sd/" + entry.Value.rid + "/matchingReturns", "true");
                                                        }
                                                updated_completed.Add(entry.Key, entry.Key);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                        standardFirebaseOperationsClass.UpdateVersion("receiptIn");
                                        break;
                                    case "Invoice":
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoicesIn() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.InvoicesIn() + "/" + entry.Value.date + "/sd/" + entry.Value.rid + "/matchingReturns", "true");
                                                        }
                                                updated_completed.Add(entry.Key, entry.Key);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                        standardFirebaseOperationsClass.UpdateVersion("invoiceIn");
                                        break;
                                }
                            }
                            else
                            {
                                switch (entry.Value.sdType)
                                {
                                    case "Receipt":
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllReceipts() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Receipts() + "/" + entry.Value.date + "/receipts/" + entry.Value.rid + "/matchingReturns", "true");
                                                        }
                                                updated_completed.Add(entry.Key, entry.Key);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                        standardFirebaseOperationsClass.UpdateVersion("receipt");
                                        break;
                                    case "Invoice":
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.AllInvoices() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                            FirebaseResponse firebase1 = client.Set(DatabaseDirectory.Invoices() + "/" + entry.Value.date + "/invoices/" + entry.Value.rid + "/matchingReturns", "true");
                                                        }
                                                updated_completed.Add(entry.Key, entry.Key);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                        standardFirebaseOperationsClass.UpdateVersion("invoice");
                                        break;
                                    case "Quotation":
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.Quotations() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        }
                                                updated_completed.Add(entry.Key, entry.Key);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                        standardFirebaseOperationsClass.UpdateVersion("quotation");
                                        break;
                                    case "Order":
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.Orders() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        }
                                                updated_completed.Add(entry.Key, entry.Key);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                        standardFirebaseOperationsClass.UpdateVersion("order");
                                        break;
                                    case "Transfer Note":
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.TransferNotes() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        }
                                                updated_completed.Add(entry.Key, entry.Key);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                        standardFirebaseOperationsClass.UpdateVersion("transferNote");
                                        break;
                                    case "Debit Note":
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.DebitNote() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        }
                                                updated_completed.Add(entry.Key, entry.Key);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                        standardFirebaseOperationsClass.UpdateVersion("debitNote");
                                        break;
                                    case "Credit Note":
                                        for (int i = 0; i < numberOfTries; i++)
                                        {
                                            try
                                            {
                                                success = false;
                                                if (entry.Value.date != null) if (entry.Value.rid != null) if (entry.Value.rid.Length > 0 && entry.Value.date.Length > 0)
                                                        {
                                                            FirebaseResponse firebase = client.Set(DatabaseDirectory.CreditNotes() + "/" + entry.Value.rid + "/matchingReturns", "true");
                                                        }
                                                updated_completed.Add(entry.Key, entry.Key);
                                                success = true;
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                Thread.Sleep(duration);
                                            }
                                        }
                                        standardFirebaseOperationsClass.activityLog(entry.Value.rid, "transaction", "Returns", eid);
                                        standardFirebaseOperationsClass.UpdateVersion("creditNote");
                                        break;
                                }
                            }
                        }
                    }
                }

            bool product_success = false;
            if (returnProductsYes)//always return to stock then if returnProducts is false, remove them as damages
            {
                //return
                ProductProcessingServicesClass processingServicesClass = new();
                if (isFsd)
                {
                    productProcessingServices.updateQuantityDeduction(products);
                }
                else
                {
                    processingServicesClass.updateQuantityEntry(products, false, "");
                }
            }
            else
            {
                //write off as damaged
            }
            checklistClass.matchingReturnsSd_updated = success;
            checklistClass.matchingReturnsSd_completed = updated_completed;
            return checklistClass;//yes but at what point did it fail, how much damage has been done and can we fix it?
        }

        public async Task<QueuedSdDataClass> UploadPdf(QueuedSdDataClass queuedSd, MemoryStream? stream, bool duplicate)
        {
            string addMe = "";
            if (duplicate)
            {
                addMe = "_duplicate";
            }
            int NumberOfRetries = 3;
            int DelayOnRetry = 3000;
            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    // Construct FirebaseStorage, path to where we want to upload the file and Put it there
                    var task = new FirebaseStorage("long-walk-pos.appspot.com")
                        .Child(queuedSd.Sd.sdType + "s")
                        .Child(queuedSd.Sd.folio + addMe + ".pdf")
                        .PutAsync(stream);

                    // await the task to wait until upload completes and get the download url
                    var downloadUrl = await task;
                    if (duplicate)
                    {
                        queuedSd.Sd.pdfLink = downloadUrl;
                        queuedSd.checklist.duplicatePdf_uploaded = true;
                    } else
                    {
                        queuedSd.Sd.pdfLink2 = downloadUrl;
                        queuedSd.checklist.originalPdf_uploaded = true;
                    }
                    break;
                }
                catch (IOException e) when (i <= NumberOfRetries)
                {
                    Thread.Sleep(DelayOnRetry);
                }
            }

            return queuedSd;
        }

        public QueuedSdDataClass updateProducts(QueuedSdDataClass queuedSd)
        {
            if (queuedSd.Sd.sdType == "Receipt" || queuedSd.Sd.sdType == "Invoice" || queuedSd.Sd.sdType == "Transfer Note")
            {
                bool runDeduction = true;
                if (queuedSd.Sd.Matching != null) if (queuedSd.Sd.Matching.Count > 0)
                    {
                        foreach (var item in queuedSd.Sd.Matching)
                        {
                            if (item.Value.sdType == "Receipt" || item.Value.sdType == "Invoice")
                            {
                                if (item.Value.rid != queuedSd.Sd.folio)
                                {
                                    runDeduction = false;
                                    queuedSd.checklist.productQuantity_updated = true;
                                    queuedSd.checklist.productQC_updated = true;
                                    queuedSd.checklist.productPC_updated = true;
                                }
                            }
                        }
                    }
                if (runDeduction)
                {
                    productProcessingServices.updateQuantity(queuedSd.Sd);
                } 


            }



            return queuedSd;
        }

        public async Task<SDChecklistClass> UploadResources(QueuedSdDataClass queuedSd, Action<QueuedSdDataClass, bool> method)
        {
            bool successOriginal = false;
            bool successDuplicate = false;
            SourceDocumentClass2 sourceDocument = queuedSd.Sd;

            if (sourceDocument.sdType == "Receipt" || sourceDocument.sdType == "Invoice" || sourceDocument.sdType == "Transfer Note")
            {
                bool runDeduction = true;
                if (sourceDocument.Matching != null) if (sourceDocument.Matching.Count > 0)
                    {
                        foreach (var item in sourceDocument.Matching)
                        {
                            if (item.Value.sdType == "Receipt" || item.Value.sdType == "Invoice")
                            {
                                if (item.Value.rid != sourceDocument.folio)
                                {
                                    runDeduction = false;
                                }
                            }
                        }
                    }
                if (runDeduction)
                {
                    productProcessingServices.updateQuantity(sourceDocument);
                }
            }

            int NumberOfRetries = 3;
            int DelayOnRetry = 3000;
            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    // Construct FirebaseStorage, path to where we want to upload the file and Put it there
                    var task = new FirebaseStorage("long-walk-pos.appspot.com")
                        .Child(sourceDocument.sdType + "s")
                        .Child(sourceDocument.folio + "_duplicate.pdf")
                        .PutAsync(queuedSd.duplicate);

                    // Track progress of the upload
                    task.Progress.ProgressChanged += (s, e) => doneUploading(s, e);

                    // await the task to wait until upload completes and get the download url
                    var downloadUrl = await task;
                    queuedSd.Sd.pdfLink = downloadUrl;
                    queuedSd.checklist.duplicatePdf_uploaded = true;
                    break;
                }
                catch (IOException e) when (i <= NumberOfRetries)
                {
                    Thread.Sleep(DelayOnRetry);
                }
            }

            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    // Construct FirebaseStorage, path to where we want to upload the file and Put it there
                    var task = new FirebaseStorage("long-walk-pos.appspot.com")
                        .Child(sourceDocument.sdType + "s")
                        .Child(sourceDocument.folio + ".pdf")
                        .PutAsync(queuedSd.original);

                    // Track progress of the upload
                    task.Progress.ProgressChanged += (s, e) => doneUploading(s, e);

                    // await the task to wait until upload completes and get the download url
                    var downloadUrl = await task;
                    queuedSd.Sd.pdfLink2 = downloadUrl;
                    queuedSd.checklist.originalPdf_uploaded = true;
                    break;
                }
                catch (IOException e) when (i <= NumberOfRetries)
                {
                    Thread.Sleep(DelayOnRetry);
                }
            }

            return queuedSd.checklist;
        }

        private void doneUploading(object s, FirebaseStorageProgress e)
        {
            Console.WriteLine($"Progress: {e.Percentage} %");
            if (e.Percentage == 100)
            {
                //MessageBox.Show("We are at 100%", "system testing");
            }
        }

        private void RemoveFromQueuedData(string folio)
        {
            //retrieve or create que
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            string dir = complete + @"\data\" + uid + @"\businesses\" + bid;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            dir += @"\QueuedData.txt";

            Dictionary<string, QueuedSdDataClass> queuedDataArray = new Dictionary<string, QueuedSdDataClass>();
            if (File.Exists(dir))
            {
                using (StreamReader r = new StreamReader(dir))
                {
                    string json = r.ReadToEnd();
                    if (json != null)
                    {
                        queuedDataArray = JsonConvert.DeserializeObject<Dictionary<string, QueuedSdDataClass>>(json);
                    }
                }
            }

            if (queuedDataArray == null)
            {
                queuedDataArray = new Dictionary<string, QueuedSdDataClass>();
            }

            if (queuedDataArray.ContainsKey(folio))
            {
                queuedDataArray.Remove(folio);
            }

            if(queuedDataArray.Count > 0)
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        string data = JsonConvert.SerializeObject(queuedDataArray);
                        File.WriteAllText(dir, data);
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(duration);
                    }
                }
            } else
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        File.Delete(dir);
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(duration);
                    }
                }
                try
                {
                    System.IO.File.WriteAllText(dir, "");
                }
                catch (Exception)
                {

                }
            }

        }

        private void QueueData(QueuedSdDataClass queuedData)
        {
            //retrieve or create que
            string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string complete = Path.Combine(systemPath, "WildGrass");
            string dir = complete + @"\data\" + uid + @"\businesses\" + bid;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            dir += @"\QueuedData.txt";

            Dictionary<string, QueuedSdDataClass> queuedDataArray = new Dictionary<string, QueuedSdDataClass>();
            if (File.Exists(dir))
            {
                using (StreamReader r = new StreamReader(dir))
                {
                    string json = r.ReadToEnd();
                    if (json != null)
                    {
                        queuedDataArray = JsonConvert.DeserializeObject<Dictionary<string, QueuedSdDataClass>>(json);
                    }
                }
            }

            if (queuedDataArray == null)
            {
                queuedDataArray = new Dictionary<string, QueuedSdDataClass>();
            }


            if (!queuedDataArray.ContainsKey(queuedData.Sd.folio))
            {
                queuedDataArray.Add(queuedData.Sd.folio, queuedData);
            }


            for (int i = 0; i < numberOfTries; i++)
            {
                try
                {
                    string data = JsonConvert.SerializeObject(queuedDataArray);
                    File.WriteAllText(dir, data);
                }
                catch (Exception ex)
                {
                    Thread.Sleep(duration);
                }
            }
        }

        private void EmptyCall(bool success)
        {

        }
    }
}
