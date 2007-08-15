CSC = mcs

CSFLAGS = 		\
	-target:library

PACKAGE_DEPENDENCIES =							\
	-pkg:gtk-sharp-2.0						\
	-pkg:gnome-sharp-2.0

ASSEMBLIES = 					\
	$(PACKAGE_DEPENDENCIES) 		\
	-r:Mono.Posix				\
	-r:$(top_builddir)/csboard.exe


#
# URL loader plugin
#

URLLOADER_TARGET = PGNUrlLoader.dll
URLLOADER_CSFILES = $(srcdir)/PGNUrlLoader.cs
URLLOADER_RESOURCES =

$(URLLOADER_TARGET): $(URLLOADER_CSFILES) $(top_builddir)/csboard.exe
	$(CSC) -out:$@ $(CSFLAGS) $(ASSEMBLIES) $(URLLOADER_CSFILES) $(URLLOADER_RESOURCES)


pluginsdir = $(prefix)/share/csboard/plugins
plugins_DATA = 				\
	$(URLLOADER_TARGET)

EXTRA_DIST =            	\
	$(URLLOADER_CSFILES)

CLEANFILES =			\
	$(URLLOADER_TARGET)
