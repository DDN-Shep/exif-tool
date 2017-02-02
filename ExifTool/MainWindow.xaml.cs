using ExifLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ExifTool
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
    public partial class MainWindow
    {
        protected List<FileInfo> Files { get; set; }

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
            if (Files?.Count > 0)
            {
                foreach (var file in Files)
                {
                    try
                    {
                        using (var reader = new ExifReader(file.FullName))
                        {
                            ushort orientation;

                            if (reader.GetTagValue(ExifTags.Orientation, out orientation))
                            {
                                Log($"{file.FullName} : {orientation}");
                            }
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

                Files = new List<FileInfo>();

                foreach (var path in fileDialog.FileNames)
                {
                    Files.Add(new FileInfo(path));
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
                        var directory = new DirectoryInfo(path);

                        Files = new List<FileInfo>();

                        Files.AddRange(directory.GetFiles("*.jpg"));
                        Files.AddRange(directory.GetFiles("*.jpeg"));
                    }
                }
            }
        }
    }
}
