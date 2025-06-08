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

`JourneyWebView2` _should_ be a drop-in replacement for `WebView2`, but it is only a proof-of-concept so _caveat emptor_!

## Abstract

**Journey** is an experiment into whether there could be a better - or at least more visually informative - way of presenting a users short-term browsing history. The end result is a branching, tree-based record of a users per-tab browsing history, which aims to seamlessly integrate into the browser user interface, be intuitive and easy-to-use, and visually delight the user.

## Definitions

- _**browser:**_ a software application with a graphical user interface for displaying, and navigating between, web pages

- _**browsing area:**_ the area of the _browser_ in which web content is displayed, typically the area below the address bar, and which does not include the _browser_ UI

- _**history:**_ the typical _history_ feature presented to the user that displays a list of all pages visited, across all tabs and sessions, usually with a date and time stamp, and often with the ability to search for specific pages

- _**session:**_ the period of activity that starts when you open a browser window or tab, and ends when you close it

- _**travellog:**_ the short-term history associated with a page or tab within a _browser_ that allows for navigation backwords and forwards, sometimes referred to internally within a _browser_ as the _session history_

## Introduction

A typical use case for a _browser_ is to use a search engine to find the solution to a problem, which a user will often do by initiating a search in their _browser_ and then clicking on links that appear in the search results. The user may click on several links, returning to the search results between each link, and repeat this for a number of iterations. In doing this, the user can end up in a situation whereby they have visited a number of pages, but are unable to remember which page contained the most helpful solution to their problem.

The situation can be further compounded if several results lead to different answers on the same web site, resulting in many of the page having similar URLs and titles. The user may be forced to return to the search results and click on each link again, or use the _history_ feature of their _browser_ to attempt to find the page they were looking for. This can be both time consuming and frustrating, especially if the user has visited a number of pages within a short period of time.

This scenario naturally posits the question, _"could the travellog be more helpful in such scenarios?"_ Whilst there are already a number of tools and extensions to enhance the _history_ of a _browser_ [^better-history] [^browser-history-plus] [^history-plus], at the time of this project there appears to be no tool or extension that enhances the _travellog_.

As a result, this project proposes a way of enhancing the _travellog_, allowing users to see it in a more visual way, which shows the organic path they took when visting pages. This enhanced _travellog_ allows users to see all of the pages they have visited within their _session_, representing the "journey" they have taken from when the session started until the page they are presently on. Based on this idea, the proposed enhanced _travellog_ tool has been named **Journey**.

## Goals

The following goals were identified for **Journey**:

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

The data for the users **Journey** consists of a series of pages visited (_nodes_), each one linked to the page that came before it (_parent_) and to the pages that came after it (_children_). Each page can have multiple _children_, given that the user can navigate backwards and forwards through their _travellog_ at any time, resulting in branches in their navigation history. Therefore, **Journey** moves beyond storing data as a simple list, as existing _travellogs_ do.

According to _Adrian Rusu_: [^tree-drawing-algorithms]

> "The typical data structure for modeling hierarchical information is a tree whose vertices represent entities and whose edges correspond to relationships between entities."

It becomes clear that our data lends itself naturally to a tree data structure, whereby our _nodes_ (pages visited) form a hierachy, with each _node_ representing a page the user has visited, and the relationships between _parent_ and _child_ nodes representing the chronology in which the pages have been visited. 

It must be noted however, that this chronology is limited only to the order that links were followed from each page, and not to the times that they were followed; the depth of nodes within the tree does not indicate order.

For example, if the user navigates back from a page that has a depth of 10 (is 10-levels deep within the tree) to a page that has a depth of 2, and then visits a new page, that new page will be have a depth of 3. However, this page was visited after the page with a depth of 10. This illustrates how depth cannot be used to determine the order in which pages were visited in relation to other nodes within the tree.

At a high-level, there are three types of defined tree structures, with various further specialisations within each type: **binary tree**, **ternary tree**, and **n-ary tree**. These are illustrated below:

<div align="center">

![Tree Diagram](res/images/tree-types.png)  
_Types of trees_ [^types-of-trees]

</div>

As each node in our tree structure could have any number of child elements, an _n-ary tree_ (also known as a _general tree_) will be used to store the users **Journey**.

## Visualisation

When it comes to visualising tree structures, _Adrian Rusu_ goes on to state: [^tree-drawing-algorithms]

> "Visualizations of hierarchical structures are only useful to the degree that the associated diagrams effectively convey information to the people that use them. A good diagram helps the reader understand the system, but a poor diagram can be confusing."

