using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Schema;

namespace AppConsolePosTestWithNunit
{
    public class Parser
    {
        // something that will read the XML file
        private XmlReader reader = null;

        // define the settings that I use while reading the XML file.
        private XmlReaderSettings settings;
        //log4net member
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Parser()
        {

            //delete old txt parsed files
            string[] oldFiles = Directory.GetFiles(@"D:\Epson_Copia_Chiavetta_Gialla2\ToolAggiornato\PosTestWithNunit\XmlFolder\", "*.txt", SearchOption.TopDirectoryOnly);
            foreach(string namefile in oldFiles)
            {
                try
                {
                    File.Delete(namefile);
                }
                catch  { }
            }

            //parsing della directory XmlFolder con tutti i file xml di test
            string[] fileArray = Directory.GetFiles(@"D:\Epson_Copia_Chiavetta_Gialla2\ToolAggiornato\PosTestWithNunit\XmlFolder\", "*.xml", SearchOption.TopDirectoryOnly);
            foreach (string namefile in fileArray)
            {
                try
                {
                    // XSD
                    settings = new XmlReaderSettings();
                    settings.ValidationType = ValidationType.Schema;
                    settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
                    settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
                    settings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(this.ValidationEventHandle);

                    //Per ogni xml file mi genero un corrispettivo .txt parsato
                    string extension = Path.GetExtension(namefile);
                    string mytxtFile = Path.ChangeExtension(namefile, ".txt");

                    // validate the filewith the given setting.
                    // reader = new XmlTextReader(namefile);

                    reader = XmlReader.Create(namefile, settings);

                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element: // The node is an element.

                                /*
                                  Console.Write("\n Name " + reader.Name);
                                  Console.Write("\n Local Name " + reader.LocalName);
                                  Console.WriteLine("\n Value " + reader.Value);
                                     Console.WriteLine("\n Depth " + reader.Depth);

                                     Console.WriteLine("\n Attribute Count " + reader.AttributeCount);
                                 */
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
                                                   /*
                                                   Console.WriteLine("\n " + reader.Value);
                                                   for (int i = 0; i < reader.AttributeCount; ++i)
                                                   { reader.MoveToAttribute(i); }
                                                   reader.MoveToElement();
                                                   reader.MoveToFirstAttribute();
                                                   reader.MoveToNextAttribute();
                                                   */
                                break;
                            case XmlNodeType.Attribute: //Display the attribute of the element
                                                        /* 
                                                         Console.WriteLine("\n " + reader.Value);
                                                         * */
                                break;
                        }
                    }
                    log.Info("Validation of file " + namefile + " Passed");
                    
                }
                catch (Exception ex)
                {
                    log.Error("Error validating file " + namefile + ex.Message);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            } 
        }

        private void ValidationEventHandle(object sender, ValidationEventArgs arg)
        {
            //If we are here, it's because something is going wrong with my XML.
            log.Error("\r\n\t Validation XML failed: " + arg.Message);

            // throw an exception.
            throw new Exception("Validation XML failed: " + arg.Message);
        }

       
    }
}

