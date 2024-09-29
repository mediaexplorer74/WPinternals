﻿// Copyright (c) 2018, Rene Lergner - @Heathcliff74xda
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Windows;

namespace WPinternals
{
    internal class RegistrationViewModel : ContextViewModel
    {
        private readonly Action Completed;
        private readonly Action Failed;

        internal RegistrationViewModel(Action Completed, Action Failed)
            : base()
        {
            this.Completed = Completed;
            this.Failed = Failed;
        }

        private DelegateCommand _ExitCommand = null;
        public DelegateCommand ExitCommand
        {
            get
            {
                //return _ExitCommand ??= new DelegateCommand(() => Application.Current.Shutdown());
                return _ExitCommand ??= new DelegateCommand(() =>
                {
                    try
                    {
                        App.Config.RegistrationName = "Some Name";
                        App.Config.RegistrationEmail = "Email@email.com";
                        App.Config.RegistrationSkypeID = "SkypeID";
                        App.Config.RegistrationTelegramID = "@TelegramID";
                        App.Config.RegistrationKey = "1234567890";//Registration.CalcRegKey();

                        LogFile.BeginAction("Registration");
                        LogFile.EndAction("Registration");

                        App.Config.WriteConfig();

                        Completed();
                    }
                    catch
                    {
                        Failed();
                    }
                });
            }
        }

        private DelegateCommand _ContinueCommand = null;
        public DelegateCommand ContinueCommand
        {
            get
            {
                return _ContinueCommand ??= new DelegateCommand(() =>
                    {
                        try
                        {
                            App.Config.RegistrationName = _Name;
                            App.Config.RegistrationEmail = _Email;
                            App.Config.RegistrationSkypeID = _SkypeID;
                            App.Config.RegistrationTelegramID = _TelegramID;
                            App.Config.RegistrationKey = Registration.CalcRegKey();

                            LogFile.BeginAction("Registration");
                            LogFile.EndAction("Registration");

                            App.Config.WriteConfig();

                            Completed();
                        }
                        catch
                        {
                            Failed();
                        }
                    });
            }
        }

        public bool IsRegistrationComplete
        {
            get
            {
                return (_Name?.Length >= 2) && (_Email?.Length >= 5);
            }
        }

        private string _Name = null;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(IsRegistrationComplete));
            }
        }

        private string _Email = null;
        public string Email
        {
            get
            {
                return _Email;
            }
            set
            {
                _Email = value;
                OnPropertyChanged(nameof(Email));
                OnPropertyChanged(nameof(IsRegistrationComplete));
            }
        }

        private string _SkypeID = null;
        public string SkypeID
        {
            get
            {
                return _SkypeID;
            }
            set
            {
                _SkypeID = value;
                OnPropertyChanged(nameof(SkypeID));
            }
        }

        private string _TelegramID = null;
        public string TelegramID
        {
            get
            {
                return _TelegramID;
            }
            set
            {
                _TelegramID = value;
                OnPropertyChanged(nameof(TelegramID));
            }
        }
    }
}
