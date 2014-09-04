using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using System.Collections.Generic;
using System.Threading;

namespace AurelienRibon.Ui.SyntaxHighlightBox
{
    public partial class SyntaxHighlightBox : TextBox
    {
        public static readonly DependencyProperty IsLineNumbersMarginVisibleProperty = 
            DependencyProperty.Register(
                "IsLineNumbersMarginVisible", 
                typeof(bool), 
                typeof(SyntaxHighlightBox), 
                new PropertyMetadata(true));


        public bool IsLineNumbersMarginVisible
        {
            get { return (bool)GetValue(IsLineNumbersMarginVisibleProperty); }
            set { SetValue(IsLineNumbersMarginVisibleProperty, value); }
        }


        public double LineHeight
        {
            get { return mLineHeight; }
            set
            {
                if (value != mLineHeight)
                {
                    mLineHeight = value;
                    mBlockHeight = MaxLineCountInBlock * value;
                    TextBlock.SetLineStackingStrategy(this, LineStackingStrategy.BlockLineHeight);
                    TextBlock.SetLineHeight(this, mLineHeight);
                }
            }
        }

        public int MaxLineCountInBlock
        {
            get { return mMaxLineCountInBlock; }
            set
            {
                mMaxLineCountInBlock = value > 0 ? value : 0;
                mBlockHeight = value * LineHeight;
            }
        }


        public IHighlighter CurrentHighlighter { get; set; }


        public event Action<int, DrawingContext, Rect> BeforeDrawingTextLine;
        public event Action<int, DrawingContext, Rect> BeforeDrawingLineNumber;


        private DrawingControl mRenderCanvas;
        private DrawingControl mLineNumbersCanvas;
        private ScrollViewer mScrollViewer;
        private double mLineHeight;
        private int mTotalLineCount;
        private List<InnerTextBlock> mBlocks;
        private double mBlockHeight;
        private int mMaxLineCountInBlock;

       
        public SyntaxHighlightBox()
        {
            InitializeComponent();

            MaxLineCountInBlock = 100;
            LineHeight = FontSize * 1.3;
            mTotalLineCount = 1;
            mBlocks = new List<InnerTextBlock>();

            Loaded += (s, e) =>
            {
                InvalidateBlocks(0);
                InvalidateVisual();
            };

            SizeChanged += (s, e) =>
            {
                if (e.HeightChanged == false) return;

                UpdateBlocks();
                InvalidateVisual();
            };

            TextChanged += (s, e) =>
            {
                UpdateTotalLineCount();
                InvalidateBlocks(e.Changes.First().Offset);
                InvalidateVisual();
            };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // This was in the Loaded event handler. If set there, the Template.FindName calls 
            // returned null if more than one Textbox was created. So here it works fine.
            mRenderCanvas = (DrawingControl)Template.FindName("PART_RenderCanvas", this);
            mLineNumbersCanvas = (DrawingControl)Template.FindName("PART_LineNumbersCanvas", this);
            mScrollViewer = (ScrollViewer)Template.FindName("PART_ContentHost", this);

            mLineNumbersCanvas.Width = GetFormattedTextWidth(string.Format("{0:0000}", mTotalLineCount)) + 5;

            mScrollViewer.ScrollChanged += OnScrollChanged;

            InvalidateBlocks(0);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            DrawBlocks();
            base.OnRender(drawingContext);
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0) UpdateBlocks();

            InvalidateVisual();
        }

        

        private void UpdateTotalLineCount()
        {
            mTotalLineCount = TextUtilities.GetLineCount(Text);

            if (mLineNumbersCanvas == null) return;

            mLineNumbersCanvas.Width = GetFormattedTextWidth(string.Format("{0:0000}", mTotalLineCount)) + 5;
        }

        private void UpdateBlocks()
        {
            if (mBlocks.Count == 0) return;

            // While something is visible after last block...
            while (!mBlocks.Last().IsLast && mBlocks.Last().Position.Y + mBlockHeight - VerticalOffset < ActualHeight)
            {
                int firstLineIndex = mBlocks.Last().LineEndIndex + 1;
                int lastLineIndex = firstLineIndex + mMaxLineCountInBlock - 1;
                lastLineIndex = lastLineIndex <= mTotalLineCount - 1 ? lastLineIndex : mTotalLineCount - 1;

                int firstCharIndex = mBlocks.Last().CharEndIndex + 1;
                int lastCharIndex = TextUtilities.GetLastCharIndexFromLineIndex(Text, lastLineIndex); // to be optimized (forward search)

                if (lastCharIndex <= firstCharIndex)
                {
                    mBlocks.Last().IsLast = true;
                    return;
                }

                InnerTextBlock block = new InnerTextBlock(
                    firstCharIndex,
                    lastCharIndex,
                    mBlocks.Last().LineEndIndex + 1,
                    lastLineIndex,
                    LineHeight);

                block.RawText = block.GetSubString(Text);
                block.LineNumbers = GetFormattedLineNumbers(block.LineStartIndex, block.LineEndIndex);
                mBlocks.Add(block);
                FormatBlock(block, mBlocks.Count > 1 ? mBlocks[mBlocks.Count - 2] : null);
            }
        }

