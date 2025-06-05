<div align="center">

# Journey

A UI/UX experiment in visual, branching browser history.

_“The journey is the reward.”_  
**Lao Tzu**

SCREENSHOT

</div>

**Journey** is a **C# WPF** user control which aims to provide the ability to view your browsing history as an interactive tree diagram.

This repo contains the following projects:

Project            | Description
-------------------|------------
**Journey**        | An implementation of the **Journey** concept. `JourneyWebView2` implements `IWebView2`, and internally wraps a `WebView2` instance.
**JourneyBrowser** | A minimal "browser" implementation that uses the `JourneyWebView2` control to demonstrate the **Journey** concept.

`JourneyWebView2` _should_ be a drop-in replacement for `WebView2`, but it was only developed as a proof-of-concept, so your mileage may vary!

## Abstract

**Journey** is an experiment into whether there could be a better - or at least more visually informative - way of presenting a users short-term browsing history. The end result is a branching, tree-based record of a users per-tab browsing history, which aims to seamlessly integrate into the browser user interface, be intuitive and easy-to-use, and visually delight the user.

## Definitions

- _**browser:**_ a software application with a graphical user interface for displaying, and navigating between, web pages

- _**browsing area:**_ the area of the _browser_ in which web content is displayed, typically the area below the address bar, and which does not include the _browser_ UI

- _**history:**_ the typical _history_ feature presented to the user that displays a list of all pages visited, across all tabs and sessions, usually with a date and time stamp, and often with the ability to search for specific pages

- _**travellog:**_ the short-term history associated with a page or tab within a _browser_ that allows for navigation backwords and forwards, sometimes referred to internally within a _browser_ as the _session history_

## Introduction

A typical use case for a _browser_ is to use a search engine to find the solution to a problem. A user will often do this by initiating a search in their _browser_ and then clicking on links that appear in the search results. The user may click on several links, returning to the search results between each link, and repeat this for a number of iterations. In doing this the user can end up in a situation whereby they have visited a number of pages, but are unable to remember which page contained the most helpful solution to their problem.

The situation could be further compounded if several of the results led to different answers on the same web site, resulting in many of the results having similar URLs and titles. The user may be forced to return to the search results and click on each link again, or use the _history_ feature of their _browser_ to attempt to find the page they were looking for. However, this can be time consuming and frustrating, especially if the user visited a number of pages in a short period of time.

The scenario posits the question of whether it could be possible for the _travellog_ to be more helpful in such scenarios. There are already a number of tools and extensions to enhance the _history_ of a _browser_ [^BetterHistory] [^BrowserHistoryPlus] [^HistoryPlus], but no tool or extension that would enhance the _travellog_.

This project was undertaken to discover a way of enhancing the _travellog_ and providing a better solution to the problem. The solution would need to allow the user to see their _travellog_ in a more visual way, and show the organic path the user took when visting pages in their _browser_.

## Goals

The following goals were identified for the project:

- **Structured**

  The tool should represent the users browsing _travellog_ as a visual structure, with each page visited represented as a node in the structure.

- **Visual**

  The tool should be a "visual tool"; visual elements, such as thumbnail images of each page visited, or animations, should be used to aid the user in using the tool.

- **Interactive**

  The tool should be interactive, allowing the user to navigate between previously visited pages by interacting directly with the tool.

- **Intuitive**

  The tool should be easy to use, and not require any additional training or documentation to use.

- **Consistency**

  The tool should follow the same interactivity principles as the _browser_; that is, elements such as mouse cursors, shortcut keys, and means of interacting with elements should be consistent between viewing a web page and viewing the travellog.

- **Seamless**

  The tool should integrate seamlessly into the users _browser_ interface, and not require any additional steps to use, other than a way of invoking the tool.

- **Performant**

  The tool should be performant, and not slow down the users browsing experience.

## Research

The first step out research into ways of visualising branching choices, which would form the core focus of the experiment.

### Network Graph

_"A network graph is a chart that displays relations between elements (nodes) using simple links. Network graph allows us to visualize clusters and relationships between the nodes quickly..."_ [^Networkgraph]

<div align="center">

![Network Graph](res/images/network-graph.png)  
_An example of a network graph_ [^Networkgraph]

</div>

Network graphs were the first visualisation considered for the project. However, they offered a level of complexity beyond the needs of the project, and the typical arrangement for a network graph meant that they did not meet the project's goals - they are typically not visually conducive to relaying the chronology of nodes.

### Flowchart

_"A flowchart is a type of diagram that represents a workflow or process. A flowchart can also be defined as a diagrammatic representation of an algorithm, a step-by-step approach to solving a task."_ [^Flowchart]

<div align="center">

![Flowchart](res/images/flowchart.png)  
_An example of a flowchart_ [^Flowchart]

</div>

