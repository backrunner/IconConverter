using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iconConverterExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFiles)]
    public class IconConverterExtension:SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            foreach (var filePath in SelectedItemPaths)
            {
                var extension = Path.GetExtension(filePath).ToLower();
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp")
                {
                    return true;
                }
            }
            return false;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            //菜单
            var menu = new ContextMenuStrip();
            //子菜单
            var submenu = new ToolStripMenuItem("转换图片为.ico图标");
            var separator = new ToolStripSeparator();

            //菜单项
            var size256 = new ToolStripMenuItem("256 x 256");
            size256.Click += Size256_Click;
            submenu.DropDownItems.Add(size256);

            var size128 = new ToolStripMenuItem("128 x 128");
            size128.Click += Size128_Click;
            submenu.DropDownItems.Add(size128);

            var size64 = new ToolStripMenuItem("64 x 64");
            size64.Click += Size64_Click;
            submenu.DropDownItems.Add(size64);

            var size32 = new ToolStripMenuItem("32 x 32");
            submenu.DropDownItems.Add(size32);
            size32.Click += Size32_Click;

            var size16 = new ToolStripMenuItem("16 x 16");
            submenu.DropDownItems.Add(size16);
            size16.Click += Size16_Click;

            var faviconMode = new ToolStripMenuItem("favicon.ico");
            submenu.DropDownItems.Add(faviconMode);
            faviconMode.Click += FaviconMode_Click;

            menu.Items.Add(separator);
            menu.Items.Add(submenu);
            menu.Items.Add(separator);
            return menu;
        }

        private void FaviconMode_Click(object sender, EventArgs e)
        {
            foreach (var filePath in SelectedItemPaths)
            {
                string extension = Path.GetExtension(filePath).ToLower();
                if (extension.Equals(".jpg") || extension.Equals(".jpeg") || extension.Equals(".png") || extension.Equals(".png"))
                {
                    var option = new IconConvertOption(filePath, 0, true);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(IconConverter.Convert), option);
                }
            }
        }

        private void Size16_Click(object sender, EventArgs e)
        {
            Convert(16);
        }

        private void Size32_Click(object sender, EventArgs e)
        {
            Convert(32);
        }

        private void Size64_Click(object sender, EventArgs e)
        {
            Convert(64);
        }

        private void Size128_Click(object sender, EventArgs e)
        {
            Convert(128);
        }

        private void Size256_Click(object sender, EventArgs e)
        {
            Convert(256);
        }

        private void Convert(int size)
        {
            foreach (var filePath in SelectedItemPaths)
            {
                string extension = Path.GetExtension(filePath).ToLower();
                if (extension.Equals(".jpg") || extension.Equals(".jpeg") || extension.Equals(".png") || extension.Equals(".png"))
                {
                    var option = new IconConvertOption(filePath, size);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(IconConverter.Convert), option);
                }
            }
        }
    }
}
