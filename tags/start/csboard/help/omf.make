# 
# No modifications of this Makefile should be necessary,
# and when you do, document it in the ChangeLog file.
#
# This file contains the build instructions for installing OMF files.  It is
# generally called from the makefiles for particular formats of documentation.
#
# Note that you must configure your package with --localstatedir=/var/lib
# so that the scrollkeeper-update command below will update the database
# in the standard scrollkeeper directory.
#
# If it is impossible to configure with --localstatedir=/var/lib, then
# modify the definition of scrollkeeper_localstate_dir so that
# it points to the correct location. Note that you must still use 
# $(localstatedir) in this or when people build RPMs it will update
# the real database on their system instead of the one under RPM_BUILD_ROOT.
#
# Note: This make file is not incorporated into xmldocs.make because, in
#       general, there will be other documents install besides XML documents
#       and the makefiles for these formats should also include this file.
#
# For more information, see: http://scrollkeeper.sourceforge.net/    
#

omf_dest_dir=$(datadir)/omf/@PACKAGE@
scrollkeeper_localstate_dir = $(localstatedir)/scrollkeeper

omf: omf_timestamp

omf_timestamp: $(omffile)
	-for file in $(omffile); do \
	  scrollkeeper-preinstall $(docdir)/$(docname).xml $(srcdir)/$$file $$file.out; \
	done
	touch omf_timestamp

install-data-hook-omf:
	$(mkinstalldirs) $(DESTDIR)$(omf_dest_dir)
	for file in $(omffile); do \
		$(INSTALL_DATA) $$file.out $(DESTDIR)$(omf_dest_dir)/$$file; \
	done
	echo $(omffile) $(omfincfile) debug
	for file in $(omfincfile); do \
		$(INSTALL_DATA) $(srcdir)/$$file $(DESTDIR)$(docdir)/$$file; \
	done
	-scrollkeeper-update -p $(scrollkeeper_localstate_dir) -o $(DESTDIR)$(omf_dest_dir)

uninstall-local-omf:
	-for file in $(srcdir)/*.omf; do \
		basefile=`basename $$file`; \
		rm -f $(omf_dest_dir)/$$basefile; \
	done
	-rmdir $(omf_dest_dir)
	-scrollkeeper-update -p $(scrollkeeper_localstate_dir)
