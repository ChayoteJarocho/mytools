#!/bin/bash

current=`gsettings get org.mate.interface gtk-theme`
if [ "$current" = "'Yaru-blue'" ]; then
  gsettings set org.mate.interface gtk-theme 'Yaru-blue-dark'
else
  gsettings set org.mate.interface gtk-theme 'Yaru-blue'
fi