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
using System.Text;
using System.Collections;
using Mono.Unix;

namespace Chess
{
	namespace Game
	{

		public struct MoveInfo
		{
			public int src_rank, src_file, dest_rank, dest_file;
			public ChessPiece movedPiece;
			public bool special_move;

			public void SetInfo (int sr, int sf, int dr, int df,
					     bool spl_move)
			{
				src_rank = sr;
				src_file = sf;
				dest_rank = dr;
				dest_file = df;
				special_move = spl_move;
			}

			public void SetMovement (int sr, int sf, int dr,
						 int df)
			{
				SetInfo (sr, sf, dr, df, false);
			}
		}

		public class ChessGamePlayer
		{
			int gameStatus;

			public const int GAME_STATUS_NONE = 0;
			public const int GAME_STATUS_PLAYING = 1;
			public const int GAME_STATUS_END = 2;

			public enum CastleType
			{
				SHORT_CASTLE,
				LONG_CASTLE
			};

			public ChessSide whites, blacks;
			public ChessPiece[,] positions;

			MoveInfo lastMoveInfo;

			public MoveInfo LastMoveInfo
			{
				get
				{
					return lastMoveInfo;
				}
			}

			ColorType turn;

			private void FlipTurn ()
			{
				if (turn == ColorType.WHITE)
					turn = ColorType.BLACK;
				else
					turn = ColorType.WHITE;
			}

			private ChessGamePlayer ()
			{
				gameStatus = GAME_STATUS_NONE;
				positions = new ChessPiece[8, 8];
			}

			private void updatePositions ()
			{
				for (int i = 0; i < 8; i++)
					for (int j = 0; j < 8; j++)
						positions[i, j] = null;

				IList pieces = whites.allPieces ();

				foreach (ChessPiece piece in pieces)
				{
					positions[piece.Rank, piece.File] =
						piece;
				}

				pieces = blacks.allPieces ();

				foreach (ChessPiece piece in pieces)
				{
					positions[piece.Rank, piece.File] =
						piece;
				}
			}

			private void StartGame (ChessSide whites,
						ChessSide blacks)
			{
				/* clear */
				for (int i = 0; i < 8; i++)
					for (int j = 0; j < 8; j++)
						positions[i, j] = null;

				turn = ColorType.WHITE;
				this.whites = whites;
				this.blacks = blacks;
				updatePositions ();

				gameStatus = GAME_STATUS_PLAYING;
			}

			public int GameStatus
			{
				get
				{
					return gameStatus;
				}
			}

			public static ColorType GetColor (int i, int j)
			{
				i = i & 0x1;
				j = j & 0x1;

				return (i ^ j) ==
					0 ? ColorType.WHITE : ColorType.BLACK;
			}

