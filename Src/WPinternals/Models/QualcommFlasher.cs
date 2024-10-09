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

namespace WPinternals
{
    internal enum FlashUnit
    {
        Bytes,
        Sectors
    }

    internal class QualcommFlasher
    {
        private QualcommSerial Serial;

        public QualcommFlasher(QualcommSerial Serial)
        {
            this.Serial = Serial;
        }

        public void CloseSerial()
        {
            Serial.Close();
        }

        public void Hello()
        {
            byte[] Command = new byte[]
            {
                0x01, // Hello command
                0x51, 0x43, 0x4F, 0x4D, 0x20, 0x66, 0x61, 0x73, 0x74, 0x20, 0x64, 0x6F, 0x77, 0x6E, 0x6C, 0x6F, // "QCOM fast download protocol host"
                0x61, 0x64, 0x20, 0x70, 0x72, 0x6F, 0x74, 0x6F, 0x63, 0x6F, 0x6C, 0x20, 0x68, 0x6F, 0x73, 0x74,
                0x02,
                0x02, // Protocol version - Must be at least 0x02
                0x01
            };

            Serial.SendCommand(Command, new byte[] { 0x02 });
        }

        public void SetSecurityMode(byte Mode)
        {
            byte[] Command = new byte[2];
            Command[0] = 0x17;
            Command[1] = Mode;

            Serial.SendCommand(Command, new byte[] { 0x18 });
        }

        // Use PartitionID 0x21
        public void OpenPartition(byte PartitionID)
        {
            byte[] Command = new byte[2];
            Command[0] = 0x1B;
            Command[1] = PartitionID;

            Serial.SendCommand(Command, new byte[] { 0x1C });
        }

        public void ClosePartition()
        {
            Serial.SendCommand(new byte[] { 0x15 }, new byte[] { 0x16 });
        }

        public void Flash(UInt32 StartInBytes, Stream Data, UInt32 LengthInBytes = UInt32.MaxValue)
        {
            Flash(StartInBytes, Data, null, null, LengthInBytes);
        }

        public void Flash(UInt32 StartInBytes, Stream Data, Action<int, TimeSpan?> ProgressUpdateCallback, UInt32 LengthInBytes = UInt32.MaxValue)
        {
            Flash(StartInBytes, Data, ProgressUpdateCallback, null, LengthInBytes);
        }

        public void Flash(UInt32 StartInBytes, Stream Data, ProgressUpdater UpdaterPerSector, UInt32 LengthInBytes = UInt32.MaxValue)
        {
            Flash(StartInBytes, Data, null, UpdaterPerSector, LengthInBytes);
        }

        public void Flash(UInt32 StartInBytes, Stream Data, Action<int, TimeSpan?> ProgressUpdateCallback, ProgressUpdater UpdaterPerSector, UInt32 LengthInBytes = UInt32.MaxValue)
        {
            long Remaining;
            if ((LengthInBytes == UInt32.MaxValue) || (LengthInBytes > (Data.Length - Data.Position)))
                Remaining = Data.Length - Data.Position;
            else
                Remaining = LengthInBytes;
            UInt32 CurrentLength;
            byte[] Buffer = new byte[0x405];
            byte[] ResponsePattern = new byte[5];
            byte[] FinalCommand;
            Buffer[0] = 0x07;
            ResponsePattern[0] = 0x08;
            UInt32 CurrentPosition = StartInBytes;

            ProgressUpdater Progress = UpdaterPerSector;
            if ((Progress == null) && (ProgressUpdateCallback != null))
                Progress = new ProgressUpdater(GetSectorCount((UInt64)Remaining), ProgressUpdateCallback);

            while (Remaining > 0)
            {
                System.Buffer.BlockCopy(BitConverter.GetBytes(CurrentPosition), 0, Buffer, 1, 4); // Start is in bytes and in Little Endian (on Samsung devices start is in sectors!)
                System.Buffer.BlockCopy(BitConverter.GetBytes(CurrentPosition), 0, ResponsePattern, 1, 4); // Start is in bytes and in Little Endian (on Samsung devices start is in sectors!)

                if (Remaining >= 0x400)
                    CurrentLength = 0x400;
                else
                    CurrentLength = (UInt32)Remaining;

                CurrentLength = (uint)Data.Read(Buffer, 5, (int)CurrentLength);

                if (CurrentLength < 0x400)
                {
                    FinalCommand = new byte[CurrentLength + 5];
                    System.Buffer.BlockCopy(Buffer, 0, FinalCommand, 0, (int)CurrentLength + 5);
                }
                else
                    FinalCommand = Buffer;

                Serial.SendCommand(FinalCommand, ResponsePattern);

                CurrentPosition += CurrentLength;
                Remaining -= CurrentLength;

                if (Progress != null)
                    Progress.IncreaseProgress(GetSectorCount(CurrentLength));
            }
        }

