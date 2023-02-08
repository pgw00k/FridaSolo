#pragma once
#include "BaseCmdArgument.h"
#include "frida-core.h"
#include "FridaScriptLoader.h"

#include <format>

using namespace ConsoleHelper;

class FridaCmdArgument:public BaseCmdArgument
{
public:

	unsigned int target_pid;
	FridaDeviceManager* manager;
	GError* error = NULL;
	FridaDeviceList* devices;
	gint num_devices, i;
	FridaDevice* local_device;
	FridaSession* session;
	FridaScript* script;
	char* TargetName;
	char* ScriptPath;
	bool IsPause;
	bool IsNeedSpawn;
	
	static GMainLoop* loop ;

	static int ListDevices(BaseCmdArgument* self, char* arg, int argIndex)
	{
		FridaCmdArgument* fridacmdarg = (FridaCmdArgument*)self;
		fridacmdarg->GetDevices();
		int num_devices = frida_device_list_size(fridacmdarg->devices);
		for (int i = 0; i != num_devices; i++)
		{
			FridaDevice* device = frida_device_list_get(fridacmdarg->devices, i);
			g_print("[*] Found device: \"%s\"\n", frida_device_get_name(device));
			g_object_unref(device);
		}
		return 0;
	}

	static int GetUsbDevice(BaseCmdArgument* self, char* arg, int argIndex)
	{
		FridaCmdArgument* fridacmdarg = (FridaCmdArgument*)self;
		fridacmdarg->GetDeviceByType(FridaDeviceType::FRIDA_DEVICE_TYPE_USB);
		g_assert(fridacmdarg->local_device != NULL);
		return 0;
	}

	static int SetTargetName(BaseCmdArgument* self, char* arg, int argIndex)
	{
		FridaCmdArgument* fridacmdarg = (FridaCmdArgument*)self;
		fridacmdarg->TargetName = fridacmdarg->Arguments[argIndex + 1];
		return 1;
	}

	static int SetScriptPath(BaseCmdArgument* self, char* arg, int argIndex)
	{
		FridaCmdArgument* fridacmdarg = (FridaCmdArgument*)self;
		fridacmdarg->ScriptPath = fridacmdarg->Arguments[argIndex + 1];
		return 1;
	}

	static int SetNoPause(BaseCmdArgument* self, char* arg, int argIndex)
	{
		FridaCmdArgument* fridacmdarg = (FridaCmdArgument*)self;
		fridacmdarg->IsPause = false;
		return 0;
	}

	static void on_message(FridaScript* script,const gchar* message,GBytes* data,gpointer user_data)
	{
		JsonParser* parser;
		JsonObject* root;
		const gchar* type;

		parser = json_parser_new();
		json_parser_load_from_data(parser, message, -1, NULL);
		root = json_node_get_object(json_parser_get_root(parser));

		type = json_object_get_string_member(root, "type");
		if (strcmp(type, "log") == 0)
		{
			const gchar* log_message;

			log_message = json_object_get_string_member(root, "payload");
			g_print("%s\n", log_message);
		}
		else
		{
			g_print("on_message: %s\n", message);
		}

		g_object_unref(parser);
	}

	FridaCmdArgument(int argc, char* argv[]):BaseCmdArgument(argc,argv)
	{
		IsPause = true;
		IsNeedSpawn = true;

		OptionActions.try_emplace((char*)"-devices", &FridaCmdArgument::ListDevices);
		OptionActions.try_emplace((char*)"-u", &FridaCmdArgument::GetUsbDevice);
		OptionActions.try_emplace((char*)"-f", &FridaCmdArgument::SetTargetName);
		OptionActions.try_emplace((char*)"-l", &FridaCmdArgument::SetScriptPath);
		OptionActions.try_emplace((char*)"--no-pause", &FridaCmdArgument::SetNoPause);

#ifdef DEBUG

		for (std::map<char*, OptionAction>::iterator it = OptionActions.begin(); it != OptionActions.end(); it++)
		{
			printf("Argument %s is defined.\n", it->first);
		}

#endif // DEBUG

		manager = frida_device_manager_new();
	}
	~FridaCmdArgument()
	{

	}

