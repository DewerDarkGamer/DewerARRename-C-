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

            var reader = new BarcodeReader();
            var result = reader.Decode(bitmap);

            string newName = result?.Text;

            if (string.IsNullOrEmpty(newName))
            {
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    using (var page = engine.Process(bitmap))
                    {
                        newName = page.GetText().Trim();
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
