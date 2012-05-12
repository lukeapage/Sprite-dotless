using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace spritedotless
{
    public class SpriteImage : IDisposable
    {
        public PositionType PositionType
        {
            get;
            set;
        }

        public Point Position
        {
            get;
            set;
        }

        public Size Size
        {
            get
            {
                return Image.Size;
            }
        }

        private Image _image = null;
        private Image Image
        {
            get
            {
                if (_image == null)
                {
                    GetInfo();
                }
                return _image;
            }
        }

        public string Filename
        {
            get;
            set;
        }

        public SpriteList SpriteList
        {
            get;
            set;
        }

        public SpriteImage(string filename, PositionType positionType, SpriteList list)
        {
            Filename = filename;
            PositionType = positionType;
            SpriteList = list;
        }

        public void GetInfo()
        {
            _image = Image.FromFile(Filename);
        }

        public void DrawOnTo(Graphics bitmap, Size totalSize)
        {
            int repeatXCount = PositionType == PositionType.Horizontal ? totalSize.Width / Size.Width : 1,
                repeatYCount = PositionType == PositionType.Vertical ? totalSize.Height / Size.Height : 1;

            for (int repeatX = 0; repeatX < repeatXCount; repeatX++)
            {
                for (int repeatY = 0; repeatY < repeatYCount; repeatY++)
                {
                    bitmap.DrawImage(Image, Position.X + (repeatX * Size.Width), Position.Y + (repeatY * Size.Height));
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_image != null)
            {
                _image.Dispose();
                _image = null;
            }
        }

        #endregion
    }
}
