CSC = mcs

CSFLAGS = 		\
	-target:library

PACKAGE_DEPENDS =							\
	-pkg:gtk-sharp-2.0						\
	-pkg:glade-sharp-2.0

ASSEMBLIES = 					\
	$(PACKAGE_DEPENDS)	 		\
	-r:Mono.Posix				\
	-r:$(srcdir)/db4o.dll			\
	-r:$(top_builddir)/csboard.exe

# Games DB plugin
#

GAMEDB_TARGET = GameDbPlugin.dll
GAMEDB_CSFILES = 				\
	$(srcdir)/PGNGameDetails.cs	 	\
	$(srcdir)/GameDb.cs			\
	$(srcdir)/GameDbBrowserUI.cs		\
	$(srcdir)/GameDbBrowser.cs		\
	$(srcdir)/GamesEditorDialog.cs		\
	$(srcdir)/GamesCollectionDialog.cs	\
	$(srcdir)/GameDbPlugin.cs

GAMEDB_RESOURCES =					\
	-resource:$(srcdir)/dbicon.png,dbicon.png	\
	-resource:$(srcdir)/gamedb.glade

$(GAMEDB_TARGET): $(GAMEDB_CSFILES) $(top_builddir)/csboard.exe $(srcdir)/gamedb.glade
	$(CSC) -out:$@ $(CSFLAGS) $(ASSEMBLIES) $(GAMEDB_CSFILES) $(GAMEDB_RESOURCES)

if WINDOWS_BUILD
compile.bat: $(GAMEDB_CSFILES) $(top_builddir)/csboard.exe $(srcdir)/gamedb.glade
	echo "$(CSC) -out:$(GAMEDB_TARGET) $(CSFLAGS) $(ASSEMBLIES) $(GAMEDB_CSFILES) $(GAMEDB_RESOURCES)" > _compile.bat
	sed 's/\//\\/g' _compile.bat > compile.bat
	rm _compile.bat
endif
#

pluginsdir = $(prefix)/share/csboard/plugins
plugins_DATA = 				\
	$(GAMEDB_TARGET)

if WINDOWS_BUILD
plugins_DATA += compile.bat
endif

db4odir=$(prefix)/share/csboard
db4o_DATA = db4o.dll

resources = \
	dbicon.png \
	gamedb.glade

EXTRA_DIST =            	\
	$(GAMEDB_CSFILES)	\
	$(db4o_DATA)		\
	$(resources)

CLEANFILES =			\
	$(GAMEDB_TARGET)
