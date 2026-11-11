using SmartFileManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFileManager.Core.Services
{
    public class FileScannerService
    {
        // Добавляем IProgress<int> в параметры
        public Task<List<FileModel>> ScanAsync(string path, IProgress<int>? progress, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => ScanDirectory(path, progress, cancellationToken), cancellationToken);
        }

        private List<FileModel> ScanDirectory(string path, IProgress<int>? progress, CancellationToken cancellationToken)
        {
            var result = new List<FileModel>();
            if (!Directory.Exists(path)) return result;

            var rootDir = new DirectoryInfo(path);
            ScanFolder(rootDir, result, progress, cancellationToken);
            return result;
        }

        private void ScanFolder(DirectoryInfo dir, List<FileModel> result, IProgress<int>? progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                foreach (var file in dir.GetFiles())
                {
                    if ((file.Attributes & FileAttributes.System) != 0 || (file.Attributes & FileAttributes.Hidden) != 0)
                        continue;

                    result.Add(new FileModel
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        SizeBytes = file.Length,
                        Extension = file.Extension.ToLowerInvariant(),
                        LastModified = file.LastWriteTime
                    });

                    // Сообщаем интерфейсу, что нашли еще один файл
                    if (result.Count % 50 == 0) // Сообщаем не на каждом файле, а каждые 50 штук (для оптимизации)
                        progress?.Report(result.Count);
                }

                foreach (var subDir in dir.GetDirectories())
                {
                    if ((subDir.Attributes & FileAttributes.System) != 0 || (subDir.Attributes & FileAttributes.Hidden) != 0)
                        continue;

                    ScanFolder(subDir, result, progress, cancellationToken);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
            catch (IOException) { }
        }
    }
}
