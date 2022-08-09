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
using TimeLineTool;
using System.Collections.ObjectModel;

namespace TimeLineTestApp
{
    /*Notes:
     * This simple little demo app doesn't leverage data binding, and doesn't demonstrate some of the things that are available.
     * It does give you a feel for how you can do everything, and should give someone a start so they knw what they can do via more poweful data binding practices.
     */ 
	
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		ObservableCollection<ITimeLineDataItem> data = new ObservableCollection<ITimeLineDataItem>();
        public ObservableCollection<ITimeLineDataItem> t2Data = new ObservableCollection<ITimeLineDataItem>();
        public ObservableCollection<ITimeLineDataItem> t3Data = new ObservableCollection<ITimeLineDataItem>();
		ObservableCollection<ITimeLineDataItem> listboxData = new ObservableCollection<ITimeLineDataItem>();
		public MainWindow()
		{
			InitializeComponent();

			var tmp1 = new TempDataType()
			{
				StartTime = TimeSpan.FromMilliseconds(30),
				EndTime = TimeSpan.FromMilliseconds(180),
				Name = "Temp 1"			
			};
			var temp3 = new TempDataType()
			{
				StartTime = TimeSpan.FromMilliseconds(440),
				EndTime = TimeSpan.FromMilliseconds(600),
				Name = "Temp 3"
			};

            t2Data.Add(tmp1);
            t3Data.Add(temp3);

			//TimeLine2.Items = data;
			TimeLine2.StartTime = TimeSpan.FromHours(0);            
            TimeLine3.StartTime = TimeSpan.FromHours(0);
            TimeLine2.Items = t2Data;
			TimeLine3.Items = t3Data;
			TimeLine2.ViewLevel = TimeLineViewLevel.MilliSeconds;
			TimeLine3.ViewLevel = TimeLineViewLevel.MilliSeconds;
			TimeLine2.EndTime = TimeSpan.FromMilliseconds(1300);
			TimeLine3.EndTime = TimeSpan.FromMilliseconds(1300);
			TimeLine2.CanLineChange = false;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			/*var tli = TimeLine1.Items[TimeLine1.Items.Count - 1] as TimeLineItemControl;
			var adder = new TimeLineItemControl()
			{
				StartTime = tli.EndTime.AddHours(15),
				EndTime = tli.EndTime.AddHours(30),
				ViewLevel = TimeLine1.ViewLevel,
				Content = new Button(){Content=(TimeLine1.Items.Count+1).ToString()}
			};
			ctrls.Add(adder);*/
			/*if (TimeLine1.ViewLevel == TimeLineViewLevel.Hours)
			{
				TimeLine1.ViewLevel = TimeLineViewLevel.Minutes;
			}
			else
			{
				TimeLine1.ViewLevel = TimeLineViewLevel.Hours;
			}*/
		}

		
		private void Slider_Scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			TimeLine3.UnitSize = Slider_Scale.Value;
			TimeLine2.UnitSize = Slider_Scale.Value;
		}

        private void FoundMe_MouseWheel(object sender, MouseWheelEventArgs e)
        {
			if(Keyboard.IsKeyDown(Key.LeftCtrl))
            {
				TimeLine3.UnitSize *= 1 + (e.Delta > 0 ? 0.1 : -0.1);
			}
        }
    }
}
