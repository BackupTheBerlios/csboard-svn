#!/bin/sh
#
# Run this to generate configure script.
#

DIE=true
PROJECT="CsBoard"

# If you are going to use the non-default name for automake becase your OS
# installaion has multiple versions, you need to call both aclocal and automake
# with that version number, as they come from the same package.
#AM_VERSION='-1.8'

ACLOCAL=aclocal$AM_VERSION
AUTOMAKE=automake$AM_VERSION
AUTOCONF=autoconf

ACVER=`$AUTOCONF --version | grep '^autoconf' | sed 's/.*) *//'`
case "$ACVER" in
'' | 0.* | 1.* | 2.[0-4]* | \
2.5[0-1] | 2.5[0-1][a-z]* )
  cat >&2 <<_EOF_

	You must have autoconf 2.52 or later installed to compile $PROJECT.
	Download the appropriate package for your distribution/OS,
	or get the source tarball at ftp://ftp.gnu.org/pub/gnu/autoconf/
_EOF_
  DIE="exit 1"
  ;;
esac


AMVER=`$AUTOMAKE --version | grep '^automake' | sed 's/.*) *//'`
case "$AMVER" in
'' | 0.* | 1.[0-5]* )

  cat >&2 <<_EOF_

	You must have automake 1.6 or later installed to compile $PROJECT.
	Download the appropriate package for your distribution/OS,
	or get the source tarball at ftp://ftp.gnu.org/pub/gnu/automake/
_EOF_
  DIE="exit 1"
  ;;
esac

$DIE
    
    echo $ACLOCAL
    $ACLOCAL
    echo glib-gettextize
    glib-gettextize
    echo intltoolize --force --automake
    intltoolize --force --automake
    echo $AUTOMAKE -a
    $AUTOMAKE -a || exit 1
    echo $AUTOCONF
    $AUTOCONF || exit 1

echo
echo "Now type \"./configure [options]\" and \"make\" to compile $PROJECT."
