using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using BsfEditor.Model;
using Microsoft.Win32;
using Newtonsoft.Json;
using ToxicRagers.Helpers;
using ToxicRagers.Stainless.Formats;

namespace BsfEditor.ViewModel
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        #region Fields
        private string _selectedFilePath;
        private int _selectedIndex;
        #endregion

        #region Properties and Indexers
        public ObservableCollection<Entry> Entries { get; } = [];
        public RelayCommand ImportFileCommand { get; }

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
            private set
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
            ImportFileCommand = new RelayCommand(ImportFile);
            MoveSelectionCommand = new RelayCommand<int>(MoveSelection, arg => SelectedIndex > -1 &&
                                                                               SelectedIndex < Entries.Count &&
                                                                               SelectedIndex + arg >= 0 &&
                                                                               SelectedIndex + arg < Entries.Count);
        }
        #endregion

        #region Private members
        private BSF CreateBsf()
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
            return bsf;
        }

        private static bool EqualsBsf(string extension)
        {
            return extension.Equals(".bsf", StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool EqualsJson(string extension)
        {
            return extension.Equals(".json", StringComparison.InvariantCultureIgnoreCase);
        }

        private void ImportFile()
        {
            if (!ShowFileDialog(false, out var path)) return;
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                MessageBox.Show("Your selected file doesn't exist (how?)");
                return;
            }
            if (!ReadIntoBsf(fileInfo, out var result)) return;
            foreach (var kvp in result)
            {
                Entries.Add(new Entry(kvp.Key, kvp.Value));
            }
        }

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

                if (!ReadIntoBsf(fileInfo, out var result)) return;
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

        private static bool ReadIntoBsf(FileInfo fileInfo, out BSF result)
        {
            result = null;
            if (EqualsBsf(fileInfo.Extension))
            {
                result = BSF.Load(fileInfo.FullName);
                if (result != null) return true;
                MessageBox.Show("Failed to read file, probably not BSF");
            }
            else if (EqualsJson(fileInfo.Extension))
            {
                using var streamReader = File.OpenText(fileInfo.FullName);
                var serializer = new JsonSerializer();
                result = serializer.Deserialize(streamReader, typeof(BSF)) as BSF;
                return result != null;
            }
            return false;
        }

        private void SaveFile()
        {
            try
            {
                var duplicateKeys = Entries.GroupBy(e => e.Key).Where(g => g.Skip(1).Any()).Select(g => g.Key).ToList();
                if (duplicateKeys.Count != 0)
                {
                    MessageBox.Show($"You have duplicate key values:\n{string.Join("\n", duplicateKeys)}");
                    return;
                }
                if (!ShowFileDialog(true, out var savePath, SelectedFilePath)) return;
                var bsf = CreateBsf();
                SaveToFile(savePath, bsf);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error while saving file: " + exception);
            }
        }

        private static void SaveToFile(string savePath, BSF bsf)
        {
            var extension = Path.GetExtension(savePath);
            if (EqualsBsf(extension))
            {
                bsf.Save(savePath);
            }
            else if (EqualsJson(extension))
            {
                using var streamWriter = new StreamWriter(savePath);
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(streamWriter, bsf);
            }
            else
            {
                MessageBox.Show($"Unknown extension, can't save: {extension}");
            }
        }

        private static bool ShowFileDialog(bool isSaveDialog, out string path, string initialPath = null)
        {
            FileDialog fileDialog;
            const string stringFormat = "All supported formats|*.bsf;*.json" +
                                        "|Binary String Format|*.bsf" +
                                        "|Java Script Object Notation|*.json";
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
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
