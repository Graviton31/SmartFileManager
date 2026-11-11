using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFileManager.Core.Models
{
    public class Category
    {
        // 1. Имя категории (системное, на английском).
        public required string Name { get; init; }

        // 2. Отображаемое имя (для человека, на русском).
        public required string DisplayName { get; init; }

        // 3. Имя папки, куда будем складывать файлы (например, "Картинки").
        public required string TargetFolder { get; init; }

        // 4. Список расширений, которые относятся к этой категории.
        public List<string> Extensions { get; set; } = [];
    }
}
