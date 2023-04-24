using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SafeCopy
{
    internal class SafeCopyApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string assembly = @"C:\ProgramData\Autodesk\Revit\Addins\2022\SafeCopy.dll";
            string tabName = "NPP1";
            application.CreateRibbonTab(tabName);
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Изменение");
            PushButtonData buttonData = new PushButtonData(nameof(SafeCopyMain), "Безопасное копирование", assembly, typeof(SafeCopyMain).FullName)
            {
                LargeImage = PngImageSource("SafeCopy.Resources.SafeCopy.png")
            };
            panel.AddItem(buttonData);
            return Result.Succeeded;
        }
        private System.Windows.Media.ImageSource PngImageSource(string embeddedPath)
        {
            //string[] resNames = this.GetType().Assembly.GetManifestResourceNames();
            Stream stream = this.GetType().Assembly.GetManifestResourceStream(embeddedPath);
            var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            return decoder.Frames[0];
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
