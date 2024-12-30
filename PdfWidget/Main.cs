using System;
using Gtk;
using PDFiumCore;

namespace PdfWidget
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            fpdfview.FPDF_InitLibrary();
            Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}
