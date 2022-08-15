using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;

namespace TimeLines
{
	public class TimeLinesDataBase : INotifyPropertyChanged
	{
        public ObservableCollection<TimeLinesDataBase> Childs { get; set; } = new ObservableCollection<TimeLinesDataBase>();
        public ObservableCollection<ITimeLineData> Datas { get; set; } = new ObservableCollection<ITimeLineData>();

        #region BaseItem
        public event PropertyChangedEventHandler PropertyChanged;
        private bool isSelected;
        private bool isExpanded;
        public void OnPropertyChanged([CallerMemberName]  string propName = null)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set { if (isSelected == value) return; isSelected = value; OnPropertyChanged(); }
        }

        public bool IsExpanded
        {
            get { return isExpanded; }
            set { if (isExpanded == value) return; isExpanded = value; OnPropertyChanged(); }
        }
        #endregion
    }
	public interface ITimeLineData
	{
		TimeSpan? StartTime { get; set; }
		TimeSpan? EndTime { get; set; }
        bool? bResizable { get; }
	}
}
