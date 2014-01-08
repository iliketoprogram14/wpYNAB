using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace YNABv1.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Metadata : INotifyPropertyChanged
    {
        private String size;
        private int bytes;
        private String rev;
        private DateTime modified;
        private List<Metadata> contents;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        public Metadata(string json)
        {
            if (json == "") {
                modified = new DateTime();
                contents = new List<Metadata>();
                return;
            }
            JObject obj = JObject.Parse(json);
            size = (String)obj["size"];
            bytes = (int)obj["bytes"];
            Path = (String)obj["path"];
            IsDir = (bool)obj["is_dir"];
            rev = (String)obj["rev"];
            Icon = (String)obj["icon"];

            JToken outVal;
            if (obj.TryGetValue("modified", out outVal))
                modified = DateTime.Parse((String)obj["modified"]);

            if (IsDir && obj.TryGetValue("contents", out outVal)) {
                contents = new List<Metadata>();
                Array contentsArray = obj["contents"].ToArray();
                foreach (JToken temp in contentsArray)
                    contents.Add(new Metadata(temp.ToString()));
            }
        }

        #region Get/Set
        public String Path
        {
            get; set;
        }

        public String Icon
        {
            get; set;
        }

        public bool IsDir
        {
            get; set;
        }

        public String Name
        {
            get {
                string[] parts = Path.Split('/');
                return parts[parts.Length - 1];
            }
            set {
                string[] parts = Path.Split('/');
                string name = parts[parts.Length - 1];
                if (name == value || value == "")
                    return;

                string newPath = "";
                for (int i = 0; i < parts.Length - 1; i++)
                    newPath += "/" + parts[i];
                newPath += value;
                Path = newPath;
                NotifyPropertyChanged("Path");
            }
        }

        public String IconPath
        {
            get {
                if (Icon == "folder_photos")
                    return "Assets/DropboxIcons/folder_camera48.gif";
                else if (Icon == "page_white_word")
                    return "Assets/DropboxIcons/word48.gif";
                return "Assets/DropboxIcons/" + Icon + "48.gif";
            }
            set { }
        }

        public List<Metadata> Contents
        {
            get { return contents; }
        }
        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
