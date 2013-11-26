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
using System.Text.RegularExpressions;
using System.IO;


public struct Video
{
    public String url;
    public String title;
}

namespace YoutubeDownloader
{
    public partial class Form1 : Form
    {
        List<Video> listVideo = new List<Video>();
        WebClient webclient = new WebClient();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            if (lineURL.Text.Contains("youtube"))
            {
                getVideo(lineURL.Text);

            }
            else if(Regex.IsMatch(lineURL.Text, ".{18}"))
            {
                getVideos(lineURL.Text);
            }
            else
            {
                MessageBox.Show("Veuillez entrer soit un code de playlist soit l'url d'une vidéo.");
            }
            
            if(checkConvert.Checked)
            {  
                label2.Text = "Conversion...";
                convertVideos();
            }
            else
            {
                label2.Text = "Téléchargement...";
                downloadVideos();
            }
        }


        private void downloadVideos()
        {
            downloadDialog dd = new downloadDialog(listVideo);

            if(dd.ShowDialog() == DialogResult.OK)
            {
                label2.Text = "Terminé";
            }
        }

        private void convertVideos()
        {
            convertDialog cd = new convertDialog(listVideo);

            if(cd.ShowDialog() == DialogResult.OK)
            {
                label2.Text = "Terminé";
            }
        }

        private void getVideos(String playlist)
        {
            List<string> urls = new List<string>();

            String src = webclient.DownloadString("https://www.youtube.com/playlist?list=" + playlist);

            String nbVideo = Regex.Match(src, "videoCount\" content=\"([0-9]*)").Groups[1].ToString();

            label2.Text = "Récupération des informations...";
            toolStripProgressBar1.Maximum = int.Parse(nbVideo);
            for (int i = 1; i < int.Parse(nbVideo)+1; i++)
            {
                String url = Regex.Match(src, "v=.*index=" + i.ToString()).ToString();
                String tmp = "https://www.youtube.com/watch?" + url;
                
                if (!urls.Contains(tmp))
                {
                    statusLabel.Text = i.ToString() + "/" + int.Parse(nbVideo);
                    toolStripProgressBar1.Value = i;
                    urls.Add(tmp);
                    getVideo(tmp);
                }
            }
        }

        private bool isUrlValid(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

        private void getVideo(String url)
        { 
            bool isOk = false;
            int chosenOne = 0;
            String title = "";
            String baseUrl = "";
            String sign = "";

            while (!isOk)
            {
                String html = webclient.DownloadString(url);

                title = Regex.Match(html, "title\" content=\".*").ToString();
                if (title != "")
                {
                    title = title.Remove(0, 16);
                    title = title.Remove(title.LastIndexOf('"'));
                    string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                    Regex rn = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                    title = rn.Replace(title, "_");
                }
                else
                {
                    return;
                }

                html = Uri.UnescapeDataString(Regex.Match(html, "url_encoded_fmt_stream_map\": \"(.+?)\"",
                    RegexOptions.Singleline).Groups[1].ToString());

                MatchCollection matchesUrls = Regex.Matches(html, "url=(.+?)\\\\", RegexOptions.Singleline);
                MatchCollection matchesSigs = Regex.Matches(html, "sig=([A-Z0-9\\.]*)", RegexOptions.Singleline);

                if (!(matchesSigs.Count > 0))
                {
                    return;
                }

                for (int i = 0; i < matchesUrls.Count; i++)
                {
                    if (matchesUrls[i].Groups[1].ToString().Contains("itag=18") && chosenOne == 0)
                    {
                        chosenOne = i;
                    }

                    if (matchesUrls[i].Groups[1].ToString().Contains("itag=22"))
                    {
                        chosenOne = i;
                    }
                }
                baseUrl = matchesUrls[chosenOne].Groups[1].ToString();
                sign = "&signature=" + matchesSigs[chosenOne].Groups[1].ToString();
                isOk = isUrlValid(baseUrl + sign);
            }

            Video v;
            v.title = title;
            v.url = baseUrl + sign;
            
            listVideo.Add(v);
        }
    }
}
