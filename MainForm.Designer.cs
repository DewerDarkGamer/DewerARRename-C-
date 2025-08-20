namespace BarcodeRename
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TextBox txtResult;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.txtResult = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();

            // btnSelectFile
            this.btnSelectFile.Location = new System.Drawing.Point(20, 20);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(120, 30);
            this.btnSelectFile.Text = "Select Image";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);

            // pictureBox
            this.pictureBox.Location = new System.Drawing.Point(20, 70);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(400, 200);
            this.pictureBox.TabStop = false;

            // txtResult
            this.txtResult.Location = new System.Drawing.Point(20, 300);
            this.txtResult.Name = "txtResult";
            this.txtResult.Size = new System.Drawing.Size(400, 23);

            // MainForm
            this.ClientSize = new System.Drawing.Size(500, 400);
            this.Controls.Add(this.btnSelectFile);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.txtResult);
            this.Name = "MainForm";
            this.Text = "Barcode Rename";

            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
