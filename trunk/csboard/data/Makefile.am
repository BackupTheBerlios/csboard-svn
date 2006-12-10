bin_SCRIPTS = csboard csviewer

EXTRA_DIST =\
	csboard.prjx	\
	csboard.cmbx	\
	csboard.png	\
	csboard.desktop.in \
	csviewer.desktop.in \
	csboard.spec.in \
	csboard.schemas.in

desktopdir = $(prefix)/share/applications
desktop_in_files = csboard.desktop.in csviewer.desktop.in
desktop_DATA = $(desktop_in_files:.desktop.in=.desktop)
@INTLTOOL_DESKTOP_RULE@

pixmapdir = $(prefix)/share/pixmaps
pixmap_DATA = csboard.png

schemasdir = $(sysconfdir)/gconf/schemas
schemas_in_files = csboard.schemas.in
schemas_DATA = $(schemas_in_files:.schemas.in=.schemas)

@INTLTOOL_SCHEMAS_RULE@

if GCONF_SCHEMAS_INSTALL
install-data-local:
	if test -z "$(DESTDIR)" ; then \
		for p in $(schemas_DATA) ; do \
			GCONF_CONFIG_SOURCE=$(GCONF_SCHEMA_CONFIG_SOURCE) $(GCONFTOOL) --makefile-install-rule $$p ; \
		done ; \
	fi
	-killall -s HUP gconfd-2
else
install-data-local:
endif

if GCONF_SCHEMAS_INSTALL
uninstall-local:
	if test -z "$(DESTDIR)" ; then \
		for p in $(schemas_DATA) ; do \
			GCONF_CONFIG_SOURCE=$(GCONF_SCHEMA_CONFIG_SOURCE) $(GCONFTOOL) --makefile-uninstall-rule $$p ; \
		done ; \
	fi
	-killall -s HUP gconfd-2
else
uninstall-local:
endif

DISTCLEANFILES = \
	$(desktop_DATA)	\
	$(schemas_DATA)
