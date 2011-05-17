using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Simple_Image_Resizer.Extensions;

namespace Simple_Image_Resizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int totalCount = 0;
        int processedCount = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbInputFormat.SelectedIndex = 0;
            cmbOutputFormat.SelectedIndex = 0;
            cmbRate.SelectedIndex = 6;
        }

        private void txtFolder_DoubleClick(object sender, EventArgs e)
        {
            btnFolderSelect_Click(sender, e);
        }

        private void btnFolderSelect_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                txtFolder.Text = folderBrowserDialog1.SelectedPath;

                totalCount = Directory.GetFiles(txtFolder.Text, cmbInputFormat.Text).Count();
                lblFileCount.Text = totalCount.ToString();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                totalCount = Directory.GetFiles(txtFolder.Text, cmbInputFormat.Text).Count();
                if (totalCount == 0)
                    throw new Exception("The selected directory is empty.");

                processedCount = 0;
                updateProgressBar();

                var args = new WorkerArguments();
                args.FileTypes = cmbInputFormat.Text;
                args.Path = txtFolder.Text;
                args.Rate = Int32.Parse(cmbRate.Text);

                lblFileCount.Text = totalCount.ToString();

                btnStart.Enabled = false;
                backgroundWorker1.RunWorkerAsync(args);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStart.Enabled = true;
            }

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = e.Argument as WorkerArguments;

            var dir = Directory.CreateDirectory(args.Path);
            var files = dir.GetFiles(args.FileTypes);

            var targetDir = Directory.CreateDirectory(args.Path + "\\Resized");

            var rate = args.Rate.ToDouble() / 100;

            foreach (var file in files)
            {
                using (Image originalImage = System.Drawing.Image.FromFile(file.FullName))
                {
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

                    backgroundWorker1.ReportProgress(0);
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            processedCount++;

            lblCount.Text = processedCount.ToString() + " / " + totalCount.ToString();

            updateProgressBar();
        }


        private void updateProgressBar()
        {
            progressBar.Value = (int)Math.Floor((processedCount.ToDouble() / totalCount.ToDouble()) * 100);
        }


        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MessageBox.Show("Finished!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
