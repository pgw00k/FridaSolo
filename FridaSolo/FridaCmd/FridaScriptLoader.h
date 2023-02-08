#pragma once
#include <string.h>
#include <string>
#include <fstream>

class ScriptLoader
{
public:
	std::string Buffer;
	static std::string REPL;

	static void LoadREPL(const char* ScriptPath);
	ScriptLoader(char* ScriptPath, bool IsRPC = true);
	~ScriptLoader();
	const char* GetData();

private:

};