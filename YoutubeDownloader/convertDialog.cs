using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace YoutubeDownloader
{
    public partial class convertDialog : Form
    {
        Queue<Video> listVideo = new Queue<Video>();
        Process p = new Process();  
        private delegate void progressBarDelegateHandler();
        private delegate void labelDelegateHandler(string txt);

        public convertDialog(List<Video> l)
        {
            InitializeComponent();

            foreach(var v in l)
            {
                listVideo.Enqueue(v);
            }

            progressBar1.Maximum = listVideo.Count;
            convertVideo();
            button1.Text = "Terminé";
        }

        private void convertVideo()
        {
            if (listVideo.Any())
            {
                Video v = listVideo.Dequeue();

                if(label1.InvokeRequired)
                {
                    label1.Invoke(new labelDelegateHandler(majLabel), v.title);
                }
                else
                {
                    majLabel(v.title);
                }

                p.EnableRaisingEvents = true;
                p.Exited += p_Exited;
                p.StartInfo.Arguments = String.Format(@"""{0}"" ""{1}""", v.url, v.title + ".mp3");
                p.StartInfo.FileName = "launch.bat";
                p.Start();

                if(progressBar1.InvokeRequired)
                {
                    progressBar1.Invoke(new progressBarDelegateHandler(majProgressBar));
                }
                else
                {
                    majProgressBar();
                }
            }
        }

        void p_Exited(object sender, EventArgs e)
        {
            convertVideo();
        }

        private void majProgressBar()
        {
            progressBar1.Value += 1;
            labelPercent.Text = (progressBar1.Value / progressBar1.Maximum * 100).ToString() + "%";           
        }

        private void majLabel(string txt)
        {
            label1.Text = txt;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!p.HasExited)
            {
                p.Kill();
            }
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
