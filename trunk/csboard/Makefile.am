SUBDIRS= . src po data help resource

PARSER_SOURCES = \
	$(top_srcdir)/src/parser/PGNParserException.cs \
	$(top_srcdir)/src/parser/PGNTokenizer.cs \
	$(top_srcdir)/src/parser/PGNChessGame.cs \
	$(top_srcdir)/src/parser/PGNChessMove.cs \
	$(top_srcdir)/src/parser/PGNTag.cs \
	$(top_srcdir)/src/parser/PGNNAG.cs \
	$(top_srcdir)/src/parser/PGNGameLoader.cs \
	$(top_srcdir)/src/parser/PGNParser.cs

GAME_PLAYER_SOURCES = \
	$(top_srcdir)/src/game/ChessPiece.cs\
	$(top_srcdir)/src/game/ChessSide.cs\
	$(top_srcdir)/src/game/Rook.cs\
	$(top_srcdir)/src/game/Bishop.cs\
	$(top_srcdir)/src/game/Knight.cs\
	$(top_srcdir)/src/game/Queen.cs\
	$(top_srcdir)/src/game/King.cs\
	$(top_srcdir)/src/game/Pawn.cs\
	$(top_srcdir)/src/game/ChessBoardConstants.cs\
	$(top_srcdir)/src/game/ChessGamePlayer.cs\
	$(top_srcdir)/src/game/Utils.cs\
	$(top_srcdir)/src/game/ChessException.cs\
	$(top_srcdir)/src/game/InvalidMoveException.cs

VIEWER_SOURCES = \
	$(top_srcdir)/src/viewer/GameSession.cs \
	$(top_srcdir)/src/viewer/GameViewer.cs \
	$(top_srcdir)/src/viewer/ChessGameWidget.cs \
	$(top_srcdir)/src/viewer/ChessGameInfoWidget.cs \
	$(top_srcdir)/src/viewer/PositionSnapshot.cs \
	$(top_srcdir)/src/viewer/ViewerBoard.cs \
	$(top_srcdir)/src/viewer/GamesListWidget.cs

PLUGIN_SOURCES = \
	$(top_srcdir)/src/viewer/PGNBufferLoader.cs		\
	$(top_srcdir)/src/viewer/PGNFileLoader.cs		\
	$(top_srcdir)/src/viewer/PluginManager.cs		\
	$(top_srcdir)/src/viewer/PluginManagerDialog.cs

SOURCE = \
	$(top_srcdir)/src/App.cs \
	$(top_srcdir)/src/Window.cs \
	$(top_srcdir)/src/IControl.cs \
	$(top_srcdir)/src/Board.cs \
	$(top_srcdir)/src/PlayerBoard.cs \
	$(top_srcdir)/src/Figure.cs \
	$(top_srcdir)/src/GnuChess.cs \
	$(top_srcdir)/src/Book.cs \
	$(top_srcdir)/src/Promotion.cs \
	$(top_srcdir)/src/Position.cs \
	$(top_srcdir)/src/Session.cs	\
	$(top_srcdir)/src/Crafty.cs \
	$(top_srcdir)/src/Phalanx.cs \
	$(top_srcdir)/src/ProgressBar.cs \
	$(top_srcdir)/src/ICS.cs \
	$(top_srcdir)/src/ICSClient.cs \
	$(PARSER_SOURCES) \
	$(GAME_PLAYER_SOURCES) \
	$(VIEWER_SOURCES) \
	$(PLUGIN_SOURCES) \
	$(top_srcdir)/src/ICSSeek.cs

TEST_GAMES = \
	test/a.pgn \
	test/b.pgn \
	test/c.pgn \
	test/d.pgn \
	test/e.pgn \
	test/f.pgn \
	test/black.pgn \
	test/draw.pgn \
	test/promotion.pgn 

IMAGES= \
	images/black-bishop.svg		\
	images/black-king.svg		\
	images/black-knight.svg		\
	images/black-pawn.svg		\
	images/black-queen.svg		\
	images/black-rook.svg		\
	images/white-bishop.svg		\
	images/white-king.svg		\
	images/white-knight.svg		\
	images/white-pawn.svg		\
	images/white-queen.svg		\
	images/white-rook.svg

EXTRA_DIST = \
	COPYING \
	AUTHORS \
	INSTALL \
	NEWS \
	TODO \
	images/AUTHOR \
	images/LICENSE.html \
	config.cs.in	\
	intltool-update.in	\
	intltool-extract.in	\
	intltool-merge.in	\
	gnome-doc-utils.make	\
	$(TEST_GAMES)	\
	$(SOURCE)	\
	$(IMAGES)

imagesdir=$(prefix)/share/csboard/images
images_DATA = $(IMAGES)

appdir=$(prefix)/share/csboard
app_DATA=csboard.exe

RESOURCES =				\
	-resource:$(srcdir)/resource/csboard.glade,csboard.glade

csboard.exe: $(SOURCE) config.cs resource/csboard.glade
	mcs $(BASE_DEPENDENCIES_LIBS) -r:System.Web $(RESOURCES) \
	-out:csboard.exe $(SOURCE) config.cs \
	-r:Mono.Posix

CLEANFILES = csboard.exe

DISTCLEANFILES = 		\
	gnome-doc-utils.make	\
	intltool-extract 	\
	intltool-merge 		\
	intltool-update

DISTCHECK_CONFIGURE_FLAGS = --disable-schemas-install --disable-scrollkeeper

# Ignore scrollkeeper issues for now.  @#*$& scrollkeeper
distuninstallcheck_listfiles = find . -type f -print | grep -v scrollkeeper | grep -v /share/gnome/help/ | grep -v \.omf
