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

namespace Chess
{
	namespace Parser
	{
		public class PGNNAG
		{
			static string[] values = {
				"null annotation",	// 0
				"good move",	// (traditional "!")", // 1
				"poor move",	// (traditional "?")", // 2
				"very good move",	// (traditional "!!")", // 3
				"very poor move",	// (traditional "??")", // 4
				"speculative move",	// (traditional "!?")", // 5
				"questionable move",	// (traditional "?!")", // 6
				"forced move (all others lose quickly)",	// 7
				"singular move (no reasonable alternatives)",	// 8
				"worst move",	// 9
				"drawish position",	// 10
				"equal chances, quiet position",	// 11
				"equal chances, active position",	// 12
				"unclear position",	// 13
				"White has a slight advantage",	// 14
				"Black has a slight advantage",	// 15
				"White has a moderate advantage",	// 16
				"Black has a moderate advantage",	// 17
				"White has a decisive advantage",	// 18
				"Black has a decisive advantage",	// 19
				"White has a crushing advantage (Black should resign)",	// 20
				"Black has a crushing advantage (White should resign)",	// 21
				"White is in zugzwang",	// 22
				"Black is in zugzwang",	// 23
				"White has a slight space advantage",	// 24
				"Black has a slight space advantage",	// 25
				"White has a moderate space advantage",	// 26
				"Black has a moderate space advantage",	// 27
				"White has a decisive space advantage",	// 28
				"Black has a decisive space advantage",	// 29
				"White has a slight time (development) advantage",	// 30
				"Black has a slight time (development) advantage",	// 31
				"White has a moderate time (development) advantage",	// 32
				"Black has a moderate time (development) advantage",	// 33
				"White has a decisive time (development) advantage",	// 34
				"Black has a decisive time (development) advantage",	// 35
				"White has the initiative",	// 36
				"Black has the initiative",	// 37
				"White has a lasting initiative",	// 38
				"Black has a lasting initiative",	// 39
				"White has the attack",	// 40
				"Black has the attack",	// 41
				"White has insufficient compensation for material deficit",	// 42
				"Black has insufficient compensation for material deficit",	// 43
				"White has sufficient compensation for material deficit",	// 44
				"Black has sufficient compensation for material deficit",	// 45
				"White has more than adequate compensation for material deficit",	// 46
				"Black has more than adequate compensation for material deficit",	// 47
				"White has a slight center control advantage",	// 48
				"Black has a slight center control advantage",	// 49
				"White has a moderate center control advantage",	// 50
				"Black has a moderate center control advantage",	// 51
				"White has a decisive center control advantage",	// 52
				"Black has a decisive center control advantage",	// 53
				"White has a slight kingside control advantage",	// 54
				"Black has a slight kingside control advantage",	// 55
				"White has a moderate kingside control advantage",	// 56
				"Black has a moderate kingside control advantage",	// 57
				"White has a decisive kingside control advantage",	// 58
				"Black has a decisive kingside control advantage",	// 59
				"White has a slight queenside control advantage",	// 60
				"Black has a slight queenside control advantage",	// 61
				"White has a moderate queenside control advantage",	// 62
				"Black has a moderate queenside control advantage",	// 63
				"White has a decisive queenside control advantage",	// 64
				"Black has a decisive queenside control advantage",	// 65
				"White has a vulnerable first rank",	// 66
				"Black has a vulnerable first rank",	// 67
				"White has a well protected first rank",	// 68
				"Black has a well protected first rank",	// 69
				"White has a poorly protected king",	// 70
				"Black has a poorly protected king",	// 71
				"White has a well protected king",	// 72
				"Black has a well protected king",	// 73
				"White has a poorly placed king",	// 74
				"Black has a poorly placed king",	// 75
				"White has a well placed king",	// 76
				"Black has a well placed king",	// 77
				"White has a very weak pawn structure",	// 78
				"Black has a very weak pawn structure",	// 79
				"White has a moderately weak pawn structure",	// 80
				"Black has a moderately weak pawn structure",	// 81
				"White has a moderately strong pawn structure",	// 82
				"Black has a moderately strong pawn structure",	// 83
				"White has a very strong pawn structure",	// 84
				"Black has a very strong pawn structure",	// 85
				"White has poor knight placement",	// 86
				"Black has poor knight placement",	// 87
				"White has good knight placement",	// 88
				"Black has good knight placement",	// 89
				"White has poor bishop placement",	// 90
				"Black has poor bishop placement",	// 91
				"White has good bishop placement",	// 92
				"Black has good bishop placement",	// 93
				"White has poor rook placement",	// 84
				"Black has poor rook placement",	// 85
				"White has good rook placement",	// 86
				"Black has good rook placement",	// 87
				"White has poor queen placement",	// 98
				"Black has poor queen placement",	// 99
				"White has good queen placement",	// 100
				"Black has good queen placement",	// 101
				"White has poor piece coordination",	// 102
				"Black has poor piece coordination",	// 103
				"White has good piece coordination",	// 104
				"Black has good piece coordination",	// 105
				"White has played the opening very poorly",	// 106
				"Black has played the opening very poorly",	// 107
				"White has played the opening poorly",	// 108
				"Black has played the opening poorly",	// 109
				"White has played the opening well",	// 110
				"Black has played the opening well",	// 111
				"White has played the opening very well",	// 112
				"Black has played the opening very well",	// 113
				"White has played the middlegame very poorly",	// 114
				"Black has played the middlegame very poorly",	// 115
				"White has played the middlegame poorly",	// 116
				"Black has played the middlegame poorly",	// 117
				"White has played the middlegame well",	// 118
				"Black has played the middlegame well",	// 119
				"White has played the middlegame very well",	// 120
				"Black has played the middlegame very well",	// 121
				"White has played the ending very poorly",	// 122
				"Black has played the ending very poorly",	// 123
				"White has played the ending poorly",	// 124
				"Black has played the ending poorly",	// 125
				"White has played the ending well",	// 126
				"Black has played the ending well",	// 127
				"White has played the ending very well",	// 128
				"Black has played the ending very well",	// 129
				"White has slight counterplay",	// 130
				"Black has slight counterplay",	// 131
				"White has moderate counterplay",	// 132
				"Black has moderate counterplay",	// 133
				"White has decisive counterplay",	// 134
				"Black has decisive counterplay",	// 135
				"White has moderate time control pressure",	// 136
				"Black has moderate time control pressure",	// 137
				"White has severe time control pressure",	// 138
				"Black has severe time control pressure"	// 139
			};
			byte value;
			public byte Value
			{
				get
				{
					return value;
				}
			}

			public PGNNAG (byte value)
			{
				this.value = value;
			}

			public override string ToString ()
			{
				return value >=
					values.Length ? value.
					ToString () : values[value];
			}

			public string Markup ()
			{
				string format =
					"<b><i><span foreground=\"{0}\">{1}</span></i></b>";
				if (value >= 1 && value <= 9)
				  {	// abt the move
					  return String.
						  Format
						  (format,
						   "#800000", ToString ());
				  }

				if (value >= 10 && value <= 135)
				  {	// current position
					  return String.
						  Format
						  (format,
						   "#008000", ToString ());
				  }
				if (value > 136 && value <= 139)
				  {	// time pressure
					  return String.
						  Format
						  (format,
						   "#000080", ToString ());
				  }

				return ToString ();
			}
		}
	}
}
