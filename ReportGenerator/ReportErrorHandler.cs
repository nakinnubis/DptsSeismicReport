using System;

namespace ReportGenerator
{
    public class ReportErrorHandler: SystemException
    {
        public string ReportMessage { get; set; }

        public ReportErrorHandler()
        {
            ReportMessage = "An Error Occured, The scracth file might be poorly formated!!!";
        }
    }
}
