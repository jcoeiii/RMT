using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PdfSharp;
using PdfSharp.Drawing;
using System.Drawing;
using PdfSharp.Pdf;
using MigraDoc;

using System.Diagnostics; // to start processes

namespace Tugwell
{
    public class Print2Pdf
    {
        private PdfDocument pdf = new PdfSharp.Pdf.PdfDocument();
        private PdfPage pdfPage;

        public enum TextStyle
        {
            Regular = 0,
            Bold = 1,
            Italic = 2,
            BoldItalic = 3,
            Underline = 4,
            Strikeout = 8,
        }

        public enum TextPos
        {
            //Default,
            TopLeft,
            Center,
            TopCenter,
            BottomCenter,
            TopRight
        }

        public class TextInfo
        {
            public TextInfo(List<double> rows, List<double> cols)
            {
                _row = rows;
                _col = cols;

                Font = "Times New Roman";
                Size = 8.0;
                Textstyle = TextStyle.Regular;
                Textpos = TextPos.TopLeft;
                Mycolor = XBrushes.Black;

                TextMarginLR = 0.0;
                TextMarginTB = 0.0;
            }

            public TextInfo()
            {
                _row = null;
                _col = null;

                Font = "Times New Roman";
                Size = 8.0;
                Textstyle = TextStyle.Regular;
                Textpos = TextPos.TopLeft;
                Mycolor = XBrushes.Black;

                TextMarginLR = 0.0;
                TextMarginTB = 0.0;
            }

            public TextInfo(string font, double size)
            {
                _row = null;
                _col = null;

                Font = font;
                Size = size;
                Textstyle = TextStyle.Regular;
                Textpos = TextPos.TopLeft;
                Mycolor = XBrushes.Black;

                TextMarginLR = 0.0;
                TextMarginTB = 0.0;
            }

            private List<double> _row, _col;
            public string Font { get; set; }
            public double Size { get; set; }
            public TextStyle Textstyle { get; set; }
            public TextPos Textpos { get; set; }
            public XBrush Mycolor { get; set; }
            public double TextMarginLR { get; set; }
            public double TextMarginTB { get; set; }
            public double X1 { get; set; }
            public double Y1 { get; set; }
            public double X2 { get; set; }
            public double Y2 { get; set; }

            public double GetRow(int index)
            {
                return _row[index];
            }

            public double GetCol(int index)
            {
                return _col[index];
            }
        }

        public TextInfo CreateTextInfo()
        {
            return new TextInfo();
        }

        public TextInfo CreateTextInfo(string font, double size)
        {
            return new TextInfo(font, size);
        }

        public Print2Pdf()
        {
            AddPage();
        }

        public bool Save(string filename)
        {
            try
            {
                pdf.Save(filename);
            }
            catch { return false; }
            return true;
        }

        public bool ShowPDF(string filename)
        {
            try
            {
                Process.Start(filename);
            }
            catch { return false; }
            return true;
        }

        private void AddPage()
        {
            pdfPage = pdf.AddPage();
        }

        public double Width { get { return pdfPage.Width.Point; } }
        public double Height { get { return pdfPage.Height.Point; } }

        public void AddText(TextInfo ti, string text)
        {
            AddText(ti, text, ti.X1, ti.Y1, ti.X2, ti.Y2);
        }

