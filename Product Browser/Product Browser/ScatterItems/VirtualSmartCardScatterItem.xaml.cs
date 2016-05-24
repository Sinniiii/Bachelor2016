﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;
using DatabaseModel;
using DatabaseModel.Model;
using System.Data.Entity;
using System.Linq;
using System.Windows.Threading;
using System.IO;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// Interaction logic for TagWindow.xaml
    /// </summary>
    public partial class VirtualSmartCardScatterItem : ABBScatterItem
    {

        #region Physics, position and graphics constants

        // Radius of gravity circle that pulls objects in
        readonly double
            CIRCLE_SIZE = 150d;

        readonly int MAX_IMAGES_BEFORE_CONTAINER = 1;

        readonly Size
            SCATTERITEM_DOCUMENT_STARTING_SIZE = new Size(150, 175),
            SCATTERITEM_VIDEO_STARTING_SIZE = new Size(200, 125),
            SCATTERITEM_IMAGE_STARTING_SIZE = new Size(200, 125);

        readonly Vector
            SCATTERITEM_DOCUMENT_STARTING_POSITION = new Vector(0, -180),
            SCATTERITEM_VIDEO_STARTING_POSITION = new Vector(-225, 0),
            SCATTERITEM_IMAGE_STARTING_POSITION = new Vector(225, 0),

            SCATTERITEM_DOCUMENT_POSITION_OFFSET = new Vector(0, -20),
            SCATTERITEM_VIDEO_POSITION_OFFSET = new Vector(-20, 0),
            SCATTERITEM_IMAGE_POSITION_OFFSET = new Vector(20, 0);

        readonly double
            SCATTERITEM_DOCUMENT_STARTING_ROTATION = 0d,
            SCATTERITEM_VIDEO_STARTING_ROTATION = 0d,
            SCATTERITEM_IMAGE_STARTING_ROTATION = 0d;

        readonly double
            ANIMATION_PULSE_1_STARTING_OPACITY = 0.05d,
            ANIMATION_PULSE_1_OPACITY_CHANGE = 0.01d,
            ANIMATION_PULSE_1_MAX_OPACITY = 0.65,
            ANIMATION_PULSE_1_MIN_OPACITY = 0.05d,

            ANIMATION_PULSE_2_STARTING_OPACITY = 0.05d,
            ANIMATION_PULSE_2_OPACITY_CHANGE = 0.01d,
            ANIMATION_PULSE_2_MAX_OPACITY = 0.65,
            ANIMATION_PULSE_2_MIN_OPACITY = 0.05d;


        #endregion

        #region Fields

        SmartCard smartCard = null;

        ObservableCollection<ABBScatterItem> view;

        List<ABBScatterItem> 
            physicsItemsActive = new List<ABBScatterItem>(10),
            physicsItemsActiveLowPriority = new List<ABBScatterItem>(10),
            physicsItemsInactive = new List<ABBScatterItem>(10);

        DispatcherTimer physicsTimer,
                        physicsTimerLowPriority,
                        animationPulseTimer;

        bool pulseUp1 = true, pulseUp2 = true;

        /// <summary>
        /// These are helpers to determine visibility of UI elements in TagWindow.xaml. Start by displaying loading
        /// </summary>
        bool foundSmartCard = false,
            notFoundSmartCard = false,
            loadingSmartCard = true;

        #endregion

        #region Properties

        public long TagId { get; private set; }

        private string _smartCardName = "";
        public string SmartCardName
        {
            get { return _smartCardName; }
            set
            {
                _smartCardName = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// These are states, setting one to true disables the others
        /// </summary>
        public bool FoundSmartCard { get { return foundSmartCard; }
            set
            {
                if (!value)
                    return;

                foundSmartCard = value;
                loadingSmartCard = !value;
                notFoundSmartCard = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("NotFoundSmartCard");
                NotifyPropertyChanged("LoadingSmartCard");
            }
        }

        public bool NotFoundSmartCard { get { return notFoundSmartCard; }
            set {
                if (!value)
                    return;

                notFoundSmartCard = value;
                foundSmartCard = !value;
                loadingSmartCard = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("LoadingSmartCard");
                NotifyPropertyChanged("FoundSmartCard");
            }
        }

        public bool LoadingSmartCard { get { return loadingSmartCard; }
            set
            {
                if (!value)
                    return;

                loadingSmartCard = value;
                notFoundSmartCard = !value;
                foundSmartCard = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("NotFoundSmartCard");
                NotifyPropertyChanged("FoundSmartCard");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Captures movement of this scatteritem, to notify connected cards
        /// </summary>
        /// <param name="e"></param>
        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            Moved();
        }

        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            base.OnManipulationCompleted(e);
            
            if (ActualCenter.Y < Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Height * 0.05d
                || ActualCenter.Y > Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Height * 0.95d
                || ActualCenter.X < 0
                || ActualCenter.X > Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Width)
                Die();
        }

        private void ScatterViewItemMovedHandler(object sender, EventArgs args)
        {
            if (!physicsTimerLowPriority.IsEnabled)
                physicsTimerLowPriority.Start();

            if (physicsItemsInactive.Contains(sender)) // If it's null, it already exists in the active container so no need to do anything
            {
                ABBScatterItem item = (ABBScatterItem)sender;

                item.Deceleration = 0.001536d;

                physicsItemsInactive.Remove(item);
                physicsItemsActiveLowPriority.Add(item);

                // Add all inactive to active
                physicsItemsActive.AddRange(physicsItemsInactive);
                
                // Clear inactive
                physicsItemsInactive.Clear();

                // Calculate new positions for all of them
                CalculateNewPositions(physicsItemsActive);

                // Start timer if not running
                if (!physicsTimer.IsEnabled)
                    physicsTimer.Start();
            }
        }

        private void PhysicsEventHandler(object sender, EventArgs args)
        {
            List<ABBScatterItem> toDiscard = new List<ABBScatterItem>(physicsItemsActive.Count);

            for (int i = 0; i < physicsItemsActive.Count; i++)
                switch (physicsItemsActive[i].RunHighPriority(ActualCenter, ActualOrientation))
                {
                    case RunState.LowPriority:
                        ScatterViewItemLowPriority(physicsItemsActive[i]);
                        toDiscard.Add(physicsItemsActive[i]);
                        break;
                    case RunState.Locked:
                        ScatterViewItemLocked(physicsItemsActive[i]);
                        toDiscard.Add(physicsItemsActive[i]);
                        break;
                    default:
                        break;
                }

            physicsItemsActive.RemoveAll(a => toDiscard.Contains(a));

            if (physicsItemsActive.Count == 0)
                physicsTimer.Stop();
        }

        private void PhysicsLowPriorityEventHandler(object sender, EventArgs args)
        {
            List<ABBScatterItem> toDiscard = new List<ABBScatterItem>(physicsItemsActive.Count);

            for (int i = 0; i < physicsItemsActiveLowPriority.Count; i++)
                switch (physicsItemsActiveLowPriority[i].RunLowPriority(ActualCenter, ActualOrientation, CIRCLE_SIZE))
                {
                    case RunState.HighPriority:
                        ScatterViewItemHighPriority(physicsItemsActiveLowPriority[i]);
                        toDiscard.Add(physicsItemsActiveLowPriority[i]);
                        break;
                    default:
                        break;
                }

            physicsItemsActiveLowPriority.RemoveAll(a => toDiscard.Contains(a));

            if (physicsItemsActiveLowPriority.Count == 0)
                physicsTimerLowPriority.Stop();
        }

        private void PhysicsDieEventHandler(object sender, EventArgs args)
        {
            Opacity -= 0.05d;

            foreach (ABBScatterItem item in physicsItemsActive)
            {
                item.RunHighPriority(ActualCenter, ActualOrientation);
                item.Opacity -= 0.05d;
            }

            if(Opacity <= 0)
            {
                physicsTimer.Stop();
                Dead();
            }
        }

        public override void AnimationPulseHandler(object sender, EventArgs args)
        {
            grad.Angle = (grad.Angle + 0.5d) % 360d;

            if (pulseUp1)
            {
                animationPulse1.Opacity += ANIMATION_PULSE_1_OPACITY_CHANGE;

                if (animationPulse1.Opacity >= ANIMATION_PULSE_1_MAX_OPACITY)
                    pulseUp1 = false;
            }
            else
            {
                animationPulse1.Opacity -= ANIMATION_PULSE_1_OPACITY_CHANGE;

                if (animationPulse1.Opacity <= ANIMATION_PULSE_1_MIN_OPACITY)
                    pulseUp1 = true;
            }

            if (pulseUp2)
            {
                animationPulse2.Opacity += ANIMATION_PULSE_2_OPACITY_CHANGE;

                if (animationPulse2.Opacity >= ANIMATION_PULSE_2_MAX_OPACITY)
                    pulseUp2 = false;
            }
            else
            {
                animationPulse2.Opacity -= ANIMATION_PULSE_2_OPACITY_CHANGE;

                if (animationPulse2.Opacity <= ANIMATION_PULSE_2_MIN_OPACITY)
                    pulseUp2 = true;
            }
        }

        #endregion

        #region Methods

        public void Moved()
        {
            // Instant
            foreach (ABBScatterItem item in physicsItemsInactive)
                item.MoveToOriginalPosition(ActualCenter, ActualOrientation);
        }

        private void Die()
        {
            physicsTimer.Tick -= PhysicsEventHandler;
            physicsTimer.Tick += PhysicsDieEventHandler;

            if (!physicsTimer.IsEnabled)
                physicsTimer.Start();

            if (physicsTimerLowPriority.IsEnabled)
                physicsTimerLowPriority.Stop();

            physicsItemsActive.AddRange(physicsItemsActiveLowPriority);
            physicsItemsActive.AddRange(physicsItemsInactive);

            physicsItemsInactive.Clear();
            physicsItemsActiveLowPriority.Clear();

            this.Deleting = true;

            foreach(ABBScatterItem item in physicsItemsActive)
            {
                item.Deleting = true;

                if (item is VideoScatterItem)
                    (item as VideoScatterItem).KillVideo();
            }
        }

        private void Dead()
        {
            view.Remove(this);

            foreach (ABBScatterItem item in physicsItemsActive)
                view.Remove(item);

            animationPulseTimer.Stop();
            physicsTimer.Stop();
            physicsTimerLowPriority.Stop();
        }

        private void ScatterViewItemLocked(ABBScatterItem e)
        {
            physicsItemsInactive.Add(e);

            if (e is VideoScatterItem)
                (e as VideoScatterItem).PauseVideo();

            if (physicsItemsActive.Count == 0)
                physicsTimer.Stop();
        }

        private void ScatterViewItemLowPriority(ABBScatterItem e)
        {
            if (!physicsTimerLowPriority.IsEnabled)
                physicsTimerLowPriority.Start();

            e.Deceleration = 0.001536d;

            physicsItemsActiveLowPriority.Add(e);
        }

        private void ScatterViewItemHighPriority(ABBScatterItem e)
        {
            if (!physicsTimer.IsEnabled)
                physicsTimer.Start();

            e.Deceleration = double.NaN; // Prevent inertia

            // Add all inactive to active
            physicsItemsActive.AddRange(physicsItemsInactive);

            // And add this one
            physicsItemsActive.Add(e);
            // Then clear inactive
            physicsItemsInactive.Clear();

            // Calculate new positions for all active items
            CalculateNewPositions(physicsItemsActive);
        }

        private void CalculateNewPositions(List<ABBScatterItem> items)
        {
            var documents = items.Where(a => a is DocumentScatterItem).ToList();
            var images = items.Where(a => a is ImageScatterItem || a is ImageContainerScatterItem).ToList();
            var videos = items.Where(a => a is VideoScatterItem).ToList();

            for (int i = 0; i < documents.Count; i++)
            {
                documents[i].OriginalPositionOffset = (Point)(SCATTERITEM_DOCUMENT_STARTING_POSITION + SCATTERITEM_DOCUMENT_POSITION_OFFSET * i);
                documents[i].ZIndex = i;
            }

            for (int i = 0; i < images.Count; i++)
            {
                images[i].OriginalPositionOffset = (Point)(SCATTERITEM_IMAGE_STARTING_POSITION + SCATTERITEM_IMAGE_POSITION_OFFSET * i);
                images[i].ZIndex = i;
            }

            for (int i = 0; i < videos.Count; i++)
            {
                videos[i].OriginalPositionOffset = (Point)(SCATTERITEM_VIDEO_STARTING_POSITION + SCATTERITEM_VIDEO_POSITION_OFFSET * i);
                videos[i].ZIndex = videos.Count - 1 - i;
            }

            // Ensure this is below the others
            this.ZIndex = -1;
        }

        /// <summary>
        /// Initializes this SmartCard by getting data from database and creating child elements. Should only be called
        /// after this SmartCardScatterViewItem has been added to the ScattterView visual tree(use UpdateLayout on scatterview)
        /// </summary>
        /// <param name="view"></param>
        public async void InitializeVirtualSmartCard(Color colorTheme)
        {
            // Remove shadow of this smartcard, or we get an ugly effect
            var ssc = this.GetTemplateChild("shadow") as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
            ssc.Visibility = Visibility.Hidden;

            ABBDataContext context = new ABBDataContext();

            // Use Task.Run instead of extremely slow EF6.0 async functions when using varchar
            smartCard = await Task<SmartCard>.Run( 
                () =>
                {
                    return context.SmartCards
                    .Where(a => a.TagId == TagId)
                    .Include(s => s.DataItems.Select(d => d.DataField))
                    .Include(s => s.CardImage.DataField)
                    .FirstOrDefault();
                });

            List<SmartCardDataItem> dataItems = null;

            GradientColor = colorTheme;

            if (smartCard == null || (dataItems = smartCard.DataItems.ToList()) == null || dataItems.Count == 0)
            {
                NotFoundSmartCard = true;
                return;
            }

            FoundSmartCard = true;
            SmartCardName = smartCard.Name;

            // If we have a card image, use that for the center object
            if(smartCard.CardImage != null)
            {
                Image cardImage = new Image();
                cardImage.Source = smartCard.CardImage.GetImageSource();
                cardImage.Stretch = System.Windows.Media.Stretch.Fill;
                centerObject.Content = cardImage;

                foundControl.Visibility = Visibility.Hidden;
            }

            List<ABBScatterItem> scatterItems = new List<ABBScatterItem>();

            // Place all images inside one scatter item if > max images, else individual scatter items
            dataItems = smartCard.DataItems.Where(a => a.Category == SmartCardDataItemCategory.Image).ToList();
            if(dataItems.Count > MAX_IMAGES_BEFORE_CONTAINER)
            {
                ABBScatterItem item = new ImageContainerScatterItem(dataItems);
                scatterItems.Add(item);
            }
            else
            {
                foreach(SmartCardDataItem dataItem in dataItems)
                {
                    ABBScatterItem item = new ImageScatterItem(dataItem);
                    scatterItems.Add(item);
                }
            }

            // One scatter item each for everything else
            dataItems = smartCard.DataItems.Where(a => a.Category != SmartCardDataItemCategory.Image).ToList();
            for (int i = 0; i < dataItems.Count; i++)
            {
                ABBScatterItem item = null;
                switch (dataItems[i].Category)
                {
                    case SmartCardDataItemCategory.Document:
                        item = new DocumentScatterItem(dataItems[i]);
                        break;
                    case SmartCardDataItemCategory.Video:
                        if(File.Exists(SmartCardDataItem.VIDEO_FOLDER + smartCard.TagId + @"\" + dataItems[i].Name)) // Don't add if we cant find video
                            item = new VideoScatterItem(dataItems[i]);
                        break;
                }

                if(item != null)
                    scatterItems.Add(item);
            }

            for (int i = 0; i < scatterItems.Count; i++)
            {
                ABBScatterItem physics = scatterItems[i];

                // Set color theme
                physics.GradientColor = colorTheme;

                // Register for animation ticks
                animationPulseTimer.Tick += physics.AnimationPulseHandler;

                scatterItems[i].TouchDown += ScatterViewItemMovedHandler;
                scatterItems[i].MouseDown += ScatterViewItemMovedHandler;

                scatterItems[i].Center = this.ActualCenter;
                scatterItems[i].Orientation = this.ActualOrientation;

                if(scatterItems[i] is DocumentScatterItem)
                {
                    physics.OriginalOrientationOffset = SCATTERITEM_DOCUMENT_STARTING_ROTATION;
                    physics.OriginalSize = SCATTERITEM_DOCUMENT_STARTING_SIZE;
                    physics.PullOffset = (Point)SCATTERITEM_DOCUMENT_STARTING_POSITION;

                    physics.Width = SCATTERITEM_DOCUMENT_STARTING_SIZE.Width;
                    physics.Height = SCATTERITEM_DOCUMENT_STARTING_SIZE.Height;

                    documentPlaceholder.Visibility = Visibility.Visible;
                }
                else if (scatterItems[i] is ImageContainerScatterItem || scatterItems[i] is ImageScatterItem)
                {
                    physics.OriginalOrientationOffset = SCATTERITEM_IMAGE_STARTING_ROTATION;
                    physics.OriginalSize = SCATTERITEM_IMAGE_STARTING_SIZE;
                    physics.PullOffset = (Point)SCATTERITEM_IMAGE_STARTING_POSITION;

                    physics.Width = SCATTERITEM_IMAGE_STARTING_SIZE.Width;
                    physics.Height = SCATTERITEM_IMAGE_STARTING_SIZE.Height;

                    imagePlaceholder.Visibility = Visibility.Visible;
                }
                else
                {
                    physics.OriginalOrientationOffset = SCATTERITEM_VIDEO_STARTING_ROTATION;
                    physics.OriginalSize = SCATTERITEM_VIDEO_STARTING_SIZE;
                    physics.PullOffset = (Point)SCATTERITEM_VIDEO_STARTING_POSITION;

                    physics.Width = SCATTERITEM_VIDEO_STARTING_SIZE.Width;
                    physics.Height = SCATTERITEM_VIDEO_STARTING_SIZE.Height;

                    videoPlaceholder.Visibility = Visibility.Visible;
                }

                view.Add(scatterItems[i]);
                physicsItemsActive.Add(physics);
            }

            CalculateNewPositions(physicsItemsActive);
            physicsTimer.Start();
        }

        #endregion

        public VirtualSmartCardScatterItem(ObservableCollection<ABBScatterItem> view, long tagId)
        {
            InitializeComponent();

            this.view = view;
            
            TagId = tagId;

            physicsTimer = new DispatcherTimer(DispatcherPriority.Render, this.Dispatcher);
            physicsTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            physicsTimer.Tick += PhysicsEventHandler;

            physicsTimerLowPriority = new DispatcherTimer(DispatcherPriority.Render, this.Dispatcher);
            physicsTimerLowPriority.Interval = new TimeSpan(0, 0, 0, 0, 200);
            physicsTimerLowPriority.Tick += PhysicsLowPriorityEventHandler;

            animationPulseTimer = new DispatcherTimer(DispatcherPriority.Render, this.Dispatcher);
            animationPulseTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            animationPulseTimer.Tick += AnimationPulseHandler;
            animationPulseTimer.Start();

            animationPulse1.Opacity = ANIMATION_PULSE_1_STARTING_OPACITY;
            animationPulse2.Opacity = ANIMATION_PULSE_2_STARTING_OPACITY;
        }
    }
}