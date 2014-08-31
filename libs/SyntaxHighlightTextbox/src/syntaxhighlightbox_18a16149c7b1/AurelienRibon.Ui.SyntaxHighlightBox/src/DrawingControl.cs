using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace AurelienRibon.Ui.SyntaxHighlightBox
{
    public class DrawingControl : FrameworkElement
    {
        private VisualCollection mVisuals;
        private DrawingVisual mVisual;

        public DrawingControl()
        {
            mVisual = new DrawingVisual();
            mVisuals = new VisualCollection(this);
            mVisuals.Add(mVisual);
        }

        public DrawingContext GetContext()
        {
            return mVisual.RenderOpen();
        }

        protected override int VisualChildrenCount
        {
            get { return mVisuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= mVisuals.Count)
                throw new ArgumentOutOfRangeException();
            return mVisuals[index];
        }
    }
}
