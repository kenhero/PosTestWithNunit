using NUnit.Framework;
using System;
using Microsoft.PointOfService;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Collections;
using System.Linq;
using System.Text;
//using LibraryTests;
using System.Xml.Linq;
using System.Xml;


// Used for file and directory
// manipulation
using System.IO;
using System.Reflection;
using System.Dynamic;

using System.Net;


using FiscalReceipt.Library;
using System.Threading;
//using FiscalDocument.Library;
//using Report.Library;

namespace AppConsolePosTestWithNunit
{


    [TestFixture]
    public class CustomTests
    {
        //log var
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //IEnumerable<XElement> child1 = XDocument.Load("myConfiguration.xml").Element("State").Elements();

        readonly string printerName = "fiscalPrinter";
        readonly string electronicJournalName = "ElectronicJournal";
        static FiscalReceipt.Library.FiscalReceipt mc;
        static FiscalReceipt.Library.Lottery lt;
        static ElectronicJournal.Library.ElectronicJournal ej;
        static FiscalReceipt.Library.VatManager vm;
        static string[] listofTests;
        static FiscalReceipt.Library.WebService ws;
        static FiscalReceipt.Library.Xml2 xml2;

        //FiscalDocument.Library.FiscalDocument fd;
        //Report.Library.Report rp;


        public CustomTests()
        {
            //FileInfo fileInfo = new FileInfo(@"E:\PosTestWithNunit\AppConsolePosTestWithNunit\log4net.config");
            //log4net.Config.XmlConfigurator.Configure(fileInfo);
        }



        [SetUp]
        public void InitAccount()
        {
            //arrange
            //string printerName = "fiscalPrinter1";
            mc = new FiscalReceipt.Library.FiscalReceipt();
            //ej = new ElectronicJournal.Library.ElectronicJournal();
            //vm = new FiscalReceipt.Library.VatManager();
            //fd = new FiscalDocument.Library.FiscalDocument(printerName);
            //rp = new Report.Library.Report(printerName);
            listofTests = listOfTest();
        }





