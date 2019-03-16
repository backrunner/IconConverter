using BluwolfIcons;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace iconConverterExtension
{
    public static class IconConverter
    {
        public static void Convert(object obj)
        {
            var option = obj as IconConvertOption;
            ConvertToIco(option.FilePath, option.Size, option.faviconMode);
        }

        public static void ConvertToIco(string filePath, int iconSize, bool faviconMode=false)
        {
            byte[] b_image;
            try
            {
                b_image = File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            //输出路径
            ISupportedImageFormat format = new PngFormat { Quality = 100 };
            string savePath = Path.GetDirectoryName(filePath) + "/" + Path.GetFileNameWithoutExtension(filePath) + ".icontemp";
            string iconPath = Path.GetDirectoryName(filePath) + "/" + Path.GetFileNameWithoutExtension(filePath) + ".ico";
            using (MemoryStream inStream = new MemoryStream(b_image))
            {
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                {
                    if (faviconMode)
                    {
                        for (var i = 0; i < 2; i++)
                        {
                            int _iconSize = (int)Math.Pow(2, i + 4);
                            iconPath = Path.GetDirectoryName(filePath) + "/" + Path.GetFileNameWithoutExtension(filePath) + "_" + _iconSize.ToString() + ".ico";
                            //设置宽高
                            System.Drawing.Size size = new System.Drawing.Size(_iconSize, _iconSize);
                            try
                            {
                                imageFactory.Load(inStream).Format(format).Resize(size).Save(savePath);
                            }
                            catch (Exception ex)
                            {
                                throw (ex);
                            }
                            #region 包装为icon
                            var icon = new Icon();
                            //读入缓存
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(savePath);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            //保存为ico
                            icon.Images.Add(new PngIconImage(bitmap));
                            icon.Save(iconPath);
                            #endregion
                        }
                    }
                    else
                    {
                        //获取宽高
                        System.Drawing.Size size = new System.Drawing.Size(iconSize, iconSize);
                        //转换成png在resize后保存到temp
                        try
                        {
                            imageFactory.Load(inStream).Format(format).Resize(size).Save(savePath);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                        #region 包装为icon
                        var icon = new Icon();
                        //读入缓存
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(savePath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        //保存为ico
                        icon.Images.Add(new PngIconImage(bitmap));
                        icon.Save(iconPath);
                        #endregion
                    }
                }
            }
            //删除临时文件
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
        }
    }
}
