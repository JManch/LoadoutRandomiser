# LoadoutRandomiser

LoadoutRandomiser is a simple console application for generating random game loadouts. 

## Currently Supported Games

- Battlefield V

## Adding a Game

To add a game create a .loadout file in the LoadoutData directory. Loadouts are defined by "categories" and "options". To create a category write the category name followed by a ":". To create an option, write the option name followed by a ",". Options can only be children of categories whilst categories can only be children of options. To signify the children use {} brackets after the parent category/option. Child categories of an option will only be rolled if the parent option is randomly selected. This is useful for specifying class-specific categories/options (an example can be seen in the BattlefieldV.loadout file). The syntax is not sensitive to linebreaks so you can include category and point definitions on the same line if that improve readability.