using System.Diagnostics;

namespace NV.CT.CTS.Helpers
{
    public class ConsoleLogRedirector
    {
        private static Lazy<ConsoleLogRedirector> _instance = new Lazy<ConsoleLogRedirector>(() => new ConsoleLogRedirector());

        private string _logPath = string.Empty;
        private FileStream? _logStream;
        private StreamWriter? _logWriter;
        private TextWriter? _oldConsoleOut;
        private readonly string _processName;

        private bool isRedirectActivated;

        private static object _lockObj = new object();

        private ConsoleLogRedirector()
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            _processName = processName.Replace("NV.CT.", "").Replace(".App", "");
        }

        public static ConsoleLogRedirector Instance => _instance.Value;

        public bool StartConsoleLogRedirect(string loggingRoot)
        {
            if (isRedirectActivated)
            {
                StopConsoleLogRedirect();
            }

            SetDefaultRedirectPath(loggingRoot);

            try
            {
                _logStream = new FileStream(_logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                _logWriter = new StreamWriter(_logStream);
                _logWriter.AutoFlush = true;

                _oldConsoleOut = Console.Out;
                Console.SetOut(_logWriter);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            Console.WriteLine($"Switch console log to {_logPath}");
            isRedirectActivated = true;

            return true;
        }

        private void SetDefaultRedirectPath(string loggingRoot)
        {
            if (string.IsNullOrEmpty(_logPath))
            {
                lock (_lockObj)
                {
                    if (string.IsNullOrEmpty(_logPath))
                    {
                        var fileName = Path.Combine(loggingRoot, $"{_processName}.FacadeProxy_{DateTime.Now:yyyyMMdd}.log");
                        if (!File.Exists(fileName))
                        {
                            File.Create(fileName).Close();
                        }
                        _logPath = fileName;
                    }
                }
            }
        }

        public void StopConsoleLogRedirect()
        {
            if (_oldConsoleOut is not null)
                Console.SetOut(_oldConsoleOut);
            _logWriter?.Close();
            _logStream?.Close();
            isRedirectActivated = false;
        }
    }
}
