using SmartFileManager.Core.Models;

namespace SmartFileManager.Core.Services
{
    public class FileOrganizerService
    {
        private readonly CategoryResolverService _resolver = new();

        /// <summary>
        /// Перемещает файлы в папки в зависимости от их категории.
        /// </summary>
        public async Task OrganizeFilesAsync(
            List<FileModel> files,
            List<Category> categories,
            string rootPath,
            IProgress<(int current, int total)>? progress,
            CancellationToken ct)
        {
            int total = files.Count;
            int current = 0;

            // Запускаем в фоновом потоке, чтобы не вешать интерфейс
            await Task.Run(() =>
            {
                foreach (var file in files)
                {
                    ct.ThrowIfCancellationRequested(); // Проверка на отмену

                    try
                    {
                        // 1. Узнаем категорию (например, "Картинки")
                        string targetFolderName = _resolver.ResolveTargetFolder(file.Extension, categories);

                        // 2. Формируем путь к новой папке
                        string targetDir = Path.Combine(rootPath, targetFolderName);
                        Directory.CreateDirectory(targetDir); // Создаем, если нет

                        // 3. Если файл УЖЕ лежит в этой папке, пропускаем его
                        string currentDir = Path.GetDirectoryName(file.FullPath) ?? "";
                        if (string.Equals(currentDir, targetDir, StringComparison.OrdinalIgnoreCase))
                        {
                            current++;
                            progress?.Report((current, total));
                            continue;
                        }

                        // 4. Формируем новый путь
                        string targetPath = Path.Combine(targetDir, file.Name);

                        // 5. Проверяем на конфликт имен (если файл с таким именем уже есть)
                        if (File.Exists(targetPath))
                        {
                            targetPath = GetUniqueFilePath(targetDir, file.Name);
                        }

                        // 6. Перемещаем файл
                        File.Move(file.FullPath, targetPath);

                        // 7. Обновляем путь в модели (чтобы интерфейс видел новый путь)
                        file.FullPath = targetPath;
                    }
                    catch (Exception ex)
                    {
                        // Если файл не удалось переместить (занят, нет прав), просто идем дальше
                        // В реальном проекте тут можно писать в лог, но для курсовой сойдет
                        System.Diagnostics.Debug.WriteLine($"Ошибка перемещения {file.FullPath}: {ex.Message}");
                    }

                    // 8. Сообщаем о прогрессе
                    current++;
                    progress?.Report((current, total));
                }
            }, ct);
        }

        /// <summary>
        /// Генерирует уникальное имя файла, если в папке уже есть файл с таким же именем.
        /// Например: "photo.jpg" -> "photo (1).jpg".
        /// </summary>
        private string GetUniqueFilePath(string directory, string fileName)
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            int counter = 1;

            // Формируем первый вариант: photo (1).jpg
            string newPath = Path.Combine(directory, $"{nameWithoutExt} ({counter}){extension}");

            // Пока файл с таким именем существует, увеличиваем счетчик
            while (File.Exists(newPath))
            {
                counter++;
                newPath = Path.Combine(directory, $"{nameWithoutExt} ({counter}){extension}");
            }

            return newPath;
        }
    }
}