using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.ComponentModel;

namespace PptLyricMaker.Module
{
    public class Option : INotifyPropertyChanged
    {
        string defaultPptFormSearchPath_in;
        public string defaultPptFormSearchPath { get { return defaultPptFormSearchPath_in; } set { defaultPptFormSearchPath_in = value; NotifyPropertyChanged(); } }
        string defaultPptOutPath_in;
        public string defaultPptOutPath { get { return defaultPptOutPath_in; } set { defaultPptOutPath_in = value; NotifyPropertyChanged(); } }
        
        public Option()
        {
            // 파일 존재 확인
            FileInfo fi = new FileInfo(Module.FilePath.OPTION_PATH);
            if (!fi.Exists)
            {
                DirectoryInfo di = new DirectoryInfo(Module.FilePath.DATA_PATH);
                if (!di.Exists)
                {
                    di.Create();
                }

                // 파일이 없는 경우
                StreamWriter optionFile = new StreamWriter(Module.FilePath.OPTION_PATH);
                optionFile.WriteLine(defaultPptFormSearchPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\");
                optionFile.Write(defaultPptOutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\");
                optionFile.Close();
            }
            else
            {
                // 파일이 있는 경우
                StreamReader optionFile = new StreamReader(Module.FilePath.OPTION_PATH);
                defaultPptFormSearchPath = optionFile.ReadLine();
                defaultPptOutPath = optionFile.ReadToEnd();
                optionFile.Close();
            }
        }

        public void saveOptionData()
        {
            StreamWriter optionFile = new StreamWriter(Module.FilePath.OPTION_PATH);
            optionFile.WriteLine(defaultPptFormSearchPath);
            optionFile.Write(defaultPptOutPath);
            optionFile.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
