﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegendExamples.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExampleLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using OxyPlot;
    using OxyPlot.Series;

    [Examples("Legends")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public static class LegendExamples
    {
        [Example("Legend at right top inside")]
        public static PlotModel LegendRightTopInside()
        {
            var model = CreateModel();
            model.LegendPlacement = LegendPlacement.Inside;
            model.LegendPosition = LegendPosition.RightTop;
            return model;
        }

        [Example("Legend at right top outside")]
        public static PlotModel LegendRightTopOutside()
        {
            var model = CreateModel();
            model.LegendPlacement = LegendPlacement.Outside;
            model.LegendPosition = LegendPosition.RightTop;
            return model;
        }

        [Example("Legend at BottomLeft outside horizontal")]
        public static PlotModel LegendBottomLeftHorizontal()
        {
            var model = CreateModel(4);
            model.LegendPlacement = LegendPlacement.Outside;
            model.LegendPosition = LegendPosition.BottomLeft;
            model.LegendOrientation = LegendOrientation.Horizontal;
            return model;
        }

        [Example("Legend at TopLeft outside vertical")]
        public static PlotModel LegendTopLeftVertical()
        {
            var model = CreateModel(4);
            model.LegendPlacement = LegendPlacement.Outside;
            model.LegendPosition = LegendPosition.TopLeft;
            model.LegendOrientation = LegendOrientation.Vertical;
            return model;
        }

        [Example("Default position")]
        public static PlotModel LegendDefault()
        {
            var model = CreateModel();
            return model;
        }

        [Example("Hidden Legend")]
        public static PlotModel LegendHidden()
        {
            var model = CreateModel();
            model.IsLegendVisible = false;
            return model;
        }

        [Example("Grayscale colors")]
        public static PlotModel LegendGrayscale()
        {
            var model = CreateModel();
            model.DefaultColors = new List<OxyColor> { OxyColors.Black, OxyColors.Gray };
            model.LegendSymbolLength = 32;
            return model;
        }

        [Example("Clipped legends")]
        public static PlotModel ClippedLegends()
        {
            var model = CreateModel(1);
            model.Series[0].Title = "1234567890 abcdefghijklmnopqrstuvwxyzæøå ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ 1234567890 abcdefghijklmnopqrstuvwxyzæøå ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ";
            model.LegendPlacement = LegendPlacement.Inside;
            model.LegendPosition = LegendPosition.RightTop;
            return model;
        }

        [Example("Clipped legends RightTop outside with MaxWidth")]
        public static PlotModel ClippedLegendsOutside()
        {
            var model = ClippedLegends();
            model.LegendPlacement = LegendPlacement.Outside;
            model.LegendMaxWidth = 200;
            return model;
        }

        [Example("Clipped legends TopRight outside")]
        public static PlotModel ClippedLegendsRight()
        {
            var model = ClippedLegends();
            model.LegendPlacement = LegendPlacement.Outside;
            model.LegendPosition = LegendPosition.TopRight;
            return model;
        }

        private static PlotModel CreateModel(int n = 20)
        {
            var model = new PlotModel { Title = "LineSeries", LegendBackground = OxyColor.FromAColor(200, OxyColors.White), LegendBorder = OxyColors.Black };
            for (int i = 1; i <= n; i++)
            {
                var s = new LineSeries { Title = "Series " + i };
                model.Series.Add(s);
                for (double x = 0; x < 2 * Math.PI; x += 0.1)
                {
                    s.Points.Add(new DataPoint(x, (Math.Sin(x * i) / i) + i));
                }
            }

            return model;
        }
    }
}