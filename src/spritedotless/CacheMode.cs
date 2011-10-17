using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spritedotless
{
    [Flags]
    public enum CacheMode
    {
        None,
        ConfigInMemoryImagesInFiles,
        Memory,
        File
    }
}
