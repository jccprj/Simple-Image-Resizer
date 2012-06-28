using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

        long startTimeTicks = 0;

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

        private void txtFolder_TextChanged(object sender, EventArgs e)
        {
            cmbRate_TextChanged(sender, e);
        }

        private void btnFolderSelect_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void cmbRate_TextChanged(object sender, EventArgs e)
        {
            if(!Directory.Exists(txtFolder.Text))
            {
                return;
            }
            
            var dir = new DirectoryInfo(txtFolder.Text);
            var files = dir.GetFiles(cmbInputFormat.Text);

            //Update file count
            progressBar.Maximum = files.Count();
            lblFileCount.Text = progressBar.Maximum.ToString();

            //Update rate label 
            if (progressBar.Maximum > 0)
            {
                var file = files.FirstOrDefault();

                using (Image originalImage = Image.FromFile(file.FullName))
                {
                    var rate = double.Parse(cmbRate.Text) / 100d;

                    int newWidth = (int)Math.Ceiling(originalImage.Width * rate);
                    int newHeight = (int)Math.Ceiling(originalImage.Height * rate);

                    StringBuilder sb = new StringBuilder();
                    sb.Append("e.g.: First picture will be resized from ");
                    sb.Append(originalImage.Width);
                    sb.Append("x");
                    sb.Append(originalImage.Height);
                    sb.Append(" to ");
                    sb.Append(newWidth);
                    sb.Append("x");
                    sb.Append(newHeight);

                    lblRateExample.Text = sb.ToString();
                }
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
                startTimeTicks = DateTime.Now.Ticks;

                progressBar.Maximum = Directory.GetFiles(txtFolder.Text, cmbInputFormat.Text).Count();
                if (progressBar.Maximum == 0)
                {
                    MessageBox.Show("The selected directory is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnStart.Enabled = true;
                    return;
                }

                progressBar.Value = 0;

                var args = new WorkerArguments();
                args.FileTypes = cmbInputFormat.Text;
                args.Path = txtFolder.Text;
                args.Rate = Int32.Parse(cmbRate.Text);

                lblFileCount.Text = progressBar.Maximum.ToString();
                
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
                            backgroundWorker1.ReportProgress(0);
                        }
                    });

            }
            catch (AggregateException ex)
            {
                throw ex.InnerExceptions.First();
            }

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.PerformStep();
            lblCount.Text = progressBar.Value + " / " + progressBar.Maximum.ToString();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                var ts = new TimeSpan(DateTime.Now.Ticks - startTimeTicks);

                MessageBox.Show("Finished!\nProcessing time: " + ts.TotalSeconds, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
