# kingsSafe Unity 自动构建机制探针

这是自有小样本的研究代码，验证“只导入插件、沿用原 Build”能否把三条机制自动带入真实 Player。不是产品 SDK、抗外挂方案或性能测试。

首轮实测于 2026-10-30：Unity 6000.3.6f1 / macOS arm64 主机 / Mac Standalone universal（x64 + arm64）Mono Player，以本机默认 arm64 执行；未验证 x64 运行。详细分析见[可行性报告](../../docs/build-hardening-feasibility.md)，机器可读摘要见[首轮结果](results-2026-10-30.json)和[新建空工程自动复现结果](results-clean-2026-10-30.json)。两轮原业务源码摘要相同；新工程生成的资产标识不同，不要求两轮 Bundle 文件摘要相同，只验证各自基线与保护构建的原输入保持。

## 文件

- `Fixture/`：原始游戏与原始构建入口，无 kingsSafe 属性、安全类型或业务调用。
- `Plugin/`：研究插件，自动处理构建副本；内部诊断代码仅为研究样本服务。
- `reproduce.py`：新建隔离工程，构建基线，复制插件，再沿用同一入口构建两次并运行。
- `verify.py`：检查源码指纹、正负结果、重复构建及本地签名；通过表示机制证据完整，不表示产品安全通过。
- `Cecil.editor.meta`：确保研究依赖只在 Editor 使用。
- `Project/`、`artifacts/`：本地生成，已忽略；不分发 Unity 文件、Cecil 二进制、Player 或完整日志。

## 已观察结果

原业务两个 C# 文件、原构建入口和输入 Bundle 的 SHA-256 保持。处理一个字段、七处字段访问与一个同步资源调用；正常结果仍是弹药 97、得分 298、固定文本内容相同。单独写坏字段在下一次业务读取被拒绝；修改研究容器密文被认证检查拒绝。私有方法原名消失；不改代码的重复构建也实际运行通过。本地 ad-hoc 签名由插件自动完成并通过严格验证。

**已观察绕过**：同时改值与校验状态、回放全部旧状态，均被简化字段保护接受。这些是结果的一部分，不是应当隐藏的失败。

## 复现

需要 macOS、本机已有 Unity **6000.3.6f1** 及有效授权、Mac Mono 构建支持、可用的 `/usr/bin/codesign` 和 Python 3。不下载或安装其他保护器；Cecil 0.11.1 从本机 Unity 安装中复制，仅用于隔离 Editor 插件。首次启动 Unity 可能正常解析其默认包；脚本不办理授权、不安装平台模块。

在项目根目录运行：

```sh
python3 research/unity-build-probe/reproduce.py
```

如 Unity 安装在其他位置：

```sh
python3 research/unity-build-probe/reproduce.py --unity '/absolute/path/Unity.app/Contents/MacOS/Unity'
```

脚本只创建新的研究目录，拒绝覆盖已有目录。生成路径会在结束时输出。Unity 需要本地许可证及包管理通信，受限执行环境可能需要允许其正常系统通信。复现不会改动现有游戏工程、Unity 安装或网站。

当前首次手工调试工程的证据可用以下命令重新核验：

```sh
python3 research/unity-build-probe/verify.py
```

自动从新目录复现得到的证据，使用该目录下的 Project 和 artifacts 参数核验：

```sh
python3 research/unity-build-probe/verify.py --project '/new/work/Project' --artifacts '/new/work/artifacts'
```

## 明确限制

资源是完整内存 AES-CBC + HMAC 容器，使用**公开的测试常量密钥**，不是发行方案，也没有证明抵抗已知源码提取。只处理本地 `.bundle` 和 `LoadFromFile(string)`，没有覆盖其他重载、异步、场景、Addressables 或 Resources。未运行独立提取器；更换头并不能单独证明防拆包强度。

内存是固定 XOR 加互补校验，只演示表示与访问改写。没有防重放、密钥轮换或可信业务进展。通过反射的受控写入模拟存储损坏，没有使用外部修改器、调试器或读取进程；没有探测窥视或自终止。

逻辑只验证私有方法改名；未实现强控制流混淆或 REA / AI 效果评测。简化遍历还处理了 Unity 生成辅助方法，不具备生产保留规则。任意反射、序列化、协议、跨程序集、Burst、Jobs、泛型、并发和复杂引用均未建立兼容保证。

只识别一个 Int32.ToString() 只读取地址模式；其他 ref/out 与地址逃逸仍排除。字段自动选择不意味着能自动理解游戏公平规则。

后处理曾破坏签名，现只在本机 Mac 研究流程中自动重新 ad-hoc 签名；正式发行身份、公证和其他平台签名未验证。没有 Windows IL2CPP、Android、iOS、完整功能和视觉回归或性能验收。

FixtureBuild 自己会生成场景和资源，这是该原始样本的构建行为。源码指纹只覆盖列出的三个 C# 文件和输入 Bundle，不能扩大为整个工程所有文件与配置都已验证不变。正式产品必须扩大快照、覆盖报告与回归矩阵。
