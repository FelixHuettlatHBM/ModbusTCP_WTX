﻿// <copyright file="INetConnection.cs" company="Hottinger Baldwin Messtechnik GmbH">
//
// HBM.WT.API, a library to communicate with HBM weighing technology devices  
//
// The MIT License (MIT)
//
// Copyright (C) Hottinger Baldwin Messtechnik GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// </copyright>
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HBM.Weighing.API
{
    /// <summary>
    /// Define the common communication-interface
    /// </summary>
    public interface INetConnection
    {
        event EventHandler BusActivityDetection;
        event EventHandler<DataEvent> RaiseDataEvent;

        void Connect();
     
        int Read(object index);
    
        Task<ushort[]> ReadAsync();
        Task<int> WriteAsync(ushort index, ushort commandParam);

        void Write(object index, int data);

        void WriteArray(ushort index, ushort[] data);

        void Disconnect();
        
        Dictionary<string, int> getData();

        int NumofPoints     { get; set; }
        bool IsConnected    { get; }
        string IpAddress    { get; set; }
        int SendingInterval { get; set; }

    }
    
}
