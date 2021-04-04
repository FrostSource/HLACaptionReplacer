# Writing caption files

Before we get into the things I've learned about closed captions in Source 2 I want to recommend at least reading [this wiki article](https://developer.valvesoftware.com/wiki/Closed_Captions) about S1 captions as it still applies here.

Caption files are plain text files with a very simple structure consisting of a defined language and a list of sound event names and their text value.

Below is the simplest possible caption file with only one sound defined:

    "lang"
    {
        "Language" "English"
        "Tokens"
        {
            "addon.my_sound" "Hello world."
        }
    }

The formatting and spelling are very important for the compiler to read your files. If something is not recognized the compiler will print a syntax error message to help you debug the issue. Something **important** to note is that captions do not seem to recognize capitalized sound event names `Addon.MySound` will not work, you will have to refer to it in lower-case form.


By default captions will display 

# Removing base game captions