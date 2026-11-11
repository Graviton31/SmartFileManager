using SmartFileManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFileManager.Core.Services
{
    internal class CategoryResolverService
    {
        public string ResolveTargetFolder(string extension, List<Category> categories)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return "Прочее";
            }

            // Приводим к нижнему регистру
            var normalizedExt = extension.ToLowerInvariant();

            foreach (var category in categories)
            {
                // Проверяем, есть ли расширение в списке
                if (category.Extensions.Contains(normalizedExt))
                {
                    return category.TargetFolder;
                }
            }

            return "Прочее";
        }
    }
}
