using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Windows.Networking.Sockets;
using Windows.Networking;

namespace SharpOSC
{
    public class UDPSender
    {
        public int Port
        {
            get { return _port; }
        }
        int _port;

        public string Address
        {
            get { return _address; }
        }
        string _address;

        internal const long Invalid = -1;
        private const long MaxIPv4Value = uint.MaxValue; // the native parser cannot handle MaxIPv4Value, only MaxIPv4Value - 1
        private const int Octal = 8;
        private const int Decimal = 10;
        private const int Hex = 16;

        IPEndPoint RemoteIpEndPoint;
        Socket sock;
        DatagramSocket receivingUdpClient;

        //internal static unsafe long ParseNonCanonical(char* name, int start, ref int end, bool notImplicitFile)
        //{
        //    int numberBase = Decimal;
        //    char ch;
        //    long* parts = stackalloc long[4];
        //    long currentValue = 0;
        //    bool atLeastOneChar = false;

        //    // Parse one dotted section at a time
        //    int dotCount = 0; // Limit 3
        //    int current = start;
        //    for (; current < end; current++)
        //    {
        //        ch = name[current];
        //        currentValue = 0;

        //        // Figure out what base this section is in
        //        numberBase = Decimal;
        //        if (ch == '0')
        //        {
        //            numberBase = Octal;
        //            current++;
        //            atLeastOneChar = true;
        //            if (current < end)
        //            {
        //                ch = name[current];
        //                if (ch == 'x' || ch == 'X')
        //                {
        //                    numberBase = Hex;
        //                    current++;
        //                    atLeastOneChar = false;
        //                }
        //            }
        //        }

        //        // Parse this section
        //        for (; current < end; current++)
        //        {
        //            ch = name[current];
        //            int digitValue;

        //            if ((numberBase == Decimal || numberBase == Hex) && '0' <= ch && ch <= '9')
        //            {
        //                digitValue = ch - '0';
        //            }
        //            else if (numberBase == Octal && '0' <= ch && ch <= '7')
        //            {
        //                digitValue = ch - '0';
        //            }
        //            else if (numberBase == Hex && 'a' <= ch && ch <= 'f')
        //            {
        //                digitValue = ch + 10 - 'a';
        //            }
        //            else if (numberBase == Hex && 'A' <= ch && ch <= 'F')
        //            {
        //                digitValue = ch + 10 - 'A';
        //            }
        //            else
        //            {
        //                break; // Invalid/terminator
        //            }

        //            currentValue = (currentValue * numberBase) + digitValue;

        //            if (currentValue > MaxIPv4Value) // Overflow
        //            {
        //                return Invalid;
        //            }

        //            atLeastOneChar = true;
        //        }

        //        if (current < end && name[current] == '.')
        //        {
        //            if (dotCount >= 3 // Max of 3 dots and 4 segments
        //                || !atLeastOneChar // No empty segmets: 1...1
        //                // Only the last segment can be more than 255 (if there are less than 3 dots)
        //                || currentValue > 0xFF)
        //            {
        //                return Invalid;
        //            }
        //            parts[dotCount] = currentValue;
        //            dotCount++;
        //            atLeastOneChar = false;
        //            continue;
        //        }
        //        // We don't get here unless We find an invalid character or a terminator
        //        break;
        //    }

        //    // Terminators
        //    if (!atLeastOneChar)
        //    {
        //        return Invalid;  // Empty trailing segment: 1.1.1.
        //    }
        //    else if (current >= end)
        //    {
        //        // end of string, allowed
        //    }
        //    else if ((ch = name[current]) == '/' || ch == '\\' || (notImplicitFile && (ch == ':' || ch == '?' || ch == '#')))
        //    {
        //        end = current;
        //    }
        //    else
        //    {
        //        // not a valid terminating character
        //        return Invalid;
        //    }

        //    parts[dotCount] = currentValue;
        //    // Parsed, reassemble and check for overflows
        //    switch (dotCount)
        //    {
        //        case 0: // 0xFFFFFFFF
        //            if (parts[0] > MaxIPv4Value)
        //            {
        //                return Invalid;
        //            }
        //            return parts[0];
        //        case 1: // 0xFF.0xFFFFFF
        //            if (parts[1] > 0xffffff)
        //            {
        //                return Invalid;
        //            }
        //            return (parts[0] << 24) | (parts[1] & 0xffffff);
        //        case 2: // 0xFF.0xFF.0xFFFF
        //            if (parts[2] > 0xffff)
        //            {
        //                return Invalid;
        //            }
        //            return (parts[0] << 24) | ((parts[1] & 0xff) << 16) | (parts[2] & 0xffff);
        //        case 3: // 0xFF.0xFF.0xFF.0xFF
        //            if (parts[3] > 0xff)
        //            {
        //                return Invalid;
        //            }
        //            return (parts[0] << 24) | ((parts[1] & 0xff) << 16) | ((parts[2] & 0xff) << 8) | (parts[3] & 0xff);
        //        default:
        //            return Invalid;
        //    }
        //}

        //private static unsafe bool ParseCanonical(string name, byte* numbers, int start, int end)
        //{
        //    for (int i = 0; i < 4; ++i)
        //    {
        //        byte b = 0;
        //        char ch;
        //        for (; (start < end) && (ch = name[start]) != '.' && ch != ':'; ++start)
        //        {
        //            b = (byte)(b * 10 + (byte)(ch - '0'));
        //        }
        //        numbers[i] = b;
        //        ++start;
        //    }
        //    return numbers[0] == 127;
        //}

        //public static unsafe bool Ipv4StringToAddress(string ipString, out long address)
        //{
        //    //Debug.Assert(ipString != null);
        //    long tmpAddr;
        //    int end = ipString.Length;
        //    fixed (char* ipStringPtr = ipString)
        //    {
        //        tmpAddr = ParseNonCanonical(ipStringPtr, 0, ref end, notImplicitFile: true);
        //    }

        //    if (tmpAddr != Invalid && end == ipString.Length)
        //    {
        //        // IPv4AddressHelper.ParseNonCanonical returns the bytes in the inverse order.
        //        // Reverse them and return success.
        //        address =
        //            ((0xFF000000 & tmpAddr) >> 24) |
        //            ((0x00FF0000 & tmpAddr) >> 8) |
        //            ((0x0000FF00 & tmpAddr) << 8) |
        //            ((0x000000FF & tmpAddr) << 24);
        //        return true;
        //    }
        //    else
        //    {
        //        // Failed to parse the address.
        //        address = 0;
        //        return false;
        //    }
        //}

        //public UDPSender(string address, int port)
        //{
        //    _port = port;
        //    _address = address;

        //    sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        //    long longaddress;
        //    Ipv4StringToAddress(address, out longaddress);
        //        //return new IPAddress(longaddress);
        //    var addresses = new System.Net.IPAddress(longaddress);
        //    //if (addresses.Length == 0) throw new Exception("Unable to find IP address for " + address);

        //    RemoteIpEndPoint = new IPEndPoint(addresses, port);
        //}

        public void Send(byte[] message)
        {
            var args = new SocketAsyncEventArgs();
            args.UserToken = sock;
            args.RemoteEndPoint = RemoteIpEndPoint;
            //message = Encoding.UTF8.GetBytes("Hello World");
            args.SetBuffer(message, 0, message.Length);
            sock.SendToAsync(args);
            //			sock.SendToAsync(message, RemoteIpEndPoint);
        }

        public void Send(OscPacket packet)
        {
            byte[] data = packet.GetBytes();
            Send(data);
        }

        public void Close()
        {
            sock.Dispose();
        }
    }
}
