CSC = mcs

CSFLAGS = 		\
	-target:library

PACKAGE_DEPENDENCIES =							\
	-pkg:gtk-sharp-2.0						\
	-pkg:glade-sharp-2.0						\
	-pkg:rsvg-sharp-2.0

ASSEMBLIES = 					\
	$(PACKAGE_DEPENDENCIES) 		\
	-r:Mono.Posix				\
	-r:$(top_builddir)/csboard.exe

# URL loader plugin
#

ECODB_TARGET = EcoDBPlugin.dll
ECODB_CSFILES = 				\
	$(srcdir)/OpeningBrowserUI.cs		\
	$(srcdir)/EcoDBPlugin.cs		\
	$(srcdir)/OpeningsDb.cs
ECODB_RESOURCES = 				\
	-resource:$(top_srcdir)/resource/eco.pgn,eco.pgn

$(ECODB_TARGET): $(ECODB_CSFILES) $(top_builddir)/csboard.exe $(top_srcdir)/resource/eco.pgn
	$(CSC) -out:$@ $(CSFLAGS) $(ASSEMBLIES) $(ECODB_CSFILES) $(ECODB_RESOURCES)

if WINDOWS_BUILD
compile.bat: $(ECODB_CSFILES) $(top_builddir)/csboard.exe $(top_srcdir)/resource/eco.pgn
	echo "$(CSC) -out:$(ECODB_TARGET) $(CSFLAGS) $(ASSEMBLIES) $(ECODB_CSFILES) $(ECODB_RESOURCES)" > _compile.bat
	sed 's/\//\\/g' _compile.bat > compile.bat
	rm _compile.bat
endif

pluginsdir = $(prefix)/share/csboard/plugins

plugins_DATA = 				\
	$(ECODB_TARGET)

if WINDOWS_BUILD
plugins_DATA += compile.bat
endif

EXTRA_DIST =            	\
	$(ECODB_CSFILES)

CLEANFILES =			\
	$(ECODB_TARGET)
