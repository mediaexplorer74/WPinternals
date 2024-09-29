﻿// Copyright (c) 2018, Rene Lergner - wpinternals.net - @Heathcliff74xda
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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace WPinternals
{
    internal class BackupViewModel: ContextViewModel
    {
        private PhoneNotifierViewModel PhoneNotifier;
        private Action Callback;
        private Action SwitchToUnlockBoot;

        internal BackupViewModel(PhoneNotifierViewModel PhoneNotifier, Action SwitchToUnlockBoot, Action Callback)
            : base()
        {
            IsFlashModeOperation = true;

            this.PhoneNotifier = PhoneNotifier;
            this.SwitchToUnlockBoot = SwitchToUnlockBoot;
            this.Callback = Callback;
        }

        internal override void EvaluateViewState()
        {
            if (!IsActive)
                return;

            if (SubContextViewModel == null)
            {
                ActivateSubContext(new BackupTargetSelectionViewModel(PhoneNotifier, SwitchToUnlockBoot, DoBackupArchive, DoBackup));
                IsSwitchingInterface = false;
            }

            if (SubContextViewModel is BackupTargetSelectionViewModel)
                ((BackupTargetSelectionViewModel)SubContextViewModel).EvaluateViewState();
        }

        internal async void DoBackup(string EFIESPPath, string MainOSPath, string DataPath)
        {
            try
            {
                IsSwitchingInterface = true;
                await SwitchModeViewModel.SwitchToWithProgress(PhoneNotifier, PhoneInterfaces.Lumia_MassStorage,
                    (msg, sub) => ActivateSubContext(new BusyViewModel(msg, sub)));
                BackupTask(EFIESPPath, MainOSPath, DataPath);
            }
            catch (Exception Ex)
            {
                ActivateSubContext(new MessageViewModel(Ex.Message, Callback));
            }
        }

        internal async void DoBackupArchive(string ArchivePath)
        {
            try
            {
                IsSwitchingInterface = true;
                await SwitchModeViewModel.SwitchToWithProgress(PhoneNotifier, PhoneInterfaces.Lumia_MassStorage,
                    (msg, sub) => ActivateSubContext(new BusyViewModel(msg, sub)));
                BackupArchiveTask(ArchivePath);
            }
            catch (Exception Ex)
            {
                ActivateSubContext(new MessageViewModel(Ex.Message, Callback));
            }
        }

        internal void BackupTask(string EFIESPPath, string MainOSPath, string DataPath)
        {
            IsSwitchingInterface = false;
            new Thread(() =>
                {
                    bool Result = true;

                    ActivateSubContext(new BusyViewModel("Initializing backup..."));

                    ulong TotalSizeSectors = 0;
                    int PartitionCount = 0;

                    MassStorage Phone = (MassStorage)PhoneNotifier.CurrentModel;

                    Phone.OpenVolume(false);
                    byte[] GPTBuffer = Phone.ReadSectors(1, 33);
                    GPT GPT = new WPinternals.GPT(GPTBuffer);
                    Partition Partition;
                    try
                    {
                        if (EFIESPPath != null)
                        {
                            Partition = GPT.Partitions.Where(p => p.Name == "EFIESP").First();
                            TotalSizeSectors += Partition.SizeInSectors;
                            PartitionCount++;
                        }

                        if (MainOSPath != null)
                        {
                            Partition = GPT.Partitions.Where(p => p.Name == "MainOS").First();
                            TotalSizeSectors += Partition.SizeInSectors;
                            PartitionCount++;
                        }

                        if (DataPath != null)
                        {
                            Partition = GPT.Partitions.Where(p => p.Name == "Data").First();
                            TotalSizeSectors += Partition.SizeInSectors;
                            PartitionCount++;
                        }
                    }
                    catch (Exception Ex)
                    {
                        LogFile.LogException(Ex);
                        Result = false;
                    }

                    BusyViewModel Busy = new BusyViewModel("Create backup...", MaxProgressValue: TotalSizeSectors, UIContext: UIContext);
                    ProgressUpdater Updater = Busy.ProgressUpdater;
                    ActivateSubContext(Busy);

                    int i = 0;
                    if (Result)
                    {
                        try
                        {
                            if (EFIESPPath != null)
                            {
                                i++;
                                Busy.Message = "Create backup of partition EFIESP (" + i.ToString() + @"/" + PartitionCount.ToString() + ")";
                                Phone.BackupPartition("EFIESP", EFIESPPath, Updater);
                            }
                        }
                        catch (Exception Ex)
                        {
                            LogFile.LogException(Ex);
                            Result = false;
                        }
                    }

                    if (Result)
                    {
                        try
                        {
                            if (MainOSPath != null)
                            {
                                i++;
                                Busy.Message = "Create backup of partition MainOS (" + i.ToString() + @"/" + PartitionCount.ToString() + ")";
                                Phone.BackupPartition("MainOS", MainOSPath, Updater);
                            }
                        }
                        catch (Exception Ex)
                        {
                            LogFile.LogException(Ex);
                            Result = false;
                        }
                    }

                    if (Result)
                    {
                        try
                        {
                            if (DataPath != null)
                            {
                                i++;
                                Busy.Message = "Create backup of partition Data (" + i.ToString() + @"/" + PartitionCount.ToString() + ")";
                                Phone.BackupPartition("Data", DataPath, Updater);
                            }
                        }
                        catch (Exception Ex)
                        {
                            LogFile.LogException(Ex);
                            Result = false;
                        }
                    }

                    Phone.CloseVolume();

                    if (!Result)
                    {
                        ActivateSubContext(new MessageViewModel("Failed to create backup!", Exit));
                        return;
                    }

                    ActivateSubContext(new MessageViewModel("Successfully created a backup!", Exit));
                }).Start();
        }

        internal void BackupArchiveTask(string ArchivePath)
        {
            IsSwitchingInterface = false;
            new Thread(() =>
            {
                bool Result = true;

                ActivateSubContext(new BusyViewModel("Initializing backup..."));

                ulong TotalSizeSectors = 0;
                int PartitionCount = 3;

                MassStorage Phone = (MassStorage)PhoneNotifier.CurrentModel;

                try
                {
                    Phone.OpenVolume(false);
                    byte[] GPTBuffer = Phone.ReadSectors(1, 33);
                    GPT GPT = new WPinternals.GPT(GPTBuffer);

                    Partition Partition;

                    try
                    {
                        Partition = GPT.Partitions.Where(p => p.Name == "EFIESP").First();
                        TotalSizeSectors += Partition.SizeInSectors;

                        Partition = GPT.Partitions.Where(p => p.Name == "MainOS").First();
                        TotalSizeSectors += Partition.SizeInSectors;

                        Partition = GPT.Partitions.Where(p => p.Name == "Data").First();
                        TotalSizeSectors += Partition.SizeInSectors;
                    }
                    catch (Exception Ex)
                    {
                        LogFile.LogException(Ex);
                        Result = false;
                    }

                    BusyViewModel Busy = new BusyViewModel("Create backup...", MaxProgressValue: TotalSizeSectors, UIContext: UIContext);
                    ProgressUpdater Updater = Busy.ProgressUpdater;
                    ActivateSubContext(Busy);
                    ZipArchiveEntry Entry;
                    Stream EntryStream = null;

                    using (FileStream FileStream = new FileStream(ArchivePath, FileMode.Create))
                    {
                        using (ZipArchive Archive = new ZipArchive(FileStream, ZipArchiveMode.Create))
                        {
                            int i = 0;

                            if (Result)
                            {
                                try
                                {
                                    Entry = Archive.CreateEntry("EFIESP.bin", CompressionLevel.Optimal);
                                    EntryStream = Entry.Open();
                                    i++;
                                    Busy.Message = "Create backup of partition EFIESP (" + i.ToString() + @"/" + PartitionCount.ToString() + ")";
                                    Phone.BackupPartition("EFIESP", EntryStream, Updater);
                                }
                                catch (Exception Ex)
                                {
                                    LogFile.LogException(Ex);
                                    Result = false;
                                }
                                finally
                                {
                                    if (EntryStream != null)
                                        EntryStream.Close();
                                    EntryStream = null;
                                }
                            }

                            if (Result)
                            {
                                try
                                {
                                    Entry = Archive.CreateEntry("MainOS.bin", CompressionLevel.Optimal);
                                    EntryStream = Entry.Open();
                                    i++;
                                    Busy.Message = "Create backup of partition MainOS (" + i.ToString() + @"/" + PartitionCount.ToString() + ")";
                                    Phone.BackupPartition("MainOS", EntryStream, Updater);
                                }
                                catch (Exception Ex)
                                {
                                    LogFile.LogException(Ex);
                                    Result = false;
                                }
                                finally
                                {
                                    if (EntryStream != null)
                                        EntryStream.Close();
                                    EntryStream = null;
                                }
                            }

                            if (Result)
                            {
                                try
                                {
                                    Entry = Archive.CreateEntry("Data.bin", CompressionLevel.Optimal);
                                    EntryStream = Entry.Open();
                                    i++;
                                    Busy.Message = "Create backup of partition Data (" + i.ToString() + @"/" + PartitionCount.ToString() + ")";
                                    Phone.BackupPartition("Data", EntryStream, Updater);
                                }
                                catch (Exception Ex)
                                {
                                    LogFile.LogException(Ex);
                                    Result = false;
                                }
                                finally
                                {
                                    if (EntryStream != null)
                                        EntryStream.Close();
                                    EntryStream = null;
                                }
                            }
                        }
                    }
                }
                catch { }
                finally
                {
                    Phone.CloseVolume();
                }

                if (!Result)
                {
                    ActivateSubContext(new MessageViewModel("Failed to create backup!", Exit));
                    return;
                }

                ActivateSubContext(new MessageViewModel("Successfully created a backup!", Exit));
            }).Start();
        }

        private void Exit()
        {
            IsSwitchingInterface = false;
            ActivateSubContext(null);
            Callback();
        }
    }
}
