<h1 align="center">uGUI UI Manager</h1>

Library for screen transitions, transition animations, transition history stacking, and screen lifecycle management in Unity's uGUI.

<p align="center">
  <img width="80%" src="https://user-images.githubusercontent.com/47441314/137313323-b2f24a0c-1ee3-4df0-a175-05fba32d9af3.gif" alt="Demo">
</p>

## Table of Contents
<details>
<summary>Details</summary>

- [Overview](#overview)modalconscre
    - [Features](#features)

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
//await handle.Task; // You can also use await.
//handle.OnTerminate += () => { }; // You can also use callback.
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

