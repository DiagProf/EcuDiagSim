# `EcuDiagSim`

The purpose of EcuDiagSim is to simulate the Electronic Control Unit (ECU) diagnostic communication. More precisely, it simulates the response that an ECU returns to a request coming from a diagnostic tester. The definition of the simulated diagnosis communication is done in Lua. The transmission of the diagnosis communication down to the physics is ensured via an ISO22900-2 compatible vehicle communication interface (VCI). 

The last sentence implies that at least one native **D-PDU-API must be installed** on the computer and an associated **VCI must be available** before EcuDiagSim can work. (*So if you don't have this 2 things don't waste your time here*)  [more about VCI](https://github.com/DiagProf/ISO22900.II#introduction)

## Table of Contents

1. [Introduction](#introduction)
2. [State of the work](#state-of-the-work)
3. [Usage](#usage)
4. [Lua file structure](#lua-file-structure)

## Introduction

The idea for this project came to me one day when I had to fill a diagnostic application with content, but I couldn't do any tests on real ECUs because they weren't available. The only thing I had was a lot of incomplete documentation of the ECUs and CAN traces made with the previous diagnostic tool. Now there are some great ECU diagnostic simulations on the market but they are either very limited e.g. only EOBD or you have to spend hours filling databases before you can start. I wanted something quick and easy to create from the raw bytes of the CAN trace, but flexible enough to bring dynamic into the simulation where you need it. Another point that was also important to me. If the flashing of an ECU is implemented for the first time based on a specification then you can be almost sure that the first test on the real ECU goes wrong. Anyone who has ever done this knows what I'm talking about :-). So it is a great thing if you can run the first tests against an ECU simulation to fix the obvious errors.

One thing was clear from the beginning, it should be a simulation that works under any diagnostic tool. If this is the requirement, the simulation must be carried through to the physical layers. So that nothing has to be changed in the diagnostic tool for the simulation. The diagnostic tool does not notice that it is talking to a simulated ECU (of course "simulated" only from a diagnostic point of view). When I programmed the ISO22900-2 C# wrapper, I read in the ISO22900-2 pdf that the API can also be used to simulate ECUs. With that in mind the idea for suitable hardware was already found. Now I needed a format in which I could keep the simulation data. After some searching for inspiration I came across this [project](https://github.com/AVL-DiTEST-DiagDev/car-simulator) .The PI they used seemed a little too unsuitable for my purposes, but using Lua to store the simulation data is a brilliant idea from my point of view and meets my requirements exactly. With [NeoLua](https://github.com/neolithos/neolua) I found a great implementation of Lua that I can use Lua in the C# world.

## State of the work

I quickly but everything together to see if it would work. That's why the code still needs some rework. But it works and it even works really cool. That's why I've published it here so that anyone who wants can try it out. The Lua file format that the user needs will not change that much in the future. The solution stored in the repository above contains 2 projects EcuDiagSim is the library in which basically everything happens. The goal is to make EcuDiagSim a nuget package at the end (if it is reliable enough) so that you can integrate it into your own application and use it e.g. for interation testing.

At the present time, EcuDiagSim is ready to simulate ECUs for [ISO-TP](https://en.wikipedia.org/wiki/ISO_15765-2) with all its characteristics (11bit-CANId, 29bit-CANId, Extended Addressing, etc.).  

## Usage

At the Momant... Clone the repository and set the EcuDiagSim.App project as the start project in Visual Studio 2020. Build and launch the project. Don't forget.... build the project for x86 if you have a 32bit version of the D-PDU-API installed or for x64 if you have a 64bit version of the D-PDU-API installed.

*If someone tells me how to build an application that runs without a certificate, I will publish the executable here. So don't hesitate to tell me how to configure this project to get an executable EXE (without the certificate hocus-pocus) out of it.*

After starting the application you should see this. Go to settings and select a VCI (I assume that you have a D-PDU-API capable VCI and everything is already installed.) 

![](https://github.com/DiagProf/EcuDiagSim/blob/master/images/GoToSettings.png)

![](https://github.com/DiagProf/EcuDiagSim/blob/master/images/KlickNotSelected_ToSelectVCI.png)

Depending on how the VCI is connected to the computer, it could happen that the firewall opens and you have to allow the connection, especially for VCIs with an Ethernet connection. Usually the application has to be restarted afterwards.

Once you have selected a VCI you can go back to the main page. There you can choose between selecting only one Lua file or a folder containing Lua files (possible subfolders are also included). To reproduce the whole example, you also need a diagnostic tester. To make this possible for most people, I have chosen a Lua simulation for EOBD, so that a free EOBD app and a cheap ELM327 can be used as a diagnostic tester to test the example. In the repository you will also find the folder LuaSimFileStore where the Lua EOBD simulation files are located. By the way, the folder is also intended to be used by anyone who wants to share their simulations. I would be pleased about active interest :-). So that it doesn't get completely wild, I have already created folders that show how I would like to have it roughly grouped.

![](https://github.com/DiagProf/EcuDiagSim/blob/master/images/SelectLuaFromEobdLuaExample.png)

After the start button is pressed, the Lua files are loaded and the simulation is started.

![](https://github.com/DiagProf/EcuDiagSim/blob/master/images/AfterStartButtonIsPressed.png)

Now you can start the a diagnostic tool. During my quick search and install I pick one that displays everything graphically but it doesn't matter, it's enough to show where the simulated ECU responses arrive. The EcuDiagSim.App also shows parts of the logging infomration (latest message at the top) so that you can see what is happening.

![](https://github.com/DiagProf/EcuDiagSim/blob/master/images/SimulationInAction.png)

Finally, a picture of the hardware setup. So that everyone can imagine what a required hardware structure looks like.

![](https://github.com/DiagProf/EcuDiagSim/blob/master/images/HardwareSetup.png)

## Lua file structure

Lua files don't have a "main" or anything like that. Therefore, the application must define its own entry point. 

In EcuDiagSim the content of a Lua file is assigned to a SimUnit. A SimUnit can have one or more CoreTables. The CoreTable represents, among other things, a part of the Lua file. The CoreTable is also the entry point into Lua. The entry point is defined like this. At the **highest level** in Lua is a **LuaTable** that has elements as its content. One of these elements is again a **LuaTable** with the name "**Raw**". From this it is concluded that the table containing the Raw table is the CoreTable. The following image illustrates this.

![](https://github.com/DiagProf/EcuDiagSim/blob/master/images/ExplanationOfCoreTable.png)

The name of the CoreTable in the image "YourNameForTheECU" can be freely chosen but must be unique if there are 2 CoreTables in one file. 

I would like to say at this point that 2 CoreTables in one file means 2 ECU simulations in one file. This is possible but not the preferred way. Why it is supported at all is the following reason. Imagine you simulate a body control unit and an engine control unit. Now what happens in the real vehicle is... If a VIN is written to the body control unit via diagnostics, it is distributed internally in the vehicle via CAN. In the engine control unit, this VIN can be read again via a diagnostic service and the VIN has changed. If you want to simulate something like this, both simulations must be in a Lua environment. However, this is rarely needed, so it is better to have one file per ECU simulation. 



Now a few words about the communication settings. The idea here is that you can take them from the tester data. And they are based on the ISO22900-2. Right away you don't need to worry about it if you don't understand it straight away, 95% of them can be copied over and over again and you only need to change the places with the CAN IDs. Communication settings consist of 2 LuaTables. The LuaTable named "DataForComLogicalLinkCreation" is exactly what the name says, the data with which the ComLogicalLink is created which simulates the ECU. The key and values also come from ISO22900-2. The LuaTable named "ComParamsFromTesterPointOfView" contains the parameters with which the transport protocol is setup. As the name suggests, the values are specified from the viewpoint of the diagnostic tester. The user does not notice that an internal magic shakes the values a bit. Saving the internal magic and setting the parameters right in Lua would overwhelm most users, so it's better to take a tester's perspective. The key and values also come from ISO22900-2 again. 

![](https://github.com/DiagProf/EcuDiagSim/blob/master/images/CommunicationSettings.png)