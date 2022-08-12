using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TimeLines
{

    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Ruler", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_ItemsPresenter", Type = typeof(ItemsPresenter))]
    [TemplatePart(Name = "PART_CurTimePivot", Type = typeof(TimelinePivot))]
    [TemplatePart(Name = "PART_HozitontalScroll", Type = typeof(ScrollBar))]
    [TemplatePart(Name = "PART_VerticalScrollViewer", Type = typeof(ScrollViewer))]
    [TemplatePart(Name = "PART_RulerScrollViewer", Type = typeof(ScrollViewer))]
    [TemplatePart(Name = "PART_CanvasScrollViewer", Type = typeof(ScrollViewer))]
    [TemplatePart(Name = "PART_TimePlayer", Type = typeof(TimelinePlayer))]
    public class Timelines : TreeView
    {
        Canvas PART_Canvas, PART_Ruler;
        ScrollViewer PART_VerticalScrollViewer, PART_RulerScrollViewer, PART_CanvasScrollViewer;
        ScrollBar PART_HozitontalScroll;
        ItemsPresenter PART_ItemsPresenter;
        TimelinePivot PART_CurTimePivot;
        TimelinePlayer PART_TimePlayer;
        List<TimeLineControl> TimelineControls;
      
        static Timelines()
        {
            DefaultStyleKeyProperty.OverrideMetadata
            (
                typeof(Timelines),
                new FrameworkPropertyMetadata(typeof(TreeView))
            );
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_ItemsPresenter = Template.FindName(nameof(PART_ItemsPresenter), this) as ItemsPresenter;
            if (PART_ItemsPresenter == null)
                throw new FormatException("Invalid Template Type");

            #region ScrollViewer

            PART_RulerScrollViewer = Template.FindName(nameof(PART_RulerScrollViewer), this) as ScrollViewer;
            if (PART_RulerScrollViewer == null)
                throw new FormatException("Invalid Template Type");

            PART_CanvasScrollViewer = Template.FindName(nameof(PART_CanvasScrollViewer), this) as ScrollViewer;
            if (PART_CanvasScrollViewer == null)
                throw new FormatException("Invalid Template Type");

            PART_VerticalScrollViewer = Template.FindName(nameof(PART_VerticalScrollViewer), this) as ScrollViewer;
            if (PART_VerticalScrollViewer == null)
                throw new FormatException("Invalid Template Type");

            PART_HozitontalScroll = Template.FindName(nameof(PART_HozitontalScroll), this) as ScrollBar;
            if (PART_HozitontalScroll == null)
                throw new FormatException("Invalid Template Type");

            #endregion

            PART_Canvas = Template.FindName(nameof(PART_Canvas), this) as Canvas;
            if (PART_Canvas == null)
                throw new FormatException("Invalid Template Type");

            PART_Ruler = Template.FindName(nameof(PART_Ruler), this) as Canvas;
            if (PART_Ruler == null)
                throw new FormatException("Invalid Template Type");

            PART_CurTimePivot = Template.FindName(nameof(PART_CurTimePivot), this) as TimelinePivot;
            if (PART_CurTimePivot == null)
                throw new FormatException("Invalid Template Type");

            PART_TimePlayer = Template.FindName(nameof(PART_TimePlayer), this) as TimelinePlayer;
            if (PART_TimePlayer == null)
                throw new FormatException("Invalid Template Type");

            PART_TimePlayer.Template = TimePlayerTemplate;

            PART_Ruler.MouseDown += RulerClicked;
            PART_Ruler.MouseMove += RulerClicked;

            PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
            PART_HozitontalScroll.ValueChanged += ScrollViewer_HorizontalValueChanged;

            Loaded += (s, e) =>
            {
                Redraw();
                PART_HozitontalScroll.ViewportSize = PART_Ruler.ActualWidth;
            };

            AddHandler(TreeViewItem.ExpandedEvent, (RoutedEventHandler)OnTreeViewItem_Expanded);
            AddHandler(TreeViewItem.CollapsedEvent, (RoutedEventHandler)OnTreeViewItem_Expanded);
        }
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            CreateTimelineControls();
            Redraw();
        }
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            TimelineControls = new List<TimeLineControl>();
            System.Collections.IList items = e.NewItems;
            if (items == null || items.Count <= 0) return;

            if (!(items[0] is TimeLinesDataBase)) 
                throw new Exception("Binded Item must be ItemLinesDataBase or derivation of it");

            CreateTimelineControls();

            // Redraw() require Actual Width, so timer is need.
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, ee) => { Redraw(); dispatcherTimer.Stop(); };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dispatcherTimer.Start();
        }
        void OnTreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            CreateTimelineControls();
            UpdateLayout();
            Redraw();
        }
        void CreateTimelineControls()
        {
            TimelineControls.Clear();

            foreach(TimeLinesDataBase item in TimeLineUtils.FindAllTimeLinesData(Items))
            {
                CreateTimelineControl(item);
            }
        }
        void CreateTimelineControl(TimeLinesDataBase context)
        {
            TimeLineControl control = new TimeLineControl();

            Binding startTimeBind = new Binding(nameof(StartTime)) { Source = this };
            control.SetBinding(TimeLineControl.StartTimeProperty, startTimeBind);

            Binding endTimeBind = new Binding(nameof(EndTime)) { Source = this };
            control.SetBinding(TimeLineControl.EndTimeProperty, endTimeBind);

            Binding viewLevelBind = new Binding(nameof(ViewLevel)) { Source = this };
            control.SetBinding(TimeLineControl.ViewLevelProperty, viewLevelBind);

            Binding UnitSizeBind = new Binding(nameof(UnitSize)) { Source = this };
            control.SetBinding(TimeLineControl.UnitSizeProperty, UnitSizeBind);

            control.ItemTemplateSelector = TimeItemTemplateSelector;
            control.Items = context.Datas;          

            TimelineControls.Add(control);
        }

        #region Time Property

        #region unitsize
        public Double UnitSize
        {
            get { return (Double)GetValue(UnitSizeProperty); }
            set { SetValue(UnitSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitSizeProperty =
            DependencyProperty.Register(nameof(UnitSize), typeof(Double), typeof(Timelines),
            new UIPropertyMetadata(5.0, new PropertyChangedCallback(OnUnitSizeChanged)));
        private static void OnUnitSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Timelines timelines = d as Timelines;
            if (timelines != null)
            {
                //tc.UpdateViewLevel((TimeLineViewLevel)e.NewValue);
                timelines.Redraw();
            }

        }
        #endregion

        #region ViewLevel
        public TimeLineViewLevel ViewLevel
        {
            get { return (TimeLineViewLevel)GetValue(ViewLevelProperty); }
            set { SetValue(ViewLevelProperty, value); }
        }

        public static readonly DependencyProperty ViewLevelProperty =
            DependencyProperty.Register(nameof(ViewLevel), typeof(TimeLineViewLevel), typeof(Timelines),
            new UIPropertyMetadata(TimeLineViewLevel.MilliSeconds));

        public TimeLineViewLevel MarkViewLevel
        {
            get { return (TimeLineViewLevel)GetValue(MarkViewLevelProperty); }
            set { SetValue(MarkViewLevelProperty, value); }
        }

        public static readonly DependencyProperty MarkViewLevelProperty =
            DependencyProperty.Register(nameof(MarkViewLevel), typeof(TimeLineViewLevel), typeof(Timelines),
            new UIPropertyMetadata(TimeLineViewLevel.MilliSeconds));

        #endregion

        #region minimum unit width
        // Bigger than this unit range will be gridded
        public Double MinimumUnitWidth
        {
            get { return (Double)GetValue(MinimumUnitWidthProperty); }
            set { SetValue(MinimumUnitWidthProperty, value); }
        }

        public static readonly DependencyProperty MinimumUnitWidthProperty =
            DependencyProperty.Register("MinimumUnitWidth", typeof(Double), typeof(Timelines),
                new UIPropertyMetadata(25.0,
                    new PropertyChangedCallback(OnBackgroundValueChanged)));
        #endregion

        #region start date
        public TimeSpan StartTime
        {
            get { return (TimeSpan)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartDate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register(nameof(StartTime), typeof(TimeSpan), typeof(Timelines),
            new UIPropertyMetadata(TimeSpan.FromSeconds(0), new PropertyChangedCallback(OnStartTimeChanged)));
        private static void OnStartTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Timelines tc = d as Timelines;
            if (tc != null)
            {
                tc.Redraw();
            }
        }
        #endregion

        #region end time
        public TimeSpan EndTime
        {
            get { return (TimeSpan)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartDate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register(nameof(EndTime), typeof(TimeSpan), typeof(Timelines),
            new UIPropertyMetadata(TimeSpan.FromMilliseconds(1300), new PropertyChangedCallback(OnStartTimeChanged)));

        #endregion

        #region current time
        public TimeSpan CurrentTime
        {
            get { return (TimeSpan)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartDate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(nameof(CurrentTime), typeof(TimeSpan), typeof(Timelines),
            new UIPropertyMetadata(TimeSpan.FromSeconds(0), new PropertyChangedCallback(OnCurrentTimeChanged)));
        private static void OnCurrentTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Timelines tc = d as Timelines;
            if (tc != null && tc.PART_HozitontalScroll != null)
            {
                // Auto Scroll if current is out of view
                double cur_dist = TimeLineUtils.ConvertTimeToDistance(tc.CurrentTime, tc.ViewLevel, tc.UnitSize);
                double limit = TimeLineUtils.ConvertTimeToDistance(tc.StartTime, tc.ViewLevel, tc.UnitSize);
                limit += tc.PART_HozitontalScroll.Value;
                if (cur_dist > limit + tc.PART_HozitontalScroll.ActualWidth)
                    tc.PART_HozitontalScroll.Value += tc.PART_HozitontalScroll.ActualWidth * 0.5;
                else if(cur_dist < limit)
                    tc.PART_HozitontalScroll.Value -= tc.PART_HozitontalScroll.ActualWidth * 0.5;

                tc.CurrentTimeDraw();
            }
        }
        #endregion
        
        #endregion

        #region Control Property

        #region Time Item template
        public DataTemplateSelector TimeItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(TimeItemTemplateSelectorProperty); }
            set { SetValue(TimeItemTemplateSelectorProperty, value); }
        }
        public static readonly DependencyProperty TimeItemTemplateSelectorProperty =
            DependencyProperty.Register(nameof(TimeItemTemplateSelector), typeof(DataTemplateSelector), typeof(Timelines),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnItemTemplateChanged)));
        private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Timelines tc = d as Timelines;
            if (tc != null && tc.TimelineControls != null)
            {
                foreach (TimeLineControl timeline in tc.TimelineControls)
                {
                    timeline.ItemTemplateSelector = tc.TimeItemTemplateSelector;
                }
            }
        }

        #endregion

        #region Player Template
        public ControlTemplate TimePlayerTemplate
        {
            get { return (ControlTemplate)GetValue(TimePlayerTemplateProperty); }
            set { SetValue(TimePlayerTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimePlayerTemplateProperty =
            DependencyProperty.Register(nameof(TimePlayerTemplate), typeof(ControlTemplate), typeof(Timelines),
            new UIPropertyMetadata(null));

        #endregion

        #endregion

        virtual public void ScrollViewer_HorizontalValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double offset = e.NewValue;
            PART_RulerScrollViewer.ScrollToHorizontalOffset(offset);
            PART_CanvasScrollViewer.ScrollToHorizontalOffset(offset);
        }
        virtual public void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                UnitSize *= 1 + (e.Delta > 0 ? 0.1 : -0.1);
            }
            else
            {
                double offset = PART_VerticalScrollViewer.VerticalOffset - e.Delta*0.2f;
                PART_VerticalScrollViewer.ScrollToVerticalOffset(offset);        
            }
            e.Handled = true;
        }
        virtual protected void RulerClicked(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                Point point = e.GetPosition(PART_Canvas);
                if (point.Y > RulerHeight) return;

                double X = point.X;
                CurrentTime = TimeLineUtils.ConvertDistanceToTime(X, ViewLevel, UnitSize);
            }
        }

        #region DrawCanvas

        #region unit thickness
        public int MinorUnitThickness
        {
            get { return (int)GetValue(MinorUnitThicknessProperty); }
            set { SetValue(MinorUnitThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinorUnitThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinorUnitThicknessProperty =
            DependencyProperty.Register("MinorUnitThickness", typeof(int), typeof(Timelines),
                        new UIPropertyMetadata(1, new PropertyChangedCallback(OnBackgroundValueChanged)));
        public int MajorUnitThickness
        {
            get { return (int)GetValue(MajorUnitThicknessProperty); }
            set { SetValue(MajorUnitThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MajorUnitThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MajorUnitThicknessProperty =
            DependencyProperty.Register("MajorUnitThickness", typeof(int), typeof(Timelines),
                new UIPropertyMetadata(3, new PropertyChangedCallback(OnBackgroundValueChanged)));
        private static void OnBackgroundValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLineControl tc = d as TimeLineControl;
            if (tc != null)
            {
                //tc.DrawBackGround();
            }
        }

        #endregion

        #region Ruler Height
        public float RulerHeight
        {
            get { return (float)GetValue(RulerHeightProperty); }
            set { SetValue(RulerHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinorUnitThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RulerHeightProperty =
            DependencyProperty.Register(nameof(RulerHeight), typeof(float), typeof(Timelines),
                        new UIPropertyMetadata(25.0f, new PropertyChangedCallback(OnBackgroundValueChanged)));
        #endregion

        #region Header Width
        public float HeaderWidth
        {
            get { return (float)GetValue(HeaderWidthProperty); }
            set { SetValue(HeaderWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinorUnitThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderWidthProperty =
            DependencyProperty.Register(nameof(HeaderWidth), typeof(float), typeof(Timelines),
                        new UIPropertyMetadata(150.0f, new PropertyChangedCallback(OnBackgroundValueChanged)));
        #endregion

        #region RulerGrid brush
        public Brush RulerGridBrush
        {
            get { return (Brush)GetValue(RulerGridBrushProperty); }
            set { SetValue(RulerGridBrushProperty, value); }
        }

        public static readonly DependencyProperty RulerGridBrushProperty =
            DependencyProperty.Register(nameof(RulerGridBrush), typeof(Brush), typeof(Timelines),
            new UIPropertyMetadata(new SolidColorBrush(Colors.White),
                new PropertyChangedCallback(OnBackgroundValueChanged)));

        #endregion

        #region InternalGrid Brush
        public Brush InternalGridBrush
        {
            get { return (Brush)GetValue(InternalGridBrushProperty); }
            set { SetValue(InternalGridBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HourLineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InternalGridBrushProperty =
            DependencyProperty.Register(nameof(InternalGridBrush), typeof(Brush), typeof(Timelines),
            new UIPropertyMetadata(new SolidColorBrush(new Color() { R=90,G= 90, B= 90, A=255}),
                new PropertyChangedCallback(OnBackgroundValueChanged)));

        #endregion

        #region InternalFill Brush
        public Brush InternalFillBrush
        {
            get { return (Brush)GetValue(InternalFillBrushProperty); }
            set { SetValue(InternalFillBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HourLineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InternalFillBrushProperty =
            DependencyProperty.Register(nameof(InternalFillBrush), typeof(Brush), typeof(Timelines),
            new UIPropertyMetadata(new SolidColorBrush(new Color() { R = 50, G = 50, B = 50, A = 100 }),
                new PropertyChangedCallback(OnBackgroundValueChanged)));

        #endregion

        #region CurrentTimeGridBrush
        public Brush CurrentGridBrush
        {
            get { return (Brush)GetValue(CurrentGridBrushProperty); }
            set { SetValue(CurrentGridBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HourLineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentGridBrushProperty =
            DependencyProperty.Register(nameof(CurrentGridBrush), typeof(Brush), typeof(Timelines),
            new UIPropertyMetadata(new SolidColorBrush(new Color() { R = 150, G = 150, B = 150, A = 200 }),
                new PropertyChangedCallback(OnBackgroundValueChanged)));

        #endregion
        virtual public void Redraw()
        {
            if (PART_Ruler == null) return;

            PART_Ruler.Children.Clear();
            PART_Ruler.Children.Add(PART_CurTimePivot);
            PART_Canvas.Children.Clear();
            
            double width = Math.Max(PART_HozitontalScroll.ActualWidth, TimeLineUtils.ConvertTimeToDistance(EndTime - StartTime, ViewLevel, UnitSize));
            width += 1; // to make scroll thumb exists. 0 -> invisible
            double height = DesiredSize.Height;
            PART_Ruler.Width = width;
            PART_Canvas.Width = width;
            PART_Canvas.Height = height;
            PART_ItemsPresenter.Height = height;

            PART_HozitontalScroll.Maximum = width - PART_HozitontalScroll.ActualWidth;

            DrawRulerAndVerticalGrids();

            double y = 0; int i = 0;        
            foreach(TreeViewItem element in TimeLineUtils.FindTreeViewItems(this))
            {
                double y2 = y + TimeLineUtils.GetTreeViewHeaderHeight(element);
      
                DrawColumnGrids(y2); // wannto draw background line first
                TimeLineControl control = TimelineControls[i++];
                Canvas.SetTop(control, y);
                PART_Canvas.Children.Add(control);

                y = y2;
            }
            PART_Canvas.Height = y;
            PART_ItemsPresenter.Height = y;

            CurrentTimeDraw();
        }
        private void DrawRulerAndVerticalGrids()
        {
            TimeSpan MinTimeRange = TimeLineUtils.ConvertDistanceToTime(MinimumUnitWidth, ViewLevel, UnitSize);
            for (int i = 2, span = 5, every = 5; i < 12; i++)
            {           
                if (TimeLineUtils.ConvertToTime(span, ViewLevel) >= MinTimeRange)
                {
                    TimeSpan time = TimeLineUtils.ConvertToTime(span, ViewLevel) + StartTime;
                    double firstDist = TimeLineUtils.ConvertTimeToDistance(time, ViewLevel, UnitSize);
                    DrawIncrementLines(PART_Ruler, time, firstDist, time, every);
                    DrawGridLines(PART_Ruler, time, firstDist, time, every);
                    break;
                }
                switch (i % 3)
                {
                    case 0: span += span + span / 2; every = 4; break;
                    case 1: span += span; every = 4; break;
                    case 2: span += span; every = 5; break;
                }
            }
        }
        private void DrawGridLines(Canvas grid, TimeSpan firstLineDate, Double firstLineDistance,
                TimeSpan timeStep, int majorEvery, int majorEveryOffset = 0)
        {
            Double curX = firstLineDistance;
            TimeSpan curDate = firstLineDate;
            int cnt = 1;
            while (curX < grid.Width)
            {
                Line l = new Line();
                l.ToolTip = curDate;
                l.StrokeThickness = MinorUnitThickness;
                l.X1 = 0;
                l.X2 = 0;
                l.Y2 = RulerHeight - 3;

                if ((majorEvery > 0) && ((cnt - majorEveryOffset) % majorEvery == 0))
                {
                    l.Y1 = RulerHeight * 0.66;
                    l.StrokeThickness = MajorUnitThickness;
                    TextBlock text = new TextBlock() { Text = TimeLineUtils.GetTimeMark(curDate, MarkViewLevel) };
                    grid.Children.Add(text);
                    Canvas.SetLeft(text, curX + 5);
                    Canvas.SetTop(text, l.Y1 - 5);
                }
                else
                {
                    l.Y1 = RulerHeight *0.8;
                }
                l.Stroke = RulerGridBrush;
                grid.Children.Add(l);
                Canvas.SetLeft(l, curX);

                curX += TimeLineUtils.ConvertTimeToDistance(timeStep, ViewLevel, UnitSize);
                curDate += timeStep;
                cnt++;
            }
        }
        private void DrawIncrementLines(Canvas grid, TimeSpan firstLineDate, Double firstLineDistance,
                   TimeSpan timeStep, int majorEvery, int majorEveryOffset = 0)
        {
            Double curX = firstLineDistance;
            TimeSpan curDate = firstLineDate;
            int cnt = 1;
            while (curX < grid.Width)
            {
                if ((majorEvery > 0) && ((cnt - majorEveryOffset) % majorEvery == 0))
                {
                    Line l = new Line();
                    l.ToolTip = curDate;
                    l.StrokeThickness = MinorUnitThickness;
                    l.X1 = 0;
                    l.X2 = 0;
                    l.Y1 = RulerHeight;
                    l.Y2 = Math.Max(DesiredSize.Height, grid.ActualHeight) + RulerHeight;
                    l.Stroke = InternalGridBrush;
                    grid.Children.Add(l);
                    Canvas.SetLeft(l, curX);
                }            
                curX += TimeLineUtils.ConvertTimeToDistance(timeStep, ViewLevel, UnitSize);
                curDate += timeStep;
                cnt++;
            }
        }
        private void DrawColumnGrids(double y)
        {
            Line l = new Line();
            l.VerticalAlignment = VerticalAlignment.Center;
            l.StrokeThickness = MinorUnitThickness;
            l.X1 = 0;
            l.X2 = Math.Max(DesiredSize.Width, PART_Canvas.Width);
            l.Y1 = 0;
            l.Y2 = 0;
            l.Stroke = InternalGridBrush;
            PART_Canvas.Children.Add(l);
            Canvas.SetTop(l, y);
        }
        private void CurrentTimeDraw()
        {
            if (PART_Ruler == null)
                return;

            double X = TimeLineUtils.ConvertTimeToDistance(CurrentTime, ViewLevel, UnitSize);
            Canvas.SetLeft(PART_CurTimePivot, X - PART_CurTimePivot.ActualWidth/2);
        }
        #endregion
    }

    public class TimelinePivot : Control
    { 
        static TimelinePivot()
        {
            DefaultStyleKeyProperty.OverrideMetadata
            (
                typeof(TimelinePivot),
                new FrameworkPropertyMetadata(typeof(Control))
            );
        }
    }

    public class TimelinePlayer : Control
    {
        static TimelinePlayer()
        {
            DefaultStyleKeyProperty.OverrideMetadata
            (
                typeof(TimelinePlayer),
                new FrameworkPropertyMetadata(typeof(Control))
            );
        }
    }
}
