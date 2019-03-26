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
using WayaCompress;

namespace WayaTool
{
    public partial class Form1 : Form
    {
        Waya waya = new Waya();

        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(openFileDialog1.FileName);
            }
        }

        private void loadWayaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                byte[] data = File.ReadAllBytes(openFileDialog2.FileName);

                Image image = waya.Decompress(data);

                if (image != null)
                {
                    pictureBox1.Image = image;
                }
            }
        }

        private void saveWayaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Load image first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            if ( saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                byte[] data = waya.Compress(pictureBox1.Image, trackBar1.Value);

                if (data != null)
                {
                    File.WriteAllBytes(saveFileDialog1.FileName, data);
                }
            }
        }

    }
}