Much of the value of a tree structure is in how easily it's information can be conveyed to the viewer; if a tree is visualised poorly, it's information may be hard to discern or, worse, incorrectly interpreted. It is therefore essential that the tree structure is presented in a clear and logical fashion

There are many ways in which tree structures can be visualised but after briefly looking at many types of visualisation, we find that only three are suitable for further consideration; **network graph**, **flowchart**, and **tree diagram**.

### Network Graph

> "A network graph is a chart that displays relations between elements (nodes) using simple links. Network graph allows us to visualize clusters and relationships between the nodes quickly..." [^network-graph]

<div align="center">

![Network Graph](res/images/network-graph.png)  
_An example of a network graph_ [^network-graph]

</div>

When considered more closely, we can see that network graphs offer a level of complexity beyond the needs of this project; multiple links between nodes, multiple parent nodes, and two-way relationships are some of the features that can be achieved with a network graph that are beyond our requirements.

Further, the typical arrangement for a network graph, as shown in the diagram above, is not effective at visually relaying the chronology of nodes - it is hard to see in the diagram which node comes first, as there is no natural visual starting point, and the "random" nature of the layout does not asist the user visually in determing how links were followed.

Instead, the visualisation has a stronger focus on the relationships between nodes. Given that our requirements are to illustrate the users browsing "flow", moving from one page to the next, we can see that _network graphs_ are not the appropriate visualisation for this project.

### Flowchart

> "A flowchart is a type of diagram that represents a workflow or process. A flowchart can also be defined as a diagrammatic representation of an algorithm, a step-by-step approach to solving a task." [^flowchart]

<div align="center">

![Flowchart](res/images/flowchart.png)  
_An example of a flowchart_ [^flowchart]

</div>

Initially, flowcharts appear a strong match for the requirements of the project, effective at conveying both a starting point and a natural chronology. However, we find that flowcharts present nodes in the "present tense"; that is, as a series of _decisions_ (diamond shaped nodes), showing choices a user _can_ make, and their possible outcomes, _processes_ and _terminals_ (rectangular and rounded rectangular nodes respectively).

This means that flowcharts are more suited to showing a process that can be followed, with the user making choices at each step, rather than showing a record of choices already made. In the case of **Journey**, we are not looking to show possible decisions, as decisions are simply the available links on each page, and so we find that _flowcharts_ are also not the appropriate visualisation for this project.

### Tree Diagram

> "A tree structure, tree diagram, or tree model is a way of representing the hierarchical nature of a structure in a graphical form. It is named a "tree structure" because the classic representation resembles a tree, although the chart is generally upside down compared to a biological tree, with the "stem" at the top and the "leaves" at the bottom." [^tree-structure]

<div align="center">

![Tree Diagram](res/images/tree-diagram.png)  
_An example of a tree diagram_ [^tree-structure]

</div>

Tree diagrams are a common way of representing hierarchical data, and are often used for representing data with sub-data, such as categories and sub-categories, as shown in the diagram above. They are also excellent for representing chronological relationships between choices, with a root node representing the initial starting point, and each branch representing the outcome of different choices. In this way they are similar to flowcharts, but without the "present tense" nature of flowcharts, whereby each decision is also represented.

They also meet our requirement of effectively conveying information, with a natural flow from the root node downwards, and a clear visual representation of the hierarchy of nodes. We can see that this makes them ideal for representing the users **Journey**, and so are the most appropriate visualisation to use for this project.

## Tree Layout

When creating a tree diagram there are challenges around how to arrange nodes in a performant manner that is both visually pleasing, and without any node collisions or overlaps. Thankfully, this is a field that has been well studied with numerous algorithms [^tree-drawing-algorithms] having been created for drawing tree diagrams.

For this project we will be working with a _layered_ tree, whereby all nodes of the same depth are visually drawn on the same horizontal line. This is a common way of drawing trees, often referred to as _tidy trees_, and Wetherell and Shannon [^tidy-drawings] presented the first O(n) algorithm to draw them in 1979, along with formalising three aesthetic rules for tree layout. Reingold and Tilford [^tidier-drawings] then continued this work, and in 1981 improved the algorithm and added a fourth aesthetic rule.

Whilst much work has since been done in this area, such as by Walker [^walker], and Buchheim, Jünger, and Leipert [^buchheim], we find that the Reingold-Tilford algorithm continues to be one of the most popular and widely used algorithms for drawing tidy trees, and appropriate for the tree structures we will be drawing for this project.

## Beautiful and Usable

