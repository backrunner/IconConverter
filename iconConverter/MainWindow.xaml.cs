using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using BluwolfIcons;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace iconConverter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public class ConvertOption
    {
        public ConvertOption(string filePath, int outputWidth, int outputHeight)
        {
            FilePath = filePath;
            OutputWidth = outputWidth;
            OutputHeight = outputHeight;
        }
        public string FilePath { get; set; }
        public int OutputWidth { get; set; }
        public int OutputHeight { get; set; }
    }
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 图片文件路径
        /// </summary>
        private string[] filePaths;

        //输出设置
        private int outputWidth = 0;
        private int outputHeight = 0;
        private bool faviconMode = false;

        private int completedCount = 0;
        private int errorConvertCount = 0;

        /// <summary>
        /// MetroWindow对话框的默认设置
        /// </summary>
        private MetroDialogSettings generalDialogSetting = new MetroDialogSettings()
        {
            AffirmativeButtonText = "确定",
            DialogTitleFontSize = 17,
            DialogMessageFontSize = 14
        };


        private void Btn_selectFie_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Title = "请选择需要转换为图标的图片文件",
                Filter = "支持的图片文件|*.jpg;*.jpeg;*.png;*.bmp|JPG图片|*.jpg;*.jpeg|PNG图片|*.png|BMP位图|*.bmp",
                Multiselect = true
            };
            if (fileDialog.ShowDialog() == true)
            {
                filePaths = fileDialog.FileNames;
                if (filePaths.Length > 1)
                {
                    SetFilePath(filePaths.Length);
                }
                else
                {
                    SetFilePath(filePaths[0]);
                }
            }
        }

        //文件拖放
        private void Rect_dragArea_Drop(object sender, DragEventArgs e)
        {
            var selectedPaths = new List<string>((string[])e.Data.GetData(DataFormats.FileDrop));
            for (var i = selectedPaths.Count - 1; i >= 0; i--)
            {
                var extension = Path.GetExtension(selectedPaths[i]);
                if (!(extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png") || extension.ToLower().Equals(".bmp")))
                {
                    selectedPaths.RemoveAt(i);
                }
            }
            filePaths = selectedPaths.ToArray();
            if (filePaths.Length > 1)
            {
                SetFilePath(filePaths.Length);
            }
            else
            {
                SetFilePath(filePaths[0]);
            }
        }

        private void Lbl_dragArea_Drop(object sender, DragEventArgs e)
        {
            Rect_dragArea_Drop(sender, e);
        }
        private void Img_preview_Drop(object sender, DragEventArgs e)
        {
            Rect_dragArea_Drop(sender, e);
        }

        //配置文件地址
        private async void SetFilePath(string path)
        {
            string extension = Path.GetExtension(path).ToLower();
            if (extension.Equals(".jpg") || extension.Equals(".jpeg") || extension.Equals(".png") || extension.Equals(".bmp"))
            {
                txt_filePath.Text = path;
                //设置UI的预览
                this.BeginInvoke(() =>
                {
                    BitmapImage image = new BitmapImage(new Uri(path, UriKind.Absolute));
                    img_preview.Source = image;
                    lbl_dragArea.Visibility = Visibility.Hidden;
                });
            }
            else
            {
                await this.ShowMessageAsync("类型错误", "工具只支持 jpg，png，bmp 格式的文件。", MessageDialogStyle.Affirmative, generalDialogSetting);
                return;
            }
        }

        private void SetFilePath(int pathCount)
        {
            txt_filePath.Text = "(已选择" + pathCount.ToString() + "个图片文件)";
            this.BeginInvoke(() =>
            {
                BitmapImage image = new BitmapImage(new Uri(filePaths[0], UriKind.Absolute));
                img_preview.Source = image;
                lbl_dragArea.Visibility = Visibility.Hidden;
            });
        }

        private void Cb_preset_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (cb_preset.SelectedIndex)
            {
                default:
                    txt_outputHeight.IsEnabled = true;
                    txt_outputWidth.IsEnabled = true;
                    faviconMode = false;
                    outputWidth = outputHeight = (int)Math.Pow(2, 9 - cb_preset.SelectedIndex);
                    txt_outputWidth.Text = outputWidth.ToString();
                    txt_outputHeight.Text = outputHeight.ToString();
                    break;
                case 0:
                    txt_outputHeight.IsEnabled = true;
                    txt_outputWidth.IsEnabled = true;
                    faviconMode = false;
                    txt_outputHeight.Text = "";
                    txt_outputWidth.Text = "";
                    break;
                case 6:
                    //favico
                    txt_outputHeight.IsEnabled = false;
                    txt_outputWidth.IsEnabled = false;
                    faviconMode = true;
                    break;
            }
        }

        //生成
        private void Btn_generate_Click(object sender, RoutedEventArgs e)
        {
            //重置计数器
            completedCount = 0;
            errorConvertCount = 0;

            //获取宽高
            if (txt_outputWidth.Text.Trim().Length < 1 || txt_outputHeight.Text.Trim().Length < 1)
            {
                this.ShowMessageAsync("错误", "请输入宽高或选择一个预设。", MessageDialogStyle.Affirmative, generalDialogSetting);
                return;
            }

            var outputWidth = 32;
            var outputHeight = 32;
            int.TryParse(txt_outputWidth.Text.Trim(), out outputWidth);
            int.TryParse(txt_outputHeight.Text.Trim(), out outputHeight);

            if (filePaths.Length > 1)
            {
                foreach (var path in filePaths)
                {
                    var option = new ConvertOption(path,outputWidth,outputHeight);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(WorkGenerate),option);
                }
                while (completedCount < filePaths.Length) ;
                if (errorConvertCount > 0)
                {
                    this.ShowMessageAsync("完成", ".ico图标文件生成完成，但有"+errorConvertCount.ToString()+"个文件出错。", MessageDialogStyle.Affirmative, generalDialogSetting);
                } else
                {
                    this.ShowMessageAsync("完成", ".ico图标文件生成完成。", MessageDialogStyle.Affirmative, generalDialogSetting);
                }
            }
            else
            {
                GenerateIcoFile(filePaths[0],outputWidth,outputHeight);
                this.ShowMessageAsync("完成", ".ico图标文件生成完成。", MessageDialogStyle.Affirmative, generalDialogSetting);
            }

        }

        private void WorkGenerate(object state)
        {
            var convertOption = state as ConvertOption;
            GenerateIcoFile(convertOption.FilePath, convertOption.OutputWidth, convertOption.OutputHeight);
        }

        private void GenerateIcoFile(string filePath, int outputWidth, int outputHeight, bool errorMessage=false)
        {
            byte[] b_image;
            try
            {
                b_image = File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                if (errorMessage)
                {
                    MessageBox.Show("读取图片时遇到错误。\n" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                completedCount++;
                errorConvertCount++;
                return;
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
                            int iconSize = (int)Math.Pow(2, i + 4);
                            iconPath = Path.GetDirectoryName(filePath) + "/" + Path.GetFileNameWithoutExtension(filePath) + "_" + iconSize.ToString() + ".ico";
                            //设置宽高
                            System.Drawing.Size size = new System.Drawing.Size(iconSize, iconSize);
                            try
                            {
                                imageFactory.Load(inStream).Format(format).Resize(size).Save(savePath);
                            }
                            catch (Exception ex)
                            {
                                if (errorMessage)
                                {
                                    MessageBox.Show("操作图片时遇到错误。\n" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                completedCount++;
                                errorConvertCount++;
                                return;
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
                        System.Drawing.Size size = new System.Drawing.Size(outputWidth, outputHeight);
                        //转换成png在resize后保存到temp
                        try
                        {
                            imageFactory.Load(inStream).Format(format).Resize(size).Save(savePath);
                        }
                        catch (Exception ex)
                        {
                            if (errorMessage)
                            {
                                MessageBox.Show("操作图片时遇到错误。\n" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            completedCount++;
                            errorConvertCount++;
                            return;
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
            completedCount++;
        }



        #region 输入限制
        private void Txt_outputHeight_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private void Txt_outputWidth_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }
        #endregion

        #region 响应输入
        private void Txt_outputHeight_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            cb_preset.SelectedIndex = 0;
        }

        private void Txt_outputWidth_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            cb_preset.SelectedIndex = 0;
        }
        #endregion
    }
}
