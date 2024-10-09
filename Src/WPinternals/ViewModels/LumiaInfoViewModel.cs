// LumiaInfoViewModel (ContextViewModel)

using System;

namespace WPinternals
{
    internal class LumiaInfoViewModel: ContextViewModel
    {
        internal PhoneInterfaces? CurrentInterface;
        internal IDisposable CurrentModel;
        internal PhoneNotifierViewModel PhoneNotifier;
        private Action<PhoneInterfaces> ModeSwitchRequestCallback;
        private Action SwitchToGettingStarted;

        internal LumiaInfoViewModel
        (
            PhoneNotifierViewModel PhoneNotifier, 
            Action<PhoneInterfaces> ModeSwitchRequestCallback, 
            Action SwitchToGettingStarted
        ) : base()
        {
            this.PhoneNotifier = PhoneNotifier;
            this.ModeSwitchRequestCallback = ModeSwitchRequestCallback;
            this.SwitchToGettingStarted = SwitchToGettingStarted;

            CurrentInterface = PhoneNotifier.CurrentInterface;
            CurrentModel = PhoneNotifier.CurrentModel;

            PhoneNotifier.NewDeviceArrived += NewDeviceArrived;
            PhoneNotifier.DeviceRemoved += DeviceRemoved;
        }

        ~LumiaInfoViewModel()
        {
            PhoneNotifier.NewDeviceArrived -= NewDeviceArrived;
            PhoneNotifier.DeviceRemoved -= DeviceRemoved;
        }

        void DeviceRemoved()
        {
            CurrentInterface = null;
            CurrentModel = null;
            ActivateSubContext(null);
        }

        void NewDeviceArrived(ArrivalEventArgs Args)
        {
            CurrentInterface = Args.NewInterface;
            CurrentModel = Args.NewModel;

            // Determine SubcontextViewModel
            switch (CurrentInterface)
            {
                case null:
                case PhoneInterfaces.Lumia_Bootloader:
                    ActivateSubContext(null);
                    break;
                case PhoneInterfaces.Lumia_Normal:
                    ActivateSubContext(new NokiaNormalViewModel((NokiaPhoneModel)CurrentModel, 
                        ModeSwitchRequestCallback));
                    break;
                case PhoneInterfaces.Lumia_Flash:
                    ActivateSubContext(new NokiaFlashViewModel((NokiaFlashModel)CurrentModel, 
                        ModeSwitchRequestCallback, SwitchToGettingStarted));
                    break;
                case PhoneInterfaces.Lumia_Label:
                    ActivateSubContext(new NokiaLabelViewModel((NokiaPhoneModel)CurrentModel, 
                        ModeSwitchRequestCallback));
                    break;
                case PhoneInterfaces.Lumia_MassStorage:
                    ActivateSubContext(new NokiaMassStorageViewModel((MassStorage)CurrentModel));
                    break;
            };
        }
    }
}
