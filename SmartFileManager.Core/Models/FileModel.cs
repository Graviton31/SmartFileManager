using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFileManager.Core.Models
{
    public class FileModel
    {
        // Имя файла.
        public required string Name { get; init; }

        // Полный путь.
        public required string FullPath { get; init; }

        // Размер в байтах.
        public long SizeBytes { get; init; }

        // Расширение (например, ".jpg").
        public required string Extension { get; init; }

        // Дата последнего изменения.
        public DateTime LastModified { get; init; }

        // Имя категории (например, "Images"). string. get; set; (потому что категория может измениться, если мы применим другое правило сортировки).
        public string CategoryName { get; set; } = "Other";

        // Хэш файла (SHA-256). string? (может быть null). get; set; (т.к. изначально пустой, заполняется позже).
        public string? Hash { get; set; }

        // Признак дубликата. bool. get; set;.
        public bool IsDuplicate { get; set; }
    }
}
