using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EMediaPlayer
{
    public class SettingParameters
    {
        private double windowWidth;
        private double windowHeight;
        private string pathHistory;

        private const char deliminator = '|';

        public double WindowWidth
        {
            get { return windowWidth; }
            set { 
                windowWidth =value;
                Properties.Settings.Default.WindowWidth = windowWidth;
            }
        }

        public double WindowHeight
        {
            get { return windowHeight; }
            set { 
                windowHeight = value;
                Properties.Settings.Default.WindowHeight = windowHeight;
            }
        }

        public string[] PathHistory
        {
            get
            {
                return pathHistory.Split(deliminator);
            }
            set
            {
                pathHistory = string.Join(deliminator, value);
                Properties.Settings.Default.pathHistory = pathHistory;
            }
        }
        

        public SettingParameters()
        {
            windowWidth = Properties.Settings.Default.WindowWidth;
            windowHeight = Properties.Settings.Default.WindowHeight;
            pathHistory = Properties.Settings.Default.pathHistory;
        }

        public void SaveSettingParameters()
        {
            Properties.Settings.Default.Save();
        }
    }
}
