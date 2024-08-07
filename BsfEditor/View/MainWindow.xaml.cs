﻿using System;
using System.Diagnostics;
using System.Windows.Data;
using BsfEditor.Model;

namespace BsfEditor.View
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Fields
        private string _searchText;
        #endregion

        #region Properties and Indexers
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                Debug.WriteLine(value);
                (TryFindResource("EntriesWithSearch") as CollectionViewSource)?.View.Refresh();
            }
        }
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Private members
        private void EntriesWithSearch_OnFilter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                e.Accepted = true;
                return;
            }
            if (e.Item is Entry entry)
            {
                e.Accepted = entry.Key.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase) ||
                             entry.Value.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase);
            }
        }
        #endregion
    }
}
