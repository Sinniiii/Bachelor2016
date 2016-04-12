using System;
using System.Windows;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Surface.Core;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using TouchEventArgs = System.Windows.Input.TouchEventArgs;
using Tesseract;

//Baard was here

namespace Product_Browser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SurfaceWindow
    {

        public MainWindow()
        {
            InitializeComponent();

            InitializeCamera();

            Bitmap bitmap = new Bitmap("testimage.bmp");
            TesseractEngine ocrEngine = new TesseractEngine("", "eng", EngineMode.Default);
            ocrEngine.SetVariable("tessedit_char_whitelist", "0123456789");
            Tesseract.Page page = ocrEngine.Process(bitmap);

            Console.WriteLine(page.GetText());
            int success = 0;
            int.TryParse(page.GetText(), out success);
            Console.WriteLine(success);
            File.WriteAllText("result", success.ToString());

            // ScatterView
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            TagVisualizationDefinition tagDef = new TagVisualizationDefinition();
            
            tagDef.Source = new Uri("TagWindow.xaml", UriKind.Relative);
            tagDef.MaxCount = 10;
            tagDef.Value = e.TouchDevice.GetTagData().Value;

            tagVisualizer.Definitions.Add(tagDef);
        }

        /****** TEMP ***************/
        private byte[] normalizedImage;
        private Microsoft.Surface.Core.ImageMetrics normalizedMetrics;
        System.Drawing.Imaging.ColorPalette pal;
        bool imageAvailable;

        private TouchTarget touchTarget;

        private void InitializeCamera()
        {
            touchTarget = new TouchTarget(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            touchTarget.EnableInput();
            touchTarget.EnableImage(ImageType.Normalized);
            touchTarget.FrameReceived += OnFrameReceived;
            
        }

        protected void OnFrameReceived(object o, FrameReceivedEventArgs e)
        {
            imageAvailable = false;
            
            if (normalizedImage == null)
            {
                int paddingLeft,
                    paddingRight;

                imageAvailable = e.TryGetRawImage(Microsoft.Surface.Core.ImageType.Normalized,
                    Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Left,
                    Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Top,
                    Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Width,
                    Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Height,
                    out normalizedImage,
                    out normalizedMetrics,
                    out paddingLeft,
                    out paddingRight);
            }
            else
            {
                ;
                imageAvailable = e.UpdateRawImage(Microsoft.Surface.Core.ImageType.Normalized,
                     normalizedImage,
                     Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Left,
                     Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Top,
                     Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Width,
                     Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Height);
            }
        }

        private void SurfaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!imageAvailable)
                return;

            DisableRawImage();
            imageAvailable = false;

            // Copy the normalizedImage byte array into a Bitmap object.

            GCHandle h = GCHandle.Alloc(normalizedImage, GCHandleType.Pinned);
            IntPtr ptr = h.AddrOfPinnedObject();
            Bitmap imageBitmap = new Bitmap(normalizedMetrics.Width,
                                    normalizedMetrics.Height,
                                    normalizedMetrics.Stride,
                                    System.Drawing.Imaging.PixelFormat.Format8bppIndexed,
                                    ptr);

            // The preceding code converts the bitmap to an 8-bit indexed color image. 
            // The following code creates a grayscale palette for the bitmap.
            Convert8bppBMPToGrayscale(imageBitmap);
            
            Bitmap clone = new Bitmap(imageBitmap.Width, imageBitmap.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(imageBitmap, new System.Drawing.Rectangle(0, 0, clone.Width, clone.Height));
            }


            // The bitmap is now available to work with 
            // (such as, save to a file, send to a processing API, and so on).
            if (File.Exists("test.bmp"))
            {
                File.Delete("test.bmp");
            }

            clone.RotateFlip(RotateFlipType.RotateNoneFlipX);
            clone.Save(@"test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            
            TesseractEngine ocrEngine = new TesseractEngine("./tessdata", "end", EngineMode.Default);
            Tesseract.Page page = ocrEngine.Process(clone);
            Console.WriteLine(page.GetText());
            

            EnableRawImage();
        }

        private void Convert8bppBMPToGrayscale(Bitmap bmp)
        {
            if (pal == null) // pal is defined at module level as --- ColorPalette pal;
            {
                pal = bmp.Palette;
                for (int i = 0; i < 256; i++)
                {
                    pal.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
                }
            }

            bmp.Palette = pal;
        }

        private void EnableRawImage()
        {
            touchTarget.EnableImage(Microsoft.Surface.Core.ImageType.Normalized);
            touchTarget.FrameReceived += OnFrameReceived;
        }

        private void DisableRawImage()
        {
            touchTarget.DisableImage(Microsoft.Surface.Core.ImageType.Normalized);
            touchTarget.FrameReceived -= OnFrameReceived;
        }
        /**************** TEMP *********************************/
    }
}
