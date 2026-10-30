# kingsSafe 接入与构建

kingsSafe 的接入目标是将加固集成到 Unity 构建过程中，让开发者沿用既有开发和发行流程。这里说明设计中的接入路径，尚不提供可执行的安装命令。

## 接入方式

1. 在 Unity 项目中接入配套项目包与配置。
2. 选择受保护资产、关键数据与核心逻辑，确认现有加载方式和代码访问方式的适配范围。
3. 沿用构建入口，生成包含保护产物与必要运行组件的应用。
4. 使用相同内容和场景，对照功能、资源、存档与运行性能。

原始输入与发行副本分开管理。必须适配的路径在构建前检查，未覆盖对象不计入防护范围。

## 构建时加固

官网中的 Build 代表整个构建流程。不同防护接入适合的阶段，不要求在一个回调内完成全部处理。

| 防护 | 构建过程 | 应用运行时 |
| --- | --- | --- |
| 资产加固 | 处理选定资源，生成保护容器与配套清单 | 按需校验、解密与加载 |
| 内存保护 | 适配选定字段与读写点，打入必要运行组件 | 保护存储与校验关键数据访问 |
| 逻辑混淆 | 在相应代码处理阶段执行兼容的变换 | 执行语义保持的业务逻辑 |

具体扩展点与缓存行为需要按 Unity 版本、脚本后端和目标平台验证。

## 多平台构建

| 平台 | Unity 与平台工具的关系 |
| --- | --- |
| iOS | Unity 生成 Xcode 工程，再由 Xcode 构建应用 |
| Android | Unity 使用 Gradle 构建 APK 或 AAB，也可以导出 Gradle 工程后构建 |
| Windows | Unity 可以直接生成 EXE 与配套文件，导出 Visual Studio 工程为可选方式 |

官网将三条路径归纳为 Unity 到 Build 再到平台产物。当前首个研究环境为 Windows x64 与 IL2CPP，iOS 和 Android 的支持以对应版本兼容性验证为准。

平台流程参考 Unity 官方说明：[iOS 构建](https://docs.unity3d.com/cn/6000.0/Manual/iphone-BuildProcess.html)、[Android 构建](https://docs.unity3d.com/cn/6000.0/Manual/android-build-settings.html)、[Windows 构建](https://docs.unity3d.com/cn/2023.1/Manual/WindowsStandaloneBinaries.html)。

## 保持原有体验

正常玩家无需新增启动操作。功能、资源质量、存档和外部协议保持一致；加载时间、帧时、峰值内存和 GC 分别测量。加载失败与保护异常需要明确处理，不能静默使用未保护内容作为替代。

详细机制见[技术方案](architecture.md)，验证方式见[攻防演练](exercises.md)。
