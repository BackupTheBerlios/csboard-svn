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


pluginsdir = $(prefix)/share/csboard/plugins
plugins_DATA = 				\
	$(PGNPRINTER_TARGET)

EXTRA_DIST =            	\
	$(PGNPRINTER_CSFILES)

CLEANFILES =			\
	$(PGNPRINTER_TARGET)
