using EMediaPlayer;
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
        //SeekBar編集中か
        private bool canEditingSeekBar = false;

        private MediaState mediaState;

        //スライダー更新用タイマー
        private DispatcherTimer sliderTimer;

        //メディアの総時間（ミリ秒）
        private double mediaMaxValue = 0.0;

        //マウスの現在地（キャンバスの相対値）
        private Point mousePoint = new Point();

        //シークバーに触れているか
        private bool isTouchSeekBar = false;

        private SettingParameters settingParameters;

        private MediaFilePathManager mFilePathManager;

        public MainWindow()
        {
            //Read setting values
            settingParameters = new SettingParameters();

            InitializeComponent();

            InitializeTimer();

            mFilePathManager = new MediaFilePathManager();

            if (mFilePathManager.IsContainFile)
            {
                StartCurrentMedia();
            }

            SetWindowSizeInit();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //SeekBar描画
            BuildSeekBar();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            mFilePathManager = new MediaFilePathManager(paths);

            if(mFilePathManager.IsContainFile)
            {
                StartCurrentMedia();
            }

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    break;
            }

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
            //メディアの最大値をミリセカンドの総数で割り当てる
            mediaMaxValue = myMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;

            BuildSeekBar();
        }

        private void Element_MediaEnded(object sender, EventArgs e)
        {
            PlayNextMedia();
        }

        private void Canvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Playerが利用不可
            if (!AvailablePlayer())
                return;

            //SeekBarに触れてる場合はSeekBar編集可能
            canEditingSeekBar = isTouchSeekBar;

            //SeekBarを編集可能
            if (canEditingSeekBar)
            {
                //メディアの再生位置を変更
                SetMediaPositionFromMousePosition();

                //再描画
                BuildSeekBar();

                //一時停止
                Pause();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            //Playerが利用不可
            if (!AvailablePlayer())
                return;

            //マウスの現在地を割り当てる
            mousePoint = e.GetPosition(seekBarCanvas);

            //SeekBarのあたり判定（上下10ピクセルかつ左右0ピクセル以下が閾値）
            isTouchSeekBar = Math.Abs(mousePoint.Y - Line1.Y1) <= 10.0 &
                Line1.X1 <= mousePoint.X & mousePoint.X <= Line2.X2;

            //SeekBar編集中か
            if (canEditingSeekBar)
            {
                //メディアのポジションを変更する
                SetMediaPositionFromMousePosition();
            }

            //SeekBar再描画
            BuildSeekBar();
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            //SeekBarのあたり判定をFalseにする
            isTouchSeekBar = false;

            //SeekBar編集中
            if(canEditingSeekBar)
            {
                canEditingSeekBar = false;
                Play();
            }
        }

        private void Canvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Playerが利用不可
            if (!AvailablePlayer())
                return;

            //SeekBarを未編集状態にする
            canEditingSeekBar = false;

            //再生開始
            Play();
        }

        private void SliderTimer_Tick(object sender,EventArgs e)
        {
            //SeekBar編集可能か
            if (canEditingSeekBar)
                return;

            switch (mediaState)
            {
                case MediaState.Play:
                    BuildSeekBar();
                    SetTimelineBlock();
                    break;
                default:
                    //タイマーストップ
                    sliderTimer.Stop();
                    break;
            }
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BuildSeekBar();

            SetWindowSizeParameters();
        }

        private void Element_MouseDown(object sender, MouseEventArgs e)
        {
            //Playerが利用不可
            if (!AvailablePlayer())
                return;

            if (mediaState == MediaState.Play)
                Pause();
            else
                Play();
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

            //Set title name of screen
            SetTitleOfScreen(path);

            //マニュアル設定
            myMediaElement.LoadedBehavior = MediaState.Manual;
            myMediaElement.Source = uri;

            myMediaElement.Position = new TimeSpan(0, 0, 0);
        }

        private void PlayNextMedia()
        {
            mFilePathManager.SetNextIndex();
            StartCurrentMedia();
        }

        private void PlayPrevMedia()
        {
            mFilePathManager.SetPrevIndex();
            StartCurrentMedia();
        }

        private void StartCurrentMedia()
        {
            string path = mFilePathManager.CurrentMediaPath;
            if (path != null)
            {
                InitializeMedia(path);
              
                Play();
            }
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

        private void SetTimelineBlock()
        {
            TimeSpan ts = myMediaElement.Position;
            timelineBlock.Text = ts.ToString(@"hh\:mm\:ss");
        }

        private bool AvailablePlayer()
        {
            return myMediaElement.HasAudio;
        }
        
        private void SetMediaPositionFromMousePosition()
        {
            double d = (mousePoint.X - Line1.X1) / (Line2.X2 - Line1.X1) * mediaMaxValue;
            int milliSecond = d < 0 ? 0 : (int)d;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, milliSecond);

            myMediaElement.Position = ts;
        }
        
        private void BuildSeekBar()
        {
            double maxValue = mediaMaxValue;
            double currentValue = myMediaElement.Position.TotalMilliseconds;

            double width = seekBarCanvas.ActualWidth - Line1.X1 * 2;

            Line1.X2 = Line2.X1 = maxValue == 0 ? Line1.X1 : width * currentValue / maxValue +5;


            //円描画
            double radius = isTouchSeekBar ? 5 : 0;
            double left = mousePoint.X - radius;
            double top = Line1.Y1 - radius;

            Circle1.Width = Circle1.Height = radius * 2;
            Circle1.Margin = new Thickness(left, top, 0, 0);

            Line2.X2 = width;
        }

        private void SetWindowSizeInit()
        {
            if (settingParameters.WindowWidth != 0)
                this.Width = settingParameters.WindowWidth;
            if (settingParameters.WindowHeight != 0)
                this.Height = settingParameters.WindowHeight;
        }

        private void SetWindowSizeParameters()
        {
            settingParameters.WindowWidth = this.Width;
            settingParameters.WindowHeight = this.Height;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            settingParameters.SaveSettingParameters();

            if (AvailablePlayer())
                Stop();
        }

        private void SetTitleOfScreen(string path)
        {
            string filename= System.IO.Path.GetFileNameWithoutExtension(path);
            this.Title = filename;
        }
    }
}
