using CsvHelper;
using ReportGenerator.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ReportGenerator
{
    public class HelperMethods : IReportRepository
    {
        private string commandsgy = "SEGY CHECK COMMAND";
        private string commandsgd = "SEGD CHECK COMMAND";
        private string commandcopy = "COPY COMMAND";

        public bool BatchChecker(string x, string tape)
        {
            try
            {
                if (x.Contains($"BATCHCOPY - Copying input tape "))
                {
                    return true;
                }
                if (x.Contains($"BATCHCOPY - Copying tape {tape}"))
                {
                    return true;
                }
                return x.Contains("Executing BATCHCOPY command -");
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }

        }

        public bool CheckIfBatchCopy(string file)
        {
            try
            {
                var contents = File.ReadLines(file).Contains("Command: Batchcopy");
                if (contents)
                {
                    return true;
                }
                return false;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public bool CheckIfSegDData(List<string> str)
        {
            try
            {
                var d = str.Select(x => !x.StartsWith("Length =   3200")).FirstOrDefault();
                if (d)
                {
                    return false;
                }
                return true;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }

        }

        public bool CheckIsSegDCheck(string file)
        {
            throw new NotImplementedException();
        }

        public bool CheckIsSegYCheck(string file)
        {
            throw new NotImplementedException();
        }

        public string CheckReportType(string query)
        {
            try
            {
                query = query.ToUpper();
                if (query == "SGY CHECK" || query == "SGYC" || query == "SEGY CHECK")
                {
                    return commandsgy;
                }
                if (query == "SGD CHECK" || query == "SGDC" || query == "SEGD CHECK")
                {
                    return commandsgd;
                }
                return commandcopy;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public void CreateExcelReport(List<Reports> reports)
        {
            try
            {
                var data = reports.ToArray();
                Console.WriteLine("if Report name for the file is not specified the default settings will be used!");
                Console.Write("Enter The Directory where you want to save your report to with file name:  ");
                var outdir = Console.ReadLine();
                //  if(outdir.EndsWith(".csv"))
                using (var mem = new MemoryStream())
                using (var writer = new StreamWriter(mem))
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.Configuration.Delimiter = ",";
                    csvWriter.WriteField("Serial Number");
                    csvWriter.WriteField("Tape Id");
                    csvWriter.WriteField("Number of Files");
                    csvWriter.WriteField("File Number Start");
                    csvWriter.WriteField("File Number End");
                    csvWriter.WriteField("Data Size");
                    csvWriter.WriteField("File Format");
                    csvWriter.NextRecord();
                    int i = 0;
                    foreach (var project in data)

                    {
                        i++;
                        csvWriter.WriteField(i);
                        csvWriter.WriteField(project.TapeId);
                        csvWriter.WriteField(project.NumberOfFiles);
                        csvWriter.WriteField(project.FileNumStart);
                        csvWriter.WriteField(project.FileNumEnd);
                        csvWriter.WriteField(project.DataSize);
                        csvWriter.WriteField(project.FileFormat);
                        csvWriter.NextRecord();
                    }

                    writer.Flush();
                    var result = Encoding.UTF8.GetString(mem.ToArray());
                    File.WriteAllText($"{outdir}", result);
                }
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }

        }

        public List<SegDCheckExtract> EssembleShotPointExtract(string file)
        {
            throw new NotImplementedException();
        }

        public List<string> GetFileSize(List<string> str)
        {
            try
            {
                var filesize = str.Where(c => c.Contains("Transfer total")).ToList();
                return filesize;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public List<Reports> GetFileStartAndEnd(List<string> str, List<string> TapeNo)
        {
            try
            {
                var _tapeNo = TapeNo.ToArray();
                List<string> result = new List<string>();
                List<Reports> reports = new List<Reports>();
                string firstessemble;
                string lastessemble;
                string filesize;
                int startpos;
                int endpos;
                int nosfile;
                int filestart;
                int fileend;
                Reports _reports = new Reports();
                if (CheckIfSegDData(str))
                {
                    foreach (var tape in _tapeNo)
                    {
                        var st = str.SkipWhile(x => !BatchChecker(x, tape)) // skips everything before 
                            .Skip(1) // and <rs:data> itself
                            .TakeWhile(x => !x.Contains($"BATCHCOPY - Unloading input tape {tape}")) // and take up to </rs:data>
                            .ToList();
                        firstessemble = st.Where(c => c.Contains("Copied SEGY ensemble")).FirstOrDefault().Replace("Copied SEGY ensemble ", "");
                        firstessemble = firstessemble.Substring(0, firstessemble.LastIndexOf(", f"));
                        lastessemble = st.Where(c => c.Contains("Copied SEGY ensemble")).LastOrDefault().Replace("Copied SEGY ensemble ", "");
                        lastessemble = lastessemble.Substring(0, lastessemble.LastIndexOf(", f"));
                        filestart = int.Parse(firstessemble);
                        fileend = int.Parse(lastessemble);
                        _reports.FileNumStart = filestart;
                        _reports.FileNumEnd = fileend;
                        _reports.TapeId = tape;
                        _reports.FileFormat = "SEGY";
                        filesize = st.Where(c => c.Contains("Transfer total")).FirstOrDefault();
                        startpos = filesize.IndexOf("Mb,");
                        endpos = filesize.LastIndexOf("G") + 1;
                        filesize = filesize.Substring(startpos + 3).Trim();
                        nosfile = int.Parse(lastessemble) - int.Parse(firstessemble);
                        _reports.NumberOfFiles = nosfile;
                        _reports.DataSize = filesize;
                        reports.Add(_reports);
                    }
                }
                foreach (var tape in _tapeNo)
                {
                    var st = str.SkipWhile(x => !BatchChecker(x, tape)) // skips everything before 
                        .Skip(1) // and <rs:data> itself
                        .TakeWhile(x => !x.Contains($"BATCHCOPY - Unloading input tape {tape}")) // and take up to </rs:data>
                        .ToList();
                    firstessemble =
                        st.Where(c =>
                                c.Contains("Length = ") || c.Contains("Length =   2432") && !c.Contains("Length =   7680 "))
                            .FirstOrDefault().Replace("Length = ", "").TrimStart();
                    firstessemble = firstessemble.Substring(0, firstessemble.LastIndexOf("  ID = "));
                    lastessemble = st.Where(c => c.Contains("Length = ")).LastOrDefault().Replace("Length = ", "").TrimStart();
                    lastessemble = lastessemble.Substring(0, lastessemble.LastIndexOf("  ID = "));
                    filesize = st.Where(c => c.Contains("Transfer total")).FirstOrDefault();
                    startpos = filesize.IndexOf("Mb,");
                    endpos = filesize.LastIndexOf("G") + 1;
                    filesize = filesize.Substring(startpos + 3).Trim();
                    nosfile = int.Parse(lastessemble) - int.Parse(firstessemble);
                    filestart = int.Parse(firstessemble);
                    fileend = int.Parse(lastessemble);
                    _reports = new Reports
                    {
                        TapeId = tape,
                        NumberOfFiles = nosfile,
                        FileNumStart = filestart,
                        FileNumEnd = fileend,
                        DataSize = filesize,
                        FileFormat = "SEGD"
                    };
                    reports.Add(_reports);
                }

                return reports;
            }
            catch (Exception)
            {
                throw new ReportErrorHandler();
            }
        }

        public List<Reports> GetFileStartAndEndSingleCopy(List<string> str, string TapeNo)
        {
            try
            {

                var _tapeNo = TapeNo.TrimStart();
                List<string> result = new List<string>();
                List<Reports> reports = new List<Reports>();
                List<string> st;
                string firstessemble;
                string lastessemble = null;
                string filesize = null;
                int startpos;
                int endpos;
                int nosfile = 0;
                int filestart = 0;
                int fileend = 0;
                Reports _reports = new Reports();
                if (!CheckIfSegDData(str))
                {
                    st = str.SkipWhile(x => !x.Contains("Command: Copy to deof")) // skips everything before 
                        .Skip(1) // and <rs:data> itself
                        .TakeWhile(x => !x.Contains($"Command: Unload nowait input")) // and take up to </rs:data>
                        .ToList();
                    firstessemble = st.FirstOrDefault(c => c.Contains("Length = ") || c.Contains("Length =   2432") && !c.Contains("Length =   7680 "))?.Replace("Length = ", "").TrimStart();
                    if (firstessemble != null)
                    {
                        firstessemble = firstessemble.Substring(0,
                            firstessemble.LastIndexOf("  ID = ", StringComparison.Ordinal));
                        lastessemble = st.LastOrDefault(c => c.Contains("Length = "))?.Replace("Length = ", "").TrimStart();
                        if (lastessemble != null)
                        {
                            lastessemble = lastessemble.Substring(0,
                                lastessemble.LastIndexOf("  ID = ", StringComparison.Ordinal));
                            filesize = st.FirstOrDefault(c => c.Contains("Transfer total"));
                            if (filesize != null)
                            {
                                startpos = filesize.IndexOf("Mb,", StringComparison.Ordinal);
                                endpos = filesize.LastIndexOf("G", StringComparison.Ordinal) + 1;
                                filesize = filesize.Substring(startpos + 3).Trim();
                                nosfile = int.Parse(lastessemble) - int.Parse(firstessemble);
                                filestart = int.Parse(firstessemble);
                                fileend = int.Parse(lastessemble);
                                _reports = new Reports
                                {
                                    TapeId = _tapeNo,
                                    NumberOfFiles = nosfile,
                                    FileNumStart = filestart,
                                    FileNumEnd = fileend,
                                    DataSize = filesize,
                                    FileFormat = "SEGD"
                                };
                            }
                        }
                    }

                    reports.Add(_reports);
                }
                else
                {
                    st = str.SkipWhile(x => !x.Contains("Length =   3200")) // skips everything before 
                        .Skip(1) // and <rs:data> itself
                        .TakeWhile(x => !x.Contains($"Command: Unload nowait input")) // and take up to </rs:data>
                        .ToList();
                    firstessemble = st.FirstOrDefault(c => c.Contains("Length = "))?.Replace("Length = ", "").TrimStart();
                    if (firstessemble != null)
                    {
                        firstessemble = firstessemble.Substring(0, firstessemble.LastIndexOf("  ID = ", StringComparison.Ordinal));
                        lastessemble = st.LastOrDefault(c => c.Contains("Length = "))
                            ?.Replace("Length = ", "")
                            .TrimStart();
                        if (lastessemble != null)
                        {
                            lastessemble = lastessemble.Substring(0,
                                lastessemble.LastIndexOf("  ID = ", StringComparison.Ordinal));
                            filesize = st.FirstOrDefault(c => c.Contains("Transfer total"));
                            if (filesize != null)
                            {
                                startpos = filesize.IndexOf("Mb,", StringComparison.Ordinal);
                                endpos = filesize.LastIndexOf("G", StringComparison.Ordinal) + 1;
                                filesize = filesize.Substring(startpos + 3).Trim();
                            }

                            nosfile = int.Parse(lastessemble) - int.Parse(firstessemble);
                        }

                        filestart = int.Parse(firstessemble);
                    }

                    if (lastessemble != null) fileend = int.Parse(lastessemble);
                    _reports = new Reports
                    {
                        TapeId = _tapeNo,
                        NumberOfFiles = nosfile,
                        FileNumStart = filestart,
                        FileNumEnd = fileend,
                        DataSize = filesize,
                        FileFormat = "SEGY"
                    };
                    reports.Add(_reports);
                }
                return reports;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }

        }

        public string GetMyTapeNoBatchCopy(string arg)
        {
            try
            {
                arg = arg.Replace("BATCHCOPY - Copying tape ", "");
                arg = arg.Substring(0, arg.IndexOf("{"));
                return arg;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public string GetMyTapeNoBatchCopyManyToOneOutput(string arg)
        {
            try
            {
                arg = arg.Replace("BATCHCOPY - Copying input tape ", "");
                arg = arg.Substring(0, arg.IndexOf("{"));
                return arg;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public string GetMyTapeNoSingleCopy(string arg)
        {
            try
            {
                arg = arg.Replace("Reel id:", "");
                // arg = arg.Substring (0, arg.LastIndexOf (""));
                return arg;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public List<string> GetStringBefore(List<string> str)
        {
            try
            {
                const string strcheck = "BATCHCOPY - Copying tape ";
                str = str.Where(c => c.Contains(strcheck)).Select(GetMyTapeNoBatchCopy).ToList();
                return str;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public List<string> GetStringBeforeManyToOneOutput(List<string> str)
        {
            try
            {
                const string strcheck = "BATCHCOPY - Copying input tape ";
                str = str.Where(c => c.Contains(strcheck)).Select(GetMyTapeNoBatchCopyManyToOneOutput).ToList();
                return str;
            }
            catch (Exception)
            {
                throw new ReportErrorHandler();
            }

        }

        public string GetStringBeforeSingle(List<string> str)
        {
            try
            {
                const string strcheck = "Reel id:";
                var strs = str.Where(c => c.Contains(strcheck)).Select(GetMyTapeNoSingleCopy).FirstOrDefault();
                return strs;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public bool IsManyToManyBatchCopy(string file)
        {
            try
            {
                if (File.ReadLines(file).Contains("Executing BATCHCOPY command - Many inputs to Many output"))
                {
                    return true;
                }
                return false;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public bool IsManyToOneBatchCopy(string file)
        {
            try
            {
                if (File.ReadLines(file).Contains("Executing BATCHCOPY command - Many inputs to One output"))
                {
                    return true;
                }
                return false;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public bool IsOneToManyBatchCopy(string file)
        {
            try
            {
                if (File.ReadLines(file).Contains("Executing BATCHCOPY command - Many inputs to One output"))
                {
                    return true;
                }
                return false;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public bool IsOneToOneBatchCopy(string file)
        {
            try
            {
                if (File.ReadLines(file).Contains("Executing BATCHCOPY command - One input to One output"))
                {
                    return true;
                }
                return false;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public bool IsSequential(int[] array)
        {
            return array.OrderBy(a => a).Zip(array.Skip(1), (a, b) => (a + 1) == b).All(x => x);
        }

        public List<Reports> ManyToOneReport(List<string> reports, List<string> tapes)
        {
            try
            {
                var report = new List<Reports>();
                foreach (var a in tapes)
                {
                    var st = reports.SkipWhile(x => !x.Contains($"BATCHCOPY - Copying input tape {a}")).Skip(1) // and <rs:data> itself
                        .TakeWhile(x => !x.Contains($"BATCHCOPY - Unloading input tape {a}")) // and take up to </rs:data>
                        .ToList();
                    var firstessemble = st.FirstOrDefault(c => c.Contains("Copied SEGY ensemble"))?.Replace("Copied SEGY ensemble ", "");
                    if (firstessemble != null)
                    {
                        firstessemble = firstessemble.Substring(0, firstessemble.LastIndexOf(", f", StringComparison.Ordinal));
                        var lastessemble = st.LastOrDefault(c => c.Contains("Copied SEGY ensemble"))
                            ?.Replace("Copied SEGY ensemble ", "");
                        if (lastessemble != null)
                        {
                            lastessemble = lastessemble.Substring(0, lastessemble.LastIndexOf(", f", StringComparison.Ordinal));
                            var filestart = int.Parse(firstessemble);
                            var fileend = int.Parse(lastessemble);
                            //_reports.FileNumStart = filestart;
                            // _reports.FileNumEnd = fileend;
                            var filesize = st.FirstOrDefault(c => c.Contains("Transfer total"));
                            if (filesize != null)
                            {
                                var startpos = filesize.IndexOf("Mb,", StringComparison.Ordinal);
                                filesize = filesize.Substring(startpos + 3).Trim();
                                var nosfile = int.Parse(lastessemble) - int.Parse(firstessemble);
                                var rep = new Reports
                                {
                                    TapeId = a,
                                    NumberOfFiles = nosfile,
                                    FileNumStart = filestart,
                                    FileNumEnd = fileend,
                                    FileFormat = "SEGY",
                                    DataSize = filesize
                                };
                                report.Add(rep);
                            }
                        }
                    }
                }
                return report;
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public List<SegDTapeSummary> TapeSummary(List<string> file)
        {
            List<string> data;
            var st = file.SkipWhile(x => !x.Contains($"D P T S   S E G D C H K   T A P E   S U M M A R Y")).Skip(5) // and <rs:data> itself
                      .TakeWhile(x =>!x.Contains($" -------------------") || x.Contains($"SEGDCHK - DPTS SEGD Format") || x.Contains($"Finished SEGDCHK")) // and take up to </rs:data>
                      .ToList();
            var fileSize = file.FirstOrDefault(c => c.Contains("Completed Physical reel, Transfer total")).Replace("Completed Physical reel, Transfer total ", "");
            fileSize = fileSize.Split(',').Select(c => c).LastOrDefault();
            data = st;
            if (isSegDContainsMoreThanOne(data))
            {
                var finaloutput = new List<SegDTapeSummary>();
                var tempdata = new List<SegDTapeSummary>();
                var k = ExtractInfoFromIsSegDContainsMoreThanOne(data);
                foreach (var item in k)
                {
                    var it = item.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s));
                    var iitem = string.Join(",", it);
                    if (!string.IsNullOrWhiteSpace(iitem))
                    {

                        var outputParse = OutputParse(iitem);
                        outputParse.FileSize = fileSize;
                        outputParse.FileType = "SEGD";
                        tempdata.Add(outputParse);
                    }
                }
                return tempdata;
            }
            else
            {
                List<SegDTapeSummary> datas = new List<SegDTapeSummary>();
                foreach (var item in data)
                {
                    var it = item.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s));
                    var iitem = string.Join(",", it);
                    if (!string.IsNullOrWhiteSpace(iitem))
                    {
                        var outputParse = OutputParse(iitem);
                        outputParse.FileSize = fileSize;
                        outputParse.FileType = "SEGD";
                        datas.Add(outputParse);
                    }

                    //  Console.WriteLine(iitem);
                }
                return datas;
            }
            //try
            //{


            //}
            //          catch (ReportErrorHandler e)
            //          {
            //              Console.WriteLine(e.Message);
            ////throw new NotImplementedException();
            //             // throw;
            //          }

        }

        public SegDTapeSummary OutputParse(string output)
        {
            var outP = output.Split(',').Select(c => c).ToList();
            return new SegDTapeSummary
            {
                Reel = outP[0],
                FirstFFID = outP[2],
                LastFFID = outP[3],
                FFIDCount = outP[4],
                NoOfTrace = outP[5]
            };
            //if (string.IsNullOrWhiteSpace(output)) 

        }
        public SegYTapeSummary OutputParseSegY(string output)
        {
            var outP = output.Split(',').Select(c => c).ToList();
            return new SegYTapeSummary
            {
                Reel = outP[0],
                FirstFFID = outP[2],
                LastFFID = outP[3],
                FFIDCount = outP[4],
                TraceCount = outP[6]
            };
            //if (string.IsNullOrWhiteSpace(output)) 

        }

        public void CreateExcelReport(List<SegDTapeSummary> reports)
        {
            try
            {
                var reelid = GetReelIdsFromIsMoreThanOne(reports);
                var res = IsMoreThanOneFinalized(reports, reelid);
                // var  finaloutput.AddRange(res);                
                var data = res.ToArray();
                Console.WriteLine("if Report name for the file is not specified the default settings will be used!");
                Console.Write("Enter The Directory where you want to save your report to with file name:  ");
                var outdir = Console.ReadLine();
                //  if(outdir.EndsWith(".csv"))
                using (var mem = new MemoryStream())
                using (var writer = new StreamWriter(mem))
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.Configuration.Delimiter = ",";
                    csvWriter.WriteField("Serial Number");
                    csvWriter.WriteField("Reel ");
                    csvWriter.WriteField("First FFID");
                    csvWriter.WriteField("Last FFID");
                    csvWriter.WriteField("FFID COUNT");
                    csvWriter.WriteField("Trace COUNT");
                    csvWriter.WriteField("FILE SIZE");
                    csvWriter.WriteField("File Format");
                    csvWriter.NextRecord();
                    int i = 0;
                    foreach (var project in data)

                    {
                        i++;
                        csvWriter.WriteField(i);
                        csvWriter.WriteField(project.Reel);
                        csvWriter.WriteField(project.FirstFFID);
                        csvWriter.WriteField(project.LastFFID);
                        csvWriter.WriteField(project.FFIDCount);
                        csvWriter.WriteField(project.NoOfTrace);
                        csvWriter.WriteField(project.FileSize);
                        csvWriter.WriteField(project.FileType);
                        csvWriter.NextRecord();
                    }

                    writer.Flush();
                    var result = Encoding.UTF8.GetString(mem.ToArray());
                    File.WriteAllText($"{outdir}", result);
                }
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public void CreateExcelReport(List<SegYTapeSummary> reports)
        {
            try
            {
                var reelid = GetReelIdsFromIsMoreThanOne(reports);
                var res = IsMoreThanOneFinalized(reports, reelid);
                // var  finaloutput.AddRange(res);                
                var data = res.ToArray();
                Console.WriteLine("if Report name for the file is not specified the default settings will be used!");
                Console.Write("Enter The Directory where you want to save your report to with file name:  ");
                var outdir = Console.ReadLine();
                //  if(outdir.EndsWith(".csv"))
                using (var mem = new MemoryStream())
                using (var writer = new StreamWriter(mem))
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.Configuration.Delimiter = ",";
                    csvWriter.WriteField("Serial Number");
                    csvWriter.WriteField("Reel ");
                    csvWriter.WriteField("First FFID");
                    csvWriter.WriteField("Last FFID");
                    csvWriter.WriteField("FFID COUNT");
                    csvWriter.WriteField("Trace COUNT");
                    csvWriter.WriteField("FILE SIZE");
                    csvWriter.WriteField("File Format");
                    csvWriter.NextRecord();
                    int i = 0;
                    foreach (var project in data)

                    {
                        i++;
                        csvWriter.WriteField(i);
                        csvWriter.WriteField(project.Reel);
                        csvWriter.WriteField(project.FirstFFID);
                        csvWriter.WriteField(project.LastFFID);
                        csvWriter.WriteField(project.FFIDCount);
                        csvWriter.WriteField(project.TraceCount);
                        csvWriter.WriteField(project.FileSize);
                        csvWriter.WriteField(project.FileType);
                        csvWriter.NextRecord();
                    }

                    writer.Flush();
                    var result = Encoding.UTF8.GetString(mem.ToArray());
                    File.WriteAllText($"{outdir}", result);
                }
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
        }

        public bool isSegDContainsMoreThanOne(List<string> st)
        {
            if (st != null)
            {
                if (st.Count(c => c.Contains("-1")) == 1 || st.Count(c => c.Contains("-1")) > 1 || st.Count > 1)
                {
                    return true;
                }

            }
            return false;
        }
        public List<string> ExtractInfoFromIsSegDContainsMoreThanOne(List<string> args)
        {
            var filterhelp = new List<SegDTapeSummary>();

            var str = args.Skip(1).TakeWhile(c => !string.IsNullOrWhiteSpace(c)).ToList();
            return str;

        }
        public List<string> GetReelIdsFromIsMoreThanOne(List<SegDTapeSummary> dTapeSummaries)
        {
            var reelid = dTapeSummaries.Select(c => c.Reel).Distinct().ToList();
            return reelid;
        }
        public List<string> GetReelIdsFromIsMoreThanOne(List<SegYTapeSummary> dTapeSummaries)
        {
            var reelid = dTapeSummaries.Select(c => c.Reel).Distinct().ToList();
            return reelid;
        }
        public List<SegDTapeSummary> IsMoreThanOneFinalized(List<SegDTapeSummary> dTapeSummaries, List<string> reelid)
        {
            var dTapeSummary = new List<SegDTapeSummary>();
            foreach (var reel in reelid)
            {
                var dts = dTapeSummaries.Select(c => new SegDTapeSummary
                {
                    Reel = reel,
                    FFIDCount = $"{FFIDSum(dTapeSummaries, reel)}",
                    NoOfTrace = $"{TraceSum(dTapeSummaries, reel)}",
                    FirstFFID = $"{FidSelector(dTapeSummaries, reel)}",
                    LastFFID = $"{LastFidSelector(dTapeSummaries, reel)}",
                    FileSize = $"{FileSizeSelector(dTapeSummaries, reel)}",
                    FileType = c.FileType

                }).Distinct().FirstOrDefault();
                if (!dTapeSummary.Contains(dts))
                {
                    dTapeSummary.Add(dts);
                }
            }
            return dTapeSummary;
        }
        public List<SegYTapeSummary> IsMoreThanOneFinalized(List<SegYTapeSummary> dTapeSummaries, List<string> reelid)
        {
            var dTapeSummary = new List<SegYTapeSummary>();
            foreach (var reel in reelid)
            {
                var dts = dTapeSummaries.Select(c => new SegYTapeSummary
                {
                    Reel = reel,
                    FFIDCount = $"{FFIDSum(dTapeSummaries, reel)}",
                    TraceCount = $"{TraceSum(dTapeSummaries, reel)}",
                    FirstFFID = $"{FidSelector(dTapeSummaries, reel)}",
                    LastFFID = $"{LastFidSelector(dTapeSummaries, reel)}",
                    FileSize = $"{FileSizeSelector(dTapeSummaries, reel)}",
                    FileType = c.FileType

                }).Distinct().FirstOrDefault();
                if (!dTapeSummary.Contains(dts))
                {
                    dTapeSummary.Add(dts);
                }
            }
            return dTapeSummary;
        }
        private static string FidSelector(List<SegDTapeSummary> dTapeSummaries, string reel)
        {
            return dTapeSummaries.Where(rel => rel.Reel == reel && rel.FirstFFID !="-1").Select(sc => sc.FirstFFID).FirstOrDefault().ToString();
        }
        private static string FidSelector(List<SegYTapeSummary> dTapeSummaries, string reel)
        {
            return dTapeSummaries.Where(rel => rel.Reel == reel && rel.FirstFFID != "-1").Select(sc => sc.FirstFFID).FirstOrDefault().ToString();
        }
        private static string FileSizeSelector(List<SegDTapeSummary> dTapeSummaries, string reel)
        {
            return dTapeSummaries.Where(rel => rel.Reel == reel).Select(sc => sc.FileSize).FirstOrDefault().ToString();
        }
        private static string FileSizeSelector(List<SegYTapeSummary> dTapeSummaries, string reel)
        {
            return dTapeSummaries.Where(rel => rel.Reel == reel).Select(sc => sc.FileSize).FirstOrDefault().ToString();
        }
        private static string LastFidSelector(List<SegDTapeSummary> dTapeSummaries, string reel)
        {
            return dTapeSummaries.Where(rel => rel.Reel == reel).Select(sc => sc.LastFFID).LastOrDefault().ToString();
        }
        private static string LastFidSelector(List<SegYTapeSummary> dTapeSummaries, string reel)
        {
            return dTapeSummaries.Where(rel => rel.Reel == reel).Select(sc => sc.LastFFID).LastOrDefault().ToString();
        }
        private static double TraceSum(List<SegDTapeSummary> dTapeSummaries, string reel)
        {
            return dTapeSummaries.Where(k => k.Reel == reel).Select(i => double.Parse(i.NoOfTrace)).Sum();
        }
        private static double TraceSum(List<SegYTapeSummary> dTapeSummaries, string reel)
        {
            return dTapeSummaries.Where(k => k.Reel == reel).Select(i => double.Parse(i.TraceCount)).Sum();
        }

        private static double? FFIDSum(List<SegDTapeSummary> dTapeSummaries, string reel)
        {
            return dTapeSummaries.Where(k => k.Reel == reel).Select(i => DoubleParser(i)).Sum();
        }
        private static double? FFIDSum(List<SegYTapeSummary> dTapeSummaries, string reel)
        {           
            return dTapeSummaries.Where(k => k.Reel == reel).Select(i => DoubleParser(i)).Sum();
        }

        private static double? DoubleParser(dynamic i)
        {
            double number;
            double defaultval = 0.0;
            return double.TryParse(i.FFIDCount, out number) ? number : defaultval;
            //if ()
            //{
            //    return ;
            //}
            //else
            //{
            //   return ;
            //}
        }

        public List<SegYTapeSummary> SegyTapeSummary(List<string> file)
        {
            try
            {
                List<string> data;
                var st = file.SkipWhile(x => !x.Contains($"D P T S   S E G Y C H K   T A P E   S U M M A R Y")).Skip(5) // and <rs:data> itself
                          .TakeWhile(x => !x.Contains($"  ----------------------") ||x.Contains($"SEGYCHK - DPTS SEGY Format Checking Program") || x.Contains($"Finished S E G Y C H K")) // and take up to </rs:data>
                          .ToList();              
                var fileSize = file.FirstOrDefault(c => c.Contains("Completed Physical reel, Transfer total")).Replace("Completed Physical reel, Transfer total ", "");
                fileSize = fileSize.Split(',').Select(c => c).LastOrDefault();
                data = st;
                if (isSegYContainsMoreThanOne(data))
                {
                    var finaloutput = new List<SegYTapeSummary>();
                    var tempdata = new List<SegYTapeSummary>();
                    var k = ExtractInfoFromIsSegDContainsMoreThanOne(data);
                    foreach (var item in k)
                    {
                        var it = item.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s));
                        var iitem = string.Join(",", it);
                        if (!string.IsNullOrWhiteSpace(iitem))
                        {

                            var outputParse = OutputParseSegY(iitem);
                            outputParse.FileSize = fileSize;
                            outputParse.FileType = "SEGY";
                            tempdata.Add(outputParse);
                        }
                    }
                    return tempdata;
                }
                else
                {
                    List<SegYTapeSummary> datas = new List<SegYTapeSummary>();
                    foreach (var item in data)
                    {
                        var it = item.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s));
                        var iitem = string.Join(",", it);
                        if (!string.IsNullOrWhiteSpace(iitem))
                        {
                            var outputParse = OutputParseSegY(iitem);
                            outputParse.FileSize = fileSize;
                            outputParse.FileType = "SEGY";
                            datas.Add(outputParse);
                        }

                        //  Console.WriteLine(iitem);
                    }
                    return datas;
                }
            }
            catch (ReportErrorHandler)
            {
                throw new ReportErrorHandler();
            }
            
        }

        private bool isSegYContainsMoreThanOne(List<string> data)
        {
            if (data != null)
            {
                if (data.Count > 1)
                {
                    var c = data.Count;
                    return true;
                }
            }
            return false;
        }
    }
}
