//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Library General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
//
// Copyright (C) 2004 Nickolay V. Shmyrev


// New filechooser from GTK 2.4 should be used. While this 
// feature is not implemented in gtk-sharp, we implement it here.

namespace Gtk {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	
	public enum FileChooserAction {
	   Open,
	   Save, 
	   SelectFolder,
	   CreateFolder
	}

	public class FileChooserDialog : Gtk.Dialog {
         
		~FileChooserDialog()
		{
			Dispose();
		}

		protected FileChooserDialog (GLib.GType gtype) : base(gtype) {}

		[DllImport("libgtk-x11-2.0.so.0")]
		static extern IntPtr gtk_file_chooser_dialog_new(IntPtr title, IntPtr widget, int action, IntPtr data);

		public FileChooserDialog(string title, Window window, FileChooserAction action)
		{
                	IntPtr p = (window != null) ? window.Handle : ((IntPtr) 0);
		        IntPtr s = Marshal.StringToHGlobalAuto (title);
		
                	Raw = gtk_file_chooser_dialog_new(s, p, (int) action, IntPtr.Zero);
			Marshal.FreeHGlobal (s);
		}


		[DllImport("libgtk-x11-2.0.so.0")]
		static extern IntPtr gtk_file_chooser_dialog_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = gtk_file_chooser_dialog_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
    		}
		
		[DllImport("libgtk-x11-2.0.so.0")]
		static extern IntPtr gtk_file_chooser_get_filename(IntPtr raw);

		[DllImport("libgtk-x11-2.0.so.0")]
		static extern void gtk_file_chooser_set_filename(IntPtr raw, IntPtr filename);


		public string Filename {
			get  {
				IntPtr raw_ret = gtk_file_chooser_get_filename(Handle);
				string ret = Marshal.PtrToStringAuto (raw_ret);
				return ret;
			}
			set  {
			        IntPtr ins = Marshal.StringToHGlobalAuto (value);
				gtk_file_chooser_set_filename(Handle, ins);
				Marshal.FreeHGlobal (ins);
			}
		}

        }
}
