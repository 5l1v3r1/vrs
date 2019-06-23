﻿// Copyright © 2019 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using SixLabors.Fonts;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing.ImageSharp
{
    /// <summary>
    /// The default ImageSharp implementation of <see cref="VrsDrawing.IFontFactory"/>.
    /// </summary>
    class FontFactory : CommonFontFactory
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        protected override bool ImageLibraryDrawTextWillTruncate => false;

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="pointSize"></param>
        /// <param name="fontStyle"></param>
        /// <param name="isCached"></param>
        /// <returns></returns>
        protected override VrsDrawing.IFont CreateFontWrapper(VrsDrawing.IFontFamily fontFamily, float pointSize, VrsDrawing.FontStyle fontStyle, bool isCached)
        {
            return new FontWrapper(
                SystemFonts.CreateFont(fontFamily.Name, pointSize, Convert.ToImageSharpFontStyle(fontStyle)),
                isCached
            );
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="drawing"></param>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected override void MeasureText(VrsDrawing.IDrawing drawing, VrsDrawing.IFont font, string text, out float width, out float height)
        {
            width = 0F;
            height = 0F;

            if(font is FontWrapper fontWrapper) {
                var size = TextMeasurer.Measure(text, new RendererOptions(fontWrapper.NativeFont));
                width = size.Width;
                height = size.Height;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<VrsDrawing.IFontFamily> GetInstalledFonts()
        {
            foreach(var family in SystemFonts.Families) {
                yield return new FontFamilyWrapper(family, false);
            }
        }
    }
}