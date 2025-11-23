using System;
using System.ComponentModel;

namespace FLS.Models
{
    public class UserCourse : INotifyPropertyChanged
    {
        private string _section = string.Empty;

        public int Id { get; set; }
        public Course Course { get; set; }
        
        public string Section
        {
            get => _section;
            set
            {
                if (_section != value)
                {
                    _section = value;
                    OnPropertyChanged(nameof(Section));
                }
            }
        }
        
        public DateTime EnrolledDate { get; set; } = DateTime.Now;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