			protected void getSource (PieceType pieceType,
						  out int rank, out int file,
						  ChessSide side,
						  int dest_rank,
						  int dest_file, string move)
			{
				rank = -1;
				file = -1;
				ChessPiece destPiece =
					positions[dest_rank, dest_file];
				int exchange = 0;
				if (destPiece != null)
					exchange =
						ChessBoardConstants.
						MOVE_EXCHANGE;

				if (pieceType == PieceType.KING)
				  {
					  ChessPiece cp = side.King;
					  rank = cp.Rank;
					  file = cp.File;
					  return;
				  }

				IList list = side.getPiecesOfType (pieceType);
				if (pieceType == PieceType.PAWN
				    && move.Length > 2)
					exchange =
						destPiece ==
						null ? ChessBoardConstants.
						MOVE_ENPASSANT :
						ChessBoardConstants.
						MOVE_EXCHANGE;

				ArrayList cands = new ArrayList ();
				foreach (ChessPiece cp in list)
				{
					if (cp.
					    isValidMove (dest_rank, dest_file,
							 positions,
							 exchange |
							 ChessBoardConstants.
							 MOVE_DEBUG))
					  {
						  cands.Add (cp);
					  }
				}

				if (cands.Count == 0)
				  {
					  Console.Error.
						  WriteLine
						  (GetPositionsString ());
					  throw new
						  InvalidMoveException
						  (Catalog.
						   GetString ("Invalid move ")
						   + move +
						   Catalog.
						   GetString
						   (". Couldn't find any candidate for this."));
				  }

				if (cands.Count == 1)
				  {
					  ChessPiece cp =
						  (ChessPiece) cands[0];
					  rank = cp.Rank;
					  file = cp.File;
					  return;
				  }

				// Ambiguity
				// More than one candidates
				char ch;
				int movelen = move.Length;
				if (cands.Count == 2)
				  {
					  if (movelen < 3)
						  throw new
							  InvalidMoveException
							  (Catalog.
							   GetString
							   ("Insufficient chars in move ")
							   + move);
					  ch = move[move.Length - 3];
					  if (ch >= 'a' && ch <= 'h')
					    {
						    // find the piece with matching file
						    foreach (ChessPiece temp
							     in cands)
						    {
							    if (ch ==
								'a' +
								temp.File)
							      {
								      rank = temp.Rank;
								      file = temp.File;
								      return;
							      }
						    }
					    }
					  else if (ch >= '1' && ch <= '8')
					    {
						    foreach (ChessPiece temp
							     in cands)
						    {
							    if (ch ==
								'1' +
								temp.Rank)
							      {
								      rank = temp.Rank;
								      file = temp.File;
								      return;
							      }
						    }
					    }
					  else
					    {
						    throw new
							    InvalidMoveException
							    (Catalog.
							     GetString
							     ("Invalid move ")
							     + move);
					    }
				  }
				else
				  {
					  if (movelen < 4)
						  throw new
							  InvalidMoveException
							  (Catalog.
							   GetString
							   ("Invalid move ") +
							   move);
					  char file_ch =
						  move[move.Length - 3];
					  char rank_ch =
						  move[move.Length - 4];
					  rank = rank_ch - 'a';
					  file = file_ch - '1';
				  }
			}

			/*
			 * The callers must ensure that this will be called only if the opponent
			 * pawn advanced in the previous move.
			 */
			protected bool move_enpass (int i1, int j1, int i2,
						    int j2)
			{
				ChessPiece cp = positions[i1, j1];
				ChessPiece pawn = positions[i1, j2];
				if (cp == null || pawn == null)
					return false;
				if (cp.Type != PieceType.PAWN
				    || pawn.Type != PieceType.PAWN
				    || !pawn.Equals (lastMoveInfo.movedPiece))
					return false;

				if (j2 - j1 != 1 && j2 - j1 != -1)
					return false;

				if (cp.Color == ColorType.WHITE && i2 != 5)
					return false;
				if (cp.Color == ColorType.BLACK && i2 != 2)
					return false;

				if (positions[i2, j2] != null)
					return false;

				removePiece (i1, j2);
				pawn.removeFromSide ();
				removePiece (i1, j1);
				setPiece (cp, i2, j2);
				lastMoveInfo.movedPiece = cp;
				lastMoveInfo.SetInfo (i1, j1, i2, j2, true);

				FlipTurn ();
				return true;
			}

			protected bool move (int i1, int j1, int i2, int j2)
			{
				return move (i1, j1, i2, j2,
					     PromotionType.NONE);
			}

			public void startGame ()
			{
				gameStatus = GAME_STATUS_PLAYING;
			}

			public void stopGame ()
			{
				gameStatus = GAME_STATUS_END;
			}
			protected bool move (int i1, int j1, int i2, int j2,
					     PromotionType
					     promoted_piece_type)
			{
				if (gameStatus != GAME_STATUS_PLAYING)
				  {
					  return false;
				  }
				//                CBSquare cbs = new CBSquare( i1, j1 );
				ChessPiece piece = positions[i1, j1];
				if (piece == null)
				  {
					  // No source piece
					  return false;
				  }

				// Not your turn!
				if (piece.Color != turn)
				  {
					  return false;
				  }

				/* special case for enpass */
				if (piece.Type == PieceType.PAWN && j1 != j2
				    && positions[i2, j2] == null)
					return move_enpass (i1, j1, i2, j2);

				bool promotion_case =
					ChessUtils.isPawnPromotion (piece,
								    i2);
				if (piece.Type == PieceType.PAWN
				    && promotion_case
				    && promoted_piece_type ==
				    PromotionType.NONE)
					// promotion case but promotion type not specified
					return false;

				/* Now change the position */
				piece = removePiece (i1, j1);
				if (positions[i2, j2] != null)
				  {
					  positions[i2, j2].removeFromSide ();
					  removePiece (i2, j2);
				  }
				/* special case for pawn */
				if (promotion_case)
				  {
					  ChessPiece promoted_piece =
						  createPiece ((PieceType)
							       promoted_piece_type,
							       piece.Color,
							       i2, j2);
					  piece.removeFromSide ();
					  piece = promoted_piece;
					  piece.addToSide ();
				  }
				setPiece (piece, i2, j2);
				lastMoveInfo.movedPiece = piece;
				lastMoveInfo.SetInfo (i1, j1, i2, j2,
						      promotion_case);

				FlipTurn ();
				return true;
			}

