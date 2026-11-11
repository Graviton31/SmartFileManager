using SmartFileManager.Core.Models;
using SmartFileManager.Core.Services;

namespace SmartFileManager.App;

// Если ты НЕ переименовывал форму, напиши: public partial class Form1 : Form
public partial class MainForm : Form
{
    private readonly System.Diagnostics.Stopwatch _stopwatch = new();
    private readonly FileScannerService _scanner = new();
    private CancellationTokenSource? _cts;

    public MainForm()
    {
        InitializeComponent();
        SetupGrid();

        // Подписываемся на событие перерисовки ячеек (для покраски)
        dgvFiles.CellFormatting += DgvFiles_CellFormatting;
    }

    private void SetupGrid()
    {
        dgvFiles.AutoGenerateColumns = false;
        dgvFiles.AllowUserToAddRows = false;
        dgvFiles.ReadOnly = true;
        dgvFiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Имя", DataPropertyName = nameof(FileModel.Name), FillWeight = 30 });
        dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Путь", DataPropertyName = nameof(FileModel.FullPath), FillWeight = 55 });
        dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Размер (байт)", DataPropertyName = nameof(FileModel.SizeBytes), FillWeight = 15 });
        dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Категория", DataPropertyName = nameof(FileModel.CategoryName), FillWeight = 15 });
        dgvFiles.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Группа (Хэш)",
            Name = "HashColumn", // Дадим ей имя, чтобы потом обращаться
            FillWeight = 20
        });
    }

    // === СКАНИРОВАНИЕ ===
    private async void btnScan_Click(object sender, EventArgs e)
    {
        var path = txtPath.Text.Trim();
        if (!Directory.Exists(path))
        {
            MessageBox.Show("Выберите существующую папку!");
            return;
        }

        _stopwatch.Restart();

        _cts = new CancellationTokenSource();
        SetBusy(true);
        dgvFiles.DataSource = null;

        // Настраиваем полоску на режим "Змейка"
        progressBar1.Style = ProgressBarStyle.Marquee;
        progressBar1.Visible = true;

        // Прогресс для сканирования
        var progress = new Progress<int>(count =>
        {
            // Показываем сколько прошло времени
            lblStatus.Text = $"Сканирование... Найдено: {count}. Прошло: {_stopwatch.Elapsed:mm\\:ss}";
        });

        try
        {
            var files = await _scanner.ScanAsync(path, progress, _cts.Token);
            dgvFiles.DataSource = files;

            _stopwatch.Stop(); // Останавливаем таймер
            lblStatus.Text = $"Готово. Найдено: {files.Count} файлов. Время: {_stopwatch.Elapsed:mm\\:ss}";
        }
        catch (OperationCanceledException)
        {
            lblStatus.Text = "Сканирование отменено.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
            progressBar1.Visible = false;
            _cts.Dispose();
            _cts = null;
        }
    }

    // === ПОИСК ДУБЛИКАТОВ ===
    private async void btnFindDuplicates_Click(object sender, EventArgs e)
    {
        if (dgvFiles.DataSource is not List<FileModel> files || files.Count == 0)
        {
            MessageBox.Show("Сначала просканируйте папку!");
            return;
        }

        _stopwatch.Restart(); // Запускаем таймер перед поиском

        SetBusy(true);
        _cts = new CancellationTokenSource();

        // Сбрасываем флаги дубликатов от прошлого поиска
        foreach (var f in files) f.IsDuplicate = false;
        dgvFiles.Refresh(); // Заставляем таблицу перерисоваться (убрать старые красные цвета)

        // Настраиваем полоску на обычный режим (от 0 до 100%)
        progressBar1.Style = ProgressBarStyle.Blocks;
        progressBar1.Value = 0;
        progressBar1.Visible = true;
        lblStatus.Text = "Поиск дубликатов...";

        var progress = new Progress<(int current, int total)>(p =>
        {
            progressBar1.Maximum = p.total;
            progressBar1.Value = p.current;

            // Защита от деления на ноль, если p.current == 0
            string etaText = "";
            if (p.current > 0 && p.current < p.total)
            {
                // Расчет ETA
                double secondsPerFile = _stopwatch.Elapsed.TotalSeconds / p.current;
                double remainingSeconds = secondsPerFile * (p.total - p.current);

                // Переводим секунды в красивый формат
                var eta = TimeSpan.FromSeconds(remainingSeconds);
                etaText = $" | Осталось: ~{eta:mm\\:ss}";
            }

            lblStatus.Text = $"Проверка: {p.current} из {p.total}{etaText}";
        });

        try
        {
            var finder = new DuplicateFinderService();
            var duplicates = await finder.FindDuplicatesAsync(files, progress, _cts.Token);

            // Сортируем список так, чтобы файлы с одинаковым хэшем шли подряд!
            files.Sort((f1, f2) => string.Compare(f1.Hash, f2.Hash));

            // Теперь обновляем таблицу
            dgvFiles.DataSource = null;
            dgvFiles.DataSource = files;

            // ВАЖНО: После того как IsDuplicate стал true, заставляем таблицу перерисоваться.
            // Событие CellFormatting поймает это и покрасит строки.
            dgvFiles.Refresh();

            _stopwatch.Stop(); // Останавливаем таймер
            int totalDuplicateFiles = duplicates.Sum(group => group.Count);
            lblStatus.Text = $"Готово за {_stopwatch.Elapsed:mm\\:ss}. Найдено групп: {duplicates.Count} (всего дублей: {totalDuplicateFiles} шт.)";
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
        {
            lblStatus.Text = "Поиск отменен.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}");
        }
        finally
        {
            progressBar1.Visible = false;
            SetBusy(false);
            _cts.Dispose();
            _cts = null;
        }
    }

    // === СОБЫТИЕ ПОКРАСКИ ===
    private void DgvFiles_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex >= 0 && dgvFiles.Rows[e.RowIndex].DataBoundItem is FileModel model)
        {
            // Красим строки (твой старый код)
            if (model.IsDuplicate)
                e.CellStyle.BackColor = Color.LightCoral;
            else
                e.CellStyle.BackColor = Color.White;

            // Если рисуется колонка "Группа" (HashColumn)
            if (dgvFiles.Columns[e.ColumnIndex].Name == "HashColumn")
            {
                // Показываем первые 8 символов хэша, если он есть
                e.Value = model.Hash != null ? model.Hash.Substring(0, 8) : "";
                e.FormattingApplied = true; // Говорим таблице, что мы сами задали значение
            }
        }
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog(this) == DialogResult.OK)
            txtPath.Text = dialog.SelectedPath;
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }

    private void SetBusy(bool isBusy)
    {
        btnBrowse.Enabled = !isBusy;
        btnScan.Enabled = !isBusy;
        btnFindDuplicates.Enabled = !isBusy;
        btnCancel.Enabled = isBusy;
        Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
    }

    private void dgvFiles_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        // Если кликнули по заголовку, ничего не делаем
        if (e.RowIndex < 0) return;

        // Берем файл из строки
        if (dgvFiles.Rows[e.RowIndex].DataBoundItem is FileModel file)
        {
            try
            {
                // Встроенная команда Windows: "Открой этот файл той программой, которая стоит по умолчанию"
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = file.FullPath,
                    UseShellExecute = true // Обязательно true для открытия файлов
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка");
            }
        }
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        // Получаем все выделенные строки
        if (dgvFiles.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выделите файлы для удаления!", "Внимание");
            return;
        }

        // Спрашиваем подтверждение (защита от случайного удаления)
        var result = MessageBox.Show(
            $"Вы уверены, что хотите удалить {dgvFiles.SelectedRows.Count} файл(ов) с диска?\nЭто действие нельзя отменить.",
            "Подтверждение удаления",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes) return;

        // Получаем список файлов из нашей модели данных
        var files = dgvFiles.DataSource as List<FileModel>;

        if (files == null) return; // Если там null, дальше не идем

        var filesToRemove = new List<FileModel>();

        foreach (DataGridViewRow row in dgvFiles.SelectedRows)
        {
            if (row.DataBoundItem is FileModel file)
            {
                try
                {
                    // Физически удаляем файл с диска
                    File.Delete(file.FullPath);

                    // Запоминаем, что его надо убрать из таблицы
                    filesToRemove.Add(file);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось удалить {file.Name}:\n{ex.Message}", "Ошибка доступа");
                }
            }
        }

        // Удаляем успешно удаленные файлы из нашего списка
        foreach (var f in filesToRemove)
        {
            files.Remove(f);
        }

        // Обновляем таблицу (перерисовываем)
        dgvFiles.DataSource = null;
        dgvFiles.DataSource = files;
        dgvFiles.Refresh();

        lblStatus.Text = $"Удалено файлов: {filesToRemove.Count}";
    }

    private async void btnOrganize_Click(object sender, EventArgs e)
    {
        if (dgvFiles.DataSource is not List<FileModel> files || files.Count == 0)
        {
            MessageBox.Show("Сначала просканируйте папку!");
            return;
        }

        var result = MessageBox.Show(
            $"Вы уверены, что хотите разложить {files.Count} файлов по категориям?\n" +
            "Они будут перемещены в подпапки (Картинки, Документы и т.д.)",
            "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result != DialogResult.Yes) return;

        // Настраиваем UI на время работы
        SetBusy(true);
        _cts = new CancellationTokenSource();

        // Настраиваем прогресс-бар
        progressBar1.Style = ProgressBarStyle.Blocks;
        progressBar1.Value = 0;
        progressBar1.Visible = true;

        _stopwatch.Restart();

        // Создаем список категорий (в будущем мы будем загружать его из настроек/JSON)
        // Пока создадим базовый набор прямо здесь (или вынеси в отдельный класс-фабрику)
        var categories = new List<Category>
    {
        new Category { Name = "Documents", DisplayName = "Документы", TargetFolder = "Документы", Extensions = [".doc", ".docx", ".pdf", ".txt", ".ppt", ".pptx", ".xls", ".xlsx"] },
        new Category { Name = "Images", DisplayName = "Картинки", TargetFolder = "Картинки", Extensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" ]},
        new Category { Name = "Video", DisplayName = "Видео", TargetFolder = "Видео", Extensions = [".mp4", ".avi", ".mkv", ".wmv", ".flv", ".mpeg"] },
        new Category { Name = "Audio", DisplayName = "Звук", TargetFolder = "Звук", Extensions = [".mp3", ".wav", " .aac", ".midi"] },
        new Category { Name = "Archives", DisplayName = "Архивы", TargetFolder = "Архивы", Extensions = [".zip", ".rar", ".7z", ".gzip"] },
        new Category { Name = "InternetFiles", DisplayName = "Интернет Файлы", TargetFolder = "Интернет Файлы", Extensions = [".html", ".htm", ".mht"] },
        new Category { Name = "SystemAndRunnable", DisplayName = "Системные и исполняемые", TargetFolder = "Системные и исполняемые", Extensions = [".exe", ".dll", ".ini", ".bat", ".iso"] }
    };

        var progress = new Progress<(int current, int total)>(p =>
        {
            progressBar1.Maximum = p.total;
            progressBar1.Value = p.current;
            lblStatus.Text = $"Сортировка: {p.current} из {p.total}";
        });

        try
        {
            var organizer = new FileOrganizerService();

            // Корневая папка для сортировки — та же папка, которую мы сканировали
            string rootPath = txtPath.Text.Trim();

            await organizer.OrganizeFilesAsync(files, categories, rootPath, progress, _cts.Token);

            _stopwatch.Stop();
            lblStatus.Text = $"Сортировка завершена за {_stopwatch.Elapsed:mm\\:ss}";

            // Обновляем таблицу, так как пути изменились!
            dgvFiles.Refresh();
        }
        catch (OperationCanceledException)
        {
            lblStatus.Text = "Сортировка отменена.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
            progressBar1.Visible = false;
            _cts.Dispose();
            _cts = null;
        }
    }
}