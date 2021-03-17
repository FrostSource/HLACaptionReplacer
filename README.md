# HLACaptionReplacer

This program can, in theory, write Source 2 caption files in the compiled format. Captions can be successfully modified provided they don't change the byte order too much.
I still don't understand the significance of the Blocks within the caption file; adding just one more will cause it to be loaded incorrectly.
Custom sound events also won't trigger the captions that relate to them.

HLACaptionReplacer takes two input files through command line arguments to modify a caption file, e.g. `closecaption_english.dat` and `my_modify_file.txt`
It then outputs a new file in the same location as the compiled caption file, e.g. `closecaption_english_new.dat`

This example modification file replaces lines that show during the `speech:dark_convo_2` SpeakConcept:
```
// Basic comments work

// A sound event name on its own tells the program to delete the caption
vo.01_13035

// Sound event name followed by caption text tells the program to replace it with that text
vo.05_02219 <HEADSET><clr:210,100,210>Example replaced caption
vo.01_13036 <clr:100,190,100>Umm… do you have <i>anything<i> to say to me mate??
vo.05_00158 <HEADSET><clr:210,100,210>Ah! Check this!<cr>Alyx, you one fine hottie, ya know that?
```

If you don't want to build and run HLACaptionReplacer separately, you can look at the Test project to see how you can run the program with your chosen arguments.
