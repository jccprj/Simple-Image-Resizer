using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        }

        Stopwatch stopWatch = new Stopwatch();

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
            }
            else
            {
                progressBar.Maximum = 0;
                lblFileCount.Text = "0";
            }
        }
        private void RecalculateRateExample()
        {
            if (Directory.Exists(txtFolder.Text) && progressBar.Maximum > 0)
            {
                using (Image originalImage = Image.FromFile(Directory.GetFiles(txtFolder.Text, cmbInputFormat.Text)[0]))
                {
                    var rate = double.Parse(cmbRate.Text) / 100d;

                    int newWidth = (int)Math.Ceiling(originalImage.Width * rate);
                    int newHeight = (int)Math.Ceiling(originalImage.Height * rate);

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
            if (!Directory.Exists(txtFolder.Text))
            {
                MessageBox.Show("The selected directory does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                stopWatch.Restart();

                progressBar.Maximum = Directory.GetFiles(txtFolder.Text, cmbInputFormat.Text).Length;
                if (progressBar.Maximum == 0)
                {
                    MessageBox.Show("The selected directory is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                progressBar.Value = 0;

                var args = new WorkerArguments();
                args.FileTypes = cmbInputFormat.Text;
                args.Path = txtFolder.Text;
                args.Rate = Int32.Parse(cmbRate.Text);

                lblFileCount.Text = progressBar.Maximum.ToString();

                btnStart.Enabled = false;
                backgroundWorker.RunWorkerAsync(args);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStart.Enabled = true;
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = e.Argument as WorkerArguments;

            var files = new DirectoryInfo(args.Path).GetFiles(args.FileTypes);

            var targetDir = Directory.CreateDirectory(args.Path + "\\Resized");

            try
            {
                Parallel.ForEach<FileInfo>(files, (file, state) =>
                    {
                        if (state.ShouldExitCurrentIteration) return;

                        using (Image originalImage = Image.FromFile(file.FullName))
                        {
                            var rate = args.Rate / 100d;
                            int newWidth = (int)Math.Ceiling(originalImage.Width * rate);
                            int newHeight = (int)Math.Ceiling(originalImage.Height * rate);

                            using (Bitmap newImage = new Bitmap(newWidth, newHeight))
                            {
                                using (Graphics graphic = Graphics.FromImage((Image)newImage))
                                {
                                    graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                    graphic.SmoothingMode = SmoothingMode.HighQuality;
                                    graphic.CompositingQuality = CompositingQuality.HighQuality;

                                    graphic.DrawImage(originalImage, 0, 0, newWidth, newHeight);

                                    newImage.Save(targetDir.FullName + "\\" + file.Name, ImageFormat.Jpeg);
                                }
                            }
                            backgroundWorker.ReportProgress(0);
                        }
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
