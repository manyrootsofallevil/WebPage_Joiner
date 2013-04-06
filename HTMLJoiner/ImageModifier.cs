using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTMLJoiner
{
    public class ImageModifier
    {
        string FileName { get; set; }

        public ImageModifier(string fileName)
        {
            FileName = fileName;
        }

        public void WriteMessageToImage(string message, string outputFile)
        {

            Color FillColor = Color.FromArgb(127, 255, 255, 255);
            SolidBrush FillBrush = new SolidBrush(FillColor);
            Rectangle FillRectangle = new Rectangle(0, 0, 227, 50);

            Font TextFont = new Font("Comic Sans MS", 18);
            SolidBrush TextBrush = new SolidBrush(Color.Navy);
            StringFormat TextFormat = new StringFormat();
            TextFormat.Alignment = StringAlignment.Center;
            TextFormat.LineAlignment = StringAlignment.Center;
            System.Drawing.Image GreetingImage = System.Drawing.Image.FromFile(FileName);
            Graphics DrawingSurface = Graphics.FromImage(GreetingImage);
            DrawingSurface.FillRectangle(FillBrush, FillRectangle);
            DrawingSurface.DrawString(message, TextFont, TextBrush, FillRectangle, TextFormat);
            GreetingImage.Save(outputFile, ImageFormat.Jpeg);
        }
    }
}
