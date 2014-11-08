﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilteringExamples.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExampleLibrary
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;

    [Examples("Filtering data points")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public static class FilteringExamples
    {
        [Example("Filtering NaN points")]
        public static PlotModel FilteringInvalidPoints()
        {
            var plot = new PlotModel { Title = "Filtering NaN points" };
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left });

            var ls1 = new LineSeries();
            ls1.Points.Add(new DataPoint(double.NaN, double.NaN));
            ls1.Points.Add(new DataPoint(1, 0));
            ls1.Points.Add(new DataPoint(2, 10));
            ls1.Points.Add(new DataPoint(double.NaN, 20));
            ls1.Points.Add(new DataPoint(3, 10));
            ls1.Points.Add(new DataPoint(4, 0));
            ls1.Points.Add(new DataPoint(4.5, double.NaN));
            ls1.Points.Add(new DataPoint(5, 0));
            ls1.Points.Add(new DataPoint(7, 7));
            ls1.Points.Add(new DataPoint(double.NaN, double.NaN));
            ls1.Points.Add(new DataPoint(double.NaN, double.NaN));
            ls1.Points.Add(new DataPoint(7, 0));
            ls1.Points.Add(new DataPoint(double.NaN, double.NaN));
            plot.Series.Add(ls1);

            return plot;
        }

        [Example("Filtering undefined points")]
        public static PlotModel FilteringUndefinedPoints()
        {
            var plot = new PlotModel { Title = "Filtering undefined points" };
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left });

            var ls1 = new LineSeries();
            ls1.Points.Add(DataPoint.Undefined);
            ls1.Points.Add(new DataPoint(1, 0));
            ls1.Points.Add(new DataPoint(2, 10));
            ls1.Points.Add(DataPoint.Undefined);
            ls1.Points.Add(new DataPoint(3, 10));
            ls1.Points.Add(new DataPoint(4, 0));
            ls1.Points.Add(DataPoint.Undefined);
            ls1.Points.Add(new DataPoint(5, 0));
            ls1.Points.Add(new DataPoint(7, 7));
            ls1.Points.Add(DataPoint.Undefined);
            ls1.Points.Add(DataPoint.Undefined);
            ls1.Points.Add(new DataPoint(7, 0));
            ls1.Points.Add(DataPoint.Undefined);
            plot.Series.Add(ls1);

            return plot;
        }

        [Example("Filtering invalid points (log axis)")]
        public static PlotModel FilteringInvalidPointsLog()
        {
            var plot = new PlotModel { Title = "Filtering invalid points on logarithmic axes" };
            plot.Axes.Add(new LogarithmicAxis { Position = AxisPosition.Bottom });
            plot.Axes.Add(new LogarithmicAxis { Position = AxisPosition.Left });

            var ls = new LineSeries();
            ls.Points.Add(new DataPoint(double.NaN, double.NaN));
            ls.Points.Add(new DataPoint(1, 1));
            ls.Points.Add(new DataPoint(10, 10));
            ls.Points.Add(new DataPoint(0, 20));
            ls.Points.Add(new DataPoint(100, 2));
            ls.Points.Add(new DataPoint(1000, 12));
            ls.Points.Add(new DataPoint(4.5, 0));
            ls.Points.Add(new DataPoint(10000, 4));
            ls.Points.Add(new DataPoint(100000, 14));
            ls.Points.Add(new DataPoint(double.NaN, double.NaN));
            ls.Points.Add(new DataPoint(1000000, 5));
            ls.Points.Add(new DataPoint(double.NaN, double.NaN));
            plot.Series.Add(ls);
            return plot;
        }

        [Example("Filtering points outside (-1,1)")]
        public static PlotModel FilteringPointsOutsideRange()
        {
            var plot = new PlotModel { Title = "Filtering points outside (-1,1)" };
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, FilterMinValue = -1, FilterMaxValue = 1 });
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, FilterMinValue = -1, FilterMaxValue = 1 });

            var ls = new LineSeries();
            for (double i = 0; i < 200; i += 0.01)
            {
                ls.Points.Add(new DataPoint(0.01 * i * Math.Sin(i), 0.01 * i * Math.Cos(i)));
            }

            plot.Series.Add(ls);
            return plot;
        }
    }
}