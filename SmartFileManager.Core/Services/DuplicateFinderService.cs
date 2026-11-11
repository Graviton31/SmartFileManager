using SmartFileManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartFileManager.Core.Services
{
    public class DuplicateFinderService
    {
        // Добавляем параметр progress в метод!
        public async Task<List<List<FileModel>>> FindDuplicatesAsync(
            List<FileModel> files,
            IProgress<(int current, int total)>? progress, // <- ВОТ ОН
            CancellationToken ct)
        {
            var result = new List<List<FileModel>>();

            // 1. Группируем по размеру
            var sizeGroups = files.GroupBy(f => f.SizeBytes).Where(g => g.Count() > 1).ToList();

            // Считаем общее количество файлов, которые нужно проверить
            int totalFilesToHash = sizeGroups.Sum(g => g.Count());
            int processedCount = 0;

            foreach (var sizeGroup in sizeGroups)
            {
                ct.ThrowIfCancellationRequested();

                // 2. СЧИТАЕМ БЫСТРЫЙ ХЭШ (параллельно)
                await Parallel.ForEachAsync(sizeGroup, async (file, token) =>
                {

                    token.ThrowIfCancellationRequested();

                    if (file.Hash == null)
                    {
                        file.Hash = await FileHasherService.ComputeSmartQuickHashAsync(file.FullPath, token);
                    }

                    // Увеличиваем счетчик потокобезопасно и сообщаем прогресс
                    int current = Interlocked.Increment(ref processedCount);
                    progress?.Report((current, totalFilesToHash));
                });

                // 3. Группируем по быстрому хэшу
                var quickHashGroups = sizeGroup.GroupBy(f => f.Hash).Where(g => g.Count() > 1);

                foreach (var quickGroup in quickHashGroups)
                {

                    ct.ThrowIfCancellationRequested();

                    // 4. СЧИТАЕМ ПОЛНЫЙ ХЭШ
                    foreach (var file in quickGroup)
                    {
                        if (file.SizeBytes > 1024 * 1024)
                        {
                            file.Hash = await FileHasherService.ComputeHashAsync(file.FullPath, ct);
                        }

                    }

                    // 5. Финальная группировка
                    var finalGroups = quickGroup.GroupBy(f => f.Hash).Where(g => g.Count() > 1);

                    foreach (var finalGroup in finalGroups)
                    {
                        foreach (var f in finalGroup)
                        {
                            f.IsDuplicate = true;
                        }
                        result.Add(finalGroup.ToList());
                    }
                }
            }
            return result;
        }
    }
}
