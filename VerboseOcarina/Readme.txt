Verbose Ocarina by mzxrules, 2013-2019
November 26th, 2019 release 

# Intro
This tool is designed to print out various stats in regards to scene and room
files. Initally a rough program created to understand scene/room headers in
order to understand the so called "wrong warp" glitch, this program can now
parse all 101 scenes within the commercially released roms. 

# Using
When first starting the program, it will want you to point it toward a NTSC
1.0 rom. If you don't have a 1.0 rom, simply select a dummy file. You can 
change the file it looks at later by editing "rompath.xml"

# Features
- Automatic rom decompression: Provided the rom is in "big endian order", the
  parser is able to extract information from a compressed rom.

- Supports nearly all known versions of Ocarina of Time and Majora's Mask

- Automaticly saves the path to each rom to "rompath.xml".

- Scene/Room parser: Converts the scene and room headers into a more readable
  form. When "scene" is selected:

      Fetch: parses a single scene and any child rooms. Unreferenced scene or
      room headers will not be listed.

      Fetch All: parses all scenes within a given rom.

- Actor/Object/Sceneword (scene/room headers) finder: 

      Fetch: Finds all actors, objects, or scenewords with a given id across
      all scenes and rooms

      Fetch All: Lists all actors,objects, or scenewords across all scenes
      and rooms

- Cutscene parser: Converts cutscenes into a readable form.
    When "cutscene" is selected:

      Fetch: given a rom address, treats the data as if it were a cutscene.

      Fetch All is not used.

 - Text dump: dumps game text.
    When "text" is selected:

      Fetch: Dumps all text for a given text id, for all languages supported
      by the rom.
       
      Fetch All is not used.