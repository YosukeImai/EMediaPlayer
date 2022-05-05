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

        public SettingParameters()
        {
            windowWidth = Properties.Settings.Default.WindowWidth;
            windowHeight = Properties.Settings.Default.WindowHeight;
        }

        public void SaveSettingParameters()
        {
            Properties.Settings.Default.Save();
        }
    }
}
