using System.Collections.Generic;

namespace ReportGenerator.Repository
{
    public interface IReportRepository
    {
        /// <summary>
        /// This method Check what type of report that is to be generated.
        /// It then perform the output based on the query command. 
        /// To perform SEGY CHECK simple instruct the commandline that is SEGY CHECK
        /// To perform SEGD CHECK simple instruct the commandline that is SEGD CHECK
        /// To perform Copy simple instruct the commandline that is COPY
        /// </summary>
        /// <param name="query"></param>
        /// <returns>returns the type of script to be run is it SEGD CHECK, SEGY CHECK OR COPY</returns>
        string CheckReportType(string query);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str">List of String as parameter</param>
        /// <returns>returns the list of string before </returns>
        List<string> GetStringBefore(List<string> str);
        /// <summary>
        /// gets the string before when considering Many To one Output
        /// </summary>
        /// <param name="str"></param>
        /// <returns>List of strings</returns>
        List<string> GetStringBeforeManyToOneOutput(List<string> str);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns>string</returns>
        string GetStringBeforeSingle(List<string> str);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <returns>returns the tape number by scanning through scratch file</returns>
        string GetMyTapeNoBatchCopy(string arg);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <returns>returns Tape number in the case of Many to one output</returns>
        string GetMyTapeNoBatchCopyManyToOneOutput(string arg);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        string GetMyTapeNoSingleCopy(string arg);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        List<string> GetFileSize(List<string> str);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="TapeNo"></param>
        /// <returns></returns>
        List<Reports> GetFileStartAndEnd(List<string> str, List<string> TapeNo);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="tape"></param>
        /// <returns></returns>
        bool BatchChecker(string x, string tape);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="TapeNo"></param>
        /// <returns></returns>
        List<Reports> GetFileStartAndEndSingleCopy(List<string> str, string TapeNo);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        bool CheckIfBatchCopy(string file);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        bool CheckIfSegDData(List<string> str);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        bool IsOneToManyBatchCopy(string file);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        bool IsManyToManyBatchCopy(string file);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        bool IsManyToOneBatchCopy(string file);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        bool IsOneToOneBatchCopy(string file);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reports"></param>
        void CreateExcelReport(List<Reports> reports);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reports"></param>
        /// <param name="tapes"></param>
        /// <returns></returns>
        List<Reports> ManyToOneReport(List<string> reports, List<string> tapes);

        //string CheckFileExtention(string)
        bool CheckIsSegDCheck(string file);
        bool CheckIsSegYCheck(string file);
    }
}
