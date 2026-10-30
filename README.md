# kingsSafe

[官网](https://safe.kingsjoys.com/) · [研究文档](docs/architecture.md) · [Issues](https://github.com/kingsjoys/kingsSafe/issues) · [参与贡献](CONTRIBUTING.md) · [Apache-2.0](LICENSE)

kingsSafe 面向 Unity 游戏，以三个并列核心功能建设开源安全加固与验证体系：资产加固、内存保护、逻辑混淆，分别对应守护创作、捍卫公平、保护创新。交付先从资产加固开始，再开展关键数据内存保护；开发顺序不代表功能的重要性等级。

项目以公开研究、独立验证和可复现回归持续改进安全性。REA 类逆向工具作为技术评测手段之一，不作为品牌定位或联合产品关系。

这是一个系统工程：定位、目标、威胁模型、评测基准、技术方案、实现、质量验证、开源协作和发布维护都属于交付。当前处于设计与可行性验证阶段，已在隔离小样本上实测构建自动接入机制，尚未交付产品级防护器，也未通过产品安全、兼容性和性能验收。

## 快速开始

本仓库包含设计、隔离实验和官网，尚无可安装的产品 SDK。

```sh
git clone https://github.com/kingsjoys/kingsSafe.git
cd kingsSafe
npm --prefix www run build
npm --prefix www run check
npm --prefix www run dev
```

官网需要 Node.js 22+，无需安装第三方前端依赖。Unity 实验的指定版本、授权和复现方法见[研究样本](research/unity-build-probe/README.md)。

| 目录 | 内容 |
| --- | --- |
| `docs/` | 定位、目标、技术方案、路线图、评测标准和研究 |
| `research/` | 自有样本、机制探针、复现工具和结果摘要 |
| `www/` | 官网、产品文档和静态构建工具 |
| `.github/` | CI、Pages 部署、Issue 与 PR 模板 |

## 设计原则

接入硬目标为只引入 kingsSafe 插件，沿用原 Build，不要求团队修改业务源码、字段类型、场景、资源组织或构建脚本。插件自动处理构建副本并报告覆盖范围；当前方案仍需验证自动接入，不将手工改造样本计为产品完成。

- 玩家运行透明，无新增启动器、管理员权限或额外操作；允许必要的资源解密与数据保护组件。
- 功能和资源无损，保持资源质量、业务行为、存档和对外协议。
- 性能开销单独实测并披露，预算在实验前固定，不预先承诺零开销。
- 源码和格式可以公开，安全不能依赖实现不公开；实际部署密钥单独管理。
- 防护效果以可复现任务验证，公开成功、失败、绕过与适用范围。
- 定位、目标、路线图、技术方案和验证标准先于实现确定。

“透明”描述正常玩家体验，“无损”描述功能和资源内容，性能是独立维度。不能用其中一项通过替代其他项。

## 文档导航

| 文档 | 回答的问题 |
| --- | --- |
| [项目定位](docs/positioning.md) | 为谁解决什么问题，三项核心功能如何定位 |
| [构建加固可行性与实测](docs/build-hardening-feasibility.md) | 只加插件能否带入三条机制，哪些已验证、哪些仍需突破 |
| [Unity 自动构建研究样本](research/unity-build-probe/README.md) | 原源码、原 Build 对照与可复现正负结果 |
| [内存窥探检测专项调研](docs/memory-observation-research.md) | 只加插件的探测与自终止路线、三平台边界和原型验收 |
| [目标与要求](docs/requirements.md) | 保护范围、威胁边界和透明无损契约 |
| [系统路线图](docs/roadmap.md) | 各阶段产物与进入下一阶段的条件 |
| [技术方案](docs/architecture.md) | 资源与内存保护如何接入 Unity |
| [评测与验证标准](docs/validation.md) | 预先固定任务、对照组、判定与性能方法 |
| [第一版计划](docs/mvp-plan.md) | 资源保护版本的任务与交付条件 |
| [开源共建与安全验证](docs/open-collaboration.md) | 公开实现条件、复现、修复和发布流程 |
| [调研结论](docs/research.md) | 技术依据与证据边界 |
| [来源索引](docs/sources.md) | 官方文档与研究资料 |
| [官网部署与域名](docs/deployment.md) | GitHub Pages 自动发布与二级域名配置 |

## 交付顺序

| 阶段 | 主要交付 |
| --- | --- |
| 设计基线 | 定位、目标、威胁模型、技术候选、评测与验收标准 |
| v0.1 资源保护 | 指定 AssetBundle 保护、配套加载、缓存检查、内容等价与攻防报告 |
| v0.2 关键数据保护 | 少量敏感字段的抗搜索、篡改检测和冻结场景验证 |
| 逻辑混淆 | 保护核心算法、独有机制与关键实现，验证分析和篡改成本 |
| 持续运营 | 攻击复现进入回归集，修复通过验证后发布 |

首个研究目标为 Unity 6.3 LTS 一个固定补丁、Windows x64、IL2CPP。先覆盖明确清单中的本地 AssetBundle；Addressables 作为独立适配，锁定包版本后验证。Resources、内置场景数据、任意安装包和所有资源加载路径不默认属于已覆盖范围。

资源最终需要被客户端使用，进程内的数据保护也可能被定向或更高权限攻击绕过。项目衡量直接提取与篡改成功率、攻击成本、功能正确性和性能代价，不承诺绝对不可提取或防住全部外挂。

最近更新：2026 年 10 月 30 日。本轮交付包括构建加固可行性报告、Mac Mono 隔离机制探针和协调修改/旧状态回放反例。Windows IL2CPP、外部窥探检测与自终止仍未实测。项目代码、文档与原创网站资产采用 [Apache-2.0](LICENSE)；第三方组件各自遵循原许可证。尚未发布产品版本，后续工作通过 Issues、里程碑和 Pull Request 管理。

贡献方式见 [CONTRIBUTING.md](CONTRIBUTING.md)，私密风险报告见 [SECURITY.md](SECURITY.md)，变更见 [CHANGELOG.md](CHANGELOG.md)。
