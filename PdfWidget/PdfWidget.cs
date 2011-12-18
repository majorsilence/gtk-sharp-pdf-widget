using System;


namespace PdfWidget
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PdfWidget : Gtk.Bin
	{

		private Poppler.Document pdf;
		private int pageIndex = 0;
					
		public PdfWidget ()
		{
			this.Build ();
		}
				
		private void RenderPage (ref Gtk.Image img) 
		{
	        Gdk.Pixbuf pixbuf = img.Pixbuf;
			Poppler.Page page = this.pdf.GetPage(this.pageIndex);
			double width=0D;
			double height=0D;
			page.GetSize(out width, out height);
			
	        page.RenderToPixbuf(0, 0, (int)width, (int)height, 1.0, 0, pixbuf);
	        img.Pixbuf = pixbuf;
						
    	}
		
		public void LoadPdf(string pdfFileName)
		{
			pdf = Poppler.Document.NewFromFile(pdfFileName, "");	
			SetContinuousPageMode();
		}
		
		private void SetSinglePageMode()
		{
			foreach (Gtk.Widget w in vboxImages)
			{
				vboxImages.Remove(w);
			}
			
					
			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, false, 8, 800, 600);
		
			this.image1.Pixbuf = pixbuf;
			vboxImages.Add (image1);
			
			RenderPage(ref this.image1);
			
		}
		
		private void SetContinuousPageMode()
		{
			foreach (Gtk.Widget w in vboxImages.AllChildren)
			{
				vboxImages.Remove(w);
			}
				
			for (this.pageIndex = 0; this.pageIndex < pdf.NPages; this.pageIndex++)
			{
				Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, false, 8, 800, 600);
				Gtk.Image image1 = new Gtk.Image();
				image1.Pixbuf = pixbuf;		
				image1.Name = "image1";
				
				vboxImages.Add (image1);
				RenderPage(ref image1);
			}
			
			
		}
		
		protected void OnContinuousCheckBoxClicked (object sender, System.EventArgs e)
		{
			if (ContinuousCheckBox.Active)
			{
			}
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
			this.pageIndex ++;
			if (pdf.NPages < this.pageIndex)
			{
				this.pageIndex = pdf.NPages;
			}
			RenderPage(ref this.image1);
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
			this.pageIndex --;
			if (this.pageIndex <0)
			{
				this.pageIndex = 0;
			}
			
			RenderPage(ref this.image1);
		}

	}
}

