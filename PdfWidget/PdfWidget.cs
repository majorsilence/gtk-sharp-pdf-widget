// License:  There is no license.  You can use this in any software with any license you want.
// Original Author: Peter Gill <peter@majorsilence.com>

using System;
using DtronixPdf;
using System.Runtime.Intrinsics;

namespace PdfWidget
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class PdfWidget : Gtk.Bin
    {
        private PdfDocument pdf;

        private int pageIndex = 0;
        private int pageHeight = 0;
        private float scale = 1f;

        public PdfWidget()
        {
            this.Build();

            scrolledwindow1.Vadjustment.ValueChanged += HandleScrollChanged;
        }

        private byte[] BitmapPointerToByteArray(IntPtr bitmapPointer, int length)
        {
            byte[] byteArray = new byte[length];
            System.Runtime.InteropServices.Marshal.Copy(bitmapPointer, byteArray, 0, length);
            return byteArray;
        }

        private void RenderPage()
        {
            var page = pdf.GetPage(this.pageIndex);

            int width = (int)(page.Width * scale);
            pageHeight = (int)(page.Height * scale);
            long length = width * pageHeight * 4;

            // Calculate the stride (row length in bytes) for the Pixbuf
            int stride = width * 4; // 4 bytes per pixel (ARGB32)

            // Render the PDF page into the pixel buffer
            var bitmap = page.Render(new PdfPageRenderConfig()
            {
                Scale = scale,
                Viewport = Vector128.Create(0, 0, (float)width, (float)pageHeight)
            });

            // Convert the bitmap pointer to a byte array
            byte[] bitmapData = BitmapPointerToByteArray(new IntPtr(bitmap.Pointer), stride * pageHeight);

            // Create a Pixbuf from the raw pixel data
            var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, width, pageHeight);

            // Copy the rendered pixel buffer into the Pixbuf
            System.Runtime.InteropServices.Marshal.Copy(bitmapData, 0, pixbuf.Pixels, bitmapData.Length);

            Gtk.Image img = new Gtk.Image();
            img.SetSizeRequest(width, pageHeight);
            img.Pixbuf = pixbuf;
            img.Name = "image1";

            vboxImages.Add(img);
        }


        private void RenderPages()
        {
            foreach (Gtk.Widget w in vboxImages.AllChildren)
            {
                vboxImages.Remove(w);
            }

            for (this.pageIndex = 0; this.pageIndex < pdf.Pages; this.pageIndex++)
            {
                RenderPage();
            }

            CurrentPage.Value = CurrentPage.Value + 0;
            this.ShowAll();
        }

        /// <summary>
        /// Loads the pdf.  This is the function you call when you want to display a pdf.
        /// </summary>
        /// <param name='pdfFileName'>
        /// Pdf file name.
        /// </param>
        public void LoadPdf(string pdfFileName)
        {
            // Load the document.
            pdf = PdfDocument.Load(pdfFileName, null);

            PageCountLabel.Text = @"/" + (pdf.Pages - 1).ToString();
            CurrentPage.Value = 0;
            CurrentPage.Adjustment.Upper = pdf.Pages - 1;

            RenderPages();
        }



        /// <summary>
        /// Raises the next button clicked event.  Only used in single page mode.
        /// </summary>
        /// <param name='sender'>
        /// Sender.
        /// </param>
        /// <param name='e'>
        /// E.
        /// </param>
        protected void OnNextButtonClicked(object sender, System.EventArgs e)
        {
            if (pdf.Pages > (int)CurrentPage.Value)
            {
                CurrentPage.Value = CurrentPage.Value + 1;
            }
        }

        /// <summary>
        /// Raises the previous button clicked event. Only used in single page mode.
        /// </summary>
        /// <param name='sender'>
        /// Sender.
        /// </param>
        /// <param name='e'>
        /// E.
        /// </param>
        protected void OnPreviousButtonClicked(object sender, System.EventArgs e)
        {
            if ((int)CurrentPage.Value > 0)
            {
                CurrentPage.Value = CurrentPage.Value - 1;
            }
        }

        protected void OnFirstPageButtonClicked(object sender, System.EventArgs e)
        {
            CurrentPage.Value = 0;
        }

        protected void OnLastPageButtonClicked(object sender, System.EventArgs e)
        {
            CurrentPage.Value = pdf.Pages;
        }

        private int previousPage = 0;
        protected void OnCurrentPageValueChanged(object sender, System.EventArgs e)
        {

            if (_ignorePageChange)
            {
                // If the page is changed because of scrolling skip everything below.
                // However still set the previous page incase the user starts using the button.
                previousPage = (int)CurrentPage.Value;
                return;
            }

            if (CurrentPage.Value == previousPage)
            {
                return;
            }


            int pageDifference = Math.Abs(previousPage - (int)CurrentPage.Value);
            int moveHeight = ((vboxImages.Spacing + pageHeight) * pageDifference);

            if ((int)CurrentPage.Value == 0)
            {
                scrolledwindow1.Hadjustment.Value = scrolledwindow1.Hadjustment.Lower;
                scrolledwindow1.Vadjustment.Value = scrolledwindow1.Vadjustment.Lower;
            }
            else if ((int)CurrentPage.Value == pdf.Pages)
            {
                scrolledwindow1.Hadjustment.Value = scrolledwindow1.Hadjustment.Lower;
                scrolledwindow1.Vadjustment.Value = scrolledwindow1.Vadjustment.Upper - pageHeight;
            }
            else if (previousPage > (int)CurrentPage.Value)
            { // Move back one page
                scrolledwindow1.Hadjustment.Value = scrolledwindow1.Hadjustment.Lower;
                scrolledwindow1.Vadjustment.Value = scrolledwindow1.Vadjustment.Value - moveHeight;
            }
            else if ((int)CurrentPage.Value > previousPage)
            { // Move forward 1 page
                scrolledwindow1.Hadjustment.Value = scrolledwindow1.Hadjustment.Lower;
                scrolledwindow1.Vadjustment.Value = scrolledwindow1.Vadjustment.Value + moveHeight;
            }


            previousPage = (int)CurrentPage.Value;
        }


        protected void OnPrintButtonClicked(object sender, System.EventArgs e)
        {
            Gtk.PrintOperation print = new Gtk.PrintOperation();
            // Tell the Print Operation how many pages there are
            print.NPages = this.pdf.Pages;

            print.BeginPrint += new Gtk.BeginPrintHandler(OnBeginPrint);
            print.DrawPage += new Gtk.DrawPageHandler(OnDrawPage);
            print.EndPrint += new Gtk.EndPrintHandler(OnEndPrint);

            // Run the Print Operation and tell it what it should do (Export, Preview, Print, PrintDialog)
            // And provide a parent window if applicable
            print.Run(Gtk.PrintOperationAction.PrintDialog, null);
            print = null;
        }

        /// <summary>
        /// OnBeginPrint - Load up the Document to be printed and analyze it.
        /// </summary>
        /// <param name="obj">The Print Operation</param>
        /// <param name="args">The BeginPrintArgs passed by the Print Operation</param>
        private void OnBeginPrint(object obj, Gtk.BeginPrintArgs args)
        {

        }

        /// <summary>
        /// OnDrawPage - Draws the Content of each Page to be printed
        /// </summary>
        /// <param name="obj">The Print Operation</param>
        /// <param name="args">The DrawPageArgs passed by the Print Operation</param>
        private void OnDrawPage(object obj, Gtk.DrawPageArgs args)
        {
            // Create a Print Context from the Print Operation
            Gtk.PrintContext context = args.Context;

            // Create a Cairo Context from the Print Context
            Cairo.Context cr = context.CairoContext;

            var pg = this.pdf.GetPage(args.PageNr);

            // pg.RenderForPrintingWithOptions(cr, Poppler.PrintFlags.Document);

        }

        /// <summary>
        /// OnEndPrint - Executed at the end of the Print Operation
        /// </summary>
        /// <param name="obj">The Print Operation</param>
        /// <param name="args">The EndPrintArgs passed by the Print Operation</param>
        private void OnEndPrint(object obj, Gtk.EndPrintArgs args)
        {
        }



        protected void OnSaveButtonClicked(object sender, System.EventArgs e)
        {
            object[] param = new object[4];
            param[0] = "Cancel";
            param[1] = Gtk.ResponseType.Cancel;
            param[2] = "Save";
            param[3] = Gtk.ResponseType.Accept;

            using Gtk.FileChooserDialog fc =
             new Gtk.FileChooserDialog("Save File As",
                                         null,
                                         Gtk.FileChooserAction.Save,
                                         param);


            if (fc.Run() == (int)Gtk.ResponseType.Accept)
            {
                try
                {
                    Uri file = new Uri(fc.Filename);
                    pdf.Save(file.AbsoluteUri);
                }
                catch (Exception ex)
                {
                    using Gtk.MessageDialog m = new Gtk.MessageDialog(null, Gtk.DialogFlags.Modal, Gtk.MessageType.Info,
                        Gtk.ButtonsType.Ok, false,
                        "Error Saving Copy of PDF." + System.Environment.NewLine + ex.Message);

                    m.Run();
                    m.Destroy();
                }
            }
            //Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
            fc.Destroy();
        }

        private bool _ignorePageChange = false;
        void HandleScrollChanged(object sender, EventArgs e)
        {
            // vertical value changed
            int currentVAdjustment = (int)scrolledwindow1.Vadjustment.Value;
            int currentPage = (int)CurrentPage.Value;

            int truePageHeight = vboxImages.Spacing + pageHeight;

            int scrollPage = currentVAdjustment / truePageHeight;
            if (scrollPage != currentPage)
            {
                _ignorePageChange = true;
                CurrentPage.Value = scrollPage;
                _ignorePageChange = false;
            }
        }

        private void ZoomIn()
        {
            scale += 0.5f;
            RenderPages();
        }

        private void ZoomOut()
        {
            scale -= 0.5f;
            RenderPages();
        }
    }
}

