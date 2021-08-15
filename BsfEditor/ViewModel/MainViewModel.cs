using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using BsfEditor.Model;
using Microsoft.Win32;
using ToxicRagers.Helpers;
using ToxicRagers.Stainless.Formats;

namespace BsfEditor.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields
        private string _selectedFilePath;
        private int _selectedIndex;
        #endregion

        #region Properties and Indexers
        public ObservableCollection<Entry> Entries { get; } = new ObservableCollection<Entry>();

        public bool LoggingEnabled
        {
            get => Logger.Level > 0;
            set
            {
                Logger.Level = value ? Logger.LogLevel.All : 0;
                OnPropertyChanged();
            }
        }

        public RelayCommand<int> MoveSelectionCommand { get; }
        public RelayCommand OpenFileCommand { get; }
        public RelayCommand SaveFileCommand { get; }

        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                _selectedFilePath = value;
                OnPropertyChanged();
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructors
        public MainViewModel()
        {
            OpenFileCommand = new RelayCommand(OpenFile);
            SaveFileCommand = new RelayCommand(SaveFile, () => Entries.Count > 0);
            MoveSelectionCommand = new RelayCommand<int>(MoveSelection, arg => SelectedIndex > -1 &&
                                                                               SelectedIndex < Entries.Count &&
                                                                               SelectedIndex + arg >= 0 &&
                                                                               SelectedIndex + arg < Entries.Count);
        }
        #endregion

        #region Private members
        private void MoveSelection(int arg)
        {
            var selectedItem = Entries[SelectedIndex];
            var newIndex = SelectedIndex + arg;
            Entries.RemoveAt(SelectedIndex);
            Entries.Insert(newIndex, selectedItem);
            SelectedIndex = newIndex;
        }

        private void OpenFile()
        {
            try
            {
                if (!ShowFileDialog(false, out var path)) return;
                var fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                {
                    MessageBox.Show("Your selected file doesn't exist (how?)");
                    return;
                }

                var result = BSF.Load(fileInfo.FullName);
                if (result == null)
                {
                    MessageBox.Show("Failed to read file, probably not BSF");
                    return;
                }
                Entries.Clear();
                foreach (var kvp in result)
                {
                    Entries.Add(new Entry(kvp.Key, kvp.Value));
                }
                Logger.LogToFile(Logger.LogLevel.Info, $"There are {Entries.Count} entries");
                SelectedFilePath = path;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error while opening file: " + exception);
            }
        }

        private void SaveFile()
        {
            try
            {
                if (!ShowFileDialog(true, out var savePath, SelectedFilePath)) return;
                var bsf = new BSF();
                foreach (var entry in Entries)
                {
                    if (entry.Key.Length > byte.MaxValue)
                    {
                        Logger.LogToFile(Logger.LogLevel.Info, $"Skipping entry {entry.Key}, key length too long >{byte.MaxValue}");
                        continue;
                    }
                    if (entry.Value.Length > short.MaxValue)
                    {
                        Logger.LogToFile(Logger.LogLevel.Info, $"Skipping entry {entry.Key}, value length too long >{short.MaxValue}");
                        continue;
                    }
                    bsf.Add(entry.Key, entry.Value);
                }
                bsf.Save(savePath);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error while saving file: " + exception);
            }
        }

        private static bool ShowFileDialog(bool isSaveDialog, out string path, string initialPath = null)
        {
            FileDialog fileDialog;
            const string stringFormat = "Binary String Format|*.bsf";
            if (isSaveDialog)
            {
                fileDialog = new SaveFileDialog();
            }
            else
            {
                fileDialog = new OpenFileDialog
                {
                    CheckFileExists = true,
                };
            }
            fileDialog.Title = "Select file";
            fileDialog.Filter = stringFormat;
            fileDialog.CheckPathExists = true;
            fileDialog.InitialDirectory = initialPath ?? string.Empty;

            var dialogResult = fileDialog.ShowDialog();
            path = fileDialog.FileName;
            return dialogResult == true;
        }
        #endregion

        #region Protected members
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
