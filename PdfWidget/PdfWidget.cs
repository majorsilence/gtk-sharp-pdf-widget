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
		
		public void LoadPdf(string pdfFileName)
		{
			pdf = Poppler.Document.NewFromFile(pdfFileName, "");			
			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, false, 8, 800, 600);
		
			this.image1.Pixbuf = pixbuf;
			RenderPage();
		}
		
		private void RenderPage () 
		{
	        Gdk.Pixbuf pixbuf = this.image1.Pixbuf;
			Poppler.Page page = this.pdf.GetPage(this.pageIndex);
	        page.RenderToPixbuf(0, 0, 800, 600, 1.0, 0, pixbuf);
	        this.image1.Pixbuf = pixbuf;
    	}
		
		protected void OnNextButtonClicked (object sender, System.EventArgs e)
		{
			this.pageIndex ++;
			if (pdf.NPages < this.pageIndex)
			{
				this.pageIndex = pdf.NPages;
			}
			RenderPage();
		}

		protected void OnPreviousButtonClicked (object sender, System.EventArgs e)
		{
			this.pageIndex --;
			if (this.pageIndex <0)
			{
				this.pageIndex = 0;
			}
			
			RenderPage();
		}
	}
}

