using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace spritedotless
{
    public interface IImageUrlProvider
    {
        string GetImageUrl(string identifier, CacheMode cacheMode);

        string SaveImage(string identifier, CacheMode cacheMode, Image image);
    }
}
