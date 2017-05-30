using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
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
    public sealed partial class videoPlayer : Page
    {
        public StorageFolder videoLib = Windows.Storage.KnownFolders.VideosLibrary;
        public DispatcherTimer timer;
        public List<string> videoName = new List<string>();
        public List<string> videoPath = new List<string>();
        public bool isPause = false;
        int videoIndex = -1;

        public videoPlayer()
        {
          
            this.InitializeComponent();
            getAllVideo();
        }



        public void stopMusic()
        {
            myMediaElement.Stop();
        }


        public void playBtn()
        {
            if (play.Symbol.Equals(Symbol.Play))
            {
                if (!isPause)
                {
                    videoView.SelectedItem = videoView.Items[0];
                    videosView_Play(0);
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


        public async Task playVideo(string name, string path)
        {
            play.Symbol = Symbol.Pause;
            //videoLib = Windows.Storage.KnownFolders.MusicLibrary;
            StorageFile song;
            Debug.WriteLine(name);
            Debug.WriteLine(path);
            if (path == "NOT") song = await videoLib.GetFileAsync(name);
            else song = await videoLib.GetFileAsync(path + name);
            var stream = await song.OpenReadAsync();

            myMediaElement.SetSource(stream, stream.ContentType);
            myMediaElement.Play();

        }

        public async void getAllVideo()
        {

            var videos = (await videoLib.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName)).ToList();
            foreach (var video in videos)
            {
                if (!video.ContentType.Contains("video")) continue;

                videoName.Add(video.Name);
                string[] splitVideo = video.Path.Split('\\');
                if (splitVideo[4] == video.Name)
                {
                    videoPath.Add("NOT");
                }
                else
                {
                    videoPath.Add(splitVideo[4] + @"\");
                }

                videoView.Items.Add(video.Name);
            }
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

        public void Timer_Tick(object sender, object e)
        {
            sliderTimeline.Value = myMediaElement.Position.TotalMilliseconds;
        }

        public void SeekToMediaPosition(object sender, RangeBaseValueChangedEventArgs e)
        {
            int sliderValue = (int)sliderTimeline.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, sliderValue);
            myMediaElement.Position = ts;
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

       

        public void videosView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            isPause = false;
            int index = videoView.SelectedIndex;

            videosView_Play(index);
            Debug.WriteLine("Selected play");
        }

        public async void videosView_Play(int index)
        {
            if (videoView.SelectedItem != null)
            {
                videoIndex = index;

                await playVideo(videoName[index], videoPath[index]);

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

        private async void nextOrPreviousSong(bool next)
        {
            if (videoIndex != -1)
            {
                if (next)
                { videoIndex++; }
                else
                {
                    videoIndex--;
                }
                try
                {
                    await playVideo(videoName[videoIndex], videoPath[videoIndex]);
                }
                catch (ArgumentOutOfRangeException)
                {
                    if (next) videoIndex = 0;
                    else videoIndex = videoName.Count - 1;

                    await playVideo(videoName[videoIndex], videoPath[videoIndex]);

                }

                videoView.SelectedItem = videoView.Items[videoIndex];

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

        private void myMediaElement_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode) view.ExitFullScreenMode();
            else ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            myMediaElement.IsFullWindow = !myMediaElement.IsFullWindow;
            

        }
    }
}
