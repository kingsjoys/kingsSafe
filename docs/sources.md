# kingsSafe 来源索引

初次检索与整理日期为 2026 年 10 月 29 日；10 月 30 日新增[构建加固可行性与实测](build-hardening-feasibility.md)，其中单列 Unity 回调、Addressables、现有产品接入的一手来源及隔离样本证据。官方资料用于确认公开能力和接口；研究论文用于形成可验证假设。产品验收仍以固定版本、授权样本上的实验为准。

## 项目与安全研究

| 编号 | 来源 | 用途与边界 |
| --- | --- | --- |
| R01 | [REA 官方仓库](https://github.com/morluto/rea) | 分析工具、证据和案例；采用时固定 commit、发行版本和底层引擎 |
| R02 | [ACE 端游加固](https://www.anticheatexpert.com/products/lite-anti-cheat-pc) | 厂商公开的保护能力；不是独立效果测试 |
| R03 | [腾讯云 ACE 产品功能](https://cloud.tencent.cn/document/product/654/30316) | 多层保护及构建流程集成；页面显示更新于 2025 年 3 月 18 日 |
| R04 | [IEICE 混淆与 LLM 函数命名研究](https://globals.ieice.org/en_transactions/information/10.1587/transinf.2025KBP0015/_f) | 2026 年 9 月正式发表；任务是函数名恢复，不能外推完整破解率 |
| R05 | [BinDeObfBench](https://arxiv.org/abs/2604.08083) | 2026 年 4 月预印本；混淆伪代码处理与模型差异 |
| R06 | [SRE Bench v2](https://arxiv.org/html/2608.11469v2) | 2026 年 10 月 1 日版本；规模、预算、重复尝试与验证问题 |
| R07 | [VMSpec](https://arxiv.org/abs/2610.29566) | 2026 年 10 月 21 日预印本；特定虚拟化样本上的自动化分析 |
| R08 | [Denuvo 反作弊产品](https://irdeto.com/video-games/denuvo-anti-cheat) | 厂商公开的代码、运行时和行为检测体系 |
| R09 | [OWASP MASVS RESILIENCE](https://mas.owasp.org/MASVS/11-MASVS-RESILIENCE/) | 纵深防御原则；适用主体是移动应用 |
| R10 | [Riot Fog of War](https://technology.riotgames.com/news/demolishing-wallhacks-valorants-fog-war) | 2020 年技术文章；服务端信息最小化与工程约束 |

## Unity 官方接口与运行约束

| 编号 | 来源 | 用途与边界 |
| --- | --- | --- |
| U01 | [Unity 6.3 IL2CPP](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/scripting-backends/il2cpp) | 托管 IL 到 C++ 和原生代码的管线 |
| U02 | [OnPostBuildPlayerScriptDLLs](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/build/ipostbuildplayerscriptdlls/onpostbuildplayerscriptdlls) | 处理本次 Player 托管程序集；官方提示额外构建成本 |
| U03 | [IPreprocessBuildWithContext](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/build/ipreprocessbuildwithcontext) | 构建前校验候选接口；实现须按固定补丁核对签名和支持情况 |
| U04 | [IPostprocessBuildWithContext](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/build/ipostprocessbuildwithcontext) | 构建后处理候选接口；发行签名仍由最终流水线协调 |
| U05 | [Managed code stripping](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/script-compilation/managed-code-stripping) | Linker、保留规则和静态分析边界 |
| U06 | [Unity 6.3 序列化规则](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/script-serialization/rules) | 字段、托管引用和资源序列化约束；M0 对固定补丁和相关包复核 |
| U07 | [Unity 6.3 IL2CPP 限制](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/scripting-backends/il2cpp/scripting-restrictions) | 反射、AOT、序列化等限制 |

旧的 `IPreprocessBuildWithReport` 和 `IPostprocessBuildWithReport` 官方页面指向 WithContext 接口。适配层以选定版本实测结果为准，不同时注册新旧入口造成重复处理。官方文档是接入候选依据，不代表已完成 kingsSafe 构建验证。

## 资源加载与密码接口

| 编号 | 来源 | 用途与边界 |
| --- | --- | --- |
| U08 | [AssetBundle LoadFromStreamAsync](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/assetbundle/loadfromstreamasync) | Stream 的随机读取、线程与寿命约束；不是已有加密实现 |
| U09 | [Addressables IResourceProvider](https://docs.unity3d.com/Packages/com.unity.addressables@2.7/api/UnityEngine.ResourceManagement.ResourceProviders.IResourceProvider.html) | 资源提供与释放接口候选；包版本需固定后验收 |
| C01 | [.NET AesGcm](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm?view=net-10.0) | 认证加密接口参考；不证明 Unity IL2CPP 支持该 API |
| W01 | [Windows 进程安全与访问权限](https://learn.microsoft.com/en-us/windows/win32/procthread/process-security-and-access-rights) | 进程内存读写的系统权限边界 |

## 直接提取工具候选

| 编号 | 来源 | 用途与边界 |
| --- | --- | --- |
| T01 | [AssetRipper](https://github.com/AssetRipper/AssetRipper) | 资源分析候选；锁定版本并先验证原包能导出 |
| T02 | [UnityPy](https://github.com/K0lb3/UnityPy) | 资源解析和提取候选；按资产与版本检验能力 |
| T03 | [AssetStudio 原仓库](https://github.com/Perfare/AssetStudio) | 仓库已归档，仅兼容时作补充；解析失败不是安全证明 |

新增来源整理于 2026 年 10 月 29 日。技术方案是候选设计，所有保护效果和性能数值须来自实际实验；评测文档中的研发预算提案不是已测指标。