			public static void NormalizeMove (string str,
							  out string
							  normalized_str)
			{
				if (str.EndsWith ("N")
				    && !str.EndsWith ("=N"))
				  {
					  /* novelty */
					  str = str.Substring (0,
							       str.Length -
							       1);
				  }

				int extra_chars = 0;
				for (int i = str.Length - 1; i >= 0; i--)
				  {
					  char ch = str[i];
					  if ("!?#+-".IndexOf (ch) < 0)
						  break;
					  extra_chars++;
				  }
				if (extra_chars > 0)
				  {	// strip extra chars
					  str = str.Substring (0,
							       str.Length -
							       extra_chars);
				  }

				if (str.IndexOf ('-') > 0)
				  {
					  string lower = str.ToLower ();
					  if (lower.Equals ("o-o-o")
					      || lower.Equals ("o-o"))
					    {
						    normalized_str = lower;
						    return;
					    }
				  }

				int strlen = str.Length;
				if (strlen > 3 && str[strlen - 3] == 'x')
				  {	// remove the 'x' denoting an exchange
					  str = str.Substring (0,
							       strlen - 3) +
						  str.Substring (strlen - 2);
				  }

				normalized_str = str;
			}

			public bool Move (string str)
			{
				NormalizeMove (str, out str);
				bool result = false;
				if (str.IndexOf ("-") > 0)
				  {
					  if (str.Equals ("o-o"))
					    {
						    result = castle (turn ==
								     ColorType.
								     WHITE ?
								     whites :
								     blacks,
								     turn,
								     CastleType.
								     SHORT_CASTLE);
					    }
					  else if (str.Equals ("o-o-o"))
					    {
						    result = castle (turn ==
								     ColorType.
								     WHITE ?
								     whites :
								     blacks,
								     turn,
								     CastleType.
								     LONG_CASTLE);
					    }

					  return result;
				  }

				PromotionType promotion_type;
				GetPromotionInfo (ref str,
						  out promotion_type);

				PieceType pieceType =
					ChessUtils.getPiece (str);
				int dest_rank, dest_file;
				if (!ChessUtils.
				    getSquare (str, out dest_rank,
					       out dest_file))
				  {
					  return false;
				  }

				int src_rank, src_file;
				getSource (pieceType, out src_rank,
					   out src_file,
					   turn ==
					   ColorType.WHITE ? whites : blacks,
					   dest_rank, dest_file, str);
				if (src_rank == -1 || src_file == -1)
					return false;

				return move (src_rank, src_file, dest_rank,
					     dest_file, promotion_type);
			}

			private void GetPromotionInfo (ref string move,
						       out PromotionType type)
			{
				type = PromotionType.NONE;

				int len = move.Length;
				if (len < 2 || move[len - 2] != '='
				    || Char.IsNumber (move[len - 1]))
					return;

				string str;
				str = move;

				char last_char = str[len - 1];
				switch (last_char)
				  {
				  case 'Q':
					  type = PromotionType.QUEEN;
					  break;
				  case 'R':
					  type = PromotionType.ROOK;
					  break;
				  case 'B':
					  type = PromotionType.BISHOP;
					  break;
				  case 'N':
					  type = PromotionType.KNIGHT;
					  break;
				  default:
					  throw new
						  InvalidMoveException
						  (Catalog.
						   GetString ("Invalid move ")
						   + str +
						   Catalog.
						   GetString
						   (". Invalid promotion type: ")
						   + last_char);
				  }

				move = str.Substring (0, len - 2);
			}

			public ColorType Turn
			{
				get
				{
					return turn;
				}
			}

			private ChessPiece removePiece (int i, int j)
			{
				ChessPiece cp = positions[i, j];
				positions[i, j] = null;
				return cp;
			}

