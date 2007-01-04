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
	$(srcdir)/GameDb.cs			\
	$(srcdir)/GameDbPlugin.cs

GAMEDB_RESOURCES =

$(GAMEDB_TARGET): $(GAMEDB_CSFILES) $(top_builddir)/csboard.exe
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
