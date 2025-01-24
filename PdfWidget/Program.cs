using System;
using Gtk;

namespace PdfWidget
{
	class Program
	{
		public static void Main (string[] args)
		{
            Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
            Application.Run ();
		}
	}
}
