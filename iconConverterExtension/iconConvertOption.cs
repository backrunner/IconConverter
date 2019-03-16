using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iconConverterExtension
{
    public class IconConvertOption
    {
        public IconConvertOption(string filePath, int size)
        {
            FilePath = filePath;
            Size = size;
            faviconMode = false;
        }
        public IconConvertOption(string filePath, int size, bool faviconMode)
        {
            FilePath = filePath;
            Size = size;
            this.faviconMode = faviconMode;
        }
        public string FilePath { get; set; }
        public int Size { get; set; }
        public bool faviconMode { get; set; }
    }
}
