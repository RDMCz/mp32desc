# mp32desc

Creates text summaries for collections of your mp3 (or other audio) files.

## Screenshot

![1.0.2](./Screenshot.jpg)

## Why?

When I upload albums/custom mixes to YouTube, I use this program to automatically generate descriptions with proper timestamps, so YouTube will correctly segment the video timeline. Sometimes, I use it to take notes about tracks in an "unknown" playlist I'm currently listening to. It could also be used to share information about your offline playlists with others, but there might be better ways to do it. This program is probably pretty niche, but it's definitely useful to _someone_.

## Dependencies

* .NET 8
* [Zeugma440/atldotnet](https://github.com/Zeugma440/atldotnet) ([MIT](https://github.com/Zeugma440/atldotnet/blob/main/LICENSE))

## Changelog

### 1.0.3

* _Reveal in explorer_ button improvements/fixes
  * Can now open paths with commas in them
  * If there is a window with this path already opened, don't open it again and just focus the existing one
* Added app icon

### 1.0.2

* Show which and how many unsupported/non-audio files were not loaded
* Explicit file order by their full path (ascending) (instead of implicit `Directory.EnumerateFiles` order)

### 1.0.1

* Inform user if selected directory is empty

### 1.0.0

* Initial release
