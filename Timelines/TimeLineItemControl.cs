using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Input;

namespace TimeLines
{
	//public class TimeLineItemControl:ContentPresenter
	public class TimeLineItemControl : Button
	{
		private Boolean _ready = true;
		internal Boolean ReadyToDraw
		{
			get { return _ready; }
			set
			{
				_ready = value;
			}
		}

		#region unitsize
		public Double UnitSize
		{
			get { return (Double)GetValue(UnitSizeProperty); }
			set { SetValue(UnitSizeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for UnitSize.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty UnitSizeProperty =
			DependencyProperty.Register("UnitSize", typeof(Double), typeof(TimeLineItemControl),
			new UIPropertyMetadata(5.0,
					new PropertyChangedCallback(OnUnitSizeChanged)));

		private static void OnUnitSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			
			TimeLineItemControl ctrl = d as TimeLineItemControl;
			if (ctrl != null)
			{
				ctrl.PlaceOnCanvas();
			}
			
		}
		#endregion

		#region ViewLevel
		public TimeLineViewLevel ViewLevel
		{
			get { return (TimeLineViewLevel)GetValue(ViewLevelProperty); }
			set { SetValue(ViewLevelProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ViewLevel.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ViewLevelProperty =
			DependencyProperty.Register("ViewLevel", typeof(TimeLineViewLevel), typeof(TimeLineItemControl),
			new UIPropertyMetadata(TimeLineViewLevel.Hours,
				new PropertyChangedCallback(OnViewLevelChanged)));
		private static void OnViewLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeLineItemControl ctrl = d as TimeLineItemControl;
			if (ctrl != null)
			{
				ctrl.PlaceOnCanvas();
			}
		 

		}
		#endregion

		#region timeline start time
		public TimeSpan TimeLineStartTime
		{
			get { return (TimeSpan)GetValue(TimeLineStartTimeProperty); }
			set { SetValue(TimeLineStartTimeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TimeLineStartTime.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TimeLineStartTimeProperty =
			DependencyProperty.Register("TimeLineStartTime", typeof(TimeSpan), typeof(TimeLineItemControl),
			new UIPropertyMetadata(TimeSpan.Zero,
				new PropertyChangedCallback(OnTimeValueChanged)));
		#endregion

		#region TimelineMaxTime
		public TimeSpan TimelineMaxTime
		{
			get { return (TimeSpan)GetValue(TimelineMaxTimeProperty); }
			set { SetValue(TimelineMaxTimeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TimeLineStartTime.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TimelineMaxTimeProperty =
			DependencyProperty.Register(nameof(TimelineMaxTime), typeof(TimeSpan), typeof(TimeLineItemControl),
			new UIPropertyMetadata(TimeSpan.MaxValue, new PropertyChangedCallback(OnTimeValueChanged)));
		#endregion

		#region start time
		public TimeSpan StartTime
		{
			get { return (TimeSpan)GetValue(StartTimeProperty); }
			set { SetValue(StartTimeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for StartTime.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StartTimeProperty =
			DependencyProperty.Register("StartTime", typeof(TimeSpan), typeof(TimeLineItemControl),
			new UIPropertyMetadata(TimeSpan.FromSeconds(0),
				new PropertyChangedCallback(OnTimeValueChanged)));


		#endregion

		#region end time
		public TimeSpan EndTime
		{
			get { return (TimeSpan)GetValue(EndTimeProperty); }
			set { SetValue(EndTimeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for EndTime.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty EndTimeProperty =
			DependencyProperty.Register("EndTime", typeof(TimeSpan), typeof(TimeLineItemControl),
			new UIPropertyMetadata(TimeSpan.FromMinutes(5),
									new PropertyChangedCallback(OnTimeValueChanged)));

		#endregion
		public Double EditBorderThreshold
        {
            get { return (Double)GetValue(EditBorderThresholdProperty); }
            set { SetValue(EditBorderThresholdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditBorderThreshold.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditBorderThresholdProperty =
            DependencyProperty.Register("EditBorderThreshold", typeof(Double), typeof(TimeLineItemControl), new UIPropertyMetadata(4.0, new PropertyChangedCallback(OnEditThresholdChanged)));

        private static void OnEditThresholdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLineItemControl ctrl = d as TimeLineItemControl;                  
        }

		private static void OnTimeValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            TimeLineItemControl ctrl = d as TimeLineItemControl;
            if (ctrl != null)
                ctrl.PlaceOnCanvas();
		}

		internal void PlaceOnCanvas()
		{
			var w = CalculateWidth();
			if (w > 0)
				Width = w;
			var p = CalculateLeftPosition();
			if (p >= 0)
			{
				Canvas.SetLeft(this, p);
			}
		}
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			// disable auto scroll when focused
			RequestBringIntoView += (s, ee) => { ee.Handled = true; };
		}
        private ContentPresenter _LeftIndicator;
        private ContentPresenter _RightIndicator;
        public override void OnApplyTemplate()
        {
            _LeftIndicator = Template.FindName("PART_LeftIndicator", this) as ContentPresenter;
            _RightIndicator = Template.FindName("PART_RightIndicator", this) as ContentPresenter;
            if (_LeftIndicator != null)
                _LeftIndicator.Visibility = System.Windows.Visibility.Collapsed;
            if (_RightIndicator != null)
                _RightIndicator.Visibility = System.Windows.Visibility.Collapsed;
            base.OnApplyTemplate();
        }		
		internal Double CalculateWidth()
		{	
			try
			{
				TimeSpan start = (TimeSpan)GetValue(StartTimeProperty);
				TimeSpan end = (TimeSpan)GetValue(EndTimeProperty);
				TimeSpan duration = end.Subtract(start);
				return TimeLineUtils.ConvertTimeToDistance(duration, ViewLevel, UnitSize);
			}
			catch (Exception)
			{
				return 0;
			}		
		}
		internal Double CalculateLeftPosition()
		{
			TimeSpan start = (TimeSpan)GetValue(StartTimeProperty);
			TimeSpan timelinestart = (TimeSpan)GetValue(TimeLineStartTimeProperty);

			TimeSpan Duration = start.Subtract(timelinestart);
			return TimeLineUtils.ConvertTimeToDistance(Duration, ViewLevel, UnitSize);
		}
        private void SetIndicators(System.Windows.Visibility left, System.Windows.Visibility right)
        {
            if (_LeftIndicator != null)
            {
                _LeftIndicator.Visibility = left;
            }
            if (_RightIndicator != null)
            {
                _RightIndicator.Visibility = right;
            }
        }

        #region MouseEvents
        protected override void OnMouseMove(MouseEventArgs e)
        {
            switch (GetClickAction())
            {                
                case TimeLineAction.StretchStart:
                    SetIndicators(System.Windows.Visibility.Visible, System.Windows.Visibility.Collapsed);
                    break;
                case TimeLineAction.StretchEnd:
                    SetIndicators(System.Windows.Visibility.Collapsed, System.Windows.Visibility.Visible);
                    //curso = Cursors.SizeWE;//Cursors.Hand;//Cursors.ScrollWE;
                    break;
                default:
                    SetIndicators(System.Windows.Visibility.Collapsed, System.Windows.Visibility.Collapsed);
                    break;
            }
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            SetIndicators(System.Windows.Visibility.Collapsed, System.Windows.Visibility.Collapsed);
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }
            base.OnMouseLeave(e);
        }
        #endregion

        #region manipulation tools
        internal TimeLineAction GetClickAction()
		{	
			var X = Mouse.GetPosition(this).X;
            Double borderThreshold = (Double)GetValue(EditBorderThresholdProperty);// 4;
			Double unitsize = (Double)GetValue(UnitSizeProperty);
			
			if (X < borderThreshold)
				return TimeLineAction.StretchStart;
			if (X > Width - borderThreshold)
				return TimeLineAction.StretchEnd;
			return TimeLineAction.Move;
			
		}
		
		internal bool CanDelta(int StartOrEnd, Double deltaX)
		{			
			Double unitS = (Double)GetValue(UnitSizeProperty);
			Double threshold = unitS / 3.0;
			Double newW = unitS;
			if (StartOrEnd == 0)//we are moving the start
			{
				if (deltaX < 0)
					return true;
				//otherwises get what our new width would be
				newW = Width - deltaX;//delta is + but we are actually going to shrink our width by moving start +
				return newW > threshold;
			}
			else
			{
				if (deltaX > 0)
					return true;
				newW = Width + deltaX;
				return newW > threshold;
			}
		}
		
        internal TimeSpan GetDeltaTime(Double deltaX)
		{
			return TimeLineUtils.ConvertDistanceToTime(deltaX, ViewLevel, UnitSize);
		}
		
		internal void GetPlacementInfo(ref Double left, ref Double width, ref Double end)
		{
			left = Canvas.GetLeft(this);
			width = Width;
			end = left + Width;
			//Somewhere on the process of removing a timeline control from the visual tree
			//it resets our start time to min value.  In that case it then results in ridiculous placement numbers
			//that this feeds to the control and crashes the whole app in a strange way.
			if(TimeLineStartTime == TimeSpan.MinValue)
			{
				left = 0;
				width = 1;
				end = 1;
			}
		}

		internal void MoveMe(Double deltaX)
		{
			TimeSpan DeltaTime = TimeLineUtils.ConvertDistanceToTime(deltaX, ViewLevel, UnitSize);

			if (StartTime + DeltaTime < TimeLineStartTime)
				DeltaTime = TimeLineStartTime - StartTime;
			if (EndTime + DeltaTime > TimelineMaxTime)
				DeltaTime = EndTime - TimelineMaxTime;

			StartTime += DeltaTime;
			EndTime += DeltaTime;
		}

		internal void MoveEndTime(double delta)
		{
			Width += delta;
			//calculate our new end time
			TimeSpan s = (TimeSpan)GetValue(StartTimeProperty);
			TimeSpan ts = TimeLineUtils.ConvertDistanceToTime(Width, ViewLevel, UnitSize);
			if (StartTime + ts > TimelineMaxTime)
				return;
			EndTime = s.Add(ts);
		}

		internal void MoveStartTime(double delta)
		{
			Double curLeft = Canvas.GetLeft(this);
			if (curLeft == 0 && delta < 0)
				return;
			curLeft += delta;
			Width = Width - delta;
			if (curLeft < 0)
			{
				//we need to 
				Width -= curLeft;//We are moving back to 0 and have to fix our width to not bump a bit.
				curLeft = 0;
			}
			Canvas.SetLeft(this, curLeft);
			//recalculate start time;
			TimeSpan ts = TimeLineUtils.ConvertDistanceToTime(curLeft, ViewLevel, UnitSize);
			StartTime = TimeLineStartTime.Add(ts);
			
		}

		internal void MoveToNewStartTime(TimeSpan start)
		{
			TimeSpan s = (TimeSpan)GetValue(StartTimeProperty);
			TimeSpan e = (TimeSpan)GetValue(EndTimeProperty);
			TimeSpan duration = e.Subtract(s);
			StartTime = start;
			EndTime = start.Add(duration);
			PlaceOnCanvas();
			 
		}
		#endregion
		
		/// <summary>
		/// Sets up with a default of 55 of our current units in size.
		/// </summary>
		internal void InitializeDefaultLength()
		{
			TimeSpan duration = TimeLineUtils.ConvertDistanceToTime(10 * (Double)GetValue(UnitSizeProperty), ViewLevel, UnitSize);
			EndTime = StartTime.Add(duration);
			Width = CalculateWidth();
		}
	}
}