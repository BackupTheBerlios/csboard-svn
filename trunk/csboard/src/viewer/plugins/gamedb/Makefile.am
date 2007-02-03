CSC = mcs

CSFLAGS = 		\
	-target:library

ASSEMBLIES = 					\
	$(BASE_DEPENDENCIES_LIBS) 		\
	-r:Mono.Posix				\
	-r:$(srcdir)/db4o.dll			\
	-r:$(top_builddir)/csboard.exe

# Games DB plugin
#

GAMEDB_TARGET = GameDbPlugin.dll
GAMEDB_CSFILES = 				\
	$(srcdir)/PGNGameDetails.cs	 	\
	$(srcdir)/GameDb.cs			\
	$(srcdir)/GameDbBrowser.cs		\
	$(srcdir)/GamesEditorDialog.cs		\
	$(srcdir)/GamesCollectionDialog.cs	\
	$(srcdir)/GameDbPlugin.cs

GAMEDB_RESOURCES =				\
	-resource:$(srcdir)/gamedb.glade

$(GAMEDB_TARGET): $(GAMEDB_CSFILES) $(top_builddir)/csboard.exe $(srcdir)/gamedb.glade
	$(CSC) -out:$@ $(CSFLAGS) $(ASSEMBLIES) $(GAMEDB_CSFILES) $(GAMEDB_RESOURCES)

#

pluginsdir = $(prefix)/share/csboard/plugins
plugins_DATA = 				\
	$(GAMEDB_TARGET)

db4odir=$(prefix)/share/csboard
db4o_DATA = db4o.dll

EXTRA_DIST =            	\
	$(GAMEDB_CSFILES)

CLEANFILES =			\
	$(GAMEDB_TARGET)
