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

        public void DrawOnTo(Graphics bitmap)
        {
            bitmap.DrawImage(Image, Position);
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
