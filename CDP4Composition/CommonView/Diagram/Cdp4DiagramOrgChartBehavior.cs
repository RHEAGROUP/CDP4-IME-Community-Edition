﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cdp4DiagramOrgChartBehavior.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    
    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;
    
    using EventAggregator;
    
    using Point = System.Windows.Point;

    /// <summary>
    /// Allows proper callbacks on the 
    /// </summary>
    public class Cdp4DiagramOrgChartBehavior : DiagramOrgChartBehavior
    {
        /// <summary>
        /// The name of the data format used for drag-n-drop operations
        /// </summary>
        public static readonly DataFormat DataFormat = DataFormats.GetDataFormat("CDP4.DragDrop");

        /// <summary>
        /// The <see cref="IDragInfo"/> object that contains information about the drag operation.
        /// </summary>
        private IDragInfo dragInfo;

        /// <summary>
        /// The <see cref="IDiagramDropInfo"/> that contains information about the drop operation.
        /// </summary>
        private IDiagramDropInfo dropInfo;

        /// <summary>
        /// A value indicating whether a drag operation is currently in progress
        /// </summary>
        private bool dragInProgress;

        #region DependencyProperties
        /// <summary>
        /// The dependency property that allows setting the source to the view-model representing a diagram object
        /// </summary>
        public static readonly DependencyProperty DiagramObjectSourceProperty = DependencyProperty.Register("DiagramObjectSource", typeof(INotifyCollectionChanged), typeof(Cdp4DiagramOrgChartBehavior), new FrameworkPropertyMetadata(DiagramObjectSourceChanged));

        /// <summary>
        /// The dependency property that allows setting the source to the view-model representing a diagram object
        /// </summary>
        public static readonly DependencyProperty DiagramConnectorSourceProperty = DependencyProperty.Register("DiagramConnectorSource", typeof(INotifyCollectionChanged), typeof(Cdp4DiagramOrgChartBehavior), new FrameworkPropertyMetadata(DiagramConnectorSourceChanged));

        /// <summary>
        /// The dependency property that allows setting the <see cref="IEventPublisher"/>
        /// </summary>
        public static readonly DependencyProperty EventPublisherProperty = DependencyProperty.Register("EventPublisher", typeof(IEventPublisher), typeof(Cdp4DiagramOrgChartBehavior));
        #endregion

        /// <summary>
        /// Initializes static members of the <see cref="Cdp4DiagramOrgChartBehavior"/> class.
        /// </summary>
        static Cdp4DiagramOrgChartBehavior()
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="INotifyCollectionChanged"/> containing the view-momdel for the <see cref="DiagramContentItem"/>
        /// </summary>
        public INotifyCollectionChanged DiagramObjectSource
        {
            get { return (INotifyCollectionChanged)this.GetValue(DiagramObjectSourceProperty); }
            set { this.SetValue(DiagramObjectSourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="INotifyCollectionChanged"/> containing the view-model for the <see cref="DiagramConnector"/>
        /// </summary>
        public INotifyCollectionChanged DiagramConnectorSource
        {
            get { return (INotifyCollectionChanged)this.GetValue(DiagramConnectorSourceProperty); }
            set { this.SetValue(DiagramConnectorSourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEventPublisher"/>
        /// </summary>
        public IEventPublisher EventPublisher
        {
            get { return (IEventPublisher)this.GetValue(EventPublisherProperty); }
            set { this.SetValue(EventPublisherProperty, value); }
        }

        /// <summary>
        /// Called when the <see cref="DiagramObjectSource"/> is changed
        /// </summary>
        /// <param name="caller">The source of the call.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void DiagramObjectSourceChanged(DependencyObject caller, DependencyPropertyChangedEventArgs e)
        {
            var diagramBehavior = (Cdp4DiagramOrgChartBehavior)caller;
            diagramBehavior.InitializeDiagramObjectSource(e.OldValue, e.NewValue);

            var oldlist = e.OldValue as IList;
            var newlist = e.NewValue as IList;
            diagramBehavior.ComputeOldNewDiagramObject(oldlist, newlist);
        }

        /// <summary>
        /// Called when the <see cref="DiagramConnectorSource"/> is changed
        /// </summary>
        /// <param name="caller">The source of the call.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void DiagramConnectorSourceChanged(DependencyObject caller, DependencyPropertyChangedEventArgs e)
        {
            var diagramBehavior = (Cdp4DiagramOrgChartBehavior)caller;
            diagramBehavior.InitializeConnectorSourceChanged(e.OldValue, e.NewValue);

            var oldlist = e.OldValue as IList;
            var newlist = e.NewValue as IList;
            diagramBehavior.ComputeOldNewDiagramConnector(oldlist, newlist);
        }

        /// <summary>
        /// Initialize the Event-Handler to subscribe on changes on the Diagram-object source collection
        /// </summary>
        /// <param name="oldValue">The old collection</param>
        /// <param name="newValue">The new collection</param>
        /// <exception cref="InvalidOperationException">If the reference is changed</exception>
        private void InitializeDiagramObjectSource(object oldValue, object newValue)
        {
            var oldList = oldValue as INotifyCollectionChanged;
            var newList = newValue as INotifyCollectionChanged;
            if (oldList == null && newList != null)
            {
                newList.CollectionChanged += this.OnDiagramObjectSourceCollectionChanged;
            }

            if (oldList != null && newList != null)
            {
                throw new InvalidOperationException("The reference to the collection should not be changed");
            }
        }

        /// <summary>
        /// Initialize the Event-Handler to subscribe on changes on the Diagram-connector source collection
        /// </summary>
        /// <param name="oldValue">The old collection</param>
        /// <param name="newValue">The new collection</param>
        /// <exception cref="InvalidOperationException">If the reference is changed</exception>
        private void InitializeConnectorSourceChanged(object oldValue, object newValue)
        {
            var oldList = oldValue as INotifyCollectionChanged;
            var newList = newValue as INotifyCollectionChanged;
            
            if (oldList == null && newList != null)
            {
                newList.CollectionChanged += this.OnDiagramConnectorSourceCollectionChanged;
            }

            if (oldList != null && newList != null)
            {
                throw new InvalidOperationException("The reference to the collection should not be changed");
            }
        }

        /// <summary>
        /// Add or Remove associated views 
        /// </summary>
        /// <param name="sender">The caller</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/></param>
        private void OnDiagramConnectorSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.ComputeOldNewDiagramConnector(e.OldItems, e.NewItems);
        }

        private void ComputeOldNewDiagramConnector(IList oldValue, IList newValue)
        {
            if (oldValue != null)
            {
                foreach (IDiagramConnectorViewModel item in oldValue)
                {
                    var diagramObj = this.AssociatedObject.Items.SingleOrDefault(x => x.DataContext == item);
                    if (diagramObj != null)
                    {
                        this.AssociatedObject.Items.Remove(diagramObj);
                    }
                }
            }

            if (newValue != null)
            {
                foreach (IDiagramConnectorViewModel item in newValue)
                {
                    var diagramObj = new Cdp4DiagramConnector(item, this);
                    this.AssociatedObject.Items.Add(diagramObj);
                }
            }
        }

        /// <summary>
        /// Add or Remove associated views 
        /// </summary>
        /// <param name="sender">The caller</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/></param>
        private void OnDiagramObjectSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.ComputeOldNewDiagramObject(e.OldItems, e.NewItems);
        }

        /// <summary>
        /// Compute the Diagram-objects
        /// </summary>
        /// <param name="oldValue">The collection containing the old objects</param>
        /// <param name="newValue">The collection containing the new objects</param>
        private void ComputeOldNewDiagramObject(IList oldValue, IList newValue)
        {
            if (oldValue != null)
            {
                foreach (IDiagramObjectViewModel item in oldValue)
                {
                    var diagramObj = this.AssociatedObject.Items.SingleOrDefault(x => x.DataContext == item);
                    if (diagramObj != null)
                    {
                        this.AssociatedObject.Items.Remove(diagramObj);
                    }
                }
            }

            if (newValue != null)
            {
                foreach (IDiagramObjectViewModel item in newValue)
                {
                    var diagramObj = new Cdp4DiagramContentItem(item, this);
                    this.AssociatedObject.Items.Add(diagramObj);
                }
            }
        }

        /// <summary>
        /// Reinitializes the viewmodel selection collection when the control collection changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The arguments.</param>
        public void OnControlSelectionChanged(object sender, EventArgs e)
        {
            this.EventPublisher.Publish(new DiagramSelectEvent(this.AssociatedObject.SelectedItems.Select(x => (DiagramContentItem)x)));
        }
        
        /// <summary>
        /// The event-handler for the <see cref="DiagramControl.Items"/> collection change
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/></param>
        private void OnControlCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // add not processed, a view component shall not be added without the component
            var oldlist = e.OldItems;
            if (oldlist == null)
            {
                return;
            }

            foreach (var oldItem in oldlist)
            {
                var diagramItem = oldItem as DiagramItem;
                if (diagramItem != null)
                {
                    this.EventPublisher.Publish(new DiagramDeleteEvent((IRowViewModelBase<Thing>)diagramItem.DataContext));
                }
            }
        }

        /// <summary>
        /// The on attached event handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Items.CollectionChanged += this.OnControlCollectionChanged;
            this.AssociatedObject.SelectionChanged += this.OnControlSelectionChanged;

            this.AssociatedObject.PreviewMouseLeftButtonDown += this.PreviewMouseLeftButtonDown;
            this.AssociatedObject.PreviewMouseLeftButtonUp += this.PreviewMouseLeftButtonUp;
            this.AssociatedObject.PreviewMouseMove += this.PreviewMouseMove;

            this.AssociatedObject.AllowDrop = true;
            this.AssociatedObject.PreviewDragEnter += this.PreviewDragEnter;
            this.AssociatedObject.PreviewDragOver += this.PreviewDragOver;
            this.AssociatedObject.PreviewDragLeave += this.PreviewDragLeave;
            this.AssociatedObject.PreviewDrop += this.PreviewDrop;
        }

        /// <summary>
        /// Unsubscribes eventhandlers when detaching.
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.Items.CollectionChanged -= this.OnControlCollectionChanged;
            this.AssociatedObject.SelectionChanged -= this.OnControlSelectionChanged;

            this.AssociatedObject.PreviewMouseLeftButtonDown -= this.PreviewMouseLeftButtonDown;
            this.AssociatedObject.PreviewMouseLeftButtonUp -= this.PreviewMouseLeftButtonUp;
            this.AssociatedObject.PreviewMouseMove -= this.PreviewMouseMove;

            this.AssociatedObject.PreviewDragEnter -= this.PreviewDragEnter;
            this.AssociatedObject.PreviewDragOver -= this.PreviewDragOver;
            this.AssociatedObject.PreviewDragLeave -= this.PreviewDragLeave;
            this.AssociatedObject.PreviewDrop -= this.PreviewDrop;

            base.OnDetaching();
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewMouseLeftButtonDown"/> event
        /// </summary>
        /// <remarks>
        /// Occurs when the left mouse button is pressed while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs"/> associated to the event</param>
        private void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 1 || VisualTreeExtensions.HitTestScrollBar(sender, e)
                                  || VisualTreeExtensions.HitTestGridColumnHeader(sender, e))
            {
                this.dragInfo = null;
                return;
            }

            this.dragInfo = new DragInfo(sender, e);
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewMouseLeftButtonUp"/> event
        /// </summary>
        /// <remarks>
        /// Occurs when the left mouse button is released while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs"/> associated to the event</param>
        private void PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.dragInfo = null;
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewMouseMove"/> event
        /// </summary>
        /// <remarks>
        /// Occurs when the mouse pointer moves while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs"/> associated to the event</param>
        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (this.dragInfo != null && !this.dragInProgress)
            {
                var dragStart = this.dragInfo.DragStartPosition;
                var position = e.GetPosition(null);

                if (Math.Abs(position.X - dragStart.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(position.Y - dragStart.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var dragSource = this.AssociatedObject.DataContext as IDragSource;
                    if (dragSource != null)
                    {
                        dragSource.StartDrag(this.dragInfo);

                        if (this.dragInfo.Effects != DragDropEffects.None && this.dragInfo.Payload != null)
                        {
                            var data = new DataObject(DataFormat.Name, this.dragInfo.Payload);
                            try
                            {
                                this.dragInProgress = true;
                                DragDrop.DoDragDrop(this.AssociatedObject, data, this.dragInfo.Effects);
                            }
                            finally
                            {
                                this.dragInProgress = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewDragEnter"/> event
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="DragEventArgs"/> associated to the event</param>
        /// <remarks>
        /// Occurs when the input system reports an underlying drag event with this element as the drag target.
        /// </remarks>
        private void PreviewDragEnter(object sender, DragEventArgs e)
        {
            this.PreviewDragOver(sender, e);
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewDragOver"/> event
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="DragEventArgs"/> associated to the event</param>
        /// <remarks>
        /// Occurs when the input system reports an underlying drag event with this element as the potential drop target.
        /// </remarks>
        private void PreviewDragOver(object sender, DragEventArgs e)
        {
            this.dropInfo = new DiagramDropInfo(sender, e);

            var dropTarget = this.AssociatedObject.DataContext as IDropTarget;
            if (dropTarget != null)
            {
                dropTarget.DragOver(this.dropInfo);

                e.Effects = this.dropInfo.Effects;
                e.Handled = true;
            }

            var dependencyObject = sender as DependencyObject;
            if (dependencyObject != null)
            {
                this.Scroll(dependencyObject, e);
            }
        }

        /// <summary>
        /// Scrolls the target of the drop.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> that needs to be scrolled.
        /// </param>
        /// <param name="e">
        /// The drag event.
        /// </param>
        /// <remarks>
        /// Any <see cref="DependencyObject"/> can be a target.
        /// </remarks>
        private void Scroll(DependencyObject dependencyObject, DragEventArgs e)
        {
            var scrollViewer = dependencyObject.GetVisualDescendant<ScrollViewer>();

            if (scrollViewer != null)
            {
                var position = e.GetPosition(scrollViewer);
                var scrollMargin = Math.Min(scrollViewer.FontSize * 2, scrollViewer.ActualHeight / 2);

                if (position.X >= scrollViewer.ActualWidth - scrollMargin &&
                    scrollViewer.HorizontalOffset < scrollViewer.ExtentWidth - scrollViewer.ViewportWidth)
                {
                    scrollViewer.LineRight();
                }
                else if (position.X < scrollMargin && scrollViewer.HorizontalOffset > 0)
                {
                    scrollViewer.LineLeft();
                }
                else if (position.Y >= scrollViewer.ActualHeight - scrollMargin &&
                         scrollViewer.VerticalOffset < scrollViewer.ExtentHeight - scrollViewer.ViewportHeight)
                {
                    scrollViewer.LineDown();
                }
                else if (position.Y < scrollMargin && scrollViewer.VerticalOffset > 0)
                {
                    scrollViewer.LineUp();
                }
            }
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewDragLeave"/> event
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="DragEventArgs"/> associated to the event</param>
        /// <remarks>
        /// Occurs when the input system reports an underlying drag event with this element as the drag origin.
        /// </remarks>
        private void PreviewDragLeave(object sender, DragEventArgs e)
        {
            this.dropInfo = null;
            e.Handled = true;
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewDrop"/> event
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="DragEventArgs"/> associated to the event</param>
        /// <remarks>
        /// Occurs when the input system reports an underlying drop event with this element as the drop target.
        /// </remarks>
        private void PreviewDrop(object sender, DragEventArgs e)
        {
            this.dropInfo = new DiagramDropInfo(sender, e)
            {
                DiagramDropPoint = this.GetDiagramPositionFromMousePosition(this.dropInfo.DropPosition)
            };

            if (this.AssociatedObject.DataContext is IDropTarget dropTarget)
            {
                dropTarget.Drop(this.dropInfo);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Converts control coordinates into document coordinates.
        /// </summary>
        /// <param name="dropPosition">The control <see cref="System.Windows.Point"/> where the drop occurs.</param>
        /// <returns>The document drop position.</returns>
        private Point GetDiagramPositionFromMousePosition(Point dropPosition)
        {
            return this.AssociatedObject.PointToDocument(dropPosition);
        }

        /// <summary>
        /// Activates a new <see cref="ConnectorTool"/> in the Diagram control.
        /// </summary>
        public void ActivateConnectorTool()
        {
            this.AssociatedObject.ActiveTool = new ConnectorTool();
        }

        /// <summary>
        /// Resets the active tool.
        /// </summary>
        public void ResetTool()
        {
            this.AssociatedObject.ActiveTool = null;
        }
    }
}
