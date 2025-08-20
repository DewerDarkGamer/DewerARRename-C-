using System.Drawing;
using ZXing;
using ZXing.Windows.Compatibility;

namespace BarcodeRename
{
    public partial class MainForm : Form
    {
        private readonly BarcodeReader _reader;

        public MainForm()
        {
            InitializeComponent();
            
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
                        BarcodeFormat.ITF
                    }
                }
            };

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // สร้าง Controls
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };

            Button btnSelectFiles = new Button
            {
                Text = "Select Files",
                Dock = DockStyle.Fill,
                Height = 40
            };
            btnSelectFiles.Click += BtnSelectFiles_Click;

            Button btnSelectFolder = new Button
            {
                Text = "Select Folder",
                Dock = DockStyle.Fill,
                Height = 40
            };
            btnSelectFolder.Click += BtnSelectFolder_Click;

            ListBox logListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Name = "logListBox"
            };

            mainLayout.Controls.Add(btnSelectFiles, 0, 0);
            mainLayout.Controls.Add(btnSelectFolder, 0, 1);
            mainLayout.Controls.Add(logListBox, 0, 2);

            this.Controls.Add(mainLayout);
            this.Size = new Size(600, 400);
            this.Text = "Barcode Rename";
        }

        private void BtnSelectFiles_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ProcessFiles(openFileDialog.FileNames);
            }
        }

        private void BtnSelectFolder_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog folderDialog = new FolderBrowserDialog();
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
            ListBox logListBox = (ListBox)Controls.Find("logListBox", true)[0];
            logListBox.Items.Clear();

            foreach (string filePath in files)
            {
                try
                {
                    using var bitmap = new Bitmap(filePath);
                    string barcodeText = ReadBarcode(bitmap);

                    if (!string.IsNullOrEmpty(barcodeText))
                    {
                        string directory = Path.GetDirectoryName(filePath)!;
                        string extension = Path.GetExtension(filePath);
                        string newFilePath = Path.Combine(directory, $"{barcodeText}{extension}");

                        // ถ้ามีไฟล์อยู่แล้ว ให้เพิ่มตัวเลขต่อท้าย
                        int counter = 1;
                        while (File.Exists(newFilePath))
                        {
                            newFilePath = Path.Combine(directory, $"{barcodeText}_{counter}{extension}");
                            counter++;
                        }

                        File.Move(filePath, newFilePath);
                        logListBox.Items.Add($"Renamed: {Path.GetFileName(filePath)} -> {Path.GetFileName(newFilePath)}");
                    }
                    else
                    {
                        logListBox.Items.Add($"No barcode found in: {Path.GetFileName(filePath)}");
                    }
                }
                catch (Exception ex)
                {
                    logListBox.Items.Add($"Error processing {Path.GetFileName(filePath)}: {ex.Message}");
                }
            }
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
