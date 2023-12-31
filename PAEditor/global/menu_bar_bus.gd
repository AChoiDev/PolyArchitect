extends Node
##
## Signal bus and manager for the editor menu bar allowing for easy handling of user
## requests and managing of context specific menus.
##

# Signals
# File
## User requested to quit, ensure user has saved before exiting.
signal file_quit_pressed();

# Edit

# Windows

# Help
## User requested to know more about the program.
signal help_about_pressed();
