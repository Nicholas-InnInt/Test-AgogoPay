using Neptune.NsPay.VietQR;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System;
using Abp.Collections.Extensions;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Neptune.NsPay.Commons;

namespace Neptune.NsPay.Web.Startup
{
    public class FileWatcherService : IDisposable
    {
        private readonly FileSystemWatcher _fileWatcher;
        private readonly string _directoryPath;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isStarted = false;
        private readonly ILogger<FileWatcherService> _logger;

        public FileWatcherService(string directoryPath, ILogger<FileWatcherService> logger)
        {
            _directoryPath = directoryPath;
            _logger = logger;
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
            _logger.LogInformation($"File {e.ChangeType}: {e.FullPath} - {e.Name.ToLower()}");

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

                            if(!content.IsNullOrEmpty())
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
                            _logger.LogError(ex.Message);
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