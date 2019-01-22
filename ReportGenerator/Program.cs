using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReportGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var pdreport = new HelperMethods();
            var dateYear = DateTime.Now;
            var intro = $"Petrodata Management Services Ltd 1999-{dateYear.Year}";
            var otherstyle = "========================================================";
            Console.WriteLine(otherstyle);
            Console.WriteLine(intro);
            Console.WriteLine(otherstyle);

            var softwaretype = "Trancription Report and QC Generator Software from Diplomat Tape Copy";
            Console.WriteLine(softwaretype);
            var getinputQuery = Console.ReadLine();
            var query = getinputQuery;
            switch (query)
            {
                case "SEGY CHECK COMMAND":
                    Console.WriteLine("You used a Segy Check Command");
                    Console.ReadKey();
                    break;
                case "SEGD CHECK COMMAND":
                    Console.WriteLine("You used a Segd Check Command");
                    Console.ReadKey();
                    break;
                case "COPY COMMAND":                    
                     DoTranscriptionReport(pdreport);
                    Console.ReadKey();
                    break;
                default:
                    Console.WriteLine("It Seems not to Understand your Request.. Things to do, 1. Try again, 2. Check you followed the Guide, 3.You can suggest it as a feature");
                    Console.ReadKey();
                    break;
            }            
        }

        private static void DoTranscriptionReport(HelperMethods pdreport)
        {
            Console.Write("Specify the location of your .Scratch File: ");
            var g = Console.ReadLine();
            //  Console.WriteLine(g);
            var file = Directory.GetFiles($"{g}",
                "*.SCRATCH",
                SearchOption.AllDirectories);
            List<string> content;
            List<string> tapes;
            string tape;
            var str = new List<Reports>();
            foreach (var f in file)
            {
                content = File.ReadAllLines(f).ToList();
                if (pdreport.CheckIfBatchCopy(f))
                {
                    if (pdreport.IsManyToOneBatchCopy(f))
                    {
                        tapes = pdreport.GetStringBeforeManyToOneOutput(content);
                        var ttapes = tapes;
                        var strBatch = pdreport.ManyToOneReport(content, ttapes);
                        str.AddRange(strBatch);
                    }
                    if (pdreport.IsOneToOneBatchCopy(f))
                    {
                        tapes = pdreport.GetStringBefore(content);
                        var strBatch = pdreport.GetFileStartAndEnd(content, tapes);
                        str.AddRange(strBatch);
                    }

                }
                if (!pdreport.CheckIfBatchCopy(f))
                {
                    tape = pdreport.GetStringBeforeSingle(content);
                    var sak = pdreport.GetFileStartAndEndSingleCopy(content, tape);
                    str.AddRange(sak);
                }
            }
            pdreport.CreateExcelReport(str);
            Console.Write("Done Successfully");
        }
    }
}