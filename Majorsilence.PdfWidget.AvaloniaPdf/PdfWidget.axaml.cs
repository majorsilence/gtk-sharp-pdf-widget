using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DtronixPdf;

namespace Majorsilence.PdfWidget.AvaloniaPdf
{
    public partial class PdfWidget : UserControl, IDisposable
    {
        // DtronixPdf document
        private PdfDocument? pdf;
        private int pageCount = 0;
        private int currentPage = 0; // zero-based
        private double zoom = 1.0; // 1.0 == 100%

        private CancellationTokenSource? renderCts;

        // Simple in-memory cache: key = pageIndex + "@" + zoomKey
        private readonly ConcurrentDictionary<string, WriteableBitmap> bitmapCache = new();

        public PdfWidget()
        {
            InitializeComponent();

            OpenButton.Click += OpenButton_Click;
            ZoomInButton.Click += ZoomInButton_Click;
            ZoomOutButton.Click += ZoomOutButton_Click;
            PreviousButton.Click += PreviousButton_Click;
            NextButton.Click += NextButton_Click;

            // initialize UI
            PageInfo.Text = string.Empty;
        }

        private async void OpenButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filters.Add(new FileDialogFilter { Name = "PDF files", Extensions = { "pdf" } });
            dlg.AllowMultiple = false;
            var result = await dlg.ShowAsync(GetTopLevel());
            if (result != null && result.Length > 0)
            {
                await OpenAsync(result[0]);
            }
        }

        private async Task OpenAsync(string path)
        {
            CancelRendering();
            ClearUiPages();

            // Support http(s) by downloading to temp file
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // quick download
                var httpClient = new System.Net.Http.HttpClient();
                var data = await httpClient.GetByteArrayAsync(path);
                var tmp = Path.GetTempFileName();
                await File.WriteAllBytesAsync(tmp, data);
                path = tmp;
            }

            pdf = PdfDocument.Load(path, null);
            pageCount = pdf.Pages;
            currentPage = 0;
            UpdatePageInfo();

            // populate placeholders for pages
            for (int i = 0; i < pageCount; i++)
            {
                var img = new Image();
                img.Width = 100; // placeholder until rendered
                img.Height = 100;
                img.Tag = i;
                PagesPanel.Children.Add(img);
            }

            // start rendering visible pages (simple: render all pages in background but sequentially)
            renderCts = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                // first low-res quick pass, then high-res
                for (int pass = 0; pass < 2; pass++)
                {
                    if (renderCts.IsCancellationRequested) break;
                    for (int i = 0; i < pageCount; i++)
                    {
                        if (renderCts.IsCancellationRequested) break;
                        double targetZoom = (pass == 0) ? Math.Min(zoom, 0.25) : zoom;
                        try
                        {
                            var bmp = await RenderPageAsync(i, targetZoom, renderCts.Token).ConfigureAwait(false);
                            if (bmp != null)
                            {
                                // apply to UI
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    if (i < PagesPanel.Children.Count)
                                    {
                                        var img = PagesPanel.Children[i] as Image;
                                        if (img != null)
                                        {
                                            img.Source = bmp;
                                            img.Width = bmp.PixelSize.Width / bmp.Dpi.X * 96; // approximate device units
                                            img.Height = bmp.PixelSize.Height / bmp.Dpi.Y * 96;
                                        }
                                    }
                                });
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // ignore
                            break;
                        }
                        catch (Exception ex)
                        {
                            // avoid crashing UI thread; report on UI
                            await Dispatcher.UIThread.InvokeAsync(() => PageInfo.Text = "Render error: " + ex.Message);
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        private void ClearUiPages()
        {
            PagesPanel.Children.Clear();
            bitmapCache.Clear();
            pdf = null;
            pageCount = 0;
            currentPage = 0;
            PageInfo.Text = string.Empty;
        }

        private void CancelRendering()
        {
            try
            {
                renderCts?.Cancel();
                renderCts?.Dispose();
            }
            catch { }
            renderCts = null;
        }

        private async Task<WriteableBitmap?> RenderPageAsync(int pageIndex, double targetZoom, CancellationToken ct)
        {
            if (pdf == null) return null;
            string key = pageIndex + "@" + (int)(targetZoom * 1000);
            if (bitmapCache.TryGetValue(key, out var existing) && existing != null)
                return existing;

            return await Task.Run(async () =>
            {
                ct.ThrowIfCancellationRequested();

                var page = pdf.GetPage(pageIndex);
                int width = (int)Math.Max(1, Math.Round(page.Width * targetZoom));
                int height = (int)Math.Max(1, Math.Round(page.Height * targetZoom));

                var config = new PdfPageRenderConfig()
                {
                    Scale = (float)targetZoom,
                    Viewport = Vector128.Create(0f, 0f, (float)width, (float)height)
                };

                var bitmap = page.Render(config);
                long length = width * (long)height * 4;
                int stride = width * 4;
                byte[] bitmapData = new byte[length];
                Marshal.Copy(new IntPtr(bitmap.Pointer), bitmapData, 0, (int)length);
                
                // create WriteableBitmap on UI thread because framebuffer locks may require UI thread
                WriteableBitmap? wb = null;
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var pixelSize = new PixelSize(width, height);
                    var dpi = new Vector(96, 96);
                    wb = new WriteableBitmap(pixelSize, dpi, PixelFormat.Bgra8888, AlphaFormat.Unpremul);
                    using (var fb = wb.Lock())
                    {
                        Marshal.Copy(bitmapData, 0, fb.Address, bitmapData.Length);
                    }

                    // add to cache
                    bitmapCache[key] = wb!;
                });

                return wb;
            }, ct).ConfigureAwait(false);
        }

