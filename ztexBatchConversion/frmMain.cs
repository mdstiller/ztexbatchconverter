using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Threading;
using _3DS.GPU;

namespace ztexBatchConversion
{
    public partial class frmMain : Form
    {
        private string sOpenSelectedPath = "";
        private string sBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private Coba.ztexformat ztexFormat;
        private int iFileCount = 0;
        private BackgroundWorker oBackgroundWorker = new BackgroundWorker();
        private string sConversionStatusOutput = "";
        private string sExtractSelectedPath = "";

        public frmMain()
        {
            InitializeComponent();
            //Shown += new EventHandler(frmMain_Shown);
            oBackgroundWorker.WorkerSupportsCancellation = true;
            oBackgroundWorker.WorkerReportsProgress = true;
            oBackgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorker_DoWork);
            oBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker_ProgressChanged);
            oBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
        }    

        /*void frmMain_Shown(object sender, EventArgs e)
        {
            // Start the background worker
            //oBackgroundWorker.RunWorkerAsync();
        }*/

        // On worker thread so do our thing!
        void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker oBackgroundWorker = sender as BackgroundWorker;

            string[] oSelectedFiles = (string[])e.Argument;

            for (int i = 0; i < oSelectedFiles.Count(); i++)
            {
                convertZtexFiles(oSelectedFiles[i], i);
                if (oBackgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
            }

            sConversionStatusOutput = "Conversion Complete.";
            oBackgroundWorker.ReportProgress(0);

            MessageBox.Show("Conversion of " + iFileCount + " files complete.", "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);            
        }
        // Back on the 'UI' thread so we can update the progress bar
        void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // The progress percentage is a property of e            
            pgbProgress.Value = e.ProgressPercentage;
            lblCount.Text = "Status: Converted " + e.ProgressPercentage + " of " + iFileCount.ToString() + " files of type ZTEX.";
            txtOutput.AppendText(sConversionStatusOutput + "\r\n");
        }

        void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string sCancelledMessage = "Conversion was cancelled.";

            if (e.Cancelled)
            {
                MessageBox.Show(sCancelledMessage, "Conversion Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOutput.Text = sCancelledMessage;
            }

            fileToolStripMenuItem.Enabled = true;
            btnBrowse.Enabled = true;
            btnConvert.Enabled = true;
            btnExit.Text = "Exit";
            lblCount.Text = "";
            pgbProgress.Value = 0;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlgFolderBrowser = new FolderBrowserDialog();
            dlgFolderBrowser.Description = "Select Source Folder...";
            dlgFolderBrowser.SelectedPath = sBaseDirectory;
            dlgFolderBrowser.ShowDialog();
            sOpenSelectedPath = dlgFolderBrowser.SelectedPath;
            countZtextFiles();
        }

        private void countZtextFiles()
        {
            iFileCount = 0;

            if (sOpenSelectedPath.Equals(""))
            {
                lblCount.Text = "No folder was selected.";
                btnConvert.Enabled = false;
                return;
            }

            iFileCount = Directory.GetFiles(sOpenSelectedPath, "*.tex").Count();
            string sOutput = "";

            if (iFileCount > 0)
            {
                sOutput = "Found " + iFileCount.ToString() + " files of type ZTEX.";
                btnConvert.Enabled = true;
            }
            else
            {
                sOutput = "No files of type ZTEX found.";
                btnConvert.Enabled = false;
            }

            lblCount.Text = sOutput;
            pgbProgress.Maximum = iFileCount;
        }

        private void convertZtexFiles(string sSelectedFile, int iFileNumber)
        {
            string sFormatString = "[{0}] Converting {1} to png {2}.";
            string oBaseFile = sSelectedFile;            
            string sCurrentFile = sSelectedFile.Remove(0, sSelectedFile.LastIndexOf('\\') + 1);
            bool bFailed = false;

            ztexFormat = new Coba.ztexformat(sSelectedFile);            

            for (int index = 0; index < this.ztexFormat.Header.Count; ++index)
            {
                try
                {
                    IntPtr oPointer = new IntPtr(this.ztexFormat.Entry[index].Size);

                    byte[] numArray = new byte[oPointer.ToInt32()];

                    using (FileStream fileStream = new FileStream(sSelectedFile, FileMode.Open))
                    {
                        fileStream.Position = (long)this.ztexFormat.Entry[index].Offset;
                        fileStream.Read(numArray, 0, numArray.Length);
                    }

                    string sExtractedFileName = (this.ztexFormat.Entry[index].Name).Replace(':', '_').Replace('/', '_') + ".png";

                    using (MemoryStream memory = new MemoryStream())
                    {
                        using (FileStream fileStream = new FileStream(sExtractSelectedPath + "\\" + sExtractedFileName, FileMode.Create, FileAccess.Write))
                        {
                            Bitmap oExtractedBitmap = this.getTexBitmap(numArray, index, this.ztexFormat.Entry[index].TextureFormat);
                            oExtractedBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                            byte[] bytes = memory.ToArray();
                            fileStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                catch
                {
                    bFailed = true;
                    sConversionStatusOutput = String.Format(sFormatString, DateTime.Now, sCurrentFile, "failed");                    
                }
            }       

            if(!bFailed)
                sConversionStatusOutput = String.Format(sFormatString, DateTime.Now, sCurrentFile, "passed");
    
            oBackgroundWorker.ReportProgress(iFileNumber + 1);
        }                       

        private void btnConvert_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlgFolderBrowser = new FolderBrowserDialog();
            dlgFolderBrowser.Description = "Select Destination Folder...";
            dlgFolderBrowser.SelectedPath = sBaseDirectory;
            DialogResult oDialogResult = dlgFolderBrowser.ShowDialog();
            
            if (oDialogResult == DialogResult.Cancel)
                return;

            btnBrowse.Enabled = false;
            btnConvert.Enabled = false;
            btnExit.Text = "Cancel Conversion";
            fileToolStripMenuItem.Enabled = false;

            sExtractSelectedPath = dlgFolderBrowser.SelectedPath;  
            string[] oSelectedFiles =  Directory.GetFiles(sOpenSelectedPath);

            txtOutput.Text = "Converting files in folder " + getDirectory(oSelectedFiles[0]) + "\r\n"
                           + "Storing converted files in folder " + sExtractSelectedPath + " \r\n";

            oBackgroundWorker.RunWorkerAsync(argument:oSelectedFiles);
        }

        private string getDirectory(string sFileName)
        {
            int iDirectory = sFileName.LastIndexOf('\\');
            return sFileName.Remove(iDirectory, sFileName.Length - iDirectory);
        }

        private Bitmap getTexBitmap(byte[] binary, int index, uint texFormat)
        {
            try
            {
                uint num = texFormat;
                if (num <= 5U)
                {
                    if ((int)num == 1)
                        return Textures.ToBitmap(binary, 0, (int)this.ztexFormat.Entry[index].W, (int)this.ztexFormat.Entry[index].H, Textures.ImageFormat.RGB565, false);
                    if ((int)num == 5)
                        return Textures.ToBitmap(binary, 0, (int)this.ztexFormat.Entry[index].W, (int)this.ztexFormat.Entry[index].H, Textures.ImageFormat.RGBA4, false);
                }
                else
                {
                    switch (num)
                    {
                        case 9:
                            return Textures.ToBitmap(binary, 0, (int)this.ztexFormat.Entry[index].W, (int)this.ztexFormat.Entry[index].H, Textures.ImageFormat.RGBA8, false);
                        case 24:
                            return Textures.ToBitmap(binary, 0, (int)this.ztexFormat.Entry[index].W, (int)this.ztexFormat.Entry[index].H, Textures.ImageFormat.ETC1, false);
                        case 25:
                            return Textures.ToBitmap(binary, 0, (int)this.ztexFormat.Entry[index].W, (int)this.ztexFormat.Entry[index].H, Textures.ImageFormat.ETC1A4, false);
                    }
                }
                return (Bitmap)null;
            }
            catch
            {                
            }
            return (Bitmap)null;

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            dlgAbout oAbout = new dlgAbout();
            oAbout.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text.Equals("Cancel Conversion"))
                oBackgroundWorker.CancelAsync();
            else            
                Application.Exit();            
        }
    }
}