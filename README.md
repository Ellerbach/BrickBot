# BrickBot
Bot to use various Lego databases to find quickly set, part, minifig, instructions

To try this project, go to [BrickBot](https://brickbot.azurewebsites.net) website and try it! Just start by saying hello or anything else :-)
BrickBot is using the Microsoft Bot Framework available [here](https://dev.botframework.com/).

## Project organization
The project is a webapp deployed in Azure. So the project is organized like a webapp.

![Project organization](/doc/projectorg.png)

I've tried to organized the project as logical as possible. 
* The Model folder contains all the models used by the various Lego database services as well as the meta model used in the project.
* The Services folder contains the code to access the 4 services (Bricklink, Brickset, Rebrickable and Peeron), to extract the needed data and repackage to the meta model
* Dialogs contains the main bot dialog
* Images contains the images used by the bot and the web site
* Controllers contains the call back for the bot which then call the main Dialog
* Rest of files and folders are used to host the resources, the main webpage, terms and privacy

## The Brick Services
4 main databases are used to grab the data regarding the Lego set, parts, minifig, 

[![Bricklink](/BrickBot/Images/bricklink-square.png)](http://bricklink.com) [![BrickSet](/BrickBot/Images/brickset-square.png)](http://www.brickset.com) [![Rebrickable](/BrickBot/Images/rebrickable-square.png)](http://www.rebrickable.com) [![Peeron](/BrickBot/Images/peeron-square.png)](http://www.peeron.com)

### Bricklink service
Bricklink service uses brciklink API, full documentation available [here](http://apidev.bricklink.com/redmine/projects/bricklink-api/wiki). Those API are using a kind of OAuth which is not fully standard. So the project includes the way to manage it, create the OAuthh signature and other needed elements. Please note that any signature which will include a + ou / will fail. Reason why in the code, a new nonce and signature is regenerated up to the point it won't contains any of those caracters.

### Brickset service
Brickset is using WSDL. Full documentation available [here](http://brickset.com/tools/webservices/v2). So project is using directly the WSDL to create a first service. This service is then package into a meta service to support the IBrickService interface.

### Rebrickable service
Rebrickable is using a simple key authentication for accessing the service. Full documentation available [here](https://rebrickable.com/api/) It does provide a swagger but even if the swagger is marked as valid, it does not allow to generate the classes autoimatically. So calls to the API are done manually.

### Peeron service
This is where it is extremely fun as Peeron does not have API at all. So all the data are generating crawling the generated html. The code is just straight forward, just working.

## IBrickService
The IBrickService interface is used by all the brick services. I does allow from the bot to get a generic access to any of those services based only on the meta data of the services. All the dialogues for the bots are generates automatically. It does allow as well to add any service.
```C#
public interface IBrickService
{
     bool CanGetSetInfo();
     bool CanGetPartInfo();
     bool CanGetInstructionsInfo();
     bool CanGetMinifigInfo();
     bool CanGetGearInfo();
     bool CanGetBookInfo();
     bool CanGetCatalogInfo();
     bool CanGetMOCInfo();
     BrickItem GetBrickInfo(string number, ItemType typedesc);
     ServiceProvider GetServiceProvider { get; }
     List<ItemType> GetSupportedInfo { get; }
}
```
Please note that all the brick services need to be serializable as it's a requirement for the bot. It does imply that when you're using properties inside the class, al of them need to be able to be fully serializable. Here example with the RebrickableService class
```C#
[Serializable]
public class RebrickableService : IBrickService
```

## Main Dialog
The main dialog is initialized whenvere the user type something. It does then enumerate all Brick Services and once one selected, expose all the supported request, ask for complementary needed informaiton (a set number for example), then search for the results, process them, create the hero card and go back to the main service exposure. It can be explained with the following schema:
![How it does work](/doc/howitworks.png)
This way of working does allow a very fast search for the needed information. The only inconvinient is the main menu which does display right after the results and will make the user scrolling a bit to find the the results. 

## where to find this bot
This bot can be used directly thru the webcontrol and you have it on the [BrickBot](https://brickbot.azurewebsites.net) page.
You can as well find it on:
* [Skype](https://join.skype.com/bot/db05ed1d-3f20-49c5-95b6-734000d8bbee)
* Telegram [@LegoBrickBot](https://telegram.me/LegoBrickBot)
* Facebook [Messenger](https://www.messenger.com/t/913025845467179)
BrickBot is searchable and published on the main Bot directory.

# Special thanks
Thanks a lot to the Don from Bricklink for the help on making the specific Oauth working. 
Thanks to BrickSet administrator for his help and encouragement in the project. 
Thanks to Rebrickable administrator forhis help as well. 
And finally thanks for Peeron for having the same page structure which does allow easilly to find the information :-) 