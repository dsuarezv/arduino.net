﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XpsExporter.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Provides functionality to export plots to xps.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OxyPlot.Xps
{
    using System.IO;
    using System.IO.Packaging;
    using System.Printing;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Xps.Packaging;

    using OxyPlot.Wpf;

    /// <summary>
    /// Provides functionality to export plots to xps.
    /// </summary>
    public class XpsExporter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XpsExporter" /> class.
        /// </summary>
        public XpsExporter()
        {
            this.Width = 600;
            this.Height = 400;
            this.Background = OxyColors.White;
        }

        /// <summary>
        /// Gets or sets the width of the output document.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the output document.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public OxyColor Background { get; set; }

#if !NET35
        /// <summary>
        /// Gets or sets the text formatting mode.
        /// </summary>
        /// <value>The text formatting mode.</value>
        public TextFormattingMode TextFormattingMode { get; set; }
#endif

        /// <summary>
        /// Exports the specified plot model to an xps file.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="background">The background color.</param>
        public static void Export(IPlotModel model, string fileName, double width, double height, OxyColor background)
        {
            using (var xpsPackage = Package.Open(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var doc = new XpsDocument(xpsPackage))
                {
                    var canvas = new Canvas { Width = width, Height = height, Background = background.ToBrush() };
                    canvas.Measure(new Size(width, height));
                    canvas.Arrange(new Rect(0, 0, width, height));

                    var rc = new ShapesRenderContext(canvas);
#if !NET35
                    rc.TextFormattingMode = TextFormattingMode.Ideal;
#endif

                    model.Update(true);
                    model.Render(rc, width, height);

                    canvas.UpdateLayout();

                    var xpsdw = XpsDocument.CreateXpsDocumentWriter(doc);
                    xpsdw.Write(canvas);
                }
            }
        }

        /// <summary>
        /// Exports the specified <see cref="PlotModel" /> to the specified <see cref="Stream" />.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public static void Export(IPlotModel model, Stream stream, double width, double height)
        {
            var exporter = new XpsExporter { Width = width, Height = height };
            exporter.Export(model, stream);
        }

        /// <summary>
        /// Prints the specified plot model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="width">The width (using the actual media width if set to NaN).</param>
        /// <param name="height">The height (using the actual media height if set to NaN).</param>
        public static void Print(IPlotModel model, double width, double height)
        {
            var exporter = new XpsExporter { Width = width, Height = height, Background = model.Background };
            exporter.Print(model);
        }

        /// <summary>
        /// Exports the specified <see cref="PlotModel" /> to the specified <see cref="Stream" />.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="stream">The stream.</param>
        public void Export(IPlotModel model, Stream stream)
        {
            using (var xpsPackage = Package.Open(stream))
            {
                using (var doc = new XpsDocument(xpsPackage))
                {
                    var canvas = new Canvas { Width = this.Width, Height = this.Height, Background = this.Background.ToBrush() };
                    canvas.Measure(new Size(this.Width, this.Height));
                    canvas.Arrange(new Rect(0, 0, this.Width, this.Height));

                    var rc = new ShapesRenderContext(canvas);
#if !NET35
                    rc.TextFormattingMode = this.TextFormattingMode;
#endif
                    model.Update(true);
                    model.Render(rc, this.Width, this.Height);

                    canvas.UpdateLayout();

                    var xpsdw = XpsDocument.CreateXpsDocumentWriter(doc);
                    xpsdw.Write(canvas);
                }
            }
        }

        /// <summary>
        /// Prints the specified plot model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Print(IPlotModel model)
        {
            PrintDocumentImageableArea area = null;
            var xpsDocumentWriter = PrintQueue.CreateXpsDocumentWriter(ref area);
            if (xpsDocumentWriter != null)
            {
                var width = this.Width;
                var height = this.Height;
                if (double.IsNaN(width))
                {
                    width = area.MediaSizeWidth;
                }

                if (double.IsNaN(height))
                {
                    height = area.MediaSizeHeight;
                }

                var canvas = new Canvas { Width = width, Height = height, Background = this.Background.ToBrush() };
                canvas.Measure(new Size(width, height));
                canvas.Arrange(new Rect(0, 0, width, height));

                var rc = new ShapesRenderContext(canvas);
#if !NET35
                rc.TextFormattingMode = this.TextFormattingMode;
#endif
                model.Update(true);
                model.Render(rc, width, height);

                canvas.UpdateLayout();

                xpsDocumentWriter.Write(canvas);
            }
        }
    }
}