SUBDIRS=po data help resource

SOURCE = \
	$(top_srcdir)/src/App.cs \
	$(top_srcdir)/src/Window.cs \
	$(top_srcdir)/src/IControl.cs \
	$(top_srcdir)/src/Board.cs \
	$(top_srcdir)/src/Figure.cs \
	$(top_srcdir)/src/GnuChess.cs \
	$(top_srcdir)/src/Book.cs \
	$(top_srcdir)/src/Promotion.cs \
	$(top_srcdir)/src/Position.cs \
	$(top_srcdir)/src/Session.cs	\
	$(top_srcdir)/src/Gettext.cs \
	$(top_srcdir)/src/Crafty.cs \
	$(top_srcdir)/src/Phalanx.cs \
	$(top_srcdir)/src/ProgressBar.cs \
	$(top_srcdir)/src/ICS.cs \
	$(top_srcdir)/src/ICSClient.cs \
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

csboard.exe: $(SOURCE) config.cs 
	mcs $(BASE_DEPENDENCIES_LIBS) -out:csboard.exe $(SOURCE) config.cs

CLEANFILES = csboard.exe

DISTCLEANFILES = 		\
	gnome-doc-utils.make	\
	intltool-extract 	\
	intltool-merge 		\
	intltool-update

DISTCHECK_CONFIGURE_FLAGS = --disable-schemas-install --disable-scrollkeeper

# Ignore scrollkeeper issues for now.  @#*$& scrollkeeper
distuninstallcheck_listfiles = find . -type f -print | grep -v scrollkeeper | grep -v /share/gnome/help/ | grep -v \.omf
