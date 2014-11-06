﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Example.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExampleLibrary
{
    using OxyPlot;

    public class Example
    {
        public Example(PlotModel model, IPlotController controller = null)
        {
            this.Model = model;
            this.Controller = controller;
        }

        public IPlotController Controller { get; private set; }

        public PlotModel Model { get; private set; }
    }
}