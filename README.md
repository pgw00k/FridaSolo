# FridaCSolo
这个项目的主要目的就是摆脱 Frida 使用时关联的Python环境。

尽量做到在Windows下一个 命令行EXE + Frida.dll 就能直接开始调试和Hook。

整个项目所有内容都是基于 **x64** 系统来构建的，需要注意。

# Environment
- 2023_02_02
- frida-clr(commit 7bcf6fcdf26afe9dae338deea3b7204880b1ae28)
- [Frida 16.0.8](https://github.com/frida/frida/releases/tag/16.0.8) ([frida-clr-16.0.8-windows-x86_64.dll](https://github.com/frida/frida/releases/download/16.0.8/frida-clr-16.0.8-windows-x86_64.dll.xz) [frida-core-devkit-16.0.8-windows-x86_64.exe](https://github.com/frida/frida/releases/download/16.0.8/frida-core-devkit-16.0.8-windows-x86_64.exe))
- VS2022(17.4.2)
- Win10(21H2)
- NetFramework 4.8

# BuildSetup
要开始编译前，需要先使用 `FridaSolo\GetFridaCLR.ps1` 和 `FridaSolo\GetFridaDevKit.ps1` 脚本来获取相应的库（当然，你自己下载也是可以的，CLR库记得改名成 **Frida.dll** 就可以了，一定要改，不然这个库加载不了）。

# Notices
- VS编译时，因为直接引用了 Frida.dll，所以在 `Properties->Build->Platform target` 要设置为 **x64**。
- 另外，要把 frida-clr-16.0.8-windows-x86_64.dll 改名为 **Frida.dll**，一定要改，不然引用会失效。
- 由于 Frida.DeviceManager 这个类要依赖 System.Windows.Threading.Dispatcher 来创建，所以需要引入 WindowsBase。
- 直接使用 Frida-CLR 时回调部分 **script_on_message** 会进行UI线程检测（防止UI卡住），所以在命令行模式时会出现回调失效的问题。

> WindowsBase 主要是用来处理GUI界面的一些刷新内容的，其实命令行理论上是可以去掉的，但是这样的话要编译CLR的源码，这里懒得改了。

# 参考
- 主要参考 [frida-clr](https://github.com/frida/frida-clr/) 中的 HelloFrida 来进行编写。
- 脚本部分参考了 [frida-netstandard](https://github.com/lenacloud/frida-netstandard) 这个库。
