using ATL;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace mp32desc
{
    public partial class MainWindow : Window
    {
        // Used to store all tracks from currently selected directory (and its subdirectories)
        private List<Track> audioFiles;
        // Used to update text/progressBar in the UI from another thread that loads files from newly selected directory (it can take a while)
        private readonly Progress<Tuple<int, int>> changeDirectoryProgress;

        public MainWindow()
        {
            InitializeComponent();
            changeDirectoryProgress = new(x => { TextBlock_FilesInfo.Text = $"{x.Item1}/{x.Item2}"; ProgressBar.Value = (double)x.Item1 / x.Item2; });
            // Initial state of the app
            Button_Copy.IsEnabled = false;
            Button_Save.IsEnabled = false;
            TextBox_Path.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            TextBox_Template.Text = "{artist} – {year} – {trackNumber} – {title}";
            ProgressBar.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// `Button_RevealInExplorer`: Opens a Windows file explorer with path present in `TextBox_Path` (either initial value or directory that user has selected)
        /// </summary>
        private void OnOpenDirectoryButtonClick(object sender, RoutedEventArgs e)
            => Process.Start("explorer.exe", TextBox_Path.Text);

        /// <summary>
        /// `Button_ChangeDirectory` Shows dialog for selecting new directory, fills `audioFiles` with tracks from this directory (and its subdirectories), fills `TextBlock_FilesInfo` with info about how many files were imported and what extensions are they
        /// </summary>
        private async void OnChangeDirectoryButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new();

            // If user chooses a directory from the dialog
            if (openFolderDialog.ShowDialog() == true) {
                // Importing can take a while (if there is a lot of files), so disable buttons (we don't want user clicking them while import is in progress) and show progress bar
                Button_ChangeDirectory.IsEnabled = false;
                Button_RevealInExplorer.IsEnabled = false;
                Button_Generate.IsEnabled = false;
                Button_Copy.IsEnabled = false;
                Button_Save.IsEnabled = false;
                TextBox_Result.Text = ""; // May contain old "data"; if user wants to choose new directory that means they probably don't need this anymore
                ProgressBar.Visibility = Visibility.Visible;

                // Use directory path from the dialog: set `TextBox_Path` to it (so user can see what directory is selected, `TextBox_Path` is also used by `OnOpenDirectoryButtonClick`)
                string selectedFolderName = openFolderDialog.FolderName;
                TextBox_Path.Text = selectedFolderName;
                // Values for `audioFiles` and `TextBlock_FilesInfo` are calculated asynchronously so the UI does not completely freeze and we can update progress bar
                var crate = await Task.Run(() => Func.Folder2AudioFilesCollection(selectedFolderName, changeDirectoryProgress));
                audioFiles = crate.Collection;
                TextBlock_FilesInfo.Text = crate.Text;
            }

            // Enable previsouly disabled buttons (so user can use them) except `Button_Copy` and `Button_Save`: those're enabled after generating first description (no need to copy/save empty string (`TextBox_Result`)); Hide progress bar
            Button_ChangeDirectory.IsEnabled = true;
            Button_RevealInExplorer.IsEnabled = true;
            Button_Generate.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// `Button_Generate`: Fills `TextBox_Result` with description for files in `audioFiles` that is determined by user "pattern" (internally called userTemplate) in `TextBox_Template`
        /// </summary>
        private void OnGenerateButtonClick(object sender, RoutedEventArgs e)
        {
            // No need to run through empty collection (happens when directory has nop supported files in it)
            if (audioFiles is null || audioFiles.Count == 0) {
                TextBox_Result.Text = "Choose a directory with some audio files first.";
                return;
            }
            TextBox_Result.Text = Func.TrackCollectionAndStringFormatTemplate2String(
                audioFiles,
                TextBox_Template.Text,
                (bool)CheckBox_Subdirectories.IsChecked,
                (bool)CheckBox_Timestamps.IsChecked
            );
            // Now that `TextBox_Result` user can use copy/save functionality
            Button_Copy.IsEnabled = true;
            Button_Save.IsEnabled = true;
        }

        /// <summary>
        /// `Button_Copy`: Inserts content `TextBox_Result` into user's clipboard
        /// </summary>
        private void OnCopyButtonClick(object sender, RoutedEventArgs e)
            => Clipboard.SetText(TextBox_Result.Text);

        /// <summary>
        /// `Button_Save`: Opens a dialog where user can save `TextBox_Result` content into a (.txt) file
        /// </summary>
        private void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new() {
                DefaultExt = ".txt",
                InitialDirectory = TextBox_Path.Text,
                FileName = "description.txt",
                Filter = "All(*.*)|*",
            };
            if (dialog.ShowDialog() == true) {
                File.WriteAllText(dialog.FileName, TextBox_Result.Text);
            }
        }

        /// <summary>
        /// Button "?": Shows button's context menu
        /// </summary>
        private void OnHelperButtonClick(object sender, RoutedEventArgs e)
            => ContextMenu_Helper.IsOpen = true;

        /// <summary>
        /// `ContextMenu_Helper`: Pastes selected userTemplate field into `TextBox_Template` (with curly braces)
        /// </summary>
        private void OnContextMenuItemClick(object sender, RoutedEventArgs e)
        {
            var menuItemSender = sender as MenuItem;
            TextBox_Template.Text += $"{{{menuItemSender.Header}}}";
        }
    }
}
