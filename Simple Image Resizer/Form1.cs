using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Simple_Image_Resizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            cmbOutputFormat.SelectedIndex = 0;
        }

        ImageFormat saveFormat = ImageFormat.Jpeg;
        Stopwatch stopWatch = new Stopwatch();
        string folder, inFileType, outFileType;
        double rate;

        private void cmbOutputFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbOutputFormat.SelectedIndex == 0) saveFormat = ImageFormat.Jpeg;
            if (cmbOutputFormat.SelectedIndex == 1) saveFormat = ImageFormat.Bmp;
        }

        private void txtFolder_DoubleClick(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }
        private void btnFolderSelect_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void txtFolder_TextChanged(object sender, EventArgs e)
        {
            RecalculateFileCount();
            RecalculateRateExample();
        }
        private void cmbInputFormat_TextUpdate(object sender, EventArgs e)
        {
            RecalculateFileCount();
            RecalculateRateExample();
        }
        private void cmbInputFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            RecalculateFileCount();
            RecalculateRateExample();
        }
        private void cmbRate_TextChanged(object sender, EventArgs e)
        {
            RecalculateRateExample();
        }

        private void RecalculateFileCount()
        {
            if (Directory.Exists(txtFolder.Text))
            {
                progressBar.Maximum = Directory.GetFiles(txtFolder.Text, cmbInputFormat.Text).Length;
                lblFileCount.Text = progressBar.Maximum.ToString();
                btnStart.Enabled = (progressBar.Maximum != 0);
            }
            else
            {
                progressBar.Maximum = 0;
                lblFileCount.Text = "0";
                btnStart.Enabled = false;
            }
        }
        private void RecalculateRateExample()
        {
            if (Directory.Exists(txtFolder.Text) && progressBar.Maximum > 0)
            {
                using (Image originalImage = Image.FromFile(Directory.GetFiles(txtFolder.Text, cmbInputFormat.Text)[0]))
                {
                    var rate = double.Parse(cmbRate.Text) / 100d;

                    int newWidth = (int)(originalImage.Width * rate);
                    int newHeight = (int)(originalImage.Height * rate);

                    StringBuilder sb = new StringBuilder();
                    sb.Append("e.g.: First picture will be resized from ");
                    sb.Append(originalImage.Width).Append("x").Append(originalImage.Height);
                    sb.Append(" to ").Append(newWidth).Append("x").Append(newHeight);

                    lblRateExample.Text = sb.ToString();
                }
            }
            else
            {
                lblRateExample.Text = string.Empty;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                progressBar.Value = 0;
                stopWatch.Restart();

                this.folder = txtFolder.Text;
                this.inFileType = cmbInputFormat.Text;
                this.outFileType = cmbOutputFormat.Text;
                this.rate = double.Parse(cmbRate.Text) / 100d;

                btnStart.Enabled = false;
                backgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStart.Enabled = true;
            }
        }
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var files = new DirectoryInfo(folder).GetFiles(this.inFileType);
            var targetDir = Directory.CreateDirectory(this.folder + "\\Resized");

            try
            {
                Parallel.ForEach<FileInfo>(files, (file) =>
                {
                    using (Image originalImage = Image.FromFile(file.FullName))
                    {
                        int newWidth = (int)(originalImage.Width * this.rate);
                        int newHeight = (int)(originalImage.Height * this.rate);

                        Bitmap newBitmap = new Bitmap(originalImage, newWidth, newHeight);
                        string newFilename = Path.GetFileNameWithoutExtension(file.FullName) + Path.GetExtension(outFileType);
                        newBitmap.Save(targetDir.FullName + "\\" + newFilename, saveFormat);
                        newBitmap.Dispose();
                    }
                    backgroundWorker.ReportProgress(0);
                });
            }
            catch (AggregateException ex)
            {
                throw ex.InnerExceptions[0];
            }
        }
        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.PerformStep();
            lblCount.Text = progressBar.Value + " / " + progressBar.Maximum;
        }
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                stopWatch.Stop();
                MessageBox.Show("Finished!\nProcessing time: " + stopWatch.Elapsed.TotalSeconds, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            btnStart.Enabled = true;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string aboutText = "Copyright © 2011 João Carlos Pena, Arlindo Pereira.\n\nThis program is free software: you can redistribute it and/or modify\nit under the terms of the GNU General Public License as published by\nthe Free Software Foundation, either version 3 of the License, or\n(at your option) any later version.\n\nThis program is distributed in the hope that it will be useful,\nbut WITHOUT ANY WARRANTY; without even the implied warranty of\nMERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\nGNU General Public License for more details.\n\nYou should have received a copy of the GNU General Public License\nalong with this program.  If not, see <http://www.gnu.org/licenses/>.";
            MessageBox.Show(aboutText, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
