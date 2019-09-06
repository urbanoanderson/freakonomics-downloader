# Freakonomics Radio Downloader

![Feakonomics radio](http://freakonomics.com/wp-content/themes/freako_2.0/images/logo_apple_left.png)

## About

Freakonomics Radio is an amazing podcast by Stephen J. Dubner. The show is free and every episode can be found on `http://freakonomics.com/archive/` but there is no way to download all episodes at once and spotify does not have them all. This simple .NETCore program uses selenium to access the archive and download all the episodes of the podcast.

If you like the show, consider accessing the official web page and supporting it.

## Setup

- Install .Net Core 2.2
- Install Google Chrome
- Download Chrome's Selenium WebDriver available [here](https://chromedriver.chromium.org/downloads)
- Edit `appsettings.json` with the path to your WebDriver file (without the filename) and your desired output folder

## To Run

- Run command: `dotnet run` and wait a lot
- Go listen!
