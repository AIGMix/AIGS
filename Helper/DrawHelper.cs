using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Media.Imaging;

namespace AIGS.Helper
{
    public class DrawHelper
    {
        public int Width;
        public int Height;

        Bitmap m_Bitmap;
        Graphics DrawHandle;

        public DrawHelper(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;

            try
            {
                m_Bitmap = new Bitmap(Width, Height);
                DrawHandle = Graphics.FromImage(m_Bitmap);
                DrawHandle.Clear(Color.White);
            }
            catch { }
        }

        public void Draw(Point[] pPoint, Pen aPen, int i0Point1Line2Polygon, int iRadius = 3)
        {
            try
            {
                if (i0Point1Line2Polygon == 0)
                {
                    DrawHandle.DrawEllipse(aPen, pPoint[0].X - iRadius, pPoint[0].Y - iRadius, iRadius * 2, iRadius * 2);
                }
                if (i0Point1Line2Polygon == 1)
                {
                    if (pPoint.Count() >= 2)
                        DrawHandle.DrawLines(aPen, pPoint);
                }
                if (i0Point1Line2Polygon == 2)
                {
                    if (pPoint.Count() >= 3)
                        DrawHandle.DrawPolygon(aPen, pPoint);
                }
            }
            catch { }
        }

        public void Clear()
        {
            DrawHandle.Clear(Color.White);
        }

        public BitmapImage GetBitmapImage()
        {
            BitmapImage aImage = AIGS.Common.Convert.ConverBitmapToBitmapImage(m_Bitmap);
            return aImage;
        }

        public Bitmap GetBitmap()
        {
            return m_Bitmap;
        }

        public System.Windows.Media.ImageSource GetImageSource()
        {
            System.Windows.Media.ImageSource aImage = AIGS.Common.Convert.ConverBitmapToImageSource(m_Bitmap);
            return aImage;
        }

        public static Image GetImageFromNet(string url)
        {
            Image img;
            try
            {
                WebRequest request = WebRequest.Create(url);
                using (WebResponse response = request.GetResponse())
                    img = Image.FromStream(response.GetResponseStream());
            }
            catch
            {
                img = null;
            }
            return img;
        }
    }
}
