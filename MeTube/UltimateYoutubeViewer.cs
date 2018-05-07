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
using MeTube3;

namespace MeTube3
{
    public partial class UltimateYoutubeViewer : Form
    {
        public bool videoReady = false;
        public UltimateYoutubeViewer()
        {
            InitializeComponent();
            meTube = new MeTube(VideoPanel.Handle);
            videoReady = true;
        }

        MeTube meTube;

        bool fullScreen = false;

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

        public void Form1_Load(object sender, EventArgs e) { backgroundWorker1.RunWorkerAsync(); }

        public void MeTube()
        {
            meTube.SetVideo("Pr8rOmTcf5Q");
            meTube.PlayVideo();
            //GoFullscreen(true);
        }

        private void VideoPanel_Resize(object sender, EventArgs e)
        {
            if (videoReady)
                meTube.SetResolution(VideoPanel.Width, VideoPanel.Height);
        }

        private void UltimateYoutubeViewer_KeyPress(object sender, KeyPressEventArgs e) { if (e.KeyChar == (char)Keys.Enter) { GoFullscreen(fullScreen = !fullScreen); } }

        private void VideoPanel_MouseCaptureChanged(object sender, EventArgs e)
        {
            GoFullscreen(true);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            MeTube();
        }
    }
}
