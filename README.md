# What is it?

The program allows you to quickly compile multiple caption source files into the "game" folder of your addon.

# Using the program

## Where do my files go?

Closed caption source files (e.g. `closecaption_english.txt`) go into the `\resource\subtitles\` folder in the root directory of your addon. It is not created by default so you will need to create the two folders yourself.

- Caption source file names **must** start with `closecaption_`, they can then be followed by any naming convention you wish for your project.
- The file must use the extension `.txt` and be plain text (see [Writing caption files](Writing%20caption%20files.md) for more information).
- Sub-directories are allowed to further organize your files, all sub-directories will be searched for valid files.
- The files will be compiled into a mirrored `\resource\subtitles\` folder in the `\game\` path of your addon with the extension `.dat`.

## Loading captions in-game

There are two ways to load captions for your addon:

1. Allow Source to load your captions and add them onto the existing captions. This has the benefit of keeping all the base game SFX captions and Alyx ammo mentions. Nothing needs to be done for this besides compiling your captions. If you want to keep most base game captions but remove some see [Removing base game captions](Writing%20caption%20files.md#Removing_base_game_captions).
2. Override the base game captions using a script and the console command `cc_lang`. This will cause base game captions to not be displayed, only captions from your custom files will appear. This requires using unique language names like `closecaption_english_custom.dat`. The scripts for this are supplied for you to use and require minimal editing.

## Managing multiple files

HLACaptionCompiler can take multiple files with the same defined language and compile them into a single language file allowing you to split your caption files up to be more manageable. Although you can use any name after the initial `closecaption_` it is recommended to follow it by the language name and then the file's catagory, although this is entirely up to you.

Here is an example file structure for two languages:

    resource\
        subtitles\
            english\
                closecaption_english-johnson_choreo.txt
                closecaption_english-act2.txt
                closecaption_english-sfx.txt
            french\
                closecaption_french-johnson_choreo.txt
                closecaption_french-act2.txt
                closecaption_french-sfx.txt

The `english` and `french` folders are purely for organization and don't affect the way the files are compiled. The above files will be compiled into the `\game\` folder of your addon as the following:

    resource\
        subtitles\
            closecaption_english.dat
            closecaption_french.dat

## Running the exe

`HLACaptionCompiler.exe` will run a little differently depending on the context:

1. If executed from an addon folder it will search for captions source files and automatically compile any into the related "game" folder for that addon.

2. If executed outside of an addon folder it will present you with a menu to choose from any addons it finds in your Half-Life: Alyx installation path.

3. If a valid caption source file (or files) is dragged onto the exe, the file(s) will be compiled and the output directory will be the same as the file(s).

When compiling, the source files will be examined for any errors it can catch including duplicate tokens. It has simple error reporting to tell you the line number and position it thinks the error occured in the source file to help narrow it down.

## From the command line

`HLACaptionCompiler.exe` accepts files, directories, short hand options prepended by a dash "`-s`", and long hand options predended by double dashes "`--settings`". Short hand options can also be chained together with a single dash "`-spv`"

The following is an example of compiling an addon with `verbose` and `pause` settings:

    HLACaptionCompiler.exe -vp "C:\Program Files (x86)\Steam\steamapps\common\Half-Life Alyx\content\hlvr_addons\my_addon"

## Command line settings/options

If executing the program from the command line you can specify some arguments to alter its behavior. A settings file can also be generated for those who don't like to use the command line.

Any non-valid setting will be considered as a source file and checked for validity. If the path is a single source file it will be compiled to its original location, if the path is an addon folder, all source files in the addon will be compiled. You may pass as many files and addons as command line arguments as you wish.

Short hand options are case-sensitive.

- -h / --help

    Displays these options and a link to this github.

- -S / --settings
    
    Generates a `settings.json` file or overwrites the existing one, allowing the user to modify settings without using the command line. The program will read from this file when launching.

- -p / --pause
  
    The program will pause after finishing and wait for user input. Useful for debugging files which aren't compiling.

- -v / --verbose
  
    The program will output more messages to the console as it completes each action or finds a problem.

- -s / -strict

    Compile with strict syntax checking. Requires all keys and values to be enclosed with double quotes and will fail on duplicate tokens.

# Directives/Pre-Processors

Directives are special commands that perform actions on the input text (caption source) (usually before it is examined and compiled). Directives are fairly strict in their formatting and some can only exist at the top of the file before `"lang"` is encountered. They have the following format:
    
    #directive-name value1 value2 ...

All text for a single directive must exist on one line. Values after the directive name may be optional. If you want the compiler to temporarily ignore a directive you can simply comment it out:

    //#directive-name value1 value2 ...

## List of directives

- **#define name value**
  
    Every instance of `name` in the source file will be replaced with its assigned `value`, allowing you to define a color or piece of text once and instance it anywhere in the file. This means it's important to choose a name that will not be encountered anywhere in your actualy dialogue, it's a good idea to use symbols to differentiate them for regular dictionary words.

    `name` and `value` may not contain any whitespace and quotes cannot be used to avoid this limitation as quotes are valid characters for the `name` and `value`.

    The following captions are for the same character and thus use the same color:
    
        ...
        scenes.johnson_scared_01 "<clr:29,72,191>Did you hear that? It sounds like…"
        scenes.johnson_scared_02 "<clr:29,72,191>There it is again. What is that?"
        scenes.johnson_scared_03 "<clr:29,72,191>I'm getting out of here!"
        ...
    
    If you decide later on to use a different color for this character then changing three lines is trivial enough, but you may have dozens of lines for your character, and while find/replace exists, the `define` directive simplifies this process by allowing you to define the color at the top of the file:

        # $Color-Johnson clr:29,72,191
        ...
        scenes.johnson_scared_01 "<$Color-Johnson>Did you hear that? It sounds like…"
        scenes.johnson_scared_02 "<$Color-Johnson>There it is again. What is that?"
        scenes.johnson_scared_03 "<$Color-Johnson>I'm getting out of here!"
        ...

    When the file is compiled the output is exactly the same as the one before, but now you can quickly iterate on color changes.

    Another neat little example is combining often used tags into a new one to save you some key strokes. Here we use italics and bold a lot:

        ...
        combined.tag.example "<I><B>Every<I><B> second <I><B>word<I><B> is <I><B>italics<I><B> and <I><B>bold<I><B>."
        ...
    
    So we define a new value to make things look a little neater and simpler:

        # <IB> <I><B>
        ...
        combined.tag.example "<IB>Every<IB> second <IB>word<IB> is <IB>bold<IB> and <IB>italics<IB>."
        ...
    
    Remember that the `define` directive isn't just for tags and will replace anything. This last example shows how to release your captions for non USA players:

        # color colour
        # center centre
        # dialog dialogue

    **Cautionary note:** Define directives will affect other directives, meaning define or and name is very important. This can be useful for creating template defines (see this example).