Flowcharts were the next visualisation to be considered, and were a strong match for the goals of the project. However, flowcharts present nodes in the "present tense", as a series of questions, rather than showing the chronological layout of choices already made.

### Tree Diagram

_"A tree structure, tree diagram, or tree model is a way of representing the hierarchical nature of a structure in a graphical form. It is named a "tree structure" because the classic representation resembles a tree, although the chart is generally upside down compared to a biological tree, with the "stem" at the top and the "leaves" at the bottom."_ [^Treestructure]

<div align="center">

![Tree Diagram](res/images/tree-diagram.png)  
_An example of a tree diagram_ [^Treestructure]

</div>



  Tree diagrams are excellent for representing hierarchical relationships between choices. The root node represents the initial starting point, with each branch representing a different choice and it's subsequent choices and outcomes.




## Design

After the research was completed, and the goals for the project identified, the next step was to design the tool. This involved working through a number of concepts and prototypes in order to iterate towards a final design that addressed all of the identified goals, and could then be used to create a working implementation. This implementation could then be used to gather feedback from users and evaluate both the design against the goals, and whether the tool was a useful addition to the users browsing experience.

### Tree Structure

- A _general tree_ must be used to store the users travellog, given that a user may visit any number of pages after any other page during a browsing session.

- The root node of the tree structure will represent the initial page visited by the user for that browser tab or window, and each subsequent page visited will be represented as a child node of the previous page.

- Each page within the travellog will be represented equally within the tree; that is, each node should be of the same size and design.

### Visual

- Each page visited will be represented by a thumbnail image of the page _at the point at which it was navigated away from_; this is to vsually aid the user by showing each page as it was when the user last interacted with it.

- Each page visited will show the title of the page.

- Each page visited will show the URL of the page.

- Arrows should be shown to illustrate the direction of travel between pages, with the arrow pointing from the parent node to the child node.

### Interactive

- The user should be able to interact with the tree structure by clicking on any node within the tree.

- The user should be able to pan around the tree structure.

- The user should be able to scroll the tree structure.

- The user should be able to zoom in and out of the tree structure.

### Consistency

- Scrolling the mouse wheel "down" (towards the user) should move visual elements "up" (towards the top of the screen), and the inverse should apply accordingly.

- When a modifier key is held, scrolling the mouse wheel "down" (towards the user) should zoom out, and the inverse should apply accordingly.

- The CTRL key should be used as the modifier key for zooming.

- When clicking an interactive element will result in a navigation action, the mouse cursor should change to a pointer hand.

- Interacting with elements should be achieved by clicking the left mouse button.

### Intuitive

- When the user views the travellog, the current page should be highlighted in the tree structure to illustrate their current browsing "position".

- Pressing the ESC key should close the travellog and return the user to their current page.

### Seamless

- The travellog should appear within the browsing area of the users current browser tab or window.

- The current page should transition into the travellog, to visually illustrate to the user the link between their current page and the travellog.

- When the user selects a page, the travellog should transition back into the selected page, to visually illustrate to the user the link between their action and the result.

### Performant

- Browsing performance should not be impacted by the tool being available within the browser.

- The tool should transition in and out of the browsing area quickly, and not cause any noticeable lag or delay.

- The tool should remain performant when being used, ideally with no lag or delay when interacting with the tree structure.

- The tool should use as little memory as possible, such as for storing travellog information.

- The tool should not consume any CPU cycles when not being used.

## Implementation

Due to familiarity with the development environment, and the ability to rapidly develop and iterate on the design, the tool was implemented as a **WPF** application using **C#**.

To 

The tool uses a WebView2 control to display the travellog within the browsing area of the users current browser tab or window.


## Challenges
No access to history
- Would be better with full access
- GitHub issue
- Could replace state to manage history based on current branch, meaning any site could be visited, not just the active path.
- Work around is “active path”

Visual integration with webview2 - airspace issue
- Made harder to visually integrate
- Could use composition version but lose possible performance and drm

Memory usage
- Bigger history means more images
- Average image size is xxx
- Maybe cull older images, or lower res of older images? 

Couldn’t resize browser, so used screenshot

Cannot overlay webview2, so had to switch to/from image seamlessly

Poor shadow performance


## Improvements

## Conclusion

Thoughts

Video

Hopefully proves interesting

## Acknowledgements


[^BetterHistory]: [BetterHistory.io](https://betterhistory.io)
[^BrowserHistoryPlus]: [Browser History Plus](https://browserhistory.net)
[^HistoryPlus]: [HistoryPlus](https://chromewebstore.google.com/detail/history-plus/kloodnjmhgicecceindgbfpjencnhajh)
[^Flowchart]: [Flowchart](https://en.wikipedia.org/wiki/Flowchart)
[^Treestructure]: [Tree structure](https://en.wikipedia.org/wiki/Tree_structure)
[^Networkgraph]: [Network graph](https://www.highcharts.com/blog/tutorials/network-graph/)