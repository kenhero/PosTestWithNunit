using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;



namespace LibraryTests
{
    public class myXmlDocument : XDocument
    {
       public static XDocument d;
       public myXmlDocument()
       {
            d = new XDocument(
                new XElement("State",
                    new XElement("FiscalReceipt", //state that you want to analize
                        new XElement("functionName", "initFiscalDevice"),
                            new XElement("var", "FiscalPrinter"),
                            new XElement("var", "1"),
                        new XElement("functionName", "BeginFiscalReceipt"),
                            new XElement("var", "true"),
                            new XElement("var", "1"), //Method, params, number of iteration
                        new XElement("functionName", "PrintRecItem"),
                            new XElement("var", "ITEM"),
                            new XElement("var", "(decimal)10000"),
                            new XElement("var", "(int)1000"),
                            new XElement("var", "(int)3"),
                            new XElement("var", "(decimal)10000"),
                            new XElement("var", "1"),
                        new XElement("functionName", "PrintRecTotal"),
                            new XElement("var", "(decimal)10000"),
                            new XElement("var", "(decimal)10000"),
                            new XElement("var", "0CASH"),
                            new XElement("var", "10")
                                )
                            )
                        );

            d.Declaration = new XDeclaration("1.0", "utf-8", "true");
            //Console.WriteLine(d);

            d.Save("myConfiguration.xml");
        }
        
    }
}
