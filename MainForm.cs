using System.Drawing;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Windows.Compatibility;
using Tesseract;

namespace BarcodeRename
{
    public partial class MainForm : Form
    {
        private readonly BarcodeReader _reader;
        private readonly ListBox _logListBox;
        private readonly TesseractEngine _tesseract;
        private const int MIN_BARCODE_LENGTH = 10;
        private const int MAX_BARCODE_LENGTH = 12;

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

            // สร้าง BarcodeReader พร้อมการตั้งค่าที่เหมาะสม
            _reader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    TryInverted = true,
                    PureBarcode = false,
                    PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.ITF,
                        BarcodeFormat.CODE_93,
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.UPC_A,
                        BarcodeFormat.UPC_E,
                        BarcodeFormat.QR_CODE,
                        BarcodeFormat.DATA_MATRIX,
                        BarcodeFormat.PDF_417
                    },
                    ReturnCodabarStartEnd = true,
                    AssumeCode39CheckDigit = true,
                    CharacterSet = "UTF-8"
                }
            };

            // สร้าง TesseractEngine
            _tesseract = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            _tesseract.DefaultPageSegMode = PageSegMode.SingleBlock;

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
            _logListBox.Items.Add($"Barcode length requirement: {MIN_BARCODE_LENGTH}-{MAX_BARCODE_LENGTH} characters");
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

        private void ProcessFiles(string[] files)
        {
            _logListBox.Items.Clear();

            foreach (string filePath in files)
            {
                try
                {
                    List<string> barcodes;
                    using (var bitmap = new Bitmap(filePath))
                    {
                        barcodes = ReadAllBarcodes(bitmap);
                    }

                    if (barcodes.Any())
                    {
                        _logListBox.Items.Add($"Found {barcodes.Count} barcodes in: {Path.GetFileName(filePath)}");
                        
                        // กรองและแสดงเฉพาะ barcodes ที่มีความยาว 10-12 ตัวอักษร
                        var validBarcodes = barcodes
                            .Where(b => b.Length >= MIN_BARCODE_LENGTH && b.Length <= MAX_BARCODE_LENGTH)
                            .ToList();

                        foreach (var barcode in barcodes)
                        {
                            bool isValid = barcode.Length >= MIN_BARCODE_LENGTH && barcode.Length <= MAX_BARCODE_LENGTH;
                            _logListBox.Items.Add($"  - {barcode} (Length: {barcode.Length}) {(isValid ? "[VALID]" : "[INVALID]")}");
                        }

                        if (validBarcodes.Any())
                        {
                            string selectedBarcode = validBarcodes[0];
                            string directory = Path.GetDirectoryName(filePath)!;
                            string extension = Path.GetExtension(filePath);
                            string currentFileName = Path.GetFileNameWithoutExtension(filePath);
                            string newFileName = selectedBarcode;
                            string newFilePath = Path.Combine(directory, $"{newFileName}{extension}");

                            // ตรวจสอบว่าชื่อไฟล์เดิมตรงกับ barcode
                            if (currentFileName.Equals(newFileName, StringComparison.OrdinalIgnoreCase))
                            {
                                _logListBox.Items.Add($"Skipped: File already has correct name - {Path.GetFileName(filePath)}");
                                continue;
                            }

                            // ถ้ามีไฟล์อยู่แล้ว ให้เพิ่มตัวเลขต่อท้าย
                            int counter = 1;
                            while (File.Exists(newFilePath))
                            {
                                newFilePath = Path.Combine(directory, $"{newFileName}_{counter}{extension}");
                                counter++;
                            }

                            Thread.Sleep(100);
                            File.Move(filePath, newFilePath);
                            _logListBox.Items.Add($"Renamed: {Path.GetFileName(filePath)} -> {Path.GetFileName(newFilePath)}");
                        }
                        else
                        {
                            _logListBox.Items.Add($"Skipped: No barcode meets length requirement ({MIN_BARCODE_LENGTH}-{MAX_BARCODE_LENGTH} characters)");
                        }
                    }
                    else
                    {
                        _logListBox.Items.Add($"No barcode found in: {Path.GetFileName(filePath)}");
                    }

                    _logListBox.Items.Add("");
                }
                catch (Exception ex)
                {
                    _logListBox.Items.Add($"Error processing {Path.GetFileName(filePath)}: {ex.Message}");
                    _logListBox.Items.Add("");
                }
            }

            _logListBox.Items.Add("");
            _logListBox.Items.Add($"Process completed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _logListBox.Items.Add($"Barcode length requirement: {MIN_BARCODE_LENGTH}-{MAX_BARCODE_LENGTH} characters");
        }

        private List<string> ReadAllBarcodes(Bitmap image)
        {
            try
            {
                var barcodes = new List<string>();
                
                // พยายามอ่าน barcode ก่อน
                var results = _reader.DecodeMultiple(image);
                if (results != null)
                {
                    foreach (var result in results)
                    {
                        if (!string.IsNullOrWhiteSpace(result.Text))
                        {
                            barcodes.Add(result.Text);
                        }
                    }
                }

                // ถ้าอ่าน barcode ไม่ได้ ลองอ่านข้อความด้วย OCR
                if (!barcodes.Any())
                {
                    using (var processedImage = PreprocessImage(image))
                    {
                        // ลองอ่าน barcode จากภาพที่ประมวลผลแล้ว
                        results = _reader.DecodeMultiple(processedImage);
                        if (results != null)
                        {
                            foreach (var result in results)
                            {
                                if (!string.IsNullOrWhiteSpace(result.Text))
                                {
                                    barcodes.Add(result.Text);
                                }
                            }
                        }

                        // ถ้ายังอ่านไม่ได้ ใช้ OCR
                        if (!barcodes.Any())
                        {
                            var qaNumber = ExtractQANumber(processedImage);
                            if (!string.IsNullOrWhiteSpace(qaNumber))
                            {
                                barcodes.Add(qaNumber);
                            }
                        }
                    }
                }

                return barcodes;
            }
            catch
            {
                return new List<string>();
            }
        }

        private Bitmap PreprocessImage(Bitmap original)
        {
            try
            {
                // สร้างภาพใหม่
                Bitmap processed = new Bitmap(original.Width, original.Height);

                // ปรับความคมชัดและความแตกต่างของสี
                using (Graphics g = Graphics.FromImage(processed))
                {
                    ImageAttributes attributes = new ImageAttributes();

                    // ปรับ contrast และ brightness ใหม่สำหรับพื้นหลังสีส้ม
                    ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                    {
                        new float[] {3.0f, 0, 0, 0, 0},    // เพิ่ม contrast ของสีแดง
                        new float[] {0, 3.0f, 0, 0, 0},    // เพิ่ม contrast ของสีเขียว
                        new float[] {0, 0, 3.0f, 0, 0},    // เพิ่ม contrast ของสีน้ำเงิน
                        new float[] {0, 0, 0, 1.0f, 0},    // คงค่า alpha
                        new float[] {-0.8f, -0.8f, -0.8f, 0, 1}  // ปรับ brightness ให้เข้มขึ้น
                    });

                    attributes.SetColorMatrix(colorMatrix);

                    // วาดภาพใหม่ด้วยการปรับแต่ง
                    Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
                    g.DrawImage(original, rect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }

                // เพิ่มการแปลงเป็นภาพขาวดำ
                for (int x = 0; x < processed.Width; x++)
                {
                    for (int y = 0; y < processed.Height; y++)
                    {
                        Color pixel = processed.GetPixel(x, y);
                        int grayScale = (int)((pixel.R * 0.299) + (pixel.G * 0.587) + (pixel.B * 0.114));
                        
                        // ปรับความเข้มของสี (threshold)
                        int threshold = 128;
                        int newValue = grayScale < threshold ? 0 : 255;
                        
                        processed.SetPixel(x, y, Color.FromArgb(pixel.A, newValue, newValue, newValue));
                    }
                }

                return processed;
            }
            catch
            {
                return new Bitmap(original);
            }
        }

        private string ExtractQANumber(Bitmap image)
        {
            try
            {
                using (var page = _tesseract.Process(image))
                {
                    var text = page.GetText();
                    var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    // ค้นหาข้อความที่ขึ้นต้นด้วย Q และมีความยาว 10-12 ตัวอักษร
                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        if (trimmedLine.StartsWith("Q") && 
                            trimmedLine.Length >= MIN_BARCODE_LENGTH && 
                            trimmedLine.Length <= MAX_BARCODE_LENGTH)
                        {
                            // ตรวจสอบว่าเป็นรหัสที่ถูกต้อง (ตัวอักษรและตัวเลขเท่านั้น)
                            if (trimmedLine.All(c => char.IsLetterOrDigit(c)))
                            {
                                return trimmedLine;
                            }
                                                }
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                if (_tesseract != null)
                {
                    _tesseract.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private System.ComponentModel.IContainer components = null;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Barcode Rename";
        }
    }
}
