using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeTube3
{
    public partial class MainMenu : Form
    {
        class Camera
        {
            public Camera(int width, int height)
            {
                w = (float)width;
                h = (float)height;
            }
            public float w = 1;
            public float h = 1;
            public float x = 0;
            public float y = 0;
            public float z = -1;
        }

        class Video
        {
            public Video(string VideoID, System.Windows.Forms.Control.ControlCollection frame)
            {
                // Image
                ID = VideoID;
                image = new PictureBox();
                image.SizeMode = PictureBoxSizeMode.AutoSize;
                image.Load(Application.StartupPath + "../../../../Database/Thumbs/"+VideoID+".jpg");
                frame.Add(image);
                // Text
                label = new Label();
                label.Text = "zelda video";
                frame.Add(label);
            }

            public void Reposition(Camera camera)
            {
                // Projection Space
                float size = 1.0f / (z - camera.z);
                float xpos = (x - camera.x) * size;
                float ypos = (y - camera.y) * size;

                // View Space
                xpos = (xpos + 0.5f) * camera.w;
                ypos = (ypos + 0.5f) * camera.h;

                image.Location = new Point((int)xpos, (int)ypos);
                label.Location = new Point((int)xpos, (int)ypos);
            }

            public float x = 0;
            public float y = 0;
            public float z = 0;
            public string ID;
            public PictureBox image;
            public Label label;
        }


        Camera camera;
        List<Video> videos;

        public void Redraw()
        {
            foreach(Video Vid in videos)
            {
                Vid.Reposition(camera);

            }
        }


        public MainMenu()
        {
            InitializeComponent();
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            camera = new Camera(Bounds.X, Bounds.Y);
            videos = new List<Video>();
            videos.Add(new Video("NntQ86FHcMY", Controls));
        }

        private void MainMenu_Scroll(object sender, ScrollEventArgs e)
        {
            var inny = e.ScrollOrientation > 0;
            if(inny)
                camera.z *= 0.9f;
            else
                camera.z *= 1.1f;
        }
    }
}
