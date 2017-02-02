using ExifLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ExifTool
{
    public partial class MainWindow
    {
        /*
            1 = Horizontal (normal) 
            2 = Mirror horizontal 
            3 = Rotate 180 
            4 = Mirror vertical 
            5 = Mirror horizontal and rotate 270 CW 
            6 = Rotate 90 CW 
            7 = Mirror horizontal and rotate 90 CW 
            8 = Rotate 270 CW
        */
        protected enum ExifOrientation
        {
            None,
            Normal,
            MirrorHorizontal,
            Rotate180,
            MirrorVertical,
            MirrorHorizontalAndRotate270,
            Rotate90,
            MirrorHorizontalAndRotate90,
            Rotate270
        }

        protected List<FileSystemInfo> FilingList { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                tbxLog.AppendText($"{message} {Environment.NewLine}");
            });
        }

        private void Run()
        {
            var files = new List<FileInfo>();

            foreach (var filingInfo in FilingList)
            {
                if (filingInfo is DirectoryInfo)
                {
                    var directory = filingInfo as DirectoryInfo;

                    files.AddRange(directory.GetFiles("*.jpg"));
                    files.AddRange(directory.GetFiles("*.jpeg"));
                }
                else if (filingInfo is FileInfo)
                {
                    files.Add(filingInfo as FileInfo);
                }
            }

            if (files?.Count > 0)
            {
                var codecInfo = ImageCodecInfo.GetImageEncoders().Where(e => e.MimeType == "image/jpeg").FirstOrDefault();
                var encoderInfo = new EncoderParameters
                {
                    Param = new[]
                    {
                        new EncoderParameter(Encoder.Quality, 100L)
                    }
                };

                foreach (var file in files)
                {
                    try
                    {
                        ushort orientation = 0;

                        using (var reader = new ExifReader(file.FullName))
                        {
                            if (reader.GetTagValue(ExifTags.Orientation, out orientation))
                            {
                                Log($"{file.FullName} : {orientation}");
                            }
                        }

                        using (var image = Image.FromFile(file.FullName))
                        {
                            switch ((ExifOrientation)orientation)
                            {
                                case ExifOrientation.Rotate90: image.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                                case ExifOrientation.Rotate180: image.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                                case ExifOrientation.Rotate270: image.RotateFlip(RotateFlipType.Rotate270FlipNone); break;

                                case ExifOrientation.MirrorHorizontal: image.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                                case ExifOrientation.MirrorHorizontalAndRotate90: image.RotateFlip(RotateFlipType.Rotate90FlipX); break;
                                case ExifOrientation.MirrorHorizontalAndRotate270: image.RotateFlip(RotateFlipType.Rotate270FlipX); break;

                                case ExifOrientation.MirrorVertical: image.RotateFlip(RotateFlipType.RotateNoneFlipY); break;
                            }

                            image.RemovePropertyItem(0x0112);
                            image.Save(file.FullName, codecInfo, encoderInfo);
                        }
                    }
                    catch (ExifLibException ex)
                    {
                        Log(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Log(ex.Message);
                    }
                }
            }
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => Run());
        }

        private void btnSelectFiles_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                DefaultExt = ".jpeg",
                Filter = "Image Files(*.jpg;*.jpeg)|*.jpg;*.jpeg",
                Multiselect = true,
                RestoreDirectory = true
            };

            if (fileDialog.ShowDialog() == true)
            {
                tbxLocation.Text = fileDialog.FileName;

                FilingList = new List<FileSystemInfo>();

                foreach (var path in fileDialog.FileNames)
                {
                    FilingList.Add(new FileInfo(path));
                }
            }
        }

        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.Desktop
            })
            {
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var path = tbxLocation.Text = folderDialog.SelectedPath;

                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        FilingList = new List<FileSystemInfo>();

                        FilingList.Add(new DirectoryInfo(path));
                    }
                }
            }
        }
    }
}
