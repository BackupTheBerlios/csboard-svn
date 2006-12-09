CSC = mcs

CSFLAGS = 		\
	-target:library

ASSEMBLIES = 					\
	$(BASE_DEPENDENCIES_LIBS) 		\
	$(URL_LOADER_PLUGIN_LIBS) 		\
	-r:$(top_builddir)/csboard.exe


#
# URL loader plugin
#

URLLOADER_TARGET = PGNUrlLoader.dll
URLLOADER_CSFILES = $(srcdir)/PGNUrlLoader.cs
URLLOADER_RESOURCES =

$(URLLOADER_TARGET): $(URLLOADER_CSFILES) $(top_builddir)/csboard.exe
	$(CSC) -out:$@ $(CSFLAGS) $(ASSEMBLIES) $(URLLOADER_CSFILES) $(URLLOADER_RESOURCES)

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
	$(URLLOADER_TARGET)		\
	$(PGNPRINTER_TARGET)

EXTRA_DIST =            	\
	$(URLLOADER_CSFILES)

CLEANFILES =			\
	$(URLLOADER_TARGET)	\
	$(PGNPRINTER_TARGET)
