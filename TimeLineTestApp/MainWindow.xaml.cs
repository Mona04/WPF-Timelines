using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimeLines;
using System.Collections.ObjectModel;

namespace TimeLineTestApp
{
	public class TempDatas : TimeLinesDataBase
	{
		public string ChannelName { get; set; }
	}
	public class TempDataType : ITimeLineData
	{
		public TimeSpan? StartTime { get; set; }
		public TimeSpan? EndTime { get; set; }
		public String Name { get; set; }
	}
	public partial class MainWindow : Window
	{
		ObservableCollection<TempDatas> datas = new ObservableCollection<TempDatas>();
		public MainWindow()
		{
			InitializeComponent();

			var tmp1 = new TempDataType()
			{
				StartTime = TimeSpan.FromMilliseconds(30),
				EndTime = TimeSpan.FromMilliseconds(180),
				Name = "Temp 1"			
			};
			var tmp3 = new TempDataType()
			{
				StartTime = TimeSpan.FromMilliseconds(440),
				EndTime = TimeSpan.FromMilliseconds(600),
				Name = "Temp 3"
			};
			datas.Add(new TempDatas());
			datas[0].Childs.Add(new TempDatas());
			datas.Add(new TempDatas());
			datas.Add(new TempDatas());
			datas.Add(new TempDatas());
			datas.Add(new TempDatas());
			datas.Add(new TempDatas());
			datas[0].ChannelName = "Transform";
			datas[1].ChannelName = "Notifies";
			(datas[0].Childs[0] as TempDatas).ChannelName = "Scale";
			datas[0].Childs[0].Datas.Add(tmp1);
		
			datas[2].Datas.Add(tmp3); datas[2].ChannelName = "Tests";
			datas[3].Datas.Add(tmp3); datas[3].ChannelName = "Tests";
			datas[4].Datas.Add(tmp3); datas[4].ChannelName = "Tests";
			datas[5].Datas.Add(tmp3); datas[5].ChannelName = "Tests";
			
			//Timelines.ItemsSource = datas;
		}

        private void Slider_Scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
			Timelines.CurrentTime = TimeSpan.FromMilliseconds(e.NewValue);
        }
    }
}