        public void Flash(UInt32 StartInBytes, byte[] Data, UInt32 OffsetInBytes = 0, UInt32 LengthInBytes = UInt32.MaxValue)
        {
            Flash(StartInBytes, Data, null, null, OffsetInBytes, LengthInBytes);
        }

        public void Flash(UInt32 StartInBytes, byte[] Data, Action<int, TimeSpan?> ProgressUpdateCallback, UInt32 OffsetInBytes = 0, UInt32 LengthInBytes = UInt32.MaxValue)
        {
            Flash(StartInBytes, Data, ProgressUpdateCallback, null, OffsetInBytes, LengthInBytes);
        }

        public void Flash(UInt32 StartInBytes, byte[] Data, ProgressUpdater UpdaterPerSector, UInt32 OffsetInBytes = 0, UInt32 LengthInBytes = UInt32.MaxValue)
        {
            Flash(StartInBytes, Data, null, UpdaterPerSector, OffsetInBytes, LengthInBytes);
        }

        public void Flash(UInt32 StartInBytes, byte[] Data, Action<int, TimeSpan?> ProgressUpdateCallback, ProgressUpdater UpdaterPerSector, UInt32 OffsetInBytes = 0, UInt32 LengthInBytes = UInt32.MaxValue)
        {
            long RemainingBytes;
            if (OffsetInBytes > (Data.Length - 1))
                throw new ArgumentException("Wrong offset");
            if ((LengthInBytes == UInt32.MaxValue) || (LengthInBytes > (Data.Length - OffsetInBytes)))
                RemainingBytes = Data.Length - OffsetInBytes;
            else
                RemainingBytes = LengthInBytes;
            UInt32 CurrentLength;
            UInt32 CurrentOffset = OffsetInBytes;
            byte[] Buffer = new byte[0x405];
            byte[] ResponsePattern = new byte[5];
            byte[] FinalCommand;
            Buffer[0] = 0x07;
            ResponsePattern[0] = 0x08;
            UInt32 CurrentPosition = StartInBytes;

            ProgressUpdater Progress = UpdaterPerSector;
            if ((Progress == null) && (ProgressUpdateCallback != null))
                Progress = new ProgressUpdater(GetSectorCount((UInt64)RemainingBytes), ProgressUpdateCallback);

            while (RemainingBytes > 0)
            {
                System.Buffer.BlockCopy(BitConverter.GetBytes(CurrentPosition), 0, Buffer, 1, 4); // Start position is in bytes and in Little Endian (on Samsung phones the start position is in Sectors!!)
                System.Buffer.BlockCopy(BitConverter.GetBytes(CurrentPosition), 0, ResponsePattern, 1, 4); // Start position is in bytes and in Little Endian (on Samsung phones the start position is in Sectors!!)

                if (RemainingBytes >= 0x400)
                    CurrentLength = 0x400;
                else
                    CurrentLength = (UInt32)RemainingBytes;
                System.Buffer.BlockCopy(Data, (int)CurrentOffset, Buffer, 5, (int)CurrentLength);

                if (CurrentLength < 0x400)
                {
                    FinalCommand = new byte[CurrentLength + 5];
                    System.Buffer.BlockCopy(Buffer, 0, FinalCommand, 0, (int)CurrentLength + 5);
                }
                else
                    FinalCommand = Buffer;

                Serial.SendCommand(FinalCommand, ResponsePattern);

                CurrentPosition += CurrentLength;
                CurrentOffset += CurrentLength;
                RemainingBytes -= CurrentLength;

                if (Progress != null)
                    Progress.IncreaseProgress(GetSectorCount(CurrentLength));
            }
        }

        public UInt64 GetSectorCount(UInt64 ByteCount)
        {
            return (ByteCount / 0x200) + ((ByteCount % 0x200) > 0 ? (UInt64)1 : (UInt64)0);
        }

        public void Reboot()
        {
            Serial.SendCommand(new byte[] { 0x0B }, new byte[] { 0x0C });
        }
    }
}