			private void setPiece (ChessPiece piece, int i, int j)
			{
				if (piece != null)
					piece.setPosition (i, j);
				positions[i, j] = piece;
			}

			public bool castle (ChessSide side, ColorType color,
					    CastleType castle)
			{
				if (!side.King.CanCastle)
				  {
					  debug (Catalog.
						 GetString ("Cant castle (") +
						 side.King);
					  return false;
				  }

				if (side.King.File !=
				    ChessBoardConstants.FILE_E)
				  {
					  debug (Catalog.
						 GetString
						 ("King not available ") +
						 side.King);
					  return false;
				  }

				int file;
				if (castle == CastleType.LONG_CASTLE)
				  {
					  file = ChessBoardConstants.FILE_A;
				  }
				else
				  {
					  file = ChessBoardConstants.FILE_H;
				  }

				int rank;
				if (color == ColorType.WHITE)
					rank = 0;
				else
					rank = 7;

				if (positions[rank, file] == null)
				  {
					  debug (Catalog.
						 GetString
						 ("Rook not available"));
					  return false;
				  }

				int i1, i2;
				if (castle == CastleType.LONG_CASTLE)
				  {
					  i1 = ChessBoardConstants.FILE_B;
					  i2 = ChessBoardConstants.FILE_E;
				  }
				else
				  {
					  i1 = ChessBoardConstants.FILE_F;
					  i2 = ChessBoardConstants.FILE_H;
				  }

				for (int i = i1; i < i2; i++)
				  {
					  if (positions[rank, i] != null)
					    {	// some pieces blocking.. cannot castle
						    return false;
					    }
				  }

				IList attackers = new ArrayList ();
				ChessPiece.
					getAttackers ((color ==
						       ColorType.
						       WHITE ? whites :
						       blacks),
						      (color ==
						       ColorType.
						       WHITE ? blacks :
						       whites), rank,
						      ChessBoardConstants.
						      FILE_E, positions, null,
						      attackers);
				// check if king is under attack.
				// cannot castle in that case
				if (attackers.Count > 0)
				  {
					  return false;
				  }

				if (castle == CastleType.LONG_CASTLE)
					i1 = ChessBoardConstants.FILE_C;
				for (int i = i1; i < i2; i++)
				  {
					  attackers.Clear ();
					  ChessPiece.
						  getAttackers ((color ==
								 ColorType.
								 WHITE ?
								 whites :
								 blacks),
								(color ==
								 ColorType.
								 WHITE ?
								 blacks :
								 whites),
								rank, i,
								positions,
								null,
								attackers);
					  if (attackers.Count > 0)
					    {	// king will be under attack in this case
						    return false;
					    }
				  }

				if (castle == CastleType.SHORT_CASTLE)
				  {
					  setPiece (side.King, rank,
						    ChessBoardConstants.
						    FILE_G);
					  setPiece (positions
						    [rank,
						     ChessBoardConstants.
						     FILE_H], rank,
						    ChessBoardConstants.
						    FILE_F);
					  removePiece (rank,
						       ChessBoardConstants.
						       FILE_E);
					  removePiece (rank,
						       ChessBoardConstants.
						       FILE_H);

					  lastMoveInfo.movedPiece = side.King;
					  lastMoveInfo.SetInfo (rank,
								ChessBoardConstants.
								FILE_E, rank,
								ChessBoardConstants.
								FILE_G, true);
				  }
				else
				  {
					  setPiece (side.King, rank,
						    ChessBoardConstants.
						    FILE_C);
					  setPiece (positions
						    [rank,
						     ChessBoardConstants.
						     FILE_A], rank,
						    ChessBoardConstants.
						    FILE_D);
					  removePiece (rank,
						       ChessBoardConstants.
						       FILE_E);
					  removePiece (rank,
						       ChessBoardConstants.
						       FILE_A);

					  lastMoveInfo.movedPiece = side.King;
					  lastMoveInfo.SetInfo (rank,
								ChessBoardConstants.
								FILE_E, rank,
								ChessBoardConstants.
								FILE_C, true);
				  }

				FlipTurn ();
				return true;
			}

			public void debug (Object obj)
			{
				Console.WriteLine (Catalog.
						   GetString
						   ("[ChessGamePlayer] ") +
						   obj);
			}

