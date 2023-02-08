// FridaCmd.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// For use std::format£¬use C++20

#include <iostream>
#include <stdlib.h>
#include <string.h>
#include "FridaCmdArgument.h"

GMainLoop* FridaCmdArgument::loop = NULL;

int main(int argc,char* argv[])
{
    frida_init();

#ifdef _DEBUG

    ScriptLoader::LoadREPL("..\\FridaScript\\FridaREPL.js");

#else

    ScriptLoader::LoadREPL("FridaREPL.js");

#endif // DEBUG

    FridaCmdArgument* arg = new  FridaCmdArgument(argc, argv);
    arg->ProcessArgs();
    arg->FridaStart();

    std::string cmd;
    std::getline(std::cin, cmd);
    do
    {    
        arg->RunScript(cmd);
        std::getline(std::cin, cmd);
    } while (cmd[0]!='-' || std::strcmp(cmd.c_str(), "-exit") != 0);

    arg->FridaFinish();

    return 1;
}