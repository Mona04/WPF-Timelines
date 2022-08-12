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
using System.Windows.Shapes;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;


namespace TimeLines
{

    public enum TimeLineManipulationMode { Linked, Free }
    internal enum TimeLineAction { Move, StretchStart, StretchEnd }

    internal class TimeLineItemChangedEventArgs : EventArgs
    {
        public TimeLineManipulationMode Mode { get; set; }
        public TimeLineAction Action { get; set; }
        public TimeSpan DeltaTime { get; set; }
        public Double DeltaX { get; set; }

    }

    internal class TimeLineDragAdorner : Adorner
    {
        private ContentPresenter _adorningContentPresenter;
        internal ITimeLineData Data { get; set; }
        internal DataTemplate Template { get; set; }
        Point _mousePosition;
        public Point MousePosition
        {
            get
            {
                return _mousePosition;
            }
            set
            {
                if (_mousePosition != value)
                {
                    _mousePosition = value;
                    _layer.Update(AdornedElement);
                }

            }
        }

        AdornerLayer _layer;
        public TimeLineDragAdorner(TimeLineItemControl uiElement, DataTemplate template)
            : base(uiElement)
        {
            _adorningContentPresenter = new ContentPresenter();
            _adorningContentPresenter.Content = uiElement.DataContext;
            _adorningContentPresenter.ContentTemplate = template;
            _adorningContentPresenter.Opacity = 0.5;
            _layer = AdornerLayer.GetAdornerLayer(uiElement);

            _layer.Add(this);
            IsHitTestVisible = false;

        }
        public void Detach()
        {
            _layer.Remove(this);
        }
        protected override Visual GetVisualChild(int index)
        {
            return _adorningContentPresenter;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            //_adorningContentPresenter.Measure(constraint);
            return new Size((AdornedElement as TimeLineItemControl).Width, (AdornedElement as TimeLineItemControl).DesiredSize.Height);//(_adorningContentPresenter.Width,_adorningContentPresenter.Height);
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _adorningContentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(MousePosition.X - 4, MousePosition.Y - 4));
            return result;
        }

    }

    public enum TimeLineViewLevel { MilliSeconds, Seconds, Minutes, Hours, Days, Weeks, Months, Years };
    //public class TimeLineControl : ListBox


    public class TimeLineControl : Canvas
    {
        public static TimeSpan CalculateMinimumAllowedTimeSpan(double unitSize)
        {
            //minute = unitsize*pixels
            //desired minimum widh for these manipulations = 10 pixels
            int minPixels = 10;
            double hours = minPixels / unitSize;
            //convert to milliseconds
            long ticks = (long)(hours * 60 * 60000 * 10000);
            return new TimeSpan(ticks);
        }

        private Double _bumpThreshold = 1.5;
        private ScrollViewer _scrollViewer;
        static TimeLineDragAdorner _dragAdorner;
        static TimeLineDragAdorner DragAdorner
        {
            get
            {
                return _dragAdorner;
            }
            set
            {
                if (_dragAdorner != null)
                    _dragAdorner.Detach();
                _dragAdorner = value;
            }
        }
        private Boolean _synchedWithSiblings = true;
        public Boolean SynchedWithSiblings
        {
            get
            {
                return _synchedWithSiblings;
            }
            set
            {
                _synchedWithSiblings = value;
            }
        }
        internal Boolean _isSynchInstigator = false;
        internal Double SynchWidth = 0;

        Boolean _itemsInitialized = false;

        Boolean _unitSizeInitialized = false;
        Boolean _startTimeInitialized = false;

        bool bBindInitialize = false;

        #region dependency properties


        public ITimeLineData FocusOnItem
        {
            get { return (ITimeLineData)GetValue(FocusOnItemProperty); }
            set { SetValue(FocusOnItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusOnItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusOnItemProperty =
            DependencyProperty.Register("FocusOnItem", typeof(ITimeLineData), typeof(TimeLineControl), new UIPropertyMetadata(null, new PropertyChangedCallback(FocusItemChanged)));
        public static void FocusItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLineControl tc = d as TimeLineControl;
            if ((e.NewValue != null) && (tc != null))
            {
                //tc.ScrollToItem(e.NewValue as ITimeLineData);
            }

        }

        #region manager
        public ITimeLineManager Manager
        {
            get { return (ITimeLineManager)GetValue(ManagerProperty); }
            set { SetValue(ManagerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Manager.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ManagerProperty =
            DependencyProperty.Register("Manager", typeof(ITimeLineManager), typeof(TimeLineControl),
            new UIPropertyMetadata(null));

        #endregion

        #region minwidth
        public Double MinWidth
        {
            get { return (Double)GetValue(MinWidthProperty); }
            set { SetValue(MinWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.Register("MinWidth", typeof(Double), typeof(TimeLineControl), new UIPropertyMetadata(0.0));
        #endregion

        #region minheight
        public Double MinHeight
        {
            get { return (Double)GetValue(MinHeightProperty); }
            set { SetValue(MinHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinHeightProperty =
            DependencyProperty.Register("MinHeight", typeof(Double), typeof(TimeLineControl), new UIPropertyMetadata(0.0));
        #endregion

        #region background and grid dependency properties

        #region snap to grid
        public Boolean SnapToGrid
        {
            get { return (Boolean)GetValue(SnapToGridProperty); }
            set { SetValue(SnapToGridProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SnapToGrid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SnapToGridProperty =
            DependencyProperty.Register("SnapToGrid", typeof(Boolean), typeof(TimeLineControl),
                new UIPropertyMetadata(null));
        #endregion

        #endregion

        #region item template
        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplateSelector), typeof(DataTemplateSelector), typeof(TimeLineControl),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnItemTemplateChanged)));
        private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLineControl tc = d as TimeLineControl;
            if (tc != null)
            {
                tc.SetTemplate(e.NewValue as DataTemplate);
            }
        }

        #endregion

        #region Items
        public ObservableCollection<ITimeLineData> Items
        {
            get { return (ObservableCollection<ITimeLineData>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<ITimeLineData>), typeof(TimeLineControl),
            new UIPropertyMetadata(null,
                new PropertyChangedCallback(OnItemsChanged)));
        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLineControl tc = d as TimeLineControl;
            if (tc != null && tc.bBindInitialize)
            {
                tc.InitializeItems(e.NewValue as ObservableCollection<ITimeLineData>);
                tc.UpdateUnitSize(tc.UnitSize);
                tc._itemsInitialized = true;
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
            DependencyProperty.Register("ViewLevel", typeof(TimeLineViewLevel), typeof(TimeLineControl),
            new UIPropertyMetadata(TimeLineViewLevel.Hours,
                new PropertyChangedCallback(OnViewLevelChanged)));
        private static void OnViewLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLineControl tc = d as TimeLineControl;
            if (tc != null)
            {
                tc.UpdateViewLevel((TimeLineViewLevel)e.NewValue);

            }

        }
        #endregion

        #region unitsize
        public Double UnitSize
        {
            get { return (Double)GetValue(UnitSizeProperty); }
            set { SetValue(UnitSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitSizeProperty =
            DependencyProperty.Register("UnitSize", typeof(Double), typeof(TimeLineControl),
            new UIPropertyMetadata(5.0,
                new PropertyChangedCallback(OnUnitSizeChanged)));
        private static void OnUnitSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLineControl tc = d as TimeLineControl;
            if (tc != null)
            {
                tc._unitSizeInitialized = true;
                tc.UpdateUnitSize((Double)e.NewValue);
            }
        }



        #endregion

        #region start date
        public TimeSpan StartTime
        {
            get { return (TimeSpan)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartDate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register(nameof(StartTime), typeof(TimeSpan), typeof(TimeLineControl),
            new UIPropertyMetadata(TimeSpan.MinValue,
                new PropertyChangedCallback(OnStartTimeChanged)));
        private static void OnStartTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLineControl tc = d as TimeLineControl;
            if (tc != null)
            {
                tc._startTimeInitialized = true;
                tc.ReDrawChildren();
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
            DependencyProperty.Register(nameof(EndTime), typeof(TimeSpan), typeof(TimeLineControl),
            new UIPropertyMetadata(TimeSpan.MinValue,
                new PropertyChangedCallback(OnEndTimeChanged)));
        private static void OnEndTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLineControl tc = d as TimeLineControl;
            if (tc != null)
            {
                tc._startTimeInitialized = true;
                tc.ReDrawChildren();
            }
        }
        #endregion

        #region manipulation mode
        public TimeLineManipulationMode ManipulationMode
        {
            get { return (TimeLineManipulationMode)GetValue(ManipulationModeProperty); }
            set { SetValue(ManipulationModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ManipulationMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ManipulationModeProperty =
            DependencyProperty.Register("ManipulationMode", typeof(TimeLineManipulationMode), typeof(TimeLineControl), new UIPropertyMetadata(TimeLineManipulationMode.Free));
        #endregion

        #region CanLineChange
        public Boolean CanLineChange
        {
            get { return (bool)GetValue(CanLineChangeProperty); }
            set { SetValue(CanLineChangeProperty, value); }
        }

        public static readonly DependencyProperty CanLineChangeProperty =
            DependencyProperty.Register(nameof(CanLineChange), typeof(bool), typeof(TimeLineControl),
            new UIPropertyMetadata(true));

        #endregion

        #endregion

        public TimeLineControl()
        {
            Focusable = true;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;

            //Items = new ObservableCollection<ITimeLineData>();

            DragDrop.AddDragOverHandler(this, TimeLineControl_DragOver);
            DragDrop.AddDropHandler(this, TimeLineControl_Drop);
            DragDrop.AddDragEnterHandler(this, TimeLineControl_DragOver);
            DragDrop.AddDragLeaveHandler(this, TimeLineControL_DragLeave);

            AllowDrop = true;

            _scrollViewer = GetParentScrollViewer();
            Loaded += (s, e) =>
            {              
                InitializeItems(Items);
                ReDrawChildren();
                bBindInitialize = true;
            };
        }

        #region control life cycle events
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            _scrollViewer = GetParentScrollViewer();

        }


        /*
        /// <summary>
        /// I was unable to track down why this control was locking up when
        /// synchronise with siblings is checked and the parent element is closed etc.
        /// I was getting something with a contextswitchdeadblock that I was wracking my
        /// brain trying to figure out.  The problem only happened when a timeline control
        /// with a child timeline item was present.  I could have n empty timeline controls
        /// with no problem.  Adding one timeline item however caused that error when the parent element
        /// is closed etc.
        /// </summary>
        /// <param name="child"></param>
        protected override void ParentLayoutInvalidated(UIElement child)
        {
            //this event fires when something drags over this or when the control is trying to close
            if (child == _tmpDraggAdornerControl)
                return;
            if (!Children.Contains(child))
                return;
            base.ParentLayoutInvalidated(child);
            SynchedWithSiblings = false;
            //Because this layout invalidated became neccessary, I had to then put null checks on all attempts
            //to get a timeline item control.  There appears to be some UI threading going on so that just checking the children count
            //at the begining of the offending methods was not preventing me from crashing.  
            Children.Clear();
        }*/
        #endregion

        #region miscellaneous helpers
        private ScrollViewer GetParentScrollViewer()
        {
            DependencyObject item = VisualTreeHelper.GetParent(this);
            while (item != null)
            {
                String name = "";
                var ctrl = item as Control;
                if (ctrl != null)
                    name = ctrl.Name;
                if (item is ScrollViewer)
                {
                    return item as ScrollViewer;
                }
                item = VisualTreeHelper.GetParent(item);
            }
            return null;
        }

        private void SetTemplate(DataTemplate dataTemplate)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                TimeLineItemControl titem = Children[i] as TimeLineItemControl;
                if (titem != null)
                    titem.ContentTemplate = dataTemplate;
            }
        }

        private void InitializeItems(ObservableCollection<ITimeLineData> observableCollection)
        {
            if (observableCollection == null)
                return;
            this.Children.Clear();

            foreach (ITimeLineData data in observableCollection)
            {
                TimeLineItemControl adder = CreateTimeLineItemControl(data);

                Children.Add(adder);
            }
            Items.CollectionChanged -= Items_CollectionChanged;
            Items.CollectionChanged += Items_CollectionChanged;
        }

        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var itm = e.NewItems[0] as ITimeLineData;
                if (itm.StartTime.HasValue && itm.StartTime.Value == TimeSpan.MinValue)
                {//newly created item isn't a drop in so we need to instantiate and place its control.
                    TimeSpan duration = itm.EndTime.Value.Subtract(itm.StartTime.Value);
                    if (Items.Count == 1)//this is the first one added
                    {
                        itm.StartTime = StartTime;
                        itm.EndTime = StartTime.Add(duration);
                    }
                    else
                    {
                        var last = Items.OrderBy(i => i.StartTime.Value).LastOrDefault();
                        if (last != null)
                        {
                            itm.StartTime = last.EndTime;
                            itm.EndTime = itm.StartTime.Value.Add(duration);
                        }
                    }
                    var ctrl = CreateTimeLineItemControl(itm);
                    //The index if Items.Count-1 because of zero indexing.
                    //however our children is 1 indexed because 0 is our canvas grid.
                    Children.Insert(Items.Count, ctrl);
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                var removeItem = e.OldItems[0];
                for (int i = 1; i < Children.Count; i++)
                {
                    TimeLineItemControl checker = Children[i] as TimeLineItemControl;
                    if (checker != null && checker.DataContext == removeItem)
                    {
                        Children.Remove(checker);
                        break;
                    }
                }
            }
        }

        private TimeLineItemControl CreateTimeLineItemControl(ITimeLineData data)
        {
            Binding startBinding = new Binding("StartTime");
            startBinding.Mode = BindingMode.TwoWay;
            startBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Binding endBinding = new Binding("EndTime");
            endBinding.Mode = BindingMode.TwoWay;
            endBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            TimeSpan timelineStart = StartTime;

            TimeLineItemControl adder = new TimeLineItemControl();
            adder.TimeLineStartTime = timelineStart;
            adder.ViewLevel = ViewLevel;
            adder.DataContext = data;
            adder.Content = data;

            adder.SetBinding(TimeLineItemControl.StartTimeProperty, startBinding);
            adder.SetBinding(TimeLineItemControl.EndTimeProperty, endBinding);
  
            if (ItemTemplateSelector != null)
            {
                adder.ContentTemplate = ItemTemplateSelector.SelectTemplate(data, adder);
            }

            /*adder.PreviewMouseLeftButtonDown += item_PreviewEditButtonDown;
            adder.MouseMove += item_MouseMove;
            adder.PreviewMouseLeftButtonUp += item_PreviewEditButtonUp;*/
            adder.PreviewMouseRightButtonDown += item_PreviewEditButtonDown;
            adder.PreviewMouseRightButtonUp += item_PreviewEditButtonUp;
            adder.MouseMove += item_MouseMove;

            adder.PreviewMouseLeftButtonUp += item_PreviewDragButtonUp;
            adder.PreviewMouseLeftButtonDown += item_PreviewDragButtonDown;
            adder.UnitSize = UnitSize;

            return adder;
        }
        #endregion

        #region updaters fired on dp changes
        private void UpdateUnitSize(double size)
        {
            if (Items == null)
                return;
            for (int i = 0; i < Items.Count; i++)
            {
                TimeLineItemControl titem = GetTimeLineItemControlAt(i);
                if (titem != null)
                    titem.UnitSize = size;
            }
            ReDrawChildren();
        }
        private void UpdateViewLevel(TimeLineViewLevel lvl)
        {
            if (Items == null)
                return;
            for (int i = 0; i < Items.Count; i++)
            {

                var templatedControl = GetTimeLineItemControlAt(i);
                if (templatedControl != null)
                    templatedControl.ViewLevel = lvl;

            }
            ReDrawChildren();
            //Now we go back and have to detect if things have been collapsed
        }

        //TODO: set up the timeline start date dependency property and do this margin check
        //for all including the first one.
        public void ReDrawChildren()
        {
            if (Items == null)
            {
                return;
            }
            TimeSpan start = (TimeSpan)GetValue(StartTimeProperty);
            Double w = 0;
            Double s = 0;
            Double e = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                var mover = GetTimeLineItemControlAt(i);
                if (mover != null)
                {
                    mover.TimeLineStartTime = start;
                    mover.TimelineMaxTime = StartTime < EndTime ? EndTime : TimeSpan.MaxValue;
                    if (!mover.ReadyToDraw)
                        mover.ReadyToDraw = true;
                    mover.PlaceOnCanvas();
                    mover.GetPlacementInfo(ref s, ref w, ref e);
                }

            }
        }
        #endregion

        #region background and grid methods
        internal Double GetMyWidth()
        {
            if (StartTime < EndTime)
                return TimeLineUtils.ConvertTimeToDistance(EndTime - StartTime, ViewLevel, UnitSize);

            if (Items == null)
                return MinWidth;
           
            var lastItem = GetTimeLineItemControlAt(Items.Count - 1);
            if (lastItem == null)
                return MinWidth;

            Double l = 0, w = 0, e = 0;
            lastItem.GetPlacementInfo(ref l, ref w, ref e);
            return Math.Max(MinWidth, e);
        }
        private void SynchronizeSiblings()
        {
            if (!SynchedWithSiblings)
                return;

            var current = VisualTreeHelper.GetParent(this) as FrameworkElement;
            while (current != null && !(current is ItemsControl))
            {
                current = VisualTreeHelper.GetParent(current) as FrameworkElement;
            }

            if (current is ItemsControl)
            {
                var pnl = current as ItemsControl;
                //this is called on updates for all siblings so it could easily
                //end up infinitely looping if each time tried to synch its siblings
                Boolean isSynchInProgress = false;
                //is there a synch instigator
                Double maxWidth = GetMyWidth();

                var siblings = TimeLineUtils.FindAllTimeLineControls(current);

                foreach (var ctrl in siblings)
                {
                    var tcSib = ctrl;
                    if (tcSib != null)
                    {
                        //tcSib.UnitSize = UnitSize; // First need to sync UnitSize 
                        if (tcSib._isSynchInstigator)
                            isSynchInProgress = true;
                        maxWidth = Math.Max(maxWidth, tcSib.GetMyWidth());
                    }
                }

                SynchWidth = maxWidth;
                if (!isSynchInProgress)
                {
                    _isSynchInstigator = true;
                    foreach (var ctrl in siblings)
                    {
                        var tcSib = ctrl as TimeLineControl;
                        if (tcSib != null && tcSib != this)
                        {
                            tcSib.SynchWidth = maxWidth;
                        }
                    }
                }
                _isSynchInstigator = false;
            }
        }

        #endregion

        #region drag events and fields
        private Boolean _dragging = false;
        private Point _dragStartPosition = new Point(double.MinValue, double.MinValue);
        /// <summary>
        /// When we drag something from an external control over this I need a temp control
        /// that lets me adorn those accordingly as well
        /// </summary>
        private TimeLineItemControl _tmpDraggAdornerControl;

        TimeLineItemControl _dragObject = null;
        void item_PreviewDragButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPosition = Mouse.GetPosition(null);
            _dragObject = sender as TimeLineItemControl;

        }

        void item_PreviewDragButtonUp(object sender, MouseButtonEventArgs e)
        {
            _dragStartPosition.X = double.MinValue;
            _dragStartPosition.Y = double.MinValue;
            _dragObject = null;
        }
        void TimeLineControl_DragOver(object sender, DragEventArgs e)
        {
            TimeLineItemControl d = e.Data.GetData(typeof(TimeLineItemControl)) as TimeLineItemControl;
        
            if (d != null)
            {
                if (Manager != null)
                {
                    if (!Manager.CanAddToTimeLine(d.DataContext as ITimeLineData))
                    {
                        e.Effects = DragDropEffects.None;
                        return;
                    }
                }
                e.Effects = DragDropEffects.Move;
                //this is an internal drag or a drag from another time line control
                if (DragAdorner == null)
                {
                    _dragAdorner = new TimeLineDragAdorner(d, d.ContentTemplate);

                }
                DragAdorner.MousePosition = e.GetPosition(d);
                DragAdorner.InvalidateVisual();

            }
            else
            {//GongSolutions.Wpf.DragDrop

                var d2 = e.Data.GetData("GongSolutions.Wpf.DragDrop");
                if (d2 != null)
                {
                    if (Manager != null)
                    {
                        if (!Manager.CanAddToTimeLine(d2 as ITimeLineData))
                        {
                            e.Effects = DragDropEffects.None;
                            return;
                        }
                    }

                    e.Effects = DragDropEffects.Move;
                    if (DragAdorner == null)
                    {
                        //we are dragging from an external source and we don't have a timeline item control of any sort
                        Children.Remove(_tmpDraggAdornerControl);
                        //in order to get an adornment layer the control has to be somewhere
                        _tmpDraggAdornerControl = new TimeLineItemControl();
                        _tmpDraggAdornerControl.UnitSize = UnitSize;
                        Children.Add(_tmpDraggAdornerControl);
                        Canvas.SetLeft(_tmpDraggAdornerControl, -1000000);
                        _tmpDraggAdornerControl.DataContext = d2;
                        _tmpDraggAdornerControl.StartTime = StartTime;
                        _tmpDraggAdornerControl.InitializeDefaultLength();
                        //_tmpDraggAdornerControl.ContentTemplate = ItemTemplateSelector != null ? Itemtempl;

                        //_dragAdorner = new TimeLineDragAdorner(_tmpDraggAdornerControl, ItemTemplate);
                    }
                    DragAdorner.MousePosition = e.GetPosition(_tmpDraggAdornerControl);
                    DragAdorner.InvalidateVisual();
                }
            }
            //DragScroll(e);
        }
        void TimeLineControL_DragLeave(object sender, DragEventArgs e)
        {
            DragAdorner = null;
            Children.Remove(_tmpDraggAdornerControl);
            _tmpDraggAdornerControl = null;
        }
        void TimeLineControl_Drop(object sender, DragEventArgs e)
        {
            DragAdorner = null;

            TimeLineItemControl dropper = e.Data.GetData(typeof(TimeLineItemControl)) as TimeLineItemControl;
            ITimeLineData dropData = null;
            if (dropper == null)
            {
                //dropData = e.Data.GetData(typeof(ITimeLineData)) as ITimeLineData;
                dropData = e.Data.GetData("GongSolutions.Wpf.DragDrop") as ITimeLineData;
                if (dropData != null)
                {
                    //I haven't figured out why but
                    //sometimes when dropping from an external source
                    //the drop event hits twice.
                    //that results in ugly duplicates ending up in the timeline
                    //and it is a mess.
                    if (Items.Contains(dropData))
                        return;
                    //create a new timeline item control from this data
                    dropper = CreateTimeLineItemControl(dropData);
                    dropper.StartTime = StartTime;
                    dropper.InitializeDefaultLength();
                    Children.Remove(_tmpDraggAdornerControl);
                    _tmpDraggAdornerControl = null;

                }
            }
            var dropX = e.GetPosition(this).X;
            int newIndex = GetDroppedNewIndex(dropX);
            var curData = dropper.DataContext as ITimeLineData;
            var curIndex = Items.IndexOf(curData);
            if ((curIndex == newIndex || curIndex + 1 == newIndex) && dropData == null && dropper.Parent == this)//dropdata null is to make sure we aren't failing on adding a new data item into the timeline
            //dropper.parent==this makes it so that we allow a dropper control from another timeline to be inserted in at the start.
            {
                return;//our drag did nothing meaningful so we do nothing.
            }

            if (dropper != null)
            {
                TimeSpan start = (TimeSpan)GetValue(StartTimeProperty);
                if (newIndex == 0)
                {
                    if (dropData == null)
                    {
                        RemoveTimeLineItemControl(dropper);
                    }
                    if (dropper.Parent != this && dropper.Parent is TimeLineControl)
                    {
                        var tlCtrl = dropper.Parent as TimeLineControl;
                        tlCtrl.RemoveTimeLineItemControl(dropper);
                    }
                    InsertTimeLineItemControlAt(newIndex, dropper);
                    dropper.MoveToNewStartTime(start);
                    MakeRoom(newIndex, dropper.Width);


                }
                else//we are moving this after something.
                {

                    //find out if we are moving the existing one back or forward.
                    var placeAfter = GetTimeLineItemControlAt(newIndex - 1);
                    if (placeAfter != null)
                    {
                        start = placeAfter.EndTime;
                        RemoveTimeLineItemControl(dropper);
                        if (curIndex < newIndex && curIndex >= 0)//-1 is on an insert in which case we definitely don't want to take off on our new index value
                        {
                            //we are moving forward.
                            newIndex--;//when we removed our item, we shifted our insert index back 1
                        }
                        if (dropper.Parent != null && dropper.Parent != this)
                        {
                            var ptl = dropper.Parent as TimeLineControl;
                            ptl.RemoveTimeLineItemControl(dropper);
                        }

                        InsertTimeLineItemControlAt(newIndex, dropper);
                        dropper.MoveToNewStartTime(start);
                        MakeRoom(newIndex, dropper.Width);
                    }
                }
            }
            e.Handled = true;
        }


        #region drop helpers
        private void InsertTimeLineItemControlAt(int index, TimeLineItemControl adder)
        {
            var Data = adder.DataContext as ITimeLineData;
            if (Items.Contains(Data))
                return;

            adder.PreviewMouseRightButtonDown -= item_PreviewEditButtonDown;
            adder.MouseMove -= item_MouseMove;
            adder.PreviewMouseRightButtonUp -= item_PreviewEditButtonUp;

            adder.PreviewMouseLeftButtonUp -= item_PreviewDragButtonUp;
            adder.PreviewMouseLeftButtonDown -= item_PreviewDragButtonDown;

            adder.PreviewMouseRightButtonDown += item_PreviewEditButtonDown;
            adder.MouseMove += item_MouseMove;
            adder.PreviewMouseRightButtonUp += item_PreviewEditButtonUp;

            adder.PreviewMouseLeftButtonUp += item_PreviewDragButtonUp;
            adder.PreviewMouseLeftButtonDown += item_PreviewDragButtonDown;

            Children.Insert(index, adder);
            Items.Insert(index, Data);
        }
        private void RemoveTimeLineItemControl(TimeLineItemControl remover)
        {
            var curData = remover.DataContext as ITimeLineData;
            remover.PreviewMouseRightButtonDown -= item_PreviewEditButtonDown;
            remover.MouseMove -= item_MouseMove;
            remover.PreviewMouseRightButtonUp -= item_PreviewEditButtonUp;

            remover.PreviewMouseLeftButtonUp -= item_PreviewDragButtonUp;
            remover.PreviewMouseLeftButtonDown -= item_PreviewDragButtonDown;
            Items.Remove(curData);
            Children.Remove(remover);
        }
        private int GetDroppedNewIndex(Double dropX)
        {
            Double s = 0;
            Double w = 0;
            Double e = 0;
            for (int i = 0; i < Items.Count(); i++)
            {
                var checker = GetTimeLineItemControlAt(i);
                if (checker == null)
                    continue;
                checker.GetPlacementInfo(ref s, ref w, ref e);
                if (dropX < s)
                {
                    return i;
                }
                if (s < dropX && e > dropX)
                {
                    Double distStart = Math.Abs(dropX - s);
                    Double distEnd = Math.Abs(dropX - e);
                    if (distStart < distEnd)//we dropped closer to the start of this item
                    {
                        return i;
                    }
                    //we are closer to the end of this item
                    return i + 1;
                }
                if (e < dropX && i == Items.Count() - 1)
                {
                    return i + 1;
                }
                if (s < dropX && i == Items.Count() - 1)
                {
                    return i;
                }
            }
            return Items.Count;

        }
        private void MakeRoom(int newIndex, Double width)
        {
            int moveIndex = newIndex + 1;
            //get our forward chain and gap
            Double chainGap = 0;

            //because the grid is child 0 and we are essentially indexing as if it wasn't there
            //the child index of add after is our effective index of next
            var nextCtrl = GetTimeLineItemControlAt(moveIndex);
            if (nextCtrl != null)
            {
                Double nL = 0;
                Double nW = 0;
                Double nE = 0;
                nextCtrl.GetPlacementInfo(ref nL, ref nW, ref nE);

                Double droppedIntoSpace = 0;
                if (newIndex == 0)
                {
                    droppedIntoSpace = nL;
                }
                else
                {
                    var previousControl = GetTimeLineItemControlAt(newIndex - 1);
                    if (previousControl != null)
                    {
                        Double aL = 0;
                        Double aW = 0;
                        Double aE = 0;
                        previousControl.GetPlacementInfo(ref aL, ref aW, ref aE);
                        droppedIntoSpace = nL - aE;
                    }
                }
                Double neededSpace = width - droppedIntoSpace;
                if (neededSpace <= 0)
                    return;

                var forwardChain = GetTimeLineForwardChain(nextCtrl, moveIndex + 1, ref chainGap);

                if (chainGap < neededSpace)
                {
                    while (neededSpace > 0)
                    {
                        //move it to the smaller of our values -gap or remaning space
                        Double move = Math.Min(chainGap, neededSpace);
                        foreach (var tictrl in forwardChain)
                        {
                            tictrl.MoveMe(move);
                            neededSpace -= move;
                        }
                        //get our new chain and new gap
                        forwardChain = GetTimeLineForwardChain(nextCtrl, moveIndex + 1, ref chainGap);
                    }
                }
                else
                {
                    foreach (var tictrl in forwardChain)
                    {
                        tictrl.MoveMe(neededSpace);
                    }
                }

            }//if next ctrl is null we are adding to the very end and there is no work to do to make room.
        }
        #endregion

        #endregion


        #region edit events etc
        private Double _curX = 0;
        private TimeLineAction _action;
        void item_PreviewEditButtonUp(object sender, MouseButtonEventArgs e)
        {
            (sender as TimeLineItemControl).ReleaseMouseCapture();
            //Keyboard.Focus(this);
        }

        void item_PreviewEditButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ctrl = sender as TimeLineItemControl;

            _action = ctrl.GetClickAction();
            (sender as TimeLineItemControl).CaptureMouse();
        }



        #region key down and up
        Boolean _rightCtrlDown = false;
        Boolean _leftCtrlDown = false;
        protected void OnKeyDown(Object sender, KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _rightCtrlDown = e.Key == Key.RightCtrl;
                _leftCtrlDown = e.Key == Key.LeftCtrl;
                ManipulationMode = TimeLineManipulationMode.Linked;
            }
        }
        protected void OnKeyUp(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                _leftCtrlDown = false;
            if (e.Key == Key.RightCtrl)
                _rightCtrlDown = false;
            if (!_leftCtrlDown && !_rightCtrlDown)
                ManipulationMode = TimeLineManipulationMode.Linked;
        }

        internal void HandleItemManipulation(TimeLineItemControl ctrl, TimeLineItemChangedEventArgs e)
        {          
            Boolean doStretch = false;
            TimeSpan deltaT = e.DeltaTime;
            TimeSpan zeroT = new TimeSpan();
            int direction = deltaT.CompareTo(zeroT);
            if (direction == 0)
                return;//shouldn't happen
     

            TimeLineItemControl previous = null;
            TimeLineItemControl after = null;
            int afterIndex = -1;
            int previousIndex = -1;
            after = GetTimeLineItemControlStartingAfter(ctrl.StartTime, ref afterIndex);
            previous = GetTimeLineItemControlStartingBefore(ctrl.StartTime, ref previousIndex);
            if (after != null)
                after.ReadyToDraw = false;
            if (ctrl != null)
                ctrl.ReadyToDraw = false;
            Double useDeltaX = e.DeltaX;
            Double cLeft = 0;
            Double cWidth = 0;
            Double cEnd = 0;
            ctrl.GetPlacementInfo(ref cLeft, ref cWidth, ref cEnd);
            switch (e.Action)
            {
                case TimeLineAction.Move:
                    #region move

                    Double chainGap = Double.MaxValue;
                    if (direction > 0)
                    {
                        if (chainGap < useDeltaX)
                            useDeltaX = chainGap;
                        ctrl.MoveMe(useDeltaX);                      
                    }
                    if (direction < 0)
                    {
                        if (-chainGap > useDeltaX)                        
                            useDeltaX = chainGap;
                        ctrl.MoveMe(useDeltaX);;
                    }
                    #endregion
                    break;
                case TimeLineAction.StretchStart:
                    switch (e.Mode)
                    {
                        #region stretchstart

                        case TimeLineManipulationMode.Linked:
                            #region linked
                            Double gap = Double.MaxValue;
                            if (previous != null)
                            {
                                Double pLeft = 0;
                                Double pWidth = 0;
                                Double pEnd = 0;
                                previous.GetPlacementInfo(ref pLeft, ref pWidth, ref pEnd);
                                gap = cLeft - pEnd;
                            }
                            //if (direction < 0 && Math.Abs(gap) < Math.Abs(useDeltaX) && Math.Abs(gap) > _bumpThreshold)//if we are negative and not linked, but about to bump
                            //    useDeltaX = -gap;
                            if (Math.Abs(gap) < _bumpThreshold)
                            {//we are linked
                                if (ctrl.CanDelta(0, useDeltaX) && previous.CanDelta(1, useDeltaX))
                                {
                                    ctrl.MoveStartTime(useDeltaX);
                                    //previous.MoveEndTime(useDeltaX);
                                }
                            }
                            else if (ctrl.CanDelta(0, useDeltaX))
                            {
                                ctrl.MoveStartTime(useDeltaX);
                            }


                            break;
                            #endregion
                        case TimeLineManipulationMode.Free:
                            #region free
                            gap = Double.MaxValue;
                            doStretch = direction > 0;
                            if (direction < 0)
                            {
                                //disallow us from free stretching into another item

                                if (previous != null)
                                {
                                    Double pLeft = 0;
                                    Double pWidth = 0;
                                    Double pEnd = 0;
                                    previous.GetPlacementInfo(ref pLeft, ref pWidth, ref pEnd);
                                    gap = cLeft - pEnd;
                                }
                                else
                                {
                                    //don't allow us to stretch further than the gap between current and start time
                                    TimeSpan s = (TimeSpan)GetValue(StartTimeProperty);
                                    gap = cLeft;
                                }
                                doStretch = gap > _bumpThreshold;
                                if (gap < useDeltaX)
                                {
                                    useDeltaX = gap;
                                }
                            }

                            doStretch &= ctrl.CanDelta(0, useDeltaX);

                            if (doStretch)
                            {
                                ctrl.MoveStartTime(useDeltaX);
                            }
                            #endregion
                            break;
                        default:
                            break;
                        #endregion
                    }
                    break;
                case TimeLineAction.StretchEnd:
                    switch (e.Mode)
                    {
                        #region stretchend
                        case TimeLineManipulationMode.Linked:
                            #region linked
                            Double gap = Double.MaxValue;
                            if (after != null)
                            {
                                Double aLeft = 0;
                                Double aWidth = 0;
                                Double aEnd = 0;
                                after.GetPlacementInfo(ref aLeft, ref aWidth, ref aEnd);
                                gap = aLeft - cEnd;
                            }

                            //if (direction > 0 && gap > _bumpThreshold && gap < useDeltaX)//if we are positive, not linked but about to bump
                            //    useDeltaX = -gap;
                            if (gap < _bumpThreshold)
                            {//we are linked
                                if (ctrl.CanDelta(1, useDeltaX) && after.CanDelta(0, useDeltaX))
                                {
                                    ctrl.MoveEndTime(useDeltaX);
                                    //after.MoveStartTime(useDeltaX);
                                }
                            }
                            else if (ctrl.CanDelta(0, useDeltaX))
                            {
                                ctrl.MoveEndTime(useDeltaX);
                            }
                            break;
                            #endregion
                        case TimeLineManipulationMode.Free:
                            #region free
                            Double nextGap = Double.MaxValue;
                            doStretch = true;
                            if (direction > 0 && after != null)
                            {
                                //disallow us from free stretching into another item
                                Double nLeft = 0;
                                Double nWidth = 0;
                                Double nEnd = 0;
                                after.GetPlacementInfo(ref nLeft, ref nWidth, ref nEnd);
                                nextGap = nLeft - cEnd;
                                doStretch = nextGap > _bumpThreshold;
                                if (nextGap < useDeltaX)
                                    useDeltaX = nextGap;
                            }


                            doStretch &= ctrl.CanDelta(1, useDeltaX);
                            if (doStretch)
                            {
                                ctrl.MoveEndTime(useDeltaX);
                            }

                            break;
                            #endregion
                        default:
                            break;
                        #endregion
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion
        #endregion

        /// <summary>
        /// Mouse move is important for both edit and drag behaviors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void item_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            #region drag - left click and move
            TimeLineItemControl ctrl = sender as TimeLineItemControl;
            if (ctrl == null)
                return;

            if (CanLineChange && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var position = Mouse.GetPosition(null);
                if (Math.Abs(position.X - _dragStartPosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _dragStartPosition.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DragDrop.DoDragDrop(this, ctrl, DragDropEffects.Move | DragDropEffects.Scroll);
                    _dragging = true;
                }

                return;
            }
            #endregion


            #region edits - right click and move
            if (Mouse.Captured != ctrl)
            {
                _curX = Mouse.GetPosition(null).X;
                return;
            }

            Double mouseX = Mouse.GetPosition(null).X;
            Double deltaX = mouseX - _curX;
            if (Math.Abs(deltaX) <= 0)
                return;

            TimeSpan deltaT = ctrl.GetDeltaTime(deltaX);
            var curMode = (TimeLineManipulationMode)GetValue(ManipulationModeProperty);
            HandleItemManipulation(ctrl, new TimeLineItemChangedEventArgs()
            {
                Action = _action,
                DeltaTime = deltaT,
                DeltaX = deltaX,
                Mode = curMode
            });

            _curX = mouseX;

            SynchronizeSiblings();
        
            //When we pressed, this lost focus and we therefore didn't capture any changes to the key status
            //so we check it again after our manipulation finishes.  That way we can be linked and go out of or back into it while dragging
            ManipulationMode = TimeLineManipulationMode.Free;
            _leftCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl);
            _rightCtrlDown = Keyboard.IsKeyDown(Key.RightCtrl);
            if (_leftCtrlDown || _rightCtrlDown)
            {
                ManipulationMode = TimeLineManipulationMode.Linked;
            }
            #endregion
        }



        #region get children methods

        /// <summary>
        /// Returns a list of all timeline controls starting with the current one and moving forward
        /// so long as they are contiguous.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        private List<TimeLineItemControl> GetTimeLineForwardChain(TimeLineItemControl current, int afterIndex, ref Double chainGap)
        {
            List<TimeLineItemControl> returner = new List<TimeLineItemControl>() { current };
            Double left = 0, width = 0, end = 0;
            current.GetPlacementInfo(ref left, ref width, ref end);
            if (afterIndex < 0)
            {
                //we are on the end of the list so there is no limit.
                chainGap = Double.MaxValue;
                return returner;
            }
            Double bumpThreshold = _bumpThreshold;
            Double lastAddedEnd = end;
            while (afterIndex < Items.Count)
            {
                left = width = end = 0;
                var checker = GetTimeLineItemControlAt(afterIndex++);
                if (checker != null)
                {
                    checker.GetPlacementInfo(ref left, ref width, ref end);
                    Double gap = left - lastAddedEnd;
                    if (gap > bumpThreshold)
                    {
                        chainGap = gap;
                        return returner;
                    }
                    returner.Add(checker);
                    lastAddedEnd = end;
                }

            }
            //we have chained off to the end and thus have no need to worry about our gap
            chainGap = Double.MaxValue;
            return returner;
        }

        /// <summary>
        /// Returns a list of all timeline controls starting with the current one and moving backwoards
        /// so long as they are contiguous.  If the chain reaches back to the start time of the timeline then the
        /// ChainsBackToStart boolean is modified to reflect that.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        private List<TimeLineItemControl> GetTimeLineBackwardsChain(TimeLineItemControl current, int prevIndex, ref Boolean ChainsBackToStart, ref Double chainGap)
        {

            List<TimeLineItemControl> returner = new List<TimeLineItemControl>() { current };
            Double left = 0, width = 0, end = 0;
            current.GetPlacementInfo(ref left, ref width, ref end);
            if (prevIndex < 0)
            {
                chainGap = Double.MaxValue;
                ChainsBackToStart = left == 0;
                return returner;
            }

            Double lastAddedLeft = left;
            while (prevIndex >= 0)
            {
                left = width = end = 0;

                var checker = GetTimeLineItemControlAt(prevIndex--);
                if (checker != null)
                {
                    checker.GetPlacementInfo(ref left, ref width, ref end);
                    if (lastAddedLeft - end > _bumpThreshold)
                    {
                        //our chain just broke;
                        chainGap = lastAddedLeft - end;
                        ChainsBackToStart = lastAddedLeft == 0;
                        return returner;
                    }
                    returner.Add(checker);
                    lastAddedLeft = left;
                }

            }
            ChainsBackToStart = lastAddedLeft == 0;
            chainGap = lastAddedLeft;//gap between us and zero;
            return returner;

        }

        private TimeLineItemControl GetTimeLineItemControlStartingBefore(TimeSpan TimeSpan, ref int index)
        {
            index = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                var checker = GetTimeLineItemControlAt(i);
                if (checker != null && checker.StartTime == TimeSpan && i != 0)
                {
                    index = i - 1;
                    return GetTimeLineItemControlAt(i - 1);
                }
            }
            index = -1;
            return null;
        }

        private TimeLineItemControl GetTimeLineItemControlStartingAfter(TimeSpan TimeSpan, ref int index)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var checker = GetTimeLineItemControlAt(i);
                if (checker != null && checker.StartTime > TimeSpan)
                {
                    index = i;
                    return checker;
                }
            }
            index = -1;
            return null;
        }

        private TimeLineItemControl GetTimeLineItemControlAt(int i)
        {
            if (i < 0 || i >= Children.Count)
                return null;
            return Children[i] as TimeLineItemControl;
        }

        #endregion



    }
}
