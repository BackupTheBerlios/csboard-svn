// Catalog.cs
//
// (C) Edd Dumbill <edd@usefulinc.com> 2004
//
// C# interface to Gettext functions
// Intended as a dropin replacement for Gettext 0.14 GNU.Gettext.dll
// Thanks to Vladimir Vukicevic for his help with marshalling details.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307  USA

using System;
using System.Runtime.InteropServices;

class Catalog {
	[DllImport("libc")]
	static extern IntPtr bindtextdomain (IntPtr domainname, IntPtr dirname);
	[DllImport("libc")]
	static extern IntPtr bind_textdomain_codeset (IntPtr domainname,
		IntPtr codeset);
	[DllImport("libc")]
	static extern IntPtr textdomain (IntPtr domainname);

	// charset hardwired to utf-8: you may wish to change it
	public static void Init (String package, String localedir)
	{
		IntPtr ipackage = Marshal.StringToHGlobalAuto (package);
		IntPtr ilocaledir = Marshal.StringToHGlobalAuto (localedir);
		IntPtr iutf8 = Marshal.StringToHGlobalAuto ("UTF-8");
		bindtextdomain (ipackage, ilocaledir);
		bind_textdomain_codeset (ipackage, iutf8);
		textdomain (ipackage);
		Marshal.FreeHGlobal (ipackage);
		Marshal.FreeHGlobal (ilocaledir);
		Marshal.FreeHGlobal (iutf8);
	}

	[DllImport("libc")]
	static extern IntPtr gettext (IntPtr s);
	
	public static String GetString (String s)
	{
		IntPtr inptr = Marshal.StringToHGlobalAuto (s);
		IntPtr sptr = gettext (inptr);
		Marshal.FreeHGlobal (inptr);
		if (inptr == sptr)
			return s;
		else
			return Marshal.PtrToStringAuto (sptr);
	}

	[DllImport("libc")]
	static extern IntPtr ngettext (IntPtr s, IntPtr p, Int32 n);
	
	public static String GetPluralString (String s, String p, Int32 n)
	{
		IntPtr inptrs = Marshal.StringToHGlobalAuto (s);
		IntPtr inptrp = Marshal.StringToHGlobalAuto (p);
		IntPtr sptr = ngettext (inptrs, inptrp, n);
		Marshal.FreeHGlobal (inptrs);
		Marshal.FreeHGlobal (inptrp);
		if (sptr == inptrs)
			return s;
		else if (sptr == inptrp)
			return p;
		else
			return Marshal.PtrToStringAuto (sptr);
	}

}

