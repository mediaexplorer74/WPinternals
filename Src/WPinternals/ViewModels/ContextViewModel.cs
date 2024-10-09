// ContextViewModel

using System;
using System.ComponentModel;
using System.Threading;

namespace WPinternals
{
    internal class ContextViewModel : INotifyPropertyChanged
    {
        protected SynchronizationContext UIContext;

        public bool IsSwitchingInterface = false;
        public bool IsFlashModeOperation = false;
        private bool _IsActive = false;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void OnPropertyChanged(string propertyName)
        {
            if ((UIContext == null) && (SynchronizationContext.Current != null))
                UIContext = SynchronizationContext.Current;

            if (this.PropertyChanged != null)
            {
                if (SynchronizationContext.Current == UIContext)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                else
                {
                    UIContext.Post((s) =>
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    }, null);
                }
            }
        }
        
        private ContextViewModel _SubContextViewModel;
        public ContextViewModel SubContextViewModel
        {
            get
            {
                return _SubContextViewModel;
            }
            private set
            {
                if (_SubContextViewModel != null)
                    _SubContextViewModel.IsActive = false;
                _SubContextViewModel = value;
                if (_SubContextViewModel != null)
                    _SubContextViewModel.IsActive = IsActive;
                OnPropertyChanged("SubContextViewModel");
            }
        }

        internal ContextViewModel()
        {
            UIContext = SynchronizationContext.Current;
        }

        internal ContextViewModel(MainViewModel Main): this()
        {
        }

        internal ContextViewModel(MainViewModel Main, ContextViewModel SubContext): this(Main)
        {
            SubContextViewModel = SubContext;
        }

        internal bool IsActive
        {
            get
            {
                return _IsActive;
            }
            set
            {
                _IsActive = value;
            }
        }

        internal virtual void EvaluateViewState()
        {

        }

        internal void Activate()
        {
            IsActive = true;
            EvaluateViewState();
            if (SubContextViewModel != null)
                SubContextViewModel.Activate();
        }

        internal void ActivateSubContext(ContextViewModel NewSubContext)
        {
            if (_SubContextViewModel != null)
                _SubContextViewModel.IsActive = false;
            if (NewSubContext != null)
            {
                if (IsActive)
                    NewSubContext.Activate();
                else
                    NewSubContext.IsActive = false;
            }
            SubContextViewModel = NewSubContext;
        }

        internal void SetWorkingStatus(string Message, string SubMessage, ulong? MaxProgressValue, 
            bool ShowAnimation = true, WPinternalsStatus Status = WPinternalsStatus.Undefined)
        {
            ActivateSubContext(new BusyViewModel(Message, SubMessage, MaxProgressValue, 
                UIContext: UIContext, ShowAnimation: ShowAnimation));
        }

        internal void UpdateWorkingStatus(string Message, string SubMessage, 
            ulong? CurrentProgressValue, WPinternalsStatus Status = WPinternalsStatus.Undefined)
        {
            if (SubContextViewModel is BusyViewModel)
            {
                BusyViewModel Busy = (BusyViewModel)SubContextViewModel;
                if (Message != null)
                {
                    Busy.Message = Message;
                    Busy.SubMessage = SubMessage;
                }
                if ((CurrentProgressValue != null) && (Busy.ProgressUpdater != null))
                {
                    try
                    {
                        Busy.ProgressUpdater.SetProgress((ulong)CurrentProgressValue);
                    }
                    catch (Exception Ex)
                    {
                        LogFile.LogException(Ex);
                    }
                }
            }
        }
    }
}
