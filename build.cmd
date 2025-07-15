@echo off
pushd subdir
call src/Angor/Avalonia/build.cmd %*
popd

