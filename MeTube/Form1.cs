using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Declarations;
using Declarations.Events;
using Declarations.Media;
using Declarations.Players;
using Implementation;
using System.Diagnostics;
using System.Net;
namespace MeTube3
{
    public partial class Form1 : Form
    {
        private class UISync
        {
            private static ISynchronizeInvoke Sync;
            public static void Init(ISynchronizeInvoke sync) { Sync = sync; }
            public static void Execute(Action action) { Sync.BeginInvoke(action, null); }
        }

        IMediaPlayerFactory m_factory;
        IDiskPlayer m_player;

        struct VideoInstace
        {
            public string URL;
            public string Format;
            public int width;
            public int height;
            public bool hasAudio;
        };

        struct AudioInstace
        {
            public string URL;
            public Int64 fileSize;
        };

        class MeTube
        {
            public string youTubeVideoID = "Pr8rOmTcf5Q";

            public List<VideoInstace> videoInstances;
            public List<AudioInstace> audioInstances;

            public List<string> RunYoutubeDL(string vidID, string parametre)
            {
                if (!File.Exists("youtube-dl.exe"))
                {
                    WebClient client = new WebClient();
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                    client.DownloadFile("https://yt-dl.org/downloads/2018.04.25/youtube-dl.exe", "youtube-dl.exe");
                }

                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "youtube-dl.exe",
                        Arguments = vidID + " " + parametre,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc.Start();

                var ret = new List<string>();
                while (!proc.StandardOutput.EndOfStream)
                    ret.Add(proc.StandardOutput.ReadLine());
                return ret;
            }

            public string ReadElement(string str)
            {
                if (str.Contains(" "))
                    return str.Substring(0, str.IndexOf(" "));
                else
                    return str;
            }

            public bool RetreiveVideoStreams(string vidID, out List<VideoInstace> videoInstances, out List<AudioInstace> audioInstances)
            {
                //comboBoxQuality.Items.Clear();
                var formatData = RunYoutubeDL(vidID, "--list-formats");
                var urlData = RunYoutubeDL(vidID, "--get-url --all-formats");
                List<VideoInstace> retvideoInstances = new List<VideoInstace>();
                List<AudioInstace> retaudioInstances = new List<AudioInstace>();
                var retVideoInstance = new VideoInstace();
                var retAudioInstance = new AudioInstace();

                int streamID = 0;
                foreach (var formatLine in formatData)
                {
                    int formatID;
                    if (Int32.TryParse(ReadElement(formatLine), out formatID))
                    {
                        var res = formatLine.Substring(24, 10);
                        if (res.Contains("audio only"))
                        {
                            // Audio Streams
                            float fileSize;
                            int lastComma = formatLine.LastIndexOf(", ") + 2;
                            var fileSizeStr = formatLine.Substring(lastComma, formatLine.Length - 3 - lastComma);
                            float.TryParse(ReadElement(fileSizeStr), out fileSize);
                            retAudioInstance.fileSize = (int)(fileSize * 1024 * 1024);
                            retAudioInstance.URL = urlData[streamID];
                            retaudioInstances.Add(retAudioInstance);
                            streamID++;

                        }
                        else
                        {
                            // Video Streams
                            retVideoInstance.Format = formatLine.Substring(13, 11).Trim();
                            Int32.TryParse(ReadElement(res.Substring(0, res.IndexOf('x'))), out retVideoInstance.width);
                            Int32.TryParse(ReadElement(res.Substring(res.IndexOf('x') + 1)), out retVideoInstance.height);
                            var destciption = formatLine.Substring(36);
                            retVideoInstance.hasAudio = !destciption.Contains("video only,");
                            retVideoInstance.URL = urlData[streamID];
                            retvideoInstances.Add(retVideoInstance);
                            //comboBoxQuality.Items.Add();

                            streamID++;

                        }
                    }
                }
                videoInstances = retvideoInstances;
                audioInstances = retaudioInstances;
                return true;
            }

            public void GoFullscreen(bool fullscreen)
            {
                if (fullscreen)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    this.Bounds = Screen.PrimaryScreen.Bounds;
                }
                else
                {
                    this.WindowState = FormWindowState.Maximized;
                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                }
            }

            public void PlayVideo(string videoURL)
            {
                m_factory = new MediaPlayerFactory(true);
                m_player = m_factory.CreatePlayer<IDiskPlayer>();
                m_player.WindowHandle = VideoPanel.Handle;
                UISync.Init(this);
                IMedia stream = m_factory.CreateMedia<IMedia>(videoURL);
                IVideoPlayer m_renderer = m_factory.CreatePlayer<IVideoPlayer>();
                m_renderer.WindowHandle = VideoPanel.Handle;
                m_player.Open(stream);
                stream.Parse(false);
                m_player.Play();
            }

            public MeTube()
            {
                var youTubeVideoID = "Pr8rOmTcf5Q";

                var videoInstances = new List<VideoInstace>();
                var audioInstances = new List<AudioInstace>();
                RetreiveVideoStreams(youTubeVideoID, out videoInstances, out audioInstances);
                PlayVideo(videoInstances[0].URL);
            }
        }

        public Form1() { InitializeComponent();}

        public void Form1_Load(object sender, EventArgs e)
        {
            MeTube tube;
            tube.
        }

        public void MeTube2()
        {
            var youTubeVideoID = "Pr8rOmTcf5Q";

            var videoInstances = new List<VideoInstace>();
            var audioInstances = new List<AudioInstace>();
            RetreiveVideoStreams(youTubeVideoID, out videoInstances, out audioInstances);
            comboBoxQuality.Items.Clear();
            foreach(var video in videoInstances)
            {
                comboBoxQuality.Items.Add(video.width + " x "+video.height+ "  -  "+ video.Format);

            }
            if (comboBoxQuality.Items.Count > 0)
            {
                comboBoxQuality.SelectedIndex = 0;
                PlayVideo(videoInstances[0].URL);
            }

        }

        public void comboBoxQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            int index = comboBox.SelectedIndex;
            PlayVideo(videoInstances[index].URL);
        }
    }
}
