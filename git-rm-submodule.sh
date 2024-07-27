#!/bin/bash

if [[ $1 = "-help" ]] || [[ $1 = "-?" ]] || [[ $1 = "-h" ]]; then
    printf "usage: git-rm-submodule.sh\r\n"
    printf "\r\n"
    printf "Safely removes a git submodule for manual deletion.\r\n"
    printf "Once run, the folder containing the specified submodule is renamed to have a suffix of _tmp.\r\n"
    printf "The folder may be safely deleted after confirming proper removal of the git submodule.\r\n"
    printf "\r\n"
    exit 0;
fi

echo removing submodule $1

mv $1 $1_tmp

git submodule deinit -f -- $1
rm -rf .git/modules/$1
git rm -f $1
