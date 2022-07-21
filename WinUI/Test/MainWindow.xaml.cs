using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using FooEditEngine.WinUI;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.Activation;
using System.Linq;
using Windows.Storage.AccessCache;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Test
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        MainViewModel vm = new MainViewModel();
        PrintManager printManager;
        public MainWindow()
        {
            this.InitializeComponent();
            this.Root.DataContext = this.vm;
            this.Closed += MainWindow_Closed;
        }

        public async void OnInit(string[] files)
        {
            if (files != null)
            {
                await this.LoadManyFiles(files);
            }
            else if (this.vm.DocumentList.Count == 0)
            {
                this.vm.Initalize();
            }
            printManager = PrintManagerInterop.GetForWindow(WindowNative.GetWindowHandle(this));
            printManager.PrintTaskRequested += MainPage_PrintTaskRequested;
        }

        public async Task LoadManyFiles(string[] files)
        {
            //MRUに追加しないと後で開けない
            foreach (var file_path in files)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(file_path);
                StorageApplicationPermissions.MostRecentlyUsedList.Add(file, "mrufile");
                await this.vm.AddDocumentFromFile(file);
            }

        }
        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            printManager.PrintTaskRequested -= MainPage_PrintTaskRequested;
        }

        PrintTaskRequestedDeferral PrintTaskRequestedDeferral;
        void MainPage_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => {
                var doc = this.vm.CurrentDocument;
                var source = new DocumentSource(doc, new FooEditEngine.Padding(20, 20, 20, 20), "Caribiri", 16);
                source.ParseHF = (s, e) => { return e.Original; };
                source.Header = "header";
                source.Fotter = "footer";
                source.Forground = Microsoft.UI.Colors.Black;
                source.Keyword1 = Microsoft.UI.Colors.Blue;
                source.Keyword2 = Microsoft.UI.Colors.AliceBlue;
                source.Literal = Microsoft.UI.Colors.Brown;
                source.Comment = Microsoft.UI.Colors.Green;
                source.Url = Microsoft.UI.Colors.Blue;
                source.LineBreak = doc.LineBreak;
                source.LineBreakCount = doc.LineBreakCharCount;

                PrintTask task = null;
                task = args.Request.CreatePrintTask("test", (e) =>
                {
                    e.SetSource(source.PrintDocument);
                });
                PrintOptionBuilder<DocumentSource> builder = new PrintOptionBuilder<DocumentSource>(source);
                builder.BuildPrintOption(PrintTaskOptionDetails.GetFromPrintTaskOptions(task.Options));
                PrintTaskRequestedDeferral.Complete();
            });
            PrintTaskRequestedDeferral = args.Request.GetDeferral();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var filepicker = new FileOpenPicker();
            InitializeWithWindow.Initialize(filepicker, WindowNative.GetWindowHandle(this));
            filepicker.FileTypeFilter.Add(".txt");
            var file = await filepicker.PickSingleFileAsync();
            await this.vm.AddDocumentFromFile(file);
        }

        private async void Print_Button_Click(object sender, RoutedEventArgs e)
        {
            await PrintManagerInterop.ShowPrintUIForWindowAsync(WindowNative.GetWindowHandle(this));
        }

        private void Button_Click_New(object sender, RoutedEventArgs e)
        {
            this.vm.AddDocument();
        }
    }
}
