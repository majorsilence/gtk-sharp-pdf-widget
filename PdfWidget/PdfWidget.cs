using System;

// The Gtk developers wanted icons not to appear on buttons. 
// On Linux, this can be changed by editing the gconf key
// /desktop/gnome/interface/buttons_have_icons

// type page number /OfPageCount, 

namespace PdfWidget
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PdfWidget : Gtk.Bin
	{

		private Poppler.Document pdf;
		private int pageIndex = 0;
		private int pageHeight = 0;
		
		public PdfWidget ()
		{
			this.Build ();
		}
				
		private void RenderPage (ref Gtk.Image img) 
		{
	        
			Poppler.Page page = this.pdf.GetPage(this.pageIndex);
			double width=0D;
			double height=0D;
			page.GetSize(out width, out height);
			pageHeight = (int)height;
			
			// It is important to set the image to have the correct size
			img.Pixbuf  = new  Gdk.Pixbuf (Gdk.Colorspace.Rgb, false, 8, (int)width, (int)height);
			Gdk.Pixbuf pixbuf = img.Pixbuf;
			
	        page.RenderToPixbuf(0, 0, (int)width, (int)height, 1.0, 0, pixbuf);
	        img.Pixbuf = pixbuf;
			vboxImages.Add (img);		
    	}
		
		public void LoadPdf(string pdfFileName)
		{
			pdf = Poppler.Document.NewFromFile(pdfFileName, "");
			PageCountLabel.Text = @"/" + pdf.NPages.ToString();	
			CurrentPage.Value = 0;
			CurrentPage.Adjustment.Upper = pdf.NPages;
		
			foreach (Gtk.Widget w in vboxImages.AllChildren)
			{
				vboxImages.Remove(w);
			}
				
			for (this.pageIndex = 0; this.pageIndex < pdf.NPages; this.pageIndex++)
			{
				Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, false, 8, 0, 0);
				Gtk.Image img = new Gtk.Image();
				img.Pixbuf = pixbuf;		
				img.Name = "image1";
								
				//vboxImages.Add (img);
				RenderPage(ref img);
			}
			
			this.ShowAll();
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
		protected void OnNextButtonClicked (object sender, System.EventArgs e)
		{
			
			scrolledwindow1.Hadjustment.Value = scrolledwindow1.Hadjustment.Lower;
			scrolledwindow1.Vadjustment.Value = scrolledwindow1.Vadjustment.Value + pageHeight;
			
			if (pdf.NPages > (int)CurrentPage.Value)
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
		protected void OnPreviousButtonClicked (object sender, System.EventArgs e)
		{
			scrolledwindow1.Hadjustment.Value = scrolledwindow1.Hadjustment.Lower;
			scrolledwindow1.Vadjustment.Value = scrolledwindow1.Vadjustment.Value - pageHeight;	
		
			if ((int)CurrentPage.Value > 0)
			{
				CurrentPage.Value = CurrentPage.Value - 1;
			}
		}

		protected void OnFirstPageButtonClicked (object sender, System.EventArgs e)
		{
			scrolledwindow1.Hadjustment.Value = scrolledwindow1.Hadjustment.Lower;
			scrolledwindow1.Vadjustment.Value = scrolledwindow1.Vadjustment.Lower;	
			CurrentPage.Value = 0;
		}

		protected void OnLastPageButtonClicked (object sender, System.EventArgs e)
		{
			scrolledwindow1.Hadjustment.Value = scrolledwindow1.Hadjustment.Lower;
			scrolledwindow1.Vadjustment.Value = scrolledwindow1.Vadjustment.Upper-pageHeight;	
			CurrentPage.Value = pdf.NPages;
		}
	}
}

