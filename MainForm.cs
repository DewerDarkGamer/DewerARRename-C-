using System.Drawing;
using ZXing;
using ZXing.Windows.Compatibility;
using Tesseract;

namespace BarcodeRename
{
    public partial class MainForm : Form
    {
        private readonly IBarcodeReader<Bitmap> _reader;
        private string _imagePath = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            
            // สร้าง BarcodeReader ด้วย MultiFormatReader
            _reader = new BarcodeReader<Bitmap>(null, null, null)
            {
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true
                }
            };
        }

        // เพิ่ม event handler ที่หายไป
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
            var result = _reader.Decode(image);
            return result?.Text ?? string.Empty;
        }

        private string PerformOcr(Bitmap bitmap)
        {
            using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            // แปลง Bitmap เป็น Pix
            using var img = Pix.LoadFromFile(_imagePath); // ใช้ไฟล์โดยตรงแทนการแปลง Bitmap
            using var page = engine.Process(img);
            return page.GetText();
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
                
                // TODO: แสดงผลลัพธ์ในหน้าจอหรือทำงานอื่นต่อ
                MessageBox.Show($"Barcode: {barcodeText}\nOCR Text: {ocrText}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
