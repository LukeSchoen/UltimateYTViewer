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

    class UISync
    {
        static ISynchronizeInvoke Sync;
        public static void Init(ISynchronizeInvoke sync) { Sync = sync; }
        public static void Execute(Action action) { Sync.BeginInvoke(action, null); }
    }

    class MeTube
    {
        public MeTube(IntPtr windowHandle)
        {
            videoStreams = new List<VideoInstace>();
            audioStreams = new List<AudioInstace>();
            m_windowHandle = windowHandle;
            m_width = 640;
            m_width = 480;
        }

        public void PlayVideo() { m_playing = true; UpdateConnectionStream(); }
        public void StopVideo() { m_playing = false; UpdateConnectionStream(); }

        public long GetTimeMS() { return m_player.Time; }
        public void SetTimeMS(long timeMS) { m_player.Time = m_currentMS; }

        public string GetVideo() { return m_currentVidID; }
        public void SetVideo(string VidID) { m_currentVidID = VidID; UpdateConnectionStream(); }

        public void SetResolution(int width, int height) { m_width = width; m_height = height; UpdateConnectionStream(); }
        public long GetDurationMS() { return m_player.Length; }

        // Private //

        void UpdateConnectionStream()
        {
            if (m_previoisVidID != m_currentVidID) RetreiveVideoStreamData(m_currentVidID); m_previoisVidID = m_currentVidID;
            m_currentVideoStreamID = FindBestVideoStream(m_width, m_height);
            if (m_player != null)
                if (m_player.Time > 0)
                    m_currentMS = m_player.Time;

            //System.Threading.Thread.Sleep(100);

            if (m_currentVideoStreamURL != videoStreams[m_currentVideoStreamID].URL)
            {
                m_currentVideoStreamURL = videoStreams[m_currentVideoStreamID].URL;
                if (m_player != null)
                    m_player.Stop();
                m_factory = new MediaPlayerFactory(true);
                m_player = m_factory.CreatePlayer<IDiskPlayer>();
                m_player.WindowHandle = m_windowHandle;
                IMedia stream = m_factory.CreateMedia<IMedia>(m_currentVideoStreamURL);
                IVideoPlayer m_renderer = m_factory.CreatePlayer<IVideoPlayer>();
                m_player.Open(stream);
                stream.Parse(false);
                if (m_playing) m_player.Play(); else m_player.Stop();
                m_player.Time = m_currentMS;
            }
            else
                if (m_playing) m_player.Play(); else m_player.Stop();
        }

        int FindBestVideoStream(int width, int height)
        {
            int videoStreamID = 0;
            bool found = false;
            // Find Smallest above width and height
            if (!found)
            {
                int bestSize = int.MaxValue;
                int bestItem = 0;
                int currentItem = 0;
                foreach (var video in videoStreams)
                {
                    if (video.width * video.height < bestSize)
                    {
                        bool widthSuff = (video.width >= width);
                        bool heightSuff = (video.height >= height);
                        if (widthSuff && heightSuff)
                        {
                            bestSize = video.width * video.height;
                            bestItem = currentItem;
                            found = true;
                        }
                    }
                    currentItem++;
                }
                videoStreamID = bestItem;
            }
            // Find Biggest
            if (!found)
            {
                int bestSize = 0;
                int bestItem = 0;
                int currentItem = 0;
                foreach (var video in videoStreams)
                {
                    if (video.width * video.height > bestSize)
                    {
                        bestSize = video.width * video.height;
                        bestItem = currentItem;
                    }
                    currentItem++;
                }
                videoStreamID = bestItem;
            }
            return videoStreamID;
        }

        List<string> RunYoutubeDL(string vidID, string parametre)
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

        string ReadElement(string str)
        {
            if (str.Contains(" "))
                return str.Substring(0, str.IndexOf(" "));
            else
                return str;
        }

        bool RetreiveVideoStreamData(string vidID)
        {
            var formatData = RunYoutubeDL(vidID, "--list-formats");
            var urlData = RunYoutubeDL(vidID, "--get-url --all-formats");
            audioStreams.Clear();
            videoStreams.Clear();
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
                        audioStreams.Add(retAudioInstance);
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
                        videoStreams.Add(retVideoInstance);
                        streamID++;
                    }
                }
            }
            return true;
        }

        int m_width = 0;
        int m_height = 0;
        IDiskPlayer m_player;
        bool m_playing = false;
        IMediaPlayerFactory m_factory;
        List<VideoInstace> videoStreams;
        List<AudioInstace> audioStreams;

        IntPtr m_windowHandle;
        long m_currentMS = 0;
        int m_currentVideoStreamID = 0;
        string m_currentVidID = "Pr8rOmTcf5Q";
        string m_previoisVidID;
        string m_currentVideoStreamURL;
    }
}