			// images[blksq/whtsq][side][idx] - idx = {KING, QUEEN, ROOK, KNIGHT, BISHOP}
			public ChessPiece createPiece (PieceType type,
						       ColorType color,
						       int rank, int file)
			{
				ChessPiece piece = null;
				ChessSide myside, oppside;
				if (color == ColorType.WHITE)
				  {
					  myside = whites;
					  oppside = blacks;
				  }
				else
				  {
					  myside = blacks;
					  oppside = blacks;
				  }

				switch (type)
				  {
				  case PieceType.KING:
					  piece = new King (color, rank, file,
							    myside, oppside);
					  break;
				  case PieceType.QUEEN:
					  piece = new Queen (color, rank,
							     file, myside,
							     oppside);
					  break;
				  case PieceType.ROOK:
					  piece = new Rook (color, rank, file,
							    myside, oppside);
					  break;
				  case PieceType.KNIGHT:
					  piece = new Knight (color, rank,
							      file, myside,
							      oppside);
					  break;
				  case PieceType.BISHOP:
					  piece = new Bishop (color, rank,
							      file, myside,
							      oppside);
					  break;
				  default:
					  return null;
				  }

				return piece;
			}

			public static bool isMate (King king,
						   ChessPiece[,] positions)
			{
				for (int i = 0, rank = king.Rank - 1; i < 3;
				     i++, rank++)
				  {
					  for (int j = 0, file =
					       king.File - 1; j < 3;
					       j++, file++)
					    {
						    if (rank < 0 || rank > 7
							|| file < 0
							|| file > 7
							|| rank == file)
							    continue;
						    if (king.
							isValidMove (rank,
								     file,
								     positions,
								     ChessBoardConstants.
								     MOVE_EXCHANGE))
							    return false;
					    }
				  }

				return true;
			}

			public void PrintPositions ()
			{
				Console.WriteLine (Catalog.
						   GetString ("Turn: ") +
						   (turn ==
						    ColorType.WHITE ? Catalog.
						    GetString ("WHITE") :
						    Catalog.
						    GetString ("BLACK")));
				Console.WriteLine (GetPositionsString ());
				Console.WriteLine ("================\n");
				Console.WriteLine (Catalog.
						   GetString ("Whites:\n") +
						   whites);
				Console.WriteLine (Catalog.
						   GetString ("Blacks:\n") +
						   blacks);
			}

			public string GetPositionsString ()
			{
				StringBuilder buffer = new StringBuilder ();
				buffer.Append ("\n");
				/* Note that the rank is traversed in reverse order so that 0 comes at the end
				 * i.e., white pieces comes at the bottom
				 */
				for (int i = 7; i >= 0; i--)
				  {
					  for (int j = 0; j < 8; j++)
					    {
						    if (positions[i, j] ==
							null)
							    buffer.Append
								    (" .");
						    else if (positions[i, j].
							     Type ==
							     PieceType.PAWN)
							    buffer.Append
								    (positions
								     [i,
								      j].
								     Color ==
								     ColorType.
								     WHITE ?
								     " P" :
								     " p");
						    else
							    buffer.Append (" "
									   +
									   (positions
									    [i,
									     j].
									    Color
									    ==
									    ColorType.
									    WHITE
									    ?
									    positions
									    [i,
									     j].
									    getNotationPrefix
									    ()
									    :
									    positions
									    [i,
									     j].
									    getNotationPrefix
									    ().
									    ToLower
									    ()));
					    }
					  buffer.Append ("\n");
				  }
				buffer.Append ("\n");
				return buffer.ToString ();
			}

