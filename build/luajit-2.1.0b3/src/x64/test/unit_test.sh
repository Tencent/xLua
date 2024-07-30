#!/bin/sh
DIR=$(cd $(dirname $0); pwd)
cd $DIR

LUAJIT=$DIR/../../luajit
HASERR=0

find $DIR/unit -name "*.lua" -print | while read x; do
    $LUAJIT $x >/dev/null 2>/dev/null
    if [ $? -eq 0 ]; then
        echo "$x ok"
    else
        HASERR=1
        echo "$x failed"
    fi
done

if [ $HASERR -eq 0 ]; then
    exit 0
fi

exit 1
