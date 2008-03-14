SUBDIRS= . src po data help resource

PARSER_SRCS = \
	$(top_srcdir)/src/parser/PGNParserException.cs \
	$(top_srcdir)/src/parser/PGNTokenizer.cs \
	$(top_srcdir)/src/parser/PGNChessGame.cs \
	$(top_srcdir)/src/parser/PGNChessMove.cs \
	$(top_srcdir)/src/parser/PGNTag.cs \
	$(top_srcdir)/src/parser/PGNNAG.cs \
	$(top_srcdir)/src/parser/PGNGameLoader.cs \
	$(top_srcdir)/src/parser/PGNParser.cs

GAME_PLAYER_SRCS = \
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

VIEWER_SRCS = \
	$(top_srcdir)/src/viewer/GameSession.cs \
	$(top_srcdir)/src/viewer/ChessGame.cs \
	$(top_srcdir)/src/viewer/Widgets/CairoViewerBoard.cs \
	$(top_srcdir)/src/viewer/Widgets/ChessGameBoard.cs \
	$(top_srcdir)/src/viewer/Widgets/ChessGameView.cs \
	$(top_srcdir)/src/viewer/Widgets/ChessGameWidget.cs \
	$(top_srcdir)/src/viewer/Widgets/GamesListWidget.cs \
	$(top_srcdir)/src/viewer/Widgets/GameViewerWidget.cs \
	$(top_srcdir)/src/viewer/ViewerMenuBar.cs \
	$(top_srcdir)/src/viewer/GameViewer.cs \
	$(top_srcdir)/src/viewer/GameViewerUI.cs \
	$(top_srcdir)/src/viewer/PositionSnapshot.cs \
	$(top_srcdir)/src/viewer/CairoPositionSnapshot.cs \
	$(top_srcdir)/src/viewer/ViewerBoard.cs \
	$(top_srcdir)/src/viewer/BufferDialog.cs \
	$(top_srcdir)/src/viewer/PGNTree.cs

PLUGIN_SRCS = \
	$(top_srcdir)/src/viewer/PGNBufferLoader.cs		\
	$(top_srcdir)/src/viewer/PGNFileLoader.cs		\
	$(top_srcdir)/src/PluginManager.cs		\
	$(top_srcdir)/src/PluginManagerDialog.cs

ICS_PLAYER_SRCS = \
	$(top_srcdir)/src/ics/ICSClient.cs \
	$(top_srcdir)/src/ics/ParserUtils.cs \
	$(top_srcdir)/src/ics/GameAdvertisement.cs \
	$(top_srcdir)/src/ics/MoveDetails.cs \
	$(top_srcdir)/src/ics/ResultNotification.cs \
	$(top_srcdir)/src/ics/GameDetails.cs \
	$(top_srcdir)/src/ics/GameInfo.cs \
	$(top_srcdir)/src/ics/CommandSender.cs \
	$(top_srcdir)/src/ics/MovesGetter.cs \
	$(top_srcdir)/src/ics/RelayGetter.cs \
	$(top_srcdir)/src/ics/RelayTournamentsGetter.cs \
	$(top_srcdir)/src/ics/RelayGamesGetter.cs \
	$(top_srcdir)/src/ics/RelayTournament.cs \
	$(top_srcdir)/src/ics/RelayGame.cs \
	$(top_srcdir)/src/ics/ICSSeek.cs \
	$(top_srcdir)/src/ics/MatchChallenge.cs \
	$(top_srcdir)/src/ics/GameObservationManager.cs \
	$(top_srcdir)/src/ics/UI/ICSShell.cs \
	$(top_srcdir)/src/ics/UI/ICSConfigDialog.cs \
	$(top_srcdir)/src/ics/UI/ICSConfigWidget.cs \
	$(top_srcdir)/src/ics/UI/GameAdvertisementsWidget.cs \
	$(top_srcdir)/src/ics/UI/ICSDetailsWidget.cs \
	$(top_srcdir)/src/ics/UI/ICSGameObserverWidget.cs \
	$(top_srcdir)/src/ics/UI/ObservableGamesWidget.cs \
	$(top_srcdir)/src/ics/UI/ChessMovesWidget.cs \
	$(top_srcdir)/src/ics/UI/ObservingGamePage.cs \
	$(top_srcdir)/src/ics/UI/PlayerPage.cs \
	$(top_srcdir)/src/ics/UI/Graph.cs \
	$(top_srcdir)/src/ics/UI/GameAdvertisementGraph.cs \
	$(top_srcdir)/src/ics/UI/ICSMenuBar.cs \
	$(top_srcdir)/src/ics/UI/RelayTournamentsView.cs \
	$(top_srcdir)/src/ics/UI/ShellTextView.cs \
	$(top_srcdir)/src/ics/UI/NotificationWidget.cs

