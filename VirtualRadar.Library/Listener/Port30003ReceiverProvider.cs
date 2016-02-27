﻿// Copyright © 2016 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="IReceiverFormatProvider"/> for BaseStation format receivers.
    /// </summary>
    class Port30003ReceiverProvider : IReceiverFormatProvider
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UniqueId
        {
            get { return DataSource.Port30003; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ShortName
        {
            get { return Strings.Port30003Feed; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsRawFormat { get { return false; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IMessageBytesExtractor CreateMessageBytesExtractor()
        {
            return Factory.Singleton.Resolve<IPort30003MessageBytesExtractor>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="extractor"></param>
        /// <returns></returns>
        public bool IsUsableBytesExtractor(IMessageBytesExtractor extractor)
        {
            return extractor != null && extractor is IPort30003MessageBytesExtractor;
        }
    }
}
