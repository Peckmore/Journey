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

`JourneyWebView2` _should_ be a drop-in replacement for `WebView2`, but it was only developed as a proof-of-concept so _caveat emptor_!

## Abstract

**Journey** is an experiment into whether there could be a better - or at least more visually informative - way of presenting a users short-term browsing history. The end result is a branching, tree-based record of a users per-tab browsing history, which aims to seamlessly integrate into the browser user interface, be intuitive and easy-to-use, and visually delight the user.

## Definitions

- _**browser:**_ a software application with a graphical user interface for displaying, and navigating between, web pages

- _**browsing area:**_ the area of the _browser_ in which web content is displayed, typically the area below the address bar, and which does not include the _browser_ UI

- _**history:**_ the typical _history_ feature presented to the user that displays a list of all pages visited, across all tabs and sessions, usually with a date and time stamp, and often with the ability to search for specific pages

- _**sessions:**_ XXXXX

- _**travellog:**_ the short-term history associated with a page or tab within a _browser_ that allows for navigation backwords and forwards, sometimes referred to internally within a _browser_ as the _session history_

## Introduction

A typical use case for a _browser_ is to use a search engine to find the solution to a problem, which a user will often do by initiating a search in their _browser_ and then clicking on links that appear in the search results. The user may click on several links, returning to the search results between each link, and repeat this for a number of iterations. In doing this, the user can end up in a situation whereby they have visited a number of pages, but are unable to remember which page contained the most helpful solution to their problem.

The situation can be further compounded if several of the results lead to different answers on the same web site, resulting in many of the results having similar URLs and titles. The user may be forced to return to the search results and click on each link again, or use the _history_ feature of their _browser_ to attempt to find the page they were looking for. This can, however, be time consuming and frustrating, especially if the user visited a number of pages within a short period of time.

This scenario naturally posits the question, _"could the travellog be more helpful in such scenarios?"_ Whilst there are already a number of tools and extensions to enhance the _history_ of a _browser_ [^better-history] [^browser-history-plus] [^history-plus], at the time of this project there appears to be no tool or extension that would enhance the _travellog_.

As a result, this project proposes a way of enhancing the _travellog_ in order to allow users to see their _travellog_ in a more visual way, and show the organic path they took when visting pages in their _browser_. The enhanced _travellog_ will allow users to see all of the pages they have visited within their _session_, representing the journey they have taken from when the session started until the page they are presently on. Based on this, the proposed enhanced _travellog_ tool is henceforth referred to as **Journey**.

## Goals

The following goals were identified for the project:

- **Structured**

  The tool should represent the users browsing _travellog_ as a visual structure, with each page visited represented as a node within the structure.

- **Visual**

  The tool should be a "visual tool"; visual elements, such as thumbnail images of each page visited, or animations, should be used to aid the user in using the tool.

- **Interactive**

  The tool should be interactive, allowing the user to navigate between previously visited pages by interacting directly with the tool.

- **Intuitive**

  The tool should be easy to use, and not require any additional training or documentation to use.

- **Consistent**

  The tool should follow the same interactivity principles as the _browser_; that is, elements such as mouse cursors, shortcut keys, and means of interacting with elements should be consistent between viewing a web page and viewing the travellog.

- **Seamless**

  The tool should integrate seamlessly into the users _browser_ interface, and not require any additional steps to use, other than a way of invoking the tool.

- **Performant**

  The tool should be performant, and not slow down the users browsing experience.

## Data Structure

The data for the users **Journey** consists of a series of pages visited (_nodes_), each one linked to the page that came before it (_parent_) and to the pages that came after it (_children_). However, the data is not a simple list, as each page could have multiple _children_, given that the user can navigate backwards and forwards through their _travellog_ at any time, resulting in branches in their navigation history.

According to _Adrian Rusu_: _"The typical data structure for modeling hierarchical information is a tree whose vertices represent entities and whose edges correspond to relationships between entities."_ [^trees] Based on this, it becomes clear that our data lends itself naturally to a tree data structure, whereby our _nodes_ (pages visited) form a hierachy, with each _node_ representing a page the user has visited, and the relationships between _parent_ and _child_ nodes representing the chronology in which the pages were visited. 

It must be noted however, that this chronology is limited only to the order that links were followed from each page, and not to the times that they were followed; the depth of nodes within the tree does not indicate order.

For example, if the user navigates back from a page that has a depth of 10 (is 10-levels deep within the tree) to a page that has a depth of 2, and then navigates to another page, the new page will be have a depth of 3. However, this page was visited after the page with a depth of 10. This illustrates how depth cannot be used to determine the order in which pages were visited in relation to arbitrary nodes within the tree.

At a high-level, there are three types of defined tree data structures, with various further specialisations within each type: **binary tree**, **ternary tree**, and **n-ary tree**. These are illustrated below:

