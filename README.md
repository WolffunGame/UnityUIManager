<h1 align="center">uGUI UI Manager</h1>

Library for screen transitions, transition animations, transition history stacking, and screen lifecycle management in Unity's uGUI.

[//]: # (<p align="center">)

[//]: # (  <img width="80%" src="https://user-images.githubusercontent.com/47441314/137313323-b2f24a0c-1ee3-4df0-a175-05fba32d9af3.gif" alt="Demo">)

[//]: # (</p>)

## Table of Contents
<details>
<summary>Details</summary>

- [Overview](#overview)
     - [Features](#features)
- [Basic Screen Transition](#basic-screen-transition)
     - [Concept of screens and transition](#concept-of-screens-and-transitions)
     - [Create screen and transition](#create-screen-and-transition)
     - [Create modal and transition](#create-modal-and-transition)
     - [Create sheet and transition](#create-sheet-and-transistion)
     - [How to wait for the transition](#how-to-wait-for-the-transition)
     - [Getting containers with static methods](#getting-containers-with-static-methods)
- [Screen Transition Animation](#screen-transition-animation)
     - [Setting common transition animations](#setting-common-transition-animations)
     - [Setting transition animation for each screen](#setting-transition-animation-for-each-screen)
     - [Change transition animation according to partner screen](#change-transition-animation-according-to-partner-screen)
     - [Create simple transition animation and drawing order](#screen-transition-animation-and-drawing-order)
     - [Create simple transition animations easily](#create-simple-transition-animations-easily)
     - [Create animation with Timeline](#create-animation-with-timeline)
- [Lifecycle Events](#lifecycle-events)
     - [Lifecycle events of the screen](#lifecycle-events-of-the-screen)
     - [Lifecycle events of the modal](#lifecycle-events-of-the-modal)
     - [Lifecycle events of the sheet](#lifecycle-events-of-the-sheet)
</details>

## Overview

#### Features
* You can create screens, modals, tabs and their transitions easily and flexibly.
* Manage screen lifecycle and memory from load to destroy.
* Separated workflow from animators for complex screen transition animations.
* Well-separated library from no extra functions (ex. GUI library, state machine).
* And standard features such as history stacking and clicks prevention during transitions.

## Basic Screen Transition

### Concept of screens and transitions
Unity Screen Navigator classifies screens into three types: "Screen", "Modal" and "Sheet".

"Screen" is the screen that transition in sequence. For example, when you transition form Screen A to Screen B, Screen A will be stacked in the history. And when you return from Screen B. Screen A will be redisplayed with its states intact.

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

### Create screen and transition
To create the screen transition, first attach the "Screen Container" component to and GameObject under the Canvas. The screens will be displayed to fit it, so adjust the size. Next, attach the `Screen` component to root GameObject under the Resources folder with an arbitrary name. And call `ScreenContainer.Push()` with the Resources path to display the screen.

```cs
ScreenContainer screenContainer;

var handle = screenContainer.Push("ExampleScreen", true);

// wait for the transition to finish
yield return handle;
```

Also, use `ScreenContainer.Pop()` to discard the active screen and display the previous screen.

```cs
ScreenContainer screenContainer;

// Pop the active screen
var handle = screenContainer.Pop(true);

// Wait for the transition to finish.
yield return handle;
```

### Create modal and transition
To create the modal transition, first attach the "Modal Container" component to an GameObject under the Canvas. In general, modals are desinged to cover ther entire window with their backdrop and block clicks. Therefore, the size of the GameObject should basically be set to match the window size. Next, attach `Modal` component to the root GameObject of the modal view. This root GameObject will be adjusted to fit the size of the ` Modal Container`. So if you want to create the modal with margins, create a child GameObject with a smaller size and create the content inside it.

<p align="center">
  <img width="70%" src="https://user-images.githubusercontent.com/47441314/136698661-e4e247b6-7938-4fb5-8f6f-f2897f42eebe.png" alt="Demo">
</p>

Place this GameObject under the Resources folder with an arbitrary name. And call `ModalContainer.Push()` with the Resources path to display the screen.

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
To create the sheet transition, first attach the "Sheet Container" component to an GameObject under the Canvas. The sheets will be displayed to fit it, so adjust the size. Next, attach `Sheet` component to the root GameObject of the sheet view. Place this GameObject under the Resources folder with and arbittrary name. Call `SheetContainer.Register()` with their Resources path to create the sheet. After it is created, you can change the active sheet by calling `SheetContainer.Show()`.

```cs
SheetContainer sheetContainer;

// Instantiate the sheet named "ExampleSheet"
var registerHandle = sheetContainer.Register("ExampleSheet");
yield return registerHandle;

// Show the sheet named "ExampleSheet"
var showHandle = sheetContainer.Show("ExampleSheet", false);
yield return showHandle;
```

Note that when multiple sheets with same resource keys are instantiated by the `Register()` method, the identity of the sheet instance cannot guaranteed by the resource key. In such case, use the sheet ID instead of the resource key, as shown below.

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
yield return screenContainer.Push("ExampleScreen", true);
```

To wait in an asynchronous method, use await for `AsyncProcessHandle.Task` as follow.

```cs
await screenContainer.Push("ExampleScreen", true);
```

#### Getting containers with static methods

Each container (`ScreenContainer` / `ModalContainer` / `SheetContainer`) has static methods to get the instance. Using `Container.Of()` as follow, you can get the container that is attached to the nearest parent form the given Transform or RectTransform.

```cs
var screenContainer = ScreenContainer.Of(transform);

var modalContainer = ModalContainer.Of(transform);

var sheetContainer = SheetContainer.Of(transform);
```

Also, you can set the `Name` property in the container's Inspector to get the container by its name. In this case, use the `Container.Find()` method as follow.

```cs
var screenContainer = ScreenContainer.Find("SomeScreenContainer");

var modalContainer = ModalContainer.Find("SomeModalContainer");

var sheetContainer = SheetContaiern.Find("SomeSheetContainer");
```

## Screen Transition Animation

#### Setting common transition animations
In default, a standard transition animation is set for reach screen type. You can create a class derived from `TransitionAnimationObject` to create custom transition animation. This class has a property and methods to define the animation behavior.

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
You can also set up different animation for each screen. Each Screen, Modal, and Sheet component has the `AnimationContainer` property. You can set the transition animation to it.

<p align ="center">
    <img width="60%" src="https://user-images.githubusercontent.com/47441314/137632127-2e224b47-3ef1-4fdd-a64a-986b38d5ea6a.png">
</p>

You can change the transition animation of this screen by setting the `Asset Type` to `Scriptable Object` and assigning the `TransitionAnimationObject` described in the previous section to `Animation Object`. Also, you can use MonoBehaviour instead of the ScriptableObject. In this case, first create a class that extends `TransitionAnimationBehaviour`.

Refer to [SimpleTransitionAnimationBehaviour]() for practical implementation.
Then, attach this component and set the `Asset Type` to `MonoBehaviour` and assign the reference to `Animation Behaviour`.

#### Change transition animation according to partner screen
For example, when screen A enters and screen B exits, screen B is called the "Partner Screen" of screen A. If you enter the name of the partner sreen in the property as shown below, the transition animation will be applied only when this name matches the partner screen name.

 <p align="center">
    <img width="60%" src="https://user-images.githubusercontent.com/47441314/137632918-9d777817-d2dc-43c9-bd7e-c6a1713a5f26.png">
 </p>

 In default, the prefab name is used as the screen name. If you want to name it explicitly, uncheck `Use Prefab Name as Identifier` and enter a name in the `Identifier` property.

<p align="center">
    <img width="60%" src="https://user-images.githubusercontent.com/47441314/137632986-f5727a42-4c27-48aa-930d-e7b0673b978f.png">
</p>

In addition, regular expressions can be used for the `Partner Screen Identifier Regex`. And if multiple animations are set, they will be evaluated in order from top.

#### Screen transition animation and drawing order

In the transition animation of a screen with a partner screen, the drawing order can be important. For example, an animation where the screen covers the partner screen. If you want to control the drawing order, use the `Rendering Order` property.

<p align="center">
    <img width="60%" src="https://user-images.githubusercontent.com/47441314/137633021-4e864c77-baa0-4d42-a8e7-b0183f7302f5.png">
</p>

During the screen transtion, the screen is drawn in the order of decreasing this value. Note that modals do not have a `Rendering Order` property, since the newest one is always displayed in front.

#### Create simple transition animations easily
You can use `SimpleTransitionAnimationObject` as a transition animation implementation. This can be created from `Asset > Create Screen Navigator > Simple Transition Animation`. Then, a ScriptableObject as shown below will be generated, and you can set up the animation from the inspector.

<p align="center">
    <img width="60%" src="https://user-images.githubusercontent.com/47441314/137326944-112e0254-cd27-4d49-a32b-9c436b9537e4.png">
</p>

You can also use `SimpleTransitionAnimationBehaviour` as a MonoBehaviour implementation of this. This is used by attaching directly to a GameObject.

<p align="center">
  <img width="60%" src="https://user-images.githubusercontent.com/47441314/137326555-90cdce8d-98da-4a00-99cc-5a65c1086760.png">
</p>

|Property Name|Description|
|-|-|
|Delay|Delay time before the animation starts (seconds).|
|Duration|Animation duration (seconds).|
|Ease Type|Type of the easing functions.|
|Before Alignment|Relative position from the container before transition.|
|Before Scale|Scale before transition.|
|Before Alpha|Transparency before transition.|
|After Alignment|Relative position from the container after transition.|
|After Scale|Scale after transition.|
|After Alpha|Transparency after transition.|

#### Create animation with Timeline
You can use Timeline to create transition animation.
It is recommended to use Timeline for complex transitin animation.

<p align="center">
  <img width="60%" src="https://user-images.githubusercontent.com/47441314/137634258-135b454e-04b5-49e8-a87a-bfb6ede03f49.gif">
</p>

To implement this, first attach the `Timeline Transition Animation Behaviour` to a GameObject. And assign `Playable Director` and `Timeline Asset` to properties.

<p align="center">
  <img width="60%" src="https://user-images.githubusercontent.com/47441314/137633599-dd8b204e-e6ec-46bf-b93c-ee54b4ac3d59.png">
</p>

`Play On Awake` property of `Playable Director` need to be unchecked.

<p align="center">
  <img width="60%" src="https://user-images.githubusercontent.com/47441314/137633492-4d837177-a381-486f-8942-df26e522da91.png">
</p>

Finally, assign this `Timeline Transition Animation Behaviour` to the `Animation Container`.

<p align="center">
  <img width="60%" src="https://user-images.githubusercontent.com/47441314/137633821-1fa1a8d6-ca41-49ca-aacf-dcf7f744c0b1.png">
</p>

## Lifecycle Events

#### Lifecycle events of the screen 
By overriding following methods in the class derived from the `Screen` class, you can write the processs associated with the lifecycle of the screen.

```cs

public class SomeScreen : Screen
{
    // Called just after this screen is loaded.
    public override IEnumerator Initialize() { yield break; }
    // Called just before this screen is released.
    public override IEnumerator Cleanup() { yield break; }
    // Called just before this screen is displayed by the Push transition.
    public override IEnumerator WillPushEnter() { yield break; }
    // Called just after this screen is displayed by the Push transition.
    public override void DidPushEnter() { }
    // Called just before this screen is hidden by the Push transition.
    public override IEnumerator WillPushExit() { yield break; }
    // Called just after this screen is hidden by the Push transition.
    public override void DidPushExit() { }
    // Called just before this screen is displayed by the Pop transition.
    public override IEnumerator WillPopEnter() { yield break; }
    // Called just after this screen is displayed by the Pop transition.
    public override void DidPopEnter() { }
    // Called just before this screen is hidden by the Pop transition.
    public override IEnumerator WillPopExit() { yield break; }
    // Called just after this screen is hidden by the Pop transition.
    public override void DidPopExit() { }
}

```

You can also register lifecycle events externally by `Screen.AddLifeCycleEvents()` as below.

```cs

// IScreenLifecycleEvent is the interface that has lifecycle events described above.
// You can specify the execution priority with the second argument.
//  Less than 0: executed before Screen lifecycle event.
//  Greater than 0: executed after Screen lifecycle event.
IScreenLifecycleEvent lifecycleEventImpl;
Screen screen;
screen.AddLifecycleEvent(lifecycleEventImpl, -1);

// It is also possible to register only some lifecycle events as follows.
IEnumerator OnWillPushEnter()
{
    // Some code.
    yield break;
}
screen.AddLifecycleEvent(onWillPushEnter: OnWillPushEnter);

```

And you can also hook transition events from the container by passing object that implements `IScreenContainerCallbackReceiver` to `ScreenContainer.AddCallbackReceiver()`.

```cs

public interface IScreenContainerCallbackReceiver
{
    // Called just before the Push transition is executed.
    void BeforePush(Screen enterScreen, Screen exitScreen);
    // Called just after the Push transition is executed.
    void AfterPush(Screen enterScreen, Screen exitScreen);
    // Called just before the Pop transition is executed.
    void BeforePop(Screen enterScreen, Screen exitScreen);
    // Called just after the Pop transition is executed.
    void AfterPop(Screen enterScreen, Screen exitScreen);
}

```

#### Lifecycle events of the modal
By overriding following methods in the class derived from the `Modal` class, you can write the processes associated with the lifecycle of the modal.

```cs

public class SomeModal : Modal
{
    // Called just after this modal is loaded.
    public override IEnumerator Initialize() { yield break; }
    // Called just before this modal is released.
    public override IEnumerator Cleanup() { yield break; }
    // Called just before this model is displayed by the Push transition.
    public override IEnumerator WillPushEnter() { yield break; }
    // Called just after this modal is displayed by the Push transition.
    public override void DidPushEnter() { }
    // Called just before this modal is hidden by the Push transition.
    public override IEnumerator WillPushExit() { yield break; }
    // Called just after this modal is hidden by the Push transition.
    public override void DidPushExit() { }
    // Called just before this modal is displayed by the Pop transition.
    public override IEnumerator WillPopEnter() { yield break; }
    // Called just after this modal is displayed by the Pop transition.
    public override void DidPopEnter() { }
    // Called just before this modal is hidden by the Pop transition.
    public override IEnumerator WillPopExit() { yield break; }
    // Called just after this modal is hidden by the Pop transition.
    public override void DidPopExit() { }
}

```

You can also register lifecycle events externally by `Modal.AddLifecycleEvents()` as below.

```cs

// IModalLifecycleEvent is the interface that has lifecycle events described above.
// You can specify the execution priority with the second argument.
//  Less than 0: executed before Modal lifecycle event.
//  Greater than 0: executed after Modal lifecycle event.
IModalLifecycleEvent lifecycleEventImpl;
Modal modal;
Modal.AddLifecycleEvent(lifecycleEventImpl, -1);

// It is also possible to register only some lifecycle events as follows.
IEnumerator OnWillPushEnter()
{
    // Some code.
    yield break;
}
modal.AddLifecycleEvent(onWillPushEnter: OnWillPushEnter);

```

And you can also hook transition events from the container by passing object that implements `IModalContainerCallbackReceiver` to `ModalContainer.AddCallbackReceiver()`.

```cs

public interface IModalContainerCallbackReceiver
{
    // Called just before the Push transition is executed.
    void BeforePush(Modal enterModal, Modal exitModal);
    // Called just after the Push transition is executed.
    void AfterPush(Modal enterModal, Modal exitModal);
    // Called just before the Pop transition is executed.
    void BeforePop(Modal enterModal, Modal exitModal);
    // Called just after the Pop transition is executed.
    void AfterPop(Modal enterModal, Modal exitModal);
}

```

#### Lifecycle events of the sheet
By overriding following methods in the class derived from the `Sheet` class, you can write the processes associated with the lifecycle of the sheet.

```cs

public class SomeSheet : Sheet
{
    // Called just after this sheet is loaded.
    public override IEnumerator Initialize() { yield break; }
    // Called just before this sheet is released.
    public override IEnumerator Cleanup() { yield break; }
    // Called just before this sheet is displayed.
    public override IEnumerator WillEnter() { yield break; }
    // Called just after this sheet is displayed.
    public override void DidEnter() { }
    // Called just before this sheet is hidden.
    public override IEnumerator WillExit() { yield break; }
    // Called just after this sheet is hidden.
    public override void DidExit() { }
}

```

You can also register lifecycle events externally by `Sheet.AddLifecycleEvents()` as below.

```cs

// ISheetLifecycleEvent is the interface that has lifecycle events described above.
// You can specify the execution priority with the second argument.
//  Less than 0: executed before Sheet lifecycle event.
//  Greater than 0: executed after Sheet lifecycle event.
ISheetLifecycleEvent lifecycleEventImpl;
Sheet sheet;
Sheet.AddLifecycleEvent(lifecycleEventImpl, -1);

// It is also possible to register only some lifecycle events as follows.
IEnumerator OnWillEnter()
{
    // Some code.
    yield break;
}
sheet.AddLifecycleEvent(onWillEnter: OnWillEnter);

```

And you can also hook transition events from the container by passing object that implements `ISheetContainerCallbackReceiver` to `SheetContainer.AddCallbackReceiver()`.

```cs

public interface ISheetContainerCallbackReceiver
{
    // Called just before the Show transition is executed.
    void BeforeShow(Sheet enterSheet, Sheet exitSheet);
    // Called just after the Show transition is executed.
    void AfterShow(Sheet enterSheet, Sheet exitSheet);
    // Called just before the Hide transition is executed.
    void BeforeHide(Sheet exitSheet);
    // Called just after the Hide transition is executed.
    void AfterHide(Sheet exitSheet);
}

```