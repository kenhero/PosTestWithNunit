using Microsoft.PointOfService;
using System;

namespace Report.Library
{
    public class Report
    {


        private PosExplorer posExplorer;
        private PosCommon posCommonFP;


        //exception counter
        public static int NumExceptions = 0;


        public Report(string printerName)
        {
            try
            {
                posExplorer = new PosExplorer();
                // Console.WriteLine("Taking FiscalPrinter device ");
                DeviceInfo fp = posExplorer.GetDevice("FiscalPrinter", printerName);
                posCommonFP = (PosCommon)posExplorer.CreateInstance(fp);
                posCommonFP.StatusUpdateEvent += new StatusUpdateEventHandler(co_OnStatusUpdateEvent);

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
                }
                else
                {
                    Console.WriteLine(e.ToString());
                }
            }
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



        //Method to test the original FiscalDocument
        public int testReportClass(string printerName)
        {
            //only for debug
            string printerState = null;
            try
            {

                // Console.WriteLine("Initializing FiscalPrinter ");
                FiscalPrinter fiscalprinter = (FiscalPrinter)posCommonFP;

                Console.WriteLine("Performing Open() method ");
                fiscalprinter.Open();

                Console.WriteLine("Performing Claim() method ");
                fiscalprinter.Claim(1000);

                Console.WriteLine("Setting DeviceEnabled property ");
                fiscalprinter.DeviceEnabled = true;

                Console.WriteLine("Performing ResetPrinter() method ");
                fiscalprinter.ResetPrinter();

                // RT Specific Commands - BEG

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

                // Check RT status
                Console.WriteLine("DirectIO (RT status)");
                strObj[0] = "01";
                dirIO = posCommonFP.DirectIO(0, 1138, strObj);
                iData = dirIO.Data;
                Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                Console.WriteLine("DirectIO() : iObj = " + iObj[0]);


                //Print Current State
                Console.WriteLine("Printer state = " + fiscalprinter.PrinterState.ToString());
                printerState = fiscalprinter.PrinterState.ToString();

                rtType = iObj[0].Substring(3, 2);
                Console.WriteLine("RT type: " + rtType);
                int rtTypeInt = Convert.ToInt32(rtType);
                if (rtTypeInt == 1)
                {
                    isRT = false;
                    Console.WriteLine("Printer is NOT RT model");
                }
                else
                if (rtTypeInt == 2)
                {
                    isRT = true;
                    Console.WriteLine("Printer is in RT model");
                }


                //Print Current State
                Console.WriteLine("Printer state = " + fiscalprinter.PrinterState.ToString());
                printerState = fiscalprinter.PrinterState.ToString();
                Console.WriteLine("from Monitor should be to Report State");
                fiscalprinter.PrintReport(ReportType.Date, (string)"010720190000", (string)"020720190000");
                printerState = fiscalprinter.PrinterState.ToString();
                //Print Current State
                Console.WriteLine("Printer state = " + fiscalprinter.PrinterState.ToString());
                printerState = fiscalprinter.PrinterState.ToString();
                //Print ZReport
                fiscalprinter.PrintZReport();

                //Print Current State, I should be now in Report State
                Console.WriteLine("Printer state = " + fiscalprinter.PrinterState.ToString());
                printerState = fiscalprinter.PrinterState.ToString();

                fiscalprinter.ResetPrinter();
                //Print Current State, I should be now in Report State
                Console.WriteLine("Printer state = " + fiscalprinter.PrinterState.ToString());
                printerState = fiscalprinter.PrinterState.ToString();

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
                }
                else
                {
                    Console.WriteLine(e.ToString());
                }

            }
            return NumExceptions;
        }
    }
}