<div align="center">

![Tree Diagram](res/images/tree-types.png)  
_Types of trees_ [^tree-types]

</div>

As each node in our tree could have any number of child elements, an _N-ary Tree_ (also known as a _General Tree_) will be used to store the users **Journey**.

## Visualisation

When it comes to visualising tree structures, _Adrian Rusu_ goes on to state: _"Visualizations of hierarchical structures are only useful to the degree that the associated diagrams effectively convey information to the people that use them. A good diagram helps the reader understand the system, but a poor diagram can be confusing."_ [^trees]

As Adrian makes clear, much of the value of a tree structure is in how easily it's information can be conveyed to the viewer. If a tree is visualised poorly, it's information may be hard to discern or, worse, incorrectly interpreted; it is essential that the data is presented in a clear and logical fashion

There are many ways in which this data can be visualised, but many of them are not suited to the requirements of this project. Ultimately only three visualisation types will be considered; **network graph**, **flowchart**, and **tree diagram**.

### Network Graph

_"A network graph is a chart that displays relations between elements (nodes) using simple links. Network graph allows us to visualize clusters and relationships between the nodes quickly..."_ [^network-graph]

<div align="center">

![Network Graph](res/images/network-graph.png)  
_An example of a network graph_ [^network-graph]

</div>

Network graphs offered a level of complexity beyond the needs of the project, such as supporting multiple "links" or relationships between nodes. Their typical arrangement, as shown in the illustration above, also meant that they were not visually conducive to relaying the chronology of nodes, but had a stronger focus on the relationships between the nodes instead,

The focus of the visualisation was to illustrate the users browsing "flow", moving from one page to the next, with a heavy focus on the chronology of the pages. This meant that network graphs ultimately weren't chosen for the project.

### Flowchart

_"A flowchart is a type of diagram that represents a workflow or process. A flowchart can also be defined as a diagrammatic representation of an algorithm, a step-by-step approach to solving a task."_ [^flowchart]

<div align="center">

![Flowchart](res/images/flowchart.png)  
_An example of a flowchart_ [^flowchart]

</div>

Flowcharts were a strong match for the goals of the project. However, they present nodes in the "present tense", as a series of questions, showing choices a user _can_ make, rather than showing the chronological layout of choices already made. As the project only needed to effectively show the outcomes of choices, not what the choices were, flowcharts were also not chosen for the project.

### Tree Diagram

_"A tree structure, tree diagram, or tree model is a way of representing the hierarchical nature of a structure in a graphical form. It is named a "tree structure" because the classic representation resembles a tree, although the chart is generally upside down compared to a biological tree, with the "stem" at the top and the "leaves" at the bottom."_ [^tree-structure]

<div align="center">

![Tree Diagram](res/images/tree-diagram.png)  
_An example of a tree diagram_ [^tree-structure]

</div>

  Tree diagrams are excellent for representing chronological relationships between choices, with a root node representing the initial starting point, and each branch representing a different choice. They are also visually simple, which made them the strongest match for the project, and the visualisation approach ultimately chosen.



## Layout

A tree is a relatively simple structure, but there is complexity in how to lay out a tree diagram such that nodes are evenly spaced and do not overlap. This is a field that has been well studied, with numerous algorithms [^trees] having been created for drawing tree diagrams.

The most popular amongst

asd[^tidier-drawings]


## Design

After the research was completed, and the goals for the project identified, the next step was to design the tool. This involved working through a number of concepts and prototypes in order to iterate towards a final design that addressed all of the identified goals, and could then be used to create a working implementation. This implementation could then be used to gather feedback from users and evaluate both the design against the goals, and whether the tool was a useful addition to the users browsing experience.

### Structured

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

### Consistent

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


[^better-history]: [BetterHistory.io](https://betterhistory.io)
[^browser-history-plus]: [Browser History Plus](https://browserhistory.net)
[^history-plus]: [HistoryPlus](https://chromewebstore.google.com/detail/history-plus/kloodnjmhgicecceindgbfpjencnhajh)
[^flowchart]: [Flowchart](https://en.wikipedia.org/wiki/Flowchart)
[^tree-structure]: [Tree structure](https://en.wikipedia.org/wiki/Tree_structure)
[^network-graph]: [Network graph](https://www.highcharts.com/blog/tutorials/network-graph/)
[^tree-types]: [Types of trees in data structures](https://www.geeksforgeeks.org/types-of-trees-in-data-structures/)
[^trees]: [Tree Drawing Algorithms](https://cs.brown.edu/people/rtamassi/gdhandbook/chapters/trees.pdf), mirrored [here](res/trees.pdf)
[^tidier-drawings]: [Tider Drawings of Trees](https://reingold.co/tidier-drawings.pdf), mirrored [here](res/tidier-drawings.pdf)