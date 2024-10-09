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
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WPinternals
{
    internal static class QualcommLoaders
    {
        internal static List<QualcommPartition> GetPossibleLoadersForRootKeyHash(string Path, byte[] RootKeyHash)
        {
            List<QualcommPartition> Result = new List<QualcommPartition>();

            try
            {
                IEnumerable<string> FilePaths = Directory.EnumerateFiles(Path);
                foreach (string FilePath in FilePaths)
                {
                    try
                    {
                        FileInfo Info = new FileInfo(FilePath);
                        if (Info.Length <= 0x80000)
                        {
                            QualcommPartition Loader;

#if DEBUG
                            System.Diagnostics.Debug.Print("Evaluating loader: " + FilePath);
#endif

                            byte[] Binary = ParseAsHexFile(FilePath);
                            if (Binary == null)
                                Loader = new QualcommPartition(FilePath);
                            else
                                Loader = new QualcommPartition(Binary);

                            if ((StructuralComparisons.StructuralEqualityComparer.Equals(Loader.RootKeyHash, RootKeyHash))
                                && (ByteOperations.FindUnicode(Loader.Binary, "QHSUSB_ARMPRG") != null)) // To detect that this is a loader, and not SBL1 or something. V1 loaders are QHSUSB_ARMPRG. V2 loaders are QHSUSB__BULK. Only V1 supported for now, because V2 only accepts signed payload.
                            {
                                Result.Add(Loader);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return Result;
        }

        internal static byte[] ParseAsHexFile(string FilePath)
        {
            byte[] Result = null;

            try
            {
                string[] Lines = File.ReadAllLines(FilePath);
                byte[] Buffer = null;
                int BufferSize = 0;

                foreach (string Line in Lines)
                {
                    if (Line[0] != ':')
                        throw new BadImageFormatException();

                    byte[] LineBytes = Converter.ConvertStringToHex(Line.Substring(1));

                    if ((LineBytes[0] + 5) != LineBytes.Length)
                        throw new BadImageFormatException();

                    if (Buffer == null)
                        Buffer = new byte[0x40000];

                    if (LineBytes[3] == 0) // This is mem data
                    {
                        System.Buffer.BlockCopy(LineBytes, 4, Buffer, BufferSize, LineBytes[0]);
                        BufferSize += LineBytes[0];
                    }
                }

                Result = new byte[BufferSize];
                System.Buffer.BlockCopy(Buffer, 0, Result, 0, BufferSize);
            }
            catch { }

            return Result;
        }
    }
}
