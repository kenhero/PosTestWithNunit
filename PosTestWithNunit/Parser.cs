using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace LibraryTests
{
    public class Parser
    {
        private XmlTextReader reader;
        public Parser()
        {
            //parsing della directory XmlFolder con tutti i file xml di test
            try
            {
                //string prova = Directory.GetCurrentDirectory();
                string[] fileArray = Directory.GetFiles(@"E:\tool\PosTestWithNunit\XmlFolder\", "*.xml", SearchOption.TopDirectoryOnly);

                foreach (string namefile in fileArray)
                {

                    reader = new XmlTextReader(namefile);

                    //Per ogni xml file mi genero un corrispettivo .txt parsato
                    string extension = Path.GetExtension(namefile);
                    string mytxtFile = Path.ChangeExtension(namefile, ".txt");

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

                                /*
                                  Console.Write("\n Name " + reader.Name);
                                  Console.Write("\n Local Name " + reader.LocalName);
                                  Console.WriteLine("\n Value " + reader.Value);
                                     Console.WriteLine("\n Depth " + reader.Depth);

                                     Console.WriteLine("\n Attribute Count " + reader.AttributeCount);
                                 */
                                if (reader.AttributeCount != 0)
                                {
                                    string lines = null;
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
                }
                reader.Close();
            }
            catch(Exception e)
            {
                CustomTests.log.Error("", e);
            }
        }


    }
}

