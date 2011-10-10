using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spritedotless
{
    public interface IImageUrlProvider
    {
        string GetImageUrl(string identifier);
    }
}
