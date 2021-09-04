using Docnet.Core;
using Docnet.Core.Models;
using Microsoft.Win32;
using MVVM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PDFSlicer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<PDFPageInfo> currentPages = new List<PDFPageInfo>();
        void UpdateSummary()
        {
            logs.Text = $"Extracted {currentPages.Count} pages";
            summary.Text = $"Selected {currentPages.Where(p => p.Selected).Count()} pages";
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            currentPages.ForEach(p => p.Selected = false);
        }

        string OutputFolder
        {
            get
            {
                var storePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PDFSlicer");

                return storePath;
            }
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var outputPath = Generator.Generate(currentPages.Where(p => p.Selected).ToList(), OutputFolder);
                if (outputPath != null)
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = outputPath,
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                logs.Text = ex.Message;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() != true)
                    return;

                var path = openFileDialog.FileName;// @"C:\Users\evmur\Downloads\Year 4 Maths Week 9.pdf";
                var target = OutputFolder;
                
                if (Directory.Exists(target))
                    Directory.Delete(target, true);

                Directory.CreateDirectory(target);

                currentPages = Generator.Run(path, target);

                currentPages.ForEach(p =>
                {
                    p.PropertyChanged += (s, a) =>
                    {
                        UpdateSummary();
                    };

                    p.ViewCommand = new CommandImpl(() =>
                    {
                        p.Selected = !p.Selected;
                        //webBrowser.Load(p.Path);
                    });
                });

                pagesCatalogue.ItemsSource = currentPages;
                UpdateSummary();
            }
            catch (Exception ex)
            {
                logs.Text = ex.Message;
            }
        }
    }
}
