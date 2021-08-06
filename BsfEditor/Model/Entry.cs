using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BsfEditor.Model
{
    public class Entry : INotifyPropertyChanged
    {
        #region Fields
        private string _key;
        private string _value;
        #endregion

        #region Properties and Indexers
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructors
        //needed for DataGrid, proper way to do this would probably be to have an EntryViewModel instead...
        public Entry()
        {
        }

        public Entry(string key, string value)
        {
            _key = key;
            _value = value;
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
