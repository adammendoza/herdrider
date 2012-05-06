using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoGo;

using System.IO;

// Nwazet
using NwazetGoHelpers = Nwazet.Go.Helpers;
using NwazetGoFonts = Nwazet.Go.Fonts;
using NwazetGoImaging = Nwazet.Go.Imaging;
using NwazetGoDisplayTouchScreen = Nwazet.Go.Display.TouchScreen;
using NwazetGoSD = Nwazet.Go.SD;

// Helpers
using HelpersFonts = Helpers.Fonts;
using HelpersHardware = Helpers.Hardware;
using HelpersImaging = Helpers.Imaging;
using HelpersMath = Helpers.Math;
using HelpersSound = Helpers.Sound;

namespace HerdRider
{
    public class Program
    {

        // Color scheme
        public static readonly ushort ColorBackground = (ushort)NwazetGoImaging.BasicColor.White;
        public static readonly ushort ColorText = (ushort)NwazetGoImaging.BasicColor.White;
        public static readonly ushort ColorButton = (ushort)NwazetGoImaging.GrayScaleValues.Gray_50;
        public static readonly ushort ColorButtonBorder = (ushort)NwazetGoImaging.GrayScaleValues.Gray_50;
        public static readonly ushort ColorButtonText = (ushort)NwazetGoImaging.BasicColor.White;
        public static readonly ushort ColorButtonActive = (ushort)NwazetGoImaging.DefaultColorTheme.Base;
        public static readonly ushort ColorButtonActiveBorder = (ushort)NwazetGoImaging.DefaultColorTheme.Darker;
        public static readonly ushort ColorButtonActiveText = (ushort)NwazetGoImaging.BasicColor.Black;
        public static readonly ushort ColorMenu = (ushort)NwazetGoImaging.GrayScaleValues.Gray_30;
        public static readonly ushort ColorMenuLighter = (ushort)NwazetGoImaging.GrayScaleValues.Gray_50;
        public static readonly ushort ColorMenuText = (ushort)NwazetGoImaging.BasicColor.White;
        public static readonly ushort ColorMenuActive = (ushort)NwazetGoImaging.DefaultColorTheme.Darker;
        public static readonly ushort ColorMenuActiveLighter = (ushort)NwazetGoImaging.DefaultColorTheme.Base;
        public static readonly ushort ColorMenuActiveText = (ushort)NwazetGoImaging.BasicColor.Black;

        public static readonly NwazetGoImaging.RoundedCornerStyle CornerStyle = NwazetGoImaging.RoundedCornerStyle.All;

        public static void Main()
        {

            var canvas = new NwazetGoImaging.VirtualCanvas(TouchEventHandler, WidgetClickedHandler);
            canvas.Initialize(GoSockets.Socket5);

            CalibrateTouchscreen(canvas);
            // write your code here
            InitializeHerdRider(canvas);


        }


        public static void CalibrateTouchscreen(NwazetGoImaging.VirtualCanvas canvas)
        {
            try
            {
                var sd = new NwazetGoSD.SDCardReader();
                sd.Initialize(GoSockets.Socket8);
                var calibrationDataFilename = @"SD\TouchscreenCalibration.bin";
                // If the touchscreen calibration data was previously retrieved from the display module and was stored to an SD card,
                // the calibration data can be sent to the display module instead of calling TouchscreenCalibration() before using
                // the touchscreen for the first time.
                if (File.Exists(calibrationDataFilename))
                {
                    using (var calibrationDataFile = new FileStream(calibrationDataFilename, FileMode.Open))
                    {
                        var context = new NwazetGoHelpers.BasicTypeDeSerializerContext(calibrationDataFile);
                        var matrix = new NwazetGoDisplayTouchScreen.CalibrationMatrix();
                        matrix.Get(context);
                        canvas.SetTouchscreenCalibrationMatrix(matrix);
                    }
                }
                else
                {
                    // No pre-existing calibration data, create it...
                    using (var calibrationDataFile = new FileStream(calibrationDataFilename, FileMode.Create))
                    {
                        var matrix = canvas.GetTouchscreenCalibrationMatrix();
                        var context = new NwazetGoHelpers.BasicTypeSerializerContext(calibrationDataFile);
                        matrix.Put(context);
                    }
                }
                sd.Dispose();
            }
            catch (Exception)
            {
                Debug.Print("SD Card or file I/O error: manual calibration required.");
                canvas.TouchscreenCalibration();
            }
        }


