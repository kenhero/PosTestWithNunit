using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.PointOfService;

namespace ElectronicJournal.Library
{
    public class ElectronicJournal
    {
        static PosExplorer posExplorer;
        static PosCommon posCommonEJ;
        static Microsoft.PointOfService.ElectronicJournal electronicjournal;

        //exception counter
        public static int NumExceptionsEJ = 0;

        public PosExplorer PosExplorer
        {
            get { return posExplorer; }
            //set {   posExplorer = new PosExplorer();    }
        }

        public PosCommon PosCommonEJ
        {
            get { return posCommonEJ; }
            //set {  }
        }


        //ElectronicJournalClass base class
        public ElectronicJournal()
        {
            try
            {
                // Console.WriteLine("Initializing PosExplorer ");
                posExplorer = new PosExplorer();

                // Console.WriteLine("Taking ElectronicJournal device ");
                DeviceInfo ej = posExplorer.GetDevice("ElectronicJournal", "ElectronicJournal1");

                // Console.WriteLine("Creating instance of ElectronicJournal device ");
                posCommonEJ = (PosCommon)posExplorer.CreateInstance(ej);
                posCommonEJ.StatusUpdateEvent += new StatusUpdateEventHandler(co_OnStatusUpdateEvent);
            }
            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                NumExceptionsEJ++;
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
                    case Microsoft.PointOfService.ElectronicJournal.StatusIdle:
                        text = "Idle";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusMediumFull:
                        text = "Medium Full";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusMediumInserted:
                        text = "Medium Inserted";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusMediumNearFull:
                        text = "Medium Near Full";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusMediumRemoved:
                        text = "Medium Removed";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusMediumSuspended:
                        text = "Medium Suspended";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusPowerOff:
                        text = "Power Off";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusPowerOffline:
                        text = "Power Offline";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusPowerOffOffline:
                        text = "Power Off Offline";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusPowerOnline:
                        text = "Power Online";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusUpdateFirmwareComplete:
                        text = "Firmware Complete";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusUpdateFirmwareCompleteDeviceNotRestored:
                        text = "Firmware Complete Device Not Restored";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusUpdateFirmwareFailedDeviceNeedsFirmware:
                        text = "Firmware Failed Device Nedds Firmware";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusUpdateFirmwareFailedDeviceOk:
                        text = "Firmware Failed Device Ok";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusUpdateFirmwareFailedDeviceUnknown:
                        text = "Firmware Failed Device Unknown";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusUpdateFirmwareFailedDeviceUnrecoverable:
                        text = "Firmware Failed Device Unrecoverable";
                        break;
                    case Microsoft.PointOfService.ElectronicJournal.StatusUpdateFirmwareProgress:
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


        //Method to initialize the Electronic Journal
        public int initElectronicJournal(string ElectronicJournalName)
        {
            try
            {
                /* test tolto perchè queste operazioni le faccio nel costruttore di base
                // Console.WriteLine("Initializing PosExplorer ");
                posExplorer = new PosExplorer();

                // Console.WriteLine("Taking ElectronicJournal device ");
                DeviceInfo ej = posExplorer.GetDevice("ElectronicJournal", ElectronicJournalName);

                // Console.WriteLine("Creating instance of ElectronicJournal device ");
                posCommonEJ = (PosCommon)posExplorer.CreateInstance(ej);
                */

                // Console.WriteLine("Initializing ElectronicJournal ");
                electronicjournal = (Microsoft.PointOfService.ElectronicJournal)posCommonEJ;

                Console.WriteLine("Performing Open() method ");
                electronicjournal.Open();

                Console.WriteLine("Performing Claim() method ");
                electronicjournal.Claim(1000);

                Console.WriteLine("Setting DeviceEnabled property ");
                electronicjournal.DeviceEnabled = true;

                Console.WriteLine("Setting DataEventEnabled property ");
                electronicjournal.DataEventEnabled = true;

            }
            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                NumExceptionsEJ++;
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

                return NumExceptionsEJ;
            }
            return 0;
        }

