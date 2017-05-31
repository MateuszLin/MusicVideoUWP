using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MyPlayer
{

 
    public sealed partial class playersLists : Page
    {
       
        public playersLists()
        {
            this.InitializeComponent();
            getAllMusic();
            getlistsMusicsCB();
        }

     
        /// <summary>
        /// Pobranie całej muzyki z bazy
        /// </summary>
        private void getAllMusic()
        {
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "lists.db");
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLitePlatformWinRT(), path))
            {
                var all = conn.Query<listsMusics>("SELECT name from listsMusics where listName = ?", "ALL");
                foreach(var item in all)
                {
                    allMusicLV.Items.Add(item.name);
                }

            }
        }

        /// <summary>
        /// Pobranie stworzonych przez użytkownika list odtwarzań do comboboxa
        /// </summary>
        private void getlistsMusicsCB()
        {
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "lists.db");
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLitePlatformWinRT(), path))
            {
                var query = conn.Query<listsMusics>("SELECT DISTINCT listName from listsMusics");
                foreach (var item in query)
                {
                    if(item.listName != "ALL")
                        listMusicsCB.Items.Add(item.listName);
                }
            }
        }


        private async void allMusicLV_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
                String item = allMusicLV.SelectedItem.ToString();
                String temp = listMusicsCB.SelectedItem.ToString();
                if (item != null && temp != null)
                {
                    addToList(item);
                }
            }catch(NullReferenceException)
            {
                await (new Windows.UI.Popups.MessageDialog("Brak wybranej listy lub nie wybrano utworu").ShowAsync());
            }
        
        }

        private async void addToList(String item)
        {
            int er = 0;
            foreach(String songName in listsMusicLV.Items)
            {
                if (songName == item)
                {
                    await (new Windows.UI.Popups.MessageDialog("Podany utwór jest już na liście").ShowAsync());
                    er = 1;
                }
            }
            if(er == 0)
            listsMusicLV.Items.Add(item);
        }


        private void listsMusicLV_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            String item = listsMusicLV.SelectedItem.ToString();
            if(item != null)
                removeFromlist(item);
        }

        private void removeFromlist(String item)
        {
                    listsMusicLV.Items.Remove(item);
        }

        private void listMusicsCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                changeView(listMusicsCB.SelectedItem.ToString());
                deleteBtn.IsEnabled = true;
            }catch(Exception )
            { }
            
        }

        /// <summary>
        /// zmiana listy na inną wybraną z comboboxa
        /// </summary>
        /// <param name="listName"></param>
        private void changeView(String listName)
        {
            listsMusicLV.Items.Clear();
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "lists.db");
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLitePlatformWinRT(), path))
            {
                var list = conn.Query<listsMusics>("SELECT name FROM listsMusics WHERE listName = ?", listName);
                foreach(var item in list)
                {
                    listsMusicLV.Items.Add(item.name);
                }
            }

        }

      
        private void textBox1_KeyUp(object sender, KeyRoutedEventArgs e)
       {
            if (e.Key == VirtualKey.Enter)
                addToComboBox();
        }

        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            addToComboBox();
        }

        /// <summary>
        /// dodanie nowej listy do comboboxa
        /// </summary>
        private async void addToComboBox()
        {
            String text = textBox1.Text;
            if(text.Length != 0)
            {
                try
                {
                    foreach (var item in listMusicsCB.Items)
                    {
                        if (item.ToString() == text)
                        {
                            setComboBox(item.ToString());
                            throw new NullReferenceException();
                        }
                    }
                    listMusicsCB.Items.Add(text);
                    await (new Windows.UI.Popups.MessageDialog("Lista została utworzona").ShowAsync());
                    setComboBox(text);
                }catch(NullReferenceException e)
                {
                    await (new Windows.UI.Popups.MessageDialog("Podana nazwa już istnieje").ShowAsync());
                }
            }
            else
            {
                await (new Windows.UI.Popups.MessageDialog("Brak nazwy dla nowej listy").ShowAsync());
            }
        }

        private void setComboBox(String text)
        {
            foreach (String item in listMusicsCB.Items)
            {
                if (item == text)
                    listMusicsCB.SelectedItem = item;
            }
        }

        private async void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (listsMusicLV.Items.Count() == 0)
                await(new Windows.UI.Popups.MessageDialog("Nie mozna zapisac pustej listy!").ShowAsync());
            else
                saveChange();
        }

        /// <summary>
        /// zapisanie listy do bazy
        /// </summary>
        private async void saveChange()
        {

            clearList();
            addNewList();
            await (new Windows.UI.Popups.MessageDialog("Lista zapisana pomyślnie").ShowAsync());
            
        }

        /// <summary>
        /// dodawania piosenek do bazy
        /// </summary>
        private void addNewList()
        {
            String listN = listMusicsCB.SelectedItem.ToString();
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "lists.db");
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLitePlatformWinRT(), path))
            {
                foreach (String songName in listsMusicLV.Items)
                {
                    var list = conn.Query<listsMusics>("SELECT name, path FROM listsMusics WHERE listName = ? AND name = ?", "ALL", songName);
                    foreach (var item in list)
                    {
                        conn.Insert(new listsMusics
                        {
                            name = item.name,
                            path = item.path,
                            listName = listN
                        });
                    }
                }
            }
        }

        /// <summary>
        /// usuniecie listy z bazy
        /// </summary>
        private void clearList()
        {
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "lists.db");
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLitePlatformWinRT(), path))
            {
                conn.Query<listsMusics>("DELETE FROM listsMusics WHERE listName = ?", listMusicsCB.SelectedItem.ToString());
     
            }

        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            deleteListFromDB();
            listMusicsCB.Items.Clear();
            listsMusicLV.Items.Clear();
            getlistsMusicsCB();
        }

        private void deleteListFromDB()
        {
            String listN = listMusicsCB.SelectedItem.ToString();

            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "lists.db");
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLitePlatformWinRT(), path))
            {
                conn.Query<listsMusics>("DELETE FROM listsMusics WHERE listName = ?", listN);
            }
        }
    }
}
