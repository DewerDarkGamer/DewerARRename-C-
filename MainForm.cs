using System.Drawing;
using ZXing;
using ZXing.Windows.Compatibility;
using Tesseract;

namespace BarcodeRename
{
    public partial class MainForm : Form
    {
        private readonly IBarcodeReader _reader;
        private string _imagePath = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            
            // สร้าง BarcodeReader แบบใหม่
            _reader = new BarcodeReader
            {
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.QR_CODE,
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.EAN_8
                    }
                }
            };
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _imagePath = openFileDialog.FileName;
                    ProcessImage(_imagePath);
                }
            }
        }

        private string ReadBarcode(Bitmap image)
        {
            try
            {
                using (var barcodeBitmap = new Bitmap(image))
                {
                    LuminanceSource source = new BitmapLuminanceSource(barcodeBitmap);
                    var binarizer = new ZXing.Common.HybridBinarizer(source);
                    var binBitmap = new BinaryBitmap(binarizer);
                    var result = _reader.Decode(binBitmap);
                    return result?.Text ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading barcode: {ex.Message}", "Barcode Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }
        }

        private string PerformOcr(Bitmap bitmap)
        {
            try
            {
                using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(_imagePath);
                using var page = engine.Process(img);
                return page.GetText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error performing OCR: {ex.Message}", "OCR Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }
        }

        private void ProcessImage(string imagePath)
        {
            try
            {
                using var bitmap = new Bitmap(imagePath);
                
                // อ่าน Barcode
                string barcodeText = ReadBarcode(bitmap);
                
                // ทำ OCR
                string ocrText = PerformOcr(bitmap);
                
                // แสดงผลลัพธ์
                string result = "Results:\n";
                if (!string.IsNullOrEmpty(barcodeText))
                    result += $"Barcode: {barcodeText}\n";
                if (!string.IsNullOrEmpty(ocrText))
                    result += $"OCR Text: {ocrText}";
                
                MessageBox.Show(result, "Processing Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
