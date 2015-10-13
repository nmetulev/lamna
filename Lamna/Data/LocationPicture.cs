using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lamna.Data
{
    public class LocationPicture : INotifyPropertyChanged
    {
        public string ID { get; set; }
        private string _imageUri;

        public string ImageUri
        {
            get { return _imageUri; }
            set { _imageUri = value; RaisePropertyChanged(); }
        }

        public string InkUri { get; set; }
        private string _rawImageUri;

        public string RawImageUri
        {
            get { return _rawImageUri; }
            set { _rawImageUri = value; RaisePropertyChanged(); }
        }

        public LocationEnumaration Location { get; set; }
        public DefectEnumeration Defect { get; set; }
        public string Note { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            var propertyChanged = this.PropertyChanged;

            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