        private void InvalidateBlocks(int changeOffset)
        {
            InnerTextBlock changedBlock = null;

            for (int i = 0; i < mBlocks.Count; i++)
            {
                if (mBlocks[i].CharStartIndex <= changeOffset && changeOffset <= mBlocks[i].CharEndIndex + 1)
                {
                    changedBlock = mBlocks[i];
                    break;
                }
            }

            if (changedBlock == null && changeOffset > 0)
            { 
                changedBlock = mBlocks.Last();
            }

            int fvline = changedBlock != null ? changedBlock.LineStartIndex : 0;
            int lvline = GetIndexOfLastVisibleLine();
            int fvchar = changedBlock != null ? changedBlock.CharStartIndex : 0;
            int lvchar = TextUtilities.GetLastCharIndexFromLineIndex(Text, lvline);

            if (changedBlock != null)
            { 
                mBlocks.RemoveRange(mBlocks.IndexOf(changedBlock), mBlocks.Count - mBlocks.IndexOf(changedBlock));
            }

            int localLineCount = 1;
            int charStart = fvchar;
            int lineStart = fvline;

            for (int i = fvchar; i < Text.Length; i++)
            {
                if (Text[i] == '\n') localLineCount += 1;

                if (i == Text.Length - 1)
                {
                    string blockText = Text.Substring(charStart);
                    InnerTextBlock block = new InnerTextBlock(
                        charStart,
                        i, lineStart,
                        lineStart + TextUtilities.GetLineCount(blockText) - 1,
                        LineHeight);

                    block.RawText = block.GetSubString(Text);
                    block.LineNumbers = GetFormattedLineNumbers(block.LineStartIndex, block.LineEndIndex);
                    block.IsLast = true;

                    foreach (InnerTextBlock b in mBlocks)
                    { 
                        if (b.LineStartIndex == block.LineStartIndex) throw new Exception();
                    }

                    mBlocks.Add(block);
                    FormatBlock(block, mBlocks.Count > 1 ? mBlocks[mBlocks.Count - 2] : null);
                    break;
                }

                if (localLineCount > mMaxLineCountInBlock)
                {
                    InnerTextBlock block = new InnerTextBlock(
                        charStart,
                        i,
                        lineStart,
                        lineStart + mMaxLineCountInBlock - 1,
                        LineHeight);

                    block.RawText = block.GetSubString(Text);
                    block.LineNumbers = GetFormattedLineNumbers(block.LineStartIndex, block.LineEndIndex);

                    foreach (InnerTextBlock b in mBlocks)
                    { 
                        if (b.LineStartIndex == block.LineStartIndex) throw new Exception();
                    }

                    mBlocks.Add(block);
                    FormatBlock(block, mBlocks.Count > 1 ? mBlocks[mBlocks.Count - 2] : null);

                    charStart = i + 1;
                    lineStart += mMaxLineCountInBlock;
                    localLineCount = 1;

                    if (i > lvchar) break;
                }
            }
        }


        private void DrawBlocks()
        {
            if (!IsLoaded || mRenderCanvas == null || mLineNumbersCanvas == null) return;

            using (var dc = mRenderCanvas.GetContext())
            { 
                using (var dc2 = mLineNumbersCanvas.GetContext())
                {
                    for (int i = 0; i < mBlocks.Count; i++)
                    {
                        InnerTextBlock block = mBlocks[i];
                        Point blockPos = block.Position;
                        double top = blockPos.Y - VerticalOffset;
                        double bottom = top + mBlockHeight;

                        if (top < ActualHeight && bottom > 0)
                        {
                            try
                            {
                                UserLineDrawEvent(block, top, bottom, dc);

                                dc.DrawText(block.FormattedText, new Point(2 - HorizontalOffset, top));

                                if (IsLineNumbersMarginVisible)
                                {
                                    var p = new Point(mLineNumbersCanvas.ActualWidth, top + 1);

                                    UserLineNumberDrawEvent(block, top + 1, bottom, dc2);

                                    dc2.DrawText(block.LineNumbers, p);
                                }
                            }
                            catch
                            {
                                var a = 0;
                                // Don't know why this exception is raised sometimes.
                                // Reproduce steps:
                                // - Sets a valid syntax highlighter on the box.
                                // - Copy a large chunk of code in the clipboard.
                                // - Paste it using ctrl+v and keep these buttons pressed.
                            }
                        }
                    }
                }
            }
        }