			public ArrayList GetPosition ()
			{
				ArrayList list = new ArrayList ();
				list.Add ("");
				list.Add ("white  KQkq");

				StringBuilder buffer = new StringBuilder ();
				/* Note that the rank is traversed in reverse order so that 0 comes at the end
				 * i.e., white pieces comes at the bottom
				 */
				for (int i = 7; i >= 0; i--)
				  {
					  buffer.Remove (0, buffer.Length);
					  for (int j = 0; j < 8; j++)
					    {
						    if (positions[i, j] ==
							null)
							    buffer.Append
								    (".");
						    else if (positions[i, j].
							     Type ==
							     PieceType.PAWN)
							    buffer.Append
								    (positions
								     [i,
								      j].
								     Color ==
								     ColorType.
								     WHITE ?
								     "P" :
								     "p");
						    else
							    buffer.Append ((positions[i, j].Color == ColorType.WHITE ? positions[i, j].getNotationPrefix () : positions[i, j].getNotationPrefix ().ToLower ()));
						    buffer.Append (" ");
					    }
					  list.Add (buffer.ToString ());
				  }

				list.Add ("");
				return list;
			}

/*
			public static ChessGamePlayer CreatePlayer ()
			{
				ChessSide white, black;
				ChessSide.GetDefaultSides (out white,
							   out black);
				ChessGamePlayer game = new ChessGamePlayer ();
				game.StartGame (white, black);
				return game;
			}
*/
			public static ChessGamePlayer CreatePlayer ()
			{
				string fen_str =
					"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
				return CreateFromFEN (fen_str);
			}

			public string GetPositionAsFEN ()
			{
				StringBuilder buffer = new StringBuilder ();
				/* Note that the rank is traversed in reverse order so that 0 comes at the end
				 * i.e., white pieces comes at the bottom
				 */
				for (int i = 7; i >= 0; i--)
				  {
					  int count = 0;
					  for (int j = 0; j < 8; j++)
					    {
						    if (positions[i, j] ==
							null)
						      {
							      count++;
							      continue;
						      }
						    if (count > 0)
						      {
							      buffer.Append
								      (count);
							      count = 0;
						      }
						    buffer.Append
							    (GetFENCharForPiece
							     (positions
							      [i, j]));
					    }
					  if (count > 0)
						  buffer.Append (count);
					  if (i != 1)
						  buffer.Append ('/');
				  }

				buffer.Append (' ');
				buffer.Append (String.
					       Format ("{0} ",
						       turn ==
						       ColorType.
						       WHITE ? 'w' : 'b'));
				if (!whites.King.CanCastle
				    && !blacks.King.CanCastle)
					buffer.Append ('-');
				if (whites.King.CanCastle)
					buffer.Append ("KQ");
				if (blacks.King.CanCastle)
					buffer.Append ("kq");
				buffer.Append (' ');

				buffer.Append ("- ");	// enpass case. ignore for now
				buffer.Append ("0 ");

				return buffer.ToString ();
			}

			private static char GetFENCharForPiece (ChessPiece
								piece)
			{
				char ch = ' ';
				switch (piece.Type)
				  {
				  case PieceType.PAWN:
					  ch = 'p';
					  break;
				  case PieceType.ROOK:
					  ch = 'r';
					  break;
				  case PieceType.KNIGHT:
					  ch = 'n';
					  break;
				  case PieceType.BISHOP:
					  ch = 'p';
					  break;
				  case PieceType.QUEEN:
					  ch = 'q';
					  break;
				  case PieceType.KING:
					  ch = 'k';
					  break;
				  }

				if (piece.Color == ColorType.WHITE)
					return Char.ToUpper (ch);

				return ch;
			}

			public static ArrayList GetDefaultPosition ()
			{
				ArrayList list = new ArrayList ();
				list.Add ("");
				list.Add ("white  KQkq");
				list.Add ("r n b q k b n r ");
				list.Add ("p p p p p p p p ");
				list.Add (". . . . . . . . ");
				list.Add (". . . . . . . . ");
				list.Add (". . . . . . . . ");
				list.Add (". . . . . . . . ");
				list.Add ("P P P P P P P P ");
				list.Add ("R N B Q K B N R ");
				list.Add ("");
				return list;
			}

			public static bool IsDefaultPosition(ArrayList list) {
				ArrayList deflist = GetDefaultPosition();
				if(deflist.Count != list.Count)
					return false;
				int i;
				for(i = 0; i < deflist.Count; i++)
					if(!deflist[i].Equals(list[i]))
						return false;
				return true;
			}

