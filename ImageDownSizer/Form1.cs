using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageDownSizer
{
    public partial class Form1 : Form
    {
        private Image selectedImage;
  

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedImagePath = openFileDialog.FileName;
                    selectedImage = Image.FromFile(selectedImagePath);
                    pictureBox1.Image = selectedImage;
                }
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (!float.TryParse(textBox1.Text, out float downscalingFactor))
            {
                MessageBox.Show("Invalid downscaling factor. Please enter a valid number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Please select an image first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Image downScaledImage = ModifyImage((Bitmap)pictureBox1.Image, downscalingFactor);

            pictureBox1.Image = downScaledImage;
        }



        private Bitmap ModifyImage(Bitmap initial, double reduction)
        {
            if (reduction <= 0 || reduction > 1) throw new ArgumentOutOfRangeException("reduction");

            int targetWidth = Math.Max(1, (int)(initial.Width * reduction));
            int targetHeight = Math.Max(1, (int)(initial.Height * reduction));
            Bitmap outputImage = new Bitmap(targetWidth, targetHeight, initial.PixelFormat);

            BitmapData initialData = initial.LockBits(new Rectangle(0, 0, initial.Width, initial.Height), ImageLockMode.ReadOnly, initial.PixelFormat);
            BitmapData outputData = outputImage.LockBits(new Rectangle(0, 0, targetWidth, targetHeight), ImageLockMode.WriteOnly, outputImage.PixelFormat);

            int depth = Bitmap.GetPixelFormatSize(initial.PixelFormat) / 8;
            byte[] initialArray = new byte[initialData.Stride * initial.Height];
            byte[] outputArray = new byte[outputData.Stride * targetHeight];

            Marshal.Copy(initialData.Scan0, initialArray, 0, initialArray.Length);

            for (int i = 0; i < targetHeight; i++)
            {
                for (int j = 0; j < targetWidth; j++)
                {
                    long totalRed = 0, totalGreen = 0, totalBlue = 0, totalPixels = 0;
                    for (int y = (int)(i / reduction); y < (int)Math.Ceiling((i + 1) / reduction); y++)
                    {
                        for (int x = (int)(j / reduction); x < (int)Math.Ceiling((j + 1) / reduction); x++)
                        {
                            int originalIndex = y * initialData.Stride + x * depth;
                            totalBlue += initialArray[originalIndex];
                            totalGreen += initialArray[originalIndex + 1];
                            totalRed += initialArray[originalIndex + 2];
                            totalPixels++;
                        }
                    }
                    int resultIndex = i * outputData.Stride + j * depth;
                    outputArray[resultIndex] = (byte)(totalBlue / totalPixels);
                    outputArray[resultIndex + 1] = (byte)(totalGreen / totalPixels);
                    outputArray[resultIndex + 2] = (byte)(totalRed / totalPixels);
                }
            }

            Marshal.Copy(outputArray, 0, outputData.Scan0, outputArray.Length);
            initial.UnlockBits(initialData);
            outputImage.UnlockBits(outputData);
            return outputImage;
        }




        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = selectedImage;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
