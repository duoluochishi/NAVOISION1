namespace NV.CT.LogManagement.Models
{
    public class LogLineModel
    {
        private const int MAX_OMISSIVE_CONTENT_LENGTH = 100;

        public int LineNumber { get; set; }

        public string OccurredDateTime { get; set; }

        public string LogLevel { get; set; }

        public string ModuleName { get; set; }

        public string ServiceName { get; set; }

        public string Content { get; set; }

        public string OmissiveContent => GetOmissiveContent(this.Content);

        public string ThreadID { get; set; } = string.Empty; //Not implemented yet, will do this later.

        public LogFileProfileModel LogFile { get; set; }


        private string GetOmissiveContent(string content)
        { 
            if (string.IsNullOrWhiteSpace(content)) 
            {
                return string.Empty;
            }

            return content.Length <= MAX_OMISSIVE_CONTENT_LENGTH ? content : string.Concat(content.Substring(0, MAX_OMISSIVE_CONTENT_LENGTH), "...");
        
        }
    }
}
