using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Text;
using System.IO;

namespace Ephemeral
{
    static class ResourceFontFamily
    {
        public static FontFamily FromStream(Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return FromBytes(buffer);
        }

        public static FontFamily FromBytes(byte[] buffer)
        {
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                Marshal.Copy(buffer, 0, ptr, buffer.Length);

                var fontCollection = new PrivateFontCollection();
                fontCollection.AddMemoryFont(ptr, buffer.Length);

                return fontCollection.Families[0];
            }
            finally
            {
                handle.Free();
            }
        }
    }

    public static class Gentium
    {
        static Gentium()
        {
            GentiumFontFamily = ResourceFontFamily.FromBytes(Resources.GentiumFont);
        }

        static public FontFamily FontFamily
        {
            get { return GentiumFontFamily; }
        }

        static FontFamily GentiumFontFamily;
    }
}
