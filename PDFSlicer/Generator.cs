using Docnet.Core;
using Docnet.Core.Models;
using MVVM;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Linq;

namespace PDFSlicer
{
    public class PDFPageInfo : ViewModel
    {
        public int Index { get; set; }
        public string Path { get; set; }

        public ICommand ViewCommand { get; set; }

        bool _selected = false;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged();
            }
        }
    }

    public class Generator
    {
        public static List<PDFPageInfo> Run(string source, string output)
        {
            var result = new List<PDFPageInfo>();

            using (var docReader = DocLib.Instance.GetDocReader(source, new PageDimensions(1)))
            {
                for (var pageIndex = 0; pageIndex < docReader.GetPageCount(); pageIndex++)
                {
                    var page = docReader.GetPageReader(pageIndex);
                    var bytes = page.GetImage();

                    var image = Image.LoadPixelData<Bgra32>(bytes, page.GetPageWidth(), page.GetPageHeight());

                    var displayIndex = pageIndex + 1;
                    var pageInfo = new PDFPageInfo()
                    { 
                        Index = displayIndex,
                        Path = Path.Combine(output, $"{displayIndex}.png")
                    };

                    image.SaveAsPng(pageInfo.Path);
                    result.Add(pageInfo);
                }
            }

            return result;
        }

        public static string Generate(List<PDFPageInfo> source, string output)
        {
            if (source == null || source.Count == 0)
                throw new Exception("Nothing selected");

            if (!Directory.Exists(output))
                throw new DirectoryNotFoundException(output);

            var content = new StringBuilder();
            content.AppendLine("<html>");
            content.AppendLine("<style>");
            content.AppendLine("</style>");
            content.AppendLine("<body>");

            var pairs = source
                .Zip(source.Skip(1).Concat(new PDFPageInfo[] { null }), (l, r) => new { idx = source.IndexOf(l), l, r })
                .Where(s => s.idx % 2 == 0);
                ;

            foreach (var pair in pairs)
            {
                content.AppendLine($"<div><img src='{pair.l.Path}'/><img src='{pair.r?.Path ?? ""}'/></div>");
            }            

            content.AppendLine("</body>");
            content.AppendLine("</html>");

            var outputPath = Path.Combine(output, "index.html");
            File.WriteAllText(outputPath, content.ToString());
            return outputPath;                
        }
    }
}
