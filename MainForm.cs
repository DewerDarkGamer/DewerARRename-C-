// MainForm.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using ZXing;
using ZXing.Windows;
using Tesseract;

namespace BarcodeRename
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (Bitmap bitmap = new Bitmap(ofd.FileName))
                    {
                        // ✅ ใช้ Generic BarcodeReader<Bitmap>
                        var reader = new BarcodeReader<Bitmap>();
                        var result = reader.Decode(bitmap);

                        if (result != null)
                            txtResult.Text = "Barcode: " + result.Text;
                        else
                            txtResult.Text = "ไม่พบ Barcode";

                        // ✅ ส่งต่อไป Tesseract OCR
                        using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                        {
                            // แปลง Bitmap → Pix
                            using (var pix = PixConverter.ToPix(bitmap))
                            using (var page = engine.Process(pix))
                            {
                                txtResult.AppendText(Environment.NewLine + "OCR: " + page.GetText());
                            }
                        }
                    }
                }
            }
        }
    }
}
