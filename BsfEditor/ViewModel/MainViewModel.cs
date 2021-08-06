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

        public RelayCommand SelectFileCommand { get; }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructors
        public MainViewModel()
        {
            SelectFileCommand = new RelayCommand(SelectFile);
            OpenFileCommand = new RelayCommand(OpenFile, () => !string.IsNullOrWhiteSpace(_selectedFilePath));
            SaveFileCommand = new RelayCommand(SaveFile, () => !string.IsNullOrWhiteSpace(_selectedFilePath) && Entries.Count > 0);
        }
        #endregion

        #region Private members
        private void OpenFile()
        {
            try
            {
                var fileInfo = new FileInfo(SelectedFilePath);
                if (!fileInfo.Exists)
                {
                    MessageBox.Show("Your selected file doesn't exist");
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
                bsf.Save(SelectedFilePath);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error while saving file: " + exception);
            }
        }

        private void SelectFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Title = "Select file"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                SelectedFilePath = openFileDialog.FileName;
            }
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
