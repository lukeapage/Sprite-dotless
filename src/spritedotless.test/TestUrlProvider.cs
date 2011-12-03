using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spritedotless.test
{
    public class TestUrlProvider : IImageUrlProvider
    {
        private Dictionary<string, System.Drawing.Image> _images = new Dictionary<string, System.Drawing.Image>();

        public string GetImageUrl(string identifier, CacheMode cacheMode)
        {
            return "";
        }

        public string SaveImage(string identifier, CacheMode cacheMode, System.Drawing.Image image)
        {
            _images[identifier] = image;

            return "";
        }
    }
}
