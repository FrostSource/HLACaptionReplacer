# Writing caption files

Before we get into the things I've learned about closed captions in Source 2 I want to recommend at least reading [this wiki article](https://developer.valvesoftware.com/wiki/Closed_Captions) about S1 captions as it still applies here.

Caption files are plain text files with a very simple structure consisting of a defined language and a list of sound event names and their text value.

Below is a simple caption file with only two sounds defined:

    lang
    {
        Language "English"
        Tokens
        {
            addon.my_sound    "Hello world."
            addon.my_sound2   "<I>Are you there?<I>"
        }
    }

I will quickly run through each keyword/section.

- `lang`

    Defines this file as a caption file. This must be present as the root section and be opened and closed with braces `{` `}`.

- `Language`
  
    Defines the language of the file, must be followed directly by the name of the language. Can be anything and will only affect the resulting compiled file name.

- `Tokens`

    Defines the start of a set of tokens. Must be opened and closed with braces `{` `}`. All captions are written within this section.

- `addon.my_sound "Hello world."`

    This is a `token`/`value` pair. The `token` is the sound event name and the `value` is the definition of the caption text, including any special tags. You may have as many of these as you want inside the `Tokens` section, with each `token`/`value` pair living on its own line.

The formatting and spelling are very important for the compiler to read your files, but case-sensitivity is not important _except_ for the tokens themselves. If something is not recognized the compiler will print a syntax error message to help you debug the issue. Something **important** to note is that captions do not seem to recognize capitalized sound event names, `Addon.My_Sound` will not work, you will have to refer to it in lower-case form `addon.my_sound`.

Quotes around words define a `string`, that is a set of characters including white space. `Strings` are not required for single word definitions like `lang`, `Tokens` or `addon.my_sound` because they have no white space between them, but it is recommended to at least use them for all keyword values `"English"` (The compiler will enforce this if run in strict mode) and required for any sentence with white space `"Hello world."`.

Caption length by default is based on an option selected by the player in the settings menu, and for any dialogue line that is longer than a couple words this will be too short. Valve seems to use VCD files to define the timings of each subtitle line and we currently don't have access to these. (You can see this by playing a vo sound event through regular means like Russel's club sandwhich line "vo.05_00159", each line will zip by unlike in the release game). I have found three methods for timing custom subtitles and there are examples for each one in my public repo for [Vinny Almost Misses Christmas](https://github.com/FrostSource/vinny_christmas):

1. If it is a dialogue soundevent of type `hlvr_update_vo_default` then you can use the property `vsnd_duration` to define the exact length of the sound to help the game figure out when each caption line should appear:

        ...
        narrator.asking_for_help = 
	    {
		    type = "hlvr_update_vo_default"
		    vsnd_duration = 3.332
        ...
    
    https://github.com/FrostSource/vinny_christmas/blob/main/soundevents/narrator_soundevents.vsndevts

2. The above can also be accomplished by using the `<len:>` tag to define the number of seconds the caption should display for, this is especially useful for types that don't support `vsnd_duration`.

    **GITHUB LINK HERE**

3. Split up long dialogue into multiple files/sound events and define the timings in scripts or Hammer.

    https://github.com/FrostSource/vinny_christmas/blob/main/soundevents/vinny_christmas_soundevents.vsndevts
    (see "vinny.jerma_saw_failed_attempts_01" /02 /03)

4. Cheat long dialogue files that can't be split by creating separate silent sound events and splitting up the caption lines into each one.

    **CAPTION SOUNDEVENTS HERE**

Sounds played on player using the property "ToLocalPlayer" do not seem to display captions. The workaround I used for this is with a speaker script that plays the passed in sound using `EmitSoundOn(sound, Entities:GetLocalPlayer())` or equivalent. 

---

# Removing base game captions

Removing/overwriting base game captions is simple, requiring you just to assign a blank value to a sound event name. Below we will remove some Alyx ammo captions and replace some SFX captions:

    // Previously "<clr:255,190,255>Last three."
    "vo.01_20179"	""
	"vo.01_20181"	""
	"vo.01_20183"	""
	"vo.01_20185"	""

    // Invetory replacements
    "inventory.resingrab"	"<sfx:1>[Resin Pulled Out Of Backpack]"
    "inventory.backpackgrabitemresin"	"<sfx:1><norepeat:1>[Resin Stored Inside Backpack]"
