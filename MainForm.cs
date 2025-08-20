using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ZXing;
using Tesseract;

namespace BarcodeRename
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent(); // UI ของ WinForms
        }

        private void ProcessImage(string filePath)
        {
            Bitmap bitmap = (Bitmap)Image.FromFile(filePath);

            var reader = new ZXing.BarcodeReader<Bitmap>();
            var result = reader.Decode(bitmap);

            string newName = result?.Text;

            if (string.IsNullOrEmpty(newName))
            {
                using (var engine = new Tesseract.TesseractEngine("./tessdata", "eng", Tesseract.EngineMode.Default))
{
    using (var pix = Tesseract.PixConverter.ToPix(bitmap))
    {
        using (var result = engine.Process(pix))
        {
            txtResult.Text = result.GetText();
        }
    }
}

            }

            if (!string.IsNullOrEmpty(newName))
            {
                string dir = Path.GetDirectoryName(filePath);
                string ext = Path.GetExtension(filePath);
                string newFile = Path.Combine(dir, newName + ext);

                try
                {
                    File.Move(filePath, newFile);
                }
                catch
                {
                    MessageBox.Show("ไม่สามารถเปลี่ยนชื่อไฟล์: " + filePath);
                }
            }
        }
    }
}
