using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public class listsMusics
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string listName { get; set; }
    }
    public sealed partial class musicPlayer : Page
    {
        public StorageFolder musicLib = Windows.Storage.KnownFolders.MusicLibrary;
        public DispatcherTimer timer;
        public List<string> musicName = new List<string>();
        public List<string> musicPath = new List<string>();
        public bool isPause = false;
        int songIndex = -1;
        public SystemMediaTransportControls systemControls;
        SystemMediaTransportControlsDisplayUpdater updater;


        public  musicPlayer()
        {
            this.InitializeComponent();
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "lists.db");
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLitePlatformWinRT(), path))
            {
                try {
                        conn.Query<listsMusics>("DELETE FROM listsMusics WHERE listName = ?", "ALL");
                        sqlGetAllMusic();
                    
                }
                catch (Exception)
                {
                    conn.CreateTable<listsMusics>();
                    sqlGetAllMusic();
                } 
            }

            systemControls = SystemMediaTransportControls.GetForCurrentView();

            systemControls.ButtonPressed += SystemControls_ButtonPressed;
            myMediaElement.CurrentStateChanged += MediaElement_CurrentStateChanged;

            systemControls.IsPlayEnabled = true;
            systemControls.IsPauseEnabled = true;
            systemControls.IsNextEnabled = true;
            systemControls.IsPreviousEnabled = true;

            updater = systemControls.DisplayUpdater;
            updater.Type = MediaPlaybackType.Music;
            updater.Update();
        }

        private void MediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (myMediaElement.CurrentState)
            {
                case MediaElementState.Playing:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaElementState.Paused:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaElementState.Stopped:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
                case MediaElementState.Closed:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                default:
                    break;
            }
        }

        void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {

            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    PlayMedia();
                    break;

                case SystemMediaTransportControlsButton.Pause:
                    PauseMedia();
                    break;

                case SystemMediaTransportControlsButton.Next:
                    PauseMedia();
                    nextOrPreviousSong(true);
                    break;

                case SystemMediaTransportControlsButton.Previous:
                    PauseMedia();
                    nextOrPreviousSong(false);
                    break;

                default:
                break;
        }

    }

    async void PlayMedia()
    {
        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        {
            Debug.WriteLine("playmedia");
            playBtn();
        });
    }

    async void PauseMedia()
    {
        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        {
            playBtn();
        });

    }

    public void getlistsMusicsCB()
        {
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "lists.db");
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLitePlatformWinRT(), path))
            {
                var query = conn.Query<listsMusics>("SELECT DISTINCT listName from listsMusics");
                foreach(var item in query)
                {
                    listsMusicsCB.Items.Add(item.listName);
                }
            }
            listsMusicsCB.SelectedItem = "ALL";
        }

        public async void sqlGetAllMusic()
        {
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "lists.db");
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLitePlatformWinRT(), path))
            { 
                var songs = (await musicLib.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName)).ToList();
                int i;
                String split, songName;

                foreach (var song in songs)
                {

                    if (!song.ContentType.Contains("audio")) continue;
                    i = 4;
                    split = "";

                    songName = song.Name;
                    string[] splitSong = song.Path.Split('\\');
                    if (splitSong[4] == song.Name)
                    {

                        split = "NOT";
                    }
                    else
                    {
                        while (splitSong[i] != song.Name)
                        {
                            split += splitSong[i] + @"\";
                            i++;
                        }
                    }

                    conn.Insert(new listsMusics
                    {
                        name = songName,
                        path = split,
                        listName = "ALL"
                    });
                }


            }
            getlistsMusicsCB();
        }
        

        public void playBtn()
        {
            if (play.Symbol.Equals(Symbol.Play))
            {
                if (!isPause)
                {
                    songsView.SelectedItem = songsView.Items[0];
                    songsView_Play(0);
                }
                else myMediaElement.Play();
                play.Symbol = Symbol.Pause;
            }
            else
            {
                myMediaElement.Pause();
                isPause = true;
                play.Symbol = Symbol.Play;
            }
        }


        public async Task newPlayMusic(string name, string path)
        {
          
            musicLib = Windows.Storage.KnownFolders.MusicLibrary;
            StorageFile song;
            Debug.WriteLine(name);
            Debug.WriteLine(path);
            if (path == "NOT") song = await musicLib.GetFileAsync(name);
            else song = await musicLib.GetFileAsync(path + name);
            var stream = await song.OpenReadAsync();

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                play.Symbol = Symbol.Pause;
                myMediaElement.SetSource(stream, stream.ContentType);
                myMediaElement.Play();
                updater.MusicProperties.Title = name;
                updater.Update();
                
            });
         
           
        }

        public void Element_MediaOpened(object sender, RoutedEventArgs e)
        {

            sliderTimeline.Maximum = myMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            setVolume(sliderVolume.Value);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += new EventHandler<object>(Timer_Tick);
            timer.Start();
        }

        
          private void Element_MediaEnded(object sender, RoutedEventArgs e)
          {
            nextOrPreviousSong(true);
          }
          

        public void ChangeVolume(object sender, RangeBaseValueChangedEventArgs e)
        {

            setVolume(sliderVolume.Value);
        }


        public void setVolume(double value)
        {
            myMediaElement.Volume = value / 100;
        }


        public void SeekToMediaPosition(object sender, RangeBaseValueChangedEventArgs e)
        {
            int sliderValue = (int)sliderTimeline.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, sliderValue);
            myMediaElement.Position = ts;
        }


        public void Timer_Tick(object sender, object e)
        {
            sliderTimeline.Value = myMediaElement.Position.TotalMilliseconds;
        }


        public void songsView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            isPause = false;
            int index = songsView.SelectedIndex;
            
            songsView_Play(index);
            Debug.WriteLine("Selected play");
        }


        public async void songsView_Play(int index)
        {
            if (songsView.SelectedItem != null)
            {
                songIndex = index;
                
               await newPlayMusic(musicName[index], musicPath[index]);
               
            }
        }


        private void buttonPrevious_Click(object sender, RoutedEventArgs e)
        {
            nextOrPreviousSong(false);
            Debug.WriteLine("Previous");
        }


        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            nextOrPreviousSong(true);
            Debug.WriteLine("Next");
        }


        private async void  nextOrPreviousSong(bool next)
        {
            if (songIndex != -1)
            {
                if(next)
                { songIndex++; }
                else
                {
                    songIndex--;
                }
                try
                {
                    await newPlayMusic(musicName[songIndex], musicPath[songIndex]);
                }
                catch (ArgumentOutOfRangeException)
                {
                    if (next) songIndex = 0;
                    else songIndex = musicName.Count - 1;
                    await newPlayMusic(musicName[songIndex], musicPath[songIndex]);

                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>

                {
                    songsView.SelectedItem = songsView.Items[songIndex];

                });

            }
           
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            play.Symbol = Symbol.Play;
            myMediaElement.Stop();
        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            playBtn();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(playersLists));
        }

        private void listsMusicsCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine(listsMusicsCB.SelectedItem.ToString());
            changeView(listsMusicsCB.SelectedItem.ToString());
            
        }


        public void changeView(String item)
        {
            songsView.Items.Clear();
            musicName = new List<string>();
            musicPath = new List<string>();

            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "lists.db");
            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLitePlatformWinRT(), path))
            {
                var list = conn.Query<listsMusics>("Select name, path from listsMusics where listName = ?", item);
                foreach (var song in list)
                {
                    musicName.Add(song.name);
                    songsView.Items.Add(song.name);
                    musicPath.Add(song.path);
                    
                }
            }
            
        }

      
    }
}
