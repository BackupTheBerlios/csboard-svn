CSC = mcs

CSFLAGS = 		\
	-target:library

ASSEMBLIES = 					\
	$(BASE_DEPENDENCIES_LIBS) 		\
	-r:Mono.Posix				\
	-r:$(top_builddir)/csboard.exe

#
# Printing plugin
#

PGNPRINTER_TARGET = PGNPrinterPlugin.dll
PGNPRINTER_CSFILES = 			\
	$(srcdir)/PrintWrapper.cs 	\
	$(srcdir)/PGNExporter.cs 	\
	$(srcdir)/PGNPrinterPlugin.cs
PGNPRINTER_RESOURCES =

$(PGNPRINTER_TARGET): $(PGNPRINTER_CSFILES) $(top_builddir)/csboard.exe
	$(CSC) -out:$@ $(CSFLAGS) $(ASSEMBLIES) $(PGNPRINTER_CSFILES) $(PGNPRINTER_RESOURCES)

if WINDOWS_BUILD
compile.bat: $(PGNPRINTER_CSFILES) $(top_builddir)/csboard.exe
	echo "$(CSC) -out:$(PGNPRINTER_TARGET) $(CSFLAGS) $(ASSEMBLIES) $(PGNPRINTER_CSFILES) $(PGNPRINTER_RESOURCES)" > _compile.bat
	sed 's/\//\\/g' _compile.bat > compile.bat
	rm _compile.bat
endif

pluginsdir = $(prefix)/share/csboard/plugins
plugins_DATA = 				\
	$(PGNPRINTER_TARGET)

if WINDOWS_BUILD
plugins_DATA += compile.bat
endif

EXTRA_DIST =            	\
	$(PGNPRINTER_CSFILES)

CLEANFILES =			\
	$(PGNPRINTER_TARGET)
