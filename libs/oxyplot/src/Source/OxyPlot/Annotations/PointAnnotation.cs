﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PointAnnotation.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents an annotation that shows a point.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OxyPlot.Annotations
{
    /// <summary>
    /// Represents an annotation that shows a point.
    /// </summary>
    public class PointAnnotation : ShapeAnnotation
    {
        /// <summary>
        /// The position transformed to screen coordinates.
        /// </summary>
        private ScreenPoint screenPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointAnnotation" /> class.
        /// </summary>
        public PointAnnotation()
        {
            this.Size = 4;
            this.TextMargin = 2;
            this.Shape = MarkerType.Circle;
            this.TextVerticalAlignment = VerticalAlignment.Top;
        }

        /// <summary>
        /// Gets or sets the x-coordinate of the center.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the y-coordinate of the center.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the size of the rendered point.
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// Gets or sets the distance between the rendered point and the text.
        /// </summary>
        public double TextMargin { get; set; }

        /// <summary>
        /// Gets or sets the shape of the rendered point.
        /// </summary>
        /// <value>The shape.</value>
        public MarkerType Shape { get; set; }

        /// <summary>
        /// Renders the polygon annotation.
        /// </summary>
        /// <param name="rc">The render context.</param>
        /// <param name="model">The plot model.</param>
        public override void Render(IRenderContext rc, PlotModel model)
        {
            base.Render(rc, model);

            this.screenPosition = this.Transform(this.X, this.Y);

            // clip to the area defined by the axes
            var clippingRectangle = this.GetClippingRect();

            rc.DrawMarker(clippingRectangle, this.screenPosition, this.Shape, null, this.Size, this.Fill, this.Stroke, this.StrokeThickness);

            if (!string.IsNullOrEmpty(this.Text))
            {
                var dx = -(int)this.TextHorizontalAlignment * (this.Size + this.TextMargin);
                var dy = -(int)this.TextVerticalAlignment * (this.Size + this.TextMargin);
                var textPosition = this.screenPosition + new ScreenVector(dx, dy);
                rc.DrawClippedText(
                    clippingRectangle,
                    textPosition,
                    this.Text,
                    this.ActualTextColor,
                    this.ActualFont,
                    this.ActualFontSize,
                    this.ActualFontWeight,
                    this.TextRotation,
                    this.TextHorizontalAlignment,
                    this.TextVerticalAlignment);
            }
        }

        /// <summary>
        /// When overridden in a derived class, tests if the plot element is hit by the specified point.
        /// </summary>
        /// <param name="args">The hit test arguments.</param>
        /// <returns>
        /// The result of the hit test.
        /// </returns>
        protected override HitTestResult HitTestOverride(HitTestArguments args)
        {
            if (this.screenPosition.DistanceTo(args.Point) < this.Size)
            {
                return new HitTestResult(this, this.screenPosition);
            }

            return null;
        }
    }
}