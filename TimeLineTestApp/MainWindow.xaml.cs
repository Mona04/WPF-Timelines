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
		public static int cnt = 0;
		static Random rand = new Random();
		public TempDataType()
        {
			cnt++;
			Name = "Temp" + cnt.ToString();
			StartTime = TimeSpan.FromMilliseconds(rand.Next(0, 600));
			EndTime = StartTime + TimeSpan.FromMilliseconds(rand.Next(300, 700));
        }
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
				StartTime = TimeSpan.FromMilliseconds(1),
				EndTime = TimeSpan.FromMilliseconds(2),
			};
			var tmp3 = new TempDataType()
			{
				StartTime = TimeSpan.FromMilliseconds(2),
				EndTime = TimeSpan.FromMilliseconds(3),
			};

			var TimeLinesDatas = new ObservableCollection<TempDatas>();
			TimeLinesDatas.Add(new TempDatas());
			TimeLinesDatas[0].Childs.Add(new TempDatas());
			TimeLinesDatas.Add(new TempDatas());
			TimeLinesDatas[0].ChannelName = "Transform";
			(TimeLinesDatas[0].Childs[0] as TempDatas).ChannelName = "Scale";
			TimeLinesDatas[1].ChannelName = "Notifies";
			TimeLinesDatas[0].Childs[0].Datas.Add(tmp1);
			TimeLinesDatas[1].Datas.Add(tmp3);

			Timelines.ItemsSource = TimeLinesDatas;
		}

        private void Slider_Scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
			Timelines.CurrentTime = TimeSpan.FromMilliseconds(e.NewValue);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
		{ 
			Source();
        }
		void Source()
        {
			datas.Clear();

			datas.Add(new TempDatas());
			datas[0].Childs.Add(new TempDatas());
			datas.Add(new TempDatas());
			datas.Add(new TempDatas());
			datas.Add(new TempDatas());
			datas.Add(new TempDatas());
			datas.Add(new TempDatas());
			datas[0].ChannelName = "Transform" + TempDataType.cnt.ToString();
			datas[1].ChannelName = "Notifies";
			(datas[0].Childs[0] as TempDatas).ChannelName = "Scale";
			datas[0].Childs[0].Datas.Add(new TempDataType());

			datas[2].Datas.Add(new TempDataType()); datas[2].ChannelName = "Tests";
			datas[3].Datas.Add(new TempDataType()); datas[3].ChannelName = "Tests";
			datas[4].Datas.Add(new TempDataType()); datas[4].ChannelName = "Tests";
			datas[5].Datas.Add(new TempDataType()); datas[5].ChannelName = "Tests";

			Timelines.ItemsSource = datas;
		}
    }
    class TempDataTemplateSeletor : DataTemplateSelector
    {
		public DataTemplate TempDataType { get; set; }
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is TempDataType)
				return TempDataType;
			return null;
		}
	}
}
