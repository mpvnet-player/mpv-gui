﻿using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace HandyControl.Tools.Interop
{
    internal sealed class BitmapHandle : WpfSafeHandle
    {
        [SecurityCritical]
        private BitmapHandle() : this(true)
        {
        }

        [SecurityCritical]
        private BitmapHandle(bool ownsHandle) : base(ownsHandle, CommonHandles.GDI)
        {
        }

        [SecurityCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return InteropMethods.DeleteObject(handle);
        }

        [SecurityCritical]
        internal HandleRef MakeHandleRef(object obj)
        {
            return new HandleRef(obj, handle);
        }

        [SecurityCritical]
        internal static BitmapHandle CreateFromHandle(IntPtr hbitmap, bool ownsHandle = true)
        {
            return new BitmapHandle(ownsHandle)
            {
                handle = hbitmap,
            };
        }
    }
}
