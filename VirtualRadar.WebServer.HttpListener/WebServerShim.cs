﻿// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.Owin;
using Owin;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.WebServer.HttpListener
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// A shim that glues OWIN and the old web site interface together.
    /// </summary>
    /// <remarks>
    /// The intention is that this will be short-lived and will not appear in the final form of
    /// the OWIN version of the web server. I just need it so that the program continues to work
    /// while everything is being ported.
    /// </remarks>
    class WebServerShim
    {
        /// <summary>
        /// The web server that we expose OWIN stuff through.
        /// </summary>
        public OwinWebServer WebServer { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="webServer"></param>
        public WebServerShim(OwinWebServer webServer)
        {
            WebServer = webServer;
        }

        /// <summary>
        /// Configures the app builder for OWIN operations.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IAppBuilder app)
        {
            var shimMiddleware = new Func<AppFunc, AppFunc>(ShimMiddleware);
            app.Use(shimMiddleware);
        }

        /// <summary>
        /// The shim's OWIN middleware.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc ShimMiddleware(AppFunc next)
        {
            AppFunc appFunc = async(IDictionary<string, object> environment) => {
                var context = new Context(environment);
                HandleRequest(context);

                if(!context.StopProcessing) {
                    await next.Invoke(environment);
                }
            };

            return appFunc;
        }

        /// <summary>
        /// Raises events on the web server object to expose a web request to the rest of the system.
        /// </summary>
        /// <param name="context"></param>
        private void HandleRequest(Context context)
        {
            var requestArgs = new RequestReceivedEventArgs(context.Request, context.Response, WebServer.Root);
            var requestReceivedEventArgsId = requestArgs.UniqueId;

            try {
                var startTime = WebServer.Provider.UtcNow;

                WebServer.OnBeforeRequestReceived(requestArgs);
                WebServer.OnRequestReceived(requestArgs);
                WebServer.OnAfterRequestReceived(requestArgs);

                if(!requestArgs.Handled) {
                    context.Response.StatusCode = HttpStatusCode.NotFound;
                }

                var fullClientAddress = String.Format("{0}:{1}", requestArgs.ClientAddress, context.Request.RemoteEndPoint.Port);
                var responseArgs = new ResponseSentEventArgs(requestArgs.PathAndFile, fullClientAddress, requestArgs.ClientAddress,
                                                                context.Response.ContentLength, requestArgs.Classification, context.Request,
                                                                (int)context.Response.StatusCode, (int)(WebServer.Provider.UtcNow - startTime).TotalMilliseconds,
                                                                context.BasicUserName);
                WebServer.OnResponseSent(responseArgs);
            } catch(Exception ex) {
                // The HttpListener version doesn't log these as there can be lot of them, but given that
                // this stuff is all brand new I think I'd like to see what exceptions are being thrown
                // during processing.
                var log = Factory.Singleton.Resolve<ILog>().Singleton;
                log.WriteLine("Caught exception in general request handling event handlers: {0}", ex);

                Debug.WriteLine($"WebServer.GetContextHandler caught exception {ex}");
                WebServer.OnExceptionCaught(new EventArgs<Exception>(new RequestException(context.Request, ex)));
            }

            try {
                if(requestReceivedEventArgsId != -1) {
                    WebServer.OnRequestFinished(new EventArgs<long>(requestReceivedEventArgsId));
                }
            } catch(Exception ex) {
                var log = Factory.Singleton.Resolve<ILog>().Singleton;
                log.WriteLine("Caught exception in RequestFinished event handler: {0}", ex);
            }
        }
    }
}
