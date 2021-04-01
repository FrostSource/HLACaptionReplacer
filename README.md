# What is it?

The program allows you to quickly compile multiple caption source files into the "game" folder of your addon.

# Using the program

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

- -S / --settings
    
    Generates a `settings.json` file or overwrites the existing one, allowing the user to modify settings without using the command line. The program will read from this file when launching.

- -p / --pause
  
    The program will pause after finishing and wait for user input. Useful for debugging files which aren't compiling.

- -v / --verbose
  
    The program will output more messages to the console as it completes each action or finds a problem.

- -s / -strict

    Compile with strict syntax checking. Requires all keys and values to be enclosed with double quotes and will fail on duplicate tokens.

# Pre-Processors

Pre-processors are special commands at the top of the file that perform actions on the input text (caption source) before it is examined and compiled. Pre-processors are fairly strict in their formatting and can only exist at the top of the file before `"lang"` is encountered. They have the following format:
    
    # pre-processor-name pre-processor-value

The `name` and `value` must be on the same line and the `name` may not contain whitespace. Quotes cannot be used to avoid this limitation as quotes are valid characters for the `name` and `value`. The `value` is the *first non-whitespace* character after the name *until the end of the line*, and may be optional for some pre-processors. If you want the compiler to temporarily ignore a pre-processor you can simply comment it out:

    //# pre-processor-name pre-processor-value

## List of pre-processors

- **Key/Value**
  
    Every instance of the `key` in the source file will be replace with its assigned `value`, allowing you to define a color or piece of text once and instance it anywhere in the file. This means it's important to choose a name that will not be encountered anywhere in your actualy dialogue, it's a good idea to use symbols to differentiate them for regular dictionary words.

    The following captions are for the same character and thus use the same color:
    
        ...
        "scenes.johnson_scared_01" "<clr:29,72,191>Did you hear that? It sounds like…"
        "scenes.johnson_scared_02" "<clr:29,72,191>There it is again. What is that?"
        "scenes.johnson_scared_03" "<clr:29,72,191>I'm getting out of here!"
        ...
    
    If you decide later on to use a different color for this character then changing three lines is trivial enough, but you may have dozens of lines for your character, and while find/replace exists, pre-processors simplify this process by allowing you to define the color at the top of the file:

        # $Color-Johnson clr:29,72,191
        ...
        "scenes.johnson_scared_01" "<$Color-Johnson>Did you hear that? It sounds like…"
        "scenes.johnson_scared_02" "<$Color-Johnson>There it is again. What is that?"
        "scenes.johnson_scared_03" "<$Color-Johnson>I'm getting out of here!"
        ...

    When the file is compiled the output is exactly the same as the one before, but now you can quickly iterate on color changes.

    Another neat little example is combining often used tags into a new one to save you some key strokes. Here we use italics and bold a lot:

        ...
        "combined.tag.example" "<I><B>Every<I><B> second <I><B>word<I><B> is <I><B>italics<I><B> and <I><B>bold<I><B>."
        ...
    
    So we use a pre-processor to make things look a little neater and simpler:

        # <IB> <I><B>
        ...
        "combined.tag.example" "<IB>Every<IB> second <IB>word<IB> is <IB>bold<IB> and <IB>italics<IB>."
        ...
    
    Remember that the key/value pre-processor isn't just for tags and will replace anything. This last example shows how to release your captions for non USA players:

        # color colour
        # center centre
        # dialog dialogue