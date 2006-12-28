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


// FIXME: implement caching

namespace CsBoard
{

	using Gdk;
	using Gtk;
	using System;
	using System.IO;
	using System.Collections;

	public enum FigureType
	{
		WhiteRook,
		WhiteKing,
		WhiteQueen,
		WhiteBishop,
		WhitePawn,
		WhiteKnight,
		BlackRook,
		BlackKing,
		BlackQueen,
		BlackBishop,
		BlackPawn,
		BlackKnight,
		None
	};

	public class Figure
	{

		protected ArrayList pixbufs;
		SvgFileManager svgFileManager;

		int cursize = -1;

		  string[] files = {
		"white-rook.svg",
				"white-king.svg",
				"white-queen.svg",
				"white-bishop.svg",
				"white-pawn.svg",
				"white-knight.svg",
				"black-rook.svg",
				"black-king.svg",
				"black-queen.svg",
				"black-bishop.svg",
				"black-pawn.svg", "black-knight.svg"};

		public Figure ()
		{
			svgFileManager = SvgFileManager.Instance;
		}

		public Pixbuf GetPixbuf (FigureType type)
		{
			return (Pixbuf) pixbufs[(int) type];
		}

		public virtual void SetSize (int s)
		{
			s = Math.Max (s, 10);
			if(cursize == s)
				return;
			cursize = s;

			pixbufs = new ArrayList ();

			foreach (string filename in files) {
				pixbufs.Add (GetPixbuf (filename, s));
			}
		}

		protected virtual Gdk.Pixbuf GetPixbuf (string filename,
							int size)
		{
			return Rsvg.Tool.PixbufFromFileAtSize (svgFileManager.
							       GetFile
							       (filename),
							       size, size);
		}
	}

	public class SvgFileManager
	{
		Hashtable cache;

		struct FileDetails
		{
			public string filepath;
			public int length;
		}

		static SvgFileManager instance;

		public static SvgFileManager Instance
		{
			get
			{
				if (instance == null)
					instance = new SvgFileManager ();
				return instance;
			}
		}

		private SvgFileManager ()
		{
			cache = new Hashtable ();
		}

		public string GetFile (string filename)
		{
			string path;
			if (!GetFilePath (filename, out path)) {
				FileDetails details;
				GetFileFromAsm (filename, out details);
				cache[filename] = details;
				path = details.filepath;
			}

			return path;
		}

		private bool GetFilePath (string filename, out string path)
		{
			path = null;
			if (!cache.ContainsKey (filename))
				return false;
			FileDetails details = (FileDetails) cache[filename];
			if (!CheckFileDetails (details))
				return false;

			path = details.filepath;
			return true;
		}

		private bool CheckFileDetails (FileDetails details)
		{
			return File.Exists (details.filepath);
		}

		private void GetFileFromAsm (string filename,
					     out FileDetails details)
		{
			System.Reflection.Assembly asm =
				System.Reflection.Assembly.
				GetExecutingAssembly ();
			Stream asmstream =
				asm.GetManifestResourceStream (filename);
			string tmpfile = Path.GetTempFileName ();
			Stream stream =
				new FileStream (tmpfile, FileMode.Append,
						FileAccess.Write);
			byte[]buf = new byte[1024];
			int nread;
			int ntotal = 0;
			while ((nread =
				asmstream.Read (buf, 0, buf.Length)) > 0) {
				stream.Write (buf, 0, nread);
				ntotal += nread;
			}
			asmstream.Close ();
			stream.Close ();
			details.filepath = tmpfile;
			details.length = ntotal;
		}

		~SvgFileManager () {
			foreach (DictionaryEntry de in cache) {
				FileDetails details = (FileDetails) de.Value;
				try {
					File.Delete (details.filepath);
				}
				finally {
				}
			}
		}
	}
}
