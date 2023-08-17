<h1 align="center">uGUI UI Manager</h1>

Library for screen transitions, transition animations, transition history stacking, and screen lifecycle management in Unity's uGUI.

<p align="center">
  <img width="80%" src="https://user-images.githubusercontent.com/47441314/137313323-b2f24a0c-1ee3-4df0-a175-05fba32d9af3.gif" alt="Demo">
</p>

## Table of Contents
<details>
<summary>Details</summary>

- [Overview](#overview)
     - [Features](#features)
- [Basic Screen Transition](#basic-screen-transition)
     - [Concept of screens and transition](#concept-of-screens-and-transitions)
     - [Create page and transition](#create-page-and-transition)
     - [Create modal and transition](#create-modal-and-transition)
     - [Create sheet and transition](#create-sheet-and-transistion)
</details>

## Overview

#### Features
* You can create pages, modals, tabs and their transitions easily and flexibly.
* Manage screen lifecycle and memory from load to destroy.
* Separated workflow with animators for complex screen transition animations.
* Well-separated library with no extra functions (ex. GUI library, state machine).
* And standard features such as history stacking and click prevention during transitions.

## Basic Screen Transition

### Concept of screens and transitions
Unity Screen Navigator classifies screens into threee types: "Page", "Modal" and "Sheet".

"Page" is the screen thast transition in sequence. For example, when you transition form the Page A to Page B, Page A will be stacked in the history. And when you return form Page B. Page A will be redisplayed with its states intact

<p align="center">
  <img width="50%" src="https://user-images.githubusercontent.com/47441314/136680850-2aca1977-02c2-4730-a0d8-603934f71c80.gif" alt="Demo">
</p>

"Modal" is a screen that is stacked in a window. When it is displayed, all interaction except for the foreground modal will be blocked.

<p align="center">
  <img width="50%" src="https://user-images.githubusercontent.com/47441314/136698982-21ff5172-e38d-4d80-a976-a7ecc511c048.gif" alt="Demo">
</p>

And "Sheet" is used for tab-like GUI. History is not managed, and only one active screen is displayed.

<p align="center">
  <img width="50%" src="https://user-images.githubusercontent.com/47441314/136700074-2a4fa134-dc5d-4b72-90d8-f6b12c91fc0f.gif" alt="Demo">

These screens can be nested. And, the area of each screen can be freely specified (not necessarily the entire window).

</p>
<p align="center">
  <img width="50%" src="https://user-images.githubusercontent.com/47441314/137634860-ae202ce7-5d2d-48b1-a938-358381d16780.gif" alt="Demo">
</p>

### Create page and transition
To create the page transition, first attach the "Screen Containter" component to and GameObject under the Canvas. The pages will be displayed to fit it, so adjust the size. Next, attach the `Screen` component to root GameObject under the Resources folder with an arbitrary name. And call `ScreenContainer.Push()` with the Resources path to display the page.

```cs
ScreenContainer screenContainer;

var handle = screenContainer.Push("ExamplePage", true);

// wait for the transition to finish
yield return handle;
```

Also, use `ScreenContainer.Pop()` to discard the active page and display the previous page.

```cs
ScreenContainer screenContainer;

// Pop the active page
var handle = screenContainer.Pop(true);

// Wait for the transition to finish.
yield return handle;
```

### Create modal and transition
To create the modal transition, first attach the "Modal Container" component to an GameObject under the Canvas. In general, modals are desinged to cover ther entire window with their backdrop and block clicks. Therefore, the size of the GameObject should basically be set to match the window size. Next, attach `Modal` component to the root GameObject of the modal view. This root GameObject will be  adjusted to fit the size of the ` Modal Container`. So if you want to create the modal with margins, create a child GameObject with a smaller size and create the content inside it.

<p align="center">
  <img width="70%" src="https://user-images.githubusercontent.com/47441314/136698661-e4e247b6-7938-4fb5-8f6f-f2897f42eebe.png" alt="Demo">
</p>

Place this GameObject under the Resources folder with an arbitrary name. And call `ModalContainer.Push()` with the Resources path to display the page.

```cs
ModalContainer modalContainer;

var handle = modalContainer.Push("Exmaple", true);

// Wait for the transition to finish.
yield return handle;
```

Also, use `ModalContainer.Pop()` to discard the active modal and display the previous modal.

```cs
ModalContainer modalContainer;

// Pop the active modal.
var handle = modalContainer.Pop(true);

// Wait for the transition to finish.
yield return handle;
```

#### Create Sheet and transistion
To create the sheet transition, first attach the "Sheet Container" component to an GameObject under the Canvas. The sheets will be displayed to fit it, so adjust the size. Next, attach `Sheet` component to the root GameObject of the sheet view. Place this GameObject under the Resources folder with and arbittrary name. Call `SheetContainer.Register()` with ther Resources path to create the sheet. After it is created, you can change the active sheet by calling `SheetContainer.Show()`.

```cs
SheetContainer sheetContainer;

// Instantiate the sheet named "ExampleSheet"
var registerHandle = sheetContainer.Register("ExampleSheet");
yield return registerHandle;

// Show the sheet named "ExampleSheet"
var showHandle = sheetContainer.Show("ExampleSheet", false);
yield return showHandle;
```

Note that when multiple sheets with same resource keys are instantiated by the `Register()` method, the identity of the sheet instance cannot guaranteed by the resource key.  
In such case, use the sheet ID instead of the resource key, as shown below.

```cs
SheetContainer sheetContainer;

// Instantiate the sheet named "ExampleSheet" and get the sheet id.
var sheetId = 0;
var registerHandle = sheetContainer.Register("ExampleSheet", x =>
{
    sheetId = x.sheetId;
});
yield return registerHandle;

// Show the sheet with sheetId.
var showHandle = sheetContainer.Show(sheetId, false);
yield return showHandle;
```

Also, to hide the active sheet instead of switching it, use the `Hide()` method.

```cs
SheetContainer sheetContainer;

// Hide the active sheet.
var handle = sheetContainer.Hide(true);

// Wait for the transition to finish.
yield return handle;
```

#### How to wait for the transition
Each method for transition returns `AsyncProcessHandle` as the return value. Using this object you can wait for the transition process to finish. You can use coroutines, asynchronous methods, and callbacks to do this. to wait in a coroutine, use ` yield return` as shown below.

```cs
yield return screenContainer.Push("ExamplePage", true);
```

To wait in an asynchronous method, use await for `AsyncProcessHandle.Task` as follows.

```cs
await screenContainer.Push("ExamplePage", true);
```

#### Getting containers with static methods

Each container (`PageContainer` / `ModalContainer` / `SheetContainer`) has static methods to get the instance. Using `Container.Of()` as follows, you can get the container that is attached to the nearest parent form the given Transform or RectTransform.

```cs
var screenContainer = ScreenContainer.Of(transform);

var modalContainer = ModalContainer.Of(transform);

var sheetContainer = SheetContainer.Of(transform);
```

Also, you can set the `Name` property in the container's Inspector to get the container by its name. In this case, use the `Container.Find()` method as follows.

```cs
var screenContainer = ScreenContainer.Find("SomePageContainer");

var modalContainer = ModalContainer.Find("SomeModalContainer");

var sheetContainer = SheetContaiern.Find("SomeSheetContainer");
```

## Screen Transition Animation

#### Setting common transition animations
In default, a standard transition animation is set for reach screen type. Ypu can create a class derived from `TransitionAnimationObject` to create custom transition animation. This class has a property and methods to define the animation behavior.

```cs
// Duration (Second).
public abstract float Duration {get;}

// Initialize.
public abstract void Setup();

// Define the state at this time.
public abstract void SetTime();
```

Refer to [SimpleTransitionAnimationObject]() for the practical implementation.

Then instantiate this ScriptableObject, and assign it to `UnityScreenNavigatorSettings`. You can create `UnityScreenNavigatorSettings` from `Assets > Create > Screen Navigator Settings`.

<p align="center">
    <img width="60%" src="https://user-images.githubusercontent.com/47441314/137321487-e2267184-6eba-46a7-9f4e-468176822408.png">
</p>

#### Setting transition animation for each screen
You can also set up different animation for each screen. Each Page, Modal, and Sheet component has the `AnimationContainer` property. You can set the transition animation to it.

<p>
    <img width="60%" src="https://user-images.githubusercontent.com/47441314/137632127-2e224b47-3ef1-4fdd-a64a-986b38d5ea6a.png">
</p>

You can change the transition animation of this screen by setting the `Asset Type` to `Scriptable Object` and assigning the `TransitionAnimationObject` described in the previous section to `Animation Object`. Also, you can use MonoBehaviour instead of the ScriptableObject. In this case, first create a class that extends `TransitionAnimationBehaviour`.

Refer to [SimpleTransitionAnimationBehaviour]() for practical implementation.
Then, attach this component and set the `Asset Type` to `MonoBehaviour` and assign the reference to `Animation Behaviour`.

#### Change transition animation according to partner screen
For example, when screen A enters and screen B exits, screen B is called the "Partner Screen" of screen A. If you enter the name of the partner sreen in the property shown below, the transition animation will be applied only when this name matches the partner screen name.
 <p>
    <img width="60%" src="https://user-images.githubusercontent.com/47441314/137632918-9d777817-d2dc-43c9-bd7e-c6a1713a5f26.png">
 </p>