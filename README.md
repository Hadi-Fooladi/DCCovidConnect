# DCCovidConnect
DCCovidConnect is an app that aims to keep the DC Metropolitan area informated about the coronavirus by collecting data from varius APIs into one easy to navigate place.

# App Structure
This app's layout and navigation system uses [Xamarin.Forms' Shell](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/shell/) which uses a URI-based navation scheme.

# Pages

## Home Page
This page serves to provide at a glance information to the user that is based on the location that they input.
* COVID-19 Statistics
* Reopening Phase the state is in
* Recent news by DCCOVIDConnect
* Social Media feed from DCCovidConnect

## Map Page
This page houses the spread of the coronavirus cases in the United States

## Info Page
This page is where all of the information is stored. It connects to the WordPress database to scrape all of the information from the html stored and displays it on the app. The parser's impremention can be found [here](#wordpress-parser).

When the user enters a InfoDetailPage, the app then reads from the stored parsed JSON object and outputs view objects to create the page based on the type.

### Type Ordering
When reading the JSON object, it gets passed through a series of methods to improve readability.
1. `Parse()` to handle general layouts
2. `ParseView()` to handle individual components such as images
3. `ParseLabel()` to handle text based objects
4. `ParseText()` serves as a helper to ParseLabel()

**TODO**:
 - [ ] Search

## Settings Page
This page houses all of the settings of the app.<br/>
Current Settings include:
* Dark Mode
* Default State (Used for Home and Map page)

*To be moved as a button in Home Page*

# Services
## WordPress Parser
The parser class parses a WordPress page's HTML and converts it into a nested JSON object of nodes.
Each node is defined as:
```typescript
{
    type: [Type type],
    href?: string,
    img?: string,
    src?: string,
    text?: string,
    children?: node[]
}
```
*href, img, and src are based on if the html tag attribute exists*

## Settings
The Settings in the app are handled via the [Xam.Plugins.Settings](https://www.nuget.org/packages/Xam.Plugins.Settings/) NuGet package.

It's documentation can be found [here](https://jamesmontemagno.github.io/SettingsPlugin/).

## MapService
This service serves to handle all of the Map data and is loaded on startup. It loads all of the State and County data and their SVGs to a dictionary to be used in the Map page and other parts of the app.