        //Reset ElectronicJournal
        public int ResetElectronicJournal()
        {
            try
            {


                //Console.WriteLine("Initializing ElectronicJournal ");
                electronicjournal = (Microsoft.PointOfService.ElectronicJournal)posCommonEJ;

                //Console.WriteLine("Performing Open() method ");
                electronicjournal.Open();

                //Console.WriteLine("Performing Claim() method ");
                electronicjournal.Claim(1000);

                //Console.WriteLine("Setting DeviceEnabled property ");
                electronicjournal.DeviceEnabled = true;

                

            }
            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
                NumExceptionsEJ++;
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

                return NumExceptionsEJ;
            }
            return 0;

        }



        //Letture varie dall' Electronic Journal
        public void readFromEJ()
        {

            try
            {

                Console.WriteLine("Performing readFromEJByNumber() method ");
                string[] strObj = new string[1];
                DirectIOData dirIO;
                int iData;
                string[] iObj = new string[1];
                //Boolean isRT = false;
                //string strData = "";

                //questa poi va cambiata e chiesta come input
                string date1 = "160719";
                string date2 = "170719";

                string FRN = "";
                //string strDate = "";
                string LN = "";
                string TEXT = "";

                // Write the string to a file in append mode
                System.IO.StreamWriter file = new System.IO.StreamWriter("counters.txt", true);
                string lines = "";




                //public static PosCommon posCommonFP;
                //Check EJ Status 
                strObj[0] = "01";
                Console.WriteLine("DirectIO 1077 E/J Status");
                dirIO = posCommonEJ.DirectIO(0, 1077, strObj);
                iData = dirIO.Data;
                Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                Console.WriteLine("DirectIO() : iObj = " + iObj[0]);
                

                // READ FROM EJ BY NUMBER 3100
                Console.WriteLine("DirectIO (READ FROM EJ BY NUMBER) 3100");
                strObj[0] = "01" + date1 + "0001" + "9999" + "1";

                dirIO = posCommonEJ.DirectIO(0, 3100, strObj);
                iData = dirIO.Data;
                Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                date1 = iObj[0].Substring(2, 6);
                Console.WriteLine("date: " + date1);
                lines += "date : " + date1 + "\r\n";

                FRN = iObj[0].Substring(8, 4);
                Console.WriteLine("Current Fiscal Receipt Number: " + FRN);
                lines += "Current Fiscal Receipt Number : " + FRN + "\r\n";

                LN = iObj[0].Substring(12, 4);
                Console.WriteLine("Line Sequence Number: " + LN);
                lines += "Line Sequence Number : " + LN + "\r\n";

                TEXT = iObj[0].Substring(16, 46);
                Console.WriteLine("EJ line text: " + TEXT);
                lines += "EJ line text : " + TEXT + "\r\n";

                lines += "\r\n";
                file.WriteLine(lines);

                lines = "";

                // READ FROM EJ BY DATE 
                Console.WriteLine("DirectIO (READ FROM EJ BY DATE) 3101");
                strObj[0] = "01" + date1 + date2 + "1";

                dirIO = posCommonEJ.DirectIO(0, 3101, strObj);
                iData = dirIO.Data;
                Console.WriteLine("DirectIO(): iData = " + iData);
                iObj = (string[])dirIO.Object;
                Console.WriteLine("DirectIO() : iObj = " + iObj[0]);

                string date = iObj[0].Substring(2, 6);
                Console.WriteLine("Acutal Date: " + date);
                lines += "date : " + date + "\r\n";

                FRN = iObj[0].Substring(8, 4);
                Console.WriteLine("Current Fiscal Receipt Number: " + FRN);
                lines += "Current Fiscal Receipt Number : " + FRN + "\r\n";

                LN = iObj[0].Substring(12, 4);
                Console.WriteLine("Line Sequence Number: " + LN);
                lines += "Line Sequence Number : " + LN + "\r\n";

                TEXT = iObj[0].Substring(16, 46);
                Console.WriteLine("EJ line text: " + TEXT);
                lines += "EJ line text : " + TEXT + "\r\n";

                file.WriteLine(lines);
                lines += "\r\n";





            }
            catch (Exception e)
            {
                Console.WriteLine("----- EXCEPTION -----");
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
    }

}
