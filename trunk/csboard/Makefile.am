SUBDIRS=po data help

SOURCE = \
	src/App.cs \
	src/Window.cs \
	src/IControl.cs \
	src/Board.cs \
	src/Figure.cs \
	src/GnuChess.cs \
	src/Book.cs \
	src/Promotion.cs \
	src/Position.cs \
	src/FileChooser.cs \
	src/Session.cs	\
	src/Gettext.cs \
	src/Crafty.cs \
	src/Phalanx.cs \
	src/ProgressBar.cs \
	src/ICS.cs \
	src/ICSClient.cs \
	src/ICSSeek.cs

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
	$(TEST_GAMES)	\
	$(SOURCE)	\
	$(IMAGES)	\
	$(RESOURCE)

RESOURCE = \
	resource/csboard.glade

gladedir=$(prefix)/share/csboard/resource
glade_DATA = $(RESOURCE)

imagesdir=$(prefix)/share/csboard/images
images_DATA = $(IMAGES)

appdir=$(prefix)/share/csboard
app_DATA=csboard.exe

csboard.exe: $(SOURCE) config.cs 
	mcs $(BASE_DEPENDENCIES_LIBS) -out:csboard.exe $(SOURCE) config.cs

