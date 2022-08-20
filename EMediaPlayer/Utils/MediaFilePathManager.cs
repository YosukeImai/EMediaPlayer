using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EMediaPlayer
{
    public class MediaFilePathManager
    {
        public MediaFilePathManager()
        {
            //コマンドライン引数を取得
            string[] paths = Environment.GetCommandLineArgs();
            SetFilePath(paths);
        }

        public MediaFilePathManager(string[] paths)
        {
            SetFilePath(paths);
        }

        public bool IsContainFile
        {
            get { return mediaPaths != null && mediaPaths.Length > 0; }
        }

        public string CurrentMediaPath
        {
            get { return IsContainFile ? mediaPaths[mediaIndex] : null; }
        }

        private string[] mediaPaths;

        private int mediaIndex = -1;

        public void SetFirstIndex()
        {
            mediaIndex = 0;
        }

        public void SetNextIndex()
        {
            mediaIndex = mediaIndex == mediaPaths.Length -1 ? 0 : mediaIndex + 1;
        }

        public void SetPrevIndex()
        {
            mediaIndex = mediaIndex < 0 ? mediaPaths.Length - 1 : mediaIndex - 1;
        }

        public string[] AddPathHistory(string[] currentPathHistory, string mediaPath)
        {
            string[] result = new string[currentPathHistory.Length + 1];
            Array.Copy(currentPathHistory, result, currentPathHistory.Length);
            result[result.Length - 1] = mediaPath;

            return result;
        }

        private void SetFilePath(string[] paths)
        {
            paths = paths.Where(n => n.Contains(".mp4")).ToArray();

            if (paths.Length == 1)
            {
                string path = paths[0];
                FileInfo f = new FileInfo(path);
                DirectoryInfo d = f.Directory;
                FileInfo[] fs = d.GetFiles("*.mp4");
                paths = fs.Select(s => s.FullName).ToArray();
                mediaIndex = Array.IndexOf(paths, f.FullName);
            }
            else
                mediaIndex = 0;

            mediaPaths = paths;
        }
    }
}