**Journey** was conceived as a UI/UX experiment, and from the very beginning both usability and aesthetics have been at the very core of the project. However, given that the goal of the project is to determine whether there is feasibility in enhancing the _travellog_ with a branching structure, should we focus or prioritise one of either usability or aesthetics over the other? If we focus on one area to the detriment of the other, will the perceived usefulness of the tool be diminished?

In the study by Tractinsky et al, they found that a more visually appealing interface was perceived as more usable, even when the interface was not actually more usable.

> "This study demonstrated once again the tight relationships between users' initial perceptions of interface aesthetics and their perceptions of the system's usablity. Moreover, we showed that these relations endure event after actual use of the system." [^beautiful]

The findings of this paper are summarised under the notion _"What is beautiful is usable"_, and have been repeated in many other studies, with Hassenzahl and Monk performing: [^hassenzahl]

> "...a review of 15 papers reporting 25 independent correlations of perceived beauty with perceived usability..."

However, there have also been studies into the counter-argument of _"What is usable is beautiful"_, with Tuch et al finding that:

> "...Tractinsky’s notion ("what is beautiful is usable") can be reversed to a "what is usable is beautiful" effect under certain circumstances." [^usable]

Hamborg et al explored this counter-argument further and ultimately concluded that both usability and aesthetics are important to the end user experience:

> "Concerning user experience design, results indicate that both usability and aesthetics contribute to a positive noninstrumental valuation of a system in terms of the ability of a system to communicate a desirable identity to others (HQI) and perceived appeal." [^hamborg]

From these studies we can conclude that attention must be given to both the usability of the tool, but also the visual presentation; a tool that is not visually appealing may be perceived as less usable, and a tool that is difficult to use will connote negative reactions from users.

## Consistency

Consistency was already highlighted as a goal for the project, but knowing how important consistency is to the user experience would influence the design of the tool.

"Consistency and standards" are the fourth of Jakob Nielsen's ten heuristics:

> "Users should not have to wonder whether different words, situations, or actions mean the same thing. Follow platform and industry conventions." [^consistency]

