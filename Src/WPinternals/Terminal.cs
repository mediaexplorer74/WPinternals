// Terminal

using System;
using System.Collections.Generic;

namespace WPinternals
{
    internal static class Terminal
    {
        public static TerminalResponse Parse(byte[] Buffer, int Offset)
        {
            TerminalResponse Response = new TerminalResponse();

            // Get root node
            if (Buffer.Length >= (Offset + 8))
            {
                int NodeNumber = BitConverter.ToInt32(Buffer, Offset);
                int NodeSize = BitConverter.ToInt32(Buffer, Offset + 4);
                int End = NodeSize + Offset + 8;
                int Index = Offset + 8;
                if ((NodeNumber == 0x10000) && (End <= Buffer.Length))
                {
                    // Get subnodes
                    while (Index < End)
                    {
                        NodeNumber = BitConverter.ToInt32(Buffer, Index);
                        NodeSize = BitConverter.ToInt32(Buffer, Index + 4);
                        byte[] Raw = new byte[NodeSize];
                        Array.Copy(Buffer, Index + 8, Raw, 0, NodeSize);
                        Response.RawEntries.Add(NodeNumber, Raw);
                        Index += NodeSize + 8;
                    }
                }
            }

            // Parse subnodes
            Response.RawEntries.TryGetValue(3, out Response.PublicId);
            Response.RawEntries.TryGetValue(7, out Response.RootKeyHash);

            return Response;
        }
    }

    internal class TerminalResponse
    {
        public Dictionary<int, byte[]> RawEntries = new Dictionary<int, byte[]>();
        public byte[] PublicId = null;
        public byte[] RootKeyHash = null;
    }
}
