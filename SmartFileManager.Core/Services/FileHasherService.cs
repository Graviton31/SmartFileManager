using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SmartFileManager.Core.Services
{
    /// <summary>
    /// Вспомогательный класс для вычисления хэш-суммы файла.
    /// </summary>
    public static class FileHasherService
    {
        /// <summary>
        /// Вычисляет SHA-256 хэш файла потоково (не загружая его целиком в память).
        /// </summary>
        public static async Task<string> ComputeHashAsync(string path, CancellationToken cancellationToken = default)
        {
            // Открываем файл для чтения. FileOptions.Asynchronous делает чтение неблокирующим.
            await using var stream = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.Asynchronous);

            // Создаем алгоритм SHA-256
            using var sha256 = SHA256.Create();

            // Считаем хэш. Convert.ToHexString переводит байты в читаемую строку (HEX).
            var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
            return Convert.ToHexString(hashBytes);
        }

        public static async Task<string> ComputeSmartQuickHashAsync(string path, CancellationToken ct)
        {
            const int chunkSize = 1024 * 1024; // Размер одного куска = 1 МБ

            await using var stream = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.Read,
                65536, FileOptions.Asynchronous | FileOptions.SequentialScan);

            // Если файл маленький (меньше 3 МБ), проще прочитать его целиком
            if (stream.Length <= chunkSize * 3)
            {
                using var smallHash = SHA256.Create();
                var fullHash = await smallHash.ComputeHashAsync(stream, ct);
                return Convert.ToHexString(fullHash);
            }

            // Создаем алгоритм (для скорости можно взять MD5, но SHA256 надежнее)
            using var hashAlgorithm = SHA256.Create();
            byte[] buffer = new byte[chunkSize];

            // 1. Читаем НАЧАЛО (с 0-го байта)
            stream.Seek(0, SeekOrigin.Begin);
            int bytesRead = await stream.ReadAsync(buffer, ct);
            hashAlgorithm.TransformBlock(buffer, 0, bytesRead, null, 0);

            // 2. Читаем СЕРЕДИНУ (с половины файла)
            stream.Seek(stream.Length / 2, SeekOrigin.Begin);
            bytesRead = await stream.ReadAsync(buffer, ct);
            hashAlgorithm.TransformBlock(buffer, 0, bytesRead, null, 0);

            // 3. Читаем КОНЕЦ (с конца файла минус 1 МБ)
            stream.Seek(stream.Length - chunkSize, SeekOrigin.Begin);
            bytesRead = await stream.ReadAsync(buffer, ct);

            // Завершаем хэширование (TransformFinalBlock возвращает последнюю часть)
            hashAlgorithm.TransformFinalBlock(buffer, 0, bytesRead);

            // Возвращаем итоговый хэш
            return Convert.ToHexString(hashAlgorithm.Hash!);
        }
    }
}
