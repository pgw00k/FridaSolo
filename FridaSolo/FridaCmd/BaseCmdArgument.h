#pragma once

#include <cctype>
#include <map>

namespace ConsoleHelper
{	
	class BaseCmdArgument
	{
	public:

		struct cmp_str
		{
			bool operator()(char* a, char* b) const
			{
				return std::strcmp(a, b) < 0;
			}
		};

		typedef int (*OptionAction)(BaseCmdArgument* self, char* arg, int argIndex);
		

		char** Arguments;
		int ArgumentsCount;
		std::map<char*, OptionAction, cmp_str> OptionActions;

		BaseCmdArgument(int argc, char* argv[])
		{
			ArgumentsCount = argc;
			Arguments = new char* [ArgumentsCount];

			for (int i = 0; i < ArgumentsCount; i++)
			{
				Arguments[i] = argv[i];
			}
		}
		~BaseCmdArgument()
		{

		}

		virtual void ProcessArgs()
		{
			for (int i = 0; i < ArgumentsCount; i++)
			{
				auto arg = Arguments[i];
				if (arg[0] == '-')
				{
					for (int j = 0; j < strlen(arg); j++)
					{
						arg[j] = tolower(arg[j]);
					}
					OptionAction act = OptionActions[arg];
					if (act)
					{
						i+=act(this, arg, i);
					}
					else
					{
						printf("Argument %s is not defined.\n", arg);
					}
				}
			}
		}

	private:

	};
}
