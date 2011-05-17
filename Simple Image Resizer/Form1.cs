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

        private void txtFolder_TextChanged(object sender, EventArgs e)
        {
            cmbRate_TextChanged(sender, e);
        }


        private void btnFolderSelect_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                txtFolder.Text = folderBrowserDialog1.SelectedPath;
                txtFolder_TextChanged(sender, e);
            }
        }

        private void cmbRate_TextChanged(object sender, EventArgs e)
        {
            var dir = Directory.CreateDirectory(txtFolder.Text);
            var files = dir.GetFiles(cmbInputFormat.Text);

            //Update file count
            totalCount = files.Count();
            lblFileCount.Text = totalCount.ToString();

            //Update rate label 
            if (totalCount > 0)
            {
                var file = files.FirstOrDefault();

                using (Image originalImage = System.Drawing.Image.FromFile(file.FullName))
                {
                    var rate = cmbRate.Text.ToDouble() / 100;

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
            try
            {
                totalCount = Directory.GetFiles(txtFolder.Text, cmbInputFormat.Text).Count();
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

            

            foreach (var file in files)
            {
                using (Image originalImage = System.Drawing.Image.FromFile(file.FullName))
                {
                    var rate = args.Rate.ToDouble() / 100;
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
            MessageBox.Show("This small open source software was created by João Carlos Pena on 2011-05-16.", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        
       


    }
}
