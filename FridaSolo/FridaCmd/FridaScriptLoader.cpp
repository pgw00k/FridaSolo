#include "FridaScriptLoader.h"

std::string ScriptLoader::REPL;

ScriptLoader::ScriptLoader(char* ScriptPath, bool IsRPC)
{
	std::ifstream in(ScriptPath);
	Buffer = std::string((std::istreambuf_iterator<char>(in)), std::istreambuf_iterator<char>());
	if (IsRPC)
	{
		Buffer = ScriptLoader::REPL.append(Buffer);
	}
}
ScriptLoader::~ScriptLoader()
{
}

const char* ScriptLoader::GetData()
{
	return Buffer.c_str();
}

void ScriptLoader::LoadREPL(const char* ScriptPath)
{
	std::ifstream in(ScriptPath);
	REPL = std::string((std::istreambuf_iterator<char>(in)), std::istreambuf_iterator<char>());
}