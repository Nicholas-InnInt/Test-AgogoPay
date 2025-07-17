using Abp.Collections.Extensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.VietQR;
using Newtonsoft.Json;

namespace Neptune.NsPay.Web.TransferApi
{
    public class FileWatcherService : IDisposable
    {
        private readonly FileSystemWatcher _fileWatcher;
        private readonly string _directoryPath;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isStarted = false;

        public FileWatcherService(string directoryPath)
        {
            _directoryPath = directoryPath;

            if (!_directoryPath.IsNullOrEmpty() && Directory.Exists(directoryPath))
            {
                _fileWatcher = new FileSystemWatcher(_directoryPath)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _fileWatcher.Created += OnChanged;
                _fileWatcher.Changed += OnChanged;
                _fileWatcher.Deleted += OnChanged;
                _fileWatcher.Renamed += OnRenamed;
            }

        }

        public void Start()
        {
            if (_isStarted)
                return;

            if (_fileWatcher is not null)
            {
                _fileWatcher.EnableRaisingEvents = true;
                Console.WriteLine($"Watching directory: {_directoryPath}");
                callOnChangedOnce();
            }

            _isStarted = true;
        }

        private void callOnChangedOnce()
        {
            // manual call changed when service started
            foreach (var filePath in Directory.GetFiles(_directoryPath))
            {
                FileSystemEventArgs e = new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
                OnChanged(_fileWatcher, e);
            }

        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File {e.ChangeType}: {e.FullPath}");
            NlogLogger.Info($"File {e.ChangeType}: {e.FullPath} - {e.Name.ToLower()}");

            switch (e.Name.ToLower())
            {
                case "bankmapping.json":
                    {
                        // update bank mapping dict
                        try
                        {

                            string content = string.Empty;
                            using (var fileStream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                using (var reader = new StreamReader(fileStream))
                                {
                                    content = reader.ReadToEnd(); // Read the entire file content
                                }
                            }

                            if (!content.IsNullOrEmpty())
                            {
                                var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

                                if (jsonObj != null && jsonObj.Count > 0)
                                {
                                    WithdrawalOrderBankMapper.UpdateMappingData(jsonObj);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Error(ex.GetMessage());
                        }

                        break;
                    }
                default:
                    {
                        break;
                    }
            }

        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"File Renamed: {e.OldFullPath} to {e.FullPath}");
        }

        public void Stop()
        {
            if (_fileWatcher is not null)
            {
                _fileWatcher.EnableRaisingEvents = false;
            }
        }

        public void Dispose()
        {
            if (_fileWatcher is not null)
            {
                _fileWatcher.Dispose();
            }
        }
    }
}