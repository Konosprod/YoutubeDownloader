using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace YoutubeDownloader
{
    public partial class downloadDialog : Form
    {
        Queue<Video> listVideo = new Queue<Video>();
        WebClient weblcient = new WebClient();

        public downloadDialog(List<Video> l)
        {
            InitializeComponent();
            foreach(var v in l)
            {
                listVideo.Enqueue(v);
            }

            downloadFile();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (weblcient.IsBusy)
            {
                weblcient.CancelAsync();

            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void downloadDialog_Load(object sender, EventArgs e)
        {

        }

        private void downloadFile()
        {
            if (listVideo.Any())
            {
                var video = listVideo.Dequeue();

                weblcient.DownloadProgressChanged += client_DownloadProgressChanged;
                weblcient.DownloadFileCompleted += client_DownloadFileCompleted;

                label1.Text = Uri.UnescapeDataString(video.title);
                weblcient.DownloadFileAsync(new Uri(video.url), video.title + ".mp4");
            }
            else
            {
                buttonCancel.Text = "Terminer";
            }
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                if (!e.Cancelled)
                {
                    MessageBox.Show(e.Error.ToString());
                }
            }
            else
            {
                downloadFile();
            }
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            label2.Text = Math.Truncate(percentage).ToString() + "%";
            progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());   
        }
    }
}
