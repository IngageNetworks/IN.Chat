Chat
===
Chat is an example chat application built on top of the INgage Platform

Getting Started
---
####Basic (*For beginners*)
* **Private Messages** Just like twitter, use ```@username <message>``` to privately message somebody. Your private messages will only be visible to the recipient, and you.
* **Emojis** Your typed emoticons will automatically be converted into the emoji image. This ```:)``` automatically becomes :smile:.
* **Content Providers**
    * **Images** Turns image urls into embedded images.
    * **YouTube** Turns YouTube urls into embedded videos.

####Advanced (*Become a power-user*)
#####Commands
*   **Bacon** Returns an image of bacon, or an image of bacon on top of a website.
    * ```/bacon```
    * ```/bacon <url>```
*   **CI Continuous Integration Information**
    * ```/ci```
*   **Face Me** Applies an item (random, hipster glasses, clown nose, mustache, scumbag steve hat, or Jason mask) to any face it finds.
    * ```/face <image url>```
    * ```/hipster <image url>```
    * ```/clown <image url>```
    * ```/mustache <image url>```
    * ```/scumbag <image url>```
    * ```/jason <image url>```
*   **Ping** Ping, Echo, and Time services
    * ```/ping```
    * ```/echo <text>```
    * ```/time```
*   **Rally** Returns the specified Rally item.
    * ```/rally us<number>```
    * ```/rally ta<number>```
    * ```/rally de<number>```
*   **Stock** Stock information
    * ```/stock <ticker>```
*   **Weather** Weather information, current & forcasts
    * ```/weather <city or zip code>```

About
---

###Technology
* [ASP.NET MVC 3](http://www.asp.net/mvc)
* [SignalR](https://github.com/SignalR/SignalR)
* [Redis](http://redis.io/)
* [NLog](http://nlog-project.org/) + [logentries](https://logentries.com/)
* [Twitter Bootstrap](http://twitter.github.com/bootstrap/) + [Glyphicons Free](http://glyphicons.com/)
* [jQuery](http://jquery.com/)

###APIs
* Authentication - [INgage Networks's API](http://developer.ingagenetworks.com/)
* CI Command - [Jenkins's API]()
* Face Me Command - http://stacheable.herokuapp.com & [faceup.me](http://faceup.me/)
* Rally Command - [Rally's API](http://developer.rallydev.com/)
* Stock Command - Yahoo's Finance API (undocumented)
* Weather Command - Google's Weather API (undocumented)