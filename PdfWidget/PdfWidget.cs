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
	        
			Poppler.Page page = this.pdf.GetPage(this.pageIndex);
			double width=0D;
			double height=0D;
			page.GetSize(out width, out height);
			
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
			SetContinuousPageMode();
		}
		
		private void SetSinglePageMode()
		{
			foreach (Gtk.Widget w in vboxImages.AllChildren)
			{
				vboxImages.Remove(w);
			}
							
			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, false, 8, 800, 600);
		
			this.image1.Pixbuf = pixbuf;	
			this.pageIndex = 0;
			
			RenderPage(ref this.image1);
			this.ShowAll();
		}
		
		private void SetContinuousPageMode()
		{
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
		
		protected void OnContinuousCheckBoxClicked (object sender, System.EventArgs e)
		{
			if (ContinuousCheckBox.Active)
			{
				SetContinuousPageMode();
			}
			else
			{
				SetSinglePageMode();
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

