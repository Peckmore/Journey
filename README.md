<div align="center">

# Journey

A UI/UX experiment in visual, branching browser history.

_“Its the not the destination, It's the journey.”_<br/>
**Ralph Waldo Emerson**

SCREENSHOT

</div>

## Abstract

**Journey** is an experiment into whether there could be a better - or at least more visually informative - way of presenting a users short-term browsing history. The end result is a branching, tree-based record of a users per-tab browsing history, which aims to seamlessly integrate into the browser user interface, be intuitive and easy-to-use, and visually delight the user.

## Definitions

- _**browser:**_ a software application with a graphical user interface for displaying, and navigating between, web pages

- _**browsing area:**_ the area of the browser in which web content is displayed, typically the area below the address bar, and which does not include the browser UI

- _**history:**_ the typical _history_ feature presented to the user that displays a list of all pages visited, across all tabs and sessions, usually with a date and time stamp, and often with the ability to search for specific pages

- _**travellog:**_ the short-term history associated with a page or tab within a browser that allows for navigation backwords and forwards, sometimes referred to internally within browsers as the _travellog_

## Introduction

A typical use case for a browser is to use a search engine to find the solution to a problem. This is often done by initiating a search for the problem, and then clicking on a number of links that appear in the search results. A user may click on several links, returning to the search results between each link, and repeat this for a number of iterations. Doing so can lead to a situation whereby the user has visited a number of pages, but is unable to remember which page contained the most helpful solution to their problem.

The situation can be further compounded if several of the results lead to different answers on the same web site, resulting in many of them having similar URLs and titles. In this case, the user may be forced to return to the search results and click on each link again, or use the _history_ feature of the browser to attempt to find the page they were looking for. However, this can be time consuming and frustrating, especially if the user has visited a number of pages in a short period of time.

The problem posited the question of whether it could be possible for the travellog to be more helpful in such scenarios, and what solutions already existed to achieve this. A search for tools to enhance the travellog resulted in a number of browser extensions
^[1](https://betterhistory.io/)^ ^[2](https://browserhistory.net/)^ ^[3](https://chromewebstore.google.com/detail/history-plus/kloodnjmhgicecceindgbfpjencnhajh)^ that could enhance the _history_ of a browser, but nothing that would improve the travellog.

With no existing solution found, an experiment was undertaken to investigate a way to enhance the _travellog_, and allow for a better solution to the problem. The solution would need to allow the user to see their travellog in a more visual way, provide a beter representation of a users browsing session, allow for easy navigation between previously visited pages and, most crucially, show the organic path the user took when visting the pages in their browser.

## Research

There are many ways of visualizing branching choices, but the following types of visualization were considered for this experiment:

- **Flowchart**

  _"A flowchart is a type of diagram that represents a workflow or process. A flowchart can also be defined as a diagrammatic representation of an algorithm, a step-by-step approach to solving a task._

  _"The flowchart shows the steps as boxes of various kinds, and their order by connecting the boxes with arrows. This diagrammatic representation illustrates a solution model to a given problem. Flowcharts are used in analyzing, designing, documenting or managing a process or program in various fields."_ ^[4](https://en.wikipedia.org/wiki/Flowchart)^

  ![Flowchart](res/images/LampFlowchart.png)<br/>
  ^An example of a flowchart^ ^[4](https://en.wikipedia.org/wiki/Flowchart)^

  Flowcharts are excellent for illustrating the flow of decisions and their outcomes, with each choice leading to a new path.

- **Tree Diagrams**

  _"A tree structure, tree diagram, or tree model is a way of representing the hierarchical nature of a structure in a graphical form. It is named a "tree structure" because the classic representation resembles a tree, although the chart is generally upside down compared to a biological tree, with the "stem" at the top and the "leaves" at the bottom."_ ^[5](https://en.wikipedia.org/wiki/Tree_structure)^

  ![Tree Diagram](res/images/Binary_tree_structure.png)<br/>
  ^An example of a tree diagram^ ^[5](https://en.wikipedia.org/wiki/Tree_structure)^

  Tree diagrams are excellent for representing hierarchical relationships between choices. The root node represents the initial starting point, with each branch representing a different choice and it's subsequent choices and outcomes.

- **Network Diagrams**

  Network diagrams are suitable for visualizing complex relationships and connections between various elements, including characters, locations, and events. They can represent how choices influence multiple aspects of the narrative. 

- **Sankey Diagrams**

Sankey diagrams are particularly helpful for showing the flow of information or resources. They can be used to illustrate how choices lead to different outcomes, with the width of the "ribbons" representing the "flow" or probability of each path. 




## Goals

Based on the research undertaken, the following goals were identified for the project:

- **Tree Structure**

  The tool should represent the users browsing travellog as a tree structure, with each page visited represented as a node in the tree.

- **Visual**

  The tool should be a "visual tool"; visual elements, such as thumbnail images of each page visited, or animations, should be used to aid the user in using the tool.

- **Interactive**

  The tool should be interactive, allowing the user to navigate between previously visited pages by interacting directly with the tool.

- **Intuitive**

  The tool should be easy to use, and not require any additional training or documentation to use.

- **Consistency**

  The tool should follow the same interactivity principles as the browser; that is, elements such as mouse cursors, shortcut keys, and means of interacting with elements should be consistent between viewing a web page and viewing the travellog.

- **Seamless**

  The tool should integrate seamlessly into the users browser interface, and not require any additional steps to use, other than a way of invoking the tool.

- **Performant**

  The tool should be performant, and not slow down the users browsing experience.

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

## References
[1] BetterHistory.io, https://betterhistory.io/

[2] Browser History Plus, https://browserhistory.net/

[3] HistoryPlus, https://chromewebstore.google.com/detail/history-plus/kloodnjmhgicecceindgbfpjencnhajh

[4] Flowchart, https://en.wikipedia.org/wiki/Flowchart

[5] Tree structure, https://en.wikipedia.org/wiki/Tree_structure

[6] Graphic drawing, https://en.wikipedia.org/wiki/Graph_drawing