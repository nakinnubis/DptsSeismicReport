using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReportGenerator
{
    class Program
    {
        static void Main()
        {
            var pdreport = new HelperMethods();
            var dateYear = DateTime.Now;
            var intro = $"Petrodata Management Services Ltd 1999-{dateYear.Year}";
            var otherstyle = "========================================================";
            Console.WriteLine(otherstyle);
            Console.WriteLine(intro);
            Console.WriteLine(otherstyle);

            var softwaretype = $"Trancription Report and QC Generator Software from Diplomat Tape Copy {Environment.NewLine}";
            Console.WriteLine(softwaretype);
            Console.WriteLine($"You can run 3 type of command {Environment.NewLine}1.To run MTC4 copy report use : COPY COMMAND{Environment.NewLine}2.To run SEGD check report use: SEGD CHECK COMMAND{Environment.NewLine}3.To run SEGY check report use: SEGY CHECK COMMAND{Environment.NewLine} ");
            var getinputQuery = Console.ReadLine();
            var query = getinputQuery.Trim().ToUpper();
            switch (query)
            {
                case "SEGY CHECK COMMAND":
                    Console.WriteLine("You used a Segy Check Command");
                    DoSegYRep(pdreport);
                    //   Console.ReadKey();
                    break;
                case "SEGD CHECK COMMAND":
                    Console.WriteLine("You used a Segd Check Command");
                    DoSegDRep(pdreport);
                    // Console.ReadKey();
                    //Console.ReadKey();
                    break;
                case "COPY COMMAND":
                    DoTranscriptionReport(pdreport);
                    Console.ReadKey();
                    break;
                default:
                    Console.WriteLine("We Seems not to Understand your Request.. Things to do, 1. Try again, 2. Check you followed the Guide, 3.You can suggest it as a feature");
                    Console.ReadKey();
                    break;
            }
        }

        private static void DoTranscriptionReport(HelperMethods pdreport)
        {
            Console.Write("Specify the location of your .Scratch File: ");
            var g = Console.ReadLine();
            if (!CheckFolderNotExist(g))
            {
                if (!FileNotExist(g))
                {

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
                else
                {
                    Console.WriteLine("Folder doesn't conatin any recognized file format. This software only allow .SCRATCH, SEGYCHK & SEGDCHK");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Folder doesn't exist or empty ensure it contains file that match your query");
                Console.ReadKey();
            }

        }

        private static void DoSegDRep(HelperMethods helperMethods)
        {
            List<string> content;
            List<SegDTapeSummary> tapeSummaries = new List<SegDTapeSummary>();
            Console.Write("Specify the location of your .SEGDCHK File: ");
            var g = Console.ReadLine();
            if (!CheckFolderNotExist(g))
            {
                if (!FileNotExist(g))
                {
                    var file = Directory.GetFiles($"{g}",
                 "*.SEGDCHK",
                 SearchOption.AllDirectories);
                    foreach (var f in file)
                    {
                        content = File.ReadAllLines(f).ToList();
                        var sttt = helperMethods.TapeSummary(content);
                        tapeSummaries.AddRange(sttt);
                    }
                    helperMethods.CreateExcelReport(tapeSummaries);
                    Console.WriteLine("SEGD CHECK Report Successfully done!! Hurray");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Folder doesn't conatin any recognized file format. This software only allow .SCRATCH, SEGYCHK & SEGDCHK");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Folder doesn't exist or empty ensure it contains file that match your query");
                Console.ReadKey();
            }
        }
        private static void DoSegYRep(HelperMethods helperMethods)
        {
            List<string> content;
            List<SegYTapeSummary> tapeSummaries = new List<SegYTapeSummary>();
            Console.Write("Specify the location of your .SEGYCHK File: ");
            var g = Console.ReadLine();
            if (!CheckFolderNotExist(g))
            {
                if (!FileNotExist(g))
                {
                    var file = Directory.GetFiles($"{g}",
                    "*.SEGYCHK",
                    SearchOption.AllDirectories);
                    foreach (var f in file)
                    {
                        content = File.ReadAllLines(f).ToList();
                        var sttt = helperMethods.SegyTapeSummary(content);
                        tapeSummaries.AddRange(sttt);
                    }
                    helperMethods.CreateExcelReport(tapeSummaries);
                    Console.WriteLine("SEGY CHECK Report Successfully done!! Hurray");
                    Main();
                    
                }
                else
                {
                    Console.WriteLine("Folder doesn't conatin any recognized file format. This software only allow .SCRATCH, SEGYCHK & SEGDCHK");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Folder doesn't exist or empty ensure it contains file that match your query");
                Console.ReadKey();
            }

        }
        private static bool CheckFolderNotExist(string path)
        {
            if (string.IsNullOrEmpty(path)) return true;
            if (!Directory.Exists(path) && Directory.GetFiles(path).Length < 1)
                return true;
            return false;
        }
        private static bool FileNotExist(string path)
        {
            if (CheckFolderNotExist(path)) return true;
            var file = Directory.GetFiles($"{path}",
                "*.*",
                SearchOption.AllDirectories)
                .Where(f => f.ToUpper()
                .EndsWith(".SEGYCHK") ||
                f.ToUpper().EndsWith(".SEGDCHK") ||
                f.ToUpper().EndsWith(".SCRATCH")).Any();
            return false;
        }

    }
}