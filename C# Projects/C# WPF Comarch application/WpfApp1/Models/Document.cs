using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SetupOptimaToolkit.Models
{
    
    public class Document : INotifyPropertyChanged
    {
        public int DocumentID { get; set; }
        public string DocumentNumber { get; set; }
        private int? mainCustomerID;
        private int? payerID;
        private int? targetCustomerID;

        public DateTime DocumentDate { get; set; }
        private string mainCustomerName;
        private string payerName;
        private string targetCustomerName;
        private string documentDescription;

        public string DocumentDescription
        {
            get => documentDescription;
            set
            {
                documentDescription = value;
                NotifyPropertyChanged();
            }
        }

        public int? MainCustomerID
        {
            get => mainCustomerID;
            set
            {
                mainCustomerID = value;
                NotifyPropertyChanged();
            }
        }

        public string MainCustomerName
        {
            get => mainCustomerName;
            set
            {
                mainCustomerName = value;
                NotifyPropertyChanged();

            }
        }

        public int? PayerID
        {
            get => payerID;
            set
            {

                payerID = value;
                NotifyPropertyChanged();
            }
        }

        public string PayerName
        {
            get => payerName;
            set
            {
                payerName = value;
                NotifyPropertyChanged();
            }
        }

        public string TargetCustomerName
        {
            get => targetCustomerName;
            set
            {
                targetCustomerName = value;
                NotifyPropertyChanged();
            }
        }

        public int? TargetCustomerID
        {
            get => targetCustomerID;
            set
            {
                targetCustomerID = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