        //[Test, TestCaseSource("listOfTests")]
        [Test]
        //parsing della directory XmlFolder con tutti i file xml di test
        public void DynamicTest(string[] fileArray)
        {
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    //parsing della directory XmlFolder con tutti i file xml di test
                    //..\..\..\..\XmlFolder\

                    //NOTA IMP: Mettere il path assoluto della cartella XmlFolder altrimenti se si lancia NUnit Test prende come riferimento non dove si trova il codice ma dove si trova Nunit.exe ,ossia dentro visual studio IDe!!!
                    //string[] fileArray = Directory.GetFiles(@"E:\PosTestWithNunit\XmlFolder", "*.txt", SearchOption.TopDirectoryOnly);

                    foreach (string namefile in fileArray)
                    {

                        //test to read class,method and params from XML parser and create the right class and call the right method
                        string textFileParserResult = namefile;
                        //SharedClass.Globals.FILE_NAME = namefile.Substring(12);

                        //Per trasferire il nome del file xml nel contesto di log4net
                        log4net.GlobalContext.Properties["XmlFile"] = namefile.Substring(45).Replace("txt", "xml");

                        StreamReader sr = File.OpenText(textFileParserResult);

                        string line = null;

                        //Init object class that will be invoked
                        object instanceOld = null;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string className = line;
                            string methodName = sr.ReadLine();
                            object[] parameters = new object[10];

                            int i = 0;
                            while ((line = sr.ReadLine()) != "")
                            {

                                //per ogni metodo da chiamare ho almeno 3 informazioni ,nel seguente ordine:
                                //1) namespace.classe,nameofDll
                                //2) method
                                //3) params (0 o + params)
                                //4) numero di iterazioni da effettuare
                                parameters[i] = line;
                                i++;



                            }
                            //07/08/19 inserisco una locazione in piu' 
                            //per passargli l'ultimo parametro che sarà il nomedelfile xml in cui è stato scritto questo test
                            var newArray = new object[i - 1];
                            Array.Copy(parameters, 0, newArray, 0, newArray.Length);
                            //indica quante volte devo iterare il metodo
                            int repetitor = Convert.ToInt32(parameters[i - 1]);


                            Type type = Type.GetType(className);
                            object instanceNew;
                            if (type != null)
                            {

                                if (instanceOld != null)
                                {
                                    var oldtype = instanceOld.GetType();
                                    if (type == oldtype) // same previous class, don't create a new object!!!
                                    {
                                        instanceNew = instanceOld;
                                        //Sta parte di codice rivedibile,vorrei fare un singleton cosi' buonanotte ai check
                                    }
                                    else
                                    {
                                        //State transition, need a new object(state)
                                        instanceNew = Activator.CreateInstance(type);

                                    }
                                }
                                else
                                {
                                    instanceNew = Activator.CreateInstance(type); //first object (non mi piace cmq)

                                }

                                /*
                                if (Activator.ReferenceEquals(instanceNew, instanceOld) == true)
                                {
                                    //sto richiamando la stessa classe ma solo metodo differente,non DEVO
                                    //creare un nuovo oggetto
                                    instanceNew = instanceOld;
                                }
                                */
                                MethodInfo method = type.GetMethod(methodName);

                                if (method != null)
                                {
                                    for (int j = 0; j < repetitor; ++j)
                                    { //method.Invoke(instance, new object[] { par1 });
                                        method.Invoke(instanceNew, newArray);
                                    }
                                    instanceOld = instanceNew;
                                }
                                else
                                {
                                    log.Error("Errore nel parsing del metodo " + methodName + " invocato all'interno del file " + namefile);
                                }

                            }

                        }
                        sr.Close();
                    }
                });
            }
            catch (Exception e)
            {
                log.Error("", e);
            }


            /*
            // Arrange
            FiscalReceipt.Library.FiscalReceipt mc = new FiscalReceipt.Library.FiscalReceipt();
            // Act
            int output = mc.testFiscalReceiptClass(printerName);
            //mc.testFiscalReceiptClass();
            Console.WriteLine("output test = " + output);
            //Console.ReadLine();
            */
            // Assert
            //Assert.AreEqual(0, FiscalReceipt.Library.FiscalReceipt.NumExceptions);
            //Assert.Fail();
            

        }

        static private string[] listOfTest()
        {
            
            //NOTA IMP: Mettere il path assoluto della cartella XmlFolder altrimenti se si lancia NUnit Test prende come riferimento non dove si trova il codice ma dove si trova Nunit.exe ,ossia dentro visual studio IDe!!!
            string[] fileArray = Directory.GetFiles("D:\\Epson_Copia_Chiavetta_Gialla2\\ToolAggiornato\\PosTestWithNunit\\XmlFolder", "*.txt", SearchOption.TopDirectoryOnly);
            Array.Sort(fileArray);
            return fileArray;
        }


        //Metodo che testa la PrintRecItem,in particolare il contatore del numero degli scontrini fiscali e il totale giornaliero dopo una scontrino di vendita
        public void TestPrintRecItem(string description, string price, string quantity, string vatInfo, string unitPrice)
        {

            log.Info("Performing TestPrintRecItem Method");

            //gc,gc2 sono due "oggetti" che memorizzano i totalizzatori generali,passati alla printRecItem
            //gc memorizza la situazione prima della vendita, gc2 dopo la vendita
            //Sarà premura di questa aggiornarli prima e dopo le vendite
            FiscalReceipt.Library.GeneralCounter gc, gc2;
            gc = new FiscalReceipt.Library.GeneralCounter();
            gc2 = new FiscalReceipt.Library.GeneralCounter();


            //Aggiorniamo i contatori VAT su xml
            VatRecord vr = new VatRecord();
            vr.SetVatCounters();

            //Leggiamo il corrispettivo della IVA di cui andremo a fare una vendita (vatInfo e Item)

            string vatBefore = VatManager.getVatTableEntry(vatInfo.ToString());
            //Lordo giornaliero relativo all'aliquota Iva selezionata
            string ItemBefore = VatRecord.GetVatCounter("Day", vatBefore, "Item");
            //Netto giornaliero relativo all'aliquota Iva selezionata
            string NetBefore = VatRecord.GetVatCounter("Day", vatBefore, "Net");


            int output = mc.PrintRecItem(description, price, quantity, vatInfo, unitPrice, ref gc, ref gc2);
            //Qui devo fare il compare tra gc e gc2 e vedere se c'è coerenza in base alla printerecitem e printrectotal

            //Aggiorniamo i contatori VAT su xml
            VatRecord vr2 = new VatRecord();
            vr2.SetVatCounters();

            //Leggiamo i corrispettivi dell'iva relativa alle vendite (VatInfo) DOPO la  mc.PrintRecItem e verifichiamo la coerenza dei totalizzatori con l'entità della vendita effettuata
            string vatAfter = VatManager.getVatTableEntry(vatInfo.ToString()); //questa obv non cambia

            //Lordo giornaliero relativo all'aliquota Iva selezionata
            string ItemAfter = VatRecord.GetVatCounter("Day", vatAfter, "Item");
            //Netto giornaliero relativo all'aliquota Iva selezionata
            string NetAfter = VatRecord.GetVatCounter("Day", vatAfter, "Net");


            try
            {
                //EDIT: 29/01/2020 mi sa che questo test errore generico lo devo togliere, purtroppo numException è global e quindi dovrei
                //azzerarlo qualche volta altrimenti non fa riferimento all ultimo test effettuato
                Assert.AreEqual(0, output, "TestPrintRecItem Failed, generic error");
                //Faccio una vendita di n oggetti per cui mi aspetto che il totalizzatorei ScontriniFiscali incrementi di 1
                Assert.AreEqual(Int32.Parse(gc.FiscalRec) + 1, Int32.Parse(gc2.FiscalRec), "TestPrintRecItem Failed on gc.FiscalRec, expected " + gc.FiscalRec + 1 + "Received " + gc2.FiscalRec + "\r\n", gc.FiscalRec, gc2.FiscalRec);
                //Nel test faccio TRE vendite di (unitPrice * quantity) oggetti
                Assert.AreEqual((Int32.Parse(gc.DailyTotal) + (Int32)(Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3)), Int32.Parse(gc2.DailyTotal), "TestPrintRecItem Failed on gc.DailyTotal, expected " + (gc.DailyTotal + (Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3)) + "Received " + gc2.DailyTotal + "\r\n", gc.DailyTotal, gc2.DailyTotal);
                //Test sul Gran Totale (non posso farlo qui perchè dovrei fare un zreport e mi saltano i due test precedenti)
                //Assert.AreEqual((Int64.Parse(gc.GrandTotal) + (Int64)(Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3)), Int64.Parse(gc2.GrandTotal), "TestPrintRecItem Failed on gc.GrandTotal, expected " + (gc.GrandTotal + (Int64.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3)) + "Received " + gc2.GrandTotal + "\r\n", gc.GrandTotal, gc2.GrandTotal);

                //Test corrispettivo
                double precision = 100;
                Assert.AreEqual((double)Int32.Parse(NetBefore) + (double)((Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3)) / (1 + double.Parse(vatBefore) / 10000), (double)Int32.Parse(NetAfter), precision, "TestPrintRecItem Failed sul totalizzatore " + vatAfter + " expected " + (double.Parse(NetBefore) + (double)((Int32.Parse(quantity) / 1000 * Int32.Parse(unitPrice) * 3)) / (1 + double.Parse(vatBefore) / 10000)).ToString("#.##") + " Received " + NetAfter.ToString() + "\r\n");


            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }



        //Metodo che testa la PrintRecItem,in particolare il contatore del numero degli scontrini fiscali e il totale giornaliero dopo una scontrino di vendita
        public void Test50MicroScontrini(string description, string price, string quantity, string vatIndex, string unitPrice)
        {

            log.Info("Performing TestPrintRecItem Method");

            //gc,gc2 sono due "oggetti" che memorizzano i totalizzatori generali,passati alla printRecItem
            //gc memorizza la situazione prima della vendita, gc2 dopo la vendita
            //Sarà premura di questa aggiornarli prima e dopo le vendite
            FiscalReceipt.Library.GeneralCounter gc, gc2;
            gc = new FiscalReceipt.Library.GeneralCounter();
            gc2 = new FiscalReceipt.Library.GeneralCounter();

            GeneralCounter.SetGeneralCounter();

            //Load general counter
            gc = GeneralCounter.GetGeneralCounter();


            //Aggiorniamo i contatori VAT su xml
            VatRecord vr = new VatRecord();
            vr.SetVatCounters();

            //Leggiamo il corrispettivo della IVA di cui andremo a fare una vendita (vatInfo e Item)

            string vatBefore = VatManager.getVatTableEntry(vatIndex.ToString());
            //Lordo giornaliero relativo all'aliquota Iva selezionata
            string ItemBefore = VatRecord.GetVatCounter("Day", vatBefore, "Item");
            //Netto giornaliero relativo all'aliquota Iva selezionata
            string NetBefore = VatRecord.GetVatCounter("Day", vatBefore, "Net");

            int output = 0;
            for (int i = 0; i < 50; ++i)
            {
                //output += mc.PrintRecItem(description, price, quantity, vatInfo, unitPrice, ref gc, ref gc2);


                mc.BeginFiscalReceipt("true");

                //Eseguo TRE vendite dello stesso oggetto,stesso prezzo unitario e quantità e iva specificata dal parametro vatIndex della funzione , poi testo i totalizzatori relativi prima e dopo la vendita
                //The price parameter is not used if the unit price is different from 0(the amount is computed from the fiscal printer multiplying the unit price and the quantity).The unitName parameter is not used. Set on the SetupPOS application to print the quantity line, even if it's 1
                //Console.WriteLine("Performing PrintRecItem() method ");
                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatIndex), decimal.Parse(unitPrice), "");
                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatIndex), decimal.Parse(unitPrice), "");
                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatIndex), decimal.Parse(unitPrice), "");

                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.PrintRecTotal((decimal)10000, (decimal)(Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3), "0CASH");
                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.EndFiscalReceipt(true);

                

                }
            //Qui devo fare il compare tra gc e gc2 e vedere se c'è coerenza in base alla printerecitem e printrectotal

            GeneralCounter.SetGeneralCounter();

            //Leggiamo valori nuovi
            gc2 = GeneralCounter.GetGeneralCounter();


            //Aggiorniamo i contatori VAT su xml
            VatRecord vr2 = new VatRecord();
            vr2.SetVatCounters();

            //Leggiamo i corrispettivi dell'iva relativa alle vendite (VatInfo) DOPO la  mc.PrintRecItem e verifichiamo la coerenza dei totalizzatori con l'entità della vendita effettuata
            string vatAfter = VatManager.getVatTableEntry(vatIndex.ToString()); //questa obv non cambia

            //Lordo giornaliero relativo all'aliquota Iva selezionata
            string ItemAfter = VatRecord.GetVatCounter("Day", vatAfter, "Item");
            //Netto giornaliero relativo all'aliquota Iva selezionata
            string NetAfter = VatRecord.GetVatCounter("Day", vatAfter, "Net");


            try
            {

                Assert.AreEqual(0, output, "Test TestPrintRecItem Failed, generic error");
                //Faccio una vendita di n oggetti per cui mi aspetto che il totalizzatorei ScontriniFiscali incrementi di 1
                Assert.AreEqual(Int32.Parse(gc.FiscalRec) + 50, Int32.Parse(gc2.FiscalRec), "TestPrintRecItem Failed on gc.FiscalRec, expected " + gc.FiscalRec + 50 + "Received " + gc2.FiscalRec + "\r\n", gc.FiscalRec, gc2.FiscalRec);
                //Nel test faccio TRE vendite di (unitPrice * quantity) oggetti
                Assert.AreEqual((Int32.Parse(gc.DailyTotal) + (Int32)(Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 150)), Int32.Parse(gc2.DailyTotal), "TestPrintRecItem Failed on gc.DailyTotal, expected " + (gc.DailyTotal + (Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 150)) + "Received " + gc2.DailyTotal + "\r\n", gc.DailyTotal, gc2.DailyTotal);
                //Test sul Gran Totale (non posso farlo qui perchè dovrei fare un zreport e mi saltano i due test precedenti)
                //Assert.AreEqual((Int64.Parse(gc.GrandTotal) + (Int64)(Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3)), Int64.Parse(gc2.GrandTotal), "TestPrintRecItem Failed on gc.GrandTotal, expected " + (gc.GrandTotal + (Int64.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3)) + "Received " + gc2.GrandTotal + "\r\n", gc.GrandTotal, gc2.GrandTotal);

                //Test corrispettivo
                double precision = 1000;
                //price è / 10000; quantity = /1000; unitprice è in centesimi, ergo  /100 NOTA IMP: il "150" deriva da 3 (num di vendite dentro il singolo scontrino) * 50 (indice i =  numero di iterazioni ossia di scontrini consecutivi che eseguo)
                Assert.AreEqual((double)Int32.Parse(NetBefore) + (double)((Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 150)) / (1 + double.Parse(vatBefore) / 10000), (double)Int32.Parse(NetAfter), precision, "TestPrintRecItem Failed sul totalizzatore reparto Iva " + vatAfter + ", Expected " + (double.Parse(NetBefore) + (double)((Int32.Parse(quantity) / 1000 * Int32.Parse(unitPrice) * 1500)) / (1 + double.Parse(vatBefore) / 10000)).ToString("#.##") + " Received " + double.Parse(NetAfter).ToString("#.##") + "\r\n", NetBefore, NetAfter);


            }
            catch (AssertionException e)
            {
                log.Error(e.Message, e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa la PrintRecItem in combo con la Lotteria, in particolare il contatore del numero degli scontrini fiscali e il totale giornaliero dopo una scontrino di vendita con Lotteria
        public void TestMicroScontriniLottery(string description, string price, string quantity, string vatIndex, string unitPrice, string IdLotteryCode, string Operator)
        {

            log.Info("Performing TestMicroScontriniLottery Method");

            //gc,gc2 sono due "oggetti" che memorizzano i totalizzatori generali
            //gc memorizza la situazione prima della sequenza di vendite, gc2 dopo la sequenza
            FiscalReceipt.Library.GeneralCounter gc, gc2;
            gc = new FiscalReceipt.Library.GeneralCounter();
            gc2 = new FiscalReceipt.Library.GeneralCounter();

            GeneralCounter.SetGeneralCounter();

            //Load general counter
            gc = GeneralCounter.GetGeneralCounter();


            //Aggiorniamo i contatori VAT su xml
            VatRecord vr = new VatRecord();
            vr.SetVatCounters();

            //Leggiamo il corrispettivo della IVA di cui andremo a fare una vendita (vatInfo e Item)

            string vatBefore = VatManager.getVatTableEntry(vatIndex.ToString());
            //Lordo giornaliero relativo all'aliquota Iva selezionata
            string ItemBefore = VatRecord.GetVatCounter("Day", vatBefore, "Item");
            //Netto giornaliero relativo all'aliquota Iva selezionata
            string NetBefore = VatRecord.GetVatCounter("Day", vatBefore, "Net");

            int output;

            lt = new Lottery();

            output = lt.MicroScontriniLottery(description, price, quantity, vatIndex, unitPrice, IdLotteryCode, Operator);



            //Qui devo fare il compare tra gc e gc2 e vedere se c'è coerenza in base al metodo MicroScontriniLottery

            GeneralCounter.SetGeneralCounter();

            //Leggiamo valori nuovi
            gc2 = GeneralCounter.GetGeneralCounter();


            //Aggiorniamo i contatori VAT su xml
            VatRecord vr2 = new VatRecord();
            vr2.SetVatCounters();

            //Leggiamo i corrispettivi dell'iva relativa alle vendite (VatInfo) DOPO la  lt.MicroScontriniLottery e verifichiamo la coerenza dei totalizzatori con l'entità della vendita effettuata
            string vatAfter = VatManager.getVatTableEntry(vatIndex.ToString()); //questa obv non cambia

            //Lordo giornaliero relativo all'aliquota Iva selezionata
            string ItemAfter = VatRecord.GetVatCounter("Day", vatAfter, "Item");
            //Netto giornaliero relativo all'aliquota Iva selezionata
            string NetAfter = VatRecord.GetVatCounter("Day", vatAfter, "Net");


            try
            {

                Assert.AreEqual(0, output, "TestMicroScontriniLottery Failed, generic error");
                //Faccio una vendita di n oggetti per cui mi aspetto che il totalizzatorei ScontriniFiscali incrementi di 1
                Assert.AreEqual(Int32.Parse(gc.FiscalRec) + 50, Int32.Parse(gc2.FiscalRec), "TestMicroScontriniLottery Failed on gc.FiscalRec, expected " + gc.FiscalRec + 50 + "Received " + gc2.FiscalRec + "\r\n", gc.FiscalRec, gc2.FiscalRec);
                //Nel test faccio TRE vendite di (unitPrice * quantity) oggetti
                Assert.AreEqual((Int32.Parse(gc.DailyTotal) + (Int32)(Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 150)), Int32.Parse(gc2.DailyTotal), "TestMicroScontriniLottery Failed on gc.DailyTotal, expected " + (gc.DailyTotal + (Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 150)) + "Received " + gc2.DailyTotal + "\r\n", gc.DailyTotal, gc2.DailyTotal);
                //Test sul Gran Totale (non posso farlo qui perchè dovrei fare un zreport e mi saltano i due test precedenti)
                //Assert.AreEqual((Int64.Parse(gc.GrandTotal) + (Int64)(Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3)), Int64.Parse(gc2.GrandTotal), "TestPrintRecItem Failed on gc.GrandTotal, expected " + (gc.GrandTotal + (Int64.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3)) + "Received " + gc2.GrandTotal + "\r\n", gc.GrandTotal, gc2.GrandTotal);

                //Test corrispettivo
                double precision = 1000;
                //price è / 10000; quantity = /1000; unitprice è in centesimi, ergo  /100 NOTA IMP: il "150" deriva da 3 (num di vendite dentro il singolo scontrino) * 50 (indice i =  numero di iterazioni ossia di scontrini consecutivi che eseguo)
                Assert.AreEqual((double)Int32.Parse(NetBefore) + (double)((Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 150)) / (1 + double.Parse(vatBefore) / 10000), (double)Int32.Parse(NetAfter), precision, "TestMicroScontriniLottery Failed sul totalizzatore reparto Iva " + vatAfter + ", Expected " + (double.Parse(NetBefore) + (double)((Int32.Parse(quantity) / 1000 * Int32.Parse(unitPrice) * 1500)) / (1 + double.Parse(vatBefore) / 10000)).ToString("#.##") + " Received " + double.Parse(NetAfter).ToString("#.##") + "\r\n", NetBefore, NetAfter);


            }
            catch (AssertionException e)
            {
                log.Error(e.Message, e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }





        //Eseguo TRE vendite dello stesso oggetto, stesso prezzo unitario e quantità ma iva diversa(VatIndex 1 2 3) , 
        //poi testo il granTotal sul metodo TestGranTotal prima e dopo le vendite
        public void TestGranTotal()
        {
            log.Info("Performing TestGranTotal Method");

            //gc,gc2 sono due "oggetti" che leggono i totalizzatori generali,passati alla GranTotal
            //gc memorizza la situazione prima della vendita, gc2 dopo la vendita
            //Sarà premura di questa aggiornarli prima e dopo le vendite
            FiscalReceipt.Library.GeneralCounter gc, gc2;
            gc = new FiscalReceipt.Library.GeneralCounter();
            gc2 = new FiscalReceipt.Library.GeneralCounter();

            int output = mc.GranTotal(ref gc, ref gc2);
            //Qui devo fare il compare tra gc e gc2 e vedere se c'è coerenza in base alla printerecitem e printrectotal

            try
            {

                Assert.AreEqual(0, output, "Test TestPrintRecItem Failed, generic error");
                //Test sul Gran Totale (non posso farlo qui perchè dovrei fare un zreport e mi saltano i due test precedenti)
                Assert.AreEqual((Int64.Parse(gc.GrandTotal) + (Int64)(Int32.Parse("10000") / 1000 * decimal.Parse("100000") * 3)), Int64.Parse(gc2.GrandTotal), "TestPrintRecItem Failed on gc.GrandTotal, expected " + (gc.GrandTotal + (Int64.Parse("10000") / 1000 * decimal.Parse("100000") * 3)) + "Received " + gc2.GrandTotal + "\r\n", gc.GrandTotal, gc2.GrandTotal);


            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa resi e annulli. Le info sono memorizzate e ottenibili dalla DirectIO 2050 (get daily data),con vari indici
        //Metodo creato per testare i totalizzatori quotidiani che si ottengono con la directIO 2050 : in particolare verifico la
        //coerenza dei totalizzatori dei resi e degli annulli : Faccio prima una vendita normale,poi una seconda con 3 aliquote IVA differenti,
        //poi faccio n scontrini separati di resi in modo da azzerare completamente la vendità multi IVa .
        //Infine faccio una ulteriore vendita e subito dopo un annullo dello scontrino precedente.
        //In totale faro' 10 scontrini e un totale di 4.84 euro di vendite (due scontrini da 1.92 , e uno di 1 euro,gli altri sono resi parziali e/o annulli) 
        //Nella funzione che poi andrà a testare questo metodo leggero i totalizzatori , prima e dopo questo metodo e verifichero' che i totalizatori modificati
        //siano coerenti con tutto cio' che ho fatto qui dentro
        public void TestPrintRecRefound()
        {
            log.Info("Performing TestPrintRecRefound Method");

            FiscalReceipt.Library.RetrieveData rData;
            rData = new FiscalReceipt.Library.RetrieveData();

            try
            {

                int TotFiscRec = Int32.Parse(rData.getDailyData("24")); // Totale Scontrini Fiscali

                int TotGiorn = Int32.Parse(rData.getDailyData("28")); // Totale Giornaliero, non scende se c'è reso o annullo ma sale sempre

                int TotResi = Int32.Parse(rData.getDailyData("36")); // Totale Resi

                int TotDicVoided = Int32.Parse(rData.getDailyData("37")); // Totale Doc Annullati

                //Iva pagata per aliquota IVA 1 (22%)
                int DailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Iva pagata per aliquota IVA 2 (10%)
                int DailyTax02 = Int32.Parse(rData.getDailyData("40", "2").Substring(9, 9));

                //Iva pagata per aliquota IVA 3 (4%)
                int DailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));


                //chiamo il metodo PrintRecRefound
                int output = mc.PrintRecRefound();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo PrintRecRefound


                try
                {
                    Assert.AreEqual(0, output, "Errore nella PrintRecRefound ");
                    Assert.AreEqual(TotFiscRec + 10, Int32.Parse(rData.getDailyData("24")), "Test Total Fiscal Receipt Failed");

                    Assert.AreEqual(TotGiorn + 484, Int32.Parse(rData.getDailyData("28")), "Test Totale Giornaliero Failed");

                    Assert.AreEqual(TotResi + 192, Int32.Parse(rData.getDailyData("36")), "Test Totale Resi Failed");

                    Assert.AreEqual(TotDicVoided + 192, Int32.Parse(rData.getDailyData("37")), "Test Totale Doc Annullati Failed");

                    double precision = 1;

                    double appoggio = Double.Parse(rData.getDailyData("40", "1").Substring(9, 9));
                    Assert.AreEqual((double)DailyTax01 + 36, appoggio, precision, "Test getDailyData Failed , index 40 , VatRate 01 " + " expected " + (double)DailyTax01 + 540 + " Received " + appoggio + "\r\n");

                    appoggio = Double.Parse(rData.getDailyData("40", "2").Substring(9, 9));
                    Assert.AreEqual((double)DailyTax02 + 2, appoggio, precision, "Test getDailyData Failed , index 40 , VatRate 02 " + " expected " + (double)DailyTax01 + 22 + " Received " + appoggio + "\r\n");

                    appoggio = Double.Parse(rData.getDailyData("40", "3").Substring(9, 9));
                    Assert.AreEqual((double)DailyTax03 + 6, appoggio, precision, "Test getDailyData Failed , index 40 , VatRate 03 " + " expected " + (double)DailyTax01 + 24 + " Received " + appoggio + "\r\n");

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }

            }

            catch (Exception err)
            {
                //Generic error
                log.Error("", err);
            }

        }



        //Metodo che testa resi e annulli CON LOTTERIA. Le info sono memorizzate e ottenibili dalla DirectIO 2050 (get daily data),con vari indici
        //Metodo creato per testare i totalizzatori quotidiani che si ottengono con la directIO 2050 : in particolare verifico la
        //coerenza dei totalizzatori dei resi e degli annulli in concomitanza della LOTTERIA: Faccio prima una vendita normale (Con LOTTERIA) ,poi una seconda (Con LOTTERIA) con 3 aliquote IVA differenti,
        //poi faccio n scontrini separati di resi in modo da azzerare completamente la vendità multi IVa .
        //Infine faccio una ulteriore vendita (Con LOTTERIA) e subito dopo un annullo dello scontrino precedente.
        //In totale faro' 10 scontrini e un totale di 4.84 euro di vendite (due scontrini da 1.92 , e uno di 1 euro,gli altri sono resi parziali e/o annulli) 
        //Nella funzione che poi andrà a testare questo metodo leggero i totalizzatori , prima e dopo questo metodo e verifichero' che i totalizatori modificati
        //siano coerenti con tutto cio' che ho fatto qui dentro
        public void TestLotteryPrintRecRefound(string description)
        {
            log.Info("Performing TestLotteryPrintRecRefound Method");

            FiscalReceipt.Library.RetrieveData rData;
            rData = new FiscalReceipt.Library.RetrieveData();

            try
            {

                int TotFiscRec = Int32.Parse(rData.getDailyData("24")); // Totale Scontrini Fiscali

                int TotGiorn = Int32.Parse(rData.getDailyData("28")); // Totale Giornaliero, non scende se c'è reso o annullo ma sale sempre

                int TotResi = Int32.Parse(rData.getDailyData("36")); // Totale Resi

                int TotDicVoided = Int32.Parse(rData.getDailyData("37")); // Totale Doc Annullati

                //Iva pagata per aliquota IVA 1 (22%)
                int DailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Iva pagata per aliquota IVA 2 (10%)
                int DailyTax02 = Int32.Parse(rData.getDailyData("40", "2").Substring(9, 9));

                //Iva pagata per aliquota IVA 3 (4%)
                int DailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));


                //chiamo il metodo PrintRecRefound

                lt = new Lottery();
                lt.LotteryPrintRecRefound(description);

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo PrintRecRefound


                try
                {
                    Assert.AreEqual(TotFiscRec + 10, Int32.Parse(rData.getDailyData("24")), "Test Total Fiscal Receipt Failed");

                    Assert.AreEqual(TotGiorn + 484, Int32.Parse(rData.getDailyData("28")), "Test Totale Giornaliero Failed");

                    Assert.AreEqual(TotResi + 192, Int32.Parse(rData.getDailyData("36")), "Test Totale Resi Failed");

                    Assert.AreEqual(TotDicVoided + 192, Int32.Parse(rData.getDailyData("37")), "Test Totale Doc Annullati Failed");

                    double precision = 1;

                    double appoggio = Double.Parse(rData.getDailyData("40", "1").Substring(9, 9));
                    Assert.AreEqual((double)DailyTax01 + 36, appoggio, precision, "Test getDailyData Failed , index 40 , VatRate 01 " + " expected " + (double)DailyTax01 + 540 + " Received " + appoggio + "\r\n");

                    appoggio = Double.Parse(rData.getDailyData("40", "2").Substring(9, 9));
                    Assert.AreEqual((double)DailyTax02 + 2, appoggio, precision, "Test getDailyData Failed , index 40 , VatRate 02 " + " expected " + (double)DailyTax01 + 22 + " Received " + appoggio + "\r\n");

                    appoggio = Double.Parse(rData.getDailyData("40", "3").Substring(9, 9));
                    Assert.AreEqual((double)DailyTax03 + 6, appoggio, precision, "Test getDailyData Failed , index 40 , VatRate 03 " + " expected " + (double)DailyTax01 + 24 + " Received " + appoggio + "\r\n");

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }

            }

            catch (Exception err)
            {
                //Generic error
                log.Error("", err);
            }

        }


        // 11/12/19  
        // Test per verificare coerenza tra output del comando 1134 (Read Lottery Status) e i file di risposta dell' AdE ai doc. Lotteria
        // A differenza dei metodi nella classe Lottery cerchiamo di automatizzare il test in modo da verificare in automatico la coerenza 
        // tra cio' che parso e leggo e cio' che mi da il comando analogo del firmware
        // input: date (YYYYMMDD)

        public void TestReadLotteryStatus(string date)
        {
            log.Info("Performing TestReadLotteryStatus Method");
            try
            {
                //mc = new FiscalReceipt.Library.FiscalReceipt();
                ws = new FiscalReceipt.Library.WebService();
                lt = new FiscalReceipt.Library.Lottery();

                //mc.initFiscalDevice(printerName);
                //mc.ResetPrinter();
                //ws.commuteMode("RT");
                //string regex = "20191112";
                ws.OuterHTMLParser(date);

            }
            catch (PosControlException e)
            {
                //POS Error
                log.Error("PosControlException: " + e.Message, e);
            }
            catch (Exception err)
            {
                //Generic error
                log.Error("", err);
            }
        }





        //Metodo che testa sette forme di pagamento (scontrini) e poi controlla che il totale doc vendite
        //Sia aggiornato con il totale delle vendite effettuate in questo test.
        //Sono tutte vendite fate al 22% di aliquota 
        public void TestFormePagamento()
        {
            log.Info("Performing TestFormePagamento Method");

            FiscalReceipt.Library.RetrieveData rData;
            rData = new FiscalReceipt.Library.RetrieveData();

            try
            {


                int TotGiorn = Int32.Parse(rData.getDailyData("28")); // Totale Giornaliero, non scende se c'è reso o annullo ma sale sempre


                //Iva pagata per aliquota IVA 1 (22%)
                int DailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int DailyNet01 = Int32.Parse(rData.getDailyData("40" , "1").Substring(0, 9));


                //chiamo il metodo PrintRecRefound
                int output = mc.FormePagamento();



                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo FormePagamento

                int TotGiornUpdate = Int32.Parse(rData.getDailyData("28")); // Totale Giornaliero, non scende se c'è reso o annullo ma sale sempre


                //Iva pagata per aliquota IVA 1 (22%)
                int DailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int DailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                try
                {
                    Assert.AreEqual(TotGiorn + 111000, TotGiornUpdate, "Test Totale Giornaliero Failed sul metodo TestFormePagamento TotGiorn, expected " + (TotGiorn + 7000) + "received " + TotGiornUpdate);

                    Assert.AreEqual(DailyTax01 + DailyNet01 + 111000, DailyTax01Updated + DailyNet01Updated, "Test GetDailyData Failed sul metodo TestFormePagamento Tax + Net , expected " + (DailyTax01 + DailyNet01 + 7000).ToString() +  " received " + DailyTax01Updated + DailyNet01Updated);

                    Assert.AreEqual(DailyTax01 + 20016, DailyTax01Updated , "Test GetDailyData indice 40 Failed sul metodo TestFormePagamento DailyTax01, expected " + (DailyTax01 + 20017) + " received " + DailyTax01Updated );

                    Assert.AreEqual(DailyNet01 + 90984, DailyNet01Updated, "Test GetDailyData indice 40 Failed sul metodo TestFormePagamento DailyNet01 , expected " + (DailyNet01 + 90983) + " received " + DailyNet01Updated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }

            }

            catch (Exception err)
            {
                //Generic error
                log.Error("", err);
            }

        }


        //Metodo che testa sette forme di pagamento (scontrini) con Lotteria e poi controlla che il totale giornaliero
        //Sia aggiornato con il totale delle vendite effettuate in questo test.
        //Sono tutte vendite fate al 22% di aliquota
        //Inoltre gli scontrini con lotteria vengono stampati con l'orario in anticipo, con l'orario in avanti e con l'orario
        //Nella zona di interdizione (22-00) e si verifica che non ci siano errori generici
        //Infineviene ripristinato l'ORARIO
        //Andrebbe teoricamente controllato nel WebService che i doc lotteria siano stati inviati con esito AC
        public void TestFormePagamentoLottery(string IdLotteryCode, string Operator)
        {
            log.Info("Performing TestFormePagamentoLottery Method");

            FiscalReceipt.Library.RetrieveData rData;
            rData = new FiscalReceipt.Library.RetrieveData();

            try
            {

                //Questo è un test funzionale, per verificare che la lotteria non abbia causato errori di regressione nei totalizzatori
                int TotGiorn = Int32.Parse(rData.getDailyData("28")); // Totale Giornaliero, non scende se c'è reso o annullo ma sale sempre


                //Iva pagata per aliquota IVA 1 (22%)
                int DailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int DailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                lt = new Lottery();
                
                //chiamo il metodo SendLotteryCodePagMisto
                int output = lt.SendLotteryCodePagMisto(IdLotteryCode, Operator);



                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo FormePagamento

                int TotGiornUpdate = Int32.Parse(rData.getDailyData("28")); // Totale Giornaliero, non scende se c'è reso o annullo ma sale sempre


                //Iva pagata per aliquota IVA 1 (22%)
                int DailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int DailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                try
                {
                    Assert.AreEqual(TotGiorn + 93000, TotGiornUpdate, "Test Totale Giornaliero Failed sul metodo TestFormePagamento, expected " + (TotGiorn + 93000) + "received " + TotGiornUpdate);

                    Assert.AreEqual(DailyTax01 + DailyNet01 + 93000, DailyTax01Updated + DailyNet01Updated, "Test GetDailyData Failed sul metodo TestFormePagamento , expected " + (DailyTax01 + DailyNet01 + 7000).ToString() + " received " + DailyTax01Updated + DailyNet01Updated);

                    Assert.AreEqual(DailyTax01 + 16770, DailyTax01Updated, "Test GetDailyData indice 40 Failed sul metodo TestFormePagamento , expected " + (DailyTax01 + 16770).ToString() + " received " + DailyTax01Updated);

                    Assert.AreEqual(DailyNet01 + 76230, DailyNet01Updated, "Test GetDailyData indice 40 Failed sul metodo TestFormePagamento , expected " + (DailyNet01 + 76230).ToString() + " received " + DailyNet01Updated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }

                try
                {
                    //Sposto l'orario alle 6 del mattino, quindi sicuramente la sposto indietro
                    mc.ChangeTime("0600");
                    //chiamo il metodo SendLotteryCodePagMisto
                    output = lt.SendLotteryCodePagMisto(IdLotteryCode, Operator);
                    Assert.AreEqual(0, output, "Errore all'interno del metodo SendLotteryCodePagMisto con orario indietro ");

                    //Sposto l'orario in avanti
                    mc.ChangeTime("1800");
                    //chiamo il metodo SendLotteryCodePagMisto
                    output = lt.SendLotteryCodePagMisto(IdLotteryCode, Operator);
                    Assert.AreEqual(0, output, "Errore all'interno del metodo SendLotteryCodePagMisto con orario indietro ");

                    //Sposto l'orario nella fascia in cui è vietato mandare xml di lotterie
                    mc.ChangeTime("2205");
                    //chiamo il metodo SendLotteryCodePagMisto
                    output = lt.SendLotteryCodePagMisto(IdLotteryCode, Operator);
                    Assert.AreEqual(0, output, "Errore all'interno del metodo SendLotteryCodePagMisto con orario indietro ");

                    //Ripristino orario
                    mc.ChangeTime();

                }
                catch (AssertionException e)
                {
                    //Ripristino orario
                    mc.ChangeTime();
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }

            }
            catch (Exception err)
            {
                //Generic error
                log.Error("", err);
            }
        }

        //Metodo che testa il Set Header Line number with Company
        public void TestSetHeaderLineWithCompany(string index, string Val)
        {
            log.Info("Performing TestSetHeaderLineWithCompany Method");
            try
            {
                lt = new Lottery();
                //chiamo il metodo SendLotteryCodePagMisto
                int output = lt.SetConfiguration(index , Val.PadLeft(3, '0'));
                Assert.AreEqual(0, output, "Errore all'interno del metodo SendLotteryCodePagMisto con orario indietro ");
            }
            catch (AssertionException e)
            {
                //NUnit Test exception
                log.Error("NUnit Test Exception", e);
            }
            catch (Exception err)
            {
                //Generic error
                log.Error("Generic Error", err);
            }
        }


        //Il nome è già esaustivo, Testa una sequenza di "adjustment" ossia di annulli e/o resi
        public void TestAdjustmentSequence()
        {
            log.Info("Performing TestAdjustmentSequence Method");

            try
            {
                mc = new FiscalReceipt.Library.FiscalReceipt();
                ws = new FiscalReceipt.Library.WebService();
                mc.resetPrinter();
                int output = 0;

                /*
                //Switch to MF
                ws.commuteMode("MF");

                output = mc.AdjustmentSequence();
                //La AdjustmentSequence ha all'interno una sequenza accettata SOLO in MF ma VIETATA in RT/Demo per cui quì non deve generare errori
                Assert.AreEqual(0, output);

                mc = new FiscalReceipt.Library.FiscalReceipt();
                mc.resetPrinter();
                //Mi metto prima in modalità Demo per fare il test
                ws.commuteMode("Demo");

                //Reset contatore errori
                output = 0;
                output = mc.AdjustmentSequence();

                //Assert.AreEqual(0, output);
                // output > 0 significa che la AdjustmentSequence DEVE generare una eccezione,ossia fallire perchè dentro ci sono delle
                //operazioni che non sono legali in Demo o RT
                Assert.Greater(output, 0);
                log.Error("Eccezione generata aspettata,Test Passed");
                ws.commuteMode("RT");
                */
                //Reset contatore errori
                output = 0;
                
                output = mc.AdjustmentSequence();

                //Assert.AreEqual(0, output);
                // output > 0 significa che la AdjustmentSequence DEVE generare una eccezione,ossia fallire perchè dentro ci sono delle
                //operazioni che non sono legali in Demo o RT
                Assert.Greater(output, 0);
                log.Error("Eccezione generata aspettata,Test Passed");
            }
            catch (AssertionException e)
            {
                //NUnit Test exception
                log.Error("NUnit Test Exception", e);
            }

            catch (Exception err)
            {
                //Generic error
                log.Error("Generic Error", err);
            }

        }


        //Test per verificare coerenza tra Zrep scritto su WebService e Zreport from Memoria Fiscale
        public void TestZrepJsonFile()
        {
            log.Info("Performing TestZrepJsonFile Method");
            try
            {

                FiscalReceipt.Library.GeneralCounter gc, gc2;
                FiscalReceipt.Library.WebService ws;
                gc = new FiscalReceipt.Library.GeneralCounter();
                FiscalReceipt.Library.GeneralCounter.SetGeneralCounter();
                gc = FiscalReceipt.Library.GeneralCounter.GetGeneralCounter();

                ws = new FiscalReceipt.Library.WebService();

                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();

                int ZrepFromDirectIo = Int32.Parse(rData.getDailyData("27")); //Chiusure giornaliere Z Report

                int ZrepFromDriver = Int32.Parse(gc.ZRep);
                int ZrepFromJson = ws.ZrepJsonFile();

                Assert.AreEqual(ZrepFromDirectIo, ZrepFromDriver, "Errore incongruenza tra ZrepfromDirectIO e Zrep from Driver .NET");

                Assert.AreEqual(ZrepFromDirectIo, ZrepFromJson, "Errore incongruenza tra ZrepfromDirectIO e Zrep from Web Service Json File");

                Assert.AreEqual(ZrepFromDriver, ZrepFromJson, "Errore incongruenza tra Zrep from Driver .NET e Zrep from Web Service Json File");
            }

            catch (AssertionException e)
            {
                //NUnit Test exception
                log.Error("NUnit Test Exception", e);
            }


            catch (Exception err)
            {
                //Generic error
                log.Error("Generic Error", err);
            }

}


        public void TestMolinari()
        {
            log.Info("Performing TestMolinari Method");
            int output = 0;
            try
            {
                mc = new FiscalReceipt.Library.FiscalReceipt();
                ws = new FiscalReceipt.Library.WebService();
                mc.resetPrinter();
                //Mi metto prima in modalità Demo per fare il test perchè è qui che fallisce
                ws.commuteMode("Demo");

                //output+= mc.initFiscalDevice("FiscalPrinter");
                output+= mc.BeginFiscalReceipt("true");
                output+= mc.resetPrinter();

                /*
                FiscalReceipt.Library.FiscalReceipt.fiscalprinter = (FiscalPrinter)FiscalReceipt.Library.FiscalReceipt.posCommonFP;

                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.Open();

                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.Claim(1000);

                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.DeviceEnabled = true;

                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.ResetPrinter();
                */
                


                output+= mc.testFiscalReceiptClass("FiscalPrinter");
                output+= mc.resetPrinter();

                try
                {
                    Assert.AreEqual(0, output, "TestMolinari sulla EndFiscalReceipt Failed");

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }

            }
            catch (PosControlException e )
            {
                //POS Error
                log.Error("PosControlException: " + e.Message , e);
            }
            catch (Exception err)
            {
                //Generic error
                log.Error("", err);
            }
          
        }



        //Metodo che testa l'Acconto dell'Xml 2.0 Esempio 2 e poi controlla che le voci modificate dell'xml relativo
        //Siano aggiornate correttamente
        //31082020 Testero' i nuovi indici della getDailyData prima e dopo lo scontrino come ho fatto per le forme di pag std
        public void TestAccontoConsegnaBene()
        {
            log.Info("Performing TestAcconto Method");
            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();
                
                //Non serve qui
                //mc.ZReport();

                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                /*
                //Aggiorniamo i contatori VAT su xml
                FiscalReceipt.Library.VatRecord vr, vr2;
                vr = new VatRecord();
                vr2 = new VatRecord();

                vr.SetVatCounters();

                //La classe VatRecord posso anche non usarla perchè non mi serva quasi mai serializzare le imposte e in piu' il lordo e netto delle aliquote me li ricavo 
                //Leggiamo il corrispettivo della IVA di cui andremo a fare una vendita (vatInfo e Item)
                
                string vat1Before = VatManager.getVatTableEntry("1");

                //Netto ammontare per aliquota Iva 1 (22%) AMMONTARE 
                int DailyNet01bis = Convert.ToInt32(VatRecord.GetVatCounter("Day", vat1Before, "Net"));

                //Lordo giornaliero relativo all'aliquota Iva selezionata
                int ItemBefore = Convert.ToInt32(VatRecord.GetVatCounter("Day", vat1Before, "Item"));
                */


                int TotGiorn = Convert.ToInt32(gc.DailyTotal);

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int DailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int DailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                int DailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Netto ammontare per aliquota Iva 3 (4%) AMMONTARE
                int DailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

                //Iva pagata per aliquota IVA 13 (ES%) IMPOSTA
                int DailyTax13 = Int32.Parse(rData.getDailyData("40", "00").Substring(9, 9));

                //Netto ammontare per aliquota Iva 13 (ES%) AMMONTARE
                int DailyNet13 = Int32.Parse(rData.getDailyData("40", "00").Substring(0, 9));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.AccontoConsegnaBene(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto


                int TotGiornUpdate = Convert.ToInt32(gc2.DailyTotal);

                int TotAccontiUpdate = Int32.Parse(gc2.DailyAcconti);

                //Iva pagata per aliquota IVA 1 (22%)
                int DailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int DailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%)
                int DailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Netto ammontare per aliquota Iva 3 (4%)
                int DailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

                //Iva pagata per aliquota IVA 1 (ES%)
                int DailyTax13Updated = Int32.Parse(rData.getDailyData("40", "00").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (ES%)
                int DailyNet13Updated = Int32.Parse(rData.getDailyData("40", "00").Substring(0, 9));

                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(5000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty,ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                /*
                 * <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
                <PagatoContanti>100,00</PagatoContanti>
                <PagatoElettronico>60,00</PagatoElettronico>
                <Totali>
                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                Assert.AreEqual("100.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "100" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                Assert.AreEqual("60.00", xmlStruct.totali.PagatoElettronico, "Errore campo PagatoElettronico dentro l'xml , expected " + "60" + " received " + xmlStruct.totali.NumeroDocCommerciali);

                /*
                <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>9,02</Imposta>
                </IVA>
                <Ammontare>81,97</Ammontare>
                <ImportoParziale>40,98</ImportoParziale>
                <Beniinsospeso>40,98</Beniinsospeso>
                */
                Assert.AreEqual("9.02", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "9.02" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("81.97", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "81.97" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("40.98", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "40.98" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("40.98", xmlStruct.riepilogos[1].BeniInSospeso, "Errore campo Beniinsospeso relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "40.98" + " received " + xmlStruct.riepilogos[1].BeniInSospeso);

                /*
                 <IVA>
                <AliquotaIVA>4.00</AliquotaIVA>
                <Imposta>0,38</Imposta>
                </IVA>
                <Ammontare>9,62</Ammontare>
                <ImportoParziale>9,62</ImportoParziale>
                */
                Assert.AreEqual("0.38", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.38" + " received " + xmlStruct.riepilogos[4].Imposta);
                Assert.AreEqual("9.62", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "9.62" + " received " + xmlStruct.riepilogos[4].Ammontare);
                Assert.AreEqual("9.62", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "9.62" + " received " + xmlStruct.riepilogos[4].ImportoParziale);

                /*
                 * <Natura>N4<Natura>
                <Ammontare>100</Ammontare>
                <ImportoParziale>100,00</ImportoParziale>
                */
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[0].Ammontare);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[0].ImportoParziale);




                //*****************Test sui Totalizzatori**********************
                //TODO: ancora non pronti i totalizzatori nuovi
  
                try
                {
                    Assert.AreEqual(TotGiorn + 1600000, TotGiornUpdate, "Test Totale Giornaliero Failed sul metodo TestFormePagamento, expected " + (TotGiorn + 16000) + "received " + TotGiornUpdate);

                    Assert.AreEqual(DailyTax01 + DailyNet01 + 5000, DailyTax01Updated + DailyNet01Updated, "Test GetDailyData Failed sul metodo TestAcconto , expected " + (DailyTax01 + DailyNet01 + 5000).ToString() + " received " + DailyTax01Updated + DailyNet01Updated);

                    Assert.AreEqual(DailyTax01 + 902, DailyTax01Updated , "Test GetDailyData Failed sul metodo TestAcconto , expected " + (DailyTax01 + 902).ToString() + " received " + DailyTax01Updated);

                    Assert.AreEqual(DailyTax03 + DailyNet03 + 1000, DailyTax03Updated + DailyNet03Updated, "Test GetDailyData Failed sul metodo TestAcconto , expected " + (DailyTax03 + DailyNet03 + 1000).ToString() + " received " + DailyTax03Updated + DailyNet03Updated);

                    Assert.AreEqual(DailyTax03 + 038, DailyTax03Updated, "Test GetDailyData indice 40 Failed sul metodo TestAcconto , expected " + (DailyTax03 + 038).ToString() + " received " + DailyTax03Updated);

                    Assert.AreEqual(DailyNet13 + 10000, DailyNet13Updated, "Test GetDailyData indice 40 Failed sul metodo TestAcconto , expected " + (DailyNet13 + 10000).ToString() + " received " + DailyNet13Updated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }

            }
            catch (Exception err)
            {
                //Generic error
                log.Error("", err);
            }

        }


        
        //Metodo che testa il metodo AccontoGenerico Esempio 2Bis
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestAccontoGenerico()
        {
            log.Info("Performing TestAccontoGenerico Method");

            try
            {
                Assert.Multiple(() =>
                {   
                    //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                    FiscalReceipt.Library.RetrieveData rData;
                    rData = new FiscalReceipt.Library.RetrieveData();
                    xml2 = new Xml2();


                    FiscalReceipt.Library.GeneralCounter gc, gc2;
                    gc = new FiscalReceipt.Library.GeneralCounter();
                    gc2 = new FiscalReceipt.Library.GeneralCounter();

                    //Aggiorno i totalizatori generali (serialization)
                    GeneralCounter.SetGeneralCounter();

                    //Load general counter (deserialization)
                    gc = GeneralCounter.GetGeneralCounter();

                    int TotGiorn = Convert.ToInt32(gc.DailyTotal);

                    int TotScontoAPagare = Convert.ToInt32(gc.ScontoAPagare); // Totale ScontoAPagare

                    //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                    int DailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                    //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                    int DailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                    //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                    int DailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 3 (4%) AMMONTARE
                    int DailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));



                    string zRep = String.Empty;
                    //chiamo il metodo PrintRecRefound
                    int output = xml2.AccontoGenerico(ref zRep);



                    //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                    //Aggiorno i totalizatori generali
                    GeneralCounter.SetGeneralCounter();

                    //Load general counter
                    gc2 = GeneralCounter.GetGeneralCounter();

                    //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                    int TotGiornUpdate = Convert.ToInt32(gc2.DailyTotal);

                    int TotScontoAPagareUpdated = Convert.ToInt32(gc2.ScontoAPagare); // Totale ScontoAPagare

                    //Iva pagata per aliquota IVA 1 (22%)
                    int DailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 1 (22%)
                    int DailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                    //Iva pagata per aliquota IVA 3 (4%)
                    int DailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 3 (4%)
                    int DailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));


                    //Faccio chiusura
                    mc.ZReport();
                    Thread.Sleep(3000);

                    //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                    string data = DateTime.Now.ToString("yyyyMMdd");
                    //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                    Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                    xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                    //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare
                    /*
                    <Totali>
                    <NumeroDocCommerciali>2</NumeroDocCommerciali>
                    <PagatoContanti>172,00</PagatoContanti>
                    <ScontoApagare>22,00</ScontoApagare>
                    </Totali>
                    */

                    Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                    Assert.AreEqual("2", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "2" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                    Assert.AreEqual("172.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "172.00" + " received " + xmlStruct.totali.PagatoContanti);
                    Assert.AreEqual("22.00", xmlStruct.totali.ScontoApagare, "Errore campo ScontoApagare dentro l'xml , expected " + "22.00" + " received " + xmlStruct.totali.ScontoApagare);

                    /* <IVA>
                    <AliquotaIVA>22.00</AliquotaIVA>
                    <Imposta>22,00</Imposta>
                    </IVA>
                    <Ammontare>100,00</Ammontare>
                    <ImportoParziale>100,00</ImportoParziale>
                    */
                    Assert.AreEqual("22.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "22,00" + " received " + xmlStruct.riepilogos[1].Imposta);
                    Assert.AreEqual("100.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "100,00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                    Assert.AreEqual("100.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "100,00" + " received " + xmlStruct.riepilogos[1].ImportoParziale);

                    /*
                     <IVA>
                    <AliquotaIVA>4.00</AliquotaIVA>
                    <Imposta>1,92</Imposta>
                    </IVA>
                    <Ammontare>48,08</Ammontare>
                    <ImportoParziale>48,08</ImportoParziale>
                    */
                    Assert.AreEqual("1.92", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "1,92" + " received " + xmlStruct.riepilogos[4].Imposta);
                    Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48,08" + " received " + xmlStruct.riepilogos[4].Ammontare);
                    Assert.AreEqual("48.08", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48,08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);

                    /*
                    <Natura>N2<Natura>
                    <Ammontare>22,00</Ammontare>
                    <ImportoParziale>22,00</ImportoParziale>
                    */
                    Assert.AreEqual("22.00", xmlStruct.riepilogos[0].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "22,00" + " received " + xmlStruct.riepilogos[0].Ammontare);
                    Assert.AreEqual("22.00", xmlStruct.riepilogos[0].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "22,00" + " received " + xmlStruct.riepilogos[0].ImportoParziale);



                    //*****************Test sui Totalizzatori**********************


                   
                    Assert.AreEqual(TotGiorn + 1720000, TotGiornUpdate, "Test Totale Giornaliero Failed sul metodo TestAccontoGenerico, expected " + (TotGiorn + 1720000) + " received " + TotGiornUpdate);

                    Assert.AreEqual(TotScontoAPagare + 2200, TotScontoAPagareUpdated, "Test Totale Sconto A Pagare Failed sul metodo TestAccontoGenerico, expected " + (TotScontoAPagare + 2200) + " received " + TotScontoAPagareUpdated);

                    Assert.AreEqual(DailyTax01 + DailyNet01 + 12200, DailyTax01Updated + DailyNet01Updated, "Test GetDailyData Failed sul metodo TestAccontoGenerico , expected " + (DailyTax01 + DailyNet01 + 12200) + " received " + DailyTax01Updated + DailyNet01Updated);

                    Assert.AreEqual(DailyTax01 + 2200, DailyTax01Updated, "Test GetDailyData Failed sul metodo TestAccontoGenerico , expected " + (DailyTax01 + 2200).ToString() + " received " + DailyTax01Updated);

                    Assert.AreEqual(DailyTax03 + DailyNet03 + 5000, DailyTax03Updated + DailyNet03Updated, "Test GetDailyData indice 40 Failed sul metodo TestAccontoGenerico , expected " + (DailyTax03 + DailyNet03 + 5000) + " received " + DailyTax03Updated + DailyNet03Updated);

                 
               });
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
                
            }

        }


        //Metodo che testa il metodo VenditaBeniPagamentoNRBeneConsegnato Esempio 3
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestVenditaBeniPagamentoNRBeneConsegnato()
        {
            log.Info("Performing TestVenditaBeniPagamentoNRBeneConsegnato Method");

            try
            {
                Assert.Multiple(() => { 
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int TotGiorn = Convert.ToInt32(gc.DailyTotal);

                int nonRiscossoBeniServizi = Convert.ToInt32(gc.DailyNonRiscossoBeniServizi); // Totale Non Riscosso BeniServizi

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                int dailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Netto ammontare per aliquota Iva 3 (4%) AMMONTARE
                int dailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

                //Iva pagata per aliquota IVA 13 (ES%) IMPOSTA
                int dailyTax13 = Int32.Parse(rData.getDailyData("40", "00").Substring(9, 9));

                //Netto ammontare per aliquota Iva 13 (ES%) AMMONTARE
                int dailyNet13 = Int32.Parse(rData.getDailyData("40", "00").Substring(0, 9));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.VenditaBeniPagamentoNRBeneConsegnato(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int TotGiornUpdate = Convert.ToInt32(gc2.DailyTotal);

                int nonRiscossoBeniServiziUpdate = Convert.ToInt32(gc2.DailyNonRiscossoBeniServizi); // Totale Non Riscosso BeniServizi

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%)
                int dailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Netto ammontare per aliquota Iva 3 (4%)
                int dailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

                //Iva pagata per aliquota IVA 13 (ES%) IMPOSTA
                int dailyTax13Updated = Int32.Parse(rData.getDailyData("40", "00").Substring(9, 9));

                //Netto ammontare per aliquota Iva 13 (ES%) AMMONTARE
                int dailyNet13Updated = Int32.Parse(rData.getDailyData("40", "00").Substring(0, 9));
                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(3000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare
                /*
                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
                <PagatoContanti>210,00</PagatoContanti>
                <PagatoElettronico>80,00</PagatoElettronico>
                </Totali>
                 */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                Assert.AreEqual("210.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "210.00" + " received " + xmlStruct.totali.PagatoContanti);
                Assert.AreEqual("80.00", xmlStruct.totali.PagatoElettronico, "Errore campo PagatoElettronico dentro l'xml , expected " + "80.00" + " received " + xmlStruct.totali.PagatoElettronico);

                /* <IVA>
                    <AliquotaIVA>22.00</AliquotaIVA>
                    <Imposta>27,05</Imposta>
                    </IVA>
                    <Ammontare>122,95</Ammontare>
                    <ImportoParziale>122,95</ImportoParziale>
                */
                Assert.AreEqual("27.05", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "27,05" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("122.95", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122,95" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("122.95", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122,95" + " received " + xmlStruct.riepilogos[1].ImportoParziale);

                /*
                 <IVA>
                <AliquotaIVA>4.00</AliquotaIVA>
                <Imposta>1,92</Imposta>
                </IVA>
                <Ammontare>48,08</Ammontare>
                <ImportoParziale>48,08</ImportoParziale>
                */
                Assert.AreEqual("1.92", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "1,92" + " received " + xmlStruct.riepilogos[4].Imposta);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48,08" + " received " + xmlStruct.riepilogos[4].Ammontare);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48,08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);

                /*
                <Natura>N4<Natura>
                <Ammontare>100</Ammontare>
                <ImportoParziale>100,00</ImportoParziale>
                */
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100,00" + " received " + xmlStruct.riepilogos[0].Ammontare);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100,00" + " received " + xmlStruct.riepilogos[0].ImportoParziale);


                //*****************Test sui Totalizzatori**********************


               
                Assert.AreEqual(TotGiorn + 3000000, TotGiornUpdate, "Test Totale Giornaliero Failed sul metodo TestVenditaBeniPagamentoNRBeneConsegnato, expected " + (TotGiorn + 2900000) + "received " + TotGiornUpdate);

                Assert.AreEqual(nonRiscossoBeniServizi + 1000, nonRiscossoBeniServiziUpdate, "Test Totale nonRiscossoBeniServizi Failed sul metodo TestVenditaBeniPagamentoNRBeneConsegnato, expected " + (TotGiorn + 2900000) + "received " + TotGiornUpdate);

                Assert.AreEqual(dailyTax01 + dailyNet01 + 15000, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestVenditaBeniPagamentoNRBeneConsegnato , expected " + (dailyTax01 + dailyNet01 + 15000) + " received " + dailyTax01Updated + dailyNet01Updated);

                Assert.AreEqual(dailyTax01 + 2705, dailyTax01Updated, "Test GetDailyData Failed sul metodo TestVenditaBeniPagamentoNRBeneConsegnato , expected " + (dailyTax01 + 2705) + " received " + dailyTax01Updated);

                Assert.AreEqual(dailyTax03 + dailyNet03 + 5000, dailyTax03Updated + dailyNet03Updated, "Test GetDailyData indice 40 Failed sul metodo TestVenditaBeniPagamentoNRBeneConsegnato , expected " + (dailyTax03 + dailyNet03 + 500000) + " received " + dailyTax01Updated + dailyNet03Updated);

                Assert.AreEqual(dailyTax03 + 192, dailyTax03Updated, "Test GetDailyData indice 40 Failed sul metodo TestVenditaBeniPagamentoNRBeneConsegnato , expected " + (dailyTax03 + 19200) + " received " + dailyTax03Updated);

                Assert.AreEqual(dailyNet13 + 10000, dailyNet13Updated, "Test GetDailyData indice 40 Failed sul metodo TestVenditaBeniPagamentoNRBeneConsegnato , expected " + (dailyNet13 + 10000) + " received " + dailyNet13Updated);

               });
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }




        //Metodo che testa il metodo ServizioNonRiscosso Esempio 4
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestServizioNonRiscosso()
        {
            log.Info("Performing TestServizioNonRiscosso Method");

            try
            {
                Assert.Multiple(() =>
                {       
                    //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                    FiscalReceipt.Library.RetrieveData rData;
                    rData = new FiscalReceipt.Library.RetrieveData();
                    xml2 = new Xml2();


                    FiscalReceipt.Library.GeneralCounter gc, gc2;
                    gc = new FiscalReceipt.Library.GeneralCounter();
                    gc2 = new FiscalReceipt.Library.GeneralCounter();

                    //Aggiorno i totalizatori generali (serialization)
                    GeneralCounter.SetGeneralCounter();

                    //Load general counter (deserialization)
                    gc = GeneralCounter.GetGeneralCounter();

                    int totGiorn = Convert.ToInt32(gc.DailyTotal);

                    int nonRiscossoServizi = Convert.ToInt32(gc.DailyNonRiscossoServizi); // Totale Non Riscosso Servizi

                    //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                    int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                    //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                    int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                    //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                    int dailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 3 (4%) AMMONTARE
                    int dailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

                    //Iva pagata per aliquota IVA 13 (ES%) IMPOSTA
                    int dailyTax13 = Int32.Parse(rData.getDailyData("40", "00").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 13 (ES%) AMMONTARE
                    int dailyNet13 = Int32.Parse(rData.getDailyData("40", "00").Substring(0, 9));


                    string zRep = String.Empty;
                    //chiamo il metodo PrintRecRefound
                    int output = xml2.ServizioNonRiscosso(ref zRep);



                    //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                    //Aggiorno i totalizatori generali
                    GeneralCounter.SetGeneralCounter();

                    //Load general counter
                    gc2 = GeneralCounter.GetGeneralCounter();

                    //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                    int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                    int nonRiscossoServiziUpdated = Convert.ToInt32(gc2.DailyNonRiscossoServizi); // Totale Non Riscosso Servizi

                    //Iva pagata per aliquota IVA 1 (22%)
                    int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 1 (22%)
                    int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                    //Iva pagata per aliquota IVA 3 (4%)
                    int dailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 3 (4%)
                    int dailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

                    //Iva pagata per aliquota IVA 13 (ES%) IMPOSTA
                    int dailyTax13Updated = Int32.Parse(rData.getDailyData("40", "00").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 13 (ES%) AMMONTARE
                    int dailyNet13Updated = Int32.Parse(rData.getDailyData("40", "00").Substring(0, 9));
                    //Faccio chiusura
                    mc.ZReport();
                    

                    //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                    string data = DateTime.Now.ToString("yyyyMMdd");
                    //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                    Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                    xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                    //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare
                    /*
                    <Totali>
                    <NumeroDocCommerciali>1</NumeroDocCommerciali>
                    <PagatoContanti>210,00</PagatoContanti>
                    <PagatoElettronico>80,00</PagatoElettronico>
                    </Totali>
                     */
                    Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                    Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                    Assert.AreEqual("210.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "210.00" + " received " + xmlStruct.totali.PagatoContanti);
                    Assert.AreEqual("80.00", xmlStruct.totali.PagatoElettronico, "Errore campo PagatoElettronico dentro l'xml , expected " + "80.00" + " received " + xmlStruct.totali.PagatoElettronico);

                    /* <IVA>
                    <AliquotaIVA>22.00</AliquotaIVA>
                    <Imposta>26,15</Imposta>
                    </IVA>
                    <Ammontare>122,95</Ammontare>
                    <ImportoParziale>118,85</ImportoParziale>
                    <NonRiscossoServizi>4,10</NonRiscossoServizi>
                    */
                    Assert.AreEqual("26.15", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "26,15" + " received " + xmlStruct.riepilogos[1].Imposta);
                    Assert.AreEqual("118.85", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "118,85" + " received " + xmlStruct.riepilogos[1].Ammontare);
                    Assert.AreEqual("100.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "100,00" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                    Assert.AreEqual("4.10", xmlStruct.riepilogos[1].NonRiscossoServizi, "Errore campo NonRiscossoServizi relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "4,10" + " received " + xmlStruct.riepilogos[1].NonRiscossoServizi);

                    /*
                     <IVA>
                    <IVA>
                    <AliquotaIVA>4.00</AliquotaIVA>
                    <Imposta>1,86</Imposta>
                    </IVA>
                    <Ammontare>48,08</Ammontare>
                    <ImportoParziale>46,47</ImportoParziale>
                    <NonRiscossoServizi>1,60</NonRiscossoServizi>
                    */
                    Assert.AreEqual("1.86", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "1,86" + " received " + xmlStruct.riepilogos[4].Imposta);
                    Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48,08" + " received " + xmlStruct.riepilogos[4].Ammontare);
                    Assert.AreEqual("46.47", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "46,47" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                    Assert.AreEqual("1.61", xmlStruct.riepilogos[4].NonRiscossoServizi, "Errore campo NonRiscossoServizi relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "1,60" + " received " + xmlStruct.riepilogos[4].NonRiscossoServizi);

                    /*
                    <Natura>N4<Natura>
                    <Ammontare>100</Ammontare>
                    <ImportoParziale>96,67</ImportoParziale>
                    <NonRiscossoServizi>3,33</NonRiscossoServizi>
                    */
                    Assert.AreEqual("100.00", xmlStruct.riepilogos[0].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100,00" + " received " + xmlStruct.riepilogos[0].Ammontare);
                    Assert.AreEqual("3.33", xmlStruct.riepilogos[0].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "3,33" + " received " + xmlStruct.riepilogos[0].ImportoParziale);
                    Assert.AreEqual("3.33", xmlStruct.riepilogos[0].NonRiscossoServizi, "Errore campo NonRiscossoServizi relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "3,33" + " received " + xmlStruct.riepilogos[0].NonRiscossoServizi);




                    //*****************Test sui Totalizzatori**********************
                   

                  
                    Assert.AreEqual(totGiorn + 3000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestServizioNonRiscosso, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(nonRiscossoServizi + 1000, nonRiscossoServiziUpdated, "Test Totale Giornaliero Failed sul metodo TestServizioNonRiscosso, expected " + (nonRiscossoServizi + 1000) + "received " + nonRiscossoServiziUpdated);

                    Assert.AreEqual(dailyTax01 + dailyNet01 + 15000, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestServizioNonRiscosso , expected " + (dailyTax01 + dailyNet01 + 15000) + " received " + dailyTax01Updated + dailyNet01Updated);

                    Assert.AreEqual(dailyTax01 + 2615, dailyTax01Updated, "Test GetDailyData indice 40 Failed sul metodo TestServizioNonRiscosso , expected " + (dailyTax01 + 2615) + " received " + dailyTax03Updated);

                    Assert.AreEqual(dailyTax03 + dailyNet03 + 5000, dailyTax03Updated + dailyNet03Updated, "Test GetDailyData Failed sul metodo TestServizioNonRiscosso , expected " + (dailyTax03 + dailyNet03 + 5000) + " received " + dailyTax03Updated + dailyNet03Updated);

                    Assert.AreEqual(dailyTax03 + 186, dailyTax03Updated, "Test GetDailyData indice 40 Failed sul metodo TestServizioNonRiscosso , expected " + (dailyTax03 + 186) + " received " + dailyTax03Updated);

                    Assert.AreEqual(dailyNet13 + 10000, dailyNet13Updated, "Test GetDailyData indice 40 Failed sul metodo TestServizioNonRiscosso , expected " + (dailyNet13 + 10000) + " received " + dailyNet13Updated);

                
                   
               });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa il metodo SegueFattura Esempio 5
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestSegueFattura()
        {
            log.Info("Performing TestSegueFattura Method");

            try
            {
                Assert.Multiple(() =>
                {
                    //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                    FiscalReceipt.Library.RetrieveData rData;
                    rData = new FiscalReceipt.Library.RetrieveData();
                    xml2 = new Xml2();


                    FiscalReceipt.Library.GeneralCounter gc, gc2;
                    gc = new FiscalReceipt.Library.GeneralCounter();
                    gc2 = new FiscalReceipt.Library.GeneralCounter();

                    //Aggiorno i totalizatori generali (serialization)
                    GeneralCounter.SetGeneralCounter();

                    //Load general counter (deserialization)
                    gc = GeneralCounter.GetGeneralCounter();

                    int totGiorn = Convert.ToInt32(gc.DailyTotal);

                    int nonRiscossoFattura = Convert.ToInt32(gc.DailyNonRiscossoFatture); // Totale Non Riscosso Fatture

                    //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                    int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                    //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                    int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                    //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                    int dailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 3 (4%) AMMONTARE
                    int dailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

                    //Iva pagata per aliquota IVA 13 (ES%) IMPOSTA
                    int dailyTax13 = Int32.Parse(rData.getDailyData("40", "13").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 13 (ES%) AMMONTARE
                    int dailyNet13 = Int32.Parse(rData.getDailyData("40", "13").Substring(0, 9));


                    string zRep = String.Empty;
                    //chiamo il metodo PrintRecRefound
                    int output = xml2.SegueFattura(ref zRep);



                    //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                    //Aggiorno i totalizatori generali
                    GeneralCounter.SetGeneralCounter();

                    //Load general counter
                    gc2 = GeneralCounter.GetGeneralCounter();

                    //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                    int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                    int nonRiscossoFatturaUpdated = Convert.ToInt32(gc2.DailyNonRiscossoFatture); // Totale Non Riscosso Fatture

                    //Iva pagata per aliquota IVA 1 (22%)
                    int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 1 (22%)
                    int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                    //Iva pagata per aliquota IVA 3 (4%)
                    int dailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 3 (4%)
                    int dailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

                    //Iva pagata per aliquota IVA 13 (ES%) IMPOSTA
                    int dailyTax13Updated = Int32.Parse(rData.getDailyData("40", "13").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 13 (ES%) AMMONTARE
                    int dailyNet13Updated = Int32.Parse(rData.getDailyData("40", "13").Substring(0, 9));

                    //Faccio chiusura
                    mc.ZReport();


                    //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                    string data = DateTime.Now.ToString("yyyyMMdd");
                    //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                    Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                    xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                    //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                    /*
                    <Totali>
                    <NumeroDocCommerciali>1</NumeroDocCommerciali>
                    </Totali>
                    */
                    Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                    Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);

                    /* <IVA>
                   <AliquotaIVA>22.00</AliquotaIVA>
                    <Imposta>0,0</Imposta>
                    </IVA>
                    <Ammontare>122,95</Ammontare>
                    <ImportoParziale>0,00</ImportoParziale>
                    <NonRiscossoFatture>122,95</NonRiscossoFatture>
                    */


                    Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0,00" + " received " + xmlStruct.riepilogos[1].Imposta);
                    Assert.AreEqual("122.95", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122,95" + " received " + xmlStruct.riepilogos[1].Ammontare);
                    Assert.AreEqual("122.95", xmlStruct.riepilogos[1].NonRiscossoFatture, "Errore campo NonRiscossoFatture relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122,5" + " received " + xmlStruct.riepilogos[1].NonRiscossoFatture);
                    Assert.AreEqual("0.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].ImportoParziale + ", expected " + "0,00" + " received " + xmlStruct.riepilogos[1].ImportoParziale);

                    /*
                     <IVA>
                    <IVA>
                    <AliquotaIVA>4.00</AliquotaIVA>
                    <Imposta>0,00</Imposta>
                    </IVA>
                    <Ammontare>48,08</Ammontare>
                    <ImportoParziale>0,00</ImportoParziale>
                    <NonRiscossoFatture>48.08</NonRiscossoFatture>

                    */
                    Assert.AreEqual("0.00", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0,00" + " received " + xmlStruct.riepilogos[4].Imposta);
                    Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48,08" + " received " + xmlStruct.riepilogos[4].Ammontare);
                    Assert.AreEqual("0.00", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].ImportoParziale + ", expected " + "0,00" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                    Assert.AreEqual("48.08", xmlStruct.riepilogos[4].NonRiscossoFatture, "Errore campo NonRiscossoFatture relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48,08" + " received " + xmlStruct.riepilogos[4].NonRiscossoFatture);

                    /*
                    <Natura>N4<Natura>
                    <Ammontare>100,00</Ammontare>
                    <ImportoParziale>0,00</ImportoParziale>
                    <NonRiscossoFatture>100,00</NonRiscossoFatture>
                    */
                    Assert.AreEqual("100.00", xmlStruct.riepilogos[0].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100,00" + " received " + xmlStruct.riepilogos[0].Ammontare);
                    Assert.AreEqual("100.00", xmlStruct.riepilogos[0].NonRiscossoFatture, "Errore campo NonRiscossoFatture relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100,00" + " received " + xmlStruct.riepilogos[0].NonRiscossoFatture);
                    Assert.AreEqual("0.00", xmlStruct.riepilogos[0].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].ImportoParziale + ", expected " + "0,00" + " received " + xmlStruct.riepilogos[0].ImportoParziale);




                    //*****************Test sui Totalizzatori**********************
                    //TODO: ancora non pronti i totalizzatori nuovi

                    try
                    {
                        Assert.AreEqual(totGiorn + 3000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestSegueFattura, expected " + totGiorn + "received " + totGiornUpdated);

                        Assert.AreEqual(dailyTax01 + dailyNet01 + 12295, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestSegueFattura , expected " + (dailyTax01 + dailyNet01 + 12295).ToString() + " received " + dailyTax01Updated + dailyNet01Updated);

                        Assert.AreEqual(dailyTax03 + 4808, dailyTax03Updated + dailyNet03Updated, "Test GetDailyData indice 40 Failed sul metodo TestSegueFattura , expected " + (dailyTax03 + dailyNet03 + 4808).ToString() + " received " + dailyTax03Updated + dailyNet03Updated);

                        Assert.AreEqual(nonRiscossoFattura + 30000, nonRiscossoFatturaUpdated, "Test nonRiscossoFattura Failed sul metodo TestSegueFattura , expected " + (nonRiscossoFattura + 30000).ToString() + " received " + nonRiscossoFattura + nonRiscossoFatturaUpdated);

                        
                    }
                    catch (AssertionException e)
                    {
                        //NUnit Test exception
                        log.Error("NUnit Test Exception", e);
                    }
                });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }

        //Metodo che testa il metodo Omaggio Esempio 6
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestOmaggio()
        {
            log.Info("Performing TestOmaggio Method");

            try
            {
                Assert.Multiple( () =>
                { 
                    //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                    FiscalReceipt.Library.RetrieveData rData;
                    rData = new FiscalReceipt.Library.RetrieveData();
                    xml2 = new Xml2();


                    FiscalReceipt.Library.GeneralCounter gc, gc2;
                    gc = new FiscalReceipt.Library.GeneralCounter();
                    gc2 = new FiscalReceipt.Library.GeneralCounter();

                    //Aggiorno i totalizatori generali (serialization)
                    GeneralCounter.SetGeneralCounter();

                    //Load general counter (deserialization)
                    gc = GeneralCounter.GetGeneralCounter();


                    int totGiorn = Convert.ToInt32(gc.DailyTotal);

                    int nonRiscossoOmaggio = Convert.ToInt32(gc.DailyNonRiscossoOmaggio); // Totale Non Riscosso Omaggio

                    //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                    int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                    //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                    int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                    //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                    int dailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 3 (4%) AMMONTARE
                    int dailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));



                    string zRep = String.Empty;
                    //chiamo il metodo PrintRecRefound
                    int output = xml2.Omaggio(ref zRep);



                    //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                    //Aggiorno i totalizatori generali
                    GeneralCounter.SetGeneralCounter();

                    //Load general counter
                    gc2 = GeneralCounter.GetGeneralCounter();

                    //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto
                    int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                    int nonRiscossoOmaggioUpdated = Convert.ToInt32(gc2.DailyNonRiscossoOmaggio); // Totale Non Riscosso Omaggio

                    //Iva pagata per aliquota IVA 1 (22%)
                    int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 1 (22%)
                    int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                    //Iva pagata per aliquota IVA 3 (4%)
                    int dailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 3 (4%)
                    int dailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

               
                    //Faccio chiusura
                    mc.ZReport();


                    //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                    string data = DateTime.Now.ToString("yyyyMMdd");
                    //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                    Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                    xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                    //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                    /*
                    <Totali>
                    <NumeroDocCommerciali>1</NumeroDocCommerciali>
                    <PagatoContanti>200,00</PagatoContanti>
                    </Totali>
                    */
                    Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                    Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                    Assert.AreEqual("200.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "200.00" + " received " + xmlStruct.totali.PagatoContanti);

                    /* <IVA>
                   <IVA>
                    <AliquotaIVA>22.00</AliquotaIVA>
                    <Imposta>45,08</Imposta>
                    </IVA>
                    <Ammontare>204,92</Ammontare>
                    <ImportoParziale>204,92</ImportoParziale>
                    <NonRiscossoOmaggio>81,97</NonRiscossoOmaggio>
                    */


                    Assert.AreEqual("45.08", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "45,08" + " received " + xmlStruct.riepilogos[1].Imposta);
                    Assert.AreEqual("204.92", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204,92" + " received " + xmlStruct.riepilogos[1].Ammontare);
                    Assert.AreEqual("81.97", xmlStruct.riepilogos[1].NonRiscossoOmaggio, "Errore campo NonRiscossoFatture relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "81,97" + " received " + xmlStruct.riepilogos[1].NonRiscossoOmaggio);

                    /*
                     <IVA>
                    <IVA>
                    <IVA>
                    <AliquotaIVA>4.00</AliquotaIVA>
                    <Imposta>1,92</Imposta>
                    </IVA>
                    <Ammontare>48,08</Ammontare>
                    <ImportoParziale>48,08</ImportoParziale>

                    */
                    Assert.AreEqual("1.92", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "1,92" + " received " + xmlStruct.riepilogos[4].Imposta);
                    Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48,08" + " received " + xmlStruct.riepilogos[4].Ammontare);
                    Assert.AreEqual("48.08", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48,08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);


                    //*****************Test sui Totalizzatori**********************
                

                    try
                    {
                        Assert.AreEqual(totGiorn + 2000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestOmaggio, expected " + (totGiorn + 2000000) + "received " + totGiornUpdated);

                        Assert.AreEqual(dailyTax01 + dailyNet01 + 25000, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestOmaggio , expected " + (dailyTax01 + dailyNet01 + 25000) + " received " + dailyTax01Updated + dailyNet01Updated);

                        Assert.AreEqual(dailyTax01 + 450800, dailyTax01Updated, "Test GetDailyData indice 40 Failed sul metodo TestOmaggio , expected " + (dailyTax01 + 450800) + " received " + dailyTax01Updated);

                        Assert.AreEqual(dailyTax03 + dailyNet03 + 5000, dailyTax03Updated + dailyNet03Updated, "Test GetDailyData Failed sul metodo TestOmaggio , expected " + (dailyTax03 + dailyNet03 + 5000) + " received " + dailyTax03Updated + dailyNet03Updated);

                        Assert.AreEqual(dailyTax03 + 19200, dailyTax01Updated, "Test GetDailyData indice 40 Failed sul metodo TestOmaggio , expected " + (dailyTax03 + 19200) + " received " + dailyTax03Updated);

                        Assert.AreEqual(nonRiscossoOmaggio + 10000, nonRiscossoOmaggioUpdated, "Test nonRiscossoOmaggio Failed sul metodo TestOmaggio , expected " + (nonRiscossoOmaggio + 20000) + " received " + nonRiscossoOmaggioUpdated);

                    }
                    catch (AssertionException e)
                    {
                        //NUnit Test exception
                        log.Error("NUnit Test Exception", e);
                    }
            });

        }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }

        //Metodo che testa il metodo ScontoAPagare Esempio 7
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestScontoAPagare()
        {
            log.Info("Performing TestScontoAPagare Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int scontoAPagare = Convert.ToInt32(gc.ScontoAPagare); // Totale Sconto A Pagare

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                int dailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Netto ammontare per aliquota Iva 3 (4%) AMMONTARE
                int dailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));



                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.ScontoAPagare(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int scontoAPagareUpdated = Convert.ToInt32(gc2.ScontoAPagare); // Totale Sconto A Pagare

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%)
                int dailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Netto ammontare per aliquota Iva 3 (4%)
                int dailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));


                //Faccio chiusura
                mc.ZReport();


                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                Assert.Multiple(() =>
                {
                    //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                    /*
                    <Totali>
                    <NumeroDocCommerciali>1</NumeroDocCommerciali>
                    <PagatoContanti>300,00</PagatoContanti>
                    <ScontoApagare>0,04</ScontoApagare>

                    */
                   Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                   Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                   Assert.AreEqual("300.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "300.00" + " received " + xmlStruct.totali.PagatoContanti);
                   Assert.AreEqual("0.04", xmlStruct.totali.ScontoApagare, "Errore campo ScontoApagare dentro l'xml , expected " + "0.04" + " received " + xmlStruct.totali.ScontoApagare);

                    /* 
                    <IVA>
                    <IVA>
                    <AliquotaIVA>22.00</AliquotaIVA>
                    <Imposta>45,09</Imposta>
                    </IVA>
                    <Ammontare>204,95</Ammontare>
                    <ImportoParziale>204,95</ImportoParziale>
                    */


                   Assert.AreEqual("45.09", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "45,09" + " received " + xmlStruct.riepilogos[1].Imposta);
                   Assert.AreEqual("204.95", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204,95" + " received " + xmlStruct.riepilogos[1].Ammontare);
                   Assert.AreEqual("204.95", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204,95" + " received " + xmlStruct.riepilogos[1].ImportoParziale);

                    /*
                     <IVA>
                    <AliquotaIVA>4.00</AliquotaIVA>
                    <Imposta>1,92</Imposta>
                    </IVA>
                    <Ammontare>48,08</Ammontare>
                    <ImportoParziale>48,08</ImportoParziale>

                    */
                   Assert.AreEqual("1.92", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "1,92" + " received " + xmlStruct.riepilogos[4].Imposta);
                   Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48,08" + " received " + xmlStruct.riepilogos[4].Ammontare);
                   Assert.AreEqual("48.08", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48,08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);


                    //*****************Test sui Totalizzatori**********************
                    //TODO: ancora non pronti i totalizzatori nuovi

                    try
                    {
                       Assert.AreEqual(totGiorn + 3000400, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestScontoAPagare, expected " + (totGiorn + 7000) + "received " + totGiornUpdated);

                       Assert.AreEqual(scontoAPagare + 4, scontoAPagareUpdated, "Test Totale Giornaliero Failed sul metodo TestScontoAPagare, expected " + (scontoAPagare + 4) + "received " + scontoAPagareUpdated);

                       Assert.AreEqual(dailyTax01 + dailyNet01 + 25004, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestScontoAPagare , expected " + (dailyTax01 + dailyNet01 + 25004) + " received " + dailyTax01Updated + dailyNet01Updated);

                       Assert.AreEqual(dailyTax01 + 4509, dailyTax01Updated, "Test GetDailyData indice 40 Failed sul metodo TestScontoAPagare , expected " + (dailyTax01 + 4509) + " received " + dailyTax01Updated);

                       Assert.AreEqual(dailyTax03 + dailyNet03 + 5000, dailyTax03Updated + dailyNet03Updated, "Test GetDailyData Failed sul metodo TestScontoAPagare , expected " + (dailyTax03 + dailyNet03 + 5004) + " received " + dailyTax03Updated + dailyNet03Updated);

                       Assert.AreEqual(dailyTax03 + 192, dailyTax03Updated, "Test GetDailyData indice 40 Failed sul metodo TestScontoAPagare , expected " + (dailyTax03 + 19200) + " received " + dailyTax03Updated);

                    }
                    catch (AssertionException e)
                    {
                         //NUnit Test exception
                         log.Error("NUnit Test Exception", e);
                    }
                });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }

        //Metodo che testa il metodo ArrotondamentoDifetto Esempio 8
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestArrotondamentoDifetto()
        {
            log.Info("Performing TestArrotondamentoDifetto Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int negativeRounding = Convert.ToInt32(gc.DailyRoundingNegativo); // Totale rounding negativo

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                int dailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Netto ammontare per aliquota Iva 3 (4%) AMMONTARE
                int dailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));



                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.ArrotondamentoDifetto(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int negativeRoundingUpdated = Convert.ToInt32(gc2.DailyRoundingNegativo); // Totale rounding negativo

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%)
                int dailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Netto ammontare per aliquota Iva 3 (4%)
                int dailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));


                //Faccio chiusura
                mc.ZReport();


                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                Assert.Multiple(() =>
                {
                    /*
                    <Totali>
                    <NumeroDocCommerciali>1</NumeroDocCommerciali>
                    <PagatoContanti>300,00</PagatoContanti>
                    <ScontoApagare>0,02</ScontoApagare>

                    */
                    Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                    Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                    Assert.AreEqual("300.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "300.00" + " received " + xmlStruct.totali.PagatoContanti);
                    Assert.AreEqual("0.02", xmlStruct.totali.ScontoApagare, "Errore campo ScontoApagare dentro l'xml , expected " + "0,02" + " received " + xmlStruct.totali.ScontoApagare);

                    /* 
                    <IVA>
                    <IVA>
                    <AliquotaIVA>22.00</AliquotaIVA>
                    <Imposta>45,09</Imposta>
                    </IVA>
                    <Ammontare>204,93</Ammontare>
                    <ImportoParziale>204,93</ImportoParziale>
                    */

                    Assert.AreEqual("45.09", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "45.09" + " received " + xmlStruct.riepilogos[1].Imposta);
                    Assert.AreEqual("204.93", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204.93" + " received " + xmlStruct.riepilogos[1].Ammontare);
                    Assert.AreEqual("204.93", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204.93" + " received " + xmlStruct.riepilogos[1].ImportoParziale);

                    /*
                     <IVA>
                    <AliquotaIVA>4.00</AliquotaIVA>
                    <Imposta>1,92</Imposta>
                    </IVA>
                    <Ammontare>48,08</Ammontare>
                    <ImportoParziale>48,08</ImportoParziale>

                    */
                    Assert.AreEqual("1.92", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "1.92" + " received " + xmlStruct.riepilogos[4].Imposta);
                    Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].Ammontare);
                    Assert.AreEqual("48.08", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);


                    //*****************Test sui Totalizzatori**********************
                    //TODO: ancora non pronti i totalizzatori nuovi

                    try
                    {
                        Assert.AreEqual(totGiorn + 3000200, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestArrotondamentoDifetto, expected " + (totGiorn + 3000200) + "received " + totGiornUpdated);

                        Assert.AreEqual(negativeRounding + 2, negativeRoundingUpdated, "Test negativeroundingUpdated Failed sul metodo TestArrotondamentoDifetto, expected " + (negativeRounding + 4) + "received " + negativeRoundingUpdated);

                        Assert.AreEqual(dailyTax01 + dailyNet01 + 25002, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestArrotondamentoDifetto , expected " + (dailyTax01 + dailyNet01 + 25002) + " received " + dailyTax01Updated + dailyNet01Updated);

                        Assert.AreEqual(dailyTax01  + 4509, dailyTax01Updated , "Test GetDailyData Failed sul metodo TestArrotondamentoDifetto , expected " + (dailyTax01  + 4509) + " received " + dailyTax01Updated);

                        Assert.AreEqual(dailyTax03 + 192, dailyTax03Updated, "Test GetDailyData indice 40 Failed sul metodo TestArrotondamentoDifetto , expected " + (dailyTax03 + 192) + " received " + dailyTax03Updated);

                        Assert.AreEqual(dailyTax03 + dailyNet03 + 5000, dailyTax03Updated + dailyNet03Updated, "Test GetDailyData Failed sul metodo TestArrotondamentoDifetto , expected " + (dailyTax03 + dailyNet03 + 5000) + " received " + dailyTax03Updated + dailyNet03Updated);

                    }
                    catch (AssertionException e)
                    {
                        //NUnit Test exception
                        log.Error("NUnit Test Exception", e);
                    }

                });
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }

        //Metodo che testa il metodo ArrotondamentoEccesso Esempio 9
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestArrotondamentoEccesso()
        {
            log.Info("Performing TestArrotondamentoEccesso Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int positiveRounding = Convert.ToInt32(gc.DailyRoundingPositivo); // Totale rounding positivo
                
                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                int dailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Netto ammontare per aliquota Iva 3 (4%) AMMONTARE
                int dailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));



                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.ArrotondamentoEccesso(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int positiveRoundingUpdated = Convert.ToInt32(gc2.DailyRoundingPositivo); // Totale rounding positivo

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%)
                int dailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Netto ammontare per aliquota Iva 3 (4%)
                int dailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));


                //Faccio chiusura
                mc.ZReport();


                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                Assert.Multiple(() =>
               {
                    /*
                    <Totali>
                    <NumeroDocCommerciali>1</NumeroDocCommerciali>
                    <PagatoContanti>300,10</PagatoContanti>

                    */
                   Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                   Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                   Assert.AreEqual("300.10", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "300.10" + " received " + xmlStruct.totali.PagatoContanti);

                    /* 
                    <IVA>
                    <IVA>
                    <AliquotaIVA>22.00</AliquotaIVA>
                    <Imposta>45,1</Imposta>
                    </IVA>
                    <Ammontare>204,99</Ammontare>
                    <ImportoParziale>204,99</ImportoParziale>
                    */

                   Assert.AreEqual("45.10", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "45.10" + " received " + xmlStruct.riepilogos[1].Imposta);
                   Assert.AreEqual("204.99", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204.99" + " received " + xmlStruct.riepilogos[1].Ammontare);
                   Assert.AreEqual("204.99", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204.99" + " received " + xmlStruct.riepilogos[1].ImportoParziale);

                    /*
                    <IVA>
                    <AliquotaIVA>4.00</AliquotaIVA>
                    <Imposta>1,92</Imposta>
                    </IVA>
                    <Ammontare>48,08</Ammontare>
                    <ImportoParziale>48,08</ImportoParziale>

                    */
                   Assert.AreEqual("1.92", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "1.92" + " received " + xmlStruct.riepilogos[4].Imposta);
                   Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].Ammontare);
                   Assert.AreEqual("48.08", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);


                    //*****************Test sui Totalizzatori**********************


                    try
                    {
                       Assert.AreEqual(totGiorn + 3000900, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestArrotondamentoEccesso, expected " + (totGiorn + 3001000) + "received " + totGiornUpdated);

                       Assert.AreEqual(positiveRounding + 1, positiveRoundingUpdated, "Test positiveRoundingUpdated Failed sul metodo TestArrotondamentoDifetto, expected " + (positiveRounding + 1) + "received " + positiveRoundingUpdated);

                       Assert.AreEqual(dailyTax01 + dailyNet01 + 25009, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestArrotondamentoDifetto , expected " + (dailyTax01 + dailyNet01 + 25009) + " received " + dailyTax01Updated + dailyNet01Updated);

                       Assert.AreEqual(dailyTax01 + 4510, dailyTax01Updated, "Test GetDailyData Failed sul metodo TestArrotondamentoDifetto , expected " + (dailyTax01 + 4510) + " received " + dailyTax01Updated);

                       Assert.AreEqual(dailyTax03 + 192, dailyTax03Updated, "Test GetDailyData indice 40 Failed sul metodo TestArrotondamentoDifetto , expected " + (dailyTax03 + 192) + " received " + dailyTax03Updated);

                       Assert.AreEqual(dailyTax03 + dailyNet03 + 5000, dailyTax03Updated + dailyNet03Updated, "Test GetDailyData Failed sul metodo TestArrotondamentoDifetto , expected " + (dailyTax03 + dailyNet03 + 5000) + " received " + dailyTax03Updated + dailyNet03Updated);

                   }
                   catch (AssertionException e)
                   {
                        //NUnit Test exception
                        log.Error("NUnit Test Exception", e);
                   }
               });
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa il metodo Ventilazione Esempio 10
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestVentilazione()
        {
            log.Info("Performing TestVentilazione Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal); 

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                int dailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Netto ammontare per aliquota Iva 3 (4%) AMMONTARE
                int dailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));



                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.Ventilazione(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);


                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%)
                int dailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Netto ammontare per aliquota Iva 3 (4%)
                int dailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));


                //NOTA: Se devo ventilare su dei reparti li associo all index iva 14 in modo che non faccio confusione se lavoro con 2 ateco, uno ventilato e uno no
                //Iva "pagata" per la ventilazione 
                int dailyTax14Updated = Int32.Parse(rData.getDailyData("40", "14").Substring(9, 9));

                //Netto ammontare per la ventilazione 
                int dailyNet14Updated = Int32.Parse(rData.getDailyData("40", "14").Substring(0, 9));

                //Faccio chiusura
                mc.ZReport();


                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                //Assert.Multiple(() =>
                //{
                /*
                <Totali>
                <NumeroDocCommerciali>2</NumeroDocCommerciali>
                <PagatoContanti>600,00</PagatoContanti>
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                    Assert.AreEqual("2", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                    Assert.AreEqual("600.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "300.10" + " received " + xmlStruct.totali.PagatoContanti);

                /* 
                <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>45,08</Imposta>
                </IVA>
                <Ammontare>204,92</Ammontare>
                <ImportoParziale>204,92</ImportoParziale>
                */

                Assert.AreEqual("45.08", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "45.10" + " received " + xmlStruct.riepilogos[1].Imposta);
                    Assert.AreEqual("204.92", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204.99" + " received " + xmlStruct.riepilogos[1].Ammontare);
                    Assert.AreEqual("204.92", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204.99" + " received " + xmlStruct.riepilogos[1].ImportoParziale);

                /*
                <IVA>
                <AliquotaIVA>4.00</AliquotaIVA>
                <Imposta>1,92</Imposta>
                </IVA>
                <Ammontare>48,08</Ammontare>
                <ImportoParziale>48,08</ImportoParziale>

                */
                Assert.AreEqual("1.92", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "1.92" + " received " + xmlStruct.riepilogos[4].Imposta);
                    Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].Ammontare);
                    Assert.AreEqual("48.08", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);

                /*
                <Ventilazione>SI</ Ventilazione>
                <Ammontare>300,00</Ammontare>
                <ImportoParziale>300,00</ImportoParziale> 

                */

                Assert.AreEqual("SI", xmlStruct.riepilogos[5].VentilazioneIVA, "Errore campo VentilazioneIVA " + ", expected " + "SI" + " received " + xmlStruct.riepilogos[5].VentilazioneIVA);
                Assert.AreEqual("300.00", xmlStruct.riepilogos[5].Ammontare, "Errore campo Ammontare relativo alla VentilazioneIVA"  + ", expected " + "300.00" + " received " + xmlStruct.riepilogos[5].Ammontare);
                Assert.AreEqual("300.00", xmlStruct.riepilogos[5].ImportoParziale, "Errore campo ImportoParziale relativo alla VentilazioneIVA"  + ", expected " + "300.00" + " received " + xmlStruct.riepilogos[5].ImportoParziale);


                //*****************Test sui Totalizzatori**********************


                try
                {
                        Assert.AreEqual(totGiorn + 6000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestVentilazione, expected " + (totGiorn + 6000000) + "received " + totGiornUpdated);

                        Assert.AreEqual(dailyTax01 + dailyNet01 + 25000, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestVentilazione , expected " + (dailyTax01 + dailyNet01 + 25000) + " received " + dailyTax01Updated + dailyNet01Updated);

                        Assert.AreEqual(dailyTax01 + 4508, dailyTax01Updated, "Test GetDailyData Failed sul metodo TestVentilazione , expected " + (dailyTax01 + 4508) + " received " + dailyTax01Updated);

                        Assert.AreEqual(dailyTax03 + 192, dailyTax03Updated, "Test GetDailyData indice 40 Failed sul metodo TestVentilazione , expected " + (dailyTax03 + 192) + " received " + dailyTax03Updated);

                        Assert.AreEqual(dailyTax03 + dailyNet03 + 5000, dailyTax03Updated + dailyNet03Updated, "Test GetDailyData Failed sul metodo TestVentilazione , expected " + (dailyTax03 + dailyNet03 + 5000) + " received " + dailyTax03Updated + dailyNet03Updated);

                    }
                    catch (AssertionException e)
                    {
                        //NUnit Test exception
                        log.Error("NUnit Test Exception", e);
                    }
                //});
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }
        /*
        public void scontrinoVentilato()
        {
            try
            {
                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.BeginFiscalReceipt(true);
                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.PrintRecItem("Bene A", (decimal)1606500, (int)1000, (int)4, (decimal)1606500, "");

                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.PrintRecItem("Bene B", (decimal)500000, (int)5000, (int)5, (decimal)100000, "");
                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.PrintRecItem("Bene C", (decimal)1000000, (int)1000, (int)4, (decimal)1000000, "");
                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.PrintRecSubtotal((decimal)3000000);

                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)3000000, "0CONTANTI");
                FiscalReceipt.Library.FiscalReceipt.fiscalprinter.EndFiscalReceipt(false);
            }
            catch (Exception err)
            {
                log.Error("", err);
            }
        }
        */


        //Metodo che testa il metodo Ventilazione Esempio 10 bis
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestVentilazioneMista()
        {
            log.Info("Performing TestVentilazione Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva "pagata" all' aliquota 14 (AL= Altro) /dove faro' puntare i reparti con ateco ventilato
                int dailyTax14 = Int32.Parse(rData.getDailyData("40", "14").Substring(9, 9));

                //Iva "pagata" all' aliquota 14 (AL= Altro) /dove faro' puntare i reparti con ateco ventilato
                int dailyNet14 = Int32.Parse(rData.getDailyData("40", "14").Substring(0, 9));

                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.VentilazioneMista(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);


                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //NOTA: Se devo ventilare su dei reparti li associo all index iva 14 in modo che non faccio confusione se lavoro con 2 ateco, uno ventilato e uno no
                //Iva "pagata" per la ventilazione 
                int dailyTax14Updated = Int32.Parse(rData.getDailyData("40", "14").Substring(9, 9));

                //Netto ammontare per la ventilazione 
                int dailyNet14Updated = Int32.Parse(rData.getDailyData("40", "14").Substring(0, 9));

                //Faccio chiusura
                mc.ZReport();


                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                //Assert.Multiple(() =>
                //{
                /*
                <Totali>
                    <NumeroDocCommerciali>1</NumeroDocCommerciali>
                    <PagatoContanti>300,00</PagatoContanti>
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                Assert.AreEqual("300.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "300.00" + " received " + xmlStruct.totali.PagatoContanti);

                /* 
                <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>18,03</Imposta>
                </IVA>
                <Ammontare>81,97</Ammontare>
                <ImportoParziale>81,97</ImportoParziale>
                <CodiceAttivita>AABB</CodiceAttivita>
                */

                Assert.AreEqual("18.03", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "18.03" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("81.97", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "81.97" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("81.97", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "81.97" + " received " + xmlStruct.riepilogos[1].ImportoParziale);


                /*
                <Ventilazione>SI</ Ventilazione>
                <Ammontare>200,00</Ammontare>
                <ImportoParziale>200,00</ImportoParziale>
                <CodiceAttivita>AABB</CodiceAttivita>

                */

                Assert.AreEqual("SI", xmlStruct.riepilogos[5].VentilazioneIVA, "Errore campo VentilazioneIVA " + ", expected " + "SI" + " received " + xmlStruct.riepilogos[5].VentilazioneIVA);
                Assert.AreEqual("200.00", xmlStruct.riepilogos[5].Ammontare, "Errore campo Ammontare relativo alla VentilazioneIVA" + ", expected " + "200.00" + " received " + xmlStruct.riepilogos[5].Ammontare);
                Assert.AreEqual("200.00", xmlStruct.riepilogos[5].ImportoParziale, "Errore campo ImportoParziale relativo alla VentilazioneIVA" + ", expected " + "200.00" + " received " + xmlStruct.riepilogos[5].ImportoParziale);


                //*****************Test sui Totalizzatori**********************


                try
                {
                    Assert.AreEqual(totGiorn + 3000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestVentilazioneMista, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTax01 + dailyNet01 + 10000, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestVentilazioneMista , expected " + (dailyTax01 + dailyNet01 + 10000) + " received " + dailyTax01Updated + dailyNet01Updated);

                    Assert.AreEqual(dailyTax01 + 1803, dailyTax01Updated, "Test GetDailyData Failed sul metodo TestVentilazioneMista , expected " + (dailyTax01 + 1803) + " received " + dailyTax01Updated);

                    Assert.AreEqual(dailyTax14 + 0 , dailyTax14Updated, "Test GetDailyData indice 40 Failed sul metodo TestVentilazioneMista , expected " + (dailyTax14 + 0) + " received " + dailyTax14Updated);

                    Assert.AreEqual(dailyTax14 + dailyNet14 + 20000, dailyTax14Updated + dailyNet14Updated, "Test GetDailyData Failed sul metodo TestVentilazioneMista , expected " + (dailyTax14 + dailyNet14 + 20000) + " received " + dailyTax14Updated + dailyNet14Updated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //});
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa il metodo AnnulloPrimoScenario, esempio 11
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestAnnulloPrimoScenario()
        {
            log.Info("Performing TestAnnulloPrimoScenario Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int totAmmontareAnnulli = Convert.ToInt32(gc.FiscalRecVoid);

                //Iva ANNULLATA per aliquota IVA 1 (22%) IMPOSTA
                int dailyTaxVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (22%)
                int dailyNetVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(5, 9));

                //Iva ANNULLATA all' aliquota 3 (4%) 
                int dailyTaxVoid03 = Int32.Parse(rData.getDailyData("42", "3").Substring(15, 9));

                //Netto annullato all' aliquota 3 (4%) ù
                int dailyNetVoid03 = Int32.Parse(rData.getDailyData("42", "3").Substring(5, 9));

                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.AnnulloPrimoScenario(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int totAmmontareAnnulliUpdated = Convert.ToInt32(gc2.FiscalRecVoid);

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTaxVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(15, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNetVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(5, 9));

                //Iva pagata per aliquota IVA 3 (4%)
                int dailyTaxVoid03Updated = Int32.Parse(rData.getDailyData("42", "03").Substring(15, 9));

                //Netto ammontare per aliquota IVA 3 (4%)
                int dailyNetVoid03Updated = Int32.Parse(rData.getDailyData("42", "03").Substring(5, 9));


               


                //Faccio chiusura
                mc.ZReport();


                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                //Assert.Multiple(() =>
                //{
                /*
                <Totali>
                    <NumeroDocCommerciali>2</NumeroDocCommerciali>
                    <PagatoContanti>300,00</PagatoContanti>
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("2", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                Assert.AreEqual("300.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "300.00" + " received " + xmlStruct.totali.PagatoContanti);

                /* 
                <IVA>            
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>0,00</Imposta>
                </IVA>
                <Ammontare>204,92</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <TotaleAmmontareAnnulli>204,92</TotaleAmmontareAnnulli>
                */

                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("204.92", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204.92" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("204.92", xmlStruct.riepilogos[1].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204.92" + " received " + xmlStruct.riepilogos[1].ImportoParziale);


                /*
               <IVA>
               <AliquotaIVA>4.00</AliquotaIVA>
               <Imposta>0,00</Imposta>
               </IVA>
               <Ammontare>48,08</Ammontare>
               <ImportoParziale>0,00</ImportoParziale>
               <TotaleAmmontareAnnulli>48,08</TotaleAmmontareAnnulli>
                */

                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].Imposta);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);


                //*****************Test sui Totalizzatori**********************


                try
                {
                    Assert.AreEqual(totGiorn + 3000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestAnnulloPrimoScenario, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTaxVoid01 + dailyNetVoid01 + 25000, dailyTaxVoid01Updated + dailyNetVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloPrimoScenario , expected " + (dailyTaxVoid01 + dailyNetVoid01 + 25000) + " received " + dailyTaxVoid01Updated + dailyNetVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid01 + 4508, dailyTaxVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloPrimoScenario , expected " + (dailyTaxVoid01 + 4508) + " received " + dailyTaxVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid03 + 192, dailyTaxVoid03Updated, "Test GetDailyData indice 40 Failed sul metodo TestAnnulloPrimoScenario , expected " + (dailyTaxVoid03 + 192) + " received " + dailyTaxVoid03Updated);

                    Assert.AreEqual(dailyTaxVoid03 + dailyNetVoid03 + 5000, dailyTaxVoid03Updated + dailyNetVoid03Updated, "Test GetDailyData Failed sul metodo TestAnnulloPrimoScenario , expected " + (dailyTaxVoid03 + dailyNetVoid03 + 5000) + " received " + dailyTaxVoid03Updated + dailyNetVoid03Updated);

                    Assert.AreEqual(totAmmontareAnnulli + 30000 , totAmmontareAnnulliUpdated , "Test GetDailyData Failed sul metodo TestAnnulloPrimoScenario , expected " + (totAmmontareAnnulli + 30000) + " received " + totAmmontareAnnulliUpdated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //});
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa il metodo AnnulloSecondoScenario, esempio 12
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestAnnulloSecondoScenario()
        {
            log.Info("Performing TestAnnulloSecondoScenario Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int totAmmontareAnnulli = Convert.ToInt32(gc.FiscalRecVoid);

                //Iva ANNULLATA per aliquota IVA 1 (22%) IMPOSTA
                int dailyTaxVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (22%)
                int dailyNetVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(5, 9));

                //Iva ANNULLATA all' aliquota 3 (4%) 
                int dailyTaxVoid03 = Int32.Parse(rData.getDailyData("42", "3").Substring(15, 9));

                //Netto annullato all' aliquota 3 (4%) ù
                int dailyNetVoid03 = Int32.Parse(rData.getDailyData("42", "3").Substring(5, 9));

                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.AnnulloSecondoScenario(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int totAmmontareAnnulliUpdated = Convert.ToInt32(gc2.FiscalRecVoid);

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTaxVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(15, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNetVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(5, 9));

                //Iva pagata per aliquota IVA 3 (4%)
                int dailyTaxVoid03Updated = Int32.Parse(rData.getDailyData("42", "03").Substring(15, 9));

                //Netto ammontare per aliquota IVA 3 (4%)
                int dailyNetVoid03Updated = Int32.Parse(rData.getDailyData("42", "03").Substring(5, 9));





                //Faccio chiusura
                mc.ZReport();


                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                //Assert.Multiple(() =>
                //{
                /*
                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);

                /* 
               <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>-45,08</Imposta>
                </IVA>
                <Ammontare>0,00</Ammontare>
                <ImportoParziale>-204,92</ImportoParziale>
                <TotaleAmmontareAnnulli>204,92</TotaleAmmontareAnnulli>
                */

                Assert.AreEqual("-45.08", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "-45.08" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("-204.92", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "-204.92" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("204.92", xmlStruct.riepilogos[1].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204.92" + " received " + xmlStruct.riepilogos[1].ImportoParziale);


                /*
               <IVA>
                <AliquotaIVA>4.00</AliquotaIVA>
                <Imposta>-1,92</Imposta>
                </IVA>
                <Ammontare>0,00</Ammontare>
                <ImportoParziale>-48,08</ImportoParziale>
                <TotaleAmmontareAnnulli>48,08</TotaleAmmontareAnnulli>
                */

                Assert.AreEqual("-1.92", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "-1.92" + " received " + xmlStruct.riepilogos[4].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].Ammontare);
                Assert.AreEqual("-48.08", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "-48.08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);


                //*****************Test sui Totalizzatori**********************


                try
                {
                    Assert.AreEqual(totGiorn + 000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestAnnulloPrimoScenario, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTaxVoid01 + dailyNetVoid01 + 25000, dailyTaxVoid01Updated + dailyNetVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloPrimoScenario , expected " + (dailyTaxVoid01 + dailyNetVoid01 + 25000) + " received " + dailyTaxVoid01Updated + dailyNetVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid01 + 4508, dailyTaxVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloPrimoScenario , expected " + (dailyTaxVoid01 + 4508) + " received " + dailyTaxVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid03 + 192, dailyTaxVoid03Updated, "Test GetDailyData indice 40 Failed sul metodo TestAnnulloPrimoScenario , expected " + (dailyTaxVoid03 + 192) + " received " + dailyTaxVoid03Updated);

                    Assert.AreEqual(dailyTaxVoid03 + dailyNetVoid03 + 5000, dailyTaxVoid03Updated + dailyNetVoid03Updated, "Test GetDailyData Failed sul metodo TestAnnulloPrimoScenario , expected " + (dailyTaxVoid03 + dailyNetVoid03 + 5000) + " received " + dailyTaxVoid03Updated + dailyNetVoid03Updated);

                    Assert.AreEqual(totAmmontareAnnulli + 30000, totAmmontareAnnulliUpdated, "Test GetDailyData Failed sul metodo TestAnnulloPrimoScenario , expected " + (totAmmontareAnnulli + 30000) + " received " + totAmmontareAnnulliUpdated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //});
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }




        //Metodo che testa il metodo AnnulloTerzoScenario, esempio 13
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestAnnulloTerzoScenario()
        {
            log.Info("Performing TestAnnulloTerzoScenario Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int totAmmontareAnnulli = Convert.ToInt32(gc.FiscalRecVoid);

                //Iva ANNULLATA per aliquota IVA 1 (22%) IMPOSTA
                int dailyTaxVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (22%)
                int dailyNetVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(5, 9));

                //Iva ANNULLATA all' aliquota 3 (4%) 
                int dailyTaxVoid03 = Int32.Parse(rData.getDailyData("42", "3").Substring(15, 9));

                //Netto annullato all' aliquota 3 (4%) 
                int dailyNetVoid03 = Int32.Parse(rData.getDailyData("42", "3").Substring(5, 9));

                //Iva ANNULLATA all' aliquota N4 (ES) 
                int dailyTaxVoid13 = Int32.Parse(rData.getDailyData("42", "13").Substring(15, 9));

                //Netto annullato all' aliquota N4 (ES) 
                int dailyNetVoid13 = Int32.Parse(rData.getDailyData("42", "13").Substring(5, 9));

                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.AnnulloTerzoScenario(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int totAmmontareAnnulliUpdated = Convert.ToInt32(gc2.FiscalRecVoid);

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTaxVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(15, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNetVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(5, 9));

                //Iva pagata per aliquota IVA 3 (4%)
                int dailyTaxVoid03Updated = Int32.Parse(rData.getDailyData("42", "03").Substring(15, 9));

                //Netto ammontare per aliquota IVA 3 (4%)
                int dailyNetVoid03Updated = Int32.Parse(rData.getDailyData("42", "03").Substring(5, 9));

                //Iva ANNULLATA all' aliquota N4 (ES) 
                int dailyTaxVoid13Updated = Int32.Parse(rData.getDailyData("42", "13").Substring(15, 9));

                //Netto annullato all' aliquota N4 (ES) 
                int dailyNetVoid13Updated = Int32.Parse(rData.getDailyData("42", "13").Substring(5, 9));



                //Faccio chiusura
                mc.ZReport();


                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                //Assert.Multiple(() =>
                //{
                /*
                <Totali>
                <NumeroDocCommerciali>3</NumeroDocCommerciali>
                <PagatoContanti>260,00</PagatoContanti>
                <PagatoElettronico>40,00</PagatoElettronico>
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("3", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "3" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                Assert.AreEqual("260.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "300.10" + " received " + xmlStruct.totali.PagatoContanti);
                Assert.AreEqual("40.00", xmlStruct.totali.PagatoElettronico, "Errore campo PagatoContanti dentro l'xml , expected " + "40.00" + " received " + xmlStruct.totali.PagatoElettronico);

                /* 
                <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>9,02</Imposta>
                </IVA>
                <Ammontare>163,93</Ammontare>
                <ImportoParziale>40,98</ImportoParziale>
                <TotaleAmmontareAnnulli>122,95</TotaleAmmontareAnnulli>
                <BeniInSospeso>0,00</BeniInSospeso>
                <CodiceAttivita>aaa</CodiceAttivita>
                */

                Assert.AreEqual("9.02", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "9.02" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("163.93", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "163.93" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("40.98", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "40.98" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("122.95", xmlStruct.riepilogos[1].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122.95" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareAnnulli);


                /*
               <IVA>
                <AliquotaIVA>4.00</AliquotaIVA>
                <Imposta>0,00</Imposta>
                </IVA>
                <Ammontare>48,08</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <TotaleAmmontareAnnulli>48,08</TotaleAmmontareAnnulli>
                */

                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "-1.92" + " received " + xmlStruct.riepilogos[4].Imposta);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "-48.08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].TotaleAmmontareAnnulli);


                /*
                <Riepilogo>
                <Natura>N4<Natura>
                <Ammontare>100</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <TotaleAmmontareAnnulli>100,00</TotaleAmmontareAnnulli>
                 */
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[0].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "-48.08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].TotaleAmmontareAnnulli);


                //*****************Test sui Totalizzatori**********************


                try
                {
                    Assert.AreEqual(totGiorn + 3000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestAnnulloTerzoScenario, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTaxVoid01 + dailyNetVoid01 + 10000, dailyTaxVoid01Updated + dailyNetVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloTerzoScenario , expected " + (dailyTaxVoid01 + dailyNetVoid01 + 25000) + " received " + dailyTaxVoid01Updated + dailyNetVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid01 + 1803, dailyTaxVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloTerzoScenario , expected " + (dailyTaxVoid01 + 4508) + " received " + dailyTaxVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid03 + 192, dailyTaxVoid03Updated, "Test GetDailyData indice 40 Failed sul metodo TestAnnulloTerzoScenario , expected " + (dailyTaxVoid03 + 192) + " received " + dailyTaxVoid03Updated);

                    Assert.AreEqual(dailyTaxVoid03 + dailyNetVoid03 + 5000, dailyTaxVoid03Updated + dailyNetVoid03Updated, "Test GetDailyData Failed sul metodo TestAnnulloTerzoScenario , expected " + (dailyTaxVoid03 + dailyNetVoid03 + 5000) + " received " + dailyTaxVoid03Updated + dailyNetVoid03Updated);

                    Assert.AreEqual(totAmmontareAnnulli + 25000, totAmmontareAnnulliUpdated, "Test GetDailyData Failed sul metodo TestAnnulloTerzoScenario , expected " + (totAmmontareAnnulli + 30000) + " received " + totAmmontareAnnulliUpdated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //});
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa il Buono Monouso, Esempio 14
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestBuonoMonouso()
        {
            log.Info("Performing TestBuonoMonouso Method");

            try
            {
           //     Assert.Multiple(() =>
           //     {
                    //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                    FiscalReceipt.Library.RetrieveData rData;
                    rData = new FiscalReceipt.Library.RetrieveData();
                    xml2 = new Xml2();


                    FiscalReceipt.Library.GeneralCounter gc, gc2;
                    gc = new FiscalReceipt.Library.GeneralCounter();
                    gc2 = new FiscalReceipt.Library.GeneralCounter();

                    //Aggiorno i totalizatori generali (serialization)
                    GeneralCounter.SetGeneralCounter();

                    //Load general counter (deserialization)
                    gc = GeneralCounter.GetGeneralCounter();


                    int totGiorn = Convert.ToInt32(gc.DailyTotal);

                  
                    //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                    int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                    //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                    int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));


                    string zRep = String.Empty;
                    //chiamo il metodo PrintRecRefound
                    int output = xml2.BuonoMonouso(ref zRep);



                    //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                    //Aggiorno i totalizatori generali
                    GeneralCounter.SetGeneralCounter();

                    //Load general counter
                    gc2 = GeneralCounter.GetGeneralCounter();

                    //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto
                    int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                    
                    //Iva pagata per aliquota IVA 1 (22%)
                    int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                    //Netto ammontare per aliquota Iva 1 (22%)
                    int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                   

                    //Faccio chiusura
                    mc.ZReport();


                    //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                    string data = DateTime.Now.ToString("yyyyMMdd");
                    //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                    Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                    xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                    //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                    /*
                    
                    <Totali>
                    <NumeroDocCommerciali>1</NumeroDocCommerciali>
                    <PagatoContanti>122,00</PagatoContanti>
                    */
                    Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                    Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                    Assert.AreEqual("122.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "200.00" + " received " + xmlStruct.totali.PagatoContanti);

                    /* <IVA>
                    <AliquotaIVA>22.00</AliquotaIVA>
                    <Imposta>22,00</Imposta>
                    </IVA>
                    <Ammontare>100,00</Ammontare>
                    <ImportoParziale>100,00</ImportoParziale>
                    <CodiceAttivita>aaa</CodiceAttivita>
                    </Riepilogo>
                    */


                    Assert.AreEqual("22.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "45,08" + " received " + xmlStruct.riepilogos[1].Imposta);
                    Assert.AreEqual("100.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204,92" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                    Assert.AreEqual("100.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo NonRiscossoFatture relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "81,97" + " received " + xmlStruct.riepilogos[1].Ammontare);

                   

                    //*****************Test sui Totalizzatori**********************


                    try
                    {
                        Assert.AreEqual(totGiorn + 1220000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestBuonoMonouso, expected " + (totGiorn + 2000000) + "received " + totGiornUpdated);

                        Assert.AreEqual(dailyTax01 + dailyNet01 + 12200, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestBuonoMonouso , expected " + (dailyTax01 + dailyNet01 + 25000) + " received " + dailyTax01Updated + dailyNet01Updated);

                        Assert.AreEqual(dailyTax01 + 2200, dailyTax01Updated, "Test GetDailyData indice 40 Failed sul metodo TestBuonoMonouso , expected " + (dailyTax01 + 450800) + " received " + dailyTax01Updated);
                        
                       
                    }
                    catch (AssertionException e)
                    {
                        //NUnit Test exception
                        log.Error("NUnit Test Exception", e);
                    }
           //     });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa l' utilizzo del  Buono Monouso, Esempio 15
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestUtilizzoBuonoMonouso()
        {
            log.Info("Performing TestUtilizzoBuonoMonouso Method");

            try
            {
                //     Assert.Multiple(() =>
                //     {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();


                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                
                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.UtilizzoBuonoMonouso(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto
                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);


                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));



                //Faccio chiusura
                mc.ZReport();


                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                /*

                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
               
                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                
                /*<IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>0,00</Imposta>
                </IVA>
                <Ammontare>100,00</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <BeniInSospeso>100,00</BeniInSospeso>
                */


                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" +  " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo NonRiscossoFatture relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[1].BeniInSospeso, "Errore campo NonRiscossoFatture relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[1].BeniInSospeso);



                //*****************Test sui Totalizzatori**********************


                try
                {

                    //TODO Non so se sia corretto o meno ma qui nn sale nel il totale gior ne il tasse e netto
                    Assert.AreEqual(totGiorn + 000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestUtilizzoBuonoMonouso, expected " + (totGiorn + 2000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTax01 + dailyNet01 + 00000, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestUtilizzoBuonoMonouso , expected " + (dailyTax01 + dailyNet01 + 25000) + " received " + dailyTax01Updated + dailyNet01Updated);

                    Assert.AreEqual(dailyTax01 + 0, dailyTax01Updated, "Test GetDailyData indice 40 Failed sul metodo TestUtilizzoBuonoMonouso , expected " + (dailyTax01 + 450800) + " received " + dailyTax01Updated);


                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //     });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa l' utilizzo del  Buono Multiuso, Esempio 16
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestBuonoMultiuso()
        {
            log.Info("Performing TestUBuonoMultiuso Method");

            try
            {
                //     Assert.Multiple(() =>
                //     {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();


                int totGiorn = Convert.ToInt32(gc.DailyTotal);


                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax13 = Int32.Parse(rData.getDailyData("40", "0").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet13 = Int32.Parse(rData.getDailyData("40", "0").Substring(0, 9));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.BuonoMultiuso(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto
                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);


                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax13Updated = Int32.Parse(rData.getDailyData("40", "0").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet13Updated = Int32.Parse(rData.getDailyData("40", "0").Substring(0, 9));



                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(5000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                /*

                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
                <PagatoContanti>122,00</PagatoContanti>
                </Totali>
               
                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);

                /*
                 * <Natura>N2<Natura>
                    <Ammontare>122,00</Ammontare>
                    <ImportoParziale>122,00</ImportoParziale>
                */


                Assert.AreEqual("122.00", xmlStruct.riepilogos[0].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122.00" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("122.00", xmlStruct.riepilogos[0].Ammontare, "Errore campo NonRiscossoFatture relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                


                //*****************Test sui Totalizzatori**********************


                try
                {

                    //TODO Non so se sia corretto o meno ma qui nn sale nel il totale gior ne il tasse e netto
                    Assert.AreEqual(totGiorn + 1220000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestUBuonoMultiuso, expected " + (totGiorn + 1220000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTax13 + dailyNet13 + 12200, dailyTax13Updated + dailyNet13Updated, "Test GetDailyData Failed sul metodo TestUBuonoMultiuso , expected " + (dailyTax13 + dailyNet13 + 12200) + " received " + dailyTax13Updated + dailyNet13Updated);

                    Assert.AreEqual(dailyTax13 + 0, dailyTax13Updated, "Test GetDailyData indice 40 Failed sul metodo TestUBuonoMultiuso , expected " + dailyTax13 + " received " + dailyTax13Updated);


                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //     });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa l' utilizzo del  Buono Multiuso Utilizzo, Esempio 17
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestBuonoMultiusoUtilizzo()
        {
            log.Info("Performing TestBuonoMultiusoUtilizzo Method");

            try
            {
                //     Assert.Multiple(() =>
                //     {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();


                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int totScontoAPagare = Convert.ToInt32(gc.ScontoAPagare); // Totale ScontoAPagare

                int totScontoAPagareMultiuso = Convert.ToInt32(gc.DailyBuonoMultiuso); // Totale ScontoAPagare MultiUso

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.BuonoMultiusoUtilizzo(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto
                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int totScontoAPagareUpdated = Convert.ToInt32(gc2.ScontoAPagare); // Totale ScontoAPagare

                int totScontoAPagareMultiusoUpdated = Convert.ToInt32(gc2.DailyBuonoMultiuso); // Totale ScontoAPagare MultiUso

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));



                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(5000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                /*

                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
                <ScontoApagare>122,00</ScontoApagare>
                </Totali>
               
                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                Assert.AreEqual("122.00", xmlStruct.totali.ScontoApagare, "Errore campo ScontoApagare dentro l'xml , expected " + "122.00" + " received " + xmlStruct.totali.ScontoApagare);

                /*
                 IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>22,00</Imposta>
                </IVA>
                <Ammontare>100,00</Ammontare>
                <ImportoParziale>100,00</ImportoParziale>
                */

                Assert.AreEqual("22.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "22.00" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[1].Ammontare);



                //*****************Test sui Totalizzatori**********************


                try
                {

                    //TODO Non so se sia corretto o meno ma qui nn sale nel il totale gior ne il tasse e netto
                    Assert.AreEqual(totGiorn + 1220000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestBuonoMultiusoUtilizzo, expected " + (totGiorn + 1220000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTax01 + dailyNet01 + 12200, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestBuonoMultiusoUtilizzo , expected " + (dailyTax01 + dailyNet01 + 12200) + " received " + dailyTax01Updated + dailyNet01Updated);

                    Assert.AreEqual(dailyTax01 + 2200, dailyTax01Updated, "Test GetDailyData indice 40 Failed sul metodo TestBuonoMultiusoUtilizzo , expected " + dailyTax01 + 2200 + " received " + dailyTax01Updated);

                    Assert.AreEqual(totScontoAPagareMultiuso + 12200, totScontoAPagareMultiusoUpdated, "Test GetDailyData indice 40 Failed sul metodo TestBuonoMultiusoUtilizzo , expected " + totScontoAPagareMultiuso + 12200 + " received " + totScontoAPagareMultiusoUpdated);


                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //     });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa l' utilizzo del  Buono Multiuso Celiachia, Esempio 18
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestBuonoMultiusoCeliachia()
        {
            log.Info("Performing TestBuonoMultiusoUtilizzo Method");

            try
            {
                //     Assert.Multiple(() =>
                //     {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();


                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int totTicket = Convert.ToInt32(gc.DailyTotalTicket); // Totale ScontoAPagare

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.BuonoMultiusoCeliachia(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto
                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int totTicketUpdated = Convert.ToInt32(gc2.DailyTotalTicket); // Totale ScontoAPagare

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));



                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(5000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                /*

                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
                <Ticket>
                <PagatoTicket>122,00</PagatoTicket>
                <NumeroTicket>1</NumeroTicket>
                </Ticket>
                </Totali>
               
                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                Assert.AreEqual("122.00", xmlStruct.totali.PagatoTicket, "Errore campo ScontoApagare dentro l'xml , expected " + "122.00" + " received " + xmlStruct.totali.PagatoTicket);
                Assert.AreEqual("1", xmlStruct.totali.NumeroTicket, "Errore campo ScontoApagare dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroTicket);

                /*
                <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>22,00</Imposta>
                </IVA>
                <Ammontare>100,00</Ammontare>
                <ImportoParziale>100,00</ImportoParziale>
                */

                Assert.AreEqual("22.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "22.00" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[1].Ammontare);



                //*****************Test sui Totalizzatori**********************


                try
                {

                    //TODO Non so se sia corretto o meno ma qui nn sale nel il totale gior ne il tasse e netto
                    Assert.AreEqual(totGiorn + 1220000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestBuonoMultiusoCeliachia, expected " + (totGiorn + 1220000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTax01 + dailyNet01 + 12200, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestBuonoMultiusoCeliachia , expected " + (dailyTax01 + dailyNet01 + 12200) + " received " + dailyTax01Updated + dailyNet01Updated);

                    Assert.AreEqual(dailyTax01 + 2200, dailyTax01Updated, "Test GetDailyData indice 40 Failed sul metodo TestBuonoMultiusoCeliachia , expected " + dailyTax01 + 2200 + " received " + dailyTax01Updated);

                    Assert.AreEqual(totTicket + 12200, totTicketUpdated, "Test GetDailyData indice 40 Failed sul metodo TestBuonoMultiusoCeliachia , expected " + totTicket + 12200 + " received " + totTicketUpdated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //     });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa l' utilizzo del  Ticket Restaurant, Esempio 20
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestTicketRestaurant()
        {
            log.Info("Performing TestTicketRestaurant Method");

            try
            {
                //     Assert.Multiple(() =>
                //     {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();


                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int totTicket = Convert.ToInt32(gc.DailyTotalTicket); // Totale ScontoAPagare

                int numTicket = Convert.ToInt32(gc.DailyTotalNumberTicket);

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                int dailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

                //Iva pagata per aliquota IVA 13 (ES) IMPOSTA
                int dailyTax13 = Int32.Parse(rData.getDailyData("40", "13").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet13 = Int32.Parse(rData.getDailyData("40", "13").Substring(0, 9));

                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.TicketRestaurant(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto
                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int totTicketUpdated = Convert.ToInt32(gc2.DailyTotalTicket); // Totale ScontoAPagare

                int numTicketUpdated = Convert.ToInt32(gc2.DailyTotalNumberTicket);

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                int dailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                ////Netto ammontare per aliquota Iva (4%)
                int dailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

                //Iva pagata per aliquota IVA 13 (ES) IMPOSTA
                int dailyTax13Updated = Int32.Parse(rData.getDailyData("40", "13").Substring(9, 9));

                ////Netto ammontare per aliquota Iva (ES)
                int dailyNet13Updated = Int32.Parse(rData.getDailyData("40", "13").Substring(0, 9));


                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(5000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                /*

                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
                <PagatoContanti>210,00</PagatoContanti>
                <PagatoElettronico>80,00</PagatoElettronico>
                <Ticket>
                <PagatoTicket>10,00</PagatoTicket>
                <NumeroTicket>1</NumeroTicket>
                </Ticket>
                </Totali>
               
                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                Assert.AreEqual("10.00", xmlStruct.totali.PagatoTicket, "Errore campo ScontoApagare dentro l'xml , expected " + "122.00" + " received " + xmlStruct.totali.PagatoTicket);
                Assert.AreEqual("1", xmlStruct.totali.NumeroTicket, "Errore campo ScontoApagare dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroTicket);
                Assert.AreEqual("80.00", xmlStruct.totali.PagatoElettronico, "Errore campo PagatoElettronico dentro l'xml , expected " + "80.00" + " received " + xmlStruct.totali.PagatoElettronico);
                Assert.AreEqual("210.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "210.00" + " received " + xmlStruct.totali.PagatoContanti);

                /*
                <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>27,05</Imposta>
                </IVA>
                <Ammontare>122,95</Ammontare>
                <ImportoParziale>122,95</ImportoParziale>
                */

                Assert.AreEqual("27.05", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "22.00" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("122.95", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122.95" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("122.95", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122.95" + " received " + xmlStruct.riepilogos[1].Ammontare);

                /*
                 * <IVA>
                    <AliquotaIVA>4.00</AliquotaIVA>
                    <Imposta>1,92</Imposta>
                    </IVA>
                    <Ammontare>48,08</Ammontare>
                    <ImportoParziale>48,08</ImportoParziale>
                 * */

                Assert.AreEqual("1.92", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "1.92" + " received " + xmlStruct.riepilogos[4].Imposta);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].Ammontare);

                /*
                 * <Natura>N4<Natura>
                    <Ammontare>100,00</Ammontare>
                    <ImportoParziale>100,00</ImportoParziale>
                    <CodiceAttivita>aaa</CodiceAttivita>
                 * 
                 */
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[0].ImportoParziale);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[0].Ammontare);



                //*****************Test sui Totalizzatori**********************


                try
                {

                    //TODO Non so se sia corretto o meno ma qui nn sale nel il totale gior ne il tasse e netto
                    Assert.AreEqual(totGiorn + 3000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestTicketRestaurant, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTax01 + dailyNet01 + 15000, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestTicketRestaurant , expected " + dailyTax01 + dailyNet01 + 15000 + " received " + dailyTax01Updated + dailyNet01Updated);

                    Assert.AreEqual(dailyTax01 + 2705, dailyTax01Updated, "Test GetDailyData indice 40 Failed sul metodo TestTicketRestaurant , expected " + dailyTax01 + 2705 + " received " + dailyTax01Updated);

                    Assert.AreEqual(dailyTax03 + dailyNet03 + 5000, dailyTax03Updated + dailyNet03Updated, "Test GetDailyData Failed sul metodo TestTicketRestaurant , expected " + dailyTax03 + dailyNet03 + 5000 + " received " + dailyTax01Updated + dailyNet01Updated);

                    Assert.AreEqual(dailyTax03 + 192, dailyTax03Updated, "Test GetDailyData indice 40 Failed sul metodo TestTicketRestaurant , expected " + dailyTax03 + 1920 + " received " + dailyTax03Updated);

                    Assert.AreEqual(dailyTax13 + dailyNet13 + 10000, dailyTax13Updated + dailyNet13Updated, "Test GetDailyData Failed sul metodo TestTicketRestaurant , expected " + dailyTax13 + dailyNet13 + 10000 + " received " + dailyTax13Updated + dailyNet13Updated);

                    Assert.AreEqual(dailyNet13 + 10000, dailyNet13Updated, "Test GetDailyData indice 40 Failed sul metodo TestTicketRestaurant , expected " + dailyNet13 + 10000 + " received " + dailyNet13Updated);

                    Assert.AreEqual(totTicket + 1000, totTicketUpdated, "Test GetDailyData indice 40 Failed sul metodo TestTicketRestaurant , expected " + totTicket + 1000 + " received " + totTicketUpdated);

                    Assert.AreEqual(numTicket + 1, numTicketUpdated, "Test GetDailyData indice 40 Failed sul metodo TestTicketRestaurant , expected " + numTicket + 1 + " received " + numTicketUpdated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //     });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }

        //Metodo che testa l' annullo del buono monouso, Esempio 21
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestAnnulloBuonoMonouso()
        {
            log.Info("Performing TestAnnulloBuonoMonouso Method");

            try
            {
                //     Assert.Multiple(() =>
                //     {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();


                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int totBuonoMonouso = Convert.ToInt32(gc.DailyTotalTicket); // Totale Buono Monouso

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                int totAnnulli = Int32.Parse(gc.FiscalRecVoid);

                Int64 granTotAnnulli = Int64.Parse(rData.getDailyData("39", "01"));

                Int64 totAnnulli01 = Int64.Parse(rData.getDailyData("42", "01"));

                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.AnnulloBuonoMonouso(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto
                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int totBuonoMonousoUpdated = Convert.ToInt32(gc2.DailyBuonoMonouso); // Totale Buono Monouso

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));
       
                int totAnnulliUpdated = Int32.Parse(gc2.FiscalRecVoid);

                Int64 granTotAnnulliUpdated = Int64.Parse(rData.getDailyData("39", "01"));

                Int64 totAnnulli01Updated = Int64.Parse(rData.getDailyData("42", "01"));


                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(5000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                /*

                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
                </Totali>
               
                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                
                /*
                <IVA>
                <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>0,00</Imposta>
                </IVA>
                <Ammontare>0,00</Ammontare>
                <ImportoParziale>0,00</ImportoParziale> 
                <TotaleAmmontareAnnulli>100,00</TotaleAmmontareAnnulli> 
                <BeniInSospeso>-100,00</BeniInSospeso>
                                */

                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("-100.00", xmlStruct.riepilogos[1].BeniInSospeso, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "-100.00" + " received " + xmlStruct.riepilogos[1].BeniInSospeso);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[1].TotaleAmmontareAnnulli, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareAnnulli);



                //*****************Test sui Totalizzatori**********************


                try
                {

                    //TODO Non so se sia corretto o meno ma qui nn sale nel il totale gior ne il tasse e netto
                    Assert.AreEqual(totGiorn + 0000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestAnnulloBuonoMonouso, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    
                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //     });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa il pagamento tramite DCRaSSN (farmacia), Esempio 22
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestNonRiscossoDaSSN()
        {
            log.Info("Performing TestNonRiscossoDaSSN Method");

            try
            {
                //     Assert.Multiple(() =>
                //     {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();


                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int totNonRiscossoDaSSN = Convert.ToInt32(gc.DailyNonRiscossoDaSSN); // Totale Non Riscosso Da SSN

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                int dailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                ////Netto ammontare per aliquota Iva (4%)
                int dailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

                //Iva pagata per aliquota IVA 13 (ES) IMPOSTA
                int dailyTax13 = Int32.Parse(rData.getDailyData("40", "13").Substring(9, 9));

                ////Netto ammontare per aliquota Iva (ES)
                int dailyNet13 = Int32.Parse(rData.getDailyData("40", "13").Substring(0, 9));
                
                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.NonRiscossoDaSSN(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto
                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int totNonRiscossoDaSSNUpdated = Convert.ToInt32(gc2.DailyNonRiscossoDaSSN); // Totale Non Riscosso Da SSN

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                //Iva pagata per aliquota IVA 3 (4%) IMPOSTA
                int dailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                ////Netto ammontare per aliquota Iva (4%)
                int dailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));

                //Iva pagata per aliquota IVA 13 (ES) IMPOSTA
                int dailyTax13Updated = Int32.Parse(rData.getDailyData("40", "13").Substring(9, 9));

                ////Netto ammontare per aliquota Iva (ES)
                int dailyNet13Updated = Int32.Parse(rData.getDailyData("40", "13").Substring(0, 9));



                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(5000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                /*

                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
                </Totali>
               
                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);

                /*
                <IVA>
               <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>0,0</Imposta>
               </IVA>
                <Ammontare>122,95</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <NonRiscossoDCRaSSN>122,95</NonRiscossoDCRaSSN>
                                */

                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("122.95", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122.95" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("122.95", xmlStruct.riepilogos[1].NonRiscossoDCRaSSN, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122.95" + " received " + xmlStruct.riepilogos[1].NonRiscossoDCRaSSN);
                
                /*
                 <IVA>
                <AliquotaIVA>4.00</AliquotaIVA>
                <Imposta>0,00</Imposta>
                </IVA>
                <Ammontare>48,08</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <NonRiscossoDCRaSSN>48.08</NonRiscossoDCRaSSN> 
                 */
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].Ammontare);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].NonRiscossoDCRaSSN, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].NonRiscossoDCRaSSN);

                /*
                <Riepilogo>
                <Natura>N4<Natura>
                <Ammontare>100,00</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <NonRiscossoDCRaSSN>100,00</NonRiscossoDCRaSSN>
                <CodiceAttivita>aaaa</CodiceAttivita>
                </Riepilogo> 
                 */

                Assert.AreEqual("0.00", xmlStruct.riepilogos[0].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[0].ImportoParziale);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[0].Ammontare);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].NonRiscossoDCRaSSN, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[0].NonRiscossoDCRaSSN);



                //*****************Test sui Totalizzatori**********************


                try
                {

                    //TODO Non so se sia corretto o meno ma qui nn sale nel il totale gior ne il tasse e netto
                    Assert.AreEqual(totGiorn + 3000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestNonRiscossoDaSSN, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(totNonRiscossoDaSSN + 30000, totNonRiscossoDaSSNUpdated, "Test Totale Giornaliero Failed sul metodo TestNonRiscossoDaSSN, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTax01 + dailyNet01 + 15000, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestNonRiscossoDaSSN , expected " + dailyTax01 + dailyNet01 + 15000 + " received " + dailyTax01Updated + dailyNet01Updated);

                    Assert.AreEqual(dailyTax01 + 0, dailyTax01Updated, "Test GetDailyData indice 40 Failed sul metodo TestNonRiscossoDaSSN , expected " + dailyTax01 + 0 + " received " + dailyTax01Updated);

                    Assert.AreEqual(dailyTax03 + dailyNet03 + 5000, dailyTax03Updated + dailyNet03Updated, "Test GetDailyData Failed sul metodo TestNonRiscossoDaSSN , expected " + dailyTax03 + dailyNet03 + 5000 + " received " + dailyTax01Updated + dailyNet01Updated);

                    Assert.AreEqual(dailyTax03 + 0, dailyTax03Updated, "Test GetDailyData indice 40 Failed sul metodo TestNonRiscossoDaSSN , expected " + dailyTax03 + 0 + " received " + dailyTax03Updated);

                    Assert.AreEqual(dailyTax13 + dailyNet13 + 10000, dailyTax13Updated + dailyNet13Updated, "Test GetDailyData Failed sul metodo TestNonRiscossoDaSSN , expected " + dailyTax13 + dailyNet13 + 10000 + " received " + dailyTax13Updated + dailyNet13Updated);

                    Assert.AreEqual(dailyNet13 + 10000, dailyNet13Updated, "Test GetDailyData indice 40 Failed sul metodo TestNonRiscossoDaSSN , expected " + dailyNet13 + 10000 + " received " + dailyNet13Updated);


                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //     });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa il metodo LotteriaScontoAPagare Esempio 23
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestLotteriaScontoAPagare()
        {
            log.Info("Performing TestLotteriaScontoAPagare Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int scontoAPagare = Convert.ToInt32(gc.ScontoAPagare); // Totale Sconto A Pagare

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.LotteriaScontoAPagare(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int scontoAPagareUpdated = Convert.ToInt32(gc2.ScontoAPagare); // Totale Sconto A Pagare

                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                

                //Faccio chiusura
                mc.ZReport();


                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Assert.Multiple(() =>
            //    {
                    //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                    /*
                    <Totali>
                    <NumeroDocCommerciali>1</NumeroDocCommerciali>
                    <PagatoContanti>122,00</PagatoContanti>
                    <ScontoApagare>22.00</ScontoApagare>

                    */
                    Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                    Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                    Assert.AreEqual("122.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml , expected " + "300.00" + " received " + xmlStruct.totali.PagatoContanti);
                    Assert.AreEqual("22.00", xmlStruct.totali.ScontoApagare, "Errore campo ScontoApagare dentro l'xml , expected " + "0.04" + " received " + xmlStruct.totali.ScontoApagare);

                    /* 
                    <IVA>
                    <IVA>
                    <AliquotaIVA>22.00</AliquotaIVA>
                    <Imposta>22.00</Imposta>
                    </IVA>
                    <Ammontare>100.00</Ammontare>
                    <ImportoParziale>100.00</ImportoParziale>
                    */


                    Assert.AreEqual("22.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "45,09" + " received " + xmlStruct.riepilogos[1].Imposta);
                    Assert.AreEqual("100.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204,95" + " received " + xmlStruct.riepilogos[1].Ammontare);
                    Assert.AreEqual("100.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204,95" + " received " + xmlStruct.riepilogos[1].ImportoParziale);

                   

                    //*****************Test sui Totalizzatori**********************
                    //TODO: ancora non pronti i totalizzatori nuovi

                    try
                    {
                        Assert.AreEqual(totGiorn + 1220000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestLotteriaScontoAPagare, expected " + (totGiorn + 7000) + "received " + totGiornUpdated);

                        Assert.AreEqual(scontoAPagare + 2200, scontoAPagareUpdated, "Test Totale Giornaliero Failed sul metodo TestLotteriaScontoAPagare, expected " + (scontoAPagare + 4) + "received " + scontoAPagareUpdated);

                        Assert.AreEqual(dailyTax01 + dailyNet01 + 12200, dailyTax01Updated + dailyNet01Updated, "Test GetDailyData Failed sul metodo TestLotteriaScontoAPagare , expected " + (dailyTax01 + dailyNet01 + 25004) + " received " + dailyTax01Updated + dailyNet01Updated);

                        Assert.AreEqual(dailyTax01 + 2200, dailyTax01Updated, "Test GetDailyData indice 40 Failed sul metodo TestLotteriaScontoAPagare , expected " + (dailyTax01 + 4509) + " received " + dailyTax01Updated);

                        
                    }
                    catch (AssertionException e)
                    {
                        //NUnit Test exception
                        log.Error("NUnit Test Exception", e);
                    }
           //     });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }



        //Metodo che testa il metodo Reso Acconto Servizi Esempio 24
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestResoAccontoServizi()
        {
            log.Info("Performing TestResoAccontoServizi Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int nonRiscossoServizi = Convert.ToInt32(gc.DailyNonRiscossoServizi); // Totale Non Riscosso Servizi

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyRefundTax01 = Int32.Parse(rData.getDailyData("41", "01"));

                int totResi = Int32.Parse(rData.getDailyData("36", "1"));

                UInt64 granTotResi =  UInt64.Parse(rData.getDailyData("38", "01"));

                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.ResoAccontoServizi(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int nonRiscossoServiziUpdated = Convert.ToInt32(gc2.DailyNonRiscossoServizi); // Totale Non Riscosso Servizi


                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyRefundTax01Updated = Int32.Parse(rData.getDailyData("41", "01"));


                int totResiUpdated = Int32.Parse(rData.getDailyData("36", "1"));


                //Faccio chiusura
                mc.ZReport();


                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Assert.Multiple(() =>
                //    {
                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                /*
                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                
                /* 
                <IVA>
                <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>-9,02</Imposta>
                </IVA>
                <Ammontare>0,00</Ammontare>
                <ImportoParziale>-40,98</ImportoParziale>
                <TotaleAmmontareResi>40,98</TotaleAmmontareResi>
                */


                Assert.AreEqual("-9.02", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "-9.02" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("-40.98", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "2-40.98" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("40.98", xmlStruct.riepilogos[1].TotaleAmmontareResi, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "40.98" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareResi);



                //*****************Test sui Totalizzatori**********************
                
                //I gran totali si aggiornano DOPO lo ZReport per cui devo leggermelo dopo la chiusura (ecco perchè l'ho messo qui)
                UInt64 granTotResiUpdated = UInt64.Parse(rData.getDailyData("38", "01"));

                try
                {
                    Assert.AreEqual(totGiorn + 0000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestResoAccontoServizi, expected " + (totGiorn + 7000) + "received " + totGiornUpdated);

                    Assert.AreEqual(nonRiscossoServizi + 00, nonRiscossoServiziUpdated, "Test Totale Giornaliero Failed sul metodo TestResoAccontoServizi, expected " + (nonRiscossoServizi + 0) + "received " + nonRiscossoServiziUpdated);

                    Assert.AreEqual(dailyRefundTax01 + 902, dailyRefundTax01Updated, "Test GetDailyData Failed sul metodo TestResoAccontoServizi , expected " + (dailyRefundTax01 + 902) + " received " + dailyRefundTax01Updated);

                    Assert.AreEqual(totResi + 5000, totResiUpdated, "Test GetDailyData indice 40 Failed sul metodo TestResoAccontoServizi , expected " + (totResi + 5000) + " received " + totResiUpdated);

                    Assert.AreEqual(granTotResi + 5000, granTotResiUpdated, "Test GetDailyData indice 40 Failed sul metodo TestResoAccontoServizi , expected " + (granTotResi + 5000) + " received " + granTotResiUpdated);


                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //     });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }




        //Metodo che testa il metodo Reso Acconto Servizi Secondo Scenario Esempio 24 bis 
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestResoAccontoServiziSecondoScenario()
        {
            log.Info("Performing TestResoAccontoServiziSecondoScenario Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);

                int nonRiscossoServizi = Convert.ToInt32(gc.DailyNonRiscossoServizi); // Totale Non Riscosso Servizi

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyRefundTax01 = Int32.Parse(rData.getDailyData("41", "01"));

                //Iva pagata per aliquota IVA 1 (4%) IMPOSTA
                int dailyRefundTax03 = Int32.Parse(rData.getDailyData("41", "03"));

                //Iva pagata per aliquota IVA 1 (ES%) IMPOSTA
                int dailyRefundTax11 = Int32.Parse(rData.getDailyData("41", "11"));

                int totResi = Int32.Parse(rData.getDailyData("36", "1"));

                UInt64 granTotResi = UInt64.Parse(rData.getDailyData("38", "01"));

                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.ResoAccontoServiziSecondoScenario(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);

                int nonRiscossoServiziUpdated = Convert.ToInt32(gc2.DailyNonRiscossoServizi); // Totale Non Riscosso Servizi


                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyRefundTax01Updated = Int32.Parse(rData.getDailyData("41", "01"));

                //Iva pagata per aliquota IVA 1 (4%) IMPOSTA
                int dailyRefundTax03Updated = Int32.Parse(rData.getDailyData("41", "03"));

                //Iva pagata per aliquota IVA 1 (ES%) IMPOSTA
                int dailyRefundTax11Updated = Int32.Parse(rData.getDailyData("41", "11"));

                int totResiUpdated = Int32.Parse(rData.getDailyData("36", "1"));

                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(10000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Assert.Multiple(() =>
                //    {
                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                /*
               <Totali>
                <NumeroDocCommerciali>3</NumeroDocCommerciali>
                <PagatoContanti>50,00</PagatoContanti>
                <PagatoElettronico>250,00</PagatoElettronico>
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("3", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                Assert.AreEqual("50.00", xmlStruct.totali.PagatoContanti, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "50.00" + " received " + xmlStruct.totali.PagatoContanti);
                Assert.AreEqual("250.00", xmlStruct.totali.PagatoElettronico, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "250.00" + " received " + xmlStruct.totali.PagatoElettronico);

                /* 
                <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>9,02</Imposta>
                </IVA>
                <Ammontare>204,92</Ammontare>
                <ImportoParziale>40,98</ImportoParziale>
                <NonRiscossoServizi>81,97</NonRiscossoServizi>
                <TotaleAmmontareResi>81,97</TotaleAmmontareResi>
                */


                Assert.AreEqual("9.02", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "9.02" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("204.92", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204.92" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("81.97", xmlStruct.riepilogos[1].NonRiscossoServizi, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "81.97" + " received " + xmlStruct.riepilogos[1].NonRiscossoServizi);
                Assert.AreEqual("81.97", xmlStruct.riepilogos[1].TotaleAmmontareResi, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "81.97" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareResi);


                /*
                <IVA>
                <AliquotaIVA>4.00</AliquotaIVA>
                <Imposta>0,00</Imposta>
                </IVA>
                <Ammontare>48,08</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <TotaleAmmontareResi>48,08</TotaleAmmontareResi>
                 */

                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].Imposta);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].TotaleAmmontareResi, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].TotaleAmmontareResi);


                /*
                <Riepilogo>
                <Natura>N4<Natura>
                <Ammontare>100</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <TotaleAmmontareResi>100,00</TotaleAmmontareResi>
                <CodiceAttivita>aaa</CodiceAttivita>
                </Riepilogo>
                 */

                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[0].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[0].ImportoParziale, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[0].ImportoParziale);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].TotaleAmmontareResi, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[0].TotaleAmmontareResi);

                //*****************Test sui Totalizzatori**********************

                //I gran totali si aggiornano DOPO lo ZReport per cui devo leggermelo dopo la chiusura (ecco perchè l'ho messo qui)
                UInt64 granTotResiUpdated = UInt64.Parse(rData.getDailyData("38", "01"));
                try
                {
                    Assert.AreEqual(totGiorn + 4000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestResoAccontoServiziSecondoScenario, expected " + (totGiorn + 7000) + "received " + totGiornUpdated);

                    Assert.AreEqual(nonRiscossoServizi + 10000, nonRiscossoServiziUpdated, "Test Totale Giornaliero Failed sul metodo TestResoAccontoServiziSecondoScenario, expected " + (nonRiscossoServizi + 0) + "received " + nonRiscossoServiziUpdated);

                    Assert.AreEqual(dailyRefundTax01 + 1803, dailyRefundTax01Updated, "Test GetDailyData Failed sul metodo TestResoAccontoServiziSecondoScenario , expected " + (dailyRefundTax01 + 902) + " received " + dailyRefundTax01Updated);

                    Assert.AreEqual(dailyRefundTax03 + 192, dailyRefundTax03Updated, "Test GetDailyData Failed sul metodo TestResoAccontoServiziSecondoScenario , expected " + (dailyRefundTax01 + 902) + " received " + dailyRefundTax01Updated);

                    Assert.AreEqual(dailyRefundTax11 + 0, dailyRefundTax11Updated, "Test GetDailyData Failed sul metodo TestResoAccontoServiziSecondoScenario , expected " + (dailyRefundTax01 + 902) + " received " + dailyRefundTax01Updated);

                    Assert.AreEqual(totResi + 25000, totResiUpdated, "Test GetDailyData indice 40 Failed sul metodo TestResoAccontoServiziSecondoScenario , expected " + (totResi + 5000) + " received " + totResiUpdated);

                    Assert.AreEqual(granTotResi + 25000, granTotResiUpdated, "Test GetDailyData indice 40 Failed sul metodo TestResoAccontoServiziSecondoScenario , expected " + (granTotResi + 5000) + " received " + granTotResiUpdated);


                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //     });

            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }





        //Metodo che testa il metodo Annullo Utilizzo Buono Monouso, esempio 25
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestAnnulloUtilizzoBuonoMonouso()
        {
            log.Info("Performing TestAnnulloUtilizzoBuonoMonouso Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);


                //Iva ANNULLATA per aliquota IVA 1 (22%) IMPOSTA
                int dailyTaxVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (22%)
                int dailyNetVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                int totAnnulli = Int32.Parse(gc.FiscalRecVoid);

                Int64 granTotAnnulli = Int64.Parse(rData.getDailyData("39", "01"));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.AnnulloUtilizzoBuonoMonouso(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);


                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTaxVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(15, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNetVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));

                int totAnnulliUpdated = Int32.Parse(gc2.FiscalRecVoid);

               



                //Faccio chiusura
                mc.ZReport();


                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                //Assert.Multiple(() =>
                //{
                /*
                <Totali>
                <NumeroDocCommerciali>3</NumeroDocCommerciali>
                <PagatoContanti>122,00</PagatoContanti>
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("3", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                Assert.AreEqual("122.00", xmlStruct.totali.PagatoContanti, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "122.00" + " received " + xmlStruct.totali.PagatoContanti);

                /* 
               <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>22,00</Imposta>
                </IVA>
                <Ammontare>200,00</Ammontare>
                <ImportoParziale>100,00</ImportoParziale>
                <BeniInSospeso>0,00</BeniInSospeso>
                <TotaleAmmontareAnnulli>100,00</TotaleAmmontareAnnulli>
                */

                Assert.AreEqual("22.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "22.00" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("200.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "200.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                //Assert.AreEqual("0.00", xmlStruct.riepilogos[1].BeniInSospeso, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].BeniInSospeso);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[1].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareAnnulli);




                //*****************Test sui Totalizzatori**********************
                //Lo metto qui perchè devo calcolarlo solo dopo lo ZReport altrimenti non cambia 

                Int64 granTotAnnulliUpdated = Int64.Parse(rData.getDailyData("39", "01"));
                try
                {
                    Assert.AreEqual(totGiorn + 000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestAnnulloUtilizzoBuonoMonouso, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTaxVoid01 + dailyNetVoid01 + 25000, dailyTaxVoid01Updated + dailyNetVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloUtilizzoBuonoMonouso , expected " + (dailyTaxVoid01 + dailyNetVoid01 + 25000) + " received " + dailyTaxVoid01Updated + dailyNetVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid01 + 4508, dailyTaxVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloUtilizzoBuonoMonouso , expected " + (dailyTaxVoid01 + 4508) + " received " + dailyTaxVoid01Updated);

                    Assert.AreEqual(totAnnulli + 12200, totAnnulliUpdated, "Test GetDailyData indice 40 Failed sul metodo TestAnnulloUtilizzoBuonoMonouso , expected " + (totAnnulli + 12200) + " received " + totAnnulliUpdated);
                    
                    Assert.AreEqual(granTotAnnulli + 12200, granTotAnnulliUpdated, "Test GetDailyData Failed sul metodo TestAnnulloUtilizzoBuonoMonouso , expected " + (granTotAnnulli + 12200) + " received " + granTotAnnulliUpdated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //});
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa il metodo Annullo Non Riscosso da SSN, esempio 26
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestAnnulloNonRiscossoDaSSN()
        {
            log.Info("Performing TestAnnulloNonRiscossoDaSSN Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);


                //Iva ANNULLATA per aliquota IVA 1 (22%) IMPOSTA
                int dailyTaxVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (22%)
                int dailyNetVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));



                //Iva ANNULLATA per aliquota IVA 1 (4%) IMPOSTA
                int dailyTaxVoid03 = Int32.Parse(rData.getDailyData("42", "3").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (4%)
                int dailyNetVoid03 = Int32.Parse(rData.getDailyData("42", "3").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (4%) IMPOSTA
                int dailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));




                //Iva ANNULLATA per aliquota IVA 1 (ES%) IMPOSTA
                int dailyTaxVoid11 = Int32.Parse(rData.getDailyData("42", "11").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (ES%)
                int dailyNetVoid11 = Int32.Parse(rData.getDailyData("42", "11").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (ES%) IMPOSTA
                int dailyTax11 = Int32.Parse(rData.getDailyData("40", "11").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet11 = Int32.Parse(rData.getDailyData("40", "11").Substring(0, 9));



                int totAnnulli = Int32.Parse(gc.FiscalRecVoid);

                Int64 granTotAnnulli = Int64.Parse(rData.getDailyData("39", "01"));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.AnnulloNonRiscossoDaSSN(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);


                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTaxVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(15, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNetVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));





                //Iva ANNULLATA per aliquota IVA 1 (4%) IMPOSTA
                int dailyTaxVoid03Updated = Int32.Parse(rData.getDailyData("42", "3").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (4%)
                int dailyNetVoid03Updated = Int32.Parse(rData.getDailyData("42", "3").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (4%) IMPOSTA
                int dailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));




                //Iva ANNULLATA per aliquota IVA 1 (ES%) IMPOSTA
                int dailyTaxVoid11Updated = Int32.Parse(rData.getDailyData("42", "11").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (ES%)
                int dailyNetVoid11Updated = Int32.Parse(rData.getDailyData("42", "11").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (ES%) IMPOSTA
                int dailyTax11Updated = Int32.Parse(rData.getDailyData("40", "11").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet11Updated = Int32.Parse(rData.getDailyData("40", "11").Substring(0, 9));





                int totAnnulliUpdated = Int32.Parse(gc2.FiscalRecVoid);





                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(10000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                //Assert.Multiple(() =>
                //{
                /*
                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);


                /* 
               <IVA>
               <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>0,0</Imposta>
                </IVA>
                <Ammontare>0,00</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <NonRiscossoDCRaSSN>-122,95</NonRiscossoDCRaSSN>
                <TotaleAmmontareAnnulli>122,95</TotaleAmmontareAnnulli>
                */

                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("-122.95", xmlStruct.riepilogos[1].NonRiscossoDCRaSSN, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "-122.95" + " received " + xmlStruct.riepilogos[1].NonRiscossoDCRaSSN);
                Assert.AreEqual("122.95", xmlStruct.riepilogos[1].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122.95" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareAnnulli);

                /*
                <IVA>
                <AliquotaIVA>4.00</AliquotaIVA>
                <Imposta>0,00</Imposta>
                </IVA>
                <Ammontare>0,00</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <NonRiscossoDCRaSSN>-48.08</NonRiscossoDCRaSSN>
                <TotaleAmmontareAnnulli>48,08</TotaleAmmontareAnnulli> 
                 */
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                Assert.AreEqual("-48.08", xmlStruct.riepilogos[4].NonRiscossoDCRaSSN, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "-48.08" + " received " + xmlStruct.riepilogos[4].NonRiscossoDCRaSSN);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].TotaleAmmontareAnnulli);


                /*
                 * <Riepilogo>
                    <Natura>N4<Natura>
                    <Ammontare>0,00</Ammontare>
                    <ImportoParziale>0,00</ImportoParziale>
                    <NonRiscossoDCRaSSN>-100,00</NonRiscossoDCRaSSN>
                    <TotaleAmmontareAnnulli>100,00</TotaleAmmontareAnnulli>
                    <CodiceAttivita>aaaa</CodiceAttivita>
                    </Riepilogo>
                 */

                Assert.AreEqual("0.00", xmlStruct.riepilogos[0].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[0].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                Assert.AreEqual("-100.00", xmlStruct.riepilogos[0].NonRiscossoDCRaSSN, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "-100.00" + " received " + xmlStruct.riepilogos[1].NonRiscossoDCRaSSN);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareAnnulli);




                //*****************Test sui Totalizzatori**********************
                //Lo metto qui perchè devo calcolarlo solo dopo lo ZReport altrimenti non cambia 

                Int64 granTotAnnulliUpdated = Int64.Parse(rData.getDailyData("39", "01"));
                try
                {
                    Assert.AreEqual(totGiorn + 000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestAnnulloNonRiscossoDaSSN, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTaxVoid01 + dailyNetVoid01 + 15000, dailyTaxVoid01Updated + dailyNetVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloNonRiscossoDaSSN , expected " + (dailyTaxVoid01 + dailyNetVoid01 + 15000) + " received " + dailyTaxVoid01Updated + dailyNetVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid01 + 2705, dailyTaxVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloNonRiscossoDaSSN , expected " + (dailyTaxVoid01 + 4508) + " received " + dailyTaxVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid03 + dailyNetVoid03 + 5000, dailyTaxVoid03Updated + dailyNetVoid03Updated, "Test GetDailyData Failed sul metodo TestAnnulloNonRiscossoDaSSN , expected " + (dailyTaxVoid03 + dailyNetVoid03 + 5000) + " received " + dailyTaxVoid03Updated + dailyNetVoid03Updated);

                    Assert.AreEqual(dailyTaxVoid03 + 192, dailyTaxVoid03Updated, "Test GetDailyData Failed sul metodo TestAnnulloNonRiscossoDaSSN , expected " + (dailyTaxVoid01 + 4508) + " received " + dailyTaxVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid11 + dailyNetVoid11 + 10000, dailyTaxVoid11Updated + dailyNetVoid11Updated, "Test GetDailyData Failed sul metodo TestAnnulloNonRiscossoDaSSN , expected " + (dailyTaxVoid11 + dailyNetVoid11 + 10000) + " received " + dailyTaxVoid11Updated + dailyNetVoid11Updated);

                    Assert.AreEqual(dailyTaxVoid11 + 0, dailyTaxVoid11Updated, "Test GetDailyData Failed sul metodo TestAnnulloNonRiscossoDaSSN , expected " + (dailyTaxVoid11 + 0) + " received " + dailyTaxVoid11Updated);

                    Assert.AreEqual(totAnnulli + 30000, totAnnulliUpdated, "Test GetDailyData indice 40 Failed sul metodo TestAnnulloNonRiscossoDaSSN , expected " + (totAnnulli + 12200) + " received " + totAnnulliUpdated);

                    Assert.AreEqual(granTotAnnulli + 30000, granTotAnnulliUpdated, "Test GetDailyData Failed sul metodo TestAnnulloNonRiscossoDaSSN , expected " + (granTotAnnulli + 12200) + " received " + granTotAnnulliUpdated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //});
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }


        //Metodo che testa il metodo Annullo DC Segue Fattura, esempio 27
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestAnnulloDCSegueFattura()
        {
            log.Info("Performing TestAnnulloDCSegueFattura Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);


                //Iva ANNULLATA per aliquota IVA 1 (22%) IMPOSTA
                int dailyTaxVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (22%)
                int dailyNetVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));



                //Iva ANNULLATA per aliquota IVA 1 (4%) IMPOSTA
                int dailyTaxVoid03 = Int32.Parse(rData.getDailyData("42", "3").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (4%)
                int dailyNetVoid03 = Int32.Parse(rData.getDailyData("42", "3").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (4%) IMPOSTA
                int dailyTax03 = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet03 = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));




                //Iva ANNULLATA per aliquota IVA 1 (ES%) IMPOSTA
                int dailyTaxVoid11 = Int32.Parse(rData.getDailyData("42", "00").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (ES%)
                int dailyNetVoid11 = Int32.Parse(rData.getDailyData("42", "00").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (ES%) IMPOSTA
                int dailyTax11 = Int32.Parse(rData.getDailyData("40", "00").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet11 = Int32.Parse(rData.getDailyData("40", "00").Substring(0, 9));



                int totAnnulli = Int32.Parse(gc.FiscalRecVoid);

                Int64 granTotAnnulli = Int64.Parse(rData.getDailyData("39", "01"));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.AnnulloDCSegueFattura(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);


                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTaxVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(15, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNetVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));





                //Iva ANNULLATA per aliquota IVA 1 (4%) IMPOSTA
                int dailyTaxVoid03Updated = Int32.Parse(rData.getDailyData("42", "3").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (4%)
                int dailyNetVoid03Updated = Int32.Parse(rData.getDailyData("42", "3").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (4%) IMPOSTA
                int dailyTax03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet03Updated = Int32.Parse(rData.getDailyData("40", "3").Substring(0, 9));




                //Iva ANNULLATA per aliquota IVA 1 (ES%) IMPOSTA
                int dailyTaxVoid11Updated = Int32.Parse(rData.getDailyData("42", "00").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (ES%)
                int dailyNetVoid11Updated = Int32.Parse(rData.getDailyData("42", "00").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (ES%) IMPOSTA
                int dailyTax11Updated = Int32.Parse(rData.getDailyData("40", "00").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet11Updated = Int32.Parse(rData.getDailyData("40", "00").Substring(0, 9));



                int totAnnulliUpdated = Int32.Parse(gc2.FiscalRecVoid);


                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(10000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                //Assert.Multiple(() =>
                //{
                /*
                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);


                /* 
               <IVA>
               <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>0,0</Imposta>
                </IVA>
                <Ammontare>0,00</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <NonRiscossoFatture>-122,95</NonRiscossoFatture>
                <TotaleAmmontareAnnulli>122,95</TotaleAmmontareAnnulli>
                */

                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("-122.95", xmlStruct.riepilogos[1].NonRiscossoFatture, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "-122.95" + " received " + xmlStruct.riepilogos[1].NonRiscossoDCRaSSN);
                Assert.AreEqual("122.95", xmlStruct.riepilogos[1].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122.95" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareAnnulli);

                /*
                <IVA>
                <IVA>
                <AliquotaIVA>4.00</AliquotaIVA>
                <Imposta>0,00</Imposta>
                </IVA>
                <Ammontare>0,00</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <NonRiscossoFatture>-48.08</NonRiscossoFatture>
                <TotaleAmmontareAnnulli>48,08</TotaleAmmontareAnnulli>
                 */
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                Assert.AreEqual("-48.08", xmlStruct.riepilogos[4].NonRiscossoFatture, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "-48.08" + " received " + xmlStruct.riepilogos[4].NonRiscossoDCRaSSN);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[4].TotaleAmmontareAnnulli);


                /*
                <Riepilogo>
                <Natura>N4<Natura>
                <Ammontare>0,00</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <NonRiscossoFatture>-100,00</NonRiscossoFatture>
                <TotaleAmmontareAnnulli>100,00</TotaleAmmontareAnnulli>
                <CodiceAttivita>aaaa</CodiceAttivita>
                </Riepilogo>
                 */

                Assert.AreEqual("0.00", xmlStruct.riepilogos[0].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[0].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[4].ImportoParziale);
                Assert.AreEqual("-100.00", xmlStruct.riepilogos[0].NonRiscossoFatture, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "-100.00" + " received " + xmlStruct.riepilogos[1].NonRiscossoDCRaSSN);
                Assert.AreEqual("100.00", xmlStruct.riepilogos[0].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[0].AliquotaIVA + ", expected " + "100.00" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareAnnulli);


                //*****************Test sui Totalizzatori**********************
                //Lo metto qui perchè devo calcolarlo solo dopo lo ZReport altrimenti non cambia 

                Int64 granTotAnnulliUpdated = Int64.Parse(rData.getDailyData("39", "01"));
                try
                {
                    Assert.AreEqual(totGiorn + 000000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestAnnulloDCSegueFattura, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTaxVoid01 + dailyNetVoid01 + 15000, dailyTaxVoid01Updated + dailyNetVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloDCSegueFattura , expected " + (dailyTaxVoid01 + dailyNetVoid01 + 15000) + " received " + dailyTaxVoid01Updated + dailyNetVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid01 + 2705, dailyTaxVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloDCSegueFattura , expected " + (dailyTaxVoid01 + 4508) + " received " + dailyTaxVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid03 + dailyNetVoid03 + 5000, dailyTaxVoid03Updated + dailyNetVoid03Updated, "Test GetDailyData Failed sul metodo TestAnnulloDCSegueFattura , expected " + (dailyTaxVoid03 + dailyNetVoid03 + 5000) + " received " + dailyTaxVoid03Updated + dailyNetVoid03Updated);

                    Assert.AreEqual(dailyTaxVoid03 + 192, dailyTaxVoid03Updated, "Test GetDailyData Failed sul metodo TestAnnulloDCSegueFattura , expected " + (dailyTaxVoid01 + 4508) + " received " + dailyTaxVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid11 + dailyNetVoid11 + 10000, dailyTaxVoid11Updated + dailyNetVoid11Updated, "Test GetDailyData Failed sul metodo TestAnnulloDCSegueFattura , expected " + (dailyTaxVoid11 + dailyNetVoid11 + 10000) + " received " + dailyTaxVoid11Updated + dailyNetVoid11Updated);

                    Assert.AreEqual(dailyTaxVoid11 + 0, dailyTaxVoid11Updated, "Test GetDailyData Failed sul metodo TestAnnulloDCSegueFattura , expected " + (dailyTaxVoid11 + 0) + " received " + dailyTaxVoid11Updated);

                    Assert.AreEqual(totAnnulli + 30000, totAnnulliUpdated, "Test GetDailyData indice 40 Failed sul metodo TestAnnulloDCSegueFattura , expected " + (totAnnulli + 12200) + " received " + totAnnulliUpdated);

                    Assert.AreEqual(granTotAnnulli + 30000, granTotAnnulliUpdated, "Test GetDailyData Failed sul metodo TestAnnulloDCSegueFattura , expected " + (granTotAnnulli + 12200) + " received " + granTotAnnulliUpdated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //});
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }



        //Metodo che testa il metodo Annullo Acconto Servizio, esempio 28
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestAnnulloAccontoServizio1()
        {
            log.Info("Performing TestAnnulloAccontoServizio1 Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);


                //Iva ANNULLATA per aliquota IVA 1 (22%) IMPOSTA
                int dailyTaxVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (22%)
                int dailyNetVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));


                int totAnnulli = Int32.Parse(gc.FiscalRecVoid);

                Int64 granTotAnnulli = Int64.Parse(rData.getDailyData("39", "01"));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.AnnulloAccontoServizio1(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);


                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTaxVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(15, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNetVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));


                int totAnnulliUpdated = Int32.Parse(gc2.FiscalRecVoid);


                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(10000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                //Assert.Multiple(() =>
                //{
                /*
                <Totali>
                <NumeroDocCommerciali>2</NumeroDocCommerciali>
                <PagamentoContanti>50,00</PagamentoContanti>
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("2", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);
                Assert.AreEqual("50.00", xmlStruct.totali.PagatoContanti, "Errore campo PagatoContanti dentro l'xml, expected " + "50.00" + " received " + xmlStruct.totali.PagatoContanti);

                /* 
               <IVA>
               <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>0,00</Imposta>
                </IVA>
                <Ammontare>122,95</Ammontare>
                <ImportoParziale>0,00</ImportoParziale>
                <TotaleAmmontareAnnulli>122,95</TotaleAmmontareAnnulli>
                <NonRiscossoServizi>0,00</NonRiscossoServizi>
                */

                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("122.95", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122.95" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                //Assert.AreEqual("0.00", xmlStruct.riepilogos[1].NonRiscossoServizi, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].NonRiscossoServizi);
                Assert.AreEqual("122.95", xmlStruct.riepilogos[1].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122.95" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareAnnulli);

                

                //*****************Test sui Totalizzatori**********************
                //Lo metto qui perchè devo calcolarlo solo dopo lo ZReport altrimenti non cambia 

                Int64 granTotAnnulliUpdated = Int64.Parse(rData.getDailyData("39", "01"));
                try
                {
                    Assert.AreEqual(totGiorn + 1500000, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestAnnulloAccontoServizio1, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTaxVoid01 + dailyNetVoid01 + 15000, dailyTaxVoid01Updated + dailyNetVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloAccontoServizio1 , expected " + (dailyTaxVoid01 + dailyNetVoid01 + 15000) + " received " + dailyTaxVoid01Updated + dailyNetVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid01 + 2705, dailyTaxVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloAccontoServizio1 , expected " + (dailyTaxVoid01 + 4508) + " received " + dailyTaxVoid01Updated);

                    Assert.AreEqual(totAnnulli + 15000, totAnnulliUpdated, "Test GetDailyData indice 40 Failed sul metodo TestAnnulloAccontoServizio1 , expected " + (totAnnulli + 12200) + " received " + totAnnulliUpdated);

                    Assert.AreEqual(granTotAnnulli + 15000, granTotAnnulliUpdated, "Test GetDailyData Failed sul metodo TestAnnulloAccontoServizio1 , expected " + (granTotAnnulli + 12200) + " received " + granTotAnnulliUpdated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //});
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }




        //Metodo che testa il metodo Annullo Acconto Servizio 2, esempio 29
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestAnnulloAccontoServizio2()
        {
            log.Info("Performing TestAnnulloAccontoServizio2 Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);


                //Iva ANNULLATA per aliquota IVA 1 (22%) IMPOSTA
                int dailyTaxVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (22%)
                int dailyNetVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));


                int totAnnulli = Int32.Parse(gc.FiscalRecVoid);

                Int64 granTotAnnulli = Int64.Parse(rData.getDailyData("39", "01"));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.AnnulloAccontoServizio2(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);


                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTaxVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(15, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNetVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));


                int totAnnulliUpdated = Int32.Parse(gc2.FiscalRecVoid);


                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(10000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                //Assert.Multiple(() =>
                //{
                /*
                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>      
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);

                /* 
               </IVA>
               <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>-9,02</Imposta>
                <Ammontare>0,00</Ammontare>
                <ImportoParziale>-40,98</ImportoParziale>
                <TotaleAmmontareAnnulli>122,95</TotaleAmmontareAnnulli>
                */

                Assert.AreEqual("-9.02", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "-9.02" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("-40.98", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "-40.98" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("122.95", xmlStruct.riepilogos[1].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "122.95" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareAnnulli);
                


                //*****************Test sui Totalizzatori**********************
                //Lo metto qui perchè devo calcolarlo solo dopo lo ZReport altrimenti non cambia 

                Int64 granTotAnnulliUpdated = Int64.Parse(rData.getDailyData("39", "01"));
                try
                {
                    Assert.AreEqual(totGiorn + 0, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestAnnulloAccontoServizio2, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTaxVoid01 + dailyNetVoid01 + 15000, dailyTaxVoid01Updated + dailyNetVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloAccontoServizio2 expected " + (dailyTaxVoid01 + dailyNetVoid01 + 15000) + " received " + dailyTaxVoid01Updated + dailyNetVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid01 + 2705, dailyTaxVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloAccontoServizio2 , expected " + (dailyTaxVoid01 + 4508) + " received " + dailyTaxVoid01Updated);

                    Assert.AreEqual(totAnnulli + 15000, totAnnulliUpdated, "Test GetDailyData indice 40 Failed sul metodo TestAnnulloAccontoServizio2 , expected " + (totAnnulli + 12200) + " received " + totAnnulliUpdated);

                    Assert.AreEqual(granTotAnnulli + 15000, granTotAnnulliUpdated, "Test GetDailyData Failed sul metodo TestAnnulloAccontoServizio2 , expected " + (granTotAnnulli + 12200) + " received " + granTotAnnulliUpdated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //});
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }



        //Metodo che testa il metodo Annullo Omaggio, Esempio 30
        //Leggo i totalizzatori prima e dopo lo scontrino e li testo.
        //Deserializzo l'XML chiusura post scontrino e testo i valori che mi interessano
        public void TestAnnulloOmaggio()
        {
            log.Info("Performing TestAnnulloOmaggio Method");

            try
            {
                //La RetrieveData mi serve cmq  perchè anche se la usa la GeneralCounter la devo cmq usare per recuperare le imposte. Dovrei usare la VatRecord ma questa è + comoda
                FiscalReceipt.Library.RetrieveData rData;
                rData = new FiscalReceipt.Library.RetrieveData();
                xml2 = new Xml2();


                FiscalReceipt.Library.GeneralCounter gc, gc2;
                gc = new FiscalReceipt.Library.GeneralCounter();
                gc2 = new FiscalReceipt.Library.GeneralCounter();

                //Aggiorno i totalizatori generali (serialization)
                GeneralCounter.SetGeneralCounter();

                //Load general counter (deserialization)
                gc = GeneralCounter.GetGeneralCounter();

                int totGiorn = Convert.ToInt32(gc.DailyTotal);


                //Iva ANNULLATA per aliquota IVA 1 (22%) IMPOSTA
                int dailyTaxVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(15, 9));

                //Netto annullato per aliquota IVA 1 (22%)
                int dailyNetVoid01 = Int32.Parse(rData.getDailyData("42", "1").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01 = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01 = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));


                int totAnnulli = Int32.Parse(gc.FiscalRecVoid);

                Int64 granTotAnnulli = Int64.Parse(rData.getDailyData("39", "01"));


                string zRep = String.Empty;
                //chiamo il metodo PrintRecRefound
                int output = xml2.AnnulloOmaggio(ref zRep);



                //Ora che ho fatto lo scontrino devo aggiornare i totalizzatori, devo farlo ora perché se faccio chiusura i dati si azzerano

                //Aggiorno i totalizatori generali
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                //Rileggo i daily total e devono essere coerenti con tutto cio' che ho fatto all'interno del metodo Acconto

                int totGiornUpdated = Convert.ToInt32(gc2.DailyTotal);


                //Iva pagata per aliquota IVA 1 (22%)
                int dailyTaxVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(15, 9));

                //Netto ammontare per aliquota Iva 1 (22%)
                int dailyNetVoid01Updated = Int32.Parse(rData.getDailyData("42", "01").Substring(5, 9));

                //Iva pagata per aliquota IVA 1 (22%) IMPOSTA
                int dailyTax01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(9, 9));

                //Giusto come controprova che funzioni come la chiamata del driver (GetVatCounter)
                int dailyNet01Updated = Int32.Parse(rData.getDailyData("40", "1").Substring(0, 9));


                int totAnnulliUpdated = Int32.Parse(gc2.FiscalRecVoid);


                //Faccio chiusura
                mc.ZReport();
                Thread.Sleep(10000);

                //*****************Test sull' XML relativo allo scontrino effettuato: *******************
                string data = DateTime.Now.ToString("yyyyMMdd");
                //Ora che ho l'xml posso creare l'oggetto XmlStruct e deserializzarlo
                Xml2.XmlStruct xmlStruct = xml2.XmlStructCreate();
                xml2.Xml2HTMLParser(String.Empty, ref xmlStruct, data, zRep);

                //Ora ho la struttura xmlStruct tutta pronta per poterla analizzare

                //Assert.Multiple(() =>
                //{
                /*
                <Totali>
                <NumeroDocCommerciali>1</NumeroDocCommerciali>      
                </Totali>

                */
                Assert.AreEqual(zRep, xmlStruct.Progressivo, "Errore campo Progressivo dentro l'xml , expected " + zRep + " received " + xmlStruct.Progressivo);
                Assert.AreEqual("1", xmlStruct.totali.NumeroDocCommerciali, "Errore campo NumeroDocCommerciali dentro l'xml , expected " + "1" + " received " + xmlStruct.totali.NumeroDocCommerciali);

                /* 
               <IVA>
                <AliquotaIVA>22.00</AliquotaIVA>
                <Imposta>-45,08</Imposta>
                </IVA>
                <Ammontare>0</Ammontare>
                <ImportoParziale>-204,92</ImportoParziale>
                <TotaleAmmontareAnnulli>204,92</TotaleAmmontareAnnulli>
                */

                Assert.AreEqual("-45.08", xmlStruct.riepilogos[1].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "-45.08" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[1].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("-204.92", xmlStruct.riepilogos[1].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "-204.92" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("204.92", xmlStruct.riepilogos[1].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[1].AliquotaIVA + ", expected " + "204.92" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareAnnulli);

                /* 
               <IVA>
                <IVA>
                <AliquotaIVA>4.00</AliquotaIVA>
                <Imposta>-1,92</Imposta>
                </IVA>
                <Ammontare>0</Ammontare>
                <ImportoParziale>-48,08</ImportoParziale>
                < TotaleAmmontareAnnulli>48,08</TotaleAmmontareAnnulli>
                */

                Assert.AreEqual("-1.92", xmlStruct.riepilogos[4].Imposta, "Errore campo Imposta relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "-1.92" + " received " + xmlStruct.riepilogos[1].Imposta);
                Assert.AreEqual("0.00", xmlStruct.riepilogos[4].Ammontare, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "0.00" + " received " + xmlStruct.riepilogos[1].Ammontare);
                Assert.AreEqual("-48.08", xmlStruct.riepilogos[4].ImportoParziale, "Errore campo Ammontare relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "-48.08" + " received " + xmlStruct.riepilogos[1].ImportoParziale);
                Assert.AreEqual("48.08", xmlStruct.riepilogos[4].TotaleAmmontareAnnulli, "Errore campo ImportoParziale relativo alla IVA" + xmlStruct.riepilogos[4].AliquotaIVA + ", expected " + "48.08" + " received " + xmlStruct.riepilogos[1].TotaleAmmontareAnnulli);


                //*****************Test sui Totalizzatori**********************
                //Lo metto qui perchè devo calcolarlo solo dopo lo ZReport altrimenti non cambia 

                Int64 granTotAnnulliUpdated = Int64.Parse(rData.getDailyData("39", "01"));
                try
                {
                    Assert.AreEqual(totGiorn + 0, totGiornUpdated, "Test Totale Giornaliero Failed sul metodo TestAnnulloOmaggio, expected " + (totGiorn + 3000000) + "received " + totGiornUpdated);

                    Assert.AreEqual(dailyTaxVoid01 + dailyNetVoid01 + 25000, dailyTaxVoid01Updated + dailyNetVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloOmaggio expected " + (dailyTaxVoid01 + dailyNetVoid01 + 15000) + " received " + dailyTaxVoid01Updated + dailyNetVoid01Updated);

                    Assert.AreEqual(dailyTaxVoid01 + 4508, dailyTaxVoid01Updated, "Test GetDailyData Failed sul metodo TestAnnulloOmaggio , expected " + (dailyTaxVoid01 + 4508) + " received " + dailyTaxVoid01Updated);

                    Assert.AreEqual(totAnnulli + 30000, totAnnulliUpdated, "Test GetDailyData indice 40 Failed sul metodo TestAnnulloOmaggio , expected " + (totAnnulli + 12200) + " received " + totAnnulliUpdated);

                    Assert.AreEqual(granTotAnnulli + 30000, granTotAnnulliUpdated, "Test GetDailyData Failed sul metodo TestAnnulloOmaggio , expected " + (granTotAnnulli + 12200) + " received " + granTotAnnulliUpdated);

                }
                catch (AssertionException e)
                {
                    //NUnit Test exception
                    log.Error("NUnit Test Exception", e);
                }
                //});
            }
            catch (AssertionException e)
            {
                log.Error("", e);
                //throw e;
            }
            catch (Exception err)
            {
                log.Error("", err);
            }

        }

        /*
        [Test]
        [Category("pass")]
        public void FiscalDocumentFunctionalTest()
        {
            // Act
            int output = fd.testFiscalDocumentClass(printerName);

            //Assert
            Assert.AreEqual(0, output);
        }

        [Test]
        [Category("pass")]
        public void ReportFunctionalTest()
        {
            // Act
            int output = rp.testReportClass(printerName);

            //Assert
            Assert.AreEqual(0, output);
        }
        */
        [TearDown]
        public void TestTearDown()
        {
            mc = null;
            //fd = null;
            //rp = null;

            //elimino il file degli oggetti parsati

            Console.WriteLine("TEST FINISHED, PRESS ENTER TO EXIT");
            Console.ReadLine();
        }









        /*



        //test per deserializzare json file from webservices


        public class DynamicJsonObject : DynamicObject
        {
            private IDictionary<string, object> Dictionary { get; set; }

            public DynamicJsonObject(IDictionary<string, object> dictionary)
            {
                this.Dictionary = dictionary;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = this.Dictionary[binder.Name];

                if (result is IDictionary<string, object>)
                {
                    result = new DynamicJsonObject(result as IDictionary<string, object>);
                }
                else if (result is ArrayList && (result as ArrayList) is IDictionary<string, object>)
                {
                    result = new List<DynamicJsonObject>((result as ArrayList).ToArray().Select(x => new DynamicJsonObject(x as IDictionary<string, object>)));
                }
                else if (result is ArrayList)
                {
                    result = new List<object>((result as ArrayList).ToArray());
                }

                return this.Dictionary.ContainsKey(binder.Name);
            }
        }




        public class DynamicJsonConverter : JavaScriptConverter
        {
            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                if (dictionary == null)
                    throw new ArgumentNullException("dictionary");

                if (type == typeof(object))
                {
                    return new DynamicJsonObject(dictionary);
                }

                return null;
            }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<Type> SupportedTypes
            {
                get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(object) })); }
            }
        }



    */


        static void Main(string[] args)
        {


            /*

            string URL = "http://10.15.17.201/www/json_files/zrep.json";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            //request.ContentType = "application/json; charset=utf-8";
            //request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("username:password"));
            //request.PreAuthenticate = true;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;


           


            using (Stream responseStream = response.GetResponseStream())

            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                string data = reader.ReadToEnd();

                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });

                dynamic entry = jss.Deserialize(data, typeof(object)) as dynamic;

                Console.WriteLine("provo a stampare lo z report " + entry.zRepNumber.Substring(1,4));

                int zrep = Int32.Parse(entry.zRepNumber.Substring(1, 4));

            }




    */











            try
            {
                //crea il doc xml (da rivedere nel formato corretto)
                // myXmlDocument xmlDocument = new myXmlDocument();

                //Make an XML parser and store data into result.txt file
                Parser myparser = new Parser();

                //Class test
                CustomTests test = new CustomTests();
                test.InitAccount();

                test.DynamicTest(listofTests);
                test.TestTearDown();




            }
            catch (Exception e)
            {
                log.Error(e.Message);

            }
        }
    }
}