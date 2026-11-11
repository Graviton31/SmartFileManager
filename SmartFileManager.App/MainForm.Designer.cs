namespace SmartFileManager.App
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtPath = new TextBox();
            btnBrowse = new Button();
            btnCancel = new Button();
            btnScan = new Button();
            lblStatus = new Label();
            dgvFiles = new DataGridView();
            btnFindDuplicates = new Button();
            progressBar1 = new ProgressBar();
            btnDelete = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvFiles).BeginInit();
            SuspendLayout();
            // 
            // txtPath
            // 
            txtPath.Location = new Point(3, 24);
            txtPath.Name = "txtPath";
            txtPath.Size = new Size(797, 23);
            txtPath.TabIndex = 0;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(12, 53);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(75, 23);
            btnBrowse.TabIndex = 1;
            btnBrowse.Text = "Выбрать";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(174, 53);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Отмена";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnScan
            // 
            btnScan.Location = new Point(93, 53);
            btnScan.Name = "btnScan";
            btnScan.Size = new Size(75, 23);
            btnScan.TabIndex = 3;
            btnScan.Text = "Сканировать";
            btnScan.UseVisualStyleBackColor = true;
            btnScan.Click += btnScan_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 6);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(95, 15);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Готово к работе";
            // 
            // dgvFiles
            // 
            dgvFiles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvFiles.Dock = DockStyle.Bottom;
            dgvFiles.Location = new Point(0, 92);
            dgvFiles.Name = "dgvFiles";
            dgvFiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFiles.Size = new Size(917, 368);
            dgvFiles.TabIndex = 5;
            dgvFiles.CellDoubleClick += dgvFiles_CellDoubleClick;
            // 
            // btnFindDuplicates
            // 
            btnFindDuplicates.Location = new Point(255, 53);
            btnFindDuplicates.Name = "btnFindDuplicates";
            btnFindDuplicates.Size = new Size(113, 23);
            btnFindDuplicates.TabIndex = 6;
            btnFindDuplicates.Text = "Найти дубликаты";
            btnFindDuplicates.UseVisualStyleBackColor = true;
            btnFindDuplicates.Click += btnFindDuplicates_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(351, 2);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(449, 16);
            progressBar1.TabIndex = 7;
            progressBar1.Visible = false;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(659, 53);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(141, 23);
            btnDelete.TabIndex = 8;
            btnDelete.Text = "Удалить выбранные";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(917, 460);
            Controls.Add(btnDelete);
            Controls.Add(progressBar1);
            Controls.Add(btnFindDuplicates);
            Controls.Add(lblStatus);
            Controls.Add(btnScan);
            Controls.Add(btnCancel);
            Controls.Add(btnBrowse);
            Controls.Add(txtPath);
            Controls.Add(dgvFiles);
            Name = "MainForm";
            ((System.ComponentModel.ISupportInitialize)dgvFiles).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtPath;
        private Button btnBrowse;
        private Button btnCancel;
        private Button btnScan;
        private Label lblStatus;
        private DataGridView dgvFiles;
        private Button btnFindDuplicates;
        private ProgressBar progressBar1;
        private Button btnDelete;
    }
}
