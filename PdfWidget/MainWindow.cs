using System;
using Gtk;

public class MainWindow: Gtk.Window
{	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		
		string filepath = @"C:\Users\peter\source\stm32\ds10314-arm-cortex-m4-32b-mcufpu-125-dmips-512kb-flash-128kb-ram-usb-otg-fs-11-tims-1-adc-13-comm-interfaces-stmicroelectronics-en.pdf";
		var w = new Majorsilence.PdfWidget.GtkPdf.PdfWidget();
		w.LoadPdf(filepath);
		this.Add(w);
		this.ShowAll();
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;	
	}

    protected virtual void Build()
    {
        // Widget MainWindow
        this.Name = "MainWindow";
        this.Title = "Main Window";
        this.WindowPosition = ((global::Gtk.WindowPosition)(4));
        if ((this.Child != null))
        {
            this.Child.ShowAll();
        }
        this.DefaultWidth = 464;
        this.DefaultHeight = 331;
        this.Show();
        this.DeleteEvent += new global::Gtk.DeleteEventHandler(this.OnDeleteEvent);
    }
}
