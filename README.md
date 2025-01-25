# GTK Sharp PDF Widget

This is a Pdfium-based GTK PDF viewer. It demonstrates using C# with .NET 8 to load a PDF into a GTK widget.

## Features

- Load and display PDF files in a GTK widget.
- Built with C# and .NET 8.
- Utilizes Pdfium for PDF rendering.

## Usuage

```bash
dotnet add package Majorsilence.PdfWidget.GtkPdf --version 2.0.2
```

See the PdfWidget.sln for a working example.

### Minimal example

Add the pdf widget to another.   It is empty, the user can use the open button to select a pdf.

```cs
var window = // ... GtkWindow, HBox, VBox, whatever gtk widet 

var pdfWidget = new Majorsilence.PdfWidget.GtkPdf.PdfWidget();
window.Add(pdfWidget);
window.ShowAll();
```


### larger example, preload pdf into widget without user interaction

Add the pdf widget to another.  Load a pdf without user interaction.  The other can load a different pdf with the open button.

```cs
var window = // ... GtkWindow, HBox, VBox, whatever gtk widet 

var pdfWidget = new Majorsilence.PdfWidget.GtkPdf.PdfWidget();
window.Add(pdfWidget);
window.ShowAll();
await pdfWidget.LoadPdfAsync("https://www.gnu.org/licenses/quick-guide-gplv3.pdf");
```


### Development

### Requirements

- .NET 8
- GTK 3

### NuGet Packages

This project references the following NuGet packages:

- [GtkSharp](https://www.nuget.org/packages/GtkSharp)
    - GTK 3
- [DtronixPdf](https://www.nuget.org/packages/DtronixPdf/)
    - Pdfium support

### Getting Started

1. Clone the repository:
    ```sh
    git clone https://github.com/majorsilence/gtk-sharp-pdf-widget.git
    ```
2. Navigate to the project directory:
    ```sh
    cd gtk-sharp-pdf-widget
    ```
3. Restore the NuGet packages:
    ```sh
    dotnet restore
    ```
4. Build the project:
    ```sh
    dotnet build
    ```
5. Run the application:
    ```sh
    dotnet run
    ```

## License

Dual license.

This project has no defined license other than:  You can use this in any software with any license you want. See the [LICENSE](LICENSE) file for details.

If you require a license; MIT License.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## Acknowledgements

- [GtkSharp](https://www.nuget.org/packages/GtkSharp)
- [DtronixPdf](https://www.nuget.org/packages/DtronixPdf/)