			public static ChessGamePlayer CreateFromFEN (string
								     fen)
			{
				PositionInfo info = new PositionInfo (fen);
				string[]lines = info.position_str.Split ('/');
				if (lines.Length != 8)
					throw new
						ChessException
						(Catalog.
						 GetString
						 ("Invalid number tokens in the FEN position"));
				ChessSide whites =
					new ChessSide (ColorType.WHITE);
				ChessSide blacks =
					new ChessSide (ColorType.BLACK);
				for (int i = 0; i < lines.Length; i++)
				  {
					  string line = lines[i];
					  int file = 0;
					  foreach(char ch in line) {
						    if (Char.IsNumber (ch))
						      {
							      file += ch - '0';
							      continue;
						      }
						    int rank = 7 - i;
						    ChessPiece piece;
						    GetPieceForFENChar (ch,
									whites,
									blacks,
									rank,
									file,
									out
									piece);
						    piece.addToSide ();
						    file++;
					    }
				  }
				whites.King.CanCastle = info.WhiteCanCastle;
				blacks.King.CanCastle = info.BlackCanCastle;
				ChessGamePlayer game = new ChessGamePlayer ();
				game.turn = info.Turn;
				game.StartGame (whites, blacks);
				return game;
			}

			private static void GetPieceForFENChar (char fench,
								ChessSide
								whites,
								ChessSide
								blacks,
								int rank,
								int file,
								out ChessPiece
								piece)
			{
				ColorType color;
				ChessSide myside, oppside;
				if (Char.IsUpper (fench))
				  {
					  color = ColorType.WHITE;
					  myside = whites;
					  oppside = blacks;
				  }
				else
				  {
					  color = ColorType.BLACK;
					  myside = blacks;
					  oppside = whites;
				  }

				char ch = Char.ToLower (fench);
				switch (ch)
				  {
				  case 'p':
					  piece = new Pawn (color, rank, file,
							    myside, oppside);
					  break;
				  case 'k':
					  piece = new King (color, rank, file,
							    myside, oppside);
					  break;
				  case 'q':
					  piece = new Queen (color, rank,
							     file, myside,
							     oppside);
					  break;
				  case 'b':
					  piece = new Bishop (color, rank,
							      file, myside,
							      oppside);
					  break;
				  case 's':
					  piece = new Knight (color, rank,
							      file, myside,
							      oppside);
					  break;
				  case 'n':
					  piece = new Knight (color, rank,
							      file, myside,
							      oppside);
					  break;
				  case 'r':
					  piece = new Rook (color, rank, file,
							    myside, oppside);
					  break;
				  default:
					  throw new
						  ChessException
						  (Catalog.
						   GetString
						   ("Invalid piece type ") +
						   ch);
				  }

			}

			struct PositionInfo
			{
				// each rank is described starting from rank 8 and ending with rank 1.
				// for each rank, the contents of each square are described from file 'a' through
				// file 'h'.
				public string position_str;
				// active color - "w" means white moves next. "b" means black moves next.
				public string active_str;
				// if neither side can castle, this is "-". Otherwise this has one or more letters:
				//   K - white can castle on king side
				//   Q - white can castle on queen side
				//   k - black can castle on king side
				//   q - black can castle on queen side
				public string castling_available_str;
				// if there is no enpassant target square, this is "-". If a pawn has just made a
				// 2-square move, this is the position "behind" the pawn.
				public string enpassant_target_str;
				// number of half moves since the last pawn advance or capture. this is used
				// to determine if a draw can be claimed under the fifty move rule
				public string halfmove_clock_str;
				// full move number - the number of full moves. this starts at 1 and is incremented
				// after blacks move.
				public string fullmove_number_str;


				public bool WhiteCanCastle;
				public bool BlackCanCastle;

				public ColorType Turn;

				public PositionInfo (string fen)
				{
					string[]lines = fen.Split (' ');
					position_str = lines[0];
					active_str = lines[1];
					if (active_str.ToLower ().
					    Equals ("b"))
						Turn = ColorType.BLACK;
					else
						Turn = ColorType.WHITE;

					castling_available_str = lines[2];
					WhiteCanCastle = false;
					BlackCanCastle = false;
					if (!castling_available_str.
					    Equals ("-"))
					  {
						  for (int i = 0;
						       i <
						       castling_available_str.
						       Length; i++)
						    {
							    char ch =
								    castling_available_str
								    [i];
							    if (ch == 'K'
								|| ch == 'Q')
								    WhiteCanCastle
									    =
									    true;
							    if (ch == 'k'
								|| ch == 'q')
								    BlackCanCastle
									    =
									    true;
						    }
					  }
					enpassant_target_str =
						lines[3].ToLower ();
					halfmove_clock_str = lines[4];
					fullmove_number_str = lines[5];
				}
			}
		}
	}
}
