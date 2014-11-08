﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScatterPoint.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents a point in a <see cref="ScatterSeries" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OxyPlot.Series
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a point in a <see cref="ScatterSeries" />.
    /// </summary>
    public class ScatterPoint : ICodeGenerating
    {
        /// <summary>
        /// The size.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        internal double size;

        /// <summary>
        /// The tag.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        internal object tag;

        /// <summary>
        /// The value.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        internal double value;

        /// <summary>
        /// The x.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        internal double x;

        /// <summary>
        /// The y.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        internal double y;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScatterPoint" /> class.
        /// </summary>
        public ScatterPoint()
        {
            this.Size = double.NaN;
            this.Value = double.NaN;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScatterPoint" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="size">The size.</param>
        /// <param name="value">The value.</param>
        /// <param name="tag">The tag.</param>
        public ScatterPoint(double x, double y, double size = double.NaN, double value = double.NaN, object tag = null)
        {
            this.x = x;
            this.y = y;
            this.size = size;
            this.value = value;
            this.tag = tag;
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public double Size
        {
            get
            {
                return this.size;
            }

            set
            {
                this.size = value;
            }
        }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>The tag.</value>
        public object Tag
        {
            get
            {
                return this.tag;
            }

            set
            {
                this.tag = value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public double Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.value = value;
            }
        }

        /// <summary>
        /// Gets or sets the X.
        /// </summary>
        /// <value>The X.</value>
        public double X
        {
            get
            {
                return this.x;
            }

            set
            {
                this.x = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y.
        /// </summary>
        /// <value>The Y.</value>
        public double Y
        {
            get
            {
                return this.y;
            }

            set
            {
                this.y = value;
            }
        }

        /// <summary>
        /// Returns C# code that generates this instance.
        /// </summary>
        /// <returns>C# code.</returns>
        public string ToCode()
        {
            if (double.IsNaN(this.size) && double.IsNaN(this.value))
            {
                return CodeGenerator.FormatConstructor(this.GetType(), "{0}, {1}", this.x, this.y);
            }

            if (double.IsNaN(this.value))
            {
                return CodeGenerator.FormatConstructor(this.GetType(), "{0}, {1}, {2}", this.x, this.y, this.size);
            }

            return CodeGenerator.FormatConstructor(
                this.GetType(), "{0}, {1}, {2}, {3}", this.x, this.y, this.size, this.value);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return this.x + " " + this.y;
        }
    }
}