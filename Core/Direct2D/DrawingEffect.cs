/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
#if WPF || WINFORM

using System;
using SharpDX;
using D2D = SharpDX.Direct2D1;

namespace FooEditEngine
{
    class DrawingEffect : ComObject, IDisposable
    {
        public HilightType Stroke;
        public Color4 Fore;
        public bool isBoldLine;
        public DrawingEffect(HilightType type, Color4 fore, bool isBoldLine)
        {
            this.Stroke = type;
            this.Fore = fore;
            this.isBoldLine = isBoldLine;
        }

        void IDisposable.Dispose()
        {
        }
    }

    sealed class SelectedEffect : DrawingEffect
    {
        public Color4 Back;

        public SelectedEffect(Color4 fore, Color4 back, bool isBoldLine) :
            base(HilightType.Select, fore, isBoldLine)
        {
            this.Back = back;
        }
    }
}

#endif