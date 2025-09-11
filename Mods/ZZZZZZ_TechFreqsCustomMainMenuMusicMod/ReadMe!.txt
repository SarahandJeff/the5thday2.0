

The TechFreqs Custom Main Menu Music Mod, V1.2P,replaces the vanilla main menu music with custom tracks, offering a personalized audio experience. It’s built with flexibility and polish in mind, controlled via a config.json file.


Key Features:
Custom Folders:
Plays custom .mp3 tracks from a specified folder (default: Resources folder within the mod folder and or, configurable to any path like C:\\Music\\Rock folder).

( should support .wav, .ogg  and .aiff files just like mp3s, but for now just copy some mp3s into the resources folder and it should play them upon startup)

Music Fade: Fades out music smoothly (default 2 seconds) when the game world finishes loading (createWorld() done), mimicking vanilla behavior.

Music Pauses: Pauses during game load and resumes (or restarts) when returning to the main menu.

Music Queuing: Queues multiple tracks, cycling through them sequentially or randomly.

Config Json Config Options:
MusicFolder: Where to load tracks (e.g., "Resources" or "C:/CustomMusic/").
MaxTracks: Limits loaded tracks (e.g., 10, or -1 for unlimited).
Volume: Sets playback volume (e.g., 0.8, range 0.0-1.0).
LoopSingleTrack: If true, loops one track; if false, plays the queue.
FadeDuration: Fade-out time in seconds (e.g., 2.0).
ShuffleTracks: Randomizes playback order if true.
FadeInDuration: Optional fade-in time on resume/restart (e.g., 0.0 to disable).


Behavior:
Starts playing on the main menu shortly after logo plays out, overriding vanilla music. ( tested using with and without SkipNewsMod and it just works)
Fades out when entering a game, resumes (or moves to the next track) when exiting back to the menu.
Handles short/long tracks seamlessly, restarting or advancing as needed.


Config JSON Example:

MusicFolder (string): Path to load tracks from. Default "Resources", but you can set it to something like "Z:/Music/".
MaxTracks (int): Limit the number of tracks loaded (e.g., 10). Set to -1 for unlimited.
Volume (float): AudioSource volume (0.0 to 1.0, e.g., 0.8).
LoopSingleTrack (bool): If true, loops the current track instead of moving to the next. If false, cycles through the queue.
FadeDuration (float): Time in seconds for fade-out (e.g., 2.0, adjustable to taste).

Extras:
ShuffleTracks (bool): Randomize track order instead of sequential.
FadeInDuration (float): Optional fade-in time when resuming/restarting (set to 0 to disable).


{
  "MusicFolder": "Resources",
  "MaxTracks": 10,
  "Volume": 0.8,
  "LoopSingleTrack": false,
  "FadeDuration": 2.0,
  "ShuffleTracks": false,
  "FadeInDuration": 0.0
}



Disclaimer:
By using this mod, you acknowledge that TechFreq is not responsible for any issues, crashes, or conflicts caused by its use.
Use at your own risk. Please backup your game files before installing any type of mod.
Thanks for downloading and enjoy!




Installation: 
Make sure harmony mod exist in the mod directory as it's required.
Download the mod files, Extract Mod files.
Please backup your world, save, and or game files.
Place them in your Mods directory of your 7 Days to Die Game.
EAC must be disabled, although i hope in the future that can be changed, as for now DLLS are not EAC supported however XML has no issue, unfortunately this is a dll modification.
THIS IS CLIENT SIDE ONLY but maybe perhaps this is also, server side and client side compatibility?
No further setup needed. Enjoy!




CREDITS:
Thanks to TechFreq & A.I, ChatGPT or Microsoft CoPilot A.I or Grok AI from Twitter or X, for helping me create the modlet, aswell as with very little modding knowledge for the game and learning as i go i couldn't do this without it and overall brainstorming and or the modding community.
I’d very much appreciate it and or any feedback for the mod(s) aswell



Support Notice:
This mod may or may not be crossposted onto, 7DaystoDieMods. For those who’d like to support 'TechFreqs' work, downloading via ModsFire on their website is (ad-powered, which earns per download) helps me a ton! There is also a direct MEGA mirror that's also available on their website, besides NexusMods which also features direct links which those aren't earned per click or download and run off Donation Points or via 'TechFreqs' Donation Links through paypal or kofi.
Also the following links below and in bio, are (ad-powered, which earns per click) and helps me a bunch!
once an ad has been seen and going through the prompts for clicking the continue button on OUO shortened links, will grant you the destination to 'TechFreqs' social media or tip pages.
However, Donations aren't expected, every little bit of support helps along the way & fuels more mods, music, and bug fixes in the future ,so thanks again for being awesome in general and checking out the mod post.


Social Media:
If you appreciate 'TechFreqs' work and want to show support, use this donation link, although not necessary. 
Kofi Page: https://ouo.io/KHl59h
I appreciate it in general for just checking out the mod posts, sharing and enjoying any of the mods in itself. Thank you again! and Happy gaming!


Love this mod? Got feedback or ideas or need to troubleshoot? 
Join the TechFreq Pretty Rad Squad Discord Server! https://ouo.io/FXCrA1
Chill with us on Discord for game chat, memes, and even more mod updates!

As for TechFreqs music, it's royalty-free music to use in your projects or for casual listening!
Source music files are available feel free to ask away, available in the discord! or for more content! 
TechFreqs Socials: https://ouo.io/CjpGJ6
Checkout the behind-the-scenes vibes today! Thank you again for checking out the mod post.