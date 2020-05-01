using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HitomiViewer
{
    /// <summary>
    /// TestImageBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TestImageBox : Window
    {
        public TestImageBox()
        {
            InitializeComponent();
        }

        private void SaveFrameToFile(BitmapSource source)
        {
            // Source: http://msdn.microsoft.com/en-us/library/ms616045.aspx  

            FileStream stream = new FileStream("test.jpg", FileMode.Create);
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(stream);
            stream.Close();
        }

        public void SetImage(BitmapImage image)
        {
            image1.Source = image;
        }

        BitmapSource GetRandomBitmapSource(int width = 200, int height = 200)
        {
            // This method returns a BitmapSource for us to test.  
            // Don't worry about the details.  

            // Source: http://msdn.microsoft.com/en-us/library/system.windows.media.imaging.bitmapsource.aspx  

            // Define parameters used to create the BitmapSource.  
            PixelFormat pf = PixelFormats.Bgr32;
            int rawStride = (width * pf.BitsPerPixel + 7) / 8;
            byte[] rawImage = new byte[rawStride * height];

            // Initialize the image with data.  
            Random value = new Random();
            value.NextBytes(rawImage);

            // Create a BitmapSource.  
            BitmapSource bitmap = BitmapSource.Create(width, height,
                96, 96, pf, null,
                rawImage, rawStride);

            // Return the BitmapSource  
            return bitmap;
        }
    }
}
