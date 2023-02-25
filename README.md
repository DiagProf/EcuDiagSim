# `EcuDiagSim`

The purpose of EcuDiagSim is to simulate the Electronic Control Unit (ECU) diagnostic communication. More precisely, it simulates the response that an ECU returns to a request coming from a diagnostic tester. The definition of the simulated diagnosis communication is done in Lua. The transmission of the diagnosis communication down to the physics is ensured via an ISO22900-2 compatible vehicle communication interface (VCI). 

The last sentence implies that at least one native **D-PDU-API must be installed** on the computer and an associated **VCI must be available** before EcuDiagSim can work. (*So if you don't have this 2 things don't waste your time here*)  [more about VCI]([DiagProf/ISO22900.II: ISO22900.II-Sharp handles all the details of operating with unmanaged ISO 22900-2 spec library (also called D-PDU-API) and lets you deal with the important stuff. (github.com)](https://github.com/DiagProf/ISO22900.II#introduction))

## Table of Contents

1. [Introduction](#introduction)
2. [State of the work](#state-of-the-work)
3. [Usage](#usage)

## Introduction

The idea for this project came to me one day when I had to fill a diagnostic application with content, but I couldn't do any tests on real ECUs because they weren't available. The only thing I had was a lot of incomplete documentation of the ECUs and CAN traces made with the previous diagnostic tool. Now there are some great ECU diagnostic simulations on the market but they are either very limited e.g. only EOBD or you have to spend hours filling databases before you can start. I wanted something quick and easy to create from the raw bytes of the CAN trace, but flexible enough to bring dynamic into the simulation where you need it. Another point that was also important to me. If the flashing of an ECU is implemented for the first time based on a specification then you can be almost sure that the first test on the real ECU goes wrong. Anyone who has ever done this knows what I'm talking about :-). So it is a great thing if you can run the first tests against an ECU simulation to fix the obvious errors.

One thing was clear from the beginning, it should be a simulation that works under any diagnostic tool. If this is the requirement, the simulation must be carried through to the physical layers. So that nothing has to be changed in the diagnostic tool for the simulation. The diagnostic tool does not notice that it is talking to a simulated ECU (of course "simulated" only from a diagnostic point of view). When I programmed the ISO22900-2 C# wrapper, I read in the ISO22900-2 pdf that the API can also be used to simulate ECUs. With that in mind the idea for suitable hardware was already found. Now I needed a format in which I could keep the simulation data. After some searching for inspiration I came across this [project]([AVL-DiTEST-DiagDev/car-simulator: Raspberry Pi as a simulated car (github.com)](https://github.com/AVL-DiTEST-DiagDev/car-simulator)) .The PI they used seemed a little too unsuitable for my purposes, but using Lua to store the simulation data is a brilliant idea from my point of view and meets my requirements exactly. With [NeoLua](https://github.com/neolithos/neolua) I found a great implementation of Lua that I can use Lua in the C# world.



## State of the work

I quickly but everything together to see if it would work. That's why the code still needs some rework. But it works and it even works really cool. That's why I've published it here so that anyone who wants can try it out. The Lua file format that the user needs will not change that much in the future. The solution stored in the repository above contains 2 projects EcuDiagSim is the library in which basically everything happens. The goal is to make EcuDiagSim a nuget package at the end (if it is reliable enough) so that you can integrate it into your own application and use it e.g. for interation testing.

At the present time, EcuDiagSim is ready to simulate ECUs for [ISO-TP](https://en.wikipedia.org/wiki/ISO_15765-2) with all its characteristics (11bit-CANId, 29bit-CANId, Extended Addressing, etc.).  



## Usage

At the Momant... Clone the repository and set the EcuDiagSim.App project as the start project in Visual Studio 2020. Build and launch the project. Don't forget.... build the project for x86 if you have a 32bit version of the D-PDU-API installed or for x64 if you have a 64bit version of the D-PDU-API installed.

*If someone tells me how to build an application that runs without a certificate, I will publish the executable here. So don't hesitate to tell me how to configure this project to get an executable EXE (without the certificate hocus-pocus) out of it.*

After starting the application you should see this. Go to settings and select a VCI (I assume that you have a D-PDU-API capable VCI and everything is already installed.) Depending on how the VCI is connected to the computer, it could happen that the firewall opens and you have to allow the connection, especially for VCIs with an Ethernet connection. Usually the application has to be restarted afterwards. 