SOURCE = \
	$(top_srcdir)/src/App.cs \
	$(top_srcdir)/src/ColorProvider.cs \
	$(top_srcdir)/src/ChessGameWidget.cs \
	$(top_srcdir)/src/ChessWindowUI.cs \
	$(top_srcdir)/src/Window.cs \
	$(top_srcdir)/src/SubApp.cs \
	$(top_srcdir)/src/IControl.cs \
	$(top_srcdir)/src/Board.cs \
	$(top_srcdir)/src/FigureManager.cs \
	$(top_srcdir)/src/CairoBoard.cs \
	$(top_srcdir)/src/CairoPlayerBoard.cs \
	$(top_srcdir)/src/PlayerBoard.cs \
	$(top_srcdir)/src/Figure.cs \
	$(top_srcdir)/src/GnuChess.cs \
	$(top_srcdir)/src/NullControl.cs \
	$(top_srcdir)/src/Book.cs \
	$(top_srcdir)/src/Promotion.cs \
	$(top_srcdir)/src/Position.cs \
	$(top_srcdir)/src/Crafty.cs \
	$(top_srcdir)/src/Phalanx.cs \
	$(top_srcdir)/src/ProgressBar.cs \
	$(top_srcdir)/src/ChessClock.cs\
	$(top_srcdir)/src/AppMenuBar.cs\
	$(PARSER_SRCS) \
	$(GAME_PLAYER_SRCS) \
	$(VIEWER_SRCS) \
	$(PLUGIN_SRCS) \
	$(ICS_PLAYER_SRCS)

if WINDOWS_BUILD
SOURCE += $(top_srcdir)/src/DummySession.cs
else
SOURCE += $(top_srcdir)/src/Session.cs
endif

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
	images/white-rook.svg		\
	images/computer.png

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

RESOURCES =										\
	-resource:$(top_srcdir)/images/white-rook.svg,white-rook.svg			\
	-resource:$(top_srcdir)/images/white-king.svg,white-king.svg			\
	-resource:$(top_srcdir)/images/white-queen.svg,white-queen.svg			\
	-resource:$(top_srcdir)/images/white-bishop.svg,white-bishop.svg		\
	-resource:$(top_srcdir)/images/white-pawn.svg,white-pawn.svg			\
	-resource:$(top_srcdir)/images/white-knight.svg,white-knight.svg		\
	-resource:$(top_srcdir)/images/black-rook.svg,black-rook.svg			\
	-resource:$(top_srcdir)/images/black-king.svg,black-king.svg			\
	-resource:$(top_srcdir)/images/black-queen.svg,black-queen.svg			\
	-resource:$(top_srcdir)/images/black-bishop.svg,black-bishop.svg		\
	-resource:$(top_srcdir)/images/black-pawn.svg,black-pawn.svg			\
	-resource:$(top_srcdir)/images/black-knight.svg,black-knight.svg		\
	-resource:$(top_srcdir)/images/computer.png,computer.png			\
	-resource:$(top_srcdir)/resource/csviewer.glade,csviewer.glade			\
	-resource:$(top_srcdir)/resource/csboard.glade,csboard.glade

RESOURCE_FILE_DEPENDS =						\
	$(top_srcdir)/images/white-rook.svg				\
	$(top_srcdir)/images/white-king.svg				\
	$(top_srcdir)/images/white-queen.svg				\
	$(top_srcdir)/images/white-bishop.svg				\
	$(top_srcdir)/images/white-pawn.svg				\
	$(top_srcdir)/images/white-knight.svg				\
	$(top_srcdir)/images/black-rook.svg				\
	$(top_srcdir)/images/black-king.svg				\
	$(top_srcdir)/images/black-queen.svg				\
	$(top_srcdir)/images/black-bishop.svg				\
	$(top_srcdir)/images/black-pawn.svg				\
	$(top_srcdir)/images/black-knight.svg				\
	$(top_srcdir)/images/black-knight.svg				\
	$(top_srcdir)/resource/eco.pgn					\
	$(top_srcdir)/resource/csboard.glade				\
	$(top_srcdir)/resource/csviewer.glade

PACKAGE_DEPENDS =							\
	-pkg:gtk-sharp-2.0						\
	-pkg:glade-sharp-2.0						\
	-pkg:rsvg-sharp-2.0

if !WINDOWS_BUILD
PACKAGE_DEPENDS += -pkg:gconf-sharp-2.0
else
app_DATA += compile.bat

compile.bat: $(SOURCE) config.cs $(RESOURCE_FILE_DEPENDS)
	echo "	mcs $(PACKAGE_DEPENDS) $(RESOURCES) \
	-out:csboard.exe $(SOURCE) config.cs \
	-r:Mono.Posix -r:Mono.Cairo" > _compile.bat
	sed 's/\//\\/g' _compile.bat > compile.bat
	rm _compile.bat

endif

csboard.exe: $(SOURCE) config.cs $(RESOURCE_FILE_DEPENDS)
	mcs $(PACKAGE_DEPENDS) $(RESOURCES) \
	-out:csboard.exe $(SOURCE) config.cs \
	-r:Mono.Posix -r:Mono.Cairo

CLEANFILES = csboard.exe

DISTCLEANFILES = 		\
	gnome-doc-utils.make	\
	intltool-extract 	\
	intltool-merge 		\
	intltool-update

DISTCHECK_CONFIGURE_FLAGS = --disable-schemas-install --disable-scrollkeeper

# Ignore scrollkeeper issues for now.  @#*$& scrollkeeper
distuninstallcheck_listfiles = find . -type f -print | grep -v scrollkeeper | grep -v /share/gnome/help/ | grep -v \.omf