This is expanded upon by Rachel Krause (from Jakob Nielsen's Nielsen Norman Group):

> "To be easy to learn and use, systems should adhere to both internal and external consistency — they should use the same patterns everywhere inside the system and should also follow web-, platform-, and domain-specific conventions." [^consistency-and-standards]

We see that consistency is key to a positive user experience, and that it is important to follow both internal and external conventions. Within the scope of this project our main focus will be on internal consistency, as conceptually the completed tool would be used within a _browser_.

> "Internal consistency relates to consistency within a product or a family of products, either within a single application or across a family or suite of applications." [^consistency-and-standards]

Our expectation is that if we maintain internal consistency with the _browser_ we are within, external consistency _should_ be provided for us, as the _browser_ should already follow external conventions.

## Design

The initial project goals were refined through the research taken, and both were used to inform the design of **Journey**. The final design requirements were generated using the MoSCow method [^moscow], and are as follows:

### Structured

- A _general tree_ **must** be used to store the users travellog, given that a user may visit any number of pages after any other page during a browsing session.

- The root node of the tree structure **must** represent the initial page visited by the user for that browser tab or window.

- Each subsequent page visited **must** be represented as a child node of the previous page.

- Each page within the travellog **must** be represented equally within the tree; that is, each node should be of the same size and design.

### Visual

- Each page visited **must** be represented by a thumbnail image of the page _at the point at which it was navigated away from_; this is to vsually aid the user by showing each page as it was when the user last interacted with it.

- Each page visited **must** show the title of the page.

- Each page visited **must** show the URL of the page.

- Arrows **should** be shown to illustrate the direction of travel between pages, with the arrow pointing from the parent node to the child node.

### Interactive

- The user **must** be able to interact with the tree structure by clicking on any node within the tree.

- The user **must** be able to pan around the tree structure.

- The user **should** be able to scroll the tree structure.

- The user **should** be able to zoom in and out of the tree structure.

### Consistent

- Interacting with elements **must** be achieved by clicking the left mouse button.

- Scrolling the mouse wheel "down" (towards the user) **should** move visual elements "up" (towards the top of the screen), and the inverse should apply accordingly.

- When a modifier key is held, scrolling the mouse wheel "down" (towards the user) **should** zoom out, and the inverse should apply accordingly.

- The CTRL key **should** be used as the modifier key for zooming.

- When clicking an interactive element results in a navigation action, the mouse cursor **should** change to a pointer hand.

### Intuitive

- Pressing the ESC key **must** close the travellog and return the user to their current page.

- When the user views the travellog, the current page **should** be highlighted in the tree structure to illustrate their current browsing "position".

### Seamless

- The travellog **must** appear within the browsing area of the users current browser tab or window.

- The current page **must** transition into the travellog, to visually illustrate to the user the link between their current page and the travellog.

- When the user selects a page, the travellog **must** transition back into the selected page, to visually illustrate to the user the link between their action and the result.

### Performant

- Browsing performance **must** not be impacted by the tool being available within the browser.

- The tool **must** remain performant when being used, ideally with no lag or delay when interacting with the tree structure.

- The tool **must** use as little memory as possible, such as for storing travellog information.

- The tool **should** transition in and out of the browsing area quickly, and not cause any noticeable lag or delay.

- The tool **should** not consume any CPU cycles when not being used.

## Implementation

Due to familiarity with the development environment, and the ability to rapidly develop and iterate on the design, the tool was implemented as a **WPF** application using **C#**.

The tool uses a WebView2 control to display the travellog within the browsing area of the users current browser tab or window.

implementation of a tree structure

implementation of a tree layout algorithm
This implementation could then be used to gather feedback from users and evaluate both the design against the goals, and whether the tool was a useful addition to the users browsing experience.

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

[^tree-drawing-algorithms]: [Tree Drawing Algorithms](https://cs.brown.edu/people/rtamassi/gdhandbook/chapters/trees.pdf), mirrored [here](res/pdf/trees.pdf)

[^types-of-trees]: [Types of trees in data structures](https://www.geeksforgeeks.org/types-of-trees-in-data-structures/)

[^network-graph]: [Network graph](https://www.highcharts.com/blog/tutorials/network-graph/)

[^flowchart]: [Flowchart](https://en.wikipedia.org/wiki/Flowchart)

[^tree-structure]: [Tree structure](https://en.wikipedia.org/wiki/Tree_structure)

[^tidy-drawings]: [Tidy Drawings of Trees](https://www.researchgate.net/publication/3189258_Tidy_Drawings_of_Trees), mirrored [here](res/pdf/Tidy_Drawings_of_Trees.pdf)

[^tidier-drawings]: [Tidier Drawings of Trees](https://reingold.co/tidier-drawings.pdf), mirrored [here](res/pdf/tidier-drawings.pdf)

[^walker]: [A Node-Positioning Algorithm for General Trees](https://www.cs.unc.edu/techreports/89-034.pdf), mirrored [here](res/pdf/89-034.pdf)

[^buchheim]: [Improving Walker's Algorithm to Run in Linear Time](https://www.researchgate.net/publication/30508504_Improving_Walker's_Algorithm_to_Run_in_Linear_Time), mirrored [here](res/pdf/Improving_Walker's_Algorithm_to_Run_in_Linear_Time.pdf)

[^beautiful]: [What is beautiful is usable](https://www.ise.bgu.ac.il/faculty/noam/papers/00_nt_ask_di_iwc.pdf), mirrored [here](res/pdf/00_nt_ask_di_iwc.pdf)

[^hassenzahl]: [The Inference of Perceived Usability From Beauty](https://www.researchgate.net/publication/233864572_The_Inference_of_Perceived_Usability_From_Beauty), mirrored [here](red/pdf/HassenzahlMonk-2010-TheInferenceofPerceivedUsabilityFromBeauty.pdf)

[^usable]: [Is beautiful really usable? Toward understanding the relation between usability, aesthetics, and affect in HCI](https://cpb-us-e1.wpmucdn.com/wp.wwu.edu/dist/8/2868/files/2018/04/Tuch-et-al-2012-Is-Beautiful-Usable-2don1em.pdf), mirrored [here](res/pdf/Tuch-et-al-2012-Is-Beautiful-Usable-2don1em.pdf)

[^hamborg]: [The Interplay between Usability and Aesthetics: More Evidence for the "What Is Usable Is Beautiful" Notion](https://onlinelibrary.wiley.com/doi/full/10.1155/2014/946239?msockid=192a01f978e06fe63aea12f079576e44), mirrored [here](res/pdf/Hamborg.pdf)

[^consistency]: [10 Usability Heuristics for User Interface Design](https://www.nngroup.com/articles/ten-usability-heuristics/)

[^consistency-and-standards]: [Maintain Consistency and Adhere to Standards (Usability Heuristic #4)](https://www.nngroup.com/articles/consistency-and-standards/)

[^moscow]: [MoSCow Methods](https://en.wikipedia.org/wiki/MoSCoW_method)