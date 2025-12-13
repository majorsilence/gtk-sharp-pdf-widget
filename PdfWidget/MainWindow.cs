using System;
using Gtk;

public class MainWindow : Gtk.Window
{
    private readonly Majorsilence.PdfWidget.GtkPdf.PdfWidget w;
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        w = new Majorsilence.PdfWidget.GtkPdf.PdfWidget();
        Build();     
        this.Add(w);
        this.ShowAll();

    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        w.Dispose();
        Application.Quit();
        a.RetVal = true;
    }

    protected virtual void Build()
    {
        this.Shown += MainWindow_Shown;
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

    private async void MainWindow_Shown(object sender, EventArgs e)
    {
        // Example directly loading a pdf, or you could let the PdfWidget open button handle it

    }
}
