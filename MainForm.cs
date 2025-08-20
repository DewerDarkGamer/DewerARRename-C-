using System.Drawing;
using ZXing;
using ZXing.Windows.Compatibility;

namespace BarcodeRename
{
    public partial class MainForm : Form
    {
        private readonly BarcodeReader _reader;
        private readonly ListBox _logListBox;
        private const int MIN_BARCODE_LENGTH = 10;

        public MainForm()
        {
            InitializeComponent();

            this.Size = new Size(800, 600);
            this.Text = "Barcode Rename";
            this.MinimumSize = new Size(600, 400);

            _logListBox = new ListBox
            {
                Dock = DockStyle.Bottom,
                Height = 400
            };

            // สร้าง BarcodeReader
            _reader = new BarcodeReader
            {
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.QR_CODE,
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.ITF
                    }
                }
            };

            // สร้าง Panel สำหรับปุ่ม
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10),
                ColumnCount = 2,
                RowCount = 1
            };

            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            var btnSelectFiles = new Button
            {
                Text = "Select Files",
                Dock = DockStyle.Fill,
                Height = 40,
                Font = new Font(Font.FontFamily, 10, FontStyle.Regular),
                Margin = new Padding(5)
            };
            btnSelectFiles.Click += (_, _) => SelectFiles();

            var btnSelectFolder = new Button
            {
                Text = "Select Folder",
                Dock = DockStyle.Fill,
                Height = 40,
                Font = new Font(Font.FontFamily, 10, FontStyle.Regular),
                Margin = new Padding(5)
            };
            btnSelectFolder.Click += (_, _) => SelectFolder();

            buttonPanel.Controls.Add(btnSelectFiles, 0, 0);
            buttonPanel.Controls.Add(btnSelectFolder, 1, 0);

            // เพิ่ม Controls เข้าฟอร์ม
            this.Controls.Add(_logListBox);
            this.Controls.Add(buttonPanel);

            // แสดงข้อความต้อนรับ
            _logListBox.Items.Add("Welcome to Barcode Rename");
            _logListBox.Items.Add("Click 'Select Files' to process individual files or 'Select Folder' to process all images in a folder");
            _logListBox.Items.Add($"Minimum barcode length: {MIN_BARCODE_LENGTH} characters");
            _logListBox.Items.Add("");
        }

        private void SelectFiles()
        {
            using var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*",
                FilterIndex = 1,
                Title = "Select Images to Rename"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ProcessFiles(openFileDialog.FileNames);
            }
        }

        private void SelectFolder()
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Select Folder Containing Images",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string[] files = Directory.GetFiles(folderDialog.SelectedPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => file.ToLower().EndsWith(".png") || 
                                 file.ToLower().EndsWith(".jpg") || 
                                 file.ToLower().EndsWith(".jpeg") || 
                                 file.ToLower().EndsWith(".bmp"))
                    .ToArray();

                if (files.Length == 0)
                {
                    _logListBox.Items.Add("No image files found in the selected folder");
                    return;
                }

                ProcessFiles(files);
            }
        }

        // ... โค้ดส่วน ProcessFiles และ ReadAllBarcodes เหมือนเดิม ...
    }
}
