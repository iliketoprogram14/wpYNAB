using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{
    public class Metadata : INotifyPropertyChanged
    {
        private String size;
        private int bytes;
        private String path;
        private bool is_dir;
        private String rev;
        private String icon;
        private DateTime modified;
        List<Metadata> contents;

        public Metadata(string json)
        {
            JObject obj = JObject.Parse(json);
            size = (String)obj["size"];
            bytes = (int)obj["bytes"];
            path = (String)obj["path"];
            is_dir = (bool)obj["is_dir"];
            rev = (String)obj["rev"];
            icon = (String)obj["icon"];
            JToken outVal;
            if (obj.TryGetValue("modified", out outVal)) {
                modified = DateTime.Parse((String)obj["modified"]);
            }
            if (is_dir && obj.TryGetValue("contents", out outVal)) {
                contents = new List<Metadata>();
                Array contentsArray = obj["contents"].ToArray();
                foreach (JToken temp in contentsArray)
                    contents.Add(new Metadata(temp.ToString()));
            }
        }

        public String Path
        {
            get { return path; }
            set {
                if (path == value)
                    return;
                path = value;
                NotifyPropertyChanged("Path");
            }
        }

        public String Icon
        {
            get { return icon; }
            set {
                if (icon == value)
                    return;
                icon = value;
                NotifyPropertyChanged("Icon");
            }
        }

        public bool IsDir
        {
            get { return is_dir; }
            set {
                if (is_dir == value)
                    return;
                is_dir = value;
                NotifyPropertyChanged("IsDir");
            }
        }

        public String Name
        {
            get {
                string[] parts = path.Split('/');
                return parts[parts.Length - 1];
            }
            set {
                string[] parts = path.Split('/');
                string name = parts[parts.Length - 1];
                if (name == value || value == "")
                    return;
                string newPath = "";
                for (int i = 0; i < parts.Length - 1; i++)
                    newPath += "/" + parts[i];
                newPath += value;
                path = newPath;
                NotifyPropertyChanged("Path");
            }
        }

        public String IconPath
        {
            get {
                String derp = "Assets/DropboxIcons/" + icon + ".gif";
                return "Assets/DropboxIcons/" + icon + "48.gif";
            }
            set
            {

            }
        }

        public List<Metadata> Contents
        {
            get { return contents; }
            set {

            }
        }
        
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
