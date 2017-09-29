# Bot.Common.Dialogs

Objectivity's wrapper around base classes from  [Bot framework](https://dev.botframework.com/). This package allows user to separate from Microsoft's implementation of BaseLuisDialog. It also contains useful toolset which helps during bot development.
# Note
This project is still a work in progress, all contributions from your site will be very desirable!

## Usage:
1. Simply Iherit from Bot.BaseDialogs.Dialogs.BaseLuisDialog in your root dialog
2. Configure LUIS provider (please see details below) 
3. Add UtilsModule and BaseDialogsModule to your Autofac container
```cs
builder.RegisterModule(new BaseDialogsModule());
builder.RegisterModule(new UtilsModule());
```
### Features:

Feature                                                                |Class|Description
---                                                                     | ---  | ---
Dialog Factory| DialogFactory.cs| Class helps with dialog creation thorugh Autofac together with dependencies             | 
Date dialog| DateDialog.cs    | Ready to use dialog which asks user for a date and use parser and LUIS to get proper value                    
Skippable dialogs|SkippableChoiceDialog.cs  SkippablePromptConfirm.cs SkippableDateDialog.cs | Skippable prompts which allow user to skip answer for bot's question
QnA Maker wrapper| QnAMaker.cs   |Wrapper around [QnaMaker](https://qnamaker.ai/) classes             | 
Seting and retrieving values from bot state [(details)](#contextHelper) | ContextHelper.cs   |Helper class which allow to set given key value pair into bot state so it can be retrieved during conversation| 
Handle picking right intents [(details)](#intentPicker)| BaseLuisDialog.cs, IntentsPicker   |Handle results from LUIS, if there are 2 simillar intents, user will be prompted which one should be used|
Redirect between dialogs| BaseLuisDialog.cs   |Sets new luis intent and redirect to proper dialog| 
___

### Seting and retrieving values from bot state
Wherever in your dialogs you can use extenstion methods on IDialogContext to get or set value in Bot State. You can store values in conversation state, or user state depending on your needs.
#### example:
```cs
await context.SetValueIntoState("key", value);
var value = await context.GetValueFromState<string>("key");
```
### Picking intents from LUIS
Library is responsible for picking the most probable intent from LUIS, in order to make it work, you need to register your own implementation of ILuisServiceProvider interface, and regiser it in autofac container:
```cs
            builder.RegisterType<MyOwnServiceProvider>()
                .Keyed<ILuisServiceProvider>(FiberModule.Key_DoNotSerialize)
                .As<ILuisServiceProvider>();
```
LUIS by default will return many results and usually intent with best score wins, howerver sometimes 2 or more intents with simillar score should be presented to bot user and choise should be on their side.
In order to take control on this behaviour that put in your web.config file following entries:

```xml
    <add key="NumberOfIntentsToPickForPrompt" value="2" />
    <add key="IntentScoreDifferenceThreshold" value="0.15" />
    <add key="IntentLowScoreThreshold" value="0.60" />
```
**NumberOfIntentsToPickForPrompt** - describes how many top intents should be consider
**IntentScoreDifferenceThreshold** - describes similarity level
**IntentLowScoreThreshold** - below this threshold intents are ignored 