	void GetDevices()
	{
		devices = frida_device_manager_enumerate_devices_sync(manager, NULL, &error);
		num_devices = frida_device_list_size(devices);
		g_assert(error == NULL);
	}

	void GetDeviceByType(FridaDeviceType Type)
	{
		if(!devices)
		{
			GetDevices();
		}
		for (i = 0; i != num_devices; i++)
		{
			FridaDevice* device = frida_device_list_get(devices, i);
			if (frida_device_get_dtype(device) == Type)
				local_device = g_object_ref(device);
			g_object_unref(device);
		}
	}

	void FridaStart()
	{
		if (!local_device)
		{
			GetDeviceByType(FridaDeviceType::FRIDA_DEVICE_TYPE_LOCAL);
			g_assert(local_device != NULL);
		}

		frida_unref(devices);
		devices = NULL;

		//loop = g_main_loop_new(NULL, TRUE);

		if (IsNeedSpawn)
		{
			FridaSpawnOptions* sopts = frida_spawn_options_new();
			target_pid = frida_device_spawn_sync(local_device, TargetName, sopts, NULL, &error);
			g_assert(error == NULL);
		}

		session = frida_device_attach_sync(local_device, target_pid, NULL, NULL, &error);
		if (error == NULL)
		{
			LoadScript();
		}
		else
		{
			g_printerr("Failed to attach: %s\n", error->message);
			g_error_free(error);
		}
	}

	void LoadScript()
	{
		FridaScriptOptions* options;

		//g_signal_connect(session, "detached", G_CALLBACK(on_detached), NULL);
		//if (frida_session_is_detached(session))
		//	goto session_detached_prematurely;

		options = frida_script_options_new();
		frida_script_options_set_name(options, "MainScript");
		frida_script_options_set_runtime(options, FRIDA_SCRIPT_RUNTIME_QJS);

		/*
		* 这种读取方式会造成脚本创建错误，原因不明
		*/
		/*
		struct stat stat_buf;
		stat(ScriptPath, &stat_buf);
		int len = stat_buf.st_size;
		FILE* f;
		fopen_s(&f, ScriptPath, "r");
		char* buffer = new char[len+];
		fread_s(&buffer, len, 1, len, f);
		fclose(f);
		*/

		ScriptLoader loader(ScriptPath);
		const char* source = loader.GetData();
		script = frida_session_create_script_sync(session, source, options, NULL, &error);
		g_assert(error == NULL);

		g_clear_object(&options);

		g_signal_connect(script, "message", G_CALLBACK(FridaCmdArgument::on_message), NULL);

		frida_script_load_sync(script, NULL, &error);
		g_assert(error == NULL);

		g_print("[*] Script loaded\n");

		if (IsNeedSpawn && !IsPause)
		{
			frida_device_resume_sync(local_device, target_pid, NULL, &error);
			g_assert(error == NULL);
		}

		//if (g_main_loop_is_running(loop))
		//	g_main_loop_run(loop);
	}

	void FridaFinish()
	{
		/*------
		*/
		frida_unref(local_device);

		frida_device_manager_close_sync(manager, NULL, NULL);
		frida_unref(manager);
		g_print("[*] Closed\n");

		//g_main_loop_unref(loop);

		/*------
		*/

		g_print("[*] Stopped\n");

		frida_script_unload_sync(script, NULL, NULL);
		frida_unref(script);
		g_print("[*] Unloaded\n");

		frida_session_detach_sync(session, NULL, NULL);
	session_detached_prematurely:
		frida_unref(session);
		g_print("[*] Detached\n");
	}

	void RunScript(std::string js)
	{
		std::string raw = std::format(R"(["frida:rpc",1,"call","fridaEvaluate",["{}"]])",js);
		const char* rawm = raw.c_str();
		printf("%s\n", rawm);
		frida_script_post(script, rawm, NULL);
	}

private:

};