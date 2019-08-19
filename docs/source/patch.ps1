$path = Split-Path -Parent $MyInvocation.MyCommand.Definition
rm $path/node_modules/highlight.js/lib/languages/cs.js
cp $path/patch/highlight.js/lib/languages/cs.js $path/node_modules/highlight.js/lib/languages/cs.js