#!/bin/bash

TOREPLACE=$1
REPLACEWITH=$2

echo replacing $TOREPLACE with $REPLACEWITH

git checkout $TOREPLACE
git pull
git checkout $REPLACEWITH
git merge -s ours $TOREPLACE
git checkout $TOREPLACE
git merge $REPLACEWITH
