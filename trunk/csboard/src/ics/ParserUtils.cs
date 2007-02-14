using System;

namespace CsBoard {
	namespace ICS {
	public class ParserException:Exception
	{
		public ParserException (string str):base (str)
		{
		}
	}

	public class ParserUtils {

		public static void SkipWhitespace (byte[]buffer, ref int idx,
					    int end)
		{
			while (buffer[idx] == ' ' && idx < end)
				idx++;
		}

		public static void ReadNameValue (byte[]buffer, ref int idx, int end,
					   out string name, out string val)
		{
			ReadWord (buffer, '=', ref idx, end, out name);
			idx++;
			ReadWord (buffer, ' ', ref idx, end, out val);
		}

		public static void ReadWord (byte[]buffer, char delim, ref int idx,
				      int end, out string word)
		{
			System.Text.Decoder decoder =
				System.Text.Encoding.UTF8.GetDecoder ();
			for (int i = idx; i < end; i++)
			  {
				  if (buffer[i] == delim)
				    {
					    char[] chrs = new char[i - idx];
					    decoder.GetChars (buffer, idx,
							      i - idx, chrs,
							      0);
					    word = new string (chrs);
					    idx = i;
					    return;
				    }
			  }

			char[] chars = new char[end - idx];
			decoder.GetChars (buffer, idx, end - idx, chars, 0);
			word = new string (chars);
			idx = end;
		}
	}
	}
}
