# Dalessandro-MGAIBUN-Modular-or-Genetic-Artificial-Intelligence-base-for-Unity
Made for unity 2021.2.8f1

No X-ray demo: (It's a video, click on the image.)

[![No X-ray demo](https://img.youtube.com/vi/xyOmNbeC46A/0.jpg)](https://www.youtube.com/watch?v=xyOmNbeC46A)

(Note: Near the end blue ball stops hunting for some secs since it's not hunger anymore)

Before we start: This project is under BSD-3-Clause!
In a Nutshell: 3-clause: "require your permission before mentioning your name for any work built on top of your software"

This project is a base for building a 3D AI game. I made it for teaching intermediate unity 3d for some friends, Feel free to use this project to learn as well.


So what it does?

The project is about making an AI (blue sphere) character navigate the environment looking for food and eating it. The project contains several red balls called devApples
that have a kcal (kilocalorie, 190 kcal by default) value. The AI starts with 99% hunger or 99% missing of its daily intake of 2500kcal. Each second the AI will gain 1%
hunger (or lose 25kcal). The AI recognise it's hunger as 5 States that generate a pleasentness value: Stuffed(+20),not hungry(0),Peckish(-10),Hungry(-50),Starving(-80).
if the pleasentness value hits -10 or less the AI will try to fix the problem (like looking for DevApples to eat), the system is designed so the AI will try to get rid of
what makes it most unpleasent first (task selection). In order to navigate the world the AI will use a Sensor: EyeVision. The Eye vision generates a FOV (field of view) 
that checks if the object is visible to the AI (no xray vision). There are 2 main layers that will filter out what is what: Entities (you need to create this filter layer) 
layer will mark what is a object visible to the AI, and the Solid (you need to create this filter layer) layer will mark obstacles (no-xray demo shows off this feature).
All features are fully customizable, the features were made in a way that it's easy to expand and the details can be edited in unity itself (or code, it's full of comments.).  

(REQUIREMENTS)

What non beginner knowlogy you require to learn from this project:            

C# interfaces, 

C# assembly references,

Unity Mesh Generation (if you want to make your own sensors),

be at least proficient in object oriented programming (you can just follow the already made code order/structure if you are still learning this)

Unity Pathfinding (only the how-to-bake part).

(FEATURES)

Project Main Features:                                                         
 
  1- Uses Unity Deafult Pathfinding.

  2- Modular needs system (make your own AI needs, like hunger(*implemented as example*) and thirst).

  3- Modular sensorial system (make your own AI sensors, like vision(*implemented as example*) and hearing).

  4- Can be stripped of subsystems or recive new ones in realtime (like gaing and lossing traits/genes).

  5- Uses Scriptable objects.

  6- Capable of detaching sensors(parent-child object behavior) from body (like multiple eyes(vision sensor) placement).

  6.5 - Comunication network that will not allow the AI pool to generate duplicates from multiple eyes/sensors.

  7- Well documented Wiki wrote by myself with the focus of teaching the system.

  8- Comes with 3 tech demos(simple, Xray test and Stress Test) and instructions(wiki) for easy setup so you can try out yourself.

  9- Comes with implemented AI Sight (FOV), AI Hunger Need, AI food (DevApple).

  10- All implementations are also packed into simple prefabs.

  11- Secure!, Almost no use of Public var! the only few used were used only to be serialized in unity editor (for easy learning).

  12- Broken down into DLL's in such a way that even if you customize the system it will try to avoid spaghetti code and self referencing.

  13- Simple system, code is full of comments, and it's Stress Tested! (Stress demo included!).

  14- Editor with Gizmos (Dev-View), you can see what the AI is doing/tracking with the Scene Editor in real time.
 
(STRESS TEST)

Tested on: I.5-3570k-OC-4.1Ghz, GTX960 Strix 6gb, 24gb ram ddr3.

Test parameters: 200 AI clones. (4 clusters of 50). and over 1800 misc objects (ground,walls,AI Food.) + native engine ilumination. 

Result: 60FPS or more on avarage. (while recording).

[![Stress Test](https://img.youtube.com/vi/sPSbKpWyK2s/0.jpg)](https://www.youtube.com/watch?v=sPSbKpWyK2s)