        public void AddText(TextInfo ti, string text, double x1, double y1, double x2, double y2)
        {
            XGraphics graph = XGraphics.FromPdfPage(pdfPage);

            string[] split = text.Split('\n');

            ti.X1 = x1;
            ti.Y1 = y1;
            ti.X2 = x2;
            ti.Y2 = y2;

            double delta = x2 - x1;

            foreach (string line in split)
            {
                XFont font = new XFont(ti.Font, ti.Size, (XFontStyle)ti.Textstyle);
                XSize size = graph.MeasureString(line, font);

                if (delta < 47.0 || line.Count() < 30 || size.Width < delta)
                {
                    if (ti.Textpos == TextPos.TopRight)
                    {
                        graph.DrawString(line, font, ti.Mycolor, new XRect(x1 + (delta - size.Width - ti.TextMarginLR * 2) + ti.TextMarginLR, ti.Y1 + ti.TextMarginTB, x2 - ti.TextMarginLR, ti.Y2 - ti.TextMarginTB),
                            //(ti.Textpos == TextPos.Default) ? XStringFormats.Default :
                            (ti.Textpos == TextPos.Center) ? XStringFormats.Center :
                            (ti.Textpos == TextPos.BottomCenter) ? XStringFormats.BottomCenter :
                            (ti.Textpos == TextPos.TopCenter) ? XStringFormats.TopCenter :
                            (ti.Textpos == TextPos.TopRight) ? XStringFormats.TopLeft :
                             XStringFormats.TopLeft);
                    }
                    else
                    {
                        graph.DrawString(line, font, ti.Mycolor, new XRect(x1 + ti.TextMarginLR, ti.Y1 + ti.TextMarginTB, x2 - ti.TextMarginLR, ti.Y2 - ti.TextMarginTB),
                            //(ti.Textpos == TextPos.Default) ? XStringFormats.Default :
                            (ti.Textpos == TextPos.Center) ? XStringFormats.Center :
                            (ti.Textpos == TextPos.BottomCenter) ? XStringFormats.BottomCenter :
                            (ti.Textpos == TextPos.TopCenter) ? XStringFormats.TopCenter :
                            (ti.Textpos == TextPos.TopRight) ? XStringFormats.TopLeft :
                             XStringFormats.TopLeft);
                    }
                    ti.Y1 += ti.Size;
                    ti.Y2 += ti.Size;
                }
                else
                {
                    List<string> subLines = new List<string>();
                    //size = graph.MeasureString(line, font);

                    int totalChars = line.Count();
                    int approxCount = (int)(((delta - 46.0) / size.Width) * (double)totalChars);

                    int i, start = 0;
                    for (i = approxCount; i < totalChars; i++)
                    {
                        if (line[i] == ' ')
                        {
                            int length = i - start + 1;
                            if ((length > 0) && ((length + start) <= totalChars))
                            {
                                subLines.Add(line.Substring(start, length));
                                start = i + 1;
                                i += approxCount;
                            }
                        }
                    }
                    try
                    {
                        subLines.Add(line.Substring(start, ((i > totalChars) ? totalChars : i) - start));
                    }
                    catch { }

                    foreach (string aline in subLines)
                    {
                        if (ti.Textpos == TextPos.TopRight)
                        {
                            XSize size2 = graph.MeasureString(aline, font);
                            graph.DrawString(line, font, ti.Mycolor, new XRect(x1 + (delta - size2.Width - ti.TextMarginLR * 2) + ti.TextMarginLR, ti.Y1 + ti.TextMarginTB, x2 - ti.TextMarginLR, ti.Y2 - ti.TextMarginTB),
                                //(ti.Textpos == TextPos.Default) ? XStringFormats.Default :
                                (ti.Textpos == TextPos.Center) ? XStringFormats.Center :
                                (ti.Textpos == TextPos.BottomCenter) ? XStringFormats.BottomCenter :
                                (ti.Textpos == TextPos.TopCenter) ? XStringFormats.TopCenter :
                                (ti.Textpos == TextPos.TopRight) ? XStringFormats.TopLeft :
                                 XStringFormats.TopLeft);
                        }
                        else
                        {
                            graph.DrawString(aline, font, ti.Mycolor, new XRect(x1 + ti.TextMarginLR, ti.Y1 + ti.TextMarginTB, x2 - ti.TextMarginLR, ti.Y2 - ti.TextMarginTB),
                                //(ti.Textpos == TextPos.Default) ? XStringFormats.Default :
                            (ti.Textpos == TextPos.Center) ? XStringFormats.Center :
                            (ti.Textpos == TextPos.BottomCenter) ? XStringFormats.BottomCenter :
                            (ti.Textpos == TextPos.TopCenter) ? XStringFormats.TopCenter :
                            (ti.Textpos == TextPos.TopRight) ? XStringFormats.TopLeft :
                             XStringFormats.TopLeft);
                        }
                        ti.Y1 += ti.Size;
                        ti.Y2 += ti.Size;
                    }
                }
            }

            graph.Dispose();
        }

        public void DrawHorizontalLine(double pinSize, double y, double margin)
        {
            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XPen pen = new XPen(XColor.FromName("Black"), pinSize);
            graph.DrawLine(pen, 0 + margin, y, this.Width - margin, y);
            graph.Dispose();
        }

        public void DrawHorizontalLineDouble(double pinSize, double y, double margin)
        {
            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XPen pen = new XPen(XColor.FromName("Black"), pinSize);
            graph.DrawLine(pen, 0 + margin, y, this.Width - margin, y);
            graph.DrawLine(pen, 0 + margin, y + pinSize * 2, this.Width - margin, y + pinSize * 2);
            graph.Dispose();
        }

        public void DrawVerticalLine(double pinSize, double x, double margin)
        {
            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XPen pen = new XPen(XColor.FromName("Black"), pinSize);
            graph.DrawLine(pen, x, 0 + margin, x, this.Height - margin);
            graph.Dispose();
        }

        public void DrawImage(Image image, double x, double y, double width, double heigth)
        {
            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XImage xi = (XImage)image;
            graph.DrawImage(xi, x, y, width, heigth);
            graph.Dispose();
        }

        public TextInfo AddTable(double xPosition, int rowCount, double rowHeight, List<double> colPercents, double tableMarginLR)
        {
            if (colPercents == null || colPercents.Count == 0)
                throw new Exception("Invalid column percents in table");

            double x = xPosition;
            double yLeft = tableMarginLR;
            double yRight = this.Width - yLeft;

            List<double> r = new List<double>();
            List<double> c = new List<double>();

            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XPen pen = new XPen(XColor.FromName("Black"), 0.5);

            for (int yy = 0; yy <= rowCount; yy++)
            {
                r.Add(x + yy * rowHeight);
                graph.DrawLine(pen, yLeft, x + yy * rowHeight, yRight, x + yy * rowHeight);
            }

            double yPos = yLeft;
            double tWidth = yRight - yLeft;

            for (int yy = 0; yy < colPercents.Count(); yy++)
            {
                graph.DrawLine(pen, yPos, x, yPos, x + rowCount * rowHeight);

                c.Add(yPos);
                yPos += tWidth * colPercents[yy] / 100;
            }
            c.Add(yPos);
            graph.DrawLine(pen, yPos, x, yPos, x + rowCount * rowHeight);
            graph.Dispose();

            return new TextInfo(r, c);
        }

        public void AddTableText(TextInfo ti, string text, int rowIndex, int colIndex)
        {
            AddText(ti, text, ti.GetCol(colIndex), ti.GetRow(rowIndex), ti.GetCol(colIndex + 1), ti.GetRow(rowIndex + 1));
        }
    }
}