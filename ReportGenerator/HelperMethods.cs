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

    }
}
