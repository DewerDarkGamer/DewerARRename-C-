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
            // ... โค้ดส่วน constructor เหมือนเดิม ...
        }

        // ... โค้ดส่วน SelectFiles และ SelectFolder เหมือนเดิม ...

        private void ProcessFiles(string[] files)
        {
            _logListBox.Items.Clear();

            foreach (string filePath in files)
            {
                try
                {
                    List<string> barcodes;
                    using (var bitmap = new Bitmap(filePath))
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, bitmap.RawFormat);
                        using var tempBitmap = new Bitmap(ms);
                        barcodes = ReadAllBarcodes(tempBitmap);
                    }

                    if (barcodes.Any())
                    {
                        // เรียงลำดับ barcodes ตามความยาวจากมากไปน้อย
                        var sortedBarcodes = barcodes
                            .OrderByDescending(b => b.Length)
                            .ToList();

                        _logListBox.Items.Add($"Found {barcodes.Count} barcodes in: {Path.GetFileName(filePath)}");
                        foreach (var barcode in sortedBarcodes)
                        {
                            _logListBox.Items.Add($"  - {barcode} (Length: {barcode.Length})");
                        }

                        // เลือก barcode ที่ยาวที่สุดและมีความยาวมากกว่าหรือเท่ากับค่าขั้นต่ำ
                        var selectedBarcode = sortedBarcodes
                            .FirstOrDefault(b => b.Length >= MIN_BARCODE_LENGTH);

                        if (selectedBarcode != null)
                        {
                            string directory = Path.GetDirectoryName(filePath)!;
                            string extension = Path.GetExtension(filePath);
                            string newFilePath = Path.Combine(directory, $"{selectedBarcode}{extension}");

                            int counter = 1;
                            while (File.Exists(newFilePath))
                            {
                                newFilePath = Path.Combine(directory, $"{selectedBarcode}_{counter}{extension}");
                                counter++;
                            }

                            Thread.Sleep(100);
                            
                            File.Move(filePath, newFilePath);
                            _logListBox.Items.Add($"Renamed using longest barcode: {Path.GetFileName(filePath)} -> {Path.GetFileName(newFilePath)}");
                        }
                        else
                        {
                            _logListBox.Items.Add($"Skipped: No barcode meets minimum length requirement ({MIN_BARCODE_LENGTH} characters)");
                        }
                    }
                    else
                    {
                        _logListBox.Items.Add($"No barcode found in: {Path.GetFileName(filePath)}");
                    }

                    _logListBox.Items.Add(""); // เพิ่มบรรทัดว่างระหว่างไฟล์
                }
                catch (Exception ex)
                {
                    _logListBox.Items.Add($"Error processing {Path.GetFileName(filePath)}: {ex.Message}");
                }
            }

            // แสดงสรุปที่ด้านล่างของล็อก
            _logListBox.Items.Add("");
            _logListBox.Items.Add($"Process completed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _logListBox.Items.Add($"Minimum barcode length: {MIN_BARCODE_LENGTH} characters");
        }

        private List<string> ReadAllBarcodes(Bitmap image)
        {
            try
            {
                var barcodes = new List<string>();
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

                return barcodes;
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