        public static void InitializeHerdRider(NwazetGoImaging.VirtualCanvas canvas)
        {
            canvas.SetOrientation(NwazetGoImaging.Orientation.Portrait);
            canvas.DrawFill(ColorBackground);
            canvas.DrawString(35, 10, (ushort)NwazetGoImaging.BasicColor.Black, NwazetGoFonts.DejaVuSansBold9.ID, "Herd Rider by Larouex");

            var fontInfo = new NwazetGoFonts.DejaVuSans9().GetFontInfo();

            var button = new NwazetGoDisplayTouchScreen.ButtonWidget(10, 30, 220, 50, fontInfo, "Configure WiFi");
            button.FillColorClicked = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(255, 255, 255);
            button.FontColorClicked = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(0, 255, 0);
            button.FillColor = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(0, 0, 255);
            canvas.RegisterWidget(button);
            canvas.RenderWidgets();

            while (!button.Clicked)
            {

                canvas.ActivateWidgets(true);
                canvas.RenderWidgets();
                canvas.Execute();

                canvas.TouchscreenWaitForEvent();

                canvas.RenderWidgets(NwazetGoImaging.Render.All);
                canvas.Execute();
             
            }
        }

        public static void BmpImageTest(NwazetGoImaging.VirtualCanvas canvas)
        {
            try {
                var sd = new NwazetGoSD.SDCardReader();
                sd.Initialize(GoSockets.Socket8);
                DisplayBmpPicture(canvas, @"Nwazet\03.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\05.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\09.bmp");
                canvas.SetOrientation(NwazetGoImaging.Orientation.Landscape);
                DisplayBmpPicture(canvas, @"Nwazet\00.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\01.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\02.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\04.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\06.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\07.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\08.bmp");
                sd.Dispose();
            }catch(Exception e){
                Debug.Print(e.Message);
                Debug.Print("You need an SD card loaded with the demo photos to run this part of the demo.");
            }
        }
        private static void DisplayBmpPicture(NwazetGoImaging.VirtualCanvas canvas, string pictureName)
        {
            canvas.DrawBitmapImage(0, 0, @"SD\" + pictureName);
            canvas.TouchscreenWaitForEvent();
        }
       
        public static int lastTouchX;
        public static int lastTouchY;

        public static void TouchEventHandler(
            object sender, 
            NwazetGoDisplayTouchScreen.TouchEvent touchEvent)
        {
            Debug.Print("------------TouchEventHandler------------");
            Debug.Print("X: " + touchEvent.X);
            Debug.Print("Y: " + touchEvent.Y);
            Debug.Print("Pressure: " + touchEvent.Pressure);

            lastTouchX = touchEvent.X;
            lastTouchY = touchEvent.Y;
        }
        
        public static void WidgetClickedHandler(
            NwazetGoImaging.VirtualCanvas canvas, 
            NwazetGoDisplayTouchScreen.Widget widget, 
            NwazetGoDisplayTouchScreen.TouchEvent touchEvent)
        {
        }

        public static void BasicTouchEventTest(
            NwazetGoImaging.VirtualCanvas canvas)
        {
            var message = "Touch Event Test";
            var fontInfo = new NwazetGoFonts.DejaVuSansBold9().GetFontInfo();
            var stringLength = fontInfo.GetStringWidth(message);

            canvas.DrawFill(ColorBackground);
            canvas.DrawString(
                (canvas.Width - stringLength) / 2, 150,
                (ushort)NwazetGoImaging.BasicColor.Black, fontInfo.ID, message);

            canvas.TouchscreenWaitForEvent();

            canvas.DrawCircleFilled(lastTouchX, lastTouchY, 4, (ushort)NwazetGoImaging.BasicColor.Red);
            canvas.Execute();

            Thread.Sleep(1000);
        }
        
        public static void TouchscreenAlphanumericDialogTest(
            NwazetGoImaging.VirtualCanvas canvas)
        {
            canvas.SetOrientation(NwazetGoImaging.Orientation.Landscape);
            var response = canvas.TouchscreenShowDialog(NwazetGoImaging.DialogType.Alphanumeric);
            Debug.Print("User Input: " + response);
            canvas.SetOrientation(NwazetGoImaging.Orientation.Portrait);
            response = canvas.TouchscreenShowDialog(NwazetGoImaging.DialogType.Alphanumeric);
            Debug.Print("User Input: " + response);
        }

        public static void BasicUITest(
            NwazetGoImaging.VirtualCanvas canvas)
        {
            canvas.SetOrientation(NwazetGoImaging.Orientation.Portrait);
            canvas.DrawFill(ColorBackground);
            canvas.DrawString(35, 10, (ushort)NwazetGoImaging.BasicColor.Black, NwazetGoFonts.DejaVuSansBold9.ID, "Herd Rider by Larouex");
            canvas.Execute();

            /*
            canvas.DrawString(5, 30, (ushort)BasicColor.Black, DejaVuSans9.ID, "DejaVu Sans 9");
            canvas.DrawString(5, 50, (ushort)BasicColor.Black, DejaVuSansMono8.ID, "DejaVu Sans Mono 8");
            canvas.SetOrientation(Orientation.Landscape);
            canvas.DrawString(5, 10, (ushort)BasicColor.Black, DejaVuSans9.ID, "DejaVu Sans 9 (Rotated)");
            canvas.SetOrientation(Orientation.Portrait);
            RenderPrimitiveShapes(canvas);

            var fontInfo = new DejaVuSans9().GetFontInfo();

            RenderCompoundShapes(canvas, fontInfo);
            RenderIcons(canvas);

            var button = new ButtonWidget(20, 285, 200, 25, fontInfo, "Continue Demo");
            canvas.RegisterWidget(button);
            canvas.RenderWidgets();
            while (!button.Clicked) {
                canvas.TouchscreenWaitForEvent();
            }
            button.Dirty = true;
            canvas.RenderWidgets();
            canvas.Execute();
            canvas.UnRegisterWidget(button);
             * */
        }
        public static void MultiWidgetTest(NwazetGoImaging.VirtualCanvas canvas)
        {
            canvas.SetOrientation(NwazetGoImaging.Orientation.Landscape);
            canvas.DrawFill(NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(255, 255, 255));

            var fontInfo = new NwazetGoFonts.DejaVuSans9().GetFontInfo();

            var redButton = new NwazetGoDisplayTouchScreen.ButtonWidget(10, 204, 44, 22, fontInfo, "Red");
            redButton.FillColor = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(255, 0, 0);
            redButton.FillColorClicked = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(255, 255, 255);
            redButton.FontColorClicked = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(255, 0, 0);

            var greenButton = new NwazetGoDisplayTouchScreen.ButtonWidget(60, 204, 44, 22, fontInfo, "Green");
            greenButton.FillColor = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(0, 255, 0);
            greenButton.FillColorClicked = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(255, 255, 255);
            greenButton.FontColorClicked = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(0, 255, 0);

            var blueButton = new NwazetGoDisplayTouchScreen.ButtonWidget(110, 204, 44, 22, fontInfo, "Blue");
            blueButton.FillColor = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(0, 0, 255);
            blueButton.FillColorClicked = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(255, 255, 255);
            blueButton.FontColorClicked = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(0, 0, 255);

            var continueButton = new NwazetGoDisplayTouchScreen.ButtonWidget(247, 204, 64, 22, fontInfo, "Continue");
            continueButton.FillColor = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(255, 255, 255);
            continueButton.FontColorClicked = NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(0, 0, 0);

            canvas.RegisterWidget(redButton);
            canvas.RegisterWidget(greenButton);
            canvas.RegisterWidget(blueButton);
            canvas.RegisterWidget(continueButton);

            canvas.WidgetClicked += ColorButtonsClickedHandler;

            canvas.RenderWidgets();

            while (!continueButton.Clicked) {
                canvas.ActivateWidgets(true);
                canvas.RenderWidgets();
                canvas.Execute();

                canvas.TouchscreenWaitForEvent();

                canvas.RenderWidgets(NwazetGoImaging.Render.All);
                canvas.Execute();
            }

            canvas.WidgetClicked -= ColorButtonsClickedHandler;

            continueButton.Dirty = true;
            continueButton.Draw(canvas);
            canvas.Execute();

            canvas.UnRegisterAllWidgets();
        }
        public static void ColorButtonsClickedHandler(
            NwazetGoImaging.VirtualCanvas canvas,
            NwazetGoDisplayTouchScreen.Widget widget,
            NwazetGoDisplayTouchScreen.TouchEvent touchEvent)
        {
            widget.Dirty = true;
            canvas.DrawFill(((NwazetGoDisplayTouchScreen.ButtonWidget)widget).FillColor);
        }

        public static void RenderCompoundShapes(
            NwazetGoImaging.VirtualCanvas canvas,
            NwazetGoFonts.FontInfo fontInfo)
        {
            canvas.DrawProgressBar(
                70, 140,
                75, 12,
                CornerStyle,
                CornerStyle,
                (ushort)NwazetGoImaging.BasicColor.Black,
                (ushort)NwazetGoImaging.GrayScaleValues.Gray_128,
                (ushort)NwazetGoImaging.GrayScaleValues.Gray_30,
                (ushort)NwazetGoImaging.BasicColor.Green,
                78);
            canvas.DrawString(5, 144, (ushort)NwazetGoImaging.BasicColor.Black, fontInfo.ID, "Progress");
            canvas.DrawString(155, 144, (ushort)NwazetGoImaging.BasicColor.Black, fontInfo.ID, "78%");
            canvas.DrawRectangleFilled(0, 275, 239, 319, (ushort)NwazetGoImaging.GrayScaleValues.Gray_80);
        }
        public static void RenderPrimitiveShapes(NwazetGoImaging.VirtualCanvas canvas)
        {
            canvas.DrawLine(5, 65, 200, 65, (ushort)NwazetGoImaging.BasicColor.Red);
            canvas.DrawLine(5, 67, 200, 67, (ushort)NwazetGoImaging.BasicColor.Green);
            canvas.DrawLine(5, 69, 200, 69, (ushort)NwazetGoImaging.BasicColor.Blue);
            canvas.DrawCircleFilled(30, 105, 23, (ushort)NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(0x33, 0x00, 0x00));
            canvas.DrawCircleFilled(30, 105, 19, (ushort)NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(0x66, 0x00, 0x00));
            canvas.DrawCircleFilled(30, 105, 15, (ushort)NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(0x99, 0x00, 0x00));
            canvas.DrawCircleFilled(30, 105, 11, (ushort)NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(0xCC, 0x00, 0x00));
            canvas.DrawCircleFilled(30, 105, 7, (ushort)NwazetGoImaging.ColorHelpers.GetRGB24toRGB565(0xFF, 0x00, 0x00));
            canvas.DrawRectangleFilled(80, 80, 180, 125, (ushort)NwazetGoImaging.GrayScaleValues.Gray_15);
            canvas.DrawRectangleFilled(85, 85, 175, 120, (ushort)NwazetGoImaging.GrayScaleValues.Gray_30);
            canvas.DrawRectangleFilled(90, 90, 170, 115, (ushort)NwazetGoImaging.GrayScaleValues.Gray_50);
            canvas.DrawRectangleFilled(95, 95, 165, 110, (ushort)NwazetGoImaging.GrayScaleValues.Gray_80);
            canvas.DrawRectangleFilled(100, 100, 160, 105, (ushort)NwazetGoImaging.GrayScaleValues.Gray_128);
        }
        public static void RenderIcons(
            NwazetGoImaging.VirtualCanvas canvas)
        {
            // Cross/Failed
            canvas.DrawRectangleRounded(10, 190, 30, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(12, 192, (ushort)NwazetGoImaging.BasicColor.Red, NwazetGoImaging.Icons16.Failed);
            canvas.DrawRectangleRounded(10, 220, 30, 240, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(12, 222, (ushort)NwazetGoImaging.BasicColor.Red, NwazetGoImaging.Icons16.Failed);
            canvas.DrawIcon16(12, 222, (ushort)NwazetGoImaging.BasicColor.White, NwazetGoImaging.Icons16.FailedInterior);
            canvas.DrawRectangleRounded(10, 250, 30, 270, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(12, 252, (ushort)NwazetGoImaging.BasicColor.White, NwazetGoImaging.Icons16.FailedInterior);

            // Alert
            canvas.DrawRectangleRounded(40, 190, 60, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(42, 192, (ushort)NwazetGoImaging.BasicColor.Yellow, NwazetGoImaging.Icons16.Alert);
            canvas.DrawRectangleRounded(40, 220, 60, 240, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(42, 222, (ushort)NwazetGoImaging.BasicColor.Yellow, NwazetGoImaging.Icons16.Alert);
            canvas.DrawIcon16(42, 222, (ushort)NwazetGoImaging.BasicColor.White, NwazetGoImaging.Icons16.AlertInterior);
            canvas.DrawRectangleRounded(40, 250, 60, 270, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(42, 252, (ushort)NwazetGoImaging.BasicColor.White, NwazetGoImaging.Icons16.AlertInterior);

            // Checkmark/Passed
            canvas.DrawRectangleRounded(70, 190, 90, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(72, 192, (ushort)NwazetGoImaging.BasicColor.Green, NwazetGoImaging.Icons16.Passed);
            canvas.DrawRectangleRounded(70, 220, 90, 240, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(72, 222, (ushort)NwazetGoImaging.BasicColor.Green, NwazetGoImaging.Icons16.Passed);
            canvas.DrawIcon16(72, 222, (ushort)NwazetGoImaging.BasicColor.White, NwazetGoImaging.Icons16.PassedInterior);
            canvas.DrawRectangleRounded(70, 250, 90, 270, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(72, 252, (ushort)NwazetGoImaging.BasicColor.White, NwazetGoImaging.Icons16.PassedInterior);

            // Info
            canvas.DrawRectangleRounded(100, 190, 120, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(102, 192, (ushort)NwazetGoImaging.BasicColor.Blue, NwazetGoImaging.Icons16.Info);
            canvas.DrawRectangleRounded(100, 220, 120, 240, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(102, 222, (ushort)NwazetGoImaging.BasicColor.Blue, NwazetGoImaging.Icons16.Info);
            canvas.DrawIcon16(102, 222, (ushort)NwazetGoImaging.BasicColor.White, NwazetGoImaging.Icons16.InfoInterior);
            canvas.DrawRectangleRounded(100, 250, 120, 270, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(102, 252, (ushort)NwazetGoImaging.BasicColor.White, NwazetGoImaging.Icons16.InfoInterior);

            // Tools/Config
            canvas.DrawRectangleRounded(130, 190, 150, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(132, 192, (ushort)NwazetGoImaging.BasicColor.Green, NwazetGoImaging.Icons16.Tools);

            // Pointer
            canvas.DrawRectangleRounded(160, 190, 180, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(162, 192, (ushort)NwazetGoImaging.BasicColor.Magenta, NwazetGoImaging.Icons16.Pointer);
            canvas.DrawRectangleRounded(160, 220, 180, 240, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(162, 222, (ushort)NwazetGoImaging.BasicColor.Magenta, NwazetGoImaging.Icons16.Pointer);
            canvas.DrawIcon16(162, 222, (ushort)NwazetGoImaging.BasicColor.White, NwazetGoImaging.Icons16.PointerDot);
            canvas.DrawRectangleRounded(160, 250, 180, 270, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(162, 252, (ushort)NwazetGoImaging.BasicColor.White, NwazetGoImaging.Icons16.PointerDot);

            // Tag
            canvas.DrawRectangleRounded(190, 190, 210, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(192, 192, (ushort)NwazetGoImaging.BasicColor.Cyan, NwazetGoImaging.Icons16.Tag);
            canvas.DrawRectangleRounded(190, 220, 210, 240, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(192, 222, (ushort)NwazetGoImaging.BasicColor.Cyan, NwazetGoImaging.Icons16.Tag);
            canvas.DrawIcon16(192, 222, (ushort)NwazetGoImaging.BasicColor.White, NwazetGoImaging.Icons16.TagDot);
            canvas.DrawRectangleRounded(190, 250, 210, 270, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(192, 252, (ushort)NwazetGoImaging.BasicColor.White, NwazetGoImaging.Icons16.TagDot);
        }

    }
}
