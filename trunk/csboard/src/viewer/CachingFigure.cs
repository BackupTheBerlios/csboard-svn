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
// Copyright (C) 2006 Ravi Kiran UVS

using System;
using System.IO;
using System.Collections;

namespace CsBoard
{
	namespace Viewer
	{
		public class CachingFigure:Figure
		{
			int size;
			Hashtable cache;

			public CachingFigure ():base ()
			{
				cache = new Hashtable ();
			}

			static CachingFigure instance;
			public static Figure Instance
			{
				get
				{
					if (instance == null)
						instance =
							new CachingFigure ();
					return instance;
				}
			}

			public override void SetSize (int size)
			{
				if (this.size == size)
					return;
				base.SetSize (size);
			}

			private static void CreateFile (System.Reflection.
							Assembly asm,
							string file,
							out string tmpfile)
			{
				Stream asmstream =
					asm.GetManifestResourceStream (file);
				tmpfile = Path.GetTempFileName ();
				Stream stream =
					new FileStream (tmpfile,
							FileMode.Append,
							FileAccess.Write);
				byte[]buf = new byte[1024];
				int nread;
				while ((nread =
					asmstream.Read (buf, 0,
							buf.Length)) > 0) {
					stream.Write (buf, 0, nread);
				}
				stream.Close ();
			}

			protected override Gdk.Pixbuf GetPixbuf (string file,
								 int size)
			{
				string filename = null;
				if (cache.ContainsKey (file))
					filename = (string) cache[file];
				if (filename == null
				    || !File.Exists (filename)) {
					System.Reflection.Assembly asm =
						System.Reflection.Assembly.
						GetExecutingAssembly ();
					CreateFile (asm, file, out filename);
					cache[file] = filename;
				}
				return Rsvg.Tool.
					PixbufFromFileAtSize (filename, size,
							      size);
			}
		}
	}
}
