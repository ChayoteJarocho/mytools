#!/bin/bash

## In dconf, the actual keys are located under /org/mate/desktop/interface/*
## In gsettings, the keys might be located under org.mate.interface.* but doesn't work well

dark_gtk_theme = 'Yaru-blue-dark'
lite_gtk_theme = 'Yaru-blue'
curr_gtk_theme = `dconf read /org/mate/desktop/interface/gtk-theme`

dark_gtk_icon_theme = 'Yaru-dark'
lite_gtk_icon_theme = 'Yaru'
curr_gtk_icon_theme = `dconf read /org/mate/desktop/interface/icon-theme`

if [ "$curr_gtk_theme" = "'Yaru-blue'" ]; then
  dconf write /org/mate/desktop/interface/gtk-theme "'$dark_gtk_theme'"
  dconf write /org/mate/desktop/interface/icon-theme "'$dark_gtk_icon_theme'"
else
  dconf write /org/mate/desktop/interface/gtk-theme "'$lite_gtk_theme'"
  dconf write /org/mate/desktop/interface/icon-theme "'$lite_gtk_icon_theme'" 
fi
