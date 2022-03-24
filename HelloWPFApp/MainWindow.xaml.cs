using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HelloWPFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Mediaのタイムスパンを変更していいか
        private bool canChangeTimespan = false;

        private MediaState mediaState;
        //private MediaState preMediaState;
        private string[] paths;

        //再生するメディアのパス配列の添え字
        private int mediaIndex = -1;

        //スライダー更新用タイマー
        private DispatcherTimer sliderTimer;

        public MainWindow()
        {
            //コマンドライン引数を取得
            string[] args = Environment.GetCommandLineArgs();
            int argsLen = args.Length;

            InitializeComponent();

            InitializeTimer();

            //コマンドライン引数にパスが格納されている
            if(args != null && argsLen > 1)
            {
                paths = new string[argsLen - 1];

                //ファイルのパスだけ抜き出し
                Array.Copy(args, 1, paths, 0, paths.Length);
                PlayFirstMedia();
            }
        }



        private void Window_Drop(object sender, DragEventArgs e)
        {
            paths = (string[])e.Data.GetData(DataFormats.FileDrop);

            if(paths !=null && paths.Length != 0)
            {
                PlayFirstMedia();
            }

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //Playerが利用不可
            if (!AvailablePlayer())
                return;

            switch (e.Key)
            {
                case Key.Space:
                    if (mediaState == MediaState.Play)
                        Pause();
                    else
                        Play();
                    break;
                case Key.Right:
                    FastForward();
                    break;
                case Key.Left:
                    Rewind();
                    break;
                case Key.PageUp:
                    PlayNextMedia();
                    break;
                case Key.PageDown:
                    PlayPrevMedia();
                    break;

            }
        }

        private void Element_MediaOpened(object sender, EventArgs e)
        {
            timelineSlider.Maximum = myMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
        }

        private void Element_MediaEnded(object sender, EventArgs e)
        {
            PlayNextMedia();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (canChangeTimespan)
            {
                TimeSpan ts = new TimeSpan(0, 0, 0, 0,(int)timelineSlider.Value);

                Pause();

                myMediaElement.Position = ts;
            }
        }

        
        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            //Playerが利用不可
            if (!AvailablePlayer())
                return;

            Console.WriteLine("Slider_PreviewMouseDown");
            canChangeTimespan = true;

            Point p = e.GetPosition(timelineSlider);

            SetSliderPositionOnMouse(p);

            Pause();
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            if (!canChangeTimespan)
                return;

            Point p = e.GetPosition(timelineSlider);

            SetSliderPositionOnMouse(p);
        }

        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Playerが利用不可
            if (!AvailablePlayer())
                return;

            Console.WriteLine("Slider_PreviewMouseUp");
            canChangeTimespan = false;
            Play();
        }

        private void SliderTimer_Tick(object sender,EventArgs e)
        {
            switch (mediaState)
            {
                case MediaState.Play:
                    SetSliderValue();
                    SetTimelineBlock();
                    break;
                default:
                    //タイマーストップ
                    sliderTimer.Stop();
                    break;
            }
        }

        private void InitializeTimer()
        {
            sliderTimer = new DispatcherTimer();
            sliderTimer.Tick += new EventHandler(SliderTimer_Tick);

            sliderTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);

            sliderTimer.Stop();

        }

        private void InitializeMedia(string path)
        {
            Uri uri = new Uri(path);
            //マニュアル設定
            myMediaElement.LoadedBehavior = MediaState.Manual;
            myMediaElement.Source = uri;

            myMediaElement.Position = TimeSpan.Zero;
        }

        private void PlayFirstMedia()
        {
            mediaIndex = 0;
            InitializeMedia(paths[mediaIndex]);
            Play();
        }

        private void PlayNextMedia()
        {
            mediaIndex++;
            mediaIndex = mediaIndex == paths.Length ? 0 : mediaIndex;
            InitializeMedia(paths[mediaIndex]);
            Play();
        }

        private void PlayPrevMedia()
        {
            mediaIndex--;
            mediaIndex = mediaIndex < 0 ? paths.Length - 1 : mediaIndex;
            InitializeMedia(paths[mediaIndex]);
            Play();
        }

        private void FastForward()
        {
            //10秒早送り
            TimeSpan ts = myMediaElement.Position.Add(new TimeSpan(0, 0, 10));
            myMediaElement.Position = ts;
        }

        private void Rewind()
        {
            //10秒巻き戻し
            TimeSpan ts = myMediaElement.Position.Subtract(new TimeSpan(0, 0, 10));
            myMediaElement.Position = ts;
        }

        private void Play()
        {
            myMediaElement.Play();

            mediaState = MediaState.Play;

            //スライダー更新用のタイマー開始
            sliderTimer.Start();
        }

        private void Stop()
        {
            myMediaElement.Stop();
            
            mediaState = MediaState.Stop;
        }

        private void Pause()
        {

            myMediaElement.Pause();
            mediaState = MediaState.Pause;
        }

        private void SetSliderValue()
        {
            TimeSpan ts = myMediaElement.Position;
            timelineSlider.Value = ts.TotalMilliseconds;

        }

        private void SetTimelineBlock()
        {
            TimeSpan ts = myMediaElement.Position;
            timelineBlock.Text = ts.ToString(@"hh\:mm\:ss");
        }

        private bool AvailablePlayer()
        {
            return myMediaElement.Source != null;
        }

        private void SetSliderPositionOnMouse(Point point)
        {       
            double v = timelineSlider.Maximum * (double)point.X / (double)timelineSlider.ActualWidth;
            timelineSlider.Value = v;
        }

        
    }
}
