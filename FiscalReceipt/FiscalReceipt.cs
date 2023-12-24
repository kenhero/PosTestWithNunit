using System;
using System.Globalization;
using System.Threading;
using System.Runtime.Serialization;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.PointOfService;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Dynamic;
using System.Collections.ObjectModel;
//using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
//using System.Dynamic;
using System.Xml.Schema;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Net.Sockets;

namespace FiscalReceipt.Library
{

    public class FiscalReceipt
    {
        public static PosExplorer posExplorer;
        public static PosCommon posCommonFP;
        public static PosCommon posCommonEJ;
        public static FiscalPrinter fiscalprinter;

        //exception counter
        public static int NumExceptions = 0;

        //firmware release
        public static string FirmwareVersion = "";
        //log4net member
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Check per verificare che l'oggetto esiste già o meno in memoria
        private static bool instance = false;
        //Check per evitare di rifare la procedura open claim enable se non è stata fatta la Close prima (altrimenti la classe FiscalPrinter è instabile)
        protected static bool opened = false;

        //Variabile che indica in che modalità ci troviamo (MF, RT o Demo)
        protected static string Mode = "";

        //Struttura creata per recuperare dal DGFE i range di ogni Zrep (inizio e fine scontrino).
        //Purtroppo non esiste un comando che mi da queste info 

        public struct ZrepRange
        {
            public int Zrep;    //Numero di ZRep
            public int start;   //Primo scontrino fiscale di quel ZRep IN QUELLA DATA***
            public int finish; //Ultimo scontrino di quel ZRep IN QUELLA DATA***
            public string date; //data dello scontrino
           
            //public string numScon;
            //public bool  isLottery;
           

        }

        // *** TODO: 17/02 Ho dovuto inserire start in ZrepRange perchè a cavallo della mezzanotte o dopo posso continuare
        // a fare scontrini senza far chiusura per cui quando loopo nell array devo sapere IN QUEL GIORNO qual è
        // stato il primo scontrino di quel ZRep perchè non necessariamente è lo scontrino 1 ma potrebbe essere lo scontrino 0729!!!
        /*
        public static FiscalReceipt Instance = new FiscalReceipt();
        public void Reset()
        {
            Instance = new FiscalReceipt();
        }

        */

        //FiscalReceipt Base Class
        public FiscalReceipt()
        {

            try
            {
                //if it's first time
                if (!instance)
                {
                    instance = true;
                    posExplorer = new PosExplorer();
                    // Console.WriteLine("Taking FiscalPrinter device ");
                    DeviceInfo fp = posExplorer.GetDevice("FiscalPrinter", "FiscalPrinter");

                    posCommonFP = (PosCommon)posExplorer.CreateInstance(fp);
                    posCommonFP.StatusUpdateEvent += new StatusUpdateEventHandler(co_OnStatusUpdateEvent);
                }

            }
            catch (Exception e)
            {
                NumExceptions++;
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);
                }
                else
                {
                    log.Fatal("", e);
                }
            }
        }

        public bool Instance { get; }

        public PosExplorer PosExplorer
        {
            get { return posExplorer; }
            //set {   posExplorer = new PosExplorer();    }
        }

        public PosCommon PosCommonFP
        {
            get { return posCommonFP; }
            //set {  }
        }

        private void co_OnStatusUpdateEvent(object source, StatusUpdateEventArgs d)
        {
            try
            {
                Console.WriteLine(d.ToString());

                string text = "unknown";
                switch (d.Status)
                {
                    case Microsoft.PointOfService.FiscalPrinter.StatusCoverOK:
                        text = "Cover OK";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusCoverOpen:
                        text = "Cover Open";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusIdle:
                        text = "Idle";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusJournalCoverOK:
                        text = "Journal Cover OK";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusJournalCoverOpen:
                        text = "Journal Cover Open";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusJournalEmpty:
                        text = "Journal Empty";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusJournalNearEmpty:
                        text = "Journal Near Empty";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusJournalPaperOK:
                        text = "Journal Paper OK";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusPowerOff:
                        text = "Power Off";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusPowerOffline:
                        text = "Power Offline";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusPowerOffOffline:
                        text = "Power Off Offline";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusPowerOnline:
                        text = "Power Online";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusReceiptCoverOK:
                        text = "Receipt Cover OK";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusReceiptCoverOpen:
                        text = "Receipt Cover Open";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusReceiptEmpty:
                        text = "Receipt Empty";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusReceiptNearEmpty:
                        text = "Receipt Near Empty";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusReceiptPaperOK:
                        text = "Receipt Paper OK";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusSlipCoverOK:
                        text = "Slip Cover OK";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusSlipCoverOpen:
                        text = "Slip Cover Open";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusSlipEmpty:
                        text = "Slip Empty";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusSlipNearEmpty:
                        text = "Slip Near Empty";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusSlipPaperOK:
                        text = "Slip Paper OK";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusUpdateFirmwareComplete:
                        text = "Firmware Complete";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusUpdateFirmwareCompleteDeviceNotRestored:
                        text = "Firmware Complete Device Not Restored";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusUpdateFirmwareFailedDeviceNeedsFirmware:
                        text = "Firmware Failed Device Needs Firmware";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusUpdateFirmwareFailedDeviceOk:
                        text = "Firmware Failed Device Ok";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusUpdateFirmwareFailedDeviceUnknown:
                        text = "Firmware Failed Device Unknown";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusUpdateFirmwareFailedDeviceUnrecoverable:
                        text = "Firmware Failed Device Unrecoverable";
                        break;
                    case Microsoft.PointOfService.FiscalPrinter.StatusUpdateFirmwareProgress:
                        text = "Firmware Progress";
                        break;

                    default:
                        text = "Unexpected status code: " + d.Status.ToString();
                        break;
                }

                Console.WriteLine("Status: " + text);
            }
            catch (Exception ae)
            {
                Console.WriteLine("Exception: " + ae);
            }
        }



        //Method to initialize POS Device
        public int initFiscalDevice(string printerName)
        {
            try
            {
                log.Info("Performing initFiscalDevice() Method");
                // test
                if (!opened)
                {// Console.WriteLine("Initializing PosExplorer ");

                    posExplorer = new PosExplorer();

                    // Console.WriteLine("Taking FiscalPrinter device ");
                    DeviceInfo fp = posExplorer.GetDevice("FiscalPrinter", printerName);

                    // Console.WriteLine("Creating instance of FiscalPrinter device ");
                    posCommonFP = (PosCommon)posExplorer.CreateInstance(fp);
                    posCommonFP.StatusUpdateEvent += new StatusUpdateEventHandler(co_OnStatusUpdateEvent);

                    // Console.WriteLine("Initializing FiscalPrinter ");
                    fiscalprinter = (FiscalPrinter)posCommonFP;
                }
                
                //EDIT: 14/10/19 if inserito per evitare una exception causa due eventuali Open durante le inizializzazioni: faccio la Close() e poi inizializzo l'oggetto FiscalPrinter
                if (!(fiscalprinter.State == ControlState.Closed))
                {
                    fiscalprinter.DeviceEnabled = false;

                    fiscalprinter.Release();

                    fiscalprinter.Close();
                }

                

                //Console.WriteLine("Performing Open() method ");
                fiscalprinter.Open();

                //Console.WriteLine("Performing Claim() method ");
                fiscalprinter.Claim(10000);

                //Console.WriteLine("Setting DeviceEnabled property ");
                fiscalprinter.DeviceEnabled = false;
                fiscalprinter.DeviceEnabled = true;

                opened = true;

                //Metodo chiamato per inizializzare la variable globale FirmwareVersion
                checkFirmwareVersion();
                //Chiamo questo metodo per inizializzare la variabile statica che mi definisce lo stato del POS tramite la variabile Mode
                //checkRtStatus();

                //Console.WriteLine("Performing ResetPrinter() method ");
                //fiscalprinter.ResetPrinter();

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                closeFiscalDevice();
                //non ha senso tecnicamente fare la reset qui considerando che sta fallendo la init...cosa vuoi resettare ,creten!
                //fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal(pce.Message);

                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Fatal(e.ToString());
                }

                
            }
            return NumExceptions;
        }


        //Method to Close POS Device (fondamentale per test multipli, da inserire come ultimo metodo nell 'xml
        public int closeFiscalDevice()
        {
            try
            {
                /* test
                // Console.WriteLine("Initializing PosExplorer ");
                posExplorer = new PosExplorer();

                // Console.WriteLine("Taking FiscalPrinter device ");
                DeviceInfo fp = posExplorer.GetDevice("FiscalPrinter", printerName);

                // Console.WriteLine("Creating instance of FiscalPrinter device ");
                posCommonFP = (PosCommon)posExplorer.CreateInstance(fp);
                posCommonFP.StatusUpdateEvent += new StatusUpdateEventHandler(co_OnStatusUpdateEvent);
                
                */
                // Console.WriteLine("Initializing FiscalPrinter ");
                //fiscalprinter = (FiscalPrinter)posCommonFP;

                log.Info("Performing closeFiscalDevice() method ");
                if (!(fiscalprinter.State == ControlState.Closed))
                {
                    //fiscalprinter.DeviceEnabled = false;

                    //fiscalprinter.Release();

                    fiscalprinter.Close();
                }
                //fiscalprinter.Close();
                //Resetto la FiscalReceipt class
                opened = false;


            }
            catch (Exception e)
            {
               

                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal(pce.Message);

                }
                else
                {
                   
                    log.Fatal(e.ToString());
                }

                return NumExceptions;
            }
            return NumExceptions;
        }


        //Method to check Firmware Version 1074
        public int checkFirmwareVersion()
        {
            try
            {
                log.Info("Performing checkFirmwareVersione");
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1074, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                FirmwareVersion = iObj[0].Substring(2, 5);
                int FiscalMemoryStatus = Convert.ToInt32(iObj[0].Substring(7, 1));
                if (FiscalMemoryStatus != 0)
                {
                    log.Error("Errore Fiscal Memory Status, expected 0 received: " + FiscalMemoryStatus);
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal(pce.Message);

                }
                else
                {
                    
                    log.Fatal(e.ToString());
                }

                return NumExceptions;
            }
            return NumExceptions;
        }

        //Method to test the original FiscalReceiptClass
        public int testFiscalReceiptClass(string printerName)
        {

            log.Info("Performing testFiscalReceiptClass() method ");
            try
            {
                /*test
                //sta parte qui di inizializzazione Pos ce l'ho qui ma ho creato un method apposta e quindi devo cancellarlo
                // Console.WriteLine("Initializing PosExplorer ");
                posExplorer = new PosExplorer();

                // Console.WriteLine("Taking FiscalPrinter device ");
                DeviceInfo fp = posExplorer.GetDevice("FiscalPrinter", printerName);

                // Console.WriteLine("Creating instance of FiscalPrinter device ");
                posCommonFP = (PosCommon)posExplorer.CreateInstance(fp);
                posCommonFP.StatusUpdateEvent += new StatusUpdateEventHandler(co_OnStatusUpdateEvent);

                */

                if (!opened)
                {// Console.WriteLine("Initializing FiscalPrinter ");
                    FiscalPrinter fiscalprinter = (FiscalPrinter)posCommonFP;

                    //Console.WriteLine("Performing Open() method ");
                    fiscalprinter.Open();

                    //Console.WriteLine("Performing Claim() method ");
                    fiscalprinter.Claim(1000);

                    //Console.WriteLine("Setting DeviceEnabled property ");
                    fiscalprinter.DeviceEnabled = true;
                }
                //Console.WriteLine("Performing ResetPrinter() method ");
                fiscalprinter.ResetPrinter();


                // RT Specific Commands - BEG

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                Boolean isRT = false;
                string strDataOracle = "";
                string strData = "";
                string rtType = "";
                string printerId = "";
                string strDate = "";
                string zRepNum = "";
                string recNum = "";

                // Check RT status 
                //Console.WriteLine("DirectIO (RT status)");
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                //Check if  Printer is in RT o non RT mode ( check quarto e quinto byte 1138 command ,secondo protocollo
                //In RT faccio i test,ossia mi prendo i totalizzatori vari ,altrimenti non RT vuol dire Misuratore Fiscale
                //quindi eseguo gli scontrini con le varie combinazioni che verranno successivamente testate rianalizzando
                //i vari totalizzatori,ossia ritornando in modalità RT
                //Lo switch tra RT e non RT la farà via sw con le directIO 
                string mainStatus = iObj[0].Substring(3, 2);
                rtType = iObj[0].Substring(2, 1);
                //Console.WriteLine("RT type: " + rtType);

                int mainStatusInt = Convert.ToInt32(mainStatus);
                if (mainStatusInt == 1)
                {
                    isRT = false;
                    //Console.WriteLine("Printer is NOT RT mode");
                }
                else
                if (mainStatusInt == 2)
                {
                    isRT = true;
                    //Console.WriteLine("Printer is in RT mode");
                }
                else
                {
                    log.Error("Error DirectIO 1138 campo MAIN, valori attesi 1(MF) o 2(RT), ricevuto invece : " + mainStatusInt);
                }

                if (isRT)
                {
                    // Fiscal receipt
                    //Console.WriteLine("Performing BeginFiscalReceipt() method ");
                    //fiscalprinter.BeginFiscalReceipt(true);
                    //Console.WriteLine("Performing PrintRecItem() method ");
                    //fiscalprinter.PrintRecItem("ITEM", (decimal)10000, (int)1000, (int)3, (decimal)10000, "");
                    //Console.WriteLine("Performing PrintRecTotal() method ");
                    //fiscalprinter.PrintRecTotal((decimal)10000, (decimal)10000, "0CONTANTI");
                    //Console.WriteLine("Performing EndFiscalReceipt() method ");
                    //fiscalprinter.EndFiscalReceipt(false);



                    // Fiscal receipt
                    //Console.WriteLine("Performing BeginFiscalReceipt() method ");
                    fiscalprinter.BeginFiscalReceipt(true);
                    //Console.WriteLine("Performing PrintRecItem() method ");
                    fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecItem("TSHIRT", (decimal)10000, (int)1000, (int)2, (decimal)12000, "");
                    fiscalprinter.PrintRecItem("CAPPELLO", (decimal)10000, (int)1000, (int)3, (decimal)30000, "");
                    fiscalprinter.PrintRecItem("BORSA", (decimal)10000, (int)1000, (int)4, (decimal)50000, "");

                    //Console.WriteLine("Performing PrintRecTotal() method ");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "0CONTANTI");
                    //Console.WriteLine("Performing EndFiscalReceipt() method ");
                    fiscalprinter.EndFiscalReceipt(false);

                    zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;



                    // Get printer ID
                    //Console.WriteLine("Get Data (Printer ID)");
                    string printerIdModel;
                    string printerIdManufacturer;
                    string printerIdNumber;
                    //Comando -35000 oracle
                    strDataOracle = fiscalprinter.GetData(FiscalData.PrinterId, (int)-35000).Data;
                    strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                    //Console.WriteLine("Returned printerId: " + strData);
                    printerIdModel = strData.Substring(0, 2);
                    printerIdNumber = strData.Substring(4, 6);
                    printerIdManufacturer = strData.Substring(2, 2);
                    printerId = printerIdManufacturer + rtType + printerIdModel + printerIdNumber;

                    if (!(String.Compare(strDataOracle, printerId) == 0))
                    {
                        log.Error("In RT/Demo Errore di uniformità tra lettura matricola comando Oracle e comando standard: con comando Oracle leggo: " + strDataOracle + "\r\n Con comando standard leggo : " + strData);
                    }
                    //Console.WriteLine("Printer Id: " + printerId);
                    //printerId = strDataOracle;

                    // Get date
                    //Console.WriteLine("Performing GetDate() method ");
                    strData = fiscalprinter.GetDate().ToString();
                    //Console.WriteLine("Date: " + strData);
                    strDate = strData.Substring(0, 2) + strData.Substring(3, 2) + strData.Substring(6, 4);
                    //Console.WriteLine("Date: " + strDate);

                    // Get Z report
                   
                    zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                    int nextInt = Int32.Parse(zRepNum) + 1;
                    zRepNum = nextInt.ToString("0000");
                   

                    // Get rec num
                    //Console.WriteLine("Get Data (Fiscal Rec)");
                    // recNum = fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data;
                    recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;
                    //Console.WriteLine("Rec Num: " + recNum);

                    // Check document is refundable
                    //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                    strObj[0] = "1" + printerId + strDate + recNum + zRepNum;	// "1" = refund
                    dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                    iData = dirIO.Data;
                    //Console.WriteLine("DirectIO(): iData = " + iData);
                    iObj = (string[])dirIO.Object;
                    //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                    int iRet = Int32.Parse(iObj[0]);
                    if (iRet == 0)
                        log.Info("Document can be Refunded");
                    else
                    {
                        log.Error("Document can NOT be Refunded");
                    }
                    // Return document print
                    //Console.WriteLine("DirectIO (Return document print)");
                    strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                    iData = dirIO.Data;
                    //Console.WriteLine("DirectIO(): iData = " + iData);
                    iObj = (string[])dirIO.Object;
                    //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)10000, (int)1);
                    fiscalprinter.PrintRecTotal((decimal)30000, (decimal)00000, "0CONTANTI");
                    fiscalprinter.EndFiscalReceipt(false);

                    // Fiscal receipt
                    //Console.WriteLine("Performing BeginFiscalReceipt() method ");
                    fiscalprinter.BeginFiscalReceipt(true);
                    //Console.WriteLine("Performing PrintRecItem() method ");
                    fiscalprinter.PrintRecItem("ITEM", (decimal)10000, (int)1000, (int)1, (decimal)10000, "");
                    //Console.WriteLine("Performing PrintRecTotal() method ");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)10000, "0CONTANTI");
                    //Console.WriteLine("Performing EndFiscalReceipt() method ");
                    fiscalprinter.EndFiscalReceipt(false);

                    // Get rec num
                    //Console.WriteLine("Get Data (Fiscal Rec)");
                    // recNum = fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data;
                    recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;
                    //Console.WriteLine("Rec Num: " + recNum);

                    // Check document is voidable
                    //Console.WriteLine("DirectIO (Check document is voidable)");
                    strObj[0] = "2" + printerId + strDate + recNum + zRepNum;	// "2" = void
                    dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                    iData = dirIO.Data;
                    //Console.WriteLine("DirectIO(): iData = " + iData);
                    iObj = (string[])dirIO.Object;
                    //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                    int iRet2 = Int32.Parse(iObj[0]);
                    if (iRet2 == 0)
                        log.Info("Document voidable");
                    else
                        log.Error("Document NOT voidable");

                    // Void document print
                    //Console.WriteLine("DirectIO (Void document print)");
                    strObj[0] = "0140001VOID " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                    iData = dirIO.Data;
                    //Console.WriteLine("DirectIO(): iData = " + iData);
                    iObj = (string[])dirIO.Object;
                    //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                    // if the receipt is voided, printer return "operator + 50" (for ex. 01+50=51)

                    // Return documents totals
                    //Console.WriteLine("DirectIO (Return documents totals)");
                    // Daily
                    //Console.WriteLine("Daily");
                    strObj[0] = "3600";
                    dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                    iData = dirIO.Data;
                    //Console.WriteLine("DirectIO(): iData = " + iData);
                    iObj = (string[])dirIO.Object;
                    //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                    // Grand total
                    //Console.WriteLine("Grand total");
                    strObj[0] = "3800";
                    dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                    iData = dirIO.Data;
                    //Console.WriteLine("DirectIO(): iData = " + iData);
                    iObj = (string[])dirIO.Object;
                    //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                    // Void documents totals
                    //Console.WriteLine("DirectIO (Void documents totals)");
                    // Daily
                    //Console.WriteLine("Daily");
                    strObj[0] = "3700";
                    dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                    iData = dirIO.Data;
                    //Console.WriteLine("DirectIO(): iData = " + iData);
                    iObj = (string[])dirIO.Object;
                    //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                    // Grand total
                    //Console.WriteLine("Grand total");
                    strObj[0] = "3900";
                    dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                    iData = dirIO.Data;
                    //Console.WriteLine("DirectIO(): iData = " + iData);
                    iObj = (string[])dirIO.Object;
                    //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                    // RT Specific Commands - END
                }
                else
                {
                    //********************************************************************************//
                    // FISCAL RECEIPT
                    //Console.WriteLine("Performing BeginFiscalReceipt() method ");




                    // Get printer ID
                    // Test Coerenza tra Comando Oracle Lettura Matricola e Comando standard (che necessità anche di una 1138.... bah)
                    string printerIdModel;
                    string printerIdManufacturer;
                    string printerIdNumber;
                    strDataOracle = fiscalprinter.GetData(FiscalData.PrinterId, (int)-35000).Data;
                    strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                    printerIdModel = strData.Substring(0, 2);
                    printerIdNumber = strData.Substring(4, 6);
                    printerIdManufacturer = strData.Substring(2, 2);
                    printerId = printerIdManufacturer + rtType + printerIdModel + printerIdNumber;
                    if (!(String.Compare(strDataOracle, printerId) == 0))
                    {
                        log.Error("In MF Errore di uniformità tra lettura matricola comando Oracle e comando standard: con comando Oracle leggo: " + strDataOracle + "\r\n Con comando standard leggo : " + strData);
                    }
                    //Console.WriteLine("Returned printerId: " + strData);





                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecMessage("RecMessage");
                    for (int i = 0; i < 20; i++)
                    {
                        fiscalprinter.PrintRecItem("TEST ITEM", (decimal)10000, (int)1000, (int)0, (decimal)10000, "");
                    }
                    fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountSurcharge, "SURC.", (decimal)10000, (int)0);
                    fiscalprinter.PrintRecItemAdjustmentVoid(FiscalAdjustment.AmountSurcharge, "VOID SURC.", (decimal)10000, (int)0);
                    //Non se puede questa sequenza in RT!!!
                    fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "DISCOUNT", (decimal)10000, (int)0);
                    fiscalprinter.PrintRecItemAdjustmentVoid(FiscalAdjustment.AmountDiscount, "VOID DISCOUNT", (decimal)10000, (int)0);
                    fiscalprinter.PrintRecItemRefund("Item REFUND", (decimal)10000, (int)1000, (int)0, (decimal)10000, "");
                    fiscalprinter.PrintRecItemRefundVoid("Item REFUND VOID", (decimal)10000, (int)1000, (int)0, (decimal)10000, "");
                    fiscalprinter.PrintRecItem("ITEM", (decimal)10000, (int)1000, (int)2, (decimal)10000, "");
                    fiscalprinter.PrintRecItemVoid("ITEM VOID", (decimal)10000, (int)1000, (int)2, (decimal)10000, "");
                    fiscalprinter.PrintRecRefund("REFUND", (decimal)10000, (int)0);
                    fiscalprinter.PrintRecRefundVoid("REFUND VOID", (decimal)10000, (int)0);
                    fiscalprinter.PrintRecItem("ITEM", (decimal)10000, (int)1000, (int)3, (decimal)10000, "");
                    fiscalprinter.PrintRecMessage("RecMessage");
                    fiscalprinter.PrintRecSubtotal((decimal)200000);
                    fiscalprinter.PrintRecSubtotalAdjustment(FiscalAdjustment.AmountDiscount, "SUBT DISC", (decimal)10000);
                    fiscalprinter.PrintRecSubtotalAdjustVoid(FiscalAdjustment.AmountDiscount, (decimal)10000);
                    fiscalprinter.PrintRecSubtotalAdjustment(FiscalAdjustment.AmountSurcharge, "SUBT SURC", (decimal)10000);
                    fiscalprinter.PrintRecSubtotalAdjustVoid(FiscalAdjustment.AmountSurcharge, (decimal)10000);
                    fiscalprinter.PrintRecTotal((decimal)230000, (decimal)130000, "0CONTANTI");
                    fiscalprinter.PrintRecNotPaid("NOT PAID", (decimal)100000);
                    fiscalprinter.PrintRecMessage("RecMessage");
                    // BARCODE
                    //Console.WriteLine("Printing barcode ");
                    strObj = new string[1];
                    strObj[0] = "010803111200002801234567890";
                    dirIO = posCommonFP.DirectIO(0, 1075, strObj);
                    iData = dirIO.Data;
                    //Console.WriteLine("DirectIO(): iData = " + iData);
                    iObj = (string[])dirIO.Object;
                    //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                    //Console.WriteLine("Performing EndFiscalReceipt() method ");
                    fiscalprinter.EndFiscalReceipt(false);
                }

                // Status request
                //Console.WriteLine("Performing DirectIO() method GET PRINTER STATUS");
                string[] strObj2 = new string[1];
                strObj2[0] = "01";
                DirectIOData dirIO2 = posCommonFP.DirectIO(0, 1074, strObj2);
                iData = dirIO2.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO2.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                /*
                Console.WriteLine("Setting DeviceEnabled property ");
                fiscalprinter.Devig


                Console.WriteLine("Performing Release() method ");
                fiscalprinter.Release();

                Console.WriteLine("Performing Close() method ");
                fiscalprinter.Close();
                */
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Fatal("Not Pos Related Error", e);

                }

                return NumExceptions;
            }
            return NumExceptions;
        }

        public int BeginFiscalReceipt(string flag)
        {
            try
            {
                /*test
                // Console.WriteLine("Initializing PosExplorer ");
                posExplorer = new PosExplorer();

                // Console.WriteLine("Taking FiscalPrinter device ");
                DeviceInfo fp = posExplorer.GetDevice("FiscalPrinter", "FiscalPrinter1");

                // Console.WriteLine("Creating instance of FiscalPrinter device ");
                posCommonFP = (PosCommon)posExplorer.CreateInstance(fp);
                posCommonFP.StatusUpdateEvent += new StatusUpdateEventHandler(co_OnStatusUpdateEvent);
                
                */
                // Console.WriteLine("Initializing FiscalPrinter ");
                //fiscalprinter = (FiscalPrinter)posCommonFP;

                /*
                //Console.WriteLine("Performing Open() method ");
                fiscalprinter.Open();

                //Console.WriteLine("Performing Claim() method ");
                fiscalprinter.Claim(1000);

                //Console.WriteLine("Setting DeviceEnabled property ");
                fiscalprinter.DeviceEnabled = true;
                */

                log.Info("Performing BeginFiscalReceipt() method ");
                //resetPrinter();

                //Console.WriteLine("Performing BeginFiscalReceipt() method ");
                //Console.WriteLine(fiscalprinter.PostLine.ToString());
                fiscalprinter.BeginFiscalReceipt(Convert.ToBoolean(flag));

                /*
                Console.WriteLine("Performing PrintRecItem() method ");
                fiscalprinter.PrintRecItem("ITEM", (decimal)10000, (int)1000, (int)3, (decimal)10000, "");
                Console.WriteLine("Performing PrintRecTotal() method ");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)10000, "0CONTANTI");
                Console.WriteLine("Performing EndFiscalReceipt() method ");
                fiscalprinter.EndFiscalReceipt(false);
                */

            }
            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Error(e.ToString(), e);
                }

                //return NumExceptions;
            }
            return NumExceptions;

        }

        //Metodo che effettua tutte le forme di pagamento disponibili al momento
        public int FormePagamento()
        {
            try
            {
                log.Info("Performing FormePagamento Method");

                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST CONTANTE $£€", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "000CONTANTI");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST CREDITO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)500000, "200CREDITO");

                fiscalprinter.EndFiscalReceipt(false);



                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST ASSEGNO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "100ASSEGNO");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST CARTA DI CREDITO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "201CARTA DI CREDITO");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST ALTRO PAGAMENTO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "203ALTRO PAGAMENTO");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST BANCOMAT", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "204BANCOMAT");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST TICKET", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)100000, (decimal)100000, "301TICKET");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST Non Riscosso Gen", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)100000, (decimal)100000, "500Non Riscosso Gen");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST Non Riscosso Beni", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)100000, (decimal)100000, "501Non Riscosso Beni");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST SCONTO A PAGARE GEN", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)100000, (decimal)100000, "600SCONTO A PAGARE GEN");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST BUONO MULTIUSO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)100000, (decimal)100000, "601BUONO MULTIUSO");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST PAMANENTO MULTIPLI", (decimal)1000000, (int)1000, (int)1, (decimal)10000000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "000CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "200CREDITO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "100ASSEGNO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "201CARTA DI CREDITO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "203ALTRO PAGAMENTO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "204BANCOMAT");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)00000, "301TICKET");

                fiscalprinter.EndFiscalReceipt(false);

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Fatal("", e);

                }

                //return NumExceptions;
            }
            return NumExceptions;
        }


        //Metodo che legge i totalizzatori generali,effettua uno scontrino di vendita e ricarica i totalizzatori generali
        public int PrintRecItem(string description, string price, string quantity, string vatInfo, string unitPrice, ref GeneralCounter gc, ref GeneralCounter gc2)
        {
            try
            {


                log.Info("Performing PrintRecItem() Method");
                //gc = new GeneralCounter();
                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();
                //Questo di scrivere su un file txt è solo temporaneo per vedere cosa succede,poi il test sarà solamente su gc e gc2 in confronto per
                //appurare che i contatori si siano aggiornati correttamente in base alle operazioni effettuate durante il test
                //string lines = "Prima della PrintRecItem " + "\r\n" + gc.ToString() + "\r\n";
                //System.IO.StreamWriter file = new System.IO.StreamWriter("testPrintRecItem.txt", true);
                //file.WriteLine(lines);
                //file.Close();


                //*****************Spostato tutto il test sulla TestPrintRecItem************

                //Aggiorniamo i contatori VAT su xml
                //VatRecord vr = new VatRecord();
                //vr.SetVatCounters();


                //Leggiamo il corrispettivo della IVA di cui andremo a fare una vendita (vatInfo e Item)

                //string vatBefore = VatManager.getVatTableEntry(vatInfo.ToString());
                //string ItemBefore = VatRecord.GetVatCounter("Day", vatBefore, "Item");

                //lines = "Day + VAT 2200 + Item " + VatRecord.GetVatCounter("Day", "2200", "Item") + "\r\n";
                //file = new System.IO.StreamWriter("testPrintRecItem.txt", true);
                //file.WriteLine(lines);
                //file.Close();

                fiscalprinter.BeginFiscalReceipt(true);
                //Console.WriteLine("PrinterState = {0} ", fiscalprinter.PrinterState.ToString());
                if (fiscalprinter.PrinterState.ToString().Equals("FiscalReceipt"))
                {

                    //Eseguo TRE vendite dello stesso oggetto,stesso prezzo unitario e quantità e iva specificata dal parametro vatInfo della funzione , poi testo i totalizzatori relativi prima e dopo la vendita
                    //The price parameter is not used if the unit price is different from 0(the amount is computed from the fiscal printer multiplying the unit price and the quantity).The unitName parameter is not used. Set on the SetupPOS application to print the quantity line, even if it's 1
                    //Console.WriteLine("Performing PrintRecItem() method ");
                    fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatInfo), decimal.Parse(unitPrice), "");
                    fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatInfo), decimal.Parse(unitPrice), "");
                    fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatInfo), decimal.Parse(unitPrice), "");

                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)(Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3), "0CONTANTI");
                    fiscalprinter.PrintRecMessage("ZReport = " + gc.ZRep);
                    fiscalprinter.EndFiscalReceipt(true);



                    /*
                    //PROVA
                    for (int i = 0; i < 20; i++)
                    {
                        fiscalprinter.PrintRecItem("TEST ITEM", (decimal)10000, (int)1000, (int)0, (decimal)10000, "");
                    }
                    fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountSurcharge, "SURC.", (decimal)10000, (int)0);
                    fiscalprinter.PrintRecItemAdjustmentVoid(FiscalAdjustment.AmountSurcharge, "VOID SURC.", (decimal)10000, (int)0);
                    //Vietato fare Sconto dopo reso
                    //fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "DISCOUNT", (decimal)10000, (int)0);
                    //fiscalprinter.PrintRecItemAdjustmentVoid(FiscalAdjustment.AmountDiscount, "VOID DISCOUNT", (decimal)10000, (int)0);
                    fiscalprinter.PrintRecItemRefund("Item REFUND", (decimal)10000, (int)1000, (int)0, (decimal)10000, "");
                    //fiscalprinter.PrintRecItemRefundVoid("Item REFUND VOID", (decimal)10000, (int)1000, (int)0, (decimal)10000, "");
                    fiscalprinter.PrintRecItem("ITEM", (decimal)10000, (int)1000, (int)2, (decimal)10000, "");
                    fiscalprinter.PrintRecItemVoid("ITEM VOID", (decimal)10000, (int)1000, (int)2, (decimal)10000, "");
                    fiscalprinter.PrintRecRefund("REFUND", (decimal)10000, (int)0);
                    fiscalprinter.PrintRecRefundVoid("REFUND VOID", (decimal)10000, (int)0);
                    fiscalprinter.PrintRecItem("ITEM", (decimal)10000, (int)1000, (int)3, (decimal)10000, "");
                    fiscalprinter.PrintRecMessage("RecMessage");
                    fiscalprinter.PrintRecSubtotal((decimal)200000);
                    fiscalprinter.PrintRecSubtotalAdjustment(FiscalAdjustment.AmountDiscount, "SUBT DISC", (decimal)10000);
                    fiscalprinter.PrintRecSubtotalAdjustVoid(FiscalAdjustment.AmountDiscount, (decimal)10000);
                    fiscalprinter.PrintRecSubtotalAdjustment(FiscalAdjustment.AmountSurcharge, "SUBT SURC", (decimal)10000);
                    fiscalprinter.PrintRecSubtotalAdjustVoid(FiscalAdjustment.AmountSurcharge, (decimal)10000);
                    fiscalprinter.PrintRecTotal((decimal)230000, (decimal)220000, "0CONTANTI");
                    //fiscalprinter.PrintRecNotPaid("NOT PAID", (decimal)100000);
                    fiscalprinter.PrintRecMessage("RecMessage");
                    fiscalprinter.EndFiscalReceipt(true);// ritorno in monitor state
                    
                    //fiscalprinter.ResetPrinter();
                    
                    
                    //E'  uno scontrino separato di reso,vediamo se aggiorna il counter

                    // Return document print
                    string[] strObj = new string[1];                    
                    strObj[0] = "0140001REFUND 0693 0001 05082019 9902EY000027";
                    DirectIOData dirIO;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);





                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)10000, (int)1);
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)10000, "0CONTANTI");
                    fiscalprinter.EndFiscalReceipt(true);

                    strObj[0] = "0140001VOID 0693 0001 05082019 9902EY000027";
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);

                    */


                }
                else
                {
                    log.Error("PrintRecItem NOT ALLOWED, YOU ARE NOT IN FISCALRECEIPT STATE");
                    NumExceptions++;
                    return NumExceptions;
                }



                //Aggiorniamo i totalizzatori generali su xml da Memoria Fiscale 
                //gc2 = new GeneralCounter();
                GeneralCounter.SetGeneralCounter();

                //Leggiamo valori nuovi
                gc2 = GeneralCounter.GetGeneralCounter();
                //lines = "Dopo la PrintRecItem " + "\r\n" + gc2.ToString() + "\r\n";
                //file = new System.IO.StreamWriter("testPrintRecItem.txt", true);
                //file.WriteLine(lines);
                //file.Close();


                //Aggiorniamo i contatori VAT su xml
                //VatRecord vr2 = new VatRecord();
                //vr2.SetVatCounters();

                //Leggiamo i corrispettivi dell'iva relativa alle vendite (VatInfo) DOPO la vendita e verifichiamo la coerenza dei totalizzatori con l'entità della vendita effettuata
                //string vatAfter = VatManager.getVatTableEntry(vatInfo.ToString()); //questa obv non cambia
                //string ItemAfter = VatRecord.GetVatCounter("Day", vatAfter, "Item");

                //file = new System.IO.StreamWriter("testPrintRecItem.txt", true);
                //lines = "Day + VAT 2200 + Item " + VatRecord.GetVatCounter("Day", "2200", "Item") + "\r\n"; 
                //file.WriteLine(lines);
                //lines = "Grand + VAT 2200 + Item " + VatRecord.GetVatCounter("Grand", "2200", "Item") + "\r\n";
                //file.WriteLine(lines);
                //file.Close();

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {
                    Console.WriteLine(e.ToString());
                    log.Fatal("", e);

                }

                return NumExceptions;
            }
            return NumExceptions;

        }

        //Eseguo TRE vendite dello stesso oggetto,stesso prezzo unitario e quantità ma reparti diversi (VatIndex 1 2 3) , 
        //poi testo il granTotal sul metodo TestGranTotal prima e dopo le vendite
        public int GranTotal(ref GeneralCounter gc, ref GeneralCounter gc2)
        {
            //Strutture classiche per effettuare la directIO
            DirectIOData dirIO;
            int iData;
            string[] iObj = new string[1];
            try
            {



                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();


                //Console.WriteLine("PrinterState = {0} ", fiscalprinter.PrinterState.ToString());
                if (fiscalprinter.PrinterState.ToString().Equals("FiscalReceipt"))
                {

                    //The price parameter is not used if the unit price is different from 0(the amount is computed from the fiscal printer multiplying the unit price and the quantity).

                    //Console.WriteLine("Performing PrintRecItem() method ");
                    fiscalprinter.PrintRecItem("Nike", decimal.Parse("10000"), Int32.Parse("10000"), 1, decimal.Parse("100000"), "");
                    fiscalprinter.PrintRecItem("Nike", decimal.Parse("10000"), Int32.Parse("10000"), 2, decimal.Parse("100000"), "");
                    fiscalprinter.PrintRecItem("Nike", decimal.Parse("10000"), Int32.Parse("10000"), 3, decimal.Parse("100000"), "");

                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)(Int32.Parse("10000") / 1000 * decimal.Parse("100000") * 3), "301ASSEGNO");
                    fiscalprinter.EndFiscalReceipt(true);
                    //Scontrino chiuso ma devo fare uno ZReport per aggiornare il Gran Total




                }
                else
                {
                    log.Error("PrintRecItem NOT ALLOWED, YOU ARE NOT IN FISCALRECEIPT STATE");
                    NumExceptions++;
                    return NumExceptions;
                }

                //Faccio uno Zreport in modo da aggiornale il gran totale
                string[] strObj = new string[1];
                strObj[0] = "01";

                dirIO = posCommonFP.DirectIO(0, 3001, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                if (iData != 3001)
                {
                    log.Error("Errore directIO 3001 Print Z Report , expected 3001 ," + " Received " + iData);
                    throw new Exception();
                }
                if (iObj[0].Length != 16)
                {
                    log.Error("Errore directIO 3001 lunghezza frame di risposta data ");
                    throw new Exception();
                }




                //Aggiorniamo i totalizzatori generali su xml da Memoria Fiscale 
                //gc2 = new GeneralCounter();
                GeneralCounter.SetGeneralCounter();

                //Leggiamo valori nuovi e li memorizziamo sull'oggetto gc2
                gc2 = GeneralCounter.GetGeneralCounter();


            }
            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {
                    Console.WriteLine(e.ToString());
                    log.Fatal("", e);

                }

                return NumExceptions;
            }
            return NumExceptions;

        }

        //E' il test che fallisce a 64bit se il file RegSettings.xml ha tutto blank nel trailer 99.
        public int TestStrano()
        {
            try
            {

                fiscalprinter.BeginFiscalReceipt(true);
                string description = "TestStrano";
                string price = "10000";
                string quantity = "10000";
                string vatInfo = "1";
                string unitPrice = "100000";

                fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatInfo), decimal.Parse(unitPrice), "");
                fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatInfo), decimal.Parse(unitPrice), "");
                fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatInfo), decimal.Parse(unitPrice), "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)(Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3), "301ASSEGNO");
                fiscalprinter.EndFiscalReceipt(false);
                fiscalprinter.ResetPrinter();
                //EDIT 30/09/19 Nel vecchio driver il test falliva , a meno che non chiudessi la conn.
                //Posso pensare di lasciarlo cosi e accertarmi che il test FALLISCA
                //fiscalprinter.Close();



                if (!opened)
                {
                    fiscalprinter = (FiscalPrinter)posCommonFP;

                    fiscalprinter.Open();

                    fiscalprinter.Claim(1000);

                    fiscalprinter.DeviceEnabled = true;

                    fiscalprinter.ResetPrinter();
                }

                // RT Specific Commands - BEG

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string rtType = "";
                string[] iObj = new string[1];
                /*
                Boolean isRT = false;
                string strData = "";
                string rtType = "";
                string printerId = "";
                string strDate = "";
                string zRepNum = "";
                string recNum = "";
                */
                // Check RT status 
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                rtType = iObj[0].Substring(3, 2);
                //Console.WriteLine("RT type: " + rtType);
                int rtTypeInt = Convert.ToInt32(rtType);
                if (rtTypeInt == 1)
                {

                    log.Info("Printer is NOT RT mode");
                }
                else
                if (rtTypeInt == 2)
                {
                    log.Info("Printer is in RT mode");
                }


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecItem("TSHIRT", (decimal)10000, (int)1000, (int)2, (decimal)12000, "");
                fiscalprinter.PrintRecItem("CAPPELLO", (decimal)10000, (int)1000, (int)3, (decimal)30000, "");
                fiscalprinter.PrintRecItem("BORSA", (decimal)10000, (int)1000, (int)5, (decimal)50000, "");


                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "0CONTANTI");


                fiscalprinter.EndFiscalReceipt(true);



            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                fiscalprinter.ResetPrinter();

                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("" + pce.Message, pce);
                    //throw;
                }
                else
                {
                    log.Error(e.ToString());
                    //log.Fatal("", e);

                }

                return NumExceptions;
            }
            return NumExceptions;
        }

        //Test custom per cliente Massimo
        public int TestMassimo()
        {
            try
            {

                fiscalprinter.BeginFiscalReceipt(true);
                string description = "Pantalone";
                string price = "159900";
                string quantity = "1000";
                string vatInfo = "3";
                string unitPrice = "159900";

                fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatInfo), decimal.Parse(unitPrice), "Unit");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)100, (int)0);
                fiscalprinter.PrintRecItem(description, decimal.Parse("199900"), Int32.Parse(quantity), Int32.Parse(vatInfo), decimal.Parse("199900"), "Unit");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)200, (int)0);

                //fiscalprinter.PrintRecItemAdjustmentVoid(FiscalAdjustment.AmountSurcharge, "VOID SURC.", (decimal)10000, (int)0);

                //fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatInfo), decimal.Parse(unitPrice), "");
                //fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatInfo), decimal.Parse(unitPrice), "");
                fiscalprinter.PrintRecTotal((decimal)359500, (decimal)359500, "001CONTANTE");
                fiscalprinter.EndFiscalReceipt(true);
                fiscalprinter.PrintRecVoid("CANCELRECEIPT");
                fiscalprinter.DirectIO(0, 1088, 2);
                //fiscalprinter.EndFiscalReceipt(false);
                //fiscalprinter.ResetPrinter();



            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                fiscalprinter.ResetPrinter();

                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("" + pce.Message, pce);
                    //throw;
                }
                else
                {
                    log.Error(e.ToString());
                    //log.Fatal("", e);

                }

                return NumExceptions;
            }
            return NumExceptions;
        }

        //Test custom per cliente Matteo
        public int TestMatteo()
        {
            try
            {

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;

                string printerIdModel;
                string printerIdManufacturer;
                string printerIdNumber;

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                printerIdModel = strData.Substring(0, 2);
                printerIdNumber = strData.Substring(4, 6);
                printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;


                //Vendita che provero' ad annullare
                fiscalprinter.BeginFiscalReceipt(true);
                //Console.WriteLine("Performing PrintRecItem() method ");
                fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)10000, "");
                fiscalprinter.PrintRecItem("TSHIRT", (decimal)10000, (int)1000, (int)2, (decimal)1200, "");
                fiscalprinter.PrintRecItem("CAPPELLO", (decimal)10000, (int)1000, (int)3, (decimal)3000, "");
                fiscalprinter.PrintRecItem("BORSA", (decimal)10000, (int)1000, (int)4, (decimal)5000, "");

                //Console.WriteLine("Performing PrintRecTotal() method ");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "0CONTANTI");
                //Console.WriteLine("Performing EndFiscalReceipt() method ");
                fiscalprinter.EndFiscalReceipt(false);




                //Recupero i dati necessari per fare il void o refund

                strData = fiscalprinter.GetDate().ToString();
                //Console.WriteLine("Date: " + strData);
                string strDate = strData.Substring(0, 2) + strData.Substring(3, 2) + strData.Substring(6, 4);

                // Get Z report
                //Console.WriteLine("Get Data (Z Report)");
                string zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                int nextInt = Int32.Parse(zRepNum) + 1;
                zRepNum = nextInt.ToString("0000");
                //Console.WriteLine("Z Report: " + zRepNum);

                // Get rec num
                //Console.WriteLine("Get Data (Fiscal Rec)");
                // recNum = fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data;
                string recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;
 
                //Chiedo se è annullabile
                log.Info("Chiedo se è anche annullabile");
                strObj[0] = "2" + printerId + strDate + recNum + zRepNum;   // "2" = VOID
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                int iRet = Convert.ToInt32(iObj[0].Substring(0, 1));
                if (iRet == 0)
                {
                    log.Info("Document can be Voidable");
                }
                else
                {
                    if (iRet == 2)
                    {
                        log.Error("Document can NOT be Voided quando in realtà dovrebbe esserlo, Data: " + strDate + "Zreport : " + zRepNum + "Scontrino Fiscale: " + recNum);
                    }
                }

                //Chiedo ora se è rendibile
                strObj[0] = "1" + printerId + strDate + recNum + zRepNum;   // "1" = refund
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                iRet = Convert.ToInt32(iObj[0].Substring(0, 1));
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    if (iRet == 2)
                    {
                        log.Error("Document can NOT be Refunded quando in realtà dovrebbe esserlo, Data: " + strDate + " Zreport : " + zRepNum + " Scontrino Fiscale: " + recNum);
                    }
                }
                if (iRet == 0)
                {
                    
                    //Ci faccio un reso di 1 euro
                    // Return document print
                    log.Info("DirectIO (Return document print)");
                    strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                    iData = dirIO.Data;
                    iObj = (string[])dirIO.Object;

                    

                    /* Da qui in poi comincio a replicare il log del cliente dopo la 1078 REFUND
                    1074 01

                    */
                    
                    for (int i = 0; i < 8; i++)
                    {
                        strObj[0] = "01";
                        dirIO = posCommonFP.DirectIO(0, 1074, strObj);
                        iData = dirIO.Data;
                        iObj = (string[])dirIO.Object;
                    }

                    //Leggo il flag 01 , APPRENDIMENTO (CHE IO HO IMPOSTATO SU "NO")
                    strObj[0] = "01";
                    dirIO = posCommonFP.DirectIO(0, 4214, strObj); // Leggo il flag 63
                    iObj = (string[])dirIO.Object;

                    Console.WriteLine("PrinterState = {0} ", fiscalprinter.PrinterState.ToString());
                    /*
                    strObj[0] = "01";
                    dirIO = posCommonFP.DirectIO(0, 1085, strObj);
                    iObj = (string[])dirIO.Object;

                    Console.WriteLine("PrinterState = {0} ", fiscalprinter.PrinterState.ToString());
                    */

                    //La BeginFiscalReceipt corrisponde alla DirectIO 1085 01
                    fiscalprinter.BeginFiscalReceipt(true);
                    Console.WriteLine("PrinterState = {0} ", fiscalprinter.PrinterState.ToString());


                    //La PrintRecRefund corrisponde alla DirectIO 1081 01
                    fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)10000, (int)1);
                    fiscalprinter.PrintRecTotal((decimal)00000, (decimal)00000, "0CONTANTI");
                    fiscalprinter.EndFiscalReceipt(false);

                    //Ritesto ancora questo scontrino, deve essere ancora rendibile 
                    strObj[0] = "1" + printerId + strDate + recNum + zRepNum;   // "1" = refund
                    dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                    iData = dirIO.Data;
                    iObj = (string[])dirIO.Object;
                    iRet = Convert.ToInt32(iObj[0].Substring(0, 1));
                    if (iRet == 0)
                    {
                        log.Info("Document can be Refunded, tutto ok");
                    }
                    else
                    {
                        if (iRet == 2)
                        {
                            log.Error("Document can NOT be Refunded quando in realtà dovrebbe esserlo, Data: " + strDate + "Zreport : " + zRepNum + "Scontrino Fiscale: " + recNum);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                fiscalprinter.ResetPrinter();

                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("" + pce.Message, pce);
                    //throw;
                }
                else
                {
                    log.Error(e.ToString());
                    //log.Fatal("", e);

                }

                return NumExceptions;
            }
            return NumExceptions;
        }


        //PrintRecTotal

        public int PrintRecTotal(string total, string payment, string description)
        {
            try
            {


                // Console.WriteLine("Initializing FiscalPrinter ");
                //fiscalprinter = (FiscalPrinter)posCommonFP;
                /*
                //Console.WriteLine("Performing Open() method ");
                fiscalprinter.Open();

                //Console.WriteLine("Performing Claim() method ");
                fiscalprinter.Claim(1000);

                //Console.WriteLine("Setting DeviceEnabled property ");
                fiscalprinter.DeviceEnabled = true;

                //fiscalprinter.ResetPrinter();
                //fiscalprinter.BeginFiscalReceipt(Convert.ToBoolean(true));
                */
                //Console.WriteLine("PrinterState = {0} ", fiscalprinter.PrinterState.ToString());
                log.Info("Performing PrintRecTotal() method ");
                fiscalprinter.PrintRecTotal(decimal.Parse(total), decimal.Parse(payment), description);
            }
            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    Console.WriteLine(e.ToString());
                    log.Fatal("", e);

                }

                return NumExceptions;
            }
            return 0;

        }

        //Metodo creato per testare i totalizzatori quotidiani che si ottengono con la directIO 2050 (o il metodo del driver Get Daily Data): in particolare verifico la
        //coerenza dei totalizzatori dei RESI e degli ANNULLI :
        /*
        
            Sequenza del suddetto metodo:
        a) Vendita di 1 euro al 4% 4 cent di Iva



        b) Documento multi aliquota che poi verrà reso parzialmente fino ad azzerarlo totalmente che sarà :
        1 euro al 22% 		18 cent IVA
        0.12 euro al 10%	1 cent  IVA
        0.30 euro al 4% 	1 cent  IVA
        0.5 euro Es		0   0 cent IVA

        Tot:1.92		Tot Iva:0.20 euro


        c)Annullo 0.10 euro al 22% 2 cent di Iva

        d)Annullo 0.10 euro al 22% 2 cent di Iva

        e)Annullo 0.80 euro al 22% 14 cent di Iva

        f)Annullo 0.12 euro al 10% 1 cent di Iva

        g)Annullo 0.30 euro al 4% 1 cent di Iva

        h)Annullo di 0.50 euro ES* 0 cent di Iva


        i)Documento multi aliquota che poi verrà reso annullato che sarà :
        1 euro al 22% 		18 cent
        0.12 euro al 10%	1 cent
        0.30 euro al 4% 	1 cent
        0.5 euro Es		0

        Tot:1.92		Tot Iva:0.20 euro

        j)Annullo di quest'ultimo documento di vendita


        */

        //In totale faro' 10 scontrini e un totale di 4.84 euro di vendite (due scontrini da 1.92 , e uno di 1 euro,gli altri sono resi parziali e/o annulli) 
        //Nella funzione che poi andrà a testare questo metodo (la TestPrintRecRefound chiamabile via xml) leggero i totalizzatori , prima e dopo questo metodo e verifichero' che i totalizatori modificati
        //siano coerenti con tutto cio' che ho fatto qui dentro: 
        //EDIT: 10/12/2020 Refactorizzata con le nuove forme di pagamento per Xml 7.0
        public int PrintRecRefound()
        {
            try
            {
                log.Info("Performing PrintRecRefound() method");
                string printerIdModel = "";
                string printerIdManufacturer = "";
                string printerIdNumber = "";
                string strData = "";
                string printerId = "";
                string strDate = "";
                string zRepNum = "";
                string recNum = "";
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                

                // Vendita random,per incrementare il contatore delle vendite: 1 euro aliquota 4%
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Random Object", (decimal)10000, (int)1000, (int)3, (decimal)10000, "");
                //fiscalprinter.PrintRecTotal((decimal)0000, (decimal)00000, "0CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)10000, "201CARTA DI CREDITO");
                fiscalprinter.EndFiscalReceipt(false);
                

                //TODO: 03/02/2020: introduzione scontrino non fiscale nel mezzo del test per simulare potenziali bug
                fiscalprinter.BeginNonFiscal();
                fiscalprinter.PrintNormal(FiscalPrinterStations.Receipt, "Test Doc Non Fiscale £$€");
                fiscalprinter.EndNonFiscal();


                //Scontrino di vendita multi aliquota per poi andare a fare dei resi parziali fino ad annullarlo completamente (annullo dopo reso è vietato, alias crash) 

                fiscalprinter.BeginFiscalReceipt(true);
                //Console.WriteLine("Performing PrintRecItem() method ");
                fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)10000, "");
                fiscalprinter.PrintRecItem("TSHIRT", (decimal)10000, (int)1000, (int)2, (decimal)1200, "");
                fiscalprinter.PrintRecItem("CAPPELLO", (decimal)10000, (int)1000, (int)3, (decimal)3000, "");
                fiscalprinter.PrintRecItem("BORSA", (decimal)10000, (int)1000, (int)4, (decimal)5000, "");

                
                //fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "0CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)19200, (decimal)19200, "201CARTA DI CREDITO");
                fiscalprinter.EndFiscalReceipt(false);


                //TODO: 03/02/2020 inserisco un doc NON FISCALE per vedere se crea "disturbo"
                fiscalprinter.BeginNonFiscal();
                fiscalprinter.PrintNormal(FiscalPrinterStations.Receipt, "Test Doc Non Fiscale");
                fiscalprinter.EndNonFiscal();



                //Reperimento dati che mi servono per verificare che l'ultimo scontrino sia refundable 
                // Get printer ID
                //Console.WriteLine("Get Data (Printer ID)");

                strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)-35000).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                printerIdModel = strData.Substring(0, 2);
                printerIdNumber = strData.Substring(4, 6);
                printerIdManufacturer = strData.Substring(2, 2);
                //printerId = printerIdManufacturer + rtType + printerIdModel + printerIdNumber;
                //Console.WriteLine("Printer Id: " + printerId);
                printerId = strData;

                // Get date
                //Console.WriteLine("Performing GetDate() method ");
                strData = fiscalprinter.GetDate().ToString();
                //Console.WriteLine("Date: " + strData);
                strDate = strData.Substring(0, 2) + strData.Substring(3, 2) + strData.Substring(6, 4);
                //Console.WriteLine("Date: " + strDate);

                // Get Z report
                //Console.WriteLine("Get Data (Z Report)");
                zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                int nextInt = Int32.Parse(zRepNum) + 1;
                zRepNum = nextInt.ToString("0000");
                //Console.WriteLine("Z Report: " + zRepNum);

                // Get rec num
                //Console.WriteLine("Get Data (Fiscal Rec)");
                // recNum = fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data;
                recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;
                //Console.WriteLine("Rec Num: " + recNum);

                // Check document is returnable
                //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                strObj[0] = "1" + printerId + strDate + recNum + zRepNum;   // "1" = return
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet = Int32.Parse(iObj[0].Substring(0, 1));
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    log.Error("Document can NOT be Refunded quando invece dovrebbe");
                    //throw new Exception();
                }

                if (iData != 9205)
                {
                    log.Error("Error DirectIO 9205 , expected iData 9205, received " + iData);
                    //throw new Exception();
                }
                // Return document print
                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)1000, (int)1);
                fiscalprinter.PrintRecTotal((decimal)19200, (decimal)1000, "201CARTA DI CREDITO");
                fiscalprinter.EndFiscalReceipt(false);


                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;


                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)1000, (int)1);
                fiscalprinter.PrintRecTotal((decimal)19200, (decimal)1000, "201CARTA DI CREDITO  ");
                fiscalprinter.EndFiscalReceipt(false);

                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new PosControlException();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new PosControlException();
                }
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)8000, (int)1);
                fiscalprinter.PrintRecTotal((decimal)80000, (decimal)8000, "201CARTA DI CREDITO ");
                fiscalprinter.EndFiscalReceipt(false);

                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }




                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)1200, (int)2);
                fiscalprinter.PrintRecTotal((decimal)19200, (decimal)1200, "201CARTA DI CREDITO  ");
                fiscalprinter.EndFiscalReceipt(false);

                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
        
                iObj = (string[])dirIO.Object;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)3000, (int)3);
                fiscalprinter.PrintRecTotal((decimal)19200, (decimal)3000, "201CARTA DI CREDITO  ");
                fiscalprinter.EndFiscalReceipt(false);

                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)5000, (int)4);
                fiscalprinter.PrintRecTotal((decimal)19200, (decimal)5000, "201CARTA DI CREDITO  ");
                fiscalprinter.EndFiscalReceipt(false);



                //TODO: 03/02/2020 inserisco un doc NON FISCALE per vedere se crea "disturbo"
                fiscalprinter.BeginNonFiscal();
                fiscalprinter.PrintNormal(FiscalPrinterStations.Receipt, "Test Doc Non Fiscale");
                fiscalprinter.EndNonFiscal();

                //Scontrino vendita multi aliquota che verrà successivamente annullato (previo test ovviamente)

                fiscalprinter.BeginFiscalReceipt(true);
                //Console.WriteLine("Performing PrintRecItem() method ");
                fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)10000, "");
                fiscalprinter.PrintRecItem("TSHIRT", (decimal)10000, (int)1000, (int)2, (decimal)1200, "");
                fiscalprinter.PrintRecItem("CAPPELLO", (decimal)10000, (int)1000, (int)3, (decimal)3000, "");
                fiscalprinter.PrintRecItem("BORSA", (decimal)10000, (int)1000, (int)4, (decimal)5000, "");

               
                fiscalprinter.PrintRecTotal((decimal)19200, (decimal)19200, "201CARTA DI CREDITO  ");
                fiscalprinter.EndFiscalReceipt(false);




                //TODO: 03/02/2020 inserisco un doc NON FISCALE per vedere se crea "disturbo"

                fiscalprinter.BeginNonFiscal();
                fiscalprinter.PrintNormal(FiscalPrinterStations.Receipt, "Test Doc Non Fiscale");
                fiscalprinter.EndNonFiscal();


                // Get date
                //Console.WriteLine("Performing GetDate() method ");
                strData = fiscalprinter.GetDate().ToString();
                //Console.WriteLine("Date: " + strData);
                strDate = strData.Substring(0, 2) + strData.Substring(3, 2) + strData.Substring(6, 4);

                // Get Z report
                //Console.WriteLine("Get Data (Z Report)");
                zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                nextInt = Int32.Parse(zRepNum) + 1;
                zRepNum = nextInt.ToString("0000");
                //Console.WriteLine("Z Report: " + zRepNum);

                // Get rec num
                //Console.WriteLine("Get Data (Fiscal Rec)");
                // recNum = fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data;
                recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;


                strObj[0] = "2" + printerId + strDate + recNum + zRepNum;   // "2" = void
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                int iRet2 = Int32.Parse(iObj[0].Substring(0, 1));

                if (iRet2 == 0)
                {
                    log.Info("Document voidable");
                    // Annullo lo scontrino e verifico che il comando risponda in maniera coerente col protocollo
                    //Console.WriteLine("DirectIO (Void document print)");
                    strObj[0] = "0140011VOID " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);

                    iData = dirIO.Data;
                    iObj = (string[])dirIO.Object;

                    if (iData != 1078)
                    {
                        log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                        throw new Exception();
                    }
                    iObj = (string[])dirIO.Object;
                    if (!(String.Equals(iObj[0], "51")))
                    {
                        log.Fatal("Error DirectIO 1078 operator, expected 51, received " + iObj[0]);
                        throw new Exception();
                    }

                }
                else
                {
                    log.Error("Document NOT voidable quando dovrebbe essere annullabile"); //E' un errore perchè dovrebbe annullarlo 
                }
                //Provo a riannullarlo per testare che non faccia cose strane (tipo annullarlo ancora o annullare altri scontrini


                strObj[0] = "2" + printerId + strDate + recNum + zRepNum;   // "2" = void
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                iRet2 = Int32.Parse(iObj[0].Substring(0, 1));
                if (iRet2 != 4)
                {
                    log.Error("Mi autorizza ad annullare un documento già annullato precedentemente");
                    strObj[0] = "0140001VOID " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);

                    iData = dirIO.Data;

                    iObj = (string[])dirIO.Object;
                    log.Error("Errore DirectIO 1078, ha permesso di annullare un documento già annullato con campo iData " + iData);
                    log.Error("Errore DirectIO 1078, ha permesso di annullare un documento già annullato con campo operator " + iObj[0]);
                    NumExceptions++;

                }
                else
                {
                    log.Info("Document NOT voidable ");
                }




            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Error("Generic Error: ", e);

                }

                return NumExceptions;
            }
            return NumExceptions;


        }

        //E' un test che quando lo chiamo in RT o Demo e DEVE FALLIRE, in MF no 
        public int AdjustmentSequence()
        {
            //TODO : 20/01/2020 non mi piace questo azzeramento, da rivedere in caso
            //NumExceptions = 0;

            try
            {
                log.Info("Performing AdjustmentSequence() method");

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("RecMessage");
                for (int i = 1; i < 20; i++)
                {
                    fiscalprinter.PrintRecItem("TEST ITEM" + i.ToString(), (decimal)10000, (int)1000, (int)i, (decimal)1000, "");
                }

                //Non se puede questa sequenza in RT!!!
                //fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "DISCOUNT", (decimal)10000, (int)0);
                //fiscalprinter.PrintRecItemAdjustmentVoid(FiscalAdjustment.AmountDiscount, "VOID DISCOUNT", (decimal)10000, (int)0);
                fiscalprinter.PrintRecItemRefund("Item REFUND", (decimal)10000, (int)1000, (int)0, (decimal)10000, "");
                fiscalprinter.PrintRecItemRefundVoid("Item REFUND VOID", (decimal)10000, (int)1000, (int)0, (decimal)10000, "");
                fiscalprinter.PrintRecItem("ITEM", (decimal)10000, (int)1000, (int)2, (decimal)10000, "");
                fiscalprinter.PrintRecItemVoid("ITEM VOID", (decimal)10000, (int)1000, (int)2, (decimal)10000, "");
                fiscalprinter.PrintRecRefund("REFUND", (decimal)10000, (int)0);
                fiscalprinter.PrintRecRefundVoid("REFUND VOID", (decimal)10000, (int)0);
                fiscalprinter.PrintRecItem("ITEM", (decimal)10000, (int)1000, (int)3, (decimal)10000, "");
                fiscalprinter.PrintRecMessage("RecMessage");
                fiscalprinter.PrintRecSubtotal((decimal)200000);
                fiscalprinter.PrintRecSubtotalAdjustment(FiscalAdjustment.AmountDiscount, "SUBT DISC", (decimal)10000);
                fiscalprinter.PrintRecSubtotalAdjustVoid(FiscalAdjustment.AmountDiscount, (decimal)10000);
                //fiscalprinter.PrintRecSubtotalAdjustment(FiscalAdjustment.AmountSurcharge, "SUBT SURC", (decimal)10000);
                //fiscalprinter.PrintRecSubtotalAdjustVoid(FiscalAdjustment.AmountSurcharge, (decimal)10000);
                fiscalprinter.PrintRecTotal((decimal)230000, (decimal)130000, "0CONTANTI");
                fiscalprinter.PrintRecNotPaid("NOT PAID", (decimal)100000);
                fiscalprinter.PrintRecMessage("RecMessage");
                fiscalprinter.EndFiscalReceipt(false);
            }

            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Error("", pce);
                    //throw;
                }
                else
                {
                    Console.WriteLine(e.ToString());
                    log.Error("Error not POS Related", e);

                }
                return NumExceptions;
            }

            return 0;
        }

        /*
        //13072020 Test introdotto tardi per un cliente che segnala una anomali sugli storni
        1) Vendita su 2 reparti di 10 cent + storno da 9 cent
        2) Vendita di 10 cent
        3) Prova ad annullare il primo e dice IMP ORA
        4) Fa chiusura e riprova ad annullare il primo e glielo fa
        Proviamo a replicare e poi a reiterare su una sequenza lunga non + 2 ma 10 scontrini

        */
        public int TestStorni()
        {

            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                Boolean isRT = false;
                string strData = "";
                string rtType = "";
                string printerId = "";
                string strDate = "";
                string zRepNum = "";
                string recNum = "";


                string printerIdModel;
                string printerIdManufacturer;
                string printerIdNumber;


                
                //1) Genero lo scontrino con Storno
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("Primo Scontrino con Storno");
                for (int i = 1; i < 3; i++)
                {
                    fiscalprinter.PrintRecItem("TEST ITEM" + i.ToString(), (decimal)10000, (int)1000, (int)i, (decimal)1000, "");
                }
                fiscalprinter.PrintRecItemVoid("ITEM VOID", (decimal)10000, (int)1000, (int)2, (decimal)900, "");

                fiscalprinter.PrintRecTotal((decimal)230000, (decimal)130000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);

                //2) Genero lo scontrino standard
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("Secondo Scontrino Standard");
                
                fiscalprinter.PrintRecItem("TEST ITEM" , (decimal)10000, (int)1000, (int)0, (decimal)1000, "");

                fiscalprinter.PrintRecTotal((decimal)230000, (decimal)130000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);
                

                //3) Provo ad annullare l'ultimo


                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                rtType = iObj[0].Substring(2, 1);



                //string strDataOracle = fiscalprinter.GetData(FiscalData.PrinterId, (int)-35000).Data;
                strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                printerIdModel = strData.Substring(0, 2);
                printerIdNumber = strData.Substring(4, 6);
                printerIdManufacturer = strData.Substring(2, 2);
                printerId = printerIdManufacturer + rtType + printerIdModel + printerIdNumber;



                strData = fiscalprinter.GetDate().ToString();
                strDate = strData.Substring(0, 2) + strData.Substring(3, 2) + strData.Substring(6, 4);

                //strDate = "10072020";

                zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                int nextInt = Int32.Parse(zRepNum) + 1;
                zRepNum = nextInt.ToString("0000");

                //zRepNum = "0243";

                // Get rec num
                recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;

                //recNum = "0002";
                // Check document is voidable
                //Console.WriteLine("DirectIO (Check document is voidable)");

                strObj[0] = "2" + printerId + strDate + recNum + zRepNum;   // "2" = void
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet2 = Int32.Parse(iObj[0]);
                if (iRet2 == 0)
                    log.Info("Document voidable");
                else
                    log.Error("Document NOT voidable");


                /*
                // Void document print
                //Console.WriteLine("DirectIO (Void document print)");
                strObj[0] = "0140001VOID " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;



                */

                

                //4) Provo ad annullare il penultimo

                int last2 = Int32.Parse(recNum) - 1;
                recNum = last2.ToString("0000");

                // Check document is voidable
                //Console.WriteLine("DirectIO (Check document is voidable)");
                strObj[0] = "2" + printerId + strDate + recNum + zRepNum;   // "2" = void
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                iRet2 = Int32.Parse(iObj[0]);
                if (iRet2 == 0)
                    log.Info("Document voidable");
                else
                    log.Error("Document NOT voidable");



                //5) Faccio chiusura e provo ad annullare il primo e il secondo

                fiscalprinter.PrintZReport();

                //6) Ripeto step 3


                
              


                zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;



                // recNum ora è = al penultimo quindi devo fare + 1 per annullare l'ultimo
                recNum = (Convert.ToInt32(recNum) + 1).ToString("0000");


                // Check document is voidable
                //Console.WriteLine("DirectIO (Check document is voidable)");
                strObj[0] = "2" + printerId + strDate + recNum + zRepNum;   // "2" = void
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                iRet2 = Int32.Parse(iObj[0]);
                if (iRet2 == 0)
                    log.Info("Document voidable");
                else
                    log.Error("Document NOT voidable");




                



                //7) Ripeto step 4
                last2 = Int32.Parse(recNum) - 1;
                recNum = last2.ToString("0000");

                // Check document is voidable
                //Console.WriteLine("DirectIO (Check document is voidable)");
                strObj[0] = "2" + printerId + strDate + recNum + zRepNum;   // "2" = void
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                iRet2 = Int32.Parse(iObj[0]);
                if (iRet2 == 0)
                    log.Info("Document voidable");
                else
                    log.Error("Document NOT voidable");




            }

            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Error("", pce);
                    //throw;
                }
                else
                {
                    Console.WriteLine(e.ToString());
                    log.Error("Error not POS Related", e);

                }
                return NumExceptions;
            }

            return 0;
        }




        //Metodo di test sulla PrintRecPackageAdjustment() e PrintRecPackageAdjustmentVoid()
        public int PackageAdjustment()
        {

            VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)123456) };

            //mi serve per stampare le aliquote iva relativa ad ogni reparto per tutti i reparti
            /*
            fiscalprinter.BeginFiscalReceipt(true);
            for (int i = 0; i < 100; ++i)
            {
                fiscalprinter.PrintRecItem("TEST ITEM", (decimal)10000, (int)1000, (int)i, (decimal)10000, "");

            }
            fiscalprinter.PrintRecTotal((decimal)1000000, (decimal)1000000, "0CONTANTI");
            fiscalprinter.EndFiscalReceipt(false);
            */

            try
            {
                fiscalprinter.BeginFiscalReceipt(true);

                //fiscalprinter.PrintRecItem("VENDITA ITEM REP " + vat[0].Id, (decimal)900000, (int)1000, (int)vat[0].Id, (decimal)900000, "");

                //cambio reparto a cui applicare sconti o maggiorazioni
                vat = new VatInfo[1] { new VatInfo(1, (decimal)12000) };

                fiscalprinter.PrintRecItem("VENDITA ITEM REP " + vat[0].Id, (decimal)50000, (int)1000, (int)vat[0].Id, (decimal)50000, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountSurcharge, "SURC.", (decimal)40000, (int)vat[0].Id);
                fiscalprinter.PrintRecItemAdjustmentVoid(FiscalAdjustment.AmountSurcharge, "VOID SURC.", (decimal)40000, (int)vat[0].Id);

                fiscalprinter.PrintRecPackageAdjustment(FiscalAdjustmentType.Surcharge, "Rep. " + vat[0].Id + " Surcharge", vat);
                fiscalprinter.PrintRecPackageAdjustVoid(FiscalAdjustmentType.Surcharge, vat);


                fiscalprinter.PrintRecItem("VENDITA ITEM REP " + vat[0].Id, (decimal)80000, (int)1000, (int)vat[0].Id, (decimal)80000, "");
                fiscalprinter.PrintRecItem("VENDITA ITEM REP " + vat[0].Id, (decimal)20000, (int)1000, (int)vat[0].Id, (decimal)20000, "");

                //PrintRecSubtotal è indifferente il campo che ci inserisco se è MAGGIORE dell'importo preciso (105 euro) ,vedere che succede se inserisco un importo inferiore
                fiscalprinter.PrintRecSubtotal(1);
                fiscalprinter.PrintRecPackageAdjustment(FiscalAdjustmentType.Discount, "Rep. " + vat[0].Id + " Discount", vat);
                fiscalprinter.PrintRecPackageAdjustVoid(FiscalAdjustmentType.Discount, vat);

                //cambio reparto a cui applicare sconti o maggiorazioni
                vat = new VatInfo[1] { new VatInfo(7, (decimal)272000) };

                fiscalprinter.PrintRecPackageAdjustment(FiscalAdjustmentType.Discount, "Rep." + vat[0].Id + "Discount", vat);

                for (int i = 0; i < 10; ++i)
                {
                    fiscalprinter.PrintRecItem("VENDITA ITEM " + i, (decimal)10000, (int)1000, (int)vat[0].Id, (decimal)10000, "");

                }

                fiscalprinter.PrintRecPackageAdjustVoid(FiscalAdjustmentType.Discount, vat);
                fiscalprinter.PrintRecMessage("Annullo ");

                
                // routine che genera suono di errore
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                strObj[0] = "0101010";
                string freq = "0100";
                string doinf = "0261";
                string re = "0293";
                string mi = "0330";
                string fa = "0349";
                string sol = "0392";
                string la = "0440";
                string si = "0494";
                string dosup = "0522";

                //int fr = Int32.Parse(freq) + (i * Int32.Parse(freq));
                //freq = fr.ToString().PadLeft(4, '0'); 
                //strObj[0] =  "0101010" + freq;
                strObj[0] = "0101010" + doinf;
                dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                System.Threading.Thread.Sleep(300);

                strObj[0] = "0101010" + re;
                dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                System.Threading.Thread.Sleep(300);

                strObj[0] = "0101010" + mi;
                dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                System.Threading.Thread.Sleep(300);

                strObj[0] = "0101010" + fa;
                dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                System.Threading.Thread.Sleep(300);

                strObj[0] = "0101010" + sol;
                dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                System.Threading.Thread.Sleep(300);

                strObj[0] = "0101010" + la;
                dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                System.Threading.Thread.Sleep(300);

                strObj[0] = "0101010" + si;
                dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                System.Threading.Thread.Sleep(300);

                strObj[0] = "0101010" + dosup;
                dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                System.Threading.Thread.Sleep(300);

      
                

                fiscalprinter.PrintRecPackageAdjustment(FiscalAdjustmentType.Surcharge, "Rep." + vat[0].Id + "Surcharge", vat);
                fiscalprinter.PrintRecPackageAdjustVoid(FiscalAdjustmentType.Surcharge, vat);
                fiscalprinter.PrintRecSubtotal(400000);
                fiscalprinter.PrintRecItem("VENDITA ITEM ", (decimal)10000, (int)1000, (int)vat[0].Id, (decimal)10000, "");
                fiscalprinter.PrintRecSubtotal(400000);

                fiscalprinter.PrintRecTotal((decimal)20000, (decimal)2500000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);

            }

            catch (Exception e)
            {
                log.Info("----- EXCEPTION -----");
                NumExceptions++;
                //reset introdotta per chiudere lo scontrino e non lasciare il pos in uno stato inconsistente
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Error("", pce);
                    //throw;
                }
                else
                {
                    log.Fatal("", e);

                }

                return NumExceptions;
            }

            return 0;
        }


        //resetPrinter
        public int resetPrinter()
        {
            try
            {
                //La reset deve solo resettare il POS e portarlo in monitor state ,non deve chiedere una nuova instanza
                /*
                //Console.WriteLine("Initializing FiscalPrinter ");
                fiscalprinter = (FiscalPrinter)posCommonFP;

                //Console.WriteLine("Performing Open() method ");
                fiscalprinter.Open();

                //Console.WriteLine("Performing Claim() method ");
                fiscalprinter.Claim(1000);

                //Console.WriteLine("Setting DeviceEnabled property ");
                fiscalprinter.DeviceEnabled = true;
                */

                log.Info("Performing ResetPrinter() method ");
                //TODO 06/02/20 Decidere se rimettere la checkRTStatus o meno e ora provo ad inserire
                //la initFiscalDevice, forse questa mi gestisce gli hard reset della stampante e riparte
                //checkRtStatus();
                initFiscalDevice("FiscalPrinter");
                fiscalprinter.ResetPrinter();

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal(pce.Message);
                    //throw;
                }
                else
                {
                    log.Fatal(e.ToString());

                }

                return NumExceptions;
            }
            return NumExceptions;

        }



        //all'interno c'è pure temporaneamente il check sull EJ da eliminare oppure da isolare con un metodo apposta
        //TODO: il check sull EJ l'ho eliminato e creato un metodo ad hoc ;-)
        public int checkRtStatus()
        {

            try
            {
                log.Info("Performing checkRtStatus() method ");


                // RT Specific Commands - BEG

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                Boolean isRT = false;
                //string strData = "";
                string rtType = "";
                //string printerId = "";
                string strDate = "";
                //string zRepNum = "";
                //string recNum = "";

                /*
                Console.WriteLine("Performing readFromEJByNumber() method ");
                //string[] strObj = new string[1];
                //DirectIOData dirIO;
                //int iData;
                //string[] iObj = new string[1];
                //Boolean isRT = false;
                //string strData = "";
                string date = "140718";
                string FRN = "";
                //string strDate = "";
                string LN = "";
                string TEXT = "";

                //Check EJ Status 
                strObj[0] = "01";
                Console.WriteLine("DirectIO 1077 E/J Status");
                dirIO = posCommonFP.DirectIO(0, 1077, strObj);
                iData = dirIO.Data;
                Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                Console.WriteLine("DirectIO() : iObj = " + iObj[0]);


                //Print from EJ By Date
                Console.WriteLine("Print from EJ By NUMBER");
                strObj[0] = "01" + "010119" + "0001" + "9999";

                dirIO = posCommonFP.DirectIO(0, 3098, strObj);
                iData = dirIO.Data;
                Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                Console.WriteLine("DirectIO() : iObj = " + iObj[0]);



                // READ FROM EJ BY NUMBER 
                Console.WriteLine("DirectIO (READ FROM EJ BY DATE)");
                strObj[0] = "01" + "1" + "0" + "010118" + "150719" + "0" + "00";

                dirIO = posCommonFP.DirectIO(0, 3103, strObj);
                iData = dirIO.Data;
                Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                date = iObj[0].Substring(2, 6);
                Console.WriteLine("date: " + date);

                FRN = iObj[0].Substring(8, 4);
                Console.WriteLine("Current Fiscal Receipt Number: " + FRN);

                LN = iObj[0].Substring(12, 4);
                Console.WriteLine("Line Sequence Number: " + LN);

                TEXT = iObj[0].Substring(16, 46);
                Console.WriteLine("EJ line text: " + TEXT);

                */

                // Check RT status 
                //Console.WriteLine("DirectIO (RT status)");
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                iData = dirIO.Data;
                if (iData != 1138)
                {
                    log.Error("Error DirectIO 1138 campo iData, expected 1138, received " + iData);
                    throw new PosControlException();
                }
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                //Check if  Printer is in RT o non RT mode ( check quarto e quinto byte 1138 command ,secondo protocollo
                rtType = iObj[0].Substring(3, 2);
                //Sub Status della stampante
                string subType = iObj[0].Substring(5, 2);
                //Console.WriteLine("RT type: " + rtType);
                int rtTypeInt = Convert.ToInt32(rtType);
                if (rtTypeInt == 1)
                {
                    //La prima volta ce lo assegno
                    if (String.IsNullOrEmpty(Mode))
                    {
                        Mode = "MF";
                    }
                    else
                    {
                        if (String.Compare(Mode, "MF") != 0)
                        {
                            log.Error("Per qualche motivo il Pos è andato in MF da solo quando il suo stato dovrebbe essere " + Mode + " , probabile crash!!!. " + "Il suo substatus è: " + subType);
                        }
                    }
                    isRT = false;

                    //EDIT: 09/01/2020: Poichè l'MF non è + utilizzato se ci va qui ora sarà un errore
                    //log.Error("Pay Attention, Printer is NOT in RT mode but in MF Mode!!!");
                }
                else
                {
                    if (rtTypeInt == 2)
                    {
                        //La prima volta ce lo assegno
                        if (String.IsNullOrEmpty(Mode))
                        {
                            Mode = "RT";
                        }
                        else
                        {
                            if ((String.Compare(Mode, "RT") != 0) && (String.Compare(Mode, "DemoRT") != 0))
                            {
                                log.Error("Per qualche motivo il Pos è andato in RT da solo quando il suo stato dovrebbe essere " + Mode + " , probabile crash!!!. " + "Il suo substatus è: " + subType);
                            }
                        }
                        isRT = true;
                        log.Info("Printer is in RT mode");
                    }
                    else
                    {
                        Mode = "Undefined";
                        log.Error("Error DirectIO 1138 campo MAIN, expected 01(MF) or 02(RT), received " + rtTypeInt + " mentre il suo substatus è : " + subType);
                        throw new PosControlException();
                    }
                }
                //Check campo DEMO_MODE
                string DemoMode = iObj[0].Substring(38, 1);

                //leggo il numero di files respinti dall ADE
                string rejfiles = iObj[0].Substring(17, 4);

                //Leggo il flag 63 , che mi indica se sono in DEMO Mode o meno. Devono essere coerenti.
                strObj[0] = "63";
                dirIO = posCommonFP.DirectIO(0, 4214, strObj); // Leggo il flag 63
                iObj = (string[])dirIO.Object;
                string status = iObj[0].Substring(2, 1);
                if (String.Compare(status, DemoMode) == 0) //Informazioni coerenti ma cosi' non so se sono in demo o NOT demo
                {
                    //tutto ok
                }
                else
                {
                    log.Error("Error DirectIO 1138 campo DEMO_MODE");
                    if (Int32.Parse(status) == 1) //sono quindi in demo mode e la 1138 dice che non è abilitato
                    {
                        log.Error("expected 1 (Demo Mode Enabled) , received " + DemoMode);
                        //return 1;
                        throw new PosControlException();

                    }
                    else
                    {
                        log.Error("expected 0 (Demo Mode Disabled) , received " + DemoMode);
                        //return 1;
                        throw new PosControlException();

                    }
                }



                //TODO 3/2/2020 devo decidere cosa fare di questa parte di codice, al momento non mi piace che stia
                //qui dentro e per ora la disattivo temporaneamente



                //Test documenti rifiutati ADE
                //sposto la data indietro ma nello stessa giornata
                //Secondo protocollo si puo fare solo se Daily Opened è falso ,ergo bisogna fare uno Zreport prima
                //Console.WriteLine("Performing PrintZReport() method ");
                fiscalprinter.PrintZReport();


                strObj[0] = DateTime.Today.ToString("ddMMyy") + "0600"; //alle 6 del mattino di sicuro io non ci sono , ergo di sicuro è indietro rispetto al time del test

                dirIO = posCommonFP.DirectIO(0, 4001, strObj); // tarocco la data indietro e non dovrebbe farlo, falso l'orario indietro puo farlo in stessa giornata
                iData = dirIO.Data;
                if (iData != 4001)
                {
                    //TODO:rivedere sta parte di codice che non mi convince, mi sembra errata. se mi aspetto che non funzioni perchè raiso l'eccezione ?
                    // 10/04/2020 certo che lo puo fare perchè sto solo spostando l'orario indietro e non la data, ergo se non funziona errore grave del firmware
                    log.Error("Error DirectIO 4001 , tentativo fallito di settare la data indietro di qualche ora");
                    //NumExceptions++;
                    //throw new PosControlException();
                }

                strObj[0] = DateTime.Now.ToString("ddMMyyHHmm");
                Int64 temporary = Int64.Parse(strObj[0]);
                temporary += 300; //sposto la data di 3 ore in avanti
                strObj[0] = temporary.ToString().PadLeft(10, '0'); ;
                dirIO = posCommonFP.DirectIO(0, 4001, strObj); // tarocco la data in avanti ma solo di di 3 ore in modo da poter ripristinare
                iData = dirIO.Data;
                if (iData != 4001)
                {
                    //TODO: rivedere anche qui , non so perchè ma non funziona +
                    log.Error("Error DirectIO 4001 campo iData, expected 4001, received " + iData);
                    //throw new PosControlException();
                }

                iObj = (string[])dirIO.Object;

                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 3001, strObj);

                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 3001, strObj);


                //System.Threading.Thread.Sleep(120000);


                //Inserito comando ridondante perchè ALCUNE volte il POS si auto riavvia...
                //Check RT Status
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1138, strObj);

                //Check RT Status
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1138, strObj);


                //Check RT Status
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                iObj = (string[])dirIO.Object;
                //leggo di nuovo il numero di files respinti dall ADE, dovrebbero essere + 2
                string rejfiles2 = iObj[0].Substring(17, 4);

                //contatore file rejected 
                int expected = Int32.Parse(rejfiles) + 2;
                if (Int32.Parse(rejfiles2) != expected)
                {
                    //EDIT: 13/01/2020 ricordarsi di rimetterlo
                    log.Error("Error DirectIO 1138 campo REJ FILES " +  " expected " + expected + " readed " + Int32.Parse(rejfiles2));
                    //Console.WriteLine("Error DirectIO 1138 campo REJ FILES " + " expected " + expected + " readed " + Int32.Parse(rejfiles2));
                    NumExceptions++;
                    //throw new PosControlException();
                }

                //Ripristino la data corretta
                strObj[0] = DateTime.Now.ToString("ddMMyyHHmm");
                dirIO = posCommonFP.DirectIO(0, 4001, strObj);
                iData = dirIO.Data;
                if (iData != 4001)
                {
                    log.Error("Error DirectIO 4001 , errore nel ripristino data corretta");
                    throw new PosControlException();
                }

            }
            catch (Exception e)
            {
                NumExceptions++;
                //resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);
                    //throw;

                    //TODO TEST 27/02: PROVIAMO A RISTARTARE IL POS SE CRASHA E VA IN TIMEOUT
                    //Se il comando va in timeout voglio che il driver si riprenda , crei un nuovo
                    //oggetto fiscalprinter e continui a lavorare.
                    //Per fare questo devo chiudere la connessione corrente, mettere il flag opened a false e
                    //richiamare la initFiscalDevice ossia creare il nuovo oggetto
                    if (pce.Message == "Stub message. Timeout")
                    {
                        opened = false;
                        fiscalprinter.Close();
                        initFiscalDevice("FiscalPrinter");
                    }
                }
                else
                {

                    log.Fatal("", e);

                }

                //return NumExceptions;
            }
            return NumExceptions;

        }


        //Xreport : semplice chiurusa giornaliera, mi serve chiamarlo singolarmente sometimes
        public int XReport()
        {
            try
            {
                log.Info("Performing XReport");

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;


                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 2001, strObj);
                int iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                if (iData != 2001)
                {
                    log.Error("Errore, XReport non avvenuto, expected risposta: 2001,  received: " + iData);
                }
            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Fatal("", e);
                }

            }
            return NumExceptions;
        }


        //Xreport : semplice chiurusa giornaliera, mi serve chiamarlo singolarmente sometimes
        public int XZReport()
        {
            try
            {
                log.Info("Performing XZReport");

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;


                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 3002, strObj);
                int iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                if (iData != 3002)
                {
                    log.Error("Errore, XZReport non avvenuto, expected risposta: 3002,  received: " + iData);
                }
            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Fatal("", e);
                }

            }
            return NumExceptions;
        }




        //Zreport : semplice chiurusa giornaliera, mi serve chiamarlo singolarmente sometimes
        public int ZReport()
        {
            try
            {
                log.Info("Performing ZReport");

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;


                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 3001, strObj);
                int iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                if (iData != 3001)
                {
                    log.Error("Errore, PrintZReport non avvenuto, expected risposta: 3001,  received: " + iData);
                }

                Thread.Sleep(3000);
               
            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Fatal("", e);
                }

            }
            return NumExceptions;
        }

        //DirectIOData 1061
        //PrintTaxCode : stampa il codice fiscale alla fine dello scontrino
        public int PrintTaxCode(string taxcode)
        {
            try
            {
                log.Info("Performing PrintTaxCode");

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;


                //print Z Report
                strObj[0] = "01" + taxcode.PadLeft(16, 'X'); ;
                dirIO = posCommonFP.DirectIO(0, 1061, strObj);
                int iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                if (iData != 1061)
                {
                    log.Error("Errore, Performing PrintTaxCode risposta: 1061,  received: " + iData);
                }
                
            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Fatal("", e);
                }

            }
            return NumExceptions;
        }


        //AutoZreport :  chiurusa giornaliera automatica (DirectIO 9013)
        //Input: ENA Attivazione (1 byte) 0 = Disabled 1 = Enabled in all cases 2 = Enabled if PERIODO INATTIVO condition is True
        //Input: FINREP Print X-01 financial report (1 byte) 1 = No 2 = Yes
        //Input: HH Ora 2 bytes
        //Input: MM Minuti 2 Bytes
        //Output: ENA

        public int SetAutoZReport(string ena, string finrep, string hour, string minute)
        {
            try
            {
                log.Info("Performing AutoZReport");

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;

                string Ena = ena.PadLeft(1);
                string Finrep = finrep.PadLeft(1);
                string Hour = hour.PadLeft(2, '0');
                string Minute = minute.PadLeft(2, '0');

                //Obbligatoria altrimenti non setta il comando
                ZReport();
                //print Z Report
                strObj[0] = Ena + Finrep + Hour + Minute;
                dirIO = posCommonFP.DirectIO(0, 9013, strObj);
                int iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                if (iData != 9013)
                {
                    log.Error("Errore comando AutoZReport non avvenuto, expected risposta: 9013,  received: " + iData);
                }
                if (String.Compare(iObj[0] , "01")!= 0)
                {
                    log.Error("Errore risposta comando AutoZReport, excpected : " + Ena + " Received: " + iObj[0]);
                }
            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Fatal("", e);
                }

            }
            return NumExceptions;
        }


        //AutoZreport :  Lettura chiurusa giornaliera automatica (DirectIO 9213)
        //Input: None
        //Output: ENA Attivazione (1 byte) 0 = Disabled 1 = Enabled in all cases 2 = Enabled if PERIODO INATTIVO condition is True
        //Output: FINREP Print X-01 financial report (1 byte) 1 = No 2 = Yes
        //Output: HH Ora dell'ultima programmazione 2 bytes
        //Output: MM Minuti dell'ultima programmazione 2 Bytes
        //Output: DD Giorno dell'ultima chiusura automatica 2 bytes
        //Output: MM Mese dell'ultima chiusura automatica 2 Bytes
        //Output: YYYY Anno dell'ultima chiusura automatica 4 bytes
        //Output: HH2 Ora dell'ultima chiusura automatica 2 Bytes
        //Output: MM2 Minuti dell'ultima chiusura automatica 2 Bytes

        public int ReadAutoZReport()
        {
            try
            {
                log.Info("Performing ReadAutoZReport");

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;

                

                //print Z Report
                strObj[0] = "";
                dirIO = posCommonFP.DirectIO(0, 9213, strObj);
                int iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                if (iData != 9213)
                {
                    log.Error("Errore comando AutoZReport non avvenuto, expected risposta: 9213,  received: " + iData);
                }
                string ENA = iObj[0].Substring(0,1);
                string FINREP = iObj[0].Substring(1, 1);
                string HH = iObj[0].Substring(2, 2);
                string mm = iObj[0].Substring(4, 2);
                string ZZZZ = iObj[0].Substring(6, 4);
                string DD = iObj[0].Substring(10, 2);
                string MM = iObj[0].Substring(12, 2);
                string YYYY = iObj[0].Substring(14, 4);
                string HH2 = iObj[0].Substring(18, 2);
                string MM2 = iObj[0].Substring(20, 2);

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Fatal("", e);
                }

            }
            return NumExceptions;
        }


        //Test composto da un loop di Richiesta stato e ZReport
        public int TestCrashPos()
        {
            try
            {
                log.Info("Performing TestCrashPos");
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                string mainStatus = "", subStatus = "";
                DirectIOData dirIO;
                int iData;

                for (int i = 0; i < 10; i++)
                {//Check RT Status
                    strObj[0] = "01";
                    dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                    iObj = (string[])dirIO.Object;

                    //Check if  Printer is in RT o non RT mode ( check quarto e quinto byte 1138 command ,secondo protocollo
                    mainStatus = iObj[0].Substring(3, 2);
                    //Console.WriteLine("RT type: " + rtType);
                    int rtTypeInt = Convert.ToInt32(mainStatus);
                    if (rtTypeInt == 1)
                    {
                        log.Error("Printer is in MF mode. " + " La variabile Mode è := " + Mode);
                    }
                    else
                    {
                        if (rtTypeInt == 2)
                        {
                            log.Info("Printer is in RT mode. " + " La variabile Mode è := " + Mode);
                        }
                        else
                        {
                            log.Error("Error DirectIO 1138 campo MAIN, expected 01(MF) or 02(RT), received " + rtTypeInt);
                            throw new PosControlException();
                        }
                    }

                    //Check SubStatus POS
                    subStatus = iObj[0].Substring(5, 2);
                    int rtSubStatus = Convert.ToInt32(subStatus);

                    if (rtSubStatus != 8)
                    {
                        log.Error("La stampante non è in RT come dovrebbe ma è nello substato " + subStatus);
                    }

                    //print Z Report
                    strObj[0] = "01";
                    dirIO = posCommonFP.DirectIO(0, 3001, strObj);

                    //Leggo il flag 63 , che mi indica se sono in DEMO Mode o meno. Devono essere coerenti.
                    strObj[0] = "63";
                    dirIO = posCommonFP.DirectIO(0, 4214, strObj); // Leggo il flag 63
                    iObj = (string[])dirIO.Object;
                }


                //Seconda fase di test : un loop di chiusure e cambi data 
                for (int i = 0; i < 5; i++)
                {

                    //print Z Report
                    strObj[0] = "01";
                    dirIO = posCommonFP.DirectIO(0, 3001, strObj);

                    //Cambio data
                    //sposto la data di 4 ore in AVANTI
                    strObj[0] = DateTime.Now.ToString("ddMMyyHHmm");
                    Int64 temporary = Int64.Parse(strObj[0]);
                    temporary += 200;
                    strObj[0] = temporary.ToString().PadLeft(10, '0');
                    dirIO = posCommonFP.DirectIO(0, 4001, strObj); // tarocco la data in avanti ma solo di 4 ore in modo da poter ripristinare
                    iData = dirIO.Data;
                    if (iData != 4001)
                    {
                        log.Error("Error DirectIO 4001 campo iData, expected 4001, received " + iData);
                        throw new PosControlException();
                    }


                    //Leggo il flag 63 , che mi indica se sono in DEMO Mode o meno. Devono essere coerenti.
                    strObj[0] = "63";
                    dirIO = posCommonFP.DirectIO(0, 4214, strObj); // Leggo il flag 63
                    iObj = (string[])dirIO.Object;

                    //print Z Report
                    strObj[0] = "01";
                    dirIO = posCommonFP.DirectIO(0, 3001, strObj);

                    //Cambio data
                    //ripristino la data di 4 ore in AVANTI
                    strObj[0] = DateTime.Now.ToString("ddMMyyHHmm").PadLeft(10, '0'); ;
                    //temporary = Int64.Parse(strObj[0]);
                    //temporary += 400;
                    //strObj[0] = temporary.ToString().PadLeft(10, '0');
                    dirIO = posCommonFP.DirectIO(0, 4001, strObj); 
                    iData = dirIO.Data;
                    if (iData != 4001)
                    {
                        log.Error("Error DirectIO 4001 campo iData, expected 4001, received " + iData);
                        throw new PosControlException();
                    }
                }
            }
            catch(Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Fatal("", e);
                }
                return NumExceptions;
            }

            return NumExceptions;
        }

            //Test inserito perchè ho scoperto che ogni tanto la stamante va in reset dopo due
            //Zreport di fila: Tale test deriva dal test CheckRtStatus messo per testare il campo
            //dei file REJ della 1138 quando sposto l'orario in avanti
        public int TestDoppioZReport()
        {
            try
            {
            log.Info("Performing TestDoppioZReport");

            string[] strObj = new string[1];
            string[] iObj = new string[1];
            DirectIOData dirIO;
            int iData;
            string mainStatus ="";
            string subStatus = "";

            //print Z Report
            strObj[0] = "01";
            dirIO = posCommonFP.DirectIO(0, 3001, strObj);

            //sposto la data di 2 ore in AVANTI
            strObj[0] = DateTime.Now.ToString("ddMMyyHHmm");
            Int64 temporary = Int64.Parse(strObj[0]);
            temporary += 200; 
            strObj[0] = temporary.ToString().PadLeft(10, '0'); 
            dirIO = posCommonFP.DirectIO(0, 4001, strObj); // tarocco la data in avanti ma solo di 4 ore in modo da poter ripristinare
            iData = dirIO.Data;
            if (iData != 4001)
            {
                log.Error("Error DirectIO 4001 campo iData nello spostare la data in avanti dopo chiusura, expected 4001, received " + iData);
                //throw new PosControlException();
            }


            iObj = (string[])dirIO.Object;

            //print Z Report
            strObj[0] = "01";
            dirIO = posCommonFP.DirectIO(0, 3001, strObj);

            //print Z Report
            strObj[0] = "01";
            dirIO = posCommonFP.DirectIO(0, 3001, strObj);

            resetPrinter();
            resetPrinter();

            //Check RT Status
            strObj[0] = "01";
            dirIO = posCommonFP.DirectIO(0, 1138, strObj);
            iObj = (string[])dirIO.Object;

            //Check if  Printer is in RT o non RT mode ( check quarto e quinto byte 1138 command ,secondo protocollo
            mainStatus = iObj[0].Substring(3, 2);
            //Console.WriteLine("RT type: " + rtType);
            int rtTypeInt = Convert.ToInt32(mainStatus);
            if (rtTypeInt == 1)
            {
                log.Error("Printer is in MF mode. " +  " La variabile Mode è := " + Mode);
            }
            else
            {
                if (rtTypeInt == 2)
                {
                    log.Info("Printer is in RT mode. " + " La variabile Mode è := " + Mode);
                }
                else
                {
                    log.Error("Error DirectIO 1138 campo MAIN, expected 01(MF) or 02(RT), received " + rtTypeInt);
                    throw new PosControlException();
                }
            }

            //Check SubStatus POS
            subStatus = iObj[0].Substring(5, 2);
            int rtSubStatus = Convert.ToInt32(subStatus);

            if(rtSubStatus != 8)
            {
                log.Error("La stampante non è in RT come dovrebbe ma è nello substato " + subStatus);
            }
            //Ripristino la data corretta
            strObj[0] = DateTime.Now.ToString("ddMMyyHHmm");
            dirIO = posCommonFP.DirectIO(0, 4001, strObj);
            iData = dirIO.Data;
            if (iData != 4001)
            {
                log.Error("Error DirectIO 4001 , errore nel ripristino data corretta");
                //throw new PosControlException();
            }

            //print Z Report
            strObj[0] = "01";
            dirIO = posCommonFP.DirectIO(0, 3001, strObj);


            //print Z Report
            strObj[0] = "01";
            dirIO = posCommonFP.DirectIO(0, 3001, strObj);

            //Check RT Status
            strObj[0] = "01";
            dirIO = posCommonFP.DirectIO(0, 1138, strObj);


        }
        catch (Exception e)
        {
            //Console.WriteLine("----- EXCEPTION -----");
            NumExceptions++;
            //resetPrinter();
            if (e is PosControlException)
            {
                PosControlException pce = (PosControlException)e;
                log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                log.Error("Message: " + pce.Message);
                //log.Fatal("", pce);
                //throw;
            }
            else
            {
                log.Fatal("", e);
            }

            return NumExceptions;
        }
        return NumExceptions;
    }

        //Metodo creato per poter cambiare ORARIO!!!(no data) dai test di piu' alto livello presenti sulla classe CustomTest
        //la stringa time deve essere del tipo "hhmm" . Se la stringa è nulla ripristina l'orario attuale
        public int ChangeTime(string time = "")
        {
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                fiscalprinter.PrintZReport();

                if (string.IsNullOrEmpty(time))
                {
                    //Ripristino la data corretta
                    strObj[0] = DateTime.Now.ToString("ddMMyyHHmm");
                    dirIO = posCommonFP.DirectIO(0, 4001, strObj);
                    iData = dirIO.Data;
                    if (iData != 4001)
                    {
                        log.Error("Error DirectIO 4001 , errore nel ripristino data corretta");
                        throw new PosControlException();
                    }
                }
                else
                {
                    strObj[0] = DateTime.Today.ToString("ddMMyy") + time;

                    dirIO = posCommonFP.DirectIO(0, 4001, strObj); 
                    iData = dirIO.Data;
                    if (iData != 4001)
                    {
                        log.Error("Error DirectIO 4001 , tentato fallito di cambiare l'orario di qualche ora");
                        NumExceptions++;
                        throw new PosControlException();
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Fatal("", e);
                }
                //return NumExceptions;
            }
            return NumExceptions;
        }

        //DirectIO 3014
        public void PrintFiscalSums()
        {
            try
            {
                log.Info("Performing PrintFiscalSums");
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;
                string date1 = "010220";
                string date2 = "110220";
                


                strObj[0] = "01" + date1 + date2;
                dirIO = posCommonFP.DirectIO(0, 3014, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Fatal("", e);
                }

                //return NumExceptions;
            }

        }

        //per ogni scontrino all'interno di quello specifico Zrep faccio vari test sullo scontrino (se è annullabile,se è  rendibile, se rendibile again dopo  cent di reso)
        private void TestScontrinoFiscaleFromEJ(string ZRep, string Fiscalre, string Date)
        {
            try
            {
               
                log.Info("Performing TestScontrinoFiscaleFromEJ");
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;

                string printerIdModel;
                string printerIdManufacturer;
                string printerIdNumber;

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                printerIdModel = strData.Substring(0, 2);
                printerIdNumber = strData.Substring(4, 6);
                printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;

                //Chiedo se è annullabile
                log.Info("Chiedo se è anche annullabile");
                strObj[0] = "2" + printerId + Date + Fiscalre + ZRep;   // "2" = VOID
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                int iRet = Convert.ToInt32(iObj[0].Substring(0, 1));
                if (iRet == 0)
                {
                    log.Info("Document can be Voidable");
                }
                else
                {
                    if (iRet == 2)
                    {
                        log.Error("Document can NOT be Voided quando in realtà dovrebbe esserlo, Data: " + Date + "Zreport : " + ZRep + "Scontrino Fiscale: " + Fiscalre);
                    }
                }

                //Chiedo ora se è rendibile
                strObj[0] = "1" + printerId + Date + Fiscalre + ZRep;   // "1" = refund
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                iRet = Convert.ToInt32(iObj[0].Substring(0,1));
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    if(iRet == 2 )
                    {
                        log.Error("Document can NOT be Refunded quando in realtà dovrebbe esserlo, Data: " + Date + " Zreport : " + ZRep + " Scontrino Fiscale: " + Fiscalre);
                    }
                }
                if (iRet == 0)
                {
                    //Ci faccio un reso simbolico di 1 cent
                    // Return document print
                    log.Info("DirectIO (Return document print)");
                    strObj[0] = "0140001REFUND " + ZRep + " " + Fiscalre + " " + Date + " " + printerId;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                    iData = dirIO.Data;
                    iObj = (string[])dirIO.Object;
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)100, (int)1);
                    fiscalprinter.PrintRecTotal((decimal)00000, (decimal)00000, "0CONTANTI");
                    fiscalprinter.EndFiscalReceipt(false);

                    //Ritesto ancora questo scontrino, deve essere ancora rendibile 
                    strObj[0] = "1" + printerId + Date + Fiscalre + ZRep;   // "1" = refund
                    dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                    iData = dirIO.Data;
                    iObj = (string[])dirIO.Object;
                    iRet = Convert.ToInt32(iObj[0].Substring(0, 1));
                    if (iRet == 0)
                    {
                        log.Info("Document can be Refunded, tutto ok");
                    }
                    else
                    {
                        if (iRet == 2)
                        {
                            log.Error("Document can NOT be Refunded quando in realtà dovrebbe esserlo, Data: " + Date + "Zreport : " + ZRep + "Scontrino Fiscale: " + Fiscalre);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Fatal("", e);
                }
                //return NumExceptions;
            }
        }


        //Letture varie dall' Electronic Journal (bisogna farle dalla classe posCommonFP non da posCommonEJ
        //come supponevo,non so perchè ma funziona solo così
        //scrive su file counters.txt il contenuto del DGFE per un range specifico di date
        public void readFromEJ(string data1 = " ", string data2 = " ")
        {
            log.Info("Performing readFromEJ() MEthod");
            // Write the string to a file in append mode
            System.IO.StreamWriter file = new System.IO.StreamWriter("counters.txt", true);
            try
            {
                ZrepRange[] arr = new ZrepRange[3650];
               
                log.Info("Performing readFromEJByNumber() method ");
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                //flag che mi indica qual è il primo scontrino della giornata 

                Boolean isFirst = false;
                string strData = "";
                string date1, date2;
                //questa poi va cambiata e chiesta come input oppure la lascio cosi' e mi prendo sempre il giorno odierno
                if (String.Compare(data1, " ") == 0)
                {
                    //Se non gli passo nulla allora prendo la data odierna , else la data richiesta
                    date1 = DateTime.Today.ToString("ddMMyy");
                    date2 = date1;
                }
                else
                {
                    date1 = data1;
                    date2 = data2;
                }

                string FRN = "";
                //string strDate = "";
                string LN = "";
                string TEXT = "";

                string lines = "";

                //public static PosCommon posCommonFP;
                //Check EJ Status 
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1077, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                //1077 EJ STATUS
                if (iData != 1077)
                {
                    log.Error("Error DirectIO 1077 campo iData, expected 1077, received " + iData);
                    throw new Exception();
                }

                if (!(String.Equals(iObj[0].Substring(0,2), "01")))
                {
                    log.Error("Error DirectIO 1077 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    throw new Exception();
                }

                string STAT = iObj[0].Substring(2, 1);
                if ( !((String.Equals(STAT, "0")) || (String.Equals(STAT, "1"))))
                {
                    log.Error("Error DirectIO 1077 campo STAT, expected 0 o 1, received " + iObj[0].Substring(2, 1));
                    switch (Int32.Parse(STAT))
                    {
                        case 2:
                            log.Error("Unformatted EJ");
                            break;
                        case 3:
                            log.Error("Previous card inserted ");
                            break;
                        case 4:
                            log.Error("Belong to another fiscal printer");
                            break;
                        case 5:
                            log.Error("Full");
                            break;
                        case 6:
                            log.Error("Not inserted");
                            break;
                        default:
                            log.Error("Risposta sconosciuta");
                            break;

                    }
                    throw new PosControlException();
                }

                /*
                // READ FROM EJ BY NUMBER 3100 (Non include le fatture pero')
                log.Info("DirectIO (READ FROM EJ BY NUMBER) 3100");
                strObj[0] = "01" + date1 + "0001" + "9999" + "0";

                dirIO = posCommonFP.DirectIO(0, 3100, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                if (iObj[0].Length > 2)

                {
                    //Ci sono dei valori e leggo finche non ho "operatore"

                    while (iObj[0].Length > 2)
                    {
                        
                        date1 = iObj[0].Substring(2, 6);
                        //Console.WriteLine("date: " + date1);
                        lines += "date : " + date1 + "\r\n";

                        FRN = iObj[0].Substring(8, 4);
                        //Console.WriteLine("Current Fiscal Receipt Number: " + FRN);
                        lines += "Current Fiscal Receipt Number : " + FRN + "\r\n";

                        LN = iObj[0].Substring(12, 4);
                        //Console.WriteLine("Line Sequence Number: " + LN);
                        lines += "Line Sequence Number : " + LN + "\r\n";

                        TEXT = iObj[0].Substring(16, 46);
                        //Console.WriteLine("EJ line text: " + TEXT);
                        lines += "EJ line text : " + TEXT + "\r\n";

                        lines += "\r\n";
                        file.WriteLine(lines);

                        lines = "";

                        strObj[0] = "01" + date1 + "0001" + "9999" + "1";
                        dirIO = posCommonFP.DirectIO(0, 3100, strObj);
                        iData = dirIO.Data;
                        iObj = (string[])dirIO.Object;
                    }
                }


                // READ FROM EJ BY DATE (Anche qui le fatture non sono incluse nel comando)
                //Console.WriteLine("DirectIO (READ FROM EJ BY DATE) 3101");
                strObj[0] = "01" + date1 + date2 + "0";

                dirIO = posCommonFP.DirectIO(0, 3101, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                if (iObj[0].Length > 2)
                {
                    while (iObj[0].Length > 2)
                    {
                       
                        string date = iObj[0].Substring(2, 6);
                        //Console.WriteLine("Actual Date: " + date);
                        lines += "date : " + date + "\r\n";

                        FRN = iObj[0].Substring(8, 4);
                        //Console.WriteLine("Current Fiscal Receipt Number: " + FRN);
                        lines += "Current Fiscal Receipt Number : " + FRN + "\r\n";

                        LN = iObj[0].Substring(12, 4);
                        //Console.WriteLine("Line Sequence Number: " + LN);
                        lines += "Line Sequence Number : " + LN + "\r\n";

                        TEXT = iObj[0].Substring(16, 46);
                        //Console.WriteLine("EJ line text: " + TEXT);
                        lines += "EJ line text : " + TEXT + "\r\n";

                        lines += "\r\n";
                        file.WriteLine(lines);

                        lines = "";

                        strObj[0] = "01" + date1 + date2 + "1";
                        dirIO = posCommonFP.DirectIO(0, 3101, strObj);
                        iData = dirIO.Data;
                        iObj = (string[])dirIO.Object;
                    }
                }
                */

                
                // READ FROM EJ BY DATE AND TYPE (Sono incluse anche le fatture qui,equivale al ZRep 99)
                
                log.Info("DirectIO (READ FROM EJ BY DATE AND TYPE) 3103");
                strObj[0] = "01" + "1" + "0" + date1 + date1 + "0" + "00";

                dirIO = posCommonFP.DirectIO(0, 3103, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                string dateold = "";
                string FRNold = "";
                string date = "";
                int Zold = 0;
                string annos = "";
                int Lastreceipt = 0;

                if (iObj[0].Length > 2)
                {
                    
                    while (iObj[0].Length > 2)
                    {

                        date = iObj[0].Substring(2, 6);
                        if(string.Compare(date, dateold) !=0)
                            //lines += "date : " + date + "\r\n";

                        
                        FRN = iObj[0].Substring(8, 4);
                        if (string.Compare(FRN, FRNold) != 0)
                            //lines += "Current Fiscal Receipt Number : " + FRN + "\r\n";


                        LN = iObj[0].Substring(12, 4);
                        //Console.WriteLine("Line Sequence Number: " + LN);
                        lines +=  LN + "\r" + " ";

                        TEXT = iObj[0].Substring(16, 46);
                       
                        lines +=  TEXT + "\r\n";



                        if ((String.Compare(TEXT.Substring(12, 12), "DOCUMENTO?N.") == 0) || (String.Compare(TEXT.Substring(12, 12), "DOCUMENTO N.") == 0))
                        {
                            //Ho trovato uno scontrino fiscale , lo testo per far si che:
                            //1) Sia rendibile, se ok ci tolgo un centesimo e richiedo se rendibile again
                            //2) Se è annullabile
                            // Se uno dei due test fallisce log.Error(Zrep e Fiscalreceipt)
                            int Zrep = Convert.ToInt32(TEXT.Substring(25, 4));
                            int Fiscalreceipt = Convert.ToInt32(TEXT.Substring(30, 4));
                            annos = "20" + date.Substring(4, 2);

                            //Inizia uno Zrep nuovo ergo devo aggiornare l'array con i limiti di Zold
                            if ((Zrep != Zold) && (Zold != 0)) //cambio di Zrep e non è il primo
                            {
                                arr[Zold].Zrep = Zold;
                                arr[Zold].finish = Lastreceipt;
                                arr[Zold].date = date.Substring(0, 4) + annos; //questo non cambia perchè siamo sempre nella stessa data
                                if (!(isFirst)) //è il primo e anche l'unico quindi è inutile mettere il flag a true
                                {
                                    isFirst = true;
                                    arr[Zrep].start = Fiscalreceipt;
                                }
                                Zold = Zrep;
                                Lastreceipt = Fiscalreceipt;
                                //isFirst = false;
                                arr[Zrep].Zrep = Zrep;
                                arr[Zrep].start = Fiscalreceipt;
                                arr[Zrep].date = date.Substring(0, 4) + annos;
                            }
                            else
                            {
                                if ((Zrep != Zold) && (Zold == 0)) //E' il primo Zrep
                                {
                                    Zold = Zrep;
                                    Lastreceipt = Fiscalreceipt;
                                    arr[Zold].finish = Lastreceipt;
                                    arr[Zold].Zrep = Zold;
                                    arr[Zold].date = date.Substring(0, 4) + annos;
                                    if (!(isFirst)) //è il primo 
                                    {
                                        isFirst = true;
                                        arr[Zrep].start = Fiscalreceipt;
                                    }

                                }
                                else
                                if (Zrep == Zold) //Sta loopando sullo stesso Zrep
                                {
                                    if (!(isFirst))
                                    {
                                        isFirst = true;
                                        arr[Zrep].start = Fiscalreceipt;
                                    }
                                    Lastreceipt = Fiscalreceipt;
                                }
                            }



                        }
                        lines += "\r\n";
                        file.WriteLine(lines);

                        lines = "";

                        strObj[0] = "01" + "1" + "0" + date1 + date1 + "1" + "00";

                        dirIO = posCommonFP.DirectIO(0, 3103, strObj);
                        iData = dirIO.Data;
                        iObj = (string[])dirIO.Object;

                        FRNold = FRN;
                        dateold = date;
                    }

                    //ultimo ZReport del giorno
                    arr[Zold].Zrep = Zold;
                    arr[Zold].finish = Lastreceipt;
                    arr[Zold].date = date.Substring(0, 4) + annos;

                    for (int i = 0; i < 3650; i++)
                    {   //Per ogni Zrep
                        if (arr[i].Zrep != 0)
                        {

                            for (int j = arr[i].start; j <= arr[i].finish; ++j)
                            {
                                //per ogni scontrino all'interno di quello specifico Zrep

                                TestScontrinoFiscaleFromEJ(arr[i].Zrep.ToString().PadLeft(4, '0'), j.ToString().PadLeft(4, '0'), arr[i].date);
                            }
                        }

                    }


                    //chiudo il file descriptor senno la ricorsione non mi funziona, tanto cmq lo riapro in append mode
                    file.Close();
                    int giorno = Convert.ToInt32(date1.Substring(0, 2));
                    int mese = Convert.ToInt32(date1.Substring(2, 2));
                    int anno = 2000 + Convert.ToInt32(date1.Substring(4, 2));

                    DateTime data = new DateTime(anno, mese, giorno, 00, 00, 00);

                    DateTime target = new DateTime();
                    if (data2 == " ")
                    {
                        target = DateTime.Now;
                    }
                    else
                    {
                        target = new DateTime(Convert.ToInt32(data2.Substring(4, 2)), Convert.ToInt32(data2.Substring(2, 2)), Convert.ToInt32(data2.Substring(0, 2)), 0, 0, 0);
                    }

                    if (data.DayOfYear < target.DayOfYear)
                    {//Incrementiamo il giorno per cambiare data (al giorno dopo)
                        data = data.AddDays(1);
                        int nextgiorno = data.Day;
                        int nextmese = data.Month;
                        int nextanno = data.Year;
                        readFromEJ(nextgiorno.ToString().PadLeft(2, '0') + nextmese.ToString().PadLeft(2, '0') + nextanno.ToString().Substring(2, 2).PadLeft(2, '0'), nextgiorno.ToString().PadLeft(2, '0') + nextmese.ToString().PadLeft(2, '0') + nextanno.ToString().Substring(2, 2).PadLeft(2, '0'));
                    }
                    else
                    {
                        //E' finito l'algoritmo
                    }

                }
                else
                //E' finito il giorno e non c'è nulla ergo passo al giorno successivo
                {
                    //chiudo il file descriptor senno la ricorsione non mi funziona, tanto cmq lo riapro in append mode
                    file.Close();
                    int giorno = Convert.ToInt32(date1.Substring(0, 2));
                    int mese = Convert.ToInt32(date1.Substring(2, 2));
                    int anno = 2000 + Convert.ToInt32(date1.Substring(4, 2));

                    DateTime data = new DateTime(anno, mese, giorno, 00, 00, 00);

                    DateTime target = new DateTime();
                    target = DateTime.Now;

                    if (data2 == " ")
                    {
                        target = DateTime.Now;
                    }
                    else
                    {
                        target = new DateTime(Convert.ToInt32(data2.Substring(4, 2)), Convert.ToInt32(data2.Substring(2, 2)), Convert.ToInt32(data2.Substring(0, 2)), 0, 0, 0);
                    }

                    if (data.DayOfYear < target.DayOfYear)
                    {//Incrementiamo il giorno per cambiare data (al giorno dopo)
                        data = data.AddDays(1);
                        int nextgiorno = data.Day;
                        int nextmese = data.Month;
                        int nextanno = data.Year;
                        readFromEJ(nextgiorno.ToString().PadLeft(2, '0') + nextmese.ToString().PadLeft(2, '0') + nextanno.ToString().Substring(2, 2).PadLeft(2, '0'), nextgiorno.ToString().PadLeft(2, '0') + nextmese.ToString().PadLeft(2, '0') + nextanno.ToString().Substring(2, 2).PadLeft(2, '0'));
                    }
                    else
                    {
                        //E' finito l'algoritmo
                    }
                }

                

                
                /*
                // READ FROM EJ BY NUMBER AND TYPE
                //comando lungo ,imposto un timeout diverso dal default

                //posCommonFP.DirectIO(-112, 20000, "");
                log.Info("DirectIO (READ FROM EJ BY NUMBER AND TYPE) 3104");
                strObj[0] = "01" + "1" + "0" + date1 + "0001" + "9999" + "0" + "00";

                
                int Lastreceipt = 0;

                dirIO = posCommonFP.DirectIO(0, 3104, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                //string dateold = "";
                //string FRNold = "";
                int Zold = 0;
                string annos = "";
                date = "";
                //TODO: sto if mi sembra ridondante con la parte in comune dell else, refactorizzare + avanti quando ho + tempo libero
                if (iObj[0].Length > 2)
                {
                    while (iObj[0].Length > 2)
                    {


                        date = iObj[0].Substring(2, 6);
                        if (string.Compare(date, dateold) != 0)
                            lines += "date : " + date + "\r\n";

                        FRN = iObj[0].Substring(8, 4);
                        if (string.Compare(FRN, FRNold) != 0)
                            lines += "Current Fiscal Receipt Number : " + FRN + "\r\n";

                        LN = iObj[0].Substring(12, 4);
                        //Console.WriteLine("Line Sequence Number: " + LN);
                        lines += "Line Sequence Number : " + LN + "\r\n";

                        TEXT = iObj[0].Substring(16, 46);
                        //Console.WriteLine("EJ line text: " + TEXT);
                        lines += "EJ line text : " + TEXT + "\r\n";

                        
                        //ho trovato uno scontrino fiscale nuovo
                        if ((String.Compare(TEXT.Substring(12,12) , "DOCUMENTO?N.") == 0) || (String.Compare(TEXT.Substring(12, 12), "DOCUMENTO N.") == 0))
                        {
                            //Ho trovato uno scontrino fiscale , lo testo per far si che:
                            //1) Sia rendibile, se ok ci tolgo un centesimo e richiedo se rendibile again
                            //2) Se è annullabile
                            // Se uno dei due test fallisce log.Error(Zrep e Fiscalreceipt)
                            int Zrep = Convert.ToInt32(TEXT.Substring(25, 4));
                            int  Fiscalreceipt = Convert.ToInt32(TEXT.Substring(30, 4));
                            annos = "20" + date.Substring(4, 2);
                            
                            //Inizia uno Zrep nuovo ergo devo aggiornare l'array con i limiti di Zold
                            if ((Zrep != Zold) && (Zold != 0)) //cambio di Zrep e non è il primo
                            {
                                arr[Zold].Zrep = Zold;
                                arr[Zold].finish = Lastreceipt;
                                arr[Zold].date = date.Substring(0, 4) + annos; //questo non cambia perchè siamo sempre nella stessa data
                                if (!(isFirst)) //è il primo e anche l'unico quindi è inutile mettere il flag a true
                                {
                                    isFirst = true;
                                    arr[Zrep].start = Fiscalreceipt;
                                }
                                Zold = Zrep;
                                Lastreceipt = Fiscalreceipt;
                                //isFirst = false;
                                arr[Zrep].Zrep = Zrep;
                                arr[Zrep].start = Fiscalreceipt;
                                arr[Zrep].date = date.Substring(0, 4) + annos;
                            }
                            else
                            {
                                if ((Zrep != Zold) && (Zold == 0)) //E' il primo Zrep
                                {
                                    Zold = Zrep;
                                    Lastreceipt = Fiscalreceipt;
                                    arr[Zold].finish = Lastreceipt;
                                    arr[Zold].Zrep = Zold;
                                    arr[Zold].date = date.Substring(0, 4) + annos;
                                    if (!(isFirst)) //è il primo 
                                    {
                                        isFirst = true;
                                        arr[Zrep].start = Fiscalreceipt;
                                    }
                                   
                                }
                                else
                                if (Zrep == Zold) //Sta loopando sullo stesso Zrep
                                {
                                    if (!(isFirst)) 
                                    {
                                        isFirst = true;
                                        arr[Zrep].start = Fiscalreceipt;
                                    }
                                    Lastreceipt = Fiscalreceipt;
                                }
                            }


                            //spostata in giu' dopo la fine del loop
                            //TestScontrinoFiscaleFromEJ(Zrep, Fiscalreceipt, date.Substring(0, 4) + annos);
                            

                        }

                        lines += "\r\n";
                        file.WriteLine(lines);

                        lines = "";

                        strObj[0] = "01" + "1" + "0" + date1 + "0001" + "9999" + "1" + "00";
                        dirIO = posCommonFP.DirectIO(0, 3104, strObj);
                        iData = dirIO.Data;
                        iObj = (string[])dirIO.Object;

                        FRNold = FRN;
                        dateold = date;

                    }
                    //ultimo ZReport del giorno
                    arr[Zold].Zrep = Zold;
                    arr[Zold].finish = Lastreceipt;
                    arr[Zold].date = date.Substring(0, 4) + annos;

                    for (int i = 0; i < 3650; i++)
                    {   //Per ogni Zrep
                        if (arr[i].Zrep != 0)
                        {
                            
                            for (int j = arr[i].start; j <= arr[i].finish; ++j)
                            {
                                //per ogni scontrino all'interno di quello specifico Zrep
                                
                                TestScontrinoFiscaleFromEJ(arr[i].Zrep.ToString().PadLeft(4, '0'), j.ToString().PadLeft(4, '0'), arr[i].date);
                            }
                        }

                    }


                    //chiudo il file descriptor senno la ricorsione non mi funziona, tanto cmq lo riapro in append mode
                    file.Close();
                    int giorno = Convert.ToInt32(date1.Substring(0, 2));
                    int mese = Convert.ToInt32(date1.Substring(2, 2));
                    int anno = 2000 + Convert.ToInt32(date1.Substring(4, 2));

                    DateTime data = new DateTime(anno, mese, giorno, 00, 00, 00);

                    DateTime target = new DateTime();
                    if (data2 == " ")
                    {
                        target = DateTime.Now;
                    }
                    else
                    {
                        target = new DateTime(Convert.ToInt32(data2.Substring(4,2)), Convert.ToInt32(data2.Substring(2, 2)), Convert.ToInt32(data2.Substring(0 , 2)) , 0, 0, 0);
                    }

                    if (data.DayOfYear < target.DayOfYear)
                    {//Incrementiamo il giorno per cambiare data (al giorno dopo)
                        data = data.AddDays(1);
                        int nextgiorno = data.Day;
                        int nextmese = data.Month;
                        int nextanno = data.Year;
                        readFromEJ(nextgiorno.ToString().PadLeft(2, '0') + nextmese.ToString().PadLeft(2, '0') + nextanno.ToString().Substring(2, 2).PadLeft(2, '0'));
                    }
                    else
                    {
                        //E' finito l'algoritmo
                    }

                }
                else
                //E' finito il giorno e non c'è nulla ergo passo al giorno successivo
                {
                    //chiudo il file descriptor senno la ricorsione non mi funziona, tanto cmq lo riapro in append mode
                    file.Close();
                    int giorno = Convert.ToInt32(date1.Substring(0, 2));
                    int mese = Convert.ToInt32(date1.Substring(2, 2));
                    int anno = 2000 + Convert.ToInt32(date1.Substring(4, 2));
                    
                    DateTime data = new DateTime(anno, mese, giorno, 00 , 00, 00);

                    DateTime target = new DateTime();
                    target = DateTime.Now;

                    if (data.DayOfYear < target.DayOfYear)
                    {//Incrementiamo il giorno per cambiare data (al giorno dopo)
                        data = data.AddDays(1);
                        int nextgiorno = data.Day;
                        int nextmese = data.Month;
                        int nextanno = data.Year;
                        readFromEJ(nextgiorno.ToString().PadLeft(2, '0') + nextmese.ToString().PadLeft(2,'0') +  nextanno.ToString().Substring(2,2).PadLeft(2,'0'));
                    }
                    else
                    {
                        //E' finito l'algoritmo
                    }
                }
                */

                //PROVA GET FISCAL SERIANL NUMBER
                log.Info("DirectIO (GET FISCAL SERIANL NUMBER) 3217");
                strObj[0] = "01";

                dirIO = posCommonFP.DirectIO(0, 3217, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                if (iData != 3217)
                {
                    log.Error("Errore DirectIO 3217 campo iData, expected " + 3217 + " received " + iData );
                    throw new PosControlException();

                }

                string SN = iObj[0].Substring(2, 6);
                if ((Int32.Parse(SN) < 0 ) || (Int32.Parse(SN) > 999999))
                {
                    log.Error("Errore DirectIO 3217 campo SN, valore fuori range in quanto ricevuto " + SN);
                    throw new PosControlException();
                }
                string MOD = iObj[0].Substring(8, 2);

               
                        
                string VENDOR = iObj[0].Substring(10, 2);
                if (Int32.Parse(VENDOR) != 99)
                {
                    log.Error("Errore DirectIO 3217 campo VENDOR, expected 99 , received " + VENDOR);
                }

                //Console.WriteLine("SN = {0} , MOD = {1} , VENDOR = {2}", SN, MOD, VENDOR);
                file.Close();
             }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    log.Fatal("", e);
                }
                file.Close();
            }
        }
        
        public int TestMolinari()
        {
            int output = 0;
            try
            {
                initFiscalDevice("FiscalPrinter");
                fiscalprinter.BeginFiscalReceipt(true);
                resetPrinter();
                output = testFiscalReceiptClass("FiscalPrinter");
                resetPrinter();
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);
                    //throw;
                }
                else
                {

                    log.Fatal("", e);

                }

                return NumExceptions;
            }
            return output;
        }

        //Test DirectIO 1145 Sound Buzzer
        public int TestBuzzer()
        {
            try
            {
                
              // routine che genera suono di errore
              string[] strObj = new string[1];
              DirectIOData dirIO;
              int iData;
              strObj[0] = "0101010";
              string freq = "0100";
              string doinf = "0261";
              string re = "0293";
              string mi = "0330";
              string fa = "0349";
              string sol = "0392";
              string la = "0440";
              string si = "0494";
              string dosup = "0522";

                  //int fr = Int32.Parse(freq) + (i * Int32.Parse(freq));
                  //freq = fr.ToString().PadLeft(4, '0'); 
                  //strObj[0] =  "0101010" + freq;
                  strObj[0] = "0101010" + doinf;
                  dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                  System.Threading.Thread.Sleep(300);

                  strObj[0] = "0101010" + re;
                  dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                  System.Threading.Thread.Sleep(300);

                  strObj[0] = "0101010" + mi;
                  dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                  System.Threading.Thread.Sleep(300);

                  strObj[0] = "0101010" + fa;
                  dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                  System.Threading.Thread.Sleep(300);

                  strObj[0] = "0101010" + sol;
                  dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                  System.Threading.Thread.Sleep(300);

                  strObj[0] = "0101010" + la;
                  dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                  System.Threading.Thread.Sleep(300);

                  strObj[0] = "0101010" + si;
                  dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                  System.Threading.Thread.Sleep(300);

                  strObj[0] = "0101010" + dosup;
                  dirIO = posCommonFP.DirectIO(0, 1145, strObj);
                  System.Threading.Thread.Sleep(300);


              
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);
                    //throw;
                }
                else
                {

                    log.Fatal("", e);

                }

                
            }
            return NumExceptions;
        }






        //06/02/2020 Metodo creato per Andrea per creare 1000 fatture consecutive
        //Serve per testare i resi (successivamente)

        public int Create1000Fatture()
        {
            log.Info("Performing Create1000Fatture Method ");
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;


                for (int j = 0; j < 1000; ++j)
                {
                  

                   
                    //DirectIO 1089: Comando di apertura Fatture diretta
                    //Apertura Fattura 
                    strObj[0] = "0100000";
                    dirIO = PosCommonFP.DirectIO(0, 1089, strObj);
                    fiscalprinter.BeginFiscalReceipt(true);
                    // Vendita
                    fiscalprinter.PrintRecItem("Vendita tramite Fattura", (decimal)10000, (int)1000, (int)1, (decimal)50000, "");
                    // Pagamento
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)10000, "000CONTANTI");
                    fiscalprinter.PrintRecTotal((decimal)20000, (decimal)20000, "201CARTA DI CREDITO");
                    fiscalprinter.PrintRecTotal((decimal)20000, (decimal)20000, "301TICKET");
                    fiscalprinter.EndFiscalReceipt(true);
                   
                    
                    //Fattura che segue a scontrino fiscale
                    //Nota:TODO in fase di testing le fatture le troverai dal 25 settembre 2019 in poi,non prima!!! 
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("Random Object No Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");

                    fiscalprinter.EndFiscalReceipt(true);
                    string myLineNumber = "";
                    string myLineText = "";
                    for (int i = 1; i < 4; i++) // 20 righe possibili. 
                    {
                        myLineNumber = i.ToString("00"); // Deve essere due digit 
                        myLineText = "Riga addizionale " + i;
                        myLineText = myLineText + "                                              ";
                        myLineText = myLineText.Substring(0, 46); // 0,46 in caso dei modelli “Intelligent” 
                        strObj[0] = "01" + "5" + myLineNumber + "0" + "1" + myLineText;
                        dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                    }


                    // Inviare le righe del cliente. 
                    string myLineType = "6";
                    for (int i = 1; i < 6; i++) // 5 righe possibili (non programmabile). 
                    {
                        myLineNumber = i.ToString("00"); // Deve essere due digit 
                        myLineText = "Riga cliente " + i;
                        myLineText = myLineText + "                                              ";
                        myLineText = myLineText.Substring(0, 46); // 0, 46 in caso dei modelli “Intelligent” 
                        strObj[0] = "01" + myLineType + myLineNumber + "0" + "1" + myLineText;
                        dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                    }

                    //Richiedere Fattura a seguito scontrino fiscale / documento commerciale
                    strObj[0] = "01" + "00000";
                    dirIO = posCommonFP.DirectIO(0, 1052, strObj);

                    
                   


                }

                
            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;
        }






        //05/02/2020 Metodo creato per creare un database con scontrini lotterie e non, dal primo
        //gennaio 2019 fino ad oggi: per farlo utilizzo una versione di Debug speciale che mi consente
        //di settare la data indietro : lo scopo è creare poi un DGFE che verrà copiato in backup
        //da utilizzare per test futuri quando ho necessità di formattare la memoria fiscale e il dgfe
        

        public int CreateDataBase()
        {
            log.Info("Performing CreateDataBase Method ");
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;

               
                for(int j = 1; j < 2; ++j)
                {
                    //Scontrini con lotteria
                    for (int i = 0; i < 10; ++i)
                    {
                        
                        fiscalprinter.BeginFiscalReceipt(true);
                        fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                        fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                        strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                        dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                        fiscalprinter.EndFiscalReceipt(true);
                    }

                    //Scontrini con lotteria
                    for (int i = 0; i < 5; ++i)
                    {

                        fiscalprinter.BeginFiscalReceipt(true);
                        fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                        fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                        strObj[0] = "01" + "ABCDEFGH" + "        " + "0000";
                        dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                        fiscalprinter.EndFiscalReceipt(true);
                    }



                    for (int i = 0; i < 3; ++i)
                    {
                        //DirectIO 1089: Comando di apertura Fatture diretta
                        //Apertura Fattura 
                        strObj[0] = "0100000";
                        dirIO = PosCommonFP.DirectIO(0, 1089, strObj);
                        fiscalprinter.BeginFiscalReceipt(true);
                        // Vendita
                        fiscalprinter.PrintRecItem("Vendita tramite Fattura", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                        // Pagamento
                        fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                        fiscalprinter.EndFiscalReceipt(true);
                    }

                    //Fattura che segue a scontrino fiscale
                    //Nota:TODO in fase di testing le fatture le troverai dal 25 settembre 2019 in poi,non prima!!! 
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("Random Object No Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");

                    fiscalprinter.EndFiscalReceipt(true);
                    string myLineNumber = "";
                    string myLineText = "";
                    for (int i = 1; i < 4; i++) // 20 righe possibili. 
                    {
                        myLineNumber = i.ToString("00"); // Deve essere due digit 
                        myLineText = "Riga addizionale " + i;
                        myLineText = myLineText + "                                              ";
                        myLineText = myLineText.Substring(0, 46); // 0,46 in caso dei modelli “Intelligent” 
                        strObj[0] = "01" + "5" + myLineNumber + "0" + "1" + myLineText;
                        dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                    }


                    // Inviare le righe del cliente. 
                    string myLineType = "6";
                    for (int i = 1; i < 6; i++) // 5 righe possibili (non programmabile). 
                    {
                        myLineNumber = i.ToString("00"); // Deve essere due digit 
                        myLineText = "Riga cliente " + i;
                        myLineText = myLineText + "                                              ";
                        myLineText = myLineText.Substring(0, 46); // 0, 46 in caso dei modelli “Intelligent” 
                        strObj[0] = "01" + myLineType + myLineNumber + "0" + "1" + myLineText;
                        dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                    }

                    //Richiedere Fattura a seguito scontrino fiscale / documento commerciale
                    strObj[0] = "01" + "00000";
                    dirIO = posCommonFP.DirectIO(0, 1052, strObj);



                    //Scontrini senza lotteria
                    for (int i = 0; i < 10; ++i)
                    {
                        fiscalprinter.BeginFiscalReceipt(true);
                        fiscalprinter.PrintRecItem("Random Object No Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                        fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");

                        fiscalprinter.EndFiscalReceipt(true);

                    }



                    //Faccio chiusura 
                    fiscalprinter.PrintZReport();
                    
                    //Leggo data
                    dirIO = posCommonFP.DirectIO(0, 4201, strObj);
                    iData = dirIO.Data;
                    if (iData != 4201)
                    {
                        log.Error("Error DirecIO 4201 , tentativo fallito di leggere la data ");
                        NumExceptions++;
                        //throw new PosControlException();

                    }

                    iObj = (string[])dirIO.Object;

                    int giorno = Convert.ToInt32(iObj[0].Substring(0, 2));
                    int mese = Convert.ToInt32(iObj[0].Substring(2, 2));
                    int anno = 2000 + Convert.ToInt32(iObj[0].Substring(4, 2));
                    int ora = Convert.ToInt32(iObj[0].Substring(6, 2));
                    int minuti = Convert.ToInt32(iObj[0].Substring(8, 2));
                    DateTime data = new DateTime(anno, mese, giorno, ora, minuti, 00);

                    //Incrementiamo il giorno per cambiare data (al giorno dopo)
                    data = data.AddDays(1);
                    if (data <= System.DateTime.Now)
                    {
                        strObj[0] = data.ToString("ddMMyyHHmm");
                        dirIO = posCommonFP.DirectIO(0, 4001, strObj);
                        iData = dirIO.Data;
                        if (iData != 4001)
                        {
                            log.Error("Error DirecIO 4001 , tentativo fallito di settare la data avanti di un giorno");
                            NumExceptions++;
                            //throw new PosControlException();

                        }
                        ZReport();
                    }
                }
               
            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;
        }

        //Metodo creato per creare un minidb di dati di input misti scontrini fiscale e fatture
        public int ScontrinoLungo()
        {
            log.Info("Performing CreateRandomSequence Method ");
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;

                //Scontrino con lotteria
                fiscalprinter.BeginFiscalReceipt(true);
                for (int i = 0; i < 100; i++)
                {
                    fiscalprinter.PrintRecItem("Random Object number " + i.ToString() , (decimal)10000, (int)1000, (int)1, (decimal)1000, "");
                }
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
               
           
                fiscalprinter.EndFiscalReceipt(true);
            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;
        }

        //Metodo creato per creare un minidb di dati di input misti scontrini fiscale e fatture
        public int CreateRandomSequence()
        {
            log.Info("Performing CreateRandomSequence Method ");
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;
   
                //Scontrino con lotteria
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                fiscalprinter.EndFiscalReceipt(true);


                string zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                int nextInt = Int32.Parse(zRepNum) + 1;
                zRepNum = nextInt.ToString("0000");

                // Get rec num
                string recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;

                

                // Get date
                //Console.WriteLine("Performing GetDate() method ");
                strData = fiscalprinter.GetDate().ToString();
                //Console.WriteLine("Date: " + strData);
                string strDate = strData.Substring(0, 2) + strData.Substring(3, 2) + strData.Substring(6, 4);



                //LO ANNULLIAMO
                strObj[0] = "0140001VOID " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;


                //DirectIO 1089: Comando di apertura Fatture diretta
                //Apertura Fattura 
                strObj[0] = "0100000";
                dirIO = PosCommonFP.DirectIO(0, 1089, strObj);
                fiscalprinter.BeginFiscalReceipt(true);
                // Vendita
                fiscalprinter.PrintRecItem("Vendita tramite Fattura", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                // Pagamento
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                fiscalprinter.EndFiscalReceipt(true);




                //Uno scontrino due fatture
                //Scontrini con lotteria
                for (int i = 0; i < 4; i++)
                {
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    //strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                    //dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                    fiscalprinter.EndFiscalReceipt(true);
                }
                for (int i = 0; i < 2; i++)
                {//DirectIO 1089: Comando di apertura Fatture diretta
                    //Apertura Fattura 
                    strObj[0] = "0100000";
                    dirIO = PosCommonFP.DirectIO(0, 1089, strObj);
                    fiscalprinter.BeginFiscalReceipt(true);
                    // Vendita
                    fiscalprinter.PrintRecItem("Vendita tramite Fattura", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    // Pagamento
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    fiscalprinter.EndFiscalReceipt(true);
                }

                XReport();

                //2 Scontrini una fattura
                for (int i = 0; i < 2; ++i)
                {
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                    dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                    fiscalprinter.EndFiscalReceipt(true);
                }

                //DirectIO 1089: Comando di apertura Fatture diretta
                //Apertura Fattura 
                strObj[0] = "0100000";
                dirIO = PosCommonFP.DirectIO(0, 1089, strObj);
                fiscalprinter.BeginFiscalReceipt(true);
                // Vendita
                fiscalprinter.PrintRecItem("Vendita tramite Fattura", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                // Pagamento
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                fiscalprinter.EndFiscalReceipt(true);

           

                //1 sc 3 fatt
                //Scontrini con lotteria

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                fiscalprinter.EndFiscalReceipt(true);

                XReport();

                for (int i = 0; i < 3; ++i)
                {//DirectIO 1089: Comando di apertura Fatture diretta
                    //Apertura Fattura 
                    strObj[0] = "0100000";
                    dirIO = PosCommonFP.DirectIO(0, 1089, strObj);
                    fiscalprinter.BeginFiscalReceipt(true);
                    // Vendita
                    fiscalprinter.PrintRecItem("Vendita tramite Fattura", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    // Pagamento
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    fiscalprinter.EndFiscalReceipt(true);

                }
                XReport();
                //3 sc 1 fat
                //Scontrini con lotteria
                for (int i = 0; i < 3; ++i)
                {
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                    dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                    fiscalprinter.EndFiscalReceipt(true);
                }
                XReport();

                //DirectIO 1089: Comando di apertura Fatture diretta
                //Apertura Fattura 
                strObj[0] = "0100000";
                dirIO = PosCommonFP.DirectIO(0, 1089, strObj);
                fiscalprinter.BeginFiscalReceipt(true);
                // Vendita
                fiscalprinter.PrintRecItem("Vendita tramite Fattura", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                // Pagamento
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                fiscalprinter.EndFiscalReceipt(true);

                //Faccio chiusura 
                //fiscalprinter.PrintZReport();
                XZReport();

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;
        }




        //Metodo creato per creare un minidb di dati di input misti scontrini fiscale e biglietti cinema
        public int CreateRandomSequenceForCinema()
        {
            log.Info("Performing CreateRandomSequenceForCinema Method ");
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;



                //Scontrino con lotteria

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                fiscalprinter.EndFiscalReceipt(true);


                //Entro in modalità Biglietteria
                string Keyword = "9167";
                string randomNum = "56";
                strObj[0] = randomNum + Keyword ;
                dirIO = posCommonFP.DirectIO(0, 5001, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                //Apro Titolo di Accesso
                strObj[0] = randomNum + Keyword;
                dirIO = posCommonFP.DirectIO(0, 5002, strObj);

                //Stampa Riga del Titolo
                strObj[0] = randomNum + Keyword + "R" + "1" + "TEST BIGLIETTO CINEMA                   ";
                dirIO = posCommonFP.DirectIO(0, 5003, strObj);

                //Stampa Sigillo
                strObj[0] = randomNum + Keyword + "S" + "1" + "xxxxxxxxxxxxxx SIGILLO                  ";
                dirIO = posCommonFP.DirectIO(0, 5003, strObj);

                //Chiudo Titolo di Accesso
                strObj[0] = randomNum + Keyword;
                dirIO = posCommonFP.DirectIO(0, 5004, strObj);

                
                //Esco modalità Biglietteria
                strObj[0] = randomNum + Keyword;
                dirIO = posCommonFP.DirectIO(0, 5005, strObj);
                


                //Uno scontrino due biglietti cinema
                //Scontrini con lotteria

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                fiscalprinter.EndFiscalReceipt(true);

                //Entro in modalità Biglietteria
                 Keyword = "9167";
                 randomNum = "56";
                for (int i = 0; i < 2; i++)
                {
                    strObj[0] = randomNum + Keyword;
                    dirIO = posCommonFP.DirectIO(0, 5001, strObj);
                    iData = dirIO.Data;
                    iObj = (string[])dirIO.Object;

                    //Apro Titolo di Accesso
                    strObj[0] = randomNum + Keyword;
                    dirIO = posCommonFP.DirectIO(0, 5002, strObj);

                    //Stampa Riga del Titolo
                    strObj[0] = randomNum + Keyword + "R" + "1" + "TEST BIGLIETTO CINEMA                   ";
                    dirIO = posCommonFP.DirectIO(0, 5003, strObj);

                    //Stampa Sigillo
                    strObj[0] = randomNum + Keyword + "S" + "1" + "xxxxxxxxxxxxxx SIGILLO                  ";
                    dirIO = posCommonFP.DirectIO(0, 5003, strObj);

                    //Chiudo Titolo di Accesso
                    strObj[0] = randomNum + Keyword;
                    dirIO = posCommonFP.DirectIO(0, 5004, strObj);


                    //Esco modalità Biglietteria
                    strObj[0] = randomNum + Keyword;
                    dirIO = posCommonFP.DirectIO(0, 5005, strObj);

                }


                //2 Scontrini un biglietto Cinema
                for (int i = 0; i < 2; ++i)
                {
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                    dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                    fiscalprinter.EndFiscalReceipt(true);
                }

                //Entro in modalità Biglietteria
                 Keyword = "9167";
                 randomNum = "56";
                strObj[0] = randomNum + Keyword;
                dirIO = posCommonFP.DirectIO(0, 5001, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                //Apro Titolo di Accesso
                strObj[0] = randomNum + Keyword;
                dirIO = posCommonFP.DirectIO(0, 5002, strObj);

                //Stampa Riga del Titolo
                strObj[0] = randomNum + Keyword + "R" + "1" + "TEST BIGLIETTO CINEMA                   ";
                dirIO = posCommonFP.DirectIO(0, 5003, strObj);

                //Stampa Sigillo
                strObj[0] = randomNum + Keyword + "S" + "1" + "xxxxxxxxxxxxxx SIGILLO                  ";
                dirIO = posCommonFP.DirectIO(0, 5003, strObj);

                //Chiudo Titolo di Accesso
                strObj[0] = randomNum + Keyword;
                dirIO = posCommonFP.DirectIO(0, 5004, strObj);


                //Esco modalità Biglietteria
                strObj[0] = randomNum + Keyword;
                dirIO = posCommonFP.DirectIO(0, 5005, strObj);




                //1 sc 3 biglietti cinema
                //Scontrini con lotteria

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                fiscalprinter.EndFiscalReceipt(true);

                //Entro in modalità Biglietteria
                Keyword = "9167";
                randomNum = "56";
                for (int i = 0; i < 3; i++)
                {
                    strObj[0] = randomNum + Keyword;
                    dirIO = posCommonFP.DirectIO(0, 5001, strObj);
                    iData = dirIO.Data;
                    iObj = (string[])dirIO.Object;

                    //Apro Titolo di Accesso
                    strObj[0] = randomNum + Keyword;
                    dirIO = posCommonFP.DirectIO(0, 5002, strObj);

                    //Stampa Riga del Titolo
                    strObj[0] = randomNum + Keyword + "R" + "1" + "TEST BIGLIETTO CINEMA                   ";
                    dirIO = posCommonFP.DirectIO(0, 5003, strObj);

                    //Stampa Sigillo
                    strObj[0] = randomNum + Keyword + "S" + "1" + "xxxxxxxxxxxxxx SIGILLO                  ";
                    dirIO = posCommonFP.DirectIO(0, 5003, strObj);

                    //Chiudo Titolo di Accesso
                    strObj[0] = randomNum + Keyword;
                    dirIO = posCommonFP.DirectIO(0, 5004, strObj);


                    //Esco modalità Biglietteria
                    strObj[0] = randomNum + Keyword;
                    dirIO = posCommonFP.DirectIO(0, 5005, strObj);

                }

                //3 sc 1 cinema
                //Scontrini con lotteria
                for (int i = 0; i < 3; ++i)
                {
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                    dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                    fiscalprinter.EndFiscalReceipt(true);
                }

                //Entro in modalità Biglietteria
                Keyword = "9167";
                randomNum = "56";
                for (int i = 0; i < 1; i++)
                {
                    strObj[0] = randomNum + Keyword;
                    dirIO = posCommonFP.DirectIO(0, 5001, strObj);
                    iData = dirIO.Data;
                    iObj = (string[])dirIO.Object;

                    //Apro Titolo di Accesso
                    strObj[0] = randomNum + Keyword;
                    dirIO = posCommonFP.DirectIO(0, 5002, strObj);

                    //Stampa Riga del Titolo
                    strObj[0] = randomNum + Keyword + "R" + "1" + "TEST BIGLIETTO CINEMA                   ";
                    dirIO = posCommonFP.DirectIO(0, 5003, strObj);

                    //Stampa Sigillo
                    strObj[0] = randomNum + Keyword + "S" + "1" + "xxxxxxxxxxxxxx SIGILLO                  ";
                    dirIO = posCommonFP.DirectIO(0, 5003, strObj);

                    //Chiudo Titolo di Accesso
                    strObj[0] = randomNum + Keyword;
                    dirIO = posCommonFP.DirectIO(0, 5004, strObj);


                    //Esco modalità Biglietteria
                    strObj[0] = randomNum + Keyword;
                    dirIO = posCommonFP.DirectIO(0, 5005, strObj);

                }


                //Faccio chiusura 
                fiscalprinter.PrintZReport();

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;
        }




        //Crea 50 scontrini fiscali seguiti da 50 fatture e poi fa chiusura
        public int Create50Fiscal50Invoices()
        {
            log.Info("Performing CreateOneFiscal50Invoices Method ");
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;

                for (int i = 0; i < 50; ++i)
                {
                    //DirectIO 1089: Comando di apertura Fatture diretta
                    //Apertura Fattura 
                    strObj[0] = "0100000";
                    dirIO = PosCommonFP.DirectIO(0, 1089, strObj);
                    fiscalprinter.BeginFiscalReceipt(true);
                    // Vendita
                    fiscalprinter.PrintRecItem("Vendita tramite Fattura", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    // Pagamento
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    fiscalprinter.EndFiscalReceipt(true);
                }

                //Scontrino normale
                for (int i = 0; i <= 50; i++)
                {
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("Random Object No Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");

                    fiscalprinter.EndFiscalReceipt(true);
                }
                //Faccio chiusura 
                fiscalprinter.PrintZReport();


            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;
        }


        //Test Creato per verificare che funzionino i comandi dei resi e degli annulli
        //E' un test già fatto in passato ma a quanto pare alcuni clienti hanno un bug
        //sui resi , credo, principalmente e/o annulli.
        //E' probabile che sia dovuto al fatto che in mezzo ci sono fatture quindi per ora
        //mi credo un metodo che si prende in input : ZRep, StartReceipt, FinalReceipt
        //ossia , ovviamente, ZReport, scontrino iniziale e finale da analizzare
        //l'obiettivo è usare i comandi che testano se lo scontrino è annullabile o rendibile senza
        //annullarlo o renderlo realmente

        public int TestVoidableRefundableReceipts(string zrep, string start, string finish, string data)
        {
            try
            {
                log.Info("Performing TestVoidableRefundableReceipts");
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;

                string printerIdModel;
                string printerIdManufacturer;
                string printerIdNumber;
                ZReport();

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                printerIdModel = strData.Substring(0, 2);
                printerIdNumber = strData.Substring(4, 6);
                printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;
                /*
                string recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;
                string zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                int nextInt = Int32.Parse(zRepNum) + 1;
                zRepNum = nextInt.ToString("0000");
                */
                for (int i = Convert.ToInt32(start); i <= Convert.ToInt32(finish); i++)
                {
                    // Check document is returnable
                    log.Info("DirectIO (Check if Document can be Refunded)");
                    strObj[0] = "1" + printerId + data + i.ToString().PadLeft(4,'0') + zrep.PadLeft(4,'0');   // "1" = refund
                    dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                    iData = dirIO.Data;
                   
                    iObj = (string[])dirIO.Object;
                   
                    int iRet = Int32.Parse(iObj[0].Substring(0,1));
                    if (iRet == 0)
                        log.Info("Document can be Refunded");
                    else
                    {
                        if (iRet == 2)
                        {   log.Error("Document" + i.ToString() + "of ZRep: " + zrep + " can NOT be Refunded");
                            //throw new PosControlException();
                        }
                    }


                    strObj[0] = "2" + printerId + data + i.ToString().PadLeft(4, '0') + zrep.PadLeft(4, '0'); 	// "2" = void

                    dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                    iData = dirIO.Data;
                    iObj = (string[])dirIO.Object;
                    int iRet2 = Int32.Parse(iObj[0]);
                    if (iRet2 == 0)
                        log.Info("Document Voidable");
                    else
                        log.Error("Document NOT Voidable");

                }

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;

        }

  
        //Metodo che aggiunge descrizioni per i pagamenti solo ACCONTO, OMAGGIO E BUONO MONOUSO
        //DirectIO 108x
        public int SetDescriptionPayment()
        {
            try
            {
                log.Info("Performing SetDescriptionPayment");
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;

                string op = "01"; //Operator
                string descr = "blablabla"; //Description
                string modifierAmount = "999999999"; //ammontare associato al tipo di pagamento
                string modifierType = "00"; //00 = ACCONTO, 01 = OMAGGIO , 02 = BUONO MONOUSO
                string dep = "01"; //01-99
                string lr = "1"; //1 or 2 display first 20 char, display last char

                strObj[0] = op + descr + modifierAmount + modifierType + dep + lr;
                //TODO 140920 Da rieditare per aggiungere il comando quando verrà creato
                dirIO = posCommonFP.DirectIO(0, 1080, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1080)
                {
                    log.Error("Error DirecIO 1080 , tentativo fallito di utilizzare il comando di SetDescriptionPayment");
                    throw new Exception();
                }
                

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;

        }


        //DirectIO 3016
        //Set Configuration
        //TODO: controllare se e dove lo uso: lo uso per es x l' arrotondamento (index 27, Val 1 o 2 o 3)
        public int SetRetailHeaderLine()
        {
            try
            {
                log.Info("Performing SetRetailHeaderLine Method");
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                
                string appoggio = "IT07511580156".PadRight(40, ' ');
                strObj[0] = "98" + appoggio.PadRight(40, ' ');
                dirIO = posCommonFP.DirectIO(0, 3016, strObj);
                appoggio = "IT07511580156".PadRight(40, ' ');
                strObj[0] = "99" + appoggio.PadRight(40, ' ');
                dirIO = posCommonFP.DirectIO(0, 3016, strObj);
                appoggio = "IT07511580156".PadRight(40, ' ');
                strObj[0] = "01" + appoggio.PadRight(40, ' ');
                dirIO = posCommonFP.DirectIO(0, 3016, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error", e);
                }
            }
            return NumExceptions;
        }


        //Metodo che effettua il pagamento compreso le nuove forme 
        //DirectIO 1084
        public int PaymentCommand(string Description, string Amount, string PaymentType, string Index)
        {
            try
            {
                log.Info("Performing PaymentCommand");
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;

                string op = "01"; //Operator
                string descr = Description.PadRight(20,' '); //Description
                string amount = Amount.PadLeft(9, '0'); //ammontare associato al tipo di pagamento
                string paymentype = PaymentType; //0 cash , 1 cheque, 2 Credit or Credit Card, 3 Ticket, 4 Ticket with number, 5 No Paid (NON RISCOSSO)
                string index = Index.PadLeft(2, '0'); ; //Cheque no sense, Credit and Cash: 00 , Cash with Description 01-05 Credit card and ticket 01: 10 
                string lr = "1"; //1 or 2 display first 20 char, display last char

                strObj[0] = op + descr + amount + paymentype + index + lr;
                dirIO = posCommonFP.DirectIO(0, 1084, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1084)
                {
                    log.Error("Error DirecIO 1084 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }


            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;

        }

    }




    //Classe creata per la gestione , programmazione e recupero dati dai reparti
    public class VatManager : FiscalReceipt
    {

        //FiscalReceipt base class
        public VatManager()
        {
            try
            {
                /*
                posExplorer = new PosExplorer();
                // Console.WriteLine("Taking FiscalPrinter device ");
                DeviceInfo fp = posExplorer.GetDevice("FiscalPrinter", "FiscalPrinter1");

                posCommonFP = (PosCommon)posExplorer.CreateInstance(fp);
                //posCommonFP.StatusUpdateEvent += new StatusUpdateEventHandler(co_OnStatusUpdateEvent);
                
                */
                // Console.WriteLine("Initializing FiscalPrinter ");
                if (!opened)
                {
                    fiscalprinter = (FiscalPrinter)posCommonFP;
                    //Console.WriteLine("Performing Open() method ");
                    fiscalprinter.Open();

                    //Console.WriteLine("Performing Claim() method ");
                    fiscalprinter.Claim(1000);

                    //Console.WriteLine("Setting DeviceEnabled property ");
                    fiscalprinter.DeviceEnabled = true;

                    //Console.WriteLine("Performing ResetPrinter() method ");
                    //fiscalprinter.ResetPrinter();
                }

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Fatal("", pce);

                }
                else
                {
                    log.Error(e.ToString());
                    //log.Fatal("", e);

                }
            }
        }


        //DirectIO 4202
        //Get Department Parameters
        public void GetDepParam(string index)
        {
            log.Info("Performing GetDepParam() Method");
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                string depNumber = index.PadLeft(2, '0');  //da 01 a 99
                string description = "";
                string p1 = "";
                string p2 = "";
                string p3 = "";
                string single = "";
                string vatGroup = "";
                string priceLimit = "";
                string printGroup = "";
                string productGroup = "";
                string MU = ""; // Invoice unit of measure
                string DN; 


                // GET DEPARTMENT PARAMETER
                strObj[0] = depNumber;

                dirIO = posCommonFP.DirectIO(0, 4202, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;

                if ( iData != 4202)
                {
                    log.Error("Error DirectIO 4202 campo iData, expected 4202, received " + iData);
                    throw new Exception();
                }
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                DN = iObj[0].Substring(0, 2);
                if ( String.Compare(DN , depNumber) != 0)
                {
                    log.Error("Error DirectIO 4202 campo DN in response, expected " + depNumber + " received " + DN);
                    throw new Exception();
                }
                description = iObj[0].Substring(2, 20);
                //Console.WriteLine("description: " + description);

                p1 = iObj[0].Substring(22, 9);
                //Console.WriteLine("Unit price 1: " + p1);

                p2 = iObj[0].Substring(31, 9);
                //Console.WriteLine("Unit price 2: " + p2);

                p3 = iObj[0].Substring(40, 9);
                //Console.WriteLine("Unit price 3: " + p3);

                single = iObj[0].Substring(49, 1);

                vatGroup = iObj[0].Substring(50, 2);

                priceLimit = iObj[0].Substring(52, 9);

                printGroup = iObj[0].Substring(61, 2);

                productGroup = iObj[0].Substring(63, 2);

                MU = iObj[0].Substring(65, 2);

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    //log.Error(e.ToString());
                    log.Fatal("", e);

                }

            }
        }


        //DirectIO 4205
        //Legge l'aliquota IVA associata alla VAT Table Entry(1 to 9)
        public static string getVatTableEntry(string index)
        {
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                string N = index.PadLeft(2, '0'); //da 01 a 09, indica l'indice della table entry ( 2 bytes)
                string VAL = "";
                string nRet; //valore di ritorno corrispondente al parametro N


                // GET DEPARTMENT PARAMETER
                //Console.WriteLine("DirectIO (GET VAT TABLE ENTRY)");
                strObj[0] = N;

                dirIO = posCommonFP.DirectIO(0, 4205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;

                if (iData != 4205)
                {
                    log.Error("Error DirectIO 4205 campo iData, expected 4205, received " + iData);
                    throw new Exception();
                }
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                nRet = iObj[0].Substring(0, 2);

                if((String.Compare(nRet, N)) != 0)
                {
                    log.Error("Error DirectIO 4205 campo N, expected " + N + " received " + nRet);
                    throw new Exception();
                }

                VAL = iObj[0].Substring(2, 4);
                //Console.WriteLine("Index Table : {0} Vat rate : {1} ", N, VAL);

                return VAL;

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                     log.Error("", pce);
                    
                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Error("", e);

                }
                return NumExceptions.ToString();
            }

        }


        //DirectIO 4005
        //Scrive l'aliquota IVA associata alla VAT Table Entry(1 to 9 ovviamente,leggi directIO 4205)
        public void setVatTableEntry(string index, string vatRate)
        {
            try
            {
                log.Info("Performing setVatTableEntry Method");
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                string N = index.PadLeft(2, '0'); //da 01 a 09, indica l'indice della table entry (2 bytes)
                string VAL = vatRate.PadLeft(4, '0'); //da 0000 a 9999, 4 bytes,le prime due cifre rappresentano gli interi,le ultime due i decimali



                // GET DEPARTMENT PARAMETER
                //Console.WriteLine("DirectIO (GET DEPARTMENT PARAMETER)");
                strObj[0] = N + VAL;

                dirIO = posCommonFP.DirectIO(0, 4005, strObj);
                iData = dirIO.Data;
                
                iObj = (string[])dirIO.Object;
                

                
                

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

        }


        //10/09/2020TODO: aggiornata DirectIO 4002 con nuovi parametri
        //DirectIO 4002
        //Set Department Parameters
        public void setDepPar()
        {

            log.Info("Performing setDepPar() Method");

            string[] strObj = new string[1];
            DirectIOData dirIO;
            int iData;
            //int nextInt;
            string[] iObj = new string[1];
            //string strData = "";
            //string rtType = "";
            //string printerId = "";
            //string strDate = "";
            //string zRepNum = "";
            //string zRepNum_pre = "";
            //string recNum = "";
            //string recNum_pre = "";
            string rep_amount = "001000000";
            string num_rep = "01";
            string tax_index = "01";    // da zero e 19
            string rep_description = "SOFTWARE HOUSE      ";
            string vatIndex = "01"; //Per esempio da 00 a 59
            string pLIM = "999999999";
            string prnGPR = "00";
            string prodGPR = "00";
            string mu = "EU";
            string salesType = "0"; //00 = Goods , 01 = Service
            string salesAttribute = "00"; //00 = No sconto a reparto  , 01 = Sconto a reparto 
            string atecoIndex = "01"; //00-99 , ma che poi nella realtà saranno max 3 e tipo : Rep1 - Rep20 associati al Codice Ateco 1, 
            // Rep21 - Rep40 associati al Codice Ateco 2, Rep 41 - Rep 60 codice ateco 3

            try
            {
                ZReport();
                // SET DEPARTMENT PARAMETER con DirectIO 4002 aggiornata al 10/09/20
                num_rep = "01";
                //tax_index = "01";
                strObj[0] = num_rep + rep_description + rep_amount + rep_amount + rep_amount + "0" + vatIndex  + pLIM + prnGPR  + prodGPR + mu + salesType + salesAttribute + atecoIndex;
                dirIO = posCommonFP.DirectIO(0, 4002, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
              


                num_rep = "02";
                rep_description = "CASEIFICIO          ";
                salesType = "1"; //0 = Goods , 1 = Service
                vatIndex = "02";
                salesAttribute = "01";
                atecoIndex = "02";
                strObj[0] = num_rep + rep_description + rep_amount + rep_amount + rep_amount + "0" + vatIndex + pLIM + prnGPR + prodGPR + mu + salesType + salesAttribute + atecoIndex;
                dirIO = posCommonFP.DirectIO(0, 4002, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                num_rep = "03";
                rep_description = "ASTROLOGO           ";
                salesType = "1"; //0 = Goods , 1 = Service
                vatIndex = "03";
                salesAttribute = "01";
                atecoIndex = "03";
                strObj[0] = num_rep + rep_description + rep_amount + rep_amount + rep_amount + "0" + vatIndex + pLIM + prnGPR + prodGPR + mu + salesType + salesAttribute + atecoIndex;
                dirIO = posCommonFP.DirectIO(0, 4002, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                num_rep = "04";
                rep_description = "ASTROLOGO           ";
                salesType = "1"; //0 = Goods , 1 = Service
                vatIndex = "04";
                salesAttribute = "01";
                atecoIndex = "02";
                strObj[0] = num_rep + rep_description + rep_amount + rep_amount + rep_amount + "0" + vatIndex + pLIM + prnGPR + prodGPR + mu + salesType + salesAttribute + atecoIndex;
                dirIO = posCommonFP.DirectIO(0, 4002, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                num_rep = "05";
                tax_index = "00";
                strObj[0] = num_rep + rep_description + rep_amount + rep_amount + rep_amount + "0" + tax_index + rep_amount + "0000";
                dirIO = posCommonFP.DirectIO(0, 4002, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                num_rep = "06";
                tax_index = "10";
                strObj[0] = num_rep + rep_description + rep_amount + rep_amount + rep_amount + "0" + tax_index + rep_amount + "0000";
                dirIO = posCommonFP.DirectIO(0, 4002, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                num_rep = "07";
                tax_index = "11";
                strObj[0] = num_rep + rep_description + rep_amount + rep_amount + rep_amount + "0" + tax_index + rep_amount + "0000";
                dirIO = posCommonFP.DirectIO(0, 4002, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                num_rep = "08";
                tax_index = "12";
                strObj[0] = num_rep + rep_description + rep_amount + rep_amount + rep_amount + "0" + tax_index + rep_amount + "0000";
                dirIO = posCommonFP.DirectIO(0, 4002, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                num_rep = "09";
                tax_index = "13";
                strObj[0] = num_rep + rep_description + rep_amount + rep_amount + rep_amount + "0" + tax_index + rep_amount + "0000";
                dirIO = posCommonFP.DirectIO(0, 4002, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                num_rep = "10";
                tax_index = "14";
                strObj[0] = num_rep + rep_description + rep_amount + rep_amount + rep_amount + "0" + tax_index + rep_amount + "0000";
                dirIO = posCommonFP.DirectIO(0, 4002, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                // STAMPA ZREP
                Console.WriteLine("Performing PrintZReport() method ");
                fiscalprinter.PrintZReport();
                //System.Threading.Thread.Sleep(5000);


            }
            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

        }


        //20/10/2020TODO: aggiornata DirectIO 4002 con nuovi parametri
        //DirectIO 4002
        //Set Department Parameters Customizzato  ,si differisce da   perché programmo uno specifico reparto e basta
        public void setDepParEmbedded(string Num_rep, string AtecoIndex, string VatIndex, string SalesType,  string description)
        {

            log.Info("Performing setDepParEmbedded() Method");

            string[] strObj = new string[1];
            DirectIOData dirIO;
            int iData;
            //int nextInt;
            string[] iObj = new string[1];
           
            string rep_amount = "001000000";
            string num_rep = Num_rep.PadLeft(2,'0');
            
            string rep_description = description.PadRight(20,' ');
            string vatIndex = VatIndex.PadLeft(2,'0'); //Per esempio da 00 a 59
            string pLIM = "999999999";
            string prnGPR = "00";
            string prodGPR = "00";
            string mu = "EU";
            string salesType = SalesType; // "0"; 0 = Goods , 1 = Service
            string salesAttribute = "00"; //00 = Reparto con Sconto a Pagare No , 01 = Reparto con Sconto a pagare SI
            string atecoIndex = AtecoIndex.PadLeft(2,'0'); //00-99 , ma che poi nella realtà saranno max 3 e tipo : Rep1 - Rep20 associati al Codice Ateco 1, 
            // Rep21 - Rep40 associati al Codice Ateco 2, Rep 41 - Rep 60 codice ateco 3

            try
            {
                //ZReport();
                // SET DEPARTMENT PARAMETER con DirectIO 4002 aggiornata al 10/09/20
                
                
                strObj[0] = num_rep + rep_description + rep_amount + rep_amount + rep_amount + "0" + vatIndex + pLIM + prnGPR + prodGPR + mu + salesType + salesAttribute + atecoIndex;
                dirIO = posCommonFP.DirectIO(0, 4002, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                if (iData != 4002)
                {
                    log.Error("Errore risposta DirectIO 4002, expected 4002, received: " + iData.ToString());
                }


                //ZReport();
          
            }
            catch (Exception e)
            {
               
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

        }



    }

    //Classe che si occupa del Recupero dei veri totalizzatori giornalieri
    public class RetrieveData : FiscalReceipt
    {

        //FiscalReceipt base class
        public RetrieveData()
        {
            try
            {
                /*
                posExplorer = new PosExplorer();
                // Console.WriteLine("Taking FiscalPrinter device ");
                DeviceInfo fp = posExplorer.GetDevice("FiscalPrinter", "FiscalPrinter1");

                posCommonFP = (PosCommon)posExplorer.CreateInstance(fp);
                //posCommonFP.StatusUpdateEvent += new StatusUpdateEventHandler(co_OnStatusUpdateEvent);
                
                */
                if (!opened)
                {
                    fiscalprinter = (FiscalPrinter)posCommonFP;
                    //Console.WriteLine("Performing Open() method ");
                    fiscalprinter.Open();

                    //Console.WriteLine("Performing Claim() method ");
                    fiscalprinter.Claim(1000);

                    //Console.WriteLine("Setting DeviceEnabled property ");
                    fiscalprinter.DeviceEnabled = true;

                    //Console.WriteLine("Performing ResetPrinter() method ");
                    //fiscalprinter.ResetPrinter();
                }

                //Console.WriteLine("Performing ResetPrinter() method ");
                //fiscalprinter.ResetPrinter();


            }
            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {
                    Console.WriteLine(e.ToString());
                    log.Fatal("", e);
                }
            }
        }

        //DirectIO 1070
        //Get Fiscal Receipt
        public int getFiscalReceiptNumber(string Operator)
        {
            try
            {
                log.Info("Performing getFiscalReceiptNumber() Method ");
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                string op = Operator.PadLeft(2, '0');  //da 01 a 12
                string fiscalreceipt = "";
                string printercondition = "";

                // GET FISCAL RECEIPT
                //Console.WriteLine("DirectIO (GET FISCAL RECEIPT) 1070");
                strObj[0] = op;

                dirIO = posCommonFP.DirectIO(0, 1070, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                //Check risposta alla DirectIO 1070
                if (iData != 1070)
                {
                    log.Error("Error DirectIO 1070 , expected iData 1070, received " + iData);
                    throw new Exception();
                }
               

                fiscalreceipt = iObj[0].Substring(2, 4);
                //Console.WriteLine("fiscalreceipt: " + fiscalreceipt);
                printercondition = iObj[0].Substring(6, 1);
                if (Convert.ToInt32(printercondition) == 0)
                {
                    log.Info("The current printout is " + fiscalreceipt);
                    return Convert.ToInt32(fiscalreceipt);
                }
                else
                {
                    log.Info("The next printout is " + fiscalreceipt);
                    return (Convert.ToInt32(fiscalreceipt) + 1);
                }

                

            }
            catch (Exception e)
            {
                
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    
                    log.Fatal("", e);

                }

                return NumExceptions;
            }

        }



        //DirectIO 2050
        //Get Daily Data
        //Legge le statistiche giornaliere e i registri interni. e le scrive su un file di testo GETDAILYDATA.txt
        //Serialize Daily Data on txt file and also return FRCN = iObj[0].Substring(15, 9) substring
        public string  getDailyData(string index, string param = "")
        {
            log.Info("Perform getDailyData");
            System.IO.StreamWriter file = new System.IO.StreamWriter("GETDAILYDATA.txt", true);
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                string ind = index.PadLeft(2, '0');  //Index , 2 byte , chiamiamo il contatore 24, ossia Fiscal receipts, commercial documents and credit notes
                string par = param.PadRight(2, '1');
                string type = ""; //Due byte
                string FRCN = ""; // 9 byte, numero totale di oggetti
                strObj[0] = ind + par;

                /*
                // GET DAILY DATA (A seconda dell'index in input abbiamo uno dei daily stat or internal registers
                log.Info("DirectIO (GET FISCAL RECEIPT) 2050 index := " + index );
                if (String.Compare(ind, "19") == 0 || String.Compare(ind, "40") == 0 || String.Compare(ind, "41") == 0 || String.Compare(ind, "42") == 0)
                {
                    strObj[0] = ind + "01";
                }
                */

                dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                type = iObj[0].Substring(0, 2);
                //log.Info("Fiscal receipts and credit notes: " + type);
                FRCN = iObj[0].Substring(15, 9);
                //log.Info("Total number of fiscal receipts and credit notes: " + FRCN);

                
                switch (index)
                {
                    case "2": // Resi
                    {
                        string lines = "Index : " + index + " Resi " + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "5":   // Annullati
                    {
                        string lines = "Index : " + index + " Annullati " + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "7":   // Sconti percentuale
                    {
                        string lines = "Index : " + index + " Sconti percentuale " + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    //EDIT : Nuovi indici aggiunti per testare le forme di pagamento
                    case "9":   // CREDIT RECOVERIES IN CASH
                    {
                        string lines = "Index : " + index + " Totale Credit Recoveries Cash" + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "10":   // CASH IN TRANSACTIONS
                        {
                        string lines = "Index : " + index + " Totale Cash In Transaction" + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "11":   // CASH OUT TRANSACTIONS
                    {
                        string lines = "Index : " + index + " Totale Cash Out Transaction" + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "12":   // CASH OUT TRANSACTIONS
                    {
                        string lines = "Index : " + index + " Current Cash By Currency" + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "13":   // TOTAL CURRENT CASH
                    {
                        string lines = "Index : " + index + " Totale Pagamento Cash" + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "14":   // CREDIT RECOVERIES BY CHEQUE
                    {
                    string lines = "Index : " + index + " Totale Credit Recoveries By Cheque" + "\r" + "= : " + FRCN + "\r\n";

                    file.WriteLine(lines);
                    file.Close();
                    return FRCN;
                    break;
                    }
                    case "15":   // CHEQUE IN TRANSACTIONS
                    {
                    string lines = "Index : " + index + " Totale Cheque In Transactions" + "\r" + "= : " + FRCN + "\r\n";

                    file.WriteLine(lines);
                    file.Close();
                    return FRCN;
                    break;
                    }
                    case "16":   // CHEQUE OUT TRANSACTIONS
                    {
                    string lines = "Index : " + index + " Totale Cheque Out Transactions" + "\r" + "= : " + FRCN + "\r\n";

                    file.WriteLine(lines);
                    file.Close();
                    return FRCN;
                    break;
                    }
                    case "17":   // CURRENT CHEQUES
                    {
                    string lines = "Index : " + index + " Totale Current Cheques" + "\r" + "= : " + FRCN + "\r\n";

                    file.WriteLine(lines);
                    file.Close();
                    return FRCN;
                    break;
                    }
                    case "18":  // CREDIT AND CREDIT CARD PAYMENTS
                    {
                        if (Int32.Parse(param) == 0)
                        {
                            string lines = "Index : " + index + " Totale Pagamento Credito" + "\r" + "= : " + FRCN + "\r\n";
                            file.WriteLine(lines);
                        }
                        else
                        {
                            string lines = "Index : " + index + " Totale Pagamento Credit Card " + param + "\r" + "= : " + FRCN + "\r\n";
                            file.WriteLine(lines);
                        }

                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "19":  // TICKET PAYMENTS
                    {
                        string lines = "Index : " + index + " Totale Pagamento Ticket:= " + FRCN + "\r\n" + "Totale numero ticket: " + iObj[0].Substring(5,9);

                        file.WriteLine(lines);
                        file.Close();
                        if (String.Compare(param, "02") == 0) //gli passo 02 e allora voglio il  totale dei ticket
                            {
                                int temp = 0;
                               
                                for(int i = 1; i <= 10; i++)
                                {
                                    string[] obj = new string[1];
                                    string[] iobj = new string[1];
                                    iobj[0] = "19" + i.ToString().PadLeft(2, '0');
                                    obj = (string[])posCommonFP.DirectIO(0, 2050, iobj).Object;
                                    temp += Convert.ToInt32(obj[0].Substring(15,9));
                                }
                                return temp.ToString(); ; //Se gli passo 02 voglio il totale ticket
                        }
                        else //gli passo 01 e allora voglio il numero totale dei ticket
                        {
                                int temp = 0;

                                for (int i = 1; i <= 10; i++)
                                {
                                    string[] obj = new string[1];
                                    string[] iobj = new string[1];
                                    iobj[0] = "19" + i.ToString().PadLeft(2, '0');
                                    obj = (string[])posCommonFP.DirectIO(0, 2050, iobj).Object;
                                    temp += Convert.ToInt32(obj[0].Substring(5, 9));
                                }
                                return temp.ToString(); ; //Se gli passo 02 voglio il totale ticket
                            }
                        break;
                    }
                    case "21":  // CASH DRAWER OPENINGS
                    {
                        string lines = "Index : " + index + " Numero di Cash Drawer openings " + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;     
                        break;
                    }
                    case "24": // Fiscal Receipt
                    {
                        string lines = "Index : " + index + " Totale Fiscal Receipt " + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "25":  // Scontrini emessi memoria fiscale
                    {
                        string lines = "Index : " + index + " Scontrini emessi memoria fiscale " + "\r" + "= : " + iObj[0].Substring(0, 2) + "\r\n";
                        lines += "Total number of fiscal receipts and credit notes : " + "\r" + "= : " + iObj[0].Substring(15, 9) + "\r\n";


                        file.WriteLine(lines);
                        file.Close();
                        return iObj[0].Substring(0, 2);
                        break;
                    }
                    case "27":  // Chiusure giornaliere Z Report
                    {
                        string lines = "Index : " + index + " Numero chiusure giornaliere " + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "28":  // Totale giornaliero
                    {
                        string lines = "Index : " + index + " Totale giornaliero " + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "29": // CASH WITH DESCRIPTION PAYMENTS
                    {
                        string lines = "Index : " + index + " Totale Cash With Description Payments " + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "32":  // Grand Total e Numero chiusure giornaliere
                    {
                        string lines = "Index : " + index + " Grand Total " + "\r" + "= : " + iObj[0].Substring(0, 14) + "\r\n";

                        lines += "Credit Note Gran Total : " + "\r" + "= : " + iObj[0].Substring(14, 14) + "\r\n";
                        lines += "Numero chiusure giornaliere : " + "\r" + "= : " + iObj[0].Substring(28, 4) + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return iObj[0].Substring(28, 4);
                        break;
                    }
                    case "36":  // Totale Documenti Resi
                    {
                        string lines = "Index : " + index + " Totale Documenti Resi " + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "37":  // Totale Documenti Annnullati
                    {
                        string lines = "Index : " + index + " Totale Documenti Annnullati " + "\r" + "= : " + FRCN + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "38":  // Gran Totale Documenti Resi
                    {
                        string lines = "Index : " + index + " Gran Total " + "\r" + "= : " + iObj[0].Substring(0, 14) + "\r\n";
                        lines += "Gran Totale Documenti Resi : " + "\r" + "= : " + iObj[0].Substring(14, 14) + "\r\n";
                        lines += "Numero di chiusure giornaliere fiscali : " + "\r" + "= : " + iObj[0].Substring(28, 4) + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        switch (param)
                        {
                            case "00":
                                return iObj[0].Substring(0, 14);
                                break;
                            case "01":
                                return iObj[0].Substring(14, 14);
                                break;
                            case "02":
                                return iObj[0].Substring(28, 4);
                                break;
                            default:
                                return "";
                        }
                    }
                    case "39":   // Gran Totale Documenti Annulli
                    {
                        string lines = "Index : " + index + " Gran Total " + "\r" + "= : " + iObj[0].Substring(0, 14) + "\r\n";
                        lines += "Gran Totale Documenti Annulli : " + "\r" + "= : " + iObj[0].Substring(14, 14) + "\r\n";
                        lines += "Numero di chiusure giornaliere fiscali : " + "\r" + "= : " + iObj[0].Substring(28, 4) + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        switch (param)
                        {
                            case "00":
                                return iObj[0].Substring(0, 14);
                                break;
                            case "01":
                                return iObj[0].Substring(14, 14);
                                break;
                            case "02":
                                return iObj[0].Substring(28, 4);
                                break;
                            default:
                                return "";
                        }
                    }
                    case "40":   // Totale Vendite giornaliere
                    {
                        string output = "";
                        for (int i = 0; i < 20; ++i)
                        {
                            strObj[0] = ind + i.ToString().PadLeft(2, '0');
                            dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                            iObj = (string[])dirIO.Object;
                            FRCN = iObj[0].Substring(15, 9);
                            string lines = "Index : " + index + " Totale IVA giornaliera di vendita per indice IVA: " + "\r" + "= : " + FRCN + "\r" + " indice IVA : " + i.ToString() + "\r\n";
                            file.WriteLine(lines);
                            if (i == Int32.Parse(param))
                                output = iObj[0].Substring(5, 9) + FRCN;
                        }
                        file.Close();
                        return output;
                    }
                    case "41":  // Totale Resi Giornalieri
                    {
                        string output = "";
                        for (int i = 0; i < 20; ++i)
                        {
                            strObj[0] = ind + i.ToString().PadLeft(2, '0');
                            dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                            iObj = (string[])dirIO.Object;
                            FRCN = iObj[0].Substring(15, 9);
                            string lines = "Index : " + index + " Totale IVA Resi Giornalieri per indice IVA: " + "\r" + "= : " + FRCN + "\r" + " indice IVA : " + i.ToString() + "\r\n";
                            file.WriteLine(lines);
                            if (i == Int32.Parse(param))
                                output = FRCN;
                        }
                        file.Close();
                        return output;

                    }
                    case "42":  // Totale Annulli Giornalieri
                    {
                        string output = "";
                        for (int i = 0; i < 20; ++i)
                        {
                            strObj[0] = ind + i.ToString().PadLeft(2, '0');
                            dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                            iObj = (string[])dirIO.Object;
                            FRCN = iObj[0];
                            string lines = "Index : " + index + "Totale IVA di vendite annullate per indice IVA: " + "\r" + "= : " + FRCN + "\r" + " indice IVA : " + i.ToString() + "\r\n";
                            file.WriteLine(lines);
                            if (i == Int32.Parse(param))
                                output = FRCN;
                        }
                        file.Close();
                        return output;
                    }
                    case "43":  // Totale Netti Giornalieri (40-41-42)
                    {
                        string output = "";
                        for (int i = 0; i < 20; ++i)
                        {
                            strObj[0] = ind + i.ToString().PadLeft(2, '0');
                            dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                            iObj = (string[])dirIO.Object;
                            FRCN = iObj[0].Substring(15, 9);
                            string lines = "Index : " + index + "Totale IVA di (Vendite - Resi - Annulli) per indice IVA: " + "\r" + "= : " + FRCN + "\r" + " indice IVA : " + i.ToString() + "\r\n";
                            file.WriteLine(lines);
                            if (i == Int32.Parse(param))
                                output = FRCN;
                        }
                        file.Close();
                        return output;
                    }
                    case "61":  // Primo e Ultimo scontrino giornaliero
                    {
                        string lines = "Index : " + index + " Prima e Ultima FATTURA giornaliera " + "\r" + "= : " + "\r\n";
                        lines += "Prima fattura giornaliera : " + "\r" + "= : " + iObj[0].Substring(5, 9) + "\r\n";
                        lines += "Ultima fattura giornaliera : " + "\r" + "= : " + iObj[0].Substring(15, 9) + "\r\n";

                        file.WriteLine(lines);
                        file.Close();
                        if (Int32.Parse(param) == 0)
                            return iObj[0].Substring(5, 9);
                        else return iObj[0].Substring(15, 9);
                    }     
                    case "70":  // DailyAcconti
                    {
                        int temp = 0;
                        for (int i = 1; i < 100; i++)
                        {
                            strObj[0] = "70" + i.ToString().PadLeft(2,'0');

                            dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                            iData = dirIO.Data;

                            iObj = (string[])dirIO.Object;
                            type = iObj[0].Substring(0, 2);
                            //log.Info("Fiscal receipts and credit notes: " + type);
                            FRCN = iObj[0].Substring(15, 9);
                            temp += Convert.ToInt32(FRCN);
                        }
                        string lines = "Index : " + index + " DailyAcconti   " + "\r" + "= : " + temp.ToString() + "\r\n";
                        file.WriteLine(lines);
                        file.Close();
                        return temp.ToString();
                        break;
                    }
                    case "71":  // DailyNonRiscossoOmaggio
                    {
                        int temp = 0;
                        for (int i = 1; i < 100; i++)
                        {
                            strObj[0] = "71" + i.ToString().PadLeft(2, '0');

                            dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                            iData = dirIO.Data;

                            iObj = (string[])dirIO.Object;
                            type = iObj[0].Substring(0, 2);
                            //log.Info("Fiscal receipts and credit notes: " + type);
                            FRCN = iObj[0].Substring(15, 9);
                            temp += Convert.ToInt32(FRCN);
                        }
                        string lines = "Index : " + index + " DailyNonRiscossoOmaggio   " + "\r" + "= : " + temp.ToString() + "\r\n";
                        file.WriteLine(lines);
                        file.Close();
                        return temp.ToString();
                        break;
                    }
                    case "72":  // DailyBuonoMonouso
                    {
                        int temp = 0;
                        for (int i = 1; i < 100; i++)
                        {
                            strObj[0] = "72" + i.ToString().PadLeft(2, '0');

                            dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                            iData = dirIO.Data;

                            iObj = (string[])dirIO.Object;
                            type = iObj[0].Substring(0, 2);
                            //log.Info("Fiscal receipts and credit notes: " + type);
                            FRCN = iObj[0].Substring(15, 9);
                            temp += Convert.ToInt32(FRCN);
                        }
                        string lines = "Index : " + index + " DailyBuonoMonouso   " + "\r" + "= : " + temp.ToString() + "\r\n";
                        file.WriteLine(lines);
                        file.Close();
                        return temp.ToString();
                        break;
                    }
                    case "73":  // DailyNonRiscossoBeniServizi
                    {
                       
                        strObj[0] = "73" + "01";

                        dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                        iData = dirIO.Data;

                        iObj = (string[])dirIO.Object;
                        type = iObj[0].Substring(0, 2);
                        //log.Info("Fiscal receipts and credit notes: " + type);
                        FRCN = iObj[0].Substring(15, 9);
                        
                       
                        string lines = "Index : " + index + " DailyNonRiscossoBeniServizi   " + "\r" + "= : " + FRCN + "\r\n";
                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "74":  // DailyNonRiscossoBeni
                        {
                            
                           
                            strObj[0] = "74" + "01";

                            dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                            iData = dirIO.Data;

                            iObj = (string[])dirIO.Object;
                            type = iObj[0].Substring(0, 2);
                            //log.Info("Fiscal receipts and credit notes: " + type);
                            FRCN = iObj[0].Substring(15, 9);
                                
                          
                            string lines = "Index : " + index + " DailyNonRiscossoBeni   " + "\r" + "= : " + FRCN + "\r\n";
                            file.WriteLine(lines);
                            file.Close();
                            return FRCN;
                            break;
                        }
                    case "75":  // DailyNonRiscossoServizi
                    {
                        
                        strObj[0] = "75" + "01";

                        dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                        iData = dirIO.Data;

                        iObj = (string[])dirIO.Object;
                        type = iObj[0].Substring(0, 2);
                        //log.Info("Fiscal receipts and credit notes: " + type);
                        FRCN = iObj[0].Substring(15, 9);
                            
                       
                        string lines = "Index : " + index + " DailyNonRiscossoServizi   " + "\r" + "= : " + FRCN + "\r\n";
                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "76":  // DailyNonRiscossoFatture 
                    {
                        
                        strObj[0] = "76" + "01";

                        dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                        iData = dirIO.Data;

                        iObj = (string[])dirIO.Object;
                        type = iObj[0].Substring(0, 2);
                        //log.Info("Fiscal receipts and credit notes: " + type);
                        FRCN = iObj[0].Substring(15, 9);
                       
                        string lines = "Index : " + index + " DailyNonRiscossoFatture   " + "\r" + "= : " + FRCN + "\r\n";
                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                   
                    case "78":  // DailyNonRiscossoDaSSN   
                    {
                         strObj[0] = "78" + "01";

                        dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                        iData = dirIO.Data;

                        iObj = (string[])dirIO.Object;
                        type = iObj[0].Substring(0, 2);
                        //log.Info("Fiscal receipts and credit notes: " + type);
                        FRCN = iObj[0].Substring(15, 9);
                    
                        string lines = "Index : " + index + " DailyNonRiscossoDaSSN   " + "\r" + "= : " + FRCN + "\r\n";
                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "79":  // ScontoAPagare        
                    {
                        
                        strObj[0] = "79" + "01";

                        dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                        iData = dirIO.Data;

                        iObj = (string[])dirIO.Object;
                        type = iObj[0].Substring(0, 2);
                        //log.Info("Fiscal receipts and credit notes: " + type);
                        FRCN = iObj[0].Substring(15, 9);
             
                        string lines = "Index : " + index + " ScontoAPagare   " + "\r" + "= : " + FRCN + "\r\n";
                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "80":  // DailyBuonoMultiuso        
                    {
                        
                        strObj[0] = "80" + "01";

                        dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                        iData = dirIO.Data;

                        iObj = (string[])dirIO.Object;
                        type = iObj[0].Substring(0, 2);
                        //log.Info("Fiscal receipts and credit notes: " + type);
                        FRCN = iObj[0].Substring(15, 9);
                           
                     
                        string lines = "Index : " + index + " DailyBuonoMultiuso   " + "\r" + "= : " + FRCN + "\r\n";
                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    case "81":  // DailyRoundingNegativo
                        {
                           
                            strObj[0] = "81" + "01";

                            dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                            iData = dirIO.Data;

                            iObj = (string[])dirIO.Object;
                            type = iObj[0].Substring(0, 2);
                            //log.Info("Fiscal receipts and credit notes: " + type);
                            FRCN = iObj[0].Substring(15, 9);
                               
                            string lines = "Index : " + index + " DailyRoundingNegativo   " + "\r" + "= : " + FRCN + "\r\n";
                            file.WriteLine(lines);
                            file.Close();
                            return FRCN;
                            break;
                        }
                    case "82":  // DailyRoundingPositivo
                    {
                       
                        strObj[0] = "82" + "01";

                        dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                        iData = dirIO.Data;

                        iObj = (string[])dirIO.Object;
                        type = iObj[0].Substring(0, 2);
                        //log.Info("Fiscal receipts and credit notes: " + type);
                        FRCN = iObj[0].Substring(15, 9);
                           
                        string lines = "Index : " + index + " DailyRoundingPositivo   " + "\r" + "= : " + FRCN + "\r\n";
                        file.WriteLine(lines);
                        file.Close();
                        return FRCN;
                        break;
                    }
                    default:
                        return "";
                }
            }
            catch (Exception e)
            {
                //log.Info("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Error("", pce);
                }
                else
                {
                    file.Flush();
                    file.Close();
                    log.Error("", e);
                }
                return "";
            }

        }




    }

    public class VatRecord
    {
        public string TotalizerType { get; set; }
        public string VatRate { get; set; }
        public string Item { get; set; }
        public string Net { get; set; }



        //ottiene le info sui contatori in memoria relativi alle aliquote IVA (lordo e netto)
        public static VatRecord[] SetVatCounter()
        {
            //Creazione Array di record di 18 locazioni, 9 per l'IVA giornaliera e 9 per l'IVA totale
            VatRecord[] records = new VatRecord[18];
            for (int i = 0; i < 18; ++i)
            {
                records[i] = new VatRecord { TotalizerType = "" };
            }

            try
            {
                /*
                // Write the string to a file in append mode
                System.IO.StreamWriter file = new System.IO.StreamWriter("counters.txt", true);
                string lines = "";

                
                Console.WriteLine("Performing testGetData method ");
                Console.WriteLine("CheckHealthText " + FiscalReceipt.fiscalprinter.CheckHealthText);
                FiscalReceipt.fiscalprinter.CheckTotal = true;

                //GetData Method Test
                //Sarebbe il SUBtotale dello scontrino corrente
                //Console.WriteLine("FPTR_GD_CURRENT_TOTAL (1) " + fiscalprinter.GetData(FiscalData.CurrentTotal, (int)0).Data);

                Console.WriteLine("FPTR_GD_DAILY_TOTAL (2) " + FiscalReceipt.fiscalprinter.GetData(FiscalData.DailyTotal, (int)0).Data);
                lines += "FiscalData.DailyTotal = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.DailyTotal, (int)0).Data + "\r\n";

                Console.WriteLine("FPTR_GD_RECEIPT_NUMBER (3) " + FiscalReceipt.fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data);
                lines += "FiscalData.ReceiptNumber = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data + "\r\n";

                Console.WriteLine("FPTR_GD_REFUND (4) " + FiscalReceipt.fiscalprinter.GetData(FiscalData.Refund, (int)0).Data);
                lines += "FiscalData.Refund = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.Refund, (int)0).Data + "\r\n";

                //fiscalprinter.ResetPrinter();
                //fiscalprinter.PrintXReport();
                //fiscalprinter.PrintZReport();

                //Indica il totale da pagare dello scontrino CORRENTE
                //Console.WriteLine("FPTR_GD_NOT_PAID (5) " + fiscalprinter.GetData(FiscalData.NotPaid, (int)0).Data);
                //lines += "FiscalData.NotPaid = " + fiscalprinter.GetData(FiscalData.NotPaid, (int)0).Data + "\n";

                Console.WriteLine("FPTR_GD_MID_VOID (6) " + FiscalReceipt.fiscalprinter.GetData(FiscalData.NumberOfVoidedReceipts, (int)0).Data);
                lines += "FiscalData.NumberOfVoidedReceipts = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.NumberOfVoidedReceipts, (int)0).Data + "\n";

                Console.WriteLine("FPTR_GD_Z_REPORT (7) " + FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data);
                lines += "FiscalData.ZReport = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data + "\r\n";

                Console.WriteLine("FPTR_GD_GRAND_TOTAL (8) " + FiscalReceipt.fiscalprinter.GetData(FiscalData.GrandTotal, (int)0).Data);
                lines += "FiscalData.GrandTotal = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.GrandTotal, (int)0).Data + "\r\n";

                //Necessario per la successiva istruzione
                //fiscalprinter.EndFiscalReceipt(false);

                Console.WriteLine("FPTR_GD_FISCAL_REC \n Indicates the number of fiscal receipts / commercial documents of the day." +
                    " \n Increased after the fiscal receipt has been closed. " +
                    "\n Cleared after printing the daily closure. (20) " +
                    "\n " + FiscalReceipt.fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data);
                lines += "FiscalData.FiscalReceipt = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data + "\r\n";

                Console.WriteLine("FPTR_GD_FISCAL_REC_VOID \n Indicates the number of canceled fiscal receipts / commercial documents of the day." +
                    " \n Increased after void the fiscal receipt. Cleared after printing the daily closure. \n " +
                    "Any cancellation documents issued do not changes this total. (21) " +
                    "\n " + FiscalReceipt.fiscalprinter.GetData(FiscalData.FiscalReceiptVoid, (int)0).Data);
                lines += "FiscalData.FiscalReceiptVoid = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.FiscalReceiptVoid, (int)0).Data + "\r\n";

                file.WriteLine(lines);


                //fiscalprinter.PrintXReport();
                //fiscalprinter.PrintZReport();

                */
                

                //Recupero di tutti i contatori dalla GetTotalizer per ogni tipo di TotalizerType 
                //possibile che possiamo selezionare (FiscalTotalizerType enum). Sono 2 tipi, Day e Grand
                //Console.WriteLine("Fiscalprinter.TotalizerType = " + FiscalReceipt.fiscalprinter.TotalizerType);


                for (int j = 2, x = 0; j < 5; j = j + 2, x = x + 9)
                {



                    FiscalReceipt.fiscalprinter.TotalizerType = (Microsoft.PointOfService.FiscalTotalizerType)j;
                    //lines = "fiscalprinter.TotalizerType = " + FiscalReceipt.fiscalprinter.TotalizerType + "\r\n";
                    //file.WriteLine(lines);

                    for (int i = 1; i < 10; ++i)
                    {

                        string liness = "";

                        //int vat = fiscalprinter.GetVatEntry(i, 0);

                        // me la dava già la fiscal printer ma l'ho implementata anche io con la directIO
                        string vat = VatManager.getVatTableEntry(i.ToString());
                        liness += "Vat Rate = " + vat + "\r\n";

                        //Console.WriteLine(FiscalReceipt.fiscalprinter.GetTotalizer(i, FiscalTotalizer.Item) + "\r\n");
                        liness += "FiscalTotalizer.Item = " + FiscalReceipt.fiscalprinter.GetTotalizer(i, FiscalTotalizer.Item) + "\r\n";

                        //Useless perchè all'interno dello scontrino corrente
                        //Console.WriteLine(fiscalprinter.GetTotalizer(i, FiscalTotalizer.Discount) + "\r\n");
                        //liness += "FiscalTotalizer.Discount = " + fiscalprinter.GetTotalizer(i, FiscalTotalizer.Discount) + "\r\n";

                        //Useless perchè all'interno dello scontrino corrente
                        //Console.WriteLine(fiscalprinter.GetTotalizer(i, FiscalTotalizer.Refund) + "\r\n");
                        //liness += "FiscalTotalizer.Refund = " + fiscalprinter.GetTotalizer(i, FiscalTotalizer.Refund) + "\r\n";

                        //Useless perchè all'interno dello scontrino corrente
                        //Console.WriteLine(fiscalprinter.GetTotalizer(i, FiscalTotalizer.ItemVoid) + "\r\n");
                        //liness += "FiscalTotalizer.ItemVoid = " + fiscalprinter.GetTotalizer(i, FiscalTotalizer.ItemVoid) + "\r\n";

                        //Useless perchè all'interno dello scontrino corrente
                        //Console.WriteLine(fiscalprinter.GetTotalizer(i, FiscalTotalizer.Gross) + "\r\n");
                        //liness += "FiscalTotalizer.Gross = " + fiscalprinter.GetTotalizer(i, FiscalTotalizer.Gross) + "\r\n";

                        //Console.WriteLine(FiscalReceipt.fiscalprinter.GetTotalizer(i, FiscalTotalizer.Net) + "\r\n");
                        liness += "FiscalTotalizer.Net = " + FiscalReceipt.fiscalprinter.GetTotalizer(i, FiscalTotalizer.Net) + "\r\n";


                        //file.WriteLine(liness);


                        records[i - 1 + x] = new VatRecord
                        {
                            TotalizerType = FiscalReceipt.fiscalprinter.TotalizerType.ToString(),
                            VatRate = vat.ToString(),
                            Item = FiscalReceipt.fiscalprinter.GetTotalizer(i, FiscalTotalizer.Item),
                            Net = FiscalReceipt.fiscalprinter.GetTotalizer(i, FiscalTotalizer.Net)
                        };



                    }
                }
                //file.Close();


            }
            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                FiscalReceipt.NumExceptions++;
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    throw;
                }
                else
                {
                    Console.WriteLine(e.ToString());
                }

                return records;
            }
            return records;

        }

        //Serializza SetVatCounters to xml file
        public void SetVatCounters()
        {
            XDocument xmlDocument = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),

                new XComment("Creating an XML Tree all daily and grand/IVA counters... "),

                new XElement("Records",

                    from VatRecord in VatRecord.SetVatCounter()
                    select new XElement("Counter", new XAttribute("TotalizerType", VatRecord.TotalizerType),
                                new XElement("VatRate", VatRecord.VatRate),
                                new XElement("Item", VatRecord.Item),
                                new XElement("Net", VatRecord.Net))
                            ));

            xmlDocument.Save(@"VatCounters.xml");
            //Set general counter to xml file
            GeneralCounter.SetGeneralCounter();
        }

        //effettua una query sull'XML dei Vat Counter per cercare il dato esplicitamente richiesto
        public static string GetVatCounter(string TotType, string Vat, string data)
        {
            IEnumerable<string> Items = from Counter in XDocument.Load(@"VatCounters.xml")
                                                                                     .Descendants("Counter")

                                        where (string)Counter.Attribute("TotalizerType") == TotType &&
                                              (string)Counter.Element("VatRate") == Vat
                                        orderby (string)Counter.Element("VatRate") descending
                                        select Counter.Element(data).Value;

            /*
            foreach (string item in Items)
            {
                //Console.WriteLine(item);
            }
            */
            return Items.ElementAt(0);
        }

        public override string ToString()
        {
            string line = "";
            line += ("TotalizerType " + TotalizerType + "\r\n");
            line += ("VatRate " + VatRate + "\r\n");
            line += ("Item " + Item + "\r\n");
            line += ("Net " + Net + "\r\n");
            return line;
        }
    }


    //TODO: 15092020: Upgrade di questa classe con le nuove forme di pagamenti e quindi i relativi totalizzatori che verranno testati (come i vecchi)
    //Classe creata per recuperare i dati relativi ai contatori generali
    [Serializable]
    public class GeneralCounter : FiscalReceipt, ISerializable
    {
        public string DailyTotal { get; set; }
        public string ReceiptNumber { get; set; }
        public string Refund { get; set; }
        public string ZRep { get; set; }
        public string GrandTotal { get; set; }
        public string FiscalRec { get; set; }
        public string FiscalRecVoid { get; set; }
        public string DailyTotalNumberTicket { get; set; }
        public string DailyNonRiscossoBeniServizi { get;  set; }
        public string DailyTotalTicket { get; set; }
        public string DailyNonRiscossoServizi { get; set; }
        public string DailyNonRiscossoFatture { get; set; }
        public string DailyNonRiscossoDaSSN { get; set; }
        public string DailyNonRiscossoOmaggio { get; set; }
        public string ScontoAPagare { get; set; }
        public string DailyBuonoMonouso { get; set; }
        public string DailyBuonoMultiuso { get; set; }
        public string DailyAcconti{ get; set; }
        public string DailyNonRiscossoBeni { get; set; }
        public string DailyRoundingNegativo { get; set; }
        public string DailyRoundingPositivo { get; set; }
        //Default constructor needed for serialize class
        public GeneralCounter() { }

        public GeneralCounter(string dailyTotal, string receiptNumber, string refund, string zReport, string grandTotal, string fiscalRec, string fiscalReceiptVoid, string dailyTotalNumberTicket, string dailyTotalTicket, string dailyNonRiscossoServizi, string dailyNonRiscossoFatture, string dailyNonRiscossoDaSSN, string dailyNonRiscossoOmaggio, string scontoAPagare, string dailyBuonoMonouso, string dailyBuonoMultiuso, string dailyAcconti, string dailyNonRiscossoBeni , string dailyRoundingNegativo , string dailyRoundingPositivo )
        {
            DailyTotal = dailyTotal;
            ReceiptNumber = receiptNumber;
            Refund = refund;
            ZRep = zReport;
            GrandTotal = grandTotal;
            FiscalRec = fiscalRec;
            FiscalRecVoid = fiscalReceiptVoid;
            DailyTotalNumberTicket = dailyTotalNumberTicket;
            DailyTotalTicket = dailyTotalTicket;
            DailyNonRiscossoServizi = dailyNonRiscossoServizi;
            DailyNonRiscossoBeni = dailyNonRiscossoBeni;
            DailyNonRiscossoFatture = dailyNonRiscossoFatture;
            DailyNonRiscossoDaSSN = dailyNonRiscossoDaSSN;
            DailyNonRiscossoOmaggio = dailyNonRiscossoOmaggio;
            ScontoAPagare = scontoAPagare;
            DailyBuonoMonouso = dailyBuonoMonouso;
            DailyBuonoMultiuso = dailyBuonoMultiuso;
            DailyAcconti = dailyAcconti;
            DailyRoundingNegativo = dailyRoundingNegativo;
            DailyRoundingPositivo = dailyRoundingPositivo;

        }

        public override string ToString()
        {
            string line = "";
            line += ("DailyTotal " + DailyTotal + "\r\n");
            line += ("ReceiptNumber " +ReceiptNumber + "\r\n");
            line += ("Refund " + Refund + "\r\n");
            line += ("ZReport " + ZRep + "\r\n");
            line += ("GrandTotal " + GrandTotal + "\r\n");
            line += ("FiscalRec " + FiscalRec + "\r\n");
            line += ("FiscalRecVoid " + FiscalRecVoid + "\r\n");
            line += ("DailyTotalNumberTicket " + DailyTotalNumberTicket + "\r\n");
            line += ("DailyTotalTicket " + DailyTotalTicket + "\r\n");         
            line += ("DailyNonRiscossoServizi " + DailyNonRiscossoServizi + "\r\n");
            line += ("DailyNonRiscossoBeni " + DailyNonRiscossoBeni + "\r\n");
            line += ("DailyNonRiscossoFatture " + DailyNonRiscossoFatture + "\r\n");
            line += ("DailyNonRiscossoDaSSN " + DailyNonRiscossoDaSSN + "\r\n");
            line += ("DailyNonRiscossoOmaggio " + DailyNonRiscossoOmaggio + "\r\n");
            line += ("ScontoAPagare " + ScontoAPagare + "\r\n");
            line += ("DailyBuonoMonouso " + DailyBuonoMonouso + "\r\n");
            line += ("DailyBuonoMultiuso " + DailyBuonoMultiuso + "\r\n");
            line += ("DailyAcconti " + DailyAcconti + "\r\n");
            line += ("DailyRoundingNegativo " + DailyRoundingNegativo + "\r\n");
            line += ("DailyRoundingPositivo " + DailyRoundingPositivo + "\r\n");
            return line;
        }

        // Serialization function (Stores Object Data in File)
        // SerializationInfo holds the key value pairs
        // StreamingContext can hold additional info
        // but we aren't using it here
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Assign key value pair for your data
            info.AddValue("DailyTotal", DailyTotal);
            info.AddValue("ReceiptNumber", ReceiptNumber);
            info.AddValue("Refund", Refund);
            info.AddValue("ZReport", ZRep);
            info.AddValue("GrandTotal", GrandTotal);
            info.AddValue("FiscalRec", FiscalRec);
            info.AddValue("FiscalRecVoid", FiscalRecVoid);
            info.AddValue("DailyTotalTicket", DailyTotalTicket);
            info.AddValue("DailyTotalNumberTicket", DailyTotalNumberTicket);
            info.AddValue("DailyNonRiscossoBeni", DailyNonRiscossoBeni);
            info.AddValue("DailyNonRiscossoServizi", DailyNonRiscossoServizi);
            info.AddValue("DailyNonRiscossoFatture", DailyNonRiscossoFatture);
            info.AddValue("DailyNonRiscossoDaSSN", DailyNonRiscossoDaSSN);
            info.AddValue("DailyNonRiscossoOmaggio", DailyNonRiscossoOmaggio);
            info.AddValue("ScontoAPagare", ScontoAPagare);
            info.AddValue("DailyBuonoMonouso", DailyBuonoMonouso);
            info.AddValue("DailyBuonoMultiuso", DailyBuonoMultiuso);
            info.AddValue("DailyAcconti", DailyAcconti);
            info.AddValue("DailyNonRiscossoBeni", DailyNonRiscossoBeni);
            info.AddValue("DailyRoundingNegativo", DailyRoundingNegativo);
            info.AddValue("DailyRoundingPositivo", DailyRoundingPositivo);

        }

        // The deserialize function (Removes Object Data from File)
        public GeneralCounter(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the properties
            DailyTotal = (string)info.GetValue("DailyTotal", typeof(string));
            ReceiptNumber = (string)info.GetValue("ReceiptNumber", typeof(string));
            Refund = (string)info.GetValue("Refund", typeof(string));
            ZRep = (string)info.GetValue("ZReport", typeof(string));
            GrandTotal = (string)info.GetValue("GrandTotal", typeof(string));
            FiscalRec = (string)info.GetValue("FiscalRec", typeof(string));
            FiscalRecVoid = (string)info.GetValue("FiscalRecVoid", typeof(string));
            DailyTotalTicket = (string)info.GetValue("DailyTotalTicket", typeof(string));
            DailyTotalNumberTicket = (string)info.GetValue("DailyTotalNumberTicket", typeof(string));
            DailyNonRiscossoBeniServizi = (string)info.GetValue("DailyNonRiscossoBeniServizi", typeof(string));
            DailyNonRiscossoBeni = (string)info.GetValue("DailyNonRiscossoBeni", typeof(string));
            DailyNonRiscossoServizi = (string)info.GetValue("DailyNonRiscossoServizi", typeof(string));
            DailyNonRiscossoFatture = (string)info.GetValue("DailyNonRiscossoFatture", typeof(string));
            DailyNonRiscossoDaSSN = (string)info.GetValue("DailyNonRiscossoDaSSN", typeof(string));
            DailyNonRiscossoOmaggio = (string)info.GetValue("DailyNonRiscossoOmaggio", typeof(string));
            ScontoAPagare = (string)info.GetValue("ScontoAPagare", typeof(string));
            DailyBuonoMonouso = (string)info.GetValue("DailyBuonoMonouso", typeof(string));
            DailyBuonoMultiuso = (string)info.GetValue("DailyBuonoMultiuso", typeof(string));
            DailyAcconti = (string)info.GetValue("DailyAcconti", typeof(string));
            DailyNonRiscossoBeni = (string)info.GetValue("DailyNonRiscossoBeni", typeof(string));
            DailyRoundingNegativo = (string)info.GetValue("DailyRoundingNegativo", typeof(string));
            DailyRoundingPositivo = (string)info.GetValue("DailyRoundingPositivo", typeof(string));


        }

        // Deserialize from XML to the object
        public static GeneralCounter GetGeneralCounter()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(GeneralCounter));
            TextReader reader = new StreamReader("GeneralCounter.xml");
            object obj = deserializer.Deserialize(reader);
            GeneralCounter gc = (GeneralCounter)obj;
            reader.Close();
            return gc;
        }





        //Metodo che aggiorna i contatori generali prelevando i dati dalla memoria fiscale tramite il driver .NET e li serializza su GeneralCounter.xml file
        public static void SetGeneralCounter(string reparto ="01")
        {

            GeneralCounter generalcounter = new GeneralCounter();

            RetrieveData rData = new RetrieveData();
            string rep = reparto.PadLeft(2, '0');  //default reparto 01
            //string lines = "";

            //GetData Method Test
            //Sarebbe il SUBtotale dello scontrino corrente
            //Console.WriteLine("FPTR_GD_CURRENT_TOTAL (1) " + fiscalprinter.GetData(FiscalData.CurrentTotal, (int)0).Data);

            //Console.WriteLine("FPTR_GD_DAILY_TOTAL (2) " + FiscalReceipt.fiscalprinter.GetData(FiscalData.DailyTotal, (int)0).Data);
            //lines += "FiscalData.DailyTotal = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.DailyTotal, (int)0).Data + "\r\n";
            generalcounter.DailyTotal = FiscalReceipt.fiscalprinter.GetData(FiscalData.DailyTotal, (int)0).Data;

            //Console.WriteLine("FPTR_GD_RECEIPT_NUMBER (3) " + FiscalReceipt.fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data);
            //lines += "FiscalData.ReceiptNumber = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data + "\r\n";
            generalcounter.ReceiptNumber = FiscalReceipt.fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data;

            //Console.WriteLine("FPTR_GD_REFUND (4) " + FiscalReceipt.fiscalprinter.GetData(FiscalData.Refund, (int)0).Data);
            //lines += "FiscalData.Refund = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.Refund, (int)0).Data + "\r\n";
            generalcounter.Refund = FiscalReceipt.fiscalprinter.GetData(FiscalData.Refund, (int)0).Data;

            //fiscalprinter.ResetPrinter();
            //fiscalprinter.PrintXReport();
            //fiscalprinter.PrintZReport();

            //Indica il totale da pagare dello scontrino CORRENTE
            //Console.WriteLine("FPTR_GD_NOT_PAID (5) " + fiscalprinter.GetData(FiscalData.NotPaid, (int)0).Data);
            //lines += "FiscalData.NotPaid = " + fiscalprinter.GetData(FiscalData.NotPaid, (int)0).Data + "\n";

            //Console.WriteLine("FPTR_GD_MID_VOID (6) " + fiscalprinter.GetData(FiscalData.NumberOfVoidedReceipts, (int)0).Data);
            //lines += "FiscalData.NumberOfVoidedReceipts = " + fiscalprinter.GetData(FiscalData.NumberOfVoidedReceipts, (int)0).Data + "\n";

            //Console.WriteLine("FPTR_GD_Z_REPORT (7) " + FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data);
            //lines += "FiscalData.ZReport = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data + "\r\n";
            generalcounter.ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

            //Console.WriteLine("FPTR_GD_GRAND_TOTAL (8) " + FiscalReceipt.fiscalprinter.GetData(FiscalData.GrandTotal, (int)0).Data);
            //lines += "FiscalData.GrandTotal = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.GrandTotal, (int)0).Data + "\r\n";
            generalcounter.GrandTotal = FiscalReceipt.fiscalprinter.GetData(FiscalData.GrandTotal, (int)0).Data;

            //Necessario per la successiva istruzione
            //fiscalprinter.EndFiscalReceipt(false);

            //Console.WriteLine("FPTR_GD_FISCAL_REC \n Indicates the number of fiscal receipts / commercial documents of the day." +
            //    " \n Increased after the fiscal receipt has been closed. " +
            //     "\n Cleared after printing the daily closure. (20) " +
            //    "\n " + FiscalReceipt.fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data);
            //lines += "FiscalData.FiscalReceipt = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data + "\r\n";
            generalcounter.FiscalRec = FiscalReceipt.fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;


            //TODO WARNING: 18/11/20 CHIEDERE A MOLINARI PERCHè NN VA FiscalReceiptVoid
            /*
            //Console.WriteLine("FPTR_GD_FISCAL_REC_VOID \n Indicates the number of canceled fiscal receipts / commercial documents of the day." +
            //    " \n Increased after void the fiscal receipt. Cleared after printing the daily closure. \n " +
            //    "Any cancellation documents issued do not changes this total. (21) " +
            //   "\n " + FiscalReceipt.fiscalprinter.GetData(FiscalData.FiscalReceiptVoid, (int)0).Data);
            //lines += "FiscalData.FiscalReceiptVoid = " + FiscalReceipt.fiscalprinter.GetData(FiscalData.FiscalReceiptVoid, (int)0).Data + "\r\n";
            generalcounter.FiscalRecVoid = FiscalReceipt.fiscalprinter.GetData(FiscalData.FiscalReceiptVoid, (int)0).Data;
            */

            generalcounter.FiscalRecVoid = rData.getDailyData("37", "01");

            generalcounter.DailyTotalTicket = rData.getDailyData("19", "02");

            generalcounter.DailyTotalNumberTicket = rData.getDailyData("19", "01");

            generalcounter.DailyNonRiscossoBeniServizi = rData.getDailyData("73", rep);

            generalcounter.DailyNonRiscossoBeni = rData.getDailyData("74", rep);

            generalcounter.DailyNonRiscossoServizi = rData.getDailyData("75", rep);

            generalcounter.DailyNonRiscossoFatture = rData.getDailyData("76", rep);

            generalcounter.DailyNonRiscossoDaSSN = rData.getDailyData("78", rep);

            generalcounter.DailyNonRiscossoOmaggio = rData.getDailyData("71", rep);

            generalcounter.ScontoAPagare = rData.getDailyData("79", rep);

            generalcounter.DailyBuonoMonouso = rData.getDailyData("72", rep);

            generalcounter.DailyBuonoMultiuso = rData.getDailyData("80", rep);

            generalcounter.DailyAcconti = rData.getDailyData("70", rep);

            generalcounter.DailyRoundingNegativo = rData.getDailyData("81", rep);

            generalcounter.DailyRoundingPositivo = rData.getDailyData("82", rep);

            //Serialization General Counter 
            XmlSerializer serializer = new XmlSerializer(typeof(GeneralCounter));
            using (TextWriter tw = new StreamWriter("GeneralCounter.xml"))
            {
                serializer.Serialize(tw, generalcounter);
                tw.Flush();
                tw.Close();

            }
            

        }
    }

    //Classe creata per la gestione del WebService
    public class WebService : FiscalReceipt
    {

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

        public interface ILottery
        {
            bool IsLottery { get; set; }
            string Result { get; set; }
            string CodError { get; set; }
            string GetDate { get; }
            string GetZRep { get; }
            string GetNumScont { get; }
            string GetTillID { get; }
        }

        public struct LotteryStruct : ILottery
        {
            public string _zrep ;
            public string _numScon;
            public string _result;
            public string _codError;
            public bool _isLottery;
            public string _date;
            public string _tillID;

            
            public LotteryStruct(bool isLottery, string date, string codError, string result, string numScon, string zrep, string tillID )
            {
                this._isLottery = isLottery;
                this._date = date;
                this._codError = codError;
                this._result = result;
                this._numScon = numScon;
                this._zrep = zrep;
                this._tillID = tillID;
            }
            
            public bool IsLottery
            {
                set { _isLottery = value; }
                get { return _isLottery; }
            }

            public string Date
            {
                set { _date = value; }
                get { return _date; }
            }
            public string Result
            {
                set { _result = value; }
                get { return _result; }
            }
            public string CodError
            {
                set { _codError = value; }
                get { return _codError; }
            }

            public string GetDate
            {
                get { return _date; }
            }

            public string GetZRep
            {
                get { return _zrep; }
            }

            public string GetNumScont
            {
                get { return _numScon; }
            }

            public string GetTillID
            {
                get { return _tillID;  }
            }
        }
        

        //Devo scegliere se usare un'array o una list, credo sia meglio una list
        // LotteryStruct[] lotteries = new LotteryStruct[100];

        //Lo creo static per motivi vari ,poi vediamo se tenerlo così
        public static List<ILottery> listOfLottery = new List<ILottery>();

        public WebService()
        {
            //Inizializzo la lista dei doc lotterie da parsare 
            //Forse non mi serve questa init , lo faccio direttamente nel metodo LotteryFolderParser
            for (int i = 1; i <= 100; i++)
            {
                listOfLottery.Add(new LotteryStruct { _zrep = "0000", _numScon = i.ToString().PadLeft(4,'0'), Result = "05",  CodError ="FFFFF", IsLottery = false, _tillID = "00000000" });
            }
        }



        //metodo che confronta lo zRepNumber scritto nel file zrep.json del web service con lo Z Report ottenuto con la DirectIo 2050 indice 27
        public int ZrepJsonFile()
        {

            string[] strObj = new string[1];
            DirectIOData dirIO;
            int iData;
            string[] iObj = new string[1];
            
            //DirectIO 4219 GET LAN PARAMETERS       
            strObj[0] = "01";
            dirIO = posCommonFP.DirectIO(0, 4219, strObj);

            
            
            iData = dirIO.Data;
            //Console.WriteLine("DirectIO(): iData = " + iData);
            iObj = (string[])dirIO.Object;

            int uno = Int32.Parse(iObj[0].Substring(3, 3));
            string primo = uno.ToString();
            int due = Int32.Parse(iObj[0].Substring(7, 3));
            string secondo = due.ToString();
            int tre = Int32.Parse(iObj[0].Substring(11, 3));
            string terzo = tre.ToString();
            int quattro = Int32.Parse(iObj[0].Substring(15, 3));
            string quarto = primo +"." + secondo +"." + terzo + "." + quattro.ToString();

            //string URL = "http://" + quarto + "/www/json_files/zrep.json";
            string URL = "http://" + quarto + "/www/dati-rt/lotteria/20200227/";

            try
            {

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

                    //Console.WriteLine("provo a stampare lo z report " + entry.zRepNumber.Substring(1, 4));
                    //string zreport = entry.zRepNumber.Substring(1, 4);
                    int zrep = Int32.Parse(entry.zRepNumber.Substring(1, 4));
                    return zrep;
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions += 1;
                {

                    log.Error("Error Reading Url " + URL, e);

                }

                return NumExceptions;
            }
        }

        // method containing a string and the regex to check the string itself 
        public static bool isWhatILookingFor(string inputXml, string regex)
        {
            //string strRegex = @"-LOTTERIA.xml$"; // test matching string , dovrebbe funzionare
            string strRegex = @regex;
            // Class Regex Repesents an 
            // immutable regular expression. 
            // Format                Pattern 
            // Example
            // xxxxxxxxxx           ^[0 - 9]{ 10}$  
            // +xx xx xxxxxxxx     ^\+[0 - 9]{ 2}\s +[0 - 9]{ 2}\s +[0 - 9]{ 8}$ 
            // xxx - xxxx - xxxx   ^[0 - 9]{ 3} -[0 - 9]{ 4}-[0 - 9]{ 4}$ 
            Regex re = new Regex(strRegex);

            // The IsMatch method is used to validate 
            // a string or to ensure that a string 
            // conforms to a particular pattern. 
            if (re.IsMatch(inputXml))
                return (true);
            else
                return (false);
        }

        /*
        // method containing the regex 
        private bool isValidXmlLottery2(string inputXml)
        {
            string strRegex = @"^[0 - 9]{ 10}$"; // test matching string , dovrebbe funzionare

            // Class Regex Repesents an 
            // immutable regular expression. 
            // Format                Pattern 
            // Example
            // xxxxxxxxxx           ^[0 - 9]{ 10}$ 
            // +xx xx xxxxxxxx     ^\+[0 - 9]{ 2}\s +[0 - 9]{ 2}\s +[0 - 9]{ 8}$ 
            // xxx - xxxx - xxxx   ^[0 - 9]{ 3} -[0 - 9]{ 4}-[0 - 9]{ 4}$ 
            Regex re = new Regex(strRegex);

            // The IsMatch method is used to validate 
            // a string or to ensure that a string 
            // conforms to a particular pattern. 
            if (re.IsMatch(inputXml))
                return (true);
            else
                return (false);
        }

        */


        //Metodo che DOVREBBE parsare la cartella /dati-rt/lotteria/"data"  e tutti i file LOTTERIA.xml che sono all'interno
        //L'obiettivo è per ogni documento commerciale controlli che l'ammontare sia uguale al totale importo pagato
        //input : string urlString = doc da parsare
        //output: int scontrini , conta il numero di scontrini lotteria
        public static int LotteryFolderSentParser(string urlString,  ref int scontrini)
        {
            // URL di prova
            String URLString = urlString;

            //ammontare da parsare e controllare che corrisponda alla sommatoria degli importi relativi
            double ammontare = 0;
            //Store of the previous XmlNodeType.Element
            string lastElement = "";
            //Contatore globale delle vendite
            int counter = 0;

            string[] strObj = new string[1];
            DirectIOData dirIO;
            int iData;
            string[] iObj = new string[1];


            try
            {

                //mi prendo le info importanti relative al doc in questione direttamente dall ' url
                int Zrep = Convert.ToInt32(urlString.Substring(urlString.Length - 35,4).PadLeft(4,'0'));
                int Inizio = Convert.ToInt32(urlString.Substring(urlString.Length - 29, 4));
                int Fine = Convert.ToInt32(urlString.Substring(urlString.Length - 23, 4));
                int Lunghezza = Convert.ToInt32(urlString.Substring(urlString.Length - 17, 4));
                string data = urlString.Substring(urlString.Length - 50, 6);
                //devo rovesciare la data nel formato DDMMYY
                data = data.Substring(4,2) + data.Substring(2,2) + data.Substring(0,2);
                string tillID = urlString.Substring(urlString.Length - 61, 8);

                //var LotteryList = new[] { new { ZRep = Zrep.ToString().PadLeft(4), NumScon= "0001", isLottery = false } }.ToList();
                //Rimuovo tutto gli elementi eventuali di parser precedenti
                listOfLottery.Clear();
                //Creo una lista lunga quanto la lunghezza dell'xml inviato
                for (int i = Inizio; i <= Fine; i++)
                {
                    listOfLottery.Add(new LotteryStruct { _zrep = Zrep.ToString().PadLeft(4, '0'), _numScon = i.ToString().PadLeft(4,'0'), IsLottery = false, Date = data, CodError = "FFFFF", Result = "05", _tillID = tillID });
                }
            
                // something that will read the XML file
                XmlTextReader reader = null;

                
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlString);
                //request.ContentType = "application/json; charset=utf-8";
                //request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("username:password"));
                //request.PreAuthenticate = true;

                string credentials = "epson:epson";

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls; // SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | 


                String username = "epson";
                String password = "epson";
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                request.Headers.Add("Authorization", "Basic " + encoded);
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36";
                CookieContainer myContainer = new CookieContainer();
                request.Credentials = new NetworkCredential(username, password);
                request.CookieContainer = myContainer;
                request.Referer = urlString;
                request.PreAuthenticate = true;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    Stream responseStream = response.GetResponseStream();

                    reader = new XmlTextReader(URLString, responseStream);


                    log.Info("Parsing " + URLString + " file");

                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element: // The node is an element.

                                //Console.Write("<" + reader.Name);
                                /*
                                if (String.Equals(reader.Name , "Ammontare"))

                                {
                                    //Qui devo iniziare a looppare per trovare il tag "Ammontare", XmlNodeType.Text relativo
                                    reader.Read();
                                    ammontare = Convert.ToDouble(reader.Value);
                                    //Trovato Ammontare ora vado a loopare fino a quando non trovo /Vendita come endElement


                                }
                                */
                                lastElement = reader.Name;
                                break;

                            case XmlNodeType.Text: //Display the text in each element.

                                if (string.Compare(lastElement, "Ammontare") == 0)
                                {
                                    ammontare = Convert.ToDouble(reader.Value);
                                    counter++;
                                    break;
                                }

                                if (string.Compare(lastElement, "Importo") == 0) //E' una forma di pagamento , puo essere unica o parziale (cash , carda di credito, ticket)
                                {
                                    ammontare -= Convert.ToDouble(reader.Value);
                                    break;
                                }

                                //Ho trovato un doc lotteria, devo fillare la listOfLottery listOfLottery 
                                if (string.Compare(lastElement, "NumeroProgressivo") == 0)
                                {
                                    int index = Convert.ToInt32(reader.Value.Substring(5, 4));
                                    listOfLottery[index - Inizio].IsLottery = true;
                                }

                                break;

                            case XmlNodeType.EndElement:

                                if (String.Equals(reader.Name, "Vendita")) //Ho finito di parsare un Doc Commerciale , ammontare e tot importo devono corrispondere else errore grave

                                {
                                    if (ammontare != 0)
                                    {
                                        log.Error("Errore parsando il link " + URLString + "alla riga: " + reader.LinePosition + " Importo totale pagamento non corrisponde all'ammontare da pagare ");
                                        //EDIT 09/01/2020 : ho elimitato il throw new Exception perchè preferisco che mi segnali
                                        //l'errore con un log ma continui a lavorare fino alla fine del doc altrimenti la struttura
                                        //listOflottery non viene interamente completata
                                        //throw new Exception();
                                    }

                                }
                                break;
                        }
                    }
                }

                log.Info("Validation of file " + URLString + " Passed");
                //Console.WriteLine("Validation of file " + URLString + " Passed without issues ");
                Console.WriteLine("Totale scontrini con lotteria parsati : " + counter);

                //Test inutile nonchè insensato perchè posso avere scontrini NON di lotteria in mezzo
                /*
                if (counter != WebService.listOfLottery.Count)
                {
                    throw new Exception();
                }
                */
                scontrini += counter;


            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions += 1;
                {

                    log.Error("Error Parsing Url " + URLString, e);

                }

                    
            }
            return NumExceptions;
        }


        //Metodo che parsa la cartella www/dati-rt/lotteria/
        public int OuterHTMLParser(string regex = @"^[0-9]{8}$")
        {
            string html = String.Empty;
            try
            {
                if (String.Compare(regex, " ") ==0)
                {
                    regex = @"^[0-9]{8}$";
                }
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                //DirectIO 4219 GET LAN PARAMETERS       
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 4219, strObj);

                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;

                int uno = Int32.Parse(iObj[0].Substring(3, 3));
                string primo = uno.ToString();
                int due = Int32.Parse(iObj[0].Substring(7, 3));
                string secondo = due.ToString();
                int tre = Int32.Parse(iObj[0].Substring(11, 3));
                string terzo = tre.ToString();
                int quattro = Int32.Parse(iObj[0].Substring(15, 3));
                string quarto = primo + "." + secondo + "." + terzo + "." + quattro.ToString();

                string URL ="http://"+ quarto + "/dati-rt/lotteria/";

                //string URL = @"G:\Server\XmlPerServer\Appoggio\dati-rt";

                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                // There are various options, set as needed
                htmlDoc.OptionFixNestedTags = true;

                
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                string credentials = "epson:epson";
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

                String username = "epson";
                String password = "epson";
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                request.Headers.Add("Authorization", "Basic " + encoded);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                CookieContainer myContainer = new CookieContainer();
                request.Credentials = new NetworkCredential(username, password);
                request.CookieContainer = myContainer;
                request.PreAuthenticate = true;

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;


                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    //string data = reader.ReadToEnd();


                    // filePath is a path to a file containing the html
                    htmlDoc.Load(reader);
                    //htmlDoc.Load(URL);

                    // Use:  htmlDoc.LoadHtml(xmlString);  to load from a string (was htmlDoc.LoadXML(xmlString)

                    // ParseErrors is an ArrayList containing any errors from the Load statement
                    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                    {
                        // Handle any parse errors as required
                        //mancano 4 tag di chiusura nel WebService

                    }
                  
                    if (htmlDoc.DocumentNode != null)
                    {
                        HtmlAgilityPack.HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//@href");
                        HtmlAgilityPack.HtmlNodeCollection bodyNode2 = htmlDoc.DocumentNode.SelectNodes("//@href");

                        if (bodyNode != null)
                        {
                            // Do something with bodyNode
                            //Console.WriteLine("Test");
                        }

                        
                        if(bodyNode2.Count != 0)
                        {
                            /*
                            for (int i = 3; i < bodyNode2.Count; i = i + 2)
                            {
                                //Console.WriteLine(bodyNode2[i].GetDirectInnerText());
                                LotteryFolderParser(bodyNode2[i].GetDirectInnerText() ,ref scontrini);
                            }
                            */
                            for (int i = 0; i < bodyNode2.Count; i++)
                            {
                                //EDIT 11/12/19 refactoring method , invece del regex fisso ce lo passo da input
                                // if (isWhatILookingFor(bodyNode2[i].GetDirectInnerText() , @"^\d{8}\/$"))
                                if (isWhatILookingFor(bodyNode2[i].GetDirectInnerText(), @regex))
                                {
                                    //LotteryFolderParser(bodyNode2[i].GetDirectInnerText(), ref scontrini);
                                    InnerHTMLParser(URL + bodyNode2[i].GetDirectInnerText() + "/");

                                }
                            }

                        }
                        
                    }
                   
                }

            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                        Console.WriteLine(html = streamReader.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions += 1;
                {

                    log.Error("Error OuterHTMLParser  ", e);

                }

            }
            return NumExceptions;
        }



        // Parser HTML interno (cioè all'interno di una specifica data) 
        // 
        public static int InnerHTMLParser(string path, string regex = @"-LOTTERIA.xml$")
        {
            try
            {//Totalizzatore generale scontrini con lotteria parsati
                int scontrini = 0;

                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                // There are various options, set as needed
                htmlDoc.OptionFixNestedTags = true;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);
                //request.ContentType = "application/json; charset=utf-8";
                //request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("username:password"));
                //request.PreAuthenticate = true;

                string credentials = "epson:epson";

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls; // SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | 

               
                String username = "epson";
                String password = "epson";
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                request.Headers.Add("Authorization", "Basic " + encoded);
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36";
                CookieContainer myContainer = new CookieContainer();
                request.Credentials = new NetworkCredential(username, password);
                request.CookieContainer = myContainer;
                request.PreAuthenticate = true;
               
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;



                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    //string data = reader.ReadToEnd();

                    // filePath is a path to a file containing the html
                    htmlDoc.Load(reader);
                    //htmlDoc.Load(URL);

                    // Use:  htmlDoc.LoadHtml(xmlString);  to load from a string (was htmlDoc.LoadXML(xmlString)

                    // ParseErrors is an ArrayList containing any errors from the Load statement
                    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                    {
                        // Handle any parse errors as required
                        //mancano 4 tag di chiusura nel WebService

                    }

                    if (htmlDoc.DocumentNode != null)
                    {
                        HtmlAgilityPack.HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//@href");
                        HtmlAgilityPack.HtmlNodeCollection bodyNode2 = htmlDoc.DocumentNode.SelectNodes("//@href");

                        if (bodyNode != null)
                        {
                            // Do something with bodyNode
                            //Console.WriteLine("Test");
                        }

                        if (bodyNode2.Count != 0)
                        {
                            /*
                            for (int i = 3; i < bodyNode2.Count; i = i + 2)
                            {
                                //Console.WriteLine(bodyNode2[i].GetDirectInnerText());
                                LotteryFolderParser(bodyNode2[i].GetDirectInnerText() ,ref scontrini);
                            }
                            */
                            for (int i = 0; i < bodyNode2.Count; i++)
                            {
                                //if (isWhatILookingFor(bodyNode2[i].GetDirectInnerText(), "-LOTTERIA.xml$"))

                                if (isWhatILookingFor(bodyNode2[i].GetDirectInnerText(), regex)) 
                                {
                                    //Se deve parsare i doc lotteria inviati 
                                    if (String.Compare(regex, "-LOTTERIA.xml$") == 0)
                                    {
                                       
                                        WebService.LotteryFolderSentParser(path + bodyNode2[i].GetDirectInnerText(), ref scontrini);
                                    }
                                    else
                                        //else deve parsare i doc lotteria di risposta
                                         Lottery.LotteryFolderResponseParser(path + bodyNode2[i].GetDirectInnerText());
                                }
         
                            }

                        }

                    }

                }

                //E' solo temporaneo , eventualmente da utilizzare questa info come check sul comando nuovo
                //Console.WriteLine("Totale scontrini con lotteria parsati = : " + scontrini);
                log.Info("Totale scontrini con lotteria parsati = : " + scontrini);
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions += 1;
                {

                    log.Error("Error InnerHTMLParser Parser ", e);

                }

            }
            return NumExceptions;
        }



        //Test che controlla se nel link http://10.15.17.201/www/dati-rt/demo/ viene aggiornata la lista dei report dopo 2 chiusure in demo
        public int TestWebServiceDatiRtDemo()
        {

            try
            {
                log.Info("Performing TestWebServiceDatiRtDemo");
                
                //Recuper dati chiusure giornaliere da webservice dati-rt/demo
                string URL = "http://10.15.17.201/www/dati-rt/demo/";
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                //request.ContentType = "application/json; charset=utf-8";
                //request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("username:password"));
                //request.PreAuthenticate = true;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");

                MatchCollection matches;

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string data = reader.ReadToEnd();

                    matches = regex.Matches(data);

                }


                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 3001, strObj);

                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 3001, strObj);


                //leggo numero ZReport

                //Leggo lo ZRep Number
                strObj[0] = "2700";

                dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                string ZrepNum = iObj[0].Substring(20, 4);
                //log.Info("Test ZrepNum Demo Mode = " + ZrepNum);

                strObj[0] = ZrepNum.PadLeft(4, '0');

                Thread.Sleep(1000);

                request = (HttpWebRequest)WebRequest.Create(URL);

                response = request.GetResponse() as HttpWebResponse;

                MatchCollection matches2;

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string data = reader.ReadToEnd();

                    matches2 = regex.Matches(data);

                }

                if (matches2.Count != (matches.Count + 4))  //Ogni Zreport genera un file di risposta per cui alla fine devo avere 4 file in + 
                {
                    log.Error("Error WebService number of Z Report " + " http://10.15.17.201/www/dati-rt/demo/ " + " expected " + (matches.Count + 2) + " readed " + matches2.Count);

                }

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Fatal("Generic Error", e);

                }
            }
            return NumExceptions;



        }






        //Test che controlla se nel link http://10.15.17.201/www/dati-rt/demo/ viene aggiornata la lista dei report dopo 2 chiusure tecnicamente da rifiutare in quanto in orari errati (in demo)
        public int TestWebServiceDatiRtDemoRifiutati()
        {

            try
            {
                log.Info("Performing TestWebServiceDatiRtDemoRifiutati");
                //Faccio chiusura giornaliera
                fiscalprinter.PrintZReport();

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string rtType;

                strObj[0] = DateTime.Now.ToString("ddMMyyHHmm");
                Int64 temporary = Int64.Parse(strObj[0]);
                temporary += 400; //sposto la data di 6 ore in avanti
                strObj[0] = temporary.ToString().PadLeft(10, '0'); ;
                dirIO = posCommonFP.DirectIO(0, 4001, strObj); // tarocco la data in avanti ma solo di di 6 ore in modo da poter ripristinare
                iData = dirIO.Data;
                if (iData != 4001)
                {
                    log.Error("Error DirectIO 4001 campo iData, expected 4001, received " + iData);
                    throw new PosControlException();
                }



                //Recuper dati chiusure giornaliere da webservice dati-rt/demo
                string URL = "http://10.15.17.201/www/dati-rt/demo/";
               

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                //request.ContentType = "application/json; charset=utf-8";
                //request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("username:password"));
                //request.PreAuthenticate = true;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");

                MatchCollection matches;

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string data = reader.ReadToEnd();

                    matches = regex.Matches(data);

                }



                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 3001, strObj);


                
                //Leggo lo ZRep Number
                strObj[0] = "2700";

                dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                string ZrepNum = iObj[0].Substring(20, 4);

                strObj[0] = ZrepNum.PadLeft(4, '0');
                /*
                //READ ZREP ID ANSWER FROM TAX AUTORITHY
                //Prova per capire che succede
                string result;
                do
                {
                    dirIO = posCommonFP.DirectIO(0, 9217, strObj);
                    iObj = (string[])dirIO.Object;
                    result = iObj[0].Substring(0, 1);
                    log.Error(result);
                } while (result != "3");

                */





                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 3001, strObj);

                /*
                //Leggo lo ZRep Number
                strObj[0] = "2700";

                dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                ZrepNum = iObj[0].Substring(20, 4);

                strObj[0] = ZrepNum.PadLeft(4, '0');

                //READ ZREP ID ANSWER FROM TAX AUTORITHY
                //Prova per capire che succede
                do
                {
                    dirIO = posCommonFP.DirectIO(0, 9217, strObj);
                    iObj = (string[])dirIO.Object;
                    result = iObj[0].Substring(0, 1);
                    log.Error(result);
                } while (result != "3");

                */


                Thread.Sleep(1000);



                //Riconto gli ZReport da WebService

                request = (HttpWebRequest)WebRequest.Create(URL);

                response = request.GetResponse() as HttpWebResponse;

                MatchCollection matches2;

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string data = reader.ReadToEnd();

                    matches2 = regex.Matches(data);

                }

                if (matches2.Count != (matches.Count + 4))  //Ogni Zreport genera un file di risposta per cui alla fine devo avere 4 file in + 
                {
                    log.Error("Error WebService number of Z Report " + " http://10.15.17.201/www/dati-rt/demo/ " + " expected " + (matches.Count + 4) + " readed " + matches2.Count);

                }



                //Ripristino la data corretta EDIT: DateTime.Now.ToString("ddMMyyhhmm"); non va bene come formato,bisogna vedere quello giusto
                strObj[0] = DateTime.Now.ToString("ddMMyyHHmm");
                dirIO = posCommonFP.DirectIO(0, 4001, strObj);
                iData = dirIO.Data;
                if (iData != 4001)
                {
                    log.Error("Error DirectIO 4001 , errore nel ripristino data corretta");
                    throw new PosControlException();
                }


            }
            catch (Exception e)
            {

                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Fatal("Generic Error", e);

                }
            }
            return NumExceptions;



        }






        //Test che controlla se nel link http://10.15.17.201/www/dati-rt/rifiutati/ vengono aggiornati i report dopo 2 chiusure in RT
        //Con la data spostata volutamente in avanti (quindi simulando un errore e 2 chiusure): la lista dei rifiutati deve aumentare di 2
        public int TestWebServiceDatiRtRifiutati()
        {
            log.Info("Performing TestWebServiceDatiRtRifiutati");
            try
            {
                //Faccio chiusura giornaliera
                fiscalprinter.PrintZReport();

                
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string rtType;

                strObj[0] = DateTime.Now.ToString("ddMMyyHHmm");
                Int64 temporary = Int64.Parse(strObj[0]);
                temporary += 400; //sposto la data di 6 ore in avanti
                strObj[0] = temporary.ToString().PadLeft(10, '0'); ;
                dirIO = posCommonFP.DirectIO(0, 4001, strObj); // tarocco la data in avanti ma solo di di 6 ore in modo da poter ripristinare
                iData = dirIO.Data;
                if (iData != 4001)
                {
                    log.Error("Error DirectIO 4001 campo iData, expected 4001, received " + iData);
                    throw new PosControlException();
                }

                
                iObj = (string[])dirIO.Object;
                
                // Check RT status 
                //Console.WriteLine("DirectIO (RT status)");
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                iData = dirIO.Data;
                if (iData != 1138)
                {
                    log.Error("Error DirectIO 1138 campo iData, expected 1138, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;

                //Verifico che la Printer sia in RT: conditio sine qua non ( check quarto e quinto byte 1138 command ,secondo protocollo) 
                rtType = iObj[0].Substring(3, 2);
                //Console.WriteLine("RT type: " + rtType);
                int rtTypeInt = Convert.ToInt32(rtType);
                if (rtTypeInt == 1)
                {
                    
                    log.Error("Printer is NOT in RT mode, per effettuare tale test va messa prima in RT");
                    return 1;
                }
                else
                {
                    if (rtTypeInt == 2)
                    {
                        
                        log.Info("Printer is in RT mode");
                    }
                    else
                    {
                        log.Error("Error DirectIO 1138 campo MAIN, expected or 02(RT), received " + rtTypeInt);
                        return 1;
                        throw new Exception();
                    }
                }
                
                //leggo il numero di files respinti dall ADE
                string rejfiles = iObj[0].Substring(17, 4);
                
                //Recuper dati dei report rifiutati da webservice dati-rt/rifiutati , qui pero' devo usare la directIO per leggere il mio ip
                //edit: questa può essere tranquillamente un metodo di questa classe,gli passo l'url e lui conta i file
                string URL = "http://10.15.17.201/www/dati-rt/rifiutati/";
               

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                //request.ContentType = "application/json; charset=utf-8";
                //request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("username:password"));
                //request.PreAuthenticate = true;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");

                MatchCollection matches;

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string data = reader.ReadToEnd();

                    matches = regex.Matches(data);

                }

                //Nota: Tecnicamente matches.count dovrebbe essere uguale a rejfiles letto dalla 1138 ma il secondo è + affidabile

                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 3001, strObj);


                //Leggo lo ZRep Number
                strObj[0] = "2700";
                
                dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                string ZrepNum = iObj[0].Substring(20, 4);

                strObj[0] = ZrepNum.PadLeft(4, '0');

                
                //READ ZREP ID ANSWER FROM TAX AUTORITHY
                //Prova per capire che succede
                string result;
                do
                {
                    dirIO = posCommonFP.DirectIO(0, 9217, strObj);
                    iObj = (string[])dirIO.Object;
                    result = iObj[0].Substring(0, 1);
                    //log.Info(result);
                } while (result != "3");
                
                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 3001, strObj);

                strObj[0] = "2700";

                dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                ZrepNum = iObj[0].Substring(20, 4);

                strObj[0] = ZrepNum.PadLeft(4, '0');

                //READ ZREP ID ANSWER FROM TAX AUTORITHY
                //Prova per capire che succede
                
                do
                {
                    dirIO = posCommonFP.DirectIO(0, 9217, strObj);
                    iObj = (string[])dirIO.Object;
                    result = iObj[0].Substring(0, 1);
                    //log.Info(result);
                } while (result != "3");



                //Rifaccio la lettura dallo stesso url,i file ora rifiutati devono essere incrementati di due E me li deve/dovrebbe segnare anche la 1138
                request = (HttpWebRequest)WebRequest.Create(URL);

                response = request.GetResponse() as HttpWebResponse;

                MatchCollection matches2;

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string data = reader.ReadToEnd();

                    matches2 = regex.Matches(data);

                }

                if (matches2.Count != (matches.Count + 4))  //Ogni Zreport genera un file di risposta per cui alla fine devo avere 4 file in + 
                {
                    log.Error("Error WebService number of Z Report " + " http://10.15.17.201/www/dati-rt/rifiutati/ " + " expected " + (matches.Count + 4) + " readed " + matches2.Count);
                    //throw new Exception();
                    NumExceptions++;
                }



                //Controllo anche sulla 1138 che i rifiutati siano aumentati di 2 
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                iData = dirIO.Data;
                if (iData != 1138)
                {
                    log.Error("Error DirectIO 1138 campo iData, expected 1138, received " + iData);
                    throw new PosControlException();
                }
                iObj = (string[])dirIO.Object;

                //leggo il numero di files respinti dall ADE
                string rejfiles2 = iObj[0].Substring(17, 4);

                if (Int32.Parse(rejfiles2) != (Int32.Parse(rejfiles) + 2))
                {
                    log.Error("Error DirectIO 1138 campo REJ FILES, expected" + (Int32.Parse(rejfiles) + 2) + ", received " + rejfiles2);
                    //throw new PosControlException();
                    NumExceptions++;
                }


                
                //Ripristino la data corretta EDIT: DateTime.Now.ToString("ddMMyyhhmm"); non va bene come formato,bisogna vedere quello giusto
                strObj[0] = DateTime.Now.ToString("ddMMyyHHmm");
                dirIO = posCommonFP.DirectIO(0, 4001, strObj);
                iData = dirIO.Data;
                if (iData != 4001)
                {
                    log.Error("Error DirectIO 4001 , errore nel ripristino data corretta");
                    throw new PosControlException();
                }
                

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions += 1;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Fatal("Generic Error", e);

                }

                //return NumExceptions;
            }
            return NumExceptions;

        }


        //Test che controlla se nella cartella del WebService http://10.15.17.201/www/dati-rt/today/ viene aggiornata la lista degli Zreport 
        //dopo 2 chiusure in RT : la lista degli ZReport deve aumentare di 2
        public int TestWebServiceDatiRtAccettati()
        {
            log.Info("Performing TestWebServiceDatiRtAccettati");
            try
            {
                //Faccio chiusura giornaliera
                fiscalprinter.PrintZReport();


                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string rtType;

                string today = DateTime.Now.ToString("yyyyMMdd");


                // Check RT status 
                //Console.WriteLine("DirectIO (RT status)");
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                iData = dirIO.Data;
                if (iData != 1138)
                {
                    log.Error("Error DirectIO 1138 campo iData, expected 1138, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;


                //Verifico che la Printer sia in RT: conditio sine qua non ( check quarto e quinto byte 1138 command ,secondo protocollo) 
                rtType = iObj[0].Substring(3, 2);
                //Console.WriteLine("RT type: " + rtType);
                int rtTypeInt = Convert.ToInt32(rtType);
                if (rtTypeInt == 1)
                {

                    log.Error("Printer is NOT in RT mode, per effettuare tale test va messa prima in RT");
                    return 1;
                }
                else
                {
                    if (rtTypeInt == 2)
                    {

                        log.Info("Printer is in RT mode");
                    }
                    else
                    {
                        log.Error("Error DirectIO 1138 campo MAIN, expected or 02(RT), received " + rtTypeInt);
                        return 1;
                        throw new Exception();
                    }
                }




                //Leggo direttamente il mio IP senza passarcelo in maniera statica
                /*

                //DirectIO 4219 GET LAN PARAMETERS       
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 4219, strObj);



                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;

                int uno = Int32.Parse(iObj[0].Substring(3, 3));
                string primo = uno.ToString();
                int due = Int32.Parse(iObj[0].Substring(7, 3));
                string secondo = due.ToString();
                int tre = Int32.Parse(iObj[0].Substring(11, 3));
                string terzo = tre.ToString();
                int quattro = Int32.Parse(iObj[0].Substring(15, 3));
                string quarto = primo + "." + secondo + "." + terzo + "." + quattro.ToString();

                string URL = "http://" + quarto";
                */









                //Recuper elenco ZReport da webservice dati-rt/today , qui pero' devo usare la directIO per leggere il mio ip
                //edit: questa può essere tranquillamente un metodo di questa classe,gli passo l'url e lui conta i file
                string URL = "http://10.15.17.201/www/dati-rt/" + today + "/";


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                //request.ContentType = "application/json; charset=utf-8";
                //request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("username:password"));
                //request.PreAuthenticate = true;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");

                MatchCollection matches;

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string data = reader.ReadToEnd();

                    matches = regex.Matches(data);

                }

                //Nota: Tecnicamente matches.count dovrebbe essere uguale a rejfiles letto dalla 1138 ma il secondo è + affidabile

                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 3001, strObj);


                //Leggo lo ZRep Number
                strObj[0] = "2700";

                dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                string ZrepNum = iObj[0].Substring(20, 4);

                strObj[0] = ZrepNum.PadLeft(4, '0');

                //READ ZREP ID ANSWER FROM TAX AUTORITHY
                //Prova per capire che succede
                string result;
                do
                {
                    dirIO = posCommonFP.DirectIO(0, 9217, strObj);
                    iObj = (string[])dirIO.Object;
                    result = iObj[0].Substring(0, 1);
                    //log.Info(result);
                } while (result != "0");

                //print Z Report
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 3001, strObj);

                strObj[0] = "2700";

                dirIO = posCommonFP.DirectIO(0, 2050, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                ZrepNum = iObj[0].Substring(20, 4);

                strObj[0] = ZrepNum.PadLeft(4, '0');

                //READ ZREP ID ANSWER FROM TAX AUTORITHY
                //Prova per capire che succede

                do
                {
                    dirIO = posCommonFP.DirectIO(0, 9217, strObj);
                    iObj = (string[])dirIO.Object;
                    result = iObj[0].Substring(0, 1);
                    //log.Info(result);
                } while (result != "0");

                Thread.Sleep(5000);

                //Rifaccio la lettura dallo stesso url,i file ora devono essere incrementati di sei 
                request = (HttpWebRequest)WebRequest.Create(URL);

                response = request.GetResponse() as HttpWebResponse;

                MatchCollection matches2;

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string data = reader.ReadToEnd();

                    matches2 = regex.Matches(data);

                }

                if (matches2.Count != (matches.Count + 6))  //Ogni Zreport corretto genera 2 xml e 1 txt di risposta per cui alla fine devo avere 6 file in + 
                {
                    log.Error("Error del WebService number of Z Report su " + URL + " expected " + (matches.Count + 6) + " readed " + matches2.Count);
                    //throw new Exception();
                    NumExceptions++;
                }


            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions += 1;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Fatal("Generic Error", e);

                }

                //return NumExceptions;
            }
            return NumExceptions;

        }

        //TODO controllare se testo mai questo metodo
        //Test sulla DirectIO 4034
        //Ha molti parametri ma io l'ho implementato principalmente per riavviare il Web Server
        //Il WebServer va o andrebbe riavviato semplicemente quando non risponde (tipo quando faccio le GET e vado in TimeOut
        public int TestWebServerIntelligentParameters()
        {
            log.Info("Performing TestWebServerIntelligentParameters");
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                string appoggio = "                                                            fine";
                strObj[0] = "019900" + appoggio;
                dirIO = posCommonFP.DirectIO(0, 4034, strObj); 

                iObj = (string[])dirIO.Object;
                iData = dirIO.Data;


            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions += 1;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Fatal("Generic Error", e);

                }

                //return NumExceptions;
            }
            return NumExceptions;
        }




        //DirectIO 4014 63
        //Comando SET Flag (4014) Flag = 63 Demo Mode VAL = 1/0 
        //Comando SET Flag (4014) Flag = 62 SIMULAZIONE Mode VAL = 1/0 
        //Update: inserito anche il comando per switchare in automatico in RT (3333 + 1433 + X + Contante)
        //EDIT: 26/09/19 Metodo che simula i comandi del Libretto Elettronico degli interventi tecnici con la DirectIO 9002
        public void commuteMode(string state)
        {
            log.Info("Performing commuteMode: " + state + "\n");
            //E' un flag che ho settato per beccarmi la segnalazione se per caso
            //la stampante proviene o va in MF senza autorizzazione (seg fault)
            bool test = false;
            //TODO: da eliminare, è solo per non farlo andare troppo in MF senno mi finiscono gli switch
            /*
            if(String.Compare(state,"MF") == 0)
            {
                state = "RT";
            }
            */
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                //EDIT: 170920 aggiunta modifica con l'intro del nuovo flag SIMULAZIONE
                //Flag 63
                strObj[0] = "63";
                if (state.Equals("Demo", StringComparison.OrdinalIgnoreCase))
                {

                    //test , non so se devo fare la printzreport anche qui
                    
                    //Aumento il timeout perchè ogni tanto lo ZReport lo esegue ma poi mi va in exception
                    dirIO = PosCommonFP.DirectIO(-112, 60000, strObj);
                    strObj[0] = "01";
                    dirIO = posCommonFP.DirectIO(0, 3001, strObj);


                    //Leggo lo stato passato da cui venivo
                    strObj[0] = "63";
                    dirIO = posCommonFP.DirectIO(0, 4214, strObj); // Leggo il flag 63
                    iObj = (string[])dirIO.Object;
                    string status = iObj[0].Substring(2, 1);
                    if (String.Compare(status, "1") == 0) //Sono in DemoRT,non devo far nulla
                    {
                        //strObj[0] = "631"; //3333 1463 X 
                    }
                    else //Ero in MF o RT,mi basta fare la 3333 1463 X EDIT: Ora esiste anche lo stato Simulazione
                    {
                        //Controllo prima il flag Simulazione, se sono li devo fare chiusura, disattivarlo (quindi vado in RT e poi vado in Demo)
                        strObj[0] = "62";
                        dirIO = posCommonFP.DirectIO(0, 4214, strObj);
                        iObj = (string[])dirIO.Object;
                        status = iObj[0].Substring(2, 1);
                        if (String.Compare(status, "1") == 0) //Sono in simulazione
                        {   //Faccio chiusura e disattivo la simulazione, ergo vado in RT. Da li mi porto in demo
                            dirIO = PosCommonFP.DirectIO(-112, 60000, strObj);
                            strObj[0] = "01";
                            dirIO = posCommonFP.DirectIO(0, 3001, strObj);
                            strObj[0] = "620";
                            dirIO = posCommonFP.DirectIO(0, 4014, strObj);
                            iData = dirIO.Data;
                            if (iData != 4014)
                            {
                                log.Error("Errore Risposta 4014 nel passare da simulazione ad RT, expected 4014, received " + iData);
                                throw new PosControlException();
                            }
                        }

                        dirIO = PosCommonFP.DirectIO(-112, 60000, strObj);
                        strObj[0] = "631";
                        dirIO = posCommonFP.DirectIO(0, 4014, strObj); // Setto il flag 63
                        //Aumento profondità di test controllando anche la risposta della 4014 (SET FLAG)
                        iObj = (string[])dirIO.Object;
                        iData = dirIO.Data;
                        if (iData != 4014)
                        {
                            log.Error("Errore Risposta 4014 (SET FLAG), expected 4014, received " + iData);
                            //throw new PosControlException();
                        }
                    }

                    Mode = "DemoRT";

                }
                else if (state.Equals("MF", StringComparison.OrdinalIgnoreCase))
                {
                    

                    // ATTENZIONE,PER POTER SWITCHARE IN MF DEVO FARE PRIMA UNA CHIUSURA GIORNALIERA (PRINTZ REPORT)
                    
                    strObj[0] = "01";
                    dirIO = posCommonFP.DirectIO(0, 3001, strObj);


                    //EDIT: 25/09/19 Ho sniffato i pacchetti HTTP del WebService (che alla fine manda delle DirectIO ),testate con POSTMAN
                    //Per fare le stesse cose del WebService ma via Software


                    //Leggo lo stato passato da cui venivo
                    strObj[0] = "63";
                    dirIO = posCommonFP.DirectIO(0, 4214, strObj); // Leggo il flag 63
                    iObj = (string[])dirIO.Object;
                    string status = iObj[0].Substring(2, 1);
                    if (String.Compare(status, "1") == 0) //Sono in DemoRT
                    {

                        //Devo prima andare in RT e poi fare disattivazione (sicuro?prova a farlo direttamente)
                        //EDIT: Ho provato,non si può andare direttamente da DemoRT in MF CENSITO
                        strObj[0] = "630"; //3333 1463 X 
                        dirIO = posCommonFP.DirectIO(0, 4014, strObj);
                        //Aumento profondità di test controllando anche la risposta della 4014 (SET FLAG)
                        iObj = (string[])dirIO.Object;
                        iData = dirIO.Data;
                        if (iData != 4014)
                        {
                            log.Error("Errore Risposta 4014 (SET FLAG), expected 4014, received " + iData);
                            throw new PosControlException();
                        }
                        //Ora mi sono messo in RT ,quindi posso fare la Disattivazione in MF CENSITO
                        //EDIT 03/10/19: non è mica vero che sono in RT, devo fare una DirectIO 1138 per sapere dove sto...

                        // Check RT status , chiedo tramite la 1138
                        strObj[0] = "01";
                        dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                        iData = dirIO.Data;
                        if (iData != 1138)
                        {
                            log.Error("Error DirectIO 1138 campo iData, expected 1138, received " + iData);
                            throw new PosControlException();
                        }
                        iObj = (string[])dirIO.Object;

                        //Controllo che anche la 1138 mi dice che sono in MF ( check quarto e quinto byte 1138 command ,secondo protocollo)
                        string Sub = iObj[0].Substring(5, 2);
                        int SubInt = Convert.ToInt32(Sub);
                        if (SubInt == 6)
                        {
                            log.Info("Printer is just in MF mode, nothing to do");
                        }
                        else
                        if (SubInt == 8)
                        {
                            
                            string ora = DateTime.Now.ToString("ddMMyyHHmmss");



                            string Text = "                                                                                                fine";
                            strObj[0] = "12NVRNDR59B10F205DIT12825980159                 IT07511580156                 ";
                            strObj[0] += ora + Text;

                            dirIO = posCommonFP.DirectIO(0, 9002, strObj); // RT Service Command (Disattivazione)
                            iData = dirIO.Data;
                            iObj = (string[])dirIO.Object;
                            /* EDIT 03/10/19 delle volte mi da 00 delle volte mi da 01 ma cmq il comando lo esegue,nn è discriminante
                            if (!(String.Equals(iObj[0].Substring(0, 2), "01")))
                            {
                                log.Error("Richiesta Disattivazione Fallita, Error DirectIO 9002 operator, expected 01, received " + iObj[0].Substring(0, 2));
                                throw new PosControlException();
                            }
                            */
                            if (iData != 9002)
                            {
                                log.Error("Error DirectIO 9002 campo iData, expected 9002, received " + iData);
                                throw new PosControlException();
                            }

                        }
                        // Check RT status , chiedo conferma tramite la 1138
                        strObj[0] = "01";
                        dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                        iData = dirIO.Data;
                        if (iData != 1138)
                        {
                            log.Error("Error DirectIO 1138 campo iData, expected 1138, received " + iData);
                            throw new PosControlException();
                        }
                        iObj = (string[])dirIO.Object;

                        //Controllo che anche la 1138 mi dice che sono in MF ( check quarto e quinto byte 1138 command ,secondo protocollo)
                        string rtType = iObj[0].Substring(3, 2);
                        int rtTypeInt = Convert.ToInt32(rtType);
                        Sub = iObj[0].Substring(5, 2);
                        SubInt = Convert.ToInt32(Sub);
                        if ((rtTypeInt == 1) && (SubInt == 6))
                        {
                            log.Info("Printer is in MF CENSITO Mode");
                        }
                        else
                        {
                            log.Error("Errore sul processo di Disattivazione,la 9002 mi dice che è andata a buon fine ,la 1138 NO!!!");
                            throw new PosControlException();
                        }
                    }
                    else //Se non sono in DemoRT devo fare la 1138 CheckRT Status ancora per capire se sono in RT o MF
                    {
                        // Check RT status 
                        strObj[0] = "01";
                        dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                        iData = dirIO.Data;
                        if (iData != 1138)
                        {
                            log.Error("Error DirectIO 1138 campo iData, expected 1138, received " + iData);
                            throw new PosControlException();
                        }
                        iObj = (string[])dirIO.Object;

                        string rtType = iObj[0].Substring(3, 2);
                        int rtTypeInt = Convert.ToInt32(rtType);
                        //string Sub = iObj[0].Substring(5, 2);
                        //int SubInt = Convert.ToInt32(Sub);
                        if (rtTypeInt == 1)
                        {
                            //Sto già in MF,ergo non devo far nulla
                            log.Info("Printer is in MF mode");
                        }
                        else
                        {
                            //Sono in RT
                            //strObj[0] = "330";
                            //Faccio chiusura prima

                            strObj[0] = "01";
                            DirectIOData dirIO2 = posCommonFP.DirectIO(0, 1074, strObj);
                            iData = dirIO2.Data;
                            //Console.WriteLine("DirectIO(): iData = " + iData);
                            iObj = (string[])dirIO2.Object;


                            dirIO = PosCommonFP.DirectIO(-112, 180000, strObj);
                            fiscalprinter.PrintZReport();
                            string ora = DateTime.Now.ToString("ddMMyyHHmmss");



                            string Text = "                                                                                                fine";
                            strObj[0] = "12NVRNDR59B10F205DIT12825980159                 IT07511580156                 ";
                            strObj[0] += ora + Text;

                            dirIO = PosCommonFP.DirectIO(-112, 180000, strObj);
                            dirIO = posCommonFP.DirectIO(0, 9002, strObj); // RT Service Command (Disattivazione)
                            iData = dirIO.Data;
                            iObj = (string[])dirIO.Object;

                            if (!(String.Equals(iObj[0].Substring(0, 2), "01")))
                            {
                                log.Error("Richiesta Disattivazione Fallita, Error DirectIO 9002 operator, expected 01, received " + iObj[0].Substring(0, 2));
                                throw new PosControlException();
                            }

                            if (iData != 9002)
                            {
                                log.Error("Error DirectIO 9002 campo iData, expected 9002, received " + iData);
                                throw new PosControlException();
                            }


                            // Check RT status , chiedo conferma tramite la 1138
                            strObj[0] = "01";
                            dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                            iData = dirIO.Data;
                            if (iData != 1138)
                            {
                                log.Error("Error DirectIO 1138 campo iData, expected 1138, received " + iData);
                                throw new PosControlException();
                            }
                            iObj = (string[])dirIO.Object;

                            //Controllo che anche la 1138 mi dice che sono in MF ( check quarto e quinto byte 1138 command ,secondo protocollo)
                            //rtType = iObj[0].Substring(3, 2);
                            //rtTypeInt = Convert.ToInt32(rtType);
                            string Sub = iObj[0].Substring(5, 2);
                            int SubInt = Convert.ToInt32(Sub);
                            if (SubInt == 6)
                            {
                                log.Info("Printer is in MF CENSITO Mode");
                            }
                            else
                            {
                                log.Error("Errore sul processo di Disattivazione,la 9002 mi dice che è andata a buon fine ,la 1138 NO!!!");
                                throw new PosControlException();
                            }

                        }

                    }

                    Mode = "MF";
                }
                else if (state.Equals("RT", StringComparison.OrdinalIgnoreCase))
                {
                    // ATTENZIONE,PER POTER SWITCHARE IN RT DEVO FARE PRIMA UNA CHIUSURA GIORNALIERA (PRINTZ REPORT)
                    //Console.WriteLine("Performing PrintZReport() method ");
                    //Aumento il timeout perchè ogni tanto lo ZReport lo esegue ma poi mi va in exception
                    dirIO = PosCommonFP.DirectIO(-112, 30000, strObj);
                    strObj[0] = "01";
                    dirIO = posCommonFP.DirectIO(0, 3001, strObj);

                    //Leggo se attualmente sono in demo mode o meno
                    strObj[0] = "63";
                    dirIO = posCommonFP.DirectIO(0, 4214, strObj);
                    iObj = (string[])dirIO.Object;
                    string status = iObj[0].Substring(2, 1);
                    if (String.Compare(status, "1") == 0) //Vengo dallo stato Demo RT quindi setto a 0 il Flag 63 e vado in Demo off
                    {
                        strObj[0] = "630";
                        dirIO = posCommonFP.DirectIO(0, 4014, strObj);
                        //Aumento profondità di test controllando anche la risposta della 4014 (SET FLAG)
                        iObj = (string[])dirIO.Object;
                        iData = dirIO.Data;
                        if (iData != 4014)
                        {
                            log.Error("Errore Risposta 4014 (SET FLAG), expected 4014, received " + iData);
                            throw new PosControlException();
                        }
                    }

                    //Leggo se attualmente sono in Simulazione mode o meno
                    strObj[0] = "62";
                    dirIO = posCommonFP.DirectIO(0, 4214, strObj);
                    iObj = (string[])dirIO.Object;
                    status = iObj[0].Substring(2, 1);
                    if (String.Compare(status, "1") == 0) //Vengo dallo stato Simulazione quindi metto a 0 il Flag 62 e vado in RT
                    {
                        strObj[0] = "620";
                        dirIO = posCommonFP.DirectIO(0, 4014, strObj);
                        //Aumento profondità di test controllando anche la risposta della 4014 (SET FLAG)
                        iObj = (string[])dirIO.Object;
                        iData = dirIO.Data;
                        if (iData != 4014)
                        {
                            log.Error("Errore Risposta 4014 (SET FLAG), expected 4014, received " + iData);
                            throw new PosControlException();
                        }
                    }
                    //Adesso Non so se sono in MF o RT ,1138 quindi

                    // Check RT status 
                    strObj[0] = "01";
                    dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                    iData = dirIO.Data;
                    if (iData != 1138)
                    {
                        log.Error("Error DirectIO 1138 campo iData, expected 1138, received " + iData);
                        throw new PosControlException();
                    }
                    iObj = (string[])dirIO.Object;
                    string rtType = iObj[0].Substring(3, 2);
                    int rtTypeInt = Convert.ToInt32(rtType);
                    if (rtTypeInt == 2)
                    {
                        //Sto già in RT,ergo non devo far nulla
                        //TODO:03/02/2020 va fatta una chiusura per andare effettivamente in RT
                        //ma io proporrei un messaggio su visore altrimenti ,ad esempio, gli scontrini lotteria non li fa
                        log.Info("Printer is in RT mode");
                        //ZRep
                        fiscalprinter.PrintZReport();
                    }
                    else
                    {
                        //Faccio chiusura prima
                        fiscalprinter.PrintZReport();
                        //Sono in MF Censito,devo andare in RT
                        if (test)
                        {
                            log.Error("Attenzione, la macchina stranamento si trova in MF quando non dovrebbe");
                            throw new Exception();
                        }
                        string ora = DateTime.Now.ToString("ddMMyyHHmmss");



                        string Text = "                                                                                                fine";
                        strObj[0] = "02NVRNDR59B10F205DIT12825980159                 IT07511580156                 ";
                        strObj[0] += ora + Text;

                        dirIO = posCommonFP.DirectIO(0, 9002, strObj); // RT Service Command (Attivazione)
                        iData = dirIO.Data;
                        iObj = (string[])dirIO.Object;

                        if (!(String.Equals(iObj[0].Substring(0, 2), "01")))
                        {
                            log.Error("Richiesta Attivazione Fallita, Error DirectIO 9002 operator, expected 01, received " + iObj[0].Substring(0, 2));
                            throw new PosControlException();
                        }

                        if (iData != 9002)
                        {
                            log.Error("Error DirectIO 9002 campo iData, expected 9002, received " + iData);
                            throw new PosControlException();
                        }

                    }

                    Mode = "RT";
                }
                else if ((state.Equals("Simulazione", StringComparison.OrdinalIgnoreCase)))
                {
                    //Leggo il flag 62
                    //Leggo lo stato passato da cui venivo
                    strObj[0] = "62";
                    dirIO = posCommonFP.DirectIO(0, 4214, strObj); // Leggo il flag 62
                    iObj = (string[])dirIO.Object;
                    string status = iObj[0].Substring(2, 1);
                    if (String.Compare(status, "1") == 0) //Sono in Simulazione, ergo non devo far nulla
                    {
                        //strObj[0] = "631"; //3333 1463 X 
                    }
                    else //Ero in MF o RT,mi basta fare la 3333 1462 X
                    {
                        dirIO = PosCommonFP.DirectIO(-112, 60000, strObj);
                        strObj[0] = "621";
                        dirIO = posCommonFP.DirectIO(0, 4014, strObj); // Scrivo il flag 62
                        //Aumento profondità di test controllando anche la risposta della 4014 (SET FLAG)
                        iObj = (string[])dirIO.Object;
                        iData = dirIO.Data;
                        if (iData != 4014)
                        {
                            log.Error("Errore Risposta 4014 (SET FLAG), expected 4014, received " + iData);
                            throw new PosControlException();
                        }
                    }

                    Mode = "Simulazione";
                }

                //Reset Printer per motivi di sicurezza/sincronismo
                resetPrinter();
            }
            catch (Exception e)
            {
               
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {                 
                    log.Fatal("Generic Error", e);
                }
            }
        }

        //DirectIO 4232
        public int GetEmailParam()
        {
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                log.Info("Performing GetEmailParam");
                for (int i = 1; i < 11; i++)
                //DirectIO 4232 GET EMAIL PARAMETERS       
                {
                    strObj[0] = i.ToString().PadLeft(3, '0');
                    dirIO = posCommonFP.DirectIO(0, 4232, strObj);

                    iData = dirIO.Data;
                    //Console.WriteLine("DirectIO(): iData = " + iData);
                    iObj = (string[])dirIO.Object;
                }

            }
            catch (Exception e)
            {

                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("Generic Error", e);

                }
            }

            return NumExceptions;
        }


        //DirectIO 4032
        public int SetEmailParam(string serverType, string serverName, string portnumber, string username, string password, string mailfrom, string maildest, string mailnotif, string type)
        {

            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                 
                string buffer = " ";

                log.Info("Performing SetEmailParam");
                //DirectIO 4032 SET EMAIL PARAMETERS       
                strObj[0] = "001" + serverType.PadRight(64,' ');
                dirIO = posCommonFP.DirectIO(0, 4032, strObj);

                strObj[0] = "002" + serverName.PadRight(64, ' ');
                dirIO = posCommonFP.DirectIO(0, 4032, strObj);

                strObj[0] = "003" + portnumber.PadRight(64, ' ');
                dirIO = posCommonFP.DirectIO(0, 4032, strObj);

                strObj[0] = "004" + username.PadRight(64, ' ');
                dirIO = posCommonFP.DirectIO(0, 4032, strObj);

                strObj[0] = "005" + password.PadRight(64, ' ');
                dirIO = posCommonFP.DirectIO(0, 4032, strObj);

                strObj[0] = "006" + "3"  + buffer.PadRight(63, ' ');
                dirIO = posCommonFP.DirectIO(0, 4032, strObj);

                strObj[0] = "007" + mailfrom.PadRight(64, ' ');
                dirIO = posCommonFP.DirectIO(0, 4032, strObj);

                strObj[0] = "008" + mailfrom.PadRight(64, ' ');
                dirIO = posCommonFP.DirectIO(0, 4032, strObj);

                strObj[0] = "009" + maildest.PadRight(64, ' ');
                dirIO = posCommonFP.DirectIO(0, 4032, strObj);

                strObj[0] = "010" + type.PadRight(64, ' ');
                dirIO = posCommonFP.DirectIO(0, 4032, strObj);

                iData = dirIO.Data;
                
                iObj = (string[])dirIO.Object;

            }
            catch (Exception e)
            {

                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("Generic Error", e);

                }
            }

            return NumExceptions;
        }

        //DirectIO 1146 Send Fiscal Document Via Mail 
        public int SendFiscalDocViaEmail(string ENA, string Sel, string addr)
        {
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                string Spare = " ";
                log.Info("Performing SendFiscalDocViaEmail");
                //DirectIO 1146 Send Fiscal Document Via Mail  
                strObj[0] = "01" + "00" + ENA + Sel.PadLeft(2, '0') + addr.PadRight(64, ' ') + Spare.PadRight(64, ' ');
                dirIO = posCommonFP.DirectIO(0, 1146, strObj);

                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;

            }
            catch (Exception e)
            {

                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("Generic Error", e);

                }
            }

            return NumExceptions;
        }

    }


    //Classe creata per la gestione della Lotteria
    public class Lottery : FiscalReceipt
    {
        //Variabile statica che memorizza lo stato di attivazione del checksum, di default è true
        public static bool checkSumFlag = false;

        //Lottery base class
        public Lottery()
        {
            try
            {
                /*
                posExplorer = new PosExplorer();
                // Console.WriteLine("Taking FiscalPrinter device ");
                DeviceInfo fp = posExplorer.GetDevice("FiscalPrinter", "FiscalPrinter1");

                posCommonFP = (PosCommon)posExplorer.CreateInstance(fp);
                //posCommonFP.StatusUpdateEvent += new StatusUpdateEventHandler(co_OnStatusUpdateEvent);
                
                */
                if (!opened)
                {
                    fiscalprinter = (FiscalPrinter)posCommonFP;
                    //Console.WriteLine("Performing Open() method ");
                    fiscalprinter.Open();

                    //Console.WriteLine("Performing Claim() method ");
                    fiscalprinter.Claim(1000);

                    //Console.WriteLine("Setting DeviceEnabled property ");
                    fiscalprinter.DeviceEnabled = true;

                    //Console.WriteLine("Performing ResetPrinter() method ");
                    //fiscalprinter.ResetPrinter();
                }

                //Console.WriteLine("Performing ResetPrinter() method ");
                //fiscalprinter.ResetPrinter();


            }
            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    Console.WriteLine(e.ToString());
                    log.Fatal("Generic Error", e);
                }
            }
        }


        //Metodo che valida l'XML creato per la lotteria relativo al XSD associato
        public int XmlLotteryValidate()
        {

            //Qui ci metto la parte di codice che controlla l'xml con l'xsd dei corr. 2.0
            //Diciamo che per ora lo creo io a mano l'xml e poi uso sto metodo per parsarlo
            //e per controllare che rispetti le regole dell'XSD

            try
            {
                
                // define the settings that I use while reading the XML file.
                XmlReaderSettings settings;

                //parsing della directory XmlFolder con tutti i file xml di test
                string[] fileArray = Directory.GetFiles(@"D:\Epson_Copia_Chiavetta_Gialla2\ToolAggiornato\PosTestWithNunit\FiscalReceipt\XmlLotteriaDaTestare", "*.xml", SearchOption.TopDirectoryOnly);
                //string[] fileArray = Directory.GetFiles(@"C:\Users\BVittorino\Downloads\Chiusure Rifiutate\", "*.xml", SearchOption.TopDirectoryOnly);


                // XSD
                
                settings = new XmlReaderSettings();
                
                /*
                settings.Schemas.Add(null, @"D:\Epson_Copia_Chiavetta_Gialla2\Documentazione Progetto\allegati finale new_5\DocCommercialiLotteriaTypes_v1.0.xsd");
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(this.ValidationEventHandle);
                */


                foreach (string namefile in fileArray)
                {

                    try
                    {

                        string xmlString = System.IO.File.ReadAllText(namefile); 

                        // Encode the XML string in a UTF-8 byte array
                        byte[] encodedString = Encoding.UTF8.GetBytes(xmlString);

                        // Put the byte array into a stream and rewind it to the beginning
                        MemoryStream ms = new MemoryStream(encodedString);
                        ms.Flush();
                        ms.Position = 0;

                        // Build the XmlDocument from the MemorySteam of UTF-8 encoded bytes
                        XmlDocument xmld = new XmlDocument();
                        xmld.Load(ms);






                        
                        xmld.Schemas.Add(null, @"D:\Epson_Copia_Chiavetta_Gialla2\Documentazione Progetto\allegati finale new_5\DocCommercialiLotteriaTypes_v1.0.xsd");
                        settings.ValidationType = ValidationType.Schema;
                        settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
                        settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
                        settings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(this.ValidationEventHandle);
                        xmld.Validate(this.ValidationEventHandle);
                        
                    }
                    catch (Exception e)
                    {
                        log.Fatal("Generic Error: ", e);
                        
                    }


                }




            }
            catch (Exception e)
            {
                NumExceptions++;
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                }
                else
                {
                    log.Fatal("Generic Error: ", e);
                }

            }
            return NumExceptions;
        }

        private void ValidationEventHandle(object sender, ValidationEventArgs arg)
        {
            //If we are here, it's because something is going wrong with my XML.
            log.Error("\r\n\t Validation XML failed: " + arg.Message);

            // throw an exception.
            throw new Exception("Validation XML failed: " + arg.Message);
        }




        //DirectIO 1135
        //Send the Lottery Personal ID CODE
        public int SendLotteryCode(string IdLotteryCode, string Operator)
        {

            log.Info("Performing SendLotteryCode Method");
            try
            {
                
                fiscalprinter.BeginFiscalReceipt(true);
                //fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                //fiscalprinter.PrintRecItem("TSHIRT", (decimal)10000, (int)1000, (int)2, (decimal)1200, "");
                //fiscalprinter.PrintRecItem("CAPPELLO", (decimal)10000, (int)1000, (int)3, (decimal)38000, "");
                fiscalprinter.PrintRecItem("BORSA", (decimal)100, (int)1000, (int)1, (decimal)100, "");

                //301TICKET - 000CONTANTI
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                /*
                //Set Header Line
                strObj[0] = "26003";
                dirIO = posCommonFP.DirectIO(0, 4015, strObj);
                */

                string op = Operator.PadLeft(2, '0');  //da 01 a 99AAAAAAAE

                //IdLotteryCode da 2 a 15 byte,poi ci devo addare un terminatore stringa quindi 16
                string IDCODE = IdLotteryCode;
                //string IDCODE = 0x0f.ToString() + Console.ReadLine() + 0x0f.ToString();

                //TODO 31/01/20 
                //analizzo il checksum per vedere se è giusto o errato: i test cambiano in base a ciò
                bool checkSumResult = CheckSumAlgorithm(IDCODE);
                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                //Check risposta alla DirectIO 1135
                if ((iData != 1135) && (String.Compare(Mode, "RT") == 0) && (checkSumResult) && (checkSumFlag))
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                    NumExceptions++;
                }
                if((String.Compare(Mode, "RT") == 0) && (!checkSumResult) && (checkSumFlag) && (iData != 0) )
                {
                    log.Error("Error DirectIO 1135, expected iData 0, received : " + iData);
                }
                //TODO : 16/01/20 se il lottery code è errato il test fallisce in RT perchè restituisce 30 invece di 01 ergo sto test lo devo togliere
                //TODO : 30/01/20 refactorizzato con l'intro dell algoritmo di checksum per cui posso gestire anche l'errore 30
                //In RT deve dare 01 else error
                
                if (!(String.Equals(iObj[0].Substring(0, 2), "01")) && (String.Equals(FiscalReceipt.Mode , "RT")) && (checkSumResult) && (checkSumFlag))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                    NumExceptions++;
                    //TODO : 31/01/20 decidere se andare in exception o meno, ma mi sa di si 
                }
                //
                if (!(String.Equals(iObj[0].Substring(0, 2), "30")) && (String.Equals(FiscalReceipt.Mode, "RT")) && !(checkSumResult) && (checkSumFlag))
                {
                    log.Error("Richiesta Invio Lottery Code con CheckSum sbagliato, Error DirectIO 1135 operator, expected 30, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                    NumExceptions++;
                    //TODO : 31/01/20 decidere se andare in exception o meno, ma mi sa di si 
                }


                // In MF deve restituire 17 (impossibile ora) else error
                if ((!(String.Equals(iObj[0].Substring(0, 2), "17")) && (!(String.Equals(iObj[0].Substring(0, 2), "30")))) && ((String.Equals(FiscalReceipt.Mode, "MF")) || (String.Equals(FiscalReceipt.Mode, "Demo"))))
                {
                    log.Error("In Mode " + Mode + " Invio Lottery Personal ID CODE anomalo, Error DirectIO 1135 operator, expected 17 o 30, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                    NumExceptions++;
                }

                fiscalprinter.EndFiscalReceipt(false);

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("Generic Error", e);

                }
            }
            return NumExceptions;

        }

        //Test sull' annullo comando 1135 (Send Lottery Code)
        //si puo fare in due modi : mandando un 1135 con 16 blank o con una 1088(reset printer)

        public int TestResetLotteryCode(string IdLotteryCode, string Operator)        
        {

            log.Info("Performing TestResetLotteryCode Method");
            try
            {

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Test 1", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");


                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

              
                string op = Operator.PadLeft(2, '0');  //da 01 a 99AAAAAAAE

                //IdLotteryCode da 2 a 15 byte,poi ci devo addare un terminatore stringa quindi 16
                string IDCODE = IdLotteryCode;
                //string IDCODE = 0x0f.ToString() + Console.ReadLine() + 0x0f.ToString();

                //Controllo checksum dell ' IDCODE
                bool checkSumResult = CheckSumAlgorithm(IDCODE);
                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                //Annullo comando lotteria con una DirectIO 1135 con 16 blank
                strObj[0] = op + "        " + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                fiscalprinter.EndFiscalReceipt(false);


                //Ripeto il test con la DirectIO 1088 (reset)
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Test 2", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");

                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                fiscalprinter.ResetPrinter();
                

                //verifico che il comando lotteria precedentemente annullato non vada a finire nello scontrino successivo
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Test 3", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                //Ripeto il test provando il comando 1135 prima della BeginFiscalReceipt
                strObj[0] = op + IDCODE + "        " + "0000";
                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                //Ripeto il test con la DirectIO 1088 (reset)
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Test 4", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                fiscalprinter.ResetPrinter();


                //verifico che il comando lotteria precedentemente annullato non vada a finire nello scontrino successivo
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Test 5", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                //Ripeto il test con la printrecVoid dopo il comando lotteria
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Test 6", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");

                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                fiscalprinter.PrintRecVoid("CANCELRECEIPT");
                fiscalprinter.EndFiscalReceipt(false);

                
                //Testo che dopo l'annullo scontrino precedente non mandi il comando lotteria allo scontrino seguente per errore
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Test 7", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);



                //Ripeto il test provando il comando 1135 e poi la ResetPrinter prima della BeginFiscalReceipt
                //per verificare che lo scontrino non abbia cmq il codice lotteria all'interno: test modifica nuovo fw
                strObj[0] = op + IDCODE + "        " + "0000";
                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                fiscalprinter.ResetPrinter();

                //Ripeto il test con la DirectIO 1088 (reset)
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Test 8", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);

                fiscalprinter.PrintZReport();


            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("Generic Error", e);

                }
            }
            return NumExceptions;
        }



        private bool CheckSumAlgorithm(string LotteryCode)
        {
            log.Info("Performing ChecksumAlgorithm Method ");
            try
            {
                
                byte[] lotterycode = Encoding.ASCII.GetBytes(LotteryCode);
                int counter = 0;
                for(int i = 0; i < lotterycode.Length-1; i++)
                {
                    //se l'indice è pari
                    if((i + 1) % 2 == 0)
                    {
                        //se è un numero il check digit è uguale al numero stesso
                        if (lotterycode[i] >= '0' && lotterycode[i] <= '9')
                        {
                            counter += lotterycode[i];
                        }
                        else
                        // allora è una lettera (obbligatoria maiuscola)
                        {
                            counter += (lotterycode[i] - 65);
                        }
                    }
                    else
                    //se l'indice è dispari non c'è una regola, devo usare uno switch
                    {
                        switch((char) lotterycode[i])
                        {
                            case 'A':
                                counter += 1;
                                break;
                            case '0':
                                counter += 1;
                                break;
                            case 'B':
                                counter += 0;
                                break;
                            case '1':
                                counter += 0;
                                break;
                            case 'C':
                                counter += 5;
                                break;
                            case '2':
                                counter += 5;
                                break;
                            case 'D':
                                counter += 7;
                                break;
                            case '3':
                                counter += 7;
                                break;
                            case 'E':
                                counter += 9;
                                break;
                            case '4':
                                counter += 9;
                                break;
                            case 'F':
                                counter += 13;
                                break;
                            case '5':
                                counter += 13;
                                break;
                            case 'G':
                                counter += 15;
                                break;
                            case '6':
                                counter += 15;
                                break;
                            case 'H':
                                counter += 17;
                                break;
                            case '7':
                                counter += 17;
                                break;
                            case 'I':
                                counter += 19;
                                break;
                            case '8':
                                counter += 19;
                                break;
                            case 'J':
                                counter += 21;
                                break;
                            case '9':
                                counter += 21;
                                break;
                            case 'K':
                                counter += 2;
                                break;
                            case 'L':
                                counter += 4;
                                break;
                            case 'M':
                                counter += 18;
                                break;
                            case 'N':
                                counter += 20;
                                break;
                            case 'O':
                                counter += 11;
                                break;
                            case 'P':
                                counter += 3;
                                break;
                            case 'Q':
                                counter += 6;
                                break;
                            case 'R':
                                counter += 8;
                                break;
                            case 'S':
                                counter += 12;
                                break;
                            case 'T':
                                counter += 14;
                                break;
                            case 'U':
                                counter += 16;
                                break;
                            case 'V':
                                counter += 10;
                                break;
                            case 'W':
                                counter += 22;
                                break;
                            case 'X':
                                counter += 25;
                                break;
                            case 'Y':
                                counter += 24;
                                break;
                            case 'Z':
                                counter += 23;
                                break;
                        }
                    }
                }

                counter = counter % 26;
                char checkcounter = (char)(counter + 65);
                if (checkcounter - lotterycode[lotterycode.Length - 1 ] == 0)
                {
                    return true; //checksum corretto
                }
                else
                {
                    return false; //checksum errato
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("Generic Error", e);

                }
            }
            return false;
        }


        //DirectIO 4015
        //Set Configuration
        //TODO: controllare se e dove lo uso: lo uso per es x l' arrotondamento (index 27, Val 1 o 2 o 3)
        public int SetConfiguration(string index , string Val)
        {
            try
            {
                log.Info("Performing SetConfiguration Method");
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                if (String.Compare(index, "27") == 0)
                {
                    if ((Convert.ToInt32(Val) < 0 || Convert.ToInt32(Val) > 3))
                    {
                        log.Error("I valori di settaggio sono fuori dal range (0-3)");
                    }

                }

                //Set Header Line
                strObj[0] = index.PadLeft(2,'0') + Val.PadLeft(3, '0');
                dirIO = posCommonFP.DirectIO(0, 4015, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
              
                if (Convert.ToInt32(iObj[0]) < 1 || Convert.ToInt32(iObj[0]) > 12)
                {
                    log.Error("Error DirectIO 4015 campo OP, valore fuori range, accettato tra 01 e 12 , ricevuto: " + iObj[0]);
                    throw new PosControlException();
                }
               
                
            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error", e);
                }
            }
            return NumExceptions;
        }
        

        //Test creato perchè ho scoperto che ogni tanto la 1135 mi restituisce codice 17 ma invece lo scontrino con lott lo fa
        // Edit: da demo ad RT la stampante è in RT uffi
        public int StressTestLotteryCode()
        {
            log.Info("Performing StressTestLotteryCode Method");
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                strObj[0] = "01";
                for (int i = 0; i < 10; i++)
                {
                    dirIO = PosCommonFP.DirectIO(-112, 60000, strObj);
                    strObj[0] = "01";
                    dirIO = posCommonFP.DirectIO(0, 3001, strObj);
                    SendLotteryCode("ABCDEFGN", "01");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {
                    Console.WriteLine(e.ToString());
                    log.Fatal("Generic Error", e);
                }
            }
            return NumExceptions;

        }
        //DirectIO 1135
        //Send the Lottery Personal ID CODE with Mixed Payment Form
        public int SendLotteryCodePagMisto(string IdLotteryCode, string Operator)
        {
            log.Info("Performing SendLotteryCodePagMisto Method");
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                string op = Operator.PadLeft(2, '0');  //da 01 a 99
                //IdLotteryCode da 2 a 15 byte,poi ci devo addare un terminatore stringa quindi 16
                string IDCODE = IdLotteryCode;

                
                //Invio Scontrino con Lotteria
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("TEST CONTANTE", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "000CONTANTI");
                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                //Check risposta alla DirectIO 1135
                if ((iData != 1135) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }
                if (!(String.Equals(iObj[0].Substring(0, 2), "01")) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }

                fiscalprinter.EndFiscalReceipt(false);


                //Invio Scontrino con Lotteria
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("TEST CREDITO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)500000, "200CREDITO");
                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                //Check risposta alla DirectIO 1135
                if ((iData != 1135) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }
                if (!(String.Equals(iObj[0].Substring(0, 2), "01")) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }

                fiscalprinter.EndFiscalReceipt(false);


                //Invio Scontrino con Lotteria
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("TEST ASSEGNO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "100ASSEGNO");
                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                //Check risposta alla DirectIO 1135
                if ((iData != 1135) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }
                if (!(String.Equals(iObj[0].Substring(0, 2), "01")) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }


                fiscalprinter.EndFiscalReceipt(false);


                //Invio Scontrino con Lotteria
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("TEST CARTA DI CREDITO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "201CARTA DI CREDITO");
                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                //Check risposta alla DirectIO 1135
                if ((iData != 1135) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }
                if (!(String.Equals(iObj[0].Substring(0, 2), "01")) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }

                fiscalprinter.EndFiscalReceipt(false);


                //Invio Scontrino con Lotteria
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("TEST ALTRO PAGAMENTO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "203ALTRO PAGAMENTO");
                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                //Check risposta alla DirectIO 1135
                if ((iData != 1135) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }
                if (!(String.Equals(iObj[0].Substring(0, 2), "01")) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }

                fiscalprinter.EndFiscalReceipt(false);


                //Invio Scontrino con Lotteria
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("TEST BANCOMAT", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "204BANCOMAT");
                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                //Check risposta alla DirectIO 1135
                if ((iData != 1135) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }
                if (!(String.Equals(iObj[0].Substring(0, 2), "01")) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }


                fiscalprinter.EndFiscalReceipt(false);


                //Invio Scontrino con Lotteria
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("TEST TICKET", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "301TICKET");
                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                //Check risposta alla DirectIO 1135
                if ((iData != 1135) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }
                if (!(String.Equals(iObj[0].Substring(0, 2), "01")) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }

                fiscalprinter.EndFiscalReceipt(false);
                


                //Invio Scontrino con Lotteria E PAGAMENTO MULTIPLI!!!
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("TEST PAGANENTO MULTIPLI", (decimal)430000, (int)1000, (int)1, (decimal)4300000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "000CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "200CREDITO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "100ASSEGNO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "201CARTA DI CREDITO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "203ALTRO PAGAMENTO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "204BANCOMAT");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)00000, "301TICKET");

                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                //Check risposta alla DirectIO 1135
                if ((iData != 1135) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }
                if (!(String.Equals(iObj[0].Substring(0, 2), "01")) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }

                fiscalprinter.EndFiscalReceipt(false);



                //Invio Scontrino con Lotteria E PAGAMENTO MULTIPLI!!!
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("TEST PAGANENTO STRANO", (decimal)430000, (int)1000, (int)1, (decimal)4300000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "000CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "200CREDITO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)00000, "301TICKET");

                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                //Check risposta alla DirectIO 1135
                if ((iData != 1135) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }
                if (!(String.Equals(iObj[0].Substring(0, 2), "01")) && (String.Equals(Mode, "RT")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }

                fiscalprinter.EndFiscalReceipt(false);
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("Generic Error", e);

                }
            }

            return NumExceptions;

        }

        //DirectIO 1134 Read lottery status
        //input: 
        // string date: data della richiesta GGMMYY
        // string KindOfRequest: tipo di richiesta (da implementare ancora , 00 fixed!) 
        //output:
        // int NumExceptions: numero totale di eccezioni contate
        //EDIT: quello del 15/11/19 Zrep 2414 contiene gli scartati per data futuro (se volessi parsarli)
        public int ReadLotteryStatus(string data, string zrep,string tillID,  ref string numlotok, ref string numlotrej)
        {        
            log.Info("Performing ReadLotteryStatus Method ");
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                string date = data;
                bool last_release = true;
                string zRepNum;

                string idCassa = tillID.PadLeft(8,'0');
                //se non lo fornisco prendo l'ultimo da chiudere
                //EDIT: 18/12/19 prendo l'ultimo CHIUSO!!!
                if (String.Compare(zrep, " ") == 0)
                {
                    zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                    int nextInt = Int32.Parse(zRepNum);
                    zRepNum = nextInt.ToString("0000");
                }
                else
                {
                    zRepNum = zrep;
                }
                //string kindOfRequest = kos;
                string kindOfRequest = "00";
                if (last_release)
                {
                    strObj[0] = "01" + idCassa + zRepNum + date + kindOfRequest;
                }
                else
                {
                    strObj[0] = "01" + idCassa + date  + kindOfRequest;
                }
                dirIO = posCommonFP.DirectIO(0, 1134, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                if (iData != 1134)
                {
                    log.Error("Errore DirectIO 1134 campo iData, expected " + 1134 + " received " + iData);
                    throw new PosControlException();

                }

                // Fare il check di questo campo
                // Indipendente dalla data
                string filesToSend = iObj[0].Substring(22, 4);

                // Fare il check di questo campo
                // Indipendente dalla data
                string oldFiles = iObj[0].Substring(26, 4);

                // Fare il check di questo campo
                // Indipendente dalla data
                string rejFiles = iObj[0].Substring(30, 4);

                // Lottery in pancia
                string arrayReceipts = iObj[0].Substring(34, 4);

                // numero scontrini lotteria nella cartella TO SEND per quella data e Zrep
                string lotteryToSend = iObj[0].Substring(38, 4);

                // Numero scontrini accettati dal TA per quel ZRep e Date
                // Confrontare questo valore con il numero degli scontrini ok del parser nella stessa data (obv)
                string numLotteryOk = iObj[0].Substring(42, 4);

                //Restituisco il valore in uscita che utilizzero' nel GestioneACResponse
                numlotok = numLotteryOk;

                // Numero scontrini rigettati dal TA per quel ZRep e Date
                string numLotteryRej = iObj[0].Substring(46, 4);
                //restituisco il valore in uscita che utilizzerò nel GestioneSEResponse
                numlotrej = numLotteryRej;

            }
            catch(Exception e)
            {
                NumExceptions++;
                
                //fiscalprinter.ClearOutput();
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                    if (String.Compare(pce.Message , "Stub message. Timeout") == 0) //Timeout vuol dire che ha crashato qui, provo a ristabilire la connessione
                    {
                        opened = false;
                        //todo gestire questa cosa
                        //non sono sicuro se chiamare la init o meno qui, mettendo opened = false dovrebbe bastare
                        //FiscalReceipt.initFiscalDevice("FiscalPrinter");
                    }

                }
                else
                {
                    log.Fatal("Generic Error: " , e);
                }
            }

            return NumExceptions;

        }


        //DirectIO 9016 Move Rejected file to history folder
        //input: 
        // string FolderType: 00 = ZReport , 01 = Lottery
        //output:
        // int NumExceptions: numero totale di eccezioni contate
        
        //EDIT: 26/11/19 Metodo sperimentale provvisorio:
        //Per adesso lo modifico perché mi servono un po' di ZRep rifiutati in modo da 
        //creare un po' di file dentro l'archivio rifiutati. L' obiettivo è ricreare
        //il timeout del driver e guardare con wireshark che succede
        public int MoveRejectedFiles(string folderType)
        {
            log.Info("Performing MoveRejectedFiles Method ");
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                /*
                //Target : creo svagonata di Zrep rifiutati spostando l' orario
                //Step 1: Spostare l'orario in avanti di 6 ore 
                fiscalprinter.PrintZReport();
                strObj[0] = DateTime.Now.AddHours(6).ToString("ddMMyyHHmm");
                dirIO = posCommonFP.DirectIO(0, 4001, strObj);
                iData = dirIO.Data;
                if( iData != 4001)
                {
                    log.Error("Error DirecIO 4001 , tentativo fallito di settare la data avanti di 6 ore");
                    NumExceptions++;
                    //throw new PosControlException();

                }

                //Step :2 
                //stampo scontrini e mando ZRep che dovrebbero tutti essere spostati
                for (int i = 0; i < 50; ++i)
                {
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                    dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                    fiscalprinter.EndFiscalReceipt(true);
                    fiscalprinter.PrintZReport();
                }
                */
                //Step 3: Sposto i file scartati nell'archivio e vediamo se genera Timeout e quanto tempo ci mette
                //string FolderType = folderType.PadLeft(2, '0');
                string FolderType = folderType;
                strObj[0] = FolderType;

                //log.Error("Debug, Start DirectIO 9016");
                dirIO = posCommonFP.DirectIO(0, 9016, strObj);
                //log.Error("Debug, Finish DirectIO 9016");
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

            }
            catch(Exception e)
            {
                NumExceptions++;
                //fiscalprinter.ResetPrinter();
                if(e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                }
                else
                {
                    log.Fatal("Generic Error: ", e);
                }

            }

            return NumExceptions;
        }




        //02/03/2020
        //Metodo che parsa il dgfe come l'analogo metodo della classe Fiscalreceipt ma in questo caso
        //cerca gli scontrini lotteria. Attenzione a non prendere resi o annulli di scontrini lotteria
        //Una volta identificati per ogni ZRep li do in pasto al metodo ReadLotteryStatusSingleReceipt
        //mi serve perchè devo testare gli scontrini lotteria cosi come vengono stampati e non solo dal lato
        //XML inviati e ricevuti 


        public int readDGFEForLottery(string data1 = " ", string data2 = " ")
        {

            log.Info("Performing readDGFEForLottery() MEthod");
            // Write the string to a file in append mode
            System.IO.StreamWriter file = new System.IO.StreamWriter("LotteryReceiptCheck.txt", true);

            try
            {
                //array per memorizzare gli scontrini Lotteria catalogati per ZReport
                ZrepRange[] arr = new ZrepRange[3650];

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                //flag che mi indica qual è il primo scontrino della giornata 
                Boolean isFirst = false;

                string strData = "";
                string date1, date2;
                //flag per identificare un reso/annullo
                bool isResoannullo = false;
                //flag per identificare uno scontrino lotteria
                bool isLottery = false;
                //flag per identificare uno scontrino fiscale
                bool isFiscalDoc = false;


                string FRN = "";
                //string strDate = "";
                string LN = "";
                string TEXT = "";

                string lines = "";

                //questa poi va cambiata e chiesta come input oppure la lascio cosi' e mi prendo sempre il giorno odierno
                if (String.Compare(data1, " ") == 0)
                {
                    //Se non gli passo nulla allora prendo la data odierna , else la data richiesta
                    date1 = DateTime.Today.ToString("ddMMyy");
                    date2 = date1;
                }
                else
                {
                    date1 = data1.Substring(4,2) + data1.Substring(2, 2) + data1.Substring(0,2);
                    if (String.Compare(data2, " ") == 0)
                    {
                        data2 = DateTime.Today.ToString("ddMMyy");
                    }
                    else
                    {
                        date2 = data2.Substring(4, 2) + data1.Substring(2, 2) + data1.Substring(0, 2); ;
                    }
                }


                //parte di codice comune con il parsing DGFE ma da settare custom


                // READ FROM EJ BY DATE AND TYPE (Sono incluse anche le fatture qui,equivale al ZRep 99)

                log.Info("DirectIO (READ FROM EJ BY DATE AND TYPE) 3103");
                strObj[0] = "01" + "1" + "0" + date1 + date1 + "0" + "00";

                dirIO = posCommonFP.DirectIO(0, 3103, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                string dateold = "";
                string FRNold = "";
                string date = "";
                int Zold = 0;
                string annos = "";
                int Lastreceipt = 0;
                int Zrep = 0;
                int Fiscalreceipt = 0;

                //Il metodo è ricorsivo , se chiamo qui la clear perdo tutti i dati dai chiamanti
                //WebService.listOfLottery.Clear();
              

                if (iObj[0].Length > 2)
                {

                    while (iObj[0].Length > 2)
                    {

                        date = iObj[0].Substring(2, 6);
                        if (string.Compare(date, dateold) != 0)
                            lines += "date : " + date + "\r\n";


                        FRN = iObj[0].Substring(8, 4);
                        if (string.Compare(FRN, FRNold) != 0)
                            lines += "Current Fiscal Receipt Number : " + FRN + "\r\n";


                        LN = iObj[0].Substring(12, 4);
                        lines += "Line Seq Num : " + LN + "\r" + " ";

                        TEXT = iObj[0].Substring(16, 46);

                        lines += "EJ line text : " + TEXT + "\r\n";

                        //Identifico un doc di reso o un annullo lotteria o non lotteria
                        if ((String.Compare(TEXT.Substring(25, 10) , "RESO MERCE") == 0) || (String.Compare(TEXT.Substring(25, 12), "ANNULLAMENTO") == 0) )
                        {
                            isResoannullo = true;
                        }


                        //Purtroppo a causa della struttura del layout se trovo "DOCUMENTO N." non so ancora se è uno scontr. lotteria o meno,dannazione
                        //Quindi mi prendo i dati ma solo dopo devo far partire il test se e solo se isResoannullo = false && isLottery = true

                        if (String.Compare(TEXT.Substring(12, 12), "DOCUMENTO N.") == 0)
                        {
                            //E' un doc fiscale ma non mi basta,devo escluderlo se è un reso/annullo + non è lotteria (non è proprio cosi,devo salvarlo cmq )
                            Zrep = Convert.ToInt32(TEXT.Substring(25, 4));
                            Fiscalreceipt = Convert.ToInt32(TEXT.Substring(30, 4));
                            annos = "20" + date.Substring(4, 2);
                            isFiscalDoc = true;

                            WebService.listOfLottery.Add(new WebService.LotteryStruct { _zrep = Zrep.ToString().PadLeft(4, '0'), _numScon = Fiscalreceipt.ToString().PadLeft(4, '0'), IsLottery = false, Date = date, CodError = "FFFFF", Result = "05" });

                        }

                        int last = WebService.listOfLottery.Count;

                        //Se trovo un reso o un annullo lotteria non mi interessa identificarlo
                        if (String.Compare(TEXT.Substring(0, 15), "Codice Lotteria") == 0)
                        {
                            isLottery = true;
                            
                            WebService.listOfLottery[last-1].IsLottery = true;
                        }



                        if ((isFiscalDoc) && (!isResoannullo) && (isLottery))
                        {
                            //Ho trovato uno scontrino lotteria , lo testo per far si che:
                            //1) Chiamo la 9218 per verificare che il comando non mi dia result 05 (not found)
                            //TODO  03/03/20 
                            //2) piu' avanti provero' ad innestare questo routine nel LotterySmartParser riempiendo la listOfLottery List da qui e non dal metodo LotteryFolderSentParser
                            // Se uno dei due test fallisce log.Error(Zrep e Fiscalreceipt)


                            /*

                            //Inizia uno Zrep nuovo ergo devo aggiornare l'array con i limiti di Zold
                            if ((Zrep != Zold) && (Zold != 0)) //cambio di Zrep e non è il primo
                            {
                               
                                arr[Zold].Zrep = Zold;
                                arr[Zold].finish = Lastreceipt;
                                arr[Zold].date = date.Substring(0, 6); // + annos; //questo non cambia perchè siamo sempre nella stessa data
                                if (!(isFirst)) //è il primo e anche l'unico quindi è inutile mettere il flag a true
                                {
                                    isFirst = true;
                                    arr[Zrep].start = Fiscalreceipt;
                                }
                                Zold = Zrep;
                                Lastreceipt = Fiscalreceipt;
                                //isFirst = false;
                                arr[Zrep].Zrep = Zrep;
                                arr[Zrep].start = Fiscalreceipt;
                                arr[Zrep].date = date.Substring(0, 6); // + annos;
                            }
                            else
                            {
                                if ((Zrep != Zold) && (Zold == 0)) //E' il primo Zrep
                                {
                                    Zold = Zrep;
                                    Lastreceipt = Fiscalreceipt;
                                    arr[Zold].finish = Lastreceipt;
                                    arr[Zold].Zrep = Zold;
                                    arr[Zold].date = date.Substring(0, 6); // + annos;
                                    if (!(isFirst)) //è il primo 
                                    {
                                        isFirst = true;
                                        arr[Zrep].start = Fiscalreceipt;
                                    }

                                }
                                else
                                if (Zrep == Zold) //Sta loopando sullo stesso Zrep
                                {
                                    if (!(isFirst))
                                    {
                                        isFirst = true;
                                        arr[Zrep].start = Fiscalreceipt;
                                    }
                                    Lastreceipt = Fiscalreceipt;
                                }
                            }

                        */
                            string resultCode = String.Empty;
                            string IdAnswer = String.Empty;
                            ReadLotteryStatusSingleReceipt(WebService.listOfLottery[last - 1].GetZRep.PadLeft(4, '0'), WebService.listOfLottery[last - 1].GetNumScont.PadLeft(4, '0'), WebService.listOfLottery[last - 1].GetTillID.PadLeft(8, '0'), WebService.listOfLottery[last - 1].GetDate, "00", ref resultCode, ref IdAnswer);
                            if (String.Compare(resultCode, "05") == 0)
                            {
                                log.Error("Errore DirectIO 9218 su uno scontrino lotteria con ZREP :=  " + WebService.listOfLottery[last - 1].GetZRep.PadLeft(4, '0') + " Numero Scontrino := " + WebService.listOfLottery[last - 1].GetNumScont.PadLeft(4, '0') + " in Data := " + WebService.listOfLottery[last - 1].GetDate);
                                log.Error("Expected Result Code := 00 or 01 or 02 or 03 or 04 , received := " + resultCode);
                                //throw new PosControlException();
                            }
                            //Azzero i flag per i prossimi scontrini da parsare dal DGFE
                            isResoannullo = false;
                            isLottery = false;
                            isFiscalDoc = false;

                        }


                        
                        lines += "\r\n";
                        file.WriteLine(lines);

                        lines = "";

                        strObj[0] = "01" + "1" + "0" + date1 + date1 + "1" + "00";

                        dirIO = posCommonFP.DirectIO(0, 3103, strObj);
                        iData = dirIO.Data;
                        iObj = (string[])dirIO.Object;

                        FRNold = FRN;
                        dateold = date;
                    }

                    //sta parte di codice non mi serve + ormai
                    /*
                    //ultimo ZReport del giorno
                    arr[Zold].Zrep = Zold;
                    arr[Zold].finish = Lastreceipt;
                    arr[Zold].date = date.Substring(0, 6); // + annos;

                    for (int i = 0; i < 3650; i++)
                    {   //Per ogni Zrep
                        if (arr[i].Zrep != 0)
                        {
                            string resultCode = ""; 

                            for (int j = arr[i].start; j <= arr[i].finish; ++j)
                            {
                                //per ogni scontrino lotteria all'interno di quello specifico Zrep controllo la 9218 che non ritorni mai 05
                                //TODO: 04/03/20 praticamente qui devo chiamare un metodo alternativo al WebService.LotteryFolderSentParser
                                //In quel metodo mi ricostruivo la struttura degli ZRep dal file XML, in questo caso faccio + o - la stessa cosa ma dal DGFE
                                //TestScontrinoFiscaleFromEJ(arr[i].Zrep.ToString().PadLeft(4, '0'), j.ToString().PadLeft(4, '0'), arr[i].date);
                                ReadLotteryStatusSingleReceipt(arr[i].Zrep.ToString().PadLeft(4, '0'), j.ToString().PadLeft(4, '0'), arr[i].date, "00",  ref resultCode);
                                if(String.Compare(resultCode, "05") == 0)
                                {
                                    log.Error("Errore DirectIO 9218 su uno scontrino lotteria con ZREP :=  " + arr[i].Zrep.ToString().PadLeft(4, '0') + " Numero Scontrino := " + j.ToString().PadLeft(4, '0') + " in Data := " + arr[i].date);
                                    log.Error("Expected Result Code := 00 or 01 or 02 or 03 or 04 , received := " + resultCode);
                                    //throw new PosControlException();
                                }

                            }
                        }

                    }
                    */

                    //chiudo il file descriptor senno la ricorsione non mi funziona, tanto cmq lo riapro in append mode
                    file.Close();
                    int giorno = Convert.ToInt32(date1.Substring(0, 2));
                    int mese = Convert.ToInt32(date1.Substring(2, 2));
                    int anno = 2000 + Convert.ToInt32(date1.Substring(4, 2));

                    DateTime data = new DateTime(anno, mese, giorno, 00, 00, 00);

                    DateTime target = new DateTime();
                    if (data2 == " ")
                    {
                        target = DateTime.Now;
                    }
                    else
                    {
                        target = new DateTime(2000 + Convert.ToInt32(data2.Substring(4, 2)), Convert.ToInt32(data2.Substring(2, 2)), Convert.ToInt32(data2.Substring(0, 2)), 0, 0, 0);
                    }

                    if (data.DayOfYear < target.DayOfYear)
                    {//Incrementiamo il giorno per cambiare data (al giorno dopo)
                        data = data.AddDays(1);
                        int nextgiorno = data.Day;
                        int nextmese = data.Month;
                        int nextanno = data.Year;
                        readDGFEForLottery(nextanno.ToString().Substring(2, 2).PadLeft(2, '0') +  nextmese.ToString().PadLeft(2, '0') + nextgiorno.ToString().PadLeft(2, '0') , target.Year.ToString().Substring(2, 2).PadLeft(2, '0') + target.Month.ToString().PadLeft(2, '0') + target.Day.ToString().PadLeft(2, '0')  );
                        //readFromEJ(nextgiorno.ToString().PadLeft(2, '0') + nextmese.ToString().PadLeft(2, '0') + nextanno.ToString().Substring(2, 2).PadLeft(2, '0'), nextgiorno.ToString().PadLeft(2, '0') + nextmese.ToString().PadLeft(2, '0') + nextanno.ToString().Substring(2, 2).PadLeft(2, '0'));
                    }
                    else
                    {
                        //E' finito l'algoritmo
                    }

                }
                else
                //E' finito il giorno e non c'è nulla ergo passo al giorno successivo (a meno che non sia arrivato alla fine obv)
                {
                    //chiudo il file descriptor senno la ricorsione non mi funziona, tanto cmq lo riapro in append mode
                    file.Close();
                    int giorno = Convert.ToInt32(date1.Substring(0, 2));
                    int mese = Convert.ToInt32(date1.Substring(2, 2));
                    int anno = 2000 + Convert.ToInt32(date1.Substring(4, 2));

                    DateTime data = new DateTime(anno, mese, giorno, 00, 00, 00);

                    DateTime target = new DateTime();
                    target = DateTime.Now;

                    if (data2 == " ")
                    {
                        target = DateTime.Now;
                    }
                    else
                    {
                        target = new DateTime(2000 + Convert.ToInt32(data2.Substring(0, 2)), Convert.ToInt32(data2.Substring(2, 2)), Convert.ToInt32(data2.Substring(4, 2)), 0, 0, 0);
                    }

                    if (data.DayOfYear < target.DayOfYear)
                    {//Incrementiamo il giorno per cambiare data (al giorno dopo)
                        data = data.AddDays(1);
                        int nextgiorno = data.Day;
                        int nextmese = data.Month;
                        int nextanno = data.Year;
                        readDGFEForLottery(nextanno.ToString().Substring(2, 2).PadLeft(2, '0') + nextmese.ToString().PadLeft(2, '0') + nextgiorno.ToString().PadLeft(2, '0'), target.Year.ToString().Substring(2, 2).PadLeft(2, '0') + target.Month.ToString().PadLeft(2, '0') + target.Day.ToString().PadLeft(2, '0'));
                    }
                    else
                    {
                        //E' finito l'algoritmo
                    }
                }

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;

        }



        //Metodo che genera semplicemente 100 Scontrini Lotteria con pagamento cash e poi li inoltra tramite chiusura
        public int Test100ScontriniLotteria()
        {
            log.Info("Performing Test100ScontriniLotteria Method ");
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;

                for (int i = 0; i < 101; ++i)
                {
                    Console.WriteLine("log.info: stampa scontrino num. : " + (int)(i + 1));
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                    dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                    fiscalprinter.EndFiscalReceipt(true);
                    System.Threading.Thread.Sleep(1000);
                }

                fiscalprinter.PrintZReport();

                /*
                //EDITTODO: 20/01/2020 : codice aggiunto per confermare che va in MF,ergo non dovrebbe eseguirmelo 
                System.Threading.Thread.Sleep(5000);

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                fiscalprinter.EndFiscalReceipt(true);
                System.Threading.Thread.Sleep(1000);

                fiscalprinter.PrintZReport();
                */

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;

        }


        //Metodo che genera semplicemente 10 Scontrini Lotteria con pagamento cash alternati a 10 scontrini fiscali std e poi li inoltra tramite chiusura
        public int CreateDBLotteryAlternato()
        {
            log.Info("Performing CreateDBLotteryAlternato Method ");
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;

                for (int i = 0; i < 10; ++i)
                {
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                    dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                    fiscalprinter.EndFiscalReceipt(true);

                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    fiscalprinter.EndFiscalReceipt(true);
                }

                fiscalprinter.PrintZReport();

                /*
                //EDIT:TODO 20/01/2020 : codice aggiunto per confermare che va in MF,ergo non dovrebbe eseguirmelo 
                System.Threading.Thread.Sleep(5000);

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                fiscalprinter.EndFiscalReceipt(true);
                System.Threading.Thread.Sleep(1000);

                fiscalprinter.PrintZReport();
                */

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;

        }





        //L'obiettivo di questo metodo è quello di creare un documento lotteria (.xml) che avrà all'interno degli scontrini fatti
        //prima di mezzanotte e degli scontrini fatti dopo mezzanotte,
        //Fin qui nulla di strano se non fosse che per farlo sposto l'orario in avanti di qualche ora in modo tale che 
        //il server "DEVE" ( "DOVREBBE" ) rispondere con una risposta AC per gli scontrini prima di mezzanotte e SC o SE per quelli dopo la 
        //mezzanotte con codice errore "Data nel futuro".
        //Purtroppo al momento , 26/11/19 i server per le lotterie verificano solo la data e non l'ora (come per i corrispettivi)
        //per cui devo fare questo barbatrucco per ottenere l'xml di risposta misto
        //EDIT: 10/12/19 In questo caso il server risponde solo per gli SE e da "per scontato" gli AC non rispondendo affatto a riguardo.
        public int GenerateLotteryDocWithWarning()
        {
            log.Info("Performing GenerateLotteryDocWithWarning Method ");
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;

                //per aggiungere giorni ad una data si usa il metodo
                //public DateTime AddDays (double value);
                /*
                //Sposto l'orario 3 ore piu avanti
                strObj[0] = DateTime.Now.AddHours(3).AddMinutes(35).ToString("ddMMyyHHmm");
                */

                //TODO: 05/02/20 modifica temporanea, devo creare una routine apposita per crearmi un
                //databasae di scontrini lotteria e non dal primo gennaio 2019 ad oggi in modo da fare i 
                //test sui resi e read lottey status lotteria visto che dicono che i resi vada in timeout 
                //e le lottery status non le trovi con lo zrep fatto il giorno seguente la data del primo 
                //scontrino di giornata
                //Mi metto la data a cavallo dell'anno nuovo cosi' aumento il test
                //strObj[0] = new DateTime(2019 , 12, 31, 23, 58, 00).ToString("ddMMyyHHmm");
                /*
                strObj[0] = DateTime.Now.AddHours(8).AddMinutes(38).ToString("ddMMyyHHmm");
                dirIO = posCommonFP.DirectIO(0, 4001, strObj);
                iData = dirIO.Data;
                if (iData != 4001)
                {
                    log.Error("Error DirecIO 4001 , tentativo fallito di settare la data avanti di 6 ore");
                    NumExceptions++;
                    //throw new PosControlException();

                }
                ZReport();
                */

                strObj[0] = DateTime.Now.AddHours(8).AddMinutes(43).ToString("ddMMyyHHmm");
                dirIO = posCommonFP.DirectIO(0, 4001, strObj);
                iData = dirIO.Data;
                if (iData != 4001)
                {
                    log.Error("Error DirecIO 4001 , tentativo fallito di settare la data avanti di 6 ore");
                    NumExceptions++;
                    //throw new PosControlException();

                }
                ZReport();

                //Step :2 
                //stampo scontrini e mando ZRep che dovrebbero tutti essere spostati
                for (int i = 0; i < 100; ++i)
                {
                    //Scontrino con lotteria
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("Random Object Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                    strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";
                    dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                    fiscalprinter.EndFiscalReceipt(true);

                    //Scontrino senza lotteria
                    fiscalprinter.BeginFiscalReceipt(true);
                    fiscalprinter.PrintRecItem("Random Object No Lottery", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                    fiscalprinter.PrintRecTotal((decimal)10000, (decimal)300000, "000CONTANTI");
                   
                    fiscalprinter.EndFiscalReceipt(true);
                    System.Threading.Thread.Sleep(1000);
                }

                //Faccio chiusura 
                fiscalprinter.PrintZReport();

            }
            catch(Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;
        }



        //DirectIO 1136 
        //Read E/J partition Info
        //input: 
        // string index : 1 byte, 2 = fiscal partition
        //                        3 = user partition
        //output:
        // int NumExceptions: numero totale di eccezioni contate

        public int ReadEjInfo(string index)
        {
            log.Info("Performing ReadEjInfo() Method ");
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                string Index = index;

                strObj[0] = "01" + Index;
                dirIO = posCommonFP.DirectIO(0, 1136, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                string UsedDisk = iObj[0].Substring(4, 13);

                string FreeDisk = iObj[0].Substring(18, 13);

                string UsedInode = iObj[0].Substring(32, 13);

                string FreeInode = iObj[0].Substring(46, 13);


            }
            catch(Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                }
                else
                {
                    log.Fatal("Generic Error: ", e);
                }
            }

            return NumExceptions;
        }


        //DirectIO 9218
        //Read the status of the LAST single lottery receipt
        //EDIT: 21/11/19 Read the status of the single lottery receipt 
        //EDIT: 26/11/19 Testero' Result (tutti i codici tranne 04), ErrCode (che sia conforme all'xml di provenienza), ID_Answer (che sia conforme all'xml di provenienza)
        //EDIT: 19/12/19 E' importante che prima di una eventuale chiamata a questo metodo (diretto da xml o tramite lo smartLotteryParser)
        //non venga fatta una chiusura o swith in RT perché se faccio chiusura e non do REC_NUM lui va a prendere l'ultimo eseguito che non esiste
        //Quindi o non faccio chiusura o se la faccio devo ricordarmi di fare ALMENO uno scontrino lotteria prima di chiamarlo
        
        //Input: Zrep, Numero Scontrino, data, Kind of Receipt (vedi documentazione) 
        //Output: Result of the request
        //Nota: sto comando funziona anche se il POS è in DEMO
        public int ReadLotteryStatusSingleReceipt(string ZRep, string REC_NUM, string tillID, string date, string KoR, ref string result, ref string IdAnswer)
        {

            log.Info("Performing ReadLotteryStatusSingleReceipt() Method ");
            string[] strObj = new string[1];
            DirectIOData dirIO;
            int iData;
            string[] iObj = new string[1];

            try
            {
                //string strData = fiscalprinter.GetDate().ToString();

                //string strDate = strData.Substring(0, 2) + strData.Substring(3, 2) + strData.Substring(8, 2) ;
                string strDate;

                //Se non lo fornisco prendo la data odierna

                if (String.Compare(date , " ") == 0)
                {
                    strDate = DateTime.Now.ToString("ddMMyy");
                }
                else
                {
                    strDate = date;
                }

               
                string zRepNum;

                //se non lo fornisco prendo l'ultimo DA CHIUDERE
                //EDIT 08/01/2020 cosa me ne faccio dell'ultimo da chiudere ?niente

                if (String.Compare(ZRep, " ") == 0)
                {
                    zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                    int nextInt = Int32.Parse(zRepNum);
                    zRepNum = nextInt.ToString("0000");
                }
                else
                {
                    zRepNum = ZRep;
                }

                string recNum; 

                //Se non lo fornisco prendo l'ultimo scontrino emesso

                if(String.Compare(REC_NUM , " ") == 0)
                {
                    recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;
                }
                else
                {
                    recNum = REC_NUM;
                }

                if (String.Compare(KoR, " ") == 0)
                {
                    KoR = "00";
                }

                string IdCassa = tillID.PadLeft(8, '0'); ;

                //TILL_ID = fisso per ora
                strObj[0] = IdCassa +  zRepNum + recNum + strDate + KoR;   
                dirIO = posCommonFP.DirectIO(0, 9218, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                if (iData != 9218)
                {
                    log.Error("Errore DirectIO 9218 campo iData, expected " + 9218 + " received " + iData);
                    //throw new PosControlException();

                }

                string _TILL_ID = iObj[0].Substring(0, 8);
                if(_TILL_ID != IdCassa)
                {
                    log.Error("Errore DirectIO 9218 campo TILL_ID, valore anomalo ricevuto := " + _TILL_ID);
                    //throw new PosControlException();
                }

                //rec_num ricevuto in risposta al comando
                string _rec_num = iObj[0].Substring(12,4);

                if((Convert.ToInt32(KoR) == 0 ) && (_rec_num != recNum))
                {
                    log.Error("Errore DirectIO 9218 campo REC_NUM, non coincide con l'input");
                    //throw new PosControlException();
                }

                // 2 bytes, from 00 to 05
                string _result = iObj[0].Substring(22, 2);
                result = _result;

                if(Convert.ToInt32(_result) < 0 && Convert.ToInt32(_result) > 5)
                {
                    log.Error("Errore DirectIO 9218 campo Result, valore anomalo ricevuto := " + _result);
                    //throw new PosControlException();
                }

                

                // 5 bytes, error code responce
                string _errCode = iObj[0].Substring(24, 5);

                // 50 bytes
                //mancano i primi 6 caratteri dell' ID
                string _id_Answer = iObj[0].Substring(29, 50);

                if(_id_Answer.Length != 50)
                {
                    log.Error("Errore DirectIO 9218 campo ID_ANSWER from Tax Authority, lunghezza anomala ");
                    throw new PosControlException();
                }

                IdAnswer = _id_Answer;
                // EDIT: 13/01/20 per ora lo commento in attesa di correzione
                //Check lunghezza frame di risposta
                /*
                if(iObj[0].Length != 85)
                {
                    log.Error("Errore DirectIO 9218 lunghezza risposta, lunghezza anomala ");
                    throw new PosControlException();
                }
                */
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("Generic Error", e);

                }
            }
            return NumExceptions;
        }

        //Void Lottery Receipt
        // EDIT: 27/11/19 posso anche toglierlo perchè era un test iniziale,alla fine è solo un puro Void che chiamavo dopo uno scontrino con lotteria
        public void VoidLotteryReceipt()
        {
            log.Info("Performing VoidLotteryReceipt() method ");

            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];



                string printerIdModel;
                string printerIdManufacturer;
                string printerIdNumber;
                //Comando -35000 oracle
                string strDataOracle = fiscalprinter.GetData(FiscalData.PrinterId, (int)-35000).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                printerIdModel = strDataOracle.Substring(0, 2);
                printerIdNumber = strDataOracle.Substring(4, 6);
                printerIdManufacturer = strDataOracle.Substring(2, 2);
                string printerId = strDataOracle;

               
                // Get date
                //Console.WriteLine("Performing GetDate() method ");
                string strData = fiscalprinter.GetDate().ToString();
                //Console.WriteLine("Date: " + strData);
                string strDate = strData.Substring(0, 2) + strData.Substring(3, 2) + strData.Substring(6, 4);
                //Console.WriteLine("Date: " + strDate);

                // Get Z report
                //Console.WriteLine("Get Data (Z Report)");
                string zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                int nextInt = Int32.Parse(zRepNum) + 1;
                zRepNum = nextInt.ToString("0000");
                //Console.WriteLine("Z Report: " + zRepNum);

                // Get rec num
                //Console.WriteLine("Get Data (Fiscal Rec)");
                // recNum = fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data;
                string recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;
                //Console.WriteLine("Rec Num: " + recNum);

                // Check document is returnable
                //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                strObj[0] = "2" + printerId + strDate + recNum + zRepNum;   // "1" = Refund "2" = Void
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet = Int32.Parse(iObj[0]);
                if (iRet == 0)
                    log.Info("Document can be Voided");
                else
                {
                    log.Error("Document can NOT be Voided");
                }


                strObj[0] = "0140001VOID " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                
                /*

                strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                //Check risposta alla DirectIO 1135
                if (iData != 1135)
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }

                if (!(String.Equals(iObj[0].Substring(0, 2), "01")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }
                */



            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("Generic Error", e);

                }
            }
        }


        //Void Lottery Receipt

        public void SendVoidLotteryReceipt()
        {
            log.Info("Performing SendVoidLotteryReceipt() method ");

            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];



                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");
                

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "0CONTANTI");


                string op = "01";  //da 01 a 99
                //IdLotteryCode da 2 a 15 byte,poi ci devo addare un terminatore stringa quindi 16
                string IDCODE = "ABCDEFGN";

                strObj[0] = op + IDCODE + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                /*
                //Check risposta alla DirectIO 1135
                if (iData != 1135)
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }

                if (!(String.Equals(iObj[0].Substring(0, 2), "01")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }
                */
                fiscalprinter.EndFiscalReceipt(false);

                //Ora lo annullo

                string printerIdModel;
                string printerIdManufacturer;
                string printerIdNumber;
                //Comando -35000 oracle
                string strDataOracle = fiscalprinter.GetData(FiscalData.PrinterId, (int)-35000).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                printerIdModel = strDataOracle.Substring(0, 2);
                printerIdNumber = strDataOracle.Substring(4, 6);
                printerIdManufacturer = strDataOracle.Substring(2, 2);
                string printerId = strDataOracle;


                // Get date
                //Console.WriteLine("Performing GetDate() method ");
                string strData = fiscalprinter.GetDate().ToString();
                //Console.WriteLine("Date: " + strData);
                string strDate = strData.Substring(0, 2) + strData.Substring(3, 2) + strData.Substring(6, 4);
                //Console.WriteLine("Date: " + strDate);

                // Get Z report
                //Console.WriteLine("Get Data (Z Report)");
                string zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                int nextInt = Int32.Parse(zRepNum) + 1;
                zRepNum = nextInt.ToString("0000");
                //Console.WriteLine("Z Report: " + zRepNum);

                // Get rec num
                //Console.WriteLine("Get Data (Fiscal Rec)");
                // recNum = fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data;
                string recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;
                //Console.WriteLine("Rec Num: " + recNum);

                // Check document is returnable
                //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                strObj[0] = "2" + printerId + strDate + recNum + zRepNum;   // "1" = Refund "2" = Void
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet = Int32.Parse(iObj[0]);
                if (iRet == 0)
                    log.Info("Document can be Voided");
                else
                {
                    log.Error("Document can NOT be Voided");
                }


                strObj[0] = "0140001VOID " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                /*

                strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                //Check risposta alla DirectIO 1135
                if (iData != 1135)
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }

                if (!(String.Equals(iObj[0].Substring(0, 2), "01")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }
                */



            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("Generic Error", e);

                }
            }
        }



        //Refund Lottery Receipt
        //WARNING: Bisogna emettere uno scontrino con lotteria prima di chiamare questo metodo
        public void RefundLotteryReceipt()
        {
            log.Info("Performing RefundLotteryReceipt() method ");

            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];



                string printerIdModel;
                string printerIdManufacturer;
                string printerIdNumber;
                //Comando -35000 oracle
                string strDataOracle = fiscalprinter.GetData(FiscalData.PrinterId, (int)-35000).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                printerIdModel = strDataOracle.Substring(0, 2);
                printerIdNumber = strDataOracle.Substring(4, 6);
                printerIdManufacturer = strDataOracle.Substring(2, 2);
                string printerId = strDataOracle;


                // Get date
                string strData = fiscalprinter.GetDate().ToString();
                string strDate = strData.Substring(0, 2) + strData.Substring(3, 2) + strData.Substring(6, 4);
                //Console.WriteLine("Date: " + strDate);

                // Get Z report
                //Console.WriteLine("Get Data (Z Report)");
                string zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                int nextInt = Int32.Parse(zRepNum) + 1;
                zRepNum = nextInt.ToString("0000");

                string recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;

                // Check document is returnable
                //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                strObj[0] = "1" + printerId + strDate + recNum + zRepNum;   // "1" = Refund
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet = Int32.Parse(iObj[0]);
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    log.Error("Document can NOT be Refunded");
                }

                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)100, (int)1);
                fiscalprinter.PrintRecTotal((decimal)100, (decimal)000, "0CONTANTI");


                /*

                strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                //Check risposta alla DirectIO 1135
                if (iData != 1135)
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }

                if (!(String.Equals(iObj[0].Substring(0, 2), "01")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }
                */
                fiscalprinter.EndFiscalReceipt(false);
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("Generic Error", e);

                }
            }
        }


        //Metodo che testa la PrintRecItem in combo con la lotteria ,in particolare il contatore del numero degli scontrini fiscali e il totale giornaliero dopo una scontrino di vendita con lotteria
        public int MicroScontriniLottery(string description, string price, string quantity, string vatIndex, string unitPrice, string IdLotteryCode, string Operator)
        {
            log.Info("Performing MicroScontriniLottery Method");
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                int output = 0;
                for (int i = 0; i < 50; ++i)
                {
                    //output += mc.PrintRecItem(description, price, quantity, vatInfo, unitPrice, ref gc, ref gc2);

                    BeginFiscalReceipt("true");

                    //Eseguo TRE vendite dello stesso oggetto,stesso prezzo unitario e quantità e iva specificata dal parametro vatInfo della funzione , poi testo i totalizzatori relativi prima e dopo la vendita
                    //The price parameter is not used if the unit price is different from 0(the amount is computed from the fiscal printer multiplying the unit price and the quantity).The unitName parameter is not used. Set on the SetupPOS application to print the quantity line, even if it's 1
                    //Console.WriteLine("Performing PrintRecItem() method ");
                    FiscalReceipt.fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatIndex), decimal.Parse(unitPrice), "");
                    FiscalReceipt.fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatIndex), decimal.Parse(unitPrice), "");
                    FiscalReceipt.fiscalprinter.PrintRecItem(description, decimal.Parse(price), Int32.Parse(quantity), Int32.Parse(vatIndex), decimal.Parse(unitPrice), "");
                    FiscalReceipt.fiscalprinter.PrintRecTotal((decimal)10000, (decimal)(Int32.Parse(quantity) / 1000 * decimal.Parse(unitPrice) * 3), "0CONTANTI");


                    string op = Operator.PadLeft(2, '0');  //da 01 a 99
                                                           //IdLotteryCode da 2 a 15 byte,poi ci devo addare un terminatore stringa quindi 16
                    string IDCODE = IdLotteryCode;

                    strObj[0] = op + IDCODE + "        " + "0000";

                    dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                    iData = dirIO.Data;
                    //Console.WriteLine("DirectIO(): iData = " + iData);
                    iObj = (string[])dirIO.Object;
                    //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                    //Check risposta alla DirectIO 1135
                    if (iData != 1135)
                    {
                        log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                        //throw new PosControlException();
                    }

                    if (!(String.Equals(iObj[0].Substring(0, 2), "01")))
                    {
                        log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                        //throw new PosControlException();
                    }

                    fiscalprinter.EndFiscalReceipt(true);

                }
            }
            catch (Exception e)
            {  //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    Console.WriteLine("ErrorCode: " + pce.ErrorCode.ToString());
                    Console.WriteLine("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    Console.WriteLine("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("Generic Error", e);

                }

            }

            return NumExceptions;

        }






        //Metodo creato per testare i totalizzatori quotidiani che si ottengono con la directIO 2050 (o il metodo del driver Get Daily Data): in particolare verifico la
        //coerenza dei totalizzatori dei RESI e degli ANNULLI in combo con la LOTTERIA:
        /*
        
            Sequenza del suddetto metodo:
        a) Vendita di 1 euro al 4% 4 cent di Iva



        b) Documento multi aliquota che poi verrà reso parzialmente fino ad azzerarlo totalmente che sarà :
        1 euro al 22% 		18 cent IVA
        0.12 euro al 10%	1 cent  IVA
        0.30 euro al 4% 	1 cent  IVA
        0.5 euro Es		0   0 cent IVA

        Tot:1.92		Tot Iva:0.20 euro


        c)Annullo 0.10 euro al 22% 2 cent di Iva

        d)Annullo 0.10 euro al 22% 2 cent di Iva

        e)Annullo 0.80 euro al 22% 14 cent di Iva

        f)Annullo 0.12 euro al 10% 1 cent di Iva

        g)Annullo 0.30 euro al 4% 1 cent di Iva

        h)Annullo di 0.50 euro ES* 0 cent di Iva


        i)Documento multi aliquota che poi verrà reso annullato che sarà :
        1 euro al 22% 		18 cent
        0.12 euro al 10%	1 cent
        0.30 euro al 4% 	1 cent
        0.5 euro Es		0

        Tot:1.92		Tot Iva:0.20 euro

        j)Annullo di quest'ultimo documento di vendita


        */

        //In totale faro' 10 scontrini e un totale di 4.84 euro di vendite con codice LOTTERIA (due scontrini da 1.92 , e uno di 1 euro,gli altri sono resi parziali e/o annulli) 
        //Nella funzione che poi andrà a testare questo metodo (la TestLotteryPrintRecRefound chiamabile via xml) leggero i totalizzatori , prima e dopo questo metodo e verifichero' che i totalizatori modificati
        //siano coerenti con tutto cio' che ho fatto qui dentro: 
        public int LotteryPrintRecRefound(string description)
        {
            try
            {
                log.Info("Performing LotteryPrintRecRefound() method");
                string printerIdModel = "";
                string printerIdManufacturer = "";
                string printerIdNumber = "";
                string strData = "";
                string printerId = "";
                string strDate = "";
                string zRepNum = "";
                string recNum = "";
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];



                // Vendita random,per incrementare il contatore delle vendite: 1 euro aliquota 4%
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Random Object", (decimal)10000, (int)1000, (int)3, (decimal)10000, "");
                fiscalprinter.PrintRecTotal((decimal)0000, (decimal)00000, description);

                string op = "01";  //costante per ora poi vediamo se addarlo come parametro di input del metodo
                //IdLotteryCode da 2 a 15 byte,poi ci devo addare un terminatore stringa quindi 16
                string IDCODE = "ABCDEFGN";

                strObj[0] = op + IDCODE + "        " + "0000";
                //Codice invio lotteria
                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                //Check risposta alla DirectIO 1135
                if (iData != 1135)
                {
                    log.Error("Error DirectIO 1135 , expected iData 1135, received " + iData);
                    //throw new PosControlException();
                }

                if (!(String.Equals(iObj[0].Substring(0, 2), "01")))
                {
                    log.Error("Richiesta Invio Lottery Personal ID CODE Fallita, Error DirectIO 1135 operator, expected 01, received " + iObj[0].Substring(0, 2));
                    //throw new PosControlException();
                }


                fiscalprinter.EndFiscalReceipt(false);





                //Scontrino di vendita CON LOTTERIA multi aliquota per poi andare a fare dei resi parziali fino ad annullarlo completamente (annullo dopo reso è vietato, alias crash) 

                fiscalprinter.BeginFiscalReceipt(true);
                //Console.WriteLine("Performing PrintRecItem() method ");
                fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)10000, "");
                fiscalprinter.PrintRecItem("TSHIRT", (decimal)10000, (int)1000, (int)2, (decimal)1200, "");
                fiscalprinter.PrintRecItem("CAPPELLO", (decimal)10000, (int)1000, (int)3, (decimal)3000, "");
                fiscalprinter.PrintRecItem("BORSA", (decimal)10000, (int)1000, (int)4, (decimal)5000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, description);
                strObj[0] = op + IDCODE + "        " + "0000";
                //Codice invio lotteria
                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                fiscalprinter.EndFiscalReceipt(false);



                //Reperimento dati che mi servono per verificare che l'ultimo scontrino sia refundable 
                // Get printer ID

                strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)-35000).Data;
                printerIdModel = strData.Substring(0, 2);
                printerIdNumber = strData.Substring(4, 6);
                printerIdManufacturer = strData.Substring(2, 2);
                printerId = strData;

                // Get date
                strData = fiscalprinter.GetDate().ToString();
                strDate = strData.Substring(0, 2) + strData.Substring(3, 2) + strData.Substring(6, 4);

                // Get Z report
                zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                int nextInt = Int32.Parse(zRepNum) + 1;
                zRepNum = nextInt.ToString("0000");

                // Get rec num
                // recNum = fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data;
                recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;

                // Check document is returnable
                strObj[0] = "1" + printerId + strDate + recNum + zRepNum;   // "1" = return
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                int iRet = Int32.Parse(iObj[0].Substring(0, 1));
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    log.Error("Document can NOT be Refunded quando invece dovrebbe");
                    throw new Exception();
                }

                if (iData != 9205)
                {
                    log.Error("Error DirectIO 9205 , expected iData 9205, received " + iData);
                    throw new Exception();
                }
                // Return document print
                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new PosControlException();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)1000, (int)1);
                fiscalprinter.PrintRecTotal((decimal)30000, (decimal)00000, description);
                fiscalprinter.EndFiscalReceipt(false);


                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;


                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)1000, (int)1);
                fiscalprinter.PrintRecTotal((decimal)30000, (decimal)00000, description);
                fiscalprinter.EndFiscalReceipt(false);

                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)8000, (int)1);
                fiscalprinter.PrintRecTotal((decimal)80000, (decimal)00000, description);
                fiscalprinter.EndFiscalReceipt(false);

                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }
                
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)1200, (int)2);
                fiscalprinter.PrintRecTotal((decimal)30000, (decimal)00000, description);
                fiscalprinter.EndFiscalReceipt(false);

                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)3000, (int)3);
                fiscalprinter.PrintRecTotal((decimal)30000, (decimal)00000, description);
                fiscalprinter.EndFiscalReceipt(false);

                strObj[0] = "0140001REFUND " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("ITEM REFUND", (decimal)5000, (int)4);
                fiscalprinter.PrintRecTotal((decimal)50000, (decimal)00000, description);
                fiscalprinter.EndFiscalReceipt(false);


                //Scontrino vendita multi aliquota che verrà successivamente annullato (previo test ovviamente)

                fiscalprinter.BeginFiscalReceipt(true);
                //Console.WriteLine("Performing PrintRecItem() method ");
                fiscalprinter.PrintRecItem("SCARPE", (decimal)10000, (int)1000, (int)1, (decimal)10000, "");
                fiscalprinter.PrintRecItem("TSHIRT", (decimal)10000, (int)1000, (int)2, (decimal)1200, "");
                fiscalprinter.PrintRecItem("CAPPELLO", (decimal)10000, (int)1000, (int)3, (decimal)3000, "");
                fiscalprinter.PrintRecItem("BORSA", (decimal)10000, (int)1000, (int)4, (decimal)5000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)00000, description);

                strObj[0] = op + IDCODE + "        " + "0000";
                //Codice invio lotteria
                dirIO = posCommonFP.DirectIO(0, 1135, strObj);

                fiscalprinter.EndFiscalReceipt(false);

                // Check document is voidable

                // Get date
                strData = fiscalprinter.GetDate().ToString();
                strDate = strData.Substring(0, 2) + strData.Substring(3, 2) + strData.Substring(6, 4);

                // Get Z report
                zRepNum = fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                nextInt = Int32.Parse(zRepNum) + 1;
                zRepNum = nextInt.ToString("0000");

                // Get rec num
                // recNum = fiscalprinter.GetData(FiscalData.ReceiptNumber, (int)0).Data;
                recNum = fiscalprinter.GetData(FiscalData.FiscalReceipt, (int)0).Data;


                strObj[0] = "2" + printerId + strDate + recNum + zRepNum;   // "2" = void
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                int iRet2 = Int32.Parse(iObj[0].Substring(0, 1));

                if (iRet2 == 0)
                {
                    log.Info("Document voidable");
                    // Annullo lo scontrino e verifico che il comando risponda in maniera coerente col protocollo
                    //Console.WriteLine("DirectIO (Void document print)");
                    strObj[0] = "0140001VOID " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);

                    iData = dirIO.Data;

                    iObj = (string[])dirIO.Object;

                    if (iData != 1078)
                    {
                        log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                        throw new Exception();
                    }
                    iObj = (string[])dirIO.Object;
                    if (!(String.Equals(iObj[0], "51")))
                    {
                        log.Fatal("Error DirectIO 1078 operator, expected 51, received " + iObj[0]);
                        throw new Exception();
                    }
                }
                else
                {
                    log.Error("Document NOT voidable quando dovrebbe essere annullabile"); //E' un errore perchè dovrebbe annullarlo 
                }

                //Provo a riannullarlo per testare che non faccia cose strane (tipo annullarlo ancora o annullare altri scontrini


                strObj[0] = "2" + printerId + strDate + recNum + zRepNum;   // "2" = void
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                iRet2 = Int32.Parse(iObj[0].Substring(0, 1));
                if (iRet2 != 4)
                {
                    log.Error("Mi autorizza ad annullare un documento già annullato precedentemente");
                    strObj[0] = "0140001VOID " + zRepNum + " " + recNum + " " + strDate + " " + printerId;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);

                    iData = dirIO.Data;

                    iObj = (string[])dirIO.Object;
                    log.Error("Errore DirectIO 1078, ha permesso di annullare un documento già annullato con campo iData " + iData);
                    log.Error("Errore DirectIO 1078, ha permesso di annullare un documento già annullato con campo operator " + iObj[0]);
                    NumExceptions++;

                }
                else
                {
                    log.Info("Document NOT voidable ");
                }


            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Error("Generic Error: ", e);

                }

                return NumExceptions;
            }
            return NumExceptions;


        }

        //Comando DirectIO 9019: Set Lottery Message
        public int SetLotteryMessage(string index, string print, string message)
        {
            try
            {

                log.Info("Performing SetLotteryMessage() Method");

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                int ind;
                string description = "";

                strObj[0] = index.PadLeft(2, '0') + print.PadLeft(1) + message.PadRight(39, ' ') + "riga " + index.PadLeft(2, '0');
                dirIO = posCommonFP.DirectIO(0, 9019, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                ind = Convert.ToInt32(iObj[0].Substring(0, 2));
                /* RISPONDE SEMPRE 0
                if (ind != (Convert.ToInt32(index) -1))
                {
                    log.Error("Errore comando 9219 campo index, expected: " + (Convert.ToInt32(index) - 1) + " Received: " + ind);
                }
                */
                if ((ind != 0) && (String.Compare(print , "0") != 0))
                {
                    log.Error("Errore comando 9219 campo index, expected: 1 "  + "Received: " + ind);
                }
                if ((ind != 0) && (String.Compare(print, "1") == 0))
                {
                    log.Error("Errore comando 9219 campo index, expected: 0 " + "Received: " + ind);
                }

            }
            catch (Exception e)
            {
                NumExceptions++;
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }
            return NumExceptions;
        }





        //Comando 9219: Read Lottery Message (la scritta sotto lo scontrino lotteria in pratica, sono max 5 righe quindi 5 indexes)
        public int ReadLotteryMessage(string index)
        {
            try
            {

                log.Info("Performing ReadLotteryMessage() Method");

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                int ind = 0;
                string print = "";
                string description = "";

                strObj[0] = index.PadLeft(2, '0');
                dirIO = posCommonFP.DirectIO(0, 9219, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                ind = Convert.ToInt32(iObj[0].Substring(0, 2));
                print = iObj[0].Substring(2, 1);
                description = iObj[0].Substring(3, 46);

                //TODO rivedere se è index o index - 1
                if (ind != (Convert.ToInt32(index) ))
                {
                    log.Error("Errore comando 9219 campo index, expected: " + (Convert.ToInt32(index) - 1) + " Received: " + ind);
                }

            }
            catch (Exception e)
            {
                NumExceptions++;
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }
            return NumExceptions;
        }







        // Test Comandi nuovi Lotteria 1134 e 9218
        // Purtroppo è una semi copia dell' OuterParser ma non posso riutilizzarla per adesso perchè
        // quel parser ,alla fine , parsa i doc xml inviati mentre questa deve parsare i doc xml di risposta
        // Piu avanti vediamo se riesco a fare un unico metodo piu' elegante
        // Edit: questa è proprio una copia di OuterParser che chiama InnerParser ma adesso mi serve un InnerParser un po' diverso
        public int LotterySmartParser(string regex = @"^[0-9]{8}$") 
        {

            try
            {
                if (String.Compare(regex, " ") == 0)
                {
                    regex = @"^[0-9]{8}$";
                }

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                //DirectIO 4219 GET LAN PARAMETERS       
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 4219, strObj);

                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                int uno = Int32.Parse(iObj[0].Substring(3, 3));
                string primo = uno.ToString();
                int due = Int32.Parse(iObj[0].Substring(7, 3));
                string secondo = due.ToString();
                int tre = Int32.Parse(iObj[0].Substring(11, 3));
                string terzo = tre.ToString();
                int quattro = Int32.Parse(iObj[0].Substring(15, 3));
                string quarto = primo + "." + secondo + "." + terzo + "." + quattro.ToString();

                string URL = "http://" + quarto + "/dati-rt/lotteria/";

                //string URL = "E:/ToolAggiornato/prova.html";

                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                // There are various options, set as needed
                htmlDoc.OptionFixNestedTags = true;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                //request.ContentType = "application/json; charset=utf-8";
                //request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("username:password"));
                //request.PreAuthenticate = true;

                string credentials = "epson:epson";

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls; // SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | 

                String username = "epson";
                String password = "epson";
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                request.Headers.Add("Authorization", "Basic " + encoded);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                CookieContainer myContainer = new CookieContainer();
                request.Credentials = new NetworkCredential(username, password);
                request.CookieContainer = myContainer;
                request.PreAuthenticate = true;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    //string data = reader.ReadToEnd();

                    // filePath is a path to a file containing the html
                    htmlDoc.Load(reader);
                    //htmlDoc.Load(URL);

                    // Use:  htmlDoc.LoadHtml(xmlString);  to load from a string (was htmlDoc.LoadXML(xmlString)

                    // ParseErrors is an ArrayList containing any errors from the Load statement
                    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                    {
                        // Handle any parse errors as required
                        //mancano 4 tag di chiusura nel WebService

                    }

                    if (htmlDoc.DocumentNode != null)
                    {
                        HtmlAgilityPack.HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//@href");
                        HtmlAgilityPack.HtmlNodeCollection bodyNode2 = htmlDoc.DocumentNode.SelectNodes("//@href");

                        if (bodyNode != null)
                        {
                            // Do something with bodyNode
                            //Console.WriteLine("Test");
                        }

                        if (bodyNode2.Count != 0)
                        {
                            /*
                            for (int i = 3; i < bodyNode2.Count; i = i + 2)
                            {
                                //Console.WriteLine(bodyNode2[i].GetDirectInnerText());
                                LotteryFolderParser(bodyNode2[i].GetDirectInnerText() ,ref scontrini);
                            }
                            */
                            for (int i = 0; i < bodyNode2.Count; i++)
                            {
                                //EDIT 11/12/19 refactoring method , invece del regex fisso ce lo passo da input
                                // if (isWhatILookingFor(bodyNode2[i].GetDirectInnerText() , @"^\d{8}\/$"))
                                //cerco le cartelle con le date
                                if (WebService.isWhatILookingFor(bodyNode2[i].InnerHtml, @regex))
                                {
                                    //LotteryFolderParser(bodyNode2[i].GetDirectInnerText(), ref scontrini);
                                    WebService.InnerHTMLParser(URL + bodyNode2[i].GetDirectInnerText() + "/", "-[A-Z]{2}-[a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12}.xml$");

                                }
                            }

                        }

                    }

                }

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions += 1;
                {

                    log.Error("Error OuterHTMLParser  ", e);

                }

            }
            return NumExceptions;
        }



        //Metodo che parsa la cartella /dati-rt/lotteria/"data" e tutti i file LOTTERIAESITO-xxxx.xml  che sono all'interno
        //L'obiettivo è parsare le risposte e interrogare la stampante con le 1134 e 9218 per verificare che ci sia coerenza
        //tra quello che parso e quello che mi dice la stampante
        public static int LotteryFolderResponseParser(string urlString)
        {
            
            int Inizio = Convert.ToInt32(urlString.Substring(urlString.Length - 74, 4));

            int index = 0;


            // something that will read the XML file
            XmlTextReader reader = null;

            // URL di prova
            String URLString = urlString;

            String IdAnswer = URLString.Substring(123, 36).PadRight(50, ' ');

            //Store of the previous XmlNodeType.Element
            string lastElement = "";

            //mi serve per discernere dal tipo di doc che sto parsando
            string esito = "";

            //error code in caso di SE o SC
            string err_code = "";

            //Risultato scontrino lotteria 
            string ID_Answer = "";

            string[] strObj = new string[1];
           
            string[] iObj = new string[1];

            try
            {
              
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlString);
                //request.ContentType = "application/json; charset=utf-8";
                //request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("username:password"));
                //request.PreAuthenticate = true;

                string credentials = "epson:epson";

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls; // SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | 


                String username = "epson";
                String password = "epson";
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                request.Headers.Add("Authorization", "Basic " + encoded);
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36";
                CookieContainer myContainer = new CookieContainer();
                request.Credentials = new NetworkCredential(username, password);
                request.CookieContainer = myContainer;
                request.Referer = urlString;
                request.PreAuthenticate = true;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    Stream responseStream = response.GetResponseStream();


                    reader = new XmlTextReader(URLString, responseStream);
                    log.Info("Parsing " + URLString + " file");

                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element: // The node is an element.

                                lastElement = reader.Name;

                                switch (lastElement)
                                {
                                    case "NumeroProgressivo":
                                        reader.Read();
                                        //mi calcolo l'indice della listOfLottery su cui devo andare a scrivere
                                        index = Convert.ToInt32(reader.Value.Substring(5, 4)) - Inizio;
                                        break;


                                    //Una volta che trovo il codice errore ho finito con lo scontrino in questione
                                    case "Codice":
                                        reader.Read();
                                        err_code = reader.Value;
                                        WebService.listOfLottery[index].CodError = err_code;
                                        WebService.listOfLottery[index].Result = "03";
                                        break;

                                    case "IdOperazione":
                                        reader.Read();
                                        ID_Answer = reader.Value;
                                        break;

                                }

                                break;

                            case XmlNodeType.Text: //Display the text in each element.

                                if (string.Compare(lastElement, "Esito") == 0)
                                {
                                    if (string.Compare(reader.Value, "AC") == 0) // i doc son tutti buoni
                                    {
                                        esito = "AC";
                                        GestioneACResponse(urlString.Substring(0, urlString.Length - 49) + ".xml", IdAnswer);
                                        break;
                                    }
                                    else
                                    if (string.Compare(reader.Value, "SE") == 0) //è un doc parzialmente scartato
                                    {
                                        esito = "SE";
                                        //qui deve andare avanti e solo alla fine chiamare GestioneSeResponse
                                        //GestioneSEResponse(urlString.Substring(0, urlString.Length - 49) + ".xml");
                                        //EDIT 17/12/19 qui devo chiamare per forza la LotteryFolderSentParser in modo da 
                                        //sapere già quanti sono gli scontrini totali del doc e quali sono/non sono lotteria
                                        int scontrini = 0;
                                        WebService.LotteryFolderSentParser(urlString.Substring(0, urlString.Length - 49) + ".xml", ref scontrini);
                                        break;
                                    }
                                    else
                                    if (string.Compare(reader.Value, "SC") == 0) //è un doc scartato (tutto , in blocco)
                                    {
                                        //Qui andrebbe fatta la stessa cosa di SE ma mi serve un doc scartato che non ho
                                        esito = "SC";
                                        //GestioneSCResponse(urlString.Substring(0, urlString.Length - 49) + ".xml");
                                        int scontrini = 0;

                                        WebService.LotteryFolderSentParser(urlString.Substring(0, urlString.Length - 49) + ".xml", ref scontrini);
                                        break;
                                    }
                                }

                                break;

                            case XmlNodeType.EndElement:

                                if (String.Equals(reader.Name, "SegnalazioniDocComm")) //Ho finito di parsare il documento intero

                                {
                                    GestioneSEResponse(urlString.Substring(0, urlString.Length - 49) + ".xml", IdAnswer);

                                }


                                if (String.Equals(reader.Name, "ListaErrori")) //Ho finito di parsare il documento intero

                                {
                                    GestioneSCResponse(urlString.Substring(0, urlString.Length - 49) + ".xml");

                                }
                                break;

                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions += 1;
                {

                    log.Error("Error Reading Url " + URLString, e);

                }

            }
            return NumExceptions;
        }



        //TODO: 30/01/20 Nuovo comando di set del flag 67 per il checksum lotteria
        //Se lo setto effettua il checksum, se lo metto a 0 non lo controlla mai ergo mai errore 30 (errore checksum)
        //Controllare tutto ciò
        //input: string flag, 1 byte , 1/0
        public int CheckSumCommand(string flag)
        {
            log.Info("Performing CheckSumCommand Method ");
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;

                string[] iObj = new string[1];
                int iData;

                strObj[0] = "67" + flag;
                dirIO = posCommonFP.DirectIO(0, 4014, strObj); // Setto il flag 67
                iObj = (string[])dirIO.Object;
                iData = dirIO.Data;

                if (iData != 4014)
                {
                    log.Error("Errore DirectIO 4014 flag 67 campo iData, expected 4014 received: " + iData);
                    throw new PosControlException();
                }
                //TODO: da inserire anche il test sulla risposta effettiva ma al momento non so come è fatta
                //aspetto documentazione scritta da Francesco
                if(String.Compare(iObj[0], "01") != 0)
                {
                    log.Error("Errore DirectIO 4014 flag 67 campo response, expected 01 received: " + iObj[0]);
                    throw new PosControlException();
                }
                if(String.Compare(flag , "1") == 0)
                {
                    checkSumFlag = true;
                }
                else
                {
                    if (String.Compare(flag, "0") == 0)
                    {
                        checkSumFlag = false;
                    }
                }
            }
            catch (Exception e)
            {
                NumExceptions += 1;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Fatal("Generic Error", e);

                }

                //return NumExceptions;
            }
            return NumExceptions;
        }



        //Metodo interno per gestire la risposta AC di un doc lotteria xml.
        //Facciamo chiarezza: Se ho AC vuol dire che son tutti buoni MA....
        //dal doc risposta posso ricavarmi ZRep, Inizio Scontrino e Fine Scontrino ma all'interno ci possono essere
        //degli scontrini senza lotteria per cui in questo caso devo chiamare il parser 
        //WebService.LotteryFolderSentParser del doc xml inviato relativo (basta sostituire nel path l'id risposta con "LOTTERIA.XML"
        private static int GestioneACResponse(string urlString, string IdAnswer)
        {
            try
            {
                string url = urlString;
                int scontrini = 0;
                //Qui chiamo il metodo che parsa il doc INVIATO e mi prendo in uscita il numero effettivo di scontrini lotteria
                Lottery lt = new Lottery();
                WebService.listOfLottery.Clear();
                WebService.LotteryFolderSentParser(url,  ref scontrini);

                /*
                // EDIT: 08/05/20 è un metodo sperimentale e ancora non perfettamente funzionante, per ora lo accantono 
                //EDIT: TODO 06//03/20 sto provano ad inserire questo metodo readDGFEForLottery al posto di LotteryFolderSentParser 
                //per vedere se posso integrarlo nel task  LotterySmartParser
                Lottery lt = new Lottery();
                WebService.listOfLottery.Clear();
                lt.readDGFEForLottery(urlString.Substring(urlString.Length - 50, 6));
                */

                string result = String.Empty;
                string NumLotOk = String.Empty;
                string NumLotRej = String.Empty;
                string _IdAnswer = String.Empty;
                for (int i = 0; i < WebService.listOfLottery.Count(); i++)
                {
                    //Qui chiamo la 1134 e mi prendo in uscita il lottery receipt ok
                    lt.ReadLotteryStatus(WebService.listOfLottery[i].GetDate, WebService.listOfLottery[i].GetZRep, WebService.listOfLottery[i].GetTillID, ref NumLotOk, ref NumLotRej);
                    
                    //Qui chiamo la 9218 e mi prendo in uscita il result e l'Id Answer
                    lt.ReadLotteryStatusSingleReceipt(WebService.listOfLottery[i].GetZRep, WebService.listOfLottery[i].GetNumScont, WebService.listOfLottery[i].GetTillID, WebService.listOfLottery[i].GetDate, "00",  ref result, ref _IdAnswer);
                    //se abbiamo uno scontrino lotteria che non da 00 come result vuol dire che c'è qualcosa che non va, cmq un errore
                    if ((WebService.listOfLottery[i].IsLottery) && (String.Compare(result, "00") != 0))
                    {
                        log.Error("Errore DirectIO 9218 su uno scontrino lotteria accettato con ZREP := " + WebService.listOfLottery[i].GetZRep + " Numero Scontrino := " + WebService.listOfLottery[i].GetNumScont + " TillID := " + WebService.listOfLottery[i].GetTillID + " in Data := " + WebService.listOfLottery[i].GetDate);
                        log.Error("Expected Result Code := 00 , received := " + result);
                        //throw new PosControlException();
                    }
                    //l'alternativa è che non sia uno scontrino lotteria per cui devo avere come result il codice "05", alias Not found
                    if (!(WebService.listOfLottery[i].IsLottery) && (String.Compare(result, "05") != 0))
                    {
                        log.Error("Errore DirectIO 9218 su uno scontrino NON di lotteria con ZREP :=  " + WebService.listOfLottery[i].GetZRep + " Numero Scontrino := " + WebService.listOfLottery[i].GetNumScont + " TillID := " + WebService.listOfLottery[i].GetTillID + " in Data := " + WebService.listOfLottery[i].GetDate);
                        log.Error("Expected Result Code := 05 , received := " + result);
                        //throw new PosControlException();
                    }
                    if (String.Compare(_IdAnswer, IdAnswer) != 0)
                    {
                        log.Error("Errore IdAnswer su uno scontrino di lotteria con ZREP :=  " + WebService.listOfLottery[i].GetZRep + " Numero Scontrino := " + WebService.listOfLottery[i].GetNumScont + " TillID := " + WebService.listOfLottery[i].GetTillID + " in Data := " + WebService.listOfLottery[i].GetDate);
                        log.Error("Expected IdAnswer := " + url.Substring(0,50) + " , received := " + IdAnswer);
                    }
                }
                //Mi accerto che cio' che conto dal doc inviato corrisponda a cio' che mi da la 1134
                //EDIT TODO: 09/03/20 questo if non è corretto perchè scontrini mi conta gli scontrini lotteria solo di quell xml inviato
                //ma se in un giorno ne mando + di uno il conto non torna perchè NumLotOk viene dalla 1134 che mi conta TUTTI gli scontrini LOTTERIA inviati in quel giorno 
                //E non solo relativo ad un xml inviato che è l'info che ottengo da scontrini
                //Ora lo disattivo ma dovrei aggiustarlo o toglierlo
                /*
                if (Convert.ToInt32(NumLotOk) != scontrini)
                {
                    log.Error("Errore DirectIO 1134 Lottery Receipt Ok relativo al documento  " + url);
                    log.Error("La 1134 mi da " + NumLotOk + " scontrini buoni mentre io ho parsato " + scontrini + " scontrini buoni");
                    //throw new PosControlException();
                }
                */

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Error("Generic Error: ", e);
                }
            }

            return NumExceptions;
        }


        //Metodo interno per gestire la risposta SE di un doc lotteria xml.
        //Facciamo chiarezza: Se ho SE vuol dire che son un po' buoni un po' scartati....
        //dal doc risposta posso ricavarmi ZRep, Inizio Scontrino e Fine Scontrino ma all'interno ci possono essere
        //degli scontrini senza lotteria per cui in questo caso devo chiamare il parser 
        //WebService.LotteryFolderParser del doc xml inviato relativo (basta sostituire nel path l'id risposta con "LOTTERIA.XML"
        private static int GestioneSEResponse(string urlString, string IdAnswer)
        {
            try
            {
                string url = urlString;
                //spostato nella LotteryResponseParser
                //int scontrini = 0;
                //WebService.LotteryFolderSentParser(url, ref scontrini);

                string _IdAnswer = String.Empty;
                Lottery lt = new Lottery();
                string resultCode = "";
                // Num Scontrini Lotteria OK from Command
                string NumLotOk = "";
                // Num Scontrini Lotteria Rejected from 1134
                string NumLotRej = "";
                //Num Scontrini Lotteria Rejected from 9218
                int LotRej = 0;
                //Id Risposta che confrontero' con l'Id estratto direttamente dall ' Url
                for (int i = 0; i < WebService.listOfLottery.Count(); i++)
                {
                    //Qui chiamo la 1134 e mi prendo in uscita il Lottery Receipt Rejected  
                    lt.ReadLotteryStatus(WebService.listOfLottery[i].GetDate, WebService.listOfLottery[i].GetZRep, WebService.listOfLottery[i].GetTillID.PadLeft(8, '0'), ref NumLotOk, ref NumLotRej);

                    //Qui chiamo la 9218 e mi prendo in uscita il Result Code: in questo caso posso avere:
                    //1) Result 00
                    //2) Result 03
                    //3) Result 05
                    lt.ReadLotteryStatusSingleReceipt(WebService.listOfLottery[i].GetZRep, WebService.listOfLottery[i].GetNumScont, WebService.listOfLottery[i].GetTillID.PadLeft(8, '0'), WebService.listOfLottery[i].GetDate,"00",  ref resultCode, ref _IdAnswer);
                    

                    if(String.Compare(resultCode, "03") == 0)
                    {
                        LotRej++;
                    }
                    //Check sull'error code relativo ad uno scontrino buono
                    if ((WebService.listOfLottery[i].IsLottery) && (String.Compare(resultCode, "00") == 0) && (WebService.listOfLottery[i].CodError != "FFFFF"))
                    {
                        log.Error("Errore DirectIO 9218 su uno scontrino di lotteria con ZREP :=  " + WebService.listOfLottery[i].GetZRep + " Numero Scontrino := " + WebService.listOfLottery[i].GetNumScont + " TillID := " + WebService.listOfLottery[i].GetTillID + " in Data := " + WebService.listOfLottery[i].GetDate);
                        log.Error("Expected Error Code := FFFFF , received := " + WebService.listOfLottery[i].CodError);
                        //throw new PosControlException();
                    }
                    //Check sull'error code relativo ad uno scontrino scartato
                    if ((WebService.listOfLottery[i].IsLottery) && (String.Compare(resultCode, "03") == 0) && (WebService.listOfLottery[i].CodError == "FFFFF"))
                    {
                        log.Error("Errore DirectIO 9218 su uno scontrino di lotteria scartato con ZREP :=  " + WebService.listOfLottery[i].GetZRep + " Numero Scontrino := " + WebService.listOfLottery[i].GetNumScont + " TillID := " + WebService.listOfLottery[i].GetTillID + " in Data := " + WebService.listOfLottery[i].GetDate);
                        log.Error("Expected an Error Code , received := " + WebService.listOfLottery[i].CodError);
                        //throw new PosControlException();
                    }
                    //l'alternativa è che non sia uno scontrino lotteria per cui devo avere come result il codice "05", alias Not found
                    if (!(WebService.listOfLottery[i].IsLottery) && (String.Compare(resultCode, "05") != 0))
                    {
                        log.Error("Errore DirectIO 9218 su uno scontrino NON di lotteria scartato con ZREP :=  " + WebService.listOfLottery[i].GetZRep + " Numero Scontrino := " + WebService.listOfLottery[i].GetNumScont + " TillID := " + WebService.listOfLottery[i].GetTillID + " in Data := " + WebService.listOfLottery[i].GetDate);
                        log.Error("Expected Result Code := 05 , received := " + resultCode);
                        //throw new PosControlException();
                    }
                    //check sull id answer della 9218 e l'idanswer che ricevo dall'Ade
                    if (String.Compare(_IdAnswer, IdAnswer) != 0)
                    {
                        log.Error("Errore IdAnswer su uno scontrino di lotteria con ZREP :=  " + WebService.listOfLottery[i].GetZRep + " Numero Scontrino := " + WebService.listOfLottery[i].GetNumScont + " TillID := " + WebService.listOfLottery[i].GetTillID + " in Data := " + WebService.listOfLottery[i].GetDate);
                        log.Error("Expected IdAnswer := " + url.Substring(0, 50) + " , received := " + IdAnswer);
                    }
                }
                //Mi accerto che ciò che mi da la 1134 relativo al NumLotRej sia uguale al mio contatore degli scartati relativo (ovviamente) a quello ZRep e quella Data
                //TODO: 07062020 questo test è l'analogo per i NumLotOk del metodo GestioneACResponse per lo stesso motivo non va bene sto test, quindi lo tolgo o lo sposto
                /*
                if (Convert.ToInt32(NumLotRej) != LotRej)
                {
                    log.Error("Anomalia riscontrata nel doc risposta " + urlString);
                    log.Error("Nel documento ci sono " + LotRej + " scontrini rigettati mentre la 1134 mi da " + NumLotRej + " scontrini rigettati ");
                    //throw new PosControlException();
                }
                */
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    //Console.WriteLine(e.ToString());
                    log.Error("Generic Error: ", e);

                }
            }

            return NumExceptions;
        }

        //Metodo interno per gestire la risposta SC di un doc lotteria xml.
        //Facciamo chiarezza: Se ho SC vuol dire che son tutti scartati MA....
        //dal doc risposta posso ricavarmi ZRep, Inizio Scontrino e Fine Scontrino ma all'interno ci possono essere
        //degli scontrini senza lotteria per cui in questo caso devo chiamare il parser 
        //WebService.LotteryFolderParser del doc xml inviato relativo (basta sostituire nel path l'id risposta con "LOTTERIA.XML"
        //WebService.LotteryFolderParser viene chiamato prima di GestioneSCResponse per cui la struttura WebService.listOfLottery è già creata
        private static void GestioneSCResponse(string urlString)

        {
            try
            {

                string url = urlString;
                
                Lottery lt = new Lottery();
                string result = String.Empty;
                string IdAnswer = String.Empty;
                for (int i = 0; i < WebService.listOfLottery.Count(); i++)
                {

                    //ReadLotteryStatus(WebService.listOfLottery[i].GetDate, WebService.listOfLottery[i].GetZRep);
                    lt.ReadLotteryStatusSingleReceipt(WebService.listOfLottery[i].GetZRep, WebService.listOfLottery[i].GetNumScont, WebService.listOfLottery[i].GetTillID.PadLeft(8, '0'), WebService.listOfLottery[i].GetDate, "00", ref result, ref IdAnswer);
                    //Gli scontrini dei doc SC non possono mai essere buoni ma solo con error code 03 o 04 al massimo
                    if ((WebService.listOfLottery[i].IsLottery) && ((String.Compare(result, "03") != 0) &&  ((String.Compare(result, "04") != 0))))
                    {
                        log.Error("Errore DirectIO 9218 su uno scontrino di lotteria con ZREP :=  " + WebService.listOfLottery[i].GetZRep + " Numero Scontrino := " + WebService.listOfLottery[i].GetNumScont + " in Data := " + WebService.listOfLottery[i].GetDate);
                        log.Error("Expected Error Code := FFFFF , received := " + WebService.listOfLottery[i].CodError);
                        throw new PosControlException();
                    }
                    //Check sull'error code relativo ad uno scontrino scartato
                    if ((WebService.listOfLottery[i].IsLottery) && (String.Compare(result, "03") == 0) && (WebService.listOfLottery[i].CodError == "FFFFF"))
                    {
                        log.Error("Errore DirectIO 9218 su uno scontrino di lotteria scartato con ZREP :=  " + WebService.listOfLottery[i].GetZRep + " Numero Scontrino := " + WebService.listOfLottery[i].GetNumScont + " in Data := " + WebService.listOfLottery[i].GetDate);
                        log.Error("Expected an Error Code , received := " + WebService.listOfLottery[i].CodError);
                        //throw new PosControlException();
                    }
                    //l'alternativa è che non sia uno scontrino lotteria per cui devo avere come result il codice "05", alias Not Found
                    if (!(WebService.listOfLottery[i].IsLottery) && (String.Compare(result, "05") != 0))
                    {
                        log.Error("Errore DirectIO 9218 su uno scontrino NON di lotteria con ZREP :=  " + WebService.listOfLottery[i].GetZRep + " Numero Scontrino := " + WebService.listOfLottery[i].GetNumScont + " in Data := " + WebService.listOfLottery[i].GetDate);
                        log.Error("Expected Result Code := 05 , received := " + result);
                        throw new PosControlException();
                    }
                }
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Error("Generic Error: ", e);

                }
            }
        }


    }
    

    //Classe creata per la gestione del nuovo corrispettivo
    public class Xml2 : FiscalReceipt
    {
        // something that will read the XML file
        private XmlReader reader = null;

        // define the settings that I use while reading the XML file.
        private XmlReaderSettings settings;

        public enum Supported_HA
        {
            SHA256, SHA384, SHA512
        }

        //Struct rappresentante il campo Riepilogo dell' Xml
        public struct Riepilogo
        {
            public string VentilazioneIVA { get; set; } //Solo quando un reparto è ventilato, in quel caso ci vedo <SI>
            public string AliquotaIVA { get; set; } //Pur non esistendo per natura N4 ci metto 0 
            public string Imposta { get; set; }     //Pur non esistendo per natura N4 ci metto 0 
            public string Ammontare { get; set; }        
            public string ImportoParziale { get; set; }
            public string TotaleAmmontareResi { get; set; }
            public string TotaleAmmontareAnnulli { get; set; }
            public string BeniInSospeso { get; set; }
            public string NonRiscossoServizi { get; set; }
            public string NonRiscossoFatture { get; set; }
            public string TotaleDaFattureRT { get; set; }
            public string NonRiscossoDCRaSSN { get; set; }
            public string NonRiscossoOmaggio { get; set; }
            public string CodiceAttivita { get; set; }

        }
        public struct Totali
        {
            public string NumeroDocCommerciali { get; set; }
            public string PagatoContanti { get; set; }
            public string PagatoElettronico { get; set; }
            public string ScontoApagare { get; set; }
            public string PagatoTicket { get; set; }
            public string NumeroTicket { get; set; }
        }

        public struct XmlStruct
        {
            public string Progressivo;
            public string DataOraRilevazione;
            public Riepilogo[] riepilogos;
            public Totali totali;

            
        }

        public XmlStruct XmlStructCreate()
        {
            XmlStruct output = new XmlStruct();
            output.Progressivo = String.Empty;
            output.DataOraRilevazione = string.Empty;
            output.riepilogos = new Riepilogo[6];
            for (int i = 0; i < 6; ++i)
            {
                switch (i)
                {
                    case 0:
                        output.riepilogos[0].AliquotaIVA = "N4";
                        break;
                    case 1:
                        output.riepilogos[1].AliquotaIVA = "22.00";
                        break;
                    case 2:
                        output.riepilogos[2].AliquotaIVA = "10.00";
                        break;
                    case 3:
                        output.riepilogos[3].AliquotaIVA = "5.00";
                        break;
                    case 4:
                        output.riepilogos[4].AliquotaIVA = "4.00";
                        break;
                    case 5:
                        output.riepilogos[5].AliquotaIVA = "NO"; //Per la ventilazione
                        break;

                }

                output.riepilogos[i].Imposta = String.Empty;
                output.riepilogos[i].Ammontare = String.Empty;
                output.riepilogos[i].VentilazioneIVA = String.Empty;
                output.riepilogos[i].ImportoParziale = String.Empty;
                output.riepilogos[i].TotaleAmmontareResi = String.Empty;
                output.riepilogos[i].TotaleAmmontareAnnulli = String.Empty;
                output.riepilogos[i].BeniInSospeso = String.Empty;
                output.riepilogos[i].NonRiscossoServizi = String.Empty;
                output.riepilogos[i].NonRiscossoFatture = String.Empty;
                output.riepilogos[i].TotaleDaFattureRT = String.Empty;
                output.riepilogos[i].NonRiscossoDCRaSSN = String.Empty;
                output.riepilogos[i].NonRiscossoOmaggio = String.Empty;
                output.riepilogos[i].CodiceAttivita = String.Empty;

            }
            output.totali.NumeroDocCommerciali = String.Empty;
            output.totali.PagatoContanti = String.Empty;
            output.totali.PagatoElettronico = String.Empty;
            output.totali.ScontoApagare = String.Empty;
            output.totali.PagatoTicket = String.Empty;
            output.totali.NumeroTicket = String.Empty;

            return output;
        }


        //
        public Xml2()
        {
            try
            {
               

                //12/03/2020 TODO: Controllare se mi serve o meno l'oggetto FiscalPrinter
                if (!opened)
                {
                    fiscalprinter = (FiscalPrinter)posCommonFP;
                    //Console.WriteLine("Performing Open() method ");
                    fiscalprinter.Open();

                    //Console.WriteLine("Performing Claim() method ");
                    fiscalprinter.Claim(1000);

                    //Console.WriteLine("Setting DeviceEnabled property ");
                    fiscalprinter.DeviceEnabled = true;

                    //Console.WriteLine("Performing ResetPrinter() method ");
                    //fiscalprinter.ResetPrinter();
                }

                //Console.WriteLine("Performing ResetPrinter() method ");
                //fiscalprinter.ResetPrinter();

               

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error", e);
                }
            }
        }



        private void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            throw new Exception();
        }


        public bool Xml2Validate2()
        {
            try
            {
                XmlDocument xmld = new XmlDocument();
                xmld.LoadXml(@"D:\Epson_Copia_Chiavetta_Gialla2\ToolAggiornato\PosTestWithNunit\FiscalReceipt\test.xml");
                xmld.Schemas.Add(null, @"CorrispettiviTypes v1.0 (tracciato V 7.0 marzo 2020) - aggiornato al 11 ... (1)");
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(this.ValidationEventHandle);
                xmld.Validate(this.ValidationEventHandle);
                return true;
            }
            catch(Exception e)
            {
                log.Fatal("Generic Error: ", e);
                return false;
            }
        }

        //Metodo che valida l'XML 2.0 rispetto al XSD relativo
        public int Xml2Validate()
        {
            //Qui ci metto la parte di codice che controlla l'xml con l'xsd dei corr. 2.0
            //Diciamo che per ora lo creo io a mano l'xml e poi uso sto metodo per parsarlo
            //e per controllare che rispetti le regole dell'XSD

            try
            {

                //parsing della directory XmlFolder con tutti i file xml di test
                string[] fileArray = Directory.GetFiles(@"D:\Epson_Copia_Chiavetta_Gialla2\ToolAggiornato\PosTestWithNunit\FiscalReceipt\SoloChiusure\", "*.xml", SearchOption.TopDirectoryOnly);
                //string[] fileArray = Directory.GetFiles(@"C:\Users\BVittorino\Downloads\Chiusure Rifiutate\", "*.xml", SearchOption.TopDirectoryOnly);


                // XSD

                settings = new XmlReaderSettings();
                //settings.Schemas.Add(null, @"G:\Epson_Copia_Chiavetta_Gialla2\ToolAggiornato\PosTestWithNunit\FiscalReceipt\CorrispettiviTypes v1.0 (tracciato V 7.0 marzo 2020).xsd");
                settings.Schemas.Add(null, @"D:\Epson_Copia_Chiavetta_Gialla2\ToolAggiornato\PosTestWithNunit\FiscalReceipt\Corrispettivi xsd agg 24032017_CorrispettiviTypes_v1.0.xsd");
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(this.ValidationEventHandle);

                foreach (string namefile in fileArray)
                {
                    
                    //Per ogni xml file mi genero un corrispettivo .txt parsato
                    string extension = Path.GetExtension(namefile);
                    string mytxtFile = Path.ChangeExtension(namefile, ".txt");

                    // validate the filewith the given setting.
                    // reader = new XmlTextReader(namefile);

                    reader = XmlReader.Create(namefile, settings);

                    //Check su un eventuale risultato di parsing precedente da eliminare
                    if (File.Exists(mytxtFile))
                    {
                        File.Delete(mytxtFile);
                    }

                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element: // The node is an element.

                                
                                  Console.WriteLine("\n Name " + reader.Name);
                                  Console.Write("\n Local Name " + reader.LocalName);
                                  //Console.WriteLine("\n Value " + reader.Value);
                                  //Console.WriteLine("\n Depth " + reader.Depth);
                                  //Console.WriteLine("\n Attribute Count " + reader.AttributeCount);
                                 
                                if (reader.AttributeCount > 2)
                                {
                                    string lines = "";
                                    for (int i = 0; i < reader.AttributeCount; ++i)
                                    {

                                        //qui mi devo creare una struttura dove memorizza nome metodo ,variabili e numero di iterazioni.
                                        // AttributeCount -2 mi da il numero delle var, l'ultimo il num delle iterazioni
                                        // devo metterle in un array di struct per es o trovare altro

                                        reader.MoveToAttribute(i);
                                        lines += reader.Value + "\r\n";



                                    }
                                    // Write the string to a file in append mode
                                    System.IO.StreamWriter file = new System.IO.StreamWriter(mytxtFile, true);
                                    file.WriteLine(lines);

                                    file.Close();
                                }
                                /*
                                reader.MoveToElement();
                                reader.MoveToFirstAttribute();
                                reader.MoveToNextAttribute();
                                */

                                break;
                            case XmlNodeType.Text: //Display the text in each element.
                                                   
                                                   Console.WriteLine("\n " + reader.Value);
                                                   for (int i = 0; i < reader.AttributeCount; ++i)
                                                   {    
                                                        reader.MoveToAttribute(i); 
                                                   }
                                                   reader.MoveToElement();
                                                   reader.MoveToFirstAttribute();
                                                   reader.MoveToNextAttribute();
                                                   
                                break;
                            case XmlNodeType.Attribute: //Display the attribute of the element
                                                        
                                                        Console.WriteLine("\n Value " + reader.Value);
                                                         
                                break;
                        }
                    }
                    Console.WriteLine("Validation of file " + namefile + " Passed");

                    //ComputeHash("password", Supported_HA.SHA256 , null);
                    //Chiamo il metodo che mi invia l'xml dei corrispettivi via cURL
                    //todo: questo sarebbe da rimettere
                    SendXML2(namefile.Substring(49));
                }


               

            }
            catch(Exception e)
            {
                NumExceptions++;
                if(e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                }
                else
                {
                    log.Fatal("Generic Error: " , e);
                }

            }
            return NumExceptions;
        }

        private void ValidationEventHandle(object sender, ValidationEventArgs arg)
        {
            //If we are here, it's because something is going wrong with my XML.
            log.Error("\r\n\t Validation XML failed: " + arg.Message);

            // throw an exception.
            throw new Exception("Validation XML failed: " + arg.Message);
        }

        public void SendXML2(string document)
        {
            try
            {
                //Qui ci mettero' il codice per chiamare curl via batch per inviare l'xml firmato : devo avere l'xml firmato da Francesco
                //e trovare il link a cui mandare i dati all' ADE
                //Devo trovare come e dove curl memorizza la risposta
                //Send xml via cURL

                const string command = @"D:\Epson_Copia_Chiavetta_Gialla2\ToolAggiornato\PosTestWithNunit\FiscalReceipt\SoloChiusure\";

                // Use ProcessStartInfo class
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = @"C:\Windows\System32\curl.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //startInfo.Arguments = "-XPOST -k  --header 'Content-Type: application/xml' --header 'Accept: application/xml' 'https://apid-ivaservizi.agenziaentrate.gov.it/v1/dispositivi/corrispettivi/' --data-binary " + "@" + command + document + " --verbose --output "  + command + "risposta.xml --progress-bar ";
                startInfo.Arguments = "-v --insecure -H 'Content-Type: application/xml'  --tlsv1.2 --cacert CA_AE_sperimentazione.pem https://v-apid-ivaservizi.agenziaentrate.gov.it/v1/dispositivi/corrispettivi/ --data-binary  " + "@" + command + document + " --output " + command + "risposta.xml ";

                //Console.WriteLine("\"These two semi colons are removed when i am printed\"");
                Console.WriteLine(startInfo.Arguments);
                /*
                    " --header 'Content-Type:application/xml'" +
                    " --header 'Accept: application/xml' " +
                    "'https://v-apid-ivaservizi.agenziaentrate.gov.it/v1/dispositivi/corrispettivi/' " +
                    "--data-binary " + command + "99MEY000066-20200312T110024-1618-CORRISP_diprova.xml" +
                    " --output "  + command + "risposta.xml " + " --progress-bar";  
                */
                //Process.Start(startInfo);

                
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
                
                
            }
            catch (Exception e)
            {
                NumExceptions++;
                log.Fatal("Eccezione sollevata nell'invio del xml via curl: ", e);

            }
        }
        
        //Test iniziali sul curl, mi sa che la devo togliere sta qui! TODO:

        public static string ComputeHash(string plainText, Supported_HA hash, byte[] salt)
        {
            int minSaltLength = 4, maxSaltLength = 16;

            byte[] SaltBytes = null;
            if (salt != null)
            {
                SaltBytes = salt;
            }
            else
            {
                Random r = new Random();
                int SaltLength = r.Next(minSaltLength, maxSaltLength);
                SaltBytes = new byte[SaltLength];
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetNonZeroBytes(SaltBytes);
                rng.Dispose();
            }

            byte[] plainData = ASCIIEncoding.UTF8.GetBytes(plainText);
            byte[] plainDataWithSalt = new byte[plainData.Length + SaltBytes.Length];

            for (int x = 0; x < plainData.Length; x++)
                plainDataWithSalt[x] = plainData[x];
            for (int n = 0; n < SaltBytes.Length; n++)
                plainDataWithSalt[plainData.Length + n] = SaltBytes[n];

            byte[] hashValue = null;

            switch (hash)
            {
                case Supported_HA.SHA256:
                    SHA256Managed sha = new SHA256Managed();
                    hashValue = sha.ComputeHash(plainDataWithSalt);
                    sha.Dispose();
                    break;
                case Supported_HA.SHA384:
                    SHA384Managed sha1 = new SHA384Managed();
                    hashValue = sha1.ComputeHash(plainDataWithSalt);
                    sha1.Dispose();
                    break;
                case Supported_HA.SHA512:
                    SHA512Managed sha2 = new SHA512Managed();
                    hashValue = sha2.ComputeHash(plainDataWithSalt);
                    sha2.Dispose();
                    break;
            }

            byte[] result = new byte[hashValue.Length + SaltBytes.Length];
            for (int x = 0; x < hashValue.Length; x++)
                result[x] = hashValue[x];
            for (int n = 0; n < SaltBytes.Length; n++)
                result[hashValue.Length + n] = SaltBytes[n];

            return Convert.ToBase64String(result);
        }


        //Metodo che parsa l'XML 2.0 e recupera tutti i vari dati con cui riempire la XmlStruct
        public int Xml2HTMLParser(string path,ref XmlStruct xmlStruct, string YYYYMMAA, string zrep)
        {
            log.Info("Performing Xml2Parser Method");
            try
            {

                bool found = false;
                string regex = zrep.PadLeft(4,'0') + @"-CORRISP.xml$";
                string URL = String.Empty;
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                WebService ws;
                //DirectIO 4219 GET LAN PARAMETERS       
                //Prima leggo il parametro DHCP
                strObj[0] = "31";
                dirIO = posCommonFP.DirectIO(0, 4219, strObj);
                iData = dirIO.Data;
                
                iObj = (string[])dirIO.Object;

                strObj[0] = "31";
                dirIO = posCommonFP.DirectIO(0, 4219, strObj);

                iData = dirIO.Data;  
                iObj = (string[])dirIO.Object;
                //check in it's in DHCP or not
                if (String.Compare(iObj[0].Substring(3,1), "1") == 0)
                {
                    strObj[0] = "32"; //E' in DHCP
                }
                else
                {
                    strObj[0] = "01";
                }
                dirIO = posCommonFP.DirectIO(0, 4219, strObj);

                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                if (String.Compare(strObj[0], "01") == 0)
                {
                    int uno = Int32.Parse(iObj[0].Substring(3, 3));
                    string primo = uno.ToString();
                    int due = Int32.Parse(iObj[0].Substring(7, 3));
                    string secondo = due.ToString();
                    int tre = Int32.Parse(iObj[0].Substring(11, 3));
                    string terzo = tre.ToString();
                    int quattro = Int32.Parse(iObj[0].Substring(15, 3));
                    string quarto = primo + "." + secondo + "." + terzo + "." + quattro.ToString();
                    URL = "http://" + quarto + "/www/dati-rt/" + YYYYMMAA + @"/";
                }
                else
                {
                    int length = iObj[0].IndexOf(' '); //var di appoggio per trovare l'ip 
                    System.Net.IPAddress ipAddress = System.Net.IPAddress.Parse(iObj[0].Substring(3, length - 3));
                    URL = "http://" + ipAddress.ToString() + "/www/dati-rt/" + YYYYMMAA + @"/";
                }
                


                //string URL = @"G:\Server\XmlPerServer\Appoggio\dati-rt";

                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                // There are various options, set as needed
                htmlDoc.OptionFixNestedTags = true;



                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                
                string credentials = "epson:epson";
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

                String username = "epson";
                String password = "epson";
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                request.Headers.Add("Authorization", "Digest " + encoded);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                CookieContainer myContainer = new CookieContainer();
                request.Credentials = new NetworkCredential(username, password);
                request.CookieContainer = myContainer;
                request.PreAuthenticate = true;
                
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;


                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    //string data = reader.ReadToEnd();


                    // filePath is a path to a file containing the html
                    htmlDoc.Load(reader);
                    //htmlDoc.Load(URL);

                    // Use:  htmlDoc.LoadHtml(xmlString);  to load from a string (was htmlDoc.LoadXML(xmlString)

                    // ParseErrors is an ArrayList containing any errors from the Load statement
                    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                    {
                        // Handle any parse errors as required
                        //mancano 4 tag di chiusura nel WebService

                    }

                    if (htmlDoc.DocumentNode != null)
                    {
                        HtmlAgilityPack.HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//@href");
                        HtmlAgilityPack.HtmlNodeCollection bodyNode2 = htmlDoc.DocumentNode.SelectNodes("//@href");

                        if (bodyNode != null)
                        {
                            // Do something with bodyNode
                            //Console.WriteLine("Test");
                        }


                        if (bodyNode2.Count != 0)
                        {
                            /*
                            for (int i = 3; i < bodyNode2.Count; i = i + 2)
                            {
                                //Console.WriteLine(bodyNode2[i].GetDirectInnerText());
                                LotteryFolderParser(bodyNode2[i].GetDirectInnerText() ,ref scontrini);
                            }
                            */
                            for (int i = 0; i < bodyNode2.Count; i++)
                            {
                                
                                // if (isWhatILookingFor(bodyNode2[i].GetDirectInnerText() , @"^\d{8}\/$"))
                                if (WebService.isWhatILookingFor(bodyNode2[i].GetDirectInnerText(), @regex))
                                {
                                    
                                    //InnerHTMLParser(URL + bodyNode2[i].GetDirectInnerText() + "/");
                                    //Console.WriteLine("Ho trovato lo xml giusto");
                                    Xml2Parser(URL + bodyNode2[i].GetDirectInnerText(), ref xmlStruct);
                                    found = true;
                                }
                            }

                        }

                    }

                }

              if (found == false)
                {
                    log.Error("Documento ZReport non trovato!!!");
                }

            }
            catch (Exception e)
            {
                NumExceptions++;
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                }
                else
                {
                    log.Fatal("Generic Error: ", e);
                }

            }
            return NumExceptions;
        }



        // Parser HTML interno (cioè all'interno di una specifica data) 
        // 
        public static int Xml2Parser(string path, ref XmlStruct xmlStruct)
        {
            // URL di prova
            String urlString = path;

            string[] strObj = new string[1];
            DirectIOData dirIO;
            int iData;
            string[] iObj = new string[1];

            try
            {

                
                // something that will read the XML file
                XmlTextReader reader = null;


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlString);
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;


                string credentials = "epson:epson";
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

                String username = "epson";
                String password = "epson";
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                request.Headers.Add("Authorization", "Digest " + encoded);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                CookieContainer myContainer = new CookieContainer();
                request.Credentials = new NetworkCredential(username, password);
                request.CookieContainer = myContainer;
                request.PreAuthenticate = true;

               

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    Stream responseStream = response.GetResponseStream();

                    reader = new XmlTextReader(urlString, responseStream);


                    log.Info("Parsing " + urlString + " file");
                    //XmlStruct xmlStruct = new XmlStruct();

                    //xmlStruct = XmlStructCreate();

                    int index = 0;
                    //reader = new XmlTextReader(path);

                   

                    while (reader.Read())
                    {

                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element: // The node is an element.

                                switch (reader.Name)
                                {
                                    case ("Progressivo"):
                                        {
                                            reader.Read();
                                            xmlStruct.Progressivo = reader.Value;
                                        }
                                        break;

                                    case ("DataOraRilevazione"):
                                        {
                                            reader.Read();
                                            xmlStruct.DataOraRilevazione = reader.Value;
                                        }
                                        break;
                                    case ("AliquotaIVA"): //qui loopo per riempire tutto il riepilogo relativo
                                        {
                                            reader.Read();
                                            switch (reader.Value)
                                            {
                                                case ("22.00"):
                                                    index = 1;
                                                    break;
                                                case ("10.00"):
                                                    index = 2;
                                                    break;
                                                case ("5.00"):
                                                    index = 3;
                                                    break;
                                                case ("4.00"):
                                                    index = 4;
                                                    break;
                                            }
                                            xmlStruct.riepilogos[index].AliquotaIVA = reader.Value;

                                        }
                                        break;
                                    case ("Natura"):
                                        {
                                            reader.Read();
                                            index = 0;
                                        }
                                        break;
                                    case ("VentilazioneIVA"):
                                        {
                                            reader.Read();
                                            index = 5;
                                            xmlStruct.riepilogos[index].VentilazioneIVA = reader.Value; //Ci metto il "SI" come da Xml
                                            xmlStruct.riepilogos[index].Imposta = String.Empty;
                                            xmlStruct.riepilogos[index].AliquotaIVA = String.Empty;
                                        }
                                        break;
                                    case ("NumeroDocCommerciali"):
                                        {
                                            reader.Read();
                                            xmlStruct.totali.NumeroDocCommerciali = reader.Value;
                                        }
                                        break;
                                    case ("PagatoContanti"):
                                        {
                                            reader.Read();
                                            xmlStruct.totali.PagatoContanti = reader.Value;
                                        }
                                        break;
                                    case ("PagatoElettronico"):
                                        {
                                            reader.Read();
                                            xmlStruct.totali.PagatoElettronico = reader.Value;
                                        }
                                        break;
                                    case ("ScontoApagare"):
                                        {
                                            reader.Read();
                                            xmlStruct.totali.ScontoApagare = reader.Value;
                                        }
                                        break;
                                    case ("PagatoTicket"):
                                        {
                                            reader.Read();
                                            xmlStruct.totali.PagatoTicket = reader.Value;
                                        }
                                        break;
                                    case ("NumeroTicket"):
                                        {
                                            reader.Read();
                                            xmlStruct.totali.NumeroTicket = reader.Value;
                                        }
                                        break;
                                    case ("Imposta"):
                                        {
                                            reader.Read();
                                            xmlStruct.riepilogos[index].Imposta = reader.Value;
                                        }
                                        break;
                                    case ("Ammontare"):
                                        {
                                            reader.Read();
                                            xmlStruct.riepilogos[index].Ammontare = reader.Value;
                                        }
                                        break;
                                    case ("ImportoParziale"):
                                        {
                                            reader.Read();
                                            xmlStruct.riepilogos[index].ImportoParziale = reader.Value;
                                        }
                                        break;
                                    case ("TotaleAmmontareResi"):
                                        {
                                            reader.Read();
                                            xmlStruct.riepilogos[index].TotaleAmmontareResi = reader.Value;
                                        }
                                        break;
                                    case ("BeniInSospeso"):
                                        {
                                            reader.Read();
                                            xmlStruct.riepilogos[index].BeniInSospeso = reader.Value;
                                        }
                                        break;
                                    case ("NonRiscossoServizi"):
                                        {
                                            reader.Read();
                                            xmlStruct.riepilogos[index].NonRiscossoServizi = reader.Value;
                                        }
                                        break;
                                    case ("NonRiscossoFatture"):
                                        {
                                            reader.Read();
                                            xmlStruct.riepilogos[index].NonRiscossoFatture = reader.Value;
                                        }
                                        break;
                                    case ("TotaleDaFattureRT"):
                                        {
                                            reader.Read();
                                            xmlStruct.riepilogos[index].TotaleDaFattureRT = reader.Value;
                                        }
                                        break;
                                    case ("NonRiscossoDCRaSSN"):
                                        {
                                            reader.Read();
                                            xmlStruct.riepilogos[index].NonRiscossoDCRaSSN = reader.Value;
                                        }
                                        break;
                                    case ("NonRiscossoOmaggio"):
                                        {
                                            reader.Read();
                                            xmlStruct.riepilogos[index].NonRiscossoOmaggio = reader.Value;
                                        }
                                        break;
                                    case ("CodiceAttivita"):
                                        {
                                            reader.Read();
                                            xmlStruct.riepilogos[index].CodiceAttivita = reader.Value;
                                        }
                                        break;
                                    case ("TotaleAmmontareAnnulli"):
                                        {
                                            reader.Read();
                                            xmlStruct.riepilogos[index].TotaleAmmontareAnnulli = reader.Value;
                                        }
                                        break;

                                }


                                break;
                            case XmlNodeType.Text: //Display the text in each element.

                                /*
                                for (int i = 0; i < reader.AttributeCount; ++i)
                                {
                                    reader.MoveToAttribute(i);
                                }
                                reader.MoveToElement();
                                reader.MoveToFirstAttribute();
                                reader.MoveToNextAttribute();
                                */

                                break;
                            case XmlNodeType.Attribute: //Display the attribute of the element


                                break;

                        }



                    }
                }

                log.Info("Parsing of file " + path + " Terminated");

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions += 1;
                {

                    log.Error("Error Parsing Url " + urlString, e);

                }


            }
            return NumExceptions;
        }






        //TODO : 06/04/2020
        //Metodo che cerca di creare tutte le forme di pagamenti possibili per l'Xml 2.0
        //E' un metodo che andrà chiamato in maniera simile a TestFormePagamento per testare
        //i totalizzatori: in questo caso si dovranno integrare i nuovi totalizzatori che
        //derivano dalle nuove forme di pagamento e dalle nuovi voci che andranno a comporre l'Xml 2.0
        //Routine che effettua tutte le forme di pagamento disponibili al momento
        public int FormePagamentoXml20()
        {
            try
            {
               log.Info("Performing FormePagamentoXml20() Method");

                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST CONTANTE", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "000CONTANTI");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST CREDITO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)500000, "200CREDITO");

                fiscalprinter.EndFiscalReceipt(false);



                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST ASSEGNO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "100ASSEGNO");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST CARTA DI CREDITO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "201CARTA DI CREDITO");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST ALTRO PAGAMENTO", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "203ALTRO PAGAMENTO");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST BANCOMAT", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "204BANCOMAT");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST TICKET", (decimal)10000, (int)1000, (int)1, (decimal)100000, "");

                fiscalprinter.PrintRecTotal((decimal)100000, (decimal)100000, "301TICKET");

                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("TEST PAMANENTO MULTIPLI", (decimal)1000000, (int)1000, (int)1, (decimal)10000000, "");

                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "000CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "200CREDITO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "100ASSEGNO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "201CARTA DI CREDITO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "203ALTRO PAGAMENTO");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)200000, "204BANCOMAT");
                fiscalprinter.PrintRecTotal((decimal)10000, (decimal)00000, "301TICKET");

                fiscalprinter.EndFiscalReceipt(false);

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                { Console.WriteLine(e.ToString());
                    log.Fatal("", e);

                }

                return NumExceptions;
            }
            return NumExceptions;
        }

        
        //Metodo per generare uno scontrino random e/o complicato per prove
        public int TestOracle()
        {
            try
            {

                log.Info("Performing TestOracle Method() Esempio 7");

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                object iObj;

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene \"A\" ", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");


                string description = "BUONO MULTIUSO";
                string amount = "12000";
                string type = "6"; //Tipo associato allo Sconto a pagare
                string subtype = "01"; //Sottotipo associato all type

                PaymentCommand(description, amount, type, subtype);
                fiscalprinter.PrintRecTotal((decimal)020000, (decimal)00000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                //fiscalprinter.EndFiscalReceipt(false);
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO


                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di chiudere lo scontrino");
                    //throw new Exception();
                }
                fiscalprinter.ResetPrinter(); //necessario per il driver window



                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)0006900, (int)1000, (int)1, (decimal)0006900, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)006500, (int)1);
                //fiscalprinter.PrintRecItem("Servizio B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                //fiscalprinter.PrintRecItem("Servizio C", (decimal)1000000, (int)1000, (int)1, (decimal)1000000, "");

                fiscalprinter.PrintRecSubtotal((decimal)0000400);
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)000400, (int)1);
                //PaymentCommand("Sconto A Pagare", "2", "6", "00");
                //fiscalprinter.PrintRecTotal((decimal)0000200, (decimal)0000000, "201CREDITO");
                //fiscalprinter.PrintRecTotal((decimal)0000000, (decimal)000000, "204BANCOMAT");
                //fiscalprinter.PrintRecTotal((decimal)0000000, (decimal)000000, "100ASSEGNO");
                //fiscalprinter.PrintRecTotal((decimal)1000200, (decimal)1000000, "0CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)000000, (decimal)00000, "0CONTANTI");


                PrintTaxCode("NVRNDR59B10F205D");
                //fiscalprinter.EndFiscalReceipt(false);
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO


                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di chiudere lo scontrino");
                    //throw new Exception();
                }
                fiscalprinter.ResetPrinter(); //necessario per il driver windows
                ZReport();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        //Esempio 1
        //Metodo per generare un esempio di scontrino dove pago un acconto per un bene in sospeso
        public int Acconto()
        {
            try
            {
                log.Info("Performing Acconto Method() Esempio 1");
                XmlStruct xmlStruct = new XmlStruct();
                
                //Creo primo uno scontrino rappresentante l'acconto e poi quello in cui lo utilizzo

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Acconto Prodotto A", (decimal)500000, (int)1000, (int)1, (decimal)500000, "");
                fiscalprinter.PrintRecItem("Prodotto B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Prodotto C", (decimal)1000000, (int)1000, (int)13, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)2000000);
                fiscalprinter.PrintRecTotal((decimal)2000000, (decimal)1000000, "0CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)2000000, (decimal)800000, "204BANCOMAT");
                fiscalprinter.PrintRecTotal((decimal)2000000, (decimal)200000, "100ASSEGNO");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);


                //Ora stampo l'esempio vero e proprio dove utilizzo tale acconto
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)12000) };
               
                //TODO: Decidere se farla o meno, per ora mi interferisce con il test su questo scontrino perchè mi azzera i totalizz

                //ZReport();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        //Esempio 2
        //Metodo per generare un esempio di scontrino dove utilizzo l'acconto rappresentato da uno scontrino precedente
        //EDIT: 27102020 ci ho aggiunto uno zrep in uscita che mi prendo da TestAcconto
        public int AccontoConsegnaBene(ref string zrep)
        {
            try
            {
               
                log.Info("Performing AccontoConsegnaBene Method() Esempio 2");

                //Ora stampo l'esempio vero e proprio dove utilizzo tale acconto
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)12000) };

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Prodotto A", (decimal)1000000, (int)1000, (int)1, (decimal)1000000, "");

                //PagamentiNegativi("Acconto", "5000", "00", "01");
                //Test PrintRecItemAdjustment in sostituzione del metodo PagamentiNegativi (DirectIO 1090)
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000Acconto", 500000, 1);

                //fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Acconto Prodotto A", (decimal)500000, (int)1);

                fiscalprinter.PrintRecItem("Prodotto B", (decimal)100000, (int)1000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Prodotto C", (decimal)1000000, (int)1000, (int)13, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)1600000);


                fiscalprinter.PrintRecTotal((decimal)1600000, (decimal)1000000, "0CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)1600000, (decimal)600000, "204BANCOMAT");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);
                
                //ZReport(); se faccio chiusura azzero i totalizzatori da testare , non va bene
                //Gli passo cmq quella che sarà la prossima chiusura
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zrep = (Convert.ToInt32(ZRep) + 1).ToString();
               

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        //Esempio 2Bis
        //Metodo per generare un esempio di scontrino dove utilizzo l'acconto generico, non associato cioè ad un bene preciso (acconto senza individuazione del bene)
        public int AccontoGenerico(ref string zRep)
        {
            try
            {
                log.Info("Performing AccontoGenerico Method() Esempio 2Bis");

                //Creo primo uno scontrino rappresentante l'acconto e poi quello in cui lo utilizzo

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Acconto ", (decimal)220000, (int)1000, (int)13, (decimal)220000, "");
                fiscalprinter.PrintRecSubtotal((decimal)220000);
                fiscalprinter.PrintRecTotal((decimal)220000, (decimal)220000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.EndFiscalReceipt(false);


                //Ora stampo l'esempio vero e proprio dove utilizzo tale acconto
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)12000) };


                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");

                fiscalprinter.PrintRecItem("Bene B", (decimal)500000, (int)1000, (int)3, (decimal)500000, "");
                fiscalprinter.PrintRecSubtotal((decimal)1720000);
                //PaymentCommand("ACCONTO", "2200", "6", "00");
                //Test PrintRecTotal nuova
                fiscalprinter.PrintRecTotal((decimal)1720000, (decimal)220000, "600ACCONTO");

                fiscalprinter.PrintRecTotal((decimal)1500000, (decimal)1500000, "0CONTANTI");

                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);
                
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        //Esempio 3: Bene consegnato
        //Generazione scontrino relativo al nome (praticamente è uno scontrino di vendita normale, l'esercente si assume l'onere di ricevere il pagamento totale)
        public int VenditaBeniPagamentoNRBeneConsegnato(ref string zRep)
        {
            try
            {
                log.Info("Performing VenditaBeniPagamentoNRBeneConsegnato Method() Esempio 3");
                
                //fiscalprinter.PrintZReport();

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)12000) };


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("Prodotto A", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);

                fiscalprinter.PrintRecItem("PRODOTTO B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("PRODOTTO C", (decimal)1000000, (int)1000, (int)13, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)2000000, "0CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)800000, "204BANCOMAT");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)100000, "100ASSEGNI");
                /*
                string description = "NONPAGATO - CREDITO";
                string amount = "1000";
                string type = "5"; //Tipo associato al non riscosso
                string subtype = "00"; //Sottotipo associato all type
                PaymentCommand(description, amount, type, subtype);
                */
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)100000, "500NONPAGATO - CREDITO");


                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(true);

                
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }

        //Esempio 4
        //Metodo per generare un esempio di scontrino dove acquisto e utilizzo il buono mono uso
        public int ServizioNonRiscosso(ref string zRep)
        {
            try
            {
                log.Info("Performing ServizioNonRiscosso Method() Esempio 4");

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                object iObj;

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Servizio A", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Servizio B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Servizio C", (decimal)1000000, (int)1000, (int)13, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);

                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)2000000, "0CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)800000, "204BANCOMAT");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)100000, "100ASSEGNI");
                //PaymentCommand("NON RISCOSSO SERVIZI", "1000", "5", "00");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)100000, "502NON RISCOSSO SERVIZI");

                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);

               
                //Gli passo cmq quella che sarà la prossima chiusura
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }

      


        //Esempio 5
        //Metodo per generare un esempio di scontrino misto beni servizi con segue fattura (quindi nn paga niente alla chiusura dello stesso)
        public int SegueFattura(ref string zRep)
        {
            try
            {
                log.Info("Performing SegueFattura Method() Esempio 5");
                
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                object iObj;

                //Fattura che segue a scontrino fiscale
                //Nota:TODO in fase di testing le fatture le troverai dal 25 settembre 2019 in poi,non prima!!! 
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Servizio B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Bene C", (decimal)1000000, (int)1000, (int)16, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);

                //segue fattura = 503


                //PaymentCommand("SEGUIRA FATTURA", "30000", "5", "03");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)3000000, "503SEGUIRA FATTURA");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);

                string myLineNumber = "";
                string myLineText = "";
                for (int i = 1; i < 4; i++) // 20 righe possibili. 
                {
                    myLineNumber = i.ToString("00"); // Deve essere due digit 
                    myLineText = "Riga addizionale " + i;
                    myLineText = myLineText + "                                              ";
                    myLineText = myLineText.Substring(0, 46); // 0,46 in caso dei modelli “Intelligent” 
                    strObj[0] = "01" + "5" + myLineNumber + "0" + "1" + myLineText;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                }


                // Inviare le righe del cliente. 
                string myLineType = "6";
                for (int i = 1; i < 6; i++) // 5 righe possibili (non programmabile). 
                {
                    myLineNumber = i.ToString("00"); // Deve essere due digit 
                    myLineText = "Riga cliente " + i;
                    myLineText = myLineText + "                                              ";
                    myLineText = myLineText.Substring(0, 46); // 0, 46 in caso dei modelli “Intelligent” 
                    strObj[0] = "01" + myLineType + myLineNumber + "0" + "1" + myLineText;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                }

                //Richiedere Fattura a seguito scontrino fiscale / documento commerciale
                strObj[0] = "01" + "00000";
                dirIO = posCommonFP.DirectIO(0, 1052, strObj);

                //fiscalprinter.EndFiscalReceipt(false);
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO

                fiscalprinter.ResetPrinter();

                //Gli passo cmq quella che sarà la prossima chiusura
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    

                }
                else
                {
                    
                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }



        //Esempio 6
        //TODO:061020 La endfiscalreceipt non calcola l'imposta sull omaggio ed è sbagliato ERROR
        //Metodo per generare un esempio di scontrino dove utilizzo l'omaggio 
        public int Omaggio(ref string zRep)
        {
            try
            {
                log.Info("Performing Omaggio Method() Esempio 6");

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                /* NON VALIDO
                //Creo primo uno scontrino rappresentante l'omaggio e poi quello in cui lo utilizzo

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("OMAGGIO", (decimal)1000000, (int)1000, (int)1, (decimal)1000000, "");
                //fiscalprinter.PrintRecSubtotal((decimal)500000);
                fiscalprinter.PrintRecTotal((decimal)1000000, (decimal)1000000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);

                //Ora utilizzo l'omaggio

                */

                fiscalprinter.BeginFiscalReceipt(true);


                fiscalprinter.PrintRecItem("Bene A", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Servizio B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Prodotto C", (decimal)1000000, (int)1000, (int)1, (decimal)1000000, "");
                //PagamentiNegativi("Omaggio Prodotto C", "10000", "01", "01");
                //Test PrintRecItemAdjustment in sostituzione del metodo PagamentiNegativi (DirectIO 1090)
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001Omaggio Prodotto C", 1000000, 1);

                fiscalprinter.PrintRecSubtotal((decimal)0000000);


                fiscalprinter.PrintRecTotal((decimal)0000000, (decimal)0000000, "0CONTANTI");
                
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);
                               
               
                //Gli passo cmq quella che sarà la prossima chiusura
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }

     
      

        //Esempio 7
        //Metodo per generare un esempio di scontrino dove c'è l' arrotondamento (ScontoAPagare)
        public int ScontoAPagare(ref string zRep)
        {
            try
            {

                log.Info("Performing ScontoAPagare Method() Esempio 7");

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                object iObj;

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1606900, (int)1000, (int)1, (decimal)1606900, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Servizio B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Servizio C", (decimal)1000000, (int)1000, (int)1, (decimal)1000000, "");

                fiscalprinter.PrintRecSubtotal((decimal)3000400);



                fiscalprinter.PrintRecTotal((decimal)3000400, (decimal)3000000, "0CONTANTI");
                //PaymentCommand("Sconto A Pagare", "4", "6", "00");
                fiscalprinter.PrintRecTotal((decimal)0000400, (decimal)0000400, "600Sconto A Pagare");
                //fiscalprinter.PrintRecTotal((decimal)0000400, (decimal)0000000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        //Esempio 8
        //Metodo per generare un esempio di scontrino dove arrotonda in automatico per difetto
        public int ArrotondamentoDifetto(ref string zRep)
        {
            try
            {
                log.Info("Performing ArrotondamentoDifetto Method() Esempio8");

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                object iObj;

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1606700, (int)1000, (int)1, (decimal)1606700, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Servizio B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Servizio C", (decimal)1000000, (int)1000, (int)1, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000200);

                fiscalprinter.PrintRecTotal((decimal)00, (decimal)00, "0CONTANTI");

                //fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)100000, "301TICKET");

                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }

        //Esempio 9
        //Metodo per generare un esempio di scontrino dove arrotonda in automatico per eccesso
        public int ArrotondamentoEccesso(ref string zRep)
        {
            try
            {
                log.Info("Performing ArrotondamentoEccesso Method() Esempio 9");

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                object iObj;

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1606900, (int)1000, (int)1, (decimal)1606900, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106000, (int)1);
                fiscalprinter.PrintRecItem("Servizio B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Servizio C", (decimal)1000000, (int)1000, (int)1, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000900);

                fiscalprinter.PrintRecTotal((decimal)00, (decimal)00, "0CONTANTI");

                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);


                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }



        //Esempio 10
        
        //E' un doppio esempio in cui uno ha il simbolo VI e l'altro NO
        
        public int Ventilazione(ref string zRep)
        {
            try
            {
                log.Info("Performing Ventilazione Method() Esempio 10");

                fiscalprinter.BeginFiscalReceipt(true);

                //E' un loop solo per trovare il reparto che ha una aliquota che possiamo utilizzare per la VI
                /*
                for (int i = 1; i <= 99; i++)
                {
                    fiscalprinter.PrintRecItem("Iva Index: " + i.ToString(), (decimal)0000100, (int)1000, (int)i, (decimal)0000100, "");
                }
                */

                fiscalprinter.PrintRecItem("Bene A", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Bene B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Servizio C", (decimal)1000000, (int)1000, (int)1, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);

                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)3000000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1606500, (int)1000, (int)4, (decimal)1606500, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Bene B", (decimal)500000, (int)5000, (int)5, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Bene C", (decimal)1000000, (int)1000, (int)4, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);

                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)3000000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();


            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }



        //Esempio 10 Bis
        //Metodo per generare un esempio di scontrino dove c'è la ventilazione mista all 'iva puntuale

        public int VentilazioneMista(ref string zRep)
        {
            try
            {
                log.Info("Performing VentilazioneMista Method() Esempio 10");

               


                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1606500, (int)1000, (int)4, (decimal)1606500, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Bene B", (decimal)500000, (int)5000, (int)5, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Servizio C", (decimal)1000000, (int)1000, (int)1, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);

                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)3000000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();


            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }



        //Esempio 11
        //Metodo per generare uno scontrino di vendita normale e il suo annullo
        public int AnnulloPrimoScenario(ref string zRep)
        {
            try
            {
                log.Info("Performing AnnulloPrimoScenario Method() Esempio 11");
                
                GeneralCounter gc = new GeneralCounter();


                //Stampo l'esempio dove utilizzo il buono monouso
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                
                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)22000) };

                //Creo primo uno scontrino rappresentante l'acconto e poi quello in cui lo utilizzo

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene \"A\" ", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Servizio \"B\" ", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Servizio \"C\" ", (decimal)1000000, (int)1000, (int)13, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)3000000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.EndFiscalReceipt(false);


                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();
                string data = DateTime.Now.ToString("ddMMyyyy");

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;

                strObj[0] = "2" + printerId + data + gc.FiscalRec.PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0');	// "2" = void

                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet2 = Int32.Parse(iObj[0]);
                if (iRet2 == 0)
                    log.Info("Document Voidable");
                else
                    log.Error("Document NOT Voidable");

                // Void document print

                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0') + " " + gc.FiscalRec.PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();


            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }

        //Esempio 12
        //Metodo per generare uno scontrino di vendita normale e il suo annullo ma qui l' annullo è fatto in una giornata seguente
        public int AnnulloSecondoScenario(ref string zRep)
        {
            try
            {
                log.Info("Performing AnnulloSecondoScenario Method() Esempio 12");
                
                GeneralCounter gc = new GeneralCounter();


                //Stampo l'esempio dove utilizzo il buono monouso
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)22000) };

                //Creo primo uno scontrino rappresentante l'acconto e poi quello in cui lo utilizzo

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene \"A\" ", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Servizio \"B\" ", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Servizio \"C\" ", (decimal)1000000, (int)1000, (int)13, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)3000000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.EndFiscalReceipt(false);

                

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();
                string data = DateTime.Now.ToString("ddMMyyyy");

                //faccio chiusura
                ZReport();

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;

                strObj[0] = "2" + printerId + data + gc.FiscalRec.PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0');	// "2" = void

                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet2 = Int32.Parse(iObj[0]);
                if (iRet2 == 0)
                    log.Info("Document Voidable");
                else
                {
                    log.Error("Document NOT Voidable");
                    throw new PosControlException();
                }

                // Void document print

                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0') + " " + gc.FiscalRec.PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }




        //Esempio 13
        //Metodo per generare uno scontrino di vendita normale e il suo annullo ma qui l' annullo è fatto in una giornata seguente
        public int AnnulloTerzoScenario(ref string zRep)
        {
            try
            {
                log.Info("Performing AnnulloTerzoScenario Method() Esempio 13");
                

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                //Acconto classico , bene ovviamente non consegnato
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Acconto Pro \"A\" ", (decimal)500000, (int)1000, (int)1, (decimal)500000, "");                
                fiscalprinter.PrintRecTotal((decimal)500000, (decimal)500000, "0CONTANTI");               
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.EndFiscalReceipt(false);

                //Consegna del bene
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Prodotto \"A\" ", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                //PagamentiNegativi("Acconto", "5000", "00", "01");   
                //Test PrintRecItemAdjustment in sostituzione del metodo PagamentiNegativi (DirectIO 1090)
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000Acconto", 500000, 1);

                fiscalprinter.PrintRecItem("Prodotto \"B\" ", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Prodotto \"C\"", (decimal)1000000, (int)1000, (int)13, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)2500000);


                fiscalprinter.PrintRecTotal((decimal)2500000, (decimal)2100000, "0CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)2500000, (decimal)400000, "204BANCOMAT");


                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);


                GeneralCounter gc = new GeneralCounter();
                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();
                string data = DateTime.Now.ToString("ddMMyyyy");

                //Annullo in giornata
                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;

                strObj[0] = "2" + printerId + data + gc.FiscalRec.PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0');	// "2" = void

                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet2 = Int32.Parse(iObj[0]);
                if (iRet2 == 0)
                    log.Info("Document Voidable");
                else
                {
                    log.Error("Document NOT Voidable");
                    throw new PosControlException();
                }

                // Void document print

                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0') + " " + gc.FiscalRec.PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();


            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }



        //Esempio 14
        //Metodo per generare un esempio di scontrino dove acquisto e utilizzo il buono mono uso
        public int BuonoMonouso(ref string zRep)
        {
            try
            {
                log.Info("Performing BuonoMonouso Method() Esempio 14");

                //Creo primo uno scontrino rappresentante l'acquisto del buono monouso

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Buono Monouso \"A\" ", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");
                //fiscalprinter.PrintRecSubtotal((decimal)500000);
                fiscalprinter.PrintRecTotal((decimal)1220000, (decimal)1220000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.EndFiscalReceipt(false);


                //Ora stampo l'esempio dove utilizzo il buono monouso
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)12000) };
                /*
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Prodotto A", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");

                PagamentiNegativi("Buono Monouso", "1200", "02", "01");
                //fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "BUONO MONO USO", (decimal)1200000, (int)1);
                fiscalprinter.PrintRecTotal((decimal)00000, (decimal)00000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);
                */
                //ZReport();
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    

                }
                else
                {
                    
                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }

        //Esempio 15
        //Metodo per generare un esempio di scontrino dove utilizzo il buono mono uso
        public int UtilizzoBuonoMonouso(ref string zRep)
        {
            try
            {
                log.Info("Performing UtilizzoBuonoMonouso Method() Esempio 15");

              
                //Ora stampo l'esempio dove utilizzo il buono monouso
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                //VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)12000) };

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Prodotto \"A\" ", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");

                //PagamentiNegativi("Buono Monouso", "12200", "02", "01");
                //Test PrintRecItemAdjustment in sostituzione del metodo PagamentiNegativi (DirectIO 1090)
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "002Buono Monouso", 1220000, 1);
                //fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "BUONO MONO USO", (decimal)1200000, (int)1);
                fiscalprinter.PrintRecTotal((decimal)00000, (decimal)00000, "0Buono Mono Uso");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }





        //Esempio 16
        //Metodo per generare un esempio di scontrino dove vendo un buono multiuso
        public int BuonoMultiuso(ref string zRep)
        {
            try
            {
                log.Info("Performing BuonoMultiuso Method() Esempio 16");

                //Creo primo uno scontrino rappresentante l'acquisto del buono multiuso

                fiscalprinter.BeginFiscalReceipt(true);
                //TODO: al posto di (int)16 ci va l'indice relativo all l'aliquota NS = Non Soggetta
                fiscalprinter.PrintRecItem("Buono Multi \"A\" ", (decimal)1220000, (int)1000, (int)13, (decimal)1220000, "");
                //fiscalprinter.PrintRecSubtotal((decimal)500000);
                fiscalprinter.PrintRecTotal((decimal)1220000, (decimal)1220000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.EndFiscalReceipt(false);


                //Ora stampo l'esempio dove utilizzo il buono monouso
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)22000) };
                /*
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");


                string description = "BUONO MULTIUSO";
                string amount = "12000";
                string type = "6"; //Tipo associato allo Sconto a pagare
                string subtype = "01"; //Sottotipo associato all type
         
                PaymentCommand(description, amount, type, subtype);
              
                //fiscalprinter.PrintRecTotal((decimal)1220000, (decimal)20000, "601 Buono MultiUso"); Non esiste ancora, Molinari work
                PaymentCommand("CASH", "000000200", "0", "00");


                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO


                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                */

                fiscalprinter.ResetPrinter();
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }

        //Esempio 17
        //Metodo per generare un esempio di scontrino dove utilizzo un buono multiuso
        public int BuonoMultiusoUtilizzo(ref string zRep)
        {
            try
            {
                log.Info("Performing BuonoMultiusoUtilizzo Method() Esempio 17");


                //Ora stampo l'esempio dove utilizzo il buono monouso
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)22000) };

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene \"A\" ", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");

                /*
                string description = "BUONO MULTIUSO";
                string amount = "12200";
                string type = "6"; //Tipo associato allo Sconto a pagare
                string subtype = "01"; //Sottotipo associato all type

                PaymentCommand(description, amount, type, subtype);
                */

                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecTotal((decimal)1220000, (decimal)1220000, "601Buono MultiUso"); 
                //PaymentCommand("CASH", "000000200", "0", "00");

                fiscalprinter.EndFiscalReceipt(false);
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO
                /*
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }


                fiscalprinter.ResetPrinter();
                */
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }


        //Esempio 18
        //Metodo per generare un esempio di scontrino dove utilizzo un buono Celiachia
        public int BuonoMultiusoCeliachia(ref string zRep)
        {
            try
            {
                log.Info("Performing BuonoMultiusoCeliachia Method() Esempio 18");
                /*
                //Creo primo uno scontrino rappresentante l'acquisto del buono multiuso

                fiscalprinter.BeginFiscalReceipt(true);
                //TODO: al posto di (int)16 ci va l'indice relativo all l'aliquota NS = Non Soggetta
                fiscalprinter.PrintRecItem("Buono MultiUso Celiachia", (decimal)1220000, (int)1000, (int)16, (decimal)1220000, "");
                //fiscalprinter.PrintRecSubtotal((decimal)500000);
                fiscalprinter.PrintRecTotal((decimal)1220000, (decimal)1220000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);

                */

                //Ora stampo l'esempio dove utilizzo il buono monouso
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)22000) };

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene \"A\" ", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");

                /*
                string description = "Buono Celiachia";
                string amount = "12200";
                string type = "4"; //Tipo associato allo Sconto a pagare
                string subtype = "10"; //Sottotipo associato all type

                PaymentCommand(description, amount, type, subtype);
                */

                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecTotal((decimal)1220000, (decimal)1220000, "401Buono Celiachia");
                fiscalprinter.EndFiscalReceipt(false);
                /*
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                fiscalprinter.ResetPrinter();
                //ZReport();
                */
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }


        //Esempio 20
        //Metodo per generare un esempio di scontrino dove acquisto e utilizzo il buono mono uso
        public int TicketRestaurant(ref string zRep)
        {
            try
            {
                log.Info("Performing TicketRestaurant Method() Esempio 20");

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                object iObj;

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Servizio \"A\"", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Servizio B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Servizio C", (decimal)1000000, (int)1000, (int)13, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);

                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)2100000, "0CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)800000, "204BANCOMAT");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)100000, "401TICKET");

                //PaymentCommand("Ticket", "1000", "4", "01");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.EndFiscalReceipt(false);
                /*
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO


                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di chiudere lo scontrino");
                    throw new Exception();
                }
                fiscalprinter.ResetPrinter();
                //ZReport();
                */
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;
                zRep = (Convert.ToInt32(ZRep) + 1).ToString();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }



        //Esempio 21
        //Metodo per generare un esempio di scontrino dove utilizzo e poi annullo il buono mono uso
        public int AnnulloBuonoMonouso(ref string zRep)
        {
            try
            {
                log.Info("Performing AnnulloBuonoMonouso Method() Esempio 21");
                GeneralCounter gc = new GeneralCounter();


                //Stampo l'esempio dove utilizzo il buono monouso
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)12000) };

                //Acquisto buono monouso
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Buono Monouso Prodotto \"A\" ", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");
                //fiscalprinter.PrintRecSubtotal((decimal)500000);
                fiscalprinter.PrintRecTotal((decimal)1220000, (decimal)1220000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.EndFiscalReceipt(false);



                //Utilizzo Buono Monouso
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Prodotto \"A\" ", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");

                //PagamentiNegativi("Buono Monouso", "12200", "02", "01");
                //Test PrintRecItemAdjustment in sostituzione del metodo PagamentiNegativi (DirectIO 1090)
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "002Buono Monouso", 1220000, 1);

                //fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "BUONO MONO USO", (decimal)1200000, (int)1);
                fiscalprinter.PrintRecTotal((decimal)00000, (decimal)00000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                ZReport();

                string data = DateTime.Now.ToString("ddMMyyyy");



                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;

                strObj[0] = "2" + printerId + data + gc.FiscalRec.PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0');	// "2" = void

                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet2 = Int32.Parse(iObj[0]);
                if (iRet2 == 0)
                    log.Info("Document Voidable");
                else
                {
                    log.Error("Document NOT Voidable");
                    throw new PosControlException();
                }


                // Void document print
                PrintTaxCode("NVRNDR59B10F205D");
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0') + " " + gc.FiscalRec.PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

                zRep = (Convert.ToInt32(ZRep) + 1).ToString();
                //ZReport();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);


                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }

        //Esempio 22
        //Metodo per generare un esempio di scontrino dove faccio un pagamento tramite DCRaSSN (farmacia)
        public int NonRiscossoDaSSN(ref string zRep)
        {
            try
            {
                log.Info("Performing ServizioNonRiscosso Method() Esempio 22");
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                object iObj;

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Bene B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Bene C", (decimal)1000000, (int)1000, (int)13, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);

                //fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)2100000, "0CONTANTI");
                //fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)800000, "201CARTA DI CREDITO");

                //PaymentCommand("NonRiscosso DCRaSSN", "30000", "5", "05");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)3000000, "505NonRiscosso DCRaSSN");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.EndFiscalReceipt(false);
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO


                /*
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                fiscalprinter.ResetPrinter();
                //ZReport();
                */
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

                zRep = (Convert.ToInt32(ZRep) + 1).ToString();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }


        //Esempio 23
        //Metodo per generare un esempio di scontrino dove c'è lotteria e Sconto a pagare
        public int LotteriaScontoAPagare(ref string zRep)
        {
            try
            {
                log.Info("Performing LotteriaScontoAPagare Method() Esempio 23");
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                object iObj;

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Prodotto \"A\"", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");

                strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;


                fiscalprinter.PrintRecSubtotal((decimal)1220000);

                fiscalprinter.PrintRecTotal((decimal)1200000, (decimal)1000000, "0CONTANTI");

                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)220000, "600Sconto a Pagare");
                //PaymentCommand("Sconto a Pagare", "2200", "6", "00");
                fiscalprinter.EndFiscalReceipt(false);
                /*
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO


                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di chiudere lo scontrino");
                    //throw new Exception();
                }
                fiscalprinter.ResetPrinter(); //necessario per il driver windows
                //ZReport();
                */
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

                zRep = (Convert.ToInt32(ZRep) + 1).ToString();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        //Esempio 24
        //Metodo per generare un esempio di scontrino dove pago un acconto e poi lo rendo ma in una chiusura diversa
        public int ResoAccontoServizi(ref string zRep)
        {
            try
            {
                log.Info("Performing ResoAccontoServizi Method() Esempio 24");

                //Stampo l'esempio dove utilizzo il buono Multiuso Celiachia
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                GeneralCounter gc = new GeneralCounter();

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)22000) };

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Servizio \"A\"", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecSubtotal((decimal)1500000);
                string description = "Non riscosso";
                string amount = "10000";
                string type = "5"; //Tipo associato allo Sconto a pagare
                string subtype = "00"; //Sottotipo associato all type

                //TODO: se chiamo prima la 1084 per il non riscosso di 100 euro, devo andare a scalare sti 100 euro dalla printrectotal altrimenti mi da errore 11, dati errati
                //PaymentCommand(description, amount, type, subtype);
                fiscalprinter.PrintRecTotal((decimal)1500000, (decimal)1000000, "502Non Riscosso");
                fiscalprinter.PrintRecTotal((decimal)500000, (decimal)500000, "0CONTANTI");


                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.EndFiscalReceipt(false);
                /*
                //PaymentCommand("CASH", "000000200", "0", "00");
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO

                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                fiscalprinter.ResetPrinter();
                */

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();
                ZReport();


                string data = DateTime.Now.ToString("ddMMyyyy");
                
                
                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;


                // Return document print
                strObj[0] = "0140001REFUND " + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0') + " " + gc.FiscalRec.PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("Servizio \"A\"", (decimal)500000, (int)1);
                fiscalprinter.PrintRecTotal((decimal)1500000, (decimal)00000, "0CONTANTI");
                //fiscalprinter.EndFiscalReceipt(false);
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO

                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                fiscalprinter.ResetPrinter();

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

                zRep = (Convert.ToInt32(ZRep) + 1).ToString();
               
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        //Esempio 24 Bis
        //Metodo per generare un esempio di scontrino dove pago un acconto, lo saldo e poi rendo il saldo
        public int ResoAccontoServiziSecondoScenario(ref string zRep)
        {
            try
            {
                log.Info("Performing ResoAccontoServiziSecondoScenario Method() Esempio 24 Bis");

                //Stampo l'esempio dove utilizzo il buono Multiuso Celiachia
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                GeneralCounter gc = new GeneralCounter();

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)22000) };

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Servizio \"A\"", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecSubtotal((decimal)1500000);
                string description = "Non riscosso";
                string amount = "10000";
                string type = "5"; //Tipo associato allo Sconto a pagare
                string subtype = "00"; //Sottotipo associato all type


                //TODO: se chiamo prima la 1084 per il non riscosso di 100 euro, devo andare a scalare sti 100 euro dalla printrectotal altrimenti mi da errore 11, dati errati

                //PaymentCommand(description, amount, type, subtype);
                fiscalprinter.PrintRecTotal((decimal)1500000, (decimal)1000000, "502Non Riscosso");
                fiscalprinter.PrintRecTotal((decimal)1500000, (decimal)500000, "0CONTANTI");


                PrintTaxCode("NVRNDR59B10F205D");
                //PaymentCommand("CASH", "000000200", "0", "00");
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO

                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                fiscalprinter.ResetPrinter();


                //Saldo
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Saldo Serv. \"A\"", (decimal)1000000, (int)1000, (int)1, (decimal)1000000, "");

                fiscalprinter.PrintRecItem("Servizio \"B\"", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Servizio \"C\"", (decimal)1000000, (int)1000, (int)11, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)2500000);
                fiscalprinter.PrintRecTotal((decimal)2500000, (decimal)2500000, "204BANCOMAT");

                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                fiscalprinter.ResetPrinter();

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                string data = DateTime.Now.ToString("ddMMyyyy");

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;


                // Return document print
                strObj[0] = "0140001REFUND " + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0') + " " + gc.FiscalRec.PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "01")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecRefund("Saldo Serv. \"A\"", (decimal)1000000, (int)1);
                fiscalprinter.PrintRecRefund("Servizio \"B\"", (decimal)500000, (int)3);
                fiscalprinter.PrintRecRefund("Servizio \"C\"", (decimal)1000000, (int)11);

                fiscalprinter.PrintRecSubtotal((decimal)2500000);
                fiscalprinter.PrintRecTotal((decimal)2500000, (decimal)00000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.EndFiscalReceipt(false);
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO
                /*
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                */
                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }



        //Esempio 25
        //Metodo per generare un esempio di scontrino dove utilizzo e poi annullo il buono mono uso
        //NOTA: E' uguale all esempio 21 ma li l'annullo è fatto in una chiusura differente
        public int AnnulloUtilizzoBuonoMonouso(ref string zRep)
        {
            try
            {
                log.Info("Performing AnnulloUtilizzoBuonoMonouso Method() Esempio 25");
                GeneralCounter gc = new GeneralCounter();

                //Stampo l'esempio dove utilizzo il buono monouso
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)12000) };

                //Acquisto buono monouso
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Buono Monouso Prodotto \"A\" ", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");
                //fiscalprinter.PrintRecSubtotal((decimal)500000);
                fiscalprinter.PrintRecTotal((decimal)1220000, (decimal)1220000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);


                //Utilizzo Buono Monouso
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Prodotto \"A\" ", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");

                //PagamentiNegativi("Buono Monouso", "12200", "02", "01");
                //Test PrintRecItemAdjustment in sostituzione del metodo PagamentiNegativi (DirectIO 1090)
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "002Buono Monouso", 1220000, 1);

                //fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "BUONO MONO USO", (decimal)1200000, (int)1);
                fiscalprinter.PrintRecTotal((decimal)00000, (decimal)00000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();


                string data = DateTime.Now.ToString("ddMMyyyy");

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;

                strObj[0] = "2" + printerId + data + gc.FiscalRec.PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0');	// "2" = void

                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet2 = Int32.Parse(iObj[0]);
                if (iRet2 == 0)
                    log.Info("Document Voidable");
                else
                {
                    log.Error("Document NOT Voidable");
                    throw new PosControlException();
                }


                // Void document print
                PrintTaxCode("NVRNDR59B10F205D");
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0') + " " + gc.FiscalRec.PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

                zRep = (Convert.ToInt32(ZRep) + 1).ToString();


            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                   

                }
                else
                {
                   
                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        //Esempio 26
        //Metodo per generare un esempio di scontrino dove annullo il buono DCRaSSN
        public int AnnulloNonRiscossoDaSSN(ref string zRep)
        {
            try
            {
                log.Info("Performing AnnulloNonRiscossoDaSSN Method() Esempio 26");

                GeneralCounter gc = new GeneralCounter();
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Bene B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Bene C", (decimal)1000000, (int)1000, (int)11, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);

                //fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)2100000, "0CONTANTI");
                //fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)800000, "201CARTA DI CREDITO");

                //PaymentCommand("NonRiscosso DCRaSSN", "30000", "5", "05");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)3000000, "505NonRiscosso DCRaSSN");
                fiscalprinter.EndFiscalReceipt(false);
                /*
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO
                PrintTaxCode("NVRNDR59B10F205D");

                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                fiscalprinter.ResetPrinter();
                */

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                ZReport();

                string data = DateTime.Now.ToString("ddMMyyyy");



                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;

                strObj[0] = "2" + printerId + data + gc.FiscalRec.PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0');	// "2" = void

                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet2 = Int32.Parse(iObj[0]);
                if (iRet2 == 0)
                    log.Info("Document Voidable");
                else
                {
                    log.Error("Document NOT Voidable");
                    throw new PosControlException();
                }


                // Void document print
                PrintTaxCode("NVRNDR59B10F205D");
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0') + " " + gc.FiscalRec.PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }



        //Esempio 27
        //Metodo che annulla uno scontrino misto beni servizi con segue fattura (quindi nn paga niente alla chiusura dello stesso)
        public int AnnulloDCSegueFattura(ref string zRep)
        {
            try
            {
                log.Info("Performing AnnulloDCSegueFattura Method() Esempio 27");

                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                GeneralCounter gc = new GeneralCounter();

                //Fattura che segue a scontrino fiscale
                
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Servizio B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Bene C", (decimal)1000000, (int)1000, (int)11, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);

                //segue fattura = 503
                //PaymentCommand("SEGUIRA FATTURA", "30000", "5", "03");
                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)3000000, "503SEGUIRA FATTURA");
                fiscalprinter.EndFiscalReceipt(false);
                /*
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO

                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di chiudere lo scontrino");
                    throw new Exception();
                }
                */
                string myLineNumber = "";
                string myLineText = "";
                for (int i = 1; i < 4; i++) // 20 righe possibili. 
                {
                    myLineNumber = i.ToString("00"); // Deve essere due digit 
                    myLineText = "Riga addizionale " + i;
                    myLineText = myLineText + "                                              ";
                    myLineText = myLineText.Substring(0, 46); // 0,46 in caso dei modelli “Intelligent” 
                    strObj[0] = "01" + "5" + myLineNumber + "0" + "1" + myLineText;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                }


                // Inviare le righe del cliente. 
                string myLineType = "6";
                for (int i = 1; i < 6; i++) // 5 righe possibili (non programmabile). 
                {
                    myLineNumber = i.ToString("00"); // Deve essere due digit 
                    myLineText = "Riga cliente " + i;
                    myLineText = myLineText + "                                              ";
                    myLineText = myLineText.Substring(0, 46); // 0, 46 in caso dei modelli “Intelligent” 
                    strObj[0] = "01" + myLineType + myLineNumber + "0" + "1" + myLineText;
                    dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                }

                //Richiedere Fattura a seguito scontrino fiscale / documento commerciale
                strObj[0] = "01" + "00000";
                dirIO = posCommonFP.DirectIO(0, 1052, strObj);

                fiscalprinter.ResetPrinter();


                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                ZReport();


                string data = DateTime.Now.ToString("ddMMyyyy");

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;

                strObj[0] = "2" + printerId + data + gc.FiscalRec.PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0');	// "2" = void

                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet2 = Int32.Parse(iObj[0]);
                if (iRet2 == 0)
                    log.Info("Document Voidable");
                else
                {
                    log.Error("Document NOT Voidable");
                    throw new PosControlException() ;
                }

                // Void document print

                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0') + " " + gc.FiscalRec.PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

                zRep = (Convert.ToInt32(ZRep) + 1).ToString();


            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);


                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        //Esempio 28
        //Metodo per generare e annullare un acconto servizio dentro la stessa chiusura
        public int AnnulloAccontoServizio1(ref string zRep)
        {
            try
            {
                log.Info("Performing AnnulloAccontoServizio1 Method() Esempio 28");


                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                GeneralCounter gc = new GeneralCounter();

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)22000) };

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Servizio \"A\"", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecSubtotal((decimal)1500000);

                /*
                string description = "Non riscosso";
                string amount = "10000";
                string type = "5"; //Tipo associato allo Sconto a pagare
                string subtype = "00"; //Sottotipo associato all type

                //TODO: se chiamo prima la 1084 per il non riscosso di 100 euro, devo andare a scalare sti 100 euro dalla printrectotal altrimenti mi da errore 11, dati errati
                PaymentCommand(description, amount, type, subtype);
                */
                fiscalprinter.PrintRecTotal((decimal)1500000,(decimal)1000000,"500Non Riscosso");
                fiscalprinter.PrintRecTotal((decimal)1500000, (decimal)500000, "0CONTANTI");
                

                PrintTaxCode("NVRNDR59B10F205D");
                //PaymentCommand("CASH", "000000200", "0", "00");
                fiscalprinter.EndFiscalReceipt(false);

                /*
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO

                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                */
                fiscalprinter.ResetPrinter();


                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();


                string data = DateTime.Now.ToString("ddMMyyyy");

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;


                // Return document print
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0') + " " + gc.FiscalRec.PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }

                fiscalprinter.ResetPrinter();

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }



        //Esempio 29
        //Metodo per generare e annullare un acconto servizio su chiusure diverse
        public int AnnulloAccontoServizio2(ref string zRep)
        {
            try
            {
                log.Info("Performing AnnulloAccontoServizio2 Method() Esempio 29");


                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                GeneralCounter gc = new GeneralCounter();

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)22000) };

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Servizio \"A\"", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecSubtotal((decimal)1500000);
                string description = "Non riscosso";
                string amount = "10000";
                string type = "5"; //Tipo associato allo Sconto a pagare
                string subtype = "00"; //Sottotipo associato all type

                //TODO: se chiamo prima la 1084 per il non riscosso di 100 euro, devo andare a scalare sti 100 euro dalla printrectotal altrimenti mi da errore 11, dati errati
                //PaymentCommand(description, amount, type, subtype);

                fiscalprinter.PrintRecTotal((decimal)1500000, (decimal)1000000, "500Non Riscosso");
                fiscalprinter.PrintRecTotal((decimal)500000, (decimal)500000, "0CONTANTI");


                PrintTaxCode("NVRNDR59B10F205D");
                fiscalprinter.EndFiscalReceipt(false);
                //PaymentCommand("CASH", "000000200", "0", "00");
                /*
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO

                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                */
                fiscalprinter.ResetPrinter();


                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                string data = DateTime.Now.ToString("ddMMyyyy");

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;


                // Return document print
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0') + " " + gc.FiscalRec.PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }

                fiscalprinter.ResetPrinter();

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }



        //Esempio 30
        //Metodo per generare un esempio di scontrino dove utilizzo e poi annullo l'omaggio
        public int AnnulloOmaggio(ref string zRep)
        {
            try
            {
                log.Info("Performing AnnulloOmaggio Method() Esempio 30");
                GeneralCounter gc = new GeneralCounter();


                //Stampo l'esempio dove utilizzo il buono monouso
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)12000) };

                //Acquisto buono monouso
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene \"A\" ", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Servizio \"B\" ", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Prodotto \"C\" ", (decimal)1000000, (int)1000, (int)1, (decimal)1000000, "");
                //PagamentiNegativi("Omaggio \"C\" ", "10000", "01", "01");
                //Test PrintRecItemAdjustment in sostituzione del metodo PagamentiNegativi (DirectIO 1090)
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001Omaggio", 1000000, 1);

                fiscalprinter.PrintRecSubtotal((decimal)2000000);
                fiscalprinter.PrintRecTotal((decimal)2000000, (decimal)2000000, "0CONTANTI");
                PrintTaxCode("NVRNDR59B10F205D");
                //fiscalprinter.PrintRecMessage("Controllo Stato Stampante");
                fiscalprinter.EndFiscalReceipt(false);



                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                ZReport(); //l' annullo è fatto in una chiusura diversa

                string data = DateTime.Now.ToString("ddMMyyyy");

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + "M" + printerIdModel + printerIdNumber;

                strObj[0] = "2" + printerId + data + gc.FiscalRec.PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0');	// "2" = void

                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                int iRet2 = Int32.Parse(iObj[0]);
                if (iRet2 == 0)
                    log.Info("Document Voidable");
                else
                {
                    log.Error("Document NOT Voidable");
                    throw new PosControlException();
                }


                // Void document print
                PrintTaxCode("NVRNDR59B10F205D");
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 1).ToString().PadLeft(4, '0') + " " + gc.FiscalRec.PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;

                string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

                zRep = (Convert.ToInt32(ZRep) + 1).ToString();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);


                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        //Generazione scontrino relativo al nome 
        public int VenditaBeniPagamentoNRBeneNonConsegnato()
        {
            try
            {
                log.Info("Performing VenditaBeniPagamentoNRBeneNonConsegnato Method()");
                //fiscalprinter.PrintZReport();

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)12000) };


                fiscalprinter.BeginFiscalReceipt(true);

                fiscalprinter.PrintRecItem("PRODOTTO A", (decimal)1606500, (int)1000, (int)1, (decimal)1606500, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);

                fiscalprinter.PrintRecItem("PRODOTTO B", (decimal)500000, (int)1000, (int)3, (decimal)500000, "");
                fiscalprinter.PrintRecItem("PRODOTTO C", (decimal)1000000, (int)1000, (int)13, (decimal)1000000, "");
                fiscalprinter.PrintRecSubtotal((decimal)3000000);

                //TODO: 22/04/20 Per ora faccio cosi' , in attesa della implementazione dei comandi nuovi
                //Simulo i 10 euro di non riscosso con uno Sconto sul subtotale, i totalizzatori dovrebbero dare la stessa cosa

                /*strObj[0] = "01" + "SIMULAZ NON RISCOSSO      " + "000001000" + "1" + "01" + "1";
                dirIO = PosCommonFP.DirectIO(0, 1083, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                */

                /*
                //23092020: Ho finalmente il comando per il NON RISCOSSO, DirectIO 1084
                strObj[0] = "01" + "NON RISCOSSO".PadRight(20,' ') + "000001000" + "5" + "00" + "1";
                dirIO = PosCommonFP.DirectIO(0, 1084, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;
                */

                //DirectIO implementata in un metodo ad hoc , PaymentCommand
                string description = "NON RISCOSSO";
                string amount = "1000";
                string type = "5"; //Tipo associato al non riscosso
                string subtype = "00"; //Sottotipo associato all type
                PaymentCommand(description, amount, type, subtype);

                //fiscalprinter.PrintRecNotPaid("NOT PAID", (decimal)100000);
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)2000000, "0CONTANTI");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)800000, "201CARTA DI CREDITO");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)100000, "200 CREDITO"); //011020 CI HO AGGIONTO STA CAZZATA DEI 10 EURO DI CREDITO, **************************
                //fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)100000, "301TICKET");
                //fiscalprinter.PrintRecNotPaid("NOT PAID", (decimal)100000);
                //fiscalprinter.PrintRecMessage("RecMessage");

                //fiscalprinter.PrintRecSubtotalAdjustment(FiscalAdjustment.AmountDiscount, "SUBT DISC", (decimal)10000);
                //fiscalprinter.EndFiscalReceipt(false);
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO


                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di chiudere lo scontrino");
                    throw new Exception();
                }
                fiscalprinter.ResetPrinter();
                //ZReport();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        //Metodo per generare un esempio di scontrino dove acquisto e utilizzo il buono multiuso e poi lo annullo
        public int AnnulloBuonoMultiuso()
        {
            try
            {
                log.Info("Performing AnnulloBuonoMultiuso Method()");

                //Stampo l'esempio dove utilizzo il buono Multiuso Celiachia
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                GeneralCounter gc = new GeneralCounter();

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)22000) };

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1220000, (int)1000, (int)1, (decimal)1220000, "");


                string description = "BUONO MULTIUSO";
                string amount = "12000";
                string type = "6"; //Tipo associato allo Sconto a pagare
                string subtype = "01"; //Sottotipo associato all type

                PaymentCommand(description, amount, type, subtype);

                //fiscalprinter.PrintRecTotal((decimal)1220000, (decimal)20000, "601 Buono MultiUso"); Non esiste ancora, Molinari work
                PaymentCommand("CASH", "000000200", "0", "00");


                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO


                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                fiscalprinter.ResetPrinter();

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();
                string data = DateTime.Now.ToString("ddMMyyyy");
                //Mi leggo l'ultimo NumExpceptions perché voglio accertarmi che non ci sia un incremento degli errori con questo specifico test
                int lastErrorNumers = NumExceptions;

                int output = TestVoidableRefundableReceipts((Convert.ToInt32(gc.ZRep) + 1).ToString(), gc.FiscalRec, gc.FiscalRec, data);

                if (output - lastErrorNumers > 0)
                {
                    log.Error("Test AnnulloBuonoMultiusoCeliachia Failed");
                    throw new PosControlException();
                }


            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }



       


        //Metodo per generare un esempio di scontrino dove acquisto e utilizzo il buono multiuso
        public int AnnulloBuonoMultiusoCeliachia()
        {
            try
            {
                log.Info("Performing AnnulloBuonoMultiusoCeliachia Method()");
                
                //Stampo l'esempio dove utilizzo il buono Multiuso Celiachia
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                GeneralCounter gc = new GeneralCounter();

                VatInfo[] vat = new VatInfo[1] { new VatInfo(1, (decimal)22000) };

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1200000, (int)1000, (int)1, (decimal)1200000, "");


                string description = "BUONO CELIACHIA";
                string amount = "12000";
                string type = "6"; //Tipo associato allo Sconto a pagare
                string subtype = "01"; //Sottotipo associato all type

                PaymentCommand(description, amount, type, subtype);

                //fiscalprinter.PrintRecTotal((decimal)1220000, (decimal)20000, "601 Buono MultiUso"); Non esiste ancora, Molinari work



                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO


                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di utilizzare il comando PaymentCommand");
                    throw new Exception();
                }
                fiscalprinter.ResetPrinter();

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();
                string data = DateTime.Now.ToString("ddMMyyyy");
                //Mi leggo l'ultimo NumExpceptions perché voglio accertarmi che non ci sia un incremento degli errori con questo specifico test
                int lastErrorNumers = NumExceptions;

                int output = TestVoidableRefundableReceipts((Convert.ToInt32(gc.ZRep) + 1).ToString(), gc.FiscalRec, gc.FiscalRec, data);

                if (output - lastErrorNumers > 0)
                {
                    log.Error("Test AnnulloBuonoMultiusoCeliachia Failed");
                    throw new PosControlException();
                }


            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }



        //DirectIO 1090 , acconto, buono monouso, omaggio, pagamenti negativi quando si utilizzano o saldano
        public int PagamentiNegativi(string Descr, string Amount, string Type , string Rep)
        {
            try
            {
                log.Info("Performing PagamentiNegativi Method()");

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string descr = Descr.PadRight(20, ' ');
                string amount = Amount.PadLeft(9, '0');
                string type = Type.PadLeft(2, '0'); //00 acconto , 01 OMAGGIO, 02 Buono Monouso
                string dep = Rep.PadLeft(2, '0');

                strObj[0] = "01" + descr + amount + type + dep + "1";

                dirIO = posCommonFP.DirectIO(0, 1090, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                if (iData != 1090)
                {
                    log.Error("Error DirecIO 1090 , tentativo fallito di utilizzare il comando PagamentiNegativi");
                    throw new Exception();
                }
                if (iData != 1090)
                {
                    log.Error("Error DirecIO 1090 , tentativo fallito di utilizzare il comando PagamentiNegativi");
                    throw new Exception();
                }

                //fiscalprinter.ResetPrinter();
                //ZReport();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                   
                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        //Metodo per generare un esempio di scontrino dove viene applicato l'arrotondamento per difetto (ScontoAPagare)
        public int ScontoAPagareDifetto(ref GeneralCounter gc, ref GeneralCounter gc2)
        {
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                object iObj;

                log.Info("Performing ScontoAPagareDifetto Method()");

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();


                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1606900, (int)1000, (int)1, (decimal)1606900, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106500, (int)1);
                fiscalprinter.PrintRecItem("Bene B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Servizio C", (decimal)1000000, (int)1000, (int)16, (decimal)1000000, "");
               

                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)3000000, "0CONTANTI");
                PaymentCommand("Sconto A Pagare", "4", "6", "00");

                //fiscalprinter.EndFiscalReceipt(false);
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO


                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di chiudere lo scontrino");
                    throw new Exception();
                }

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();
                fiscalprinter.ResetPrinter();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    

                }
                else
                {
                   
                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }

        //Metodo per generare un esempio di scontrino dove viene applicato l'arrotondamento per eccesso (ScontoAPagare)
        public int ScontoAPagareEccesso(ref GeneralCounter gc, ref GeneralCounter gc2)
        {
            try
            {
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                object iObj;

                log.Info("Performing ScontoAPagareEccesso Method()");

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene A", (decimal)1606900, (int)1000, (int)1, (decimal)1606900, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "Sconto", (decimal)106000, (int)1);
                fiscalprinter.PrintRecItem("Bene B", (decimal)500000, (int)5000, (int)3, (decimal)100000, "");
                fiscalprinter.PrintRecItem("Servizio C", (decimal)1000000, (int)1000, (int)16, (decimal)1000000, "");


                fiscalprinter.PrintRecTotal((decimal)3001000, (decimal)3001000, "0CONTANTI");
                //TODO Controllare che questo comando non vada realmente inviato e che la printer stampi in automatico la voce aggiuntiva "Sconto a Pagare" come nell esempio del Difetto
                //fiscalprinter.PrintRecTotal((decimal)200, (decimal)200, "Sconto A PAGARE");

                //fiscalprinter.EndFiscalReceipt(false);
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO


                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di chiudere lo scontrino");
                    throw new Exception();
                }
                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc2 = GeneralCounter.GetGeneralCounter();

                fiscalprinter.ResetPrinter();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }




        //DirectIO 4037 , Set ATECO Code, Index 01, 02, 03
        //TODO: atecode numerico 
        public int SetATECOCode(string AtecoIndex, string AtecoCode, string Ventilazione)
        {
            try
            {

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                //devo fare una chiusura prima per poter programmare l'ateco code
                fiscalprinter.PrintZReport();

                string atecoindex = AtecoIndex.PadLeft(2, '0'); 
                string atecocode = AtecoCode;
                string ventilazione = Ventilazione;
                string spare = "".PadLeft(10, ' ');

                strObj[0] = atecoindex + atecocode + ventilazione + "0" + spare;

                dirIO = posCommonFP.DirectIO(0, 4037, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                if (iData != 4037)
                {
                    log.Error("Error DirecIO 4037 , tentativo fallito di programmare l'Ateco Code");
                    throw new Exception();
                }

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                   
                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }



        //DirectIO 4237 , Read ATECO Code, Index 01, 02, 03
        //TODO: al momento funziona ma è formattato male, Index deve essere di due byte, non uno
        public int ReadATECOCode(string AtecoIndex)
        {
            try
            {
                log.Info("Performing ReadATECOCode Method()");

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string atecoindex = AtecoIndex.PadLeft(2,'0');
                
                strObj[0] = atecoindex;

                dirIO = posCommonFP.DirectIO(0, 4237, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                if (iData != 4237)
                {
                    log.Error("Error DirecIO 4237 , tentativo fallito di leggere l'Ateco Code");
                    //throw new Exception();
                }
                if(String.Compare(iObj[0].Substring(0,1), atecoindex) != 0)
                {
                    log.Error("Error DirecIO 4237 , index Code restituito è differente da quello richiesto");
                    
                }
                string AtecoCode = iObj[0].Substring(1, 6);
                string SPARE = iObj[0].Substring(7, 10);
                string Ventilazione = iObj[0].Substring(17, 1);
                string PrintVI = iObj[0].Substring(18, 1);


            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }


        
        //Metodo che chiama la DirectIO 4015 index 27  per settare il rounding mode (anche se nn credo sia fattibile)
        //kindofrounding: 0 = Full Rounding (default) 1 = Rounding disable 2 = only rounding negative 3 = only rounding positive
        public int SetRounding(string kindofrounding)
        {
            try
            {
                log.Info("Performing SetRounding Method()");

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                string Kindofrounding = kindofrounding;

                strObj[0] = "27" + Kindofrounding.PadLeft(3, '0'); 
                dirIO = posCommonFP.DirectIO(0, 4015, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                if (iData != 4015)
                {
                    log.Error("Error DirecIO 4015 , tentativo fallito di programmare il rounding ");
                    throw new Exception();
                }

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }



        //Metodo che chiama la DirectIO 4215 index 27  per leggere il rounding mode (anche se nn credo sia fattibile)
        //kindofrounding: 0 = Full Rounding (default) 1 = Rounding disable 2 = only rounding negative 3 = only rounding positive
        public int ReadRounding()
        {
            try
            {
                log.Info("Performing ReadRounding Method()");

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
   

                strObj[0] = "27";
                dirIO = posCommonFP.DirectIO(0, 4215, strObj);
                iData = dirIO.Data;
                iObj = (string[])dirIO.Object;

                if (iData != 4215)
                {
                    log.Error("Error DirecIO 4215 , tentativo fallito di programmare il rounding ");
                    throw new Exception();
                }
                string Kindofrounding = iObj[0].Substring(3,2);
                if (Int32.Parse(Kindofrounding) > 3 && (Int32.Parse(Kindofrounding) < 0 ) )
                {
                    log.Error("Error DirecIO 4215 , letto un tipo di rounding che è fuori dal range di programmazione ");
                   
                }

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }

        public int testForMatteoNFP()
        {
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                Lottery lt;
                GeneralCounter gc = new GeneralCounter();
                log.Info("Performing testForMatteoNFP Method()");

                //Aggiorniamo i contatori generali su xml
                //GeneralCounter.SetGeneralCounter();

                //Load general counter
                //gc = GeneralCounter.GetGeneralCounter();




                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("TEST MATTEO1");
                fiscalprinter.PrintRecItem("Test", (decimal)19900, (int)1000, (int)3, 0, "kaste");
                fiscalprinter.PrintRecSubtotal((decimal)19900);
                fiscalprinter.PrintRecTotal(19900, 10000, "0CONTANTE");
                fiscalprinter.PrintRecTotal(19900, 10000, "600Sconto a pagare");
                fiscalprinter.EndFiscalReceipt(false);



                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("TEST MATTEO2");
                fiscalprinter.PrintRecItem("Test", (decimal)19900, (int)1000, (int)3, 0, "kaste");
                fiscalprinter.PrintRecSubtotal((decimal)19900);
                fiscalprinter.PrintRecTotal(19900, 10000, "600Sconto a pagare");
                fiscalprinter.PrintRecTotal(19900, 10000, "0CONTANTE"); 
                fiscalprinter.EndFiscalReceipt(false);


                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("TEST CONTROLLO LIMITE SCONTO A PAGARE");
                fiscalprinter.PrintRecItem("Test", (decimal)19900, (int)1000, (int)3, 0, "kaste");
                fiscalprinter.PrintRecSubtotal((decimal)19900);
                fiscalprinter.PrintRecTotal(19900, 10000, "0CONTANTE");
                fiscalprinter.PrintRecTotal(19900, 50000, "600Sconto a pagare");
                fiscalprinter.EndFiscalReceipt(false);

                //ZReport();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
                return NumExceptions;
        }


        //Metodo creato per generare gli scontrini da inviare al CNR
        public int testCNR()
        {
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;
                
                Lottery lt;
                GeneralCounter gc = new GeneralCounter();
                log.Info("Performing testCNR Method()");

                //Aggiorniamo i contatori generali su xml
                //GeneralCounter.SetGeneralCounter();

                //Load general counter
                //gc = GeneralCounter.GetGeneralCounter();

                lt = new Lottery();
                //chiamo il metodo SendLotteryCodePagMisto
                int output = lt.SetConfiguration("27", "1".PadLeft(3, '0'));




                /*
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 5");
                fiscalprinter.PrintRecItem("BENE ", (decimal)288900, (int)1000, (int)7, (decimal)288900, "");
                fiscalprinter.PrintRecItem("BENE", (decimal)888800, (int)1000, (int)9, (decimal)888800, "");
                fiscalprinter.PrintRecItem("BENE", (decimal)200000, (int)1000, (int)1, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 1);


                fiscalprinter.PrintRecTotal((decimal)1177700, (decimal)1177700, "505NonRiscosso DCRaSSN");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                string model = "M";
                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                string data = DateTime.Now.ToString("ddMMyyyy");

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + model + printerIdModel + printerIdNumber;

                int app = Convert.ToInt32(gc.FiscalRec) + 1;
                // Annullo Esempio 1
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    //log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    //throw new Exception();
                }



                ZReport();

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 5bis");
                fiscalprinter.PrintRecItem("BENE ", (decimal)288900, (int)1000, (int)7, (decimal)288900, "");
                fiscalprinter.PrintRecItem("BENE", (decimal)888800, (int)1000, (int)9, (decimal)888800, "");
                fiscalprinter.PrintRecItem("BENE 22", (decimal)200000, (int)1000, (int)1, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 1);
                fiscalprinter.PrintRecItem("SERVIZIO 22", (decimal)200000, (int)1000, (int)10, (decimal)200000, "");

                fiscalprinter.PrintRecTotal((decimal)1177700, (decimal)1177700, "505NonRiscosso DCRaSSN");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport();

                */

                
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 1");
                fiscalprinter.PrintRecItem("BENE 22", (decimal)1500000, (int)1000, (int)1, (decimal)1500000, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000     ", 500000, 1);
                fiscalprinter.PrintRecItem("BENE 4", (decimal)240500, (int)1000, (int)3, (decimal)240500, "");
                fiscalprinter.PrintRecItem("Servizio 10", (decimal)1220100, (int)1000, (int)11, (decimal)1220100, "");
                fiscalprinter.PrintRecItem("ES Servizio ES", (decimal)200000, (int)1000, (int)13, (decimal)200000, "");


                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)70000, "401TICKET");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)70000, "100ASSEGNO");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)1200000, "201CARTA DI CREDITO");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)220000, "502NON RISCOSSO SERVIZI");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);
                //TODO Controllare che questo comando non vada realmente inviato e che la printer stampi in automatico la voce aggiuntiva "Sconto a Pagare" come nell esempio del Difetto
                //fiscalprinter.PrintRecTotal((decimal)200, (decimal)200, "Sconto A PAGARE");

                

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 2");
                fiscalprinter.PrintRecItem("BENE 10", (decimal)1332200, (int)1000, (int)2, (decimal)1332200, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000     ", 200000, 2);
                fiscalprinter.PrintRecItem("BENE 4", (decimal)330000, (int)1000, (int)3, (decimal)330000, "");           
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 330000, 3);
                fiscalprinter.PrintRecItem("Servizio 22", (decimal)378900, (int)1000, (int)10, (decimal)378900, "");
                fiscalprinter.PrintRecItem("BENE ES", (decimal)245600, (int)1000, (int)4, (decimal)245600, "");
                
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)20000, "401TICKET");
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)200000, "502NON RISCOSSO SERVIZI");
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)100000, "601BUONO MULTIUSO  "); 
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);

                

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 3");
                fiscalprinter.PrintRecItem("BENE ES", (decimal)350000, (int)1000, (int)4, (decimal)350000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000     ", 200000, 4);
                fiscalprinter.PrintRecItem("BENE 4", (decimal)560000, (int)1000, (int)3, (decimal)560000, "");
                fiscalprinter.PrintRecItem("SERVIZIO EE", (decimal)665400, (int)1000, (int)14, (decimal)665400, "");
                fiscalprinter.PrintRecItem("SERVIZIO 4", (decimal)125600, (int)1000, (int)12, (decimal)125600, "");
                fiscalprinter.PrintRecTotal((decimal)1701000, (decimal)1701000, "503SEGUIRA FATTURA");
                fiscalprinter.EndFiscalReceipt(false);

                
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 4");
                fiscalprinter.PrintRecItem("BENE RM", (decimal)281700, (int)1000, (int)8, (decimal)281700, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "002", 100000, 8);
                fiscalprinter.PrintRecItem("BENE 4", (decimal)388900, (int)1000, (int)3, (decimal)388900, "");
                fiscalprinter.PrintRecItem("SERVIZIO  22", (decimal)670000, (int)1000, (int)10, (decimal)670000, "");
                fiscalprinter.PrintRecItem("SERVIZIO 4", (decimal)130000, (int)1000, (int)12, (decimal)130000, "");

                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)100000, "401Buono Celiachia");
                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)150000, "601BUONO MULTIUSO  ");
                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)70000, "401TICKET");
                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)1200000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);

                

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 5");
                fiscalprinter.PrintRecItem("BENE ", (decimal)288900, (int)1000, (int)7, (decimal)288900, "");               
                fiscalprinter.PrintRecItem("BENE", (decimal)888800, (int)1000, (int)9, (decimal)888800, "");
                fiscalprinter.PrintRecItem("BENE", (decimal)200000, (int)1000, (int)1, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 1);


                fiscalprinter.PrintRecTotal((decimal)1177700, (decimal)1177700, "505NonRiscosso DCRaSSN");
                fiscalprinter.EndFiscalReceipt(false);

               
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 6");
                fiscalprinter.PrintRecItem("BENE ", (decimal)380000, (int)1000, (int)3, (decimal)380000, "");
                fiscalprinter.PrintRecItem("BENE", (decimal)133300, (int)1000, (int)1, (decimal)133300, "");
                fiscalprinter.PrintRecItem("SERVIZI", (decimal)200000, (int)1000, (int)10, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 10);


                fiscalprinter.PrintRecTotal((decimal)513300, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);

                
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 7");
                fiscalprinter.PrintRecItem("BENE ", (decimal)264700, (int)1000, (int)3, (decimal)264700, "");
                fiscalprinter.PrintRecItem("BENE ", (decimal)200000, (int)1000, (int)1, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 1);

                fiscalprinter.PrintRecTotal((decimal)264700, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


               
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 8");
                fiscalprinter.PrintRecItem("BENE ", (decimal)334600, (int)1000, (int)3, (decimal)334600, "");
                fiscalprinter.PrintRecItem("BENE ", (decimal)200000, (int)1000, (int)3, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 3);

                fiscalprinter.PrintRecTotal((decimal)334600, (decimal)34500, "600Sconto A Pagare");
                fiscalprinter.PrintRecTotal((decimal)334600, (decimal)310000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo
                
                string model = "I";
                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                string data = DateTime.Now.ToString("ddMMyyyy");

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + model + printerIdModel + printerIdNumber;

                int app = Convert.ToInt32(gc.FiscalRec) + 1;
                
                // Annullo Esempio 1
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    //log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    //throw new Exception();
                }


                app++;
                // Annullo Esempio 2
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }


                app++;
                // Annullo Esempio 3
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep)).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }

                app++;
                // Annullo Esempio 4
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) ).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }

                app++;
                // Annullo Esempio 5
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) ).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }


                app++;
                // Annullo Esempio 6
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) ).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }


                app++;
                // Annullo Esempio 7
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) ).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }

                app++;
                // Annullo Esempio 8
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) ).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    throw new Exception();
                }

                ZReport();

            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }




        //Metodo creato per generare gli scontrini da inviare al CNR
        public int testCNRAnnullo()
        {
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                Lottery lt;
                GeneralCounter gc = new GeneralCounter();
                log.Info("Performing testCNRAnnullo Method()");

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                lt = new Lottery();
                
                int output = lt.SetConfiguration("27", "1".PadLeft(3, '0'));




                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 1");
                fiscalprinter.PrintRecItem("BENE ", (decimal)1500000, (int)1000, (int)1, (decimal)1500000, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000     ", 500000, 1);
                fiscalprinter.PrintRecItem("BENE ", (decimal)240500, (int)1000, (int)3, (decimal)240500, "");
                fiscalprinter.PrintRecItem("Servizio ", (decimal)1220100, (int)1000, (int)11, (decimal)1220100, "");
                fiscalprinter.PrintRecItem("ES Servizio ", (decimal)200000, (int)1000, (int)13, (decimal)200000, "");




                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)70000, "401TICKET");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)70000, "100ASSEGNO");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)1200000, "201CARTA DI CREDITO");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)220000, "502NON RISCOSSO SERVIZI");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                string model = "I";
                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                string data = DateTime.Now.ToString("ddMMyyyy");

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + model + printerIdModel + printerIdNumber;

                int app = Convert.ToInt32(gc.FiscalRec) + 1;


                app = 1;
                // Annullo Esempio 1
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    //throw new Exception();
                }


                ZReport();
                /*
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 5bis");
                fiscalprinter.PrintRecItem("BENE ", (decimal)288900, (int)1000, (int)7, (decimal)288900, "");
                fiscalprinter.PrintRecItem("BENE", (decimal)888800, (int)1000, (int)9, (decimal)888800, "");
                fiscalprinter.PrintRecItem("BENE 22", (decimal)200000, (int)1000, (int)1, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 1);
                fiscalprinter.PrintRecItem("SERVIZIO 22", (decimal)200000, (int)1000, (int)10, (decimal)200000, "");

                fiscalprinter.PrintRecTotal((decimal)1177700, (decimal)1177700, "505NonRiscosso DCRaSSN");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport();

                */




                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 2");
                fiscalprinter.PrintRecItem("BENE ", (decimal)1332200, (int)1000, (int)2, (decimal)1332200, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000     ", 200000, 2);
                fiscalprinter.PrintRecItem("BENE", (decimal)330000, (int)1000, (int)3, (decimal)330000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 330000, 3);
                fiscalprinter.PrintRecItem("Servizio ", (decimal)378900, (int)1000, (int)10, (decimal)378900, "");
                fiscalprinter.PrintRecItem("BENE ", (decimal)245600, (int)1000, (int)4, (decimal)245600, "");

                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)20000, "401TICKET");
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)200000, "502NON RISCOSSO SERVIZI");
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)100000, "601BUONO MULTIUSO  ");
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app =  1;
                // Annullo Esempio 1
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    //throw new Exception();
                }



                ZReport();











                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 3");
                fiscalprinter.PrintRecItem("BENE ", (decimal)350000, (int)1000, (int)4, (decimal)350000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000     ", 200000, 4);
                fiscalprinter.PrintRecItem("BENE ", (decimal)560000, (int)1000, (int)3, (decimal)560000, "");
                fiscalprinter.PrintRecItem("SERVIZIO ", (decimal)665400, (int)1000, (int)14, (decimal)665400, "");
                fiscalprinter.PrintRecItem("SERVIZIO ", (decimal)125600, (int)1000, (int)12, (decimal)125600, "");
                fiscalprinter.PrintRecTotal((decimal)1701000, (decimal)1701000, "503SEGUIRA FATTURA");
                fiscalprinter.EndFiscalReceipt(false);

                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app = 1;
                // Annullo Esempio 1
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    //throw new Exception();
                }



                ZReport();




                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 4");
                fiscalprinter.PrintRecItem("BENE ", (decimal)281700, (int)1000, (int)8, (decimal)281700, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "002", 100000, 8);
                fiscalprinter.PrintRecItem("BENE", (decimal)388900, (int)1000, (int)3, (decimal)388900, "");
                fiscalprinter.PrintRecItem("SERVIZIO ", (decimal)670000, (int)1000, (int)10, (decimal)670000, "");
                fiscalprinter.PrintRecItem("SERVIZIO ", (decimal)130000, (int)1000, (int)12, (decimal)130000, "");

                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)100000, "401Buono Celiachia");
                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)150000, "601BUONO MULTIUSO  ");
                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)70000, "401TICKET");
                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)1200000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app = 1;
                // Annullo Esempio 1
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    //throw new Exception();
                }



                ZReport();





                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 5");
                fiscalprinter.PrintRecItem("BENE ", (decimal)288900, (int)1000, (int)7, (decimal)288900, "");
                fiscalprinter.PrintRecItem("BENE", (decimal)888800, (int)1000, (int)9, (decimal)888800, "");
                fiscalprinter.PrintRecItem("BENE", (decimal)200000, (int)1000, (int)1, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 1);


                fiscalprinter.PrintRecTotal((decimal)1177700, (decimal)1177700, "505NonRiscosso DCRaSSN");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app = 1;
                // Annullo Esempio 1
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    //throw new Exception();
                }



                ZReport();

             


                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 6");
                fiscalprinter.PrintRecItem("BENE ", (decimal)380000, (int)1000, (int)3, (decimal)380000, "");
                fiscalprinter.PrintRecItem("BENE", (decimal)133300, (int)1000, (int)1, (decimal)133300, "");
                fiscalprinter.PrintRecItem("SERVIZI", (decimal)200000, (int)1000, (int)10, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 10);


                fiscalprinter.PrintRecTotal((decimal)513300, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();


                app = 1;
                // Annullo Esempio 1
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    //throw new Exception();
                }



                ZReport();




                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 7");
                fiscalprinter.PrintRecItem("BENE ", (decimal)264700, (int)1000, (int)3, (decimal)264700, "");
                fiscalprinter.PrintRecItem("BENE ", (decimal)200000, (int)1000, (int)1, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 1);

                fiscalprinter.PrintRecTotal((decimal)264700, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app = 1;
                // Annullo Esempio 1
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    //throw new Exception();
                }



                ZReport();




                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 8");
                fiscalprinter.PrintRecItem("BENE ", (decimal)334600, (int)1000, (int)3, (decimal)334600, "");
                fiscalprinter.PrintRecItem("BENE ", (decimal)200000, (int)1000, (int)3, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 3);

                fiscalprinter.PrintRecTotal((decimal)334600, (decimal)34500, "600Sconto A Pagare");
                fiscalprinter.PrintRecTotal((decimal)334600, (decimal)310000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);



                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app = 1;
                // Annullo Esempio 1
                strObj[0] = "0140001VOID " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                if (iData != 1078)
                {
                    log.Error("Error DirectIO 1078 , expected iData 1078, received " + iData);
                    throw new Exception();
                }
                iObj = (string[])dirIO.Object;
                if (!(String.Equals(iObj[0], "51")))
                {
                    log.Error("Error DirectIO 1078 operator, expected 01, received " + iObj[0]);
                    //throw new Exception();
                }



                ZReport();





            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }





        //Metodo creato per generare gli scontrini da inviare al CNR con i RESI
        public int testCNRResi()
        {
            try
            {
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;
                int iData;

                Lottery lt;
                GeneralCounter gc = new GeneralCounter();
                log.Info("Performing testCNRResi Method()");

                string model = "I";
                string data = DateTime.Now.ToString("ddMMyyyy");

                string strData = fiscalprinter.GetData(FiscalData.PrinterId, (int)0).Data;
                //Console.WriteLine("Returned printerId: " + strData);
                string printerIdModel = strData.Substring(0, 2);
                string printerIdNumber = strData.Substring(4, 6);
                string printerIdManufacturer = strData.Substring(2, 2);
                string printerId = printerIdManufacturer + model + printerIdModel + printerIdNumber;
                int app, iRet;

                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                lt = new Lottery();

                //Abilito arrotondamento
                int output = lt.SetConfiguration("27", "1".PadLeft(3, '0'));
                
                
            
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 1");
                fiscalprinter.PrintRecItem("BENE 22", (decimal)1500000, (int)1000, (int)1, (decimal)1500000, "");

                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000ACCONTO", 500000, 1);
                fiscalprinter.PrintRecItem("BENE 4", (decimal)240500, (int)1000, (int)3, (decimal)240500, "");
                fiscalprinter.PrintRecItem("Servizio 10", (decimal)1220100, (int)1000, (int)11, (decimal)1220100, "");
                fiscalprinter.PrintRecItem("ES Servizio ", (decimal)200000, (int)1000, (int)13, (decimal)200000, "");


                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)70000, "401TICKET");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)70000, "100ASSEGNO");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)1200000, "201CARTA DI CREDITO");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)220000, "502NON RISCOSSO SERVIZI");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa il reso

               
                //Aggiorniamo i contatori generali su xml
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

               


                app = 1;
                // Check document is refundable
                //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                strObj[0] = "1" + printerId + data + app.ToString().PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0');   // "1" = refund
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                iRet = Int32.Parse(iObj[0]);
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    log.Error("Document can NOT be Refunded");
                }


                // Return document print
                //Console.WriteLine("DirectIO (Return document print)");
                strObj[0] = "0140001REFUND " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

             
                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 1");
                fiscalprinter.PrintRecRefund("BENE 22", (decimal)1500000, (int)1);
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000ACCONTO", 500000, 1);
                fiscalprinter.PrintRecRefund("BENE 4", (decimal)240500, (int)3);
                fiscalprinter.PrintRecRefund("Servizio 10", (decimal)1220100, (int)11);
                fiscalprinter.PrintRecRefund("ES Servizio ", (decimal)200000, (int)13);

                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)70000, "401TICKET");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)70000, "100ASSEGNO");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)1200000, "201CARTA DI CREDITO");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)220000, "502NON RISCOSSO SERVIZI");
                fiscalprinter.PrintRecTotal((decimal)3160600, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport();









                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 2");
                fiscalprinter.PrintRecItem("BENE 10", (decimal)1332200, (int)1000, (int)2, (decimal)1332200, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000ACCONTO ", 200000, 2);
                fiscalprinter.PrintRecItem("BENE 4", (decimal)330000, (int)1000, (int)3, (decimal)330000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001OMAGGIO", 330000, 3);
                fiscalprinter.PrintRecItem("Servizio 22", (decimal)378900, (int)1000, (int)10, (decimal)378900, "");
                fiscalprinter.PrintRecItem("BENE VI", (decimal)245600, (int)1000, (int)4, (decimal)245600, "");

                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)20000, "401TICKET");
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)200000, "502NON RISCOSSO SERVIZI");
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)100000, "601BUONO MULTIUSO  ");
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app = 1;

                // Check document is refundable
                //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                strObj[0] = "1" + printerId + data + app.ToString().PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0');   // "1" = refund
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                iRet = Int32.Parse(iObj[0]);
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    log.Error("Document can NOT be Refunded");
                }
                // Return document print
                //Console.WriteLine("DirectIO (Return document print)");
                strObj[0] = "0140001REFUND " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
               

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 2");
                fiscalprinter.PrintRecRefund("BENE 10", (decimal)1332200, (int)2);
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000ACCONTO ", 200000, 2);
                fiscalprinter.PrintRecRefund("BENE 4", (decimal)330000, (int)3);
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001OMAGGIO", 330000, 3);
                fiscalprinter.PrintRecRefund("Servizio 22", (decimal)378900, (int)10);
                fiscalprinter.PrintRecRefund("BENE VI", (decimal)245600, (int)4);

                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)20000, "401TICKET");
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)200000, "502NON RISCOSSO SERVIZI");
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)100000, "601BUONO MULTIUSO  ");
                fiscalprinter.PrintRecTotal((decimal)2156700, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport();











                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 3");
                fiscalprinter.PrintRecItem("BENE ES", (decimal)350000, (int)1000, (int)4, (decimal)350000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000ACCONTO     ", 200000, 4);
                fiscalprinter.PrintRecItem("BENE 4", (decimal)560000, (int)1000, (int)3, (decimal)560000, "");
                fiscalprinter.PrintRecItem("SERVIZIO EE", (decimal)665400, (int)1000, (int)14, (decimal)665400, "");
                fiscalprinter.PrintRecItem("SERVIZIO 4", (decimal)125600, (int)1000, (int)12, (decimal)125600, "");
                fiscalprinter.PrintRecTotal((decimal)1701000, (decimal)1701000, "503SEGUIRA FATTURA");
                fiscalprinter.EndFiscalReceipt(false);

                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo


                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app = 1;

                // Check document is refundable
                //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                strObj[0] = "1" + printerId + data + app.ToString().PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0');   // "1" = refund
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                iRet = Int32.Parse(iObj[0]);
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    log.Error("Document can NOT be Refunded");
                }
                // Return document print
                //Console.WriteLine("DirectIO (Return document print)");
                strObj[0] = "0140001REFUND " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);


                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 3");
                fiscalprinter.PrintRecRefund("BENE ES", (decimal)350000, (int)4);
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "000ACCONTO     ", 200000, 4);               
                fiscalprinter.PrintRecRefund("BENE 4", (decimal)560000, (int)3);              
                fiscalprinter.PrintRecRefund("SERVIZIO EE", (decimal)665400, (int)14);
                fiscalprinter.PrintRecRefund("SERVIZIO 4", (decimal)125600, (int)12);
                fiscalprinter.PrintRecTotal((decimal)1701000, (decimal)1701000, "503SEGUIRA FATTURA");
                fiscalprinter.EndFiscalReceipt(false);

                ZReport();




                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 4");
                fiscalprinter.PrintRecItem("BENE RM", (decimal)281700, (int)1000, (int)8, (decimal)281700, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "002BUONO MONOUSO", 100000, 8);
                fiscalprinter.PrintRecItem("BENE 4", (decimal)388900, (int)1000, (int)3, (decimal)388900, "");
                fiscalprinter.PrintRecItem("SERVIZIO 22", (decimal)670000, (int)1000, (int)10, (decimal)670000, "");
                fiscalprinter.PrintRecItem("SERVIZIO 4", (decimal)130000, (int)1000, (int)12, (decimal)130000, "");

                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)100000, "401Buono Celiachia");
                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)150000, "601BUONO MULTIUSO  ");
                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)70000, "401TICKET");
                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)1200000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app = 1;

                // Check document is refundable
                //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                strObj[0] = "1" + printerId + data + app.ToString().PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0');   // "1" = refund
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                iRet = Int32.Parse(iObj[0]);
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    log.Error("Document can NOT be Refunded");
                }
                // Return document print
                //Console.WriteLine("DirectIO (Return document print)");
                strObj[0] = "0140001REFUND " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
               

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 4");
                
                

                fiscalprinter.PrintRecRefund("BENE RM", (decimal)281700, (int)8);
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "002BUONO MONOUSO", 100000, 8);


                
                fiscalprinter.PrintRecRefund("BENE 4", (decimal)388900, (int)3);
                
                fiscalprinter.PrintRecRefund("SERVIZIO 22", (decimal)670000, (int)10);
                
                fiscalprinter.PrintRecRefund("SERVIZIO 4", (decimal)130000, (int)12);

                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)100000, "401Buono Celiachia");
                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)150000, "601BUONO MULTIUSO  ");
                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)70000, "401TICKET");
                fiscalprinter.PrintRecTotal((decimal)1570600, (decimal)1200000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport();





                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 5");
                fiscalprinter.PrintRecItem("BENE NI", (decimal)288900, (int)1000, (int)7, (decimal)288900, "");
                fiscalprinter.PrintRecItem("BENE AL", (decimal)888800, (int)1000, (int)9, (decimal)888800, "");
                fiscalprinter.PrintRecItem("BENE 22", (decimal)200000, (int)1000, (int)1, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001OMAGGIO", 200000, 1);


                fiscalprinter.PrintRecTotal((decimal)1177700, (decimal)1177700, "505NonRiscosso DCRaSSN");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app = 1;

                // Check document is refundable
                //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                strObj[0] = "1" + printerId + data + app.ToString().PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0');   // "1" = refund
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                iRet = Int32.Parse(iObj[0]);
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    log.Error("Document can NOT be Refunded");
                }
                // Return document print
                //Console.WriteLine("DirectIO (Return document print)");
                strObj[0] = "0140001REFUND " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 5");
                
                fiscalprinter.PrintRecRefund("BENE NI", (decimal)288900, (int)7);
                
                fiscalprinter.PrintRecRefund("BENE AL", (decimal)888800, (int)9);

                fiscalprinter.PrintRecRefund("BENE 22", (decimal)200000, (int)1);
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001OMAGGIO", 200000, 1);


                fiscalprinter.PrintRecTotal((decimal)1177700, (decimal)1177700, "505NonRiscosso DCRaSSN");
                fiscalprinter.EndFiscalReceipt(false);



                ZReport();




                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 6");
                fiscalprinter.PrintRecItem("BENE 4", (decimal)380000, (int)1000, (int)3, (decimal)380000, "");
                fiscalprinter.PrintRecItem("BENE 22", (decimal)133300, (int)1000, (int)1, (decimal)133300, "");
                fiscalprinter.PrintRecItem("SERVIZI 22", (decimal)200000, (int)1000, (int)10, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001OMAGGIO", 200000, 10);


                fiscalprinter.PrintRecTotal((decimal)513300, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo
                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app = 1;

                // Check document is refundable
                //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                strObj[0] = "1" + printerId + data + app.ToString().PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0');   // "1" = refund
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                iRet = Int32.Parse(iObj[0]);
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    log.Error("Document can NOT be Refunded");
                }
                // Return document print
                //Console.WriteLine("DirectIO (Return document print)");
                strObj[0] = "0140001REFUND " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;


                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 6");
                
                fiscalprinter.PrintRecRefund("BENE 4", (decimal)380000, (int)3);
                
                fiscalprinter.PrintRecRefund("BENE 22", (decimal)133300, (int)1);
                
                fiscalprinter.PrintRecRefund("SERVIZI 22", (decimal)200000, (int)10);
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001OMAGGIO", 200000, 10);


                fiscalprinter.PrintRecTotal((decimal)513300, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);

                ZReport();
                
                


                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 7");
                fiscalprinter.PrintRecItem("BENE 4", (decimal)264700, (int)1000, (int)3, (decimal)264700, "");
                fiscalprinter.PrintRecItem("BENE 22", (decimal)200000, (int)1000, (int)1, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001OMAGGIO", 200000, 1);

                fiscalprinter.PrintRecTotal((decimal)264700, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app = 1;

                // Check document is refundable
                //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                strObj[0] = "1" + printerId + data + app.ToString().PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0');   // "1" = refund
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                iRet = Int32.Parse(iObj[0]);
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    log.Error("Document can NOT be Refunded");
                }
                // Return document print
                //Console.WriteLine("DirectIO (Return document print)");
                strObj[0] = "0140001REFUND " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;


                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 7");
                
                fiscalprinter.PrintRecRefund("BENE 4", (decimal)264700, (int)3);
                
                fiscalprinter.PrintRecRefund("BENE 22", (decimal)200000, (int)1);
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001OMAGGIO", 200000, 1);

                fiscalprinter.PrintRecTotal((decimal)264700, (decimal)0, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport();




                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 8");
                fiscalprinter.PrintRecItem("BENE 4", (decimal)334600, (int)1000, (int)3, (decimal)334600, "");
                fiscalprinter.PrintRecItem("BENE 22", (decimal)200000, (int)1000, (int)1, (decimal)200000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 1);

                fiscalprinter.PrintRecTotal((decimal)334600, (decimal)34500, "600Sconto A Pagare");
                fiscalprinter.PrintRecTotal((decimal)334600, (decimal)310000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);



                ZReport(); //Nell esempio fa la chiusura e poi ci fa l 'annullo

                GeneralCounter.SetGeneralCounter();

                //Load general counter
                gc = GeneralCounter.GetGeneralCounter();

                app = 1;

                // Check document is refundable
                //Console.WriteLine("DirectIO (Check if Document can be Refunded)");
                strObj[0] = "1" + printerId + data + app.ToString().PadLeft(4, '0') + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0');   // "1" = refund
                dirIO = posCommonFP.DirectIO(0, 9205, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                //Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                iRet = Int32.Parse(iObj[0]);
                if (iRet == 0)
                    log.Info("Document can be Refunded");
                else
                {
                    log.Error("Document can NOT be Refunded");
                }
                // Return document print
                //Console.WriteLine("DirectIO (Return document print)");
                strObj[0] = "0140001REFUND " + (Convert.ToInt32(gc.ZRep) + 0).ToString().PadLeft(4, '0') + " " + app.ToString().PadLeft(4, '0') + " " + data + " " + printerId;
                dirIO = posCommonFP.DirectIO(0, 1078, strObj);
                iData = dirIO.Data;
                //Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;


                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecMessage("DOCUMENTO NUMERO 8");
                
                fiscalprinter.PrintRecRefund("BENE 4", (decimal)334600, (int)3);
                
                fiscalprinter.PrintRecRefund("BENE 22", (decimal)200000, (int)1);
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 200000, 1);

                fiscalprinter.PrintRecTotal((decimal)334600, (decimal)34500, "600Sconto A Pagare");
                fiscalprinter.PrintRecTotal((decimal)334600, (decimal)310000, "0CONTANTI");
                fiscalprinter.EndFiscalReceipt(false);


                ZReport();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);

                }
                else
                {
                    log.Fatal("", e);
                }
            }
            return NumExceptions;
        }





        //Metodo per generare un esempio di scontrino dove c'è lotteria e Sconto a pagare
        public int LotteriaLiveGiorgio()
        {
            try
            {
                log.Info("Performing LotteriaScontoAPagare Method() Esempio 23");
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                object iObj;

                fiscalprinter.BeginFiscalReceipt(true);
                fiscalprinter.PrintRecItem("Bene 22", (decimal)1500000, (int)1000, (int)1, (decimal)1500000, "");
                fiscalprinter.PrintRecItemAdjustment(FiscalAdjustment.AmountDiscount, "001", 500000, 1);
                fiscalprinter.PrintRecItem("Bene 22", (decimal)240500, (int)1000, (int)3, (decimal)240500, "");
                fiscalprinter.PrintRecItem("Servizio 10", (decimal)1220100, (int)1000, (int)11, (decimal)1220100, "");
                fiscalprinter.PrintRecItem("Servizio ES", (decimal)220000, (int)1000, (int)13, (decimal)200000, "");

                
                fiscalprinter.PrintRecTotal((decimal)2660600, (decimal)1200000, "201CARTADICREDITO");
                fiscalprinter.PrintRecTotal((decimal)2660600, (decimal)70000, "301TICKET");
                fiscalprinter.PrintRecTotal((decimal)2660600, (decimal)70000, "100Assegni");
                fiscalprinter.PrintRecTotal((decimal)3000000, (decimal)220000, "502NON RISCOSSO SERVIZI");
                fiscalprinter.PrintRecTotal((decimal)2660600, (decimal)0, "0CONTANTI");
                strObj[0] = "01" + "ABCDEFGN" + "        " + "0000";

                dirIO = posCommonFP.DirectIO(0, 1135, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;


                
                //PaymentCommand("Sconto a Pagare", "2200", "6", "00");
                fiscalprinter.EndFiscalReceipt(false);
                /*
                //MEGA WARNING!!!!!!!!!!!!!NON UTILIZZARE MAI IL METODO DEL DRIVER EndFiscalReceipt() ELSE NON SI CHIUDE LO SCONTRINO


                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1087, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 1087)
                {
                    log.Error("Error DirecIO 1087 , tentativo fallito di chiudere lo scontrino");
                    //throw new Exception();
                }
                fiscalprinter.ResetPrinter(); //necessario per il driver windows
                //ZReport();
                */
                //string ZRep = FiscalReceipt.fiscalprinter.GetData(FiscalData.ZReport, (int)0).Data;

                //zRep = (Convert.ToInt32(ZRep) + 1).ToString();
            }
            catch (Exception e)
            {
                NumExceptions++;
                fiscalprinter.ResetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    log.Fatal("", pce);

                }
                else
                {

                    log.Fatal("", e);

                }
            }
            return NumExceptions;
        }




    }

    //Classe creata per la gestione dei test su Server
    public class Server : FiscalReceipt
    {
        //lock object per eventuali problemi quando si chiama la CreateToken e si scrive sulla struttura
        static object acctLocl = new object();
        //variabili per memorizzare le info della cassa client
        public struct client
        {
            public string matricola { get; set; }
            public int ZRep { get; set; }
            public string _TillID { get; set; }
            public string ServerDate { get; set; }
            public int DocNum { get; set; }
            public string DailyAmount { get; set; }

            client(string Mat, int zrep, string tillid, string serverdate, int docnum , string dailyamount)
            {
                matricola = Mat;
                ZRep = zrep;
                _TillID = tillid;
                ServerDate = serverdate;
                DocNum = docnum;
                DailyAmount = dailyamount;
            }
        };

       
        public int ServerMulticassa(string _numThread)
        {
            try
            {
                int numThread = Convert.ToInt32(_numThread);
                String[] TillIdList = new String[20];
                for (int i = 0; i < numThread; ++i)
                {
                    TillIdList[i] = "AAAA" + (i + 1).ToString().PadLeft(4, '0');
                }
                Thread[] arrOfThread = new Thread[20];
                for (int i = 0; i < numThread; i++)
                {
                    int copy = i;
                    //Console.WriteLine("Index = :  {0}", i);
                    arrOfThread[copy] = new Thread(() => 
                        ServerParser(TillIdList[copy] , 2020 + copy));
                    //Assegnamo il nome ai thread con lo stesso nome della Cassa di riferimento per debug purposes
                    arrOfThread[copy].Name = TillIdList[copy];
                    arrOfThread[i].Start();
                }

                for(int i = 0; i < numThread; i++)
                {

                    arrOfThread[i].Join();
                }
                Console.WriteLine("Test Server Multicassa terminato");

            }
            catch (Exception e)
            {
                NumExceptions++;
                log.Fatal("Generic Error", e);
            }
            return NumExceptions;
        }



        //Metodo per (provare) a far fare ad un thread la chiusura e ad un thread le vendite
        public int ServerMulticassa2()
        {
            try
            {
                String[] TillIdList = new String[13];
                for (int i = 0; i < 3; ++i)
                {
                    TillIdList[i] = "AAAA" + (i + 1).ToString().PadLeft(4, '0');
                }
                Thread[] arrOfThread = new Thread[13];
                for (int i = 0; i < 1; i++)
                {
                    int copy = i;
                    //Console.WriteLine("Index = :  {0}", i);
                    arrOfThread[copy] = new Thread(() =>
                        ServerParser2(TillIdList[copy], 2020 + copy));
                    arrOfThread[copy].Name = TillIdList[copy];
                    arrOfThread[i].Start();
                }

                for (int i = 0; i < 1; i++)
                {

                    arrOfThread[i].Join();
                }
                Console.WriteLine("Test Server Multicassa terminato");

            }
            catch (Exception e)
            {
                NumExceptions++;
                log.Fatal("Generic Error", e);
            }
            return NumExceptions;
        }

        //todo 090520 in fase di assemblaggio
        //Check Lottery Server
        //todo: 130520 ho terminato il refactoring di tutta la lotterysmartparser per il server
        //la versione vecchia per l'RT è in TOSHIBA , caso mai mi dovesse servire in futuro
        //quindi questa non so se ha senso tenerla , eventualmente la elimino
        public int ServerLotterySmartParser()
        {
            try
            {
                //parsing della directory con gli xml lotteria e gli esiti
                string[] fileArray = Directory.GetFiles(@"D:\Epson_Copia_Chiavetta_Gialla2\ToolAggiornato\PosTestWithNunit\XmlFolder\", "*.xml", SearchOption.AllDirectories);
                foreach (string namefile in fileArray)
                {
                    Lottery lt = new Lottery();
                    WebService ws = new WebService();
                    int scontrini = 0;
                    WebService.LotteryFolderSentParser(namefile, ref scontrini);
                    //Lottery.LotteryFolderResponseParser();
                }
            }
            catch (Exception e)
            {
                NumExceptions++;
                if (e is SocketException)
                {
                    log.Error("SocketException: {0} ", e);
                }
                else
                {

                    log.Error("Generic Error", e);
                }

            }
            return NumExceptions;
        }

        //Metodo che parsa gli XML da inviare al Server JSRT che a sua volta li invierà al Server RT col protocollo SSL
        //E' necessario che il WebServer sia settato con SSL = 2 altrimenti il JSRT restituisce error code -99
        public int ServerParser(string TillID, int port)
        {
            client Client = new client { matricola = "", ZRep = 1, _TillID = "AAAA0001", ServerDate = "", DocNum = 1, DailyAmount =  "0.00"};

            try
            {
                //codice che serve per ricavarmi il mio IP pubblico
                int localport = port;
                Console.WriteLine("Performing Thread with TillID = {0}",  TillID);
                String address = "";
                /*
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    address = stream.ReadToEnd();
                }

                int first = address.IndexOf("Address: ") + 9;
                int last = address.LastIndexOf("</body>");
                address = address.Substring(first, last - first);
                
                */

                //parsing della directory XmlFolder con tutti i file xml di test
                string[] fileArray = Directory.GetFiles(@"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\", "*.xml", SearchOption.TopDirectoryOnly);

                IEnumerable<string> sortAscendingQuery = from file in fileArray orderby file select file;


                foreach (string namefile in sortAscendingQuery)
                {
                    //Se è un comando xml specifico per la cassa devo inviare la cassa specifica a cui inviarlo
                    String msgInput = File.ReadAllText(namefile);
                    if ((String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\3CreateToken.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\3Vendita.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\0ChiusuraAAAA0001.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\6ChiusuraAAAA0001.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4VenditaconLotteria.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\1CreateTillsAAAA0001.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\3VenditaConSconto.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\9CreateReport.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4aCreateToken") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\5CreateToken.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio1CNR.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio1CNRLottery.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio2CNR.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio2CNRLottery.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio3CNR.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio3CNRLottery.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio4CNR.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio4CNRLottery.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio5CNR.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio5CNRLottery.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio6CNR.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio6CNRLottery.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio7CNR.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio7CNRLottery.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio8CNR.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4Esempio8CNRLottery.xml") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\3ReadVatEntry.xml") == 0))
                        
                    {
                        int Place = msgInput.IndexOf("tillId");
                        if (Place != -1) //E' un comando con un tillID
                        {
                            string msgOutput = msgInput.Remove(Place + 8, 8).Insert(Place + 8, TillID);
                            msgInput = msgOutput;

                        }
                    }
                    byte[] msgBuffer = Encoding.Default.GetBytes(msgInput);


                    Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), (5000 + localport - 2020));
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), localport);

                    sck.Connect(endPoint);

                    while (!sck.Connected)
                    {
                        try
                        {
                            sck.Connect(endPoint);
                        }
                        catch (SocketException e)
                        {
                            log.Error("Errore Connessione con il server ", e);
                        }
                    }

                    if (sck.Connected)

                    {
                        //XmlDocument xmld = new XmlDocument();
                      //Xld.LoadXml(@"G:\Server\XmlPerServer\rt_server_create_receipt_lottery.xml");

                        //byte[] msgBuffer = File.ReadAllBytes(namefile);

                        
                        if ((String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4VenditaconLotteria.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\3Vendita.xml") == 0) || 
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\3VenditaConSconto.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio1CNR.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio1CNRLottery.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio2CNR.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio2CNRLottery.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio3CNR.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio3CNRLottery.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio4CNR.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio4CNRLottery.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio5CNR.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio5CNRLottery.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio6CNR.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio6CNRLottery.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio7CNR.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio7CNRLottery.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio8CNR.xml") == 0) ||
                            (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio8CNRLottery.xml") == 0))
                        {
                            int iteration = 100; //numero di scontrini che verranno inoltrati

                           
                            for (int i = 0; i < iteration; i++)
                            {

                                //ho dovuto mettere per ora sta chiusura socket per poter chiamare CreateToken
                                //se non si chiude il socket non lo riapre 
                                sck.Shutdown(SocketShutdown.Both);
                                sck.Close();
                                //chiamo la CreateToken per aggiornare i dati della cassa relativa
                                int result;
                                
                                do
                                {
                                    result = CreateToken(TillID, port, ref Client);

                                } while (result == -1);
                                Console.WriteLine("TillID = {0}, recNumber = {1}, DailyAmount = {2}", TillID, Client.DocNum, Client.DailyAmount);
                                
                                //Prima di inviare l'XML relativo dobbiamo riempire i campi con i dati corretti e formattati bene
                                int Place = msgInput.IndexOf("recNumber");
                               
                                if (Place != -1) //E' un comando con un tillID
                                {
                                    int recNumber = Client.DocNum;
                                    string msgOutput = msgInput.Remove(Place + 11, 4).Insert(Place + 11, recNumber.ToString().PadLeft(4, '0'));
                                    //in progress TODO 260520
                                    /*
                                    Place = msgInput.IndexOf("payment");
                                    decimal payment = Convert.ToDecimal(msgInput.Substring(Place + 9));
                                    */
                                    Place = msgInput.IndexOf("dailyAmount");
                                    decimal dailyAmount = decimal.Parse(Client.DailyAmount.Substring(0,7) + "," + Client.DailyAmount.Substring(7,2));

                                    if (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio1CNR.xml") == 0 || (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio1CNRLottery.xml") == 0))
                                    {
                                        dailyAmount += 266.06m;
                                    }
                                    if (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio2CNR.xml") == 0 || (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio2CNRLottery.xml") == 0))
                                    {
                                        dailyAmount += 175.67m;
                                    }
                                    if (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio3CNR.xml") == 0 || (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio3CNRLottery.xml") == 0))
                                    {
                                        dailyAmount += 150.10m;
                                    }
                                    if (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio4CNR.xml") == 0 || (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio4CNRLottery.xml") == 0 ))
                                    {
                                        dailyAmount += 137.06m;
                                    }
                                    if (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio5CNR.xml") == 0 || (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio5CNRLottery.xml") == 0))
                                    {
                                        dailyAmount += 117.77m;
                                    }
                                    if (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio6CNR.xml") == 0 || (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio6CNRLottery.xml") == 0))
                                    {
                                        dailyAmount += 51.33m;
                                    }
                                    if (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio7CNR.xml") == 0 || (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio7CNRLottery.xml") == 0))
                                    {
                                        dailyAmount += 26.47m;
                                    }
                                    if (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio8CNR.xml") == 0 || (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4Esempio8CNRLottery.xml") == 0))
                                    {
                                        dailyAmount += 33.46m;
                                    }
                                    if (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4VenditaconLotteria.xml") == 0 )
                                    {
                                        dailyAmount += 0.03m;
                                    }
                                    else //scontrini vecchi standard
                                    {
                                        dailyAmount += 0.01m;
                                    }
                                    msgOutput = msgOutput.Remove(Place + 13, 4).Insert(Place + 13, dailyAmount.ToString());
                                    string time = DateTime.Now.ToString("yyyyMMddTHHmmss");
                                    //if aggiunto per simulare gli SE Lotteria sballandno la data
                                    /*
                                    if(String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4VenditaconLotteria.xml") == 0 )
                                    {
                                        //In caso di lotteria ci aggiungo un giorno in piu' per creare gli SE
                                        time = DateTime.Now.AddDays(1).ToString("yyyyMMddTHHmmss");

                                    }
                                    */

                                    //if aggiunto per simulare gli scontrini lotteria + vecchi di un giorno
                                    /*
                                    if(String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4VenditaconLotteria.xml") == 0 )
                                    {
                                        //In caso di lotteria ci aggiungo un giorno in piu' per creare gli SE
                                        time = DateTime.Now.AddDays(-1).ToString("yyyyMMddTHHmmss");

                                    }
                                    */
                                    Place = msgInput.IndexOf("dateTime");
                                    msgOutput = msgOutput.Remove(Place + 10 , 15).Insert(Place + 10, time);
                                    Place = msgInput.IndexOf("zRepNumber");
                                    int zRepNum = Client.ZRep;
                                    msgOutput = msgOutput.Remove(Place + 12, 4).Insert(Place + 12, zRepNum.ToString().PadLeft(4,'0'));

                                    msgInput = msgOutput;

                                }
                                else
                                {
                                    log.Error("Errore di aggiornamento doc lotteria");
                                }
                                msgBuffer = Encoding.Default.GetBytes(msgInput);

                                sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                //endPoint = new IPEndPoint(IPAddress.Parse(address), (5000 + localport - 2020));
                                endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), localport);

                                sck.Connect(endPoint);

                                sck.Send(msgBuffer, 0, msgBuffer.Length, SocketFlags.None);

                                byte[] receiveBuffer = new byte[4096];

                                int rec = sck.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

                                Array.Resize(ref receiveBuffer, rec);
                                string Response = Encoding.Default.GetString(receiveBuffer);

                                //Check sulla risposta, se ricevo "false" dal Server tener conto quando genero il doc successivo
                                Place = Response.IndexOf("true");
                                //if (String.Compare(Response, Place + 9, "false" , 0 , 5, true) == 0)
                                if (Place == -1)
                                {
                                    log.Error("Ahia, qualcosa è andato storto, breakpoint");
                                }

                                Console.WriteLine("Test invio multiplo lotteria, scontrino numero: {0}", i);
                                Console.WriteLine("Received: {0} for the thread of tillId:= {1} about the XML command: {2}", Encoding.Default.GetString(receiveBuffer), TillID, namefile);
                                sck.Shutdown(SocketShutdown.Both);
                                sck.Close();
                                

                                if (i < iteration - 1)
                                {
                                    sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    //endPoint = new IPEndPoint(IPAddress.Parse(address), (5000 + localport - 2020));
                                    endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), (localport));

                                    //sck.Connect(endPoint);
                                    //Thread.Sleep(5000);
                                    while (!sck.Connected)
                                    {
                                        try
                                        {
                                            sck.Connect(endPoint);
                                        }
                                        catch (SocketException e)
                                        {
                                            log.Error("Errore Connessione con il server ", e);
                                        }
                                    }
                                }
                                Random rnd = new Random();
                                Thread.Sleep(rnd.Next(1000, 3000));
                                
                            }
                        }
                        else
                        {
                            if (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\3CreateToken.xml") != 0)
                            {
                                if ((String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\9ChiusuraServer.xml") == 0) && (String.Compare(TillID, "AAAA0001") == 0))
                                {
                                    //ChiusuraServer();
                                    Random rnd = new Random();
                                    Thread.Sleep(rnd.Next(15000, 20000));
                                    sck.Send(msgBuffer, 0, msgBuffer.Length, SocketFlags.None);

                                    byte[] receiveBuffer3 = new byte[4096];

                                    int rec3 = sck.Receive(receiveBuffer3, 0, receiveBuffer3.Length, 0);

                                    Array.Resize(ref receiveBuffer3, rec3);

                                    string info3 = Encoding.Default.GetString(receiveBuffer3);
                                    log.Error("Debug Mode ho mandato una chiusura server dal thread specifico");
                                    Console.WriteLine("Received: {0} for the thread of tillId:= {1} about the XML command: {2}", info3, TillID, namefile);

                                    sck.Shutdown(SocketShutdown.Both);
                                    sck.Close();

                                }
                                else
                                {
                                    sck.Send(msgBuffer, 0, msgBuffer.Length, SocketFlags.None);

                                    byte[] receiveBuffer = new byte[4096];

                                    int rec = sck.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

                                    Array.Resize(ref receiveBuffer, rec);

                                    string info = Encoding.Default.GetString(receiveBuffer);

                                    //Il Report di risposta di ogni cassa me lo scrivo su file (separato per ogni cassa obv)
                                    if (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\9CreateReport.xml") == 0)
                                    {
                                        System.IO.StreamWriter file = new System.IO.StreamWriter("G:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\" + "Report" + TillID + ".txt", true);
                                        file.WriteLine(info);
                                        file.Close();

                                    }


                                    Console.WriteLine("Received: {0} for the thread of tillId:= {1} about the XML command: {2}", info, TillID, namefile);
                                    sck.Shutdown(SocketShutdown.Both);
                                    sck.Close();
                                }
                            }
                            else //allora è la CreateToken, chiamo il metodo apposta
                            {
                                sck.Shutdown(SocketShutdown.Both);
                                sck.Close();
                                int result;
                                do
                                {
                                    result = CreateToken(TillID, port, ref Client);
                                } while (result == -1);

                            }
                            Thread.Sleep(1000);
                        }
                       

                        //Thread.Sleep(1000);
                        //Console.Read();
                    }
                    //Thread.Sleep(5000);
                }
            }
            catch (Exception e)
            {
                NumExceptions++;
                if (e is SocketException)
                {
                    log.Error("SocketException:  ", e);
                }
                else
                {
                    
                    log.Error("Generic Error", e);
                }
                
            }
            return NumExceptions;
        }

        //Metodo ad hoc per CreateToken
        //Tale metodo va a recuperare le info relative alla cassa TillID ossia:
        /*
         matricola 
         ZRep 
         TillID 
         ServerDate 
         DocNum 
         DailyAmount 
        */
        public static int CreateToken(string TillID, int port, ref client Client)
        {

            try
            {
                //codice che serve per ricavarmi il mio IP pubblico
                int localport = port;
                client localClient = Client;
                /*
                //Console.WriteLine("Performing Thread with TillID = {0}", TillID);
                String address = "";
                
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    address = stream.ReadToEnd();
                }

                int first = address.IndexOf("Address: ") + 9;
                int last = address.LastIndexOf("</body>");
                address = address.Substring(first, last - first);
                */

                Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), (5000 + localport - 2020));
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), localport);

                sck.Connect(endPoint);

                while (!sck.Connected)
                {
                    try
                    {
                        sck.Connect(endPoint);
                    }
                    catch (SocketException e)
                    {
                        log.Error("Errore Connessione con il server ", e);
                    }
                }

                if (sck.Connected)

                {
                    string msgInput = File.ReadAllText("D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\3CreateToken.xml");
                    
                    int Place = msgInput.IndexOf("tillId");
                    if (Place != -1) //E' un comando con un tillID
                    {
                        string msgOutput = msgInput.Remove(Place + 8, 8).Insert(Place + 8, TillID);
                        msgInput = msgOutput;

                    }
                    
                    byte[] msgBuffer = Encoding.Default.GetBytes(msgInput);


                    
                    sck.Send(msgBuffer, 0, msgBuffer.Length, SocketFlags.None);

                    byte[] receiveBuffer = new byte[4096];

                    int rec = sck.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

                    lock (acctLocl)
                    {
                        Array.Resize(ref receiveBuffer, rec);

                        string info = Encoding.Default.GetString(receiveBuffer);

                        //riempio la struttura client che mi serve quando invio scontrini "virtuali"
                        if (info.Length > 143)
                        {
                            localClient.matricola = info.Substring(94, 11);
                            localClient._TillID = info.Substring(105, 8);
                            localClient.ZRep = Convert.ToInt32(info.Substring(126, 4));
                            localClient.ServerDate = info.Substring(118, 8);
                            localClient.DocNum = Convert.ToInt32(info.Substring(130, 4));
                            localClient.DailyAmount = info.Substring(134, 9);
                        }
                        else
                        {
                            sck.Shutdown(SocketShutdown.Both);
                            sck.Close();
                            log.Error("Errore CreateToken TillID : =" +  TillID);
                            return -1;
                        }

                        //Console.WriteLine("Received: {0} for the thread of tillId:= {1} about the XML command: {2}", info, TillID, "CreateToken");
                        sck.Shutdown(SocketShutdown.Both);
                        sck.Close();
                        Client = localClient;
                    }

                }
                
            }
            catch (Exception e)
            {
                NumExceptions++;
                if (e is SocketException)
                {
                    log.Error("SocketException: {0} ", e);
                }
                else
                {

                    log.Error("Generic Error CreateToken", e);
                }
                return -1;

            }
            return 0;
        }

        //25/06/2020 TODO: da elaborare ancora, dovrei mettere i login e pass ma nn so se worka cmq perchè andrebbe incapsulato nell SSL
        public int ChiusuraServer()
        {

            try
            {


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://192.168.1.6/cgi-bin/fpmate.cgi");
                byte[] bytes;
                string msgInput = File.ReadAllText("D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\9ChiusuraServer.xml");
                bytes = System.Text.Encoding.ASCII.GetBytes(msgInput);
                request.ContentType = "text/xml; encoding='utf-8'";
                request.ContentLength = bytes.Length;
                request.Method = "POST";
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    string responseStr = new StreamReader(responseStream).ReadToEnd();
                   
                }
               
            }
            catch (Exception e)
            {
                NumExceptions++;
                if (e is SocketException)
                {
                    log.Error("SocketException: {0} ", e);
                }
                else
                {

                    log.Error("Generic Error CreateToken", e);
                }
                return -1;

            }
            return 0;
        }


        //Metodo che parsa gli XML da inviare al Server JSRT che a sua volta li invierà al Server RT col protocollo SSL
        //E' necessario che il WebServer sia settato con SSL = 2 altrimenti il JSRT restituisce error code -99
        //Faro' in modo che se TillId = AAAA0001 faccio vendite, se invece TillID = AAAA0002 faccio chiusura Server, e vediamo che succede
        public int ServerParser2(string TillID, int port)
        {
            client Client = new client { matricola = "", ZRep = 1, _TillID = "AAAA0001", ServerDate = "", DocNum = 1, DailyAmount = "0.00" };

            try
            {
                //codice che serve per ricavarmi il mio IP pubblico
                int localport = port;
                Console.WriteLine("Performing Thread with TillID = {0}", TillID);
                String address = "";
                /*
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    address = stream.ReadToEnd();
                }

                int first = address.IndexOf("Address: ") + 9;
                int last = address.LastIndexOf("</body>");
                address = address.Substring(first, last - first);
                
                */

                //parsing della directory XmlFolder con tutti i file xml di test
                string[] fileArray = Directory.GetFiles(@"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\", "*.xml", SearchOption.TopDirectoryOnly);

                IEnumerable<string> sortAscendingQuery = from file in fileArray orderby file select file;

                bool flag = false;
                byte[] msgBuffer;
                foreach (string namefile in sortAscendingQuery)
                {
                    //Se è un comando xml specifico per la cassa devo inviare la cassa specifica a cui inviarlo
                    String msgInput = File.ReadAllText(namefile);
                    if ((String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\4VenditaconLotteria.xml") == 0) && (String.Compare(TillID , "AAAA0001") == 0 ) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\6ChiusuraAAAA0001.xml") == 0) && (String.Compare(TillID, "AAAA0001") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\3CreateToken.xml") == 0) && (String.Compare(TillID, "AAAA0001") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\00jsrt_info.xml") == 0) && (String.Compare(TillID, "AAAA0001") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\1CreateTillsAAAA0001.xml") == 0) && (String.Compare(TillID, "AAAA0001") == 0)/* ||
                        (String.Compare(namefile, @"G:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\9ChiusuraServer.xml") == 0) && (String.Compare(TillID, "AAAA0001") == 0)*/)
                    {
                        int Place = msgInput.IndexOf("tillId");
                        if (Place != -1) //E' un comando con un tillID
                        {
                            string msgOutput = msgInput.Remove(Place + 8, 8).Insert(Place + 8, TillID);
                            msgInput = msgOutput;

                        }
                        flag = true;
                    }

                    if ((String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\9Reboot.xml") == 0) && (String.Compare(TillID, "AAAA0002") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\00jsrt_info.xml") == 0) && (String.Compare(TillID, "AAAA0002") == 0) ||
                        (String.Compare(namefile, @"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\9ChiusuraServer.xml") == 0) && (String.Compare(TillID, "AAAA0002") == 0))
                    {
                        
                        flag = true;
                    }
                    if (flag)
                    {
                        msgBuffer = Encoding.Default.GetBytes(msgInput);


                        Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        //IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), (5000 + localport - 2020));
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), localport);

                        sck.Connect(endPoint);

                        while (!sck.Connected)
                        {
                            try
                            {
                                sck.Connect(endPoint);
                            }
                            catch (SocketException e)
                            {
                                log.Error("Errore Connessione con il server ", e);
                            }
                        }
                        if (sck.Connected)
                        {
                            if ((String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4VenditaconLotteria.xml") == 0) || (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\3Vendita.xml") == 0) || (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\3VenditaConSconto.xml") == 0))
                            {
                                int iteration = 11; //numero di scontrini che verranno inoltrati


                                for (int i = 0; i < iteration; i++)
                                {

                                    //ho dovuto mettere per ora sta chiusura socket per poter chiamare CreateToken
                                    //se non si chiude il socket non lo riapre 
                                    sck.Shutdown(SocketShutdown.Both);
                                    sck.Close();
                                    //chiamo la CreateToken per aggiornare i dati della cassa relativa
                                    int result;

                                    do
                                    {
                                        result = CreateToken(TillID, port, ref Client);

                                    } while (result == -1);
                                    Console.WriteLine("TillID = {0}, recNumber = {1}, DailyAmount = {2}", TillID, Client.DocNum, Client.DailyAmount);

                                    //Prima di inviare l'XML relativo dobbiamo riempire i campi con i dati corretti e formattati bene
                                    int Place = msgInput.IndexOf("recNumber");

                                    if (Place != -1) //E' un comando con un tillID
                                    {
                                        int recNumber = Client.DocNum;
                                        string msgOutput = msgInput.Remove(Place + 11, 4).Insert(Place + 11, recNumber.ToString().PadLeft(4, '0'));
                                        //in progress TODO 260520
                                        /*
                                        Place = msgInput.IndexOf("payment");
                                        decimal payment = Convert.ToDecimal(msgInput.Substring(Place + 9));
                                        */
                                        Place = msgInput.IndexOf("dailyAmount");
                                        decimal dailyAmount = decimal.Parse(Client.DailyAmount.Substring(0, 7) + "," + Client.DailyAmount.Substring(7, 2));
                                        dailyAmount += 0.01m;

                                        msgOutput = msgOutput.Remove(Place + 13, 4).Insert(Place + 13, dailyAmount.ToString());
                                        string time = DateTime.Now.ToString("yyyyMMddTHHmmss");
                                        //if aggiunto per simulare gli SE Lotteria sballandno la data
                                        /*
                                        if(String.Compare(namefile, "G:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\4VenditaconLotteria.xml") == 0 && (i > 90))
                                        {
                                            //In caso di lotteria ci aggiungo un giorno in piu' per creare gli SE
                                            time = DateTime.Now.AddDays(1).ToString("yyyyMMddTHHmmss");

                                        }
                                        */
                                        Place = msgInput.IndexOf("dateTime");
                                        msgOutput = msgOutput.Remove(Place + 10, 15).Insert(Place + 10, time);
                                        Place = msgInput.IndexOf("zRepNumber");
                                        int zRepNum = Client.ZRep;
                                        msgOutput = msgOutput.Remove(Place + 12, 4).Insert(Place + 12, zRepNum.ToString().PadLeft(4, '0'));

                                        msgInput = msgOutput;

                                    }
                                    else
                                    {
                                        Console.WriteLine("Errore di aggiornamento doc lotteria");
                                    }
                                    msgBuffer = Encoding.Default.GetBytes(msgInput);

                                    sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    //endPoint = new IPEndPoint(IPAddress.Parse(address), (5000 + localport - 2020));
                                    endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), localport);

                                    sck.Connect(endPoint);

                                    sck.Send(msgBuffer, 0, msgBuffer.Length, SocketFlags.None);

                                    byte[] receiveBuffer2 = new byte[4096];

                                    int rec2 = sck.Receive(receiveBuffer2, 0, receiveBuffer2.Length, 0);

                                    Array.Resize(ref receiveBuffer2, rec2);
                                    string Response = Encoding.Default.GetString(receiveBuffer2);

                                    //Check sulla risposta, se ricevo "false" dal Server tener conto quando genero il doc successivo
                                    Place = Response.IndexOf("true");
                                    //if (String.Compare(Response, Place + 9, "false" , 0 , 5, true) == 0)
                                    if (Place == -1)
                                    {
                                        log.Error("Ahia, qualcosa è andato storto, breakpoint");
                                    }

                                    Console.WriteLine("Test invio multiplo lotteria, scontrino numero: {0}", i);
                                    Console.WriteLine("Received: {0} for the thread of tillId:= {1} about the XML command: {2}", Encoding.Default.GetString(receiveBuffer2), TillID, namefile);
                                    sck.Shutdown(SocketShutdown.Both);
                                    sck.Close();
                                    Thread.Sleep(5000);

                                    if (i < iteration - 1)
                                    {
                                        sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                        //endPoint = new IPEndPoint(IPAddress.Parse(address), (5000 + localport - 2020));
                                        endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), (localport));

                                        //sck.Connect(endPoint);
                                        //Thread.Sleep(5000);
                                        while (!sck.Connected)
                                        {
                                            try
                                            {
                                                sck.Connect(endPoint);
                                            }
                                            catch (SocketException e)
                                            {
                                                log.Error("Errore Connessione con il server ", e);
                                            }
                                        }
                                    }

                                }
                            }
                            else
                            {
                                if ((String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\9ChiusuraServer.xml") == 0) ||
                                    (String.Compare(namefile, "D:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\9Reboot.xml") == 0))
                                {
                                    sck.Shutdown(SocketShutdown.Both);
                                    sck.Close();
                                    int iteration = 10;
                                    
                                    for (int i = 0; i < iteration; i++)
                                    {
                                        Random rnd = new Random();
                                        Thread.Sleep(rnd.Next(10000, 30000));
                                        //ChiusuraServer();
                                        sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                        //endPoint = new IPEndPoint(IPAddress.Parse(address), (5000 + localport - 2020));
                                        endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), (localport));

                                        //sck.Connect(endPoint);
                                        //Thread.Sleep(5000);
                                        while (!sck.Connected)
                                        {
                                            try
                                            {
                                                sck.Connect(endPoint);
                                            }
                                            catch (SocketException e)
                                            {
                                                log.Error("Errore Connessione con il server ", e);
                                            }
                                        }
                                        sck.Send(msgBuffer, 0, msgBuffer.Length, SocketFlags.None);

                                        byte[] receiveBuffer3 = new byte[4096];

                                        int rec3 = sck.Receive(receiveBuffer3, 0, receiveBuffer3.Length, 0);

                                        Array.Resize(ref receiveBuffer3, rec3);

                                        string info3 = Encoding.Default.GetString(receiveBuffer3);
                                        log.Error("Debug Mode ho mandato una chiusura server dal thread AAAA0002");
                                         Console.WriteLine("Received: {0} for the thread of tillId:= {1} about the XML command: {2}", info3, TillID, namefile);

                                        sck.Shutdown(SocketShutdown.Both);
                                        sck.Close();
                                        
                                
                                    }
                                }
                                else
                                {
                                    sck.Send(msgBuffer, 0, msgBuffer.Length, SocketFlags.None);

                                    byte[] receiveBuffer = new byte[4096];

                                    int rec = sck.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

                                    Array.Resize(ref receiveBuffer, rec);

                                    string info = Encoding.Default.GetString(receiveBuffer);

                                    Console.WriteLine("Received: {0} for the thread of tillId:= {1} about the XML command: {2}", info, TillID, namefile);
                                    /* non ci arriverà mai qui
                                    if ((String.Compare(namefile, "G:\\Epson_Copia_Chiavetta_Gialla2\\Server\\XmlPerServer\\9ChiusuraServer.xml") == 0))

                                    {
                                        log.Error("Debug Mode ho mandato una chiusura server dal thread AAAA0002");
                                    }
                                    */
                                    sck.Shutdown(SocketShutdown.Both);
                                    sck.Close();
                                    Thread.Sleep(10000);
                                }
                            }
                        }

                        flag = false;
                    }

                    
                }
            }
            catch (Exception e)
            {
                NumExceptions++;
                if (e is SocketException)
                {
                    log.Error("SocketException:  ", e);
                }
                else
                {

                    log.Error("Generic Error", e);
                }

            }
            return NumExceptions;
        }


        //ReadSetting : index da 1 a 14 legge alcuni settaggi della printer
        public int ReadSetting(string index)
        {
            try
            {
                log.Info("Performing ReadSetting Method");

                string[] strObj = new string[1];
                string[] iObj = new string[1];
                DirectIOData dirIO;


                //strObj[0] = index.PadLeft(2,'0') + "000";
                for (int i = 0; i < 15; i++)
                {
                    strObj[0] = i.ToString().PadLeft(2, '0') + "000";
                    dirIO = posCommonFP.DirectIO(0, 9202, strObj);
                    int iData = dirIO.Data;
                    iObj = (string[])dirIO.Object;

                    if (iData != 9202)
                    {
                        log.Error("Errore, ReadSetting , expected risposta: 9202,  received: " + iData);
                    }
                    else

                    {
                        string NumRec = iObj[0].Substring(2, 3);
                        string On_Off = iObj[0].Substring(5, 1);
                        string Data = iObj[0].Substring(6, 12);
                        string FiscalCode = iObj[0].Substring(18, 16);
                        string VatLab = iObj[0].Substring(34, 30);
                        string CustVat = iObj[0].Substring(64, 30);
                        string SrvCode = iObj[0].Substring(94, 2);
                        string text = iObj[0].Substring(96, 100);
                        string Zrep = iObj[0].Substring(196, 4);
                    }
                }
            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);
                    //log.Fatal("", pce);
                    //throw;
                }
                else
                {
                    log.Fatal("", e);
                }

            }
            return NumExceptions;
        }

        //DirectIO 4037 , Set ATECO Code, Index 01, 02, 03 per Server
        //Metodo che crea un XML formattato con i dati di input da inviare al JSRT per la programmazione degli AtecoCode del Server
        //E' necessario che il WebServer sia settato con SSL = 2 altrimenti il JSRT restituisce error code -99 (verificare)
        // Xml di riferimento generico è il SetAtecoCode.axml (lo lascio axml e non xml senno il serverparser lo prende in considerazione)
        public int SetAtecoCodeServer(string AtecoIndex, string AtecoCode, string Ventilazione, string port)
        {
            
            try
            {
                int localport = Convert.ToInt32(port);
                /*
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    address = stream.ReadToEnd();
                }

                int first = address.IndexOf("Address: ") + 9;
                int last = address.LastIndexOf("</body>");
                address = address.Substring(first, last - first);
                
                */

                //Se è un comando xml specifico per la cassa devo inviare la cassa specifica a cui inviarlo
                String msgInput = File.ReadAllText(@"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\SetAtecoCode.axml");
                string stringConfiguration = "";


                string atecoindex = AtecoIndex.PadLeft(2, '0');
                string atecocode = AtecoCode;
                string ventilazione = Ventilazione;
                string spare = "".PadLeft(10, ' ');

                stringConfiguration = atecoindex + atecocode + ventilazione + "0" + spare;

                int Place = msgInput.IndexOf("data");
                if (Place != -1) 
                {
                    string msgOutput = msgInput.Remove(Place + 6, 20).Insert(Place + 6, stringConfiguration);
                    msgInput = msgOutput;

                }
                   
                byte[] msgBuffer = Encoding.Default.GetBytes(msgInput);


                Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), (5000 + localport - 2020));
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), localport);

                sck.Connect(endPoint);

                while (!sck.Connected)
                {
                    try
                    {
                        sck.Connect(endPoint);
                    }
                    catch (SocketException e)
                    {
                        log.Error("Errore Connessione con il server ", e);
                    }
                }

                if (sck.Connected)

                {
                        
                    sck.Send(msgBuffer, 0, msgBuffer.Length, SocketFlags.None);

                    byte[] receiveBuffer = new byte[4096];

                    int rec = sck.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

                    Array.Resize(ref receiveBuffer, rec);
                    string Response = Encoding.Default.GetString(receiveBuffer);

                    //Check sulla risposta, se ricevo "false" dal Server tener conto quando genero il doc successivo
                    Place = Response.IndexOf("true");
                        
                    if (Place == -1)
                    {
                        log.Error("Ahia, qualcosa è andato storto nella programmazione Ateco, breakpoint");
                    }
                    else
                    {
                        Console.WriteLine("Programmazione AtecoIndex: ", AtecoIndex);
                        Console.WriteLine("Received Response: {0} ", Encoding.Default.GetString(receiveBuffer));
                    }
                   
                    sck.Shutdown(SocketShutdown.Both);
                    sck.Close();

                }

            }
            catch (Exception e)
            {
                NumExceptions++;
                if (e is SocketException)
                {
                    log.Error("SocketException:  ", e);
                }
                else
                {

                    log.Error("Generic Error", e);
                }
                closeFiscalDevice();
            }
            return NumExceptions;
        }



      
        //DirectIO 4002
        //Set Department Parameters per Server
        //Metodo che crea un XML formattato con i dati di input da inviare al JSRT per la programmazione dei reparti del Server
        //E' necessario che il WebServer sia settato con SSL = 2 altrimenti il JSRT restituisce error code -99 (verificare)
        // Xml di riferimento generico è il SetAtecoCode.axml (lo lascio axml e non xml senno il serverparser lo prende in considerazione)
        public int SetDepParEmbeddedServer(string Num_rep, string AtecoIndex, string VatIndex, string SalesType, string description)
        {

            try
            {
                int localport = 2020;
                /*
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    address = stream.ReadToEnd();
                }

                int first = address.IndexOf("Address: ") + 9;
                int last = address.LastIndexOf("</body>");
                address = address.Substring(first, last - first);
                
                */

                //Se è un comando xml specifico per la cassa devo inviare la cassa specifica a cui inviarlo
                String msgInput = File.ReadAllText(@"D:\Epson_Copia_Chiavetta_Gialla2\Server\XmlPerServer\SetDepPar.axml");
                string stringConfiguration = "";

                string rep_amount = "001000000";
                string num_rep = Num_rep.PadLeft(2, '0');

                string rep_description = description.PadRight(20, ' ');
                string vatIndex = VatIndex.PadLeft(2, '0'); //Per esempio da 00 a 59
                string pLIM = "999999999";
                string prnGPR = "00";
                string prodGPR = "00";
                string mu = "EU";
                string salesType = SalesType; // "0"; 0 = Goods , 1 = Service
                string salesAttribute = "00"; //00 = Reparto con Sconto a Pagare No , 01 = Reparto con Sconto a pagare SI
                string atecoIndex = AtecoIndex.PadLeft(2, '0'); //00-99 , ma che poi nella realtà saranno max 3 e tipo : Rep1 - Rep20 associati al Codice Ateco 1, 
                                                                // Rep21 - Rep40 associati al Codice Ateco 2, Rep 41 - Rep 60 codice ateco 3


                stringConfiguration = num_rep + rep_description + rep_amount + rep_amount + rep_amount + "0" + vatIndex + pLIM + prnGPR + prodGPR + mu + salesType + salesAttribute + atecoIndex;



                int Place = msgInput.IndexOf("data");
                if (Place != -1)
                {
                    string msgOutput = msgInput.Remove(Place + 6, 67).Insert(Place + 6, stringConfiguration);
                    msgInput = msgOutput;

                }

                byte[] msgBuffer = Encoding.Default.GetBytes(msgInput);


                Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), (5000 + localport - 2020));
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), localport);

                sck.Connect(endPoint);

                while (!sck.Connected)
                {
                    try
                    {
                        sck.Connect(endPoint);
                    }
                    catch (SocketException e)
                    {
                        log.Error("Errore Connessione con il server ", e);
                    }
                }

                if (sck.Connected)

                {

                    sck.Send(msgBuffer, 0, msgBuffer.Length, SocketFlags.None);

                    byte[] receiveBuffer = new byte[4096];

                    int rec = sck.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

                    Array.Resize(ref receiveBuffer, rec);
                    string Response = Encoding.Default.GetString(receiveBuffer);

                    //Check sulla risposta, se ricevo "false" dal Server tener conto quando genero il doc successivo
                    Place = Response.IndexOf("true");

                    if (Place == -1)
                    {
                        log.Error("Ahia, qualcosa è andato storto nella programmazione reparto" + num_rep + ", breakpoint");
                    }
                    else
                    {
                        Console.WriteLine("Programmazione Reparto: ", num_rep);
                        Console.WriteLine("Received Response: {0} ", Encoding.Default.GetString(receiveBuffer));
                    }
                    sck.Shutdown(SocketShutdown.Both);
                    sck.Close();

                }

            }
            catch (Exception e)
            {
                NumExceptions++;
                if (e is SocketException)
                {
                    log.Error("SocketException:  ", e);
                }
                else
                {

                    log.Error("Generic Error", e);
                }

            }
            return NumExceptions;
        }


    }

    //Classe creata per la gestione dei test per le license
    public class Licence : FiscalReceipt
    {
        //FiscalReceipt base class
        public Licence()
        {
            try
            {
                /*
                posExplorer = new PosExplorer();
                // Console.WriteLine("Taking FiscalPrinter device ");
                DeviceInfo fp = posExplorer.GetDevice("FiscalPrinter", "FiscalPrinter1");

                posCommonFP = (PosCommon)posExplorer.CreateInstance(fp);
                //posCommonFP.StatusUpdateEvent += new StatusUpdateEventHandler(co_OnStatusUpdateEvent);
                
                */
                // Console.WriteLine("Initializing FiscalPrinter ");
                if (!opened)
                {
                    fiscalprinter = (FiscalPrinter)posCommonFP;
                    //Console.WriteLine("Performing Open() method ");
                    fiscalprinter.Open();

                    //Console.WriteLine("Performing Claim() method ");
                    fiscalprinter.Claim(1000);

                    //Console.WriteLine("Setting DeviceEnabled property ");
                    fiscalprinter.DeviceEnabled = true;

                    //Console.WriteLine("Performing ResetPrinter() method ");
                    //fiscalprinter.ResetPrinter();
                }

            }
            catch (Exception e)
            {
                //Console.WriteLine("----- EXCEPTION -----");
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Fatal("", pce);

                }
                else
                {
                    log.Error(e.ToString());
                    //log.Fatal("", e);

                }
            }
        }


        //DirectIO 9010
        //motsig : 0 = MOT , 1 = SIG
        public int SetFirmwareUpgradeRepository(string motsig, string URL)
        {
            try
            {
                log.Info("Performing SetFirmwareUpgradeRepository");
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;



                strObj[0] = motsig + URL.PadRight(256, ' ');
                dirIO = posCommonFP.DirectIO(0, 9010, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 9010)
                {
                    log.Error("Error DirecIO 9010 , tentativo fallito di settare l'URL del firmware");
                    throw new Exception();
                }



            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;

        }


        //DirectIO 9210
        //motsig : 0 = MOT , 1 = SIG
        public int ReadFirmwareUpgradeRepository(string motsig)
        {
            try
            {
                log.Info("Performing SetFirmwareUpgradeRepository");
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;



                strObj[0] = motsig;
                dirIO = posCommonFP.DirectIO(0, 9210, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 9210)
                {
                    log.Error("Error DirecIO 9210 , tentativo fallito di settare l'URL del firmware");
                    throw new Exception();
                }

                string Url = iObj[0].Substring(1, 256);

            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;

        }

        //DirectIO 9011
        //Metodo per aggiornare il Firmware tramite DirectIO e Url
        //EDIT: 16/12/2020 Aggiornamento Comando per gestione Server Licenze Robina
        public int ActivateFirmwareUpgrade(string _temp)
        {
            try
            {
                log.Info("Performing ActivateFirmwareUpgrade");
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;

                /*
                string extendedCommand = _extendedCommand;
                string techFC = "NVRNDR59B10F205D";
                string vatNumber = "IT12825980159";
                string userID = "epson     ";
                string passwd = "epson     ";
                string urlUsb = UrlUsb;
                string Opt = "   ";
                */

                string extendedCommand = "0";
                string techFC = "CHCFNC64E01L117X";
                string vatNumber = "IT04210780963";
                string userID = "epson     ";
                string passwd = "epson     ";
                string urlUsb = "0";
                string Opt = "   ";
                string firmVer = "10.03";
                string buildNum = "1234";
                
                string temp = _temp;
                strObj[0] = temp;
                dirIO = posCommonFP.DirectIO(0, 9011, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 9011)
                {
                    log.Error("Error DirecIO 9011 , tentativo fallito di settare l'URL del firmware");
                    throw new Exception();
                }



            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;

        }

        //DirectIO 9007
        //Metodo per settare l'URL da cui scaricare il certificato 
        public int SetUrlCACertificate(string CA_URL)
        {
            try
            {
                log.Info("Performing SetUrlCACertificate");
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;


                string ca_url = CA_URL.PadRight(256, ' ');



                strObj[0] = ca_url;
                dirIO = posCommonFP.DirectIO(0, 9007, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 9007)
                {
                    log.Error("Error DirecIO 9007 , tentativo fallito di settare l'URL del CA Certificato");
                    throw new Exception();
                }



            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;

        }

        //DirectIO 9207
        //Metodo che legge l'URL da cui si scarica il Certificato
        public int ReadUrlCACertificate()
        {
            try
            {
                log.Info("Performing ReadUrlCACertificate");
                string[] strObj = new string[1];
                string[] iObj = new string[1];
                int iData;
                DirectIOData dirIO;



                strObj[0] = "";
                dirIO = posCommonFP.DirectIO(0, 9207, strObj);
                iData = dirIO.Data;

                iObj = (string[])dirIO.Object;
                if (iData != 9207)
                {
                    log.Error("Error DirecIO 9207 , tentativo fallito di leggere l'URL del CA Certificato");
                    throw new Exception();
                }
                string ca_url = iObj[0];


            }
            catch (Exception e)
            {
                NumExceptions++;
                resetPrinter();
                if (e is PosControlException)
                {
                    PosControlException pce = (PosControlException)e;
                    log.Error("ErrorCode: " + pce.ErrorCode.ToString());
                    log.Error("ErrorCodeExtended: " + pce.ErrorCodeExtended.ToString());
                    log.Error("Message: " + pce.Message);

                }
                else
                {
                    log.Fatal("Generic Error ", e);
                }
            }

            return NumExceptions;

        }


    }
}
            
