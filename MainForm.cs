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
            
            _reader = new BarcodeReader
            {
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.ITF
                    }
                }
            };

            // สร้าง UI
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };

            var btnSelectFiles = new Button
            {
                Text = "Select Files",
                Dock = DockStyle.Fill,
                Height = 40
            };
            btnSelectFiles.Click += (_, _) => SelectFiles();

            var btnSelectFolder = new Button
            {
                Text = "Select Folder",
                Dock = DockStyle.Fill,
                Height = 40
            };
            btnSelectFolder.Click += (_, _) => SelectFolder();

            _logListBox = new ListBox
            {
                Dock = DockStyle.Fill
            };

            mainLayout.Controls.Add(btnSelectFiles, 0, 0);
            mainLayout.Controls.Add(btnSelectFolder, 0, 1);
            mainLayout.Controls.Add(_logListBox, 0, 2);

            Controls.Add(mainLayout);
        }

        private void SelectFiles()
        {
            using var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ProcessFiles(openFileDialog.FileNames);
            }
        }

        private void SelectFolder()
        {
            using var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string[] files = Directory.GetFiles(folderDialog.SelectedPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => file.ToLower().EndsWith(".png") || 
                                 file.ToLower().EndsWith(".jpg") || 
                                 file.ToLower().EndsWith(".jpeg") || 
                                 file.ToLower().EndsWith(".bmp"))
                    .ToArray();

                ProcessFiles(files);
            }
        }

        private void ProcessFiles(string[] files)
        {
            _logListBox.Items.Clear();

            foreach (string filePath in files)
            {
                try
                {
                    string barcodeText;
                    using (var bitmap = new Bitmap(filePath))
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, bitmap.RawFormat);
                        using var tempBitmap = new Bitmap(ms);
                        barcodeText = ReadBarcode(tempBitmap);
                    }

                    if (!string.IsNullOrEmpty(barcodeText))
                    {
                        // ตรวจสอบความยาวของ barcode
                        if (barcodeText.Length >= MIN_BARCODE_LENGTH)
                        {
                            string directory = Path.GetDirectoryName(filePath)!;
                            string extension = Path.GetExtension(filePath);
                            string newFilePath = Path.Combine(directory, $"{barcodeText}{extension}");

                            int counter = 1;
                            while (File.Exists(newFilePath))
                            {
                                newFilePath = Path.Combine(directory, $"{barcodeText}_{counter}{extension}");
                                counter++;
                            }

                            Thread.Sleep(100);
                            
                            File.Move(filePath, newFilePath);
                            _logListBox.Items.Add($"Renamed: {Path.GetFileName(filePath)} -> {Path.GetFileName(newFilePath)}");
                            _logListBox.Items.Add($"  Barcode length: {barcodeText.Length} characters");
                        }
                        else
                        {
                            _logListBox.Items.Add($"Skipped: {Path.GetFileName(filePath)} - Barcode too short ({barcodeText.Length} characters): {barcodeText}");
                        }
                    }
                    else
                    {
                        _logListBox.Items.Add($"No barcode found in: {Path.GetFileName(filePath)}");
                    }
                }
                catch (Exception ex)
                {
                    _logListBox.Items.Add($"Error processing {Path.GetFileName(filePath)}: {ex.Message}");
                }
            }

            // แสดงสรุปที่ด้านล่างของล็อก
            _logListBox.Items.Add("");
            _logListBox.Items.Add($"Process completed at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            _logListBox.Items.Add($"Minimum barcode length: {MIN_BARCODE_LENGTH} characters");
        }

        private string ReadBarcode(Bitmap image)
        {
            try
            {
                var result = _reader.Decode(image);
                return result?.Text ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
