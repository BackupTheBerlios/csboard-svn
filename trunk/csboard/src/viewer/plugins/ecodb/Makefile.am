CSC = mcs

CSFLAGS = 		\
	-target:library

ASSEMBLIES = 					\
	$(BASE_DEPENDENCIES_LIBS) 		\
	-r:Mono.Posix				\
	-r:$(top_builddir)/csboard.exe

# URL loader plugin
#

ECODB_TARGET = EcoDBPlugin.dll
ECODB_CSFILES = 				\
	$(srcdir)/EcoDBPlugin.cs		\
	$(srcdir)/OpeningsDb.cs
ECODB_RESOURCES = -resource:$(top_srcdir)/resource/eco.pgn,eco.pgn

$(ECODB_TARGET): $(ECODB_CSFILES) $(top_builddir)/csboard.exe $(top_srcdir)/resource/eco.pgn
	$(CSC) -out:$@ $(CSFLAGS) $(ASSEMBLIES) $(ECODB_CSFILES) $(ECODB_RESOURCES)

#

pluginsdir = $(prefix)/share/csboard/plugins
plugins_DATA = 				\
	$(ECODB_TARGET)

EXTRA_DIST =            	\
	$(ECODB_CSFILES)

CLEANFILES =			\
	$(ECODB_TARGET)
