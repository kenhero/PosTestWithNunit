# PosTestWithNunit
Come funziona il software di testing della Stampante Fiscale:

Il progetto Visual Studio "PosTestWithNunit" è composto da 6 progetti,rappresentanti la intera solution e sono:

1. AppConsolePosTestWithNunit
2. ElectronicJournal
3. FiscalDcoument.Library
4. FiscalReceipt.Library
5. PosTestWithNunit
6. Report.Library

Il primo progetto rappresenta una application consolle (.EXE) ,è l'unico progetto eseguibile che contiene l'integrazione del framework di testing Nunit e la libreria di loggin log4Net.
Esso è composto da due file i cui macro compiti sono:
file parser.cs effettua il parser di una cartella ad hoc (XmlFolder) contente tutti file xml che contengono a loro volta un lista di tutti i metodi del driver .NET o dei metodi più complessi implementati che effettuano una qualche operazione sulla stampante. 
Tali operazioni possono essere semplici operazioni (BeginFiscalReceipt per esempio) , ad operazioni piu' complesse come una vendita, un annullo ,un reso o uno scontrino nullo,o una commutazione da MF ad RT

Il file Program.cs contiene un metodo , DynamicTest , che legge i file xml parsati e dinamicamente chiama i metodi del progetto FiscalReceipt.Library contenente i vari metodi che effettuano le varie operazioni sulla stampante. Dopo aver chiamato il relativo metodo si occupa di fare unit test,ossia di testare,ad esempio, i totalizzatori prima e dopo la chiamata del metodo.
Un file di log (Mylog.log) all'interno della cartella generale memorizza al suo interno ogni informazione relativa all'andamento delle operazioni sulla stampante e alla presenza o meno di eventuali exception o errori durante la esecuzione delle operazioni o di eventuali errori in fase di Unit Test.

Il progetto 2 (dll) è stata parzialmente implementata,ossia è stata realizzato il metodo che legge i vari contatori e la memoria dell' EJ.

Il progetto 3 (dll) per ora è una semplice interfaccia relativa allo stato Document in cui puo' trovarsi la stampante.

Il progetto 4 (dll) è il progetto piu' importante in quanto comprende il core del tool ,ossia l'implementazione di 5 classi suddivise in base alla tipologia di operazioni che effettuano sulla stampante:
1. FiscalReceipt Class
2. VatManager Class
3. RetrieveData Class
4. VatRecord Class
5. GeneralCounter Class


Analisi FiscalReceipt Class
Tale classe implementa i seguenti metodi:
1. initFiscalDevice //Method to initialize POS Device
2. closeFiscalDevice         //Method to Close POS Device (fondamentale per test multipli, da inserire come ultimo metodo nell 'xml
3. testFiscalReceiptClass //Method to test the original FiscalReceiptClass
4. BeginFiscalReceipt (operazione atomica, fa la beginFiscalReceipt)
5. PrintRecItem (Fa una vendita di 3 oggetti)
6. PrintRecTotal         //PrintRecTotal,chiude lo scontrino
7. ResetPrinter         //ResetPrinter
8. checkRtStatus         //all'interno c'è pure temporaneamente il check sull EJ da eliminare oppure da isolare con un metodo apposta
9. commuteMode         //DirectIO 4014 63
        //Comando SET Flag (4014) Flag = 63 Demo Mode VAL = 1/0 
        //Switch from Demo Rt to MF and viceversa
        //Update: inserito anche il comando per switchare in automatico in RT (3333 + 1433 + X + Contante)
10. readFromEJ         //Letture varie dall' Electronic Journal (bisogna farle dalla classe posCommonFP non da posCommonEJ

Analisi VatManager Class
Tale classe implementa i seguenti metodi:

1. GetDepParam //DirectIO 4202 //Get Department Parameters
2. getVatTableEntry //DirectIO 4205  //Legge l'IVA associata alla VAT Table Entry(1 to 9)
3. setVatTableEntry         //DirectIO 4005          //Scrive l'IVA associata alla VAT Table Entry(1 to 9 ovviamente,leggi directIO 4205)
4. setDepPar         //DirectIO 4002         //Set Department Parameters

Analisi RetrieveData Class
1. getFiscalReceiptNumber  //DirectIO 1070 //Get Fiscal Receipt
2. getDailyData         //DirectIO 2050 //Get Daily Data         //Legge le statistiche giornaliere e i registri interni. Per ora chiedo solo il parametro 24

Analisi VatRecord Class
Tale classe implementa i seguenti metodi:
1. SetVatCounter         //ottiene le info sui contatori in memoria relativi alle aliquote IVA (lordo e netto)
2. SetVatCounters         //SetVatCounters to xml file
3. GetVatCounter         //effettua una query sull'XML dei Vat Counter per cercare il dato esplicitamente richiesto
4. 

Analisi GeneralCounter Class
Tale classe implementa i seguenti metodi:
1. GetObjectData         // Serialization function (Stores Object Data in File)
2. GeneralCounter         // The deserialize function (Removes Object Data from File)
3. GetGeneralCounter         // Deserialize from XML to the object
4. SetGeneralCounter         //Metodo che aggiorna i contatori generali prelevando i dati dalla memoria fiscale


I metodi sono raggruppati all'interno di ogni classe secondo una "compatibilità e affinità comportamentale" . I nomi e i commenti relativi sono abbastanza esaustivi quanto a spiegazione su cio' che fanno.







Il progetto PosTestWithNunit è una dll contente il framework di test Nunit che ,tecnicamente, non ha bisogno di funzionare tramite un application consolle ma puo' funzionare in maniera indipendente chiamandolo direttamente dal menù Test->Run o Debug dell'ide di Visual Studio ma per ora è in fase di accantonamento perchè mi sono concentrato a lavorare sulla versione con l'eseguibile,ossia il progetto AppConsolePosTestWithNunit.

Il progetto Report.Library è una semplice interfaccia non ancora implementata relativa allo stato Report della Fiscal Printer.

L'obiettivo finale del tool sarà di testare ogni tipo di comportamento della stampante (quindni switch da uno stato ad un altro ) ,effettuare tutte le possibili operazioni compatibili con lo stato in cui si trova e testare sulla memoria fiscale o l'electronic journal la coerenza tra le operazioni effettuate e i dati presenti in memoria.
Per questo tipo di test ci viene appunto incontro il framework Nunit e le sue features.