        private async void ZoomInButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            zoom *= 1.25;
            await ReRenderAllAsync();
        }

        private async void ZoomOutButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            zoom /= 1.25;
            if (zoom < 0.1) zoom = 0.1;
            await ReRenderAllAsync();
        }

        private async Task ReRenderAllAsync()
        {
            if (pdf == null) return;
            CancelRendering();
            // clear cached zoomed images (simple: clear all cache)
            bitmapCache.Clear();

            renderCts = new CancellationTokenSource();
            var ct = renderCts.Token;

            await Task.Run(async () =>
            {
                for (int i = 0; i < pageCount; i++)
                {
                    if (ct.IsCancellationRequested) break;
                    try
                    {
                        var bmp = await RenderPageAsync(i, zoom, ct).ConfigureAwait(false);
                        if (bmp != null)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                if (i < PagesPanel.Children.Count)
                                {
                                    var img = PagesPanel.Children[i] as Image;
                                    if (img != null)
                                    {
                                        img.Source = bmp;
                                        img.Width = bmp.PixelSize.Width / bmp.Dpi.X * 96;
                                        img.Height = bmp.PixelSize.Height / bmp.Dpi.Y * 96;
                                    }
                                }
                            });
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() => PageInfo.Text = "Render error: " + ex.Message);
                    }
                }
            });
        }

        private void PreviousButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (pageCount == 0) return;
            currentPage = Math.Max(0, currentPage - 1);
            UpdatePageInfo();
            BringPageIntoView(currentPage);
        }

        private void NextButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (pageCount == 0) return;
            currentPage = Math.Min(pageCount - 1, currentPage + 1);
            UpdatePageInfo();
            BringPageIntoView(currentPage);
        }

        private void UpdatePageInfo()
        {
            PageInfo.Text = $"Page {currentPage + 1} / {Math.Max(1, pageCount)}  (Zoom {(int)(zoom * 100)}%)";
        }

        private void BringPageIntoView(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= PagesPanel.Children.Count) return;
            var control = PagesPanel.Children[pageIndex] as Control;
            control?.BringIntoView();
        }

        private Window? GetTopLevel()
        {
            // attempt to find a top-level Window to show dialogs
            return this.GetVisualRoot() as Window;
        }

        public void Dispose()
        {
            CancelRendering();
            bitmapCache.Clear();
            pdf = null;
        }
    }
}
