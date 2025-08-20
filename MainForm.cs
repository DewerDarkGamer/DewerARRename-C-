using System.Drawing;
using ZXing;
using ZXing.Windows.Compatibility;
using Tesseract;

namespace BarcodeRename
{
    public partial class MainForm : Form
    {
        private readonly IBarcodeReader<Bitmap> _reader;
        
        public MainForm()
        {
            InitializeComponent();
            
            // สร้าง BarcodeReader ด้วย IBarcodeReader interface
            _reader = new BarcodeReader<Bitmap>
            {
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true
                }
            };
        }

        // ตัวอย่างเมธอดสำหรับอ่าน Barcode
        private string ReadBarcode(Bitmap image)
        {
            var result = _reader.Decode(image);
            return result?.Text ?? string.Empty;
        }

        // ตัวอย่างเมธอดสำหรับ OCR ด้วย Tesseract
        private string PerformOcr(Bitmap image)
        {
            using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            using var page = engine.Process(image);
            return page.GetText();
        }

        // ตัวอย่างการใช้งานใน event handler
        private void ScanButton_Click(object sender, EventArgs e)
        {
            using var image = new Bitmap("path_to_your_image.png");
            
            // อ่าน Barcode
            string barcodeText = ReadBarcode(image);
            
            // ทำ OCR
            string ocrText = PerformOcr(image);
            
            // ทำอย่างอื่นต่อไป...
        }
    }
}
