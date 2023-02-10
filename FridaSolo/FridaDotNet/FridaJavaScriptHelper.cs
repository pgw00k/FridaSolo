using System;
using System.Collections.Generic;
using System.Text;

namespace FridaDotNet
{
    public class FridaJavaScriptHelper
    {
        public static string RPC_Init_Definition = @"
function _init() {
    console.log('Frida REPL init');
    global.cm = null;
    global.cs = {};

    const rpcExports = {
        fridaEvaluate(expression) {
            try {
                //console.log('fridaEvaluate ${expression}');
                const result = (1, eval)(expression);
                if (result instanceof ArrayBuffer) {
                    return result;
                } else {
                    const type = (result === null) ? 'null' : typeof result;
                    return [type, result];
                }
            } catch (e) {
                return ['error', {
                    name: e.name,
                    message: e.message,
                    stack: e.stack
                }];
            }
        },
        fridaLoadCmodule(code, toolchain) {
            const cs = global.cs;

            if (cs._frida_log === undefined)
                cs._frida_log = new NativeCallback(onLog, 'void', ['pointer']);

            if (code === null) {
                recv('frida:cmodule-payload', (message, data) => {
                    code = data;
                });
            }

            global.cm = new CModule(code, cs, { toolchain });
        },
    };

    Object.defineProperty(rpc, 'exports', {
        get() {
            return rpcExports;
        },
        set(value) {
            for (const [k, v] of Object.entries(value)) {
                rpcExports[k] = v;
            }
        }
    });

    function onLog(messagePtr) {
        const message = messagePtr.readUtf8String();
        console.log(message);
    }
}

_init();
rpc.exports = this;
";
    }
}