        private void UserLineDrawEvent(InnerTextBlock block, double top, double bottom, DrawingContext dc)
        { 
            if (BeforeDrawingTextLine == null) return;

            for (int i = 0; i < block.NumLines; ++i)
            {
                var y = top + i * mLineHeight;
                BeforeDrawingTextLine(block.LineStartIndex + i, dc, new Rect(0, y, ViewportWidth, mLineHeight));
            }
        }

        private void UserLineNumberDrawEvent(InnerTextBlock block, double top, double bottom, DrawingContext dc)
        { 
            if (BeforeDrawingLineNumber == null) return;

            for (int i = 0; i < block.NumLines; ++i)
            {
                var y = top + i * mLineHeight;
                BeforeDrawingLineNumber(block.LineStartIndex + i, dc, new Rect(0, y, mLineNumbersCanvas.ActualWidth, mLineHeight));
            }
        }


        /// <summary>
        /// Returns the index of the first visible text line.
        /// </summary>
        private int GetIndexOfFirstVisibleLine()
        {
            int guessedLine = (int)(VerticalOffset / mLineHeight);
            return guessedLine > mTotalLineCount ? mTotalLineCount : guessedLine;
        }

        /// <summary>
        /// Returns the index of the last visible text line.
        /// </summary>
        private int GetIndexOfLastVisibleLine()
        {
            double height = VerticalOffset + ViewportHeight;
            int guessedLine = (int)(height / mLineHeight);
            return guessedLine > mTotalLineCount - 1 ? mTotalLineCount - 1 : guessedLine;
        }

        /// <summary>
        /// Formats and Highlights the text of a block.
        /// </summary>
        private void FormatBlock(InnerTextBlock currentBlock, InnerTextBlock previousBlock)
        {
            currentBlock.FormattedText = GetFormattedText(currentBlock.RawText);

            if (CurrentHighlighter != null)
            {
                //ThreadPool.QueueUserWorkItem(p =>
                //{
                    int previousCode = previousBlock != null ? previousBlock.Code : -1;
                    currentBlock.Code = CurrentHighlighter.Highlight(currentBlock.FormattedText, previousCode);
                //});
            }
        }

        /// <summary>
        /// Returns a formatted text object from the given string
        /// </summary>
        private FormattedText GetFormattedText(string text)
        {
            FormattedText ft = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Brushes.Black);

            ft.Trimming = TextTrimming.None;
            ft.LineHeight = mLineHeight;

            return ft;
        }

        /// <summary>
        /// Returns a string containing a list of numbers separated with newlines.
        /// </summary>
        private FormattedText GetFormattedLineNumbers(int firstIndex, int lastIndex)
        {
            string text = "";
            for (int i = firstIndex + 1; i <= lastIndex + 1; i++)
                text += i.ToString() + "\n";
            text = text.Trim();

            FormattedText ft = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                new SolidColorBrush(Color.FromRgb(0x21, 0xA1, 0xD8)));

            ft.Trimming = TextTrimming.None;
            ft.LineHeight = mLineHeight;
            ft.TextAlignment = TextAlignment.Right;

            return ft;
        }

        /// <summary>
        /// Returns the width of a text once formatted.
        /// </summary>
        private double GetFormattedTextWidth(string text)
        {
            FormattedText ft = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Brushes.Black);

            ft.Trimming = TextTrimming.None;
            ft.LineHeight = mLineHeight;

            return ft.Width;
        }


        
        private class InnerTextBlock
        {
            public string RawText;
            public FormattedText FormattedText;
            public FormattedText LineNumbers;
            public int CharStartIndex { get; private set; }
            public int CharEndIndex { get; private set; }
            public int LineStartIndex { get; private set; }
            public int LineEndIndex { get; private set; }
            public bool IsLast;
            public int Code;
            
            public Point Position 
            { 
                get { return new Point(0, LineStartIndex * mLineHeight); } 
            }

            public int NumLines
            {
                get { return LineEndIndex - LineStartIndex + 1; }
            }

            
            private double mLineHeight;


            public InnerTextBlock(int charStart, int charEnd, int lineStart, int lineEnd, double lineHeight)
            {
                CharStartIndex = charStart;
                CharEndIndex = charEnd;
                LineStartIndex = lineStart;
                LineEndIndex = lineEnd;
                mLineHeight = lineHeight;
                IsLast = false;
            }

            public string GetSubString(string text)
            {
                return text.Substring(CharStartIndex, CharEndIndex - CharStartIndex + 1);
            }

            public override string ToString()
            {
                return string.Format("L:{0}/{1} C:{2}/{3} {4}",
                    LineStartIndex,
                    LineEndIndex,
                    CharStartIndex,
                    CharEndIndex,
                    FormattedText.Text);
            }
        }
    }
}
