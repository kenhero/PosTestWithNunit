

using NUnit.Framework;
using System;
using Microsoft.PointOfService;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using LibraryTests;
using System.Xml.Linq;
using System.Xml;


// Used for file and directory
// manipulation
using System.IO;
using System.Reflection;

using FiscalReceipt.Library;
//using FiscalDocument.Library;
//using Report.Library;

namespace LibraryTests
{


    [TestFixture]
    public class CustomTests
    {

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //IEnumerable<XElement> child1 = XDocument.Load("myConfiguration.xml").Element("State").Elements();

        readonly string prova = "FiscalReceipt";
        readonly string printerName = "fiscalPrinter";
        readonly string electronicJournalName = "ElectronicJournal";
        static FiscalReceipt.Library.FiscalReceipt mc;
        static ElectronicJournal.Library.ElectronicJournal ej;
        static FiscalReceipt.Library.VatManager vm;
        //Make an XML parser and store data into result.txt file
        Parser myparser = new Parser();
        static string[] listofTests;

        //FiscalDocument.Library.FiscalDocument fd;
        //Report.Library.Report rp;

        //[SetUp]
        //[Test]
        //[Category("pass")]
        public void InitAccount()
        {
            //arrange
            string printerName = "fiscalPrinter";
            mc = new FiscalReceipt.Library.FiscalReceipt();
            //ej = new ElectronicJournal.Library.ElectronicJournal();
            //vm = new FiscalReceipt.Library.VatManager();
            //fd = new FiscalDocument.Library.FiscalDocument(printerName);
            //rp = new Report.Library.Report(printerName);
            listofTests = listOfTest();
            Console.WriteLine(listofTests);
        }

        public string[] listOfTest()
        {
            //NOTA IMP: Mettere il path assoluto della cartella XmlFolder altrimenti se si lancia NUnit Test prende come riferimento non dove si trova il codice ma dove si trova Nunit.exe ,ossia dentro visual studio IDe!!!
            string[] fileArray = Directory.GetFiles(@"..\..\..\XmlFolder", "*.txt", SearchOption.TopDirectoryOnly);
            Array.Sort(fileArray);
            return fileArray;
        }

        //[Test, TestCaseSource("listOfTests")]
        [Test]
        [Category("pass")]
        //parsing della directory XmlFolder con tutti i file xml di test
        public void DynamicTest()
        {
            InitAccount();

            //parsing della directory XmlFolder con tutti i file xml di test
            //NOTA IMP: Mettere il path assoluto della cartella XmlFolder
            string[] fileArray = Directory.GetFiles(@"..\..\..\XmlFolder", "*.txt", SearchOption.TopDirectoryOnly);

            foreach (string namefile in fileArray)
            {

                //test to read class,method and params from XML parser and create the right class and call the right method
                string textFileParserResult = namefile;
                //SharedClass.Globals.FILE_NAME = namefile.Substring(12);

                //Per trasferire il nome del file xml nel contesto di log4net
                log4net.GlobalContext.Properties["XmlFile"] = namefile.Substring(12);

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
                    Type type = null;
                    try
                    {
                        type = Type.GetType(className);
                    }
                    catch(Exception e)
                    {
                        log.Error("L'errore è il seguente", e);
                    }
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

                    }

                }
                sr.Close();
            }
            Assert.AreEqual(0, FiscalReceipt.Library.FiscalReceipt.NumExceptions);
            /*
            // Arrange
            FiscalReceipt.Library.FiscalReceipt mc = new FiscalReceipt.Library.FiscalReceipt();
            // Act
            int output = mc.testFiscalReceiptClass(printerName);
            //mc.testFiscalReceiptClass();
            Console.WriteLine("output test = " + output);
            //Console.ReadLine();
            // Assert
            Assert.AreEqual(0, output);
            //Assert.Fail();
            */

        }
        
        /*
        [Test]
        [Category("pass")]
        public void TestPrintRecItem(string description, string price, string quantity, string vatInfo, string unitPrice)
        {

            //log.Fatal("prova 123 log");

            FiscalReceipt.Library.GeneralCounter gc, gc2;
            gc = new FiscalReceipt.Library.GeneralCounter();
            gc2 = new FiscalReceipt.Library.GeneralCounter();

            int output = mc.PrintRecItem(description, price, quantity, vatInfo, unitPrice, ref gc, ref gc2);
            //Qui devo fare il compare tra gc e gc2 e vedere se c'è coerenza in base alla printerecitem e printrectotal


            Assert.AreEqual(0, output, "Test TestPrintRecItem Failed, generic error");
            //Faccio una vendita di n oggetti per cui mi aspetto che il totalizzatorei ScontriniFiscali incrementi di 1
            Assert.AreEqual(Int32.Parse(gc.FiscalRec) + 1, Int32.Parse(gc2.FiscalRec), "TestPrintRecItem Failed on gc.FiscalRec, expected " + gc.FiscalRec + 1 + "Received " + gc2.FiscalRec + "\r\n", gc.FiscalRec, gc2.FiscalRec);
            //Nel test faccio TRE vendite di (unitPrice * quantity) oggetti
            Assert.AreEqual((Int32.Parse(gc.DailyTotal) + (Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3)), Int32.Parse(gc2.DailyTotal), "TestPrintRecItem Failed on gc.DailyTotal, expected " + (gc.DailyTotal + (Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3)) + "Received " + gc2.DailyTotal + "\r\n", gc.DailyTotal, gc2.DailyTotal);
            //Assert.Fail();


        }

        */
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

            //Console.WriteLine("IT'S ALL RIGHT, PRESS ENTER TO EXIT");
            //Console.ReadLine();
        }
    }
}